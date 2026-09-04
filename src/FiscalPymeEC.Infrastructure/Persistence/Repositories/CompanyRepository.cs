using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Companies;
using Microsoft.EntityFrameworkCore;

namespace FiscalPymeEC.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CompanyRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Company?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(Company company)
    {
        _dbContext.Companies.Add(company);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}