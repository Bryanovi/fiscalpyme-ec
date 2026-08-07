using System.Text.Json;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Users;
using FiscalPymeEC.Infrastructure.Persistence;

namespace FiscalPymeEC.Infrastructure.Authentication;

public sealed class AuthenticationAuditService
    : IAuthenticationAuditService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUserService;

    public AuthenticationAuditService(
        ApplicationDbContext dbContext,
        IClock clock,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _currentUserService = currentUserService;
    }

    public async Task RecordLoginAttemptAsync(
        Guid? userId,
        string email,
        bool succeeded,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            string.IsNullOrWhiteSpace(email)
                ? "unknown"
                : email.Trim().ToLowerInvariant();

        var entityId =
            userId?.ToString()
            ?? normalizedEmail;

        var actorId =
            userId?.ToString()
            ?? "anonymous";

        var valuesJson = JsonSerializer.Serialize(
            new
            {
                Email = normalizedEmail,
                Succeeded = succeeded
            });

        var auditEntry = new AuditEntry(
            userId: actorId,
            action: succeeded
                ? AuditAction.LoginSucceeded
                : AuditAction.LoginFailed,
            entityName: nameof(User),
            entityId: entityId,
            occurredAtUtc: _clock.UtcNow,
            newValuesJson: valuesJson,
            correlationId:
                _currentUserService.CorrelationId,
            ipAddress:
                _currentUserService.IpAddress);

        _dbContext.AuditEntries.Add(auditEntry);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}