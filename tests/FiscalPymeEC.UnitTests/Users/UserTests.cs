using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.UnitTests.Users;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidInformation_CreatesUser()
    {
        var user = new User(
            "Administrador FiscalPyme",
            "ADMIN@FISCALPYME.EC",
            "password-hash",
            UserRole.Administrator);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(
            "Administrador FiscalPyme",
            user.FullName);
        Assert.Equal(
            "admin@fiscalpyme.ec",
            user.Email);
        Assert.Equal(
            "password-hash",
            user.PasswordHash);
        Assert.Equal(
            UserRole.Administrator,
            user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new User(
                "Administrador",
                "correo-invalido",
                "password-hash",
                UserRole.Administrator));

        Assert.Equal(
            "El correo electrónico no tiene un formato válido.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyPasswordHash_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new User(
                "Administrador",
                "admin@fiscalpyme.ec",
                "",
                UserRole.Administrator));

        Assert.Equal(
            "El hash de la contraseña es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidRole_ThrowsDomainException()
    {
        var invalidRole = (UserRole)999;

        var exception = Assert.Throws<DomainException>(() =>
            new User(
                "Administrador",
                "admin@fiscalpyme.ec",
                "password-hash",
                invalidRole));

        Assert.Equal(
            "El rol de usuario no es válido.",
            exception.Message);
    }

    [Fact]
    public void UpdateProfile_WithValidInformation_UpdatesUser()
    {
        var user = CreateValidUser();

        user.UpdateProfile(
            "Nuevo nombre",
            "NUEVO@FISCALPYME.EC");

        Assert.Equal("Nuevo nombre", user.FullName);
        Assert.Equal(
            "nuevo@fiscalpyme.ec",
            user.Email);
    }

    [Fact]
    public void ChangePasswordHash_WithValidHash_UpdatesHash()
    {
        var user = CreateValidUser();

        user.ChangePasswordHash("new-password-hash");

        Assert.Equal(
            "new-password-hash",
            user.PasswordHash);
    }

    [Fact]
    public void ChangeRole_WithValidRole_UpdatesRole()
    {
        var user = CreateValidUser();

        user.ChangeRole(UserRole.Seller);

        Assert.Equal(UserRole.Seller, user.Role);
    }

    [Fact]
    public void Deactivate_WhenUserIsActive_DeactivatesUser()
    {
        var user = CreateValidUser();

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_WhenUserIsAlreadyInactive_ThrowsDomainException()
    {
        var user = CreateValidUser();
        user.Deactivate();

        var exception = Assert.Throws<DomainException>(
            user.Deactivate);

        Assert.Equal(
            "El usuario ya está inactivo.",
            exception.Message);
    }

    [Fact]
    public void Activate_WhenUserIsInactive_ActivatesUser()
    {
        var user = CreateValidUser();
        user.Deactivate();

        user.Activate();

        Assert.True(user.IsActive);
    }

    private static User CreateValidUser()
    {
        return new User(
            "Administrador",
            "admin@fiscalpyme.ec",
            "password-hash",
            UserRole.Administrator);
    }
}
