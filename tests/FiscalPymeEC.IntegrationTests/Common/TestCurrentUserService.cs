using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.IntegrationTests.Common;

public sealed class TestCurrentUserService
    : ICurrentUserService
{
    public string UserId { get; set; } = "integration-test-user";

    public bool IsAuthenticated { get; set; } = true;

    public string? CorrelationId { get; set; } =
        "integration-test-request";

    public string? IpAddress { get; set; } =
        "127.0.0.1";
}