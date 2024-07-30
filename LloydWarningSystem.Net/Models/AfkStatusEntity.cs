using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LloydWarningSystem.Net.Models;

public class AfkStatusEntity
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("afk_message"), MaxLength(70)]
    public string? AfkMessage { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }

    [Column("afk_epoch")]
    public long AfkEpoch { get; set; }

    public UserDbEntity User { get; set; }
}

public static class Ext
{
    public static bool IsAfk(this AfkStatusEntity? status)
    {
        if (status is null || string.IsNullOrWhiteSpace(status.AfkMessage))
            return false;

        return true;
    }
}