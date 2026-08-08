using System.Text;
using System.Threading.RateLimiting;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Lms.Api;

/// <summary>
/// The API is a pure resource server: it validates bearer tokens and never issues them.
/// </summary>
/// <remarks>
/// That separation is what makes the migration path in
/// artifacts/design/04-adr-authentication.md §5 real — swapping to OpenIddict or a managed
/// provider means pointing <c>Authority</c> somewhere else, not rewriting authorisation.
/// </remarks>
internal static class AuthenticationSetup
{
    public static IServiceCollection AddLmsAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Leave 'sub' and 'role' as written. Without this the handler rewrites them to
                // the SOAP-era ClaimTypes URIs, and RoleClaimType below would never match.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

                    ValidateLifetime = true,

                    // The default is five minutes of grace, which would turn a deliberately
                    // short 15-minute token into a 20-minute one. The lifetime is the
                    // revocation window, so it must mean exactly what it says.
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = "name",
                    RoleClaimType = ClaimsPrincipalExtensions.RoleClaimType
                };
            });

        services.AddAuthorizationBuilder()
            // Every registered user holds Student, so this policy means "authenticated".
            .AddPolicy(AuthPolicies.Student, policy =>
                policy.RequireAuthenticatedUser().RequireRole(Roles.Student))
            .AddPolicy(AuthPolicies.Instructor, policy =>
                policy.RequireAuthenticatedUser().RequireRole(Roles.Instructor))
            .AddPolicy(AuthPolicies.Admin, policy =>
                policy.RequireAuthenticatedUser().RequireRole(Roles.Admin));

        return services;
    }

    /// <summary>
    /// Rate limits the auth endpoints: 10 requests per 5 minutes per client
    /// (artifacts/design/03-api-design.md §8).
    /// </summary>
    /// <remarks>
    /// Account lockout already caps attempts per <i>account</i>; this caps them per caller,
    /// which is what blunts credential stuffing across many accounts.
    /// </remarks>
    public static IServiceCollection AddLmsRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Auth, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
