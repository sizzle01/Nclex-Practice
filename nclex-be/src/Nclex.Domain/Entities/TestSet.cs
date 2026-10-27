using System.Text.Json;

namespace Nclex.Domain.Entities;

/// <summary>
/// A fixed list of questions with the settings an attempt of it runs under. Maps to
/// <c>test_sets</c> (database.md section 2.5). Its items are <see cref="TestSetItem"/> rows.
/// </summary>
public sealed class TestSet
{
    /// <summary>A sample set is a short taster of 10 items (PRD 5.5).</summary>
    public const int SampleQuestionCount = 10;

    // For EF Core, which sets properties directly when loading rows.
    private TestSet()
    {
    }

    public Guid Id { get; private set; }

    public ExamTrack Track { get; private set; }

    public TestSetKind Kind { get; private set; }

    /// <summary>The user a custom set belongs to; null for every other kind.</summary>
    public Guid? OwnerUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TestMode Mode { get; private set; }

    public RevealMode RevealMode { get; private set; }

    public bool Timed { get; private set; }

    /// <summary>Seconds per item; null means no item timer.</summary>
    public int? PerQuestionSec { get; private set; }

    /// <summary>Seconds for the whole attempt; null means no exam timer.</summary>
    public int? TimeBudgetSec { get; private set; }

    /// <summary>Share of points for a "likely pass" estimate, e.g. 0.6500; null means no estimate.</summary>
    public decimal? PassThreshold { get; private set; }

    public PausePolicy PausePolicy { get; private set; } = PausePolicy.Unlimited;

    public bool AllowBackNav { get; private set; }

    /// <summary>Sample sets can be started by users who have not verified their email yet.</summary>
    public bool IsSample { get; private set; }

    /// <summary>Premium sets are locked for free accounts.</summary>
    public PlanTier AccessTier { get; private set; }

    /// <summary>Only published sets are listed to candidates.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>The custom builder's filters, kept for "retake similar" (F07).</summary>
    public JsonDocument? GeneratorCriteria { get; private set; }

    /// <summary>Stopping rules for CAT sets (phase 2).</summary>
    public JsonDocument? CatConfig { get; private set; }

    /// <summary>How many items the set has; kept in step with its <see cref="TestSetItem"/> rows.</summary>
    public int QuestionCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a published curated set. The caller chooses the id, so content files can refer to
    /// the same set every time they are imported.
    /// </summary>
    public static TestSet CreatePrebuilt(Guid id, ExamTrack track, TestSetSettings settings, int questionCount, DateTimeOffset now)
    {
        var set = new TestSet
        {
            Id = id,
            Track = track,
            Kind = TestSetKind.Prebuilt,
            IsPublished = true,
            CreatedAt = now,
        };
        set.Apply(settings, questionCount);
        return set;
    }

    /// <summary>
    /// Creates a set made by the custom builder for one user (architecture.md 5.7). It is never
    /// published: only its owner sees it. <paramref name="generatorCriteria"/> keeps the filters it
    /// was built from, for "retake similar".
    /// </summary>
    public static TestSet CreateCustom(
        Guid ownerUserId,
        ExamTrack track,
        TestSetSettings settings,
        int questionCount,
        JsonDocument generatorCriteria,
        DateTimeOffset now)
    {
        var set = new TestSet
        {
            Id = Guid.CreateVersion7(now),
            Track = track,
            Kind = TestSetKind.Custom,
            OwnerUserId = ownerUserId,
            IsPublished = false,
            GeneratorCriteria = generatorCriteria,
            CreatedAt = now,
        };
        set.Apply(settings, questionCount);
        return set;
    }

    /// <summary>The settings as a candidate sees them.</summary>
    public TestSetSettings Settings => new(
        Title,
        Description,
        Mode,
        RevealMode,
        Timed,
        PerQuestionSec,
        TimeBudgetSec,
        PassThreshold,
        PausePolicy,
        AllowBackNav,
        IsSample,
        AccessTier);

    /// <summary>Replaces the settings, for example when a content file changes them.</summary>
    public void Apply(TestSetSettings settings, int questionCount)
    {
        if (!settings.HasConsistentTiming)
        {
            throw new ArgumentException("A timed set needs a timer, and an untimed set must have none.", nameof(settings));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(questionCount);

        Title = settings.Title;
        Description = settings.Description;
        Mode = settings.Mode;
        RevealMode = settings.RevealMode;
        Timed = settings.Timed;
        PerQuestionSec = settings.PerQuestionSec;
        TimeBudgetSec = settings.TimeBudgetSec;
        PassThreshold = settings.PassThreshold;
        PausePolicy = settings.PausePolicy;
        AllowBackNav = settings.AllowBackNav;
        IsSample = settings.IsSample;
        AccessTier = settings.AccessTier;
        QuestionCount = questionCount;
    }
}
