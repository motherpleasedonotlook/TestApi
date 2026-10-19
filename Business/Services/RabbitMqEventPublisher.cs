using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Business.Services;

/// <summary>
/// Реализация <see cref="IEventPublisher"/> для RabbitMQ
/// </summary>
public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IRabbitMqChannelProvider _channelProvider;
    private readonly RabbitMqConfiguration _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly int _shutdownTimeout;

    private readonly Lock _gate = new();
    private readonly TaskCompletionSource _inflightDrained =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int _inflightCount;
    private bool _stopping;
    private int _disposed;

    public RabbitMqEventPublisher(
        IRabbitMqChannelProvider channelProvider,
        RabbitMqConfiguration settings,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _channelProvider = channelProvider;
        _settings = settings;
        _logger = logger;
        _shutdownTimeout = settings.ShutdownTimeoutSeconds;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken)
    {
        if (!TryBeginPublish())
        {
            _logger.LogWarning("Попытка публикации после остановки приложения");
            return;
        }

        try
        {
            var channel = await _channelProvider.GetChannelAsync(cancellationToken);

            var json = JsonSerializer.Serialize(message, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                _settings.Exchange,
                _settings.RoutingKey,
                body,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось опубликовать сообщение в RabbitMQ");
        }
        finally
        {
            EndPublish();
        }
    }

    private bool TryBeginPublish()
    {
        lock (_gate)
        {
            if (_stopping || Volatile.Read(ref _disposed) == 1)
            {
                return false;
            }

            _inflightCount++;
            return true;
        }
    }

    private void EndPublish()
    {
        lock (_gate)
        {
            _inflightCount--;

            if (_inflightCount == 0 && _stopping)
            {
                _inflightDrained.TrySetResult();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        var drainTask = CompleteStartedPublishes();

        if (await Task.WhenAny(drainTask, Task.Delay(_shutdownTimeout)) != drainTask)
        {
            _logger.LogWarning(
                "Не все публикации RabbitMQ завершились за {ShutdownTimeout}",
                _shutdownTimeout);
        }

        await _channelProvider.DisposeAsync();
    }

    private Task CompleteStartedPublishes()
    {
        lock (_gate)
        {
            _stopping = true;
            return _inflightCount == 0 ? Task.CompletedTask : _inflightDrained.Task;
        }
    }
}