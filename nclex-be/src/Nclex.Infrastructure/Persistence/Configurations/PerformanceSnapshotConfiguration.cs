using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="PerformanceSnapshot"/> to <c>performance_snapshots</c> (database.md section 2.7).</summary>
internal sealed class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
{
    public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
    {
        builder.ToTable("performance_snapshots");

        builder.HasKey(s => new { s.UserId, s.Track, s.Dimension, s.DimensionKey });

        // The DDL keeps dimension as free text; "overall" is one more value next to the breakdown's.
        builder.Property(s => s.Dimension)
            .HasColumnType("text")
            .HasConversion(dimension => PerformanceDimensions.Name(dimension), column => ParseDimension(column));
        builder.Property(s => s.DimensionKey).HasColumnType("text");
        builder.Property(s => s.PointsEarned).HasPrecision(10, 2);
        builder.Property(s => s.MaxPoints).HasPrecision(10, 2);
        builder.Property(s => s.Accuracy)
            .HasPrecision(5, 4)
            .HasComputedColumnSql("CASE WHEN max_points > 0 THEN points_earned / max_points ELSE 0 END", stored: true);
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static PerformanceDimension ParseDimension(string column) =>
        PerformanceDimensions.TryParse(column, out var dimension)
            ? dimension
            : throw new InvalidOperationException($"Unknown performance dimension '{column}'.");
}
