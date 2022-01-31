using Microsoft.EntityFrameworkCore.Migrations;

namespace Common.Conflux.Database.Migrations
{
    public partial class FixDxConfigColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "cfx_config");

            migrationBuilder.AddColumn<string>(
                name: "ConfigKey",
                table: "cfx_config",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigKey",
                table: "cfx_config");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "cfx_config",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
