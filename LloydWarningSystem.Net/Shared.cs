using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace LloydWarningSystem.Net;

internal static class Shared
{
    public const string PreviousInstance = "from_previous_instance";

    // Runtime constants
    public static readonly DiscordColor DefaultEmbedColor = new(0xFFE4B5);

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
            .AddField("Stack Trace", $"```\n{ex.StackTrace ?? "$NO_STACK_TRACE"}\n```");

        return ex_message;
    }

    private static async Task ModifyOrSendErrorEmbed(CommandContext ctx, string error, DiscordMessage? message = null)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("REPL Error")
            .WithAuthor(ctx.User.Username)
            .WithColor(DiscordColor.Red)
            .AddField("Tried to execute", $"[this code]({ctx})")
            .WithDescription(error);

        if (message == null)
        {
            await ctx.RespondAsync(embed: embed.Build());
            return;
        }

        await message.ModifyAsync(msg =>
        {
            msg.Content = null;
            msg.RemoveEmbeds(0, msg.Embeds.Count);
            msg.AddEmbed(embed.Build());
        });
    }
}
