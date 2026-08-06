using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Companies;
using FiscalPymeEC.Domain.Customers;
using FiscalPymeEC.Domain.Invoices;
using FiscalPymeEC.Domain.Products;
using Microsoft.EntityFrameworkCore;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IClock clock)
        : base(options)
    {
        _currentUserService = currentUserService;
        _clock = clock;
    }

    public DbSet<Company> Companies =>
        Set<Company>();

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<Product> Products =>
        Set<Product>();

    public DbSet<Invoice> Invoices =>
        Set<Invoice>();

    public DbSet<AuditEntry> AuditEntries =>
        Set<AuditEntry>();

    public DbSet<User> Users =>
    Set<User>();

    public override int SaveChanges()
    {
        ApplyAuditMetadata();
        AddAuditEntries();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditMetadata();
        AddAuditEntries();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }

    private void AddAuditEntries()
    {
        var changedEntries = ChangeTracker
            .Entries<Entity>()
            .Where(entry =>
                entry.Entity is not AuditEntry &&
                entry.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted)
            .ToList();

        var auditEntries = changedEntries
            .Select(CreateAuditEntry)
            .ToList();

        AuditEntries.AddRange(auditEntries);
    }

    private AuditEntry CreateAuditEntry(
    EntityEntry<Entity> entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (ShouldIgnoreProperty(propertyName))
            {
                continue;
            }

            if (entry.State == EntityState.Modified &&
                !property.IsModified)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[propertyName] =
                        property.CurrentValue;
                    break;

                case EntityState.Modified:
                    oldValues[propertyName] =
                        property.OriginalValue;

                    newValues[propertyName] =
                        property.CurrentValue;
                    break;

                case EntityState.Deleted:
                    oldValues[propertyName] =
                        property.OriginalValue;
                    break;
            }
        }

        return new AuditEntry(
            userId: _currentUserService.UserId,
            action: GetAuditAction(entry.State),
            entityName: entry.Metadata.ClrType.Name,
            entityId: entry.Entity.Id.ToString(),
            occurredAtUtc: _clock.UtcNow,
            oldValuesJson: SerializeValues(oldValues),
            newValuesJson: SerializeValues(newValues),
            correlationId: _currentUserService.CorrelationId,
            ipAddress: _currentUserService.IpAddress);
    }

    private static AuditAction GetAuditAction(
    EntityState entityState)
    {
        return entityState switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Modified => AuditAction.Updated,
            EntityState.Deleted => AuditAction.Deleted,

            _ => throw new InvalidOperationException(
                $"El estado {entityState} no puede auditarse.")
        };
    }

    private static string? SerializeValues(
        Dictionary<string, object?> values)
    {
        if (values.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            values,
            new JsonSerializerOptions
            {
                DictionaryKeyPolicy =
                    JsonNamingPolicy.CamelCase
            });
    }

    private static bool ShouldIgnoreProperty(
        string propertyName)
    {
        string[] ignoredProperties =
        [
            "Id",
        "CreatedAtUtc",
        "CreatedBy",
        "UpdatedAtUtc",
        "UpdatedBy"
        ];

        if (ignoredProperties.Contains(propertyName))
        {
            return true;
        }

        string[] sensitiveTerms =
        [
            "Password",
        "PasswordHash",
        "Token",
        "RefreshToken",
        "PrivateKey",
        "Certificate",
        "ConnectionString"
        ];

        return sensitiveTerms.Any(term =>
            propertyName.Contains(
                term,
                StringComparison.OrdinalIgnoreCase));
    }

    private void ApplyAuditMetadata()
    {
        var auditableEntries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified);

        foreach (var entry in auditableEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.RegisterCreation(
                    _currentUserService.UserId,
                    _clock.UtcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.RegisterModification(
                    _currentUserService.UserId,
                    _clock.UtcNow);
            }
        }
    }
}
