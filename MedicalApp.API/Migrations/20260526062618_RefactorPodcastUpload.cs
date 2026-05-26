using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPodcastUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "PodcastEpisodes");

            migrationBuilder.RenameColumn(
                name: "PublishDate",
                table: "PodcastEpisodes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "AudioUrl",
                table: "PodcastEpisodes",
                newName: "FileUrl");

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "PodcastEpisodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "PodcastEpisodes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "PodcastEpisodes");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "PodcastEpisodes");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "PodcastEpisodes");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "PodcastEpisodes");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "PodcastEpisodes",
                newName: "AudioUrl");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PodcastEpisodes",
                newName: "PublishDate");

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationInSeconds",
                table: "PodcastEpisodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
