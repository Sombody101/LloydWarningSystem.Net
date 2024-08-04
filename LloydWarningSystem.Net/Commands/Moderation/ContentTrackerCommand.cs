using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LloydWarningSystem.Net.Commands.AutoCompleters;
using LloydWarningSystem.Net.Models;
using LloydWarningSystem.Net.Services.RegexServices;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace LloydWarningSystem.Net.Commands.Moderation;

[Command("tracker"), Description("Module for tracking guild messages in specific channels.")]
[RequirePermissions(DiscordPermissions.ModerateMembers), RequireGuild]
public class ContentTrackerCommand
{
    private const string configName = "config_name";
    private const string targetId = "target_channel";
    private const string reportId = "report_channel";
    private const string regex = "regex";

    private readonly IRegexService _regexService;

    public ContentTrackerCommand(IRegexService _regex)
    {
        _regexService = _regex;
    }

    /// <summary>
    /// Adds a new regex tracker to the <see cref="RegexService"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [Command("add"), Description("Creates a new regex tracker for a channel.")]
    public async ValueTask AddTrackerAsync(SlashCommandContext ctx)
    {
        var result = await PromptWithTrackerModalAsync(ctx);

        if (result is null)
            return;

        try
        {
            await _regexService.CreateRegexAsync(ctx.Guild.Id, ctx.User.Id, result);
        }
        catch (Exception e)
        {
            await ctx.FollowupAsync(new DiscordEmbedBuilder()
                .WithTitle("Unable to create tracker!")
                .WithColor(DiscordColor.Red)
                .WithDescription(e.Message));
        }

        // var tracker = _regexService.get

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Regex tracking pattern created!")
            .AddField("Name", result.Name);

        if (result.SourceChannelId is not 0)
            embed.AddField("Source Channel", $"{result.SourceChannelId} ({ctx.Guild.GetChannelAsync(result.SourceChannelId).Result.Mention})");
        else
            embed.AddField("Source Channel", "Unset (Tracker disabled)");

        if (result.ReportChannelId is not 0)
            embed.AddField("Report Channel", $"{result.ReportChannelId} ({ctx.Guild.GetChannelAsync(result.ReportChannelId).Result.Mention})");
        else
            embed.AddField("Report Channel", "Unset (Tracker disabled)");

        embed.AddField("Regex Patthen", $"```regex\n{result.RegexPattern}\n```");

        await ctx.FollowupAsync(embed);
    }

    /// <summary>
    /// Remove a <see cref="TrackingDbEntity"/> from a <see cref="DiscordGuild"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="tracker_name"></param>
    /// <returns></returns>
    [Command("delete"), Description("Deletes the specified tracker configuration")]
    public async ValueTask DeleteTrackerAsync(CommandContext ctx,
        [RemainingText]
        [SlashAutoCompleteProvider(typeof(TrackerNameAutocomplete))]
        string tracker_name)
    {
        await _regexService.DeleteRegexAsync(ctx.Guild.Id, tracker_name);

        await ctx.RespondAsync($"Deleted tracker `{tracker_name}`!");
    }

    /// <summary>
    /// Get information on a specific <see cref="TrackingDbEntity"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="tracker_name"></param>
    /// <returns></returns>
    [Command("get"), Description("Gets the configuration for a channel regex tracker.")]
    public async ValueTask GetTrackerAsync(CommandContext ctx,
        [RemainingText]
        [SlashAutoCompleteProvider(typeof(TrackerNameAutocomplete))]
        string tracker_name)
    {
        var tracker = await _regexService.GetRegexAsync(ctx.Guild.Id, tracker_name);

        if (tracker is null)
        {
            await ctx.RespondAsync($"Failed to find a tracker by the name '{tracker_name}'");
            return;
        }

        await ctx.RespondAsync(new DiscordEmbedBuilder()
            .WithTitle("Regex Tracker Info")
            .WithDescription($"### {tracker.Name}\nTracker source channel: `{tracker.SourceChannelId}`\nTracker reporter channel: `{tracker.ReportChannelId}`\n" +
            $"Tracker regex expression\n```log\n{tracker.RegexPattern}\n```"));
    }

