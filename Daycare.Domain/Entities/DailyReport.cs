using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents a daily activity report for a child, authored by staff.
/// Captures mood, meals, nap, activities, diapers, milestones, and concerns.
/// Parents may only view reports belonging to their own children.
/// </summary>
public sealed class DailyReport : BaseEntity<DailyReportId>
{
    public DailyReportId DailyReportId => Id;

    /// <summary>Child the report belongs to.</summary>
    public ChildId ChildId { get; private set; }

    /// <summary>Navigation property to the child.</summary>
    public Child? Child { get; private set; }

    /// <summary>Calendar date of the report (UTC midnight).</summary>
    public DateTime Date { get; private set; }

    /// <summary>Child's mood observed during the day (e.g., Happy, Fussy, Tired).</summary>
    public string? Mood { get; private set; }

    /// <summary>Free-text summary of meals consumed.</summary>
    public string? MealsSummary { get; private set; }

    /// <summary>Free-text summary of nap time and quality.</summary>
    public string? NapSummary { get; private set; }

    /// <summary>Free-text summary of activities participated in.</summary>
    public string? ActivitiesSummary { get; private set; }

    /// <summary>Free-text diaper change summary (count, consistency, etc.).</summary>
    public string? DiaperSummary { get; private set; }

    /// <summary>Notable developmental milestones observed.</summary>
    public string? Milestones { get; private set; }

    /// <summary>Any concerns or items needing parent follow-up.</summary>
    public string? Concerns { get; private set; }

    /// <summary>Staff member who authored this report.</summary>
    public Guid StaffId { get; private set; }

    /// <summary>Whether the report has been sent to the parent.</summary>
    public bool SentToParent { get; private set; }

    /// <summary>Timestamp when the report was sent to the parent (UTC).</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>Private constructor for EF Core.</summary>
    private DailyReport()
    {
        ChildId = null!;
    }

    /// <summary>Full constructor used by factory methods.</summary>
    private DailyReport(
        DailyReportId id,
        ChildId childId,
        DateTime date,
        Guid staffId,
        string? mood,
        string? mealsSummary,
        string? napSummary,
        string? activitiesSummary,
        string? diaperSummary,
        string? milestones,
        string? concerns)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ChildId = childId ?? throw new ArgumentNullException(nameof(childId));
        Date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        StaffId = staffId;
        Mood = Trim(mood);
        MealsSummary = Trim(mealsSummary);
        NapSummary = Trim(napSummary);
        ActivitiesSummary = Trim(activitiesSummary);
        DiaperSummary = Trim(diaperSummary);
        Milestones = Trim(milestones);
        Concerns = Trim(concerns);
        SentToParent = false;
        SentAt = null;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ─── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new daily report for a child.
    /// Business rules:
    /// – childId must be valid.
    /// – staffId must be valid.
    /// – date must not be in the future (beyond 10-minute clock tolerance).
    /// – date must not be more than 60 days in the past.
    /// </summary>
    public static Result<DailyReport> Create(
        ChildId childId,
        DateTime date,
        Guid staffId,
        string? mood = null,
        string? mealsSummary = null,
        string? napSummary = null,
        string? activitiesSummary = null,
        string? diaperSummary = null,
        string? milestones = null,
        string? concerns = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<DailyReport>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<DailyReport>.Failure("A valid Staff ID is required.");

        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        if (utcDate > DateTime.UtcNow.Date.AddDays(1))
            return Result<DailyReport>.Failure("Report date cannot be in the future.");

        if (utcDate < DateTime.UtcNow.Date.AddDays(-60))
            return Result<DailyReport>.Failure("Report date cannot be more than 60 days in the past.");

        var report = new DailyReport(
            DailyReportId.New(),
            childId,
            date,
            staffId,
            mood,
            mealsSummary,
            napSummary,
            activitiesSummary,
            diaperSummary,
            milestones,
            concerns);

        return Result<DailyReport>.Success(report);
    }

    // ─── Behaviour ────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the report content. Only allowed before the report is sent to the parent.
    /// </summary>
    public Result Update(
        string? mood,
        string? mealsSummary,
        string? napSummary,
        string? activitiesSummary,
        string? diaperSummary,
        string? milestones,
        string? concerns)
    {
        Mood = Trim(mood);
        MealsSummary = Trim(mealsSummary);
        NapSummary = Trim(napSummary);
        ActivitiesSummary = Trim(activitiesSummary);
        DiaperSummary = Trim(diaperSummary);
        Milestones = Trim(milestones);
        Concerns = Trim(concerns);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Marks the report as sent to the parent.
    /// Once sent, the report can still be updated by staff (corrections), but the sent flag stays.
    /// </summary>
    public Result MarkSentToParent()
    {
        if (SentToParent)
            return Result.Failure("Report has already been sent to the parent.");

        SentToParent = true;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
