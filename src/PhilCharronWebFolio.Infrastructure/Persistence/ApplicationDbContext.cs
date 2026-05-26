using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Entities;
using PhilCharronWebFolio.Domain.Entities.Accessibility;
using PhilCharronWebFolio.Domain.Entities.Contact;
using PhilCharronWebFolio.Infrastructure.Identity;
using DomainRoles = PhilCharronWebFolio.Domain.Constants.Roles; // ALIAS CRUCIAL ICI !

namespace PhilCharronWebFolio.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<BugReport> BugReports => Set<BugReport>();
    public DbSet<AccessibilityAudit> AccessibilityAudits => Set<AccessibilityAudit>();
    public DbSet<Finding> Findings => Set<Finding>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    //public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    //{
    //    return await base.SaveChangesAsync(cancellationToken);
    //}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
