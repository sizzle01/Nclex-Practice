using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AuditEvent"/> to the <c>audit_events</c> table (database.md section 2.9).</summary>
internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        // The DDL says bigserial; Npgsql's default for a long key would be an identity column.
        builder.Property(a => a.Id).UseSerialColumn();
        builder.Property(a => a.Action).HasColumnType("text").IsRequired();
        builder.Property(a => a.EntityType).HasColumnType("text");
        builder.Property(a => a.EntityId).HasColumnType("text");
        builder.Property(a => a.Metadata).HasColumnType("jsonb").HasDefaultValueSql("'{}'").IsRequired();
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");

        // No ON DELETE in the DDL: user rows are never deleted, only scrubbed (database.md section 4).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.ActorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
