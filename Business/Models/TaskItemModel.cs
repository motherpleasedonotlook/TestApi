namespace Business.Models;

/// <summary>
/// Модель задачи
/// </summary>
public sealed record TaskItemModel(
    Guid Id,
    string Title,
    string Priority,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    uint RowVersion);
