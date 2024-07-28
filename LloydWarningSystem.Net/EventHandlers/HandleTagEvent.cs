using DSharpPlus;
using DSharpPlus.EventArgs;
using LloydWarningSystem.Net.Context;
using LloydWarningSystem.Net.Models;

namespace LloydWarningSystem.Net.EventHandlers;

internal static class HandleTagEvent
{
    public static void HandleTag(DiscordClient client, MessageCreatedEventArgs args, LloydContext db)
    {
        return; // Not in service

        // Check alias
        var match = MessageTag.LocateTagRegex.Match(args.Message.Content);

        if (!match.Success)
            return;

        var tag_name = match.Groups[1].Value;
        if (string.IsNullOrWhiteSpace(tag_name))
            return;

        tag_name = tag_name.Trim().ToLower();

        var tag_content = db.Set<MessageTag>().Select(x => x.Name);


    }
}
