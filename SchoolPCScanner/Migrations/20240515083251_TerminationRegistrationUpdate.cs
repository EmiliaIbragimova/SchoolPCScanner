using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class TerminationRegistrationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminationRegistration_Devices_DeviceId",
                table: "TerminationRegistration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerminationRegistration",
                table: "TerminationRegistration");

            migrationBuilder.RenameTable(
                name: "TerminationRegistration",
                newName: "TerminationRegistrations");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationRegistration_DeviceId",
                table: "TerminationRegistrations",
                newName: "IX_TerminationRegistrations_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerminationRegistrations",
                table: "TerminationRegistrations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationRegistrations_Devices_DeviceId",
                table: "TerminationRegistrations",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminationRegistrations_Devices_DeviceId",
                table: "TerminationRegistrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerminationRegistrations",
                table: "TerminationRegistrations");

            migrationBuilder.RenameTable(
                name: "TerminationRegistrations",
                newName: "TerminationRegistration");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationRegistrations_DeviceId",
                table: "TerminationRegistration",
                newName: "IX_TerminationRegistration_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerminationRegistration",
                table: "TerminationRegistration",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationRegistration_Devices_DeviceId",
                table: "TerminationRegistration",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
