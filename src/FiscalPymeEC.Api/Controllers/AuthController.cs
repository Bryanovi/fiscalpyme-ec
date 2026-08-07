using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FiscalPymeEC.Api.Contracts.Authentication;
using FiscalPymeEC.Application.Authentication;
using FiscalPymeEC.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalPymeEC.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IAuthenticationAuditService
    _authenticationAuditService;

    public AuthController(
    ILoginService loginService,
    IAuthenticationAuditService authenticationAuditService)
    {
        _loginService = loginService;
        _authenticationAuditService =
            authenticationAuditService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password);

        var result = await _loginService.LoginAsync(
            command,
            cancellationToken);

        await _authenticationAuditService
            .RecordLoginAttemptAsync(
                result?.UserId,
                request.Email,
                result is not null,
                cancellationToken);

        if (result is null)
        {
            return Unauthorized(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status401Unauthorized,

                    Title = "Credenciales inválidas.",

                    Detail =
                        "El correo o la contraseña son incorrectos."
                });
        }


        return Ok(
            new LoginResponse(
                result.AccessToken,
                result.ExpiresAtUtc,
                result.UserId,
                result.FullName,
                result.Email,
                result.Role));


    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var response = new CurrentUserResponse(
            userId,
            User.FindFirstValue(ClaimTypes.Name),
            User.FindFirstValue(ClaimTypes.Email),
            User.FindFirstValue(ClaimTypes.Role));

        return Ok(response);
    }
}