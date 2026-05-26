using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Domain.Common;

public abstract class BaseEntity
{
    // On retire le "= Guid.NewGuid()". EF Core sait comment générer un Guid en base de données.
    public Guid Id { get; set; }

    protected BaseEntity() { }
}
