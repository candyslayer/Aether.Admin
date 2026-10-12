using Aether.Domain.Enums;

namespace Aether.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public bool Status { get; set; } = true;
    public DateTimeOffset? ExpireTime { get; set; }
    public long PermissionVersion { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
public abstract class TenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long Version { get; set; } = 1;
}
public sealed class Organization : TenantEntity
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public bool Status { get; set; } = true;
}
public sealed class Department : TenantEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = "";
    public string TreePath { get; set; } = "";
    public Guid? LeaderId { get; set; }
    public int Sort { get; set; }
    public bool Status { get; set; } = true;
}
public sealed class User : TenantEntity
{
    public Guid DeptId { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public bool Status { get; set; } = true;
    public Guid SecurityStamp { get; set; }
}
public sealed class Post : TenantEntity
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int Sort { get; set; }
    public bool Status { get; set; } = true;
}
public sealed class Role : TenantEntity
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public RoleType Type { get; set; } = RoleType.Custom;
    public bool Status { get; set; } = true;
}
public sealed class Menu : TenantEntity
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = "";
    public MenuType Type { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? PermissionCode { get; set; }
    public int Sort { get; set; }
    public bool Visible { get; set; } = true;
    public bool Status { get; set; } = true;
}
public sealed class UserRole { public Guid TenantId { get; set; } public Guid UserId { get; set; } public Guid RoleId { get; set; } }
public sealed class UserPost { public Guid TenantId { get; set; } public Guid UserId { get; set; } public Guid PostId { get; set; } }
public sealed class UserDepartment { public Guid TenantId { get; set; } public Guid UserId { get; set; } public Guid DeptId { get; set; } }
public sealed class RoleMenu { public Guid TenantId { get; set; } public Guid RoleId { get; set; } public Guid MenuId { get; set; } }
public sealed class DataScope : TenantEntity { public Guid RoleId { get; set; } public DataScopeType ScopeType { get; set; } }
public sealed class RoleDepartment { public Guid TenantId { get; set; } public Guid RoleId { get; set; } public Guid DeptId { get; set; } }
public sealed class AuditLog : TenantEntity
{
    public Guid? UserId { get; set; }
    public string Method { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Parameters { get; set; } = "";
    public string Ip { get; set; } = "";
    public string TraceId { get; set; } = "";
    public int Result { get; set; }
    public long DurationMs { get; set; }
}
