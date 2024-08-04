using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LloydWarningSystem.Net.Models;

public class GuildDbEntity
{
    public GuildDbEntity(ulong id)
    {
        Id = id;
        Incidents = [];
        Settings = new GuildConfigDbEntity();
        Quotes = [];
    }

    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; init; }

    public GuildConfigDbEntity Settings { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public List<QuoteDbEntity> Quotes { get; set; }

    public List<TrackingDbEntity> TrackingConfigurations { get; set; }
}

public class GuildDbEntityConfig : IEntityTypeConfiguration<GuildDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildDbEntity> builder)
    {
        builder.ToTable("guilds");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Settings)
            .WithOne(x => x.Guild)
            .HasForeignKey<GuildConfigDbEntity>(x => x.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Incidents)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Quotes)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}