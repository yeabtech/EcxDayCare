using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Enrollments.Commands;

/// <summary>
/// Command to create a new enrollment for a child.
/// </summary>
public sealed record CreateEnrollmentCommand(
    Guid ChildId,
    string ProgramType,
    string DaysOfWeek,
    DateTime StartDate,
    DateTime? EndDate = null,
    bool ContractSigned = false,
    bool ImmunizationRecord = false,
    bool BirthCertificate = false,
    bool MedicalForm = false,
    Guid? ClassroomId = null) : IRequest<Result<EnrollmentDto>>;

/// <summary>
/// Command to update an existing enrollment.
/// </summary>
public sealed record UpdateEnrollmentCommand(
    Guid EnrollmentId,
    string ProgramType,
    string DaysOfWeek,
    DateTime StartDate,
    DateTime? EndDate = null,
    bool ContractSigned = false,
    bool ImmunizationRecord = false,
    bool BirthCertificate = false,
    bool MedicalForm = false,
    Guid? ClassroomId = null,
    string? Status = null,
    string? EndReason = null) : IRequest<Result<EnrollmentDto>>;

/// <summary>
/// Command to end an active enrollment.
/// </summary>
public sealed record EndEnrollmentCommand(
    Guid EnrollmentId,
    DateTime EndDate,
    string? Reason = null) : IRequest<Result<EnrollmentDto>>;
