using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LloydWarningSystem.Net.FinderBot.Commands.Compiler;

public static class Automation
{
    [Command("run"), Description("Run a automation script using defined aliases and commands defined within the bot")]
    public static async ValueTask RunAutomationAsync(CommandContext ctx, [FromCode] string script) 
    {

    }

    [Command("getscript")]
    public static async ValueTask PrintAutomationAsync(CommandContext ctx, string script_name)
    {

    }
}
