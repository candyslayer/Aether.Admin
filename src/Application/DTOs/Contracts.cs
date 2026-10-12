using Aether.Domain.Enums;

namespace Aether.Application.DTOs;

public sealed record ApiResponse<T>(int Code, string Message, T? Data);
public sealed record LoginRequest(string TenantCode, string Username, string Password);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid UserId, Guid TenantId, Guid DeptId, string[] Roles, string[] Permissions);
public sealed record UserRequest(string Username, string? Password, string Nickname, Guid DeptId, Guid[] RoleIds, Guid[] PostIds, string? Email = null, string? Mobile = null, bool Status = true, long Version = 1);
public sealed record UserDto(Guid Id, string Username, string Nickname, Guid DeptId, string? Email, string? Mobile, bool Status, long Version, Guid[] RoleIds, Guid[] PostIds);
public sealed record DepartmentRequest(string Name, Guid OrganizationId, Guid? ParentId = null, Guid? LeaderId = null, int Sort = 0, bool Status = true, long Version = 1);
public sealed record DepartmentDto(Guid Id, string Name, Guid OrganizationId, Guid? ParentId, string TreePath, Guid? LeaderId, int Sort, bool Status, long Version, List<DepartmentDto> Children);
public sealed record CatalogRequest(string Name, string Code, int Sort = 0, bool Status = true, long Version = 1);
public sealed record CatalogDto(Guid Id, string Name, string Code, int Sort, bool Status, long Version);
public sealed record MenuRequest(string Name, MenuType Type, Guid? ParentId = null, string? Path = null, string? Component = null, string? PermissionCode = null, int Sort = 0, bool Visible = true, bool Status = true, long Version = 1);
public sealed record MenuDto(Guid Id, string Name, MenuType Type, Guid? ParentId, string? Path, string? Component, string? PermissionCode, int Sort, bool Visible, bool Status, long Version, List<MenuDto> Children);
public sealed record GrantRequest(Guid[] MenuIds, DataScopeType ScopeType, Guid[] DepartmentIds);
public sealed record GrantDto(Guid[] MenuIds, DataScopeType ScopeType, Guid[] DepartmentIds);
public sealed record Page<T>(List<T> Items, long Total, int PageNumber, int PageSize);
public sealed record AuditDto(Guid Id, Guid? UserId, string Method, string Endpoint, string Parameters, string Ip, string TraceId, int Result, long DurationMs, DateTimeOffset CreatedAt);
public sealed record IdentityState(Guid UserId, Guid TenantId, Guid DeptId, Guid SecurityStamp, long PermissionVersion);
public sealed record AuthorizationSnapshot(Guid UserId, Guid TenantId, Guid DeptId, bool IsAdministrator, string[] Roles, string[] Permissions, bool All, bool Self, Guid[] DepartmentIds);
public sealed record LoginIdentity(IdentityState State, string PasswordHash);
public sealed record TokenResult(string Token, DateTimeOffset ExpiresAt);
public sealed record AuditEntry(Guid TenantId, Guid? UserId, string Method, string Endpoint, string Parameters, string Ip, string TraceId, int Result, long DurationMs);
public sealed record HealthDto(string Status);
