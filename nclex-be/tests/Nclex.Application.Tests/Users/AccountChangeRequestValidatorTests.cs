using FluentValidation.TestHelper;
using Nclex.Application.Auth.ConfirmEmailChange;
using Nclex.Application.Users.ChangeEmail;
using Nclex.Application.Users.ChangePassword;
using Nclex.Application.Users.DeleteAccount;

namespace Nclex.Application.Tests.Users;

public sealed class AccountChangeRequestValidatorTests
{
    [Fact]
    public void Change_password_needs_the_current_password_and_a_valid_new_one()
    {
        var validator = new ChangePasswordRequestValidator();

        validator.TestValidate(new ChangePasswordRequest("old", "a-long-enough-password")).ShouldNotHaveAnyValidationErrors();
        var result = validator.TestValidate(new ChangePasswordRequest("", "short"));
        result.ShouldHaveValidationErrorFor(r => r.CurrentPassword).WithErrorCode("REQUIRED");
        result.ShouldHaveValidationErrorFor(r => r.NewPassword).WithErrorCode("TOO_SHORT");
    }

    [Theory]
    [InlineData("new@example.com", null)]
    [InlineData("", "REQUIRED")]
    [InlineData("nope", "INVALID_FORMAT")]
    public void Change_email_checks_the_new_address(string newEmail, string? code)
    {
        var result = new ChangeEmailRequestValidator().TestValidate(new ChangeEmailRequest("old", newEmail));

        if (code is null)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(r => r.NewEmail).WithErrorCode(code);
        }
    }

    [Fact]
    public void Change_email_needs_the_current_password()
    {
        new ChangeEmailRequestValidator().TestValidate(new ChangeEmailRequest("", "new@example.com"))
            .ShouldHaveValidationErrorFor(r => r.CurrentPassword).WithErrorCode("REQUIRED");
    }

    [Fact]
    public void Delete_account_needs_the_current_password()
    {
        var validator = new DeleteAccountRequestValidator();

        validator.TestValidate(new DeleteAccountRequest("old")).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new DeleteAccountRequest(""))
            .ShouldHaveValidationErrorFor(r => r.CurrentPassword).WithErrorCode("REQUIRED");
    }

    [Fact]
    public void Confirm_email_change_needs_a_token()
    {
        new ConfirmEmailChangeRequestValidator().TestValidate(new ConfirmEmailChangeRequest(""))
            .ShouldHaveValidationErrorFor(r => r.Token).WithErrorCode("REQUIRED");
    }
}
