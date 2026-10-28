# F12: Notifications

## 1. Summary

Candidates now hear about things that happen to them without refreshing the page. When a test is graded, a "You passed", "You didn't reach the passing threshold" or "Your results are ready" notification appears in the bell. The result email goes out with it, written in the same transaction as the result, so a result never exists without its notice and the notice is never sent twice. The bell's unread badge updates live over Server-Sent Events from any API replica, and polls once a minute while the stream is down. Account events from F03 (password and email changes, lockouts, new sign-ins) now appear in the bell as well as by email. A worker job reminds candidates about drafts they haven't touched for three days. On `/settings/notifications`, candidates choose per category whether they want notifications in the app, by email, or both. Security notices are always on.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f12`. It branches from `main`, which now contains F10 (fast-forwarded before this feature started; nothing was pushed).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F12_Notifications (and backfills preferences)

dotnet run --project src/Nclex.Api          # http://localhost:5080
dotnet run --project src/Nclex.Worker       # sends emails and runs the hourly draft reminders
cd ../nclex-fe && npm run dev                # http://localhost:3000
```

**In the browser:**

1. Sign in as a verified user. F08's walkthrough shows how to verify one. A bell now sits between the theme switcher and your name.
2. Open a second tab on the dashboard. In the first tab, start **Comprehensive Challenge 1** (65% threshold), answer one question and submit the test.
3. In the second tab the badge turns to **1** without a refresh. Open the bell and you'll see "You didn't reach the passing threshold this time" with your two weakest categories. Click it: it is marked read, and the results page opens at the breakdown tables (`#breakdown`).
4. Mailpit (http://localhost:8025) has the result email, with a **See my results** button and a link to notification settings.
5. Go to **Notification settings** (bell footer or user menu). Untick **Results → By email** and save. Submit another test: the bell gets a notification, but no email arrives. The **Security** row is greyed out.
6. Change your password on `/settings/account`. "Your password was changed" appears in the bell, and the security email still arrives.
7. **Polling fallback:** stop Redis with `docker compose stop redis`. The badge keeps updating, within a minute, because `stream-ticket` now returns 503 and the bell polls `unread-count`. Start Redis again and the stream reconnects. The backoff goes from 1 s up to 30 s.
8. **Draft reminder:** start any set and leave it. Age it, then press **Trigger now** on the `draft-reminders` recurring job in the API's Hangfire dashboard (http://localhost:5080/hangfire, Development only), or wait for the hourly run:

   ```bash
   docker exec nclex-postgres-1 psql -U nclex -d nclex -c "update attempts set last_activity_at = now() - interval '4 days' where status in ('in_progress','paused')"
   ```

   "Pick up where you left off: Comprehensive Practice 1 is N% done." appears once. It won't repeat until you work on the draft again and leave it for another three days.

**With curl:**

```bash
B=http://localhost:5080/api/v1
TOKEN=...   # a signed-in user's access token
H=(-H "Authorization: Bearer $TOKEN")
J=(-H 'Content-Type: application/json')

curl -s "$B/notifications?limit=5" "${H[@]}"            # {"items":[…],"nextCursor":"…"|null}
curl -s "$B/notifications?unreadOnly=true" "${H[@]}"
curl -s "$B/notifications/unread-count" "${H[@]}"       # {"count":2}
curl -s -X POST "$B/notifications/$NID/read" "${H[@]}"  # 204 (404 NOT_FOUND for someone else's)
curl -s -X POST "$B/notifications/read-all" "${H[@]}"   # 204

# Live stream: trade the token for a 30-second single-use ticket, then listen
T=$(curl -s -X POST "$B/notifications/stream-ticket" "${H[@]}" | jq -r .ticket)
curl -N "$B/notifications/stream?ticket=$T"
# event: unread_count / data: {"count":0}
# event: ping         / data: {}            (every 25 s)
# event: notification / data: {…notification…}, then a fresh unread_count

curl -s "$B/me/notification-preferences" "${H[@]}"
curl -s -X PUT "$B/me/notification-preferences" "${H[@]}" "${J[@]}" -d '{"preferences":[
  {"category":"results","inApp":true,"email":false},{"category":"reminders","inApp":true,"email":true},
  {"category":"engagement","inApp":true,"email":false},{"category":"content","inApp":true,"email":false},
  {"category":"security","inApp":true,"email":true}]}'
# Setting security to false gives 400 VALIDATION_FAILED.
```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Notifications/NotificationKinds.cs`: the two Postgres enums and the channel names.
2. `nclex-be/src/Nclex.Domain/Notifications/NotificationDefaults.cs`: which channels a notification goes to, given the preference row. Security is locked on.
3. `nclex-be/src/Nclex.Domain/Notifications/Notification.cs` and `NotificationPreference.cs`: the two entities.
4. `nclex-be/src/Nclex.Domain/Notifications/NotificationCopy.cs`: every title and body (interfaces.md 5.6), as pure functions.
5. `nclex-be/src/Nclex.Domain/Notifications/DraftReminderPolicy.cs`: "once per stretch of inactivity".
6. `nclex-be/src/Nclex.Application/Abstractions/INotificationPublisher.cs`: the producer contract and the `NewNotification` record.
7. `nclex-be/src/Nclex.Application/Notifications/ResultNotifications.cs`: the result notification grading builds (called from `GradingService.FinalizeAttemptAsync`).
8. `nclex-be/src/Nclex.Application/Notifications/SecurityNotifications.cs`: replaces F03's `SecurityEmails`.
9. `nclex-be/src/Nclex.Infrastructure/Notifications/NotificationPublisher.cs`: preference lookup, the notification row and the outbox row, all in the caller's transaction.
10. `nclex-be/src/Nclex.Infrastructure/Notifications/PublishNotificationsAfterCommit.cs` and `PendingNotifications.cs`: "COMMIT, then PUBLISH".
11. `nclex-be/src/Nclex.Infrastructure/Notifications/RedisNotificationBus.cs` and `RedisStreamTicketStore.cs`: the Redis channel and the tickets.
12. `nclex-be/src/Nclex.Api/Sse/NotificationStreamEndpoint.cs`: the SSE endpoint.
13. `nclex-be/src/Nclex.Application/Notifications/Feed/ListNotificationsHandler.cs`, `ReadState/*`, `Preferences/*`: the REST use cases. `NotificationsController` and `NotificationPreferencesController` stay thin.
14. `nclex-be/src/Nclex.Application/Notifications/DraftReminders/SendDraftRemindersHandler.cs` and `Nclex.Infrastructure/Jobs/DraftReminderJob.cs`: the reminder job.
15. `nclex-be/src/Nclex.Infrastructure/Email/Templates/ResultEmailTemplates.cs`, `DraftReminderTemplate.cs` and `NotificationEmailLayout.cs`: the new emails.
16. `nclex-fe/src/features/notifications/api/notificationsApi.ts` and `notificationQueries.ts`: fetch functions and TanStack Query hooks.
17. `nclex-fe/src/features/notifications/stream/notificationStream.ts` and `useNotificationCenter.ts`: ticket, EventSource, backoff and polling fallback.
18. `nclex-fe/src/features/notifications/components/NotificationBell.tsx`: the bell, plus `NotificationList`, `NotificationsHistory` and `NotificationPreferencesForm`.

## 4. Request flow: a result reaching another tab's bell

1. **UI (tab A):** the bell (`useNotificationCenter`) calls `POST /notifications/stream-ticket`. `IssueStreamTicketHandler` stores the SHA-256 of a random ticket in Redis for 30 s, keyed to the user. The bell opens `EventSource('/api/v1/notifications/stream?ticket=…')`. Next.js forwards it to the API.
2. **API (replica 1):** `NotificationStreamEndpoint` redeems the ticket with `GETDEL`, so it works once. It subscribes to `user:{id}:notifications` through `RedisNotificationBus` and sends `unread_count`. The bell now stops polling.
3. **UI (tab B):** the candidate presses **Submit test**, which is `POST /attempts/{id}/submit`.
4. **API (replica 2) → Application:** `SubmitAttemptHandler` begins a transaction and locks the attempt `FOR UPDATE`. `GradingService.FinalizeAttemptAsync` grades the attempt and writes the result (F10). It then calls `INotificationPublisher.PublishAsync(ResultNotifications.For(…))`.
5. **Infrastructure:** `NotificationPublisher` skips the call if this attempt already has a result notification. Otherwise it reads the user's `results` preference and works out the channels with `NotificationDefaults.ChannelsFor`. It adds the `notifications` row and, if email is on, an `email_outbox` row through `IEmailOutbox`. It also queues the row in `PendingNotifications`.
6. **Database:** `COMMIT`. `PublishNotificationsAfterCommit.TransactionCommittedAsync` fires and publishes `{notification}` to the user's Redis channel. A rollback would have discarded it instead.
7. **Back to replica 1:** its subscription receives the message and writes `event: notification`, then a recounted `event: unread_count`.
8. **UI (tab A):** `notificationStream.ts` updates the TanStack Query cache. The badge changes and any open feed refetches. Meanwhile the worker's outbox dispatcher sends the email (F02).

## 5. Database changes

Migration `F12_Notifications`, matching database.md 2.8:

- **Enums** `notification_type` and `notification_category`, with their labels in DDL order. EF generates them alphabetically, so the migration's annotations were reordered, as F08 did. `MigrationTests` checks this.
- **`notifications`**:
  - `data jsonb DEFAULT '{}'` and `channels text[]`.
  - `user_id` cascades on delete; `related_attempt_id` is set to null on delete.
  - Indexes: `notif_feed_idx (user_id, created_at DESC)` for the feed, `notif_unread_idx (user_id) WHERE read_at IS NULL` for the badge, and the partial unique `notif_result_once (related_attempt_id) WHERE type IN (result_*)`, so a grading retry can't double-notify.
- **`notification_preferences`**: primary key `(user_id, category)`. `in_app` defaults to true. `email` has no default.
- **Backfill:** every existing user gets the five default rows: email on for results and security only. It uses `ON CONFLICT DO NOTHING`, so it is safe to re-run. New users get the same rows at signup.
- `email_outbox` already existed (F02). It now also carries `result_pass`, `result_fail`, `result_ready` and `draft_reminder`.

## 6. Key decisions and deviations

**Answers to the plan's open questions.** You replied "go" without answering them, so I used the recommended options. Each is easy to change:

- **In-app off for a category:** the row is still stored, with `channels = {'email'}`. The feed, the count and SSE leave it out (`NotificationQueries.InAppFor`). This keeps a record that the email went out. If both channels are off, nothing is stored.
- **Security is locked on both channels.** The publisher ignores the stored row, and `PUT` returns 400 if security is set to off. The DDL comment only said "in-app forced on".
- **Security copy not in interfaces.md 5.6:**
  - "Your account was temporarily locked"
  - "Someone asked to change your email"
  - "Your email address was changed"

  The design's "approximate location" for new sign-ins is shown as the IP address, because there is no geo lookup.
- **Draft reminders for untimed sets** drop the "with {timeLeft} left" clause.

**Deviations and additions:**

- **`503 STREAM_UNAVAILABLE`:** a new error code and a new `AppErrorKind.Unavailable`. The design doesn't say what `stream-ticket` returns when Redis is down. The client treats 503 like a dropped stream and polls.
- **`NewNotification` was generalised.** F10's version was result-specific. It now carries type, category, text, data, an optional attempt and an optional email. `NotificationType` moved from Application to Domain.
- **F03's emails moved behind the publisher.** `SecurityEmails` became `SecurityNotifications`, and `AccountLockout.Lock` became `LockAsync`. The emails themselves are unchanged.
- **`data.weakest` holds category keys** (`["PI.PPT", …]`), as in api.md section 6, not names. The names are in the body.
- **The result deep link is `/attempts/{id}/results#breakdown`.** The results page gained the `#breakdown` anchor and scrolls to it once the data loads.
- **The preferences response has `locked` on every row** (false except security). The api.md example only shows it on security. This is an additive change.
- **PUT must send all five categories**, read literally from "Replace preferences".
- **Mark-read publishes a "read state changed" message**, so other tabs' badges drop too. The design only mentions new notifications on the channel.
- **Draft reminder deduplication uses the `notifications` table:** a reminder is created after `last_activity_at`. No new column was needed. The job runs hourly (`Notifications:DraftReminderCron`) after `Notifications:DraftReminderAfterDays` days (default 3), and handles at most 200 drafts per run.
- **The SSE endpoint is a minimal API endpoint** in `Nclex.Api/Sse`, as architecture.md section 4 lists. It is left out of the OpenAPI document because EventSource opens it, not the generated client. It sets `X-Accel-Buffering: no` and disables response buffering.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test           # 947 passed (Testcontainers Postgres and Redis)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # 239 passed, lint and typecheck clean
```

**Backend:**

- **Domain** (`tests/Nclex.Domain.Tests/Notifications`): copy and rounding, channel rules, default rows and the security lock, and the reminder policy.
- **Application:** the preferences validator, and the feed cursor round trip.
- **Integration** (`tests/Nclex.Api.IntegrationTests/Notifications`):
  - **Exactly once:** pass, fail and ready each produce exactly one notification and one email. A second submit adds nothing, and inserting a duplicate row breaks `notif_result_once`.
  - **Preferences respected:** email off means no outbox row; in-app off means an email-only row that the feed and count leave out; security ignores stored rows.
  - **Feed:** keyset paging, unread only, read and read-all, 404 for another user's notification, and bad queries.
  - **Preferences API:** the defaults, a full replace, and 400 for turning security off or leaving categories out.
  - **SSE:** two hosts on one Postgres and Redis stand in for two replicas. A result graded on replica B reaches a stream open on replica A, and mark-all-read on B drops A's badge. A ticket works once, a made-up ticket gets 401, and a rolled-back notification is never pushed. With Redis down, the ticket endpoint returns 503.
  - **Draft reminders:** a draft is reminded once; new activity earns another reminder; recent drafts and submitted attempts are left alone; and nothing is sent when reminders are switched off.
  - **Migration:** enum order, indexes, column types and defaults, and delete rules.

**Frontend** (Vitest, `src/features/notifications`):

- **Stream client:** the ticket URL, event dispatch, reconnecting with a new ticket, backoff timing and closing.
- **Bell:**
  - The badge comes from the API, then from SSE. It falls back to 60-second polling when the ticket is refused.
  - The panel opens with the latest items. Clicking a result marks it read and opens its deep link; links to other sites are not followed.
  - Mark all read clears the badge. Escape closes the panel and returns focus to the bell.
- **History:** load more, the unread filter and mark all.
- **Settings form:** the security row is disabled; saving sends all five categories; save errors are shown.

## 8. Follow-ups and known limitations

- **Not tried in a real browser.** I didn't run the app end to end. The SSE stream is tested through the in-memory test server, not through the Next.js dev rewrite proxy. Check step 3 above in a browser, and add an SSE scenario to the Playwright suite in F16.
- **Pre-existing flaky test:** `TestSetsTests.Pages_follow_the_cursor_without_repeating_sets` failed once in a full run. It pages the shared catalog with an offset cursor while other test classes import sets. F12's test sets are now titled to sort last, and two further full runs passed. The test can still flake whenever another test imports an RN set whose title sorts earlier.
- **Out of scope (as listed):** `flag_resolved` (F15), and streak and goal notifications (phase 2). The `content` and `engagement` categories already exist in preferences but nothing produces them yet.
- **Times in notification text** (the lock-until time) are UTC, as in the F03 emails. The user's time zone isn't used yet.
- **Location for new sign-ins:** only the IP address is shown; there is no geo-IP lookup.
- **One subscription per stream:** each open stream holds its own Redis subscription queue on the shared multiplexer. That is fine at the expected scale. A per-replica fan-out map would be the next step if replicas hold many thousands of streams.
- **Notification retention:** nothing deletes old notifications; consider a cleanup job.

## 9. Try it yourself

1. **New copy:** change the `result_ready` body in `NotificationCopy.ResultReady` to also say how many questions were answered. You will need `ItemsAnswered` from `AttemptResult` in `ResultNotifications.For`. Update `NotificationCopyTests` first and watch it fail.
2. **Make the reminder delay a user preference:** today it comes from configuration (`Notifications:DraftReminderAfterDays`). Follow `DraftReminderSettings` from `Nclex.Worker/Program.cs` to `SendDraftRemindersHandler`, and try a 1-day setting with the SQL in section 2, step 8.
3. **Show the newest notification as a toast:** in `useNotificationCenter`, `onNotification` currently only refetches the feeds. Keep the last notification in state and render a small `role="status"` message under the bell for five seconds. Write a test with `FakeEventSource.latest.emit("notification", aNotification())`.
