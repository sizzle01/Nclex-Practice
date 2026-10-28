using FluentValidation.TestHelper;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.ListAttempts;
using Nclex.Application.Attempts.Navigate;
using Nclex.Application.Attempts.SaveResponse;
using Nclex.Application.Attempts.UpdateItemMarks;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Tests.Attempts;

public sealed class AttemptRequestValidatorTests
{
    [Theory]
    [InlineData(null, new[] { AttemptStatus.InProgress, AttemptStatus.Paused })]
    [InlineData("submitted", new[] { AttemptStatus.Submitted })]
    [InlineData("in_progress, paused,in_progress", new[] { AttemptStatus.InProgress, AttemptStatus.Paused })]
    public void Status_filters_are_read_from_snake_case_names(string? value, AttemptStatus[] expected)
    {
        AttemptStatusFilter.TryParse(value, out var statuses).ShouldBeTrue();

        statuses.ShouldBe(expected);
    }

    [Theory]
    [InlineData("finished")]
    [InlineData("InProgress")]
    [InlineData("")]
    public void Unknown_statuses_are_refused(string value)
    {
        AttemptStatusFilter.TryParse(value, out _).ShouldBeFalse();
        new ListAttemptsRequestValidator().TestValidate(new ListAttemptsRequest(value))
            .ShouldHaveValidationErrorFor(r => r.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void List_limits_stay_between_1_and_50(int limit) =>
        new ListAttemptsRequestValidator().TestValidate(new ListAttemptsRequest(Limit: limit))
            .ShouldHaveValidationErrorFor(r => r.Limit);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void Client_seq_starts_at_1(int clientSeq, bool valid)
    {
        var result = new SaveResponseRequestValidator().TestValidate(new SaveResponseRequest(null, clientSeq));

        result.IsValid.ShouldBe(valid);
    }

    [Fact]
    public void Navigation_targets_start_at_1() =>
        new NavigateRequestValidator().TestValidate(new NavigateRequest(0)).ShouldHaveValidationErrorFor(r => r.ToPosition);

    [Fact]
    public void Struck_option_ids_must_be_short_and_not_empty()
    {
        var validator = new UpdateItemMarksRequestValidator();

        validator.TestValidate(new UpdateItemMarksRequest(StruckOptions: ["a", ""])).IsValid.ShouldBeFalse();
        validator.TestValidate(new UpdateItemMarksRequest(StruckOptions: [new string('x', 65)])).IsValid.ShouldBeFalse();
        validator.TestValidate(new UpdateItemMarksRequest(Flagged: true, StruckOptions: ["a", "b"])).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, "REQUIRED")]
    [InlineData("  ", "REQUIRED")]
    public void The_client_header_is_required(string? header, string code)
    {
        var error = Should.Throw<AppException>(() => AttemptClient.Require(header));

        error.FieldErrors.ShouldBe([new AppFieldError(AttemptClient.HeaderName, code)]);
    }

    [Fact]
    public void An_overlong_client_header_is_refused() =>
        Should.Throw<AppException>(() => AttemptClient.Require(new string('c', 65)))
            .FieldErrors.Single().Code.ShouldBe("TOO_LONG");
}
