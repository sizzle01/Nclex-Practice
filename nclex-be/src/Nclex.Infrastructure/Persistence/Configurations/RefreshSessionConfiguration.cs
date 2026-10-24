using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="RefreshSession"/> to the <c>refresh_sessions</c> table (database.md section 2.2).</summary>
internal sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");

        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.TokenHash).IsRequired();
        builder.HasIndex(s => s.TokenHash).IsUnique();
        builder.Property(s => s.UserAgent).HasColumnType("text");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.LastUsedAt).HasDefaultValueSql("now()");

        // A rotation only succeeds if the row is still unrevoked when it is written: EF adds
        // "AND revoked_at IS NULL" to the UPDATE. Two requests racing with the same token cannot
        // both rotate it; the loser is treated as reuse (architecture.md section 6).
        builder.Property(s => s.RevokedAt).IsConcurrencyToken();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<RefreshSession>()
            .WithMany()
            .HasForeignKey(s => s.ReplacedBy)
            .OnDelete(DeleteBehavior.NoAction);

        // Active sessions per user, for "log out everywhere"; and the family, for reuse revocation.
        builder.HasIndex(s => s.UserId).HasFilter("revoked_at IS NULL");
        builder.HasIndex(s => s.FamilyId);
    }
}
