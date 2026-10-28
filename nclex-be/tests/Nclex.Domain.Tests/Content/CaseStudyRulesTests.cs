using System.Text.Json;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Content;

public sealed class CaseStudyRulesTests
{
    private static readonly Guid CaseA = Guid.NewGuid();
    private static readonly Guid CaseB = Guid.NewGuid();
    private static readonly Guid MediaId = Guid.NewGuid();

    [Fact]
    public void A_scenario_with_tabs_of_exhibits_passes_and_reports_its_media()
    {
        var check = CaseStudyRules.CheckScenario(Json($$"""
            { "tabs": [
              { "id": "notes", "title": "Nurses' notes", "exhibits": [{ "id": "n", "kind": "text", "title": "0800", "body": "Dyspneic." }] },
              { "id": "photo", "title": "Wound", "exhibits": [{ "id": "p", "kind": "image", "title": "Sacrum", "mediaId": "{{MediaId}}" }] }
            ] }
            """));

        check.Issues.ShouldBeEmpty();
        check.Media.ShouldBe([new MediaReference("scenario.tabs[1].exhibits[0].mediaId", MediaId, "image")]);
    }

    [Theory]
    [InlineData("""[]""", "scenario", ValidationIssueCodes.InvalidShape)]
    [InlineData("""{ "tabs": {} }""", "scenario", ValidationIssueCodes.InvalidShape)]
    [InlineData("""{ "tabs": [] }""", "scenario.tabs", ValidationIssueCodes.TooFew)]
    [InlineData("""{ "tabs": ["notes"] }""", "scenario.tabs[0]", ValidationIssueCodes.InvalidShape)]
    [InlineData("""{ "tabs": [{ "title": "Notes", "exhibits": [{ "id": "n", "kind": "text", "title": "N", "body": "B" }] }] }""", "scenario.tabs[0].id", ValidationIssueCodes.Required)]
    [InlineData("""{ "tabs": [{ "id": "a", "exhibits": [{ "id": "n", "kind": "text", "title": "N", "body": "B" }] }] }""", "scenario.tabs[0].title", ValidationIssueCodes.Required)]
    [InlineData("""{ "tabs": [{ "id": "a", "title": "A", "exhibits": [] }] }""", "scenario.tabs[0].exhibits", ValidationIssueCodes.TooFew)]
    [InlineData("""{ "tabs": [{ "id": "a", "title": "A", "exhibits": [{ "id": "n", "kind": "text", "title": "N" }] }] }""", "scenario.tabs[0].exhibits[0].body", ValidationIssueCodes.Required)]
    [InlineData("""{ "tabs": [{ "id": "a", "title": "A", "exhibits": [{ "id": "n", "kind": "text", "title": "N", "body": "B" }] }, { "id": "a", "title": "B", "exhibits": [{ "id": "n", "kind": "text", "title": "N", "body": "B" }] }] }""", "scenario.tabs[1].id", ValidationIssueCodes.DuplicateId)]
    public void A_malformed_scenario_is_reported_with_its_path(string scenario, string path, string code)
    {
        CaseStudyRules.CheckScenario(Json(scenario)).Issues.ShouldContain(i => i.Path == path && i.Code == code);
    }

    [Fact]
    public void Tab_ids_are_read_from_the_scenario()
    {
        CaseStudyRules.TabIds(Json("""{ "tabs": [{ "id": "notes" }, { "id": "labs" }, {}] }""")).ShouldBe(["notes", "labs"], ignoreOrder: true);
        CaseStudyRules.TabIds(Json("[]")).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(new short[] { }, true)]
    [InlineData(new short[] { 1 }, true)]
    [InlineData(new short[] { 3, 1, 2 }, true)]
    [InlineData(new short[] { 1, 2, 3, 4, 5, 6 }, true)]
    [InlineData(new short[] { 2, 3 }, false)]
    [InlineData(new short[] { 1, 1, 2 }, false)]
    [InlineData(new short[] { 1, 2, 4 }, false)]
    [InlineData(new short[] { 1, 2, 3, 4, 5, 6, 7 }, false)]
    public void Positions_must_run_from_one_with_no_gap_or_repeat(short[] positions, bool complete)
    {
        CaseStudyRules.PositionsAreComplete(positions).ShouldBe(complete);
    }

    [Fact]
    public void A_set_with_whole_cases_in_order_passes()
    {
        var (ids, placements) = Items((CaseA, 1), (CaseA, 2), (null, 0), (CaseB, 1), (CaseB, 2));

        CaseStudyRules.CheckSetOrder(ids, placements.GetValueOrDefault, _ => 2).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("missing an item", new[] { 1 }, 0)]
    [InlineData("out of order", new[] { 2, 1 }, 0)]
    [InlineData("standalone item between", new[] { 1, 0, 2 }, 0)]
    public void A_set_that_splits_a_case_is_reported_at_its_first_item(string problem, int[] positions, int index)
    {
        var (ids, placements) = Items([.. positions.Select(p => p == 0 ? ((Guid?)null, 0) : ((Guid?)CaseA, p))]);

        CaseStudyRules.CheckSetOrder(ids, placements.GetValueOrDefault, _ => 2)
            .ShouldHaveSingleItem(problem).Index.ShouldBe(index);
    }

    [Fact]
    public void A_case_is_edited_in_place_only_when_something_changed()
    {
        var caseStudy = CaseStudy.Create(CaseA, ExamTrack.RN, "Heart failure", Json("""{ "tabs": [] }"""), DateTimeOffset.UnixEpoch);

        caseStudy.Update("Heart failure", Json("""{ "tabs": [ ] }""")).ShouldBeFalse();
        caseStudy.Update("Heart failure", Json("""{ "tabs": [{ "id": "notes" }] }""")).ShouldBeTrue();
        caseStudy.Scenario.RootElement.GetProperty("tabs").GetArrayLength().ShouldBe(1);
    }

    private static (List<Guid> Ids, Dictionary<Guid, CasePlacement> Placements) Items(params (Guid? CaseId, int Position)[] items)
    {
        var ids = new List<Guid>();
        var placements = new Dictionary<Guid, CasePlacement>();
        foreach (var (caseId, position) in items)
        {
            var id = Guid.NewGuid();
            ids.Add(id);
            if (caseId is { } c)
            {
                placements[id] = new CasePlacement(c, (short)position);
            }
        }

        return (ids, placements);
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;
}
