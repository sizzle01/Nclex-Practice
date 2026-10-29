using Nclex.Domain.Flags;

namespace Nclex.Domain.Tests.Flags;

public sealed class QuestionFlagTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Moderator = Guid.NewGuid();

    [Theory]
    [InlineData(FlagStatus.Open, FlagStatus.Open, true)]
    [InlineData(FlagStatus.Open, FlagStatus.Triaged, true)]
    [InlineData(FlagStatus.Open, FlagStatus.Resolved, true)]
    [InlineData(FlagStatus.Open, FlagStatus.Rejected, true)]
    [InlineData(FlagStatus.Triaged, FlagStatus.Triaged, true)]
    [InlineData(FlagStatus.Triaged, FlagStatus.Resolved, true)]
    [InlineData(FlagStatus.Triaged, FlagStatus.Rejected, true)]
    [InlineData(FlagStatus.Triaged, FlagStatus.Open, false)]
    [InlineData(FlagStatus.Resolved, FlagStatus.Resolved, false)]
    [InlineData(FlagStatus.Resolved, FlagStatus.Triaged, false)]
    [InlineData(FlagStatus.Rejected, FlagStatus.Resolved, false)]
    [InlineData(FlagStatus.Rejected, FlagStatus.Open, false)]
    public void Status_only_moves_forward_and_closed_is_final(FlagStatus from, FlagStatus to, bool allowed) =>
        QuestionFlag.CanMove(from, to).ShouldBe(allowed);

    [Fact]
    public void A_new_report_is_open_with_a_trimmed_comment()
    {
        var flag = NewFlag("  Option C is also right.  ");

        flag.Status.ShouldBe(FlagStatus.Open);
        flag.Comment.ShouldBe("Option C is also right.");
        flag.CreatedAt.ShouldBe(Now);
        flag.IsClosed.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_comment_is_stored_as_none(string? comment) =>
        NewFlag(comment).Comment.ShouldBeNull();

    [Fact]
    public void A_comment_over_the_limit_is_refused() =>
        Should.Throw<ArgumentException>(() => NewFlag(new string('x', QuestionFlag.MaxCommentLength + 1)));

    [Fact]
    public void Resolving_records_the_note_who_and_when()
    {
        var flag = NewFlag();
        flag.Triage();

        flag.Resolve("  Fixed the key.  ", Moderator, Now.AddHours(1));

        flag.Status.ShouldBe(FlagStatus.Resolved);
        flag.ResolutionNote.ShouldBe("Fixed the key.");
        flag.ResolvedBy.ShouldBe(Moderator);
        flag.ResolvedAt.ShouldBe(Now.AddHours(1));
        flag.IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void Rejecting_closes_the_report_too()
    {
        var flag = NewFlag();

        flag.Reject("Matches current guidance.", Moderator, Now);

        flag.Status.ShouldBe(FlagStatus.Rejected);
        flag.IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void Closing_needs_a_note() =>
        Should.Throw<ArgumentException>(() => NewFlag().Resolve(" ", Moderator, Now));

    [Fact]
    public void A_closed_report_cannot_change_again()
    {
        var flag = NewFlag();
        flag.Resolve("Fixed.", Moderator, Now);

        Should.Throw<InvalidOperationException>(() => flag.Reject("No.", Moderator, Now));
        Should.Throw<InvalidOperationException>(flag.Triage);
        Should.Throw<InvalidOperationException>(() => flag.AssignTo(Moderator));
    }

    private static QuestionFlag NewFlag(string? comment = null) =>
        QuestionFlag.Report(Guid.NewGuid(), Guid.NewGuid(), attemptId: null, FlagReason.IncorrectAnswerKey, comment, Now);
}
