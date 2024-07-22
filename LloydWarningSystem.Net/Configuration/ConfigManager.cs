using DSharpPlus.Commands;
using Newtonsoft.Json;
using Spectre.Console;

namespace LloydWarningSystem.Net.Configuration;

internal static class ConfigManager
{
    private const string botConfigPath = $"./configs/bot-config.json";

    private static JsonSerializer _serializer;
    
    /// <summary>
    /// Configurations specific to the functionality of the bot
    /// </summary>
    public static BotConfigModel BotConfig { get; private set; }

    /// <summary>
    /// Initializes the <see cref="JsonSerializer"/> and loads all configurations.
    /// </summary>
    public static void InitializeConfigs()
    {
        _serializer = new();
        LoadBotConfig().Wait();
    }

    /// <summary>
    /// Loads <see cref="BotConfig"/> from file.
    /// </summary>
    /// <returns></returns>
    public static async Task LoadBotConfig()
    {
        var config = await LoadConfig(botConfigPath);

        if (Equals(config, null))
            Environment.Exit(1);

        BotConfig = config;
    }

    /// <summary>
    /// Saves <see cref="BotConfig"/> to file. (Defaults to <see cref="defaultBotConfigPath"/>)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public static async Task SaveBotConfig(CommandContext? ctx = null)
    {
        if (BotConfig is null)
        {
            if (ctx != null)
                await ctx.RespondAsync("Bot storage is null : Aborting save to prevent overwriting config");

            Logging.LogError("Bot storage is null : Aborting save to prevent overwriting config");
            return;
        }

        SaveConfig(botConfigPath, BotConfig);
    }

    public static void SaveConfig(string path, BotConfigModel config)
    {
        try
        {
            using var sw = new StreamWriter(path);
            using var writer = new JsonTextWriter(sw);
            _serializer.Serialize(writer, config);

            Logging.Log($"Config saved to '{path}'.");
        }
        catch (Exception ex)
        {
            Logging.LogError($"Error saving config to: {path}\nError reason: {ex.Message}");
            AnsiConsole.WriteException(ex);
        }
    }

    public static async Task<BotConfigModel> LoadConfig(string path)
    {
        if (!File.Exists(path))
        {
            Logging.LogError($"No config file found at '{path}'. Creating one with default values.");
            await File.WriteAllTextAsync(path, "{\n}");
            return new BotConfigModel();
        }

        try
        {
            using var sr = new StreamReader(path);
            using var reader = new JsonTextReader(sr);
            BotConfigModel? config = _serializer.Deserialize<BotConfigModel>(reader);

            if (Equals(config, null))
                Logging.LogError($"Failed to deserialize configuration file to type {nameof(BotConfigModel)} from: {path}");
            else
                return config;
        }
        catch (Exception ex)
        {
            Logging.Log($"Error loading config: {ex.Message}");
        }

        return new();
    }
}