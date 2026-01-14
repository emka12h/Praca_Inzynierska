using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonNotesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalonNotes",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalonNotes",
                table: "AspNetUsers");
        }
    }
}
