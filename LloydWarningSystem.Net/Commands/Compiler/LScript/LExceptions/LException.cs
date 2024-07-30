using Antlr4.Runtime;
using LloydWarningSystem.Net.FinderBot.Commands.Compiler.LScript.LRuntime;
using System.Text;

namespace LloydWarningSystem.Net.FinderBot.Commands.Compiler.LScript.LExceptions;

/// <summary>
/// The base to all exceptions. Allows for a stack trace to be created.
/// </summary>
internal class LException : Exception
{
    // Context of exception (line, error member, etc)
    public ParserRuleContext? Context { get; init; }

    public LException(string message)
        : this(message, null) { }

    public LException(string message, ParserRuleContext? context)
        : base(message)
    {
        Context = context;
    }

    /// <summary>
    /// Create a C# flavored stack trace using <see cref="HRuntimeMembers.ScopeContexts"/>
    /// </summary>
    /// <returns></returns>
    public static string CreateStaticStackTrace(RuntimeMemory memory)
    {
        var sb = new StringBuilder();

        foreach (var context in memory.Scopes)
        {
            sb.Append(" at ");

            if (context.FunctionBeingCalled is not null)
            {
                sb.Append(context.FunctionBeingCalled.Name);

                sb.Append('(')
                    .Append(string.Join(", ", context.FunctionBeingCalled.ParameterList.Select(args => args.ArgumentName)))
                    .AppendLine(")");
            }
            else
                sb.Append("scoped-context");

            if (context.ScriptContext is not null)
            {
                var ctx = context.ScriptContext;
                sb.Append(" in ");

                sb.Append(ctx.Start.Line)
                    .Append(':')
                    .Append(ctx.Start.Column);
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Failed to resolve a function call reference. AKA: Couldn't find a function
/// </summary>
internal class LUnknownFunctionReferenceException : LException
{
    public LUnknownFunctionReferenceException(string functionName, ParserRuleContext? context)
        : base($"Unknown function '{functionName}'.")
    {
        Context = context;
    }
}

/// <summary>
/// The <see cref="HRuntimeMembers.StackContexts"/> stack is empty (the program is pretty much gone)
/// </summary>
internal class LEmptyStackException : LException
{
    public LEmptyStackException()
        : this(null)
    { }

    public LEmptyStackException(ParserRuleContext? context)
        : base("Cannot complete operation: The context stack is empty.")
    {
        Context = context;
    }
}

internal class LNullReferenceException : LException
{
    public LNullReferenceException(string message)
        : base(message)
    { }

    public LNullReferenceException(string message, ParserRuleContext? context)
        : base(message)
    {
        Context = context;
    }
}

internal class LUndefinedVariableAssignmentException : LException
{
    public LUndefinedVariableAssignmentException(string variable)
        : base($"Attempted to assign a value to undefined variable '{variable}'.")
    { }

    public LUndefinedVariableAssignmentException(string variable, ParserRuleContext? context)
        : this(variable)
    {
        Context = context;
    }
}

internal class LUndefinedVariableReferenceException : LException
{
    public LUndefinedVariableReferenceException(string variable)
    : base($"Attempted to get a value from an undefined variable '{variable}'.")
    { }

    public LUndefinedVariableReferenceException(string variable, ParserRuleContext? context)
        : this(variable)
    {
        Context = context;
    }
}

internal class LDuplicateVariableDeclarationException : LException
{
    public LDuplicateVariableDeclarationException(string variable)
        : base($"Attempted to create the variable '{variable}', but it already exists in the current scope.")
    { }

    public LDuplicateVariableDeclarationException(string variable, ParserRuleContext? context)
        : this(variable)
    {
        Context = context;
    }
}