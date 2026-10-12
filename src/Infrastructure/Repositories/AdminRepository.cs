using System.Data.Common;
using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using Aether.Domain.Enums;
using Npgsql;

namespace Aether.Infrastructure.Repositories;

public sealed class AdminRepository(NpgsqlDataSource source) : IAdminRepository, IAuditRepository
{
    private static UserDto User(DbDataReader r) => new(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetGuid(3), Sql.String(r, 4), Sql.String(r, 5), r.GetBoolean(6), r.GetInt64(7), r.GetFieldValue<Guid[]>(8), r.GetFieldValue<Guid[]>(9));
    private const string UserColumns = """
        u.id,u.username,u.nickname,u.dept_id,u.email,u.mobile,u.status,u.version,
        ARRAY(SELECT role_id FROM sys_user_roles WHERE tenant_id=u.tenant_id AND user_id=u.id),
        ARRAY(SELECT post_id FROM sys_user_posts WHERE tenant_id=u.tenant_id AND user_id=u.id)
        """;
    private static DepartmentDto Dept(DbDataReader r) => new(r.GetGuid(0), r.GetString(1), r.GetGuid(2), Sql.Guid(r, 3), r.GetString(4), Sql.Guid(r, 5), r.GetInt32(6), r.GetBoolean(7), r.GetInt64(8), []);
    private const string DeptColumns = "id,name,organization_id,parent_id,tree_path,leader_id,sort,status,version";
    private static MenuDto Menu(DbDataReader r) => new(r.GetGuid(0), r.GetString(1), (MenuType)r.GetInt32(2), Sql.Guid(r, 3), Sql.String(r, 4), Sql.String(r, 5), Sql.String(r, 6), r.GetInt32(7), r.GetBoolean(8), r.GetBoolean(9), r.GetInt64(10), []);
    private const string MenuColumns = "id,name,type,parent_id,path,component,permission_code,sort,visible,status,version";
    private static string Table(string kind) => kind switch { "roles" => "sys_role", "posts" => "sys_post", "organizations" => "sys_organization", _ => throw new ArgumentException("Unknown catalog.") };
    private static async Task Lock(NpgsqlConnection c, NpgsqlTransaction tx, Guid tenant, CancellationToken ct)
    {
        _ = await Sql.Scalar<Guid>(c, tx, "SELECT id FROM sys_tenant WHERE id=@tenant AND status AND (expire_time IS NULL OR expire_time>now()) FOR UPDATE", ct, ("tenant", tenant));
    }
    private static async Task Commit(NpgsqlConnection c, NpgsqlTransaction tx, Guid tenant, CancellationToken ct)
    {
        await Sql.Execute(c, tx, "UPDATE sys_tenant SET permission_version=permission_version+1,updated_at=now() WHERE id=@tenant", ct, ("tenant", tenant));
        await tx.CommitAsync(ct);
    }
    private static async Task ValidateIds(NpgsqlConnection c, NpgsqlTransaction tx, string table, Guid tenant, Guid[] ids, CancellationToken ct)
    {
        var count = await Sql.Scalar<long>(c, tx, $"SELECT count(*) FROM {table} WHERE tenant_id=@tenant AND id=ANY(@ids) AND status", ct, ("tenant", tenant), ("ids", ids));
        if (count != ids.Length) throw new AppException(400, "Reference is inactive, missing or belongs to another tenant.");
    }
    private static void Changed(int count)
    {
        if (count == 0) throw new AppException(409, "Resource changed or does not exist; reload and retry.");
    }
    public async Task<Page<UserDto>> UsersAsync(AuthorizationSnapshot actor, int page, int size, string? search, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var args = DataPermissionService.Parameters(actor).Concat(new (string, object?)[] { ("search", search ?? ""), ("limit", size), ("offset", (page - 1) * size) }).ToArray();
        var predicate = DataPermissionService.UserPredicate + " AND (@search='' OR strpos(lower(u.username),lower(@search))>0 OR strpos(lower(u.nickname),lower(@search))>0)";
        var total = await Sql.Scalar<long>(c, null, "SELECT count(*) FROM sys_user u WHERE " + predicate, ct, args);
        var users = await Sql.Query(c, null, "SELECT " + UserColumns + " FROM sys_user u WHERE " + predicate + " ORDER BY u.created_at,u.id LIMIT @limit OFFSET @offset", User, ct, args);
        return new(users, total, page, size);
    }
    public async Task<UserDto> SaveUserAsync(AuthorizationSnapshot actor, Guid? id, UserRequest r, string? hash, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, actor.TenantId, ct);
        await ValidateIds(c, tx, "sys_dept", actor.TenantId, [r.DeptId], ct);
        await ValidateIds(c, tx, "sys_role", actor.TenantId, r.RoleIds, ct);
        await ValidateIds(c, tx, "sys_post", actor.TenantId, r.PostIds, ct);
        var userId = id ?? Guid.NewGuid();
        UserDto? original = null;
        if (id is not null)
        {
            original = (await Sql.Query(c, tx, "SELECT " + UserColumns + " FROM sys_user u WHERE u.tenant_id=@tenant AND u.id=@id", User, ct, ("tenant", actor.TenantId), ("id", userId))).SingleOrDefault() ?? throw new AppException(404, "User not found.");
            if (!DataPermissionService.CanAccessUser(actor, userId, original.DeptId)) throw new AppException(403, "User is outside your data scope.");
            if (original.DeptId != r.DeptId) DataPermissionService.DemandDepartment(actor, r.DeptId);
            if (!actor.IsAdministrator && !original.RoleIds.Order().SequenceEqual(r.RoleIds.Order())) throw new AppException(403, "Only administrators can bind roles.");
            var protectedUser = await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_user_roles ur JOIN sys_role r ON r.tenant_id=ur.tenant_id AND r.id=ur.role_id WHERE ur.tenant_id=@tenant AND ur.user_id=@id AND r.type=1)", ct, ("tenant", actor.TenantId), ("id", userId));
            if (protectedUser && !actor.IsAdministrator) throw new AppException(403, "Only administrators can modify a system administrator.");
            if (protectedUser && (!r.Status || !original.RoleIds.Order().SequenceEqual(r.RoleIds.Order()))) throw new AppException(409, "System administrator membership cannot be removed or disabled.");
        }
        else
        {
            DataPermissionService.DemandDepartment(actor, r.DeptId);
            if (!actor.IsAdministrator && r.RoleIds.Length != 0) throw new AppException(403, "Only administrators can bind roles.");
        }
        var args = new (string, object?)[] { ("id", userId), ("tenant", actor.TenantId), ("dept", r.DeptId), ("name", r.Username), ("hash", hash), ("nickname", r.Nickname), ("email", r.Email), ("mobile", r.Mobile), ("status", r.Status), ("stamp", Guid.NewGuid()), ("version", r.Version) };
        if (id is null)
            await Sql.Execute(c, tx, "INSERT INTO sys_user(id,tenant_id,dept_id,username,password_hash,nickname,email,mobile,status,security_stamp) VALUES(@id,@tenant,@dept,@name,@hash,@nickname,@email,@mobile,@status,@stamp)", ct, args);
        else
            Changed(await Sql.Execute(c, tx, "UPDATE sys_user SET dept_id=@dept,username=@name,password_hash=coalesce(@hash,password_hash),nickname=@nickname,email=@email,mobile=@mobile,status=@status,security_stamp=CASE WHEN @hash IS NOT NULL THEN @stamp ELSE security_stamp END,version=version+1,updated_at=now() WHERE tenant_id=@tenant AND id=@id AND version=@version", ct, args));
        await Sql.Execute(c, tx, "DELETE FROM sys_user_roles WHERE tenant_id=@tenant AND user_id=@id; DELETE FROM sys_user_posts WHERE tenant_id=@tenant AND user_id=@id", ct, ("tenant", actor.TenantId), ("id", userId));
        await Sql.Execute(c, tx, "INSERT INTO sys_user_roles SELECT @tenant,@id,unnest(@roles); INSERT INTO sys_user_posts SELECT @tenant,@id,unnest(@posts)", ct, ("tenant", actor.TenantId), ("id", userId), ("roles", r.RoleIds), ("posts", r.PostIds));
        await Commit(c, tx, actor.TenantId, ct);
        return new(userId, r.Username, r.Nickname, r.DeptId, r.Email, r.Mobile, r.Status, id is null ? 1 : r.Version + 1, r.RoleIds, r.PostIds);
    }
    public async Task DeleteUserAsync(AuthorizationSnapshot actor, Guid id, CancellationToken ct)
    {
        if (id == actor.UserId) throw new AppException(409, "Cannot delete the signed-in user.");
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, actor.TenantId, ct);
        var rows = await Sql.Query(c, tx, "SELECT id,dept_id FROM sys_user WHERE tenant_id=@tenant AND id=@id", r => (r.GetGuid(0), r.GetGuid(1)), ct, ("tenant", actor.TenantId), ("id", id));
        if (rows.Count == 0) throw new AppException(404, "User not found.");
        if (!DataPermissionService.CanAccessUser(actor, id, rows[0].Item2)) throw new AppException(403, "User is outside your data scope.");
        if (await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_user_roles ur JOIN sys_role r ON r.tenant_id=ur.tenant_id AND r.id=ur.role_id WHERE ur.tenant_id=@tenant AND ur.user_id=@id AND r.type=1)", ct, ("tenant", actor.TenantId), ("id", id))) throw new AppException(409, "System administrator cannot be deleted.");
        await Sql.Execute(c, tx, "DELETE FROM sys_user WHERE tenant_id=@tenant AND id=@id", ct, ("tenant", actor.TenantId), ("id", id));
        await Commit(c, tx, actor.TenantId, ct);
    }
    public async Task<List<DepartmentDto>> DepartmentsAsync(AuthorizationSnapshot actor, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        return await Sql.Query(c, null, "SELECT " + DeptColumns + " FROM sys_dept WHERE tenant_id=@tenant AND (@all OR id=ANY(@depts) OR (@self AND id=@dept)) ORDER BY sort,id", Dept, ct, ("tenant", actor.TenantId), ("all", actor.All), ("depts", actor.DepartmentIds), ("self", actor.Self), ("dept", actor.DeptId));
    }
    public async Task<DepartmentDto> SaveDepartmentAsync(AuthorizationSnapshot actor, Guid? id, DepartmentRequest r, CancellationToken ct)
    {
        DataPermissionService.DemandAdministrator(actor);
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, actor.TenantId, ct);
        await ValidateIds(c, tx, "sys_organization", actor.TenantId, [r.OrganizationId], ct);
        if (r.LeaderId is { } leader) await ValidateIds(c, tx, "sys_user", actor.TenantId, [leader], ct);
        var nodes = await Sql.Query(c, tx, "SELECT " + DeptColumns + " FROM sys_dept WHERE tenant_id=@tenant", Dept, ct, ("tenant", actor.TenantId));
        var deptId = id ?? Guid.NewGuid();
        var original = id is null ? null : nodes.SingleOrDefault(x => x.Id == id) ?? throw new AppException(404, "Department not found.");
        var parent = r.ParentId is null ? null : nodes.SingleOrDefault(x => x.Id == r.ParentId) ?? throw new AppException(400, "Parent not found.");
        if (parent is not null && (!parent.Status || parent.OrganizationId != r.OrganizationId)) throw new AppException(400, "Parent must be active in the same organization.");
        if (original is not null && parent is not null && parent.TreePath.StartsWith(original.TreePath, StringComparison.Ordinal)) throw new AppException(409, "Department cycle detected.");
        if (original is not null && original.OrganizationId != r.OrganizationId) throw new AppException(409, "Moving between organizations is not supported.");
        var path = (parent?.TreePath ?? "/") + deptId.ToString("D") + "/";
        if (path.Length > 4000) throw new AppException(400, "Department hierarchy is too deep.");
        var args = new (string, object?)[] { ("tenant", actor.TenantId), ("id", deptId), ("name", r.Name), ("org", r.OrganizationId), ("parent", r.ParentId), ("path", path), ("leader", r.LeaderId), ("sort", r.Sort), ("status", r.Status), ("version", r.Version) };
        if (id is null)
            await Sql.Execute(c, tx, "INSERT INTO sys_dept(id,tenant_id,name,organization_id,parent_id,tree_path,leader_id,sort,status) VALUES(@id,@tenant,@name,@org,@parent,@path,@leader,@sort,@status)", ct, args);
        else
        {
            Changed(await Sql.Execute(c, tx, "UPDATE sys_dept SET name=@name,parent_id=@parent,tree_path=@path,leader_id=@leader,sort=@sort,status=@status,version=version+1,updated_at=now() WHERE tenant_id=@tenant AND id=@id AND version=@version", ct, args));
            if (original!.TreePath != path)
            {
                if (nodes.Any(n => n.Id != deptId && n.TreePath.StartsWith(original.TreePath, StringComparison.Ordinal) && path.Length + n.TreePath.Length - original.TreePath.Length > 4000)) throw new AppException(400, "Moved subtree is too deep.");
                await Sql.Execute(c, tx, "UPDATE sys_dept SET tree_path=@new||substring(tree_path from @offset),version=version+1,updated_at=now() WHERE tenant_id=@tenant AND id<>@id AND starts_with(tree_path,@old)", ct, ("tenant", actor.TenantId), ("id", deptId), ("new", path), ("old", original.TreePath), ("offset", original.TreePath.Length + 1));
            }
        }
        await Commit(c, tx, actor.TenantId, ct);
        return new(deptId, r.Name, r.OrganizationId, r.ParentId, path, r.LeaderId, r.Sort, r.Status, id is null ? 1 : r.Version + 1, []);
    }
    public async Task DeleteDepartmentAsync(AuthorizationSnapshot actor, Guid id, CancellationToken ct)
    {
        DataPermissionService.DemandAdministrator(actor);
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, actor.TenantId, ct);
        if (await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_dept WHERE tenant_id=@tenant AND parent_id=@id) OR EXISTS(SELECT 1 FROM sys_user WHERE tenant_id=@tenant AND dept_id=@id)", ct, ("tenant", actor.TenantId), ("id", id))) throw new AppException(409, "Department still has children or users.");
        var count = await Sql.Execute(c, tx, "DELETE FROM sys_dept WHERE tenant_id=@tenant AND id=@id", ct, ("tenant", actor.TenantId), ("id", id));
        if (count == 0) throw new AppException(404, "Department not found.");
        await Commit(c, tx, actor.TenantId, ct);
    }
    public async Task<List<CatalogDto>> CatalogAsync(Guid tenant, string kind, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var sort = kind == "posts" ? "sort" : "0";
        return await Sql.Query(c, null, $"SELECT id,name,code,{sort},status,version FROM {Table(kind)} WHERE tenant_id=@tenant ORDER BY name,id", r => new CatalogDto(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetBoolean(4), r.GetInt64(5)), ct, ("tenant", tenant));
    }
    public async Task<CatalogDto> SaveCatalogAsync(Guid tenant, string kind, Guid? id, CatalogRequest r, CancellationToken ct)
    {
        if (kind == "roles" && r.Code.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)) throw new AppException(409, "ADMIN is reserved.");
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, tenant, ct);
        var resource = id ?? Guid.NewGuid();
        if (kind == "roles" && id is not null && await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_role WHERE tenant_id=@tenant AND id=@id AND type=1)", ct, ("tenant", tenant), ("id", resource))) throw new AppException(409, "System role is immutable.");
        var args = new (string, object?)[] { ("tenant", tenant), ("id", resource), ("name", r.Name), ("code", r.Code), ("sort", r.Sort), ("status", r.Status), ("version", r.Version) };
        var extraCols = kind == "roles" ? ",type" : kind == "posts" ? ",sort" : "";
        var extraVals = kind == "roles" ? ",2" : kind == "posts" ? ",@sort" : "";
        if (id is null)
        {
            await Sql.Execute(c, tx, $"INSERT INTO {Table(kind)}(id,tenant_id,name,code,status{extraCols}) VALUES(@id,@tenant,@name,@code,@status{extraVals})", ct, args);
            if (kind == "roles") await Sql.Execute(c, tx, "INSERT INTO sys_data_scope(id,tenant_id,role_id,scope_type) VALUES(@scope,@tenant,@id,5)", ct, ("scope", Guid.NewGuid()), ("tenant", tenant), ("id", resource));
        }
        else
            Changed(await Sql.Execute(c, tx, $"UPDATE {Table(kind)} SET name=@name,code=@code,status=@status,version=version+1,updated_at=now(){(kind == "posts" ? ",sort=@sort" : "")} WHERE tenant_id=@tenant AND id=@id AND version=@version", ct, args));
        await Commit(c, tx, tenant, ct);
        return new(resource, r.Name, r.Code, kind == "posts" ? r.Sort : 0, r.Status, id is null ? 1 : r.Version + 1);
    }
    public async Task DeleteCatalogAsync(Guid tenant, string kind, Guid id, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, tenant, ct);
        if (kind == "roles" && await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_role WHERE tenant_id=@tenant AND id=@id AND type=1)", ct, ("tenant", tenant), ("id", id))) throw new AppException(409, "System role is immutable.");
        var count = await Sql.Execute(c, tx, $"DELETE FROM {Table(kind)} WHERE tenant_id=@tenant AND id=@id", ct, ("tenant", tenant), ("id", id));
        if (count == 0) throw new AppException(404, "Resource not found.");
        await Commit(c, tx, tenant, ct);
    }
    public async Task<List<MenuDto>> MenusAsync(AuthorizationSnapshot actor, bool assignedOnly, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        if (!assignedOnly) return await Sql.Query(c, null, "SELECT " + MenuColumns + " FROM sys_menu WHERE tenant_id=@tenant ORDER BY sort,id", Menu, ct, ("tenant", actor.TenantId));
        // Include ancestors for routing, but expose permission codes only for directly assigned nodes.
        return await Sql.Query(c, null, """
            WITH RECURSIVE assigned AS (
                SELECT DISTINCT m.id FROM sys_menu m WHERE m.tenant_id=@tenant AND m.status AND
                (@admin OR EXISTS(SELECT 1 FROM sys_role_menus rm JOIN sys_role r ON r.id=rm.role_id AND r.tenant_id=rm.tenant_id AND r.status
                    JOIN sys_user_roles ur ON ur.role_id=r.id AND ur.tenant_id=r.tenant_id
                    WHERE rm.tenant_id=@tenant AND rm.menu_id=m.id AND ur.user_id=@user))
            ), tree AS (
                SELECT m.id,m.parent_id FROM sys_menu m JOIN assigned a ON m.id=a.id WHERE m.tenant_id=@tenant
                UNION SELECT p.id,p.parent_id FROM sys_menu p JOIN tree n ON n.parent_id=p.id WHERE p.tenant_id=@tenant AND p.status
            ) SELECT m.id,m.name,m.type,m.parent_id,m.path,m.component,
                CASE WHEN a.id IS NOT NULL THEN m.permission_code ELSE NULL END,m.sort,m.visible,m.status,m.version
            FROM sys_menu m JOIN tree t ON t.id=m.id LEFT JOIN assigned a ON a.id=m.id WHERE m.tenant_id=@tenant ORDER BY m.sort,m.id
            """, Menu, ct, ("tenant", actor.TenantId), ("admin", actor.IsAdministrator), ("user", actor.UserId));
    }
    public async Task<MenuDto> SaveMenuAsync(Guid tenant, Guid? id, MenuRequest r, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, tenant, ct);
        var nodes = await Sql.Query(c, tx, "SELECT " + MenuColumns + " FROM sys_menu WHERE tenant_id=@tenant", Menu, ct, ("tenant", tenant));
        var menuId = id ?? Guid.NewGuid();
        if (id is not null && nodes.All(x => x.Id != id)) throw new AppException(404, "Menu not found.");
        var parent = r.ParentId;
        var visited = new HashSet<Guid> { menuId };
        while (parent is { } p)
        {
            if (!visited.Add(p)) throw new AppException(409, "Menu cycle detected.");
            var node = nodes.SingleOrDefault(x => x.Id == p) ?? throw new AppException(400, "Menu parent not found.");
            if (p == r.ParentId && node.Type == MenuType.Button) throw new AppException(400, "Buttons cannot have children.");
            parent = node.ParentId;
        }
        if (r.Type == MenuType.Button && (r.ParentId is null || nodes.Any(x => x.ParentId == menuId) || string.IsNullOrWhiteSpace(r.PermissionCode))) throw new AppException(400, "Button needs a parent and permission, and cannot have children.");
        var args = new (string, object?)[] { ("tenant", tenant), ("id", menuId), ("name", r.Name), ("type", (int)r.Type), ("parent", r.ParentId), ("path", r.Path), ("component", r.Component), ("permission", string.IsNullOrWhiteSpace(r.PermissionCode) ? null : r.PermissionCode), ("sort", r.Sort), ("visible", r.Visible), ("status", r.Status), ("version", r.Version) };
        if (id is null) await Sql.Execute(c, tx, "INSERT INTO sys_menu(id,tenant_id,name,type,parent_id,path,component,permission_code,sort,visible,status) VALUES(@id,@tenant,@name,@type,@parent,@path,@component,@permission,@sort,@visible,@status)", ct, args);
        else Changed(await Sql.Execute(c, tx, "UPDATE sys_menu SET name=@name,type=@type,parent_id=@parent,path=@path,component=@component,permission_code=@permission,sort=@sort,visible=@visible,status=@status,version=version+1,updated_at=now() WHERE tenant_id=@tenant AND id=@id AND version=@version", ct, args));
        await Commit(c, tx, tenant, ct);
        return new(menuId, r.Name, r.Type, r.ParentId, r.Path, r.Component, r.PermissionCode, r.Sort, r.Visible, r.Status, id is null ? 1 : r.Version + 1, []);
    }
    public async Task DeleteMenuAsync(Guid tenant, Guid id, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, tenant, ct);
        var count = await Sql.Execute(c, tx, "DELETE FROM sys_menu WHERE tenant_id=@tenant AND id=@id", ct, ("tenant", tenant), ("id", id));
        if (count == 0) throw new AppException(404, "Menu not found.");
        await Commit(c, tx, tenant, ct);
    }
    public async Task<GrantDto> GrantsAsync(Guid tenant, Guid role, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var scopes = await Sql.Query(c, null, "SELECT scope_type FROM sys_data_scope WHERE tenant_id=@tenant AND role_id=@role", r => (DataScopeType)r.GetInt32(0), ct, ("tenant", tenant), ("role", role));
        if (scopes.Count == 0) throw new AppException(404, "Role data scope not found.");
        var menus = await Sql.Query(c, null, "SELECT menu_id FROM sys_role_menus WHERE tenant_id=@tenant AND role_id=@role", r => r.GetGuid(0), ct, ("tenant", tenant), ("role", role));
        var depts = await Sql.Query(c, null, "SELECT dept_id FROM sys_role_depts WHERE tenant_id=@tenant AND role_id=@role", r => r.GetGuid(0), ct, ("tenant", tenant), ("role", role));
        return new(menus.ToArray(), scopes[0], depts.ToArray());
    }
    public async Task SetGrantsAsync(Guid tenant, Guid role, GrantRequest r, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct); await using var tx = await c.BeginTransactionAsync(ct);
        await Lock(c, tx, tenant, ct);
        await ValidateIds(c, tx, "sys_role", tenant, [role], ct);
        if (await Sql.Scalar<bool>(c, tx, "SELECT EXISTS(SELECT 1 FROM sys_role WHERE tenant_id=@tenant AND id=@role AND type=1)", ct, ("tenant", tenant), ("role", role))) throw new AppException(409, "System role is immutable.");
        await ValidateIds(c, tx, "sys_menu", tenant, r.MenuIds, ct);
        await ValidateIds(c, tx, "sys_dept", tenant, r.DepartmentIds, ct);
        await Sql.Execute(c, tx, "DELETE FROM sys_role_menus WHERE tenant_id=@tenant AND role_id=@role; DELETE FROM sys_role_depts WHERE tenant_id=@tenant AND role_id=@role", ct, ("tenant", tenant), ("role", role));
        await Sql.Execute(c, tx, "INSERT INTO sys_role_menus SELECT @tenant,@role,unnest(@menus); INSERT INTO sys_role_depts SELECT @tenant,@role,unnest(@depts); UPDATE sys_data_scope SET scope_type=@scope,updated_at=now(),version=version+1 WHERE tenant_id=@tenant AND role_id=@role", ct, ("tenant", tenant), ("role", role), ("menus", r.MenuIds), ("depts", r.DepartmentIds), ("scope", (int)r.ScopeType));
        await Commit(c, tx, tenant, ct);
    }
    public async Task<Page<AuditDto>> AuditsAsync(Guid tenant, int page, int size, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        var total = await Sql.Scalar<long>(c, null, "SELECT count(*) FROM sys_audit_log WHERE tenant_id=@tenant", ct, ("tenant", tenant));
        var logs = await Sql.Query(c, null, "SELECT id,user_id,method,endpoint,parameters::text,ip,trace_id,result,duration_ms,created_at FROM sys_audit_log WHERE tenant_id=@tenant ORDER BY created_at DESC,id LIMIT @size OFFSET @offset", r => new AuditDto(r.GetGuid(0), Sql.Guid(r, 1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetString(5), r.GetString(6), r.GetInt32(7), r.GetInt64(8), Sql.Time(r, 9)), ct, ("tenant", tenant), ("size", size), ("offset", (page - 1) * size));
        return new(logs, total, page, size);
    }
    public async Task AppendAsync(AuditEntry entry, CancellationToken ct)
    {
        await using var c = await source.OpenConnectionAsync(ct);
        await Sql.Execute(c, null, "INSERT INTO sys_audit_log(id,tenant_id,user_id,method,endpoint,parameters,ip,trace_id,result,duration_ms) VALUES(@id,@tenant,@user,@method,@endpoint,@parameters::jsonb,@ip,@trace,@result,@duration)", ct,
            ("id", Guid.NewGuid()), ("tenant", entry.TenantId), ("user", entry.UserId), ("method", entry.Method), ("endpoint", entry.Endpoint), ("parameters", entry.Parameters), ("ip", entry.Ip), ("trace", entry.TraceId), ("result", entry.Result), ("duration", entry.DurationMs));
    }
}
