using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseElevenAttendanceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendances",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DroppedOffBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PickedUpBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckOutStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ChildId",
                schema: "public",
                table: "Attendances",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ChildId_Date",
                schema: "public",
                table: "Attendances",
                columns: new[] { "ChildId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassroomId",
                schema: "public",
                table: "Attendances",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Date",
                schema: "public",
                table: "Attendances",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Status",
                schema: "public",
                table: "Attendances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances",
                schema: "public");
        }
    }
}
