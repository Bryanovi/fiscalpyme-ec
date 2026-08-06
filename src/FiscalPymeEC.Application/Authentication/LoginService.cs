using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.Application.Authentication;

public sealed class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult?> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
        {
            return null;
        }

        var normalizedEmail = command.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = _passwordHasher.Verify(
            command.Password,
            user.PasswordHash);

        if (!passwordIsValid)
        {
            return null;
        }

        var authenticationToken =
            _tokenGenerator.Generate(user);

        return new LoginResult(
            authenticationToken.AccessToken,
            authenticationToken.ExpiresAtUtc,
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
