using System.Security.Claims;
using System.Text;
using Aether.Application.DTOs;
using Aether.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Aether.Security.Jwt;

public sealed record JwtSettings(string Issuer, string Audience, string Key, int LifetimeMinutes = 15);
public sealed class TokenService(JwtSettings settings) : ITokenService
{
    public TokenResult Create(IdentityState state, AuthorizationSnapshot snapshot)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(settings.LifetimeMinutes);
        var claims = new List<Claim>
        {
            new("sub", state.UserId.ToString()), new("userId", state.UserId.ToString()),
            new("tenantId", state.TenantId.ToString()), new("deptId", state.DeptId.ToString()),
            new("stamp", state.SecurityStamp.ToString()), new("jti", Guid.NewGuid().ToString())
        };
        claims.AddRange(snapshot.Roles.Select(r => new Claim("roles", r)));
        claims.AddRange(snapshot.Permissions.Select(p => new Claim("permissions", p)));
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer, Audience = settings.Audience, Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow, NotBefore = DateTime.UtcNow, Expires = expires.UtcDateTime,
            SigningCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256)
        });
        return new(token, expires);
    }
}
