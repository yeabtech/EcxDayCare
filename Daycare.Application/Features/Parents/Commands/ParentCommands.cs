using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Parents.Commands;

/// <summary>
/// CQRS Commands for Parent operations
/// </summary>

/// <summary>
/// Create a new parent
/// </summary>
public sealed record CreateParentCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address,
    string? Notes = null) : IRequest<Result<ParentDto>>;

/// <summary>
/// Update an existing parent
/// </summary>
public sealed record UpdateParentCommand(
    Guid ParentId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address,
    string Status,
    string? Notes = null) : IRequest<Result>;

/// <summary>
/// Delete a parent
/// </summary>
public sealed record DeleteParentCommand(Guid ParentId) : IRequest<Result>;

/// <summary>
/// Link a child to a parent
/// </summary>
public sealed record LinkChildToParentCommand(Guid ParentId, Guid ChildId) : IRequest<Result>;

/// <summary>
/// Remove a child from a parent
/// </summary>
public sealed record UnlinkChildFromParentCommand(Guid ParentId, Guid ChildId) : IRequest<Result>;
