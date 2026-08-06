using System.Security.Claims;
using FiscalPymeEC.Application.Common.Interfaces;

namespace FiscalPymeEC.Api.Services;

public sealed class CurrentUserService
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? HttpContext =>
        _httpContextAccessor.HttpContext;

    public string UserId =>
        HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier)
        ?? "system";

    public bool IsAuthenticated =>
        HttpContext?.User.Identity?.IsAuthenticated
        ?? false;

    public string? CorrelationId =>
        HttpContext?.TraceIdentifier;

    public string? IpAddress =>
        HttpContext?.Connection.RemoteIpAddress?
            .ToString();
}