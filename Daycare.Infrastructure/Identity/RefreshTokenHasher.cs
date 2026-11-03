using System.Security.Cryptography;
using System.Text;

namespace Daycare.Infrastructure.Identity;

public static class RefreshTokenHasher
{
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
