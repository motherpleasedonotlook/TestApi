using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Репозиторий работы с задачами
/// </summary>
internal sealed class TaskItemRepository : ITaskItemRepository
{
    private readonly TaskItemDbContext _dbContext;

    public TaskItemRepository(TaskItemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.TaskItems.FindAsync([id], cancellationToken);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int Count)> GetPagedAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.TaskItems.AsNoTracking().OrderByDescending(x => x.CreatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, count);
    }

    public async Task RemoveAsync(TaskItem task, CancellationToken cancellationToken)
    {
        _dbContext.TaskItems.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskItem> CompleteTaskAsync(
        Guid id,
        uint rowVersion,
        CancellationToken cancellationToken)
    {
        var task = await GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            throw new KeyNotFoundException($"Задача {id} не найдена.");
        }

        _dbContext.Entry(task).Property(x => x.RowVersion).OriginalValue = rowVersion;

        task.Complete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }
}