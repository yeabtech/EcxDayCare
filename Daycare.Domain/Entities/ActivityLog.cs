using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a structured learning, play, or sensory activity logged for a child.
/// Part of the Child Daily Logs system.
/// </summary>
public sealed class ActivityLog : BaseEntity<ActivityLogId>
{
    public ChildId ChildId { get; private set; } = null!;
    public DailyReportId? DailyReportId { get; private set; }
    public DateTime Time { get; private set; }
    public int? DurationMinutes { get; private set; }
    public ActivityType ActivityType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ParticipationLevel Participation { get; private set; }
    public Guid StaffId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Child? Child { get; private set; }
    public DailyReport? DailyReport { get; private set; }

    private ActivityLog() { }

    private ActivityLog(
        ActivityLogId id,
        ChildId childId,
        DateTime time,
        int? durationMinutes,
        ActivityType activityType,
        string title,
        string? description,
        ParticipationLevel participation,
        Guid staffId,
        string? notes)
        : base(id)
    {
        ChildId = childId;
        Time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
        DurationMinutes = durationMinutes;
        ActivityType = activityType;
        Title = title;
        Description = Trim(description);
        Participation = participation;
        StaffId = staffId;
        Notes = Trim(notes);
    }

    // ─── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new ActivityLog with domain validations.
    /// </summary>
    public static Result<ActivityLog> Create(
        ChildId childId,
        DateTime time,
        int? durationMinutes,
        ActivityType activityType,
        string title,
        string? description,
        ParticipationLevel participation,
        Guid staffId,
        string? notes = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<ActivityLog>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<ActivityLog>.Failure("A valid Staff ID is required.");

        if (string.IsNullOrWhiteSpace(title))
            return Result<ActivityLog>.Failure("Activity title is required.");

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > 200)
            return Result<ActivityLog>.Failure("Activity title cannot exceed 200 characters.");

        if (durationMinutes.HasValue && durationMinutes.Value < 0)
            return Result<ActivityLog>.Failure("Duration minutes cannot be negative.");

        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result<ActivityLog>.Failure("Activity time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result<ActivityLog>.Failure("Activity time cannot be older than 60 days.");

        var log = new ActivityLog(
            ActivityLogId.New(),
            childId,
            utcTime,
            durationMinutes,
            activityType,
            trimmedTitle,
            description,
            participation,
            staffId,
            notes);

        return Result<ActivityLog>.Success(log);
    }

    // ─── Behaviour ────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates activity log details.
    /// </summary>
    public Result Update(
        DateTime time,
        int? durationMinutes,
        ActivityType activityType,
        string title,
        string? description,
        ParticipationLevel participation,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Activity title is required.");

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > 200)
            return Result.Failure("Activity title cannot exceed 200 characters.");

        if (durationMinutes.HasValue && durationMinutes.Value < 0)
            return Result.Failure("Duration minutes cannot be negative.");

        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Activity time cannot be in the future.");

        Time = utcTime;
        DurationMinutes = durationMinutes;
        ActivityType = activityType;
        Title = trimmedTitle;
        Description = Trim(description);
        Participation = participation;
        Notes = Trim(notes);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void LinkToDailyReport(DailyReportId? dailyReportId)
    {
        DailyReportId = dailyReportId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
