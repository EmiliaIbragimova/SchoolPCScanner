using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class LogUpdateStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Logs_StudentId",
                table: "Logs",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Students_StudentId",
                table: "Logs",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Students_StudentId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_StudentId",
                table: "Logs");
        }
    }
}
