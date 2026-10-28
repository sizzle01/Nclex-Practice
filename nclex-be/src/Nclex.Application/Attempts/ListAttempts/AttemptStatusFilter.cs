using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.ListAttempts;

/// <summary>Reads the <c>status</c> query parameter of GET /attempts, e.g. "in_progress,paused".</summary>
public static class AttemptStatusFilter
{
    private static readonly Dictionary<string, AttemptStatus> ByName = new(StringComparer.Ordinal)
    {
        ["in_progress"] = AttemptStatus.InProgress,
        ["paused"] = AttemptStatus.Paused,
        ["submitted"] = AttemptStatus.Submitted,
        ["abandoned"] = AttemptStatus.Abandoned,
    };

    /// <summary>The drafts the dashboard lists (architecture.md 5.3).</summary>
    public static readonly IReadOnlyList<AttemptStatus> Drafts = [AttemptStatus.InProgress, AttemptStatus.Paused];

    /// <summary>False when any part is not a status name.</summary>
    public static bool TryParse(string? value, out IReadOnlyList<AttemptStatus> statuses)
    {
        statuses = Drafts;
        if (value is null)
        {
            return true;
        }

        var parsed = new List<AttemptStatus>();
        foreach (var name in value.Split(',', StringSplitOptions.TrimEntries))
        {
            if (!ByName.TryGetValue(name, out var status))
            {
                return false;
            }

            parsed.Add(status);
        }

        statuses = parsed.Distinct().ToList();
        return true;
    }
}
