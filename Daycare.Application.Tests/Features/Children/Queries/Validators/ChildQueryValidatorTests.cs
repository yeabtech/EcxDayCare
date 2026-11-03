using FluentValidation.TestHelper;
using Daycare.Application.Features.Children.Queries;
using Daycare.Application.Features.Children.Queries.Validators;

namespace Daycare.Application.Tests.Features.Children.Queries.Validators;

public class GetAllChildrenQueryValidatorTests
{
    private readonly GetAllChildrenQueryValidator _validator;

    public GetAllChildrenQueryValidatorTests()
    {
        _validator = new GetAllChildrenQueryValidator();
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDefaultValues_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetAllChildrenQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNegativeSkip_ShouldHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: -1, Take: 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Skip)
            .WithErrorMessage("Skip must be greater than or equal to 0");
    }

    [Fact]
    public void Validate_WithZeroTake_ShouldHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: 0);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take)
            .WithErrorMessage("Take must be greater than 0");
    }

    [Fact]
    public void Validate_WithNegativeTake_ShouldHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: -1);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take);
    }

    [Fact]
    public void Validate_WithTakeExceeding100_ShouldHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: 101);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take)
            .WithErrorMessage("Take must not exceed 100");
    }

    [Fact]
    public void Validate_WithMaxAllowedTake_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: 100);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Take);
    }

    [Fact]
    public void Validate_WithHighSkipValue_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 1000, Take: 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Skip);
    }

    [Fact]
    public void Validate_WithZeroSkipAndMaxTake_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetAllChildrenQuery(Skip: 0, Take: 100);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class GetChildByIdQueryValidatorTests
{
    private readonly GetChildByIdQueryValidator _validator;

    public GetChildByIdQueryValidatorTests()
    {
        _validator = new GetChildByIdQueryValidator();
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetChildByIdQuery(Guid.NewGuid().ToString());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidGuidUpperCase_ShouldNotHaveErrors()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString().ToUpper();
        var query = new GetChildByIdQuery(guid);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyChildId_ShouldHaveError()
    {
        // Arrange
        var query = new GetChildByIdQuery("");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID is required");
    }

    [Fact]
    public void Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        var query = new GetChildByIdQuery("invalid-guid");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID must be a valid GUID");
    }

    [Fact]
    public void Validate_WithPartialGuid_ShouldHaveError()
    {
        // Arrange
        var query = new GetChildByIdQuery(Guid.NewGuid().ToString().Substring(0, 8));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId);
    }

    [Fact]
    public void Validate_WithNumericString_ShouldHaveError()
    {
        // Arrange
        var query = new GetChildByIdQuery("12345678901234567890123456789012");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId);
    }

    [Fact]
    public void Validate_WithSpecialCharacters_ShouldHaveError()
    {
        // Arrange
        var query = new GetChildByIdQuery("!@#$%^&*()_+-=[]{}|;:',.<>?/");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId);
    }
}

public class ChildExistsQueryValidatorTests
{
    private readonly ChildExistsQueryValidator _validator;

    public ChildExistsQueryValidatorTests()
    {
        _validator = new ChildExistsQueryValidator();
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new ChildExistsQuery(Guid.NewGuid().ToString());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidGuidUpperCase_ShouldNotHaveErrors()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString().ToUpper();
        var query = new ChildExistsQuery(guid);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyChildId_ShouldHaveError()
    {
        // Arrange
        var query = new ChildExistsQuery("");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID is required");
    }

    [Fact]
    public void Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        var query = new ChildExistsQuery("not-a-guid");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID must be a valid GUID");
    }

    [Fact]
    public void Validate_WithNullChildId_ShouldHaveError()
    {
        // Arrange
        var query = new ChildExistsQuery(null!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId);
    }
}
