using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Enrollments.Queries;

/// <summary>
/// Query to retrieve a single enrollment by its unique ID.
/// </summary>
public sealed record GetEnrollmentByIdQuery(Guid EnrollmentId) : IRequest<Result<EnrollmentDto>>;

/// <summary>
/// Query to retrieve the full enrollment history for a specific child.
/// </summary>
public sealed record GetEnrollmentsByChildIdQuery(Guid ChildId) : IRequest<Result<IReadOnlyList<EnrollmentDto>>>;

/// <summary>
/// Query to retrieve the active or pending enrollment for a specific child.
/// </summary>
public sealed record GetActiveEnrollmentByChildIdQuery(Guid ChildId) : IRequest<Result<EnrollmentDto>>;

/// <summary>
/// Query to retrieve paginated enrollments with optional status and filter criteria.
/// </summary>
public sealed record GetAllEnrollmentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    string? ProgramType = null,
    Guid? ClassroomId = null) : IRequest<Result<EnrollmentListDto>>;
