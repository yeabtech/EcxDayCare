using FluentValidation;
using Daycare.Application.Features.FeedingLogs.Commands;

namespace Daycare.Application.Features.FeedingLogs.Validators;

public sealed class CreateFeedingLogCommandValidator : AbstractValidator<CreateFeedingLogCommand>
{
    public CreateFeedingLogCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.FeedingType)
            .IsInEnum().WithMessage("A valid feeding type is required.");

        RuleFor(x => x.PortionConsumed)
            .IsInEnum().WithMessage("A valid portion consumed value is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.FoodItems)
            .MaximumLength(500).WithMessage("Food items description cannot exceed 500 characters.");

        RuleFor(x => x.Amount)
            .MaximumLength(100).WithMessage("Amount cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class UpdateFeedingLogCommandValidator : AbstractValidator<UpdateFeedingLogCommand>
{
    public UpdateFeedingLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Log ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.FeedingType)
            .IsInEnum().WithMessage("A valid feeding type is required.");

        RuleFor(x => x.PortionConsumed)
            .IsInEnum().WithMessage("A valid portion consumed value is required.");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required.");

        RuleFor(x => x.FoodItems)
            .MaximumLength(500).WithMessage("Food items description cannot exceed 500 characters.");

        RuleFor(x => x.Amount)
            .MaximumLength(100).WithMessage("Amount cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
