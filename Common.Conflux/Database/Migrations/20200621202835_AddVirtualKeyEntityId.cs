using Microsoft.EntityFrameworkCore.Migrations;

namespace Conflux.Database.Migrations
{
    public partial class AddVirtualKeyEntityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "FromId",
                table: "cfx_vkeys",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FromId",
                table: "cfx_vkeys",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(long));
        }
    }
}
