using FluentValidation;
using Daycare.Application.Features.Children.Commands;
using System.Text.RegularExpressions;

namespace Daycare.Application.Features.Children.Commands.Validators;

/// <summary>
/// Validator for CreateChildCommand.
/// </summary>
public sealed class CreateChildCommandValidator : AbstractValidator<CreateChildCommand>
{
    public CreateChildCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.UtcNow.AddYears(-150)).WithMessage("Date of birth seems invalid (older than 150 years)");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty().WithMessage("Enrollment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Enrollment date cannot be in the future");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500).WithMessage("Photo URL must not exceed 500 characters")
            .Matches(@"^https?://", RegexOptions.IgnoreCase).When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl))
            .WithMessage("Photo URL must be a valid HTTP(S) URL");

        RuleFor(x => x.Allergies)
            .MaximumLength(1000).WithMessage("Allergies description must not exceed 1000 characters");

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Medical notes must not exceed 2000 characters");
    }
}

/// <summary>
/// Validator for UpdateChildCommand.
/// </summary>
public sealed class UpdateChildCommandValidator : AbstractValidator<UpdateChildCommand>
{
    public UpdateChildCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.UtcNow.AddYears(-150)).WithMessage("Date of birth seems invalid (older than 150 years)");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500).WithMessage("Photo URL must not exceed 500 characters")
            .Matches(@"^https?://", RegexOptions.IgnoreCase).When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl))
            .WithMessage("Photo URL must be a valid HTTP(S) URL");

        RuleFor(x => x.Allergies)
            .MaximumLength(1000).WithMessage("Allergies description must not exceed 1000 characters");

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Medical notes must not exceed 2000 characters");
    }
}

/// <summary>
/// Validator for DeleteChildCommand.
/// </summary>
public sealed class DeleteChildCommandValidator : AbstractValidator<DeleteChildCommand>
{
    public DeleteChildCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for MarkChildInactiveCommand.
/// </summary>
public sealed class MarkChildInactiveCommandValidator : AbstractValidator<MarkChildInactiveCommand>
{
    public MarkChildInactiveCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for MarkChildWithdrawnCommand.
/// </summary>
public sealed class MarkChildWithdrawnCommandValidator : AbstractValidator<MarkChildWithdrawnCommand>
{
    public MarkChildWithdrawnCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for ReenrollChildCommand.
/// </summary>
public sealed class ReenrollChildCommandValidator : AbstractValidator<ReenrollChildCommand>
{
    public ReenrollChildCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}
