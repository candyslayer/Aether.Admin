using System.Diagnostics;
using System.Text.Json;
using Aether.Application.DTOs;
using Aether.Application.Interfaces;
namespace Aether.Api.Middleware;
public sealed class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IAuditRepository audit)
    {
        var started = Stopwatch.GetTimestamp();
        await next(context);
        if (!context.Request.Path.StartsWithSegments("/api")) return;
        if (!Guid.TryParse(context.User.FindFirst("tenantId")?.Value, out var tenant)) return;
        Guid? user = Guid.TryParse(context.User.FindFirst("sub")?.Value, out var id) ? id : null;
        // Retain route values only; bodies and arbitrary query strings may contain credentials.
        var parameters = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? "");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await audit.AppendAsync(new AuditEntry(tenant, user, context.Request.Method, context.Request.Path.Value?[..Math.Min(context.Request.Path.Value.Length, 512)] ?? "", JsonSerializer.Serialize(parameters, ApiJsonContext.Default.DictionaryStringString), context.Connection.RemoteIpAddress?.ToString() ?? "", Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier, context.Response.StatusCode, (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds), timeout.Token);
        }
        catch (Exception ex) { logger.LogError(ex, "Audit persistence failed for trace {TraceId}", context.TraceIdentifier); }
    }
}
