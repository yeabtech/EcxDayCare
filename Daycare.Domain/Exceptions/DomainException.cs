namespace Daycare.Domain.Exceptions;

/// <summary>
/// Base exception class for domain-specific business rule violations.
/// Use this when invariants are violated or invalid operations occur.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// The specific error code identifying this exception type.
    /// Used for machine-readable error handling.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Creates a new DomainException with a message and error code.
    /// </summary>
    public DomainException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode ?? GetType().Name;
    }

    /// <summary>
    /// Creates a new DomainException with a message, error code, and inner exception.
    /// </summary>
    public DomainException(string message, string? errorCode, Exception? innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? GetType().Name;
    }

    /// <summary>
    /// Returns a string representation including the error code.
    /// </summary>
    public override string ToString()
    {
        return $"{ErrorCode}: {base.ToString()}";
    }
}
