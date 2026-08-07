namespace FiscalPymeEC.Api.Contracts.Users;

public sealed record CreateUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive);