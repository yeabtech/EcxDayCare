using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Staff.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Staff.Queries.Handlers;

/// <summary>
/// MediatR Query Handlers for Staff CQRS queries
/// </summary>
public sealed class StaffQueryHandlers :
    IRequestHandler<GetStaffByIdQuery, Result<StaffDto>>,
    IRequestHandler<GetAllStaffQuery, Result<StaffListResponse>>,
    IRequestHandler<SearchStaffQuery, Result<StaffListResponse>>,
    IRequestHandler<GetStaffByClassroomQuery, Result<IEnumerable<StaffDto>>>
{
    private readonly IStaffRepository _staffRepository;

    public StaffQueryHandlers(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
    }

    public async Task<Result<StaffDto>> Handle(GetStaffByIdQuery query, CancellationToken cancellationToken)
    {
        var staffId = StaffId.Create(query.StaffId);
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
        if (staff is null)
            return Result<StaffDto>.Failure("Staff member not found");

        return Result<StaffDto>.Success(MapToDto(staff));
    }

    public async Task<Result<StaffListResponse>> Handle(GetAllStaffQuery query, CancellationToken cancellationToken)
    {
        StaffRole? role = null;
        if (!string.IsNullOrWhiteSpace(query.Role) && Enum.TryParse<StaffRole>(query.Role, true, out var parsedRole))
        {
            role = parsedRole;
        }

        var staffList = await _staffRepository.GetAllAsync(
            query.Page,
            query.PageSize,
            query.ClassroomId,
            role,
            cancellationToken);

        var total = await _staffRepository.GetTotalCountAsync(
            query.ClassroomId,
            role,
            cancellationToken);

        var dtos = staffList.Select(MapToDto).ToList();
        var response = new StaffListResponse(dtos, total, query.Page, query.PageSize);

        return Result<StaffListResponse>.Success(response);
    }

    public async Task<Result<StaffListResponse>> Handle(SearchStaffQuery query, CancellationToken cancellationToken)
    {
        var staffList = await _staffRepository.SearchAsync(
            query.Term,
            query.Page,
            query.PageSize,
            cancellationToken);

        var total = await _staffRepository.GetSearchCountAsync(query.Term, cancellationToken);

        var dtos = staffList.Select(MapToDto).ToList();
        var response = new StaffListResponse(dtos, total, query.Page, query.PageSize);

        return Result<StaffListResponse>.Success(response);
    }

    public async Task<Result<IEnumerable<StaffDto>>> Handle(GetStaffByClassroomQuery query, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(query.ClassroomId);
        var staffList = await _staffRepository.GetByClassroomIdAsync(classroomId, cancellationToken);
        var dtos = staffList.Select(MapToDto).ToList();

        return Result<IEnumerable<StaffDto>>.Success(dtos);
    }

    private static StaffDto MapToDto(Domain.Entities.Staff staff) => new(
        staff.Id.Value,
        staff.FirstName,
        staff.LastName,
        staff.Role.ToString(),
        staff.ClassroomId?.Value,
        staff.Classroom?.Name,
        staff.Phone,
        staff.Email,
        staff.HireDate,
        staff.Certifications,
        staff.ShiftStart,
        staff.ShiftEnd,
        staff.CreatedAt,
        staff.UpdatedAt);
}
