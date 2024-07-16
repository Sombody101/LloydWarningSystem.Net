using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;

namespace LloydWarningSystem.Net.FinderBot.Commands.Compiler;

[Command("cs"), Description("JIT C# (C-Sharp) Compilation via Roslyn")]
public static class CSharp
{
    [Command("run")]
    public static async Task CompileCSharp(CommandContext ctx, [FromCode] string code)
    {
        // Create an in-memory stream to capture console output
        var memoryStream = new MemoryStream();
        var originalOut = Console.Out;
        Console.SetOut(new StreamWriter(memoryStream));

        try
        {
            // Execute the script
            var options = ScriptOptions.Default.WithImports("System", "System");
            var compiled = CSharpScript.Create(code, options);
            await compiled.RunAsync();

            // Read the captured output from the stream
            memoryStream.Seek(0, SeekOrigin.Begin);
            var capturedOutput = await new StreamReader(memoryStream).ReadToEndAsync();

            // Return the captured console output
            await ctx.RespondAsync(capturedOutput);
        }
        finally
        {
            // Restore original Console output
            Console.SetOut(originalOut);
        }
    }
}