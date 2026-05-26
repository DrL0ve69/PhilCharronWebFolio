using PhilCharronWebFolio.Application.Common.Messaging;
using PhilCharronWebFolio.Application.Common.Results;

namespace PhilCharronWebFolio.Application.Accessibility.Commands;

public sealed record CreateAccessibilityAuditCommand(
    string ProjectName,
    string ProjectUrl
) : ICommand<Guid>;
