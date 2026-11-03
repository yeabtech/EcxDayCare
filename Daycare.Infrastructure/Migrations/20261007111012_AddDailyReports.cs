using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Mood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MealsSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NapSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActivitiesSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DiaperSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Milestones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Concerns = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentToParent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReports_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ChildId",
                schema: "public",
                table: "DailyReports",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ChildId_Date_Unique",
                schema: "public",
                table: "DailyReports",
                columns: new[] { "ChildId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_Date",
                schema: "public",
                table: "DailyReports",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_SentToParent",
                schema: "public",
                table: "DailyReports",
                column: "SentToParent");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_StaffId",
                schema: "public",
                table: "DailyReports",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyReports",
                schema: "public");
        }
    }
}
