using FluentValidation.TestHelper;
using Daycare.Application.Features.Children.Commands;
using Daycare.Application.Features.Children.Commands.Validators;

namespace Daycare.Application.Tests.Features.Children.Commands.Validators;

public class CreateChildCommandValidatorTests
{
    private readonly CreateChildCommandValidator _validator;

    public CreateChildCommandValidatorTests()
    {
        _validator = new CreateChildCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: "https://example.com/photo.jpg",
            Allergies: "None",
            MedicalNotes: "None");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Validate_WithFirstNameTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: new string('a', 101),
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithEmptyLastName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Validate_WithLastNameTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: new string('a', 101),
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_WithFutureDateOfBirth_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(1),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth must be in the past");
    }

    [Fact]
    public void Validate_WithFutureDateOfBirthMoreThan150YearsAgo_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-151),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WithFutureEnrollmentDate_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnrollmentDate)
            .WithErrorMessage("Enrollment date cannot be in the future");
    }

    [Fact]
    public void Validate_WithInvalidPhotoUrl_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: "not-a-url");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhotoUrl);
    }

    [Fact]
    public void Validate_WithValidHttpsPhotoUrl_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: "https://example.com/photo.jpg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhotoUrl);
    }

    [Fact]
    public void Validate_WithValidHttpPhotoUrl_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: "http://example.com/photo.jpg");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhotoUrl);
    }

    [Fact]
    public void Validate_WithNullPhotoUrl_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhotoUrl);
    }

    [Fact]
    public void Validate_WithAllergiesExceeding1000Chars_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            Allergies: new string('a', 1001));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Allergies);
    }

    [Fact]
    public void Validate_WithMedicalNotesExceeding2000Chars_ShouldHaveError()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            MedicalNotes: new string('a', 2001));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MedicalNotes);
    }
}

public class UpdateChildCommandValidatorTests
{
    private readonly UpdateChildCommandValidator _validator;

    public UpdateChildCommandValidatorTests()
    {
        _validator = new UpdateChildCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateChildCommand(
            ChildId: Guid.NewGuid().ToString(),
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male,
            PhotoUrl: "https://example.com/photo.jpg",
            Allergies: "None",
            MedicalNotes: "None");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyChildId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateChildCommand(
            ChildId: "",
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID is required");
    }

    [Fact]
    public void Validate_WithInvalidChildIdFormat_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateChildCommand(
            ChildId: "invalid-id",
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID must be a valid GUID");
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldNotHaveError()
    {
        // Arrange
        var validGuid = Guid.NewGuid().ToString();
        var command = new UpdateChildCommand(
            ChildId: validGuid,
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChildId);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateChildCommand(
            ChildId: Guid.NewGuid().ToString(),
            FirstName: "",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Daycare.Domain.Entities.Gender.Male);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}

public class DeleteChildCommandValidatorTests
{
    private readonly DeleteChildCommandValidator _validator;

    public DeleteChildCommandValidatorTests()
    {
        _validator = new DeleteChildCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new DeleteChildCommand(Guid.NewGuid().ToString());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyChildId_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteChildCommand("");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID is required");
    }

    [Fact]
    public void Validate_WithInvalidChildIdFormat_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteChildCommand("invalid-id");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId)
            .WithErrorMessage("Child ID must be a valid GUID");
    }
}

public class MarkChildInactiveCommandValidatorTests
{
    private readonly MarkChildInactiveCommandValidator _validator;

    public MarkChildInactiveCommandValidatorTests()
    {
        _validator = new MarkChildInactiveCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new MarkChildInactiveCommand(Guid.NewGuid().ToString());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidChildIdFormat_ShouldHaveError()
    {
        // Arrange
        var command = new MarkChildInactiveCommand("invalid-id");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChildId);
    }
}
