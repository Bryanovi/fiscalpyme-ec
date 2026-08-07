using FiscalPymeEC.Domain.Companies;

namespace FiscalPymeEC.Application.Companies;

public sealed record ConfigureCompanyCommand(
    string Ruc,
    string LegalName,
    string TradeName,
    string Address,
    string EstablishmentCode,
    string EmissionPointCode,
    SriEnvironment SriEnvironment);