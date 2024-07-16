using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.FinderBot;

namespace LloydWarningSystem.Net;

internal static class Program
{
    static async Task Main(string[] args)
    {
        Logging.OverrideConsoleLogging();

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

        // On close, save files
        AppDomain.CurrentDomain.ProcessExit += (e, sender) =>
        {
            Logging.Log("Saving all configs...");

            // Ensure all configs are saved
            ConfigManager.SaveBotConfig().Wait();
            ConfigManager.SaveUserStorage();
        };

        // Start the bot
        await LloydBot.StartBot();
    }
}
