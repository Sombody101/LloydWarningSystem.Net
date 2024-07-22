using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LloydWarningSystem.Net.Models;

public class GuildConfigDbEntity
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    /// <summary>
    /// Snowflake id of the guild the config is related to
    /// </summary>
    [Required, Column("discordId")]
    public ulong GuildId { get; set; }

    [Column("prefix"), MaxLength(5)]
    public string Prefix { get; set; }

    [Column("starboardEnabled")]
    public bool StarboardActive { get; set; }

    [Column("starboardChannel")]
    public ulong? StarboardChannelId { get; set; }

    [Column("starboardThreshold")]
    public int? StarboardThreshold { get; set; }

    [Column("starboardEmojiId")]
    public ulong? StarboardEmojiId { get; set; }

    [Column("starboardEmojiName"), MaxLength(50)]
    public string? StarboardEmojiName { get; set; }

    public GuildDbEntity Guild;
}