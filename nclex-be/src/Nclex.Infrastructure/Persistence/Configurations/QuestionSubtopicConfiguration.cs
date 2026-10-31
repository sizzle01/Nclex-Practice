using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="QuestionSubtopic"/> to <c>question_subtopics</c> (database.md section 2.4).</summary>
internal sealed class QuestionSubtopicConfiguration : IEntityTypeConfiguration<QuestionSubtopic>
{
    public void Configure(EntityTypeBuilder<QuestionSubtopic> builder)
    {
        builder.ToTable("question_subtopics");

        builder.HasKey(t => new { t.QuestionId, t.SubtopicId });

        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Subtopic>()
            .WithMany()
            .HasForeignKey(t => t.SubtopicId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
