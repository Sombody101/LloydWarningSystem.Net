using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.CommandChecks.Attributes;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

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

    [Command("mod"), RequireAdminUser]
    public class AfkModCommand
    {
        private readonly LloydContext _dbContext;

        public AfkModCommand(LloydContext _db)
        {
            _dbContext = _db;
        }

        [Command("set")]
        public async ValueTask ModSetAfkStatusAsync(CommandContext ctx, DiscordUser user, [RemainingText][MinMaxLength(0, 70)] string status)
        {
            var dbuser = await _dbContext.FindOrCreateUserAsync(user);

            var afkStatus = new AfkStatusEntity()
            {
                AfkEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                AfkMessage = status,
            };

            dbuser.AfkStatus = afkStatus;
            await _dbContext.SaveChangesAsync();
            await ctx.RespondAsync($"I've set {user.Mention}'s AFK status: {status}");
        }

        [Command("clear"), TextAlias("unset")]
        public async ValueTask ModClearAfkStatusAsync(CommandContext ctx, DiscordUser user)
        {
            var dbuser = await _dbContext.FindOrCreateUserAsync(user);

            dbuser.AfkStatus = null;
            await _dbContext.SaveChangesAsync();
            await ctx.RespondAsync($"I've cleared {user.Mention}'s AFK status.");
        }
    }
}
