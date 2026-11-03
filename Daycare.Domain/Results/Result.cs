namespace Daycare.Domain.Results;

/// <summary>
/// Represents the result of a domain operation.
/// Use instead of throwing exceptions for expected failures.
/// 
/// Usage:
///   var result = Result&lt;string&gt;.Success("Operation completed");
///   if (result.IsSuccess) { /* handle success */ }
///   
///   var failure = Result&lt;string&gt;.Failure("Invalid input", "INVALID_INPUT");
///   if (!failure.IsSuccess) { /* handle error */ }
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error message if the operation failed.
    /// </summary>
    public string? Error => ErrorMessage;

    /// <summary>
    /// The error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Creates a failure result with a message and optional error code.
    /// </summary>
    public static Result Failure(string errorMessage, string? errorCode = null)
    {
        return new FailureResult(errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static Result Success()
    {
        return new SuccessResult();
    }

    private sealed record SuccessResult : Result
    {
        public SuccessResult()
        {
            IsSuccess = true;
        }
    }

    private sealed record FailureResult : Result
    {
        public FailureResult(string errorMessage, string? errorCode)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            ErrorCode = errorCode ?? "UNKNOWN_ERROR";
        }
    }
}

/// <summary>
/// Generic result type for operations that return a value.
/// 
/// Usage:
///   var result = Result&lt;User&gt;.Success(newUser);
///   if (result.IsSuccess) { var user = result.Value; }
/// </summary>
/// <typeparam name="T">The type of value returned on success</typeparam>
public abstract record Result<T> : Result
{
    /// <summary>
    /// The resulting value if the operation was successful.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Creates a success result with a value.
    /// </summary>
    public static Result<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Success result cannot have a null value");
        }

        return new SuccessResult(value);
    }

    /// <summary>
    /// Creates a failure result with an error message and optional error code.
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new FailureResult(errorMessage, errorCode);
    }

    private sealed record SuccessResult : Result<T>
    {
        public SuccessResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }
    }

    private sealed record FailureResult : Result<T>
    {
        public FailureResult(string errorMessage, string? errorCode)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            ErrorCode = errorCode ?? "UNKNOWN_ERROR";
        }
    }
}
