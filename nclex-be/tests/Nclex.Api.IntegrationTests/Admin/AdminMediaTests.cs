using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Admin.Media;
using Nclex.Application.Media.GetMedia;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Admin;

public sealed class AdminMediaTests(ApiFactory factory)
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private readonly AdminApi _admin = new(factory);

    [Fact]
    public async Task An_author_uploads_an_image_and_describes_it()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var link = await UploadUrlAsync(author, "image/png", Png.Length);

        var upload = await PutAsync(link.UploadUrl, Png, "image/png");
        upload.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var patched = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/media/{link.MediaId}", author, new { altText = "Stage 2 pressure injury on the sacrum" });
        patched.StatusCode.ShouldBe(HttpStatusCode.OK);
        var view = (await patched.Content.ReadFromJsonAsync<AdminMediaView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        view.ShouldSatisfyAllConditions(
            v => v.Kind.ShouldBe("image"),
            v => v.Uploaded.ShouldBeTrue(),
            v => v.AltText.ShouldBe("Stage 2 pressure injury on the sacrum"));

        // The player's endpoint now serves it.
        var media = await _admin.SendAsync(HttpMethod.Get, $"/api/v1/media/{link.MediaId}", author);
        var shown = (await media.Content.ReadFromJsonAsync<MediaView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        var file = await _admin.SendAsync(HttpMethod.Get, shown.Url, who: null);
        (await file.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBe(Png);
    }

    [Fact]
    public async Task A_changed_signature_is_refused()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var link = await UploadUrlAsync(author, "image/png", Png.Length);

        var response = await PutAsync(link.UploadUrl.Replace("sig=", "sig=x", StringComparison.Ordinal), Png, "image/png");

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task The_file_must_have_the_declared_size_and_type()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var link = await UploadUrlAsync(author, "image/png", Png.Length);

        var tooBig = await PutAsync(link.UploadUrl, [.. Png, 0], "image/png");
        await tooBig.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        var wrongType = await PutAsync(link.UploadUrl, Png, "image/jpeg");
        await wrongType.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Theory]
    [InlineData("image/svg+xml", 100)]
    [InlineData("application/pdf", 100)]
    [InlineData("image/png", 0)]
    [InlineData("image/png", 26_214_401)]
    public async Task Only_allowed_types_and_sizes_get_an_upload_link(string mimeType, long bytes)
    {
        var author = await _admin.StaffAsync(UserRole.Author);

        var response = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/media/upload-url", author, new { mimeType, bytes });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task A_version_showing_an_image_without_alt_text_cannot_go_to_review()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var link = await UploadUrlAsync(author, "image/png", Png.Length);
        await PutAsync(link.UploadUrl, Png, "image/png");
        var draft = await _admin.McDraftAsync();
        draft["exhibits"] = new[] { new { id = "x1", kind = "image", title = "Wound", mediaId = link.MediaId } };
        var question = await _admin.QuestionOkAsync(HttpMethod.Post, "/api/v1/admin/questions", author, new { track = "RN", draft });
        var versionId = question.Versions[0].Id;

        var submit = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);
        (await submit.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("exhibits[0].mediaId.altText", "REQUIRED"));

        await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/media/{link.MediaId}", author, new { altText = "Wound photo" });
        var retry = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);
        retry.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Captions_must_be_webvtt_and_images_take_none()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var audio = await UploadUrlAsync(author, "audio/wav", 10);
        var image = await UploadUrlAsync(author, "image/png", Png.Length);

        var notVtt = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/media/{audio.MediaId}", author, new { captions = "1\n00:00 hello" });
        (await notVtt.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("captions", "INVALID_FORMAT"));
        var ok = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/media/{audio.MediaId}", author, new { captions = "WEBVTT\n\n00:00.000 --> 00:01.000\nCrackles", transcript = "Fine crackles." });
        var view = (await ok.Content.ReadFromJsonAsync<AdminMediaView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        view.HasCaptions.ShouldBeTrue();
        view.HasTranscript.ShouldBeTrue();
        var onImage = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/media/{image.MediaId}", author, new { transcript = "x" });
        await onImage.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    private async Task<UploadUrlResponse> UploadUrlAsync(AdminApi.Staff author, string mimeType, long bytes)
    {
        var response = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/media/upload-url", author, new { mimeType, bytes });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<UploadUrlResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    // Like a presigned URL: no Authorization header.
    private async Task<HttpResponseMessage> PutAsync(string url, byte[] body, string contentType)
    {
        using var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return await new AuthApi(factory).Client.PutAsync(url, content, TestContext.Current.CancellationToken);
    }
}
