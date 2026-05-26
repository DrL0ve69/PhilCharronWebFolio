using PhilCharronWebFolio.Application.Accessibility.Commands;
using PhilCharronWebFolio.Domain.Entities.Accessibility;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Application.Common.Messaging;

namespace PhilCharronWebFolio.Application.Accessibility.Commands;

public sealed class CreateAccessibilityAuditHandler(IAccessibilityAuditRepository repository) : ICommandHandler<CreateAccessibilityAuditCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateAccessibilityAuditCommand command, CancellationToken ct)
    {
        var audit = new AccessibilityAudit
        {
            ProjectName = command.ProjectName,
            ProjectUrl = command.ProjectUrl
        };

        await repository.AddAsync(audit, ct);
        await repository.SaveChangesAsync(ct);

        return audit.Id;
    }
}
