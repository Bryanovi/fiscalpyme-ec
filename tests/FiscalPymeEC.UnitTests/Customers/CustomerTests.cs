using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Customers;

namespace FiscalPymeEC.UnitTests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Constructor_WithValidCedula_CreatesCustomer()
    {
        var customer = new Customer(
            IdentificationType.Cedula,
            "1723456789",
            "Juan Pérez",
            "juan@example.com",
            "Quito",
            "0999999999");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(IdentificationType.Cedula, customer.IdentificationType);
        Assert.Equal("1723456789", customer.Identification);
        Assert.Equal("Juan Pérez", customer.Name);
        Assert.Equal("juan@example.com", customer.Email);
        Assert.Equal("Quito", customer.Address);
        Assert.Equal("0999999999", customer.Phone);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Constructor_WithInvalidCedula_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Customer(
                IdentificationType.Cedula,
                "12345",
                "Juan Pérez"));

        Assert.Equal(
            "La cédula debe contener exactamente 10 dígitos.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithLettersInRuc_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Customer(
                IdentificationType.Ruc,
                "179001234ABC1",
                "Empresa de prueba"));
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Customer(
                IdentificationType.Cedula,
                "1723456789",
                "Juan Pérez",
                "correo-invalido"));

        Assert.Equal(
            "El correo electrónico no tiene un formato válido.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithValidFinalConsumer_CreatesCustomer()
    {
        var customer = new Customer(
            IdentificationType.FinalConsumer,
            Customer.FinalConsumerIdentification,
            "Consumidor final");

        Assert.Equal("9999999999999", customer.Identification);
        Assert.Equal(IdentificationType.FinalConsumer, customer.IdentificationType);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Constructor_WithInvalidFinalConsumerIdentification_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Customer(
                IdentificationType.FinalConsumer,
                "0000000000000",
                "Consumidor final"));

        Assert.Equal(
            "El consumidor final debe utilizar la identificación 9999999999999.",
            exception.Message);
    }

    [Fact]
    public void Deactivate_WhenCustomerIsActive_DeactivatesCustomer()
    {
        var customer = CreateValidCustomer();

        customer.Deactivate();

        Assert.False(customer.IsActive);
    }

    [Fact]
    public void Deactivate_WhenCustomerIsAlreadyInactive_ThrowsDomainException()
    {
        var customer = CreateValidCustomer();
        customer.Deactivate();

        var exception = Assert.Throws<DomainException>(
            customer.Deactivate);

        Assert.Equal("El cliente ya está inactivo.", exception.Message);
    }

    [Fact]
    public void Activate_WhenCustomerIsInactive_ActivatesCustomer()
    {
        var customer = CreateValidCustomer();
        customer.Deactivate();

        customer.Activate();

        Assert.True(customer.IsActive);
    }

    [Fact]
    public void UpdateContactInformation_WithValidValues_UpdatesCustomer()
    {
        var customer = CreateValidCustomer();

        customer.UpdateContactInformation(
            "María López",
            "maria@example.com",
            "Guayaquil",
            "0988888888");

        Assert.Equal("María López", customer.Name);
        Assert.Equal("maria@example.com", customer.Email);
        Assert.Equal("Guayaquil", customer.Address);
        Assert.Equal("0988888888", customer.Phone);
    }

    private static Customer CreateValidCustomer()
    {
        return new Customer(
            IdentificationType.Cedula,
            "1723456789",
            "Juan Pérez",
            "juan@example.com");
    }
}
