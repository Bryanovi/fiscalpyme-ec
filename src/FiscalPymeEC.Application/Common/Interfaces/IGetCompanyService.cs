using FiscalPymeEC.Application.Companies;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IGetCompanyService
{
    Task<GetCompanyResult?> GetAsync(
        CancellationToken cancellationToken = default);
}