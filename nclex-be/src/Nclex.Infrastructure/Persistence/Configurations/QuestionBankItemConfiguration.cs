using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="QuestionBankItem"/> to <c>question_bank_items</c> (database.md section 2.4).</summary>
internal sealed class QuestionBankItemConfiguration : IEntityTypeConfiguration<QuestionBankItem>
{
    public void Configure(EntityTypeBuilder<QuestionBankItem> builder)
    {
        builder.ToTable("question_bank_items");

        builder.HasKey(i => new { i.BankId, i.QuestionId });

        // Deleting a bank removes its memberships; the questions themselves stay.
        builder.HasOne<QuestionBank>()
            .WithMany()
            .HasForeignKey(i => i.BankId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(i => i.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
