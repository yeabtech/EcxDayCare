namespace Daycare.Domain.Entities;

/// <summary>
/// Metric indicating how much of a meal or bottle was consumed.
/// </summary>
public enum PortionConsumed
{
    All = 1,
    Most = 2,
    Half = 3,
    Some = 4,
    None = 5
}
