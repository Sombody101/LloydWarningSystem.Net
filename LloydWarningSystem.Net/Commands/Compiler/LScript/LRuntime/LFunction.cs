using static System.Formats.Asn1.AsnWriter;

namespace LloydWarningSystem.Net.Commands.Compiler.LScript.LRuntime;

internal sealed class LFunction
{
    public string Name { get; init; }
    public LScriptParser.BlockContext? Block { get; set; }

    public ArgumentInfo[] ParameterList { get; init; }

    public LFunction(string name)
        : this(name, null)
    { }

    public LFunction(string name, LScriptParser.BlockContext? block)
    {
        Name = name;
        Block = block;
    }
}

internal sealed class ArgumentInfo
{
    public ArgumentInfo(string argumentName, string? argument)
    {
        ArgumentName = argumentName;
        Argument = argument;
    }

    public string ArgumentName { get; init; }
    public string? Argument { get; init; }
}