using System.Text.Json.Serialization;

namespace ECX_Daycare.Common;

/// <summary>
/// Standard API response envelope.
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    public static ApiResponse<T> Success(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string? error, string message = "")
    {
        var resolved = string.IsNullOrEmpty(error) ? "An unexpected error occurred." : error;
        return new ApiResponse<T>
        {
            Succeeded = false,
            Error = resolved,
            Message = string.IsNullOrEmpty(message) ? resolved : message
        };
    }
}
