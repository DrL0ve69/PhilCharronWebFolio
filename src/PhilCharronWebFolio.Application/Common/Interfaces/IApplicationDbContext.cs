using Microsoft.EntityFrameworkCore;
using PhilCharronWebFolio.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PhilCharronWebFolio.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<BugReport> BugReports { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
