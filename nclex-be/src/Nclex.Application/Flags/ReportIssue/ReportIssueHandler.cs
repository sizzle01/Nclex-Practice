using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Flags.MyFlags;
using Nclex.Domain.Entities;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.ReportIssue;

/// <summary>
/// POST /question-versions/{id}/flags: a candidate reports a problem with a question they saw.
/// One report per user, version and reason; a second is 409 FLAG_DUPLICATE.
/// </summary>
public sealed class ReportIssueHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    /// <summary>The unique index behind the duplicate rule (see QuestionFlagConfiguration).</summary>
    public const string OneReportPerReasonIndex = "ix_question_flags_user_id_question_version_id_reason";

    // Candidates only ever see published versions, or ones superseded since they were served.
    private static readonly VersionStatus[] VisibleToCandidates = [VersionStatus.Published, VersionStatus.Superseded];

    public async Task<MyFlagView> HandleAsync(Guid questionVersionId, ReportIssueRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var reason = request.Reason!.Value;
        var stem = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.Id == questionVersionId && VisibleToCandidates.Contains(v.Status))
            .Select(v => v.Stem)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new AppException(FlagErrors.VersionNotFound);

        if (request.AttemptId is { } attemptId)
        {
            await EnsureAttemptServedVersionAsync(attemptId, questionVersionId, cancellationToken);
        }

        if (await db.QuestionFlags.AnyAsync(
                f => f.UserId == userId && f.QuestionVersionId == questionVersionId && f.Reason == reason,
                cancellationToken))
        {
            throw new AppException(FlagErrors.Duplicate);
        }

        var flag = QuestionFlag.Report(userId, questionVersionId, request.AttemptId, reason, request.Comment, clock.GetUtcNow());
        db.QuestionFlags.Add(flag);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == OneReportPerReasonIndex)
        {
            // Two clicks racing past the check above.
            throw new AppException(FlagErrors.Duplicate);
        }

        return MyFlagView.From(flag, stem);
    }

    private async Task EnsureAttemptServedVersionAsync(Guid attemptId, Guid questionVersionId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (!await db.Attempts.AnyAsync(a => a.Id == attemptId && a.UserId == userId, cancellationToken))
        {
            throw new AppException(FlagErrors.AttemptNotFound);
        }

        if (!await db.AttemptItems.AnyAsync(i => i.AttemptId == attemptId && i.QuestionVersionId == questionVersionId, cancellationToken))
        {
            throw AppException.ForField("attemptId", Common.Validation.ValidationErrorCodes.InvalidValue);
        }
    }
}
