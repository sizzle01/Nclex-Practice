using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

/// <summary>Turns reports into queue rows, looking up their question and people in two queries.</summary>
public sealed class FlagQueueReader(IAppDbContext db)
{
    public async Task<IReadOnlyList<FlagQueueItem>> ReadAsync(IReadOnlyList<QuestionFlag> flags, CancellationToken cancellationToken)
    {
        var versionIds = flags.Select(f => f.QuestionVersionId).Distinct().ToList();
        var versions = await db.QuestionVersions.AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .Select(v => new { v.Id, v.QuestionId, v.Version, v.ItemType, v.Stem })
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var userIds = flags
            .SelectMany(f => new[] { f.UserId, f.AssigneeId, f.ResolvedBy })
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var people = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new FlagPerson(u.Id, u.DisplayName))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        return [.. flags.Select(flag =>
        {
            var version = versions[flag.QuestionVersionId];
            return new FlagQueueItem(
                flag.Id,
                flag.Status,
                flag.Reason,
                flag.Comment,
                new FlaggedQuestion(version.QuestionId, version.Id, version.Version, version.ItemType, StemPreview.From(version.Stem)),
                people[flag.UserId],
                flag.AssigneeId is { } assignee ? people[assignee] : null,
                flag.ResolutionNote,
                flag.ResolvedBy is { } resolver ? people[resolver] : null,
                flag.ResolvedAt,
                flag.AttemptId,
                flag.CreatedAt);
        })];
    }
}
