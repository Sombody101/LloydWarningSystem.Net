﻿using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;
using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot.Commands;

[Command("react")]
public class UserReactionCommand
{
    private readonly LloydContext _dbContext;

    public UserReactionCommand(LloydContext dbcontext)
    {
        _dbContext = dbcontext;
    }

    [Command("add"), DefaultGroupCommand]
    public async Task AddReactionAsync(CommandContext ctx, DiscordEmoji emoji)
    {
        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);
        var emoji_name = emoji.GetDiscordName();

        if (user.ReactionEmoji == emoji_name)
        {
            await ctx.RespondAsync("You already have that emoji set!");
            return;
        }
        
        user.ReactionEmoji = emoji_name;
        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"Now reacting with {emoji.Name} (`{emoji.GetDiscordName()}`)");
    }

    [Command("clear"), Description("Clears the reaction emoji (AKA: Stops the reactions)")]
    public async Task RemoveReactionAsync(CommandContext ctx)
    {
        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);

        if (user.ReactionEmoji == string.Empty)
        {
            await ctx.RespondAsync("You don't have any emoji set!");
            return;
        }

        user.ReactionEmoji = string.Empty;
        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync("Reaction emoji cleared!");
    }
}
