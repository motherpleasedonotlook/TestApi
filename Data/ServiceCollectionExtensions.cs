using Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>
/// Регистрирует репозитории
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        return services;
    }
}