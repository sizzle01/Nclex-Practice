using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Matrix;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>Validation for matrix multiple choice and matrix multiple response, which share the grid rules.</summary>
public sealed class MatrixModuleTests
{
    private static readonly string[] Rows = ["r1", "r2"];
    private static readonly string[] Columns = ["c1", "c2", "c3"];

    private readonly IItemTypeModule _mc = new MatrixMcModule();
    private readonly IItemTypeModule _mr = new MatrixMrModule();

    [Fact]
    public void A_complete_matrix_mc_item_has_no_authoring_issues()
    {
        _mc.ValidateAuthoring(Grid(), McKey(("r1", "c1"), ("r2", "c3")), CellRationale()).ShouldBeEmpty();
    }

    [Fact]
    public void Every_cell_needs_a_rationale_keyed_row_colon_column()
    {
        var rationale = CellRationale(except: "r2:c2");

        var issues = _mc.ValidateAuthoring(Grid(), McKey(("r1", "c1"), ("r2", "c3")), rationale);

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.r2:c2"),
            i => i.Code.ShouldBe(ValidationIssueCodes.MissingRationale));
    }

    [Fact]
    public void The_mc_key_needs_a_real_column_for_every_row()
    {
        var issues = _mc.ValidateAuthoring(Grid(), McKey(("r1", "c9"), ("r7", "c1")), CellRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("key.byRow.r2", ValidationIssueCodes.Required),
            ("key.byRow.r1", ValidationIssueCodes.UnknownId),
            ("key.byRow.r7", ValidationIssueCodes.UnknownId),
        ]);
    }

    [Fact]
    public void Grid_ids_must_be_unique_and_free_of_the_cell_separator_and_there_must_be_two_columns()
    {
        var content = Json(new
        {
            rows = new[] { new { id = "r1", text = "Row" }, new { id = "r1", text = "Row" }, new { id = "a:b", text = "Row" } },
            columns = new[] { new { id = "c1", text = "Column" } },
        });

        var issues = _mc.ValidateAuthoring(content, McKey(), new Rationale("Why.", new Dictionary<string, string>()));

        issues.Select(i => (i.Path, i.Code)).ShouldContain(("content.columns", ValidationIssueCodes.TooFew));
        issues.Select(i => (i.Path, i.Code)).ShouldContain(("content.rows[1].id", ValidationIssueCodes.DuplicateId));
        issues.Select(i => (i.Path, i.Code)).ShouldContain(("content.rows[2].id", ValidationIssueCodes.InvalidValue));
    }

    [Fact]
    public void An_mc_response_may_leave_rows_unanswered_but_not_name_unknown_rows_or_columns()
    {
        var response = Json(new { byRow = new Dictionary<string, string?> { ["r1"] = null, ["r2"] = "c9", ["r9"] = "c1" } });

        _mc.ValidateResponse(Grid(), response).Select(i => (i.Path, i.Code)).ShouldBe(
            [("response.byRow.r2", ValidationIssueCodes.UnknownId), ("response.byRow.r9", ValidationIssueCodes.UnknownId)]);
        _mc.ValidateResponse(Grid(), Json(new { byRow = new Dictionary<string, string>() })).ShouldBeEmpty();
    }

    [Fact]
    public void Mc_detail_shows_the_chosen_and_expected_column_per_row()
    {
        var result = _mc.Grade(Grid(), McKey(("r1", "c1"), ("r2", "c3")), Json(new { byRow = new { r1 = "c1", r2 = "c2" } }));

        result.Detail.ShouldBe(
        [
            new GradeDetail("r1", +1, Selected: "c1", Expected: "c1"),
            new GradeDetail("r2", 0, Selected: "c2", Expected: "c3", Missed: true),
        ]);
        result.MaxPoints.ShouldBe(2);
    }

    [Fact]
    public void A_complete_matrix_mr_item_has_no_authoring_issues()
    {
        _mr.ValidateAuthoring(Grid("row"), MrKey(("r1", ["r1:c1", "r1:c3"]), ("r2", ["r2:c2"])), CellRationale()).ShouldBeEmpty();
    }

    [Fact]
    public void Every_mr_group_needs_a_correct_cell_from_that_group()
    {
        var issues = _mr.ValidateAuthoring(Grid("row"), MrKey(("r1", ["r2:c1"])), CellRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("key.byGroup.r2", ValidationIssueCodes.TooFew),
            ("key.byGroup.r1[0]", ValidationIssueCodes.UnknownId),
        ]);
    }

    [Fact]
    public void Column_groups_hold_the_cells_down_a_column()
    {
        var key = MrKey(("c1", ["r1:c1"]), ("c2", ["r2:c2"]), ("c3", ["r1:c3", "r2:c3"]));

        _mr.ValidateAuthoring(Grid("column"), key, CellRationale()).ShouldBeEmpty();
        _mr.MaxPoints(Grid("column"), key).ShouldBe(4);
    }

    [Fact]
    public void An_mr_response_cannot_use_unknown_groups_or_cells_from_another_group()
    {
        var response = MrResponse(("r1", ["r1:c1", "r1:c1", "r2:c2"]), ("r9", ["r9:c1"]));

        _mr.ValidateResponse(Grid("row"), response).Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("response.byGroup.r1[1]", ValidationIssueCodes.DuplicateId),
            ("response.byGroup.r1[2]", ValidationIssueCodes.UnknownId),
            ("response.byGroup.r9", ValidationIssueCodes.UnknownId),
        ]);
    }

    private static JsonElement Grid(string? groupAxis = null)
    {
        var rows = Rows.Select(id => new { id, text = $"Row {id}" });
        var columns = Columns.Select(id => new { id, text = $"Column {id}" });
        return groupAxis is null ? Json(new { rows, columns }) : Json(new { rows, columns, groupAxis });
    }

    private static Rationale CellRationale(string? except = null) =>
        new("Why the answer is right.", (
            from row in Rows
            from column in Columns
            let cell = $"{row}:{column}"
            where cell != except
            select cell).ToDictionary(cell => cell, cell => $"Why {cell}."));

    private static JsonElement McKey(params (string Row, string Column)[] byRow) =>
        Json(new { byRow = byRow.ToDictionary(p => p.Row, p => p.Column) });

    private static JsonElement MrKey(params (string Group, string[] Cells)[] byGroup) =>
        Json(new { byGroup = byGroup.ToDictionary(p => p.Group, p => p.Cells) });

    private static JsonElement MrResponse(params (string Group, string[] Cells)[] byGroup) => MrKey(byGroup);
}
