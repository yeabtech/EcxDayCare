using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Participation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedingLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeedingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FoodItems = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PortionConsumed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedingLogs_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthLogs_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    InjuryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AffectedBodyPart = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstAidGiven = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentNotified = table.Column<bool>(type: "boolean", nullable: false),
                    ParentSignature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FollowUp = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NapLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Quality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NapLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NapLogs_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ChildId",
                schema: "public",
                table: "ActivityLogs",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ChildId_Time",
                schema: "public",
                table: "ActivityLogs",
                columns: new[] { "ChildId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Time",
                schema: "public",
                table: "ActivityLogs",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_ChildId",
                schema: "public",
                table: "FeedingLogs",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_ChildId_Time",
                schema: "public",
                table: "FeedingLogs",
                columns: new[] { "ChildId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_Time",
                schema: "public",
                table: "FeedingLogs",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_HealthLogs_ChildId",
                schema: "public",
                table: "HealthLogs",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthLogs_ChildId_Time",
                schema: "public",
                table: "HealthLogs",
                columns: new[] { "ChildId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ChildId",
                schema: "public",
                table: "Incidents",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ChildId_IncidentTime",
                schema: "public",
                table: "Incidents",
                columns: new[] { "ChildId", "IncidentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_NapLogs_ChildId",
                schema: "public",
                table: "NapLogs",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_NapLogs_ChildId_StartTime",
                schema: "public",
                table: "NapLogs",
                columns: new[] { "ChildId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_NapLogs_StartTime",
                schema: "public",
                table: "NapLogs",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FeedingLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HealthLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Incidents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NapLogs",
                schema: "public");
        }
    }
}
