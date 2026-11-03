using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParentNotifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentNotifications_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalSchema: "public",
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentNotifications_Parents_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "public",
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentNotifications_DailyReportId",
                schema: "public",
                table: "ParentNotifications",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentNotifications_ParentId_DailyReportId_Unique",
                schema: "public",
                table: "ParentNotifications",
                columns: new[] { "ParentId", "DailyReportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentNotifications_ParentId_IsRead_CreatedAt",
                schema: "public",
                table: "ParentNotifications",
                columns: new[] { "ParentId", "IsRead", "CreatedAt" });

            migrationBuilder.Sql("""
                INSERT INTO public."ParentNotifications"
                    ("Id", "ParentId", "DailyReportId", "Title", "Message", "IsRead", "ReadAt", "CreatedAt", "UpdatedAt")
                SELECT
                    gen_random_uuid(),
                    parent_child."ParentId",
                    report."Id",
                    'Daily report ready',
                    child."FirstName" || ' ' || child."LastName" || '''s daily report for '
                        || to_char(report."Date" AT TIME ZONE 'UTC', 'FMMonth FMDD, YYYY')
                        || ' is available.',
                    lower(COALESCE(report."Milestones", '')) = 'parentread',
                    CASE
                        WHEN lower(COALESCE(report."Milestones", '')) = 'parentread'
                            THEN COALESCE(report."UpdatedAt", report."SentAt", report."CreatedAt")
                        ELSE NULL
                    END,
                    COALESCE(report."SentAt", report."CreatedAt"),
                    COALESCE(report."SentAt", report."CreatedAt")
                FROM public."DailyReports" AS report
                INNER JOIN public."ParentChildren" AS parent_child
                    ON parent_child."ChildId" = report."ChildId"
                INNER JOIN public."Children" AS child
                    ON child."Id" = report."ChildId"
                WHERE report."SentToParent" = TRUE
                ON CONFLICT ("ParentId", "DailyReportId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentNotifications",
                schema: "public");
        }
    }
}
