using FiscalPymeEC.Application.Users;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface ICreateUserService
{
    Task<CreateUserResult> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default);
}