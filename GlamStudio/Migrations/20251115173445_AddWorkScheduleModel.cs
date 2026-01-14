using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkScheduleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_AspNetUsers_ApplicationUserId",
                table: "WorkSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkSchedule",
                table: "WorkSchedule");

            migrationBuilder.RenameTable(
                name: "WorkSchedule",
                newName: "WorkSchedules");

            migrationBuilder.RenameIndex(
                name: "IX_WorkSchedule_ApplicationUserId",
                table: "WorkSchedules",
                newName: "IX_WorkSchedules_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkSchedules",
                table: "WorkSchedules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedules_AspNetUsers_ApplicationUserId",
                table: "WorkSchedules",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedules_AspNetUsers_ApplicationUserId",
                table: "WorkSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkSchedules",
                table: "WorkSchedules");

            migrationBuilder.RenameTable(
                name: "WorkSchedules",
                newName: "WorkSchedule");

            migrationBuilder.RenameIndex(
                name: "IX_WorkSchedules_ApplicationUserId",
                table: "WorkSchedule",
                newName: "IX_WorkSchedule_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkSchedule",
                table: "WorkSchedule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_AspNetUsers_ApplicationUserId",
                table: "WorkSchedule",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
