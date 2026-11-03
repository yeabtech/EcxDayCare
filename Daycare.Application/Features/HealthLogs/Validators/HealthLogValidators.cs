using FluentValidation;
using Daycare.Application.Features.HealthLogs.Commands;

namespace Daycare.Application.Features.HealthLogs.Validators;

public sealed class CreateHealthLogCommandValidator : AbstractValidator<CreateHealthLogCommand>
{
    public CreateHealthLogCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("A valid health event type is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class UpdateHealthLogCommandValidator : AbstractValidator<UpdateHealthLogCommand>
{
    public UpdateHealthLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Health Log ID is required.");

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("A valid health event type is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class DeleteHealthLogCommandValidator : AbstractValidator<DeleteHealthLogCommand>
{
    public DeleteHealthLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Health Log ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}
