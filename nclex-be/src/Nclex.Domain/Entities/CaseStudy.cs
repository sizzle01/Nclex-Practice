using System.Text.Json;

namespace Nclex.Domain.Entities;

/// <summary>
/// An NGN unfolding case: one patient scenario shared by (usually) six questions.
/// Maps to <c>case_studies</c> (database.md section 2.4). The content importer creates them (F11);
/// a question joins a case through its version's <see cref="QuestionVersion.CaseStudyId"/>.
/// </summary>
public sealed class CaseStudy
{
    /// <summary>The most items one case can have (the case_position CHECK, database.md 2.4).</summary>
    public const int MaxItems = 6;

    // For EF Core, which sets properties directly when loading rows.
    private CaseStudy()
    {
    }

    public Guid Id { get; private set; }

    public ExamTrack Track { get; private set; }

    public string Title { get; private set; } = string.Empty;

    /// <summary>The scenario tabs: nurses' notes, vitals, labs, orders, history (interfaces.md, CaseStudyScenario).</summary>
    public JsonDocument Scenario { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>A new case study. The caller chooses the id, so an import file can update it later.</summary>
    public static CaseStudy Create(Guid id, ExamTrack track, string title, JsonElement scenario, DateTimeOffset now) =>
        new()
        {
            Id = id,
            Track = track,
            Title = title,
            Scenario = JsonDocument.Parse(scenario.GetRawText()),
            CreatedAt = now,
        };

    /// <summary>
    /// Replaces the title and scenario; returns false when nothing changed. Unlike question versions,
    /// a case is edited in place: past attempts keep their answers, and the scenario only sets the scene.
    /// </summary>
    public bool Update(string title, JsonElement scenario)
    {
        // jsonb does not keep key order or spacing, so the scenario is compared by value.
        if (Title == title && JsonElement.DeepEquals(Scenario.RootElement, scenario))
        {
            return false;
        }

        Title = title;
        Scenario = JsonDocument.Parse(scenario.GetRawText());
        return true;
    }
}

/// <summary>Where a question sits in a case study.</summary>
/// <param name="CaseStudyId">The case study.</param>
/// <param name="Position">1 to <see cref="CaseStudy.MaxItems"/>; the order candidates meet the items in.</param>
public sealed record CasePlacement(Guid CaseStudyId, short Position);
