using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionsFavoritesAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotMessages_AspNetUsers_PatientId",
                table: "BotMessages");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MessageType",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "BotMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageType",
                table: "BotMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "BotMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsTitleGenerated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteChats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SentAt",
                table: "Messages",
                columns: new[] { "ConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_BotMessages_SessionId",
                table: "BotMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BotMessages_SessionId_SentAt",
                table: "BotMessages",
                columns: new[] { "SessionId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteChats_UserId_ConversationId",
                table: "FavoriteChats",
                columns: new[] { "UserId", "ConversationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BotMessages_AspNetUsers_PatientId",
                table: "BotMessages",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BotMessages_ChatSessions_SessionId",
                table: "BotMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotMessages_AspNetUsers_PatientId",
                table: "BotMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_BotMessages_ChatSessions_SessionId",
                table: "BotMessages");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "FavoriteChats");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId_SentAt",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_BotMessages_SessionId",
                table: "BotMessages");

            migrationBuilder.DropIndex(
                name: "IX_BotMessages_SessionId_SentAt",
                table: "BotMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "BotMessages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "BotMessages");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "BotMessages");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ConversationId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BotMessages_AspNetUsers_PatientId",
                table: "BotMessages",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
