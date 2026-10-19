using Data.Entities;

namespace Data.Repositories;

/// <summary>
/// Репозиторий работы с задачами
/// </summary>
public interface ITaskItemRepository
{
    /// <summary>
    /// Добавить задачу
    /// </summary>
    Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задачу по идентификатору
    /// </summary>
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить страницу задач, отсортированных по убыванию времени создания
    /// </summary>
    Task<(IReadOnlyList<TaskItem> Items, int Count)> GetPagedAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить задачу
    /// </summary>
    Task RemoveAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершить задачу
    /// </summary>
    Task<TaskItem> CompleteTaskAsync(
        Guid id,
        uint rowVersion,
        CancellationToken cancellationToken = default);
}