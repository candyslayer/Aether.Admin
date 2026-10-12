using Aether.Application.DTOs;

namespace Aether.Application.Interfaces;

public interface ICurrentUser
{
    AuthorizationSnapshot Snapshot { get; }
    void Set(AuthorizationSnapshot snapshot);
}
public interface IIdentityRepository
{
    Task<LoginIdentity?> FindLoginAsync(string tenantCode, string username, CancellationToken ct);
    Task<IdentityState?> FindActiveAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<AuthorizationSnapshot> GetAuthorizationAsync(IdentityState state, CancellationToken ct);
}
public interface IAuthorizationCache
{
    Task<AuthorizationSnapshot?> GetAsync(IdentityState identity, CancellationToken ct);
    Task SetAsync(IdentityState identity, AuthorizationSnapshot snapshot, CancellationToken ct);
}
public interface IPasswordService { string Hash(string password); bool Verify(string password, string hash); }
public interface ITokenService { TokenResult Create(IdentityState state, AuthorizationSnapshot snapshot); }
public interface IAdminRepository
{
    Task<Page<UserDto>> UsersAsync(AuthorizationSnapshot actor, int page, int size, string? search, CancellationToken ct);
    Task<UserDto> SaveUserAsync(AuthorizationSnapshot actor, Guid? id, UserRequest request, string? hash, CancellationToken ct);
    Task DeleteUserAsync(AuthorizationSnapshot actor, Guid id, CancellationToken ct);
    Task<List<DepartmentDto>> DepartmentsAsync(AuthorizationSnapshot actor, CancellationToken ct);
    Task<DepartmentDto> SaveDepartmentAsync(AuthorizationSnapshot actor, Guid? id, DepartmentRequest request, CancellationToken ct);
    Task DeleteDepartmentAsync(AuthorizationSnapshot actor, Guid id, CancellationToken ct);
    Task<List<CatalogDto>> CatalogAsync(Guid tenant, string kind, CancellationToken ct);
    Task<CatalogDto> SaveCatalogAsync(Guid tenant, string kind, Guid? id, CatalogRequest request, CancellationToken ct);
    Task DeleteCatalogAsync(Guid tenant, string kind, Guid id, CancellationToken ct);
    Task<List<MenuDto>> MenusAsync(AuthorizationSnapshot actor, bool assignedOnly, CancellationToken ct);
    Task<MenuDto> SaveMenuAsync(Guid tenant, Guid? id, MenuRequest request, CancellationToken ct);
    Task DeleteMenuAsync(Guid tenant, Guid id, CancellationToken ct);
    Task<GrantDto> GrantsAsync(Guid tenant, Guid role, CancellationToken ct);
    Task SetGrantsAsync(Guid tenant, Guid role, GrantRequest request, CancellationToken ct);
    Task<Page<AuditDto>> AuditsAsync(Guid tenant, int page, int size, CancellationToken ct);
}
public interface IAuditRepository { Task AppendAsync(AuditEntry entry, CancellationToken ct); }
public sealed class AppException(int status, string message) : Exception(message) { public int Status { get; } = status; }
