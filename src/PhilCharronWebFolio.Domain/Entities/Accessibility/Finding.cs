using PhilCharronWebFolio.Domain.Common;

namespace PhilCharronWebFolio.Domain.Entities.Accessibility;

public sealed class Finding : BaseAuditableEntity
{
    public Guid AccessibilityAuditId { get; set; }
    public string WcagCriteria { get; set; } = string.Empty; // e.g., "1.1.1"
    public string Description { get; set; } = string.Empty;
    public string SuggestedFix { get; set; } = string.Empty;
    public Severity Level { get; set; }
    public bool IsFixed { get; set; }

    public enum Severity
    {
        Minor,
        Moderate,
        Serious,
        Critical
    }
}
