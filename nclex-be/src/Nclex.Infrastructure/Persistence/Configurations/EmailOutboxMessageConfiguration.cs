using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Email;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="EmailOutboxMessage"/> to the <c>email_outbox</c> table (database.md section 2.8).</summary>
internal sealed class EmailOutboxMessageConfiguration : IEntityTypeConfiguration<EmailOutboxMessage>
{
    /// <summary>Partial index the worker uses to find due emails; named as in the DDL.</summary>
    public const string PendingIndex = "outbox_pending_idx";

    public void Configure(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        // The DDL's CHECK has no name, so Postgres would call it email_outbox_status_check.
        builder.ToTable("email_outbox", table => table.HasCheckConstraint(
            "email_outbox_status_check",
            "status IN ('pending','sending','sent','failed')"));

        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.ToEmail).HasColumnType("citext").IsRequired();
        builder.Property(m => m.Template).HasColumnType("text").IsRequired();
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();

        // status is text with a CHECK in the DDL, not a Postgres enum, so it is stored as lower-case text.
        builder.Property(m => m.Status)
            .HasColumnType("text")
            .HasConversion(
                status => status.ToString().ToLowerInvariant(),
                text => Enum.Parse<OutboxStatus>(text, ignoreCase: true))
            .HasDefaultValue(OutboxStatus.Pending)
            .HasSentinel((OutboxStatus)(-1));
        builder.Property(m => m.Attempts).HasDefaultValue((short)0).HasSentinel((short)-1);
        builder.Property(m => m.SendAfter).HasDefaultValueSql("now()");
        builder.Property(m => m.ProviderId).HasColumnType("text");
        builder.Property(m => m.LastError).HasColumnType("text");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.SendAfter).HasDatabaseName(PendingIndex).HasFilter("status = 'pending'");
    }
}
