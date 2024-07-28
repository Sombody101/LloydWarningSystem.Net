using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace LloydWarningSystem.Net.Models;

public partial class MessageTag
{
    public static readonly Regex LocateTagRegex = TagRegex();

    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("tag_name")]
    public string Name { get; set; }

    [Column("tag_data")]
    public string Data { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }

    public UserDbEntity User { get; set; }

    [GeneratedRegex(@"\$(\S+)\b")]
    private static partial Regex TagRegex();
}
