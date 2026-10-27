using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence.Seed;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="BodySystem"/> to <c>body_systems</c> (database.md section 2.3).</summary>
internal sealed class BodySystemConfiguration : IEntityTypeConfiguration<BodySystem>
{
    public void Configure(EntityTypeBuilder<BodySystem> builder)
    {
        builder.ToTable("body_systems");

        // The DDL says smallserial; Npgsql's default for a short key would be an identity column.
        builder.Property(b => b.Id).UseSerialColumn();
        builder.Property(b => b.Slug).HasColumnType("text").IsRequired();
        builder.Property(b => b.Name).HasColumnType("text").IsRequired();

        builder.HasIndex(b => b.Slug).IsUnique();

        builder.HasData(TaxonomySeedData.BodySystems);
    }
}
