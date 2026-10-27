using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="TestSet"/> to <c>test_sets</c> (database.md section 2.5).</summary>
internal sealed class TestSetConfiguration : IEntityTypeConfiguration<TestSet>
{
    /// <summary>Partial index for the catalog listing (published sets by track and kind).</summary>
    public const string PublishedIndex = "ix_test_sets_track_kind";

    /// <summary>Partial index for a user's custom sets.</summary>
    public const string CustomOwnerIndex = "ix_test_sets_owner_user_id";

    public void Configure(EntityTypeBuilder<TestSet> builder)
    {
        // The DDL's CHECKs have no names; these are what Postgres would call them.
        builder.ToTable("test_sets", table =>
        {
            // Custom sets always belong to someone.
            table.HasCheckConstraint("test_sets_check", "kind <> 'custom' OR owner_user_id IS NOT NULL");
            // A timed set needs at least one timer.
            table.HasCheckConstraint("test_sets_check1", "NOT timed OR per_question_sec IS NOT NULL OR time_budget_sec IS NOT NULL");
        });

        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Title).HasColumnType("text").IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.Property(s => s.Mode).HasDefaultValue(TestMode.Practice).HasSentinel((TestMode)(-1));
        builder.Property(s => s.RevealMode).HasDefaultValue(RevealMode.Tutor).HasSentinel((RevealMode)(-1));
        builder.Property(s => s.Timed).HasDefaultValue(false);
        builder.Property(s => s.PassThreshold).HasPrecision(5, 4);
        // With a default of true, false must be the value EF always sends; true can be left to the database.
        builder.Property(s => s.AllowBackNav).HasDefaultValue(true).HasSentinel(true);
        builder.Property(s => s.IsSample).HasDefaultValue(false);
        builder.Property(s => s.AccessTier).HasDefaultValue(PlanTier.Free).HasSentinel((PlanTier)(-1));
        builder.Property(s => s.IsPublished).HasDefaultValue(false);
        builder.Property(s => s.GeneratorCriteria).HasColumnType("jsonb");
        builder.Property(s => s.CatConfig).HasColumnType("jsonb");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");

        // Fixed-shape JSON as an owned type (database.md, EF Core mapping notes), with the DDL's
        // camelCase keys so the stored JSON reads the same as the API.
        builder.OwnsOne(s => s.PausePolicy, policy =>
        {
            policy.ToJson("pause_policy");
            policy.Property(p => p.MaxPauses).HasJsonPropertyName("maxPauses");
            policy.Property(p => p.MaxPauseSec).HasJsonPropertyName("maxPauseSec");
        });

        builder.HasIndex(s => new { s.Track, s.Kind }, PublishedIndex).HasFilter("is_published");
        builder.HasIndex(s => s.OwnerUserId, CustomOwnerIndex).HasFilter("kind = 'custom'");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
