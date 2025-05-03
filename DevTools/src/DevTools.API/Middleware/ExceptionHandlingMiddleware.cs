using System.Net;
using System.Text.Json;
using DevTools.Application.DTOs.Response;
using DevTools.Application.Exceptions;
using DevTools.Domain.Exceptions;
using Newtonsoft.Json;


namespace DevTools.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

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
        var message = new List<string> { ex.Message };

        switch (ex)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;

            case UnauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                break;

            case ValidationException:
                statusCode = HttpStatusCode.BadRequest;
                break;

            case BadRequestException:
                statusCode = HttpStatusCode.BadRequest;
                break;

            case ResourceNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.Conflict;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                break;
        }

        var result = JsonConvert.SerializeObject(ApiResult<string>.Failure(message));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(result);
    }
}
