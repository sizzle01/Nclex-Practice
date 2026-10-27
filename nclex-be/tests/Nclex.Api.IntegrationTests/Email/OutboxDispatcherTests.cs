using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Email;
using Nclex.Infrastructure.Email;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Email;

/// <summary>
/// Runs the worker's dispatcher against the test database with a fake sender. These tests empty
/// the outbox first, so they run on their own (see <see cref="OutboxTestGroup"/>).
/// </summary>
[Collection(OutboxTestGroup.Name)]
public sealed class OutboxDispatcherTests(ApiFactory factory) : IAsyncLifetime
{
    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);
    private FakeEmailSender Sender => factory.EmailSender;

    public async ValueTask InitializeAsync()
    {
        Sender.Reset();
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().EmailOutbox
            .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        Sender.Reset();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task A_pending_email_is_sent_and_marked_sent()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");

        var attempted = await DispatchAsync();

        attempted.ShouldBe(1);
        var sent = Sender.Sent.ShouldHaveSingleItem();
        sent.To.ShouldBe(email);
        sent.Subject.ShouldBe("Verify your email address");
        var link = $"{ApiFactory.FrontendBaseUrl}/verify-email?token={Uri.EscapeDataString(token)}";
        sent.TextBody.ShouldContain(link);
        sent.HtmlBody.ShouldContain(link);

        var row = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        row.Status.ShouldBe(OutboxStatus.Sent);
        row.Attempts.ShouldBe((short)1);
        row.SentAt.ShouldNotBeNull();
        row.ProviderId.ShouldStartWith("fake-");
    }

    [Fact]
    public async Task Sending_clears_the_link_token_from_the_payload()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");

        await DispatchAsync();

        var row = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        row.Payload.ShouldNotContain(token);
    }

    [Fact]
    public async Task A_sent_email_is_not_sent_again()
    {
        await _api.SignupNewUserAsync();
        await DispatchAsync();

        var attempted = await DispatchAsync();

        attempted.ShouldBe(0);
        Sender.Sent.Count.ShouldBe(1);
    }

    [Fact]
    public async Task The_display_name_is_html_encoded()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupAsync(new { email, password = AuthApi.Password, track = "RN", displayName = "<b>Ada</b>" });

        await DispatchAsync();

        var sent = Sender.Sent.ShouldHaveSingleItem();
        sent.HtmlBody.ShouldContain("Hello &lt;b&gt;Ada&lt;/b&gt;,");
        sent.HtmlBody.ShouldNotContain("<b>Ada</b>");
    }

    [Fact]
    public async Task A_reset_email_links_to_the_reset_page()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        await _api.PostAsync("/api/v1/auth/password/forgot", new { email });
        var token = await _outbox.LatestTokenAsync(email, "reset_password");

        await DispatchAsync();

        var reset = Sender.Sent.Single(m => m.Subject == "Reset your password");
        reset.TextBody.ShouldContain($"{ApiFactory.FrontendBaseUrl}/reset-password?token={Uri.EscapeDataString(token)}");
    }

    [Fact]
    public async Task A_failed_send_is_retried_later()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        Sender.FailWith = new SmtpException("Service not available");

        var before = DateTimeOffset.UtcNow;
        await DispatchAsync();

        var row = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        row.Status.ShouldBe(OutboxStatus.Pending);
        row.Attempts.ShouldBe((short)1);
        row.LastError.ShouldBe("SmtpException: Service not available");
        row.SendAfter.ShouldBeGreaterThan(before.AddSeconds(25));
        (await DispatchAsync()).ShouldBe(0); // not due yet
    }

    [Fact]
    public async Task The_last_failed_attempt_marks_the_email_failed()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        await ExecuteSqlAsync($"UPDATE email_outbox SET attempts = {OutboxRetryPolicy.MaxAttempts - 1} WHERE to_email = {email}");
        Sender.FailWith = new SmtpException("Mailbox unavailable");

        await DispatchAsync();

        var row = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        row.Status.ShouldBe(OutboxStatus.Failed);
        row.Attempts.ShouldBe((short)OutboxRetryPolicy.MaxAttempts);
    }

    [Fact]
    public async Task An_unknown_template_fails_that_email_only()
    {
        var good = AuthApi.NewEmail();
        var bad = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(good);
        await _api.SignupNewUserAsync(bad);
        await ExecuteSqlAsync($"UPDATE email_outbox SET template = 'no_such_template' WHERE to_email = {bad}");

        await DispatchAsync();

        (await _outbox.ForAsync(good)).ShouldHaveSingleItem().Status.ShouldBe(OutboxStatus.Sent);
        var failed = (await _outbox.ForAsync(bad)).ShouldHaveSingleItem();
        failed.Status.ShouldBe(OutboxStatus.Pending);
        failed.LastError.ShouldNotBeNull().ShouldContain("no_such_template");
    }

    [Fact]
    public async Task Two_dispatchers_running_at_once_never_send_the_same_email()
    {
        const int emails = 30;
        await Task.WhenAll(Enumerable.Range(0, emails).Select(_ => _api.SignupNewUserAsync()));
        Sender.Delay = TimeSpan.FromMilliseconds(20);

        await Task.WhenAll(DrainAsync(), DrainAsync());

        Sender.Sent.Count.ShouldBe(emails);
        Sender.Sent.Select(m => m.To).Distinct().Count().ShouldBe(emails);
        await using var scope = factory.Services.CreateAsyncScope();
        var rows = await scope.ServiceProvider.GetRequiredService<AppDbContext>().EmailOutbox
            .ToListAsync(TestContext.Current.CancellationToken);
        rows.ShouldAllBe(r => r.Status == OutboxStatus.Sent && r.Attempts == 1);
    }

    [Fact]
    public async Task An_email_left_sending_by_a_stopped_worker_is_released_and_sent()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        // As if a worker claimed it 11 minutes ago and then crashed.
        await ExecuteSqlAsync(
            $"UPDATE email_outbox SET status = 'sending', attempts = 1, send_after = now() - interval '1 minute' WHERE to_email = {email}");

        await using var scope = factory.Services.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
        var released = await dispatcher.ReleaseExpiredClaimsAsync(TestContext.Current.CancellationToken);
        await dispatcher.DispatchDueAsync(TestContext.Current.CancellationToken);

        released.ShouldBe(1);
        var row = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        row.Status.ShouldBe(OutboxStatus.Sent);
        row.Attempts.ShouldBe((short)2);
    }

    [Fact]
    public async Task An_email_still_within_its_claim_lease_is_left_alone()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        await ExecuteSqlAsync(
            $"UPDATE email_outbox SET status = 'sending', attempts = 1, send_after = now() + interval '5 minutes' WHERE to_email = {email}");

        await using var scope = factory.Services.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

        (await dispatcher.ReleaseExpiredClaimsAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        (await dispatcher.DispatchDueAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    /// <summary>Runs one dispatcher round in its own scope, as the worker does every 2 seconds.</summary>
    private async Task<int> DispatchAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>()
            .DispatchDueAsync(TestContext.Current.CancellationToken);
    }

    private async Task DrainAsync()
    {
        while (await DispatchAsync() > 0)
        {
        }
    }

    private async Task ExecuteSqlAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database
            .ExecuteSqlAsync(sql, TestContext.Current.CancellationToken);
    }
}

/// <summary>
/// Tests that empty or drain the shared outbox. Running them apart from other tests stops them
/// sending, and so clearing the tokens of, emails that other tests are about to read.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class OutboxTestGroup
{
    public const string Name = "Email outbox";
}
