using FluentValidation.TestHelper;
using Nclex.Application.Notifications.Preferences;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Tests.Notifications;

public sealed class UpdatePreferencesRequestValidatorTests
{
    private readonly UpdatePreferencesRequestValidator _validator = new();

    private static List<PreferenceInput> AllOn() =>
        [.. Enum.GetValues<NotificationCategory>().Select(c => new PreferenceInput(c, true, true))];

    [Fact]
    public void Every_category_once_is_valid() =>
        _validator.TestValidate(new UpdatePreferencesRequest(AllOn())).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Switching_off_a_category_other_than_security_is_valid()
    {
        var preferences = AllOn();
        preferences[0] = new PreferenceInput(NotificationCategory.Results, false, false);

        _validator.TestValidate(new UpdatePreferencesRequest(preferences)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void A_missing_list_is_required() =>
        _validator.TestValidate(new UpdatePreferencesRequest(null))
            .ShouldHaveValidationErrorFor(r => r.Preferences).WithErrorCode("REQUIRED");

    [Fact]
    public void A_missing_category_is_rejected() =>
        _validator.TestValidate(new UpdatePreferencesRequest(AllOn()[1..]))
            .ShouldHaveValidationErrorFor(r => r.Preferences).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void A_repeated_category_is_rejected()
    {
        var preferences = AllOn();
        preferences[1] = preferences[0];

        _validator.TestValidate(new UpdatePreferencesRequest(preferences))
            .ShouldHaveValidationErrorFor(r => r.Preferences).WithErrorCode("INVALID_VALUE");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Security_cannot_be_switched_off(bool inApp, bool email)
    {
        var preferences = AllOn();
        preferences[^1] = new PreferenceInput(NotificationCategory.Security, inApp, email);

        _validator.TestValidate(new UpdatePreferencesRequest(preferences))
            .Errors.ShouldContain(e => e.ErrorCode == "INVALID_VALUE" && e.PropertyName.StartsWith("Preferences[4]", StringComparison.Ordinal));
    }

    [Fact]
    public void Each_channel_must_be_given()
    {
        var preferences = AllOn();
        preferences[0] = new PreferenceInput(NotificationCategory.Results, null, true);

        _validator.TestValidate(new UpdatePreferencesRequest(preferences))
            .Errors.ShouldContain(e => e.ErrorCode == "REQUIRED" && e.PropertyName == "Preferences[0].InApp");
    }
}
