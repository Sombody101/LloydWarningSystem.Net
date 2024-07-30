using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using LloydWarningSystem.Net.CommandChecks.Attributes;
using LloydWarningSystem.Net.Configuration;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace LloydWarningSystem.Net.FinderBot.Commands.Admin;

public class BotManager
{
    private readonly LloydContext _dbContext;

    public BotManager(LloydContext context)
    {
        _dbContext = context;
    }

    [Command("addadmin"),
        Description("Gives the specified user bot administrator status."),
        RequireAdminUser]
    public async Task AddAdminAsync(CommandContext ctx,
        [Description("The ID of the wanted user")] ulong user_id)
    {
        var dis_user = await ctx.Client.GetUserAsync(user_id);
        var db_user = await _dbContext.Users.FindAsync(user_id);

        if (db_user is null)
        {
            var new_user = new UserDbEntity()
            {
                Username = dis_user.Username,
                Id = dis_user.Id,
                IsBotAdmin = true, // Add user as an admin
            };

            await _dbContext.Users.AddAsync(new_user);
        }
        else if (!db_user.IsBotAdmin)
        {
            db_user.IsBotAdmin = true;
        }
        else
        {
            await ctx.RespondAsync($"{dis_user.Username} is already an administrator!");
            return;
        }

        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"{dis_user.Mention} is now registered as a bot administrator.");
    }

    [Command("removeadmin"),
        Description("Removes bot administrator status from the specified user")]
    public async ValueTask RemoveAdminAsync(CommandContext ctx, ulong user_id)
    {
        var dis_user = await ctx.Client.GetUserAsync(user_id);
        var db_user = await _dbContext.Users.FindAsync(user_id);

        if (db_user is null)
        {
            var new_user = new UserDbEntity()
            {
                Username = dis_user.Username,
                Id = dis_user.Id,
                IsBotAdmin = false, // Set to false
            };

            await _dbContext.Users.AddAsync(new_user);
        }
        else if (db_user.IsBotAdmin)
        {
            db_user.IsBotAdmin = false;
        }
        else
        {
            await ctx.RespondAsync($"{dis_user.Username} wasn't already an administrator!");
            return;
        }

        await _dbContext.SaveChangesAsync();
        await ctx.RespondAsync($"{dis_user.Username} is no longer a bot administrator.");
    }

    [Command("listadmins"), RequireAdminUser]
    public async ValueTask ListAdminsAsync(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder().WithTitle("Active Administrators");
        var admins = _dbContext.Users.Where(user => user.IsBotAdmin);

        if (!admins.Any())
        {
            embed.AddField("There currently zero administrators", $"User count: `{_dbContext.Users.Count()}`");
            await ctx.RespondAsync(embed);
            return;
        }

        foreach (var user in admins)
            embed.AddField(user.Username, user.Id.ToString());

        await ctx.RespondAsync(embed);
    }

    /* Bot owner commands */

    [Command("addprefix"),
        Description("Adds a prefix to the bots configuration (requires restart)."),
        RequireAdminUser]
    public async Task AddPrefixAsync(CommandContext ctx, params string[] prefixes)
    {
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
        // Docker should restart Lloyd automatically
        Process.Start(open_path, Shared.PreviousInstance);
#endif

        Environment.Exit(exit_code);
    }
}
