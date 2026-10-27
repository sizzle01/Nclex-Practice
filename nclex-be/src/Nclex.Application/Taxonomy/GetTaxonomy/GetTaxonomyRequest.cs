using Nclex.Domain.Entities;

namespace Nclex.Application.Taxonomy.GetTaxonomy;

/// <summary>Query string of GET /taxonomy (api.md section 3).</summary>
/// <param name="Track">RN or PN; required, because the two test plans differ.</param>
public sealed record GetTaxonomyRequest(ExamTrack? Track = null);
