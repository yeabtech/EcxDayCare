namespace Daycare.Domain.Entities;

/// <summary>
/// Represents the enrollment status of a child.
/// </summary>
public enum ChildStatus
{
    /// <summary>
    /// Child is actively enrolled.
    /// </summary>
    Enrolled = 1,

    /// <summary>
    /// Child is inactive (paused enrollment).
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Child has withdrawn from the program.
    /// </summary>
    Withdrawn = 3
}
