using Nclex.Application.Admin.Membership;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Banks;

/// <summary>A bank in the admin list, published or not.</summary>
public sealed record AdminBankSummary(
    Guid Id,
    ExamTrack Track,
    string Name,
    PlanTier AccessTier,
    bool IsPublished,
    int QuestionCount,
    DateTimeOffset CreatedAt);

/// <summary>Body of GET /admin/question-banks.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> for the next page; null on the last page.</param>
public sealed record AdminBankPage(IReadOnlyList<AdminBankSummary> Items, string? NextCursor);

/// <summary>One bank with its questions, for the bank editor.</summary>
public sealed record AdminBankView(
    Guid Id,
    ExamTrack Track,
    string Name,
    string? Description,
    PlanTier AccessTier,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    IReadOnlyList<MemberQuestion> Questions);
