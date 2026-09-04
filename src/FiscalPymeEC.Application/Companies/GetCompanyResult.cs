namespace FiscalPymeEC.Application.Companies;

public sealed record GetCompanyResult(
    Guid CompanyId,
    string Ruc,
    string LegalName,
    string TradeName,
    string Address,
    string EstablishmentCode,
    string EmissionPointCode,
    int SriEnvironment);