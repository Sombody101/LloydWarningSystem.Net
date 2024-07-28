using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.FinderBot.Commands;

[Command("alias")]
public class TagManagerCommand
{
    private readonly LloydContext _dbContext;

    public TagManagerCommand(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Command("set")]
    public async Task SetTagAsync(CommandContext ctx, string alias_name, [RemainingText] string alias_content)
    {
        Logging.Log(_dbContext is null);

        if (alias_name.StartsWith('$'))
        {
            await ctx.RespondAsync("You cannot have an alias name start with a dollar sign ($)!");
            return;
        }

        //var exiting_index = AliasStorage.DefinedAliases.
    }
}