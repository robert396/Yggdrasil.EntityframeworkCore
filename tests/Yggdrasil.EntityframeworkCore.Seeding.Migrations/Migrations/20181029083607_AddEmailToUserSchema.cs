using Microsoft.EntityFrameworkCore.Migrations;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations.Migrations
{
    public partial class AddEmailToUserSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");
        }
    }
}
