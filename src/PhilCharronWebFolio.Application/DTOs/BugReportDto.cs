using PhilCharronWebFolio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.DTOs;

public sealed record BugReportDto(
    string Title,
    string Description,
    string WcagCriteria,
    BugSeverity Severity,
    string UrlAffected
);
