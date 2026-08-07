namespace FiscalPymeEC.Application.Users;

public sealed record CreateUserResult(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive);