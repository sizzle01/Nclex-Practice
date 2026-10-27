using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="QuestionVersion"/> to <c>question_versions</c> (database.md section 2.4).</summary>
internal sealed class QuestionVersionConfiguration : IEntityTypeConfiguration<QuestionVersion>
{
    /// <summary>Partial index for the custom test builder's pool query; named as in the DDL.</summary>
    public const string FilterIndex = "qv_filter_idx";

    public void Configure(EntityTypeBuilder<QuestionVersion> builder)
    {
        // The DDL's CHECKs have no names; the first two are what Postgres would call them.
        builder.ToTable("question_versions", table =>
        {
            table.HasCheckConstraint("question_versions_time_allotment_sec_check", "time_allotment_sec > 0");
            table.HasCheckConstraint("question_versions_max_points_check", "max_points > 0");
            // A version is either in a case study with a position, or in neither.
            table.HasCheckConstraint("question_versions_case_study_check", "(case_study_id IS NULL) = (case_position IS NULL)");
        });

        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.Status).HasDefaultValue(VersionStatus.Draft).HasSentinel((VersionStatus)(-1));
        builder.Property(v => v.Stem).HasColumnType("text").IsRequired();
        builder.Property(v => v.Content).HasColumnType("jsonb").IsRequired();
        builder.Property(v => v.AnswerKey).HasColumnType("jsonb").IsRequired();
        builder.Property(v => v.Rationale).HasColumnType("jsonb").IsRequired();
        builder.Property(v => v.EducationalObjective).HasColumnType("text").IsRequired();
        builder.Property(v => v.Exhibits).HasColumnType("jsonb").HasDefaultValueSql("'[]'").IsRequired();
        builder.Property(v => v.IrtA).HasPrecision(6, 3);
        builder.Property(v => v.IrtB).HasPrecision(6, 3);
        builder.Property(v => v.IrtC).HasPrecision(6, 3);
        builder.Property(v => v.TimeAllotmentSec).HasDefaultValue(120);
        builder.Property(v => v.ReviewNotes).HasColumnType("text");
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(v => v.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<ClientNeedsCategory>()
            .WithMany()
            .HasForeignKey(v => v.ClientNeedsId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<BodySystem>()
            .WithMany()
            .HasForeignKey(v => v.BodySystemId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<CaseStudy>()
            .WithMany()
            .HasForeignKey(v => v.CaseStudyId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(v => v.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(v => v.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(v => new { v.QuestionId, v.Version }).IsUnique();
        // The DDL's own index names. Each needs a model name too, because two of them share columns.
        builder.HasIndex(v => new { v.ClientNeedsId, v.BodySystemId, v.ItemType, v.Difficulty }, FilterIndex)
            .HasDatabaseName(FilterIndex)
            .HasFilter("status = 'published'");
        builder.HasIndex(v => new { v.QuestionId, v.Version }, "qv_question_idx")
            .HasDatabaseName("qv_question_idx")
            .IsDescending(false, true);
        builder.HasIndex(v => new { v.Status, v.CreatedAt }, "qv_review_queue_idx")
            .HasDatabaseName("qv_review_queue_idx")
            .HasFilter("status = 'in_review'");
    }
}
