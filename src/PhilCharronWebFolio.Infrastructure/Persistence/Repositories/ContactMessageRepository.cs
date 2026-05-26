using Microsoft.EntityFrameworkCore;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Entities.Contact;
using System.Threading;
using System.Threading.Tasks;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Repositories;

public sealed class ContactMessageRepository(ApplicationDbContext context) : IContactMessageRepository
{
    public async Task AddAsync(ContactMessage message, CancellationToken ct)
        => await context.ContactMessages.AddAsync(message, ct);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await context.SaveChangesAsync(ct);
}
