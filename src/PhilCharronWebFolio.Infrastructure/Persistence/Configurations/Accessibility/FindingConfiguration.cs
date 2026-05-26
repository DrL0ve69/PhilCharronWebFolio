using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhilCharronWebFolio.Domain.Entities.Accessibility;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Configurations.Accessibility;

public sealed class FindingConfiguration : IEntityTypeConfiguration<Finding>
{
    public void Configure(EntityTypeBuilder<Finding> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.WcagCriteria).IsRequired().HasMaxLength(50);
        builder.Property(f => f.Description).IsRequired().HasMaxLength(1000);
        builder.Property(f => f.SuggestedFix).HasMaxLength(1000);
    }
}
