using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Incidents.Commands;

public sealed record CreateIncidentCommand(
    Guid ChildId,
    DateTime IncidentTime,
    string Location,
    string Description,
    InjuryType InjuryType,
    BodyPart AffectedBodyPart,
    string? FirstAidGiven,
    Guid StaffId,
    bool ParentNotified = false,
    string? ParentSignature = null,
    string? FollowUp = null,
    string? Notes = null) : IRequest<Result<IncidentDto>>;

public sealed record UpdateIncidentCommand(
    Guid Id,
    DateTime IncidentTime,
    string Location,
    string Description,
    InjuryType InjuryType,
    BodyPart AffectedBodyPart,
    string? FirstAidGiven,
    Guid StaffId,
    string? FollowUp = null,
    string? Notes = null) : IRequest<Result<IncidentDto>>;

public sealed record NotifyParentCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;

public sealed record RecordParentSignatureCommand(
    Guid Id,
    string Signature,
    Guid StaffId) : IRequest<Result>;

public sealed record RecordFollowUpCommand(
    Guid Id,
    string FollowUpDetails,
    Guid StaffId) : IRequest<Result>;

public sealed record DeleteIncidentCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;
