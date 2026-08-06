using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FiscalPymeEC.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Email == normalizedEmail,
                cancellationToken);
    }
}