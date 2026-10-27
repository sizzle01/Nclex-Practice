using FluentValidation.TestHelper;
using Nclex.Application.Auth.ForgotPassword;
using Nclex.Application.Auth.ResetPassword;
using Nclex.Application.Auth.VerifyEmail;

namespace Nclex.Application.Tests.Auth;

public sealed class EmailLinkRequestValidatorTests
{
    [Fact]
    public void Verify_email_needs_a_token()
    {
        var validator = new VerifyEmailRequestValidator();

        validator.TestValidate(new VerifyEmailRequest("abc")).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new VerifyEmailRequest(""))
            .ShouldHaveValidationErrorFor(r => r.Token).WithErrorCode("REQUIRED");
    }

    [Theory]
    [InlineData("ada@example.com", null)]
    [InlineData("", "REQUIRED")]
    [InlineData("not-an-email", "INVALID_FORMAT")]
    public void Forgot_password_checks_the_email_format(string email, string? code)
    {
        var result = new ForgotPasswordRequestValidator().TestValidate(new ForgotPasswordRequest(email));

        if (code is null)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(r => r.Email).WithErrorCode(code).Only();
        }
    }

    [Theory]
    [InlineData(10, null)]
    [InlineData(128, null)]
    [InlineData(0, "REQUIRED")]
    [InlineData(9, "TOO_SHORT")]
    [InlineData(129, "TOO_LONG")]
    public void Reset_password_applies_the_sign_up_password_rules(int length, string? code)
    {
        var result = new ResetPasswordRequestValidator().TestValidate(new ResetPasswordRequest("abc", new string('x', length)));

        if (code is null)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(r => r.NewPassword).WithErrorCode(code).Only();
        }
    }

    [Fact]
    public void Reset_password_needs_a_token()
    {
        new ResetPasswordRequestValidator().TestValidate(new ResetPasswordRequest("", "long-enough-password"))
            .ShouldHaveValidationErrorFor(r => r.Token).WithErrorCode("REQUIRED");
    }

    [Fact]
    public void Reset_password_signs_out_everywhere_unless_told_otherwise()
    {
        new ResetPasswordRequest("abc", "long-enough-password").SignOutEverywhere.ShouldBeTrue();
    }
}
