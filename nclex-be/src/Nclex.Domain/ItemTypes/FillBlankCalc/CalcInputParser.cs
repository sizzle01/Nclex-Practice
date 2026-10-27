using System.Globalization;

namespace Nclex.Domain.ItemTypes.FillBlankCalc;

/// <summary>
/// Reads what a candidate typed into a calculation box. Only a plain number is accepted: an optional
/// sign, digits and a "." decimal point, with surrounding spaces ignored. Thousands separators,
/// decimal commas, exponents and units are rejected, so "1,5" can never be read as 15 or 1.5.
/// </summary>
public static class CalcInputParser
{
    private const NumberStyles PlainNumber =
        NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public static bool TryParse(string? input, out decimal value) =>
        decimal.TryParse(input, PlainNumber, CultureInfo.InvariantCulture, out value);

    /// <summary>True when the box is empty, which means "not answered yet" rather than a wrong answer.</summary>
    public static bool IsBlank(string? input) => string.IsNullOrWhiteSpace(input);
}
