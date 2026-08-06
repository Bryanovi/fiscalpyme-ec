using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Authentication;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.Application.Common.Interfaces;

public interface ITokenGenerator
{
    AuthenticationToken Generate(User user);
}