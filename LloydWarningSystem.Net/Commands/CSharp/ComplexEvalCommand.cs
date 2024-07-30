using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using Humanizer;
using LloydWarningSystem.Net.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Formatter = DSharpPlus.Formatter;

namespace LloydWarningSystem.Net.FinderBot.Commands.CSharp;

public static class ComplexEvalCommand
{
    private const int MaxFormattedFieldSize = 1000;
    private const int MaxFieldNameLength = 256;

    private static readonly HttpClient _httpClient = new();

    public class Result
    {
        public object ReturnValue { get; set; }
        public string Exception { get; set; }
        public string Code { get; set; }
        public string ExceptionType { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public TimeSpan CompileTime { get; set; }
        public string ConsoleOut { get; set; }
        public string ReturnTypeName { get; set; }
    }

    [Command("cs")]
    public static async Task EvaluateCSharpAsync(TextCommandContext ctx, [FromCode] string code)
    {
        if (ctx.Channel is not DiscordChannel || ctx.User is not DiscordMember guildUser)
        {
            await ModifyOrSendErrorEmbed("The REPL can only be executed in public guild channels.", ctx);
            return;
        }

        var message = await ctx.Channel
            .SendMessageAsync(embed: new DiscordEmbedBuilder()
                .WithTitle("REPL Executing")
                .WithAuthor(ctx.User.Username)
                .WithColor(DiscordColor.Orange)
                .WithDescription($"Compiling and Executing [your code]({ctx.Message.JumpLink})...")
                .Build());

        var content = new StringContent(code, Encoding.UTF8, "text/plain");

        try
        {
            var response = await _httpClient.PostAsync(ConfigManager.BotConfig.ReplUrl, content);

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest)
            {
                await ModifyOrSendErrorEmbed($"Status Code: {(int)response.StatusCode} {response.StatusCode}", ctx, message);
                return;
            }

            var replResponse = JsonConvert.DeserializeObject<Result>(await response.Content.ReadAsStringAsync());

            if (replResponse is null)
            {
                await ModifyOrSendErrorEmbed("Failed to deserialize the REPL result from JSON to a Result!", ctx, message);
                return;
            }

            var embed = await BuildEmbedAsync(ctx.User, replResponse);

            await message.ModifyAsync(msg =>
            {
                msg.Content = null;
                msg.ClearEmbeds();
                msg.AddEmbed(embed);
            });
        }
        catch (HttpRequestException ex)
        {
            await ctx.RespondAsync($"Error communicating with REPL server: {ex.Message}");
        }
    }

    private static async Task ModifyOrSendErrorEmbed(string error, TextCommandContext ctx, DiscordMessage? message = null)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("REPL Error")
            .WithAuthor(ctx.User.Username)
            .WithColor(DiscordColor.Red)
            .AddField("Tried to execute", $"[this code]({ctx.Message.JumpLink})")
            .WithDescription(error);

        if (message is null)
        {
            await ctx.RespondAsync(embed);
        }
        else
        {
            await message.ModifyAsync(msg =>
            {
                msg.Content = null;
                msg.ClearEmbeds();
                msg.AddEmbed(embed);
            });
        }
    }

    private static async Task<DiscordEmbedBuilder> BuildEmbedAsync(DiscordUser guildUser, Result parsedResult)
    {
        var returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
        var consoleOut = parsedResult.ConsoleOut;
        var hasException = !string.IsNullOrEmpty(parsedResult.Exception);
        var status = hasException ? "Failure" : "Success";

        var embed = new DiscordEmbedBuilder()
                .WithTitle($"REPL Result: {status}")
                .WithColor(hasException ? DiscordColor.Red : DiscordColor.Green)
                .WithAuthor(guildUser.Username)
                .WithFooter($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms");

        embed.WithDescription(FormatOrEmptyCodeblock(parsedResult.Code, "cs"));

        if (parsedResult.ReturnValue is not null)
        {
            embed.AddField($"Result: {parsedResult.ReturnTypeName}".Truncate(MaxFormattedFieldSize),
                FormatOrEmptyCodeblock(returnValue.Truncate(MaxFormattedFieldSize), "json"));
        }

        if (!string.IsNullOrWhiteSpace(consoleOut))
        {
            embed.AddField("Console Output", Formatter.BlockCode(consoleOut.Truncate(MaxFormattedFieldSize), "txt"));
        }

        if (hasException)
        {
            var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
            embed.AddField($"Exception: {parsedResult.ExceptionType}".Truncate(MaxFieldNameLength),
                Formatter.BlockCode(diffFormatted.Truncate(MaxFormattedFieldSize), "diff"));
        }

        return embed;
    }

    private static string FormatOrEmptyCodeblock(string input, string language)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "```\n```";

        return Formatter.BlockCode(input, language);
    }
}