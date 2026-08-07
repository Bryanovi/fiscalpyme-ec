using FiscalPymeEC.Api.Contracts.Users;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Application.Users;
using FiscalPymeEC.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalPymeEC.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class UsersController : ControllerBase
{
    private readonly ICreateUserService _createUserService;

    public UsersController(
        ICreateUserService createUserService)
    {
        _createUserService = createUserService;
    }

    [HttpPost]
    [ProducesResponseType<CreateUserResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateUserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateUserCommand(
                request.FullName,
                request.Email,
                request.Password);

            var result =
                await _createUserService.CreateAsync(
                    command,
                    cancellationToken);

            var response = new CreateUserResponse(
                result.UserId,
                result.FullName,
                result.Email,
                result.Role,
                result.IsActive);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (DomainException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "No se pudo crear el usuario.",

                    Detail = exception.Message
                });
        }
    }
}