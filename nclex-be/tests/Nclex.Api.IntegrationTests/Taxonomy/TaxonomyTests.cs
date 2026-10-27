using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Taxonomy;
using Nclex.Application.Taxonomy.GetTaxonomy;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Api.IntegrationTests.Taxonomy;

public sealed class TaxonomyTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Rn_taxonomy_returns_the_client_needs_tree_with_leaf_weights()
    {
        var taxonomy = await GetTaxonomyAsync("RN");

        taxonomy.ClientNeeds.Select(c => c.Code).ShouldBe(["SECE", "HPM", "PSI", "PHI"]);
        var sece = taxonomy.ClientNeeds[0];
        sece.Name.ShouldBe("Safe & Effective Care Environment");
        sece.WeightMin.ShouldBeNull();
        sece.Children!.ShouldBe(
        [
            new ClientNeedsNode(2, "SECE.MOC", "Management of Care", 15, 21, null),
            new ClientNeedsNode(3, "SECE.SIC", "Safety and Infection Control", 10, 16, null),
        ]);

        // A category with no subcategories is itself a leaf, so it has the weights.
        var psi = taxonomy.ClientNeeds[2];
        (psi.WeightMin, psi.WeightMax, psi.Children).ShouldBe((6m, 12m, (IReadOnlyList<ClientNeedsNode>?)null));

        taxonomy.ClientNeeds[3].Children!.Select(c => (c.Code, c.WeightMin, c.WeightMax)).ShouldBe(
            [("PHI.BCC", 6m, 12m), ("PHI.PPT", 13m, 19m), ("PHI.RRP", 9m, 15m), ("PHI.PA", 11m, 17m)]);
    }

    [Fact]
    public async Task Pn_taxonomy_uses_the_pn_test_plan()
    {
        var taxonomy = await GetTaxonomyAsync("PN");

        var leaves = taxonomy.ClientNeeds.SelectMany(c => c.Children ?? [c]).ToList();
        leaves.Select(c => c.Code).ShouldBe(["SECE.CC", "SECE.SIC", "HPM", "PSI", "PHI.BCC", "PHI.PT", "PHI.RRP", "PHI.PA"]);
        leaves[0].ShouldBe(new ClientNeedsNode(12, "SECE.CC", "Coordinated Care", 18, 24, null));
        leaves.Sum(c => c.WeightMin!.Value).ShouldBeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task Taxonomy_lists_body_systems_item_types_and_cjmm_skills()
    {
        var taxonomy = await GetTaxonomyAsync("RN");

        taxonomy.BodySystems.Count.ShouldBe(16);
        taxonomy.BodySystems[0].ShouldBe(new BodySystemInfo(1, "cardiovascular", "Cardiovascular"));

        taxonomy.ItemTypes.Count.ShouldBe(Enum.GetValues<ItemType>().Length);
        taxonomy.ItemTypes.Where(t => t.Available).Select(t => t.Type).ShouldBe([ItemType.MultipleChoice, ItemType.Sata]);
        taxonomy.ItemTypes.ShouldContain(new ItemTypeInfo(ItemType.Sata, "Select All That Apply", true));

        taxonomy.CjmmSkills.ShouldBe(CjmmSkillCatalog.All);
        taxonomy.CjmmSkills[0].ShouldBe(new CjmmSkillInfo(CjmmSkill.RecognizeCues, "Recognize cues", "What matters most?"));
    }

    [Fact]
    public async Task Taxonomy_json_matches_the_api_contract()
    {
        var response = await GetAsync("/api/v1/taxonomy?track=RN");

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var sece = json.GetProperty("clientNeeds")[0];
        sece.TryGetProperty("weightMin", out _).ShouldBeFalse();
        var moc = sece.GetProperty("children")[0];
        moc.GetProperty("weightMin").GetDecimal().ShouldBe(15m);
        moc.TryGetProperty("children", out _).ShouldBeFalse();
        json.GetProperty("itemTypes")[0].GetProperty("type").GetString().ShouldBe("multiple_choice");
        json.GetProperty("cjmmSkills")[0].GetProperty("key").GetString().ShouldBe("recognize_cues");
    }

    [Fact]
    public async Task Missing_track_returns_400()
    {
        var response = await GetAsync("/api/v1/taxonomy");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("track", "REQUIRED")]);
    }

    [Fact]
    public async Task Unknown_track_returns_400()
    {
        var response = await GetAsync("/api/v1/taxonomy?track=XN");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("track", "INVALID_VALUE")]);
    }

    [Fact]
    public async Task Taxonomy_requires_sign_in()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/taxonomy?track=RN", accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private async Task<TaxonomyResponse> GetTaxonomyAsync(string track)
    {
        var response = await GetAsync($"/api/v1/taxonomy?track={track}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<TaxonomyResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<HttpResponseMessage> GetAsync(string path)
    {
        var signedIn = await _api.SignupNewUserAsync();
        return await _api.SendWithTokenAsync(HttpMethod.Get, path, signedIn.AccessToken);
    }
}
