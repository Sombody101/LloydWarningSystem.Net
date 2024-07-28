using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LloydWarningSystem.Net.Migrations
{
    /// <inheritdoc />
    public partial class RestoreUserReactionPropertyWithColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageAlias");

            migrationBuilder.RenameColumn(
                name: "IsBotAdmin",
                table: "users",
                newName: "is_bot_admin");

            migrationBuilder.AddColumn<string>(
                name: "reaction_emoji",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MessageTag",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tag_name = table.Column<string>(type: "TEXT", nullable: false),
                    tag_data = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTag", x => x.id);
                    table.ForeignKey(
                        name: "FK_MessageTag_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTag_user_id",
                table: "MessageTag",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageTag");

            migrationBuilder.DropColumn(
                name: "reaction_emoji",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "is_bot_admin",
                table: "users",
                newName: "IsBotAdmin");

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
    }
}
