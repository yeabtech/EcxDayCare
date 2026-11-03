using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Parents.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Parents.Commands.Handlers;

/// <summary>
/// CQRS Command Handlers for Parent operations
/// </summary>
public sealed class ParentCommandHandlers :
    IRequestHandler<CreateParentCommand, Result<ParentDto>>,
    IRequestHandler<UpdateParentCommand, Result>,
    IRequestHandler<DeleteParentCommand, Result>,
    IRequestHandler<LinkChildToParentCommand, Result>,
    IRequestHandler<UnlinkChildFromParentCommand, Result>
{
    private readonly IParentRepository _parentRepository;
    private readonly IChildRepository _childRepository;

    public ParentCommandHandlers(
        IParentRepository parentRepository,
        IChildRepository childRepository)
    {
        _parentRepository = parentRepository;
        _childRepository = childRepository;
    }

    public async Task<Result<ParentDto>> Handle(CreateParentCommand command, CancellationToken cancellationToken)
    {
        // Create parent entity
        var createResult = Parent.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Address,
            ParentStatus.Active,
            command.Notes);

        if (createResult.IsFailure)
            return Result<ParentDto>.Failure(createResult.Error!);

        var parent = createResult.Value!;

        // Add to repository
        await _parentRepository.AddAsync(parent, cancellationToken);

        var dto = MapToDto(parent);
        return Result<ParentDto>.Success(dto);
    }

    public async Task<Result> Handle(UpdateParentCommand command, CancellationToken cancellationToken)
    {
        // Fetch parent
        var parentId = ParentId.Create(command.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);

        if (parent is null)
            return Result.Failure("Parent not found");

        // Parse status
        if (!Enum.TryParse<ParentStatus>(command.Status, true, out var status))
            return Result.Failure("Invalid parent status");

        // Update parent
        var updateResult = parent.Update(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Address,
            status,
            command.Notes);

        if (updateResult.IsFailure)
            return updateResult;

        await _parentRepository.UpdateAsync(parent, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteParentCommand command, CancellationToken cancellationToken)
    {
        var parentId = ParentId.Create(command.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);

        if (parent is null)
            return Result.Failure("Parent not found");

        await _parentRepository.DeleteAsync(parent, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(LinkChildToParentCommand command, CancellationToken cancellationToken)
    {
        var parentId = ParentId.Create(command.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);

        if (parent is null)
            return Result.Failure("Parent not found");

        var childId = ChildId.Create(command.ChildId);
        var childExists = await _childRepository.ExistsAsync(childId, cancellationToken);

        if (!childExists)
            return Result.Failure("Child not found");

        var linkResult = parent.AddChild(childId);
        if (linkResult.IsFailure)
            return linkResult;

        await _parentRepository.UpdateAsync(parent, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(UnlinkChildFromParentCommand command, CancellationToken cancellationToken)
    {
        var parentId = ParentId.Create(command.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);

        if (parent is null)
            return Result.Failure("Parent not found");

        var childId = ChildId.Create(command.ChildId);
        var unlinkResult = parent.RemoveChild(childId);

        if (unlinkResult.IsFailure)
            return unlinkResult;

        await _parentRepository.UpdateAsync(parent, cancellationToken);
        return Result.Success();
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
