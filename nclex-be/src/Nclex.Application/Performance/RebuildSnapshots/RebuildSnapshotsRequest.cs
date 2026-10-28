namespace Nclex.Application.Performance.RebuildSnapshots;

/// <summary>Body of POST /admin/performance-snapshots/rebuild.</summary>
/// <param name="UserId">The user whose snapshots to rebuild.</param>
public sealed record RebuildSnapshotsRequest(Guid? UserId);
