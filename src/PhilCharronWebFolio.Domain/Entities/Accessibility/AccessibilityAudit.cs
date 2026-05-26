using PhilCharronWebFolio.Domain.Common;
using System.Collections.Generic;

namespace PhilCharronWebFolio.Domain.Entities.Accessibility;

public sealed class AccessibilityAudit : BaseAuditableEntity
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
    public ICollection<Finding> Findings { get; set; } = new List<Finding>();
}
