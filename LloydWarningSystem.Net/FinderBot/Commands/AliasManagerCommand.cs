using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;

namespace LloydWarningSystem.Net.FinderBot.Commands;

[Command("alias")]
public class AliasManagerCommand
{
    private readonly LloydContext _dbContext;

    public AliasManagerCommand(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Command("set")]
    public async Task SetAliasAsync(CommandContext ctx, string alias_name, [RemainingText] string alias_content)
    {
        Logging.Log(_dbContext is null);

        if (alias_name.StartsWith('$'))
        {
            await ctx.RespondAsync("You cannot have an alias name start with a dollar sign ($)!");
            return;
        }

        //var exiting_index = AliasStorage.DefinedAliases.
    }

    [Command("test")]
    public async Task TestAsync(CommandContext ctx, DiscordUser usr)
    {
        var user = new UserDbEntity()
        {
            Username = usr.Username,
            Id = usr.Id,
        };

        await _dbContext.Users.AddAsync(user);
        await ctx.RespondAsync($"Added user {usr.Username} to test database");
    }

    [Command("test2")]
    public async Task Test2Async(CommandContext ctx)
    {
        var user = _dbContext.Users.FirstOrDefault();

        if (user is null)
        {
            await ctx.RespondAsync("No user found in database");
            return;
        }

        await ctx.RespondAsync(new DiscordEmbedBuilder()
            .WithTitle("User found at index 0")
            .AddField("Username", user.Username)
            .AddField("ID", user.Id.ToString()));
    }
}