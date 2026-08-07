using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FiscalPymeEC.Api.Contracts.Authentication;

namespace FiscalPymeEC.IntegrationTests.Authentication;

public sealed class AuthenticationEndpointTests
    : IClassFixture<AuthenticationApiFactory>,
      IAsyncLifetime
{
    private readonly AuthenticationApiFactory _factory;

    public AuthenticationEndpointTests(
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
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            CreateValidLoginRequest());

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var loginResponse =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(
            loginResponse.AccessToken));

        Assert.Equal(
            AuthenticationApiFactory.TestEmail,
            loginResponse.Email);

        Assert.Equal(
            "Administrator",
            loginResponse.Role);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var request = new LoginRequest
        {
            Email = AuthenticationApiFactory.TestEmail,
            Password = "IncorrectPassword"
        };

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUser()
    {
        using var client = _factory.CreateClient();

        var loginHttpResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                CreateValidLoginRequest());

        var loginResponse =
            await loginHttpResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResponse.AccessToken);

        var currentUserResponse =
            await client.GetAsync(
                "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            currentUserResponse.StatusCode);

        var currentUser =
            await currentUserResponse.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        Assert.NotNull(currentUser);

        Assert.Equal(
            AuthenticationApiFactory.TestEmail,
            currentUser.Email);

        Assert.Equal(
            "Administrator",
            currentUser.Role);
    }

    private static LoginRequest CreateValidLoginRequest()
    {
        return new LoginRequest
        {
            Email = AuthenticationApiFactory.TestEmail,
            Password = AuthenticationApiFactory.TestPassword
        };
    }
}