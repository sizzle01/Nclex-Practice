using System.Net;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Catalog.ListQuestionBanks;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Catalog;

public sealed class QuestionBanksTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task Banks_list_their_servable_question_count()
    {
        var (questions, bankId) = (CatalogContent.NewIds(3), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, banks: [CatalogContent.Bank(bankId, "Cardiac basics", questions)]));
        await _content.RetireQuestionAsync(questions[2]);
        var user = await _api.SignupNewUserAsync();

        var bank = (await ListAsync(user.AccessToken, "/api/v1/question-banks")).Single(b => b.Id == bankId);

        // The retired question can't be served any more, so it isn't counted.
        bank.ShouldBe(new QuestionBankSummary(bankId, "Cardiac basics", null, PlanTier.Free, QuestionCount: 2, Locked: false));
    }

    [Fact]
    public async Task Premium_banks_are_locked_for_free_users_and_open_for_premium_users()
    {
        var (questions, bankId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, banks: [CatalogContent.Bank(bankId, "Premium bank", questions, "premium")]));
        var free = await _api.SignupNewUserAsync();
        var premium = await _api.SignupNewUserAsync();
        await _content.MakePremiumAsync(premium.Body.User.Id);

        var forFree = (await ListAsync(free.AccessToken, "/api/v1/question-banks")).Single(b => b.Id == bankId);
        var forPremium = (await ListAsync(premium.AccessToken, "/api/v1/question-banks")).Single(b => b.Id == bankId);

        forFree.Locked.ShouldBeTrue();
        forPremium.Locked.ShouldBeFalse();
    }

    [Fact]
    public async Task Banks_of_the_other_track_are_only_listed_when_asked_for()
    {
        var (questions, bankId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("PN", questions, banks: [CatalogContent.Bank(bankId, "PN bank", questions)]));
        var rnUser = await _api.SignupNewUserAsync();

        var byDefault = await ListAsync(rnUser.AccessToken, "/api/v1/question-banks");
        var pn = await ListAsync(rnUser.AccessToken, "/api/v1/question-banks?track=PN");

        byDefault.ShouldNotContain(b => b.Id == bankId);
        pn.ShouldContain(b => b.Id == bankId);
    }

    [Fact]
    public async Task Question_banks_require_sign_in()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/question-banks", accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private async Task<IReadOnlyList<QuestionBankSummary>> ListAsync(string accessToken, string path)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, path, accessToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<QuestionBankList>(AuthApi.Json, TestContext.Current.CancellationToken))!.Items;
    }
}
