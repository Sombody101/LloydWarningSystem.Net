using DSharpPlus.Entities;
using LloydWarningSystem.Net.Commands;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Services;
using Spectre.Console;
using System.Runtime.InteropServices;

namespace LloydWarningSystem.Net;

public static class Shared
{
#if DEBUG
    // Only used in debug builds for the '!restart' command
    public const string PreviousInstance = "from_previous_instance";
#endif

    // Runtime constants
    public static readonly DiscordColor DefaultEmbedColor = new(0xFFE4B5);

    public static string GenerateModalId()
        => $"modal-{Random.Shared.Next():X4}";

    public static DiscordEmbedBuilder MakeEmbedFromException(this Exception e)
    {
        var ex = e.InnerException ?? e;

        var ex_message = new DiscordEmbedBuilder()
            .WithTitle($"Bot Exception [From {Program.BuildType} Build]")
            .WithColor(DiscordColor.Red)
            .AddField("Exception Type", ex.GetType().Name, true)
            .AddField("Exception Message", ex.Message, true)
            .AddField("Exception Source", ex.Source ?? "$NO_EXCEPTION_SOURCE", true)
            .AddField("HResult", ex.HResult.ToString(), true)
            .AddField("Base", ex.TargetSite?.Name ?? "$NO_BASE_METHOD", true)
            .AddField("Stack Trace", $"```\n{ex.StackTrace ?? "$NO_STACK_TRACE"}\n```")
            .WithFooter($"Uptime: {PingCommand.FormatTickCount()}");

        return ex_message;
    }

    public static string CurrentUnixTimestampTag()
        => $"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:R>";

    public static DiscordEmbedBuilder MakeWide(this DiscordEmbedBuilder embed)
    {
        if (embed.ImageUrl is not null)
            throw new ArgumentException("Embed already has an image set; Cannot add wide image.");

        return embed.WithImageUrl("https://files.forsaken-borders.net/transparent.png");
    }

    public static async Task PrintException(this Exception e, [Optional] Type? sender)
    {
        AnsiConsole.WriteException(e);
        await LogToWebhookAsync(e, sender);
    }

    public static async Task LogToWebhookAsync(this Exception e, object sender)
    {
        await LogToWebhookAsync(e, sender?.GetType() ?? typeof(void));
    }

    public static async Task LogToWebhookAsync(this Exception e, [Optional] Type? sender)
    {
        var webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername($"Lloyd-{Program.BuildType}")
            .AddEmbed(e.MakeEmbedFromException()
                .WithFooter($"From: {sender?.Name ?? "$NO_MODULE_PASSED"}\nUptime: {PingCommand.FormatTickCount()}"));

        await Program.WebhookClient.BroadcastMessageAsync(webhookBuilder);
    }

    public static async Task<LloydContext?> TryGetDbContext([Optional] CancellationToken? token)
    {
        if (DiscordCommandService.DbContextFactory is null)
            return null;

        if (token is null)
            return await DiscordCommandService.DbContextFactory.CreateDbContextAsync();

        return await DiscordCommandService.DbContextFactory.CreateDbContextAsync((CancellationToken)token);
    }

    public static string GetDisplayName(this DiscordUser user)
    {
        if (user is DiscordMember member)
            return member.DisplayName;
        else if (!string.IsNullOrEmpty(user.GlobalName))
            return user.GlobalName;
        else if (user.Discriminator == "0")
            return user.Username;

        return $"{user.Username}#{user.Discriminator}";
    }
}
