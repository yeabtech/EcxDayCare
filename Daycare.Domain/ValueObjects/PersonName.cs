using Daycare.Domain.Results;

namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Represents a person's name (first and last).
/// Immutable value object with validation.
/// </summary>
public sealed record PersonName
{
    /// <summary>
    /// The person's first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// The person's last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Creates a Result wrapping PersonName with validation.
    /// </summary>
    public static Result<PersonName> Create(string firstName, string lastName)
    {
        try
        {
            return Result<PersonName>.Success(new PersonName(firstName, lastName));
        }
        catch (ArgumentException ex)
        {
            return Result<PersonName>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new PersonName.
    /// </summary>
    /// <exception cref="ArgumentException">If names are null, empty, or too long</exception>
    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        }

        if (firstName.Length > 100)
        {
            throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));
        }

        if (lastName.Length > 100)
        {
            throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    /// <summary>
    /// Returns the full name as "FirstName LastName".
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Returns the full name.
    /// </summary>
    public override string ToString() => FullName;
}
