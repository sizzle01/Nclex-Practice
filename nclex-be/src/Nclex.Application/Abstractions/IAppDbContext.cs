using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nclex.Domain.Audit;
using Nclex.Domain.Email;
using Nclex.Domain.Entities;

namespace Nclex.Application.Abstractions;

/// <summary>
/// The application's view of the database. Use cases depend on this interface rather than on
/// EF Core's concrete context, which lives in Nclex.Infrastructure (architecture.md section 4).
/// Entity sets are added here as features introduce tables.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshSession> RefreshSessions { get; }

    DbSet<EmailToken> EmailTokens { get; }

    /// <summary>Emails waiting to be sent. Use cases add rows through <see cref="IEmailOutbox"/>.</summary>
    DbSet<EmailOutboxMessage> EmailOutbox { get; }

    /// <summary>Security and admin events. Use cases add rows through <see cref="IAuditLog"/>.</summary>
    DbSet<AuditEvent> AuditEvents { get; }

    /// <summary>The Client Needs trees for RN and PN (seed data, read-only).</summary>
    DbSet<ClientNeedsCategory> ClientNeedsCategories { get; }

    /// <summary>Body-system tags (seed data, read-only).</summary>
    DbSet<BodySystem> BodySystems { get; }

    DbSet<MediaAsset> MediaAssets { get; }

    DbSet<CaseStudy> CaseStudies { get; }

    DbSet<Question> Questions { get; }

    DbSet<QuestionVersion> QuestionVersions { get; }

    DbSet<QuestionBank> QuestionBanks { get; }

    DbSet<QuestionBankItem> QuestionBankItems { get; }

    DbSet<TestSet> TestSets { get; }

    DbSet<TestSetItem> TestSetItems { get; }

    /// <summary>
    /// Attempts and their items. Every change runs inside a transaction that first locks the
    /// attempt row (database.md section 2.6 invariants).
    /// </summary>
    DbSet<Attempt> Attempts { get; }

    DbSet<AttemptItem> AttemptItems { get; }

    /// <summary>
    /// Reads an attempt with <c>SELECT … FOR UPDATE</c> and tracks it, or returns null when there is
    /// none. Call inside a transaction: the row stays locked until it ends.
    /// </summary>
    Task<Attempt?> LockAttemptAsync(Guid attemptId, CancellationToken cancellationToken);

    /// <summary>
    /// Starts a transaction for a use case that needs more than one SaveChanges to stay atomic,
    /// such as publishing a new question (see QuestionConfiguration for why).
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves tracked changes in one transaction.
    /// Throws <see cref="Common.UniqueConstraintViolationException"/> when a unique constraint is broken.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
