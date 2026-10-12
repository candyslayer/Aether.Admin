using Aether.Application.DTOs;
using Aether.Application.Interfaces;
namespace Aether.Api.Filters;
public sealed class PermissionFilter(string permission, bool administratorOnly) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var actor = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>().Snapshot;
        if (!actor.Permissions.Contains(permission) || (administratorOnly && !actor.IsAdministrator))
        {
            context.HttpContext.RequestServices.GetRequiredService<ILogger<PermissionFilter>>().LogWarning("Permission denied: tenant {TenantId}, user {UserId}, permission {Permission}", actor.TenantId, actor.UserId, permission);
            return ValueTask.FromResult<object?>(TypedResults.Json(new ApiResponse<string>(403, "Permission denied.", null), ApiJsonContext.Default.ApiResponseString, statusCode: 403));
        }
        return next(context);
    }
}
public static class PermissionExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission, bool administratorOnly = false)
        => builder.RequireAuthorization().AddEndpointFilter(new PermissionFilter(permission, administratorOnly));
}
