using FluentValidation.TestHelper;
using Nclex.Application.Taxonomy.GetTaxonomy;
using Nclex.Domain.Entities;

namespace Nclex.Application.Tests.Taxonomy;

public sealed class GetTaxonomyRequestValidatorTests
{
    private readonly GetTaxonomyRequestValidator _validator = new();

    [Theory]
    [InlineData(ExamTrack.RN)]
    [InlineData(ExamTrack.PN)]
    public void Rn_and_pn_are_valid(ExamTrack track) =>
        _validator.TestValidate(new GetTaxonomyRequest(track)).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Track_is_required() =>
        _validator.TestValidate(new GetTaxonomyRequest())
            .ShouldHaveValidationErrorFor(r => r.Track).WithErrorCode("REQUIRED");

    [Fact]
    public void An_undefined_track_value_is_invalid() =>
        _validator.TestValidate(new GetTaxonomyRequest((ExamTrack)7))
            .ShouldHaveValidationErrorFor(r => r.Track).WithErrorCode("INVALID_VALUE");
}
