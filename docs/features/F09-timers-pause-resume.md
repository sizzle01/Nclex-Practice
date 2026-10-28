# F09: Server-authoritative timers, pause and resume

## 1. Summary

Timed sets now run on the server's clock. The exam timer starts the first time the player opens an attempt. While it runs, the server stores only a deadline; while paused, it stores the time left, so the countdown is the same across reloads and devices and a wrong local clock changes nothing. Candidates can take a **break** (an overlay hides the question) or **Save & Exit** from the ⋯ menu. Both freeze the timers and count against the set's pause limit, and in challenge mode time past `maxPauseSec` comes off the exam time. Expiry is enforced three ways: on every write (2 s grace), by a Hangfire job scheduled for the deadline, and by a sweeper every minute. So an attempt is finalized (`submitted`, `time_expired`, every item locked with its answer) even with the browser closed. Per-question timers lock their item when they run out. Grading is F10.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f09`. It is stacked on `feature/f08-attempt-engine` (see section 6).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # no new migration in F09
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"   # if not done yet

dotnet run --project src/Nclex.Api          # http://localhost:5080, Hangfire dashboard at /hangfire (Development only)
dotnet run --project src/Nclex.Worker       # runs the expiry jobs and the sweeper; needed for expiry with the browser closed
cd ../nclex-fe && npm run dev                # http://localhost:3000
```

Hangfire creates its own tables in the `hangfire` schema the first time it starts.

**In the browser (the "done when" check):**

