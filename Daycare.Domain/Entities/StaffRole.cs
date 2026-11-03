namespace Daycare.Domain.Entities;

/// <summary>
/// Roles for daycare staff members.
/// </summary>
public enum StaffRole
{
    /// <summary>
    /// Lead Teacher with classroom responsibility
    /// </summary>
    LeadTeacher = 1,

    /// <summary>
    /// Teacher
    /// </summary>
    Teacher = 2,

    /// <summary>
    /// Assistant Teacher
    /// </summary>
    Assistant = 3,

    /// <summary>
    /// Classroom Aide
    /// </summary>
    Aide = 4,

    /// <summary>
    /// Daycare Director
    /// </summary>
    Director = 5,

    /// <summary>
    /// Administrative Staff
    /// </summary>
    Admin = 6
}
