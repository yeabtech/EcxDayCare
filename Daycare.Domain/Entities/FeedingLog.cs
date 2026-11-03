using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents an individual feeding, bottle, or meal event logged for a child.
/// Part of the Child Daily Logs system.
/// </summary>
public sealed class FeedingLog : BaseEntity<FeedingLogId>
{
    public ChildId ChildId { get; private set; } = null!;
    public DailyReportId? DailyReportId { get; private set; }
    public DateTime Time { get; private set; }
    public FeedingType FeedingType { get; private set; }
    public string? FoodItems { get; private set; }
    public string? Amount { get; private set; }
    public PortionConsumed PortionConsumed { get; private set; }
    public Guid StaffId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Child? Child { get; private set; }
    public DailyReport? DailyReport { get; private set; }

    private FeedingLog() { }

    private FeedingLog(
        FeedingLogId id,
        ChildId childId,
        DateTime time,
        FeedingType feedingType,
        string? foodItems,
        string? amount,
        PortionConsumed portionConsumed,
        Guid staffId,
        string? notes)
        : base(id)
    {
        ChildId = childId;
        Time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
        FeedingType = feedingType;
        FoodItems = Trim(foodItems);
        Amount = Trim(amount);
        PortionConsumed = portionConsumed;
        StaffId = staffId;
        Notes = Trim(notes);
    }

    // ─── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new FeedingLog with domain validations.
    /// </summary>
    public static Result<FeedingLog> Create(
        ChildId childId,
        DateTime time,
        FeedingType feedingType,
        string? foodItems,
        string? amount,
        PortionConsumed portionConsumed,
        Guid staffId,
        string? notes = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<FeedingLog>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<FeedingLog>.Failure("A valid Staff ID is required.");

        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result<FeedingLog>.Failure("Feeding time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result<FeedingLog>.Failure("Feeding time cannot be older than 60 days.");

        var log = new FeedingLog(
            FeedingLogId.New(),
            childId,
            utcTime,
            feedingType,
            foodItems,
            amount,
            portionConsumed,
            staffId,
            notes);

        return Result<FeedingLog>.Success(log);
    }

    // ─── Behaviour ────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates feeding log details.
    /// </summary>
    public Result Update(
        DateTime time,
        FeedingType feedingType,
        string? foodItems,
        string? amount,
        PortionConsumed portionConsumed,
        string? notes)
    {
        var utcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Feeding time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result.Failure("Feeding time cannot be older than 60 days.");

        Time = utcTime;
        FeedingType = feedingType;
        FoodItems = Trim(foodItems);
        Amount = Trim(amount);
        PortionConsumed = portionConsumed;
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
