using System.Text;
using Aether.Application.Interfaces;
using Aether.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Aether.Security.Jwt;

namespace Aether.Security.Authorization;

public static class AuthenticationRegistration
{
    public static IServiceCollection AddAetherAuthentication(this IServiceCollection services, JwtSettings settings)
    {
        if (Encoding.UTF8.GetByteCount(settings.Key) < 32 || settings.LifetimeMinutes is < 1 or > 60) throw new InvalidOperationException("JWT key must be at least 32 bytes; lifetime must be 1-60 minutes.");
        services.AddSingleton(settings);
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidIssuer = settings.Issuer,
                ValidateAudience = true, ValidAudience = settings.Audience,
                ValidateLifetime = true, RequireExpirationTime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "sub", RoleClaimType = "roles"
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal!;
                    if (!Guid.TryParse(principal.FindFirst("sub")?.Value, out var user) ||
                        !Guid.TryParse(principal.FindFirst("tenantId")?.Value, out var tenant) ||
                        !Guid.TryParse(principal.FindFirst("stamp")?.Value, out var stamp))
                    { context.Fail("Invalid identity."); return; }
                    var services = context.HttpContext.RequestServices;
                    var identity = await services.GetRequiredService<IIdentityRepository>().FindActiveAsync(tenant, user, context.HttpContext.RequestAborted);
                    if (identity is null || identity.SecurityStamp != stamp) { context.Fail("Identity revoked."); return; }
                    var snapshot = await services.GetRequiredService<PermissionService>().ResolveAsync(identity, context.HttpContext.RequestAborted);
                    services.GetRequiredService<ICurrentUser>().Set(snapshot);
                }
            };
        });
        services.AddAuthorization();
        return services;
    }
}
