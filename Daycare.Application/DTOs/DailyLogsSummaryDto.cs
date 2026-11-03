namespace Daycare.Application.DTOs;

/// <summary>
/// Combined summary of all daily logs (Feeding, Nap, Activity) for a child on a given date.
/// Used to integrate Child Daily Logs with Daily Reports.
/// </summary>
public sealed record DailyLogsSummaryDto(
    Guid ChildId,
    string? ChildName,
    DateTime Date,
    IReadOnlyList<FeedingLogDto> Feedings,
    IReadOnlyList<NapLogDto> Naps,
    IReadOnlyList<ActivityLogDto> Activities,
    string SuggestedMealsSummary,
    string SuggestedNapSummary,
    string SuggestedActivitiesSummary);
