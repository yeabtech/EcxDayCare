namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Represents a phone number.
/// Immutable value object with basic validation and formatting.
/// </summary>
public sealed record PhoneNumber
{
    /// <summary>
    /// The phone number value (digits only).
    /// </summary>
    public string Number { get; }

    /// <summary>
    /// Creates a new PhoneNumber.
    /// Accepts formats like: 5551234567, 555-123-4567, (555) 123-4567, +1 555 123 4567
    /// </summary>
    /// <exception cref="ArgumentException">If phone number is invalid</exception>
    public PhoneNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Phone number cannot be empty.", nameof(number));
        }

        // Extract only digits and optional leading +
        var cleaned = ExtractDigits(number);

        if (cleaned.Length < 10)
        {
            throw new ArgumentException("Phone number must have at least 10 digits.", nameof(number));
        }

        if (cleaned.Length > 15)
        {
            throw new ArgumentException("Phone number cannot exceed 15 digits.", nameof(number));
        }

        Number = cleaned;
    }

    /// <summary>
    /// Extracts only digits from a phone number string.
    /// </summary>
    private static string ExtractDigits(string input)
    {
        var digits = string.Empty;

        // Allow + for international format
        if (input.StartsWith('+'))
        {
            digits = "+";
        }

        // Extract only digits
        foreach (var c in input)
        {
            if (char.IsDigit(c))
            {
                digits += c;
            }
        }

        return digits;
    }

    /// <summary>
    /// Returns a formatted phone number in format (XXX) XXX-XXXX for 10-digit US numbers.
    /// Returns raw digits for other formats.
    /// </summary>
    public string Formatted
    {
        get
        {
            var digits = Number.Replace("+", "");

            if (digits.Length == 10)
            {
                if (digits.StartsWith("09"))
                {
                    return $"{digits.Substring(0, 4)} {digits.Substring(4, 3)} {digits.Substring(7, 3)}";
                }
                return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
            }

            // Return as-is for international or non-standard lengths
            return Number;
        }
    }

    /// <summary>
    /// Returns the phone number with digits only.
    /// </summary>
    public override string ToString() => Number;
}
