namespace Nclex.Domain.ItemTypes.FillBlankCalc;

/// <summary>
/// Fill in the blank (calculation), e.g. a dosage or drip rate, scored 0 or 1 (interfaces.md 2.8).
/// A tolerance, when set, decides; otherwise the key's rounding rule does.
/// </summary>
public sealed class FillBlankCalcModule : ItemTypeModule<FillBlankCalcContent, FillBlankCalcKey, FillBlankCalcResponse>
{
    /// <summary>The <see cref="GradeDetail.Target"/> of the single answer box.</summary>
    public const string AnswerTarget = "value";

    private const int MaxDecimals = 6;

    public override ItemType Type => ItemType.FillBlankCalc;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(FillBlankCalcContent content, FillBlankCalcKey key) => 1;

    public override GradeResult Grade(FillBlankCalcContent content, FillBlankCalcKey key, FillBlankCalcResponse? response)
    {
        var typed = response?.Value;
        var correct = CalcInputParser.TryParse(typed, out var answer) && IsCorrect(answer, content, key);
        var detail = new GradeDetail(
            AnswerTarget,
            correct ? 1 : 0,
            Selected: CalcInputParser.IsBlank(typed) ? null : typed,
            Expected: key.Value,
            Missed: !correct);
        return GradeResult.From(correct ? 1 : 0, MaxPoints(content, key), [detail]);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(FillBlankCalcContent content, FillBlankCalcKey key, Rationale rationale)
    {
        var issues = new List<ValidationIssue>();
        if (string.IsNullOrWhiteSpace(content.Unit))
        {
            issues.Add(new ValidationIssue("content.unit", ValidationIssueCodes.Required, "Give the unit of the answer, e.g. \"mL/hr\"."));
        }

        if (content.Decimals is < 0 or > MaxDecimals)
        {
            issues.Add(new ValidationIssue("content.decimals", ValidationIssueCodes.InvalidValue, $"Decimals must be between 0 and {MaxDecimals}."));
        }

        if (key.Tolerance < 0)
        {
            issues.Add(new ValidationIssue("key.tolerance", ValidationIssueCodes.InvalidValue, "The tolerance can't be negative."));
        }

        // A calculation has nothing to select, so rationale.perOption must be empty.
        issues.AddRange(IdRules.CheckRationale(rationale, []));
        return issues;
    }

    // An empty box is allowed on autosave: the candidate may have cleared it. It scores 0.
    public override IReadOnlyList<ValidationIssue> ValidateResponse(FillBlankCalcContent content, FillBlankCalcResponse response) =>
        CalcInputParser.IsBlank(response.Value) || CalcInputParser.TryParse(response.Value, out _)
            ? []
            : [new ValidationIssue("response.value", ValidationIssueCodes.NotANumber, "Enter a number, using \".\" for decimals.")];

    private static bool IsCorrect(decimal answer, FillBlankCalcContent content, FillBlankCalcKey key)
    {
        if (key.Tolerance is { } tolerance)
        {
            return Math.Abs(answer - key.Value) <= tolerance;
        }

        // Half away from zero is the rounding taught for dosage calculations (0.25 -> 0.3), unlike
        // .NET's default banker's rounding (0.25 -> 0.2).
        return key.Rounding == CalcRounding.Round
            ? Math.Round(answer, content.Decimals, MidpointRounding.AwayFromZero) == Math.Round(key.Value, content.Decimals, MidpointRounding.AwayFromZero)
            : answer == key.Value;
    }
}
