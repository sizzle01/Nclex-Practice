using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Question"/> to <c>questions</c> (database.md section 2.4).</summary>
internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.Property(q => q.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(q => q.Status).HasDefaultValue(QuestionStatus.Active).HasSentinel((QuestionStatus)(-1));
        builder.Property(q => q.AccessTier).HasDefaultValue(PlanTier.Free).HasSentinel((PlanTier)(-1));
        builder.Property(q => q.CreatedAt).HasDefaultValueSql("now()");

        // questions and question_versions point at each other, so the migration creates both tables
        // first and adds this constraint afterwards, as the DDL does. Saving a new question therefore
        // takes two steps: insert it with no current version, then set the current version.
        builder.HasOne<QuestionVersion>()
            .WithMany()
            .HasForeignKey(q => q.CurrentVersionId)
            .HasConstraintName("questions_current_version_fk")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(q => q.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
