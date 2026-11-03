using System.Text;
using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.ActivityLogs.Commands.Handlers;
using Daycare.Application.Features.FeedingLogs.Commands.Handlers;
using Daycare.Application.Features.NapLogs.Commands.Handlers;
using Daycare.Application.Features.DailyReports.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Queries.Handlers;

public sealed class GetDailyLogsSummaryQueryHandler : IRequestHandler<GetDailyLogsSummaryQuery, Result<DailyLogsSummaryDto>>
{
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly INapLogRepository _napLogRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public GetDailyLogsSummaryQueryHandler(
        IChildRepository childRepository,
        IParentRepository parentRepository,
        IFeedingLogRepository feedingLogRepository,
        INapLogRepository napLogRepository,
        IActivityLogRepository activityLogRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        _feedingLogRepository = feedingLogRepository ?? throw new ArgumentNullException(nameof(feedingLogRepository));
        _napLogRepository = napLogRepository ?? throw new ArgumentNullException(nameof(napLogRepository));
        _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
    }

    public async Task<Result<DailyLogsSummaryDto>> Handle(GetDailyLogsSummaryQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<DailyLogsSummaryDto>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            if (!parents.Any(p => p.Id.Value == query.RequestingParentId.Value))
                return Result<DailyLogsSummaryDto>.Failure("Unauthorized: You do not have permission to view logs for this child.");
        }

        var feedings = (await _feedingLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken))
            .OrderBy(f => f.Time)
            .ToList();

        var naps = (await _napLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken))
            .OrderBy(n => n.StartTime)
            .ToList();

        var activities = (await _activityLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken))
            .OrderBy(a => a.Time)
            .ToList();

        var feedingDtos = feedings.Select(f => FeedingLogCommandHandlers.MapToDto(f, child)).ToList().AsReadOnly();
        var napDtos = naps.Select(n => NapLogCommandHandlers.MapToDto(n, child)).ToList().AsReadOnly();
        var activityDtos = activities.Select(a => ActivityLogCommandHandlers.MapToDto(a, child)).ToList().AsReadOnly();

        var mealsSummary = GenerateMealsSummary(feedings);
        var napSummary = GenerateNapSummary(naps);
        var activitiesSummary = GenerateActivitiesSummary(activities);

        var childName = $"{child.Name.FirstName} {child.Name.LastName}".Trim();

        return Result<DailyLogsSummaryDto>.Success(new DailyLogsSummaryDto(
            ChildId: query.ChildId,
            ChildName: childName,
            Date: query.Date.Date,
            Feedings: feedingDtos,
            Naps: napDtos,
            Activities: activityDtos,
            SuggestedMealsSummary: mealsSummary,
            SuggestedNapSummary: napSummary,
            SuggestedActivitiesSummary: activitiesSummary));
    }

    private static string GenerateMealsSummary(List<FeedingLog> feedings)
    {
        if (feedings.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var f in feedings)
        {
            var timeStr = f.Time.ToString("h:mm tt");
            var itemDesc = !string.IsNullOrWhiteSpace(f.FoodItems) ? f.FoodItems : f.FeedingType.ToString();
            var amountStr = !string.IsNullOrWhiteSpace(f.Amount) ? $" ({f.Amount})" : string.Empty;
            sb.AppendLine($"• {timeStr} - {itemDesc}{amountStr} [Ate: {f.PortionConsumed}]");
        }
        return sb.ToString().TrimEnd();
    }

    private static string GenerateNapSummary(List<NapLog> naps)
    {
        if (naps.Count == 0) return string.Empty;

        var totalMinutes = naps.Where(n => n.DurationMinutes.HasValue).Sum(n => n.DurationMinutes!.Value);
        var sb = new StringBuilder();
        foreach (var n in naps)
        {
            var startStr = n.StartTime.ToString("h:mm tt");
            var endStr = n.EndTime.HasValue ? n.EndTime.Value.ToString("h:mm tt") : "In progress";
            var durStr = n.DurationMinutes.HasValue ? $"{n.DurationMinutes.Value} mins" : "ongoing";
            sb.AppendLine($"• {startStr} to {endStr} ({durStr}, {n.Quality})");
        }

        if (totalMinutes > 0)
        {
            var hrs = totalMinutes / 60;
            var mins = totalMinutes % 60;
            var formattedTotal = hrs > 0 ? $"{hrs}h {mins}m" : $"{mins} mins";
            sb.AppendLine($"Total nap time: {formattedTotal}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string GenerateActivitiesSummary(List<ActivityLog> activities)
    {
        if (activities.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var a in activities)
        {
            var timeStr = a.Time.ToString("h:mm tt");
            var durStr = a.DurationMinutes.HasValue ? $" ({a.DurationMinutes.Value} mins)" : string.Empty;
            sb.AppendLine($"• {timeStr} - {a.Title}{durStr}: {a.Participation}");
            if (!string.IsNullOrWhiteSpace(a.Description))
            {
                sb.AppendLine($"  {a.Description}");
            }
        }
        return sb.ToString().TrimEnd();
    }
}
