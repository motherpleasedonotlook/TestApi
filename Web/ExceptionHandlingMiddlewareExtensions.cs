using Microsoft.AspNetCore.Builder;

namespace Web;

/// <summary>
/// Расширение для регистрации Middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Регистрация кастомного обработчика исключений
    /// </summary>
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}