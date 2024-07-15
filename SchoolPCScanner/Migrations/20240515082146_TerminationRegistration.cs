using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class TerminationRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerminationRegistration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ReturnedCase = table.Column<bool>(type: "bit", nullable: false),
                    ReturnedCharger = table.Column<bool>(type: "bit", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminationRegistration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminationRegistration_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminationRegistration_DeviceId",
                table: "TerminationRegistration",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminationRegistration");
        }
    }
}
