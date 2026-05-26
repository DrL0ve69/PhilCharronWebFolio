using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Repositories;

public sealed class BugReportRepository(ApplicationDbContext context) : IBugReportRepository
{
    public async Task AddAsync(BugReport bugReport, CancellationToken ct)
        => await context.BugReports.AddAsync(bugReport, ct);

    public async Task<IEnumerable<BugReport>> GetAllAsync(CancellationToken ct)
        => await context.BugReports.ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await context.SaveChangesAsync(ct);
}
