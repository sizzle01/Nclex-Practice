using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Subtopics;

/// <summary>Query string of GET /admin/subtopics.</summary>
public sealed record ListSubtopicsRequest(ExamTrack? Track = null, short? ClientNeedsId = null);

/// <summary>Body of POST /admin/subtopics. The slug is made from the name.</summary>
/// <param name="ClientNeedsId">A Client Needs leaf.</param>
public sealed record CreateSubtopicRequest(short ClientNeedsId, string Name, short SortOrder = 0);

/// <summary>Body of PATCH /admin/subtopics/{id}. Fields left out stay as they are.</summary>
public sealed record UpdateSubtopicRequest(string? Name, short? SortOrder, bool? IsActive);

/// <summary>Body of PUT /admin/questions/{id}/subtopics: the question's whole set of tags.</summary>
public sealed record SetQuestionSubtopicsRequest(IReadOnlyList<Guid> SubtopicIds);
