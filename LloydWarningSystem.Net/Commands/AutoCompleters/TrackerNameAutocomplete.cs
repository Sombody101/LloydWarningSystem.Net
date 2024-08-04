using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;
using Microsoft.EntityFrameworkCore;

namespace LloydWarningSystem.Net.Commands.AutoCompleters;

internal class TrackerNameAutocomplete : IAutoCompleteProvider
{
    private readonly LloydContext _dbContext;

    public TrackerNameAutocomplete(LloydContext _db)
    {
        _dbContext = _db;
    }

    public async ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext ctx)
        => await _dbContext
            .Set<TrackingDbEntity>()
            .Where(x => x.GuildId == ctx.Guild.Id && x.Name.Contains(ctx.UserInput))
            .OrderBy(x => x.Name.IndexOf(ctx.UserInput))
            .Take(25)
            .ToDictionaryAsync(x => x.Name, x => (object)x.Name);

}
