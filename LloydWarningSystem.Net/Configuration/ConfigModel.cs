using Newtonsoft.Json;

namespace LloydWarningSystem.Net.Configuration;

internal class BotConfigModel : IBotConfigModel
{
    public const long AbsoluteAdmin = 518296556059885599;
    public const ulong DebugGuild = 1052365935987671210;
    public const ulong DebugChannel =
#if DEBUG
        1178519504209334323; // bot-testing-debug
#else
        1262569946312081488; // bot-testing-release
#endif

    [JsonRequired]
    [JsonProperty("bot_token")]
    public string BotToken { get; init; } = string.Empty;

    [JsonRequired]
    [JsonProperty("bot_token_debug")]
    public string DebugBotToken { get; init; } = string.Empty;

    [JsonRequired]
    [JsonProperty("command_prefixes")]
    public List<string> CommandPrefixes { get; init; } = [];

    [JsonProperty("webhook_url")]
    public string DiscordWebhookUrl { get; init; } = string.Empty;

    [JsonProperty("repl_url")]
    public string ReplUrl { get; init; } = Program.DebugBuild
        ? "http://server.lan:31337/eval" // Connect to server from dev machine
        : "http://localhost:31337/eval"; // Running from server
}