1. Sign in as a verified user (see F08's walkthrough for verifying by Mailpit or psql). Start **Comprehensive Challenge 1** on `/practice`. The top bar shows `⏱ 1:20:00` counting down.
2. Reload the page, or open it in another browser. The time continues from the same deadline.
3. Press **Pause**. The question disappears behind "Test paused" and the timer reads "paused". Wait, then press **Resume**. This set has no pause limit, so no time was lost.
4. Open **⋯ → Save & Exit**. You land on the dashboard, and the draft keeps its time.
5. To see expiry with the browser closed, close the tab and move the deadline into the past:
   `docker exec nclex-postgres-1 psql -U nclex -d nclex -c "update attempts set exam_deadline_at = now() - interval '40 seconds' where status = 'in_progress'"`.
   Within a minute the worker logs `Expiry sweeper auto-submitted 1 attempts`. Reopening the attempt shows **Time is up**.

**With curl:**

```bash
B=http://localhost:5080/api/v1
TOKEN=...; ID=...   # as in F08: a verified user's token and a started attempt of the challenge set
H=(-H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-laptop")
curl -s -X POST $B/attempts/$ID/open "${H[@]}"                                   # timer.exam.running = true, deadlineAt set
curl -s -X POST $B/attempts/$ID/pause "${H[@]}" -H 'Content-Type: application/json' -d '{"reason":"break"}'   # status paused, remainingMs frozen
curl -s -o /dev/null -w '%{http_code}\n' $B/attempts/$ID/items/1 -H "Authorization: Bearer $TOKEN"   # 423 ATTEMPT_PAUSED
curl -s -X POST $B/attempts/$ID/resume -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-phone"   # running again, phone holds the lease
# after the deadline + 2 s, any write:
curl -s -X PUT $B/attempts/$ID/items/1/response "${H[@]}" -H 'Content-Type: application/json' -d '{"response":null,"clientSeq":1}'   # 409 ATTEMPT_TIME_EXPIRED
```

There are no new test accounts. No sample set has pause limits or per-question timers yet. To try those, import a set with `"pausePolicy": {"maxPauses": 2, "maxPauseSec": 60}` or `"perQuestionSec": 60` (the integration tests do this).

## 3. Reading order

**Domain** (`nclex-be/src/Nclex.Domain/`)
1. `Timing/Countdown.cs`: one countdown as a deadline (running) or remaining ms (stopped). Start, stop, remaining time, and expiry with the 2 s grace.
2. `Timing/AttemptTimer.cs`: the pause limit, the overrun rule, when the job runs and the sweeper cutoff.
3. `Entities/PauseReason.cs` and `Entities/AttemptWriteCheck.cs`: break / save_exit, and the new `TimeExpired`, `PauseOutcome` and `ResumeOutcome`.
4. `Entities/AttemptItem.cs`: the item timer (`Enter`, `StopTimer`, `ExpireTimer`). `Lock` now also stops the timer.
5. `Entities/Attempt.cs`: `Start` holds the budget; `Open` starts the timer; also `Pause`, `Resume`, `HasExamExpiredAt`, `ExpireItemTimer` and `Finalize`.

**Application** (`nclex-be/src/Nclex.Application/`)
6. `Abstractions/IJobScheduler.cs`: how use cases schedule a job without knowing about Hangfire.
7. `Attempts/AttemptFinalizer.cs`: the one place an attempt is finalized, with the F10 grading hook.
8. `Attempts/AttemptWriteGuard.cs`: the start of every write: lock, then expire the exam or the current item.
9. `Attempts/TimerState.cs`: the `TimerState` block, filled from the countdowns.
10. `Attempts/PauseAttempt/`, `Attempts/ResumeAttempt/`: the two new endpoints.
11. `Attempts/OpenAttempt/OpenAttemptHandler.cs`, `SaveResponse/`, `Navigate/`, `UpdateItemMarks/`, `DiscardAttempt/`: now go through the guard. Open and resume schedule the expiry job.
12. `Attempts/Expiry/ExpireAttemptHandler.cs` and `SweepExpiredAttemptsHandler.cs`: what the two jobs do.

**Infrastructure and hosts**
13. `Nclex.Infrastructure/Jobs/JobsServiceCollectionExtensions.cs`: Hangfire with Postgres storage, the client/server split and the recurring sweeper.
14. `Nclex.Infrastructure/Jobs/HangfireJobScheduler.cs`, `AttemptExpiryJob.cs`, `ExpirySweeperJob.cs`.
15. `Nclex.Api/Program.cs` (client and dev dashboard), `Nclex.Worker/Program.cs` (server and recurring job), and `Controllers/AttemptsController.cs` (`/pause`, `/resume`).

**Frontend** (`nclex-fe/src/features/attempt/`)
16. `player/attemptClock.ts`: clock offset, remaining time, formatting, and the 5 and 1 minute announcement rule.
17. `player/useAttemptSession.ts`: clock sync, the 1 s tick, pause, resume, Save & Exit, `ATTEMPT_TIME_EXPIRED`, and the check made at zero.
18. `player/TimerChips.tsx`, `PauseOverlay.tsx`, `AttemptMenu.tsx`: the new pieces of the screen.
19. `player/AttemptPlayer.tsx`: how they fit in the top bar, and the "Time is up" screen.

## 4. Request flow: pausing for a break (`POST /api/v1/attempts/{id}/pause`)

1. **UI.** The candidate presses **Pause**. `useAttemptSession.pause("break")` flushes the save queue first, so no answer is left unsent, then calls `pauseAttempt` with the `X-Attempt-Client` header.
2. **API.** `AttemptsController.Pause` binds `{ "reason": "break" }` (snake_case enum). `PauseAttemptRequestValidator` requires the reason.
3. **Application.** `PauseAttemptHandler` opens a transaction.
   - `AttemptWriteGuard.LockAndApplyTimersAsync` runs `SELECT … FOR UPDATE`. If the exam timer is more than 2 s past its deadline, it finalizes the attempt through `AttemptFinalizer`, commits, and returns 409 `ATTEMPT_TIME_EXPIRED`. If the current item's timer has run out, it locks that item.
   - A repeat pause from the device that already paused returns the same state without using another pause.
   - `EnsureWritable` checks the lease and status (409 `ATTEMPT_OPENED_ELSEWHERE`, 423 `ATTEMPT_PAUSED`, …).
4. **Domain.** `Attempt.Pause` returns `LimitReached` (409 `PAUSE_LIMIT_REACHED`) if `AttemptTimer.CanPause` says no. Otherwise:
   - it stops counting time on the item and freezes the item timer;
   - it turns the exam deadline into `exam_remaining_ms` (`Countdown.StopAt`);
   - it sets `paused`, `pause_reason` and `pause_started_at`, counts the pause, and increments `timer_epoch`.
5. **Database.** One `UPDATE attempts` and one `UPDATE attempt_items`. The `CHECK (status <> 'paused' OR exam_deadline_at IS NULL)` holds. The job that was scheduled for the old deadline is still in Hangfire, but it carries the old epoch, so it will do nothing.
6. **Back.** 200 `{ status: "paused", timer }`. The hook stores it in the attempt query. `status === "paused"` disables the item query (the server would answer 423 anyway) and shows `PauseOverlay` with focus on **Resume**.
7. **Resume.** `POST /resume` subtracts `max(0, paused − maxPauseSec)` (`AttemptTimer.AfterPause`), increments the epoch, takes the lease, starts the timers, and schedules a new job for `deadline + 3 s`. If nothing is left, it finalizes and returns `ATTEMPT_TIME_EXPIRED`.

## 5. Database changes

No migration: F08's `F08_Attempts` already created every timer and pause column and `attempts_expiry_idx`. `dotnet ef migrations has-pending-model-changes` reports none.

- `attempts.pause_reason` stays `text` with its CHECK; EF now converts it to and from the `PauseReason` enum (`break`, `save_exit`).
- Columns now written: `exam_deadline_at`, `exam_remaining_ms`, `timer_epoch`, `pauses_used`, `pause_started_at`, `pause_reason`, `total_paused_ms`, `submitted_at`, `submit_reason`, `attempt_items.item_deadline_at`, `item_remaining_ms`, `locked_at`.
- `attempts_expiry_idx (exam_deadline_at) WHERE status = 'in_progress'` now serves the sweeper's query.
- **Hangfire** creates its own tables in a separate `hangfire` schema on first start, outside the EF migrations.

## 6. Key decisions and deviations

- **Branch base.** Stacked on `feature/f08-attempt-engine`, as you chose. F07 is still not on this branch.
- **Your decisions on the plan questions:**
  - Drafts never expire: a paused attempt has no deadline, so neither the job nor the sweeper touches it. This settles the disagreement between architecture.md 5.1 and the 5.3 state diagram ("paused → submitted: sweeper") in favour of 5.1.
  - Save & Exit follows the same rules as a break: it counts against `maxPauses`, and time past `maxPauseSec` comes off the exam time on resume.
  - An overrun that leaves 0 ms or less finalizes the attempt as `time_expired` on resume, and the call returns 409 `ATTEMPT_TIME_EXPIRED`.
  - Item timers have no Hangfire job. An expired item is locked on the next write (save, mark, navigate, open, pause, discard), and the player disables it at zero.
- **"Auto-submit at zero" without a submit endpoint.** `POST /submit` is F10. At zero the player disables input, flushes queued answers, waits 2.5 s (the server's grace), then sends one write (`navigate` to the current question). The server finalizes the attempt and answers `ATTEMPT_TIME_EXPIRED`, and the player shows **Time is up** with a link to the dashboard. If the server says time remains (because this device's clock offset was off), the player refreshes its state. F10 should replace this with `submitAttempt()` and route to `/attempts/[id]/results`.
- **The job runs 3 s after the deadline** (`deadline + grace + 1 s`), so the job and the write rule agree that time is up. A job that runs early does nothing, and the sweeper covers it.
- **The job checks both the epoch and the deadline.** The epoch makes stale jobs no-ops, as architecture.md says. The deadline check makes the job safe even if it is scheduled twice (for example, open on two devices).
- **`submit_reason = time_expired` maps to `ATTEMPT_TIME_EXPIRED` on every later write**, not `ATTEMPT_NOT_ACTIVE`. That way the player routes the same way whichever path finalized the attempt.
- **Discarding an attempt whose time ran out** finalizes it and returns `ATTEMPT_TIME_EXPIRED`, so its results are kept.
- **Pause is idempotent for the device that paused.** A retried request doesn't use a second pause. Resuming an attempt that is already running behaves like `/open`: it takes the lease.
- **Reopening doesn't give item time back.** F08 cuts the time-on-item count back to the last activity when a tab is reopened. The item countdown keeps running instead, like the exam timer, so reloading can't buy extra time on a question.
- **Item time counts up to the item deadline** when an expired item is locked, not up to when the expiry was noticed.
- **Hangfire wiring.**
  - The API registers Hangfire as a client only. The worker runs the server and registers the sweeper as `attempt-expiry-sweeper` (`Cron.Minutely()`).
  - The dashboard is mapped only in Development, and Hangfire's default filter allows local requests only.
- **New packages, all within the stack:** `Hangfire.Core`, `Hangfire.NetCore`, `Hangfire.AspNetCore` 1.8.25 and `Hangfire.PostgreSql` 1.21.1. `Newtonsoft.Json` 13.0.4 is pinned as a transitive version only, because Hangfire.Core would otherwise pull in 11.0.1, which has a high-severity advisory and fails the warnings-as-errors build.
- **Announcements.** The exam timer is announced at 5 and 1 minutes; the question timer at 1 minute ("… for this question"). Both use a polite live region. The chips are `role="timer"`, which screen readers don't read as they change. If a tab was asleep across both thresholds, only "1 minute remaining" is spoken.
- **`timeRemainingMs`** in the drafts list is now worked out from the deadline at read time.
- **Test-only change.** `ImportQuestionsTests` now imports through `CatalogContent`'s lock. It was the class still racing the importer's check-then-insert of its system user (`ix_users_email`), and F09's extra imports made that race happen on every full run.

## 7. Tests

| Where | What |
|---|---|
| `Nclex.Domain.Tests/Timing/CountdownTests.cs` | Start, stop and restart; remaining time from the clock; no negative times; **grace boundary at 2 000 / 2 001 ms**; a stopped countdown never expires |
| `Nclex.Domain.Tests/Timing/AttemptTimerTests.cs` | Pause limit table, including unlimited and 0; **overrun table**; `AfterPause` formula; overrun past the time left gives 0; job time; sweeper cutoff |
| `Nclex.Domain.Tests/Entities/AttemptTimingTests.cs` | Budget at start; timer starts on first open and keeps its deadline on takeover; grace; pause freezes and increments the epoch; **paused drafts never expire**; **break and Save & Exit share the limit**; unlimited practice pauses; resume restarts; **overrun deducted for break and Save & Exit**; overrun to exactly 0 is expired; **epoch 0 → 1 → 2**; item timers (start, freeze, continue, expire with grace and keep the answer, time capped at the deadline, lock on return with 0 left, pause and resume, no refund on reopen, no expiry while paused); finalize |
| `Nclex.Application.Tests/Attempts/AttemptRequestValidatorTests.cs` | Pause reason required; both reasons accepted |
| `Nclex.Api.IntegrationTests/Attempts/AttemptTimerTests.cs` (manual clock, recording scheduler) | Open starts the timer and schedules the job; untimed attempts schedule nothing; countdown across reads and the drafts list; same deadline on another device; **write inside the grace saved, after it finalized with every item locked**; expiry on open with the browser closed; discard after expiry; **delayed job early vs on time**; the job class; **stale job after pause/resume does nothing while the new one finalizes**; **sweeper: overdue yes, 20 s late no, paused no**; pause hides content (423) and freezes time; repeat pause; **shared pause limit (409)**; reason and lease checks; resume on another device with a new job (epoch 2); overrun deducted; **Save & Exit overrun finalizes on resume**; paused draft survives 40 days and the sweeper; resume while running; **item timer locks with its answer, ITEM_LOCKED, next item starts**; item timer frozen while away; **real Hangfire storage holds the scheduled job** |
| `nclex-fe/.../player/attemptClock.test.ts` | Offset with a clock 4.5 hours fast; counting down offline; frozen times; formatting; spoken text; announcement thresholds |
| `nclex-fe/.../player/TimerChips.test.tsx` | `timer` roles and labels; untimed; "time low" and "paused" words; **announces at 5 and 1 minutes only**, not on first render; question timer |
| `nclex-fe/.../player/PauseOverlay.test.tsx` | Dialog, description, focus on Resume, Enter; error; pauses-left text |
| `nclex-fe/.../player/AttemptPlayerTimers.test.tsx` | **Wrong local clock still shows server time** and ticks; pause overlay hides the question, then resume; opening paused loads no item; **Save & Exit from the ⋯ menu** with keyboard; Escape returns focus; PAUSE_LIMIT_REACHED message; No pauses left; **at zero: inputs disabled, waits out the grace, then the Time is up screen**; expired on open; question timer locks inputs |

Run them:

```bash
cd nclex-be && dotnet build && dotnet test      # Docker must be running
cd nclex-fe && npm run lint && npm run typecheck && npm test
```

Results on this branch:
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet test`: 736 passed, 0 failed (82 new).
- `npm run lint` and `npm run typecheck`: clean.
- `npm test`: 167 tests in 31 files passed (37 new).

I also ran the flow against the real API and worker on a scratch database (`nclex_f09`), so the shared dev database stayed untouched:
- open started an 80-minute deadline;
- pause froze it, and item GET returned 423;
- resume on a second client id moved the deadline by the pause length;
- Hangfire held two scheduled jobs, for epochs 0 and 2;
- with no client connected, I moved the deadline 40 s into the past, and the worker's sweeper finalized the attempt within a minute (`submitted`, `time_expired`, 40 items locked);
- the next save and open both returned 409 `ATTEMPT_TIME_EXPIRED`;
- `/hangfire` served the dashboard;
- the Next dev server served `/attempts/[id]`, `/dashboard` and `/practice`.

## 8. Follow-ups and known limitations

- **F10:**
  - replace the zero-time check with `POST /submit` and route to `/attempts/[id]/results`;
  - grade at the `AttemptFinalizer` hook;
  - show the submit confirmation.
- **The dashboard doesn't show time left on drafts yet**, although `timeRemainingMs` is now correct (interfaces.md 5.2 lists it for `/dashboard`).
- **Hangfire dashboard access** is local-only in Development. If it's ever needed elsewhere, it needs an admin-role authorization filter.
- **The sweeper finalizes every overdue attempt in one run**, each in its own transaction. Fine at current volumes; batch it if it ever lags.
- **The clock offset is synced from `/open`, `GET /attempts/{id}`, navigate, pause and resume**, not from autosave responses (the save queue doesn't pass its responses on).
- **No sample set has pause limits or per-question timers.** Content could add one, to show those rules in the demo.
- **A flaky catalog test:** `TestSetsTests.Pages_follow_the_cursor_without_repeating_sets` failed once in a full run. The catalog uses offset cursors over the shared test database, so a set another class imports mid-walk shifts the pages. It's more likely now that more test classes import sets. Keyset paging, or scoping the test to its own sets, would fix it.
- **The importer's check-then-insert of its system user** is still racy outside the tests. An upsert would fix it (also noted in F08).

## 9. Try it yourself

1. **Change the grace.** Set `AttemptTimer.WriteGrace` to 5 seconds. Which domain and integration tests fail, and why does `ExpiryJobAt` need no change? What does the frontend's `serverGraceMs` in `useAttemptSession.ts` have to become?
2. **Announce 10 minutes too.** Add `10 * 60_000` to `announcedAtMs` in `attemptClock.ts` and make `crossedAnnouncement` say "10 minutes remaining". Extend `attemptClock.test.ts` and `TimerChips.test.tsx` first, and check that a big jump (400 s to 30 s) still speaks only the smallest threshold.
3. **Refuse breaks over the limit instead of charging for them.** Write a failing test in `AttemptTimingTests.cs` for a rule like "a break longer than `maxPauseSec` finalizes the attempt". Make it pass in `Attempt.Resume`, then decide whether the rule belongs in `AttemptTimer` (pure) or the entity, and what api.md would have to say.
