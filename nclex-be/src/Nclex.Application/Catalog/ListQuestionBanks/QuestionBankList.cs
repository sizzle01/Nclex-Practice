using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListQuestionBanks;

/// <summary>Body of GET /question-banks. Banks are few, so the list is not paged.</summary>
public sealed record QuestionBankList(IReadOnlyList<QuestionBankSummary> Items);

/// <param name="QuestionCount">Questions in the bank that can be served: active, with a published version.</param>
/// <param name="Locked">True when the bank is premium and the user's plan is free.</param>
public sealed record QuestionBankSummary(
    Guid Id,
    string Name,
    string? Description,
    PlanTier AccessTier,
    int QuestionCount,
    bool Locked);
