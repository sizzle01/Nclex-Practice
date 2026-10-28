# F08: Attempt engine (untimed)

## 1. Summary

Candidates can now take a test. **Start** on `/practice` creates an attempt, or resumes the draft if one already exists for that set, and opens the player at `/attempts/[id]`. The player shows one question at a time with Previous/Next, a navigator of all questions, and Mark for review. Answers autosave about 750 ms after each change: they're queued per question, numbered with a rising `clientSeq` so the server can ignore late retries, and kept in `localStorage` while offline. Only one device writes at a time. Opening the attempt on a phone takes the **device lease** from the laptop, and the laptop then shows "This attempt is open on another device" with a **Continue here** button. The dashboard lists tests in progress with their % complete. A draft untouched for 30 days asks "resume or discard?". Timers and pause come in F09; submitting and grading come in F10.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f08`. It was branched from `feature/f06-test-sets` (see section 6).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F08_Attempts
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"   # if not done in F06

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm run dev                # http://localhost:3000
```

**In the browser (the "done when" check):**

1. Sign up at http://localhost:3000/signup with track RN.
2. Verify the email: open the link in Mailpit (http://localhost:8025), or run
   `docker exec nclex-postgres-1 psql -U nclex -d nclex -c "update users set email_verified_at = now() where email = '<you>'"`.
   Unverified accounts can only start the **RN Sample Set**.
3. On `/practice`, open **Comprehensive Practice 1** and press **Start**. Answer a few questions and move with Next, Previous and "All questions".
4. Close the tab. Open the same URL in another browser (or a private window) and sign in. The player opens at the same question, with your answers restored. The dashboard lists the test under "Continue where you left off".
5. To see the takeover, keep both windows open. Answer something in the window you opened first. After the debounce it shows the "open on another device" dialog. **Continue here** takes the lease back.

**With curl** (two "devices" are just two `X-Attempt-Client` values):

```bash
B=http://localhost:5080/api/v1
TOKEN=...   # accessToken from POST $B/auth/login, for a verified user
SET=$(curl -s "$B/test-sets?kind=prebuilt" -H "Authorization: Bearer $TOKEN" | python3 -c "import json,sys;print(json.load(sys.stdin)['items'][1]['id'])")

ID=$(curl -s -X POST $B/attempts -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "{\"testSetId\":\"$SET\"}" | python3 -c "import json,sys;print(json.load(sys.stdin)['id'])")     # 201 (200 + same id the 2nd time)
curl -s -X POST $B/attempts/$ID/open -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-laptop"
curl -s $B/attempts/$ID/items/1 -H "Authorization: Bearer $TOKEN"                                     # no key, no rationale
curl -s -X PUT $B/attempts/$ID/items/1/response -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-laptop" \
  -H 'Content-Type: application/json' -d '{"response":{"selected":"a"},"clientSeq":1}'
curl -s -X POST $B/attempts/$ID/open -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-phone"   # takeover
curl -s -X PUT $B/attempts/$ID/items/1/response -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-laptop" \
  -H 'Content-Type: application/json' -d '{"response":null,"clientSeq":2}'                            # 409 ATTEMPT_OPENED_ELSEWHERE
