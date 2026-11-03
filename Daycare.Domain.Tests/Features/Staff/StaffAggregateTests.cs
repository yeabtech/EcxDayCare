using Daycare.Domain.Entities;
using Xunit;

namespace Daycare.Domain.Tests.Features.Staff;

/// <summary>
/// Unit tests for Staff entity and domain invariants
/// </summary>
public class StaffAggregateTests
{
    private readonly DateTime _validHireDate = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private readonly TimeOnly _validShiftStart = new(8, 0);
    private readonly TimeOnly _validShiftEnd = new(17, 0);

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange & Act
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice.smith@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            null,
            "CPR Certified");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Alice", result.Value.FirstName);
        Assert.Equal("Smith", result.Value.LastName);
        Assert.Equal(StaffRole.Teacher, result.Value.Role);
        Assert.Equal("555-234-5678", result.Value.Phone);
        Assert.Equal("alice.smith@daycare.com", result.Value.Email);
        Assert.Equal(_validHireDate, result.Value.HireDate);
        Assert.Equal(_validShiftStart, result.Value.ShiftStart);
        Assert.Equal(_validShiftEnd, result.Value.ShiftEnd);
        Assert.Null(result.Value.ClassroomId);
        Assert.Equal("CPR Certified", result.Value.Certifications);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ReturnsFailure(string? firstName)
    {
        var result = Domain.Entities.Staff.Create(
            firstName!,
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("First name is required", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_ReturnsFailure(string? lastName)
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            lastName!,
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("Last name is required", result.Error);
    }

    [Fact]
    public void Create_WithTooLongFirstName_ReturnsFailure()
    {
        var longName = new string('A', 101);
        var result = Domain.Entities.Staff.Create(
            longName,
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("First name must not exceed 100 characters", result.Error);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Create_WithInvalidEmail_ReturnsFailure(string email)
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            email,
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid email address format", result.Error);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("not-a-phone-number")]
    public void Create_WithInvalidPhone_ReturnsFailure(string phone)
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            phone,
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid phone number format", result.Error);
    }

    [Fact]
    public void Create_WithShiftEndBeforeStart_ReturnsFailure()
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            new TimeOnly(17, 0),
            new TimeOnly(8, 0));

        Assert.True(result.IsFailure);
        Assert.Contains("Shift end time must be after shift start time", result.Error);
    }

    [Fact]
    public void Create_WithEqualShiftStartAndEnd_ReturnsFailure()
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            new TimeOnly(8, 0),
            new TimeOnly(8, 0));

        Assert.True(result.IsFailure);
        Assert.Contains("Shift end time must be after shift start time", result.Error);
    }

    [Fact]
    public void Create_WithHireDateInPastCentury_ReturnsFailure()
    {
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            new DateTime(1940, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _validShiftStart,
            _validShiftEnd);

        Assert.True(result.IsFailure);
        Assert.Contains("Hire date cannot be earlier than 1950", result.Error);
    }

    [Fact]
    public void Create_WithCertificationsTooLong_ReturnsFailure()
    {
        var longCert = new string('C', 501);
        var result = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            null,
            longCert);

        Assert.True(result.IsFailure);
        Assert.Contains("Certifications must not exceed 500 characters", result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesAllFields()
    {
        // Arrange
        var staff = Domain.Entities.Staff.Create(
            "Alice",
            "Smith",
            StaffRole.Teacher,
            "555-234-5678",
            "alice@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var newHireDate = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var newClassroomId = ClassroomId.Create();

        // Act
        var result = staff.Update(
            "Alicia",
            "Johnson",
            StaffRole.LeadTeacher,
            "555-999-8888",
            "alicia.johnson@daycare.com",
            newHireDate,
            new TimeOnly(7, 30),
            new TimeOnly(16, 30),
            newClassroomId,
            "Updated Certifications");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Alicia", staff.FirstName);
        Assert.Equal("Johnson", staff.LastName);
        Assert.Equal(StaffRole.LeadTeacher, staff.Role);
        Assert.Equal("555-999-8888", staff.Phone);
        Assert.Equal("alicia.johnson@daycare.com", staff.Email);
        Assert.Equal(newHireDate, staff.HireDate);
        Assert.Equal(new TimeOnly(7, 30), staff.ShiftStart);
        Assert.Equal(new TimeOnly(16, 30), staff.ShiftEnd);
        Assert.Equal(newClassroomId, staff.ClassroomId);
        Assert.Equal("Updated Certifications", staff.Certifications);
    }

    [Fact]
    public void AssignToClassroom_WithValidClassroomId_SetsClassroomId()
    {
        // Arrange
        var staff = Domain.Entities.Staff.Create(
            "Bob",
            "Brown",
            StaffRole.Aide,
            "555-333-4444",
            "bob@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var classroomId = ClassroomId.Create();

        // Act
        var result = staff.AssignToClassroom(classroomId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(classroomId, staff.ClassroomId);
    }

    [Fact]
    public void AssignToClassroom_WithNullClassroomId_ReturnsFailure()
    {
        var staff = Domain.Entities.Staff.Create(
            "Bob",
            "Brown",
            StaffRole.Aide,
            "555-333-4444",
            "bob@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var result = staff.AssignToClassroom(null!);

        Assert.True(result.IsFailure);
        Assert.Contains("Classroom ID is required", result.Error);
    }

    [Fact]
    public void RemoveFromClassroom_ClearsClassroomId()
    {
        // Arrange
        var classroomId = ClassroomId.Create();
        var staff = Domain.Entities.Staff.Create(
            "Bob",
            "Brown",
            StaffRole.Aide,
            "555-333-4444",
            "bob@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            classroomId).Value!;

        // Act
        var result = staff.RemoveFromClassroom();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(staff.ClassroomId);
    }

    [Fact]
    public void UpdateRole_WithValidRole_UpdatesRole()
    {
        var staff = Domain.Entities.Staff.Create(
            "Carol",
            "Davis",
            StaffRole.Assistant,
            "555-444-5555",
            "carol@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var result = staff.UpdateRole(StaffRole.Director);

        Assert.True(result.IsSuccess);
        Assert.Equal(StaffRole.Director, staff.Role);
    }

    [Fact]
    public void UpdateShift_WithValidTimes_UpdatesShift()
    {
        var staff = Domain.Entities.Staff.Create(
            "David",
            "Evans",
            StaffRole.Teacher,
            "555-555-6666",
            "david@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var newStart = new TimeOnly(9, 0);
        var newEnd = new TimeOnly(18, 0);

        var result = staff.UpdateShift(newStart, newEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, staff.ShiftStart);
        Assert.Equal(newEnd, staff.ShiftEnd);
    }
}
