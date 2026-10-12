using System.Text.Json;
using System.Text.Json.Serialization;
using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using StackExchange.Redis;

namespace Aether.Infrastructure.Redis;

public sealed class AuthorizationCache(IConnectionMultiplexer? redis) : IAuthorizationCache
{
    private static string Key(IdentityState s) => $"aether:permission:{s.TenantId}:{s.PermissionVersion}:{s.UserId}:{s.SecurityStamp}";
    public async Task<AuthorizationSnapshot?> GetAsync(IdentityState identity, CancellationToken ct)
    {
        if (redis is null) return null;
        try
        {
            var json = await redis.GetDatabase().StringGetAsync(Key(identity)).WaitAsync(ct);
            return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize(json.ToString(), CacheJsonContext.Default.AuthorizationSnapshot);
        }
        catch (Exception ex) when (ex is RedisException or JsonException) { return null; }
    }
    public async Task SetAsync(IdentityState identity, AuthorizationSnapshot snapshot, CancellationToken ct)
    {
        if (redis is null) return;
        try { await redis.GetDatabase().StringSetAsync(Key(identity), JsonSerializer.Serialize(snapshot, CacheJsonContext.Default.AuthorizationSnapshot), TimeSpan.FromMinutes(5)).WaitAsync(ct); }
        catch (RedisException) { /* Database remains the authoritative fallback. */ }
    }
}
[JsonSerializable(typeof(AuthorizationSnapshot))]
internal partial class CacheJsonContext : JsonSerializerContext;
