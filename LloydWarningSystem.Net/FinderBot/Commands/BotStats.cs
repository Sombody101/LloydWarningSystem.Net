using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using LloydWarningSystem.Net.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public class BotStats
{
    private readonly LloydContext _dbContext;

    public BotStats(LloydContext dbcontext)
    {
        _dbContext = dbcontext;
    }

    [Command("botstats"), Description("Get statistics about the bot")]
    public async Task GetBotStatsAsync(CommandContext ctx)
    {
        _ = await _dbContext.Users.FirstOrDefaultAsync();
        var swDb = Stopwatch.StartNew();
        _ = await _dbContext.Guilds.FirstOrDefaultAsync();
        swDb.Stop();

        using var process = Process.GetCurrentProcess();

        int members = await _dbContext.Users.CountAsync();
        int guilds = await _dbContext.Guilds.CountAsync();

        double ping = ctx.Client.GetConnectionLatency(0).TotalMicroseconds;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        string heapMemory = $"{process.PrivateMemorySize64 / 1024 / 1024} MB";

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Statistics")
            .WithColor(Shared.DefaultEmbedColor)
            .AddField("Membercount:", $"{members:n0}", true)
            .AddField("Guildcount:", $"{guilds:n0}", true)
            .AddField("Threads:", $"{ThreadPool.ThreadCount}", true)
            .AddField("Websocket Latency:", $"{ping:n0} ms", true)
            .AddField("DB Latency:", $"{swDb.ElapsedMilliseconds:n0} ms", true)
            .AddField("Memory:", heapMemory, true)
            .AddField("Uptime:", $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).TotalSeconds:n0} seconds", true);

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed).AsEphemeral();

        await ctx.RespondAsync(responseBuilder);
    }
}
