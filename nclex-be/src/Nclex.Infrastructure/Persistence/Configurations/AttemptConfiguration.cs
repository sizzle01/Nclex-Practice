using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Attempt"/> to <c>attempts</c> (database.md section 2.6).</summary>
internal sealed class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    /// <summary>A user's drafts, newest activity first (dashboard).</summary>
    public const string DraftsIndex = "attempts_drafts_idx";

    /// <summary>A user's submitted attempts, newest first (history).</summary>
    public const string HistoryIndex = "attempts_history_idx";

    /// <summary>Running exam timers, for the expiry sweeper (F09).</summary>
    public const string ExpiryIndex = "attempts_expiry_idx";

    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        // The DDL's CHECKs have no names; these are what Postgres would call them.
        builder.ToTable("attempts", table =>
        {
            table.HasCheckConstraint("attempts_pause_reason_check", "pause_reason IN ('break','save_exit')");
            // A submitted attempt always says when.
            table.HasCheckConstraint("attempts_check", "status <> 'submitted' OR submitted_at IS NOT NULL");
            // The exam clock never runs while paused (architecture.md 5.1).
            table.HasCheckConstraint("attempts_check1", "status <> 'paused' OR exam_deadline_at IS NULL");
        });

        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.Status).HasDefaultValue(AttemptStatus.InProgress).HasSentinel((AttemptStatus)(-1));
        builder.Property(a => a.CurrentPosition).HasDefaultValue(1);
        builder.Property(a => a.TimerEpoch).HasDefaultValue(0);
        builder.Property(a => a.PausesUsed).HasDefaultValue((short)0);
        // The DDL keeps pause_reason as text with a CHECK rather than a Postgres enum.
        builder.Property(a => a.PauseReason)
            .HasColumnType("text")
            .HasConversion(
                reason => reason == PauseReason.Break ? "break" : "save_exit",
                column => column == "break" ? PauseReason.Break : PauseReason.SaveExit);
        builder.Property(a => a.TotalPausedMs).HasDefaultValue(0L);
        builder.Property(a => a.ActiveClientId).HasColumnType("text");
        builder.Property(a => a.CatState).HasColumnType("jsonb");
        builder.Property(a => a.StartedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.LastActivityAt).HasDefaultValueSql("now()");

        // The settings snapshot is fixed-shape JSON, stored like test_sets.pause_policy, with
        // camelCase keys and enum names so the stored JSON is readable.
        builder.OwnsOne(a => a.Settings, settings =>
        {
            settings.ToJson("settings");
            settings.Property(s => s.Mode).HasJsonPropertyName("mode").HasConversion<string>();
            settings.Property(s => s.RevealMode).HasJsonPropertyName("revealMode").HasConversion<string>();
            settings.Property(s => s.Timed).HasJsonPropertyName("timed");
            settings.Property(s => s.PerQuestionSec).HasJsonPropertyName("perQuestionSec");
            settings.Property(s => s.TimeBudgetSec).HasJsonPropertyName("timeBudgetSec");
            settings.Property(s => s.PassThreshold).HasJsonPropertyName("passThreshold");
            settings.Property(s => s.AllowBackNav).HasJsonPropertyName("allowBackNav");
            settings.OwnsOne(s => s.PausePolicy, policy =>
            {
                policy.HasJsonPropertyName("pausePolicy");
                policy.Property(p => p.MaxPauses).HasJsonPropertyName("maxPauses");
                policy.Property(p => p.MaxPauseSec).HasJsonPropertyName("maxPauseSec");
            });
        });

        builder.HasIndex(a => new { a.UserId, a.LastActivityAt })
            .HasDatabaseName(DraftsIndex)
            .IsDescending(false, true)
            .HasFilter("status IN ('in_progress','paused')");
        builder.HasIndex(a => new { a.UserId, a.SubmittedAt })
            .HasDatabaseName(HistoryIndex)
            .IsDescending(false, true)
            .HasFilter("status = 'submitted'");
        builder.HasIndex(a => a.ExamDeadlineAt)
            .HasDatabaseName(ExpiryIndex)
            .HasFilter("status = 'in_progress'");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<TestSet>()
            .WithMany()
            .HasForeignKey(a => a.TestSetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
