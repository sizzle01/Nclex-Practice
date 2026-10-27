using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence.Seed;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="ClientNeedsCategory"/> to <c>client_needs_categories</c> (database.md section 2.3).</summary>
internal sealed class ClientNeedsCategoryConfiguration : IEntityTypeConfiguration<ClientNeedsCategory>
{
    public void Configure(EntityTypeBuilder<ClientNeedsCategory> builder)
    {
        builder.ToTable("client_needs_categories");

        // The DDL says smallserial; Npgsql's default for a short key would be an identity column.
        builder.Property(c => c.Id).UseSerialColumn();
        builder.Property(c => c.Code).HasColumnType("text").IsRequired();
        builder.Property(c => c.Name).HasColumnType("text").IsRequired();
        builder.Property(c => c.WeightMin).HasPrecision(5, 2);
        builder.Property(c => c.WeightMax).HasPrecision(5, 2);
        builder.Property(c => c.SortOrder).HasDefaultValue((short)0);

        builder.HasOne<ClientNeedsCategory>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.Track, c.Code }).IsUnique();

        builder.HasData(TaxonomySeedData.ClientNeedsCategories);
    }
}
