using FluentValidation;
using Daycare.Application.Features.Dashboard.Queries;

namespace Daycare.Application.Features.Dashboard.Validators;

public sealed class GetStaffDashboardQueryValidator : AbstractValidator<GetStaffDashboardQuery>
{
    public GetStaffDashboardQueryValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required.");
    }
}

public sealed class GetParentDashboardQueryValidator : AbstractValidator<GetParentDashboardQuery>
{
    public GetParentDashboardQueryValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent ID is required.");
    }
}
