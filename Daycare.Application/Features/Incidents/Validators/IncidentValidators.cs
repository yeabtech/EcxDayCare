using FluentValidation;
using Daycare.Application.Features.Incidents.Commands;

namespace Daycare.Application.Features.Incidents.Validators;

public sealed class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.IncidentTime)
            .NotEmpty().WithMessage("Incident time is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Incident location is required.")
            .MaximumLength(255).WithMessage("Location cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Incident description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.InjuryType)
            .IsInEnum().WithMessage("A valid injury type is required.");

        RuleFor(x => x.AffectedBodyPart)
            .IsInEnum().WithMessage("A valid body part is required.");

        RuleFor(x => x.FirstAidGiven)
            .MaximumLength(500).WithMessage("First aid description cannot exceed 500 characters.");

        RuleFor(x => x.FollowUp)
            .MaximumLength(1000).WithMessage("Follow-up cannot exceed 1000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class UpdateIncidentCommandValidator : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.IncidentTime)
            .NotEmpty().WithMessage("Incident time is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Incident location is required.")
            .MaximumLength(255).WithMessage("Location cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Incident description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.InjuryType)
            .IsInEnum().WithMessage("A valid injury type is required.");

        RuleFor(x => x.AffectedBodyPart)
            .IsInEnum().WithMessage("A valid body part is required.");

        RuleFor(x => x.FirstAidGiven)
            .MaximumLength(500).WithMessage("First aid description cannot exceed 500 characters.");

        RuleFor(x => x.FollowUp)
            .MaximumLength(1000).WithMessage("Follow-up cannot exceed 1000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class NotifyParentCommandValidator : AbstractValidator<NotifyParentCommand>
{
    public NotifyParentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}

public sealed class RecordParentSignatureCommandValidator : AbstractValidator<RecordParentSignatureCommand>
{
    public RecordParentSignatureCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required.")
            .MaximumLength(5000).WithMessage("Signature cannot exceed 5000 characters.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}

public sealed class RecordFollowUpCommandValidator : AbstractValidator<RecordFollowUpCommand>
{
    public RecordFollowUpCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.FollowUpDetails)
            .NotEmpty().WithMessage("Follow-up details are required.")
            .MaximumLength(2000).WithMessage("Follow-up details cannot exceed 2000 characters.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}

public sealed class DeleteIncidentCommandValidator : AbstractValidator<DeleteIncidentCommand>
{
    public DeleteIncidentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}
