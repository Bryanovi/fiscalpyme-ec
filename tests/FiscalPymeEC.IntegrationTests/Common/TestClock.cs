using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.IntegrationTests.Common;

public sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}