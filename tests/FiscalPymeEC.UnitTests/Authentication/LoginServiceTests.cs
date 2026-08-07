using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Authentication;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.UnitTests.Authentication;

public sealed class LoginServiceTests
{
    private static readonly DateTimeOffset ExpiresAtUtc =
        new(
            2026,
            8,
            6,
            20,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResult()
    {
        var user = CreateUser();

        var repository =
            new FakeUserRepository(user);

        var passwordHasher =
            new FakePasswordHasher(true);

        var tokenGenerator =
            new FakeTokenGenerator();

        var service = new LoginService(
            repository,
            passwordHasher,
            tokenGenerator);

        var command = new LoginCommand(
            "ADMIN@FISCALPYME.EC",
            "FiscalPyme123!");

        var result = await service.LoginAsync(command);

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal(ExpiresAtUtc, result.ExpiresAtUtc);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal("Administrator", result.Role);

        Assert.Equal(
            "admin@fiscalpyme.ec",
            repository.RequestedEmail);

        Assert.True(passwordHasher.VerifyWasCalled);
        Assert.True(tokenGenerator.GenerateWasCalled);
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsNull()
    {
        var repository =
            new FakeUserRepository(CreateUser());

        var passwordHasher =
            new FakePasswordHasher(false);

        var tokenGenerator =
            new FakeTokenGenerator();

        var service = new LoginService(
            repository,
            passwordHasher,
            tokenGenerator);

        var result = await service.LoginAsync(
            new LoginCommand(
                "admin@fiscalpyme.ec",
                "IncorrectPassword"));

        Assert.Null(result);
        Assert.True(passwordHasher.VerifyWasCalled);
        Assert.False(tokenGenerator.GenerateWasCalled);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var repository =
            new FakeUserRepository(null);

        var passwordHasher =
            new FakePasswordHasher(true);

        var tokenGenerator =
            new FakeTokenGenerator();

        var service = new LoginService(
            repository,
            passwordHasher,
            tokenGenerator);

        var result = await service.LoginAsync(
            new LoginCommand(
                "unknown@fiscalpyme.ec",
                "FiscalPyme123!"));

        Assert.Null(result);
        Assert.False(passwordHasher.VerifyWasCalled);
        Assert.False(tokenGenerator.GenerateWasCalled);
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ReturnsNull()
    {
        var user = CreateUser();
        user.Deactivate();

        var repository =
            new FakeUserRepository(user);

        var passwordHasher =
            new FakePasswordHasher(true);

        var tokenGenerator =
            new FakeTokenGenerator();

        var service = new LoginService(
            repository,
            passwordHasher,
            tokenGenerator);

        var result = await service.LoginAsync(
            new LoginCommand(
                user.Email,
                "FiscalPyme123!"));

        Assert.Null(result);
        Assert.False(passwordHasher.VerifyWasCalled);
        Assert.False(tokenGenerator.GenerateWasCalled);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ReturnsNull()
    {
        var repository =
            new FakeUserRepository(CreateUser());

        var service = new LoginService(
            repository,
            new FakePasswordHasher(true),
            new FakeTokenGenerator());

        var result = await service.LoginAsync(
            new LoginCommand(
                "",
                "FiscalPyme123!"));

        Assert.Null(result);
        Assert.False(repository.GetByEmailWasCalled);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ReturnsNull()
    {
        var repository =
            new FakeUserRepository(CreateUser());

        var service = new LoginService(
            repository,
            new FakePasswordHasher(true),
            new FakeTokenGenerator());

        var result = await service.LoginAsync(
            new LoginCommand(
                "admin@fiscalpyme.ec",
                ""));

        Assert.Null(result);
        Assert.False(repository.GetByEmailWasCalled);
    }

    private static User CreateUser()
    {
        return new User(
            "Administrador",
            "admin@fiscalpyme.ec",
            "stored-password-hash",
            UserRole.Administrator);
    }

    private sealed class FakeUserRepository(
    User? user) : IUserRepository
    {
        public bool GetByEmailWasCalled { get; private set; }

        public string? RequestedEmail { get; private set; }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            GetByEmailWasCalled = true;
            RequestedEmail = email;

            return Task.FromResult(user);
        }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail =
                email.Trim().ToLowerInvariant();

            return Task.FromResult(
                user?.Email == normalizedEmail);
        }

        public void Add(User newUser)
        {
            throw new NotSupportedException(
                "Este método no se utiliza en las pruebas de inicio de sesión.");
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Este método no se utiliza en las pruebas de inicio de sesión.");
        }
    }

    private sealed class FakePasswordHasher(
        bool verificationResult) : IPasswordHasher
    {
        public bool VerifyWasCalled { get; private set; }

        public string Hash(string password)
        {
            throw new NotSupportedException();
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            VerifyWasCalled = true;

            return verificationResult;
        }
    }

    private sealed class FakeTokenGenerator
        : ITokenGenerator
    {
        public bool GenerateWasCalled { get; private set; }

        public AuthenticationToken Generate(User user)
        {
            GenerateWasCalled = true;

            return new AuthenticationToken(
                "test-access-token",
                ExpiresAtUtc);
        }
    }
}