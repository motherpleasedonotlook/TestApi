using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web;

internal sealed class ExceptionHandlingMiddleware
{
    private const string ProblemJsonContentType = "application/problem+json";
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке запроса {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
            await HandleErrorAsync(context, ex);
        }
    }

    private static async Task HandleErrorAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problemDetails.Status = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentOutOfRangeException or ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException or DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            _ => problemDetails.Status
        };

        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = ProblemJsonContentType;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
