using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.DailyReports.Commands.Handlers;
using Daycare.Application.Features.DailyReports.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Queries.Handlers;

/// <summary>
/// Handles all DailyReport queries: GetById, GetByChild, GetByDate.
/// Parents receive only reports for their own children — authorization is enforced in the controller.
/// </summary>
public sealed class DailyReportQueryHandlers :
    IRequestHandler<GetDailyReportByIdQuery, Result<DailyReportDto>>,
    IRequestHandler<GetChildDailyReportsQuery, Result<ChildDailyReportsDto>>,
    IRequestHandler<GetDailyReportsByDateQuery, Result<IReadOnlyList<DailyReportDto>>>
{
    private readonly IDailyReportRepository _reportRepository;
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public DailyReportQueryHandlers(
        IDailyReportRepository reportRepository,
        IChildRepository childRepository,
        IParentRepository parentRepository)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    // ─── GetById ──────────────────────────────────────────────────────────────

    public async Task<Result<DailyReportDto>> Handle(GetDailyReportByIdQuery query, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(DailyReportId.Create(query.ReportId), cancellationToken);
        if (report is null)
            return Result<DailyReportDto>.Failure("Daily report not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(report.ChildId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<DailyReportDto>.Failure("Unauthorized: You do not have permission to view reports for this child.");

            if (!report.SentToParent)
                return Result<DailyReportDto>.Failure("This report is still being drafted and has not yet been sent to parents.");
        }

        var child = report.Child ?? await _childRepository.GetByIdAsync(report.ChildId, cancellationToken);
        return Result<DailyReportDto>.Success(DailyReportCommandHandlers.MapToDto(report, child));
    }

    // ─── GetByChild ───────────────────────────────────────────────────────────

    public async Task<Result<ChildDailyReportsDto>> Handle(GetChildDailyReportsQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<ChildDailyReportsDto>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<ChildDailyReportsDto>.Failure("Unauthorized: You do not have permission to view reports for this child.");
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var reports = await _reportRepository.GetByChildIdAsync(
            childId,
            query.StartDate,
            query.EndDate,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await _reportRepository.GetCountByChildIdAsync(
            childId,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var childName = $"{child.Name.FirstName} {child.Name.LastName}".Trim();

        var items = reports
            .Where(r => !query.RequestingParentId.HasValue || r.SentToParent)
            .Select(r => DailyReportCommandHandlers.MapToDto(r, child))
            .ToList()
            .AsReadOnly();

        return Result<ChildDailyReportsDto>.Success(new ChildDailyReportsDto(
            ChildId: query.ChildId,
            ChildName: childName,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            Items: items));
    }

    // ─── GetByDate ────────────────────────────────────────────────────────────

    public async Task<Result<IReadOnlyList<DailyReportDto>>> Handle(GetDailyReportsByDateQuery query, CancellationToken cancellationToken)
    {
        var date = query.Date ?? DateTime.UtcNow;

        var reports = await _reportRepository.GetByDateAsync(date, cancellationToken);

        // Eagerly fetch children not already loaded via navigation property
        var childIds = reports
            .Where(r => r.Child is null)
            .Select(r => r.ChildId)
            .Distinct()
            .ToList();

        var childCache = new Dictionary<ChildId, Child?>();
        foreach (var cid in childIds)
        {
            childCache[cid] = await _childRepository.GetByIdAsync(cid, cancellationToken);
        }

        var dtos = reports
            .Select(r =>
            {
                var child = r.Child ?? (childCache.TryGetValue(r.ChildId, out var c) ? c : null);
                return DailyReportCommandHandlers.MapToDto(r, child);
            })
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<DailyReportDto>>.Success(dtos);
    }
}
