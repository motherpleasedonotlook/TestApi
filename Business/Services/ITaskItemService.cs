using Business.Models;

namespace Business.Services;

/// <summary>
/// Сервис для работы с задачами
/// </summary>
public interface ITaskItemService
{
    /// <summary>
    /// Создать задачу
    /// </summary>
    public Task<TaskItemModel> CreateAsync(
        string title,
        string priority,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все задачи с пагинацией
    /// </summary>
    public Task<PageResultModel<TaskItemModel>> GetAllAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершить задачу
    /// </summary>
    public Task<TaskItemModel> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить задачу
    /// </summary>
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}