using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using LloydWarningSystem.Net.Configuration;
using Newtonsoft.Json;

namespace LloydWarningSystem.Net.FinderBot.Commands;

[Command("alias")]
public static class AliasManagerCommand
{
    [Command("add")]
    public static async Task AddAliasAsync(CommandContext ctx, string alias_name, [RemainingText] string alias_content)
    {
        if (alias_name.StartsWith('$'))
        {
            await ctx.RespondAsync("You cannot have an alias name start with a dollar sign ($)!");
            return;
        }

        //var exiting_index = AliasStorage.DefinedAliases.
    }
}

internal static class AliasStorage
{
    private static int lastAliasCount = 0;

    public const string AliasStoragePath = "./defined-aliases.json";

    public static List<Alias> DefinedAliases { get; set; } = [];

    public static List<Alias> LoadAliases()
        => ConfigManager.LoadConfig<List<Alias>>(AliasStoragePath).Result;

    public static void SaveAliases(List<Alias> aliases)
    {
        if (lastAliasCount == DefinedAliases.Count && AliasesHasChanged())
        {
            Logging.Log("Aborting alias save : No changes found");
            return;
        }

        ConfigManager.SaveConfig(AliasStoragePath, aliases);
    }

    private static bool AliasesHasChanged()
        => DefinedAliases.Exists(alias => alias.HasChanged);

    [Serializable]
    internal class Alias
    {
        [JsonProperty("a")]
        public string AliasName { get; init; } = string.Empty;

        [JsonProperty("b")]
        public string AliasContent { get; init; } = string.Empty;

        [JsonIgnore]
        public bool HasChanged { get; set; } = false;
    }
}