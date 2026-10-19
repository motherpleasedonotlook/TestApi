namespace Business.Models;

/// <summary>
/// Событие завершения задачи.
/// </summary>
public sealed record TaskCompletedEvent(
    Guid TaskId,
    string Title,
    string Priority,
    DateTimeOffset CompletedAt);