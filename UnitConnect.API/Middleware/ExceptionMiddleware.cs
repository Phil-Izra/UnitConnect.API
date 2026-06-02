using System.Text.Json;
using UnitConnect.Application.Exceptions;
using UnitConnect.Domain.Exceptions;

namespace UnitConnect.API.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(context, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            DomainException   => (StatusCodes.Status400BadRequest,  ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound,    ex.Message),
            ConflictException => (StatusCodes.Status409Conflict,    ex.Message),
            ForbiddenException=> (StatusCodes.Status403Forbidden,   ex.Message),
            _                 => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = status;

        var body = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(body);
    }
}
