using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="MediaAsset"/> to <c>media_assets</c> (database.md section 2.4).</summary>
internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        // The DDL's CHECK has no name, so Postgres would call it media_assets_kind_check.
        builder.ToTable("media_assets", table => table.HasCheckConstraint(
            "media_assets_kind_check",
            "kind IN ('image','audio','video')"));

        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.Kind).HasColumnType("text").IsRequired();
        builder.Property(m => m.StorageKey).HasColumnType("text").IsRequired();
        builder.Property(m => m.MimeType).HasColumnType("text").IsRequired();
        builder.Property(m => m.AltText).HasColumnType("text");
        builder.Property(m => m.CaptionsKey).HasColumnType("text");
        builder.Property(m => m.TranscriptKey).HasColumnType("text");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(m => m.StorageKey).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
