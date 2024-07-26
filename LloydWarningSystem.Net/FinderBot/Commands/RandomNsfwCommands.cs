using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;
using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot;

public static class RandomNsfwCommands
{
    [Command("kill")]
    public static async ValueTask KillUserAsync(CommandContext ctx, DiscordUser user)
    {
        if (user.Id == BotConfigModel.AbsoluteAdmin)
        {
            await ctx.RespondAsync($"You give hugs and kisses to {user.Mention} bc he's the greatest person ever and my dad :smiling_face_with_3_hearts:");
            return;
        }

        await ctx.RespondAsync($"You have just killed {user.Mention} in cold blood.\nThey were a fag anyway, so it doesn't really matter.");
    }

    [Command("fuck"), Description("Does this even need a description?")]
    public static async ValueTask FuckUserAsync(CommandContext ctx, DiscordUser user)
    {
        if (user.Id == ctx.User.Id)
        {
            await ctx.RespondAsync("Why would you fuck yourself...?\nGet a life, ffs.");
            return;
        }

        if (user.Id == BotConfigModel.AbsoluteAdmin)
        {
            await ctx.RespondAsync("Good pick lol");
            return;
        }

        if (ctx.User.Id == BotConfigModel.AbsoluteAdmin)
        {
            await ctx.RespondAsync($"You fucked {user.Mention} hard and good.\nThey can't walk now lol");
            return;
        }

        await ctx.RespondAsync("Get a life, weirdo.\nFfs.");
    }

    [Command("dick"), Description("See how big your dick is!")]
    public static async ValueTask DickAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var length = Random.Shared.Next(0, 35);
        await ctx.RespondAsync($"8{new string('=', length / 2)}D\n{(
            user is null
                ? "Your"
                : $"{user.Mention}'s"
            )} penis is {length} inch{"es".Pluralize(length)} long!");
    }

    [Command("height"), Description("See how tall you are!")]
    public static async ValueTask HeightAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var height = Random.Shared.Next(50, 95);
        await ctx.RespondAsync($"{(
            user is null
                ? "You are"
                : $"{user.Mention} is"
        )} {height / 12:n0} {Qol.Pluralize("foot", "feet", height == 1)} tall!");
    }

    [Command("weight"), Description("Fucking fatty.")]
    public static async ValueTask WeightAsync(CommandContext ctx, DiscordUser? user = null)
    {
        var weight = Random.Shared.NextDouble() * 500;
        await ctx.RespondAsync($"{(
            user is null
                ? "You are"
                : $"{user.Mention} is"
        )} {weight:n0} LBS!\nFatty!");
    }

    [Command("jerkoff")]
    public static async ValueTask JerkOffAsync(CommandContext ctx, DiscordUser? user = null)
    {
        if (user is null)
        {
            if (ctx.User.Id == BotConfigModel.AbsoluteAdmin)
            {
                await ctx.RespondAsync("Good on you, king.\nHope everything comes out okay lol");
                return;
            }

            await ctx.RespondAsync("You don't need to tell us that... fucking degenerate.");
        }
        else
        {
            if (user.Id == BotConfigModel.AbsoluteAdmin)
            {
                await ctx.RespondAsync("Good pick lol");
                return;
            }

            await ctx.RespondAsync($"You jerked {user.Mention} real good.\nThey're a little sore, and you are now a whore.");
        }
    }
}
