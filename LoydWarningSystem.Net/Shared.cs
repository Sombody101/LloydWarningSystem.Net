using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;

namespace LloydWarningSystem.Net;

internal static class Shared
{
    public const string PreviousInstance = "from_previous_instance";

    // Runtime constants
    public static readonly DiscordColor DefaultEmbedColor = new(0xFFE4B5);

    public static async Task<bool> UserIsAdmin(this CommandContext ctx)
    {
        var config = ConfigManager.UserStorage;

        if (ctx.User.Id != BotConfigModel.AbsoluteAdmin || !config.BotAdmins.ContainsKey(ctx.User.Id))
        {
            await ctx.RespondAsync("This command can only be used by an admin, you faggot.");
            return false;
        }

        return true;
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
