using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents an official enrollment of a child into the daycare program.
/// Aggregate root enforcing enrollment domain invariants and state transitions.
/// </summary>
public sealed class Enrollment : BaseEntity<EnrollmentId>
{
    public EnrollmentId EnrollmentId => Id;

    /// <summary>
    /// Identifier of the enrolled child.
    /// </summary>
    public ChildId ChildId { get; private set; }

    /// <summary>
    /// Navigation property to the enrolled child.
    /// </summary>
    public Child? Child { get; private set; }

    /// <summary>
    /// Optional classroom assigned for this enrollment.
    /// </summary>
    public ClassroomId? ClassroomId { get; private set; }

    /// <summary>
    /// Navigation property to the assigned classroom.
    /// </summary>
    public Classroom? Classroom { get; private set; }

    /// <summary>
    /// Type of daycare program (e.g. FullTime, PartTime, Preschool).
    /// </summary>
    public ProgramType ProgramType { get; private set; }

    /// <summary>
    /// Scheduled days of attendance (e.g., "Monday, Wednesday, Friday").
    /// </summary>
    public string DaysOfWeek { get; private set; } = string.Empty;

    /// <summary>
    /// Date the enrollment begins (UTC).
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Optional date the enrollment ends or is scheduled to end (UTC).
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Flag indicating whether the parent signed the daycare contract.
    /// </summary>
    public bool ContractSigned { get; private set; }

    /// <summary>
    /// Flag indicating whether up-to-date immunization records are on file.
    /// </summary>
    public bool ImmunizationRecord { get; private set; }

    /// <summary>
    /// Flag indicating whether an official birth certificate copy has been verified.
    /// </summary>
    public bool BirthCertificate { get; private set; }

    /// <summary>
    /// Flag indicating whether completed medical and emergency forms are provided.
    /// </summary>
    public bool MedicalForm { get; private set; }

    /// <summary>
    /// Current status of the enrollment.
    /// </summary>
    public EnrollmentStatus Status { get; private set; } = EnrollmentStatus.Active;

    /// <summary>
    /// Optional note or reason explaining why an enrollment ended or was modified.
    /// </summary>
    public string? EndReason { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Enrollment()
    {
        ChildId = null!;
    }

    /// <summary>
    /// Initializes an Enrollment aggregate.
    /// </summary>
    public Enrollment(
        EnrollmentId id,
        ChildId childId,
        ProgramType programType,
        string daysOfWeek,
        DateTime startDate,
        DateTime? endDate = null,
        bool contractSigned = false,
        bool immunizationRecord = false,
        bool birthCertificate = false,
        bool medicalForm = false,
        ClassroomId? classroomId = null,
        EnrollmentStatus status = EnrollmentStatus.Active,
        string? endReason = null)
        : base(id)
    {
        ChildId = childId ?? throw new ArgumentNullException(nameof(childId));
        ProgramType = programType;
        DaysOfWeek = daysOfWeek ?? throw new ArgumentNullException(nameof(daysOfWeek));
        StartDate = startDate.Kind == DateTimeKind.Utc ? startDate : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        EndDate = endDate.HasValue
            ? (endDate.Value.Kind == DateTimeKind.Utc ? endDate.Value : DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc))
            : null;
        ContractSigned = contractSigned;
        ImmunizationRecord = immunizationRecord;
        BirthCertificate = birthCertificate;
        MedicalForm = medicalForm;
        ClassroomId = classroomId;
        Status = status;
        EndReason = endReason;
    }

    /// <summary>
    /// Factory method to create an Enrollment with domain validation.
    /// </summary>
    public static Result<Enrollment> Create(
        ChildId childId,
        ProgramType programType,
        string daysOfWeek,
        DateTime startDate,
        DateTime? endDate = null,
        bool contractSigned = false,
        bool immunizationRecord = false,
        bool birthCertificate = false,
        bool medicalForm = false,
        ClassroomId? classroomId = null)
    {
        var validation = ValidateFields(childId, daysOfWeek, startDate, endDate);
        if (validation.IsFailure)
        {
            return Result<Enrollment>.Failure(validation.Error!);
        }

        var utcStart = startDate.Kind == DateTimeKind.Utc ? startDate : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var utcEnd = endDate.HasValue
            ? (endDate.Value.Kind == DateTimeKind.Utc ? endDate.Value : DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc))
            : (DateTime?)null;

        var initialStatus = utcStart.Date > DateTime.UtcNow.Date
            ? EnrollmentStatus.Pending
            : EnrollmentStatus.Active;

