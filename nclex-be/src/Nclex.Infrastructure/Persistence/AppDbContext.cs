using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Email;
using Nclex.Domain.Entities;
using Npgsql;

namespace Nclex.Infrastructure.Persistence;

/// <summary>The EF Core context for the PostgreSQL system of record.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();

    public DbSet<EmailToken> EmailTokens => Set<EmailToken>();

    public DbSet<EmailOutboxMessage> EmailOutbox => Set<EmailOutboxMessage>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<ClientNeedsCategory> ClientNeedsCategories => Set<ClientNeedsCategory>();

    public DbSet<BodySystem> BodySystems => Set<BodySystem>();

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<CaseStudy> CaseStudies => Set<CaseStudy>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuestionVersion> QuestionVersions => Set<QuestionVersion>();

    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();

    public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();

    public DbSet<TestSet> TestSets => Set<TestSet>();

    public DbSet<TestSetItem> TestSetItems => Set<TestSetItem>();

    public DbSet<UserQuestionHistory> UserQuestionHistory => Set<UserQuestionHistory>();

    public IQueryable<T> SqlQuery<T>(FormattableString sql) => Database.SqlQuery<T>(sql);

    public DbSet<Attempt> Attempts => Set<Attempt>();

    public DbSet<AttemptItem> AttemptItems => Set<AttemptItem>();

    public async Task<Attempt?> LockAttemptAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        // ToListAsync rather than SingleOrDefaultAsync: EF would wrap the SQL in a subquery to add a LIMIT.
        var rows = await Attempts
            .FromSql($"SELECT * FROM attempts WHERE id = {attemptId} FOR UPDATE")
            .ToListAsync(cancellationToken);
        return rows.SingleOrDefault();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new UniqueConstraintViolationException(pg.ConstraintName, ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // database.md section 2.1: pgcrypto provides gen_random_uuid() for primary keys,
        // citext gives case-insensitive email columns.
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
