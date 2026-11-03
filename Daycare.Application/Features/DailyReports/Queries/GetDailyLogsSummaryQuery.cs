using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Queries;

/// <summary>
/// Retrieves an aggregated summary of Feeding, Nap, and Activity logs for a child on a specific date,
/// including pre-formatted suggested text for Daily Reports.
/// </summary>
public sealed record GetDailyLogsSummaryQuery(
    Guid ChildId,
    DateTime Date,
    Guid? RequestingParentId = null) : IRequest<Result<DailyLogsSummaryDto>>;
