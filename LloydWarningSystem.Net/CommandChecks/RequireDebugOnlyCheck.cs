using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using LloydWarningSystem.Net.CommandChecks.Attributes;
using LloydWarningSystem.Net.Context;

namespace LloydWarningSystem.Net.CommandChecks;

public class RequireDebugOnlyCheck : IContextCheck<DebugOnlyAttribute>
{
    private readonly LloydContext _dbContext;

    public RequireDebugOnlyCheck(LloydContext _db)
    {
        _dbContext = _db;
    }

    public async ValueTask<string?> ExecuteCheckAsync(DebugOnlyAttribute? attribute, CommandContext context)
    {
#if !DEBUG
        return "This command can only be run on the Debug version of Lloyd!";
#else
        if (await new RequireAdminUserCheck(_dbContext).ExecuteCheckAsync(null, context) is not null)
            return "You need to be a bot administrator to use this command while it's in the Debug stage!";

        return null;
#endif    
    }
}
