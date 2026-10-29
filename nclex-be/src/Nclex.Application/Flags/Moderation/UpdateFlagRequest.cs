using System.Text.Json.Serialization;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

/// <summary>
/// Body of PATCH /admin/flags/{id} (api.md section 8). Fields left out stay as they are.
/// </summary>
/// <param name="Status">triaged, resolved or rejected. A report never goes back to open.</param>
/// <param name="ResolutionNote">Required to resolve or reject. On resolve it is sent to the reporter.</param>
/// <param name="AssigneeId">A reviewer or admin to hand the report to.</param>
public sealed record UpdateFlagRequest(FlagStatus? Status = null, string? ResolutionNote = null, Guid? AssigneeId = null)
{
    /// <summary>Longest note a moderator may write; the same limit as a reporter's comment.</summary>
    public const int MaxNoteLength = QuestionFlag.MaxCommentLength;

    // Derived, so not part of the request body or the OpenAPI schema.
    [JsonIgnore]
    public bool Closes => Status is FlagStatus.Resolved or FlagStatus.Rejected;
}
