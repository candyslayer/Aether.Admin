# Aether.Admin

.NET 10 enterprise administration API with explicit Clean Architecture layers, PostgreSQL tenant isolation, organization/department trees, users, posts, roles, menus, data scopes and audit records. No MVC controllers, assembly scanning, dynamic mapper registration or lazy loading.

## Implementation boundary

EF Core 10 owns the Code First schema, explicit configurations, migrations and generated compiled model. Normal builds explicitly register `AppDbContextModel.Instance`. The Native AOT build excludes EF tooling and uses parameterized Npgsql repositories with explicit reader mapping. This is intentional: [Microsoft documents EF Native AOT and query precompilation as experimental](https://learn.microsoft.com/en-us/ef/core/performance/nativeaot-and-precompiled-queries), with no dynamic query composition support. This project therefore does **not** claim production-ready EF LINQ execution inside Native AOT.

The permission model follows [RuoYi-Vue-Pro](https://github.com/YunaiV/ruoyi-vue-pro): role/menu grants and ALL, CUSTOM, DEPT, DEPT_CHILD, SELF scopes. Tenant administrators remain restricted to their own tenant. An administrator must provision each tenant; there is no public cross-tenant administrator API. Workflow, approvals and notifications are future modules, not implemented features.

## Start locally

On the configured local workstation, the API's Development profile loads PostgreSQL, Redis and JWT settings from .NET User Secrets (`aether-admin-local-development`). Run the Api project from the IDE or `dotnet run --project src/Api`; the default profile opens `http://127.0.0.1:5072/openapi/v1.json`. Secrets live outside the repository. Environment variables and command-line settings override the local secrets. Other workstations must configure their own secrets or use the environment setup below.

Install .NET 10 SDK. Supply secrets through process environment variables or a secret manager. Do not put database credentials in committed configuration.

```powershell
$env:AETHER_DATABASE = 'Host=localhost;Database=aether_admin;Username=postgres;Password=YOUR_SECRET'
$env:AETHER_ADMIN_PASSWORD = 'YOUR_12_TO_256_CHARACTER_PASSWORD'
dotnet tool restore
dotnet run --project tools/DbAdmin -- create-database
dotnet run --project tools/DbAdmin -- apply-sql deploy/init.sql
dotnet run --project tools/DbAdmin -- bootstrap
$env:ConnectionStrings__Database = $env:AETHER_DATABASE
$env:ConnectionStrings__Redis = 'localhost:6379,password=YOUR_REDIS_SECRET'
$env:Jwt__Key = 'YOUR_RANDOM_SIGNING_KEY_AT_LEAST_32_BYTES'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/Api --no-launch-profile --urls http://127.0.0.1:5080
```

Bootstrap creates a tenant, headquarters organization/department, `admin` user and protected ADMIN role. It refuses to overwrite an existing tenant. Set `AETHER_TENANT_CODE` to provision another tenant. `apply-sql` runs the generated idempotent migration script; migrations run separately from API startup.

OpenAPI: `/openapi/v1.json` in Development. Health endpoints: `/health/live` and `/health/ready`. Redis is optional; absence/failure falls back to authoritative database reads and readiness reports Degraded. Set `OTEL_EXPORTER_OTLP_ENDPOINT` to export traces/metrics.

## Verification

```powershell
dotnet build
./tests/integration.ps1 -BaseUrl http://127.0.0.1:5080
# After provisioning a second tenant with the same test admin password:
./tests/integration.ps1 -BaseUrl http://127.0.0.1:5080 -OtherTenantCode isolation-test
dotnet list package --vulnerable --include-transitive
```

Tests exercise live PostgreSQL and the configured Redis cache, all five scopes, permission revocation while reusing a JWT, password revocation, forbidden writes, parent-cycle checks, stale versions, cross-tenant references and audit persistence. They create uniquely named test resources and delete those resources after successful completion. Failed runs retain their resources for diagnosis. See [curl examples](docs/curl.md).

## Data and security contracts

- API responses use `{ code, message, data }`; successful code is 0. Errors use HTTP status codes and matching response codes. Health/OpenAPI retain framework formats.
- All tenant-owned relations use composite tenant/entity foreign keys. Every runtime query includes tenant scope; ALL never removes tenant isolation.
- Role data scopes combine by union. SELF means the signed-in user's record for user management. DEPT_CHILD uses recursive SQL. CUSTOM uses `sys_role_depts`. No active role means no data scope.
- JWT carries userId/tenantId/deptId/roles/permissions and a security stamp. Each authenticated request revalidates tenant expiry, user status, main department and organization status in PostgreSQL. Authorization decisions use current grants rather than trusting embedded JWT permission claims.
- Tenant permission version increments transactionally with administrative writes; Redis keys include tenant, permission version, user and security stamp. Cache expiration is five minutes. Password changes revoke existing tokens.
- Security configuration writes and role binding require tenant ADMIN. Custom roles can receive user CRUD permissions with data-scope enforcement. System roles are immutable; protected administrator membership cannot be deleted/disabled through user CRUD.
- Writes lock the tenant row to serialize hierarchy/grant operations and use entity versions for optimistic updates. This prioritizes consistency; very large SaaS tenants may need finer-grained locking after load testing.
- PBKDF2-SHA256 uses 600,000 iterations and a random 128-bit salt. Login is rate limited per direct peer IP. Behind a proxy, configure trusted forwarded headers explicitly; never trust arbitrary forwarded IPs.
- Audit records retain route parameters, result, identity, IP, trace and duration. Request bodies/query strings are intentionally omitted to avoid password/PII leakage. Anonymous login failures are structured request logs, not tenant audit rows. Audit persistence errors are logged; there is no durable outbox yet.
- `sys_user_depts` reserves additional department membership; this release uses a single main department. Admin authorization is tenant-wide, not organization-wide.

## Schema regeneration

```powershell
dotnet ef migrations add ChangeName --project src/Infrastructure --output-dir Persistence/Migrations
dotnet ef dbcontext optimize --project src/Infrastructure --output-dir Persistence/Compiled --namespace Aether.Infrastructure.Persistence.Compiled
dotnet ef migrations script --project src/Infrastructure --idempotent --output deploy/init.sql
```

Always regenerate the model and migration script after entity/configuration changes. No API-side automatic migrations.

## Native deployment

Linux Native AOT must be compiled on Linux with clang and zlib development headers:

```bash
dotnet publish src/Api -r linux-x64 -c Release -p:PublishAot=true
docker compose --env-file .env up --build -d
```

Populate `.env` using the variable names in `.env.example`. Compose exposes only the API on loopback; PostgreSQL/Redis stay on the private network. Initial schema runs only for a new PostgreSQL volume. Bootstrap the first tenant using DbAdmin with an administrative connection from a trusted environment. Use a separate least-privilege runtime database role in production, terminate HTTPS at the reverse proxy, and establish backups, retention and secret rotation before deployment.

`Dockerfile` builds the Linux native executable. CI publishes Linux AOT and runs the integration script against that executable and Redis. Native Serilog 4.3 emits one known IL2067 warning from its internal reflection destructurer; an explicit scalar destructuring policy prevents that path in this application. This diagnostic remains visible and is the only trimming diagnostic permitted as a warning. Other AOT diagnostics remain errors.

This repository supplies an implemented and tested permission core. Production acceptance still requires Linux container execution, load testing, backup/recovery validation and operational audit guarantees; a passing ordinary build alone is not a production certification.
