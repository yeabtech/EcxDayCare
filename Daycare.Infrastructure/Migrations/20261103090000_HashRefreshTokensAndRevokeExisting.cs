using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations;

[DbContext(typeof(DaycareDbContext))]
[Migration("20261103090000_HashRefreshTokensAndRevokeExisting")]
public sealed class HashRefreshTokensAndRevokeExisting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing refresh tokens were stored in plaintext; invalidate them instead of preserving bearer credentials.
        migrationBuilder.Sql("""DELETE FROM "public"."RefreshTokens";""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
