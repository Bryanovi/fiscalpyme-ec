using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;
using FiscalPymeEC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiscalPymeEC.IntegrationTests.Authentication;

public sealed class AuthenticationApiFactory
    : WebApplicationFactory<Program>
{
    public const string TestEmail =
        "integration-admin@fiscalpyme.ec";

    public const string TestPassword =
        "IntegrationTest123!";

    private const string TestJwtSecret =
        "IntegrationTestsOnlySecretKeyWithMoreThan32Bytes123456789";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var settings =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:FiscalPyme"] =
                            GetConnectionString(),

                        ["Jwt:Issuer"] =
                            "FiscalPymeEC.IntegrationTests",

                        ["Jwt:Audience"] =
                            "FiscalPymeEC.IntegrationTests.Client",

                        ["Jwt:SecretKey"] =
                            TestJwtSecret,

                        ["Jwt:ExpirationMinutes"] =
                            "60"
                    };

                configurationBuilder.AddInMemoryCollection(
                    settings);
            });
    }

    public async Task SeedUserAsync()
    {
        await using var scope =
            Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var passwordHasher =
            scope.ServiceProvider
                .GetRequiredService<IPasswordHasher>();

        await dbContext.Database.MigrateAsync();

        var user = await dbContext.Users
            .SingleOrDefaultAsync(
                existingUser =>
                    existingUser.Email == TestEmail);

        if (user is null)
        {
            var passwordHash =
                passwordHasher.Hash(TestPassword);

            user = new User(
                "Administrador de integración",
                TestEmail,
                passwordHash,
                UserRole.Administrator);

            dbContext.Users.Add(user);
        }
        else
        {
            var passwordIsValid =
                passwordHasher.Verify(
                    TestPassword,
                    user.PasswordHash);

            if (!passwordIsValid)
            {
                user.ChangePasswordHash(
                    passwordHasher.Hash(TestPassword));
            }

            if (!user.IsActive)
            {
                user.Activate();
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }
    }



    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(
            "FISCALPYME_TEST_CONNECTION")
            ?? "Host=127.0.0.1;Port=5433;Database=fiscalpyme_tests;Username=fiscalpyme;Password=fiscalpyme_dev";
    }
}