using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FiscalPymeEC.Api.Contracts.Authentication;
using FiscalPymeEC.Api.Contracts.Users;
using FiscalPymeEC.IntegrationTests.Authentication;

namespace FiscalPymeEC.IntegrationTests.Users;

public sealed class UsersEndpointTests
    : IClassFixture<AuthenticationApiFactory>,
      IAsyncLifetime
{
    private readonly AuthenticationApiFactory _factory;

    public UsersEndpointTests(
        AuthenticationApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.SeedUserAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateUser_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var request = CreateUserRequest();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_AsAdministrator_ReturnsCreatedSeller()
    {
        using var client =
            await CreateAdministratorClientAsync();

        var request = CreateUserRequest();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdUser =
            await response.Content
                .ReadFromJsonAsync<CreateUserResponse>();

        Assert.NotNull(createdUser);
        Assert.Equal(request.FullName, createdUser.FullName);
        Assert.Equal(request.Email, createdUser.Email);
        Assert.Equal("Seller", createdUser.Role);
        Assert.True(createdUser.IsActive);
        Assert.NotEqual(Guid.Empty, createdUser.UserId);
    }

    [Fact]
    public async Task CreateUser_AsSeller_ReturnsForbidden()
    {
        const string sellerPassword = "Seller123!";

        using var administratorClient =
            await CreateAdministratorClientAsync();

        var sellerRequest = new CreateUserRequest(
            "Vendedor de integración",
            CreateUniqueEmail("seller"),
            sellerPassword);

        var creationResponse =
            await administratorClient.PostAsJsonAsync(
                "/api/users",
                sellerRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            creationResponse.StatusCode);

        using var sellerClient = _factory.CreateClient();

        var loginResponse =
            await sellerClient.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = sellerRequest.Email,
                    Password = sellerPassword
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);

        sellerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        var forbiddenResponse =
            await sellerClient.PostAsJsonAsync(
                "/api/users",
                CreateUserRequest());

        Assert.Equal(
            HttpStatusCode.Forbidden,
            forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        using var client =
            await CreateAdministratorClientAsync();

        var request = CreateUserRequest();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/users",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/users",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    private async Task<HttpClient>
        CreateAdministratorClientAsync()
    {
        var client = _factory.CreateClient();

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email =
                        AuthenticationApiFactory.TestEmail,

                    Password =
                        AuthenticationApiFactory.TestPassword
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.AccessToken);

        return client;
    }

    private static CreateUserRequest CreateUserRequest()
    {
        return new CreateUserRequest(
            "Vendedor automático",
            CreateUniqueEmail("new-seller"),
            "Seller123!");
    }

    private static string CreateUniqueEmail(string prefix)
    {
        return $"{prefix}.{Guid.NewGuid():N}@fiscalpyme.ec";
    }
}