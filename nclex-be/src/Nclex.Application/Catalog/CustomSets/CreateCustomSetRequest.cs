using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Catalog.CustomSets;

/// <summary>Body of POST /test-sets/custom (api.md section 3): the filters, then how the set is taken.</summary>
/// <param name="QuestionCount">5 to 150.</param>
/// <param name="Mode">practice or challenge.</param>
/// <param name="RevealMode">tutor or exam.</param>
/// <param name="Timing">Timers; untimed when left out.</param>
/// <param name="StartNow">Start an attempt straight away. Accepted now; attempts start from F08.</param>
public sealed record CreateCustomSetRequest(
    ExamTrack? Track = null,
    IReadOnlyList<short>? ClientNeedsIds = null,
    IReadOnlyList<short>? BodySystemIds = null,
    IReadOnlyList<ItemType>? ItemTypes = null,
    IReadOnlyList<DifficultyTier>? Difficulties = null,
    PoolMode? Pool = null,
    int? QuestionCount = null,
    TestMode? Mode = null,
    RevealMode? RevealMode = null,
    CustomSetTiming? Timing = null,
    bool StartNow = false) : IPoolFilters
{
    public const int MinQuestions = 5;
    public const int MaxQuestions = 150;
}

/// <param name="Timed">Whether a timer runs; a timed set needs at least one of the two below.</param>
/// <param name="PerQuestionSec">Seconds per item.</param>
/// <param name="TimeBudgetSec">Seconds for the whole attempt.</param>
public sealed record CustomSetTiming(bool Timed = false, int? PerQuestionSec = null, int? TimeBudgetSec = null);

/// <summary>Body of a 201 from POST /test-sets/custom.</summary>
/// <param name="TestSet">The new set; only its owner can see it.</param>
/// <param name="Attempt">The started attempt when startNow is true. Always null until attempts exist (F08).</param>
public sealed record CreateCustomSetResponse(TestSetDetail TestSet, object? Attempt);
