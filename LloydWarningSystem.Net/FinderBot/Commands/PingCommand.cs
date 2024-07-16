using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;
using System.ComponentModel;
using System.Diagnostics;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class PingCommand
{
    [Command("ping"), Description("Pings the bot and returns the gateway latency.")]
    public static async Task PingAsync(CommandContext ctx)
    {
        var latency = ctx.Client.GetConnectionLatency(ctx.Guild!.Id);

        await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            .WithTitle(Random.Shared.Next() % 3948 == 0
                ? $"What you want fag?"
                : "Pong!")
            .WithColor(Shared.DefaultEmbedColor)
            .AddField($"Response latency", $"{latency.Milliseconds}ms ({latency.TotalMilliseconds}ms)"));
    }

    [Command("uptime"), Description("Get the bots uptime")]
    public static async Task UptimeAsync(CommandContext ctx)
    {
        await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            .WithTitle("Uptime")
            .WithColor(Shared.DefaultEmbedColor)
            .WithDescription(FormatTickCount()));
    }

    [Command("echo"), Description("Makes the bot create a message with your text")]
    public static async Task EchoAsync(CommandContext ctx, [RemainingText] string message)
    {
        if (!string.IsNullOrEmpty(message))
            await ctx.RespondAsync(message);
    }

    private static string FormatTickCount()
    {
        TimeSpan uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();

        int days = uptime.Days;
        int hours = uptime.Hours;
        int minutes = uptime.Minutes;
        int seconds = uptime.Seconds;

        return $"{days} day{"s".Pluralize(days != 1)}, {hours} hour{"s".Pluralize(hours != 1)}, " +
            $"{minutes} minute{"s".Pluralize(minutes != 1)}, {seconds} second{"s".Pluralize(seconds != 1)} ({uptime.TotalMilliseconds:n0}ms)";
    }
}
