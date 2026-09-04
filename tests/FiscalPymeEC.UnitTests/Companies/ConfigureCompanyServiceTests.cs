using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Application.Companies;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Companies;

namespace FiscalPymeEC.UnitTests.Companies;

public sealed class ConfigureCompanyServiceTests
{
    [Fact]
    public async Task ConfigureAsync_WhenCompanyDoesNotExist_CreatesCompany()
    {
        var repository = new FakeCompanyRepository(
            company: null);

        var service = new ConfigureCompanyService(
            repository);

        var command = CreateCommand();

        var result = await service.ConfigureAsync(command);

        Assert.NotNull(repository.AddedCompany);
        Assert.True(repository.SaveChangesWasCalled);
        Assert.True(result.WasCreated);

        Assert.Equal(
            command.Ruc,
            repository.AddedCompany.Ruc);

        Assert.Equal(
            command.EstablishmentCode,
            repository.AddedCompany.EstablishmentCode);

        Assert.Equal(
            command.EmissionPointCode,
            repository.AddedCompany.EmissionPointCode);

        Assert.Equal(
            SriEnvironment.Testing,
            repository.AddedCompany.SriEnvironment);

        Assert.Equal(
            (int)SriEnvironment.Testing,
            result.SriEnvironment);
    }

    [Fact]
    public async Task ConfigureAsync_WhenCompanyExists_UpdatesSameCompany()
    {
        var existingCompany = new Company(
            "1790012345001",
            "Razón anterior",
            "Nombre anterior",
            "Dirección anterior",
            "001",
            "001",
            SriEnvironment.Testing);

        var repository = new FakeCompanyRepository(
            existingCompany);

        var service = new ConfigureCompanyService(
            repository);

        var command = new ConfigureCompanyCommand(
            existingCompany.Ruc,
            "Nueva razón social",
            "Nuevo nombre comercial",
            "Nueva dirección",
            "002",
            "003",
            SriEnvironment.Production);

        var result = await service.ConfigureAsync(command);

        Assert.Null(repository.AddedCompany);
        Assert.True(repository.SaveChangesWasCalled);
        Assert.False(result.WasCreated);

        Assert.Equal(
            existingCompany.Id,
            result.CompanyId);

        Assert.Equal(
            "Nueva razón social",
            existingCompany.LegalName);

        Assert.Equal(
            "Nuevo nombre comercial",
            existingCompany.TradeName);

        Assert.Equal(
            "Nueva dirección",
            existingCompany.Address);

        Assert.Equal(
            "002",
            existingCompany.EstablishmentCode);

        Assert.Equal(
            "003",
            existingCompany.EmissionPointCode);

        Assert.Equal(
            SriEnvironment.Production,
            existingCompany.SriEnvironment);
    }

    [Fact]
    public async Task ConfigureAsync_WhenRucChanges_ThrowsDomainException()
    {
        var existingCompany = new Company(
            "1790012345001",
            "FiscalPyme Ecuador S.A.",
            "FiscalPyme",
            "Quito, Ecuador",
            "001",
            "001",
            SriEnvironment.Testing);

        var repository = new FakeCompanyRepository(
            existingCompany);

        var service = new ConfigureCompanyService(
            repository);

        var command = new ConfigureCompanyCommand(
            "0999999999001",
            "Otra empresa",
            "Otra empresa",
            "Guayaquil, Ecuador",
            "002",
            "002",
            SriEnvironment.Testing);

        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ConfigureAsync(command));

        Assert.Equal(
            "El RUC de la empresa configurada no puede modificarse.",
            exception.Message);

        Assert.Null(repository.AddedCompany);
        Assert.False(repository.SaveChangesWasCalled);

        Assert.Equal(
            "FiscalPyme Ecuador S.A.",
            existingCompany.LegalName);
    }

    private static ConfigureCompanyCommand CreateCommand()
    {
        return new ConfigureCompanyCommand(
            "1790012345001",
            "FiscalPyme Ecuador S.A.",
            "FiscalPyme",
            "Quito, Ecuador",
            "001",
            "001",
            SriEnvironment.Testing);
    }

    private sealed class FakeCompanyRepository(
        Company? company) : ICompanyRepository
    {
        public Company? AddedCompany { get; private set; }

        public bool SaveChangesWasCalled { get; private set; }

        public Task<Company?> GetAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(company);
        }

        public void Add(Company newCompany)
        {
            AddedCompany = newCompany;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesWasCalled = true;

            return Task.CompletedTask;
        }
    }
}