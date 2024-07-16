using DSharpPlus.Commands;
using Newtonsoft.Json;
using Spectre.Console;

namespace LloydWarningSystem.Net.Configuration;

internal static class ConfigManager
{
    private const string defaultBotConfigPath = "./configs/bot-config.json";
    private const string defaultUserStoragePath = "./configs/user-storage.json";

    private static JsonSerializer _serializer;

    private static int lastSaveHash = 0;

    /// <summary>
    /// Configurations for all user based configurations
    /// </summary>
    public static UserStorageModel UserStorage { get; private set; }

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

        Directory.CreateDirectory("./configs/");

        LoadBotConfig().Wait();
        LoadUserStorage().Wait();
    }

    /// <summary>
    /// Loads <see cref="BotConfig"/> from file.
    /// </summary>
    /// <returns></returns>
    public static async Task LoadBotConfig()
    {
        var config = await LoadConfig<BotConfigModel>(defaultBotConfigPath);

        if (Equals(config, default(BotConfigModel)))
            Environment.Exit(1);

        BotConfig = config;
    }

    /// <summary>
    /// Saves <see cref="BotConfig"/> to file. (Defaults to <see cref="defaultBotConfigPath"/>)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public static async Task SaveBotConfig(string path = defaultBotConfigPath, CommandContext? ctx = null)
    {
        if (BotConfig == null)
        {
            if (ctx != null)
                await ctx.RespondAsync("Bot storage is null : Aborting save to prevent overwriting config");

            Logging.LogError("Bot storage is null : Aborting save to prevent overwriting config");
            return;
        }

        if (ctx != null && !await ctx.UserIsAdmin())
            return; // Just return right away if not an admin

        SaveConfig(path, BotConfig);
    }

    /// <summary>
    /// Load <see cref="UserStorage"/> from file
    /// </summary>
    /// <returns></returns>
    public static async Task LoadUserStorage()
    {
        var config = await LoadConfig<UserStorageModel>(defaultUserStoragePath);

        if (Equals(config, default(UserStorageModel)))
            Environment.Exit(1);

        UserStorage = config;
    }

    /// <summary>
    /// Save <see cref="UserStorage"/> to file. (Defaults to <see cref="defaultUserStoragePath"/>)
    /// </summary>
    /// <param name="path"></param>
    public static void SaveUserStorage(string path = defaultUserStoragePath)
    {
        if (UserStorage == null)
        {
            Logging.LogError("User storage is null : Aborting save to prevent overwriting config");
            return;
        }

        int newHash = UserStorage.GetHashCode();
        if (newHash != lastSaveHash)
            return;

        lastSaveHash = newHash;

        lock (UserStorage)
        {
            SaveConfig(path, UserStorage);
        }
    }

    public static void SaveConfig<ConfigType>(string path, ConfigType config)
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

    public static async Task<ConfigType> LoadConfig<ConfigType>(string path = defaultBotConfigPath) 
        where ConfigType : class, new()
    {
        if (!File.Exists(path))
        {
            Logging.LogError($"No config file found at '{path}'. Creating one with default values.");
            await File.WriteAllTextAsync(path, "{\n}");
            return new ConfigType();
        }

        try
        {
            using var sr = new StreamReader(path);
            using var reader = new JsonTextReader(sr);
            ConfigType? config = _serializer.Deserialize<ConfigType>(reader);

            if (Equals(config, default(ConfigType)))
                Logging.LogError($"Failed to deserialize configuration file to type {typeof(ConfigType).Name} from: {path}");
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