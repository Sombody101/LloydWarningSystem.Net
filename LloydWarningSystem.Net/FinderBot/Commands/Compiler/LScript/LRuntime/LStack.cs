using Antlr4.Runtime;
using System.Runtime.InteropServices;

namespace LloydWarningSystem.Net.FinderBot.Commands.Compiler.LScript.LRuntime;

internal sealed class LStackContext
{
    private readonly LStackContext? parentStackContext;

    public readonly string Name;
    public Dictionary<string, object?> Symbols { get; private set; } = [];

    public LFunction? FunctionBeingCalled { get; init; }
    public ParserRuleContext? ScriptContext { get; init; }

    public LStackContext(string scopeName, 
        LStackContext? _previousStackContext, 
        [Optional] LFunction calledFunction,
        [Optional] ParserRuleContext scriptContext)
    {
        parentStackContext = _previousStackContext;
        Name = scopeName;
        FunctionBeingCalled = calledFunction;
        ScriptContext = scriptContext;
    }

    /// <summary>
    /// Create a new <see cref="HObject"/> in this current context
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool AddSymbol<Result>(string name, Result? value)
    {
        // Return false if the variable already exists
        if (Symbols.ContainsKey(name))
            return false;

        // Add the variable
        Symbols.Add(name, value);
        return true;
    }

    /// <summary>
    /// Set a pre-existing <see cref="HObject"/> with a new value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <exception cref="HUndefinedVariableAssignmentException"></exception>
    public bool SetSymbol<Result>(string name, Result? value)
    {
        // Return false if the variable doesn't already exist
        if (!Symbols.ContainsKey(name))
            return false;

        // Set the variable
        Symbols[name] = value;
        return true;
    }

    /// <summary>
    /// Fetch a pre-existing <see cref="HObject"/> from the current context
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool GetSymbol<Result>(string name, out Result? outValue) where Result : class
    {
        if (Symbols.TryGetValue(name, out object? value))
        {
            if (value is Result result)
            {
                outValue = result;
                return true;
            }
        }
        else if (parentStackContext is not null)
            return parentStackContext.GetSymbol(name, out outValue);

        // Failed to find the symbol
        outValue = null;
        return false;
    }

    public readonly struct ArgumentInfo
    {
        public string ArgumentName { get; init; }
        public object? Argument { get; init; }
    }
}
