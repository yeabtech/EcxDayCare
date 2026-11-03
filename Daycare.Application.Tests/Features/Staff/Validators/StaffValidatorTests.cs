using Daycare.Application.Features.Staff.Commands;
using Daycare.Application.Features.Staff.Commands.Validators;
using Daycare.Application.Features.Staff.Queries;
using Daycare.Application.Features.Staff.Queries.Validators;

namespace Daycare.Application.Tests.Features.Staff.Validators;

public class StaffValidatorTests
{
    private readonly CreateStaffCommandValidator _createValidator = new();
    private readonly UpdateStaffCommandValidator _updateValidator = new();
    private readonly GetAllStaffQueryValidator _getAllQueryValidator = new();
    private readonly SearchStaffQueryValidator _searchQueryValidator = new();

    private readonly DateTime _validHireDate = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private readonly TimeOnly _validShiftStart = new(8, 0);
    private readonly TimeOnly _validShiftEnd = new(17, 0);

    [Fact]
    public void CreateStaffCommandValidator_WithValidCommand_IsValid()
    {
        var command = new CreateStaffCommand(
            "John",
            "Doe",
            "Teacher",
            "555-123-4567",
            "john.doe@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStaffCommandValidator_WithEmptyFirstName_IsInvalid(string firstName)
    {
        var command = new CreateStaffCommand(
            firstName,
            "Doe",
            "Teacher",
            "555-123-4567",
            "john.doe@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("invalid-role")]
    [InlineData("Astronaut")]
    public void CreateStaffCommandValidator_WithInvalidRole_IsInvalid(string role)
    {
        var command = new CreateStaffCommand(
            "John",
            "Doe",
            role,
            "555-123-4567",
            "john.doe@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }

    [Fact]
    public void CreateStaffCommandValidator_WithShiftEndBeforeStart_IsInvalid()
    {
        var command = new CreateStaffCommand(
            "John",
            "Doe",
            "Teacher",
            "555-123-4567",
            "john.doe@daycare.com",
            _validHireDate,
            new TimeOnly(17, 0),
            new TimeOnly(8, 0));

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ShiftEnd");
    }

    [Fact]
    public void UpdateStaffCommandValidator_WithEmptyStaffId_IsInvalid()
    {
        var command = new UpdateStaffCommand(
            Guid.Empty,
            "John",
            "Doe",
            "Teacher",
            "555-123-4567",
            "john.doe@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StaffId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetAllStaffQueryValidator_WithInvalidPage_IsInvalid(int page)
    {
        var query = new GetAllStaffQuery(page, 10);
        var result = _getAllQueryValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void GetAllStaffQueryValidator_WithInvalidPageSize_IsInvalid(int pageSize)
    {
        var query = new GetAllStaffQuery(1, pageSize);
        var result = _getAllQueryValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchStaffQueryValidator_WithEmptyTerm_IsInvalid(string term)
    {
        var query = new SearchStaffQuery(term);
        var result = _searchQueryValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Term");
    }
}
