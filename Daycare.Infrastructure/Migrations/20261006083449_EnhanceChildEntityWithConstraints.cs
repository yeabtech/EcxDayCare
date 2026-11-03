using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceChildEntityWithConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Children",
                newName: "Children",
                newSchema: "public");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "Children",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "Children",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Children_EnrollmentDate",
                schema: "public",
                table: "Children",
                column: "EnrollmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Children_Status_CreatedAt",
                schema: "public",
                table: "Children",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Children_Gender",
                schema: "public",
                table: "Children",
                sql: "\"Gender\" IN ('Male', 'Female', 'Other', 'PreferNotToSay')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Children_Status",
                schema: "public",
                table: "Children",
                sql: "\"Status\" IN ('Enrolled', 'Inactive', 'Withdrawn')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Children_EnrollmentDate",
                schema: "public",
                table: "Children");

            migrationBuilder.DropIndex(
                name: "IX_Children_Status_CreatedAt",
                schema: "public",
                table: "Children");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Children_Gender",
                schema: "public",
                table: "Children");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Children_Status",
                schema: "public",
                table: "Children");

            migrationBuilder.RenameTable(
                name: "Children",
                schema: "public",
                newName: "Children");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Children",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Children",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
