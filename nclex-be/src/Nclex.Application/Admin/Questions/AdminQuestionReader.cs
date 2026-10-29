using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// Loads a question with all its versions as <see cref="AdminQuestionView"/>. GET returns it, and so
/// does every workflow action, so the console can redraw from one response.
/// </summary>
public sealed class AdminQuestionReader(IAppDbContext db)
{
    public async Task<AdminQuestionView> ReadAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await db.Questions.AsNoTracking().SingleOrDefaultAsync(q => q.Id == questionId, cancellationToken)
            ?? throw new AppException(AdminErrors.QuestionNotFound);
        var versions = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.QuestionId == questionId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(cancellationToken);

        var peopleIds = versions.Select(v => v.AuthorId)
            .Concat(versions.Select(v => v.ReviewerId).OfType<Guid>())
            .Distinct()
            .ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => peopleIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.Email, cancellationToken);

        return new AdminQuestionView(
            question.Id,
            question.Track,
            question.Status,
            question.AccessTier,
            question.CurrentVersionId,
            question.CreatedAt,
            question.RetiredAt,
            [.. versions.Select(v => ToView(v, names))]);
    }

    private static AdminVersionView ToView(QuestionVersion v, Dictionary<Guid, string> names) => new(
        v.Id,
        v.Version,
        v.Status,
        v.ItemType,
        v.Stem,
        v.Content.RootElement.Clone(),
        v.AnswerKey.RootElement.Clone(),
        v.Rationale.RootElement.Clone(),
        v.EducationalObjective,
        v.Exhibits.RootElement.Clone(),
        v.ClientNeedsId,
        v.BodySystemId,
        v.CjmmSkill,
        v.Difficulty,
        v.TimeAllotmentSec,
        v.MaxPoints,
        v.CaseStudyId,
        v.CasePosition,
        v.AuthorId,
        names.GetValueOrDefault(v.AuthorId, "Unknown"),
        v.ReviewerId,
        v.ReviewerId is { } reviewer ? names.GetValueOrDefault(reviewer) : null,
        v.ReviewNotes,
        v.ReviewedAt,
        v.PublishedAt,
        v.CreatedAt);
}
