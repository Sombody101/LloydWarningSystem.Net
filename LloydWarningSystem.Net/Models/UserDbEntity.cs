using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LloydWarningSystem.Net.Models;

public class UserDbEntity
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public List<ReminderDbEntity> Reminders { get; set; }
    public List<VoiceAlert> VoiceAlerts { get; set; }

    /// <summary>
    /// Can use special commands
    /// </summary>
    [DefaultValue(false)]
    public bool IsBotAdmin { get; set; }

    // /// <summary>
    // /// Used with <see cref="UserReactionCommand"/>
    // /// </summary>
    // [DefaultValue(null)]
    // public string? ReactionEmoji { get; set; }
}

public class UserDbEntityConfig : IEntityTypeConfiguration<UserDbEntity>
{
    public void Configure(EntityTypeBuilder<UserDbEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Incidents)
            .WithOne(x => x.TargetUser)
            .HasForeignKey(x => x.TargetId);

        builder.HasMany(x => x.Reminders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.VoiceAlerts)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.IsBotAdmin);

        // builder.Property(x => x.ReactionEmoji);
    }
}