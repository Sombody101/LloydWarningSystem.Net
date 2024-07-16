using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class UserReactionCommand
{
    [Command("reaction")]
    public static async Task AddReactionAsync(CommandContext ctx, DiscordEmoji emoji)
    {
        var config = ConfigManager.UserStorage;

        if (config.UserReactions.TryGetValue(ctx.User.Id, out var emoji_id)
            && emoji_id == emoji.GetDiscordName())
        {
            await ctx.RespondAsync("You already have that emoji set!");
            return;
        }

        config.UserReactions.Add(ctx.User.Id, emoji.GetDiscordName());
        await ctx.RespondAsync($"Now reacting with {emoji.Name} (`{emoji.GetDiscordName()}`)");
    }
}
