namespace FiscalPymeEC.Application.Users;

public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Password);