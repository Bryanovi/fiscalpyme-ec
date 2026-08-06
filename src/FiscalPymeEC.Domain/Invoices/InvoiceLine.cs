using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Invoices;

public sealed class InvoiceLine : Entity
{
    private InvoiceLine()
    {
        // Constructor requerido por Entity Framework Core.
    }

    internal InvoiceLine(
        Guid invoiceId,
        Guid productId,
        string productCode,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountRate,
        decimal ivaRate)
    {
        ValidateIdentifiers(invoiceId, productId);

        InvoiceId = invoiceId;
        ProductId = productId;
        ProductCode = ValidateRequiredText(
            productCode,
            "El código del producto");
        Description = ValidateRequiredText(
            description,
            "La descripción");
        Quantity = ValidateQuantity(quantity);
        UnitPrice = ValidateUnitPrice(unitPrice);
        DiscountRate = ValidateRate(
            discountRate,
            "La tasa de descuento");
        IvaRate = ValidateRate(
            ivaRate,
            "La tarifa de IVA");
    }

    public Guid InvoiceId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal DiscountRate { get; private set; }

    public decimal IvaRate { get; private set; }

    public decimal Subtotal =>
        RoundMoney(Quantity * UnitPrice);

    public decimal DiscountAmount =>
        RoundMoney(Subtotal * DiscountRate);

    public decimal TaxableBase =>
        RoundMoney(Subtotal - DiscountAmount);

    public decimal IvaAmount =>
        RoundMoney(TaxableBase * IvaRate);

    public decimal Total =>
        RoundMoney(TaxableBase + IvaAmount);

    private static void ValidateIdentifiers(
        Guid invoiceId,
        Guid productId)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new DomainException(
                "El identificador de la factura es obligatorio.");
        }

        if (productId == Guid.Empty)
        {
            throw new DomainException(
                "El identificador del producto es obligatorio.");
        }
    }

    private static decimal ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(
                "La cantidad debe ser mayor que cero.");
        }

        return quantity;
    }

    private static decimal ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new DomainException(
                "El precio unitario no puede ser negativo.");
        }

        return RoundMoney(unitPrice);
    }

    private static decimal ValidateRate(
        decimal rate,
        string fieldName)
    {
        if (rate is < 0 or > 1)
        {
            throw new DomainException(
                $"{fieldName} debe estar entre 0 y 1.");
        }

        return rate;
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

    private static decimal RoundMoney(decimal value)
    {
        return decimal.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}
