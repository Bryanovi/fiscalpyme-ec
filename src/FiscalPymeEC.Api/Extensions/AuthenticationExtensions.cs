using System.Security.Claims;
using System.Text;
using FiscalPymeEC.Domain.Users;
using FiscalPymeEC.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FiscalPymeEC.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sectionName = JwtOptions.SectionName;

        var issuer =
            configuration[$"{sectionName}:Issuer"]
            ?? throw new InvalidOperationException(
                "El emisor JWT no está configurado.");

        var audience =
            configuration[$"{sectionName}:Audience"]
            ?? throw new InvalidOperationException(
                "La audiencia JWT no está configurada.");

        var secretKey =
            configuration[$"{sectionName}:SecretKey"]
            ?? throw new InvalidOperationException(
                "La clave JWT no está configurada.");

        var secretBytes =
            Encoding.UTF8.GetBytes(secretKey);

        if (secretBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "La clave JWT debe contener al menos 32 bytes.");
        }

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                secretBytes),

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,

                        ClockSkew = TimeSpan.FromSeconds(30),

                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                "AdministratorOnly",
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        UserRole.Administrator.ToString());
                })
            .AddPolicy(
                "SellerOrAdministrator",
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        UserRole.Seller.ToString(),
                        UserRole.Administrator.ToString());
                });

        return services;
    }
}