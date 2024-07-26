using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LloydWarningSystem.Net.Migrations
{
    /// <inheritdoc />
    public partial class DbUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageAlias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageAliasesJson = table.Column<string>(type: "TEXT", nullable: false),
                    UserDbEntityId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAlias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageAlias_users_UserDbEntityId",
                        column: x => x.UserDbEntityId,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageAlias_UserDbEntityId",
                table: "MessageAlias",
                column: "UserDbEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageAlias");
        }
    }
}
