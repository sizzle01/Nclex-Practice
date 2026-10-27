namespace Nclex.Domain.Entities;

/// <summary>
/// Where a question version is in the review workflow (database enum <c>version_status</c>).
/// A version can only be edited while it is a draft (database.md section 2.4).
/// </summary>
public enum VersionStatus
{
    Draft,
    InReview,
    Approved,

    /// <summary>The version candidates see; at most one per question.</summary>
    Published,

    /// <summary>Was published, then replaced by a newer version. Kept so old attempts still render.</summary>
    Superseded,
    Rejected,
}
