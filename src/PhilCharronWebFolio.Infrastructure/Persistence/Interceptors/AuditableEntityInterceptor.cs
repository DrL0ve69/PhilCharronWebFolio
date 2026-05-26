using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Persistence.Interceptors;

public sealed class AuditableEntityInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
        //var context = eventData.Context;
        //if (context is not null)
        //{
        //    foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        //    {
        //        if (entry.State is EntityState.Added or EntityState.Modified)
        //        {
        //            var userId = currentUserService.UserId ?? "System";
        //            if (entry.State == EntityState.Added) entry.Entity.SetCreatedBy(userId);
        //            else entry.Entity.UpdateAudit(userId);
        //        }
        //    }
        //}
        //return base.SavingChangesAsync(eventData, result, ct);
    }
    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUserService.UserId.ToString();
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserService.UserId.ToString();
            }
        }
    }
}
