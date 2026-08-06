using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}
