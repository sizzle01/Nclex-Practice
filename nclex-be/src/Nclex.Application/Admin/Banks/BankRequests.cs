using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Banks;

/// <summary>Body of POST /admin/question-banks.</summary>
/// <param name="QuestionIds">The bank's questions; order does not matter, and it may start empty.</param>
public sealed record CreateBankRequest(
    ExamTrack Track,
    string Name,
    string? Description,
    PlanTier AccessTier,
    bool IsPublished,
    IReadOnlyList<Guid>? QuestionIds);

/// <summary>Body of PATCH /admin/question-banks/{id}. Fields left out stay as they are; an empty description clears it.</summary>
/// <param name="QuestionIds">The whole membership, when changing it.</param>
public sealed record UpdateBankRequest(
    string? Name,
    string? Description,
    PlanTier? AccessTier,
    bool? IsPublished,
    IReadOnlyList<Guid>? QuestionIds);

/// <summary>Query string of GET /admin/question-banks.</summary>
public sealed record ListAdminBanksRequest(ExamTrack? Track = null, int? Limit = null, string? Cursor = null);
