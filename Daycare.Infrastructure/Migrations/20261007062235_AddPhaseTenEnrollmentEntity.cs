using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daycare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseTenEnrollmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enrollments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysOfWeek = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractSigned = table.Column<bool>(type: "boolean", nullable: false),
                    ImmunizationRecord = table.Column<bool>(type: "boolean", nullable: false),
                    BirthCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    MedicalForm = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EndReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Children_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "public",
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ChildId",
                schema: "public",
                table: "Enrollments",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassroomId",
                schema: "public",
                table: "Enrollments",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_Status",
                schema: "public",
                table: "Enrollments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments",
                schema: "public");
        }
    }
}
