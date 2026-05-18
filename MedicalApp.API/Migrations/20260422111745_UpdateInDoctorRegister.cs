using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInDoctorRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalNumber",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalNumber",
                table: "Doctors");
        }
    }
}
