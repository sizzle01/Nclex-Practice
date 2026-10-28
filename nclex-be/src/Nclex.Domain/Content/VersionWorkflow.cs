using Nclex.Domain.Entities;

namespace Nclex.Domain.Content;

/// <summary>A step an author or reviewer takes on one question version (api.md section 8).</summary>
public enum VersionAction
{
    /// <summary>Change the draft's content or tags.</summary>
    Edit,

    /// <summary>draft → in_review.</summary>
    SubmitForReview,

    /// <summary>in_review → approved.</summary>
    Approve,

    /// <summary>in_review → rejected, with notes.</summary>
    Reject,

    /// <summary>approved → published; the previous published version becomes superseded.</summary>
    Publish,
}

/// <summary>Why <see cref="VersionWorkflow.Check"/> refused an action.</summary>
public enum WorkflowRefusal
{
    /// <summary>The version is not in the status the action starts from.</summary>
    WrongStatus,

    /// <summary>The actor wrote this version, and nobody reviews their own work.</summary>
    OwnVersion,
}

/// <summary>
/// The review workflow <c>draft → in_review → approved → published</c> (architecture.md section 7,
/// "Content accuracy"), as a pure rule table. Role checks happen at the API; this only knows about
/// statuses and who wrote the version.
/// </summary>
public static class VersionWorkflow
{
    /// <summary>The status a version must be in for <paramref name="action"/>.</summary>
    public static VersionStatus From(VersionAction action) => action switch
    {
        VersionAction.Edit or VersionAction.SubmitForReview => VersionStatus.Draft,
        VersionAction.Approve or VersionAction.Reject => VersionStatus.InReview,
        _ => VersionStatus.Approved,
    };

    /// <summary>The status a version has after <paramref name="action"/>.</summary>
    public static VersionStatus To(VersionAction action) => action switch
    {
        VersionAction.Edit => VersionStatus.Draft,
        VersionAction.SubmitForReview => VersionStatus.InReview,
        VersionAction.Approve => VersionStatus.Approved,
        VersionAction.Reject => VersionStatus.Rejected,
        _ => VersionStatus.Published,
    };

    /// <summary>
    /// Null when <paramref name="actorId"/> may take <paramref name="action"/> on a version in
    /// <paramref name="status"/> written by <paramref name="authorId"/>; otherwise why not.
    /// </summary>
    public static WorkflowRefusal? Check(VersionAction action, VersionStatus status, Guid authorId, Guid actorId)
    {
        if (status != From(action))
        {
            return WorkflowRefusal.WrongStatus;
        }

        // database.md 2.4: reviewer_id <> author_id. Rejecting is a review decision too.
        return IsReview(action) && authorId == actorId ? WorkflowRefusal.OwnVersion : null;
    }

    /// <summary>
    /// Draft, in review and approved versions are still on their way to candidates. A question has
    /// at most one of them at a time, so two people never rework the same question in parallel.
    /// </summary>
    public static bool IsOpen(VersionStatus status) =>
        status is VersionStatus.Draft or VersionStatus.InReview or VersionStatus.Approved;

    private static bool IsReview(VersionAction action) => action is VersionAction.Approve or VersionAction.Reject;
}
