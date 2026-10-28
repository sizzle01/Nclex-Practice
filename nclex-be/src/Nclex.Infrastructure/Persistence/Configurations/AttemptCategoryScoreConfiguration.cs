using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AttemptCategoryScore"/> to <c>attempt_category_scores</c> (database.md section 2.7).</summary>
internal sealed class AttemptCategoryScoreConfiguration : IEntityTypeConfiguration<AttemptCategoryScore>
{
    public void Configure(EntityTypeBuilder<AttemptCategoryScore> builder)
    {
        // The DDL's CHECK has no name; this is what Postgres would call it.
        builder.ToTable("attempt_category_scores", table => table.HasCheckConstraint(
            "attempt_category_scores_dimension_check",
            "dimension IN ('client_needs','client_needs_parent','body_system','item_type','cjmm_skill','difficulty')"));

        // The primary key starts with attempt_id, so it also serves the results page's read.
        builder.HasKey(s => new { s.AttemptId, s.Dimension, s.DimensionKey });

        // The DDL keeps dimension as text with a CHECK rather than a Postgres enum.
        builder.Property(s => s.Dimension)
            .HasColumnType("text")
            .HasConversion(dimension => DimensionKeys.For(dimension), column => DimensionKeys.ParseDimension(column));
        builder.Property(s => s.DimensionKey).HasColumnType("text");
        builder.Property(s => s.PointsEarned).HasPrecision(8, 2);
        builder.Property(s => s.MaxPoints).HasPrecision(8, 2);
        builder.Ignore(s => s.Pct);

        builder.HasOne<Attempt>()
            .WithMany()
            .HasForeignKey(s => s.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