curl -s "$B/attempts" -H "Authorization: Bearer $TOKEN"                                                # drafts with answeredItems
curl -s -X DELETE $B/attempts/$ID -H "Authorization: Bearer $TOKEN"                                   # 204
```

There are no new test accounts.

## 3. Reading order

**Domain** (`nclex-be/src/Nclex.Domain/Entities/`)
1. `AttemptStatus.cs`, `ItemState.cs`, `SubmitReason.cs`: the three new enums.
2. `AttemptSettings.cs`: the snapshot of the set's settings taken at start.
3. `AttemptWriteCheck.cs`: outcomes of the write rules (allowed / not active / paused / opened elsewhere), plus `SaveOutcome` and `NavigationOutcome`.
4. `AttemptItem.cs`: one served question. Covers autosave with the `clientSeq` rule, marks, time on the item, and locking.
5. `Attempt.cs`: the attempt. Covers start, the lease (`Open`, `CheckWrite`), navigation with the back-nav rule, discard, and stale drafts.

**Infrastructure**
6. `Persistence/Configurations/AttemptConfiguration.cs` and `AttemptItemConfiguration.cs`: the mapping. Settings are stored as JSON, and the DDL indexes keep their names.
7. `Persistence/AppDbContext.cs`, `LockAttemptAsync`: the `SELECT … FOR UPDATE`.
8. `Persistence/Migrations/*_F08_Attempts.cs`: the migration.

**Application** (`nclex-be/src/Nclex.Application/Attempts/`)
9. `AttemptErrors.cs` and `AttemptClient.cs`: the error codes, and the `X-Attempt-Client` header check.
10. `AttemptStore.cs`: loads the user's own attempt (with or without the row lock), and maps write checks to errors.
11. `AttemptState.cs`, `TimerState.cs`, `AttemptStateReader.cs`: the `AttemptState` response.
12. `StartAttempt/StartAttemptHandler.cs`: verified-email, plan-tier and draft-reuse rules, and resolving question versions.
13. `OpenAttempt/OpenAttemptHandler.cs`: claims the lease.
14. `GetItem/GetItemHandler.cs` and `CandidateItemView.cs`: what a candidate may see.
15. `SaveResponse/SaveResponseHandler.cs`: the autosave.
16. `Navigate/`, `UpdateItemMarks/`, `DiscardAttempt/`, `ListAttempts/`, `GetAttempt/`: the remaining endpoints.

**API**
17. `Nclex.Api/Controllers/AttemptsController.cs`: thin endpoints for api.md section 4.

**Frontend** (`nclex-fe/src/`)
18. `features/attempt/api/attemptApi.ts`: typed calls and query keys.
19. `features/attempt/player/saveQueue.ts`: the save queue (debounce, coalescing, clientSeq, retry, `localStorage` mirror).
20. `features/attempt/player/useAttemptSession.ts`: open/lease, item loading and prefetch, autosave, navigation, marks, page events.
21. `features/attempt/player/AttemptPlayer.tsx`, `NavigatorDrawer.tsx`, `OpenElsewhereDialog.tsx`, `SaveStatusText.tsx`: the player screen.
22. `features/attempt/renderers/ItemAnswer.tsx`, `MultipleChoiceRenderer.tsx`, `SataRenderer.tsx`: the renderers.
23. `app/attempts/[id]/page.tsx`: the route.
24. `features/practice/components/TestSetDetailPanel.tsx`: the Start button, now enabled.
25. `features/dashboard/components/DraftsList.tsx`: the drafts list on the dashboard.

## 4. Request flow: autosave (`PUT /api/v1/attempts/{id}/items/{position}/response`)

1. **UI.** The candidate clicks an option. `MultipleChoiceRenderer` calls `onChange({ selected: "b" })`. `useAttemptSession.answer` updates the cached item, so the radio is checked straight away, and the navigator cell. It then calls `saveQueue.enqueue(position, response)`.
2. **Save queue.** `enqueue` replaces any unsent write for that position and gives it the next `clientSeq`, which is always above the item's `clientSeq` from `GET …/items/{n}`. It mirrors the pending writes to `localStorage` and restarts the 750 ms timer. When the timer fires, `flush` sends each pending write through `saveResponse` with the `X-Attempt-Client` header. Navigation, a hidden tab, `pagehide` (with `keepalive`) and coming back online also flush.
3. **API.** `AttemptsController.SaveResponse` binds the body and the header. `FluentValidationFilter` runs `SaveResponseRequestValidator` (`clientSeq > 0`).
4. **Application.** `SaveResponseHandler` requires the header, then opens a transaction.
   - `AttemptStore.LockOwnedAsync` runs `SELECT * FROM attempts WHERE id = … FOR UPDATE` and returns 404 if the attempt isn't the caller's.
   - `EnsureWritable` returns 409 `ATTEMPT_NOT_ACTIVE`, 423 `ATTEMPT_PAUSED` or 409 `ATTEMPT_OPENED_ELSEWHERE`.
   - It loads the item and validates the response with the item type's `ValidateResponse` (400 `VALIDATION_FAILED`, with `errors[].field` such as `response.selected`).
5. **Domain.** `AttemptItem.SaveResponse` returns `ItemLocked` (409 `ITEM_LOCKED`) or `Stale` (`clientSeq` ≤ the stored value: nothing changes, still 200). Otherwise it returns `Saved`: it sets the response, `client_seq`, the state (`answered`, or `seen` when cleared) and `answered_at`. `Attempt.RecordActivity` moves `last_activity_at`.
6. **Database.** `SaveChanges` sends one `UPDATE attempt_items` and one `UPDATE attempts`, and the commit releases the row lock.
7. **Back.** 200 `{ savedAt, clientSeq, state, timer }`. The queue removes the write unless a newer one arrived meanwhile, clears `localStorage` once nothing is pending, and shows "✓ Saved". If the network fails it shows "Offline – answers are saved on this device and will sync" and retries at 0.5 s, 1 s, 2 s, 4 s, then every 5 s. A 409 `ATTEMPT_OPENED_ELSEWHERE` stops the queue, keeps the writes, and opens the takeover dialog.

## 5. Database changes

Migration `F08_Attempts`, matching database.md 2.1 and 2.6:

- **Enums:** `attempt_status` (`in_progress, paused, submitted, abandoned`), `submit_reason` (`user, time_expired, cat_stop`) and `item_state` (`unseen, seen, answered, locked`). The labels are in DDL order; the migration reorders EF's alphabetical list, as F06 did.
- **`attempts`:** every DDL column, including the timer, pause and `cat_state` columns that F09 and phase 2 will use.
  - Defaults: `status 'in_progress'`, `current_position 1`, `timer_epoch 0`, `pauses_used 0`, `total_paused_ms 0`, `started_at` / `last_activity_at now()`.
  - CHECKs: `attempts_pause_reason_check`, `attempts_check` (submitted ⇒ `submitted_at`), `attempts_check1` (paused ⇒ no deadline).
  - FKs to `users` and `test_sets`.
- **`attempt_items`:** every DDL column.
  - PK `(attempt_id, position)`, cascade delete from `attempts`, FK to `question_versions`.
  - Defaults: `state 'unseen'`, `client_seq 0`, `flagged false`, `struck_options '[]'`, `time_spent_ms 0`.
- **Indexes from the DDL, with their DDL names:**
  - `attempts_drafts_idx (user_id, last_activity_at DESC) WHERE status IN ('in_progress','paused')`: the dashboard list.
  - `attempts_history_idx (user_id, submitted_at DESC) WHERE status = 'submitted'`: history (F10).
  - `attempts_expiry_idx (exam_deadline_at) WHERE status = 'in_progress'`: the expiry sweeper (F09).
- EF also adds its usual FK indexes, `ix_attempts_test_set_id` and `ix_attempt_items_question_version_id`, as F06 did for `test_set_items`.

## 6. Key decisions and deviations

- **Branch base.** F01 to F06 are unmerged and F07 hasn't been built, so this branch starts from `feature/f06-test-sets`.
- **The builder's `startNow` is not wired.** The builder is F07. `StartAttemptHandler` takes a set id, so F07 can call it right after creating the custom set. (You said "go" without answering the four plan questions, so I went with my suggestion on each; this and the next three bullets are those.)
- **No `reveal` field in the item response yet.** api.md's item shape has `reveal` (key, grade, rationale). Until F10, `CandidateItemView` leaves the field out entirely, rather than shipping `reveal: null` with an answer-key type in the candidate schema. Adding it later is an additive change.
- **`DELETE /attempts/{id}` needs no lease.** The dashboard discards drafts without opening the player. A player still open elsewhere gets `ATTEMPT_NOT_ACTIVE` on its next write. Discarding twice returns 204; discarding a submitted attempt returns 409.
- **No `Idempotency-Key` on `POST /attempts` yet.** Starting a prebuilt set again already returns the existing draft.
- **The row lock lives in Infrastructure.** `IAppDbContext.LockAttemptAsync` hides the raw SQL. Autosave then does a tracked update under that lock instead of the `UPDATE … WHERE client_seq < $n` sketch in architecture.md 5.2. The effect is the same, because the lock serialises writes per attempt, and the rule stays in the domain where it's unit-tested.
- **Unverified users.** The `VerifiedEmail` policy from F02 can't be put on the route, because sample sets are allowed. The start handler checks `email_verified_at` itself.
- **Which questions are served.** At start, each set item resolves to its question's current published version. Retired questions are skipped, so `totalItems` can be below the set's `questionCount`. A set with nothing left to serve returns 404 `NOT_FOUND`.
- **Time per item.**
  - Navigation adds the time on the item being left to its `time_spent_ms`.
  - On `/open` (a takeover or a reload), the item the previous device left open is counted only up to `last_activity_at`, so hours with a tab closed don't count.
  - Locked items don't collect time.
- **Locked items are never written.** Autosave and `PATCH` (flag, strike) on a locked item return 409 `ITEM_LOCKED`, following the database.md invariant.
- **Positions.** A `toPosition` beyond the attempt is 400 `VALIDATION_FAILED` on `toPosition`. An item position that doesn't exist is 404. GET on an item of a discarded attempt is 409 `ATTEMPT_NOT_ACTIVE`; of a paused one, 423 `ATTEMPT_PAUSED`.
- **`GET /attempts` shape.** api.md lists the endpoint but not its rows, so I defined `AttemptSummary`: `id, testSet, status, mode, totalItems, answeredItems, startedAt, lastActivityAt, submittedAt, staleDraft, timeRemainingMs`. `status` takes a comma list and defaults to drafts. Rows are ordered by last activity. Paging reuses F06's `PageCursor`.
- **Timers are null.** Every attempt runs untimed in F08 (`timer.exam` and `timer.item` are `null`), including the challenge set. F09 fills them in.
- **Settings JSON** uses camelCase keys and enum names as written in C# (`"mode": "Practice"`). It's internal; the API always returns snake_case enums.
- **Save queue details.**
  - The `pagehide` flush uses `fetch(…, { keepalive: true })`, not `sendBeacon`, because a beacon can't carry the bearer token or the lease header.
  - The queue reads writes left in `localStorage` when it's created and sends them after `/open`, so a new answer can never overwrite them first.
  - Only answers are stored on the device, never tokens.
- **Discard on the dashboard** is offered only on stale drafts, as the "resume or discard?" prompt, and asks for confirmation inside the page.
- **There is no Submit button.** On the last question the player says submitting arrives later (F10).
- **Integers from the generated client** are typed `number | string`. `lib/format/toNumber.ts` narrows them; F06 has its own copy in `practice/format.ts`.
- **Test-only change to `CatalogContent`.** Imports now run one at a time. The F04 importer creates its system user with a check-then-insert, and two test classes importing at the same moment on a fresh database hit `ix_users_email`. The real import command never runs concurrently.

## 7. Tests

| Where | What |
|---|---|
| `Nclex.Domain.Tests/Entities/AttemptTests.cs` | Start, lease rules, discard (and twice), open marks seen, time on navigation and across visits, **takeover time capped at last activity**, back-nav refusal and locking, stale after 30 days, `clientSeq` stale rule, null clears, locked item keeps answer and marks |
| `Nclex.Application.Tests/Attempts/AttemptRequestValidatorTests.cs` | Status filter parsing, limits, `clientSeq ≥ 1`, positions, struck options, `X-Attempt-Client` required and length |
| `Nclex.Api.IntegrationTests/Attempts/StartAttemptTests.cs` | 201 with untimed state and Location, **draft reuse (200)**, new attempt after discard, **unverified users only on sample sets**, **premium tier**, unknown and unpublished sets, retired questions skipped, 400 |
| `Nclex.Api.IntegrationTests/Attempts/AttemptSessionTests.cs` | **Lease takeover** (writes before open, after takeover, Continue here), missing header, autosave restore, **stale clientSeq ignored**, null clears, invalid response 400, marks, navigation states, out-of-range positions, **back-nav and locked items**, **another user's attempt is 404 everywhere**, discarded attempt, **answer-key secrecy** (exact field list; no key, rationale, objective or "correct"), stable option order, drafts list and filters |
| `Nclex.Api.IntegrationTests/MigrationTests.cs` | Enum label order, the three partial indexes and PK, CHECKs, DDL defaults, cascade delete |
| `nclex-fe/.../player/saveQueue.test.ts` | Debounce and coalescing, numbering above the server's seq, flush with keepalive, offline mirror and backoff, the 5 s cap, picking up earlier unsent writes, lease lost keeps writes, refused writes dropped, a newer answer kept while one is in flight |
| `nclex-fe/.../renderers/ItemAnswer.test.tsx` | MC radiogroup and arrow keys, SATA group with description, content order and Space, clearing, locked, unsupported types |
| `nclex-fe/.../player/AttemptPlayer.test.tsx` | Open with a client id and focus on the stem, restore and autosave with the lease header, sending answers left by an earlier visit, Next, the **takeover dialog** with Continue here reusing the client id, an attempt that can't be opened |
| `nclex-fe/.../player/OpenElsewhereDialog.test.tsx` | `alertdialog`, focus, Enter, busy state |
| `nclex-fe/.../dashboard/components/DraftsList.test.tsx` | % complete, Resume link, empty state, stale prompt and confirmed discard |
| `nclex-fe/.../practice/components/TestSetDetailPanel.test.tsx` | Start opens the player; the EMAIL_NOT_VERIFIED message; locked sets keep Start disabled |

Run them:

```bash
cd nclex-be && dotnet build && dotnet test      # see the result below (Docker must be running)
cd nclex-fe && npm run lint && npm run typecheck && npm test
```

Results on this branch:
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet test`: 654 passed, 0 failed.
- `npm run lint` and `npm run typecheck`: clean.
- `npm test`: 130 tests in 27 files passed.

I also ran the "done when" flow against a running API, on a separate scratch database (`nclex_f08`) so the shared dev database stayed untouched:
- start returned 201;
- the laptop opened the attempt and answered;
- the phone opened it and saw the answer (`answered`, seq 1);
- the laptop's next write got 409 `ATTEMPT_OPENED_ELSEWHERE`;
- Continue here returned 200 and saves worked again;
- the drafts list showed 1/40;
- starting again returned 200 with the same draft;
- the item JSON held no key or rationale.

The Next.js dev server served `/attempts/[id]`, `/dashboard` and `/practice` without errors.

## 8. Follow-ups and known limitations

- **F07:** call `StartAttemptHandler` for `startNow`. Draft reuse applies only to prebuilt sets, so every custom set gets a new attempt.
- **F09:** fill `TimerState` from the deadline columns, set `exam_remaining_ms` at start for timed sets, pause and resume (the `ATTEMPT_PAUSED` paths are already in place), and the expiry job and sweeper (`attempts_expiry_idx` exists). Until then, timed sets run untimed. `timeRemainingMs` in the drafts list is always null.
- **F10:**
  - submit and tutor-mode item submit;
  - add `reveal` to the item response;
  - fill `myLastAttempt` in the catalog;
  - show the submit confirmation (unanswered and flagged counts are in `progress`);
  - `resultAvailable`.
- **F11:**
  - renderers for the other ten MVP types (they show a notice now);
  - show `exhibits` and `caseStudy` in the player (the API already sends them);
  - render the Markdown subset in stems and options (they're shown as plain text now).
- **Strike-through UI:** the API stores `struckOptions` and the renderers show them, but there's no control to strike yet (phase 3 UI). Struck ids aren't checked against the item's options.
- **Two quick Start clicks can create two drafts.** Nothing in the DDL makes a draft unique per set. A partial unique index on `(user_id, test_set_id) WHERE status IN ('in_progress','paused')` would close the gap, but it isn't in database.md; your call.
- **`Idempotency-Key`** on create endpoints is still to be built.
- **The dashboard shows only the first 20 drafts.**
- **Importer race:** `ImportQuestionsHandler.GetOrCreateImporterAsync` can race when two imports run on a fresh database. Tests now serialise imports; a fix would be an upsert.
- **Flaky F03 tests under full-suite load:** `LockoutTests.The_51st_login…` and `ReauthenticationTests.Five_wrong_current_passwords…` failed once in a full run and passed when re-run alone. They rely on Redis on the security host, which fails open when Redis is slow.
- **`practice/format.ts`** could use the shared `lib/format/toNumber.ts`.

## 9. Try it yourself

1. **Add a "Discard" button to every draft, not just stale ones.** In `DraftsList.tsx`, drop the `draft.staleDraft &&` condition around the Discard button and keep the confirmation. Extend `DraftsList.test.tsx` so a fresh draft can be discarded too. Then call `DELETE` twice from curl. Why is the second call still 204?
2. **Show progress in the top bar.** `AttemptState.progress` already has `answered` and `flagged`. Show "12 answered · 3 marked" next to "Question n of N" in `AttemptPlayer.tsx`. Which query has to be invalidated after an autosave for the count to stay right, and why does the player avoid doing that on every keystroke?
3. **Lock answers in exam mode when moving on.** Today only `allowBackNav = false` locks the item being left (`Attempt.Navigate`). Write a failing test in `AttemptTests.cs` for a rule like "challenge mode locks answered items when you leave them". Make it pass, then decide whether the rule belongs in `AttemptSettings` or `Attempt`.
