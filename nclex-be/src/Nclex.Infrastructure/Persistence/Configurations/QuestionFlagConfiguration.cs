using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Domain.Flags;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="QuestionFlag"/> to <c>question_flags</c> (database.md section 2.9).</summary>
internal sealed class QuestionFlagConfiguration : IEntityTypeConfiguration<QuestionFlag>
{
    /// <summary>One report per user, version and reason; a second one is a duplicate (409).</summary>
    public const string OneReportPerReasonIndex = "ix_question_flags_user_id_question_version_id_reason";

    /// <summary>The moderation queue: reports still to handle, oldest first.</summary>
    public const string QueueIndex = "flags_queue_idx";

    public void Configure(EntityTypeBuilder<QuestionFlag> builder)
    {
        // The DDL's CHECK has no name, so Postgres would call it question_flags_comment_check.
        builder.ToTable("question_flags", table => table.HasCheckConstraint(
            "question_flags_comment_check",
            $"char_length(comment) <= {QuestionFlag.MaxCommentLength}"));

        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.Comment).HasColumnType("text");
        builder.Property(f => f.Status).HasDefaultValue(FlagStatus.Open);
        builder.Property(f => f.ResolutionNote).HasColumnType("text");
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<QuestionVersion>()
            .WithMany()
            .HasForeignKey(f => f.QuestionVersionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Attempt>()
            .WithMany()
            .HasForeignKey(f => f.AttemptId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.ResolvedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(f => new { f.UserId, f.QuestionVersionId, f.Reason })
            .IsUnique()
            .HasDatabaseName(OneReportPerReasonIndex);
        builder.HasIndex(f => new { f.Status, f.CreatedAt })
            .HasDatabaseName(QueueIndex)
            .HasFilter("status IN ('open','triaged')");
    }
}
