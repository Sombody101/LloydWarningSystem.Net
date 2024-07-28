using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using System.ComponentModel;
using System.Text;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class HelpCommand
{
    [Command("help"),
        Description("Shows help information for commands.")]
    public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? command = null)
    {
        if (string.IsNullOrWhiteSpace(command))
            return context.RespondAsync(GetHelpMessage(context));

        else if (GetCommand(context.Extension.Commands.Values, command) is Command foundCommand)
            return context.RespondAsync(GetHelpMessage(context, foundCommand));

        return context.RespondAsync($"Failed to find a command by the name `{command}`.");
    }

    private static DiscordMessageBuilder GetHelpMessage(CommandContext context)
    {
        var stringBuilder = new StringBuilder();
        foreach (Command command in context.Extension.Commands.Values.OrderBy(x => x.Name))
            stringBuilder.AppendLine($"`{command.Name.Titleize()}`: {command.Description ?? "No description provided"}");

        return new DiscordMessageBuilder()
            .WithContent($"A total of {context.Extension.Commands.Values.Select(CountCommands).Sum():N0} commands were found. Use `help <command>` for more information on any of them.")
            .AddEmbed(new DiscordEmbedBuilder()
            .WithTitle("Commands")
            .WithDescription(stringBuilder.ToString()));
    }

    private static DiscordMessageBuilder GetHelpMessage(CommandContext context, Command command)
    {
        var embed = new DiscordEmbedBuilder();

        embed.WithTitle($"Help Command: `{command.FullName.Titleize()}`");
        embed.WithDescription(command.Description ?? "No description provided.");

        if (command.Subcommands.Count > 0)
            foreach (Command subcommand in command.Subcommands.OrderBy(x => x.Name))
                embed.AddField(subcommand.Name.Titleize(), command.Description ?? "No description provided.");
        else
        {
            if (command.Attributes.FirstOrDefault(x => x is RequirePermissionsAttribute) is RequirePermissionsAttribute permissions)
            {
                var commonPermissions = permissions.BotPermissions & permissions.UserPermissions;
                var botUniquePermissions = permissions.BotPermissions ^ commonPermissions;
                var userUniquePermissions = permissions.UserPermissions ^ commonPermissions;
                var builder = new StringBuilder();

                if (commonPermissions != default)
                    builder.AppendLine(commonPermissions.ToPermissionString());

                if (botUniquePermissions != default)
                {
                    builder.Append("**Bot**: ");
                    builder.AppendLine((permissions.BotPermissions ^ commonPermissions).ToPermissionString());
                }

                if (userUniquePermissions != default)
                {
                    builder.Append("**User**: ");
                    builder.AppendLine(permissions.UserPermissions.ToPermissionString());
                }

                embed.AddField("Required Permissions", builder.ToString());
            }

            embed.AddField("Usage", command.GetUsage());
            foreach (CommandParameter parameter in command.Parameters)
                embed.AddField($"{parameter.Name.Titleize()} - {context.Extension.GetProcessor<TextCommandProcessor>()
                    .Converters[GetConverterFriendlyBaseType(parameter.Type)].ReadableName}", command.Description ?? "No description provided.");

            var method = command.Method;

            if (method is not null)
            {
                embed.AddField("In Module",
                    $"```ansi\n{Formatter.Colorize(method.DeclaringType?.Name ?? "$UNKNOWN_MODULE", AnsiColor.Blue)}\n```");

                var sb = new StringBuilder("\n");

                // Get method parameters
                foreach (var param in method.GetParameters())
                {
                    // Get attributes for parameters (if any)
                    var attributes = param.CustomAttributes;
                    if (attributes.Count() is not 0)
                    {
                        foreach (var attr in attributes.Select(attr => attr.AttributeType))
                            sb.Append("\n\t[").Append(attr.Name.Replace("Attribute", string.Empty))
                                .Append("]\n");
                    }

                    sb.Append('\t')
                        .Append(param.ParameterType.IsPrimitive 
                            ? param.ParameterType.Name.ToLower() 
                            : param.ParameterType.Name)
                        .Append(' ')
                        .Append(param.Name)
                        .Append(",\n");
                }

                embed.AddField("Method Declaration",
                    $"```cs\npublic {(method.IsStatic
                        ? "static "
                        : string.Empty)}async {method.ReturnType.Name} {method.Name}({sb.ToString().TrimEnd()[0..^1]}\n)\n```");

                // Get method attributes
                sb = new("```ansi\n");
                foreach (var attribute in method.CustomAttributes
                    .Select(attr => attr.AttributeType)
                    .Where(attr => !(attr.FullName?.StartsWith("System") ?? false)))
                    sb.Append(Formatter.Colorize(attribute.Name.Replace("Attribute", string.Empty)
                            .Humanize(LetterCasing.Title), AnsiColor.Magenta))
                        .Append(",\n");

                // In order for a method to show up here, there has to be at least one attribute (CommandAttribute), so no
                // need to check for the length of the string builder before taking a slice
                embed.AddField("Attributes", $"{sb.ToString().Trim()[0..^1]}\n```");
            }

            embed.MakeWide();
            embed.WithFooter("<> = required, [] = optional");
        }

        return new DiscordMessageBuilder().AddEmbed(embed);
    }

    private static Command? GetCommand(IEnumerable<Command> commands, string name)
    {
        string commandName;
        int spaceIndex = -1;
        do
        {
            spaceIndex = name.IndexOf(' ', spaceIndex + 1);
            commandName = spaceIndex == -1
                ? name
                : name[..spaceIndex];

            commandName = commandName.Underscore();
            foreach (Command command in commands.Where(cmd => cmd.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
                return spaceIndex == -1
                    ? command
                    : GetCommand(command.Subcommands, name[(spaceIndex + 1)..]);

            // Search aliases
            foreach (Command command in commands)
            {
                foreach (Attribute attribute in command.Attributes)
                {
                    if (attribute is not TextAliasAttribute aliasAttribute)
                        continue;

                    if (aliasAttribute.Aliases.Any(alias => alias.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
                        return spaceIndex == -1
                            ? command
                            : GetCommand(command.Subcommands, name[(spaceIndex + 1)..]);
                }
            }

        } while (spaceIndex != -1);

        return null;
    }

    private static string GetUsage(this Command command)
    {
        StringBuilder builder = new();
        builder.AppendLine("```ansi");
        builder.Append('/');
        builder.Append(Formatter.Colorize(command.FullName, AnsiColor.Cyan));

        foreach (CommandParameter parameter in command.Parameters)
        {
            if (!parameter.DefaultValue.HasValue)
            {
                builder.Append(Formatter.Colorize(" <", AnsiColor.LightGray));
                builder.Append(Formatter.Colorize(parameter.Name.Titleize(), AnsiColor.Magenta));
                builder.Append(Formatter.Colorize(">", AnsiColor.LightGray));
            }
            else if (parameter.DefaultValue.Value != (parameter.Type.IsValueType
                ? Activator.CreateInstance(parameter.Type)
                : null))
            {
                builder.Append(Formatter.Colorize(" [", AnsiColor.Yellow));
                builder.Append(Formatter.Colorize(parameter.Name.Titleize(), AnsiColor.Magenta));
                builder.Append(Formatter.Colorize($" = ", AnsiColor.LightGray));
                builder.Append(Formatter.Colorize($"\"{parameter.DefaultValue.Value}\"", AnsiColor.Cyan));
                builder.Append(Formatter.Colorize("]", AnsiColor.Yellow));
            }
            else
            {
                builder.Append(Formatter.Colorize(" [", AnsiColor.Yellow));
                builder.Append(Formatter.Colorize(parameter.Name.Titleize(), AnsiColor.Magenta));
                builder.Append(Formatter.Colorize("]", AnsiColor.Yellow));
            }
        }

        builder.Append("```");
        return builder.ToString();
    }

    private static int CountCommands(this Command command)
    {
        int count = 0;
        if (command.Method is not null)
            count++;

        foreach (Command subcommand in command.Subcommands)
            count += CountCommands(subcommand);

        return count;
    }

    private static Type GetConverterFriendlyBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsEnum)
            return typeof(Enum);
        else if (type.IsArray)
            return type.GetElementType()!;

        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
