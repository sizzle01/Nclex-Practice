using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Membership;

/// <summary>A question in a set or bank, as the membership editor lists it.</summary>
/// <param name="Stem">The current (published) version's stem, or the latest one when none is published.</param>
/// <param name="PublishedVersion">Null when no version is published, so candidates would skip it.</param>
public sealed record MemberQuestion(
    Guid QuestionId,
    string Stem,
    ItemType ItemType,
    QuestionStatus Status,
    int? PublishedVersion);
