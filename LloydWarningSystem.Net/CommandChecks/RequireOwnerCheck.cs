using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace LloydWarningSystem.Net.CommandChecks;

public class RequireOwnerCheck : IContextCheck<RequireBotOwnerAttribute>
{
    public async ValueTask<string?> ExecuteCheckAsync(RequireBotOwnerAttribute attribute, CommandContext context)
    {
        if (!await IsAdmin(context))
            return "You need to be a bot owner!";

        return null;
    }

    public async ValueTask<bool> IsAdmin(CommandContext context)
    {
        var app = context.Client.CurrentApplication;
        var me = context.Client.CurrentUser;

        bool isOwner = app is not null
            ? app!.Owners!.Any(x => x.Id == context.User.Id)
            : context.User.Id == me.Id;

        return isOwner;
    }
}