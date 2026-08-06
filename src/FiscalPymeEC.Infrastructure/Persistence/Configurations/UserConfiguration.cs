using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiscalPymeEC.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id");

        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email");

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasColumnName("role")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        ConfigureAuditProperties(builder);
    }

    private static void ConfigureAuditProperties(
        EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(user => user.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(user => user.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
    }
}