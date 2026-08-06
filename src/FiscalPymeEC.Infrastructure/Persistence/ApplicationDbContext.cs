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

    public override int SaveChanges()
    {
        ApplyAuditMetadata();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditMetadata();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
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
