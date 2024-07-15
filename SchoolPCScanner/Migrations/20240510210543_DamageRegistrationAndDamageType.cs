using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolPCScanner.Migrations
{
    /// <inheritdoc />
    public partial class DamageRegistrationAndDamageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DamageRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamageRegistrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DamageRegistrations_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DamageRegistrations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DamageTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DamageRegistrationDamageType",
                columns: table => new
                {
                    DamageRegistrationsId = table.Column<int>(type: "int", nullable: false),
                    DamageTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageRegistrationDamageType", x => new { x.DamageRegistrationsId, x.DamageTypesId });
                    table.ForeignKey(
                        name: "FK_DamageRegistrationDamageType_DamageRegistrations_DamageRegistrationsId",
                        column: x => x.DamageRegistrationsId,
                        principalTable: "DamageRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DamageRegistrationDamageType_DamageTypes_DamageTypesId",
                        column: x => x.DamageTypesId,
                        principalTable: "DamageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamageRegistrationDamageType_DamageTypesId",
                table: "DamageRegistrationDamageType",
                column: "DamageTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRegistrations_DeviceId",
                table: "DamageRegistrations",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRegistrations_StudentId",
                table: "DamageRegistrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRegistrations_UserId",
                table: "DamageRegistrations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamageRegistrationDamageType");

            migrationBuilder.DropTable(
                name: "DamageRegistrations");

            migrationBuilder.DropTable(
                name: "DamageTypes");
        }
    }
}
