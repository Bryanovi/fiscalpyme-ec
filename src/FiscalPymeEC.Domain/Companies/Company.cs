using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Companies;

public sealed class Company : AuditableEntity
{
    private Company()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Company(
        string ruc,
        string legalName,
        string tradeName,
        string address)
    {
        ValidateRuc(ruc);

        Ruc = ruc.Trim();
        LegalName = ValidateRequiredText(legalName, "La razón social");
        TradeName = ValidateRequiredText(tradeName, "El nombre comercial");
        Address = ValidateRequiredText(address, "La dirección");
    }

    public string Ruc { get; private set; } = string.Empty;

    public string LegalName { get; private set; } = string.Empty;

    public string TradeName { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public void UpdateBusinessInformation(
        string legalName,
        string tradeName,
        string address)
    {
        LegalName = ValidateRequiredText(legalName, "La razón social");
        TradeName = ValidateRequiredText(tradeName, "El nombre comercial");
        Address = ValidateRequiredText(address, "La dirección");
    }

    private static void ValidateRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
        {
            throw new DomainException("El RUC es obligatorio.");
        }

        var normalizedRuc = ruc.Trim();

        if (normalizedRuc.Length != 13 || !normalizedRuc.All(char.IsDigit))
        {
            throw new DomainException(
                "El RUC debe contener exactamente 13 dígitos.");
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
}
