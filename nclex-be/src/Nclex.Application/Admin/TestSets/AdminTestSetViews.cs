using Nclex.Application.Admin.Membership;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.TestSets;

/// <summary>A curated set in the admin list, published or not.</summary>
public sealed record AdminTestSetSummary(
    Guid Id,
    ExamTrack Track,
    string Title,
    TestMode Mode,
    bool IsSample,
    PlanTier AccessTier,
    bool IsPublished,
    int QuestionCount,
    DateTimeOffset CreatedAt);

/// <summary>Body of GET /admin/test-sets.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> for the next page; null on the last page.</param>
public sealed record AdminTestSetPage(IReadOnlyList<AdminTestSetSummary> Items, string? NextCursor);

/// <summary>One curated set with its settings and ordered questions, for the set editor.</summary>
public sealed record AdminTestSetView(
    Guid Id,
    ExamTrack Track,
    TestSetSettings Settings,
    bool IsPublished,
    int QuestionCount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<MemberQuestion> Questions);
