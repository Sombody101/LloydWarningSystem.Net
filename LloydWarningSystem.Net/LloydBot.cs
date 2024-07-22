using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Extensions;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace LloydWarningSystem.Net;

internal class LloydBot
{
    public const string connectionString = "Data Source=./configs/lloyd-bot.db";

    public static IServiceProvider Services;

    public static Stopwatch startWatch;

    public async Task RunAsync()
    {
        startWatch = Stopwatch.StartNew();

        var config = ConfigManager.BotConfig;

        string token;

#if DEBUG
        if (string.IsNullOrWhiteSpace(config.DebugBotToken))
        {
            Logging.LogError($"No bot debug token provided. {config.DebugBotToken}");
            Environment.Exit(1);
        }

        token = config.DebugBotToken;
#else
        if (string.IsNullOrWhiteSpace(config.BotToken))
        {
            Logging.LogError($"No bot token provided. {config.BotToken}");
            Environment.Exit(1);
        }

        token = config.BotToken;
#endif

        await Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(config)
                    .AddDiscordClient(token, TextCommandProcessor.RequiredIntents
                        | SlashCommandProcessor.RequiredIntents
                        | DiscordIntents.MessageContents
                        | DiscordIntents.GuildMembers)
                    .ConfigureEventHandlers(builder =>
                    {
                        InitializeEvents(builder);
                    })
                    .AddSingleton<DiscordCommandService>()
                    .AddHostedService(s => s.GetRequiredService<DiscordCommandService>())
                    .AddDbContextFactory<LloydContext>(
                        options =>
                        {
                            options.UseSqlite(connectionString);
                            options.EnableDetailedErrors();
                        }
                    );

                Services = services.BuildServiceProvider();
            })
            .RunConsoleAsync();
    }

    /// <summary>
    /// Implement important Guild based events
    /// </summary>
    /// <param name="client"></param>
    private static void InitializeEvents(EventHandlingBuilder cfg)
    {
        cfg.HandleGuildMemberAdded(async (client, sender) =>
        {
        });

        cfg.HandleGuildMemberRemoved(async (client, sender) =>
        {
            await client.SendMessageAsync(sender.Guild.Channels.First().Value,
                $"{sender.Member.Mention} has left the server!");
        });

        cfg.HandleGuildCreated(async (client, sender) =>
        {
            Logging.Log($"Joined guild: {sender.Guild.Name} (id {sender.Guild.Id})");
            var channel = sender.Guild.Channels[0].Id;
            await client.SendMessageAsync(await client.GetChannelAsync(channel), "Hello!\nI'm here to look for fags joining and leaving!");
        });

        cfg.HandleMessageCreated(async (client, sender) =>
        {
#if !DEBUG
            if (sender.Channel.Id == BotConfigModel.DebugChannel)
                return; // Not in debug mode, this is the release channel
#endif

            var message = sender.Message;

            // if (message.Author is not null && config.UserReactions.TryGetValue(message.Author.Id, out var emoji_id))
            // {
            //     Logging.Log("Using emoji: " + emoji_id.EscapeMarkup());
            //     try
            //     {
            //         if (!DiscordEmoji.TryFromName(client, emoji_id, out var emoji))
            //             Logging.LogError("Failed to locate emoji");
            //         else
            //             await message.CreateReactionAsync(emoji);
            //     }
            //     catch (Exception ex)
            //     {
            //         // Don't do anything if an error occurs, there's no point sending something for EVERY message
            //         AnsiConsole.WriteException(ex);
            //     }
            // }

            // Check alias
            // int index;
            // if ((index = message.Content.IndexOf('$')) != -1)
            // {
            //     foreach (var alias in config.)
            // }
        });
    }
}
