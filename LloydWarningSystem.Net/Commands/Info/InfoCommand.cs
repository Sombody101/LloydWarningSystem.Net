using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.Commands.Info;

[Command("info"), RequirePermissions(DiscordPermissions.EmbedLinks, DiscordPermissions.None)]
public sealed partial class InfoCommand
{
    private readonly LloydContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="InfoCommand"/>.
    /// </summary>
    /// <param name="imageUtilitiesService">Required service for fetching image metadata.</param>
    /// <param name="allocationRateTracker">Required service for tracking the memory allocation rate.</param>
    public InfoCommand(AllocationRateTracker allocationRateTracker, LloydContext dbcontext)
    {
        _allocationRateTracker = allocationRateTracker;
        _dbContext = dbcontext;
    }
}