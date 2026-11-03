namespace Daycare.Domain.Entities;

/// <summary>
/// Status of an enrollment record.
/// </summary>
public enum EnrollmentStatus
{
    Pending = 1,
    Active = 2,
    Ended = 3,
    Cancelled = 4
}
