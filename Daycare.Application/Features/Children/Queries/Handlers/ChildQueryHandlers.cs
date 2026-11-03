using MediatR;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Children.Queries;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Children.Queries.Handlers;

/// <summary>
/// Handler for GetChildByIdQuery.
/// </summary>
public sealed class GetChildByIdQueryHandler : IRequestHandler<GetChildByIdQuery, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;

    public GetChildByIdQueryHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ChildDto>> Handle(GetChildByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<ChildDto>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);

        if (child is null)
            return Result<ChildDto>.Failure("Child not found", "CHILD_NOT_FOUND");

        var dto = MapToDto(child);
        return Result<ChildDto>.Success(dto);
    }

    private static ChildDto MapToDto(Child child)
    {
        return new ChildDto(
            child.Id.Value.ToString(),
            child.Name.FirstName,
            child.Name.LastName,
            child.DateOfBirth.Date,
            child.DateOfBirth.Age,
            child.Gender,
            child.PhotoUrl,
            child.EnrollmentDate,
            child.Allergies,
            child.MedicalNotes,
            child.Status.ToString(),
            child.CreatedAt,
            child.UpdatedAt);
    }
}

/// <summary>
/// Handler for GetAllChildrenQuery.
/// </summary>
public sealed class GetAllChildrenQueryHandler : IRequestHandler<GetAllChildrenQuery, Result<ChildListDto>>
{
    private readonly IChildRepository _childRepository;

    public GetAllChildrenQueryHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ChildListDto>> Handle(GetAllChildrenQuery request, CancellationToken cancellationToken)
    {
        var children = await _childRepository.GetAllAsync(request.Skip, request.Take, cancellationToken);
        var total = await _childRepository.GetCountAsync(cancellationToken);

        var dtos = children.Select(MapToDto).ToList();
        var listDto = new ChildListDto(dtos, total, request.Skip, request.Take);

        return Result<ChildListDto>.Success(listDto);
    }

    private static ChildDto MapToDto(Child child)
    {
        return new ChildDto(
            child.Id.Value.ToString(),
            child.Name.FirstName,
            child.Name.LastName,
            child.DateOfBirth.Date,
            child.DateOfBirth.Age,
            child.Gender,
            child.PhotoUrl,
            child.EnrollmentDate,
            child.Allergies,
            child.MedicalNotes,
            child.Status.ToString(),
            child.CreatedAt,
            child.UpdatedAt);
    }
}

/// <summary>
/// Handler for GetChildParentsQuery.
/// Resolves each parent link of a child into a display DTO carrying the
/// parent's name/contact details together with the role stored on the link.
/// </summary>
public sealed class GetChildParentsQueryHandler : IRequestHandler<GetChildParentsQuery, Result<IReadOnlyList<ParentLinkDto>>>
{
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public GetChildParentsQueryHandler(
        IChildRepository childRepository,
        IParentRepository parentRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    public async Task<Result<IReadOnlyList<ParentLinkDto>>> Handle(
        GetChildParentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<IReadOnlyList<ParentLinkDto>>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);

        if (!await _childRepository.ExistsAsync(childId, cancellationToken))
            return Result<IReadOnlyList<ParentLinkDto>>.Failure("Child not found", "CHILD_NOT_FOUND");

        var links = await _childRepository.GetParentLinksAsync(childId, cancellationToken);
        var dtos = new List<ParentLinkDto>(links.Count);

        foreach (var link in links)
        {
            var parent = await _parentRepository.GetByIdAsync(link.ParentId, cancellationToken);
            if (parent is null)
                continue;

            dtos.Add(new ParentLinkDto(
                parent.Id.Value,
                parent.Name.FirstName,
                parent.Name.LastName,
                parent.ContactInfo.Email,
                parent.ContactInfo.PhoneNumber,
                link.Role.ToString()));
        }

        return Result<IReadOnlyList<ParentLinkDto>>.Success(dtos);
    }
}

/// <summary>
/// Handler for ChildExistsQuery.
/// </summary>
public sealed class ChildExistsQueryHandler : IRequestHandler<ChildExistsQuery, Result<bool>>
{
    private readonly IChildRepository _childRepository;

    public ChildExistsQueryHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<bool>> Handle(ChildExistsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<bool>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);
        var exists = await _childRepository.ExistsAsync(childId, cancellationToken);
        return Result<bool>.Success(exists);
    }
}
