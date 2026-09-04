using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Companies;

namespace FiscalPymeEC.Application.Companies;

public sealed class ConfigureCompanyService
    : IConfigureCompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public ConfigureCompanyService(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<ConfigureCompanyResult> ConfigureAsync(
        ConfigureCompanyCommand command,
        CancellationToken cancellationToken = default)
    {
        var company =
            await _companyRepository.GetAsync(
                cancellationToken);

        var wasCreated = company is null;

        if (company is null)
        {
            company = new Company(
                command.Ruc,
                command.LegalName,
                command.TradeName,
                command.Address,
                command.EstablishmentCode,
                command.EmissionPointCode,
                command.SriEnvironment);

            _companyRepository.Add(company);
        }
        else
        {
            ValidateExistingRuc(
                company,
                command.Ruc);

            company.UpdateBusinessInformation(
                command.LegalName,
                command.TradeName,
                command.Address);

            company.UpdateElectronicInvoicingSettings(
                command.EstablishmentCode,
                command.EmissionPointCode,
                command.SriEnvironment);
        }

        await _companyRepository.SaveChangesAsync(
            cancellationToken);

        return new ConfigureCompanyResult(
            company.Id,
            company.Ruc,
            company.LegalName,
            company.TradeName,
            company.Address,
            company.EstablishmentCode,
            company.EmissionPointCode,
            (int)company.SriEnvironment,
            wasCreated);
    }

    private static void ValidateExistingRuc(
        Company company,
        string requestedRuc)
    {
        if (string.IsNullOrWhiteSpace(requestedRuc))
        {
            throw new DomainException(
                "El RUC es obligatorio.");
        }

        var normalizedRuc = requestedRuc.Trim();

        if (!string.Equals(
                company.Ruc,
                normalizedRuc,
                StringComparison.Ordinal))
        {
            throw new DomainException(
                "El RUC de la empresa configurada no puede modificarse.");
        }
    }
}