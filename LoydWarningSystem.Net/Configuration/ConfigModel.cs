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
}

[Serializable]
internal class UserStorageModel : IConfigModel
{
    [JsonProperty("attention_users")]
    public Dictionary<ulong, string> AttentionUsers { get; init; } = [];

    [JsonProperty("user_reactions")]
    public Dictionary<ulong, string> UserReactions { get; init; } = [];

    [JsonProperty("bot_admins")]
    public Dictionary<ulong, string> BotAdmins { get; init; } = new() {
        { BotConfigModel.AbsoluteAdmin, "oke" }
    };

    [JsonProperty("warning_messages")]
    public List<string> WarningMessages { get; init; } = [];

    [JsonProperty("user_seeking_enabled")]
    public bool LookForJoiningUsers { get; set; } = true;
}
