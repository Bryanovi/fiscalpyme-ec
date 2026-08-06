using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Companies;
using FiscalPymeEC.Domain.Customers;
using FiscalPymeEC.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration
    : IEntityTypeConfiguration<Invoice>
{
    public void Configure(
        EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id");

        builder.Property(invoice => invoice.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(invoice => invoice.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(invoice => invoice.SequentialNumber)
            .HasColumnName("sequential_number")
            .HasMaxLength(17);

        builder.HasIndex(invoice => invoice.SequentialNumber)
            .IsUnique()
            .HasDatabaseName("ux_invoices_sequential_number");

        builder.Property(invoice => invoice.AccessKey)
            .HasColumnName("access_key")
            .HasMaxLength(49);

        builder.HasIndex(invoice => invoice.AccessKey)
            .IsUnique()
            .HasDatabaseName("ux_invoices_access_key");

        builder.Property(invoice => invoice.IssuedAtUtc)
            .HasColumnName("issued_at_utc");

        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(invoice => invoice.AuthorizationNumber)
            .HasColumnName("authorization_number")
            .HasMaxLength(100);

        builder.Property(invoice => invoice.AuthorizedAtUtc)
            .HasColumnName("authorized_at_utc");

        builder.Property(invoice => invoice.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(1000);

        ConfigureRelationships(builder);
        ConfigureLines(builder);
        ConfigureCalculatedProperties(builder);
        ConfigureAuditProperties(builder);
    }

    private static void ConfigureRelationships(
        EntityTypeBuilder<Invoice> builder)
    {
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(invoice => invoice.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(invoice => invoice.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLines(
        EntityTypeBuilder<Invoice> builder)
    {
        builder.HasMany(invoice => invoice.Lines)
            .WithOne()
            .HasForeignKey(line => line.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureCalculatedProperties(
        EntityTypeBuilder<Invoice> builder)
    {
        builder.Ignore(invoice => invoice.Subtotal);
        builder.Ignore(invoice => invoice.DiscountAmount);
        builder.Ignore(invoice => invoice.TaxableBase);
        builder.Ignore(invoice => invoice.IvaAmount);
        builder.Ignore(invoice => invoice.Total);
    }

    private static void ConfigureAuditProperties(
        EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(invoice => invoice.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(invoice => invoice.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(invoice => invoice.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(invoice => invoice.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
    }
}
