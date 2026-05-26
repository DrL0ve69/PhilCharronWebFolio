using PhilCharronWebFolio.Application.Common.Messaging;
using PhilCharronWebFolio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Bugs.Commands;

public sealed record CreateBugReportCommand(
    string Title,
    string Description,
    string WcagCriteria,
    BugSeverity Severity,
    string UrlAffected
) : ICommand<Guid>; // Le Dispatcher retournera un Guid (l'ID du nouveau bug)
