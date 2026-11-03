using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a health event logged for a child.
/// Part of the Child Daily Logs system.
/// </summary>
public sealed class HealthLog : BaseEntity<HealthLogId>
{
    public ChildId ChildId { get; private set; } = null!;
    public DateTime Time { get; private set; }
    public HealthEventType EventType { get; private set; }
    public string? Description { get; private set; }
    public Guid StaffId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Child? Child { get; private set; }

    private HealthLog() { }

    private HealthLog(
        HealthLogId id,
        ChildId childId,
        DateTime time,
        HealthEventType eventType,
        string? description,
        Guid staffId,
        string? notes)
        : base(id)
    {
        ChildId = childId;
        Time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
        EventType = eventType;
        Description = Trim(description);
        StaffId = staffId;
        Notes = Trim(notes);
    }

    // ─── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new HealthLog with domain validations.
    /// </summary>
    public static Result<HealthLog> Create(
        ChildId childId,
        DateTime time,
        HealthEventType eventType,
        string? description,
        Guid staffId,
        string? notes = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<HealthLog>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<HealthLog>.Failure("A valid Staff ID is required.");

        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result<HealthLog>.Failure("Health event time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result<HealthLog>.Failure("Health event time cannot be older than 60 days.");

        var log = new HealthLog(
            HealthLogId.New(),
            childId,
            utcTime,
            eventType,
            description,
            staffId,
            notes);

        return Result<HealthLog>.Success(log);
    }

    // ─── Behaviour ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates health log details.
    /// </summary>
    public Result Update(
        DateTime time,
        HealthEventType eventType,
        string? description,
        string? notes)
    {
        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Health event time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result.Failure("Health event time cannot be older than 60 days.");

        Time = utcTime;
        EventType = eventType;
        Description = Trim(description);
        Notes = Trim(notes);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
