using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using Npgsql;
namespace Aether.Api.Middleware;
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
            if (context.Response.StatusCode >= 400 && context.Response.ContentType is null && !context.Response.HasStarted)
                await Error(context, context.Response.StatusCode, "Request rejected.");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;
            var (status, message) = ex switch
            {
                AppException a => (a.Status, a.Message),
                BadHttpRequestException b => (b.StatusCode, "Invalid request."),
                PostgresException { SqlState: "23505" } => (409, "Resource already exists."),
                PostgresException { SqlState: "23503" } => (409, "Resource is referenced or a reference is invalid."),
                PostgresException { SqlState: "23514" } => (400, "Constraint violation."),
                _ => (500, "Internal server error.")
            };
            if (status == 500) logger.LogError(ex, "Unhandled request error {TraceId}", context.TraceIdentifier);
            await Error(context, status, message);
        }
    }
    private static Task Error(HttpContext c, int status, string message)
    {
        c.Response.StatusCode = status;
        return c.Response.WriteAsJsonAsync(new ApiResponse<string>(status, message, null), ApiJsonContext.Default.ApiResponseString, cancellationToken: c.RequestAborted);
    }
}
