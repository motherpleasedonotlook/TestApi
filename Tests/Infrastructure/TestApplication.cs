using System.Globalization;
using System.Text;
using Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Tests.Infrastructure;

/// <summary>
/// Тестовое приложение
/// </summary>
public sealed class TestApplication : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const int RabbitMqPort = 5672;
    private const string RabbitMqUserName = "guest";
    private const string RabbitMqPassword = "guest";
    private const string Exchange = "task.events";
    private const string RoutingKey = "task.completed";
    private const string QueueName = "task.completed.test.queue";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("TestApi")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername(RabbitMqUserName)
        .WithPassword(RabbitMqPassword)
        .Build();

    private HttpClient? _client;
    private IConnection? _rabbitConnection;
    private IChannel? _rabbitChannel;

    public HttpClient Client =>
        _client ?? throw new InvalidOperationException($"{nameof(TestApplication)} не инициализирован");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var configurationValues = new Dictionary<string, string?>
        {
            { ConfigConstants.DbConnectionString, _postgres.GetConnectionString() },
            { ConfigConstants.RabbitMq.HostName, _rabbitMq.Hostname },
            { ConfigConstants.RabbitMq.Port, _rabbitMq.GetMappedPublicPort(RabbitMqPort).ToString(CultureInfo.InvariantCulture) },
            { ConfigConstants.RabbitMq.UserName, RabbitMqUserName },
            { ConfigConstants.RabbitMq.Password, RabbitMqPassword },
            { ConfigConstants.RabbitMq.Exchange, Exchange },
            { ConfigConstants.RabbitMq.RoutingKey, RoutingKey },
            { ConfigConstants.RabbitMq.QueueName, QueueName }
        };

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(configurationValues);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();

        _client = CreateClient();

        var configuration = Services.GetRequiredService<IConfiguration>();

        var rabbitHost = configuration[ConfigConstants.RabbitMq.HostName]!;
        var rabbitPort = int.Parse(configuration[ConfigConstants.RabbitMq.Port]!, CultureInfo.InvariantCulture);
        var rabbitUser = configuration[ConfigConstants.RabbitMq.UserName]!;
        var rabbitPassword = configuration[ConfigConstants.RabbitMq.Password]!;

        var factory = new ConnectionFactory
        {
            HostName = rabbitHost,
            Port = rabbitPort,
            UserName = rabbitUser,
            Password = rabbitPassword,
        };

        _rabbitConnection = await factory.CreateConnectionAsync();
        _rabbitChannel = await _rabbitConnection.CreateChannelAsync();

        await _rabbitChannel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true);
        await _rabbitChannel.QueueDeclareAsync(QueueName, durable: false, exclusive: false, autoDelete: false);
        await _rabbitChannel.QueueBindAsync(QueueName, Exchange, RoutingKey);
    }

    public async Task<string?> ConsumeSingleMessageAsync()
    {
        var channel = _rabbitChannel ?? throw new InvalidOperationException("RabbitMQ канал не инициализирован.");

        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, args) =>
        {
            var body = Encoding.UTF8.GetString(args.Body.Span);
            tcs.TrySetResult(body);
            return Task.CompletedTask;
        };

        var consumerTag = await channel.BasicConsumeAsync(QueueName, autoAck: true, consumer);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var _ = cts.Token.Register(() => tcs.TrySetResult(null));

        var result = await tcs.Task;

        await channel.BasicCancelAsync(consumerTag, cancellationToken: cts.Token);

        return result;
    }

    public new async Task DisposeAsync()
    {
        if (_rabbitChannel is not null)
        {
            await _rabbitChannel.DisposeAsync();
        }

        if (_rabbitConnection is not null)
        {
            await _rabbitConnection.DisposeAsync();
        }

        _client?.Dispose();

        await base.DisposeAsync();

        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}