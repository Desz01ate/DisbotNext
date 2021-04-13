using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DisbotNext.Infrastructures.Sqlite.Migrations
{
    public partial class Adderrorlogtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Method = table.Column<string>(type: "TEXT", nullable: false),
                    Log = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TriggeredById = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Members_TriggeredById",
                        column: x => x.TriggeredById,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_TriggeredById",
                table: "ErrorLogs",
                column: "TriggeredById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");
        }
    }
}
