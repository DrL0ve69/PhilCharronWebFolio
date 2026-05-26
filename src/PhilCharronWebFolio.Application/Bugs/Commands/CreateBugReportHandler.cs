using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Application.Common.Messaging;
using PhilCharronWebFolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Bugs.Commands;

public sealed class CreateBugReportHandler(IBugReportRepository repository)
    : ICommandHandler<CreateBugReportCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateBugReportCommand command, CancellationToken ct)
    {
        var bug = BugReport.Create(
            command.Title,
            command.Description,
            command.WcagCriteria,
            command.Severity,
            command.UrlAffected
        );

        await repository.AddAsync(bug, ct);
        await repository.SaveChangesAsync(ct);

        return bug.Id;
    }
}
