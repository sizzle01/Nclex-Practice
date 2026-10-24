using FluentValidation.TestHelper;
using Nclex.Application.Auth.Signup;
using Nclex.Domain.Entities;

namespace Nclex.Application.Tests.Auth;

public sealed class SignupRequestValidatorTests
{
    private readonly SignupRequestValidator _validator = new();

    private static SignupRequest Valid() => new("ada@example.com", "correct-horse", ExamTrack.RN, "Ada");

    [Fact]
    public void A_complete_request_is_valid() =>
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("", "REQUIRED")]
    [InlineData("not-an-email", "INVALID_FORMAT")]
    [InlineData("Ada <ada@example.com>", "INVALID_FORMAT")]
    [InlineData("ada@example.com, bob@example.com", "INVALID_FORMAT")]
    public void Invalid_email_is_rejected(string email, string code) =>
        _validator.TestValidate(Valid() with { Email = email })
            .ShouldHaveValidationErrorFor(r => r.Email).WithErrorCode(code).Only();

    [Fact]
    public void Email_longer_than_254_characters_is_too_long()
    {
        var email = new string('a', 243) + "@example.com"; // 255 characters

        _validator.TestValidate(Valid() with { Email = email })
            .ShouldHaveValidationErrorFor(r => r.Email).WithErrorCode("TOO_LONG");
    }

    [Theory]
    [InlineData(0, "REQUIRED")]
    [InlineData(9, "TOO_SHORT")]
    [InlineData(129, "TOO_LONG")]
    public void Password_outside_10_to_128_characters_is_rejected(int length, string code) =>
        _validator.TestValidate(Valid() with { Password = new string('x', length) })
            .ShouldHaveValidationErrorFor(r => r.Password).WithErrorCode(code).Only();

    [Theory]
    [InlineData(10)]
    [InlineData(128)]
    public void Password_at_the_length_limits_is_valid(int length) =>
        _validator.TestValidate(Valid() with { Password = new string('x', length) })
            .ShouldNotHaveValidationErrorFor(r => r.Password);

    [Fact]
    public void Missing_track_is_required() =>
        _validator.TestValidate(Valid() with { Track = null })
            .ShouldHaveValidationErrorFor(r => r.Track).WithErrorCode("REQUIRED");

    [Fact]
    public void Display_name_is_optional() =>
        _validator.TestValidate(Valid() with { DisplayName = null }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Display_name_over_80_characters_is_too_long() =>
        _validator.TestValidate(Valid() with { DisplayName = new string('a', 81) })
            .ShouldHaveValidationErrorFor(r => r.DisplayName).WithErrorCode("TOO_LONG");
}
