using Antlr4.Runtime.Misc;
using DSharpPlus.Commands;

namespace LloydWarningSystem.Net.Commands.Compiler.LScript.LRuntime;

internal class LScriptRuntime : LScriptParserBaseVisitor<string>
{
    private readonly RuntimeMemory _memory;

    public LScriptRuntime(CommandContext commandContext)
    {
        _memory = new(commandContext);

        _memory.Scopes.Push(new("<global>", null));
    }

    public override string VisitVariableAssignment([NotNull] LScriptParser.VariableAssignmentContext context)
    {
        // var varName = context.Identifier().GetText();
        // var value = Visit(context.variableDefinition().expression());
        // _memory.SetVariable(varName, value, context);
        // 
        // return value;

        return string.Empty;
    }
}
