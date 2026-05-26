using Microsoft.EntityFrameworkCore;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Entities.Accessibility;
using PhilCharronWebFolio.Infrastructure.Persistence;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Repositories;

public sealed class AccessibilityAuditRepository(ApplicationDbContext db) : IAccessibilityAuditRepository
{
    public async Task<AccessibilityAudit?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.AccessibilityAudits.FindAsync([id], ct);

    public async Task AddAsync(AccessibilityAudit audit, CancellationToken ct) =>
        await db.AccessibilityAudits.AddAsync(audit, ct);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await db.SaveChangesAsync(ct);
}
