using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using LloydWarningSystem.Net.CommandChecks;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace LloydWarningSystem.Net.FinderBot;

// https://discord.com/oauth2/authorize?client_id=1261096139701227633&permissions=964220676096&integration_type=0&scope=bot

internal static class LloydBot
{
    //public const string connectionString = "Data Source=./configs/lloyd-bot.db";

    public static IServiceProvider Services;
    public static DiscordClient Client { get; private set; }
    public static CommandsExtension Commands { get; private set; }
    public static DateTime StartTime { get; private set; }

    public static async Task StartBot()
    {
        throw new NotImplementedException("Dont use this pls");

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

        var builder = DiscordClientBuilder.CreateDefault(token,
            TextCommandProcessor.RequiredIntents
            | SlashCommandProcessor.RequiredIntents
            | DiscordIntents.MessageContents
            | DiscordIntents.GuildMembers)
            .ConfigureServices((services) =>
            {
                services.AddDbContext<LloydContext>(options =>
                {
                    // options.UseSqlite(connectionString);
                    Logging.Log("DbContext service registered");
                });
            });

        InitializeEvents(builder);

        var client = builder.Build();
        Client = client;

        // Use the commands extension
        Commands = client.UseCommands(new CommandsConfiguration()
        {
            DebugGuildId = BotConfigModel.DebugGuild, // _bin
            RegisterDefaultCommandProcessors = true,
            UseDefaultCommandErrorHandler = false,
        });

        Commands.CommandErrored += HandleCommandErrored;
        Commands.AddCommands(typeof(Program).Assembly);
        await Commands.AddProcessorsAsync(new TextCommandProcessor(new TextCommandConfiguration()
        {
            PrefixResolver = new DefaultPrefixResolver(true, config.CommandPrefixes.ToArray()).ResolvePrefixAsync
        }));

        Commands.AddCheck<EnsureDBEntitiesCheck>();
        Commands.AddCheck<RequireOwnerCheck>();

        // Interactivity
        var interactivityConfig = new InteractivityConfiguration()
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(10),
            ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
            PaginationBehaviour = PaginationBehaviour.Ignore,
            ResponseBehavior = InteractionResponseBehavior.Ignore,
            ResponseMessage = "invalid interaction",
            PaginationDeletion = PaginationDeletion.DeleteEmojis
        };

        Client.UseInteractivity(interactivityConfig);

        var status = new DiscordActivity("for some bitches", DiscordActivityType.Watching);
        await Client.ConnectAsync(status);

        try
        {
            var assembly = typeof(LloydBot).Assembly;

            await client.SendMessageAsync(await client.GetChannelAsync(BotConfigModel.DebugChannel),
                $"Bitch finder active. (v{assembly.GetName().Version}, {Program.BuildType}, R{assembly.ImageRuntimeVersion})");
        }
        catch (Exception ex)
        {
            Logging.Log("Failed to send message to debug guild channel: " + BotConfigModel.DebugChannel);
            AnsiConsole.WriteException(ex);
        }

        Logging.Log("Bot ready for commands.");
        await Task.Delay(-1);
    }

    private static async Task HandleCommandErrored(CommandsExtension sender, CommandErroredEventArgs e)
    {
        Logging.LogError($"Given command: {e.Context.Command?.Name ?? "$NULL"} [[{e.Context.Command?.FullName ?? "$NULL"}]]");

#if DEBUG
        AnsiConsole.WriteException(e.Exception);
#else
            Logging.LogError(e.Exception);
#endif

        var ex = e.Exception.InnerException ?? e.Exception;

        if (e.Context.User.Id == BotConfigModel.AbsoluteAdmin)
        {
            var ex_message = new DiscordEmbedBuilder()
                .WithTitle("Bot Exception")
                .WithColor(DiscordColor.Red)
                .AddField("Exception Type", ex.GetType().Name)
                .AddField("Exception Message", ex.Message, false)
                .AddField("Exception Source", ex.Source ?? "$NO_EXCEPTION_SOURCE")
                .AddField("Stack Trace", $"```\n{ex.StackTrace ?? "$NO_STACK_TRACE"}\n```", false)
                .AddField("HResult", ex.HResult.ToString())
                .AddField("Base", ex.TargetSite?.Name ?? "$NO_BASE_METHOD");

            await Client.SendMessageAsync(await Client.GetChannelAsync(BotConfigModel.DebugChannel), embed: ex_message.Build());
        }

        switch (ex)
        {
            case CommandNotFoundException cex:
                await e.Context.RespondAsync(new DiscordEmbedBuilder()
                    .WithTitle("Unknown command!")
                    .AddField(cex.CommandName, cex.Message)
                    .WithFooter("Use `/help` for a list of commands"));
                break;

            case ArgumentParseException:
                await e.Context.RespondAsync(ex.Message);
                break;

            case ChecksFailedException checks:
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("You cannot run this command!")
                    .WithColor(DiscordColor.Red);

                foreach (var reason in checks.Errors)
                    embed.AddField($"1. {reason.ContextCheckAttribute.GetType().Name.Humanize()}", reason.ErrorMessage);

                await e.Context.RespondAsync(embed);
                break;

            default:
                await e.Context.RespondAsync("Uh oh!\nSomething went wrong!");
                break;
        }
    }

    /// <summary>
    /// Implement important Guild based events
    /// </summary>
    /// <param name="client"></param>
    private static void InitializeEvents(DiscordClientBuilder client)
    {
        client.ConfigureEventHandlers(cfg =>
        {
            cfg.HandleGuildMemberAdded(async (client, sender) =>
            {
                // if (config.AttentionUsers.ContainsKey(client.CurrentUser.Id))
                // {
                //     await client.SendMessageAsync(await client.GetChannelAsync(1240285322814685265), "lloyd is here, bitches.");
                //     await client.SendMessageAsync(await client.GetChannelAsync(1187635474798497843), "lloyd is here, bitches.");
                // }
            });

            cfg.HandleGuildMemberRemoved(async (client, sender) =>
            {
                // if (config.AttentionUsers.ContainsKey(client.CurrentUser.Id))
                // {
                //     await client.SendMessageAsync(await client.GetChannelAsync(1240285322814685265), "LLOYD LEFT\n\nTIME TO PARTAY");
                //     await client.SendMessageAsync(await client.GetChannelAsync(1187635474798497843), "LLOYD LEFT\n\nTIME TO PARTAY");
                //     return;
                // }

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
        });
    }
}
