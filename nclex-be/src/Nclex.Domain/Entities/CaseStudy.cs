using System.Text.Json;

namespace Nclex.Domain.Entities;

/// <summary>
/// An NGN unfolding case: one patient scenario shared by (usually) six questions.
/// Maps to <c>case_studies</c> (database.md section 2.4). Case-study items arrive in a later feature.
/// </summary>
public sealed class CaseStudy
{
    // For EF Core, which sets properties directly when loading rows.
    private CaseStudy()
    {
    }

    public Guid Id { get; private set; }

    public ExamTrack Track { get; private set; }

    public string Title { get; private set; } = string.Empty;

    /// <summary>The scenario tabs: nurses' notes, vitals, labs, orders, history.</summary>
    public JsonDocument Scenario { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }
}
