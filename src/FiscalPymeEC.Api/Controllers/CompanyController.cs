using FiscalPymeEC.Api.Contracts.Companies;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Application.Companies;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalPymeEC.Api.Controllers;

[ApiController]
[Route("api/company")]
public sealed class CompanyController : ControllerBase
{
    private readonly IConfigureCompanyService
        _configureCompanyService;

    private readonly IGetCompanyService
        _getCompanyService;

    public CompanyController(
        IConfigureCompanyService configureCompanyService,
        IGetCompanyService getCompanyService)
    {
        _configureCompanyService =
            configureCompanyService;

        _getCompanyService =
            getCompanyService;
    }

    [Authorize(Policy = "SellerOrAdministrator")]
    [HttpGet]
    [ProducesResponseType<CompanyResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompanyResponse>> Get(
        CancellationToken cancellationToken)
    {
        var result = await _getCompanyService.GetAsync(
            cancellationToken);

        if (result is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status404NotFound,

                    Title =
                        "La empresa no está configurada.",

                    Detail =
                        "Un administrador debe configurar la empresa."
                });
        }

        return Ok(MapResponse(result));
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpPut]
    [ProducesResponseType<CompanyResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<CompanyResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompanyResponse>> Configure(
        [FromBody] ConfigureCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ConfigureCompanyCommand(
                request.Ruc,
                request.LegalName,
                request.TradeName,
                request.Address,
                request.EstablishmentCode,
                request.EmissionPointCode,
                (SriEnvironment)request.SriEnvironment);

            var result =
                await _configureCompanyService.ConfigureAsync(
                    command,
                    cancellationToken);

            var response = MapResponse(result);

            if (result.WasCreated)
            {
                return Created(
                    "/api/company",
                    response);
            }

            return Ok(response);
        }
        catch (DomainException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "No se pudo configurar la empresa.",

                    Detail = exception.Message
                });
        }
    }

    private static CompanyResponse MapResponse(
        GetCompanyResult result)
    {
        return new CompanyResponse(
            result.CompanyId,
            result.Ruc,
            result.LegalName,
            result.TradeName,
            result.Address,
            result.EstablishmentCode,
            result.EmissionPointCode,
            result.SriEnvironment,
            GetEnvironmentName(result.SriEnvironment));
    }

    private static CompanyResponse MapResponse(
        ConfigureCompanyResult result)
    {
        return new CompanyResponse(
            result.CompanyId,
            result.Ruc,
            result.LegalName,
            result.TradeName,
            result.Address,
            result.EstablishmentCode,
            result.EmissionPointCode,
            result.SriEnvironment,
            GetEnvironmentName(result.SriEnvironment));
    }

    private static string GetEnvironmentName(
        int sriEnvironment)
    {
        return ((SriEnvironment)sriEnvironment)
            .ToString();
    }
}