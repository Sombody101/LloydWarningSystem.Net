using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Models;
using LloydWarningSystem.Net.Context;
using Microsoft.EntityFrameworkCore;

namespace LloydWarningSystem.Net.CommandChecks;

public class EnsureDBEntitiesCheck : IContextCheck<UnconditionalCheckAttribute>
{
    private IDbContextFactory<LloydContext> _contextFactory;

    public EnsureDBEntitiesCheck(IDbContextFactory<LloydContext> dbContextFactory)
    {
        _contextFactory = dbContextFactory;
    }

    public async ValueTask<string?> ExecuteCheckAsync(UnconditionalCheckAttribute _, CommandContext context)
    {
        DiscordUser user = context.User;

        await using LloydContext dbContext = await _contextFactory.CreateDbContextAsync();

        UserDbEntity userdbEntity = new()
        {
            Id = user.Id,
            Username = user.Username,
        };

        await dbContext.Users.Upsert(userdbEntity)
            .On(x => x.Id)
            .NoUpdate()
            .RunAsync();

        if (context.Guild is null)
        {
            return null;
        }

        GuildDbEntity guildDbEntity = new(context.Guild.Id);

        await dbContext.Guilds.Upsert(guildDbEntity)
            .On(x => x.Id)
            .NoUpdate()
            .RunAsync();

        await dbContext.SaveChangesAsync();
        return null;
    }
}