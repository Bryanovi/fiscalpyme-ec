using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FiscalPymeEC.Api.Contracts.Authentication;
using FiscalPymeEC.Api.Contracts.Companies;
using FiscalPymeEC.IntegrationTests.Authentication;
using FiscalPymeEC.Api.Contracts.Users;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Companies;
using FiscalPymeEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FiscalPymeEC.IntegrationTests.Companies;

public sealed class CompanyEndpointTests
    : IClassFixture<AuthenticationApiFactory>,
      IAsyncLifetime
{
    private readonly AuthenticationApiFactory _factory;

    public CompanyEndpointTests(
        AuthenticationApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.SeedUserAsync();
        await _factory.ResetCompanyAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCompany_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/company");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCompany_WhenNotConfigured_ReturnsNotFound()
    {
        using var client = await CreateAdministratorClientAsync();

        var response = await client.GetAsync("/api/company");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConfigureCompany_AsAdministrator_ReturnsCreated()
    {
        using var client = await CreateAdministratorClientAsync();

        var request = CreateCompanyRequest();

        var response = await client.PutAsJsonAsync(
            "/api/company",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var company = await response.Content
            .ReadFromJsonAsync<CompanyResponse>();

        Assert.NotNull(company);
        Assert.Equal(request.Ruc, company.Ruc);
        Assert.Equal("001", company.EstablishmentCode);
        Assert.Equal("001", company.EmissionPointCode);
        Assert.Equal(1, company.SriEnvironment);
        Assert.Equal("Testing", company.SriEnvironmentName);
    }

    [Fact]
    public async Task ConfigureCompany_AsAdministrator_CreatesAuditEntry()
    {
        using var client = await CreateAdministratorClientAsync();

        var response = await client.PutAsJsonAsync(
            "/api/company",
            CreateCompanyRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var company = await response.Content
            .ReadFromJsonAsync<CompanyResponse>();

        Assert.NotNull(company);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var administrator = await dbContext.Users
            .SingleAsync(user =>
                user.Email == AuthenticationApiFactory.TestEmail);

        var auditEntry = await dbContext.AuditEntries
            .AsNoTracking()
            .SingleAsync(entry =>
                entry.EntityName == nameof(Company) &&
                entry.EntityId == company.CompanyId.ToString() &&
                entry.Action == AuditAction.Created);

        Assert.Equal(
            administrator.Id.ToString(),
            auditEntry.UserId);

        Assert.NotNull(auditEntry.NewValuesJson);

        Assert.Contains(
            "FiscalPyme Ecuador S.A.",
            auditEntry.NewValuesJson);
    }

    [Fact]
    public async Task ConfigureCompany_WithSameRuc_UpdatesExistingCompany()
    {
        using var client = await CreateAdministratorClientAsync();

        var creationResponse = await client.PutAsJsonAsync(
            "/api/company",
            CreateCompanyRequest());

        var createdCompany = await creationResponse.Content
            .ReadFromJsonAsync<CompanyResponse>();

        Assert.NotNull(createdCompany);

        var updateRequest = new ConfigureCompanyRequest(
            createdCompany.Ruc,
            "FiscalPyme Ecuador Actualizada S.A.",
            "FiscalPyme Actualizada",
            "Guayaquil, Ecuador",
            "002",
            "003",
            2);

        var updateResponse = await client.PutAsJsonAsync(
            "/api/company",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedCompany = await updateResponse.Content
            .ReadFromJsonAsync<CompanyResponse>();

        Assert.NotNull(updatedCompany);
        Assert.Equal(createdCompany.CompanyId, updatedCompany.CompanyId);
        Assert.Equal("FiscalPyme Actualizada", updatedCompany.TradeName);
        Assert.Equal("002", updatedCompany.EstablishmentCode);
        Assert.Equal("003", updatedCompany.EmissionPointCode);
        Assert.Equal(2, updatedCompany.SriEnvironment);
        Assert.Equal("Production", updatedCompany.SriEnvironmentName);
    }

    [Fact]
    public async Task ConfigureCompany_WithDifferentRuc_ReturnsBadRequest()
    {
        using var client = await CreateAdministratorClientAsync();

        await client.PutAsJsonAsync(
            "/api/company",
            CreateCompanyRequest());

        var changedRucRequest = new ConfigureCompanyRequest(
            "0999999999001",
            "Otra empresa S.A.",
            "Otra empresa",
            "Cuenca, Ecuador",
            "001",
            "001",
            1);

        var response = await client.PutAsJsonAsync(
            "/api/company",
            changedRucRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCompany_AsSeller_ReturnsOk()
    {
        using var administratorClient =
            await CreateAdministratorClientAsync();

        await administratorClient.PutAsJsonAsync(
            "/api/company",
            CreateCompanyRequest());

        using var sellerClient =
            await CreateSellerClientAsync();

        var response = await sellerClient.GetAsync(
            "/api/company");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfigureCompany_AsSeller_ReturnsForbidden()
    {
        using var sellerClient =
            await CreateSellerClientAsync();

        var response = await sellerClient.PutAsJsonAsync(
            "/api/company",
            CreateCompanyRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<HttpClient> CreateAdministratorClientAsync()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email = AuthenticationApiFactory.TestEmail,
                Password = AuthenticationApiFactory.TestPassword
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(login);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        return client;
    }

    private async Task<HttpClient> CreateSellerClientAsync()
    {
        using var administratorClient =
            await CreateAdministratorClientAsync();

        var sellerEmail =
            $"seller.{Guid.NewGuid():N}@fiscalpyme.ec";

        var sellerPassword = "Seller123!";

        var createUserResponse =
            await administratorClient.PostAsJsonAsync(
                "/api/users",
                new CreateUserRequest(
                    "Vendedor de integración",
                    sellerEmail,
                    sellerPassword));

        Assert.Equal(
            HttpStatusCode.Created,
            createUserResponse.StatusCode);

        var sellerClient = _factory.CreateClient();

        var loginResponse =
            await sellerClient.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = sellerEmail,
                    Password = sellerPassword
                });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(login);

        sellerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        return sellerClient;
    }

    private static ConfigureCompanyRequest CreateCompanyRequest()
    {
        return new ConfigureCompanyRequest(
            "1790012345001",
            "FiscalPyme Ecuador S.A.",
            "FiscalPyme",
            "Quito, Ecuador",
            "001",
            "001",
            1);
    }
}