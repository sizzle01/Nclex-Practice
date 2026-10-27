using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.FillBlankCalc;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class FillBlankCalcModuleTests
{
    private static readonly Rationale NoOptions = new("Rate = volume / time.", new Dictionary<string, string>());

    private readonly IItemTypeModule _module = new FillBlankCalcModule();

    [Theory]
    [InlineData("12", 12)]
    [InlineData("12.5", 12.5)]
    [InlineData(" 12.5 ", 12.5)]
    [InlineData("-0.5", -0.5)]
    [InlineData(".5", 0.5)]
    [InlineData("0012.50", 12.5)]
    public void The_parser_reads_plain_numbers(string input, double expected)
    {
        CalcInputParser.TryParse(input, out var value).ShouldBeTrue();
        value.ShouldBe((decimal)expected);
    }

    [Theory]
    [InlineData("12,5")]
    [InlineData("1,000")]
    [InlineData("1e3")]
    [InlineData("12 mL")]
    [InlineData("abc")]
    [InlineData("--1")]
    [InlineData("½")]
    [InlineData("")]
    [InlineData(null)]
    public void The_parser_rejects_anything_else(string? input)
    {
        CalcInputParser.TryParse(input, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("12,5")]
    [InlineData("12 mL")]
    [InlineData("twelve")]
    public void A_non_numeric_response_is_rejected(string input)
    {
        _module.ValidateResponse(Content(), Json(new { value = input })).ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("response.value"),
            i => i.Code.ShouldBe(ValidationIssueCodes.NotANumber));
    }

    [Theory]
    [InlineData("41.7")]
    [InlineData("")]
    [InlineData("  ")]
    public void A_number_or_an_empty_box_is_a_valid_response(string input)
    {
        _module.ValidateResponse(Content(), Json(new { value = input })).ShouldBeEmpty();
    }

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Content(), Key(), NoOptions).ShouldBeEmpty();
    }

    [Fact]
    public void Unit_decimals_and_tolerance_are_checked()
    {
        var content = Json(new { unit = " ", decimals = 9 });
        var key = Json(new { value = 2.5, tolerance = -0.1, rounding = "exact" });

        _module.ValidateAuthoring(content, key, NoOptions).Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("content.unit", ValidationIssueCodes.Required),
            ("content.decimals", ValidationIssueCodes.InvalidValue),
            ("key.tolerance", ValidationIssueCodes.InvalidValue),
        ]);
    }

    [Fact]
    public void A_calculation_has_no_per_option_rationales()
    {
        var rationale = new Rationale("Why.", new Dictionary<string, string> { ["a"] = "Nothing to select." });

        _module.ValidateAuthoring(Content(), Key(), rationale).ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.UnknownId);
    }

    [Fact]
    public void The_key_needs_a_rounding_rule()
    {
        _module.ValidateAuthoring(Content(), Json(new { value = 41.666666 }), NoOptions)
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.InvalidShape);
    }

    [Fact]
    public void Grade_detail_shows_what_was_typed_and_the_expected_value()
    {
        var result = _module.Grade(Content(), Key(), Json(new { value = "41.6" }));

        result.Detail.ShouldHaveSingleItem().ShouldBe(
            new GradeDetail(FillBlankCalcModule.AnswerTarget, 0, Selected: "41.6", Expected: 41.666666m, Missed: true));
    }

    private static JsonElement Content() => Json(new { unit = "mL/hr", decimals = 1, inputLabel = "Infusion rate" });

    private static JsonElement Key() => Json(new { value = 41.666666m, rounding = "round" });
}
