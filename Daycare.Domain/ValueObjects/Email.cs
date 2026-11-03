namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Represents an email address.
/// Immutable value object with basic validation.
/// </summary>
public sealed record Email
{
    /// <summary>
    /// The email address value.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Creates a new Email.
    /// </summary>
    /// <exception cref="ArgumentException">If email is null, empty, or invalid format</exception>
    public Email(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(address));
        }

        var trimmed = address.Trim().ToLowerInvariant();

        if (!IsValidEmail(trimmed))
        {
            throw new ArgumentException("Email format is invalid.", nameof(address));
        }

        if (trimmed.Length > 254)
        {
            throw new ArgumentException("Email cannot exceed 254 characters.", nameof(address));
        }

        Address = trimmed;
    }

    /// <summary>
    /// Validates email format using basic RFC 5322 rules.
    /// This is a simple validation; use a library for production email validation.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the email address.
    /// </summary>
    public override string ToString() => Address;
}
