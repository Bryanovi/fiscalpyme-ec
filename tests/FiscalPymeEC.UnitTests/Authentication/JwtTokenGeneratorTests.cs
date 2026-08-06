using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;
using FiscalPymeEC.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FiscalPymeEC.UnitTests.Authentication;

public sealed class JwtTokenGeneratorTests
{
    private const string Issuer = "FiscalPymeEC";
    private const string Audience = "FiscalPymeEC.Client";

    private static readonly string SecretKey =
        new('S', 64);

    private static readonly DateTimeOffset CurrentTime =
        new(
            2026,
            8,
            6,
            15,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public void Generate_WithValidUser_IncludesExpectedClaims()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var result = generator.Generate(user);

        var jwtToken =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(result.AccessToken);

        Assert.Equal(Issuer, jwtToken.Issuer);
        Assert.Contains(Audience, jwtToken.Audiences);

        Assert.Equal(
            user.Id.ToString(),
            GetClaim(jwtToken, JwtRegisteredClaimNames.Sub));

        Assert.Equal(
            user.Email,
            GetClaim(jwtToken, JwtRegisteredClaimNames.Email));

        Assert.Equal(
            user.FullName,
            GetClaim(jwtToken, ClaimTypes.Name));

        Assert.Equal(
            UserRole.Administrator.ToString(),
            GetClaim(jwtToken, ClaimTypes.Role));

        Assert.False(string.IsNullOrWhiteSpace(
            GetClaim(
                jwtToken,
                JwtRegisteredClaimNames.Jti)));
    }

    [Fact]
    public void Generate_UsesConfiguredExpiration()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var result = generator.Generate(user);

        Assert.Equal(
            CurrentTime.AddMinutes(60),
            result.ExpiresAtUtc);
    }

    [Fact]
    public void Generate_CreatesTokenWithValidSignature()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var result = generator.Generate(user);

        var validationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,

                ValidateAudience = true,
                ValidAudience = Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            SecretKey)),

                ValidateLifetime = false
            };

        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(
            result.AccessToken,
            validationParameters,
            out var validatedToken);

        Assert.NotNull(principal);
        Assert.IsType<JwtSecurityToken>(
            validatedToken);
    }

    [Fact]
    public void Constructor_WithShortSecretKey_ThrowsException()
    {
        var options = Options.Create(
            new JwtOptions
            {
                Issuer = Issuer,
                Audience = Audience,
                SecretKey = "short-key",
                ExpirationMinutes = 60
            });

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => new JwtTokenGenerator(
                    options,
                    new FixedClock(CurrentTime)));

        Assert.Equal(
            "La clave JWT debe contener al menos 32 bytes.",
            exception.Message);
    }

    private static JwtTokenGenerator CreateGenerator()
    {
        var options = Options.Create(
            new JwtOptions
            {
                Issuer = Issuer,
                Audience = Audience,
                SecretKey = SecretKey,
                ExpirationMinutes = 60
            });

        return new JwtTokenGenerator(
            options,
            new FixedClock(CurrentTime));
    }

    private static User CreateUser()
    {
        return new User(
            "Administrador",
            "admin@fiscalpyme.ec",
            "password-hash",
            UserRole.Administrator);
    }

    private static string GetClaim(
        JwtSecurityToken token,
        string claimType)
    {
        return token.Claims
            .Single(claim => claim.Type == claimType)
            .Value;
    }

    private sealed class FixedClock(
        DateTimeOffset currentTime) : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            currentTime;
    }
}