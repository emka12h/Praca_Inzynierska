using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTimeOffField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTimeOff",
                table: "WorkSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTimeOff",
                table: "WorkSchedules");
        }
    }
}
