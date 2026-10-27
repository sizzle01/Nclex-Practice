using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="QuestionBank"/> to <c>question_banks</c> (database.md section 2.4).</summary>
internal sealed class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
{
    public void Configure(EntityTypeBuilder<QuestionBank> builder)
    {
        builder.ToTable("question_banks");

        builder.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.Name).HasColumnType("text").IsRequired();
        builder.Property(b => b.Description).HasColumnType("text");
        builder.Property(b => b.AccessTier).HasDefaultValue(PlanTier.Free).HasSentinel((PlanTier)(-1));
        builder.Property(b => b.IsPublished).HasDefaultValue(false);
        builder.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    }
}
