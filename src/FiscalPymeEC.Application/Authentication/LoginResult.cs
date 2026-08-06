using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Application.Authentication;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email,
    string Role);
