using FluentValidation;
using Daycare.Application.Features.NapLogs.Commands;

namespace Daycare.Application.Features.NapLogs.Validators;

public sealed class CreateNapLogCommandValidator : AbstractValidator<CreateNapLogCommand>
{
    public CreateNapLogCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.Quality)
            .IsInEnum().WithMessage("A valid nap quality is required.");

        RuleFor(x => x)
            .Must(x => !x.EndTime.HasValue || x.EndTime.Value >= x.StartTime)
            .WithMessage("End time cannot be earlier than start time.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class UpdateNapLogCommandValidator : AbstractValidator<UpdateNapLogCommand>
{
    public UpdateNapLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Log ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.Quality)
            .IsInEnum().WithMessage("A valid nap quality is required.");

        RuleFor(x => x)
            .Must(x => !x.EndTime.HasValue || x.EndTime.Value >= x.StartTime)
            .WithMessage("End time cannot be earlier than start time.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class EndNapLogCommandValidator : AbstractValidator<EndNapLogCommand>
{
    public EndNapLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Log ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.");

        RuleFor(x => x.Quality)
            .IsInEnum().When(x => x.Quality.HasValue).WithMessage("A valid nap quality is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
