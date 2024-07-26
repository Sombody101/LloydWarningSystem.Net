using LloydWarningSystem.Net.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LloydWarningSystem.Net.Context;

public class LloydContext : DbContext
{
    public LloydContext(DbContextOptions<LloydContext> options)
        : base(options)
    { }

    public DbSet<UserDbEntity> Users { get; set; }
    public DbSet<GuildDbEntity> Guilds { get; set; }
    public DbSet<GuildConfigDbEntity> Configs { get; set; }
    public DbSet<IncidentDbEntity> Incidents { get; set; }
    public DbSet<StarboardMessageDbEntity> Starboard { get; set; }
    public DbSet<QuoteDbEntity> Quotes { get; set; }
    public DbSet<ReminderDbEntity> Reminders { get; set; }
    public DbSet<VoiceAlert> VoiceAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LloydContext).Assembly);

        // modelBuilder.Entity<MessageAlias>()
        //     .Property(e => e.MessageAliasesJson)
        //     .HasConversion(v => JsonConvert.SerializeObject(v),
        //                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(LloydBot.ConnectionString);
}
