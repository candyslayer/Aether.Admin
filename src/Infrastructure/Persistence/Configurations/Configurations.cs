using Aether.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aether.Infrastructure.Persistence.Configurations;

internal static class Configuration
{
    public static void Base<T>(EntityTypeBuilder<T> b, string table) where T : TenantEntity
    {
        b.ToTable(table); b.HasKey(x => x.Id); b.HasAlternateKey(x => new { x.TenantId, x.Id });
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.TenantId).HasColumnName("tenant_id");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        b.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1L).IsConcurrencyToken();
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("sys_tenant"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(64);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Status).HasColumnName("status");
        b.Property(x => x.ExpireTime).HasColumnName("expire_time");
        b.Property(x => x.PermissionVersion).HasColumnName("permission_version").HasDefaultValue(1L);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }
}
public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        Configuration.Base(b, "sys_organization");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(64);
        b.Property(x => x.Status).HasColumnName("status");
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        Configuration.Base(b, "sys_dept");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.ParentId).HasColumnName("parent_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.TreePath).HasColumnName("tree_path").HasMaxLength(4000);
        b.Property(x => x.LeaderId).HasColumnName("leader_id");
        b.Property(x => x.Sort).HasColumnName("sort"); b.Property(x => x.Status).HasColumnName("status");
        b.HasAlternateKey(x => new { x.TenantId, x.OrganizationId, x.Id });
        b.HasOne<Organization>().WithMany().HasForeignKey(x => new { x.TenantId, x.OrganizationId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Department>().WithMany().HasForeignKey(x => new { x.TenantId, x.OrganizationId, x.ParentId }).HasPrincipalKey(x => new { x.TenantId, x.OrganizationId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => new { x.TenantId, x.LeaderId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantId, x.ParentId });
    }
}
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        Configuration.Base(b, "sys_user");
        b.Property(x => x.DeptId).HasColumnName("dept_id");
        b.Property(x => x.Username).HasColumnName("username").HasMaxLength(64);
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(256);
        b.Property(x => x.Nickname).HasColumnName("nickname").HasMaxLength(100);
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(254);
        b.Property(x => x.Mobile).HasColumnName("mobile").HasMaxLength(32);
        b.Property(x => x.Status).HasColumnName("status");
        b.Property(x => x.SecurityStamp).HasColumnName("security_stamp");
        b.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.DeptId });
        b.HasOne<Department>().WithMany().HasForeignKey(x => new { x.TenantId, x.DeptId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> b)
    {
        Configuration.Base(b, "sys_post");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(64);
        b.Property(x => x.Sort).HasColumnName("sort"); b.Property(x => x.Status).HasColumnName("status");
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        Configuration.Base(b, "sys_role");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(64);
        b.Property(x => x.Type).HasColumnName("type"); b.Property(x => x.Status).HasColumnName("status");
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> b)
    {
        Configuration.Base(b, "sys_menu");
        b.Property(x => x.ParentId).HasColumnName("parent_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
        b.Property(x => x.Type).HasColumnName("type");
        b.Property(x => x.Path).HasColumnName("path").HasMaxLength(256);
        b.Property(x => x.Component).HasColumnName("component").HasMaxLength(256);
        b.Property(x => x.PermissionCode).HasColumnName("permission_code").HasMaxLength(128);
        b.Property(x => x.Sort).HasColumnName("sort");
        b.Property(x => x.Visible).HasColumnName("visible"); b.Property(x => x.Status).HasColumnName("status");
        b.HasOne<Menu>().WithMany().HasForeignKey(x => new { x.TenantId, x.ParentId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantId, x.ParentId });
    }
}
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("sys_user_roles"); b.HasKey(x => new { x.TenantId, x.UserId, x.RoleId });
        b.Property(x => x.TenantId).HasColumnName("tenant_id"); b.Property(x => x.UserId).HasColumnName("user_id"); b.Property(x => x.RoleId).HasColumnName("role_id");
        b.HasOne<User>().WithMany().HasForeignKey(x => new { x.TenantId, x.UserId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Role>().WithMany().HasForeignKey(x => new { x.TenantId, x.RoleId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class UserPostConfiguration : IEntityTypeConfiguration<UserPost>
{
    public void Configure(EntityTypeBuilder<UserPost> b)
    {
        b.ToTable("sys_user_posts"); b.HasKey(x => new { x.TenantId, x.UserId, x.PostId });
        b.Property(x => x.TenantId).HasColumnName("tenant_id"); b.Property(x => x.UserId).HasColumnName("user_id"); b.Property(x => x.PostId).HasColumnName("post_id");
        b.HasOne<User>().WithMany().HasForeignKey(x => new { x.TenantId, x.UserId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Post>().WithMany().HasForeignKey(x => new { x.TenantId, x.PostId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class UserDepartmentConfiguration : IEntityTypeConfiguration<UserDepartment>
{
    public void Configure(EntityTypeBuilder<UserDepartment> b)
    {
        b.ToTable("sys_user_depts"); b.HasKey(x => new { x.TenantId, x.UserId, x.DeptId });
        b.Property(x => x.TenantId).HasColumnName("tenant_id"); b.Property(x => x.UserId).HasColumnName("user_id"); b.Property(x => x.DeptId).HasColumnName("dept_id");
        b.HasOne<User>().WithMany().HasForeignKey(x => new { x.TenantId, x.UserId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Department>().WithMany().HasForeignKey(x => new { x.TenantId, x.DeptId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> b)
    {
        b.ToTable("sys_role_menus"); b.HasKey(x => new { x.TenantId, x.RoleId, x.MenuId });
        b.Property(x => x.TenantId).HasColumnName("tenant_id"); b.Property(x => x.RoleId).HasColumnName("role_id"); b.Property(x => x.MenuId).HasColumnName("menu_id");
        b.HasOne<Role>().WithMany().HasForeignKey(x => new { x.TenantId, x.RoleId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Menu>().WithMany().HasForeignKey(x => new { x.TenantId, x.MenuId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class DataScopeConfiguration : IEntityTypeConfiguration<DataScope>
{
    public void Configure(EntityTypeBuilder<DataScope> b)
    {
        Configuration.Base(b, "sys_data_scope");
        b.Property(x => x.RoleId).HasColumnName("role_id"); b.Property(x => x.ScopeType).HasColumnName("scope_type");
        b.HasIndex(x => new { x.TenantId, x.RoleId }).IsUnique();
        b.HasOne<Role>().WithMany().HasForeignKey(x => new { x.TenantId, x.RoleId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
    }
}
public sealed class RoleDepartmentConfiguration : IEntityTypeConfiguration<RoleDepartment>
{
    public void Configure(EntityTypeBuilder<RoleDepartment> b)
    {
        b.ToTable("sys_role_depts"); b.HasKey(x => new { x.TenantId, x.RoleId, x.DeptId });
        b.Property(x => x.TenantId).HasColumnName("tenant_id"); b.Property(x => x.RoleId).HasColumnName("role_id"); b.Property(x => x.DeptId).HasColumnName("dept_id");
        b.HasOne<Role>().WithMany().HasForeignKey(x => new { x.TenantId, x.RoleId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Department>().WithMany().HasForeignKey(x => new { x.TenantId, x.DeptId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        Configuration.Base(b, "sys_audit_log");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.Method).HasColumnName("method").HasMaxLength(16);
        b.Property(x => x.Endpoint).HasColumnName("endpoint").HasMaxLength(512);
        b.Property(x => x.Parameters).HasColumnName("parameters").HasColumnType("jsonb");
        b.Property(x => x.Ip).HasColumnName("ip").HasMaxLength(64);
        b.Property(x => x.TraceId).HasColumnName("trace_id").HasMaxLength(64);
        b.Property(x => x.Result).HasColumnName("result"); b.Property(x => x.DurationMs).HasColumnName("duration_ms");
        b.HasIndex(x => new { x.TenantId, x.CreatedAt });
    }
}
