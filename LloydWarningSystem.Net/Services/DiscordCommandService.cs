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
using Humanizer;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;

namespace LloydWarningSystem.Net.Services;

internal class DiscordCommandService : IHostedService
{
    public readonly IDbContextFactory<LloydContext> DbContextFactory;
    public readonly CommandsExtension Commands;
    public readonly DiscordClient Client;
    public DateTime StartTime;

    public DiscordCommandService
    (
        DiscordClient discordClient,
        IDbContextFactory<LloydContext> dbContextFactory,
        BotConfigModel config
    )
    {
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

        //Interactivity
        InteractivityConfiguration interactivityConfig = new()
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
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logging.Log("DiscordClientService started");

        //Update database to latest migration
        await using LloydContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            var sw = Stopwatch.StartNew();

            await context.Database.MigrateAsync(cancellationToken);

            sw.Stop();
            Logging.Log($"Applied pending migrations in {sw.ElapsedMilliseconds} ms");
        }

        Logging.Log("Connecting bot");
        var status = new DiscordActivity("for some bitches", DiscordActivityType.Watching);
        await Client.ConnectAsync(status);

        try
        {
            var assembly = typeof(LloydBot).Assembly;

            LloydBot.startWatch.Stop();

            await Client.SendMessageAsync(await Client.GetChannelAsync(BotConfigModel.DebugChannel),
                $"Bitch finder active. [{LloydBot.startWatch.ElapsedMilliseconds:n0}ms] [{LloydBot.startWatch.ElapsedTicks:n0} ticks] (v{assembly.GetName().Version}, {Program.BuildType}, R{assembly.ImageRuntimeVersion})");
        }
        catch (Exception ex)
        {
            Logging.Log("Failed to send message to debug guild channel: " + BotConfigModel.DebugChannel);
            AnsiConsole.WriteException(ex);
        }
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
}
