using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Subtopic"/> to <c>subtopics</c> (database.md section 2.4).</summary>
internal sealed class SubtopicConfiguration : IEntityTypeConfiguration<Subtopic>
{
    public void Configure(EntityTypeBuilder<Subtopic> builder)
    {
        builder.ToTable("subtopics");

        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Slug).HasColumnType("text").IsRequired();
        builder.Property(s => s.Name).HasColumnType("text").IsRequired();
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.SortOrder).HasDefaultValue((short)0);

        // Subtopics are deactivated, never deleted, so old tags keep their meaning.
        builder.HasOne<ClientNeedsCategory>()
            .WithMany()
            .HasForeignKey(s => s.ClientNeedsId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => new { s.ClientNeedsId, s.Slug }).IsUnique();
    }
}
