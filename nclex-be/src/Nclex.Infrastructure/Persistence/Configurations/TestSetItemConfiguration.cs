using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="TestSetItem"/> to <c>test_set_items</c> (database.md section 2.5).</summary>
internal sealed class TestSetItemConfiguration : IEntityTypeConfiguration<TestSetItem>
{
    public void Configure(EntityTypeBuilder<TestSetItem> builder)
    {
        builder.ToTable("test_set_items");

        builder.HasKey(i => new { i.TestSetId, i.Position });
        // A question appears at most once in a set.
        builder.HasIndex(i => new { i.TestSetId, i.QuestionId }).IsUnique();

        // Deleting a set removes its items; the questions themselves stay.
        builder.HasOne<TestSet>()
            .WithMany()
            .HasForeignKey(i => i.TestSetId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(i => i.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
