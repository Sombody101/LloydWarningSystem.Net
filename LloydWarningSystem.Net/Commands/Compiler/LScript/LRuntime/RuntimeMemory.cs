using Antlr4.Runtime;
using DSharpPlus.Commands;
using LloydWarningSystem.Net.Commands.Compiler.LScript.LExceptions;

namespace LloydWarningSystem.Net.Commands.Compiler.LScript.LRuntime;

internal sealed class RuntimeMemory
{
    public readonly CommandContext CallingCommandContext;

    public RuntimeMemory(CommandContext commandContext)
    {
        CallingCommandContext = commandContext;
    }

    public Stack<LStackContext> Scopes { get; init; } = new();

    public LStackContext GlobalScope => Scopes.FirstOrDefault() ?? throw new LEmptyStackException();

    /// <summary>
    /// Get the most recent context, or throws an Exception if the stack is empty
    /// </summary>
    public LStackContext CurrentContext => Scopes.Peek() ?? throw new LEmptyStackException();

    /// <summary>
    /// Adds a function to the current context onto the <see cref="StackContexts"/>
    /// </summary>
    /// <param name="function"></param>
    public void AddFunctionToContext(LFunction function)
    {
        CurrentContext.AddSymbol(function.Name, function);
    }

    /// <summary>
    /// Get a defined function for the current context on the <see cref="StackContexts"/>
    /// </summary>
    /// <param name="functionName"></param>
    /// <returns></returns>
    /// <exception cref="LUnknownFunctionReferenceException"></exception>
    public LFunction GetFunctionFromContext(string functionName, ParserRuleContext context)
    {
        _ = CurrentContext.GetSymbol(functionName, out LFunction? func);

        if (func is LFunction lfunc)
            return lfunc;

        // We found the symbol with the matching name, but it's not a function
        throw new LUnknownFunctionReferenceException(functionName, context);
    }

    // This seems to work
    // might need more testing though

    /// <summary>
    /// Create a new <see cref="HObject"/> in the current scope
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="context"></param>
    public void DeclareVariable(string name, object? value, ParserRuleContext context)
    {
        if (!CurrentContext.AddSymbol(name, value ?? string.Empty))
            throw new LDuplicateVariableDeclarationException(name, context);
    }

    /// <summary>
    /// Set a pre-existing <see cref="HObject"/> in the current scope
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <exception cref="LUndefinedVariableAssignmentException"></exception>
    public void SetVariable(string name, object? value, ParserRuleContext context)
    {
        if (!CurrentContext.SetSymbol(name, value ?? string.Empty))
            throw new LUndefinedVariableAssignmentException(name, context);
    }

    /// <summary>
    /// Get the value of a pre-existing <see cref="HObject"/> in the current scope
    /// </summary>
    /// <param name="name"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="LUndefinedVariableReferenceException"></exception>
    public object GetVariable(string name, ParserRuleContext context)
    {
        if (!CurrentContext.GetSymbol(name, out string? variable))
            throw new LUndefinedVariableReferenceException(name, context);

        return variable;
    }
}
