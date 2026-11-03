using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Enrollments.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Enrollments.Queries.Handlers;

/// <summary>
/// MediatR Handlers executing Enrollment Queries (View Enrollment, Enrollment History).
/// </summary>
public sealed class EnrollmentQueryHandlers :
    IRequestHandler<GetEnrollmentByIdQuery, Result<EnrollmentDto>>,
    IRequestHandler<GetEnrollmentsByChildIdQuery, Result<IReadOnlyList<EnrollmentDto>>>,
    IRequestHandler<GetActiveEnrollmentByChildIdQuery, Result<EnrollmentDto>>,
    IRequestHandler<GetAllEnrollmentsQuery, Result<EnrollmentListDto>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IChildRepository _childRepository;
    private readonly IClassroomRepository _classroomRepository;

    public EnrollmentQueryHandlers(
        IEnrollmentRepository enrollmentRepository,
        IChildRepository childRepository,
        IClassroomRepository classroomRepository)
    {
        _enrollmentRepository = enrollmentRepository ?? throw new ArgumentNullException(nameof(enrollmentRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _classroomRepository = classroomRepository ?? throw new ArgumentNullException(nameof(classroomRepository));
    }

    /// <summary>
    /// Gets a single enrollment by ID.
    /// </summary>
    public async Task<Result<EnrollmentDto>> Handle(GetEnrollmentByIdQuery query, CancellationToken cancellationToken)
    {
        var enrollmentId = EnrollmentId.Create(query.EnrollmentId);
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Result<EnrollmentDto>.Failure("Enrollment not found");
        }

        var child = await _childRepository.GetByIdAsync(enrollment.ChildId, cancellationToken);
        Classroom? classroom = null;
        if (enrollment.ClassroomId != null)
        {
            classroom = await _classroomRepository.GetByIdAsync(enrollment.ClassroomId, cancellationToken);
        }

        return Result<EnrollmentDto>.Success(MapToDto(enrollment, child, classroom));
    }

    /// <summary>
    /// Retrieves the enrollment history for a child (all past, current, pending enrollments).
    /// </summary>
    public async Task<Result<IReadOnlyList<EnrollmentDto>>> Handle(GetEnrollmentsByChildIdQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<IReadOnlyList<EnrollmentDto>>.Failure("Child not found");
        }

        var enrollments = await _enrollmentRepository.GetByChildIdAsync(childId, cancellationToken);
        var dtoList = new List<EnrollmentDto>();

        foreach (var enrollment in enrollments)
        {
            Classroom? classroom = null;
            if (enrollment.ClassroomId != null)
            {
                classroom = await _classroomRepository.GetByIdAsync(enrollment.ClassroomId, cancellationToken);
            }

            dtoList.Add(MapToDto(enrollment, child, classroom));
        }

        return Result<IReadOnlyList<EnrollmentDto>>.Success(dtoList);
    }

    /// <summary>
    /// Retrieves the currently active or pending enrollment for a child.
    /// </summary>
    public async Task<Result<EnrollmentDto>> Handle(GetActiveEnrollmentByChildIdQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<EnrollmentDto>.Failure("Child not found");
        }

        var enrollment = await _enrollmentRepository.GetActiveByChildIdAsync(childId, cancellationToken);
        if (enrollment is null)
        {
            return Result<EnrollmentDto>.Failure("No active enrollment found for this child");
        }

        Classroom? classroom = null;
        if (enrollment.ClassroomId != null)
        {
            classroom = await _classroomRepository.GetByIdAsync(enrollment.ClassroomId, cancellationToken);
        }

        return Result<EnrollmentDto>.Success(MapToDto(enrollment, child, classroom));
    }

    /// <summary>
    /// Retrieves a paginated list of enrollments with optional filters.
    /// </summary>
    public async Task<Result<EnrollmentListDto>> Handle(GetAllEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        EnrollmentStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<EnrollmentStatus>(query.Status, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        ProgramType? programTypeFilter = null;
        if (!string.IsNullOrWhiteSpace(query.ProgramType) && Enum.TryParse<ProgramType>(query.ProgramType, true, out var parsedProg))
        {
            programTypeFilter = parsedProg;
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        var items = await _enrollmentRepository.GetAllAsync(
            page,
            pageSize,
            statusFilter,
            programTypeFilter,
            query.ClassroomId,
            cancellationToken);

        var totalCount = await _enrollmentRepository.GetTotalCountAsync(
            statusFilter,
            programTypeFilter,
            query.ClassroomId,
            cancellationToken);

        var dtoList = new List<EnrollmentDto>();
        foreach (var enrollment in items)
        {
            var child = await _childRepository.GetByIdAsync(enrollment.ChildId, cancellationToken);
            Classroom? classroom = null;
            if (enrollment.ClassroomId != null)
            {
                classroom = await _classroomRepository.GetByIdAsync(enrollment.ClassroomId, cancellationToken);
            }

            dtoList.Add(MapToDto(enrollment, child, classroom));
        }

        var result = new EnrollmentListDto(
            Items: dtoList,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize);

        return Result<EnrollmentListDto>.Success(result);
    }

    private static EnrollmentDto MapToDto(Enrollment enrollment, Child? child, Classroom? classroom)
    {
        var childName = child != null ? $"{child.Name.FirstName} {child.Name.LastName}" : null;
        return new EnrollmentDto(
            Id: enrollment.Id.Value,
            ChildId: enrollment.ChildId.Value,
            ChildName: childName,
            ClassroomId: enrollment.ClassroomId?.Value,
            ClassroomName: classroom?.Name,
            ProgramType: enrollment.ProgramType.ToString(),
            DaysOfWeek: enrollment.DaysOfWeek,
            StartDate: enrollment.StartDate,
            EndDate: enrollment.EndDate,
            ContractSigned: enrollment.ContractSigned,
            ImmunizationRecord: enrollment.ImmunizationRecord,
            BirthCertificate: enrollment.BirthCertificate,
            MedicalForm: enrollment.MedicalForm,
            Status: enrollment.Status.ToString(),
            EndReason: enrollment.EndReason,
            CreatedAt: enrollment.CreatedAt,
            UpdatedAt: enrollment.UpdatedAt);
    }
}
