using MediatR;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;
using Daycare.Domain.Repositories;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Children.Commands;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Children.Commands.Handlers;

/// <summary>
/// Handler for CreateChildCommand.
/// </summary>
public sealed class CreateChildCommandHandler : IRequestHandler<CreateChildCommand, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public CreateChildCommandHandler(IChildRepository childRepository, IParentRepository parentRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    public async Task<Result<ChildDto>> Handle(CreateChildCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Resolve and validate the requested parents (mothers/fathers) before writing anything
            var linksResult = await ParentLinkResolver.ResolveAsync(_parentRepository, request.Parents, cancellationToken);
            if (linksResult.IsFailure)
                return Result<ChildDto>.Failure(linksResult.Error!, linksResult.ErrorCode);

            var name = new PersonName(request.FirstName, request.LastName);
            var dateOfBirth = new DateOfBirth(request.DateOfBirth);

            var child = new Child(
                ChildId.New(),
                name,
                dateOfBirth,
                request.Gender,
                request.EnrollmentDate,
                request.PhotoUrl,
                request.Allergies,
                request.MedicalNotes);

            await _childRepository.AddAsync(child, cancellationToken);

            // Persist the ParentChildren rows (role = Mother / Father / Guardian)
            await _childRepository.ReplaceParentLinksAsync(child.Id, linksResult.Value!, cancellationToken);

            var dto = MapToDto(child);
            return Result<ChildDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            return Result<ChildDto>.Failure(ex.Message, "INVALID_CHILD_DATA");
        }
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
/// Handler for UpdateChildCommand.
/// </summary>
public sealed class UpdateChildCommandHandler : IRequestHandler<UpdateChildCommand, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public UpdateChildCommandHandler(IChildRepository childRepository, IParentRepository parentRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    public async Task<Result<ChildDto>> Handle(UpdateChildCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(request.ChildId, out var childIdGuid))
                return Result<ChildDto>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

            var childId = new ChildId(childIdGuid);
            var child = await _childRepository.GetByIdAsync(childId, cancellationToken);

            if (child is null)
                return Result<ChildDto>.Failure("Child not found", "CHILD_NOT_FOUND");

            var name = new PersonName(request.FirstName, request.LastName);
            var dateOfBirth = new DateOfBirth(request.DateOfBirth);

            // Validate parent assignments up-front (only replace links when the caller sent them)
            Result<IReadOnlyList<ParentChildLink>>? linksResult = null;
            if (request.Parents is not null)
            {
                linksResult = await ParentLinkResolver.ResolveAsync(_parentRepository, request.Parents, cancellationToken);
                if (linksResult.IsFailure)
                    return Result<ChildDto>.Failure(linksResult.Error!, linksResult.ErrorCode);
            }

            child.UpdateInfo(
                name,
                dateOfBirth,
                request.Gender,
                request.PhotoUrl,
                request.Allergies,
                request.MedicalNotes);

            // Apply the enrollment status only when the caller provided one.
            if (request.Status is not null)
                child.UpdateStatus(request.Status.Value);

            // Apply the enrollment date only when the caller provided one.
            if (request.EnrollmentDate is not null)
                child.UpdateEnrollmentDate(request.EnrollmentDate.Value);

            await _childRepository.UpdateAsync(child, cancellationToken);

            if (linksResult is not null)
                await _childRepository.ReplaceParentLinksAsync(child.Id, linksResult.Value!, cancellationToken);

            var dto = MapToDto(child);
            return Result<ChildDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            return Result<ChildDto>.Failure(ex.Message, "INVALID_CHILD_DATA");
        }
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
/// Handler for DeleteChildCommand.
/// </summary>
public sealed class DeleteChildCommandHandler : IRequestHandler<DeleteChildCommand, Result>
{
    private readonly IChildRepository _childRepository;

    public DeleteChildCommandHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result> Handle(DeleteChildCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result.Failure("Invalid child ID format");

        var childId = new ChildId(childIdGuid);
        var exists = await _childRepository.ExistsAsync(childId, cancellationToken);

        if (!exists)
            return Result.Failure("Child not found");

        await _childRepository.DeleteAsync(childId, cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Handler for MarkChildInactiveCommand.
/// </summary>
public sealed class MarkChildInactiveCommandHandler : IRequestHandler<MarkChildInactiveCommand, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;

    public MarkChildInactiveCommandHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ChildDto>> Handle(MarkChildInactiveCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<ChildDto>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);

        if (child is null)
            return Result<ChildDto>.Failure("Child not found", "CHILD_NOT_FOUND");

        try
        {
            child.MarkInactive();
            await _childRepository.UpdateAsync(child, cancellationToken);
            var dto = MapToDto(child);
            return Result<ChildDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ChildDto>.Failure(ex.Message, "INVALID_CHILD_STATUS");
        }
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
/// Handler for MarkChildWithdrawnCommand.
/// </summary>
public sealed class MarkChildWithdrawnCommandHandler : IRequestHandler<MarkChildWithdrawnCommand, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;

    public MarkChildWithdrawnCommandHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ChildDto>> Handle(MarkChildWithdrawnCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<ChildDto>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);

        if (child is null)
            return Result<ChildDto>.Failure("Child not found", "CHILD_NOT_FOUND");

        try
        {
            child.MarkWithdrawn();
            await _childRepository.UpdateAsync(child, cancellationToken);
            var dto = MapToDto(child);
            return Result<ChildDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ChildDto>.Failure(ex.Message, "INVALID_CHILD_STATUS");
        }
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
/// Handler for ReenrollChildCommand.
/// </summary>
public sealed class ReenrollChildCommandHandler : IRequestHandler<ReenrollChildCommand, Result<ChildDto>>
{
    private readonly IChildRepository _childRepository;

    public ReenrollChildCommandHandler(IChildRepository childRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ChildDto>> Handle(ReenrollChildCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ChildId, out var childIdGuid))
            return Result<ChildDto>.Failure("Invalid child ID format", "INVALID_ID_FORMAT");

        var childId = new ChildId(childIdGuid);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);

        if (child is null)
            return Result<ChildDto>.Failure("Child not found", "CHILD_NOT_FOUND");

        try
        {
            child.Reenroll();
            await _childRepository.UpdateAsync(child, cancellationToken);
            var dto = MapToDto(child);
            return Result<ChildDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ChildDto>.Failure(ex.Message, "INVALID_CHILD_STATUS");
        }
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
