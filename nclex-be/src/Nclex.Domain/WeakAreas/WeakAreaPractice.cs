namespace Nclex.Domain.WeakAreas;

/// <summary>
/// The size of a weak-area practice test (architecture.md 5.7.1): long enough to practise each
/// weak area, short enough to finish in one sitting.
/// </summary>
public static class WeakAreaPractice
{
    /// <summary>Questions in a test when the weak areas have enough.</summary>
    public const int TargetQuestions = 25;

    /// <summary>The fewest questions worth a test; the same minimum as the custom builder.</summary>
    public const int MinQuestions = 5;

    /// <summary>
    /// How many questions the test will have when <paramref name="available"/> questions qualify:
    /// the target, fewer when the areas run short, and 0 when too few for a test.
    /// </summary>
    public static int SizeFor(int available) =>
        available < MinQuestions ? 0 : Math.Min(available, TargetQuestions);
}
