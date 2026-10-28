using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Media.GetMedia;

namespace Nclex.Api.IntegrationTests.Media;

/// <summary>
/// GET /media/{id} and the signed file, captions and transcript links it returns (api.md section 10), and an
/// item's exhibits reaching the candidate.
/// </summary>
public sealed class MediaEndpointsTests(ApiFactory factory) : IDisposable
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AuthApi _api = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly MediaContent _media = new();

    [Fact]
    public async Task Media_details_need_a_signed_in_user()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/media/{Guid.NewGuid()}", accessToken: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unknown_media_is_not_found()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/media/{Guid.NewGuid()}", user.AccessToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task An_image_comes_with_its_alt_text_and_a_signed_link_that_works_without_a_token()
    {
        var view = await ImportAndGetAsync(_media.ImageId);

        view.ShouldSatisfyAllConditions(
            v => v.Kind.ShouldBe("image"),
            v => v.AltText.ShouldBe("A red area over the sacrum"),
            v => v.CaptionsUrl.ShouldBeNull(),
            v => v.TranscriptUrl.ShouldBeNull(),
            v => v.Url.ShouldStartWith($"/api/v1/media/{_media.ImageId}/file?exp="),
            v => v.ExpiresAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddMinutes(9), DateTimeOffset.UtcNow.AddMinutes(11)));

        var file = await GetAnonymousAsync(view.Url);

        file.StatusCode.ShouldBe(HttpStatusCode.OK);
        file.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        file.Headers.CacheControl!.Private.ShouldBeTrue();
        (await file.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBe(MediaContent.Png);
    }

    [Fact]
    public async Task An_audio_clip_has_captions_and_a_transcript_and_answers_range_requests()
    {
        var view = await ImportAndGetAsync(_media.AudioId);

        var captions = await GetAnonymousAsync(view.CaptionsUrl!);
        captions.Content.Headers.ContentType!.MediaType.ShouldBe("text/vtt");
        (await captions.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBe(MediaContent.Captions);

        view.TranscriptUrl.ShouldStartWith($"/api/v1/media/{_media.AudioId}/transcript?exp=");
        var transcript = await GetAnonymousAsync(view.TranscriptUrl!);
        transcript.Content.Headers.ContentType!.ToString().ShouldBe("text/plain; charset=utf-8");
        (await transcript.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBe(MediaContent.Transcript);

        var range = await GetAnonymousAsync(view.Url, new RangeHeaderValue(0, 9));
        range.StatusCode.ShouldBe(HttpStatusCode.PartialContent);
        (await range.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Length.ShouldBe(10);
    }

    [Fact]
    public async Task A_changed_link_is_refused()
    {
        var image = await ImportAndGetAsync(_media.ImageId);
        var clip = await GetAsync(_media.AudioId, (await _api.SignupNewUserAsync()).AccessToken);
        var query = image.Url[image.Url.IndexOf('?', StringComparison.Ordinal)..];

        string[] changed =
        [
            image.Url.Replace("sig=", "sig=x", StringComparison.Ordinal),     // tampered signature
            $"/api/v1/media/{_media.AudioId}/file{query}",                   // another asset's file
            $"/api/v1/media/{_media.ImageId}/captions{query}",               // the same asset's other part
            $"/api/v1/media/{_media.ImageId}/transcript{query}",             // and its third part
            clip.Url.Replace("exp=", "exp=1", StringComparison.Ordinal),     // a later expiry
            $"/api/v1/media/{_media.ImageId}/file",                          // no signature at all
        ];

        foreach (var url in changed)
        {
            await (await GetAnonymousAsync(url)).ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        }
    }

    [Fact]
    public async Task A_link_stops_working_when_it_expires()
    {
        await _content.ImportOkAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);
        await using var host = new TimedTestHost(factory);
        var user = await host.Auth.SignupNewUserAsync();
        var response = await host.Auth.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/media/{_media.ImageId}", user.AccessToken);
        var view = (await response.Content.ReadFromJsonAsync<MediaView>(AuthApi.Json, TestContext.Current.CancellationToken))!;

        (await host.Auth.SendWithTokenAsync(HttpMethod.Get, view.Url, accessToken: null)).StatusCode.ShouldBe(HttpStatusCode.OK);
        host.Clock.Advance(TimeSpan.FromMinutes(11));

        await (await host.Auth.SendWithTokenAsync(HttpMethod.Get, view.Url, accessToken: null))
            .ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task An_item_brings_its_exhibits_to_the_candidate()
    {
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(
            _media.File([_media.Image(), _media.Clip()], testSets: [CatalogContent.Set(setId, $"Exhibits {setId:N}", [_media.QuestionId])]),
            _media.Files);
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);

        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 1);

        var exhibits = item.GetProperty("exhibits").EnumerateArray().ToList();
        exhibits.Select(e => e.GetProperty("kind").GetString()).ShouldBe(["flowsheet", "image", "audio"]);
        exhibits[1].GetProperty("mediaId").GetGuid().ShouldBe(_media.ImageId);
        exhibits[0].GetProperty("table").GetProperty("rows")[0].GetProperty("cells")[1].GetString().ShouldBe("96");
    }

    public void Dispose() => _media.Dispose();

    private async Task<MediaView> ImportAndGetAsync(Guid mediaId)
    {
        await _content.ImportOkAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);
        return await GetAsync(mediaId, (await _api.SignupNewUserAsync()).AccessToken);
    }

    private async Task<MediaView> GetAsync(Guid mediaId, string token)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/media/{mediaId}", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<MediaView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    // No Authorization header: the browser's <img> and <audio> elements send none.
    private async Task<HttpResponseMessage> GetAnonymousAsync(string url, RangeHeaderValue? range = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = range;
        return await _api.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
