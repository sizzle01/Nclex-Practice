using FluentValidation.TestHelper;
using Nclex.Application.Performance.GetBreakdown;
using Nclex.Application.Performance.GetTrend;
using Nclex.Application.Performance.RebuildSnapshots;
using Nclex.Domain.Performance;

namespace Nclex.Application.Tests.Performance;

public sealed class PerformanceRequestValidatorTests
{
    [Fact]
    public void A_breakdown_needs_a_dimension() =>
        new GetBreakdownRequestValidator().TestValidate(new GetBreakdownRequest())
            .ShouldHaveValidationErrorFor(r => r.Dimension).WithErrorCode("REQUIRED");

    [Fact]
    public void The_overall_dimension_has_no_breakdown() =>
        new GetBreakdownRequestValidator().TestValidate(new GetBreakdownRequest(PerformanceDimension.Overall))
            .ShouldHaveValidationErrorFor(r => r.Dimension).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void A_category_dimension_is_a_valid_breakdown() =>
        new GetBreakdownRequestValidator().TestValidate(new GetBreakdownRequest(PerformanceDimension.BodySystem))
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void The_overall_trend_needs_no_key() =>
        new GetTrendRequestValidator().TestValidate(new GetTrendRequest())
            .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void A_category_trend_needs_the_category_key(string? key) =>
        new GetTrendRequestValidator().TestValidate(new GetTrendRequest(PerformanceDimension.ClientNeeds, key))
            .ShouldHaveValidationErrorFor(r => r.Key).WithErrorCode("REQUIRED");

    [Fact]
    public void A_trend_range_must_not_end_before_it_starts()
    {
        var request = new GetTrendRequest(From: new DateOnly(2026, 9, 14), To: new DateOnly(2026, 9, 13));

        new GetTrendRequestValidator().TestValidate(request).ShouldHaveValidationErrorFor(r => r.To).WithErrorCode("INVALID_VALUE");
    }

    [Fact]
    public void A_one_day_trend_range_is_valid()
    {
        var request = new GetTrendRequest(Bucket: TrendBucket.Day, From: new DateOnly(2026, 9, 14), To: new DateOnly(2026, 9, 14));

        new GetTrendRequestValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void A_rebuild_needs_a_user_id() =>
        new RebuildSnapshotsRequestValidator().TestValidate(new RebuildSnapshotsRequest(null))
            .ShouldHaveValidationErrorFor(r => r.UserId).WithErrorCode("REQUIRED");
}
