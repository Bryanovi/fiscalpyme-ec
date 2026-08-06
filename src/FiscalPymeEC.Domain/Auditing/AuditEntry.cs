using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.Domain.Auditing;

public sealed class AuditEntry : Entity
{
    private AuditEntry()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public AuditEntry(
        string userId,
        AuditAction action,
        string entityName,
        string entityId,
        DateTimeOffset occurredAtUtc,
        string? oldValuesJson = null,
        string? newValuesJson = null,
        string? correlationId = null,
        string? ipAddress = null)
    {
        UserId = ValidateRequiredText(
            userId,
            "El identificador del usuario");

        Action = ValidateAction(action);

        EntityName = ValidateRequiredText(
            entityName,
            "El nombre de la entidad");

        EntityId = ValidateRequiredText(
            entityId,
            "El identificador de la entidad");

        OccurredAtUtc = occurredAtUtc.ToUniversalTime();

        OldValuesJson = ValidateJson(
            oldValuesJson,
            "Los valores anteriores");

        NewValuesJson = ValidateJson(
            newValuesJson,
            "Los valores nuevos");

        CorrelationId = NormalizeOptionalText(correlationId);
        IpAddress = NormalizeOptionalText(ipAddress);
    }

    public string UserId { get; private set; } = string.Empty;

    public AuditAction Action { get; private set; }

    public string EntityName { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? OldValuesJson { get; private set; }

    public string? NewValuesJson { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? IpAddress { get; private set; }

    private static AuditAction ValidateAction(AuditAction action)
    {
        if (!Enum.IsDefined(action))
        {
            throw new DomainException(
                "La acción de auditoría no es válida.");
        }

        return action;
    }

    private static string? ValidateJson(
        string? json,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var normalizedJson = json.Trim();

        try
        {
            using var document = JsonDocument.Parse(normalizedJson);
            return normalizedJson;
        }
        catch (JsonException)
        {
            throw new DomainException(
                $"{fieldName} debe contener un JSON válido.");
        }
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

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
