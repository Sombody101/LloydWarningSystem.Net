using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;
using Microsoft.EntityFrameworkCore;

namespace LloydWarningSystem.Net.FinderBot.Commands;

[Command("alias")]
public class AliasManagerCommand
{
    private readonly LloydContext _dbContext;

    public AliasManagerCommand(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Command("set"), TextAlias("add"), DefaultGroupCommand]
    public async Task SetAliasAsync(CommandContext ctx, string alias_name, [RemainingText] string alias_content)
    {
        if (alias_name.StartsWith('$'))
        {
            await ctx.RespondAsync("You cannot have an alias name start with a dollar sign ($)!");
            return;
        }

        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);
        user.MessageAliases ??= [];

        var found_alias = await _dbContext.Set<MessageTag>().Where(tag => tag.Name == alias_name && tag.UserId == ctx.User.Id)
            .FirstOrDefaultAsync();

        if (found_alias is null)
        {
            user.MessageAliases.Add(new()
            {
                User = user,
                Data = alias_content,
                Name = alias_name,
                UserId = ctx.User.Id
            });

            await _dbContext.SaveChangesAsync();
            await ctx.RespondAsync($"Created alias `{alias_name}`!");
            return;
        }

        found_alias.Data = alias_content;

        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"Updated alias `{alias_name}`!");
    }

    [Command("remove"), TextAlias("delete")]
    public async Task RemoveAliasAsync(CommandContext ctx, string alias_name)
    {
        if (alias_name.StartsWith('$'))
        {
            await ctx.RespondAsync("You cannot have an alias name start with a dollar sign ($)!");
            return;
        }

        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);
        user.MessageAliases ??= [];

        var user_tags = _dbContext.Set<MessageTag>().Where(tag => tag.UserId == ctx.User.Id);

        if (await user_tags.CountAsync() is 0)
        {
            await ctx.RespondAsync("You don't have any aliases set!");
            return;
        }

        var alias = await user_tags.Where(tag => tag.Name == alias_name).FirstOrDefaultAsync();

        if (alias is null)
        {
            await ctx.RespondAsync($"You don't have an alias by the name of '{alias_name}`!");
            return;
        }

        user.MessageAliases.Remove(alias);
        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"Removed `{alias_name}`");
    }

    [Command("list")]
    public async Task ListAliasesAsync(CommandContext ctx)
    {
        var user = await _dbContext.FindOrCreateUserAsync(ctx.User);
        user.MessageAliases ??= [];

        var set_aliases = await _dbContext.Set<MessageTag>().Where(tag => tag.UserId == ctx.User.Id).ToArrayAsync();

        if (set_aliases.Length is 0)
        {
            await ctx.RespondAsync("You don't have any set aliases!");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Your aliases");

        foreach (var alias in set_aliases)
            embed.AddField(alias.Name, $"{alias.Data.Length} bytes long");

        await ctx.RespondAsync(embed);
    }
}