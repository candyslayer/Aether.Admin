namespace Aether.Domain.ValueObjects;

public static class PermissionCatalog
{
    public static readonly string[] Codes =
    [
        "system:user:list", "system:user:create", "system:user:update", "system:user:delete",
        "system:dept:list", "system:dept:create", "system:dept:update", "system:dept:delete",
        "system:organization:list", "system:organization:create", "system:organization:update", "system:organization:delete",
        "system:post:list", "system:post:create", "system:post:update", "system:post:delete",
        "system:role:list", "system:role:create", "system:role:update", "system:role:delete",
        "system:menu:list", "system:menu:create", "system:menu:update", "system:menu:delete",
        "system:permission:assign", "system:audit:list"
    ];
}
