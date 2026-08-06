using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FiscalPymeEC.Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task SeedInitialAdministratorAsync(
        CancellationToken cancellationToken = default)
    {
        var usersExist = await _dbContext.Users
            .AnyAsync(cancellationToken);

        if (usersExist)
        {
            return;
        }

        var fullName =
            GetRequiredConfiguration(
                "SeedAdmin:FullName");

        var email =
            GetRequiredConfiguration(
                "SeedAdmin:Email");

        var password =
            GetRequiredConfiguration(
                "SeedAdmin:Password");

        var passwordHash =
            _passwordHasher.Hash(password);

        var administrator = new User(
            fullName,
            email,
            passwordHash,
            UserRole.Administrator);

        _dbContext.Users.Add(administrator);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private string GetRequiredConfiguration(
        string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"No se encontró la configuración '{key}'.");
        }

        return value;
    }
}
