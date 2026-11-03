using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a daily attendance record for an enrolled child in the daycare.
/// Aggregate root enforcing attendance invariants, time validations, and state transitions.
/// </summary>
public sealed class Attendance : BaseEntity<AttendanceId>
{
    public AttendanceId AttendanceId => Id;

    /// <summary>
    /// Identifier of the enrolled child.
    /// </summary>
    public ChildId ChildId { get; private set; }

    /// <summary>
    /// Navigation property to the enrolled child.
    /// </summary>
    public Child? Child { get; private set; }

    /// <summary>
    /// Optional classroom identifier at the time of attendance.
    /// </summary>
    public ClassroomId? ClassroomId { get; private set; }

    /// <summary>
    /// Navigation property to the classroom.
    /// </summary>
    public Classroom? Classroom { get; private set; }

    /// <summary>
    /// Calendar date of the attendance record (stored in UTC at 00:00:00).
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Exact UTC timestamp when the child checked in. Null if marked absent.
    /// </summary>
    public DateTime? CheckInTime { get; private set; }

    /// <summary>
    /// Exact UTC timestamp when the child checked out.
    /// </summary>
    public DateTime? CheckOutTime { get; private set; }

    /// <summary>
    /// Status for this attendance record (Present, Absent, Late, EarlyPickup).
    /// </summary>
    public AttendanceStatus Status { get; private set; }

    /// <summary>
    /// Name or relationship of the person who dropped off the child.
    /// </summary>
    public string? DroppedOffBy { get; private set; }

    /// <summary>
    /// Name or relationship of the person who picked up the child.
    /// </summary>
    public string? PickedUpBy { get; private set; }

    /// <summary>
    /// Optional notes or observations for this attendance record.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Identifier of the staff member who logged the check-in or absence.
    /// </summary>
    public Guid? StaffId { get; private set; }

    /// <summary>
    /// Identifier of the staff member who logged the check-out.
    /// </summary>
    public Guid? CheckOutStaffId { get; private set; }

    /// <summary>
    /// Standard morning cutoff time for determining tardiness (9:30 AM).
    /// </summary>
    public static readonly TimeSpan StandardMorningCutoff = new(9, 30, 0);

    /// <summary>
    /// Standard afternoon cutoff time for determining early pickup (3:00 PM).
    /// </summary>
    public static readonly TimeSpan StandardEarlyPickupCutoff = new(15, 0, 0);

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Attendance()
    {
        ChildId = null!;
    }

