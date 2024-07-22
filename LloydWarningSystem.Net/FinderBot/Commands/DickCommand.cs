using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Context;

using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public class SizeCommand
{
    private readonly LloydContext _dbContext;

    public SizeCommand(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Command("dick"), Description("See how big your dick is!")]
    public async ValueTask DickAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var length = Random.Shared.Next(0, 35);
        await ctx.RespondAsync($"8{new string('=', length / 2)}D\n{(
            user is null
                ? "Your"
                : $"{user.Mention}'s"
            )} penis is {length} inch{"es".Pluralize(length)} long!");
    }

    [Command("height"), Description("See how tall you are!")]
    public async ValueTask HeightAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var height = Random.Shared.Next(50, 95);
        await ctx.RespondAsync($"{(
            user is null
                ? "You are"
                : $"{user.Mention} is"
        )} {height / 12:n0} {Qol.Pluralize("foot", "feet", height == 1)} tall!");
    }

    [Command("weight"), Description("Fucking fatty.")]
    public async ValueTask WeightAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var weight = Random.Shared.NextDouble() * 500;
        await ctx.RespondAsync($"{(
            user is null
                ? "You are"
                : $"{user.Mention} is"
        )} {weight:n0} LBS!\nFatty!");
    }
}
