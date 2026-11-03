using FluentValidation;
using Daycare.Application.Features.ActivityLogs.Commands;

namespace Daycare.Application.Features.ActivityLogs.Validators;

public sealed class CreateActivityLogCommandValidator : AbstractValidator<CreateActivityLogCommand>
{
    public CreateActivityLogCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Activity title is required.")
            .MaximumLength(200).WithMessage("Activity title cannot exceed 200 characters.");

        RuleFor(x => x.ActivityType)
            .IsInEnum().WithMessage("A valid activity type is required.");

        RuleFor(x => x.Participation)
            .IsInEnum().WithMessage("A valid participation level is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0).When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration minutes cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class UpdateActivityLogCommandValidator : AbstractValidator<UpdateActivityLogCommand>
{
    public UpdateActivityLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Log ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Activity title is required.")
            .MaximumLength(200).WithMessage("Activity title cannot exceed 200 characters.");

        RuleFor(x => x.ActivityType)
            .IsInEnum().WithMessage("A valid activity type is required.");

        RuleFor(x => x.Participation)
            .IsInEnum().WithMessage("A valid participation level is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0).When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration minutes cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
