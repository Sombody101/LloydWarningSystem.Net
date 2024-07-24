using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public class UserReactionCommand
{
    private readonly LloydContext _dbContext;

    public UserReactionCommand(LloydContext dbcontext)
    {
        _dbContext = dbcontext;
    }

    [Command("reaction")]
    public async Task AddReactionAsync(CommandContext ctx, DiscordEmoji emoji)
    {
        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);
        var emoji_name = emoji.GetDiscordName();

        //if (user.ReactionEmoji == emoji_name)
        //{
        //    await ctx.RespondAsync("You already have that emoji set!");
        //    return;
        //}
        //
        //user.ReactionEmoji = emoji_name;
        // wait _dbContext.SaveChangesAsync();
        // wait ctx.RespondAsync($"Now reacting with {emoji.Name} (`{emoji.GetDiscordName()}`)");
    }
}
