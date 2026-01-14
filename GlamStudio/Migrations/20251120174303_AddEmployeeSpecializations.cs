using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSpecializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "EmployeeSpecializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSpecializations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSpecializations_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSpecializations_ApplicationUserId",
                table: "EmployeeSpecializations",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSpecializations");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
