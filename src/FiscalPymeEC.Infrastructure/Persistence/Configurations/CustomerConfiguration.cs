using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration
    : IEntityTypeConfiguration<Customer>
{
    public void Configure(
        EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .HasColumnName("id");

        builder.Property(customer => customer.IdentificationType)
            .HasColumnName("identification_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(customer => customer.Identification)
            .HasColumnName("identification")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(customer => customer.Identification)
            .IsUnique()
            .HasDatabaseName("ux_customers_identification");

        builder.Property(customer => customer.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasColumnName("email")
            .HasMaxLength(254);

        builder.Property(customer => customer.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(customer => customer.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30);

        builder.Property(customer => customer.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        ConfigureAuditProperties(builder);
    }

    private static void ConfigureAuditProperties(
        EntityTypeBuilder<Customer> builder)
    {
        builder.Property(customer => customer.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(customer => customer.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(customer => customer.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(customer => customer.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
    }
}
