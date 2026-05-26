using PhilCharronWebFolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Common.Interfaces;

public interface IBugReportRepository
{
    Task AddAsync(BugReport bugReport, CancellationToken ct);
    Task<IEnumerable<BugReport>> GetAllAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
