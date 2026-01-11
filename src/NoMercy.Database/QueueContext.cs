using Microsoft.EntityFrameworkCore;
using NoMercy.Database.Models;
using NoMercy.NmSystem.Information;

namespace NoMercy.Database;

public class QueueContext : DbContext
{
    public QueueContext(DbContextOptions<QueueContext> options) : base(options)
    {
    }

    public QueueContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={AppFiles.QueueDatabase}; Pooling=True; Cache=Shared; Foreign Keys=True;");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<string>()
            .HaveMaxLength(256);

        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.Name is "CreatedAt" or "UpdatedAt")
            .ToList()
            .ForEach(p => p.SetDefaultValueSql("CURRENT_TIMESTAMP"));

        modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .ToList()
            .ForEach(p => p.DeleteBehavior = DeleteBehavior.Cascade);

        // EncoderV2 relationships and indexes
        modelBuilder.Entity<EncodingJob>()
            .HasOne(j => j.Profile)
            .WithMany()
            .HasForeignKey(j => j.ProfileId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EncodingJob>()
            .HasMany(j => j.Tasks)
            .WithOne(t => t.Job)
            .HasForeignKey(t => t.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EncodingTask>()
            .HasOne(t => t.AssignedNode)
            .WithMany()
            .HasForeignKey(t => t.AssignedNodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EncodingTask>()
            .HasMany(t => t.ProgressUpdates)
            .WithOne(p => p.Task)
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EncodingJob>()
            .HasIndex(j => j.State);

        modelBuilder.Entity<EncodingTask>()
            .HasIndex(t => new { t.JobId, t.State });

        modelBuilder.Entity<EncodingProgress>()
            .HasIndex(p => new { p.TaskId, p.RecordedAt });

        base.OnModelCreating(modelBuilder);
    }

    public virtual DbSet<QueueJob> QueueJobs { get; set; }
    public virtual DbSet<FailedJob> FailedJobs { get; set; }
    public virtual DbSet<CronJob> CronJobs { get; set; }

    // EncoderV2 tables
    public virtual DbSet<EncodingJob> EncodingJobs { get; set; }
    public virtual DbSet<EncodingTask> EncodingTasks { get; set; }
    public virtual DbSet<EncodingProgress> EncodingProgress { get; set; }
    public virtual DbSet<EncoderNode> EncoderNodes { get; set; }
    public virtual DbSet<EncodingNodeAssignment> EncodingNodeAssignments { get; set; }
}