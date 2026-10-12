using Aether.Security.Jwt;
using Aether.Domain.ValueObjects;
using Npgsql;

var connection = Environment.GetEnvironmentVariable("AETHER_DATABASE") ?? throw new InvalidOperationException("Set AETHER_DATABASE.");
var command = args.FirstOrDefault() ?? "check";
if (command == "create-database")
{
    var target = new NpgsqlConnectionStringBuilder(connection).Database;
    if (string.IsNullOrEmpty(target) || !target.StartsWith("aether_", StringComparison.Ordinal) || target.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch != '_')) throw new InvalidOperationException("Only aether_* database names are allowed.");
    var administration = new NpgsqlConnectionStringBuilder(connection) { Database = "postgres" };
    await using var admin = new NpgsqlConnection(administration.ConnectionString); await admin.OpenAsync();
    await using var exists = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname=@name)", admin);
    exists.Parameters.AddWithValue("name", target);
    if (!(bool)(await exists.ExecuteScalarAsync())!)
    {
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{target}\"", admin); await create.ExecuteNonQueryAsync();
        Console.WriteLine($"Created {target}.");
    }
    else Console.WriteLine($"Database {target} already exists; preserved.");
    return;
}
await using var c = new NpgsqlConnection(connection); await c.OpenAsync();
if (command == "check")
{
    await using var version = new NpgsqlCommand("SELECT version()", c); Console.WriteLine(await version.ExecuteScalarAsync()); return;
}
if (command == "apply-sql")
{
    var path = args.ElementAtOrDefault(1) ?? throw new InvalidOperationException("Provide SQL file.");
    await using var sql = new NpgsqlCommand(await File.ReadAllTextAsync(path), c); await sql.ExecuteNonQueryAsync(); Console.WriteLine("SQL applied."); return;
}
if (command != "bootstrap") throw new InvalidOperationException("Expected check, create-database, apply-sql or bootstrap.");
var tenantCode = Environment.GetEnvironmentVariable("AETHER_TENANT_CODE") ?? "default";
var password = Environment.GetEnvironmentVariable("AETHER_ADMIN_PASSWORD") ?? throw new InvalidOperationException("Set AETHER_ADMIN_PASSWORD (12-256 characters).");
if (password.Length < 12 || password.Length > 256) throw new InvalidOperationException("Invalid bootstrap password length.");
await using var tx = await c.BeginTransactionAsync();
await using (var tenantExists = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM sys_tenant WHERE code=@code)", c, tx))
{
    tenantExists.Parameters.AddWithValue("code", tenantCode);
    if ((bool)(await tenantExists.ExecuteScalarAsync())!) throw new InvalidOperationException("Tenant already exists; bootstrap never resets existing credentials.");
}
var tenant = Guid.NewGuid(); var org = Guid.NewGuid(); var dept = Guid.NewGuid(); var user = Guid.NewGuid(); var role = Guid.NewGuid();
async Task Execute(string sql, params (string Name, object Value)[] parameters)
{
    await using var cmd = new NpgsqlCommand(sql, c, tx);
    foreach (var (name, value) in parameters) cmd.Parameters.AddWithValue(name, value);
    await cmd.ExecuteNonQueryAsync();
}
await Execute("INSERT INTO sys_tenant(id,name,code,status) VALUES(@id,@name,@code,true)", ("id", tenant), ("name", "Aether " + tenantCode), ("code", tenantCode));
await Execute("INSERT INTO sys_organization(id,tenant_id,name,code,status) VALUES(@id,@tenant,'Headquarters','HQ',true)", ("id", org), ("tenant", tenant));
await Execute("INSERT INTO sys_dept(id,tenant_id,organization_id,name,tree_path,sort,status) VALUES(@id,@tenant,@org,'Headquarters',@path,0,true)", ("id", dept), ("tenant", tenant), ("org", org), ("path", "/" + dept + "/"));
await Execute("INSERT INTO sys_user(id,tenant_id,dept_id,username,password_hash,nickname,status,security_stamp) VALUES(@id,@tenant,@dept,'admin',@hash,'Administrator',true,@stamp)", ("id", user), ("tenant", tenant), ("dept", dept), ("hash", new PasswordService().Hash(password)), ("stamp", Guid.NewGuid()));
await Execute("INSERT INTO sys_role(id,tenant_id,name,code,type,status) VALUES(@id,@tenant,'Tenant administrator','ADMIN',1,true)", ("id", role), ("tenant", tenant));
await Execute("INSERT INTO sys_user_roles VALUES(@tenant,@user,@role)", ("tenant", tenant), ("user", user), ("role", role));
await Execute("INSERT INTO sys_data_scope(id,tenant_id,role_id,scope_type) VALUES(@id,@tenant,@role,1)", ("id", Guid.NewGuid()), ("tenant", tenant), ("role", role));
var root = Guid.NewGuid();
await Execute("INSERT INTO sys_menu(id,tenant_id,name,type,path,sort,visible,status) VALUES(@id,@tenant,'System',1,'/system',0,true,true)", ("id", root), ("tenant", tenant));
foreach (var code in PermissionCatalog.Codes)
{
    var menu = Guid.NewGuid();
    await Execute("INSERT INTO sys_menu(id,tenant_id,parent_id,name,type,permission_code,sort,visible,status) VALUES(@id,@tenant,@parent,@name,3,@code,0,true,true)", ("id", menu), ("tenant", tenant), ("parent", root), ("name", code), ("code", code));
    await Execute("INSERT INTO sys_role_menus VALUES(@tenant,@role,@menu)", ("tenant", tenant), ("role", role), ("menu", menu));
}
await tx.CommitAsync();
Console.WriteLine($"Bootstrapped tenant {tenantCode}: tenant={tenant}, organization={org}, department={dept}, user=admin.");
