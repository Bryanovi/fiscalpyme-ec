using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public string? UpdatedBy { get; private set; }

    public void RegisterCreation(string userId, DateTimeOffset occurredAtUtc)
    {
        CreatedBy = userId;
        CreatedAtUtc = occurredAtUtc;
    }

    public void RegisterModification(string userId, DateTimeOffset occurredAtUtc)
    {
        UpdatedBy = userId;
        UpdatedAtUtc = occurredAtUtc;
    }
}
