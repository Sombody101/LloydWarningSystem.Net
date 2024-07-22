using DSharpPlus.Commands.ContextChecks;

namespace LloydWarningSystem.Net.CommandChecks;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireBotOwnerAttribute : ContextCheckAttribute
{
}
