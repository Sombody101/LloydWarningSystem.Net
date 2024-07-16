using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace LloydWarningSystem.Net.FinderBot.Commands;

public static class ShipCommand
{
    [Command("ship")]
    public static async ValueTask ShipAsync(CommandContext ctx, DiscordUser user1, DiscordUser user2, bool show_hashes = false)
    {
        await ctx.DeferResponseAsync();

        var sw = Stopwatch.StartNew();

        var user1_hash = await user1.Username.GetHashString();
        var user2_hash = await user2.Username.GetHashString();

        var avatar_hash = await (await XorHashes(user1.AvatarHash, user2.AvatarHash)).GetHashString();
        var banner_hash = await (await XorHashes(user1.BannerHash ?? "$NULL",
            user2.BannerHash ?? "$NULL")).GetHashString();

        var combined_hash = await (await XorHashes(avatar_hash, banner_hash)).GetHashString();
        var name_hash = await (await XorHashes(user1_hash, user2_hash)).GetHashString();

        var final_hash = await (await XorHashes(combined_hash, name_hash)).GetHashString();
        var percent = HashToRange(final_hash);

        string message = percent switch
        {
            > 95 => "Get to fuckin already",
            > 80 => "That's AMAZING!",
            > 70 => "That's great!",
            > 60 => "That could be better.",
            > 50 => "That's okay.",
            > 40 => "It might not be meant to be.",
            > 30 => "Good luck with that one!",
            > 15 => "Just call it off.",
            _ => "lol",
        };

        sw.Stop();

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Compatibility between {user1.Username} and {user2.Username}")
            .WithColor(Shared.DefaultEmbedColor)
            .AddField($"{percent:n0}% compatibility!", message)
            .WithFooter($"Calculation Took {sw.ElapsedMilliseconds}ms\n\nFinal comparison hash: {final_hash}");

        if (show_hashes)
            embed.AddField(nameof(user1_hash),      $"{user1_hash} ({user1_hash.HashToRange()}%)")
                .AddField(nameof(user2_hash),       $"{user2_hash} ({user2_hash.HashToRange()}%)")
                .AddField(nameof(avatar_hash),      $"{avatar_hash} ({avatar_hash.HashToRange()}%)")
                .AddField(nameof(banner_hash),      $"{banner_hash} ({banner_hash.HashToRange()}%)")
                .AddField(nameof(combined_hash),    $"{combined_hash} ({combined_hash.HashToRange()}%)")
                .AddField(nameof(name_hash),        $"{name_hash} ({name_hash.HashToRange()}%)")
                .AddField(nameof(final_hash),       $"{final_hash} ({final_hash.HashToRange()}%)");

        await ctx.RespondAsync(embed: embed);
    }

    const double scalingFactor = 10000.0;
    private static double HashToRange(this string sha256Hash)
    {
        var l = UInt128.Parse(sha256Hash, NumberStyles.HexNumber);
        // Square root operation spreads the values more
        return +(Math.Sqrt((double)l) * scalingFactor % 101);
    }

    private static async ValueTask<byte[]> GetHash(string inputString)
        => SHA256.HashData(Encoding.UTF8.GetBytes(inputString)).Skip(16).ToArray();

    private static async ValueTask<string> GetHashString(this string inputString)
    {
        var sb = new StringBuilder();
        foreach (byte b in await GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    private static async ValueTask<string> XorHashes(string hash1, string hash2)
    {
        int length = hash1.Length;
        var result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            byte b1 = Convert.ToByte(hash1[i]);
            byte b2 = Convert.ToByte(hash2[i]);
            byte xorResult = (byte)(b1 ^ b2);

            result.Append(xorResult.ToString("X2"));
        }

        return result.ToString();
    }
}
