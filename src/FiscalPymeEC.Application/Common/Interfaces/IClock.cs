using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
