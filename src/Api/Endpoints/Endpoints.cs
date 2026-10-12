using Aether.Api.Filters;
using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using Aether.Application.Services;
using Aether.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Aether.Api.Endpoints;

public static class Endpoints
{
    private static Ok<ApiResponse<T>> Ok<T>(T data) => TypedResults.Ok(new ApiResponse<T>(0, "OK", data));
    private static Ok<ApiResponse<string>> Done() => Ok("OK");
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Authentication");
        g.MapPost("/login", async (LoginRequest request, AuthService service, CancellationToken ct) => Ok(await service.LoginAsync(request, ct))).RequireRateLimiting("login");
        g.MapGet("/menus", async (AdminService service, CancellationToken ct) => Ok(await service.MenusAsync(true, ct))).RequireAuthorization();
        g.MapGet("/permissions", (ICurrentUser user) => Ok(user.Snapshot.Permissions)).RequireAuthorization();
    }
    public static void MapUserEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();
        g.MapGet("", async (AdminService service, CancellationToken ct, int page = 1, int size = 20, string? search = null) => Ok(await service.UsersAsync(page, size, search, ct))).RequirePermission("system:user:list");
        g.MapPost("", async (UserRequest request, AdminService service, CancellationToken ct) => TypedResults.Created("/api/users", new ApiResponse<UserDto>(0, "Created", await service.SaveUserAsync(null, request, ct)))).RequirePermission("system:user:create");
        g.MapPut("/{id:guid}", async (Guid id, UserRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveUserAsync(id, request, ct))).RequirePermission("system:user:update");
        g.MapDelete("/{id:guid}", async (Guid id, AdminService service, CancellationToken ct) => { await service.DeleteUserAsync(id, ct); return Done(); }).RequirePermission("system:user:delete");
    }
    public static void MapDeptEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/depts").WithTags("Departments").RequireAuthorization();
        g.MapGet("/tree", async (AdminService service, CancellationToken ct) => Ok(await service.DepartmentsAsync(ct))).RequirePermission("system:dept:list");
        g.MapPost("", async (DepartmentRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveDepartmentAsync(null, request, ct))).RequirePermission("system:dept:create", true);
        g.MapPut("/{id:guid}", async (Guid id, DepartmentRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveDepartmentAsync(id, request, ct))).RequirePermission("system:dept:update", true);
        g.MapDelete("/{id:guid}", async (Guid id, AdminService service, CancellationToken ct) => { await service.DeleteDepartmentAsync(id, ct); return Done(); }).RequirePermission("system:dept:delete", true);
    }
    public static void MapRoleEndpoints(this WebApplication app) => MapCatalog(app, "roles", "role");
    public static void MapPostEndpoints(this WebApplication app) => MapCatalog(app, "posts", "post");
    public static void MapOrganizationEndpoints(this WebApplication app) => MapCatalog(app, "organizations", "organization");
    private static void MapCatalog(WebApplication app, string kind, string permission)
    {
        var g = app.MapGroup("/api/" + kind).WithTags(kind).RequireAuthorization();
        g.MapGet("", async (AdminService service, CancellationToken ct) => Ok(await service.CatalogAsync(kind, ct))).RequirePermission($"system:{permission}:list", true);
        g.MapPost("", async (CatalogRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveCatalogAsync(kind, null, request, ct))).RequirePermission($"system:{permission}:create", true);
        g.MapPut("/{id:guid}", async (Guid id, CatalogRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveCatalogAsync(kind, id, request, ct))).RequirePermission($"system:{permission}:update", true);
        g.MapDelete("/{id:guid}", async (Guid id, AdminService service, CancellationToken ct) => { await service.DeleteCatalogAsync(kind, id, ct); return Done(); }).RequirePermission($"system:{permission}:delete", true);
    }
    public static void MapMenuEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/menus").WithTags("Menus").RequireAuthorization();
        g.MapGet("", async (AdminService service, CancellationToken ct) => Ok(await service.MenusAsync(false, ct))).RequirePermission("system:menu:list", true);
        g.MapPost("", async (MenuRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveMenuAsync(null, request, ct))).RequirePermission("system:menu:create", true);
        g.MapPut("/{id:guid}", async (Guid id, MenuRequest request, AdminService service, CancellationToken ct) => Ok(await service.SaveMenuAsync(id, request, ct))).RequirePermission("system:menu:update", true);
        g.MapDelete("/{id:guid}", async (Guid id, AdminService service, CancellationToken ct) => { await service.DeleteMenuAsync(id, ct); return Done(); }).RequirePermission("system:menu:delete", true);
    }
    public static void MapPermissionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/permissions", () => Ok(PermissionCatalog.Codes)).RequirePermission("system:permission:assign", true);
        app.MapGet("/api/roles/{id:guid}/permissions", async (Guid id, AdminService service, CancellationToken ct) => Ok(await service.GrantsAsync(id, ct))).RequirePermission("system:permission:assign", true);
        app.MapPut("/api/roles/{id:guid}/permissions", async (Guid id, GrantRequest request, AdminService service, CancellationToken ct) => { await service.SetGrantsAsync(id, request, ct); return Done(); }).RequirePermission("system:permission:assign", true);
        app.MapGet("/api/audit-logs", async (AdminService service, CancellationToken ct, int page = 1, int size = 20) => Ok(await service.AuditsAsync(page, size, ct))).RequirePermission("system:audit:list", true);
    }
}
