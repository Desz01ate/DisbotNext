using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DisbotNext.Infrastructures.Sqlite.Migrations
{
    public partial class Addtabletempchannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ChatLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "ChatLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TempChannels",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelName = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempChannels", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs",
                column: "AuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs");

            migrationBuilder.DropTable(
                name: "TempChannels");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ChatLogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<ulong>(
                name: "AuthorId",
                table: "ChatLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatLogs_Members_AuthorId",
                table: "ChatLogs",
                column: "AuthorId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
