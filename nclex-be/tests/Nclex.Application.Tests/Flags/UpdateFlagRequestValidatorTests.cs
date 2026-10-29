using FluentValidation.TestHelper;
using Nclex.Application.Flags.Moderation;
using Nclex.Domain.Flags;

namespace Nclex.Application.Tests.Flags;

public sealed class UpdateFlagRequestValidatorTests
{
    private readonly UpdateFlagRequestValidator _validator = new();

    [Theory]
    [InlineData(FlagStatus.Resolved)]
    [InlineData(FlagStatus.Rejected)]
    public void Closing_with_a_note_is_valid(FlagStatus status) =>
        _validator.TestValidate(new UpdateFlagRequest(status, "Fixed.")).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(FlagStatus.Resolved, null)]
    [InlineData(FlagStatus.Rejected, "  ")]
    public void Closing_without_a_note_is_refused(FlagStatus status, string? note) =>
        _validator.TestValidate(new UpdateFlagRequest(status, note))
            .ShouldHaveValidationErrorFor(r => r.ResolutionNote).WithErrorCode("REQUIRED");

    [Fact]
    public void Triaging_needs_no_note() =>
        _validator.TestValidate(new UpdateFlagRequest(FlagStatus.Triaged)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Assigning_alone_is_valid() =>
        _validator.TestValidate(new UpdateFlagRequest(AssigneeId: Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void A_report_cannot_be_set_back_to_open() =>
        _validator.TestValidate(new UpdateFlagRequest(FlagStatus.Open))
            .ShouldHaveValidationErrorFor(r => r.Status).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void A_note_over_the_limit_is_refused() =>
        _validator.TestValidate(new UpdateFlagRequest(FlagStatus.Resolved, new string('x', UpdateFlagRequest.MaxNoteLength + 1)))
            .ShouldHaveValidationErrorFor(r => r.ResolutionNote).WithErrorCode("TOO_LONG");

    [Fact]
    public void An_empty_patch_is_refused() =>
        _validator.TestValidate(new UpdateFlagRequest()).IsValid.ShouldBeFalse();
}
