using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class TerminationRegistrationUpdateStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TerminationRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "TerminationRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
               name: "IX_TerminationRegistrations_StudentId",
               table: "TerminationRegistrations",
               column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationRegistrations_Students_StudentId",
                table: "TerminationRegistrations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            //migrationBuilder.AddColumn<int>(
            //    name: "StudentId1",
            //    table: "TerminationRegistrations",
            //    type: "int",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_TerminationRegistrations_StudentId1",
            //    table: "TerminationRegistrations",
            //    column: "StudentId1");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_TerminationRegistrations_Students_StudentId1",
            //    table: "TerminationRegistrations",
            //    column: "StudentId1",
            //    principalTable: "Students",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminationRegistrations_Students_StudentId",
                table: "TerminationRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_TerminationRegistrations_StudentId",
                table: "TerminationRegistrations");
            //migrationBuilder.DropForeignKey(
            //    name: "FK_TerminationRegistrations_Students_StudentId1",
            //    table: "TerminationRegistrations");

            //migrationBuilder.DropIndex(
            //    name: "IX_TerminationRegistrations_StudentId1",
            //    table: "TerminationRegistrations");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TerminationRegistrations");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "TerminationRegistrations");

            //migrationBuilder.DropColumn(
            //    name: "StudentId1",
            //    table: "TerminationRegistrations");
        }
    }
}
