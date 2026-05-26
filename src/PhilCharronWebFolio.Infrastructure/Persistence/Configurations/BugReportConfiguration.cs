using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhilCharronWebFolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Configurations;

public sealed class BugReportConfiguration : IEntityTypeConfiguration<BugReport>
{
    public void Configure(EntityTypeBuilder<BugReport> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Description)
            .IsRequired();

        // Index pour les performances
        builder.HasIndex(b => b.CreatedAt);
    }
}
