using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LloydWarningSystem.Net.Migrations
{
    /// <inheritdoc />
    public partial class InitializeSqliteDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Starboard",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discordMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    discordChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    discordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    starCount = table.Column<int>(type: "INTEGER", nullable: false),
                    starboardMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    starboardChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    starboardGuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starboard", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    preferred_language = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    prefix = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    starboardEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    starboardChannel = table.Column<ulong>(type: "INTEGER", nullable: true),
                    starboardThreshold = table.Column<int>(type: "INTEGER", nullable: true),
                    starboardEmojiId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    starboardEmojiName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Configs_guilds_discordId",
                        column: x => x.discordId,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    quotedUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    content = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Quotes_guilds_discordGuildId",
                        column: x => x.discordGuildId,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guild_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    target_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    moderator_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: false),
                    CreationTimeStamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.id);
                    table.ForeignKey(
                        name: "FK_Incidents_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidents_users_target_id",
                        column: x => x.target_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reminderText = table.Column<string>(type: "TEXT", nullable: false),
                    creationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    executionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    isPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    channelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    messageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    mentionedChannel = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MentionedMessage = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reminders_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceAlerts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    channel_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    guild_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    user_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    is_repeatable = table.Column<bool>(type: "INTEGER", nullable: false),
                    last_alert = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    time_between = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceAlerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_VoiceAlerts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configs_discordId",
                table: "Configs",
                column: "discordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_guild_id",
                table: "Incidents",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_discordGuildId",
                table: "Quotes",
                column: "discordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_userId",
                table: "Reminders",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceAlerts_user_id",
                table: "VoiceAlerts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configs");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Starboard");

            migrationBuilder.DropTable(
                name: "VoiceAlerts");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
