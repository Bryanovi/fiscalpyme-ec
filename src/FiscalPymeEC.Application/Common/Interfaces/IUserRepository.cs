using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    void Add(User user);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}