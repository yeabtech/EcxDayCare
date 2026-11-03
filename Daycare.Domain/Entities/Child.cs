using Daycare.Domain.ValueObjects;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a child in the daycare system.
/// Aggregate root with all business logic for child management.
/// </summary>
public sealed class Child : BaseEntity<ChildId>
{
    /// <summary>
    /// The child's name.
    /// </summary>
    public PersonName Name { get; private set; }

    /// <summary>
    /// The child's date of birth.
    /// </summary>
    public DateOfBirth DateOfBirth { get; private set; }

    /// <summary>
    /// The child's gender.
    /// </summary>
    public Gender Gender { get; private set; }

    /// <summary>
    /// URL to the child's photo (nullable).
    /// </summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// The date the child was enrolled.
    /// </summary>
    public DateTime EnrollmentDate { get; private set; }

    /// <summary>
    /// Known allergies (nullable).
    /// </summary>
    public string? Allergies { get; private set; }

    /// <summary>
    /// Medical notes and conditions (nullable).
    /// </summary>
    public string? MedicalNotes { get; private set; }

    /// <summary>
    /// Current enrollment status.
    /// </summary>
    public ChildStatus Status { get; private set; }

    /// <summary>
    /// Foreign key to Classroom (nullable).
    /// </summary>
    public ClassroomId? ClassroomId { get; private set; }

    /// <summary>
    /// Navigation property to Classroom.
    /// </summary>
    public Classroom? Classroom { get; private set; }

    /// <summary>
    /// Navigation property to ParentChild join entities.
    /// </summary>
    public ICollection<ParentChild> ParentChildren { get; private set; } = new List<ParentChild>();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Child()
    {
        // EF Core only
    }

    /// <summary>
    /// Creates a new child entity.
    /// </summary>
    public Child(
        ChildId id,
        PersonName name,
        DateOfBirth dateOfBirth,
        Gender gender,
        DateTime enrollmentDate,
        string? photoUrl = null,
        string? allergies = null,
        string? medicalNotes = null)
        : base(id)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DateOfBirth = dateOfBirth ?? throw new ArgumentNullException(nameof(dateOfBirth));
        Gender = gender;
        EnrollmentDate = enrollmentDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(enrollmentDate, DateTimeKind.Utc)
            : enrollmentDate.ToUniversalTime();
        PhotoUrl = photoUrl;
        Allergies = allergies;
        MedicalNotes = medicalNotes;
        Status = ChildStatus.Enrolled;
    }

    /// <summary>
    /// Updates the child's basic information.
    /// </summary>
    public void UpdateInfo(
        PersonName name,
        DateOfBirth dateOfBirth,
        Gender gender,
        string? photoUrl = null,
        string? allergies = null,
        string? medicalNotes = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DateOfBirth = dateOfBirth ?? throw new ArgumentNullException(nameof(dateOfBirth));
        Gender = gender;
        PhotoUrl = photoUrl;
        Allergies = allergies;
        MedicalNotes = medicalNotes;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the child's enrollment date.
    /// </summary>
    /// <param name="enrollmentDate">The new enrollment date.</param>
    public void UpdateEnrollmentDate(DateTime enrollmentDate)
    {
        EnrollmentDate = enrollmentDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(enrollmentDate, DateTimeKind.Utc)
            : enrollmentDate.ToUniversalTime();
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the child's enrollment status.
    /// </summary>
    /// <param name="status">The new status (Enrolled, Inactive or Withdrawn).</param>
    public void UpdateStatus(ChildStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException($"'{status}' is not a valid child status", nameof(status));

        if (Status == status)
            return;

        Status = status;
        UpdateTimestamp();
    }

    /// <summary>
    /// Marks the child as inactive.
    /// </summary>
    public void MarkInactive()
    {
        if (Status == ChildStatus.Inactive)
            throw new InvalidOperationException("Child is already inactive");

        Status = ChildStatus.Inactive;
        UpdateTimestamp();
    }

    /// <summary>
    /// Marks the child as withdrawn.
    /// </summary>
    public void MarkWithdrawn()
    {
        if (Status == ChildStatus.Withdrawn)
            throw new InvalidOperationException("Child is already withdrawn");

        Status = ChildStatus.Withdrawn;
        UpdateTimestamp();
    }

    /// <summary>
    /// Re-enrolls an inactive or withdrawn child.
    /// </summary>
    public void Reenroll()
    {
        if (Status == ChildStatus.Enrolled)
            throw new InvalidOperationException("Child is already enrolled");

        Status = ChildStatus.Enrolled;
        UpdateTimestamp();
    }

    /// <summary>
    /// Assigns the child to a classroom.
    /// </summary>
    public void AssignToClassroom(ClassroomId classroomId)
    {
        ClassroomId = classroomId ?? throw new ArgumentNullException(nameof(classroomId));
        UpdateTimestamp();
    }

    /// <summary>
    /// Removes the child from their classroom.
    /// </summary>
    public void RemoveFromClassroom()
    {
        ClassroomId = null;
        Classroom = null;
        UpdateTimestamp();
    }
}
