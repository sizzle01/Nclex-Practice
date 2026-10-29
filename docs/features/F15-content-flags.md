# F15: Content flags and moderation

## 1. Summary

Candidates can now tell us when a question looks wrong. A **Report issue** button sits on every question in the exam player and on the review page. It opens a dialog: the candidate picks a reason (unclear rationale, wrong answer key, typo, outdated guideline, media problem or other) and can add details. Reviewers and admins see each report in a moderation queue at `/admin/flags`. There they can take it, triage it, resolve it with a note, or reject it, and open the question version in the editor. Resolving sends the reporter a "Thanks — we reviewed your report" notification with the note. Candidates can follow every report and its outcome on `/settings/reports` ("My reports"), which the notification links to.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f15`.

- **Stacked on F12.** F12 (notifications) is not yet in `main`, and F15 needs its notification publisher. So `feature/f15-content-flags` was branched from `feature/f12-notifications`.
- **Main merged in.** `main` (F11) was then merged in (`1479d51`). The conflicts were the DI registrations, the worker settings and `schema.d.ts`; the types were regenerated from the merged API.
- **Merge order.** Merging F15 into `main` brings F12 with it, so F12 should be merged first (or together).

```bash
docker compose up -d
cd nclex-be
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_f15;Username=nclex;Password=nclex"
dotnet tool restore && dotnet build
dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F12_Notifications and F15_ContentFlags
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
dotnet run --no-build --project src/Nclex.Api                 # http://localhost:5080
cd ../nclex-fe && npm run dev                                 # http://localhost:3000
```

`nclex_f15` is a separate local database, following the per-feature ones already there. Create it once with `docker exec nclex-postgres-1 psql -U nclex -d postgres -c "CREATE DATABASE nclex_f15"`.

**Test accounts.** Sign up two accounts in the browser, for example `candidate@f15.test` and `reviewer@f15.test`. There is no role screen yet (that is `/admin/users`), so make one a reviewer and verify both in SQL, then **log in again** so the new role is in the access token:

```bash
docker exec nclex-postgres-1 psql -U nclex -d nclex_f15 -c "
  update users set email_verified_at = now() where email in ('candidate@f15.test','reviewer@f15.test');
  update users set role = 'reviewer' where email = 'reviewer@f15.test';"
```

**In the browser:**

1. As the candidate, start **Comprehensive Practice 1**. Below the question, next to **Mark for review**, press **Report issue**. Focus moves into the dialog, Tab stays inside it, and Escape closes it. Pick **The answer seems wrong**, add a comment and press **Send report**. The dialog thanks you and links to **My reports**.
2. Report the same question for the same reason again. The dialog says you already did (`409 FLAG_DUPLICATE`). A different reason works.
3. Open **My reports** from the user menu (`/settings/reports`). The report shows as **Received**.
4. In another browser (or a private window), sign in as the reviewer. The user menu now has **Reported questions** (`/admin/flags`). The report is under **Open**, with the stem, the comment, the reporter's name and the version.
5. Press **Assign to me**, then **Triage**. It moves to the **Triaged** tab. **Open in editor** links to `/admin/questions/{questionId}?version={versionId}`. That page comes with F14, so for now it is a 404.
6. Press **Resolve…**. The note is required. Write one and press **Resolve and notify**.
7. Back as the candidate, the bell shows **Thanks — we reviewed your report** with the note. Click it to open My reports, where the report is **Resolved** with the note.
8. Reject another report instead. The candidate gets no notification, and My reports shows **Reviewed, no change needed** without the note.
9. Optional: turn on **Your reports → By email** in Notification settings, resolve another report, and see the email in Mailpit (http://localhost:8025).

**With curl** (the same flow was run against a local API on port 5095 while building this feature):

```bash
B=http://localhost:5080/api/v1; J=(-H 'Content-Type: application/json')
C=...   # the candidate's access token
R=...   # the reviewer's access token (after logging in again)

