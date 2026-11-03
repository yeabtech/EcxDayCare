using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Parents.Queries;

/// <summary>
/// CQRS Queries for Parent operations
/// </summary>

/// <summary>
/// Get a single parent by ID
/// </summary>
public sealed record GetParentQuery(Guid ParentId) : IRequest<Result<ParentDto>>;

/// <summary>
/// Get all parents with pagination
/// </summary>
public sealed record GetAllParentsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<ParentListResponse>>;

/// <summary>
/// Search parents by name or email
/// </summary>
public sealed record SearchParentsQuery(string SearchTerm, int PageNumber = 1, int PageSize = 10) : IRequest<Result<ParentListResponse>>;

/// <summary>
/// Get parents by status
/// </summary>
public sealed record GetParentsByStatusQuery(string Status) : IRequest<Result<IEnumerable<ParentDto>>>;

/// <summary>
/// Get all parents for a specific child
/// </summary>
public sealed record GetParentsByChildQuery(Guid ChildId) : IRequest<Result<IEnumerable<ParentDto>>>;

/// <summary>
/// Response for paginated parent list
/// </summary>
public sealed record ParentListResponse(
    IEnumerable<ParentDto> Items,
    int Total,
    int Page,
    int PageSize);
