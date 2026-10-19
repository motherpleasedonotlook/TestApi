using Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Business;

/// <summary>
/// Подключение сервисов
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqChannelProvider, RabbitMqChannelProvider>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        return services;
    }
}