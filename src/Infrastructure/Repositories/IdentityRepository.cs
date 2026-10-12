using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using Aether.Domain.ValueObjects;
using Npgsql;

namespace Aether.Infrastructure.Repositories;

public sealed class IdentityRepository(NpgsqlDataSource source) : IIdentityRepository
{
    private const string Active = "u.status AND t.status AND d.status AND o.status AND (t.expire_time IS NULL OR t.expire_time > now())";
    private const string Join = " FROM sys_user u JOIN sys_tenant t ON t.id=u.tenant_id JOIN sys_dept d ON d.tenant_id=u.tenant_id AND d.id=u.dept_id JOIN sys_organization o ON o.tenant_id=d.tenant_id AND o.id=d.organization_id ";
    public async Task<LoginIdentity?> FindLoginAsync(string tenantCode, string username, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var rows = await Sql.Query(c, null, "SELECT u.id,u.tenant_id,u.dept_id,u.security_stamp,t.permission_version,u.password_hash" + Join + " WHERE t.code=@code AND u.username=@name AND " + Active,
            r => new LoginIdentity(new(r.GetGuid(0), r.GetGuid(1), r.GetGuid(2), r.GetGuid(3), r.GetInt64(4)), r.GetString(5)), ct, ("code", tenantCode), ("name", username));
        return rows.SingleOrDefault();
    }
    public async Task<IdentityState?> FindActiveAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var rows = await Sql.Query(c, null, "SELECT u.id,u.tenant_id,u.dept_id,u.security_stamp,t.permission_version" + Join + " WHERE t.id=@tenant AND u.id=@user AND " + Active,
            r => new IdentityState(r.GetGuid(0), r.GetGuid(1), r.GetGuid(2), r.GetGuid(3), r.GetInt64(4)), ct, ("tenant", tenantId), ("user", userId));
        return rows.SingleOrDefault();
    }
    public async Task<AuthorizationSnapshot> GetAuthorizationAsync(IdentityState state, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var roles = await Sql.Query(c, tx, """
            SELECT r.id,r.code,r.type,coalesce(s.scope_type,5) FROM sys_role r
            JOIN sys_user_roles ur ON ur.tenant_id=r.tenant_id AND ur.role_id=r.id
            LEFT JOIN sys_data_scope s ON s.tenant_id=r.tenant_id AND s.role_id=r.id
            WHERE r.tenant_id=@tenant AND ur.user_id=@user AND r.status
            """, r => (Id: r.GetGuid(0), Code: r.GetString(1), Type: r.GetInt32(2), Scope: r.GetInt32(3)), ct, ("tenant", state.TenantId), ("user", state.UserId));
        var admin = roles.Any(r => r.Code == "ADMIN" && r.Type == 1);
        var permissions = admin ? PermissionCatalog.Codes.ToList() : await Sql.Query(c, tx, """
            SELECT DISTINCT m.permission_code FROM sys_menu m
            JOIN sys_role_menus rm ON rm.tenant_id=m.tenant_id AND rm.menu_id=m.id
            JOIN sys_role r ON r.tenant_id=rm.tenant_id AND r.id=rm.role_id AND r.status
            JOIN sys_user_roles ur ON ur.tenant_id=r.tenant_id AND ur.role_id=r.id
            WHERE m.tenant_id=@tenant AND ur.user_id=@user AND m.status AND m.permission_code IS NOT NULL
            """, r => r.GetString(0), ct, ("tenant", state.TenantId), ("user", state.UserId));
        var depts = new HashSet<Guid>();
        if (roles.Any(r => r.Scope == 3)) depts.Add(state.DeptId);
        if (roles.Any(r => r.Scope == 4))
        {
            var descendants = await Sql.Query(c, tx, """
                WITH RECURSIVE tree AS (
                    SELECT id FROM sys_dept WHERE tenant_id=@tenant AND id=@dept
                    UNION ALL SELECT d.id FROM sys_dept d JOIN tree p ON d.parent_id=p.id WHERE d.tenant_id=@tenant
                ) SELECT id FROM tree
                """, r => r.GetGuid(0), ct, ("tenant", state.TenantId), ("dept", state.DeptId));
            depts.UnionWith(descendants);
        }
        var customRoles = roles.Where(r => r.Scope == 2).Select(r => r.Id).ToArray();
        if (customRoles.Length != 0)
        {
            var custom = await Sql.Query(c, tx, "SELECT dept_id FROM sys_role_depts WHERE tenant_id=@tenant AND role_id=ANY(@roles)", r => r.GetGuid(0), ct, ("tenant", state.TenantId), ("roles", customRoles));
            depts.UnionWith(custom);
        }
        await tx.CommitAsync(ct);
        return new(state.UserId, state.TenantId, state.DeptId, admin, roles.Select(r => r.Code).ToArray(), permissions.ToArray(), admin || roles.Any(r => r.Scope == 1), roles.Any(r => r.Scope == 5), depts.ToArray());
    }
}
