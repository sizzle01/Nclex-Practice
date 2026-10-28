using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AttemptResult"/> to <c>attempt_results</c> (database.md section 2.7).</summary>
internal sealed class AttemptResultConfiguration : IEntityTypeConfiguration<AttemptResult>
{
    public void Configure(EntityTypeBuilder<AttemptResult> builder)
    {
        builder.ToTable("attempt_results");

        // One result per attempt: the primary key is also what makes a retried submit harmless.
        builder.HasKey(r => r.AttemptId);

        builder.Property(r => r.PointsEarned).HasPrecision(8, 2);
        builder.Property(r => r.MaxPoints).HasPrecision(8, 2);
        builder.Property(r => r.ScorePct).HasPrecision(5, 4);
        builder.Property(r => r.PassThreshold).HasPrecision(5, 4);
        // The DDL keeps readiness_label as text rather than a Postgres enum.
        builder.Property(r => r.ReadinessLabel)
            .HasColumnType("text")
            .HasConversion(
                label => label == ReadinessLabel.LikelyReady ? "likely_ready" : "needs_more_practice",
                column => column == "likely_ready" ? ReadinessLabel.LikelyReady : ReadinessLabel.NeedsMorePractice);
        builder.Property(r => r.CatTheta).HasPrecision(6, 3);
        builder.Property(r => r.CatSe).HasPrecision(6, 3);
        builder.Property(r => r.GradedAt).HasDefaultValueSql("now()");

        builder.HasOne<Attempt>()
            .WithOne()
            .HasForeignKey<AttemptResult>(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
