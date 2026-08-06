using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FiscalPymeEC.Application.Authentication;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FiscalPymeEC.Infrastructure.Authentication;

public sealed class JwtTokenGenerator
    : ITokenGenerator
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenGenerator(
        IOptions<JwtOptions> options,
        IClock clock)
    {
        _options = options.Value;
        _clock = clock;

        ValidateOptions(_options);
    }

    public AuthenticationToken Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var issuedAtUtc = _clock.UtcNow;

        var expiresAtUtc = issuedAtUtc.AddMinutes(
            _options.ExpirationMinutes);

        Claim[] claims =
        [
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        ];

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _options.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new AuthenticationToken(
            accessToken,
            expiresAtUtc);
    }

    private static void ValidateOptions(
        JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException(
                "El emisor JWT no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException(
                "La audiencia JWT no está configurada.");
        }

        if (Encoding.UTF8.GetByteCount(options.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "La clave JWT debe contener al menos 32 bytes.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "La duración del token JWT debe ser mayor que cero.");
        }
    }
}