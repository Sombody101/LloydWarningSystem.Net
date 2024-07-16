namespace LloydWarningSystem.Net.Configuration;

internal interface IBotConfigModel
{
    public string BotToken { get; }

    public List<string> CommandPrefixes { get; }
}

internal interface IConfigModel
{
    public Dictionary<ulong, string> AttentionUsers { get; }

    public Dictionary<ulong, string> BotAdmins { get; }

    public Dictionary<ulong, string> UserReactions { get; }

    public List<string> WarningMessages { get; }

    public bool LookForJoiningUsers { get; set; }
}