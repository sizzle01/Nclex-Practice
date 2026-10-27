using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Dropdown;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>Validation for drop-down cloze and drop-down table, which share the blank rules.</summary>
public sealed class DropdownModuleTests
{
    private readonly IItemTypeModule _cloze = new DropdownClozeModule();
    private readonly IItemTypeModule _table = new DropdownTableModule();

    [Fact]
    public void A_complete_cloze_item_has_no_authoring_issues()
    {
        _cloze.ValidateAuthoring(Cloze("First {{b1}}, then {{b2}}."), Key(), BlankRationale()).ShouldBeEmpty();
    }

    [Fact]
    public void Rationales_are_keyed_blank_colon_option()
    {
        var issues = _cloze.ValidateAuthoring(Cloze("First {{b1}}, then {{b2}}."), Key(), BlankRationale(except: "b2:a"));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.b2:a"),
            i => i.Code.ShouldBe(ValidationIssueCodes.MissingRationale));
    }

    [Fact]
    public void The_template_must_show_every_blank_exactly_once_and_no_others()
    {
        var issues = _cloze.ValidateAuthoring(Cloze("{{b1}} and {{b1}} and {{b9}}."), Key(), BlankRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("content.template", ValidationIssueCodes.DuplicateId),
            ("content.template", ValidationIssueCodes.UnknownId),
            ("content.blanks[1]", ValidationIssueCodes.Mismatch),
        ]);
    }

    [Fact]
    public void The_key_needs_an_option_of_that_blank_for_every_blank()
    {
        var key = Json(new { byBlank = new { b1 = "z" } });

        _cloze.ValidateAuthoring(Cloze("{{b1}} {{b2}}"), key, BlankRationale()).Select(i => (i.Path, i.Code)).ShouldBe(
            [("key.byBlank.b2", ValidationIssueCodes.Required), ("key.byBlank.b1", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void Blank_ids_cannot_contain_the_rationale_separator()
    {
        var options = new[] { new { id = "a", text = "A" }, new { id = "b", text = "B" } };
        var content = Json(new { template = "{{b:1}}", blanks = new[] { new { id = "b:1", options } } });
        var key = Json(new { byBlank = new Dictionary<string, string> { ["b:1"] = "a" } });
        var rationale = new Rationale("Why.", new Dictionary<string, string> { ["b:1:a"] = "A", ["b:1:b"] = "B" });

        _cloze.ValidateAuthoring(content, key, rationale).ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("content.blanks[0].id"),
            i => i.Code.ShouldBe(ValidationIssueCodes.InvalidValue));
    }

    [Fact]
    public void A_response_may_leave_blanks_unanswered_but_not_name_unknown_blanks_or_options()
    {
        var response = Json(new { byBlank = new Dictionary<string, string?> { ["b1"] = null, ["b2"] = "z", ["b9"] = "a" } });

        _cloze.ValidateResponse(Cloze("{{b1}} {{b2}}"), response).Select(i => (i.Path, i.Code)).ShouldBe(
            [("response.byBlank.b2", ValidationIssueCodes.UnknownId), ("response.byBlank.b9", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void A_complete_table_item_has_no_authoring_issues()
    {
        _table.ValidateAuthoring(Table(new { text = "BP 82/50" }, new { blankId = "b1" }, new { text = "HR 128" }, new { blankId = "b2" }), Key(), BlankRationale())
            .ShouldBeEmpty();
    }

    [Fact]
    public void A_table_cell_holds_text_or_a_blank_and_each_row_has_one_cell_per_column()
    {
        var content = Json(new
        {
            columns = new[] { "Finding", "Indicates" },
            rows = new object[]
            {
                new { id = "r1", cells = new object[] { new { text = "BP", blankId = "b1" }, new { blankId = "b2" } } },
                new { id = "r2", cells = new object[] { new { text = "HR" } } },
            },
            blanks = Blanks(),
        });

        var issues = _table.ValidateAuthoring(content, Key(), BlankRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("content.rows[0].cells[0]", ValidationIssueCodes.InvalidValue),
            ("content.rows[1].cells", ValidationIssueCodes.Mismatch),
        ]);
    }

    private static JsonElement Cloze(string template) => Json(new { template, blanks = Blanks() });

    // Two rows of two columns: a text cell, then a blank cell.
    private static JsonElement Table(params object[] cells) =>
        Json(new
        {
            columns = new[] { "Finding", "Indicates" },
            rows = new[] { new { id = "r1", cells = cells[..2] }, new { id = "r2", cells = cells[2..] } },
            blanks = Blanks(),
        });

    private static object[] Blanks() =>
    [
        new { id = "b1", options = new[] { new { id = "a", text = "A" }, new { id = "b", text = "B" } } },
        new { id = "b2", options = new[] { new { id = "a", text = "A" }, new { id = "b", text = "B" }, new { id = "c", text = "C" } } },
    ];

    private static JsonElement Key() => Json(new { byBlank = new { b1 = "a", b2 = "c" } });

    private static Rationale BlankRationale(string? except = null) =>
        new("Why the answer is right.", new[] { "b1:a", "b1:b", "b2:a", "b2:b", "b2:c" }
            .Where(id => id != except)
            .ToDictionary(id => id, id => $"Why {id}."));
}
