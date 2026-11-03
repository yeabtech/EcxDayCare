using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Staff.Queries;

/// <summary>
/// Query to get a staff member by ID
/// </summary>
public sealed record GetStaffByIdQuery(Guid StaffId) : IRequest<Result<StaffDto>>;

/// <summary>
/// Query to get all staff members with pagination and optional filters
/// </summary>
public sealed record GetAllStaffQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? ClassroomId = null,
    string? Role = null) : IRequest<Result<StaffListResponse>>;

/// <summary>
/// Query to search staff members by term
/// </summary>
public sealed record SearchStaffQuery(
    string Term,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<StaffListResponse>>;

/// <summary>
/// Query to get staff members assigned to a specific classroom
/// </summary>
public sealed record GetStaffByClassroomQuery(Guid ClassroomId) : IRequest<Result<IEnumerable<StaffDto>>>;
