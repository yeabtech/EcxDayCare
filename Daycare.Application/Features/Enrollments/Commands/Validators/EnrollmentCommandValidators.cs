using FluentValidation;
using Daycare.Application.Features.Enrollments.Commands;
using Daycare.Domain.Entities;

namespace Daycare.Application.Features.Enrollments.Commands.Validators;

public sealed class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
{
    public CreateEnrollmentCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.ProgramType)
            .NotEmpty().WithMessage("Program type is required.")
            .Must(BeAValidProgramType).WithMessage("Invalid program type.");

        RuleFor(x => x.DaysOfWeek)
            .NotEmpty().WithMessage("Days of week must be specified.")
            .MaximumLength(100).WithMessage("Days of week cannot exceed 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .Must((cmd, endDate) => !endDate.HasValue || endDate.Value.Date >= cmd.StartDate.Date)
            .WithMessage("End date cannot be earlier than start date.");
    }

    private static bool BeAValidProgramType(string programType)
    {
        return Enum.TryParse<ProgramType>(programType, true, out _);
    }
}

public sealed class UpdateEnrollmentCommandValidator : AbstractValidator<UpdateEnrollmentCommand>
{
    public UpdateEnrollmentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required.");

        RuleFor(x => x.ProgramType)
            .NotEmpty().WithMessage("Program type is required.")
            .Must(BeAValidProgramType).WithMessage("Invalid program type.");

        RuleFor(x => x.DaysOfWeek)
            .NotEmpty().WithMessage("Days of week must be specified.")
            .MaximumLength(100).WithMessage("Days of week cannot exceed 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .Must((cmd, endDate) => !endDate.HasValue || endDate.Value.Date >= cmd.StartDate.Date)
            .WithMessage("End date cannot be earlier than start date.");

        RuleFor(x => x.Status)
            .Must(status => status == null ||
                (Enum.TryParse<EnrollmentStatus>(status, true, out var parsed) && Enum.IsDefined(parsed)))
            .WithMessage("Invalid enrollment status.");

        RuleFor(x => x.EndDate)
            .NotNull().When(x => string.Equals(x.Status, nameof(EnrollmentStatus.Ended), StringComparison.OrdinalIgnoreCase))
            .WithMessage("End date is required for an ended enrollment.");
    }

    private static bool BeAValidProgramType(string programType)
    {
        return Enum.TryParse<ProgramType>(programType, true, out _);
    }
}

public sealed class EndEnrollmentCommandValidator : AbstractValidator<EndEnrollmentCommand>
{
    public EndEnrollmentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.");
    }
}
