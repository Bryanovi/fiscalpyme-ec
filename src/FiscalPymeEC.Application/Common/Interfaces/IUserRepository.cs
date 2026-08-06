using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}