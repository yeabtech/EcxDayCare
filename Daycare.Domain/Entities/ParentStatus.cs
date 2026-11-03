namespace Daycare.Domain.Entities;

/// <summary>
/// Status of a parent/guardian in the system
/// </summary>
public enum ParentStatus
{
    /// <summary>
    /// Parent is active and can pick up/drop off children
    /// </summary>
    Active = 1,

    /// <summary>
    /// Parent is inactive and cannot pick up/drop off children
    /// </summary>
    Inactive = 2
}
