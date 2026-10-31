using Nclex.Domain.Recommendations;

namespace Nclex.Domain.Tests.Recommendations;

public sealed class SubtopicNamesTests
{
    [Theory]
    [InlineData("Medication dose calculations", "medication-dose-calculations")]
    [InlineData("  Pre-administration   checks! ", "pre-administration-checks")]
    [InlineData("Type 1 diabetes", "type-1-diabetes")]
    [InlineData("---", "")]
    public void Slug_keeps_letters_and_digits_joined_by_single_hyphens(string name, string slug)
    {
        SubtopicNames.Slug(name).ShouldBe(slug);
    }

    [Theory]
    [InlineData("insulin-administration", true)]
    [InlineData("type-1-diabetes", true)]
    [InlineData("", false)]
    [InlineData("Insulin", false)]
    [InlineData("double--hyphen", false)]
    [InlineData("-leading", false)]
    public void IsSlug_accepts_only_the_canonical_form(string value, bool expected)
    {
        SubtopicNames.IsSlug(value).ShouldBe(expected);
    }

    [Theory]
    [InlineData("Heart failure", "heart failures")]
    [InlineData("Pre-op checks", "PRE OP CHECK")]
    [InlineData("Chest tubes", "chest tube!")]
    public void Names_with_the_same_meaning_share_a_comparison_key(string first, string second)
    {
        SubtopicNames.ComparisonKey(first).ShouldBe(SubtopicNames.ComparisonKey(second));
    }

    [Fact]
    public void Different_topics_keep_different_keys()
    {
        SubtopicNames.ComparisonKey("Hypoglycemia treatment").ShouldNotBe(SubtopicNames.ComparisonKey("Recognizing hypoglycemia"));
    }
}
