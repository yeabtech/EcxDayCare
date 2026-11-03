using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a nap or rest session logged for a child.
/// Supports both scheduled/completed naps and active/in-progress naps ("End Nap" action).
/// Part of the Child Daily Logs system.
/// </summary>
public sealed class NapLog : BaseEntity<NapLogId>
{
    public ChildId ChildId { get; private set; } = null!;
    public DailyReportId? DailyReportId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public NapQuality Quality { get; private set; }
    public Guid StaffId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Child? Child { get; private set; }
    public DailyReport? DailyReport { get; private set; }

    /// <summary>
    /// Duration of nap in minutes (null if nap is currently active).
    /// </summary>
    public int? DurationMinutes =>
        EndTime.HasValue ? (int)Math.Max(0, (EndTime.Value - StartTime).TotalMinutes) : null;

    private NapLog() { }

    private NapLog(
        NapLogId id,
        ChildId childId,
        DateTime startTime,
        DateTime? endTime,
        NapQuality quality,
        Guid staffId,
        string? notes)
        : base(id)
    {
        ChildId = childId;
        StartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        EndTime = endTime.HasValue ? DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc) : null;
        Quality = quality;
        StaffId = staffId;
        Notes = Trim(notes);
    }

    // ─── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new NapLog with domain validation.
    /// </summary>
    public static Result<NapLog> Create(
        ChildId childId,
        DateTime startTime,
        DateTime? endTime,
        NapQuality quality,
        Guid staffId,
        string? notes = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<NapLog>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<NapLog>.Failure("A valid Staff ID is required.");

        var utcStart = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);

        if (utcStart > DateTime.UtcNow.AddMinutes(15))
            return Result<NapLog>.Failure("Nap start time cannot be in the future.");

        if (utcStart < DateTime.UtcNow.AddDays(-60))
            return Result<NapLog>.Failure("Nap start time cannot be older than 60 days.");

        DateTime? utcEnd = null;
        if (endTime.HasValue)
        {
            utcEnd = DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc);
            if (utcEnd.Value < utcStart)
                return Result<NapLog>.Failure("Nap end time cannot be before start time.");

            if (utcEnd.Value > DateTime.UtcNow.AddMinutes(15))
                return Result<NapLog>.Failure("Nap end time cannot be in the future.");
        }

        var log = new NapLog(
            NapLogId.New(),
            childId,
            utcStart,
            utcEnd,
            quality,
            staffId,
            notes);

        return Result<NapLog>.Success(log);
    }

    // ─── Behaviour ────────────────────────────────────────────────────────────

    /// <summary>
    /// Ends an ongoing nap.
    /// </summary>
    public Result EndNap(DateTime endTime, NapQuality? quality = null, string? notes = null)
    {
        var utcEnd = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

        if (utcEnd < StartTime)
            return Result.Failure("Nap end time cannot be before start time.");

        if (utcEnd > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Nap end time cannot be in the future.");

        EndTime = utcEnd;
        if (quality.HasValue) Quality = quality.Value;
        if (notes is not null) Notes = Trim(notes);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Updates nap log details.
    /// </summary>
    public Result Update(
        DateTime startTime,
        DateTime? endTime,
        NapQuality quality,
        string? notes)
    {
        var utcStart = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);

        if (utcStart > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Nap start time cannot be in the future.");

        DateTime? utcEnd = null;
        if (endTime.HasValue)
        {
            utcEnd = DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc);
            if (utcEnd.Value < utcStart)
                return Result.Failure("Nap end time cannot be before start time.");
        }

        StartTime = utcStart;
        EndTime = utcEnd;
        Quality = quality;
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
