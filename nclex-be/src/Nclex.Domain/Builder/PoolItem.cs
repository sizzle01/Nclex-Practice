namespace Nclex.Domain.Builder;

/// <summary>A question the custom builder may use: one row of the pool query (architecture.md 5.7).</summary>
/// <param name="QuestionId">The question (not a version).</param>
/// <param name="ClientNeedsId">The Client Needs leaf it is tagged to.</param>
/// <param name="CaseStudyId">The case study it belongs to, if any.</param>
/// <param name="CasePosition">Its place in the case study (1 to 6), set exactly when <paramref name="CaseStudyId"/> is.</param>
public sealed record PoolItem(Guid QuestionId, short ClientNeedsId, Guid? CaseStudyId = null, short? CasePosition = null);

/// <summary>A selected Client Needs leaf and its share of the NCLEX test plan.</summary>
/// <param name="ClientNeedsId">The leaf.</param>
/// <param name="Weight">Its relative share, e.g. 18 for a leaf weighted 15 to 21%.</param>
public sealed record LeafWeight(short ClientNeedsId, decimal Weight);
