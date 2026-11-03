using System.Text.RegularExpressions;
using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Staff entity - represents a daycare staff member (teachers, aides, directors, admin).
/// Aggregate root encapsulating staff management business logic.
/// </summary>
public sealed class Staff : BaseEntity<StaffId>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$",
        RegexOptions.Compiled);

    public StaffId StaffId => Id;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public StaffRole Role { get; private set; }

    public ClassroomId? ClassroomId { get; private set; }

    public Classroom? Classroom { get; private set; }

    public string Phone { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateTime HireDate { get; private set; }

    public string? Certifications { get; private set; }

    public TimeOnly ShiftStart { get; private set; }

    public TimeOnly ShiftEnd { get; private set; }

    private Staff() { }

    public Staff(
        StaffId id,
        string firstName,
        string lastName,
        StaffRole role,
        string phone,
        string email,
        DateTime hireDate,
        TimeOnly shiftStart,
        TimeOnly shiftEnd,
        ClassroomId? classroomId = null,
        string? certifications = null)
        : base(id)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Role = role;
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        HireDate = hireDate.Kind == DateTimeKind.Utc ? hireDate : DateTime.SpecifyKind(hireDate, DateTimeKind.Utc);
        ShiftStart = shiftStart;
        ShiftEnd = shiftEnd;
        ClassroomId = classroomId;
        Certifications = certifications;
    }

    /// <summary>
    /// Factory method to create a new Staff entity with business validation.
    /// </summary>
    public static Result<Staff> Create(
        string firstName,
        string lastName,
        StaffRole role,
        string phone,
        string email,
        DateTime hireDate,
        TimeOnly shiftStart,
        TimeOnly shiftEnd,
        ClassroomId? classroomId = null,
        string? certifications = null)
    {
        var validationResult = ValidateFields(
            firstName,
            lastName,
            role,
            phone,
            email,
            hireDate,
            shiftStart,
            shiftEnd,
            certifications);

        if (validationResult.IsFailure)
            return Result<Staff>.Failure(validationResult.Error);

        var staff = new Staff(
            StaffId.Create(),
            firstName.Trim(),
            lastName.Trim(),
            role,
            phone.Trim(),
            email.Trim().ToLowerInvariant(),
            hireDate,
            shiftStart,
            shiftEnd,
            classroomId,
            string.IsNullOrWhiteSpace(certifications) ? null : certifications.Trim());

        return Result<Staff>.Success(staff);
    }

    /// <summary>
    /// Updates all staff member details.
    /// </summary>
    public Result Update(
        string firstName,
        string lastName,
        StaffRole role,
        string phone,
        string email,
        DateTime hireDate,
        TimeOnly shiftStart,
        TimeOnly shiftEnd,
        ClassroomId? classroomId,
        string? certifications)
    {
        var validationResult = ValidateFields(
            firstName,
            lastName,
            role,
            phone,
            email,
            hireDate,
            shiftStart,
            shiftEnd,
            certifications);

        if (validationResult.IsFailure)
            return validationResult;

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role;
        Phone = phone.Trim();
        Email = email.Trim().ToLowerInvariant();
        HireDate = hireDate.Kind == DateTimeKind.Utc ? hireDate : DateTime.SpecifyKind(hireDate, DateTimeKind.Utc);
        ShiftStart = shiftStart;
        ShiftEnd = shiftEnd;
        ClassroomId = classroomId;
        Certifications = string.IsNullOrWhiteSpace(certifications) ? null : certifications.Trim();

        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Assigns the staff member to a classroom.
    /// </summary>
    public Result AssignToClassroom(ClassroomId classroomId)
    {
        if (classroomId is null)
            return Result.Failure("Classroom ID is required for assignment");

        ClassroomId = classroomId;
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Removes the staff member from their current classroom.
    /// </summary>
    public Result RemoveFromClassroom()
    {
        ClassroomId = null;
        Classroom = null;
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Updates staff role.
    /// </summary>
    public Result UpdateRole(StaffRole role)
    {
        if (!Enum.IsDefined(typeof(StaffRole), role))
            return Result.Failure("Invalid staff role");

        Role = role;
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Updates staff shift timings.
    /// </summary>
    public Result UpdateShift(TimeOnly shiftStart, TimeOnly shiftEnd)
    {
        if (shiftEnd <= shiftStart)
            return Result.Failure("Shift end time must be after shift start time");

        ShiftStart = shiftStart;
        ShiftEnd = shiftEnd;
        UpdateTimestamp();
        return Result.Success();
    }

    private static Result ValidateFields(
        string firstName,
        string lastName,
        StaffRole role,
        string phone,
        string email,
        DateTime hireDate,
        TimeOnly shiftStart,
        TimeOnly shiftEnd,
        string? certifications)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure("First name is required");

        if (firstName.Trim().Length > 100)
            return Result.Failure("First name must not exceed 100 characters");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure("Last name is required");

        if (lastName.Trim().Length > 100)
            return Result.Failure("Last name must not exceed 100 characters");

        if (!Enum.IsDefined(typeof(StaffRole), role))
            return Result.Failure("Invalid staff role");

        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure("Phone number is required");

        var cleanPhone = phone.Trim();
        if (cleanPhone.Length < 7 || cleanPhone.Length > 20 || !PhoneRegex.IsMatch(cleanPhone))
            return Result.Failure("Invalid phone number format");

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure("Email is required");

        var cleanEmail = email.Trim();
        if (cleanEmail.Length > 256 || !EmailRegex.IsMatch(cleanEmail))
            return Result.Failure("Invalid email address format");

        if (hireDate == default)
            return Result.Failure("Hire date is required");

        if (hireDate < new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            return Result.Failure("Hire date cannot be earlier than 1950");

        if (hireDate > DateTime.UtcNow.AddYears(1))
            return Result.Failure("Hire date cannot be more than 1 year in the future");

        if (shiftEnd <= shiftStart)
            return Result.Failure("Shift end time must be after shift start time");

        if (certifications != null && certifications.Length > 500)
            return Result.Failure("Certifications must not exceed 500 characters");

        return Result.Success();
    }
}
