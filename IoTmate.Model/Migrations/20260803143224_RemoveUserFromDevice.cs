using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTmate.Model.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserFromDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId1",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_UserId1",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Devices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Devices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId1",
                table: "Devices",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId1",
                table: "Devices",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
