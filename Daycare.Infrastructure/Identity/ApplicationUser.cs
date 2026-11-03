using Microsoft.AspNetCore.Identity;

namespace Daycare.Infrastructure.Identity;

/// <summary>
/// Custom application user extending ASP.NET Core IdentityUser with GUID primary key
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = "Parent";

    public Guid? StaffId { get; set; }

    public Guid? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
