namespace Business.Services;

/// <summary>
/// Сервис отправки событий 
/// </summary>
public interface IEventPublisher : IAsyncDisposable
{
    /// <summary>
    /// Опубликовать событие
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);
}
