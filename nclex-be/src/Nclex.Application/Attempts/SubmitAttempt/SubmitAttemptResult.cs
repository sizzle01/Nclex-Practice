using Nclex.Application.Attempts.Results;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.SubmitAttempt;

/// <summary>Body returned by POST /attempts/{id}/submit; the same on every repeat of the call.</summary>
public sealed record SubmitAttemptResult(
    Guid AttemptId,
    AttemptStatus Status,
    SubmitReason SubmitReason,
    AttemptResultSummary Result);
