using System.ComponentModel.DataAnnotations;
using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

public sealed record IncidentDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime IncidentTime,
    string Location,
    string Description,
    InjuryType InjuryType,
    string InjuryTypeName,
    BodyPart AffectedBodyPart,
    string AffectedBodyPartName,
    string? FirstAidGiven,
    Guid StaffId,
    bool ParentNotified,
    string? ParentSignature,
    string? FollowUp,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateIncidentRequestDto(
    [Required] Guid ChildId,
    [Required] DateTime IncidentTime,
    [Required, StringLength(255)] string Location,
    [Required, StringLength(1000)] string Description,
    [EnumDataType(typeof(InjuryType))] InjuryType InjuryType,
    [EnumDataType(typeof(BodyPart))] BodyPart AffectedBodyPart,
    [StringLength(500)] string? FirstAidGiven,
    [StringLength(1000)] string? FollowUp,
    [StringLength(1000)] string? Notes);

public sealed record UpdateIncidentRequestDto(
    [Required] DateTime IncidentTime,
    [Required, StringLength(255)] string Location,
    [Required, StringLength(1000)] string Description,
    [EnumDataType(typeof(InjuryType))] InjuryType InjuryType,
    [EnumDataType(typeof(BodyPart))] BodyPart AffectedBodyPart,
    [StringLength(500)] string? FirstAidGiven,
    [StringLength(1000)] string? FollowUp,
    [StringLength(1000)] string? Notes);

public sealed record NotifyParentRequestDto(
    bool Notify = true);

public sealed record RecordParentSignatureRequestDto(
    [Required] string Signature);

public sealed record RecordFollowUpRequestDto(
    [Required] string FollowUpDetails);
