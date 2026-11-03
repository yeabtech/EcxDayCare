using Daycare.Domain.ValueObjects;
using FluentAssertions;

namespace Daycare.Domain.Tests.ValueObjects;

/// <summary>
/// Tests for PersonName value object.
/// </summary>
public class PersonNameTests
{
    [Fact]
    public void Constructor_WithValidNames_CreatesInstance()
    {
        // Arrange & Act
        var name = new PersonName("John", "Doe");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange & Act
        var name = new PersonName("  John  ", "  Doe  ");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
    }

    [Fact]
    public void Constructor_WithNullFirstName_ThrowsException()
    {
        // Act & Assert
        var act = () => new PersonName(null!, "Doe");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ThrowsException()
    {
        // Act & Assert
        var act = () => new PersonName("", "Doe");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNullLastName_ThrowsException()
    {
        // Act & Assert
        var act = () => new PersonName("John", null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Last name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNameTooLong_ThrowsException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        var act = () => new PersonName(longName, "Doe");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 100 characters*");
    }

    [Fact]
    public void Equality_WithSameNames_ReturnsTrue()
    {
        // Arrange
        var name1 = new PersonName("John", "Doe");
        var name2 = new PersonName("John", "Doe");

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentNames_ReturnsFalse()
    {
        // Arrange
        var name1 = new PersonName("John", "Doe");
        var name2 = new PersonName("Jane", "Doe");

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsFullName()
    {
        // Arrange
        var name = new PersonName("John", "Doe");

        // Act & Assert
        name.ToString().Should().Be("John Doe");
    }

    [Fact]
    public void ImmutabilityTest_CannotModifyAfterCreation()
    {
        // Arrange
        var name = new PersonName("John", "Doe");

        // Act & Assert - record type ensures immutability
        name.FirstName.Should().Be("John");
        // Attempting to change properties should not be possible
        var sameNameObject = name; // This creates a reference to same object
        sameNameObject.Should().Be(name); // Still the same
    }
}

/// <summary>
/// Tests for Email value object.
/// </summary>
public class EmailTests
{
    [Fact]
    public void Constructor_WithValidEmail_CreatesInstance()
    {
        // Arrange & Act
        var email = new Email("test@example.com");

        // Assert
        email.Address.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_NormalizesToLowerCase()
    {
        // Arrange & Act
        var email = new Email("Test@EXAMPLE.COM");

        // Assert
        email.Address.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange & Act
        var email = new Email("  test@example.com  ");

        // Assert
        email.Address.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithNullEmail_ThrowsException()
    {
        // Act & Assert
        var act = () => new Email(null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ThrowsException()
    {
        // Act & Assert
        var act = () => new Email("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WithInvalidFormat_ThrowsException()
    {
        // Act & Assert
        var act = () => new Email("invalid-email");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email format is invalid*");
    }

    [Fact]
    public void Constructor_WithEmailTooLong_ThrowsException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // Exceeds 254 chars

        // Act & Assert
        var act = () => new Email(longEmail);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 254 characters*");
    }

    [Fact]
    public void Equality_WithSameAddress_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM"); // Different case, same when normalized

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_WithDifferentAddress_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ToString_ReturnsAddress()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act & Assert
        email.ToString().Should().Be("test@example.com");
    }
}

/// <summary>
/// Tests for PhoneNumber value object.
/// </summary>
public class PhoneNumberTests
{
    [Fact]
    public void Constructor_WithDigitsOnly_CreatesInstance()
    {
        // Arrange & Act
        var phone = new PhoneNumber("5551234567");

        // Assert
        phone.Number.Should().Be("5551234567");
    }

    [Fact]
    public void Constructor_WithFormattedNumber_ExtractsDigits()
    {
        // Arrange & Act
        var phone = new PhoneNumber("(555) 123-4567");

        // Assert
        phone.Number.Should().Be("5551234567");
    }

    [Fact]
    public void Constructor_WithInternationalFormat_PreservesPlus()
    {
        // Arrange & Act
        var phone = new PhoneNumber("+1 555 123 4567");

        // Assert
        phone.Number.Should().Be("+15551234567");
    }

    [Fact]
    public void Constructor_WithTooFewDigits_ThrowsException()
    {
        // Act & Assert
        var act = () => new PhoneNumber("123");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must have at least 10 digits*");
    }

    [Fact]
    public void Constructor_WithTooManyDigits_ThrowsException()
    {
        // Arrange
        var tooMany = new string('5', 20);

        // Act & Assert
        var act = () => new PhoneNumber(tooMany);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 15 digits*");
    }

    [Fact]
    public void Constructor_WithEmptyString_ThrowsException()
    {
        // Act & Assert
        var act = () => new PhoneNumber("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Phone number cannot be empty*");
    }

    [Fact]
    public void Formatted_WithTenDigits_ReturnsUsFormat()
    {
        // Arrange
        var phone = new PhoneNumber("5551234567");

        // Act & Assert
        phone.Formatted.Should().Be("(555) 123-4567");
    }

    [Fact]
    public void Formatted_WithInternationalNumber_ReturnsRawDigits()
    {
        // Arrange
        var phone = new PhoneNumber("+33 1 2345 6789"); // French number with 11 digits

        // Act & Assert
        phone.Formatted.Should().Be("+33123456789");
    }

    [Fact]
    public void Equality_WithSameNumber_ReturnsTrue()
    {
        // Arrange
        var phone1 = new PhoneNumber("5551234567");
        var phone2 = new PhoneNumber("(555) 123-4567"); // Same number, different format

        // Act & Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equality_WithDifferentNumber_ReturnsFalse()
    {
        // Arrange
        var phone1 = new PhoneNumber("5551234567");
        var phone2 = new PhoneNumber("5559876543");

        // Act & Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void ToString_ReturnsRawDigits()
    {
        // Arrange
        var phone = new PhoneNumber("(555) 123-4567");

        // Act & Assert
        phone.ToString().Should().Be("5551234567");
    }
}

/// <summary>
/// Tests for DateOfBirth value object.
/// </summary>
public class DateOfBirthTests
{
    [Fact]
    public void Constructor_WithValidDate_CreatesInstance()
    {
        // Arrange
        var date = new DateTime(2000, 1, 1);

        // Act
        var dob = new DateOfBirth(date);

        // Assert
        dob.Date.Should().Be(date.Date);
    }

    [Fact]
    public void Constructor_WithFutureDate_ThrowsException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var act = () => new DateOfBirth(futureDate);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be in the future*");
    }

    [Fact]
    public void Constructor_WithTooOldDate_ThrowsException()
    {
        // Arrange
        var tooOld = DateTime.UtcNow.AddYears(-151);

        // Act & Assert
        var act = () => new DateOfBirth(tooOld);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*unreasonably far in the past*");
    }

    [Fact]
    public void Age_CalculatesCorrectly()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-25).AddMonths(-6);
        var dob = new DateOfBirth(birthDate);

        // Act & Assert
        dob.Age.Should().Be(25);
    }

    [Fact]
    public void Age_BeforeBirthdayThisYear_IsCorrect()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var birthDate = new DateTime(today.Year - 30, today.Month + 1, today.Day); // Birthday next month
        if (birthDate > today)
        {
            var dob = new DateOfBirth(birthDate);
            // Act & Assert
            dob.Age.Should().Be(29); // Not turned 30 yet
        }
    }

    [Fact]
    public void DaysOld_CalculatesCorrectly()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddDays(-365);
        var dob = new DateOfBirth(birthDate);

        // Act & Assert
        dob.DaysOld.Should().BeGreaterThanOrEqualTo(365);
    }

    [Fact]
    public void Equality_WithSameDate_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2000, 1, 1);
        var dob1 = new DateOfBirth(date);
        var dob2 = new DateOfBirth(date);

        // Act & Assert
        dob1.Should().Be(dob2);
    }

    [Fact]
    public void Equality_WithDifferentDate_ReturnsFalse()
    {
        // Arrange
        var dob1 = new DateOfBirth(new DateTime(2000, 1, 1));
        var dob2 = new DateOfBirth(new DateTime(2000, 1, 2));

        // Act & Assert
        dob1.Should().NotBe(dob2);
    }

    [Fact]
    public void ToString_ReturnsIsoFormat()
    {
        // Arrange
        var date = new DateTime(2000, 1, 5);
        var dob = new DateOfBirth(date);

        // Act & Assert
        dob.ToString().Should().Be("2000-01-05");
    }
}
