using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Highlight;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>Validation for highlight text and highlight table, which share the selection rules.</summary>
public sealed class HighlightModuleTests
{
    private readonly IItemTypeModule _text = new HighlightTextModule();
    private readonly IItemTypeModule _table = new HighlightTableModule();

    [Fact]
    public void A_complete_text_item_has_no_authoring_issues()
    {
        _text.ValidateAuthoring(Passage(), Correct("s2"), SelectableRationale("s2", "s4")).ShouldBeEmpty();
    }

    [Fact]
    public void Only_selectable_segments_need_a_rationale()
    {
        var issues = _text.ValidateAuthoring(Passage(), Correct("s2"), SelectableRationale("s1", "s2", "s4"));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.s1"),
            i => i.Code.ShouldBe(ValidationIssueCodes.UnknownId));
    }

    [Fact]
    public void The_key_cannot_mark_a_segment_that_candidates_cannot_select()
    {
        var issues = _text.ValidateAuthoring(Passage(), Correct("s1", "s9"), SelectableRationale("s2", "s4"));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("key.correct[0]", ValidationIssueCodes.NotSelectable), ("key.correct[1]", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void The_key_needs_at_least_one_correct_segment()
    {
        _text.ValidateAuthoring(Passage(), Correct(), SelectableRationale("s2", "s4"))
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.TooFew);
    }

    [Fact]
    public void A_response_with_unknown_unselectable_or_repeated_segments_is_rejected()
    {
        var issues = _text.ValidateResponse(Passage(), Json(new { selected = new[] { "s2", "s2", "s3", "s9" } }));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("response.selected[1]", ValidationIssueCodes.DuplicateId),
            ("response.selected[2]", ValidationIssueCodes.NotSelectable),
            ("response.selected[3]", ValidationIssueCodes.UnknownId),
        ]);
    }

    [Fact]
    public void Grade_detail_covers_only_selectable_segments()
    {
        var result = _text.Grade(Passage(), Correct("s2"), Json(new { selected = new[] { "s4" } }));

        result.Detail.ShouldBe([new GradeDetail("s2", 0, Missed: true), new GradeDetail("s4", -1, Selected: "s4")]);
        result.Points.ShouldBe(0);
    }

    [Fact]
    public void A_complete_table_item_has_no_authoring_issues()
    {
        _table.ValidateAuthoring(Rows(2), Correct("r1"), SelectableRationale("r0", "r1")).ShouldBeEmpty();
    }

    [Fact]
    public void Each_table_row_needs_one_cell_per_column()
    {
        var content = Json(new
        {
            columns = new[] { "Time", "Finding" },
            rows = new[] { new { id = "r0", cells = new[] { "08:00" }, selectable = true }, new { id = "r1", cells = new[] { "09:00", "Fever" }, selectable = true } },
        });

        _table.ValidateAuthoring(content, Correct("r1"), SelectableRationale("r0", "r1"))
            .ShouldHaveSingleItem().Path.ShouldBe("content.rows[0].cells");
    }

    [Fact]
    public void A_table_response_cannot_select_an_unselectable_row()
    {
        var content = Json(new
        {
            columns = new[] { "Finding" },
            rows = new[] { new { id = "r0", cells = new[] { "Fever" }, selectable = true }, new { id = "r1", cells = new[] { "Note" }, selectable = false } },
        });

        _table.ValidateResponse(content, Json(new { selected = new[] { "r1" } }))
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.NotSelectable);
    }

    // Segments s1 and s3 are plain text; s2 and s4 can be highlighted.
    private static JsonElement Passage() =>
        Json(new
        {
            segments = new[]
            {
                new { id = "s1", text = "The client has ", selectable = false },
                new { id = "s2", text = "a fever", selectable = true },
                new { id = "s3", text = " and ", selectable = false },
                new { id = "s4", text = "a cough", selectable = true },
            },
        });

    private static JsonElement Rows(int count) =>
        Json(new
        {
            columns = new[] { "Finding" },
            rows = Enumerable.Range(0, count).Select(i => new { id = $"r{i}", cells = new[] { $"Finding {i}" }, selectable = true }),
        });

    private static JsonElement Correct(params string[] correct) => Json(new { correct });

    private static Rationale SelectableRationale(params string[] ids) =>
        new("Why the answer is right.", ids.ToDictionary(id => id, id => $"Why {id}."));
}
