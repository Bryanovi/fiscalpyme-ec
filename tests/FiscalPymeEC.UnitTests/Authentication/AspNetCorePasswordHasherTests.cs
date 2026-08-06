using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Infrastructure.Authentication;

namespace FiscalPymeEC.UnitTests.Authentication;

public sealed class AspNetCorePasswordHasherTests
{
    private readonly AspNetCorePasswordHasher _passwordHasher =
        new();

    [Fact]
    public void Hash_WithValidPassword_ReturnsHash()
    {
        const string password = "FiscalPyme123!";

        var passwordHash = _passwordHasher.Hash(password);

        Assert.False(string.IsNullOrWhiteSpace(passwordHash));
        Assert.NotEqual(password, passwordHash);
    }

    [Fact]
    public void Hash_SamePasswordTwice_ReturnsDifferentHashes()
    {
        const string password = "FiscalPyme123!";

        var firstHash = _passwordHasher.Hash(password);
        var secondHash = _passwordHasher.Hash(password);

        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        const string password = "FiscalPyme123!";
        var passwordHash = _passwordHasher.Hash(password);

        var result = _passwordHasher.Verify(
            password,
            passwordHash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var passwordHash = _passwordHasher.Hash(
            "FiscalPyme123!");

        var result = _passwordHasher.Verify(
            "ContraseñaIncorrecta123!",
            passwordHash);

        Assert.False(result);
    }

    [Fact]
    public void Verify_WithMalformedHash_ReturnsFalse()
    {
        var result = _passwordHasher.Verify(
            "FiscalPyme123!",
            "not-a-valid-password-hash");

        Assert.False(result);
    }

    [Fact]
    public void Verify_WithEmptyValues_ReturnsFalse()
    {
        Assert.False(_passwordHasher.Verify("", ""));
        Assert.False(_passwordHasher.Verify("password", ""));
        Assert.False(_passwordHasher.Verify("", "hash"));
    }

    [Fact]
    public void Hash_WithEmptyPassword_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => _passwordHasher.Hash(""));

        Assert.Equal(
            "password",
            exception.ParamName);
    }
}