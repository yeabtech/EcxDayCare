using FluentValidation;
using Daycare.Application.Features.Attendance.Commands;
using Daycare.Domain.Entities;

namespace Daycare.Application.Features.Attendance.Commands.Validators;

public sealed class CheckInChildCommandValidator : AbstractValidator<CheckInChildCommand>
{
    public CheckInChildCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.DroppedOffBy)
            .MaximumLength(150).WithMessage("Dropped off by cannot exceed 150 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<AttendanceStatus>(status, true, out _))
            .WithMessage("Invalid attendance status.");
    }
}

public sealed class CheckOutChildCommandValidator : AbstractValidator<CheckOutChildCommand>
{
    public CheckOutChildCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.AttendanceId.HasValue && x.AttendanceId.Value != Guid.Empty) || (x.ChildId.HasValue && x.ChildId.Value != Guid.Empty))
            .WithMessage("Either Attendance ID or Child ID must be provided to check out.");

        RuleFor(x => x.PickedUpBy)
            .MaximumLength(150).WithMessage("Picked up by cannot exceed 150 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}

public sealed class MarkChildAbsentCommandValidator : AbstractValidator<MarkChildAbsentCommand>
{
    public MarkChildAbsentCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}
