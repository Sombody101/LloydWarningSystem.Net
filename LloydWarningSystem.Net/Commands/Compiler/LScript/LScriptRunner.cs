using Antlr4.Runtime;
using DSharpPlus.Commands;
using LloydWarningSystem.Net.Commands.Compiler.LScript.LRuntime;
using System.Text;

namespace LloydWarningSystem.Net.Commands.Compiler.LScript;

internal static class LScriptRunner
{
    public static async Task<ScriptResult> StartScriptAsync(CommandContext context, string script)
    {
        // Create lexer
        var checkpoint = DateTime.UtcNow;
        var lexer = new LScriptLexer(new AntlrInputStream(script));
        var lexTime = (DateTime.UtcNow - checkpoint).TotalMilliseconds;

        // Create parser
        checkpoint = DateTime.UtcNow;
        var parser = new LScriptParser(new CommonTokenStream(lexer));
        var parseTime = (DateTime.UtcNow - checkpoint).TotalMilliseconds;

        // Enter program
        var scriptContext = parser.script();

        string? exceptionMessage = null;
        string? finalOutput = string.Empty;
        double programTime = 0;
        try
        {
            checkpoint = DateTime.UtcNow;
            var visitor = new LScriptRuntime(context);
            finalOutput = visitor.Visit(scriptContext);
            programTime = (DateTime.UtcNow - checkpoint).TotalMilliseconds;
        }
        catch (Exception e)
        {
            exceptionMessage = e.Message;
        }

        return new ScriptResult()
        {
            RunTimeMs = programTime,
            LexTimeMs = lexTime,
            ParseTimeMs = parseTime,
            ProjectSize = script.Length,
            FinalOutput = finalOutput,
            ExitMessage = exceptionMessage,
        };
    }
}

internal class ScriptResult
{
    public double RunTimeMs { get; init; }

    public double LexTimeMs { get; init; }

    public double ParseTimeMs { get; init; }

    public int ProjectSize { get; init; }

    public int ExitCode { get; init; }

    public string? FinalOutput { get; init; }

    public string? ExitMessage { get; init; }
}