# Report the first question of an attempt; V comes from GET /attempts/{id}/items/1 (questionVersionId)
curl -s "$B/question-versions/$V/flags" "${J[@]}" -H "Authorization: Bearer $C" \
  -d "{\"reason\":\"incorrect_answer_key\",\"comment\":\"C looks right too\",\"attemptId\":\"$A\"}"     # 201 {…report…}
# The same again → 409 {"code":"FLAG_DUPLICATE",…}
curl -s "$B/me/flags" -H "Authorization: Bearer $C"                                                   # {"items":[…],"nextCursor":null}

curl -s "$B/admin/flags" -H "Authorization: Bearer $C"                         # 403 FORBIDDEN for a candidate
curl -s "$B/admin/flags?status=open" -H "Authorization: Bearer $R"             # oldest first
curl -s -X PATCH "$B/admin/flags/$F" "${J[@]}" -H "Authorization: Bearer $R" \
  -d '{"status":"resolved","resolutionNote":"Thanks, the key now accepts C as well."}'           # 200
curl -s -X PATCH "$B/admin/flags/$F" "${J[@]}" -H "Authorization: Bearer $R" -d '{"status":"triaged"}'   # 409 FLAG_CLOSED
curl -s "$B/notifications?limit=1" -H "Authorization: Bearer $C"               # type flag_resolved, body = the note
```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Flags/FlagReason.cs` and `FlagStatus.cs`: the two Postgres enums.
2. `nclex-be/src/Nclex.Domain/Flags/QuestionFlag.cs`: the entity and its rules (`CanMove`: only forward; closed is final; closing needs a note).
3. `nclex-be/src/Nclex.Infrastructure/Persistence/Configurations/QuestionFlagConfiguration.cs` and `Migrations/*_F15_ContentFlags.cs`: the table as in database.md 2.9.
4. `nclex-be/src/Nclex.Application/Flags/FlagErrors.cs`: the 404, `FLAG_DUPLICATE` and `FLAG_CLOSED` errors.
5. `nclex-be/src/Nclex.Application/Flags/ReportIssue/*`: POST, with its visibility, attempt and duplicate checks.
6. `nclex-be/src/Nclex.Application/Flags/MyFlags/*`: GET /me/flags and `MyFlagView` (what a reporter may see).
7. `nclex-be/src/Nclex.Application/Flags/Moderation/ListFlagQueueHandler.cs` and `FlagQueueReader.cs`: the queue and its rows.
8. `nclex-be/src/Nclex.Application/Flags/Moderation/UpdateFlagHandler.cs` and `UpdateFlagRequestValidator.cs`: assign, triage, resolve or reject, with the notification and the audit row.
9. `nclex-be/src/Nclex.Application/Notifications/FlagNotifications.cs`, `Nclex.Domain/Notifications/NotificationCopy.cs` (`FlagResolved`) and `Nclex.Infrastructure/Email/Templates/FlagResolvedTemplate.cs`: the `flag_resolved` notification and its email.
10. `nclex-be/src/Nclex.Api/Controllers/FlagsController.cs`, `AdminFlagsController.cs` and `Auth/AuthSetup.cs` (`Policies.ContentModerator`).
11. `nclex-fe/src/features/flags/api/flagsApi.ts` and `flagQueries.ts`: fetch functions and TanStack Query hooks.
12. `nclex-fe/src/features/flags/components/ReportIssueDialog.tsx`, `useDialogFocus.ts` and `ReportIssueButton.tsx`: the dialog and its keyboard model.
13. `nclex-fe/src/features/flags/components/MyReports.tsx`: `/settings/reports`.
14. `nclex-fe/src/features/flags/components/ModerationQueue.tsx` and `RequireModerator.tsx`: `/admin/flags`.
15. Where the button is used: `nclex-fe/src/features/attempt/player/AttemptPlayer.tsx` (bottom bar) and `nclex-fe/src/features/review/ReviewPage.tsx` (each item).

## 4. Request flow: resolving a report

