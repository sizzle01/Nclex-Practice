using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="Notification"/> to <c>notifications</c> (database.md section 2.8).</summary>
internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>The feed, newest first.</summary>
    public const string FeedIndex = "notif_feed_idx";

    /// <summary>The unread count behind the bell's badge.</summary>
    public const string UnreadIndex = "notif_unread_idx";

    /// <summary>At most one result notification per attempt, so a retried grading can't double-notify.</summary>
    public const string ResultOnceIndex = "notif_result_once";

    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.Property(n => n.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(n => n.Title).HasColumnType("text").IsRequired();
        builder.Property(n => n.Body).HasColumnType("text").IsRequired();
        builder.Property(n => n.Data).HasColumnType("jsonb").HasDefaultValueSql("'{}'").IsRequired();
        builder.Property(n => n.Channels).HasColumnType("text[]").IsRequired();
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Attempt>()
            .WithMany()
            .HasForeignKey(n => n.RelatedAttemptId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName(FeedIndex)
            .IsDescending(false, true);
        builder.HasIndex(n => n.UserId)
            .HasDatabaseName(UnreadIndex)
            .HasFilter("read_at IS NULL");
        builder.HasIndex(n => n.RelatedAttemptId)
            .HasDatabaseName(ResultOnceIndex)
            .IsUnique()
            .HasFilter("type IN ('result_pass','result_fail','result_ready')");
    }
}
