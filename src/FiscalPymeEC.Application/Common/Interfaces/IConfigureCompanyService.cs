using FiscalPymeEC.Application.Companies;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IConfigureCompanyService
{
    Task<ConfigureCompanyResult> ConfigureAsync(
        ConfigureCompanyCommand command,
        CancellationToken cancellationToken = default);
}