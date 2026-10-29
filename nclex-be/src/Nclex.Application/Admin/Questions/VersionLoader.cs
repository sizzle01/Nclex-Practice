using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// Loads a version and its question for a workflow action, and turns <see cref="VersionWorkflow"/>
/// refusals into API errors: 409 INVALID_STATUS_TRANSITION, or 403 FORBIDDEN for reviewing your
/// own version.
/// </summary>
public sealed class VersionLoader(IAppDbContext db)
{
    /// <summary>The version and its question, both tracked so the caller can change them.</summary>
    public async Task<(Question Question, QuestionVersion Version)> LoadAsync(Guid versionId, CancellationToken cancellationToken)
    {
        var version = await db.QuestionVersions.SingleOrDefaultAsync(v => v.Id == versionId, cancellationToken)
            ?? throw new AppException(AdminErrors.VersionNotFound);
        var question = await db.Questions.SingleAsync(q => q.Id == version.QuestionId, cancellationToken);
        return (question, version);
    }

    /// <summary>Throws the API error for an action the version's status or authorship rules out.</summary>
    public static void EnsureAllowed(VersionAction action, QuestionVersion version, Guid actorId)
    {
        switch (VersionWorkflow.Check(action, version.Status, version.AuthorId, actorId))
        {
            case WorkflowRefusal.WrongStatus:
                throw new AppException(AdminErrors.WrongStatus(
                    $"Version {version.Version} is {Describe(version.Status)}, so it can't be {Verb(action)}. Only a version that is {Describe(VersionWorkflow.From(action))} can."));
            case WorkflowRefusal.OwnVersion:
                throw new AppException(AdminErrors.OwnVersion);
        }
    }

    /// <summary>Retired questions take no new versions and publish nothing more.</summary>
    public static void EnsureActive(Question question)
    {
        if (question.IsRetired)
        {
            throw new AppException(AdminErrors.WrongStatus("This question is retired, so it can't change."));
        }
    }

    private static string Describe(VersionStatus status) => status switch
    {
        VersionStatus.InReview => "in review",
        _ => status.ToString().ToLowerInvariant(),
    };

    private static string Verb(VersionAction action) => action switch
    {
        VersionAction.Edit => "edited",
        VersionAction.SubmitForReview => "submitted for review",
        VersionAction.Approve => "approved",
        VersionAction.Reject => "rejected",
        _ => "published",
    };
}