    /// <summary>
    /// Initializes an Attendance aggregate with required parameters.
    /// </summary>
    public Attendance(
        AttendanceId id,
        ChildId childId,
        DateTime date,
        AttendanceStatus status,
        DateTime? checkInTime = null,
        DateTime? checkOutTime = null,
        ClassroomId? classroomId = null,
        string? droppedOffBy = null,
        string? pickedUpBy = null,
        string? notes = null,
        Guid? staffId = null,
        Guid? checkOutStaffId = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ChildId = childId ?? throw new ArgumentNullException(nameof(childId));
        Date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        Status = status;
        CheckInTime = checkInTime.HasValue ? DateTime.SpecifyKind(checkInTime.Value, DateTimeKind.Utc) : null;
        CheckOutTime = checkOutTime.HasValue ? DateTime.SpecifyKind(checkOutTime.Value, DateTimeKind.Utc) : null;
        ClassroomId = classroomId;
        DroppedOffBy = droppedOffBy?.Trim();
        PickedUpBy = pickedUpBy?.Trim();
        Notes = notes?.Trim();
        StaffId = staffId;
        CheckOutStaffId = checkOutStaffId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to record a child's check-in.
    /// Enforces temporal invariants and determines appropriate initial status (Present or Late).
    /// </summary>
    public static Result<Attendance> CheckIn(
        ChildId childId,
        DateTime checkInTime,
        ClassroomId? classroomId = null,
        string? droppedOffBy = null,
        string? notes = null,
        AttendanceStatus? explicitStatus = null,
        Guid? staffId = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
        {
            return Result<Attendance>.Failure("Valid Child ID is required for check-in.");
        }

        var utcCheckIn = DateTime.SpecifyKind(checkInTime, DateTimeKind.Utc);

        // Allow at most 10 minutes clock tolerance for client skew
        if (utcCheckIn > DateTime.UtcNow.AddMinutes(10))
        {
            return Result<Attendance>.Failure("Check-in time cannot be in the future.");
        }

        // Determine attendance status: explicit or evaluated by morning cutoff
        AttendanceStatus status;
        if (explicitStatus.HasValue && (explicitStatus.Value == AttendanceStatus.Present || explicitStatus.Value == AttendanceStatus.Late))
        {
            status = explicitStatus.Value;
        }
        else if (utcCheckIn.TimeOfDay > StandardMorningCutoff)
        {
            status = AttendanceStatus.Late;
        }
        else
        {
            status = AttendanceStatus.Present;
        }

        var attendance = new Attendance(
            AttendanceId.New(),
            childId,
            utcCheckIn.Date,
            status,
            checkInTime: utcCheckIn,
            checkOutTime: null,
            classroomId: classroomId,
            droppedOffBy: droppedOffBy,
            pickedUpBy: null,
            notes: notes,
            staffId: staffId);

        return Result<Attendance>.Success(attendance);
    }

    /// <summary>
    /// Factory method to record a child as Absent for a given date.
    /// </summary>
    public static Result<Attendance> MarkAbsent(
        ChildId childId,
        DateTime date,
        ClassroomId? classroomId = null,
        string? notes = null,
        Guid? staffId = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
        {
            return Result<Attendance>.Failure("Valid Child ID is required to mark absence.");
        }

        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        var attendance = new Attendance(
            AttendanceId.New(),
            childId,
            utcDate,
            AttendanceStatus.Absent,
            checkInTime: null,
            checkOutTime: null,
            classroomId: classroomId,
            droppedOffBy: null,
            pickedUpBy: null,
            notes: notes,
            staffId: staffId);

        return Result<Attendance>.Success(attendance);
    }

    /// <summary>
    /// Records child check-out with time validation and early pickup transition rules.
    /// </summary>
    public Result CheckOut(
        DateTime checkOutTime,
        string? pickedUpBy = null,
        string? notes = null,
        bool? isEarlyPickup = null,
        Guid? staffId = null)
    {
        if (!CheckInTime.HasValue || Status == AttendanceStatus.Absent)
        {
            return Result.Failure("Cannot check out a child who has not checked in or was marked absent.");
        }

        if (CheckOutTime.HasValue)
        {
            return Result.Failure("Child is already checked out for today.");
        }

        var utcCheckOut = DateTime.SpecifyKind(checkOutTime, DateTimeKind.Utc);

        if (utcCheckOut > DateTime.UtcNow.AddMinutes(10))
        {
            return Result.Failure("Check-out time cannot be in the future.");
        }

        if (utcCheckOut < CheckInTime.Value.AddMinutes(-1))
        {
            return Result.Failure("Check-out time cannot be earlier than check-in time.");
        }

        CheckOutTime = utcCheckOut;
        if (!string.IsNullOrWhiteSpace(pickedUpBy))
        {
            PickedUpBy = pickedUpBy.Trim();
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? notes.Trim()
                : $"{Notes} | Checkout: {notes.Trim()}";
        }

        CheckOutStaffId = staffId;
        UpdatedAt = DateTime.UtcNow;

        // Determine if early pickup applies:
        // Either explicitly requested as early pickup, or check out occurs prior to afternoon cutoff
        bool qualifiesForEarlyPickup = isEarlyPickup == true ||
            (isEarlyPickup == null && utcCheckOut.TimeOfDay < StandardEarlyPickupCutoff);

        if (qualifiesForEarlyPickup)
        {
            Status = AttendanceStatus.EarlyPickup;
        }

        return Result.Success();
    }

    /// <summary>
    /// Overrides or updates the status with an explanatory reason.
    /// </summary>
    public Result UpdateStatus(AttendanceStatus newStatus, string? reason = null)
    {
        Status = newStatus;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? $"Status updated to {newStatus}: {reason.Trim()}"
                : $"{Notes} | Status updated to {newStatus}: {reason.Trim()}";
        }
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Updates the attendance notes.
    /// </summary>
    public Result UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
