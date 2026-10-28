using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Admin.Membership;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.TestSets;

/// <summary>
/// GET, POST and PATCH /admin/test-sets[/{id}] (api.md section 8, admin only): curated sets, with
/// unpublished ones included. Attempts resolve each question's current version when they start
/// (database.md 2.5), so a newly published question appears in new attempts straight away.
/// </summary>
public sealed class AdminTestSetsHandler(
    IAppDbContext db,
    QuestionMembership membership,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminTestSetPage> ListAsync(ListAdminTestSetsRequest request, CancellationToken cancellationToken)
    {
        var (limit, offset) = AdminPaging.Read(request.Limit, request.Cursor);
        var sets = db.TestSets.AsNoTracking().Where(s => s.Kind == TestSetKind.Prebuilt);
        if (request.Track is { } track)
        {
            sets = sets.Where(s => s.Track == track);
        }

        var rows = await sets
            .OrderBy(s => s.Track).ThenBy(s => s.Title).ThenBy(s => s.Id)
            .Skip(offset)
            .Take(limit + 1)
            .Select(s => new AdminTestSetSummary(s.Id, s.Track, s.Title, s.Mode, s.IsSample, s.AccessTier, s.IsPublished, s.QuestionCount, s.CreatedAt))
            .ToListAsync(cancellationToken);
        var (items, next) = AdminPaging.Page(rows, limit, offset);
        return new AdminTestSetPage(items, next);
    }

    public async Task<AdminTestSetView> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var set = await FindAsync(id, tracked: false, cancellationToken);
        var questionIds = await db.TestSetItems.AsNoTracking()
            .Where(i => i.TestSetId == id)
            .OrderBy(i => i.Position)
            .Select(i => i.QuestionId)
            .ToListAsync(cancellationToken);
        return new AdminTestSetView(set.Id, set.Track, set.Settings, set.IsPublished, set.QuestionCount, set.CreatedAt, await membership.DescribeAsync(questionIds, cancellationToken));
    }

    public async Task<AdminTestSetView> CreateAsync(CreateTestSetRequest request, CancellationToken cancellationToken)
    {
        await CheckQuestionsAsync(request.Track, request.Settings, request.QuestionIds, cancellationToken);
        var set = TestSet.CreateCurated(request.Track, request.Settings, request.QuestionIds.Count, request.IsPublished, clock.GetUtcNow());
        db.TestSets.Add(set);
        db.TestSetItems.AddRange(TestSetItem.ForQuestions(set.Id, request.QuestionIds));
        audit.RecordAdminEvent(AuditActions.TestSetCreated, currentUser.UserId, AuditActions.TestSetEntity, set.Id, new { questionCount = set.QuestionCount, set.IsPublished });
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(set.Id, cancellationToken);
    }

    public async Task<AdminTestSetView> UpdateAsync(Guid id, UpdateTestSetRequest request, CancellationToken cancellationToken)
    {
        var set = await FindAsync(id, tracked: true, cancellationToken);
        var settings = request.Settings ?? set.Settings;
        var questionIds = request.QuestionIds ?? await db.TestSetItems
            .Where(i => i.TestSetId == id).OrderBy(i => i.Position).Select(i => i.QuestionId).ToListAsync(cancellationToken);
        // Checked even when only settings change: making a set a sample changes the rules for its questions.
        if (request.QuestionIds is not null || request.Settings is not null)
        {
            await CheckQuestionsAsync(set.Track, settings, questionIds, cancellationToken);
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        set.Apply(settings, questionIds.Count);
        if (request.IsPublished is { } isPublished)
        {
            set.SetPublished(isPublished);
        }

        if (request.QuestionIds is not null)
        {
            // Deleted straight away, because the new rows reuse the same (set, position) keys.
            await db.TestSetItems.Where(i => i.TestSetId == id).ExecuteDeleteAsync(cancellationToken);
            db.TestSetItems.AddRange(TestSetItem.ForQuestions(id, questionIds));
        }

        audit.RecordAdminEvent(AuditActions.TestSetUpdated, currentUser.UserId, AuditActions.TestSetEntity, id, new
        {
            settings = request.Settings is not null,
            questions = request.QuestionIds is not null,
            request.IsPublished,
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    private async Task CheckQuestionsAsync(ExamTrack track, TestSetSettings settings, IReadOnlyList<Guid> questionIds, CancellationToken cancellationToken)
    {
        var issues = new List<ValidationIssue>();
        if (settings.IsSample && questionIds.Count != TestSet.SampleQuestionCount)
        {
            issues.Add(new ValidationIssue("questionIds", ValidationIssueCodes.InvalidValue, $"A sample set has exactly {TestSet.SampleQuestionCount} questions."));
        }

        await membership.CheckAsync(track, questionIds, keepCasesWhole: true, issues, cancellationToken);
        if (issues.Count > 0)
        {
            throw AuthoringValidator.Invalid(issues);
        }
    }

    // Custom sets belong to candidates and CAT sets are generated, so only curated sets are managed here.
    private async Task<TestSet> FindAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        var sets = tracked ? db.TestSets : db.TestSets.AsNoTracking();
        return await sets.SingleOrDefaultAsync(s => s.Id == id && s.Kind == TestSetKind.Prebuilt, cancellationToken)
            ?? throw new AppException(AdminErrors.TestSetNotFound);
    }
}
