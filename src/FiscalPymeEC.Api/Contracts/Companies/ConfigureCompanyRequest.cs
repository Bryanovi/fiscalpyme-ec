namespace FiscalPymeEC.Api.Contracts.Companies;

public sealed record ConfigureCompanyRequest(
    string Ruc,
    string LegalName,
    string TradeName,
    string Address,
    string EstablishmentCode,
    string EmissionPointCode,
    int SriEnvironment);