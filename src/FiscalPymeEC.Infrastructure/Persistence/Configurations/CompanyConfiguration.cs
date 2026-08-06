using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration
    : IEntityTypeConfiguration<Company>
{
    public void Configure(
        EntityTypeBuilder<Company> builder)
    {
        builder.ToTable(
            "companies",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_companies_singleton_key",
                    "singleton_key = 1");
            });

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Id)
            .HasColumnName("id");

        builder.Property(company => company.Ruc)
            .HasColumnName("ruc")
            .HasMaxLength(13)
            .IsRequired();

        builder.HasIndex(company => company.Ruc)
            .IsUnique()
            .HasDatabaseName("ux_companies_ruc");

        builder.Property(company => company.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(company => company.TradeName)
            .HasColumnName("trade_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(company => company.Address)
            .HasColumnName("address")
            .HasMaxLength(500)
            .IsRequired();

        ConfigureAuditProperties(builder);
        ConfigureSingleCompanyConstraint(builder);
    }

    private static void ConfigureAuditProperties(
        EntityTypeBuilder<Company> builder)
    {
        builder.Property(company => company.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(company => company.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(company => company.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(company => company.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
    }

    private static void ConfigureSingleCompanyConstraint(
        EntityTypeBuilder<Company> builder)
    {
        builder.Property<int>("SingletonKey")
            .HasColumnName("singleton_key")
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasIndex("SingletonKey")
            .IsUnique()
            .HasDatabaseName("ux_companies_singleton_key");
    }
}