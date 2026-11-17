namespace Nclex.Domain.Cat;

/// <summary>Why an adaptive exam ended.</summary>
public enum CatStopReason
{
    /// <summary>After the minimum, the 95% confidence interval was wholly above or below the passing standard.</summary>
    ClearResult,

    /// <summary>The maximum number of questions was reached.</summary>
    MaxItems,

    /// <summary>The exam time ran out (the run-out-of-time rule decides the result).</summary>
    TimeExpired,

    /// <summary>The bank had no more questions to give; decided like <see cref="MaxItems"/>.</summary>
    PoolExhausted,
}

/// <summary>
/// When the adaptive exam stops and whether it is a pass (architecture.md 5.8). The same rule
/// decides every ending: below the minimum it is a fail, otherwise the final ability estimate is
/// compared with the passing standard. When the exam stops on a clear result that comparison
/// agrees with the confidence interval, so one rule covers all of them.
/// </summary>
public static class StoppingRule
{
    /// <summary>
    /// Checked after each answer. Null means carry on. <paramref name="answered"/> counts every
    /// answered question, the one just answered included.
    /// </summary>
    public static CatStopReason? AfterAnswer(CatConfig config, AbilityEstimate estimate, int answered)
    {
        if (answered >= config.MaxItems)
        {
            return CatStopReason.MaxItems;
        }

        if (answered >= config.MinItems && IsClear(config, estimate))
        {
            return CatStopReason.ClearResult;
        }

        return null;
    }

    /// <summary>
    /// The result of an exam that has ended for any reason. Running out of time before the minimum
    /// is a fail, as on the real NCLEX; after it, the last estimate decides.
    /// </summary>
    public static bool Passed(CatConfig config, AbilityEstimate estimate, int answered) =>
        answered >= config.MinItems && estimate.Theta >= config.PassingTheta;

    /// <summary>True when the 95% confidence interval does not contain the passing standard.</summary>
    public static bool IsClear(CatConfig config, AbilityEstimate estimate) =>
        estimate.Lower95 > config.PassingTheta || estimate.Upper95 < config.PassingTheta;
}
