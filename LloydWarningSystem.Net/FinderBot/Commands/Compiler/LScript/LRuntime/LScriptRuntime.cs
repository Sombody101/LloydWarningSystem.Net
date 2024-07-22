using Antlr4.Runtime.Misc;

namespace LloydWarningSystem.Net.FinderBot.Commands.Compiler.LScript.LRuntime;

internal class LScriptRuntime : LScriptParserBaseVisitor<string>
{
    private readonly RuntimeMemory _memory;

    public LScriptRuntime()
    {
        _memory = new();

        _memory.Scopes.Push(new("<global>", null));
    }

    public override string VisitVariableDeclaration([NotNull] LScriptParser.VariableDeclarationContext context)
    {
        var varName = context.Identifier().GetText();
        _memory.DeclareVariable(varName, string.Empty, context);

        return string.Empty;
    }
}
