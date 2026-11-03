using System.Text.RegularExpressions;
using FluentValidation;
using Daycare.Application.Features.Staff.Commands;
using Daycare.Domain.Entities;

namespace Daycare.Application.Features.Staff.Commands.Validators;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    private static readonly Regex PhoneRegex = new(
        @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$",
        RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeAValidRole).WithMessage("Invalid staff role. Allowed values: LeadTeacher, Teacher, Assistant, Aide, Director, Admin");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(PhoneRegex).WithMessage("Invalid phone number format");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required")
            .Matches(EmailRegex).WithMessage("Invalid email address format");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .GreaterThan(new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc)).WithMessage("Hire date cannot be earlier than 1950")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Hire date cannot be more than 1 year in the future");

        RuleFor(x => x.ShiftEnd)
            .GreaterThan(x => x.ShiftStart).WithMessage("Shift end time must be after shift start time");

        RuleFor(x => x.Certifications)
            .MaximumLength(500).WithMessage("Certifications must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Certifications));
    }

    private static bool BeAValidRole(string role)
    {
        return Enum.TryParse<StaffRole>(role, true, out _);
    }
}

public sealed class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    private static readonly Regex PhoneRegex = new(
        @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$",
        RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeAValidRole).WithMessage("Invalid staff role. Allowed values: LeadTeacher, Teacher, Assistant, Aide, Director, Admin");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(PhoneRegex).WithMessage("Invalid phone number format");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required")
            .Matches(EmailRegex).WithMessage("Invalid email address format");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .GreaterThan(new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc)).WithMessage("Hire date cannot be earlier than 1950")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Hire date cannot be more than 1 year in the future");

        RuleFor(x => x.ShiftEnd)
            .GreaterThan(x => x.ShiftStart).WithMessage("Shift end time must be after shift start time");

        RuleFor(x => x.Certifications)
            .MaximumLength(500).WithMessage("Certifications must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Certifications));
    }

    private static bool BeAValidRole(string role)
    {
        return Enum.TryParse<StaffRole>(role, true, out _);
    }
}

public sealed class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");
    }
}

public sealed class AssignStaffToClassroomCommandValidator : AbstractValidator<AssignStaffToClassroomCommand>
{
    public AssignStaffToClassroomCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");
    }
}
