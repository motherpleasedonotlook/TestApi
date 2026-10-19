using Business.Models;
using Business.Models.Converters;
using Data.Entities;
using Data.Entities.Enums;
using Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Business.Services;

/// <summary>
/// Сервис работы с задачами
/// </summary>
public sealed class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TaskItemService> _logger;

    public TaskItemService(
        ITaskItemRepository repository,
        IEventPublisher eventPublisher,
        ILogger<TaskItemService> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<TaskItemModel> CreateAsync(
        string title,
        string priority,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
        {
            throw new ArgumentException("Заголовок должен содержать 1-200 символов", nameof(title));
        }

        if (!Enum.TryParse<Priority>(priority, ignoreCase: true, out var parsedPriority))
        {
            throw new ArgumentException("Некорректное значение приоритета", nameof(priority));
        }

        var task = new TaskItem(title, parsedPriority);

        await _repository.AddAsync(task, cancellationToken);

        _logger.LogInformation("Создана задача {TaskId}", task.Id);

        return task.ToTaskItemModel();
    }

    public async Task<PageResultModel<TaskItemModel>> GetAllAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(offset, limit, cancellationToken);

        return new PageResultModel<TaskItemModel>(
            items.Select(item => item.ToTaskItemModel()).ToList(),
            totalCount);
    }

    public async Task<TaskItemModel> CompleteAsync(
        Guid id,
        uint rowVersion,
        CancellationToken cancellationToken)
    {
        var completedTask = await _repository.CompleteTaskAsync(id, rowVersion, cancellationToken);

        _logger.LogInformation("Задача {TaskId} завершена", id);

        var message = new TaskCompletedEvent(completedTask.Id, completedTask.Title, completedTask.Priority.ToString(), completedTask.CompletedAt.Value);
        await _eventPublisher.PublishAsync(message, cancellationToken);

        return completedTask.ToTaskItemModel();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        await _repository.RemoveAsync(task, cancellationToken);

        _logger.LogInformation("Задача {TaskId} удалена", id);

        return true;
    }
}