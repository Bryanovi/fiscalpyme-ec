using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Users;

public sealed class User : AuditableEntity
{
    private User()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public User(
        string fullName,
        string email,
        string passwordHash,
        UserRole role)
    {
        FullName = ValidateRequiredText(
            fullName,
            "El nombre completo");

        Email = ValidateEmail(email);

        PasswordHash = ValidateRequiredText(
            passwordHash,
            "El hash de la contraseña");

        Role = ValidateRole(role);
    }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void UpdateProfile(
        string fullName,
        string email)
    {
        FullName = ValidateRequiredText(
            fullName,
            "El nombre completo");

        Email = ValidateEmail(email);
    }

    public void ChangeRole(UserRole role)
    {
        Role = ValidateRole(role);
    }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = ValidateRequiredText(
            passwordHash,
            "El hash de la contraseña");
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException(
                "El usuario ya está inactivo.");
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new DomainException(
                "El usuario ya está activo.");
        }

        IsActive = true;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException(
                "El correo electrónico es obligatorio.");
        }

        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        if (!MailAddress.TryCreate(normalizedEmail, out _))
        {
            throw new DomainException(
                "El correo electrónico no tiene un formato válido.");
        }

        return normalizedEmail;
    }

    private static UserRole ValidateRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new DomainException(
                "El rol de usuario no es válido.");
        }

        return role;
    }

    private static string ValidateRequiredText(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                $"{fieldName} es obligatorio.");
        }

        return value.Trim();
    }
}
