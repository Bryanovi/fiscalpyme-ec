using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Authentication;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface ILoginService
{
    Task<LoginResult?> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);
}