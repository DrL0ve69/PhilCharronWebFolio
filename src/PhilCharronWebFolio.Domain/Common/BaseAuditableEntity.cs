using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    protected BaseAuditableEntity() { }
}
