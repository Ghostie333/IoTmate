using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTmate.Model.Migrations
{
    /// <inheritdoc />
    public partial class OutletsAsArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outlets",
                table: "DeviceRelayStates");

            migrationBuilder.AddColumn<string[]>(
                name: "OutletsNames",
                table: "DeviceRelayStates",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool[]>(
                name: "OutletsState",
                table: "DeviceRelayStates",
                type: "boolean[]",
                nullable: false,
                defaultValue: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutletsNames",
                table: "DeviceRelayStates");

            migrationBuilder.DropColumn(
                name: "OutletsState",
                table: "DeviceRelayStates");

            migrationBuilder.AddColumn<Dictionary<string, bool>>(
                name: "Outlets",
                table: "DeviceRelayStates",
                type: "jsonb",
                nullable: false);
        }
    }
}
