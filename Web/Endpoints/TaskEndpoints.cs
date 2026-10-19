using Business.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web.Contracts;
using Web.Contracts.Converters;

namespace Web.Endpoints;

/// <summary>
/// Эндпоинты работы с задачами
/// </summary>
public static class TaskEndpoints
{
    private const string DefaultPriority = "Medium";
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 100;

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks");

        group.MapPost("/", CreateAsync);
        group.MapGet("/", GetAllAsync);
        group.MapPut("/{id:guid}/complete", CompleteAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    /// <summary>
    /// Создать задачу
    /// </summary>
    private static async Task<IResult> CreateAsync(
        string title,
        string? priority,
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            priority = DefaultPriority;
        }
        var created = await service.CreateAsync(title, priority, cancellationToken);
        return Results.Created($"/tasks/{created.Id}", created);
    }

    /// <summary>
    /// Получить все задачи
    /// </summary>
    private static async Task<PageResponse<TaskResponse>> GetAllAsync(
        int? offset,
        int? limit,
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(
            offset ?? DefaultOffset,
            limit ?? DefaultLimit,
            cancellationToken);
        
        return new PageResponse<TaskResponse>(
            result.Items.Select(item => item.ToTaskResponse()).ToList(),
            result.Total);
    }

    /// <summary>
    /// Завершить задачу
    /// </summary>
    private static async Task<IResult> CompleteAsync(
        Guid id,
        uint rowVersion,
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CompleteAsync(id, rowVersion, cancellationToken);
        return Results.Ok(result.ToTaskResponse());
    }

    /// <summary>
    /// Удалить задачу
    /// </summary>
    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}