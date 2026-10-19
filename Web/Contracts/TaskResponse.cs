namespace Web.Contracts;

/// <summary>
/// Задача
/// </summary>
public sealed record TaskResponse(
    Guid Id,
    string Title,
    string Priority,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
