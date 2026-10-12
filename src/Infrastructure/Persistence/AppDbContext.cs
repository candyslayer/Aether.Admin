using Aether.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Aether.Infrastructure.Persistence.Configurations;

namespace Aether.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfiguration(new TenantConfiguration());
        b.ApplyConfiguration(new OrganizationConfiguration());
        b.ApplyConfiguration(new DepartmentConfiguration());
        b.ApplyConfiguration(new UserConfiguration());
        b.ApplyConfiguration(new PostConfiguration());
        b.ApplyConfiguration(new RoleConfiguration());
        b.ApplyConfiguration(new MenuConfiguration());
        b.ApplyConfiguration(new UserRoleConfiguration());
        b.ApplyConfiguration(new UserPostConfiguration());
        b.ApplyConfiguration(new UserDepartmentConfiguration());
        b.ApplyConfiguration(new RoleMenuConfiguration());
        b.ApplyConfiguration(new DataScopeConfiguration());
        b.ApplyConfiguration(new RoleDepartmentConfiguration());
        b.ApplyConfiguration(new AuditLogConfiguration());
    }
}
// Design-time only. Runtime opens Npgsql connections and never builds or executes EF queries.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("AETHER_DATABASE") ?? "Host=localhost;Database=aether_admin;Username=postgres")
        .Options);
}
