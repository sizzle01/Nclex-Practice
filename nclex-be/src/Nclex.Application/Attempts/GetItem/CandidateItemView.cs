using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.GetItem;

/// <summary>
/// One item as the candidate sees it (api.md section 4, GET /attempts/{id}/items/{position}). It
/// never holds the answer key, rationale or educational objective: those wait for the reveal (F10).
/// </summary>
/// <param name="Content">The item type's content, e.g. options in this attempt's shuffled order.</param>
/// <param name="Exhibits">Charts, flowsheets and media shown with the item.</param>
/// <param name="CaseStudy">The shared scenario when the item is part of an unfolding case.</param>
/// <param name="Response">The saved answer, or null.</param>
/// <param name="ClientSeq">The sequence number of the saved answer; the save queue numbers its next write above it.</param>
/// <param name="TimeAllotmentSec">Suggested time for the item.</param>
public sealed record CandidateItemView(
    int Position,
    Guid QuestionVersionId,
    ItemType ItemType,
    string Stem,
    JsonElement Content,
    JsonElement Exhibits,
    CaseStudyView? CaseStudy,
    ItemState State,
    JsonElement? Response,
    int ClientSeq,
    bool Flagged,
    IReadOnlyList<string> StruckOptions,
    int TimeAllotmentSec);

/// <summary>An unfolding case's scenario tabs, and where this item sits in the case.</summary>
public sealed record CaseStudyView(Guid Id, string Title, JsonElement Scenario, int Position);
