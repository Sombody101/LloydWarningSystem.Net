using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LloydWarningSystem.Net.CommandChecks.Attributes;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class TimeMaker
{
    [Command("epoch"), DebugOnly]
    public static async Task EpochCommand(CommandContext _ctx)
    {
        if (!_ctx.EnsureSlashContext(out var ctx))
            return;

        string modal_id = Shared.GenerateModalId();

        var drop_options = new List<DiscordSelectComponentOption>()
        {
            new("Default Time Tag", string.Empty, "Would look like: `November 28, 2018 9:01 AM`", true),

            new("Short Time", "t", "Would look like: 9:01 AM"),

            new("Long Time", "T", "Would look like: 9:01:00 AM"),

            new("Short Date", "d", "Would Look like: 11/28/2018"),

            new("Long Date", "D", "Would look like: November 28, 2018"),

            new("Short Date/Time", "f", "Would look like: November 28, 2018 9:01 AM"),

            new("Long Date/Time", "F", "Would look like: Wednesday, November 28, 2018 9:01 AM"),

            new("Relative Time", "R", "Would look like: 3 years ago")
        };

        var dropdown = new DiscordSelectComponent("dropdown", null, drop_options, false, 1, 2);

        var utc_now = DateTime.UtcNow;

        var response = new DiscordInteractionResponseBuilder()
            .WithTitle("Epoch Generator")
            .WithCustomId(modal_id)
            .AddComponents(
                new DiscordTextInputComponent("Month", "month", utc_now.Month.ToString(), "0", false),
                new DiscordTextInputComponent("Day", "day", utc_now.Day.ToString(), "0", false),
                new DiscordTextInputComponent("Hour", "hour", utc_now.Hour.ToString(), "0", false),
                new DiscordTextInputComponent("Minute", "minute", utc_now.Minute.ToString(), "0", false)
            );

        await ctx.RespondWithModalAsync(response);

        var result = await ctx.Client.GetInteractivity().WaitForModalAsync(modal_id);

        if (result.TimedOut)
        {
            await ctx.RespondAsync("You took too long to respond.");
            return;
        }

        // Process the user's inpu

        // Generate epoch time
        // ...
    }
}
