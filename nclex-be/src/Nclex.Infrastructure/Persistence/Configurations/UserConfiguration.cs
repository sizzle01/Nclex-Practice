using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="User"/> to the <c>users</c> table exactly as in database.md section 2.2.</summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>Name of the unique index on email, used to recognise "email taken" races.</summary>
    public const string EmailUniqueIndex = "ix_users_email";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");

        // citext: emails are unique and looked up ignoring case.
        builder.Property(u => u.Email).HasColumnType("citext").IsRequired();
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName(EmailUniqueIndex);

        builder.Property(u => u.PasswordHash).HasColumnType("text");
        builder.Property(u => u.Role).HasDefaultValue(UserRole.Candidate).HasSentinel((UserRole)(-1));
        builder.Property(u => u.Track).IsRequired();
        builder.Property(u => u.PlanTier).HasDefaultValue(PlanTier.Free).HasSentinel((PlanTier)(-1));
        builder.Property(u => u.DisplayName).HasColumnType("text");
        builder.Property(u => u.Timezone).HasColumnType("text").HasDefaultValue(User.DefaultTimezone);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");

        builder.Ignore(u => u.IsDeleted);
    }
}
