namespace Daycare.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ECX_Daycare_API";

    public string Audience { get; set; } = "ECX_Daycare_Client";

    public int AccessTokenExpirationMinutes { get; set; } = 15;

    public int RefreshTokenExpirationDays { get; set; } = 7;
}
