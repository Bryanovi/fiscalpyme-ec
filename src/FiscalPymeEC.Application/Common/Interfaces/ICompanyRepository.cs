using FiscalPymeEC.Domain.Companies;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetAsync(
        CancellationToken cancellationToken = default);

    void Add(Company company);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}