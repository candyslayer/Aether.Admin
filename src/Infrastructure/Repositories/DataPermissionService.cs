using Aether.Application.DTOs;
using Aether.Application.Interfaces;

namespace Aether.Infrastructure.Repositories;

public sealed class DataPermissionService
{
    // The tenant predicate is mandatory even for ALL and tenant administrators.
    public const string UserPredicate = "u.tenant_id=@tenant AND (@all OR (@self AND u.id=@actor) OR u.dept_id=ANY(@depts))";
    internal static (string Name, object? Value)[] Parameters(AuthorizationSnapshot actor) =>
        [("tenant", actor.TenantId), ("all", actor.All), ("self", actor.Self), ("actor", actor.UserId), ("depts", actor.DepartmentIds)];
    public static bool CanAccessUser(AuthorizationSnapshot actor, Guid id, Guid dept) => actor.All || (actor.Self && actor.UserId == id) || actor.DepartmentIds.Contains(dept);
    public static void DemandDepartment(AuthorizationSnapshot actor, Guid dept)
    {
        if (!actor.All && !actor.DepartmentIds.Contains(dept)) throw new AppException(403, "Department is outside your data scope.");
    }
    public static void DemandAdministrator(AuthorizationSnapshot actor)
    {
        if (!actor.IsAdministrator) throw new AppException(403, "Tenant administrator required for security configuration.");
    }
}
