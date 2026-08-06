using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Customers;
using FiscalPymeEC.Infrastructure.Persistence;
using FiscalPymeEC.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace FiscalPymeEC.IntegrationTests.Persistence;

public sealed class AuditingTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenCustomerIsCreated_CreatesAuditEntry()
    {
        var occurredAt = new DateTimeOffset(
            2026,
            8,
            6,
            18,
            0,
            0,
            TimeSpan.Zero);

        var clock = new TestClock
        {
            UtcNow = occurredAt
        };

        var currentUser = new TestCurrentUserService();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(GetConnectionString())
            .Options;

        await using var context = new ApplicationDbContext(
            options,
            currentUser,
            clock);

        await context.Database.MigrateAsync();

        await using var transaction =
            await context.Database.BeginTransactionAsync();

        var customer = new Customer(
            IdentificationType.Cedula,
            CreateRandomIdentification(),
            "Cliente de integración",
            "integration@example.com");

        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        Assert.Equal(occurredAt, customer.CreatedAtUtc);
        Assert.Equal(
            "integration-test-user",
            customer.CreatedBy);

        var auditEntry = await context.AuditEntries
            .AsNoTracking()
            .SingleAsync(entry =>
                entry.EntityId == customer.Id.ToString() &&
                entry.EntityName == nameof(Customer));

        Assert.Equal(
            AuditAction.Created,
            auditEntry.Action);

        Assert.Equal(
            "integration-test-user",
            auditEntry.UserId);

        Assert.Equal(
            "integration-test-request",
            auditEntry.CorrelationId);

        Assert.Equal(
            "127.0.0.1",
            auditEntry.IpAddress);

        Assert.Null(auditEntry.OldValuesJson);
        Assert.NotNull(auditEntry.NewValuesJson);

        using var jsonDocument = JsonDocument.Parse(
            auditEntry.NewValuesJson!);

        var customerName = jsonDocument.RootElement
            .GetProperty("name")
            .GetString();

        Assert.Equal(
            "Cliente de integración",
            customerName);

        await transaction.RollbackAsync();
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(
            "FISCALPYME_TEST_CONNECTION")
            ?? "Host=localhost;Port=5433;Database=fiscalpyme_tests;Username=fiscalpyme;Password=fiscalpyme_dev";
    }

    private static string CreateRandomIdentification()
    {
        return Random.Shared
            .NextInt64(
                1_000_000_000,
                9_999_999_999)
            .ToString();
    }
}
