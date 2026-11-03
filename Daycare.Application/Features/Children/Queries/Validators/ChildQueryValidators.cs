using FluentValidation;
using Daycare.Application.Features.Children.Queries;
using System.Text.RegularExpressions;

namespace Daycare.Application.Features.Children.Queries.Validators;

/// <summary>
/// Validator for GetAllChildrenQuery.
/// </summary>
public sealed class GetAllChildrenQueryValidator : AbstractValidator<GetAllChildrenQuery>
{
    public GetAllChildrenQueryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.Take)
            .GreaterThan(0).WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Take must not exceed 100");
    }
}

/// <summary>
/// Validator for GetChildByIdQuery.
/// </summary>
public sealed class GetChildByIdQueryValidator : AbstractValidator<GetChildByIdQuery>
{
    public GetChildByIdQueryValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for ChildExistsQuery.
/// </summary>
public sealed class ChildExistsQueryValidator : AbstractValidator<ChildExistsQuery>
{
    public ChildExistsQueryValidator()
    {
        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Child ID is required")
            .Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)
            .WithMessage("Child ID must be a valid GUID");
    }
}
