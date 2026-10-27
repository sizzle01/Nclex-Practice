using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListQuestionBanks;

/// <summary>Query string of GET /question-banks (api.md section 3).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record ListQuestionBanksRequest(ExamTrack? Track = null);
