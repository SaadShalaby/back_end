using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToDoctorSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DoctorSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DoctorSessions");
        }
    }
}
