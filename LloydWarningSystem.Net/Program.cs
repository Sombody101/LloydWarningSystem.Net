using DSharpPlus;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;
using Spectre.Console;

namespace LloydWarningSystem.Net;

internal static class Program
{
    public const string WebhookUrl = "https://discord.com/api/webhooks/1265402178474872963/trfrbn5rKpkjMxyB4BzIuU60Ez_061zZIXfFx3LsGtUVxph5tfuQs2qvvLfy7Qye1MX7";

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
        Logging.OverrideConsoleLogging();
        Logging.Log($"Bot start @ {DateTime.Now} ({BuildType} build)");

        // The bot has restarted itself, so wait for the previous instance
        // to finish saving data
        if (args.Length > 0 && args[0] == Shared.PreviousInstance)
        {
            Logging.Log("Launching from previous instance : Waiting 1000ms...");
            Task.Delay(1000).Wait();
            Logging.Log("Starting bot.");
        }

        // Load configs and initialize the serializer
        ConfigManager.InitializeConfigs();

        // Initialize webhook
        WebhookClient = new DiscordWebhookClient();
        var webhookUrl = new Uri(WebhookUrl);
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
            var lloyd = new LloydBot();
            await lloyd.RunAsync();
        }
        catch (Exception e)
        {

            if (e is TaskCanceledException)
                return;

            AnsiConsole.WriteException(e);
            await LogToWebhookAsync(e);
            Environment.Exit(69);
        }
    }

    public static async Task LogToWebhookAsync(Exception e)
    {
        var webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername($"Lloyd-{BuildType}")
            .AddEmbed(e.MakeEmbedFromException());

        await WebhookClient.BroadcastMessageAsync(webhookBuilder);
    }
}
