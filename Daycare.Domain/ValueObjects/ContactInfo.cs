using Daycare.Domain.Results;

namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Value object representing contact information (email and phone)
/// </summary>
public sealed class ContactInfo : IEquatable<ContactInfo>
{
    public string Email { get; }
    public string PhoneNumber { get; }

    // Parameterless constructor for EF Core
    private ContactInfo() { }

    private ContactInfo(string email, string phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static Result<ContactInfo> Create(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<ContactInfo>.Failure("Email is required");

        if (!IsValidEmail(email))
            return Result<ContactInfo>.Failure("Email format is invalid");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result<ContactInfo>.Failure("Phone number is required");

        if (!IsValidPhoneNumber(phoneNumber))
            return Result<ContactInfo>.Failure("Phone number format is invalid");

        return Result<ContactInfo>.Success(new ContactInfo(email.Trim(), phoneNumber.Trim()));
    }

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

    private static bool IsValidPhoneNumber(string phone)
    {
        // Basic validation: at least 10 digits
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");
        return digitsOnly.Length >= 10;
    }

    public bool Equals(ContactInfo? other)
    {
        if (other is null) return false;
        return Email == other.Email && PhoneNumber == other.PhoneNumber;
    }

    public override bool Equals(object? obj) => Equals(obj as ContactInfo);

    public override int GetHashCode() => HashCode.Combine(Email, PhoneNumber);

    public override string ToString() => $"{Email}, {PhoneNumber}";
}