        var enrollment = new Enrollment(
            EnrollmentId.New(),
            childId,
            programType,
            daysOfWeek.Trim(),
            utcStart,
            utcEnd,
            contractSigned,
            immunizationRecord,
            birthCertificate,
            medicalForm,
            classroomId,
            initialStatus);

        return Result<Enrollment>.Success(enrollment);
    }

    /// <summary>
    /// Updates enrollment configuration and schedule.
    /// </summary>
    public Result Update(
        ProgramType programType,
        string daysOfWeek,
        DateTime startDate,
        DateTime? endDate,
        bool contractSigned,
        bool immunizationRecord,
        bool birthCertificate,
        bool medicalForm,
        ClassroomId? classroomId,
        EnrollmentStatus? status = null,
        string? endReason = null)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return Result.Failure("Invalid enrollment status");
        }

        var newStatus = status ?? Status;
        if (newStatus == EnrollmentStatus.Ended && !endDate.HasValue)
        {
            return Result.Failure("End date is required for an ended enrollment");
        }

        var validation = ValidateFields(ChildId, daysOfWeek, startDate, endDate);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Error!);
        }

        ProgramType = programType;
        DaysOfWeek = daysOfWeek.Trim();
        StartDate = startDate.Kind == DateTimeKind.Utc ? startDate : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        EndDate = endDate.HasValue
            ? (endDate.Value.Kind == DateTimeKind.Utc ? endDate.Value : DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc))
            : null;
        ContractSigned = contractSigned;
        ImmunizationRecord = immunizationRecord;
        BirthCertificate = birthCertificate;
        MedicalForm = medicalForm;
        ClassroomId = classroomId;

        Status = status ?? (Status == EnrollmentStatus.Pending && StartDate.Date <= DateTime.UtcNow.Date
            ? EnrollmentStatus.Active : Status);
        EndReason = Status is EnrollmentStatus.Ended or EnrollmentStatus.Cancelled
            ? (string.IsNullOrWhiteSpace(endReason) ? EndReason ?? (Status == EnrollmentStatus.Ended ? "Enrollment completed" : "Enrollment cancelled") : endReason.Trim())
            : null;
        if (Status == EnrollmentStatus.Cancelled && !EndDate.HasValue)
        {
            EndDate = DateTime.UtcNow;
        }

        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Ends this enrollment on a specified date with an optional reason.
    /// </summary>
    public Result EndEnrollment(DateTime endDate, string? reason = null)
    {
        if (Status == EnrollmentStatus.Ended)
        {
            return Result.Failure("Enrollment has already ended");
        }

        if (Status == EnrollmentStatus.Cancelled)
        {
            return Result.Failure("Cannot end a cancelled enrollment");
        }

        var utcEnd = endDate.Kind == DateTimeKind.Utc ? endDate : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        if (utcEnd.Date < StartDate.Date)
        {
            return Result.Failure("End date cannot be earlier than start date");
        }

        EndDate = utcEnd;
        Status = EnrollmentStatus.Ended;
        EndReason = string.IsNullOrWhiteSpace(reason) ? "Enrollment completed" : reason.Trim();
        UpdateTimestamp();

        return Result.Success();
    }

    /// <summary>
    /// Cancels this enrollment before or during its lifecycle.
    /// </summary>
    public Result Cancel(string? reason = null)
    {
        if (Status == EnrollmentStatus.Ended || Status == EnrollmentStatus.Cancelled)
        {
            return Result.Failure($"Enrollment is already {Status}");
        }

        Status = EnrollmentStatus.Cancelled;
        EndReason = string.IsNullOrWhiteSpace(reason) ? "Enrollment cancelled" : reason.Trim();
        EndDate = DateTime.UtcNow;
        UpdateTimestamp();

        return Result.Success();
    }

    /// <summary>
    /// Assigns or transfers the enrollment to a classroom.
    /// </summary>
    public Result AssignClassroom(ClassroomId? classroomId)
    {
        ClassroomId = classroomId;
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Validates core invariant rules for enrollment parameters.
    /// </summary>
    private static Result ValidateFields(
        ChildId childId,
        string daysOfWeek,
        DateTime startDate,
        DateTime? endDate)
    {
        if (childId is null || childId.Value == Guid.Empty)
        {
            return Result.Failure("ChildId is required");
        }

        if (string.IsNullOrWhiteSpace(daysOfWeek))
        {
            return Result.Failure("Days of week must be specified");
        }

        if (startDate == default)
        {
            return Result.Failure("Start date must be a valid date");
        }

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            return Result.Failure("End date cannot be earlier than start date");
        }

        return Result.Success();
    }
}
