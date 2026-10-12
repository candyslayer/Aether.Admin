# Local verification, 2026-10-10

- .NET SDK 10.0.401, Windows x64.
- `dotnet build`: zero warnings, zero errors.
- EF migration model check: no pending model changes.
- PostgreSQL at user-authorized LAN server: independent `aether_admin` database initialized using generated idempotent migration SQL.
- Redis at user-authorized LAN server: authenticated connection; ready health endpoint validates availability.
- Integration script: 55 HTTP/security assertions passed with Redis enabled, including two tenants, password token revocation and administrator takeover protection.
- PostgreSQL/Redis ready health endpoint: 200 Healthy. Generated OpenAPI endpoint: 200.
- Native publish `win-x64`: succeeded, 32,932,352-byte executable. One documented Serilog IL2067 diagnostic remains visible; scalar destructuring policy avoids that reflection path.
- Native executable startup: blocked by automatic approval policy; no native runtime test success is claimed.
- Linux Native AOT/Docker: not locally executed because Docker and a Linux environment are unavailable. The included CI workflow performs Linux native publish and native integration testing when run in CI.
- NuGet transitive vulnerability audit: no reported vulnerable packages at verification time.

Credentials are supplied through process environment variables and are not stored in tracked project files. The bootstrap password used for local validation is temporary and should be rotated before any deployment.
