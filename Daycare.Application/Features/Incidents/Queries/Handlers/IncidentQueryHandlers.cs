using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Incidents.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Incidents.Queries.Handlers;

/// <summary>
/// Handles all Incident queries with strict parent authorization.
/// Parents can only access incident records for their own children.
/// </summary>
public sealed class IncidentQueryHandlers :
    IRequestHandler<GetIncidentByIdQuery, Result<IncidentDto>>,
    IRequestHandler<GetIncidentsByChildQuery, Result<IReadOnlyList<IncidentDto>>>,
    IRequestHandler<GetIncidentsByDateQuery, Result<IReadOnlyList<IncidentDto>>>,
    IRequestHandler<GetUnsignedIncidentsByChildQuery, Result<IReadOnlyList<IncidentDto>>>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public IncidentQueryHandlers(
        IIncidentRepository incidentRepository,
        IChildRepository childRepository,
        IParentRepository parentRepository)
    {
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    public async Task<Result<IncidentDto>> Handle(GetIncidentByIdQuery query, CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(query.Id), cancellationToken);
        if (incident is null)
            return Result<IncidentDto>.Failure("Incident not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(incident.ChildId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IncidentDto>.Failure("Unauthorized: You do not have permission to view incidents for this child.");
        }

        var child = incident.Child ?? await _childRepository.GetByIdAsync(incident.ChildId, cancellationToken);
        return Result<IncidentDto>.Success(MapToDto(incident, child));
    }

    public async Task<Result<IReadOnlyList<IncidentDto>>> Handle(GetIncidentsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<IncidentDto>>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IReadOnlyList<IncidentDto>>.Failure("Unauthorized: You do not have permission to view incidents for this child.");
        }

        var incidents = await _incidentRepository.GetByChildIdAsync(
            childId,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var dtos = incidents
            .Select(i => MapToDto(i, child))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<IncidentDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<IncidentDto>>> Handle(GetIncidentsByDateQuery query, CancellationToken cancellationToken)
    {
        var incidents = await _incidentRepository.GetByChildAndDateAsync(
            null!,
            query.Date,
            cancellationToken);

        var incidentDtos = new List<IncidentDto>();
        foreach (var incident in incidents)
        {
            var child = await _childRepository.GetByIdAsync(incident.ChildId, cancellationToken);
            incidentDtos.Add(MapToDto(incident, child));
        }

        return Result<IReadOnlyList<IncidentDto>>.Success(incidentDtos.AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<IncidentDto>>> Handle(GetUnsignedIncidentsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<IncidentDto>>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IReadOnlyList<IncidentDto>>.Failure("Unauthorized: You do not have permission to view incidents for this child.");
        }

        var incidents = await _incidentRepository.GetNotSignedIncidentsAsync(childId, cancellationToken);

        var dtos = incidents
            .Select(i => MapToDto(i, child))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<IncidentDto>>.Success(dtos);
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
