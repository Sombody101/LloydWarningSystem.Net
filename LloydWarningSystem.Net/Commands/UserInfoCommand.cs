﻿using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using System.Globalization;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class InfoCommand
{
    /// <summary>
    /// Sends information about the provided user.
    /// </summary>
    /// <param name="user">Which user to get information about. Leave empty to get information about yourself.</param>
    [Command("user"), TextAlias("member")]
    public static async Task GetUserInfo(CommandContext ctx, DiscordUser? user = null)
    {
        user ??= ctx.User;

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = $"Info about {user.Username}",
            Thumbnail = new() { Url = user.AvatarUrl },
            Color = Shared.DefaultEmbedColor
        };

        embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);
        embedBuilder.AddField("User Mention", user.Mention, true);

        List<string> userFlags = [];
        if (!user.Flags.HasValue || user.Flags.Value == DiscordUserFlags.None)
        {
            userFlags.Add("None");
        }
        else
        {
            for (int i = 0; i < (sizeof(DiscordUserFlags) * 8); i++)
            {
                var flag = (DiscordUserFlags)(1 << i);
                if (!user.Flags.Value.HasFlag(flag))
                    continue;

                // If the flag isn't documented, Humanize will return an empty string.
                // When that happens, we'll use the flag bit instead.
                string displayFlag = flag.Humanize().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(displayFlag))
                {
                    // For whatever reason, the spammer flag is intentionally
                    // undocumented as "bots will never have a use for it".
                    displayFlag = i == 20
                        ? "Likely spammer"
                        : $"1 << {i}";
                }

                // Capitalize the first letter of the first flag.
                if (userFlags.Count == 0)
                    displayFlag = char.ToUpper(displayFlag[0], CultureInfo.InvariantCulture) + displayFlag[1..];

                userFlags.Add(displayFlag);
            }
        }

        embedBuilder.AddField("User Flags", $"{userFlags.DefaultIfEmpty($"Unknown flags: {user.Flags}").Humanize()}.", false);
        embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);

        // The user probably wasn't in the cache. Let's try to get them from the guild.
        if (user is not DiscordMember member)
        {
            try
            {
                member = await ctx.Guild!.GetMemberAsync(user.Id);
            }
            // The user is not in the guild.
            catch (DiscordException)
            {
                await ctx.RespondAsync(embedBuilder);
                return;
            }
        }

        embedBuilder.AddField("Joined Guild", Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime), true);

        embedBuilder.AddField("Roles", member.Roles.Any()
            ? string.Join('\n', member.Roles.OrderByDescending(role => role.Position).Select(role => $"- {role.Mention}"))
            : "None", false);

        // If the user has a color, set it.
        if (!member.Color.Equals(default(DiscordColor)))
        {
            embedBuilder.Color = member.Color;
        }

        await ctx.RespondAsync(embedBuilder);
    }

    public static async Task GetUserInfo(CommandContext ctx, ulong id)
    {
        var user = await ctx.Client.GetUserAsync(id);

        if (user is null)
        {
            await ctx.RespondAsync($"Failed to find a user by the ID `{id}`");
            return;
        }

        // Pass it off to the big-boy command
        await GetUserInfo(ctx, user);
    }
}
