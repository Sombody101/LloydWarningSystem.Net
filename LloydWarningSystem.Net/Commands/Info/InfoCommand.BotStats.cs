using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using Humanizer;
using LloydWarningSystem.Net.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LloydWarningSystem.Net.FinderBot.Commands.Info;

public partial class InfoCommand
{
    private static readonly string _operatingSystem = $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}";
    private static readonly string _botVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
    private static readonly string _dSharpPlusVersion = typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
    private static partial Regex _getLastCommaRegex();

    private readonly AllocationRateTracker _allocationRateTracker = new();

    [Command("bot"), Description("Get statistics about the bot")]
    public async Task GetBotStatsAsync(CommandContext ctx)
    {
        using var process = Process.GetCurrentProcess();
        process.Refresh();

        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.WithTitle("Bot Info");
        embedBuilder.WithColor(Shared.DefaultEmbedColor);

        // Process data
        embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(CultureInfo.InvariantCulture), true);
        embedBuilder.AddField("Process Memory", process.WorkingSet64.Bytes().ToString(CultureInfo.InvariantCulture), true);
        embedBuilder.AddField("Allocation Rate", $"{_allocationRateTracker.AllocationRate.Bytes().ToString(CultureInfo.InvariantCulture)}/s", true);

        embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);
        embedBuilder.AddField("Operating System", _operatingSystem, true);
        embedBuilder.AddField("Uptime", _getLastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);

        embedBuilder.AddField("Threads", $"{ThreadPool.ThreadCount}", true);

        var latency = ctx.Client.GetConnectionLatency(0);
        string latency_value = "N/A (Wait for next heartbeat)";

        if (latency.Milliseconds is not 0)
            _getLastCommaRegex().Replace(latency.Humanize(3), " and ");

        embedBuilder.AddField("Websocket Latency", latency_value, true);

        // Db data
        _ = await _dbContext.Users.FirstOrDefaultAsync();
        var swDb = Stopwatch.StartNew();
        _ = await _dbContext.Guilds.FirstOrDefaultAsync();
        swDb.Stop();

        embedBuilder.AddField("DB Latency:", $"{swDb.ElapsedMilliseconds:n0} ms", true);
        embedBuilder.AddField("DB Size", BytesToString(new FileInfo("./configs/lloyd-bot.db").Length));

        int members = await _dbContext.Users.CountAsync();
        int guilds = await _dbContext.Guilds.CountAsync();

        embedBuilder.AddField("Member Count:", $"{members:n0}", true);
        embedBuilder.AddField("Guild Count:", $"{guilds:n0}", true);

        StringBuilder stringBuilder = new();
        stringBuilder.Append(ctx.Client.CurrentUser.Mention);
        stringBuilder.Append(", ");
        foreach (string prefix in ((DefaultPrefixResolver)ctx.Extension.GetProcessor<TextCommandProcessor>().Configuration.PrefixResolver.Target!).Prefixes)
        {
            stringBuilder.Append('`')
                .Append(prefix)
                .Append("`, ");
        }

        stringBuilder.Append(" `/`");
        embedBuilder.AddField("Prefixes", stringBuilder.ToString(), true);
        embedBuilder.AddField("Bot Version", _botVersion, true);
        embedBuilder.AddField("DSharpPlus Library Version", _dSharpPlusVersion, true);

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embedBuilder).AsEphemeral();

        await ctx.RespondAsync(responseBuilder);
    }

    public static string BytesToString(long value)
    {
        string suffix;
        double readable;
        switch (Math.Abs(value))
        {
            case >= 0x40000000:
                suffix = "GB";
                readable = value >> 20;
                break;
            case >= 0x100000:
                suffix = "MB";
                readable = value >> 10;
                break;
            case >= 0x400:
                suffix = "KB";
                readable = value;
                break;
            default:
                return value.ToString("0 B");
        }

        return $"{readable / 1024:0.##}{suffix}";
    }
}
