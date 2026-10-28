using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Admin.Membership;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Banks;

/// <summary>
/// GET, POST and PATCH /admin/question-banks[/{id}] (api.md section 8, admin only): banks and their
/// membership. A bank has no order, so case studies need not sit together; the custom builder takes
/// whole cases from its pool anyway (QuestionPool).
/// </summary>
public sealed class AdminBanksHandler(
    IAppDbContext db,
    QuestionMembership membership,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminBankPage> ListAsync(ListAdminBanksRequest request, CancellationToken cancellationToken)
    {
        var (limit, offset) = AdminPaging.Read(request.Limit, request.Cursor);
        var banks = db.QuestionBanks.AsNoTracking();
        if (request.Track is { } track)
        {
            banks = banks.Where(b => b.Track == track);
        }

        var rows = await banks
            .OrderBy(b => b.Track).ThenBy(b => b.Name).ThenBy(b => b.Id)
            .Skip(offset)
            .Take(limit + 1)
            .Select(b => new AdminBankSummary(
                b.Id,
                b.Track,
                b.Name,
                b.AccessTier,
                b.IsPublished,
                db.QuestionBankItems.Count(i => i.BankId == b.Id),
                b.CreatedAt))
            .ToListAsync(cancellationToken);
        var (items, next) = AdminPaging.Page(rows, limit, offset);
        return new AdminBankPage(items, next);
    }

    public async Task<AdminBankView> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var bank = await db.QuestionBanks.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new AppException(AdminErrors.BankNotFound);
        var questionIds = await db.QuestionBankItems.AsNoTracking()
            .Where(i => i.BankId == id)
            .Select(i => i.QuestionId)
            .ToListAsync(cancellationToken);
        var questions = await membership.DescribeAsync(questionIds, cancellationToken);
        return new AdminBankView(bank.Id, bank.Track, bank.Name, bank.Description, bank.AccessTier, bank.IsPublished, bank.CreatedAt, [.. questions.OrderBy(q => q.Stem, StringComparer.Ordinal)]);
    }

    public async Task<AdminBankView> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken)
    {
        var questionIds = request.QuestionIds ?? [];
        await CheckQuestionsAsync(request.Track, questionIds, cancellationToken);
        var bank = QuestionBank.Create(request.Track, request.Name.Trim(), Clean(request.Description), request.AccessTier, request.IsPublished, clock.GetUtcNow());
        db.QuestionBanks.Add(bank);
        db.QuestionBankItems.AddRange(questionIds.Select(q => QuestionBankItem.Create(bank.Id, q)));
        audit.RecordAdminEvent(AuditActions.BankCreated, currentUser.UserId, AuditActions.BankEntity, bank.Id, new { questionCount = questionIds.Count, bank.IsPublished });
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(bank.Id, cancellationToken);
    }

    public async Task<AdminBankView> UpdateAsync(Guid id, UpdateBankRequest request, CancellationToken cancellationToken)
    {
        var bank = await db.QuestionBanks.SingleOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new AppException(AdminErrors.BankNotFound);
        if (request.QuestionIds is not null)
        {
            await CheckQuestionsAsync(bank.Track, request.QuestionIds, cancellationToken);
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        bank.ChangeDetails(
            request.Name?.Trim() ?? bank.Name,
            request.Description is null ? bank.Description : Clean(request.Description),
            request.AccessTier ?? bank.AccessTier);
        if (request.IsPublished is { } isPublished)
        {
            bank.SetPublished(isPublished);
        }

        if (request.QuestionIds is not null)
        {
            await db.QuestionBankItems.Where(i => i.BankId == id).ExecuteDeleteAsync(cancellationToken);
            db.QuestionBankItems.AddRange(request.QuestionIds.Select(q => QuestionBankItem.Create(id, q)));
        }

        audit.RecordAdminEvent(AuditActions.BankUpdated, currentUser.UserId, AuditActions.BankEntity, id, new
        {
            details = request.Name is not null || request.Description is not null || request.AccessTier is not null,
            questions = request.QuestionIds is not null,
            request.IsPublished,
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    private async Task CheckQuestionsAsync(ExamTrack track, IReadOnlyList<Guid> questionIds, CancellationToken cancellationToken)
    {
        var issues = new List<ValidationIssue>();
        await membership.CheckAsync(track, questionIds, keepCasesWhole: false, issues, cancellationToken);
        if (issues.Count > 0)
        {
            throw AuthoringValidator.Invalid(issues);
        }
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
