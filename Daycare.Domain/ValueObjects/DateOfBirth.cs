namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Represents a date of birth and provides age calculation.
/// Immutable value object.
/// </summary>
public sealed record DateOfBirth
{
    /// <summary>
    /// The date of birth (UTC, date only - no time).
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Creates a new DateOfBirth.
    /// </summary>
    /// <exception cref="ArgumentException">If date is invalid or in the future</exception>
    public DateOfBirth(DateTime date)
    {
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
            : date.ToUniversalTime().Date;

        var today = DateTime.UtcNow.Date;

        if (utcDate > today)
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(date));
        }

        // Reasonable age limit (not older than 150 years)
        var minDate = today.AddYears(-150);
        if (utcDate < minDate)
        {
            throw new ArgumentException("Date of birth is unreasonably far in the past.", nameof(date));
        }

        Date = utcDate;
    }

    /// <summary>
    /// Calculates the current age in years.
    /// </summary>
    public int Age
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - Date.Year;

            // Adjust if birthday hasn't occurred this year
            if (Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }

    /// <summary>
    /// Calculates the age in days (useful for infants).
    /// </summary>
    public int DaysOld
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            return (int)(today - Date).TotalDays;
        }
    }

    /// <summary>
    /// Returns the date in ISO format (yyyy-MM-dd).
    /// </summary>
    public override string ToString() => Date.ToString("yyyy-MM-dd");
}
