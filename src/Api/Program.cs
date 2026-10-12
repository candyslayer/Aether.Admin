using System.Threading.RateLimiting;
using Aether.Api;
using Aether.Api.Endpoints;
using Aether.Api.Middleware;
using Aether.Application.Interfaces;
using Aether.Application.Services;
#if !NATIVE_AOT
using Aether.Infrastructure.Persistence;
using Aether.Infrastructure.Persistence.Compiled;
using Microsoft.EntityFrameworkCore;
#endif
using Aether.Infrastructure.Redis;
using Aether.Infrastructure.Repositories;
using Aether.Security.Authorization;
using Aether.Security.Jwt;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateSlimBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets("aether-admin-local-development", reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);
}
var database = builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("ConnectionStrings__Database is required.");
var jwt = new JwtSettings(builder.Configuration["Jwt:Issuer"] ?? "aether-admin", builder.Configuration["Jwt:Audience"] ?? "aether-admin", builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt__Key is required."));
builder.Host.UseSerilog((_, configuration) => configuration.Destructure.With<ScalarLogPolicy>().WriteTo.Console().Enrich.FromLogContext());
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default));
builder.Services.AddOpenApi();
builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(database).Build());
#if !NATIVE_AOT
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(database).UseModel(AppDbContextModel.Instance));
#endif
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<IAdminRepository>(s => s.GetRequiredService<AdminRepository>());
builder.Services.AddScoped<IAuditRepository>(s => s.GetRequiredService<AdminRepository>());
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddSingleton<DataPermissionService>();
var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
IConnectionMultiplexer? redis = null;
if (!string.IsNullOrWhiteSpace(redisConfiguration))
{
    var options = ConfigurationOptions.Parse(redisConfiguration);
    options.AbortOnConnectFail = false;
    redis = await ConnectionMultiplexer.ConnectAsync(options);
    builder.Services.AddSingleton(redis);
}
builder.Services.AddSingleton<IAetherRedisState>(new AetherRedisState(redis));
builder.Services.AddSingleton<IAuthorizationCache>(new AuthorizationCache(redis));
builder.Services.AddAetherAuthentication(jwt);
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql").AddCheck<RedisHealthCheck>("redis");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
    { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddOpenTelemetry().ConfigureResource(r => r.AddService("Aether.Admin"))
    .WithTracing(t => { t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation(); if (builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is not null) t.AddOtlpExporter(); })
    .WithMetrics(m => { m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation(); if (builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is not null) m.AddOtlpExporter(); });
var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapDeptEndpoints();
app.MapRoleEndpoints();
app.MapPostEndpoints();
app.MapOrganizationEndpoints();
app.MapMenuEndpoints();
app.MapPermissionEndpoints();
app.Run();
public partial class Program;
