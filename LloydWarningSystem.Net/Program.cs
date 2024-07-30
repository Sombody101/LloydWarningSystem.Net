using DSharpPlus;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Spectre.Console;

namespace LloydWarningSystem.Net;

internal static class Program
{
    public static DiscordWebhookClient WebhookClient = null!;

    public const bool DebugBuild =
#if DEBUG
        true;
#else
        false;
#endif

    public const string BuildType = DebugBuild
            ? "Debug"
            : "Release";

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .CreateLogger();

        Logging.OverrideConsoleLogging();
        Logging.Log($"Bot start @ {DateTime.Now} ({BuildType} build)");

#if DEBUG
        // The bot has restarted itself, so wait for the previous instance
        // to finish saving data
        if (args.Length > 0 && args[0] == Shared.PreviousInstance)
        {
            Logging.Log("Launching from previous instance : Waiting 1000ms...");
            Task.Delay(1000).Wait();
            Logging.Log("Starting bot.");
        }
#endif

        // Load configs and initialize the serializer
        ConfigManager.InitializeConfigs();

        // Initialize webhook
        WebhookClient = new DiscordWebhookClient();
        var webhookUrl = new Uri(ConfigManager.BotConfig.DiscordWebhookUrl);
        await WebhookClient.AddWebhookAsync(webhookUrl);

        // On close, save files
        AppDomain.CurrentDomain.ProcessExit += (e, sender) =>
        {
            Logging.Log($"[[Exit@ {DateTime.Now}]] Saving all configs...");

            // Ensure all configs are saved
            ConfigManager.SaveBotConfig().Wait();
        };

        try
        {
            // Start the bot
            await LloydBot.RunAsync();
        }
        catch (Exception ex)
        {
            var e = ex.InnerException ?? ex;

            if (e is TaskCanceledException)
                return;

            AnsiConsole.WriteException(e);
            await e.LogToWebhookAsync();
            Environment.Exit(69);
        }
    }

    /// <summary>
    /// Used for ECF CLI Migration tools
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((_, services) => services.AddDbContextFactory<LloydContext>(
            options => options.UseSqlite(LloydBot.DbConnectionString)
        ));

        return builder;
    }
}