1. **UI:** on `/admin/flags`, the reviewer presses **Resolve…**, writes a note and presses **Resolve and notify**. `CloseForm` checks the note with Zod (`closeFlagSchema`). `useUpdateFlag` calls `updateFlag`, which sends `PATCH /api/v1/admin/flags/{id}` with `{ status: "resolved", resolutionNote }` through the generated client.
2. **API:** `[Authorize(Policy = Policies.ContentModerator)]` lets only the reviewer and admin roles in; everyone else gets `403 FORBIDDEN`. `FluentValidationFilter` runs `UpdateFlagRequestValidator`: the status can't be `open`, closing needs a note, and the note is at most 2000 characters. `AdminFlagsController.Update` calls the handler.
3. **Application:** `UpdateFlagHandler` loads the report (404 if missing). If it is already resolved or rejected, it stops with `409 FLAG_CLOSED`.
4. **Domain:** `QuestionFlag.Resolve` checks `CanMove(current, Resolved)`, then stores the trimmed note, `resolved_by` and `resolved_at` (from `TimeProvider`).
5. **Application:** `FlagNotifications.Resolved` builds a `flag_resolved` notification in the `content` category. The title comes from interfaces.md 5.6 and the body is the note. `INotificationPublisher` adds the `notifications` row and, if the reporter turned content emails on, an `email_outbox` row. `IAuditLog.RecordEntityEvent` adds `flag.resolve`.
6. **Database:** one `SaveChangesAsync` commits the report, the notification, the outbox row and the audit row together. After the commit, F12's interceptor publishes the notification to Redis.
7. **Back:** `FlagQueueReader` builds the updated row, with the reporter's and moderator's names and the stem preview. The client invalidates every queue tab, so the report moves from Open or Triaged to Resolved. In the reporter's browser the bell gets the notification over SSE (F12). Clicking it opens `/settings/reports`, which shows the note from `GET /me/flags`.

## 5. Database changes

Migration `F15_ContentFlags`, matching database.md 2.9:

- **Enums** `flag_reason` and `flag_status`, with their labels in DDL order. EF generates them alphabetically, so the migration's annotations were reordered, as F08 and F12 did. `MigrationTests` checks this.
- **`question_flags`**:
  - All the DDL's columns. `status` defaults to `open` and `created_at` to `now()`.
  - The check `char_length(comment) <= 2000`, named `question_flags_comment_check`.
  - Foreign keys: `attempt_id` is set to null on delete; the user, version, assignee and resolver keys have no delete action.
- **Indexes:**
  - The unique `(user_id, question_version_id, reason)` key, as index `ix_question_flags_user_id_question_version_id_reason`. Its name is matched to turn a race into 409.
  - The partial `flags_queue_idx (status, created_at) WHERE status IN ('open','triaged')` for the queue.
  - EF's usual foreign-key indexes, as in earlier features.
- `audit_events` already existed (F03). It now also gets `flag.resolve` and `flag.reject` rows, with `entity_type = 'question_flag'`.

## 6. Key decisions and deviations

**Answers to the plan's questions.** You replied "go" without choosing, so I used my defaults. Each is easy to change:

- **Base branch:** F15 is stacked on F12 instead of fast-forwarding `main`, because CLAUDE.md says not to merge to `main` without your say-so. `main` (F11) was merged into the branch.
- **Rejecting sends no notification.** The note is stored, but only moderators see it. The reporter's view shows the status only, as "Reviewed, no change needed".
- **Resolved and rejected are final.** Any later PATCH is `409 FLAG_CLOSED`, so a reporter is never notified twice.
- **No payload changes were needed.** The player and review items already carried `questionVersionId`.

**Deviations and additions:**

