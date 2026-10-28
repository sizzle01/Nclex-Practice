using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.ListAttempts;
using Nclex.Application.Attempts.SaveResponse;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Attempts;

public sealed class AttemptSessionTests(ApiFactory factory)
{
    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    // Lease takeover (architecture.md 5.2)

    [Fact]
    public async Task Writes_need_the_lease_which_opening_on_another_device_takes_over()
    {
        var (user, attempt) = await StartedAsync();
        var beforeOpen = await _attempts.SaveAsync(user, attempt.Id, 1, "laptop", Answer("a"), 1);
        await _attempts.OpenOkAsync(user, attempt.Id, "laptop");
        var laptopSave = await _attempts.SaveAsync(user, attempt.Id, 1, "laptop", Answer("a"), 1);

        await _attempts.OpenOkAsync(user, attempt.Id, "phone");
        var laptopAfterTakeover = await _attempts.SaveAsync(user, attempt.Id, 1, "laptop", Answer("c"), 2);
        var phoneSave = await _attempts.SaveAsync(user, attempt.Id, 1, "phone", Answer("d"), 2);

        await beforeOpen.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
        laptopSave.StatusCode.ShouldBe(HttpStatusCode.OK);
        await laptopAfterTakeover.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
        phoneSave.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SelectedAsync(user, attempt.Id, 1)).ShouldBe("d");
    }

    [Fact]
    public async Task Continue_here_takes_the_lease_back()
    {
        var (user, attempt) = await StartedAsync();
        await _attempts.OpenOkAsync(user, attempt.Id, "laptop");
        await _attempts.OpenOkAsync(user, attempt.Id, "phone");

        await _attempts.OpenOkAsync(user, attempt.Id, "laptop-reloaded");
        var save = await _attempts.SaveAsync(user, attempt.Id, 1, "laptop-reloaded", Answer("b"), 1);
        var phone = await _attempts.MarkAsync(user, attempt.Id, 1, "phone", new { flagged = true });

        save.StatusCode.ShouldBe(HttpStatusCode.OK);
        await phone.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
    }

    [Fact]
    public async Task Writes_without_the_client_header_are_a_validation_error()
    {
        var (user, attempt) = await StartedAsync();

        var open = await _attempts.OpenAsync(user, attempt.Id, clientId: null);
        var save = await _attempts.SaveAsync(user, attempt.Id, 1, clientId: null, Answer("a"), 1);

        (await open.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("X-Attempt-Client", "REQUIRED"));
        await save.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    // Autosave

    [Fact]
    public async Task A_saved_answer_is_restored_when_the_item_is_fetched_again()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var save = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 3);

        var result = (await save.Content.ReadFromJsonAsync<SaveResponseResult>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        (result.ClientSeq, result.State).ShouldBe((3, ItemState.Answered));
        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 1);
        item.GetProperty("state").GetString().ShouldBe("answered");
        item.GetProperty("clientSeq").GetInt32().ShouldBe(3);
        item.GetProperty("response").GetProperty("selected").GetString().ShouldBe("b");
        var state = await StateAsync(user, attempt.Id);
        state.Progress.Answered.ShouldBe(1);
    }

    [Fact]
    public async Task A_stale_client_seq_is_ignored_and_still_gets_200()
    {
        var (user, attempt) = await OpenedAsync("c-1");
        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 5);

        var stale = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("c"), 4);
        var same = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("d"), 5);

        stale.StatusCode.ShouldBe(HttpStatusCode.OK);
        same.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await stale.Content.ReadFromJsonAsync<SaveResponseResult>(AuthApi.Json, TestContext.Current.CancellationToken))!.ClientSeq.ShouldBe(5);
        (await SelectedAsync(user, attempt.Id, 1)).ShouldBe("b");
    }

    [Fact]
    public async Task A_null_response_clears_the_answer()
    {
        var (user, attempt) = await OpenedAsync("c-1");
        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", response: null, 2);

        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 1);
        item.GetProperty("state").GetString().ShouldBe("seen");
        item.GetProperty("response").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task A_response_that_does_not_fit_the_item_type_is_refused()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var unknownOption = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("z"), 1);
        var wrongShape = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", new { selected = new[] { "a" } }, 2);

        (await unknownOption.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(e => e.Field.StartsWith("response", StringComparison.Ordinal));
        await wrongShape.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Marks_are_saved_and_shown_with_the_item()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var mark = await _attempts.MarkAsync(user, attempt.Id, 2, "c-1", new { flagged = true, struckOptions = new[] { "c" } });

        mark.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 2);
        item.GetProperty("flagged").GetBoolean().ShouldBeTrue();
        item.GetProperty("struckOptions").EnumerateArray().Select(e => e.GetString()).ShouldBe(["c"]);
        (await StateAsync(user, attempt.Id)).Navigator.Single(n => n.Position == 2).Flagged.ShouldBeTrue();
    }

    // Navigation

    [Fact]
    public async Task Navigating_moves_the_current_position_and_marks_items_seen()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var response = await _attempts.NavigateAsync(user, attempt.Id, "c-1", 3);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var state = await StateAsync(user, attempt.Id);
        state.CurrentPosition.ShouldBe(3);
        state.Navigator.Select(n => n.State).ShouldBe([ItemState.Seen, ItemState.Unseen, ItemState.Seen]);
    }

    [Fact]
    public async Task Positions_outside_the_attempt_are_refused()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var tooFar = await _attempts.NavigateAsync(user, attempt.Id, "c-1", 4);
        var zero = await _attempts.NavigateAsync(user, attempt.Id, "c-1", 0);
        var item = await _attempts.GetItemAsync(user, attempt.Id, 4);

        (await tooFar.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("toPosition", "INVALID_VALUE"));
        await zero.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        await item.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task Without_back_navigation_going_back_is_refused_and_the_item_left_is_locked()
    {
        var (user, attempt) = await OpenedAsync("c-1", new { allowBackNav = false });
        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);
        await _attempts.NavigateAsync(user, attempt.Id, "c-1", 2);

        var back = await _attempts.NavigateAsync(user, attempt.Id, "c-1", 1);
        var saveLocked = await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("c"), 2);
        var markLocked = await _attempts.MarkAsync(user, attempt.Id, 1, "c-1", new { flagged = true });

        await back.ShouldBeProblemAsync(HttpStatusCode.Conflict, "BACK_NAVIGATION_DISABLED");
        await saveLocked.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ITEM_LOCKED");
        await markLocked.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ITEM_LOCKED");
        (await SelectedAsync(user, attempt.Id, 1)).ShouldBe("b");
        var state = await StateAsync(user, attempt.Id);
        state.Settings.AllowBackNav.ShouldBeFalse();
        state.Progress.ShouldBe(new AttemptProgress(Answered: 1, Locked: 1, Flagged: 0));
    }

    // Access

    [Fact]
    public async Task Another_users_attempt_is_not_found()
    {
        var (_, attempt) = await OpenedAsync("c-1");
        var stranger = (await _attempts.VerifiedUserAsync()).AccessToken;

        await (await _attempts.GetAsync(stranger, attempt.Id)).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await (await _attempts.GetItemAsync(stranger, attempt.Id, 1)).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await (await _attempts.OpenAsync(stranger, attempt.Id, "c-2")).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await (await _attempts.SaveAsync(stranger, attempt.Id, 1, "c-1", Answer("a"), 9)).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await (await _attempts.DiscardAsync(stranger, attempt.Id)).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task A_discarded_attempt_cannot_be_written_opened_or_shown()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var discard = await _attempts.DiscardAsync(user, attempt.Id);
        var again = await _attempts.DiscardAsync(user, attempt.Id);

        discard.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await (await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("a"), 1)).ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        await (await _attempts.OpenAsync(user, attempt.Id, "c-2")).ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        await (await _attempts.GetItemAsync(user, attempt.Id, 1)).ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        (await StateAsync(user, attempt.Id)).Status.ShouldBe(AttemptStatus.Abandoned);
    }

    // Answer-key secrecy (CLAUDE.md security rule)

    [Fact]
    public async Task The_candidate_item_never_contains_the_answer_key_or_rationale()
    {
        var (user, attempt) = await OpenedAsync("c-1");
        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 1);

        item.EnumerateObject().Select(p => p.Name).ShouldBe(
        [
            "position", "questionVersionId", "itemType", "stem", "content", "exhibits", "caseStudy",
            "state", "response", "clientSeq", "flagged", "struckOptions", "timeAllotmentSec", "reveal",
        ], ignoreOrder: true);
        // Exam and tutor mode alike: nothing is revealed before the item is locked or the attempt submitted.
        item.GetProperty("reveal").ValueKind.ShouldBe(JsonValueKind.Null);
        var raw = item.GetRawText();
        raw.ShouldNotContain("answerKey", Case.Insensitive);
        raw.ShouldNotContain("rationale", Case.Insensitive);
        raw.ShouldNotContain("correct", Case.Insensitive);
        // The texts CatalogContent writes into the key's rationale and the educational objective.
        raw.ShouldNotContain("Why b");
        raw.ShouldNotContain("Prioritise safety.");
        item.GetProperty("content").GetProperty("options").GetArrayLength().ShouldBe(4);
    }

    [Fact]
    public async Task Option_order_is_the_same_every_time_the_item_is_shown()
    {
        var (user, attempt) = await OpenedAsync("c-1");

        var first = await _attempts.GetItemOkAsync(user, attempt.Id, 1);
        var second = await _attempts.GetItemOkAsync(user, attempt.Id, 1);

        first.GetProperty("content").GetRawText().ShouldBe(second.GetProperty("content").GetRawText());
    }

    // Drafts list

    [Fact]
    public async Task Drafts_are_listed_with_progress_and_discarded_ones_are_not()
    {
        var (user, attempt) = await OpenedAsync("c-1");
        await _attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);
        var (otherSet, _) = await ImportSetAsync(2);
        var discarded = await _attempts.StartOkAsync(user, otherSet);
        await _attempts.DiscardAsync(user, discarded.Id);

        var drafts = await PageAsync(user, "");
        var explicitDrafts = await PageAsync(user, "?status=in_progress,paused");
        var submitted = await PageAsync(user, "?status=submitted");
        var abandoned = await PageAsync(user, "?status=abandoned");

        var draft = drafts.Items.ShouldHaveSingleItem();
        draft.ShouldSatisfyAllConditions(
            d => d.Id.ShouldBe(attempt.Id),
            d => d.TestSet.Title.ShouldBe(attempt.TestSet.Title),
            d => (d.AnsweredItems, d.TotalItems).ShouldBe((1, 3)),
            d => d.StaleDraft.ShouldBeFalse(),
            d => d.TimeRemainingMs.ShouldBeNull());
        explicitDrafts.Items.Select(d => d.Id).ShouldBe([attempt.Id]);
        submitted.Items.ShouldBeEmpty();
        abandoned.Items.Select(d => d.Id).ShouldBe([discarded.Id]);
    }

    [Fact]
    public async Task An_unknown_status_filter_is_refused()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;

        var response = await _attempts.ListAsync(user, "?status=in_progress,finished");

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("status", "INVALID_VALUE"));
    }

    private static object Answer(string optionId) => new { selected = optionId };

    private async Task<(string Token, AttemptState Attempt)> StartedAsync(object? settings = null)
    {
        var (setId, _) = await ImportSetAsync(3, settings);
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        return (user, await _attempts.StartOkAsync(user, setId));
    }

    private async Task<(string Token, AttemptState Attempt)> OpenedAsync(string clientId, object? settings = null)
    {
        var (user, attempt) = await StartedAsync(settings);
        await _attempts.OpenOkAsync(user, attempt.Id, clientId);
        return (user, attempt);
    }

    private async Task<(Guid SetId, List<Guid> Questions)> ImportSetAsync(int questionCount, object? settings = null)
    {
        var (questions, setId) = (CatalogContent.NewIds(questionCount), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, $"Session {setId:N}", questions, settings)]));
        return (setId, questions);
    }

    private async Task<string?> SelectedAsync(string token, Guid attemptId, int position) =>
        (await _attempts.GetItemOkAsync(token, attemptId, position)).GetProperty("response").GetProperty("selected").GetString();

    private async Task<AttemptState> StateAsync(string token, Guid attemptId)
    {
        var response = await _attempts.GetAsync(token, attemptId);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<AttemptPage> PageAsync(string token, string query)
    {
        var response = await _attempts.ListAsync(token, query);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptPage>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }
}
