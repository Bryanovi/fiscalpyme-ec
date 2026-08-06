using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Application.Authentication;

public sealed record LoginCommand(
    string Email,
    string Password);
