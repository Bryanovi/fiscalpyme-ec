using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }

    bool IsAuthenticated { get; }

    string? CorrelationId { get; }

    string? IpAddress { get; }
}
