using Aether.Application.DTOs;
using Aether.Application.Interfaces;

namespace Aether.Application.Services;

public sealed class CurrentUser : ICurrentUser
{
    private AuthorizationSnapshot? _snapshot;
    public AuthorizationSnapshot Snapshot => _snapshot ?? throw new AppException(401, "Authentication required.");
    public void Set(AuthorizationSnapshot snapshot) => _snapshot = snapshot;
}
public sealed class PermissionService(IIdentityRepository identities, IAuthorizationCache cache)
{
    public async Task<AuthorizationSnapshot> ResolveAsync(IdentityState state, CancellationToken ct)
    {
        var snapshot = await cache.GetAsync(state, ct);
        if (snapshot is not null) return snapshot;
        snapshot = await identities.GetAuthorizationAsync(state, ct);
        await cache.SetAsync(state, snapshot, ct);
        return snapshot;
    }
}
public sealed class AuthService(IIdentityRepository identities, PermissionService permissions, IPasswordService passwords, ITokenService tokens)
{
    // A real PBKDF2 hash keeps the unknown-user path comparable to password verification.
    private static readonly string DummyHash = "pbkdf2-sha256$600000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        Validation.Text(request.TenantCode, 64, "tenantCode");
        Validation.Text(request.Username, 64, "username");
        Validation.Text(request.Password, 256, "password");
        var identity = await identities.FindLoginAsync(request.TenantCode, request.Username, ct);
        var valid = passwords.Verify(request.Password, identity?.PasswordHash ?? DummyHash);
        if (identity is null || !valid) throw new AppException(401, "Invalid tenant or credentials.");
        var snapshot = await permissions.ResolveAsync(identity.State, ct);
        var token = tokens.Create(identity.State, snapshot);
        return new(token.Token, token.ExpiresAt, snapshot.UserId, snapshot.TenantId, snapshot.DeptId, snapshot.Roles, snapshot.Permissions);
    }
}
public sealed class AdminService(IAdminRepository repository, ICurrentUser current, IPasswordService passwords)
{
    public Task<Page<UserDto>> UsersAsync(int page, int size, string? search, CancellationToken ct)
    {
        Validation.Paging(page, size);
        if (search?.Length > 64) throw new AppException(400, "Search is too long.");
        return repository.UsersAsync(current.Snapshot, page, size, search, ct);
    }
    public Task<UserDto> SaveUserAsync(Guid? id, UserRequest r, CancellationToken ct)
    {
        Validation.Text(r.Username, 64, "username"); Validation.Text(r.Nickname, 100, "nickname");
        Validation.Ids(r.RoleIds); Validation.Ids(r.PostIds);
        if (r.Email?.Length > 254 || r.Mobile?.Length > 32) throw new AppException(400, "Contact field is too long.");
        if (r.DeptId == Guid.Empty) throw new AppException(400, "Department is required.");
        string? hash = null;
        if (id is null || r.Password is not null)
        {
            if (r.Password is null || r.Password.Length < 12 || r.Password.Length > 256) throw new AppException(400, "Password length must be 12-256.");
            hash = passwords.Hash(r.Password);
        }
        return repository.SaveUserAsync(current.Snapshot, id, r, hash, ct);
    }
    public Task DeleteUserAsync(Guid id, CancellationToken ct) => repository.DeleteUserAsync(current.Snapshot, id, ct);
    public async Task<List<DepartmentDto>> DepartmentsAsync(CancellationToken ct)
    {
        var list = await repository.DepartmentsAsync(current.Snapshot, ct);
        var map = list.ToDictionary(x => x.Id);
        foreach (var node in list) if (node.ParentId is { } parent && map.TryGetValue(parent, out var p)) p.Children.Add(node);
        return list.Where(x => x.ParentId is null || !map.ContainsKey(x.ParentId.Value)).ToList();
    }
    public Task<DepartmentDto> SaveDepartmentAsync(Guid? id, DepartmentRequest r, CancellationToken ct)
    {
        Validation.Text(r.Name, 100, "name");
        if (r.OrganizationId == Guid.Empty) throw new AppException(400, "Organization is required.");
        return repository.SaveDepartmentAsync(current.Snapshot, id, r, ct);
    }
    public Task DeleteDepartmentAsync(Guid id, CancellationToken ct) => repository.DeleteDepartmentAsync(current.Snapshot, id, ct);
    public Task<List<CatalogDto>> CatalogAsync(string kind, CancellationToken ct) => repository.CatalogAsync(current.Snapshot.TenantId, kind, ct);
    public Task<CatalogDto> SaveCatalogAsync(string kind, Guid? id, CatalogRequest r, CancellationToken ct)
    {
        Validation.Text(r.Name, 100, "name"); Validation.Text(r.Code, 64, "code");
        return repository.SaveCatalogAsync(current.Snapshot.TenantId, kind, id, r, ct);
    }
    public Task DeleteCatalogAsync(string kind, Guid id, CancellationToken ct) => repository.DeleteCatalogAsync(current.Snapshot.TenantId, kind, id, ct);
    public async Task<List<MenuDto>> MenusAsync(bool assignedOnly, CancellationToken ct)
    {
        var list = await repository.MenusAsync(current.Snapshot, assignedOnly, ct);
        var map = list.ToDictionary(x => x.Id);
        foreach (var node in list) if (node.ParentId is { } parent && map.TryGetValue(parent, out var p)) p.Children.Add(node);
        return list.Where(x => x.ParentId is null || !map.ContainsKey(x.ParentId.Value)).ToList();
    }
    public Task<MenuDto> SaveMenuAsync(Guid? id, MenuRequest r, CancellationToken ct)
    {
        Validation.Text(r.Name, 100, "name");
        if (!Enum.IsDefined(r.Type) || r.Path?.Length > 256 || r.Component?.Length > 256) throw new AppException(400, "Invalid menu.");
        if (r.PermissionCode is { Length: > 0 } && !Aether.Domain.ValueObjects.PermissionCatalog.Codes.Contains(r.PermissionCode)) throw new AppException(400, "Unknown permission code.");
        return repository.SaveMenuAsync(current.Snapshot.TenantId, id, r, ct);
    }
    public Task DeleteMenuAsync(Guid id, CancellationToken ct) => repository.DeleteMenuAsync(current.Snapshot.TenantId, id, ct);
    public Task<GrantDto> GrantsAsync(Guid role, CancellationToken ct) => repository.GrantsAsync(current.Snapshot.TenantId, role, ct);
    public Task SetGrantsAsync(Guid role, GrantRequest r, CancellationToken ct)
    {
        Validation.Ids(r.MenuIds); Validation.Ids(r.DepartmentIds);
        if (!Enum.IsDefined(r.ScopeType)) throw new AppException(400, "Invalid data scope.");
        if (r.ScopeType != Aether.Domain.Enums.DataScopeType.Custom && r.DepartmentIds.Length != 0) throw new AppException(400, "Only CUSTOM accepts departments.");
        return repository.SetGrantsAsync(current.Snapshot.TenantId, role, r, ct);
    }
    public Task<Page<AuditDto>> AuditsAsync(int page, int size, CancellationToken ct)
    {
        Validation.Paging(page, size); return repository.AuditsAsync(current.Snapshot.TenantId, page, size, ct);
    }
}
public static class Validation
{
    public static void Text(string? value, int max, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max || value.Any(char.IsControl)) throw new AppException(400, $"Invalid {field}.");
    }
    public static void Ids(Guid[]? ids)
    {
        if (ids is null || ids.Length > 500 || ids.Contains(Guid.Empty) || ids.Distinct().Count() != ids.Length) throw new AppException(400, "Invalid ID collection.");
    }
    public static void Paging(int page, int size)
    {
        if (page < 1 || page > 100000 || size < 1 || size > 100) throw new AppException(400, "Invalid pagination.");
    }
}