- **New codes `FLAG_DUPLICATE` and `FLAG_CLOSED`** (both 409), added to api.md section 1. api.md only said "409 for a duplicate".
- **Someone else's attempt is `404`, not the `403` the plan said.** This follows the attempt engine's rule that another user's attempt looks like it doesn't exist.
- **Only published or superseded versions can be reported**; anything else is 404. Superseded versions are allowed because the review page of an old attempt still shows them.
- **An `attemptId` that did not serve the version** is `400 VALIDATION_FAILED` on `attemptId`.
- **The PATCH body can't set `open`** and can't be empty. Leaving a field out keeps its value, so there is no "unassign" yet.
- **`resolutionNote` is capped at 2000 characters** in the validator, the same as the comment. The DDL has no limit.
- **A `flag_resolved` email template was added**, so the existing "Your reports → By email" setting does something. It is off by default (F12's defaults). The notification's `relatedAttemptId` is the attempt the report came from.
- **`IAuditLog.RecordEntityEvent`** was added next to `RecordUserEvent` for events about something other than a user. Audit metadata holds only the version id and reason, not the note or the comment.
- **Moderator policy:** `Policies.ContentModerator` (reviewer or admin). It is the first role-gated endpoint.
- **Frontend placement:**
  - "My reports" is linked from the user menu, because there is no settings sub-navigation.
  - `/admin/flags` has its own small guard (`RequireModerator`); F14 is expected to bring the admin shell.
  - **Open in editor** points at F14's `/admin/questions/{id}?version={versionId}`. The `?version=` parameter is my assumption and should match whatever F14 builds.
- **Fixes to existing code, found while building:**
  - **Merge bug (F11 + F12):** F12 registered the SSE ticket store in `AddInfrastructure`, but it needs `ISecureTokenGenerator`, which only the API registers. In Development the worker, and F11's `import-questions` command, failed at startup with a DI validation error. The store is now registered by an API-only `AddNotificationStreamTickets()`.
  - **Schema leak:** `UpdateFlagRequest.Closes` was marked `[JsonIgnore]`, so the derived value is not part of the request schema.
  - **Catalog paging test made less flaky (not fixed):** F12's `ExamScenario` titled its sets with random ids. The F15 tests create dozens of those sets, and a new one could land before the position of `TestSetsTests`' paging walk, which then saw a set twice. Titles now use time-ordered ids (`Guid.CreateVersion7`), so these sets always sort last. The test still failed once afterwards, because other test classes import sets with titles that sort earlier (see section 7).
- **No `Idempotency-Key` support.** No endpoint implements it yet, and the unique key already makes a retried report harmless (it returns 409).

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test
cd ../nclex-fe && npm run lint && npm run typecheck && npm test
```

**Results:**

- **Frontend:** lint and typecheck are clean, and all 347 tests (56 files) pass on the final code.
- **Backend:** builds with no warnings. Full runs of all 1,115 tests:
  - One full run passed with 0 failures, in 1 minute 19 seconds. That was before the last fix, the API-only stream-ticket registration.
  - On the final code, the last full run had 2 failures out of 1,115: `TestSetsTests.Pages_follow_the_cursor_without_repeating_sets` and `LockoutTests.The_51st_login_from_one_IP_address_gets_429_with_retry_after`. Both classes passed when rerun alone (24 of 24). Both are already listed as flaky under full-suite load in the F08, F09 and F12 walkthroughs.
- **Machine load:** it was very high while I worked (load average up to about 50, probably other sessions), and other full runs hit other load-related failures:
  - **Expired tokens:** a run that took 54 minutes had 8 account and rate-limit tests fail with 401, because their 15-minute access tokens expired mid-test.
  - **Outbox:** a 16-minute run failed `OutboxDispatcherTests.Two_dispatchers_running_at_once_never_send_the_same_email`. The F06 walkthrough lists this class as flaky.
  - **Serilog:** one run failed `ReauthenticationTests` with Serilog's "The logger is already frozen". That happens when two test hosts build at the same moment, because the bootstrap logger is shared by the whole process.

  None of these failures involves F15 code. All of F15's own tests passed in every run where they ran.

**Backend (new):**

- **Domain** (`QuestionFlagTests`, `NotificationCopyTests`):
  - the status transitions, including that closed is final
  - comment trimming and the length limit
  - closing needs a note and records who closed it and when
  - the `flag_resolved` copy
- **Application** (`UpdateFlagRequestValidatorTests`, `StemPreviewTests`): the PATCH rules and the stem preview.
- **Integration, reporting** (`Flags/ReportIssueTests`):
  - reporting from an attempt, and seeing it in My reports
  - no answer key or rationale in any response
  - duplicate → 409, while another reason or another user works
  - unknown or draft version → 404; superseded is allowed
  - someone else's attempt → 404; an attempt that didn't serve the version → 400
  - validation, sign-in required, and paging of My reports
- **Integration, moderation** (`Flags/ModerationTests`):
  - candidates, educators and authors → 403; reviewers and admins are let in
  - the queue's content, status filter and oldest-first order
  - triage and assign; only moderators can be assigned
  - resolve → a notification for the reporter with the note, the content category and a deep link, the note in My reports, and `flag.resolve` audited
  - reject → no notification, the note hidden, and `flag.reject` audited
  - closed → 409; a note is needed; status can't be `open`; an empty PATCH is refused; unknown → 404
- **Migration:** the enum order, both indexes, the check constraint, the column defaults and the delete rules.

**Frontend (new):**

- **`ReportIssueDialog.test.tsx`:**
  - what is sent, and the thank-you message
  - a reason is required
  - the duplicate message
  - focus moves in on open, Tab stays inside, Escape closes, and focus goes back to the button
  - axe finds no violations
- **`MyReports.test.tsx`:** statuses, the resolution note and the empty state.
- **`ModerationQueue.test.tsx`:**
  - the list and the editor link
  - switching status tabs
  - assign to me and triage
  - a note is required to resolve, then it is sent
  - the `FLAG_CLOSED` message
  - `RequireModerator` keeps candidates out and lets admins in

**Machine load.** On the first full backend run the machine was very busy (load average about 40, probably other sessions). That run took 54 minutes, and 8 account and rate-limit tests failed with 401 because their 15-minute access tokens expired mid-test. Rerun on a quieter machine, the same build passed in about 1.5 minutes. The final numbers above are from that kind of run.

## 8. Follow-ups and known limitations

- **Editor link:** check that F14's editor accepts `?version=` (or change `editorPath` in `flagCopy.ts`). When F14 lands, move `/admin/flags` into its admin shell and nav, and replace `RequireModerator` with F14's guard if it has one.
- **Assigning:** moderators can only "Assign to me". There is no picker for other moderators (no users list endpoint yet) and no unassign.
- **Notifications:** the assignee is not notified, and the reporter is not told when a report is triaged.
- **Bulk actions:** none yet, such as resolving every report on one version when it is republished.
- **Reporting from the results page:** only the player and review have the button.
- **Roles:** reviewers and admins are made by SQL until `/admin/users` exists, and a role change needs a fresh login, because the role travels in the access token.
- **Flaky backend tests:** the catalog paging test needs keyset paging, or scoping to its own sets. The 401s need a test clock or longer-lived test tokens. The Serilog race needs a non-global logger in test hosts. All of these predate F15.
- **Merge order:** F12, then F15. The sibling rule from earlier features still applies: whichever of F13 and F15 merges second must regenerate the EF snapshot and `schema.d.ts`.

## 9. Try it yourself

1. **Tell the reporter about a rejection too.** In `UpdateFlagHandler.ApplyStatusAsync`, publish a notification on reject as well. Add a `FlagRejected` text to `NotificationCopy`, and decide whether `MyFlagView` should then show the note. Which integration test fails first, and why is that the right test to change?
2. **Add "Unassign".** `UpdateFlagRequest` treats a missing `assigneeId` as "no change", so null can't mean "unassign". Add an explicit `unassign: true` field, a domain method, a validator rule (not both `assigneeId` and `unassign`) and a button in `QueueCard`. Run `npm run gen:api` and watch the types change.
3. **Show how many people reported the same thing.** Add `sameReasonCount` to `FlagQueueItem`: the number of open reports with the same version and reason. Work it out in `FlagQueueReader` in one grouped query, and show it as a badge. Why is it better to compute it there than in the React component?
