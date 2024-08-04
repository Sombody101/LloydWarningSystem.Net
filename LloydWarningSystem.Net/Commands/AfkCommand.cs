using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.CommandChecks.Attributes;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using System.Runtime.InteropServices;

namespace LloydWarningSystem.Net.Commands;

[Command("afk")]
public class AfkCommand
{
    private readonly LloydContext _dbContext;

    public AfkCommand(LloydContext _db)
    {
        _dbContext = _db;
    }

    [Command("set"), DefaultGroupCommand]
    public async ValueTask SetAfkStatusAsync(CommandContext ctx, [RemainingText][MinMaxLength(0, 70)] string status)
    {
        var afkStatus = await _dbContext.Set<AfkStatusEntity>().FirstOrDefaultAsync(stat => stat.UserId == ctx.User.Id);

        if (afkStatus is not null)
            return;

        var dbuser = await _dbContext.FindOrCreateUserAsync(ctx.User);

        afkStatus = new AfkStatusEntity()
        {
            AfkEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AfkMessage = status,
        };

        dbuser.AfkStatus = afkStatus;
        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"I've set your AFK status: {status}");
    }

    public async ValueTask ListAfkUsers(CommandContext ctx, [Optional] ulong? guild_id)
    {
        guild_id ??= ctx.Guild?.Id;

        if (guild_id is null)
        {
            await ctx.RespondAsync("Error fetching guild! Try again later!");
            return;
        }

        var dbGuild = await _dbContext.Set<GuildDbEntity>().FirstOrDefaultAsync(guild => guild.Id == guild_id);

        if (dbGuild is null)
        {
            await ctx.RespondAsync("That guild is not in my database!\nAm I a member of it?");
            return;
        }

        // var afkUsers = _dbContext.Set<UserDbEntity>().Where(user => user.Id == dbGuild.);
    }
}
