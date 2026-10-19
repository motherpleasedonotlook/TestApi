using Business.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web.Contracts;
using Web.Contracts.Converters;

namespace Web.Endpoints;

/// <summary>
/// Эндпоинты для работы с задачами
/// </summary>
public static class TaskApi
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 100;
    private const string DefaultPriority = "Medium";

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoute.TaskPrefix);

        group.MapPost("/", CreateAsync);
        group.MapGet("/", GetAllAsync);
        group.MapPut(ApiRoute.TaskComplete, CompleteAsync);
        group.MapDelete(ApiRoute.Task, DeleteAsync);
    }

    /// <summary>
    /// Создать задачу
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateTaskItemRequest request,
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request.Title, request.Priority ?? DefaultPriority, cancellationToken);
        return Results.Created(ApiRoute.ForTask(created.Id), created);
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
        ITaskItemService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CompleteAsync(id, cancellationToken);
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