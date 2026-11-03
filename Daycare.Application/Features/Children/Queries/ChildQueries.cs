using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Children.Queries;

/// <summary>
/// Query to retrieve a single child by ID.
/// </summary>
public sealed record GetChildByIdQuery(string ChildId) : IRequest<Result<ChildDto>>;

/// <summary>
/// Query to retrieve all children with pagination.
/// </summary>
public sealed record GetAllChildrenQuery(
    int Skip = 0,
    int Take = 10) : IRequest<Result<ChildListDto>>;

/// <summary>
/// Query to check if a child exists.
/// </summary>
public sealed record ChildExistsQuery(string ChildId) : IRequest<Result<bool>>;

/// <summary>
/// Query to retrieve every parent linked to a child (with the stored role).
/// </summary>
public sealed record GetChildParentsQuery(string ChildId) : IRequest<Result<IReadOnlyList<ParentLinkDto>>>;
