using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration
    : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(
        EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnName("id");

        builder.Property(entry => entry.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.Action)
            .HasColumnName("action")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(entry => entry.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entry => entry.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.Property(entry => entry.OldValuesJson)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(entry => entry.NewValuesJson)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(entry => entry.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100);

        builder.Property(entry => entry.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        ConfigureIndexes(builder);
    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasIndex(entry => entry.OccurredAtUtc)
            .HasDatabaseName("ix_audit_entries_occurred_at_utc");

        builder.HasIndex(entry => entry.UserId)
            .HasDatabaseName("ix_audit_entries_user_id");

        builder.HasIndex(entry => new
        {
            entry.EntityName,
            entry.EntityId
        })
            .HasDatabaseName(
                "ix_audit_entries_entity_name_entity_id");

        builder.HasIndex(entry => entry.CorrelationId)
            .HasDatabaseName(
                "ix_audit_entries_correlation_id");
    }
}