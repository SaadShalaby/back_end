using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmotionToBotMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emotion",
                table: "BotMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emotion",
                table: "BotMessages");
        }
    }
}
