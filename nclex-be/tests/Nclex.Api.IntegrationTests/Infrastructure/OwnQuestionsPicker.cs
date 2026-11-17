using System.Collections.Concurrent;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Adaptive;
using Nclex.Domain.Cat;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// The adaptive picker, limited to questions a test imported itself. The real pool is every
/// published question on the track, which in the shared test database includes other tests'
/// questions of every item type, so a test could not know how to answer what it is served.
/// </summary>
public sealed class OwnQuestionsPicker(IAppDbContext db, ConcurrentDictionary<Guid, byte> allowed)
    : AdaptivePicker(db, new Random(19))
{
    protected override async Task<List<CatCandidate>> PoolAsync(
        ExamTrack track,
        PlanTier plan,
        IReadOnlySet<Guid> given,
        CancellationToken cancellationToken) =>
        [.. (await base.PoolAsync(track, plan, given, cancellationToken)).Where(c => allowed.ContainsKey(c.QuestionId))];
}
