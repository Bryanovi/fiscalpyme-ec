using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Companies;

public sealed class Company : AuditableEntity
{
    private Company()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Company(
        string ruc,
        string legalName,
        string tradeName,
        string address,
        string establishmentCode,
        string emissionPointCode,
        SriEnvironment sriEnvironment)
    {
        Ruc = ValidateRuc(ruc);

        LegalName = ValidateRequiredText(
            legalName,
            "La razón social");

        TradeName = ValidateRequiredText(
            tradeName,
            "El nombre comercial");

        Address = ValidateRequiredText(
            address,
            "La dirección");

        EstablishmentCode = ValidateSriCode(
            establishmentCode,
            "El código de establecimiento");

        EmissionPointCode = ValidateSriCode(
            emissionPointCode,
            "El código del punto de emisión");

        SriEnvironment = ValidateSriEnvironment(
            sriEnvironment);
    }

    public string Ruc { get; private set; } = string.Empty;

    public string LegalName { get; private set; } = string.Empty;

    public string TradeName { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string EstablishmentCode { get; private set; } =
        string.Empty;

    public string EmissionPointCode { get; private set; } =
        string.Empty;

    public SriEnvironment SriEnvironment { get; private set; }

    public void UpdateBusinessInformation(
        string legalName,
        string tradeName,
        string address)
    {
        LegalName = ValidateRequiredText(
            legalName,
            "La razón social");

        TradeName = ValidateRequiredText(
            tradeName,
            "El nombre comercial");

        Address = ValidateRequiredText(
            address,
            "La dirección");
    }

    public void UpdateElectronicInvoicingSettings(
        string establishmentCode,
        string emissionPointCode,
        SriEnvironment sriEnvironment)
    {
        EstablishmentCode = ValidateSriCode(
            establishmentCode,
            "El código de establecimiento");

        EmissionPointCode = ValidateSriCode(
            emissionPointCode,
            "El código del punto de emisión");

        SriEnvironment = ValidateSriEnvironment(
            sriEnvironment);
    }

    private static string ValidateRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
        {
            throw new DomainException(
                "El RUC es obligatorio.");
        }

        var normalizedRuc = ruc.Trim();

        if (normalizedRuc.Length != 13 ||
            !normalizedRuc.All(char.IsDigit))
        {
            throw new DomainException(
                "El RUC debe contener exactamente 13 dígitos.");
        }

        return normalizedRuc;
    }

    private static string ValidateSriCode(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                $"{fieldName} es obligatorio.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length != 3 ||
            !normalizedValue.All(char.IsDigit))
        {
            throw new DomainException(
                $"{fieldName} debe contener exactamente 3 dígitos.");
        }

        return normalizedValue;
    }

    private static SriEnvironment ValidateSriEnvironment(
        SriEnvironment sriEnvironment)
    {
        if (!Enum.IsDefined(sriEnvironment))
        {
            throw new DomainException(
                "El ambiente del SRI no es válido.");
        }

        return sriEnvironment;
    }

    private static string ValidateRequiredText(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                $"{fieldName} es obligatorio.");
        }

        return value.Trim();
    }
}