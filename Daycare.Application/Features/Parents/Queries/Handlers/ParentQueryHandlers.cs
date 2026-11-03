using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Parents.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Parents.Queries.Handlers;

/// <summary>
/// CQRS Query Handlers for Parent operations
/// </summary>
public sealed class ParentQueryHandlers :
    IRequestHandler<GetParentQuery, Result<ParentDto>>,
    IRequestHandler<GetAllParentsQuery, Result<ParentListResponse>>,
    IRequestHandler<SearchParentsQuery, Result<ParentListResponse>>,
    IRequestHandler<GetParentsByStatusQuery, Result<IEnumerable<ParentDto>>>,
    IRequestHandler<GetParentsByChildQuery, Result<IEnumerable<ParentDto>>>
{
    private readonly IParentRepository _parentRepository;

    public ParentQueryHandlers(IParentRepository parentRepository)
    {
        _parentRepository = parentRepository;
    }

    public async Task<Result<ParentDto>> Handle(GetParentQuery query, CancellationToken cancellationToken)
    {
        var parentId = ParentId.Create(query.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);

        if (parent is null)
            return Result<ParentDto>.Failure("Parent not found");

        var dto = MapToDto(parent);
        return Result<ParentDto>.Success(dto);
    }

    public async Task<Result<ParentListResponse>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken)
    {
        var parents = await _parentRepository.GetAllAsync(query.PageNumber, query.PageSize, cancellationToken);
        var total = await _parentRepository.GetTotalCountAsync(cancellationToken);

        var dtos = parents.Select(MapToDto).ToList();
        var response = new ParentListResponse(dtos, total, query.PageNumber, query.PageSize);

        return Result<ParentListResponse>.Success(response);
    }

    public async Task<Result<ParentListResponse>> Handle(SearchParentsQuery query, CancellationToken cancellationToken)
    {
        var parents = await _parentRepository.SearchAsync(query.SearchTerm, query.PageNumber, query.PageSize, cancellationToken);
        var allForCount = await _parentRepository.SearchAsync(query.SearchTerm, 1, int.MaxValue, cancellationToken);
        var total = allForCount.Count();

        var dtos = parents.Select(MapToDto).ToList();
        var response = new ParentListResponse(dtos, total, query.PageNumber, query.PageSize);

        return Result<ParentListResponse>.Success(response);
    }

    public async Task<Result<IEnumerable<ParentDto>>> Handle(GetParentsByStatusQuery query, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ParentStatus>(query.Status, true, out var status))
            return Result<IEnumerable<ParentDto>>.Failure("Invalid parent status");

        var parents = await _parentRepository.GetByStatusAsync(status, cancellationToken);
        var dtos = parents.Select(MapToDto);

        return Result<IEnumerable<ParentDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<ParentDto>>> Handle(GetParentsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
        var dtos = parents.Select(MapToDto);

        return Result<IEnumerable<ParentDto>>.Success(dtos);
    }

    private static ParentDto MapToDto(Parent parent) => new(
        parent.ParentId.Value,
        parent.Name.FirstName,
        parent.Name.LastName,
        parent.ContactInfo.Email,
        parent.ContactInfo.PhoneNumber,
        parent.Address,
        parent.Status.ToString(),
        parent.Notes,
        parent.CreatedAt,
        parent.UpdatedAt);
}
