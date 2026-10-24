using FluentValidation.TestHelper;
using Nclex.Application.Users.UpdateMe;

namespace Nclex.Application.Tests.Users;

public sealed class UpdateMeRequestValidatorTests
{
    private readonly UpdateMeRequestValidator _validator = new();

    [Fact]
    public void An_empty_patch_is_valid() =>
        _validator.TestValidate(new UpdateMeRequest()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("UTC")]
    [InlineData("Europe/London")]
    [InlineData("America/Chicago")]
    public void Iana_time_zones_are_valid(string timezone) =>
        _validator.TestValidate(new UpdateMeRequest(Timezone: timezone)).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("Mars/Olympus_Mons")]
    [InlineData("GMT Standard Time")]
    [InlineData("")]
    public void Other_time_zones_are_invalid(string timezone) =>
        _validator.TestValidate(new UpdateMeRequest(Timezone: timezone))
            .ShouldHaveValidationErrorFor(r => r.Timezone).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void Display_name_over_80_characters_is_too_long() =>
        _validator.TestValidate(new UpdateMeRequest(DisplayName: new string('a', 81)))
            .ShouldHaveValidationErrorFor(r => r.DisplayName).WithErrorCode("TOO_LONG");
}
