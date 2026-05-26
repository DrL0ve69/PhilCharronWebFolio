using PhilCharronWebFolio.Domain.Common;
using PhilCharronWebFolio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Domain.Entities;

public sealed class BugReport : BaseAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string WcagCriteria { get; private set; } = string.Empty;
    public BugSeverity Severity { get; private set; }
    public BugStatus Status { get; private set; }
    public string UrlAffected { get; private set; } = string.Empty;

    private BugReport() { }

    public static BugReport Create(string title, string description, string wcagCriteria, BugSeverity severity, string urlAffected)
    {
        return new BugReport
        {
            Title = title,
            Description = description,
            WcagCriteria = wcagCriteria,
            Severity = severity,
            Status = BugStatus.Open,
            UrlAffected = urlAffected
        };
    }
}
