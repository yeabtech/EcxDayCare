using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyLogReportLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DailyReportId",
                schema: "public",
                table: "NapLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DailyReportId",
                schema: "public",
                table: "FeedingLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DailyReportId",
                schema: "public",
                table: "ActivityLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NapLogs_DailyReportId",
                schema: "public",
                table: "NapLogs",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_DailyReportId",
                schema: "public",
                table: "FeedingLogs",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_DailyReportId",
                schema: "public",
                table: "ActivityLogs",
                column: "DailyReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "ActivityLogs",
                column: "DailyReportId",
                principalSchema: "public",
                principalTable: "DailyReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedingLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "FeedingLogs",
                column: "DailyReportId",
                principalSchema: "public",
                principalTable: "DailyReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NapLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "NapLogs",
                column: "DailyReportId",
                principalSchema: "public",
                principalTable: "DailyReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Preserve existing history by connecting records to reports for the same child/day.
            migrationBuilder.Sql("""
                UPDATE public."ActivityLogs" AS log
                SET "DailyReportId" = report."Id"
                FROM public."DailyReports" AS report
                WHERE log."ChildId" = report."ChildId"
                  AND (log."Time" AT TIME ZONE 'UTC')::date = (report."Date" AT TIME ZONE 'UTC')::date;

                UPDATE public."FeedingLogs" AS log
                SET "DailyReportId" = report."Id"
                FROM public."DailyReports" AS report
                WHERE log."ChildId" = report."ChildId"
                  AND (log."Time" AT TIME ZONE 'UTC')::date = (report."Date" AT TIME ZONE 'UTC')::date;

                UPDATE public."NapLogs" AS log
                SET "DailyReportId" = report."Id"
                FROM public."DailyReports" AS report
                WHERE log."ChildId" = report."ChildId"
                  AND (log."StartTime" AT TIME ZONE 'UTC')::date = (report."Date" AT TIME ZONE 'UTC')::date;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedingLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "FeedingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_NapLogs_DailyReports_DailyReportId",
                schema: "public",
                table: "NapLogs");

            migrationBuilder.DropIndex(
                name: "IX_NapLogs_DailyReportId",
                schema: "public",
                table: "NapLogs");

            migrationBuilder.DropIndex(
                name: "IX_FeedingLogs_DailyReportId",
                schema: "public",
                table: "FeedingLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_DailyReportId",
                schema: "public",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "DailyReportId",
                schema: "public",
                table: "NapLogs");

            migrationBuilder.DropColumn(
                name: "DailyReportId",
                schema: "public",
                table: "FeedingLogs");

            migrationBuilder.DropColumn(
                name: "DailyReportId",
                schema: "public",
                table: "ActivityLogs");
        }
    }
}
