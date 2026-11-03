namespace Daycare.Domain.Entities;

/// <summary>
/// Status of child attendance for a given day.
/// </summary>
public enum AttendanceStatus
{
    /// <summary>
    /// Child attended and arrived on time.
    /// </summary>
    Present = 1,

    /// <summary>
    /// Child did not attend daycare on this date.
    /// </summary>
    Absent = 2,

    /// <summary>
    /// Child arrived late past the regular morning cutoff time.
    /// </summary>
    Late = 3,

    /// <summary>
    /// Child was picked up early before standard dismissal time.
    /// </summary>
    EarlyPickup = 4
}
