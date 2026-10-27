using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="CaseStudy"/> to <c>case_studies</c> (database.md section 2.4).</summary>
internal sealed class CaseStudyConfiguration : IEntityTypeConfiguration<CaseStudy>
{
    public void Configure(EntityTypeBuilder<CaseStudy> builder)
    {
        builder.ToTable("case_studies");

        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Title).HasColumnType("text").IsRequired();
        builder.Property(c => c.Scenario).HasColumnType("jsonb").IsRequired();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
    }
}
