using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .HasColumnName("id");

        builder.Property(product => product.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Code)
            .IsUnique()
            .HasDatabaseName("ux_products_code");

        builder.Property(product => product.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(product => product.Type)
            .HasColumnName("product_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(product => product.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.IvaRate)
            .HasColumnName("iva_rate")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        ConfigureAuditProperties(builder);
    }

    private static void ConfigureAuditProperties(
        EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(product => product.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(product => product.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(product => product.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
    }
}