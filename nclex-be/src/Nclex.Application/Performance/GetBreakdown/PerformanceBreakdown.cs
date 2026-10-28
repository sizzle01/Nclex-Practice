using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetBreakdown;

/// <summary>Body of GET /me/performance/breakdown: one row per category, by name.</summary>
public sealed record PerformanceBreakdown(ExamTrack Track, PerformanceDimension Dimension, IReadOnlyList<BreakdownRow> Items);
