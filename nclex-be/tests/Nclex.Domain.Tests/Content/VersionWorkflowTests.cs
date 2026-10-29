using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Content;

public sealed class VersionWorkflowTests
{
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Reviewer = Guid.NewGuid();

    [Theory]
    [InlineData(VersionAction.Edit, VersionStatus.Draft)]
    [InlineData(VersionAction.SubmitForReview, VersionStatus.Draft)]
    [InlineData(VersionAction.Approve, VersionStatus.InReview)]
    [InlineData(VersionAction.Reject, VersionStatus.InReview)]
    [InlineData(VersionAction.Publish, VersionStatus.Approved)]
    public void Each_action_is_allowed_from_its_own_status(VersionAction action, VersionStatus status)
    {
        VersionWorkflow.Check(action, status, Author, Reviewer).ShouldBeNull();
    }

    [Fact]
    public void Every_other_status_is_refused()
    {
        foreach (var action in Enum.GetValues<VersionAction>())
        {
            foreach (var status in Enum.GetValues<VersionStatus>().Where(s => s != VersionWorkflow.From(action)))
            {
                VersionWorkflow.Check(action, status, Author, Reviewer).ShouldBe(WorkflowRefusal.WrongStatus, $"{action} from {status}");
            }
        }
    }

    [Theory]
    [InlineData(VersionAction.Approve)]
    [InlineData(VersionAction.Reject)]
    public void Nobody_reviews_their_own_version(VersionAction action)
    {
        VersionWorkflow.Check(action, VersionStatus.InReview, Author, actorId: Author).ShouldBe(WorkflowRefusal.OwnVersion);
    }

    [Theory]
    [InlineData(VersionAction.Edit, VersionStatus.Draft)]
    [InlineData(VersionAction.SubmitForReview, VersionStatus.Draft)]
    [InlineData(VersionAction.Publish, VersionStatus.Approved)]
    public void Authors_may_edit_submit_and_publish_their_own_versions(VersionAction action, VersionStatus status)
    {
        // Publishing an approved version is not a review: someone else already approved it.
        VersionWorkflow.Check(action, status, Author, actorId: Author).ShouldBeNull();
    }

    [Theory]
    [InlineData(VersionAction.Edit, VersionStatus.Draft)]
    [InlineData(VersionAction.SubmitForReview, VersionStatus.InReview)]
    [InlineData(VersionAction.Approve, VersionStatus.Approved)]
    [InlineData(VersionAction.Reject, VersionStatus.Rejected)]
    [InlineData(VersionAction.Publish, VersionStatus.Published)]
    public void Each_action_leads_to_its_status(VersionAction action, VersionStatus expected)
    {
        VersionWorkflow.To(action).ShouldBe(expected);
    }

    [Theory]
    [InlineData(VersionStatus.Draft, true)]
    [InlineData(VersionStatus.InReview, true)]
    [InlineData(VersionStatus.Approved, true)]
    [InlineData(VersionStatus.Published, false)]
    [InlineData(VersionStatus.Superseded, false)]
    [InlineData(VersionStatus.Rejected, false)]
    public void Open_versions_are_the_ones_still_on_their_way_to_candidates(VersionStatus status, bool open)
    {
        VersionWorkflow.IsOpen(status).ShouldBe(open);
    }
}
