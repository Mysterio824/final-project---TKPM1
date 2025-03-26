using System.Net;
using System.Text.Json;
using DevTools.Exceptions;

namespace DevTools.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode;
        string message;

        switch (ex)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            case UnauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                message = ex.Message;
                break;

            case ValidationException:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                break;
        }

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
