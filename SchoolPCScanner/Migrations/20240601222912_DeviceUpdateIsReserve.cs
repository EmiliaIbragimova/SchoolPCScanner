using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class DeviceUpdateIsReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReserve",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReserve",
                table: "Devices");
        }
    }
}
