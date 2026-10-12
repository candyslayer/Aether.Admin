using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using StackExchange.Redis;
namespace Aether.Api;
public sealed class DatabaseHealthCheck(NpgsqlDataSource source) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try { await using var c = await source.OpenConnectionAsync(cancellationToken); await using var command = new NpgsqlCommand("SELECT 1", c); await command.ExecuteScalarAsync(cancellationToken); return HealthCheckResult.Healthy(); }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("Database unavailable.", ex); }
    }
}
public sealed class RedisHealthCheck(IAetherRedisState state) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(state.Connected ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded("Redis disabled or unavailable; authorization uses database fallback."));
}
public interface IAetherRedisState { bool Connected { get; } }
public sealed class AetherRedisState(IConnectionMultiplexer? connection) : IAetherRedisState { public bool Connected => connection?.IsConnected == true; }
