using System.Security.Claims;

namespace Daycare.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        string role,
        Guid? staffId = null,
        Guid? parentId = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true);
}
