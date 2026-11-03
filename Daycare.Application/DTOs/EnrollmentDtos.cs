namespace Daycare.Application.DTOs;

/// <summary>
/// Detailed DTO representing an Enrollment.
/// </summary>
public sealed record EnrollmentDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    Guid? ClassroomId,
    string? ClassroomName,
    string ProgramType,
    string DaysOfWeek,
    DateTime StartDate,
    DateTime? EndDate,
    bool ContractSigned,
    bool ImmunizationRecord,
    bool BirthCertificate,
    bool MedicalForm,
    string Status,
    string? EndReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Request DTO for creating a new enrollment.
/// </summary>
public sealed record CreateEnrollmentDto(
    Guid ChildId,
    string ProgramType,
    string DaysOfWeek,
    DateTime StartDate,
    DateTime? EndDate = null,
    bool ContractSigned = false,
    bool ImmunizationRecord = false,
    bool BirthCertificate = false,
    bool MedicalForm = false,
    Guid? ClassroomId = null);

/// <summary>
/// Request DTO for updating an existing enrollment.
/// </summary>
public sealed record UpdateEnrollmentDto(
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
    string? EndReason = null);

/// <summary>
/// Request DTO for ending an enrollment.
/// </summary>
public sealed record EndEnrollmentDto(
    DateTime EndDate,
    string? Reason = null);

/// <summary>
/// Paged response list of enrollments.
/// </summary>
public sealed record EnrollmentListDto(
    IReadOnlyList<EnrollmentDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
