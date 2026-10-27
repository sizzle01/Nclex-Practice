using FluentValidation.TestHelper;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.ListTestSets;
using Nclex.Domain.Entities;

namespace Nclex.Application.Tests.Catalog;

public sealed class ListTestSetsRequestValidatorTests
{
    private readonly ListTestSetsRequestValidator _validator = new();

    [Fact]
    public void Everything_is_optional() =>
        _validator.TestValidate(new ListTestSetsRequest()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void A_full_request_is_valid() =>
        _validator.TestValidate(new ListTestSetsRequest(ExamTrack.PN, TestSetKind.Prebuilt, 50, PageCursor.Encode(20)))
            .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(TestSetKind.Custom)]
    [InlineData(TestSetKind.Cat)]
    [InlineData(TestSetKind.Assigned)]
    public void Only_prebuilt_sets_can_be_listed_for_now(TestSetKind kind) =>
        _validator.TestValidate(new ListTestSetsRequest(Kind: kind))
            .ShouldHaveValidationErrorFor(r => r.Kind).WithErrorCode("INVALID_VALUE");

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void The_limit_is_between_1_and_50(int limit) =>
        _validator.TestValidate(new ListTestSetsRequest(Limit: limit))
            .ShouldHaveValidationErrorFor(r => r.Limit).WithErrorCode("INVALID_VALUE");

    [Theory]
    [InlineData("not a cursor!")]
    [InlineData("aGVsbG8")] // "hello": valid base64url, but not a cursor this API made
    public void A_cursor_the_api_did_not_make_is_invalid(string cursor) =>
        _validator.TestValidate(new ListTestSetsRequest(Cursor: cursor))
            .ShouldHaveValidationErrorFor(r => r.Cursor).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void An_undefined_track_value_is_invalid() =>
        _validator.TestValidate(new ListTestSetsRequest((ExamTrack)7))
            .ShouldHaveValidationErrorFor(r => r.Track).WithErrorCode("INVALID_VALUE");
}
