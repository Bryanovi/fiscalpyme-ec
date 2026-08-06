using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Auditing;
using FiscalPymeEC.Domain.Companies;
using FiscalPymeEC.Domain.Customers;
using FiscalPymeEC.Domain.Invoices;
using FiscalPymeEC.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace FiscalPymeEC.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
