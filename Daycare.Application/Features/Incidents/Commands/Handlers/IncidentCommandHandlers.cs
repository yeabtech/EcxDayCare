using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Incidents.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Incidents.Commands.Handlers;

public sealed class IncidentCommandHandlers :
    IRequestHandler<CreateIncidentCommand, Result<IncidentDto>>,
    IRequestHandler<UpdateIncidentCommand, Result<IncidentDto>>,
    IRequestHandler<NotifyParentCommand, Result>,
    IRequestHandler<RecordParentSignatureCommand, Result>,
    IRequestHandler<RecordFollowUpCommand, Result>,
    IRequestHandler<DeleteIncidentCommand, Result>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IChildRepository _childRepository;

    public IncidentCommandHandlers(
        IIncidentRepository incidentRepository,
        IChildRepository childRepository)
    {
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<IncidentDto>> Handle(CreateIncidentCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IncidentDto>.Failure("Child not found.");

        var incidentResult = Incident.Create(
            childId,
            command.IncidentTime,
            command.Location,
            command.Description,
            command.InjuryType,
            command.AffectedBodyPart,
            command.FirstAidGiven,
            command.StaffId,
            command.ParentNotified,
            command.ParentSignature,
            command.FollowUp,
            command.Notes);

        if (incidentResult.IsFailure)
            return Result<IncidentDto>.Failure(incidentResult.Error!);

        await _incidentRepository.AddAsync(incidentResult.Value!, cancellationToken);

        return Result<IncidentDto>.Success(MapToDto(incidentResult.Value!, child));
    }

    public async Task<Result<IncidentDto>> Handle(UpdateIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(command.Id), cancellationToken);
        if (incident is null)
            return Result<IncidentDto>.Failure("Incident not found.");

        var updateResult = incident.Update(
            command.IncidentTime,
            command.Location,
            command.Description,
            command.InjuryType,
            command.AffectedBodyPart,
            command.FirstAidGiven,
            command.FollowUp,
            command.Notes);

        if (updateResult.IsFailure)
            return Result<IncidentDto>.Failure(updateResult.Error!);

        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        var child = incident.Child ?? await _childRepository.GetByIdAsync(incident.ChildId, cancellationToken);
        return Result<IncidentDto>.Success(MapToDto(incident, child));
    }

    public async Task<Result> Handle(NotifyParentCommand command, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(command.Id), cancellationToken);
        if (incident is null)
            return Result.Failure("Incident not found.");

        var result = incident.NotifyParent();
        if (result.IsFailure)
            return result;

        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(RecordParentSignatureCommand command, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(command.Id), cancellationToken);
        if (incident is null)
            return Result.Failure("Incident not found.");

        var result = incident.RecordParentSignature(command.Signature);
        if (result.IsFailure)
            return result;

        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(RecordFollowUpCommand command, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(command.Id), cancellationToken);
        if (incident is null)
            return Result.Failure("Incident not found.");

        var result = incident.RecordFollowUp(command.FollowUpDetails);
        if (result.IsFailure)
            return result;

        await _incidentRepository.UpdateAsync(incident, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(DeleteIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(command.Id), cancellationToken);
        if (incident is null)
            return Result.Failure("Incident not found.");

        await _incidentRepository.DeleteAsync(incident, cancellationToken);

        return Result.Success();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static IncidentDto MapToDto(Incident incident, Child? child) =>
        new(
            incident.Id.Value,
            incident.ChildId.Value,
            child?.Name.FullName,
            incident.IncidentTime,
            incident.Location,
            incident.Description,
            incident.InjuryType,
            incident.InjuryType.ToString(),
            incident.AffectedBodyPart,
            incident.AffectedBodyPart.ToString(),
            incident.FirstAidGiven,
            incident.StaffId,
            incident.ParentNotified,
            incident.ParentSignature,
            incident.FollowUp,
            incident.Notes,
            incident.CreatedAt,
            incident.UpdatedAt);
}
