using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Customers;

public sealed class Customer : AuditableEntity
{
    public const string FinalConsumerIdentification = "9999999999999";

    private Customer()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Customer(
        IdentificationType identificationType,
        string identification,
        string name,
        string? email = null,
        string? address = null,
        string? phone = null)
    {
        ValidateIdentification(identificationType, identification);

        IdentificationType = identificationType;
        Identification = identification.Trim();
        Name = ValidateRequiredText(name, "El nombre");
        Email = ValidateEmail(email);
        Address = NormalizeOptionalText(address);
        Phone = NormalizeOptionalText(phone);
    }

    public IdentificationType IdentificationType { get; private set; }

    public string Identification { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public string? Phone { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void UpdateContactInformation(
        string name,
        string? email,
        string? address,
        string? phone)
    {
        Name = ValidateRequiredText(name, "El nombre");
        Email = ValidateEmail(email);
        Address = NormalizeOptionalText(address);
        Phone = NormalizeOptionalText(phone);
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("El cliente ya está inactivo.");
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new DomainException("El cliente ya está activo.");
        }

        IsActive = true;
    }

    private static void ValidateIdentification(
        IdentificationType identificationType,
        string identification)
    {
        if (string.IsNullOrWhiteSpace(identification))
        {
            throw new DomainException("La identificación es obligatoria.");
        }

        var normalizedIdentification = identification.Trim();

        switch (identificationType)
        {
            case IdentificationType.Cedula:
                ValidateNumericLength(
                    normalizedIdentification,
                    10,
                    "La cédula debe contener exactamente 10 dígitos.");
                break;

            case IdentificationType.Ruc:
                ValidateNumericLength(
                    normalizedIdentification,
                    13,
                    "El RUC debe contener exactamente 13 dígitos.");
                break;

            case IdentificationType.Passport:
                if (normalizedIdentification.Length is < 3 or > 20)
                {
                    throw new DomainException(
                        "El pasaporte debe contener entre 3 y 20 caracteres.");
                }

                break;

            case IdentificationType.FinalConsumer:
                if (normalizedIdentification != FinalConsumerIdentification)
                {
                    throw new DomainException(
                        $"El consumidor final debe utilizar la identificación {FinalConsumerIdentification}.");
                }

                break;

            default:
                throw new DomainException(
                    "El tipo de identificación no es válido.");
        }
    }

    private static void ValidateNumericLength(
        string value,
        int requiredLength,
        string errorMessage)
    {
        if (value.Length != requiredLength || !value.All(char.IsDigit))
        {
            throw new DomainException(errorMessage);
        }
    }

    private static string ValidateRequiredText(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} es obligatorio.");
        }

        return value.Trim();
    }

    private static string? ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim();

        if (!MailAddress.TryCreate(normalizedEmail, out _))
        {
            throw new DomainException(
                "El correo electrónico no tiene un formato válido.");
        }

        return normalizedEmail;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}