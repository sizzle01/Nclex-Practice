using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.TestSets;

/// <summary>Body of POST /admin/test-sets: a curated (prebuilt) set.</summary>
/// <param name="QuestionIds">The questions in the order they are served; at least one.</param>
/// <param name="IsPublished">Whether candidates can see it now; false lets an admin build it up first.</param>
public sealed record CreateTestSetRequest(ExamTrack Track, TestSetSettings Settings, IReadOnlyList<Guid> QuestionIds, bool IsPublished);

/// <summary>Body of PATCH /admin/test-sets/{id}. Each part left out stays as it is.</summary>
/// <param name="Settings">The whole settings object, when changing any setting.</param>
/// <param name="QuestionIds">The whole ordered list, when changing membership.</param>
public sealed record UpdateTestSetRequest(TestSetSettings? Settings, IReadOnlyList<Guid>? QuestionIds, bool? IsPublished);

/// <summary>Query string of GET /admin/test-sets.</summary>
public sealed record ListAdminTestSetsRequest(ExamTrack? Track = null, int? Limit = null, string? Cursor = null);
