using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class LogsTableIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Logs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_IdentityUserId",
                table: "Logs",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_AspNetUsers_IdentityUserId",
                table: "Logs",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_AspNetUsers_IdentityUserId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_IdentityUserId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Logs");
        }
    }
}
