using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Products;

namespace FiscalPymeEC.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_WithValidInformation_CreatesProduct()
    {
        var product = new Product(
            "prod-001",
            "Teclado mecánico",
            ProductType.Product,
            50.25m,
            0.15m,
            "Teclado USB");

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("PROD-001", product.Code);
        Assert.Equal("Teclado mecánico", product.Name);
        Assert.Equal(ProductType.Product, product.Type);
        Assert.Equal(50.25m, product.UnitPrice);
        Assert.Equal(0.15m, product.IvaRate);
        Assert.Equal("Teclado USB", product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Constructor_WithServiceType_CreatesService()
    {
        var service = new Product(
            "serv-001",
            "Consultoría",
            ProductType.Service,
            100m,
            0.15m);

        Assert.Equal("SERV-001", service.Code);
        Assert.Equal(ProductType.Service, service.Type);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Product(
                "PROD-001",
                "Producto de prueba",
                ProductType.Product,
                -10m,
                0.15m));

        Assert.Equal(
            "El precio unitario no puede ser negativo.",
            exception.Message);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_WithInvalidIvaRate_ThrowsDomainException(
        decimal ivaRate)
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Product(
                "PROD-001",
                "Producto de prueba",
                ProductType.Product,
                10m,
                ivaRate));

        Assert.Equal(
            "La tarifa de IVA debe estar entre 0 y 1.",
            exception.Message);
    }

    [Fact]
    public void Constructor_RoundsPriceToTwoDecimals()
    {
        var product = CreateValidProduct(10.126m);

        Assert.Equal(10.13m, product.UnitPrice);
    }

    [Fact]
    public void ChangePrice_WithValidPrice_UpdatesPrice()
    {
        var product = CreateValidProduct();

        product.ChangePrice(25.456m);

        Assert.Equal(25.46m, product.UnitPrice);
    }

    [Fact]
    public void ChangeIvaRate_WithValidRate_UpdatesRate()
    {
        var product = CreateValidProduct();

        product.ChangeIvaRate(0.05m);

        Assert.Equal(0.05m, product.IvaRate);
    }

    [Fact]
    public void Deactivate_WhenProductIsActive_DeactivatesProduct()
    {
        var product = CreateValidProduct();

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void Deactivate_WhenProductIsAlreadyInactive_ThrowsDomainException()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        var exception = Assert.Throws<DomainException>(
            product.Deactivate);

        Assert.Equal(
            "El producto o servicio ya está inactivo.",
            exception.Message);
    }

    [Fact]
    public void Activate_WhenProductIsInactive_ActivatesProduct()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        product.Activate();

        Assert.True(product.IsActive);
    }

    private static Product CreateValidProduct(decimal price = 10m)
    {
        return new Product(
            "PROD-001",
            "Producto de prueba",
            ProductType.Product,
            price,
            0.15m);
    }
}
