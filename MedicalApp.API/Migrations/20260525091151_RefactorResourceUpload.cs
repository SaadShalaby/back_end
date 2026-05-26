using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorResourceUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Resources",
                newName: "MimeType");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Resources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "Resources",
                newName: "Url");

            migrationBuilder.AlterColumn<double>(
                name: "FileSize",
                table: "Resources",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
