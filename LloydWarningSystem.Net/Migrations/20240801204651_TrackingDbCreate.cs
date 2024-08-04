using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LloydWarningSystem.Net.Migrations
{
    /// <inheritdoc />
    public partial class TrackingDbCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackingDbEntity",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    creation_epoch = table.Column<ulong>(type: "INTEGER", nullable: false),
                    channel_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    report_channel = table.Column<ulong>(type: "INTEGER", nullable: false),
                    tracking_regex = table.Column<string>(type: "TEXT", nullable: false),
                    editor_list = table.Column<string>(type: "TEXT", nullable: false),
                    items_flagged = table.Column<uint>(type: "INTEGER", nullable: false),
                    GuildDbEntityId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingDbEntity", x => x.id);
                    table.ForeignKey(
                        name: "FK_TrackingDbEntity_guilds_GuildDbEntityId",
                        column: x => x.GuildDbEntityId,
                        principalTable: "guilds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingDbEntity_GuildDbEntityId",
                table: "TrackingDbEntity",
                column: "GuildDbEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingDbEntity_GuildId",
                table: "TrackingDbEntity",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingDbEntity_name",
                table: "TrackingDbEntity",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackingDbEntity");
        }
    }
}
