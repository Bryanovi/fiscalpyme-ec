using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FiscalPymeEC.Infrastructure.Authentication;

public sealed class AspNetCorePasswordHasher
    : IPasswordHasher
{
    private static readonly object UserContext = new();

    private readonly PasswordHasher<object> _passwordHasher =
        new();

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "La contraseña es obligatoria.",
                nameof(password));
        }

        return _passwordHasher.HashPassword(
            UserContext,
            password);
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var result =
                _passwordHasher.VerifyHashedPassword(
                    UserContext,
                    passwordHash,
                    password);

            return result is
                PasswordVerificationResult.Success or
                PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
