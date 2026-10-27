using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="EmailToken"/> to the <c>email_tokens</c> table (database.md section 2.2).</summary>
internal sealed class EmailTokenConfiguration : IEntityTypeConfiguration<EmailToken>
{
    public void Configure(EntityTypeBuilder<EmailToken> builder)
    {
        builder.ToTable("email_tokens");

        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.Purpose).IsRequired();
        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.Property(t => t.NewEmail).HasColumnType("citext");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");

        // A link can only be used once even if it is clicked twice at the same moment: EF adds
        // "AND consumed_at IS NULL" to the UPDATE, so the second request's save fails.
        builder.Property(t => t.ConsumedAt).IsConcurrencyToken();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Still-usable tokens per user and purpose: superseding old links and the resend limit.
        builder.HasIndex(t => new { t.UserId, t.Purpose }).HasFilter("consumed_at IS NULL");
    }
}
