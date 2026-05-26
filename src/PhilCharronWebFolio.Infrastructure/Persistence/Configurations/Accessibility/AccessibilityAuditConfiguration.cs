using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhilCharronWebFolio.Domain.Entities.Accessibility;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Configurations.Accessibility;

public sealed class AccessibilityAuditConfiguration : IEntityTypeConfiguration<AccessibilityAudit>
{
    public void Configure(EntityTypeBuilder<AccessibilityAudit> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ProjectName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.ProjectUrl).IsRequired();
        builder.HasMany(a => a.Findings).WithOne().HasForeignKey(f => f.AccessibilityAuditId);
    }
}
