using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using System.ComponentModel;

namespace LloydWarningSystem.Net.Commands.Compiler;

public static class Automation
{
    [Command("run"), Description("Run a automation script using defined aliases and commands defined within the bot")]
    public static async ValueTask RunAutomationAsync(CommandContext ctx, [FromCode] string script)
    {

    }

    [Command("runscript")]
    public static async ValueTask RunAutomationFromAliasAsync(CommandContext ctx, string script_name)
    {
        throw new NotImplementedException("This class doesn't have access to the DB, nor does the DB implement script storage yet!");
    }

    [Command("getscript")]
    public static async ValueTask PrintAutomationAsync(CommandContext ctx, string script_name)
    {

    }
}
