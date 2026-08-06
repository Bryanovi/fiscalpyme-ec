namespace FiscalPymeEC.Api.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email,
    string Role);