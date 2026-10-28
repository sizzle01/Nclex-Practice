using System.Text.Json;

namespace Nclex.Application.Attempts.SubmitAttempt;

/// <summary>
/// Body of POST /attempts/{id}/submit (api.md section 4). Optional: <c>finalResponses</c> carries
/// answers the save queue had not sent yet, so nothing typed just before Submit is lost.
/// </summary>
public sealed record SubmitAttemptRequest(IReadOnlyList<FinalResponse>? FinalResponses = null);

/// <summary>One unsent answer, in the shape of an autosave.</summary>
/// <param name="Response">The answer; null clears it.</param>
public sealed record FinalResponse(int Position, JsonElement? Response, int ClientSeq);
