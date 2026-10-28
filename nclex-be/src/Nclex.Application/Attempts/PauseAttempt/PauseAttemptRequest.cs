using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.PauseAttempt;

/// <summary>Body of POST /attempts/{id}/pause (api.md section 4): <c>break</c> or <c>save_exit</c>.</summary>
public sealed record PauseAttemptRequest(PauseReason? Reason);
