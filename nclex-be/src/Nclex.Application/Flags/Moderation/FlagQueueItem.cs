using Nclex.Domain.Flags;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Flags.Moderation;

/// <summary>A person named on a report: the reporter, the assignee or whoever closed it.</summary>
/// <param name="DisplayName">Null when the user gave none.</param>
public sealed record FlagPerson(Guid Id, string? DisplayName);

/// <summary>The question version a report is about. Enough to find it in the editor; no answer key.</summary>
public sealed record FlaggedQuestion(Guid QuestionId, Guid VersionId, int Version, ItemType ItemType, string StemPreview);

/// <summary>One report in the moderation queue (GET /admin/flags, PATCH /admin/flags/{id}).</summary>
public sealed record FlagQueueItem(
    Guid Id,
    FlagStatus Status,
    FlagReason Reason,
    string? Comment,
    FlaggedQuestion Question,
    FlagPerson Reporter,
    FlagPerson? Assignee,
    string? ResolutionNote,
    FlagPerson? ResolvedBy,
    DateTimeOffset? ResolvedAt,
    Guid? AttemptId,
    DateTimeOffset CreatedAt);

/// <summary>One page of the queue, oldest first.</summary>
public sealed record FlagQueuePage(IReadOnlyList<FlagQueueItem> Items, string? NextCursor);
