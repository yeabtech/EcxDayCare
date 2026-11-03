namespace Daycare.Application.DTOs;

public record LoginRequestDto(
    string Email,
    string Password);

public record RegisterRequestDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = "Parent",
    Guid? StaffId = null,
    Guid? ParentId = null);

public record RefreshTokenRequestDto(
    string RefreshToken);

public record RevokeTokenRequestDto(
    string RefreshToken);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid? StaffId,
    Guid? ParentId);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    CurrentUserDto User);
