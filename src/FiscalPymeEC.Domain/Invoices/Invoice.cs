using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Invoices;

public sealed class Invoice : AuditableEntity
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Invoice(Guid companyId, Guid customerId)
    {
        if (companyId == Guid.Empty)
        {
            throw new DomainException(
                "El identificador de la empresa es obligatorio.");
        }

        if (customerId == Guid.Empty)
        {
            throw new DomainException(
                "El identificador del cliente es obligatorio.");
        }

        CompanyId = companyId;
        CustomerId = customerId;
    }

    public Guid CompanyId { get; private set; }

    public Guid CustomerId { get; private set; }

    public string? SequentialNumber { get; private set; }

    public string? AccessKey { get; private set; }

    public DateTimeOffset? IssuedAtUtc { get; private set; }

    public string? AuthorizationNumber { get; private set; }

    public DateTimeOffset? AuthorizedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public InvoiceStatus Status { get; private set; } =
        InvoiceStatus.Draft;

    public IReadOnlyCollection<InvoiceLine> Lines =>
        _lines.AsReadOnly();

    public decimal Subtotal =>
        RoundMoney(_lines.Sum(line => line.Subtotal));

    public decimal DiscountAmount =>
        RoundMoney(_lines.Sum(line => line.DiscountAmount));

    public decimal TaxableBase =>
        RoundMoney(_lines.Sum(line => line.TaxableBase));

    public decimal IvaAmount =>
        RoundMoney(_lines.Sum(line => line.IvaAmount));

    public decimal Total =>
        RoundMoney(_lines.Sum(line => line.Total));

    public InvoiceLine AddLine(
        Guid productId,
        string productCode,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountRate,
        decimal ivaRate)
    {
        EnsureDraft();

        var line = new InvoiceLine(
            Id,
            productId,
            productCode,
            description,
            quantity,
            unitPrice,
            discountRate,
            ivaRate);

        _lines.Add(line);

        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();

        var line = _lines.FirstOrDefault(
            existingLine => existingLine.Id == lineId);

        if (line is null)
        {
            throw new DomainException(
                "El detalle de factura no existe.");
        }

        _lines.Remove(line);
    }

    public void Issue(
        string sequentialNumber,
        string accessKey,
        DateTimeOffset issuedAtUtc)
    {
        EnsureDraft();

        if (_lines.Count == 0)
        {
            throw new DomainException(
                "La factura debe contener al menos un detalle.");
        }

        SequentialNumber = ValidateSequentialNumber(
            sequentialNumber);

        AccessKey = ValidateAccessKey(accessKey);
        IssuedAtUtc = issuedAtUtc;
        Status = InvoiceStatus.Issued;
    }

    public void MarkXmlAsGenerated()
    {
        EnsureStatus(
            InvoiceStatus.Issued,
            "Solo una factura emitida puede marcarse con XML generado.");

        Status = InvoiceStatus.XmlGenerated;
    }

    public void MarkAsSigned()
    {
        EnsureStatus(
            InvoiceStatus.XmlGenerated,
            "Solo una factura con XML generado puede marcarse como firmada.");

        Status = InvoiceStatus.Signed;
    }

    public void MarkAsReceived()
    {
        EnsureStatus(
            InvoiceStatus.Signed,
            "Solo una factura firmada puede marcarse como recibida.");

        Status = InvoiceStatus.Received;
    }

    public void Authorize(
        string authorizationNumber,
        DateTimeOffset authorizedAtUtc)
    {
        EnsureStatus(
            InvoiceStatus.Received,
            "Solo una factura recibida puede ser autorizada.");

        if (string.IsNullOrWhiteSpace(authorizationNumber))
        {
            throw new DomainException(
                "El número de autorización es obligatorio.");
        }

        AuthorizationNumber = authorizationNumber.Trim();
        AuthorizedAtUtc = authorizedAtUtc;
        RejectionReason = null;
        Status = InvoiceStatus.Authorized;
    }

    public void Reject(string rejectionReason)
    {
        EnsureStatus(
            InvoiceStatus.Received,
            "Solo una factura recibida puede ser rechazada.");

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new DomainException(
                "El motivo del rechazo es obligatorio.");
        }

        RejectionReason = rejectionReason.Trim();
        AuthorizationNumber = null;
        AuthorizedAtUtc = null;
        Status = InvoiceStatus.Rejected;
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainException(
                "Solo se pueden modificar facturas en borrador.");
        }
    }

    private void EnsureStatus(
    InvoiceStatus expectedStatus,
    string errorMessage)
    {
        if (Status != expectedStatus)
        {
            throw new DomainException(errorMessage);
        }
    }

    private static string ValidateSequentialNumber(
        string sequentialNumber)
    {
        if (string.IsNullOrWhiteSpace(sequentialNumber))
        {
            throw new DomainException(
                "El número secuencial es obligatorio.");
        }

        var normalizedSequential = sequentialNumber.Trim();
        var parts = normalizedSequential.Split('-');

        var isValid =
            parts.Length == 3 &&
            parts[0].Length == 3 &&
            parts[1].Length == 3 &&
            parts[2].Length == 9 &&
            parts.All(part => part.All(char.IsDigit));

        if (!isValid)
        {
            throw new DomainException(
                "El número secuencial debe tener el formato 001-001-000000001.");
        }

        return normalizedSequential;
    }

    private static string ValidateAccessKey(string accessKey)
    {
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new DomainException(
                "La clave de acceso es obligatoria.");
        }

        var normalizedAccessKey = accessKey.Trim();

        if (normalizedAccessKey.Length != 49 ||
            !normalizedAccessKey.All(char.IsDigit))
        {
            throw new DomainException(
                "La clave de acceso debe contener exactamente 49 dígitos.");
        }

        return normalizedAccessKey;
    }

    private static decimal RoundMoney(decimal value)
    {
        return decimal.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}