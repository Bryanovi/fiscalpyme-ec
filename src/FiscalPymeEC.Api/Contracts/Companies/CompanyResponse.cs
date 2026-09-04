namespace FiscalPymeEC.Api.Contracts.Companies;

public sealed record CompanyResponse(
    Guid CompanyId,
    string Ruc,
    string LegalName,
    string TradeName,
    string Address,
    string EstablishmentCode,
    string EmissionPointCode,
    int SriEnvironment,
    string SriEnvironmentName);