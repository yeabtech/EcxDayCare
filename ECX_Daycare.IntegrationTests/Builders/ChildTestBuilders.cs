using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;

namespace ECX_Daycare.IntegrationTests.Builders;

/// <summary>
/// Builder for creating test CreateChildDto instances.
/// </summary>
public class CreateChildDtoBuilder
{
    private string _firstName = "John";
    private string _lastName = "Doe";
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-5);
    private Gender _gender = Gender.Male;
    private DateTime _enrollmentDate = DateTime.UtcNow.AddMonths(-2);
    private string? _photoUrl = "https://example.com/photo.jpg";
    private string? _allergies = "None";
    private string? _medicalNotes = "None";

    public CreateChildDtoBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public CreateChildDtoBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public CreateChildDtoBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public CreateChildDtoBuilder WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public CreateChildDtoBuilder WithEnrollmentDate(DateTime enrollmentDate)
    {
        _enrollmentDate = enrollmentDate;
        return this;
    }

    public CreateChildDtoBuilder WithPhotoUrl(string? photoUrl)
    {
        _photoUrl = photoUrl;
        return this;
    }

    public CreateChildDtoBuilder WithAllergies(string? allergies)
    {
        _allergies = allergies;
        return this;
    }

    public CreateChildDtoBuilder WithMedicalNotes(string? medicalNotes)
    {
        _medicalNotes = medicalNotes;
        return this;
    }

    public CreateChildDto Build()
    {
        return new CreateChildDto(
            _firstName,
            _lastName,
            _dateOfBirth,
            _gender,
            _enrollmentDate,
            _photoUrl,
            _allergies,
            _medicalNotes);
    }
}

/// <summary>
/// Builder for creating test UpdateChildDto instances.
/// </summary>
public class UpdateChildDtoBuilder
{
    private string _firstName = "John";
    private string _lastName = "Doe";
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-5);
    private Gender _gender = Gender.Male;
    private string? _photoUrl = "https://example.com/photo.jpg";
    private string? _allergies = "None";
    private string? _medicalNotes = "None";

    public UpdateChildDtoBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UpdateChildDtoBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UpdateChildDtoBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public UpdateChildDtoBuilder WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public UpdateChildDtoBuilder WithPhotoUrl(string? photoUrl)
    {
        _photoUrl = photoUrl;
        return this;
    }

    public UpdateChildDtoBuilder WithAllergies(string? allergies)
    {
        _allergies = allergies;
        return this;
    }

    public UpdateChildDtoBuilder WithMedicalNotes(string? medicalNotes)
    {
        _medicalNotes = medicalNotes;
        return this;
    }

    public UpdateChildDto Build()
    {
        return new UpdateChildDto(
            _firstName,
            _lastName,
            _dateOfBirth,
            _gender,
            _photoUrl,
            _allergies,
            _medicalNotes);
    }
}

/// <summary>
/// Builder for creating test Child domain entities.
/// </summary>
public class ChildBuilder
{
    private ChildId _id = ChildId.New();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-5);
    private Gender _gender = Gender.Male;
    private DateTime _enrollmentDate = DateTime.UtcNow.AddMonths(-2);
    private string? _photoUrl = "https://example.com/photo.jpg";
    private string? _allergies = "None";
    private string? _medicalNotes = "None";

    public ChildBuilder WithId(ChildId id)
    {
        _id = id;
        return this;
    }

    public ChildBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public ChildBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public ChildBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public ChildBuilder WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public ChildBuilder WithEnrollmentDate(DateTime enrollmentDate)
    {
        _enrollmentDate = enrollmentDate;
        return this;
    }

    public ChildBuilder WithPhotoUrl(string? photoUrl)
    {
        _photoUrl = photoUrl;
        return this;
    }

    public ChildBuilder WithAllergies(string? allergies)
    {
        _allergies = allergies;
        return this;
    }

    public ChildBuilder WithMedicalNotes(string? medicalNotes)
    {
        _medicalNotes = medicalNotes;
        return this;
    }

    public Child Build()
    {
        return new Child(
            _id,
            new PersonName(_firstName, _lastName),
            new DateOfBirth(_dateOfBirth),
            _gender,
            _enrollmentDate,
            _photoUrl,
            _allergies,
            _medicalNotes);
    }
}
