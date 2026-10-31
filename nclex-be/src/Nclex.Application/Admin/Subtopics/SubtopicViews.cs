using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Subtopics;

/// <summary>A subtopic in the admin console.</summary>
/// <param name="ClientNeedsCode">Code of the Client Needs leaf, e.g. "PHI.PPT".</param>
/// <param name="QuestionCount">Questions tagged with it.</param>
public sealed record SubtopicView(
    Guid Id,
    ExamTrack Track,
    short ClientNeedsId,
    string ClientNeedsCode,
    string ClientNeedsName,
    string Slug,
    string Name,
    bool IsActive,
    short SortOrder,
    int QuestionCount);

/// <summary>Every matching subtopic. The list is a small, hand-kept taxonomy, so it is not paged.</summary>
public sealed record SubtopicList(IReadOnlyList<SubtopicView> Items);

/// <summary>A subtopic tag on a question.</summary>
public sealed record QuestionSubtopicView(Guid Id, string Name, short ClientNeedsId, bool IsActive);
