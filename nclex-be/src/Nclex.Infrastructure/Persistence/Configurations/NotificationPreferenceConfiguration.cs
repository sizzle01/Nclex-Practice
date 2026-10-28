using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="NotificationPreference"/> to <c>notification_preferences</c> (database.md section 2.8).</summary>
internal sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(p => new { p.UserId, p.Category });
        // The DDL defaults in_app to true; email has no default because it depends on the category.
        // ValueGeneratedNever: EF must always send in_app, or a false would be taken for "unset"
        // and become the default true.
        builder.Property(p => p.InApp).HasDefaultValue(true).ValueGeneratedNever();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
