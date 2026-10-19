using RabbitMQ.Client;

namespace Business;

/// <summary>
/// Провайдер канала RabbitMQ
/// </summary>
public interface IRabbitMqChannelProvider : IAsyncDisposable
{
    /// <summary>
    /// Получить открытый канал
    /// </summary>
    Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);
}