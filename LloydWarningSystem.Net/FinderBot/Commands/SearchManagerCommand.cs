using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;
using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class SearchManagerCommand
{
    [Command("searching"), Description("Gets or sets if the bot should check for joining users")]
    public static async Task SearchAsync(CommandContext ctx, string enabled = "show")
    {
        switch (enabled)
        {
            case "show":
                await ctx.RespondAsync(ConfigManager.UserStorage.LookForJoiningUsers
                    ? "Still searching."
                    : "Not searching.");
                return;

            case "enable":
                ConfigManager.UserStorage.LookForJoiningUsers = true;
                await ctx.RespondAsync("Now searching.");
                return;

            case "disable":
                ConfigManager.UserStorage.LookForJoiningUsers = false;
                await ctx.RespondAsync("Stopped searching.");
                return;

            default:
                await ctx.RespondAsync($"Unknown option '{enabled}'.\nUse `enable` or `disable` to set the configuration.");
                return;
        }
    }

    [Command("wanted"), Description("Shows all users currently being tracked")]
    public static async Task PrintLookingForAsync(CommandContext ctx)
    {
        var builder = new DiscordEmbedBuilder()
        {
            Title = "Users being tracked.",
            Color = DiscordColor.Teal,
        };

        foreach (var user in ConfigManager.UserStorage.AttentionUsers)
            builder.AddField((await ctx.Guild.GetMemberAsync(user.Key)).Username ?? user.Value ?? "N/A", user.Key.ToString());

        await ctx.RespondAsync(embed: builder.Build());
    }

    [Command("track"), Description("Adds a user to the wanted list")]
    public static async Task AddUser(CommandContext ctx, params ulong[] user_ids)
    {
        var config = ConfigManager.UserStorage;

        foreach (var user_id in user_ids)
        {
            var user = await ctx.Guild.GetMemberAsync(user_id);

            if (user == null)
            {
                await ctx.RespondAsync($"Failed to find any users by the ID '{user_id}'");
                continue;
            }

            config.AttentionUsers.Add(user_id, user.Username);
        }
    }

    [Command("forcetrack"), Description("Force adds a user to the wanted list")]
    public static async Task ForceAddUser(CommandContext ctx, params ulong[] user_ids)
    {
        var config = ConfigManager.UserStorage;

        foreach (var user_id in user_ids)
        {
            var user = await ctx.Guild.GetMemberAsync(user_id);

            config.AttentionUsers.Add(user_id, user.Username);
        }
    }
}
