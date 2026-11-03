using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "public",
                table: "ParentChildren",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Guardian");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParentChildren_Role",
                schema: "public",
                table: "ParentChildren",
                sql: "\"Role\" IN ('Guardian', 'Mother', 'Father')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ParentChildren_Role",
                schema: "public",
                table: "ParentChildren");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "public",
                table: "ParentChildren");
        }
    }
}
