using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IAuthenticationAuditService
{
    Task RecordLoginAttemptAsync(
        Guid? userId,
        string email,
        bool succeeded,
        CancellationToken cancellationToken = default);
}
