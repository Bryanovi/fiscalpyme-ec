using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.Application.Companies;

public sealed class GetCompanyService
    : IGetCompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompanyService(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<GetCompanyResult?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var company =
            await _companyRepository.GetAsync(
                cancellationToken);

        if (company is null)
        {
            return null;
        }

        return new GetCompanyResult(
            company.Id,
            company.Ruc,
            company.LegalName,
            company.TradeName,
            company.Address,
            company.EstablishmentCode,
            company.EmissionPointCode,
            (int)company.SriEnvironment);
    }
}