    /// <summary>
    /// Get every <see cref="TrackingDbEntity"/> associated with a <see cref="DiscordGuild"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [Command("list"), Description("Lists all regex trackers for this guild.")]
    public async ValueTask ListTrackersAsync(CommandContext ctx)
    {
        var trackers = await _regexService.GetRegexesOwnedByGuildAsync(ctx.Guild.Id);

        if (trackers is null || trackers.Count is 0)
        {
            await ctx.RespondAsync("This guild doesn't have any regex trackers set!");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Set Regex Trackers");

        foreach (var tracker in trackers)
            embed.AddField(tracker.Name,
                $"Source channel: `{tracker.SourceChannelId}`\nReporter channel: `{tracker.ReportChannelId}`\nRegex expression: ```regex\n{tracker.RegexPattern}\n```");

        await ctx.RespondAsync(embed);
    }

    /// <summary>
    /// Get the edit history of a specific guild <see cref="TrackingDbEntity"/>
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="tracker_name"></param>
    /// <returns></returns>
    [Command("blame"), Description("Gets the edit history of a tracker.")]
    public async ValueTask GetEditorsAsync(CommandContext ctx,
        [RemainingText]
        [SlashAutoCompleteProvider(typeof(TrackerNameAutocomplete))]
        string tracker_name)
    {
        var editBlames = await _regexService.GetBlameRegexEditorsAsync(ctx.Guild.Id, tracker_name);

        var formattedEditors = new StringBuilder();

        foreach (var blame in editBlames)
        {
            if (blame.ErrorReason is not null)
            {
                formattedEditors.AppendLine($"1. Failed to fetch edit details!\n - {blame.ErrorReason}");
                continue;
            }

            var epoch = blame.TimeOfChange.ToUnixTimeSeconds();
            formattedEditors.AppendLine($"1. `{blame.EditorId}` ({ctx.Client.GetUserAsync(blame.EditorId).Result.Username})")
                .Append(FormatChanges(blame.ChangesMade))
                .AppendLine($" - Change made <t:{epoch}:F>, or <t:{epoch}:R>");
        }

        var isActive = _regexService.TrackerDisabled(await _regexService.GetRegexAsync(ctx.Guild.Id, tracker_name));

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Tracker Editor History [{(isActive ? "ACTIVE" : "INACTIVE")}]")
            .WithDescription(formattedEditors.ToString());

        await ctx.RespondAsync(embed);
    }

    [Command("edit")]
    public class TrackerModifications
    {
        private readonly IRegexService _regexService;

        public TrackerModifications(IRegexService _regex)
        {
            _regexService = _regex;
        }

        /// <summary>
        /// Change one or more properties of a <see cref="TrackingDbEntity"/> via a modal
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tracker_name"></param>
        /// <returns></returns>
        [Command("edit"), Description("Edit a regex tracker")]
        public async ValueTask EditTrackerAsync(SlashCommandContext ctx,
            [RemainingText]
        [SlashAutoCompleteProvider(typeof(TrackerNameAutocomplete))]
        string tracker_name)
        {
            var tracker = await _regexService.GetRegexAsync(ctx.Guild.Id, tracker_name);

            if (tracker is null)
            {
                await ctx.RespondAsync($"Failed to find a tracker by the name `{tracker_name}`");
                return;
            }

            var result = await PromptWithTrackerModalAsync(ctx, tracker);

            if (result is null)
                return;

            var changedItems = new StringBuilder();

            if (result.Name != tracker.Name)
                changedItems.AppendLine($"1. Tracker name changed from `{tracker.Name}` to `{result.Name}`");

            if (result.SourceChannelId != tracker.SourceChannelId)
                changedItems.AppendLine($"1. Source channel changed from `{tracker.SourceChannelId}` to `{result.SourceChannelId}`");

            if (result.ReportChannelId != tracker.ReportChannelId)
                changedItems.AppendLine($"1. Reporter channel changed from `{tracker.ReportChannelId}` to `{result.ReportChannelId}`");

            if (result.RegexPattern != tracker.RegexPattern)
                changedItems.AppendLine($"1. Regex pattern changed from\n```regex\n{tracker.RegexPattern}\n``` to\n```regex\n{result.RegexPattern}\n```");

            try
            {
                await _regexService.ModifyRegexAsync(ctx.Guild.Id, ctx.User.Id, tracker_name, result);
            }
            catch (Exception e)
            {
                await ctx.FollowupAsync(new DiscordEmbedBuilder()
                    .WithTitle("Unable to edit tracker!")
                    .WithColor(DiscordColor.Red)
                    .WithDescription(e.Message));
            }

            await ctx.FollowupAsync($"Updated tracker!\n{changedItems}");
        }


    }

    private static async ValueTask<TrackingConfigurationSummary?> PromptWithTrackerModalAsync(SlashCommandContext ctx, TrackingDbEntity? modifying = null)
    {
        string modalName = Shared.GenerateModalId();
        bool isModifying = modifying is not null;

        var interaction = ctx.Client.GetInteractivity();

        var modal = new DiscordInteractionResponseBuilder()
            .WithCustomId(modalName)
            .WithTitle("Channel Tracking Configuration")

            // Config name
            .AddComponents(new DiscordTextInputComponent("Configuration Name", configName, "My Config", isModifying
                ? modifying.Name
                : null, max_length: 70))

            // Source channel ID
            .AddComponents(new DiscordTextInputComponent("Look for messages in:", targetId, "Channel ID (0 disables tracker)", isModifying
                ? modifying.SourceChannelId.ToString()
                : null, max_length: 20))

            // Reporter channel ID
            .AddComponents(new DiscordTextInputComponent("Report messages in:", reportId, "Channel ID (0 disables tracker)", isModifying
                ? modifying.ReportChannelId.ToString()
                : null, max_length: 20))

            // Regex string
            .AddComponents(new DiscordTextInputComponent("Regex String", regex, "^([a-zA-Z0-9_\\-\\.]+)@([a-zA-Z0-9_\\-\\.]+)$", isModifying
                ? modifying.RegexPattern
                : null, max_length: 512, required: false));

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);

        var response = await interaction.WaitForModalAsync(modalName);

        if (response.TimedOut)
        {
            await ctx.FollowupAsync("The tracking configuration builder timed out!");
            return null;
        }

        var errors = new StringBuilder();

        // Validate channels
        if (!ulong.TryParse(response.Result.Values[targetId], out var target_id))
            errors.AppendLine("The target channel ID must be a number!");

        if (target_id is not 0)
        {
            var channel = await ctx.Guild.GetChannelAsync(target_id);
            if (channel is null)
                errors.AppendLine($"Failed to find a target channel with the given ID `{target_id}`!");
        }

        if (!ulong.TryParse(response.Result.Values[reportId], out var report_id))
            errors.AppendLine("The report channel ID must be a number!");

        if (report_id is not 0)
        {
            var channel = await ctx.Guild.GetChannelAsync(report_id);
            if (channel is null)
                errors.AppendLine($"Failed to find a report channel with the given ID `{report_id}`!");
        }

        // Test the regex
        var pattern = response.Result.Values[regex];
        try
        {
            Regex.Match(string.Empty, pattern);
        }
        catch (ArgumentException ex)
        {
            errors.AppendLine("The given regex expression does not work!")
                .Append($" - {ex.Message}");
        }

        // Report errors, return if any
        if (errors.Length is not 0)
        {
            await ctx.FollowupAsync(new DiscordEmbedBuilder()
                .WithTitle("An error occured while creating your configuration!")
                .WithDescription(errors.ToString()));

            return null;
        }

        // Return organized data
        return new()
        {
            Name = response.Result.Values[configName],
            SourceChannelId = target_id,
            ReportChannelId = report_id,
            RegexPattern = pattern,
        };
    }

    private static string FormatChanges(TrackingConfigurationChange change)
    {
        var changeBuilder = new StringBuilder();

        if (change.HasFlag(TrackingConfigurationChange.NameChange))
            changeBuilder.AppendLine(" - Changed config name.");

        if (change.HasFlag(TrackingConfigurationChange.SourceChannelChange))
            changeBuilder.AppendLine(" - Changed source channel ID.");

        if (change.HasFlag(TrackingConfigurationChange.ReportChannelChange))
            changeBuilder.AppendLine(" - Changed report channel ID.");

        if (change.HasFlag(TrackingConfigurationChange.RegexPatternChange))
            changeBuilder.AppendLine(" - Changed regex pattern.");

        if (change == 0)
            changeBuilder.AppendLine(" - Tracker created.");

        return changeBuilder.ToString();
    }
}