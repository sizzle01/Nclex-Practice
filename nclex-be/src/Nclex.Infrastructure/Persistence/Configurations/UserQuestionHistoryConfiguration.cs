using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="UserQuestionHistory"/> to <c>user_question_history</c> (database.md section 2.7).</summary>
internal sealed class UserQuestionHistoryConfiguration : IEntityTypeConfiguration<UserQuestionHistory>
{
    public void Configure(EntityTypeBuilder<UserQuestionHistory> builder)
    {
        builder.ToTable("user_question_history");

        builder.HasKey(h => new { h.UserId, h.QuestionId });
        builder.Property(h => h.TimesSeen).HasDefaultValue(0);

        // History is personal data, so it goes when the user row does (as in the DDL).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(h => h.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
