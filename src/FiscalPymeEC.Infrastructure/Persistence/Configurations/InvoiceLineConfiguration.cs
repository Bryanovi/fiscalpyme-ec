using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Invoices;
using FiscalPymeEC.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration
    : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(
        EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .HasColumnName("id");

        builder.Property(line => line.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.HasIndex(line => line.InvoiceId)
            .HasDatabaseName("ix_invoice_lines_invoice_id");

        builder.Property(line => line.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.HasIndex(line => line.ProductId)
            .HasDatabaseName("ix_invoice_lines_product_id");

        builder.Property(line => line.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(line => line.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(line => line.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(line => line.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(line => line.DiscountRate)
            .HasColumnName("discount_rate")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(line => line.IvaRate)
            .HasColumnName("iva_rate")
            .HasPrecision(5, 4)
            .IsRequired();

        ConfigureProductRelationship(builder);
        ConfigureCalculatedProperties(builder);
    }

    private static void ConfigureProductRelationship(
        EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCalculatedProperties(
        EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.Ignore(line => line.Subtotal);
        builder.Ignore(line => line.DiscountAmount);
        builder.Ignore(line => line.TaxableBase);
        builder.Ignore(line => line.IvaAmount);
        builder.Ignore(line => line.Total);
    }
}
