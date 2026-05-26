using PhilCharronWebFolio.Domain.Entities.Accessibility;

namespace PhilCharronWebFolio.Application.Common.Interfaces;

public interface IAccessibilityAuditRepository
{
    Task<AccessibilityAudit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(AccessibilityAudit audit, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
