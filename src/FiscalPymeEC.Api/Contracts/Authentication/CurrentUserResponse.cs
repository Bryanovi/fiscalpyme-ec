namespace FiscalPymeEC.Api.Contracts.Authentication;

public sealed record CurrentUserResponse(
    string UserId,
    string? Name,
    string? Email,
    string? Role);