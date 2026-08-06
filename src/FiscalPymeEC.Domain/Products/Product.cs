using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Products;

public sealed class Product : AuditableEntity
{
    private Product()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Product(
        string code,
        string name,
        ProductType type,
        decimal unitPrice,
        decimal ivaRate,
        string? description = null)
    {
        Code = ValidateCode(code);
        Name = ValidateRequiredText(name, "El nombre");
        Type = ValidateProductType(type);
        UnitPrice = ValidateAndRoundPrice(unitPrice);
        IvaRate = ValidateIvaRate(ivaRate);
        Description = NormalizeOptionalText(description);
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ProductType Type { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal IvaRate { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void UpdateInformation(
        string name,
        ProductType type,
        string? description)
    {
        Name = ValidateRequiredText(name, "El nombre");
        Type = ValidateProductType(type);
        Description = NormalizeOptionalText(description);
    }

    public void ChangePrice(decimal unitPrice)
    {
        UnitPrice = ValidateAndRoundPrice(unitPrice);
    }

    public void ChangeIvaRate(decimal ivaRate)
    {
        IvaRate = ValidateIvaRate(ivaRate);
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException(
                "El producto o servicio ya está inactivo.");
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new DomainException(
                "El producto o servicio ya está activo.");
        }

        IsActive = true;
    }

    private static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("El código es obligatorio.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > 50)
        {
            throw new DomainException(
                "El código no puede superar los 50 caracteres.");
        }

        return normalizedCode;
    }

    private static ProductType ValidateProductType(ProductType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(
                "El tipo de producto o servicio no es válido.");
        }

        return type;
    }

    private static decimal ValidateAndRoundPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new DomainException(
                "El precio unitario no puede ser negativo.");
        }

        return decimal.Round(
            unitPrice,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal ValidateIvaRate(decimal ivaRate)
    {
        if (ivaRate is < 0 or > 1)
        {
            throw new DomainException(
                "La tarifa de IVA debe estar entre 0 y 1.");
        }

        return ivaRate;
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

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
