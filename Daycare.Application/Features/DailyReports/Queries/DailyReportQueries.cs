using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Queries;

/// <summary>Retrieve a single daily report by ID.</summary>
public sealed record GetDailyReportByIdQuery(
    Guid ReportId,
    Guid? RequestingParentId = null) : IRequest<Result<DailyReportDto>>;

/// <summary>Retrieve all reports for a child with optional date-range filtering and pagination.</summary>
public sealed record GetChildDailyReportsQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 30,
    Guid? RequestingParentId = null) : IRequest<Result<ChildDailyReportsDto>>;

/// <summary>Retrieve all reports authored on a specific date (staff feed).</summary>
public sealed record GetDailyReportsByDateQuery(
    DateTime? Date = null) : IRequest<Result<IReadOnlyList<DailyReportDto>>>;
