using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DisbotNext.Infrastructures.Sqlite.Migrations
{
    public partial class TempChannelnowcreatedasagroupandwillbedeleteasagroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "TempChannels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "TempChannels",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "TempChannels");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "TempChannels");
        }
    }
}
