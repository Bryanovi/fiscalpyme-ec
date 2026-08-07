namespace FiscalPymeEC.Api.Contracts.Users;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password);