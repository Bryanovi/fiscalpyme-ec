using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Invoices;

namespace FiscalPymeEC.UnitTests.Invoices;

public sealed class InvoiceTests
{
    private const string ValidSequential = "001-001-000000001";

    private static readonly string ValidAccessKey =
        new('1', 49);

    [Fact]
    public void Constructor_WithValidIdentifiers_CreatesDraftInvoice()
    {
        var invoice = CreateInvoice();

        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Empty(invoice.Lines);
        Assert.Equal(0m, invoice.Total);
    }

    [Fact]
    public void Constructor_WithEmptyCompanyId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Invoice(Guid.Empty, Guid.NewGuid()));

        Assert.Equal(
            "El identificador de la empresa es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Invoice(Guid.NewGuid(), Guid.Empty));

        Assert.Equal(
            "El identificador del cliente es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void AddLine_CalculatesInvoiceTotals()
    {
        var invoice = CreateInvoice();

        invoice.AddLine(
            Guid.NewGuid(),
            "PROD-001",
            "Producto de prueba",
            2m,
            100m,
            0.10m,
            0.15m);

        Assert.Single(invoice.Lines);
        Assert.Equal(200m, invoice.Subtotal);
        Assert.Equal(20m, invoice.DiscountAmount);
        Assert.Equal(180m, invoice.TaxableBase);
        Assert.Equal(27m, invoice.IvaAmount);
        Assert.Equal(207m, invoice.Total);
    }

    [Fact]
    public void AddMultipleLines_CalculatesCombinedTotals()
    {
        var invoice = CreateInvoice();

        invoice.AddLine(
            Guid.NewGuid(),
            "PROD-001",
            "Producto",
            2m,
            100m,
            0.10m,
            0.15m);

        invoice.AddLine(
            Guid.NewGuid(),
            "SERV-001",
            "Servicio sin IVA",
            1m,
            50m,
            0m,
            0m);

        Assert.Equal(250m, invoice.Subtotal);
        Assert.Equal(20m, invoice.DiscountAmount);
        Assert.Equal(230m, invoice.TaxableBase);
        Assert.Equal(27m, invoice.IvaAmount);
        Assert.Equal(257m, invoice.Total);
    }

    [Fact]
    public void RemoveLine_WithExistingLine_RemovesLine()
    {
        var invoice = CreateInvoice();
        var line = AddValidLine(invoice);

        invoice.RemoveLine(line.Id);

        Assert.Empty(invoice.Lines);
        Assert.Equal(0m, invoice.Total);
    }

    [Fact]
    public void RemoveLine_WithUnknownLine_ThrowsDomainException()
    {
        var invoice = CreateInvoice();

        var exception = Assert.Throws<DomainException>(() =>
            invoice.RemoveLine(Guid.NewGuid()));

        Assert.Equal(
            "El detalle de factura no existe.",
            exception.Message);
    }

    [Fact]
    public void Issue_WithoutLines_ThrowsDomainException()
    {
        var invoice = CreateInvoice();

        var exception = Assert.Throws<DomainException>(() =>
            invoice.Issue(
                ValidSequential,
                ValidAccessKey,
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "La factura debe contener al menos un detalle.",
            exception.Message);
    }

    [Fact]
    public void Issue_WithValidInformation_ChangesStatusToIssued()
    {
        var invoice = CreateInvoice();
        AddValidLine(invoice);

        var issuedAt = new DateTimeOffset(
            2026,
            8,
            6,
            15,
            30,
            0,
            TimeSpan.Zero);

        invoice.Issue(
            ValidSequential,
            ValidAccessKey,
            issuedAt);

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(ValidSequential, invoice.SequentialNumber);
        Assert.Equal(ValidAccessKey, invoice.AccessKey);
        Assert.Equal(issuedAt, invoice.IssuedAtUtc);
    }

    [Fact]
    public void Issue_WithInvalidSequential_ThrowsDomainException()
    {
        var invoice = CreateInvoice();
        AddValidLine(invoice);

        var exception = Assert.Throws<DomainException>(() =>
            invoice.Issue(
                "000000001",
                ValidAccessKey,
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "El número secuencial debe tener el formato 001-001-000000001.",
            exception.Message);
    }

    [Fact]
    public void Issue_WithInvalidAccessKey_ThrowsDomainException()
    {
        var invoice = CreateInvoice();
        AddValidLine(invoice);

        var exception = Assert.Throws<DomainException>(() =>
            invoice.Issue(
                ValidSequential,
                "12345",
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "La clave de acceso debe contener exactamente 49 dígitos.",
            exception.Message);
    }

    [Fact]
    public void AddLine_AfterInvoiceWasIssued_ThrowsDomainException()
    {
        var invoice = CreateIssuedInvoice();

        var exception = Assert.Throws<DomainException>(() =>
            AddValidLine(invoice));

        Assert.Equal(
            "Solo se pueden modificar facturas en borrador.",
            exception.Message);
    }

    [Fact]
    public void RemoveLine_AfterInvoiceWasIssued_ThrowsDomainException()
    {
        var invoice = CreateInvoice();
        var line = AddValidLine(invoice);

        invoice.Issue(
            ValidSequential,
            ValidAccessKey,
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<DomainException>(() =>
            invoice.RemoveLine(line.Id));

        Assert.Equal(
            "Solo se pueden modificar facturas en borrador.",
            exception.Message);
    }

    private static Invoice CreateInvoice()
    {
        return new Invoice(
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    private static InvoiceLine AddValidLine(Invoice invoice)
    {
        return invoice.AddLine(
            Guid.NewGuid(),
            "PROD-001",
            "Producto de prueba",
            2m,
            100m,
            0.10m,
            0.15m);
    }

    private static Invoice CreateIssuedInvoice()
    {
        var invoice = CreateInvoice();
        AddValidLine(invoice);

        invoice.Issue(
            ValidSequential,
            ValidAccessKey,
            DateTimeOffset.UtcNow);

        return invoice;
    }
}
