using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LloydWarningSystem.Net.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBotAdminProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "users");

            migrationBuilder.AddColumn<bool>(
                name: "IsBotAdmin",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBotAdmin",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "users",
                type: "TEXT",
                nullable: true);
        }
    }
}
