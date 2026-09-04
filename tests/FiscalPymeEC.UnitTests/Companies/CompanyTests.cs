using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Companies;

namespace FiscalPymeEC.UnitTests.Companies;

public sealed class CompanyTests
{
    [Fact]
    public void Constructor_WithValidInformation_CreatesCompany()
    {
        var company = new Company(
            "1790012345001",
            "FiscalPyme Ecuador S.A.",
            "FiscalPyme",
            "Quito, Ecuador",
            "001",
            "001",
            SriEnvironment.Testing);

        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal("1790012345001", company.Ruc);
        Assert.Equal("FiscalPyme Ecuador S.A.", company.LegalName);
        Assert.Equal("FiscalPyme", company.TradeName);
        Assert.Equal("Quito, Ecuador", company.Address);
    }

    [Fact]
    public void Constructor_WithInvalidRucLength_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Company(
                "123456789",
                "FiscalPyme Ecuador S.A.",
                "FiscalPyme",
                "Quito, Ecuador",
                "001",
                "001",
                SriEnvironment.Testing));

        Assert.Equal(
            "El RUC debe contener exactamente 13 dígitos.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithLettersInRuc_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Company(
                "179001234ABC1",
                "FiscalPyme Ecuador S.A.",
                "FiscalPyme",
                "Quito, Ecuador",
                "001",
                "001",
                SriEnvironment.Testing));
    }

    [Fact]
    public void Constructor_WithEmptyLegalName_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Company(
                "1790012345001",
                "",
                "FiscalPyme",
                "Quito, Ecuador",
                "001",
                "001",
                SriEnvironment.Testing));

        Assert.Equal(
            "La razón social es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidEstablishmentCode_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Company(
                "1790012345001",
                "FiscalPyme Ecuador S.A.",
                "FiscalPyme",
                "Quito, Ecuador",
                "01",
                "001",
                SriEnvironment.Testing));

        Assert.Equal(
            "El código de establecimiento debe contener exactamente 3 dígitos.",
            exception.Message);
    }

    [Fact]
    public void UpdateBusinessInformation_WithValidValues_UpdatesCompany()
    {
        var company = new Company(
            "1790012345001",
            "Nombre anterior",
            "Comercial anterior",
            "Dirección anterior",
                "001",
                "001",
                SriEnvironment.Testing);

        company.UpdateBusinessInformation(
            "Nueva razón social",
            "Nuevo nombre comercial",
            "Nueva dirección");

        Assert.Equal("Nueva razón social", company.LegalName);
        Assert.Equal("Nuevo nombre comercial", company.TradeName);
        Assert.Equal("Nueva dirección", company.Address);

        Assert.Equal("001", company.EstablishmentCode);
        Assert.Equal("001", company.EmissionPointCode);

        Assert.Equal(
            SriEnvironment.Testing,
            company.SriEnvironment);
    }
}
