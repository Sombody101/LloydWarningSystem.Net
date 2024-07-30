using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.Extensions;
using Humanizer;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.EventHandlers;
using LloydWarningSystem.Net.Models;
using LloydWarningSystem.Net.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace LloydWarningSystem.Net;

internal static class LloydBot
{
    public const string DbConnectionString = "Data Source=./configs/lloyd-bot.db";

    public static Stopwatch startWatch;

    public static async Task RunAsync()
    {
        startWatch = Stopwatch.StartNew();

        var config = ConfigManager.BotConfig;

        string token = Program.DebugBuild
            ? config.DebugBotToken
            : config.BotToken;

        if (string.IsNullOrWhiteSpace(token))
        {
#if DEBUG
            Logging.LogError($"No bot debug token provided. {token}");
#else
            Logging.LogError($"No bot token provided. {token}");
#endif
            Environment.Exit(1);
        }

        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((ctx, services) =>
            {
                services.AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace)
                        .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                        .AddConsole();
                });

                services.AddSingleton(config);
                services.AddSingleton<DiscordCommandService>();
                services.AddHostedService(s => s.GetRequiredService<DiscordCommandService>());

                services.AddDiscordClient(token, TextCommandProcessor.RequiredIntents
                    | SlashCommandProcessor.RequiredIntents
                    | DiscordIntents.MessageContents
                    | DiscordIntents.GuildMembers
                    | DiscordIntents.GuildEmojisAndStickers);

                services.AddDbContextFactory<LloydContext>(
                    options =>
                    {
                        Logging.Log("Adding SQLite DB service");
                        options.UseSqlite(DbConnectionString);
                    }
                );

                services.AddMemoryCache(options =>
                {
                    Logging.WriteOrganizedData("Adding DB memory cache", new() {
                            { "Compaction", options.CompactionPercentage.ToString() },
                            { "Scan Freq", options.ExpirationScanFrequency.Humanize() },
                            { "Cache Limit", options.SizeLimit?.ToString() ?? "No limit found" },
                    });
                });

                services.AddSingleton(new AllocationRateTracker());
                services.ConfigureEventHandlers(builder =>
                {
                    InitializeEvents(builder);
                });
            })
            .RunConsoleAsync();
    }

    /// <summary>
    /// Implement important Guild based events
    /// </summary>
    /// <param name="client"></param>
    private static void InitializeEvents(EventHandlingBuilder cfg)
    {
        cfg.HandleGuildMemberRemoved(async (client, args) =>
        {
            await client.SendMessageAsync(args.Guild.Channels.First().Value,
                $"{args.Member.Mention} has left the server!");
        });

        cfg.HandleGuildCreated(async (client, args) =>
        {
            Logging.Log($"Joined guild: {args.Guild.Name} (id {args.Guild.Id})");
            var channel = args.Guild.Channels[0].Id;
            await client.SendMessageAsync(await client.GetChannelAsync(channel), "Hello!\nI'm here to look for fags joining and leaving!");
        });

        cfg.HandleGuildMemberAdded(async (client, args) =>
        {
            // My server
            if (!Program.DebugBuild && args.Guild.Id == BotConfigModel.DebugGuild)
            {
                var channel = await client.GetChannelAsync(1052365936574877820);
                await channel.SendMessageAsync($"Hey there, {args.Member.Mention}! Welcome to the server! 👋\n\n" +
                    $"We've got two Lloyd bot instances for you to try:\n* **Testing Ground:** {(await client.GetChannelAsync(1178519504209334323)).Mention} " +
                    $"- This bot has the latest features but might be a little buggy. 🚧\n* **Stable Bot:** {(await client.GetChannelAsync(1262569946312081488)).Mention} " +
                    $"- This bot is more stable but might have fewer features. 🤖\n" +
                    "I've gone ahead and given you the `Bot Tester` role which allows you to send commands in these channels!");

                // Add the 'Bot Tester' role
                await args.Member.GrantRoleAsync(args.Guild.GetRole(1262519248807264308)!);
            }
        });

        cfg.HandleGuildMemberRemoved(async (client, args) =>
        {
            // My server
            if (!Program.DebugBuild && args.Guild.Id == BotConfigModel.DebugGuild)
            {
                var channel = await client.GetChannelAsync(1052365936574877820);
                await channel.SendMessageAsync($"{args.Member.Mention} left the server!\nWhat a fucking fag.");
            }
        });

        cfg.HandleMessageCreated(async (client, args) =>
        {
            var db = (await Shared.TryGetDbContext())!;

            var message = args.Message;
            if (message.Author is null)
                return;

            var user = await db.Users.FindAsync(message.Author.Id);
            if (user is null)
                // User is not in DB
                return;

            if (!args.Author.IsBot)
            {
                var afkUsers = await db.Set<AfkStatusEntity>()
                                       .Where(x => x.UserId == args.Author.Id
                                            || args.MentionedUsers.Select(u => u.Id).Contains(x.UserId))
                                       .ToListAsync();
                // Check AFK
                var authorAfk = afkUsers.Find(x => x.UserId == args.Author.Id);
                if (authorAfk?.IsAfk() == true && args.Message.Content.Length > 5)
                {
                    // Message is larger than 5 characters
                    user.AfkStatus = null;
                    await db.SaveChangesAsync();
                    await args.Message.RespondAsync($"Welcome back {args.Author.Mention}!\nI've removed your AFK status.");
                }

                // Respond with AFK users and when they went AFK
                if (args.MentionedUsers.Any())
                {
                    var sb = new StringBuilder();

                    var afkMentionedUsers = afkUsers.Where(x => x.UserId != args.Author.Id && x.IsAfk());
                    if (afkMentionedUsers.Any())
                        foreach (var afkUser in afkMentionedUsers)
                            sb.AppendLine($"<@{afkUser.UserId}> is afk <t:{afkUser.AfkEpoch}:R>: {afkUser.AfkMessage}");

                    if (sb.Length > 0)
                        await args.Message.RespondAsync(sb.ToString());
                }
            }

            await HandleTagEvent.HandleTag(client, args, db);

            var emoji_str = user.ReactionEmoji;
            if (!string.IsNullOrWhiteSpace(emoji_str))
            {
                try
                {
                    if (!DiscordEmoji.TryFromName(client, emoji_str, out var emoji))
                        Logging.LogError("Failed to locate emoji");
                    else
                        await message.CreateReactionAsync(emoji);
                }
                catch (Exception ex)
                {
                    await ex.PrintException();
                }
            }
        });
    }
}
