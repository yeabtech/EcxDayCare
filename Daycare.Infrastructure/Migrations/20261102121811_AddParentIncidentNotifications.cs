using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentIncidentNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParentIncidentNotifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentIncidentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentIncidentNotifications_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalSchema: "public",
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentIncidentNotifications_Parents_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "public",
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentIncidentNotifications_IncidentId",
                schema: "public",
                table: "ParentIncidentNotifications",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentIncidentNotifications_ParentId_IncidentId_Unique",
                schema: "public",
                table: "ParentIncidentNotifications",
                columns: new[] { "ParentId", "IncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentIncidentNotifications_ParentId_IsRead_CreatedAt",
                schema: "public",
                table: "ParentIncidentNotifications",
                columns: new[] { "ParentId", "IsRead", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentIncidentNotifications",
                schema: "public");
        }
    }
}
