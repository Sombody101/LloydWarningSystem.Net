using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;

namespace LloydWarningSystem.Net.Commands;

public class CreditsCommand
{
    private const string Lunar = $"<@336733686529654798> (oolunar)";
    private const string Plerx = $"<@350967844957192192> (plerx0715)";
    private const string Velvet = $"<@209279906280898562> (velvet.toroyashi)";
    private const string Me = $"<@518296556059885599> (oke6969)";

    private readonly LloydContext _dbContext;

    public CreditsCommand(LloydContext _db)
    {
        _dbContext = _db;
    }

    [Command("credits"), TextAlias("credit")]
    public async ValueTask ShowCreditsAsync(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder()
                .WithTitle("Credits")
                .WithThumbnail(ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(new DiscordColor(0x00ccff));

        embed.AddField("Bot Daddy", Me);
        embed.AddField("Codebase & Info Commands", Lunar);
        embed.AddField("Database Layout & Host Services", Plerx);
        embed.AddField("Regex Emotional Support", Velvet);

        embed.AddField("Bot Testers", string.Join('\n', _dbContext.Set<UserDbEntity>()
            .Where(user => user.IsBotAdmin)
            .Select(user => GetUserMention(ctx, user.Id))));

        embed.WithFooter("This bot is essentially a scrapbook of other peoples code :p");

        await ctx.RespondAsync(embed);
    }

    private static string GetUserMention(CommandContext ctx, ulong id)
        => $"<@{id}> {ctx.Client.GetUserAsync(id).Result.Username}";
}
