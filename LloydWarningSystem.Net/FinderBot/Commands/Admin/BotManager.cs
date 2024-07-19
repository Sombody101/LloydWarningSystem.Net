using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.Configuration;
using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot.Commands.Admin;

public static class BotManager
{
    [Command("addadmin"),
        Description("Adds user(s) to the bot administrators list."),
        RequireApplicationOwner]
    public static async Task AddAdminAsync(CommandContext ctx,
        [Description("User(s) to add to the administrators list.")] params DiscordUser[] users)
    {
        if (!await ctx.UserIsAdmin())
            return;

        var config = ConfigManager.UserStorage;

        foreach (var user in users)
            if (!config.BotAdmins.TryAdd(user.Id, user.Username))
                await ctx.RespondAsync($"{user.Username} is already an administrator!");

        ConfigManager.SaveUserStorage();
        await ctx.RespondAsync($"Added {users.Length} user{'s'.Pluralize(users.Length != 1)} to the administrators list.");
    }

    [Command("addprefix"),
        Description("Adds a prefix to the bots configuration (requires restart)."),
        RequireApplicationOwner]
    public static async Task AddPrefixAsync(CommandContext ctx, params string[] prefixes)
    {
        if (!await ctx.UserIsAdmin())
            return;

        var config = ConfigManager.BotConfig;

        foreach (var prefix in prefixes)
            if (config.CommandPrefixes.Contains(prefix))
                await ctx.RespondAsync($"The prefix `{prefix}` is already in use!");
            else
                config.CommandPrefixes.Add(prefix);

        await ConfigManager.SaveBotConfig();
        await ctx.RespondAsync($"Added {prefixes.Length} prefix{"es".Pluralize(prefixes.Length != 1)}");
    }

    [Command("restart"),
        Description("Restarts the bot."),
        RequireApplicationOwner]
    public static async ValueTask RestartAsync(CommandContext ctx, int exit_code = 0)
    {
        if (!await ctx.UserIsAdmin())
            return;

        var open_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName + ".exe");

        await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            .WithTitle("Restarting")
            .WithColor(Shared.DefaultEmbedColor)
            .AddField("Exit Code", exit_code.ToString())
            .AddField("Restart Location", open_path)
            .AddField("Restart Time", DateTime.Now.ToString())
            .AddField("Restart Time UTC", DateTime.UtcNow.ToString())
            .WithFooter("Restart will take ~1000ms to account for file stream exits and bot initialization.")
        );

#if DEBUG
        // Docker should restart Lloyd automatically, so only do this when compiled as Debug
        System.Diagnostics.Process.Start(open_path, Shared.PreviousInstance);
#endif

        Environment.Exit(exit_code);
    }
}
