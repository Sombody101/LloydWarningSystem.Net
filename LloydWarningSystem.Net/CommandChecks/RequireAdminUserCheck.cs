using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using LloydWarningSystem.Net.CommandChecks.Attributes;
using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.CommandChecks;

public class RequireAdminUserCheck : IContextCheck<RequireAdminUserAttribute>
{
    private readonly LloydContext _dbContext;

    public RequireAdminUserCheck(LloydContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<string?> ExecuteCheckAsync(RequireAdminUserAttribute? _, CommandContext context)
    {
        var user = await _dbContext.Users.FindAsync(context.User.Id);

        if (user is null || !user.IsBotAdmin && !RequireOwnerCheck.IsOwner(context))
            return "You need to be a bot administrator!";

        return null;
    }
}