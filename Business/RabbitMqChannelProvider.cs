using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Business;

/// <summary>
/// Реализует <see cref="IRabbitMqChannelProvider"/>
/// </summary>
public sealed class RabbitMqChannelProvider : IRabbitMqChannelProvider
{
    private readonly RabbitMqConfiguration _settings;
    private readonly ILogger<RabbitMqChannelProvider> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly int _shutdownTimeout;

    private IConnection? _connection;
    private IChannel? _channel;
    private int _disposed;

    public RabbitMqChannelProvider(
        RabbitMqConfiguration settings,
        ILogger<RabbitMqChannelProvider> logger)
    {
        _settings = settings;
        _logger = logger;
        _shutdownTimeout = settings.ShutdownTimeoutSeconds;
    }

    public async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _settings.Exchange,
                type: ExchangeType.Topic,
                durable: true, cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        using var cts = new CancellationTokenSource(_shutdownTimeout);

        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken: cts.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при закрытии канала RabbitMQ");
        }

        try
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync(cancellationToken: cts.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при закрытии соединения RabbitMQ");
        }

        _channel?.Dispose();
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}