using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;
using System.Text;

namespace LloydWarningSystem.Net.Services;

internal class DiscordCommandService : IHostedService
{
    public static DiscordCommandService? StaticInstance { get; private set; }
    public static IDbContextFactory<LloydContext>? DbContextFactory { get; private set; }

    public readonly DiscordClient Client;
    public readonly CommandsExtension Commands;
    public DateTime StartTime;

    public DiscordCommandService
    (
        DiscordClient discordClient,
        IDbContextFactory<LloydContext> dbContextFactory,
        BotConfigModel config
    )
    {
        StaticInstance = this;

        Client = discordClient;
        StartTime = DateTime.Now;
        DbContextFactory = dbContextFactory;

        CommandsConfiguration commandsConfiguration = new()
        {
            DebugGuildId = BotConfigModel.DebugGuild,
            UseDefaultCommandErrorHandler = false,
        };

        Commands = Client.UseCommands(commandsConfiguration);
        _ = Commands.AddProcessorsAsync(new TextCommandProcessor(new TextCommandConfiguration()
        {
            PrefixResolver = new DefaultPrefixResolver(true, config.CommandPrefixes.ToArray()).ResolvePrefixAsync,
        }));

        var assembly = typeof(Program).Assembly;
        Commands.AddCommands(assembly);
        Commands.AddChecks(assembly);
        Commands.CommandErrored += HandleCommandErrored;

        // Interactivity
        var interactivityConfig = new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromMinutes(10),
            PollBehaviour = PollBehaviour.KeepEmojis,
            ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
            PaginationBehaviour = PaginationBehaviour.Ignore,
            ResponseBehavior = InteractionResponseBehavior.Ignore,
            PaginationDeletion = PaginationDeletion.DeleteEmojis
        };

        Client.UseInteractivity(interactivityConfig);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logging.Log("DiscordClientService started");

        //Update database to latest migration
        await using var context = await DbContextFactory!.CreateDbContextAsync(cancellationToken);
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            var sw = Stopwatch.StartNew();

            await context.Database.MigrateAsync(cancellationToken);

            sw.Stop();
            Logging.Log($"Applied pending migrations in {sw.ElapsedMilliseconds:n0} ms");
        }

        Logging.Log("Connecting bot");
        var status = new DiscordActivity("for some bitches", DiscordActivityType.Watching);
        await Client.ConnectAsync(status);

        try
        {
            var assembly = typeof(LloydBot).Assembly;

            LloydBot.startWatch.Stop();

            var init_embed = new DiscordEmbedBuilder()
                .WithTitle("Bitch Finder Active")
                .WithColor(DiscordColor.SpringGreen)
                .AddField("Start time", $"{LloydBot.startWatch.ElapsedMilliseconds:n0}ms", true)
                .AddField("Tick count", $"{LloydBot.startWatch.ElapsedTicks:n0} ticks", true)
                .AddField("Bot version", $"v{assembly.GetName().Version}", true)
                .AddField("Build type", $"***{Program.BuildType}***", true)
                .AddField("Runtime version", $"R{assembly.ImageRuntimeVersion}", true)
                .MakeWide();

            await Client.SendMessageAsync(await Client.GetChannelAsync(BotConfigModel.DebugChannel), init_embed);
        }
        catch (Exception ex)
        {
            Logging.Log("Failed to send message to debug guild channel: " + BotConfigModel.DebugChannel);
            AnsiConsole.WriteException(ex);
            await ex.LogToWebhookAsync();
        }

#if !DEBUG
        Logging.Log("Starting Minecraft Logging Service"); // But really isn't a service
        var sender = new RandomMinecraftSender(Client);
        _ = sender.StartSendingMessages();
#endif
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Client.DisconnectAsync();
    }

    private async Task HandleCommandErrored(CommandsExtension sender, CommandErroredEventArgs e)
    {
        Logging.LogError($"Given command: {e.Context.Command?.Name ?? "$NULL"} [[{e.Context.Command?.FullName ?? "$NULL"}]]");

#if DEBUG
        AnsiConsole.WriteException(e.Exception);
#else
            Logging.LogError(e.Exception);
#endif


        var ex = e.Exception.InnerException ?? e.Exception;

#if DEBUG
        if (e.Context.User.Id == BotConfigModel.AbsoluteAdmin)
            await Client.SendMessageAsync(await Client.GetChannelAsync(BotConfigModel.DebugChannel), ex.MakeEmbedFromException());
#endif

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

                var sb = new StringBuilder();
                foreach (var reason in checks.Errors)
                    sb.Append(reason.ErrorMessage).Append('\n');

                embed.AddField("Reason", sb.ToString().TrimEnd());

                await e.Context.RespondAsync(embed);
                break;

            default:
                await e.Context.RespondAsync("Uh oh!\nSomething went wrong!");
                break;
        }
    }
}
