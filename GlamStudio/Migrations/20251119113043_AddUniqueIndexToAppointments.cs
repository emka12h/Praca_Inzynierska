using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlamStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_EmployeeID",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_EmployeeID_AppointmentDate_AppointmentTime",
                table: "Appointments",
                columns: new[] { "EmployeeID", "AppointmentDate", "AppointmentTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_EmployeeID_AppointmentDate_AppointmentTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_EmployeeID",
                table: "Appointments",
                column: "EmployeeID");
        }
    }
}
