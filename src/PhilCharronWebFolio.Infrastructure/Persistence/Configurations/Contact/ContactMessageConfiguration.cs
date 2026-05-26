using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhilCharronWebFolio.Domain.Entities.Contact;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Configurations.Contact;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SenderEmail).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Subject).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Message).IsRequired();
    }
}
