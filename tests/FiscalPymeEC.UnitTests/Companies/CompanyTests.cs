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
            "Quito, Ecuador");

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
                "Quito, Ecuador"));

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
                "Quito, Ecuador"));
    }

    [Fact]
    public void Constructor_WithEmptyLegalName_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Company(
                "1790012345001",
                "",
                "FiscalPyme",
                "Quito, Ecuador"));

        Assert.Equal(
            "La razón social es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void UpdateBusinessInformation_WithValidValues_UpdatesCompany()
    {
        var company = new Company(
            "1790012345001",
            "Nombre anterior",
            "Comercial anterior",
            "Dirección anterior");

        company.UpdateBusinessInformation(
            "Nueva razón social",
            "Nuevo nombre comercial",
            "Nueva dirección");

        Assert.Equal("Nueva razón social", company.LegalName);
        Assert.Equal("Nuevo nombre comercial", company.TradeName);
        Assert.Equal("Nueva dirección", company.Address);
    }
}
