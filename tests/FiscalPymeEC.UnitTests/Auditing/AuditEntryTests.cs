using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Common;

namespace FiscalPymeEC.UnitTests.Auditing;

public sealed class AuditEntryTests
{
    [Fact]
    public void Constructor_WithValidInformation_CreatesAuditEntry()
    {
        var occurredAt = new DateTimeOffset(
            2026,
            8,
            6,
            10,
            30,
            0,
            TimeSpan.Zero);

        var auditEntry = new AuditEntry(
            userId: "user-123",
            action: AuditAction.Updated,
            entityName: "Customer",
            entityId: Guid.NewGuid().ToString(),
            occurredAtUtc: occurredAt,
            oldValuesJson: "{\"Name\":\"Juan\"}",
            newValuesJson: "{\"Name\":\"Pedro\"}",
            correlationId: "request-123",
            ipAddress: "127.0.0.1");

        Assert.NotEqual(Guid.Empty, auditEntry.Id);
        Assert.Equal("user-123", auditEntry.UserId);
        Assert.Equal(AuditAction.Updated, auditEntry.Action);
        Assert.Equal("Customer", auditEntry.EntityName);
        Assert.Equal(occurredAt, auditEntry.OccurredAtUtc);
        Assert.Equal(
            "{\"Name\":\"Juan\"}",
            auditEntry.OldValuesJson);
        Assert.Equal(
            "{\"Name\":\"Pedro\"}",
            auditEntry.NewValuesJson);
        Assert.Equal(
            "request-123",
            auditEntry.CorrelationId);
        Assert.Equal("127.0.0.1", auditEntry.IpAddress);
    }

    [Fact]
    public void Constructor_ConvertsDateToUtc()
    {
        var localDate = new DateTimeOffset(
            2026,
            8,
            6,
            10,
            0,
            0,
            TimeSpan.FromHours(-5));

        var auditEntry = CreateAuditEntry(
            occurredAt: localDate);

        Assert.Equal(
            TimeSpan.Zero,
            auditEntry.OccurredAtUtc.Offset);

        Assert.Equal(
            15,
            auditEntry.OccurredAtUtc.Hour);
    }

    [Fact]
    public void Constructor_WithInvalidOldValuesJson_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new AuditEntry(
                "user-123",
                AuditAction.Updated,
                "Customer",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                oldValuesJson: "{invalid-json}"));

        Assert.Equal(
            "Los valores anteriores debe contener un JSON válido.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidNewValuesJson_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new AuditEntry(
                "user-123",
                AuditAction.Updated,
                "Customer",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                newValuesJson: "{invalid-json}"));

        Assert.Equal(
            "Los valores nuevos debe contener un JSON válido.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new AuditEntry(
                "",
                AuditAction.Created,
                "Customer",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "El identificador del usuario es obligatorio.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidAction_ThrowsDomainException()
    {
        var invalidAction = (AuditAction)999;

        var exception = Assert.Throws<DomainException>(() =>
            new AuditEntry(
                "user-123",
                invalidAction,
                "Customer",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow));

        Assert.Equal(
            "La acción de auditoría no es válida.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithOptionalValuesEmpty_StoresNull()
    {
        var auditEntry = new AuditEntry(
            "user-123",
            AuditAction.Created,
            "Customer",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            oldValuesJson: "",
            newValuesJson: " ",
            correlationId: "",
            ipAddress: " ");

        Assert.Null(auditEntry.OldValuesJson);
        Assert.Null(auditEntry.NewValuesJson);
        Assert.Null(auditEntry.CorrelationId);
        Assert.Null(auditEntry.IpAddress);
    }

    private static AuditEntry CreateAuditEntry(
        DateTimeOffset occurredAt)
    {
        return new AuditEntry(
            "user-123",
            AuditAction.Created,
            "Customer",
            Guid.NewGuid().ToString(),
            occurredAt);
    }
}