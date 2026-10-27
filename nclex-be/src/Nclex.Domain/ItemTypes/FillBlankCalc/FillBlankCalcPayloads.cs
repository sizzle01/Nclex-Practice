namespace Nclex.Domain.ItemTypes.FillBlankCalc;

/// <summary>What candidates see beside the input box (interfaces.md 2.8).</summary>
/// <param name="Unit">Shown after the box, e.g. "mL/hr".</param>
/// <param name="Decimals">How many decimal places the answer should have; also used by <c>round</c> scoring.</param>
/// <param name="InputLabel">An accessible label for the box, e.g. "Infusion rate".</param>
public sealed record FillBlankCalcContent(string Unit, int Decimals, string? InputLabel = null);

/// <summary>The correct value and how close an answer must be.</summary>
/// <param name="Value">The exact correct answer.</param>
/// <param name="Tolerance">When set, any answer within this distance of the value is correct, and <paramref name="Rounding"/> is not used.</param>
/// <param name="Rounding"><c>exact</c>: the answer must equal the value. <c>round</c>: both are rounded to the content's decimals first.</param>
public sealed record FillBlankCalcKey(decimal Value, CalcRounding Rounding, decimal? Tolerance = null);

/// <summary>What the candidate typed, unparsed; the server parses it so every client is scored the same way.</summary>
public sealed record FillBlankCalcResponse(string Value);

/// <summary>How a calculation answer is compared with the key when there is no tolerance.</summary>
public enum CalcRounding
{
    Exact,
    Round,
}
