using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageClientReadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Messages",
                newName: "IsReadBySalon");

            migrationBuilder.AddColumn<bool>(
                name: "IsReadByClient",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadByClient",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "IsReadBySalon",
                table: "Messages",
                newName: "IsRead");
        }
    }
}
