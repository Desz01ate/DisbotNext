using Microsoft.EntityFrameworkCore.Migrations;

namespace DisbotNext.Infrastructure.Postgres.Migrations
{
    public partial class AddAuthorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "ChatLogs",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatLogs_AuthorId",
                table: "ChatLogs",
                newName: "IX_ChatLogs_MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatLogs_Members_MemberId",
                table: "ChatLogs",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatLogs_Members_MemberId",
                table: "ChatLogs");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "ChatLogs",
                newName: "AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatLogs_MemberId",
                table: "ChatLogs",
                newName: "IX_ChatLogs_AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs",
                column: "AuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
