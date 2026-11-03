using FluentValidation;
using Daycare.Application.Features.Staff.Queries;

namespace Daycare.Application.Features.Staff.Queries.Validators;

public sealed class GetAllStaffQueryValidator : AbstractValidator<GetAllStaffQuery>
{
    public GetAllStaffQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }
}

public sealed class SearchStaffQueryValidator : AbstractValidator<SearchStaffQuery>
{
    public SearchStaffQueryValidator()
    {
        RuleFor(x => x.Term)
            .NotEmpty().WithMessage("Search term is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }
}

public sealed class GetStaffByIdQueryValidator : AbstractValidator<GetStaffByIdQuery>
{
    public GetStaffByIdQueryValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");
    }
}

public sealed class GetStaffByClassroomQueryValidator : AbstractValidator<GetStaffByClassroomQuery>
{
    public GetStaffByClassroomQueryValidator()
    {
        RuleFor(x => x.ClassroomId)
            .NotEmpty().WithMessage("Classroom ID is required");
    }
}
