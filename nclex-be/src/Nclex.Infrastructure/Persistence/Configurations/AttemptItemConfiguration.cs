using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AttemptItem"/> to <c>attempt_items</c> (database.md section 2.6).</summary>
internal sealed class AttemptItemConfiguration : IEntityTypeConfiguration<AttemptItem>
{
    public void Configure(EntityTypeBuilder<AttemptItem> builder)
    {
        builder.ToTable("attempt_items");

        builder.HasKey(i => new { i.AttemptId, i.Position });

        builder.Property(i => i.State).HasDefaultValue(ItemState.Unseen).HasSentinel((ItemState)(-1));
        builder.Property(i => i.Response).HasColumnType("jsonb");
        builder.Property(i => i.ClientSeq).HasDefaultValue(0);
        builder.Property(i => i.Flagged).HasDefaultValue(false);
        builder.Property(i => i.TimeSpentMs).HasDefaultValue(0L);
        builder.Property(i => i.PointsEarned).HasPrecision(6, 2);
        builder.Property(i => i.GradeDetail).HasColumnType("jsonb");

        // A JSON array of option ids. Stored as jsonb text and compared by content, so EF notices
        // when the list changes.
        builder.Property(i => i.StruckOptions)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'")
            .HasConversion(
                ids => JsonSerializer.Serialize(ids, JsonSerializerOptions.Default),
                json => JsonSerializer.Deserialize<List<string>>(json, JsonSerializerOptions.Default) ?? new List<string>(),
                new ValueComparer<IReadOnlyList<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    ids => ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.GetHashCode(StringComparison.Ordinal))),
                    ids => ids.ToList()))
            .IsRequired();

        // Deleting an attempt removes its items; the question versions stay.
        builder.HasOne<Attempt>()
            .WithMany()
            .HasForeignKey(i => i.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<QuestionVersion>()
            .WithMany()
            .HasForeignKey(i => i.QuestionVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
