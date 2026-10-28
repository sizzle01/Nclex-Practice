using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.ReportIssue;

/// <summary>Body of POST /question-versions/{id}/flags (api.md section 7).</summary>
/// <param name="Reason">Why the question is being reported.</param>
/// <param name="Comment">Optional details, up to 2000 characters.</param>
/// <param name="AttemptId">The attempt the question was seen in, when reported from the player or review page.</param>
public sealed record ReportIssueRequest(FlagReason? Reason, string? Comment = null, Guid? AttemptId = null);
