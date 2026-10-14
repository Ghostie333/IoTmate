using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTmate.Model.Migrations
{
    /// <inheritdoc />
    public partial class AddDaysOfWeekToRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool[]>(
                name: "DaysOfWeek",
                table: "Rules",
                type: "boolean[]",
                nullable: false,
                defaultValue: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "Rules");
        }
    }
}
