# F10: Grading, results and review

## 1. Summary

When a candidate finishes a test, it is now graded and they see their results. Submit asks first, showing how many questions are unanswered or marked for review. The grade comes from the item-type graders, so partial credit counts. The candidate then lands on a results page with:

- the score
- a readiness estimate, when the set has a pass threshold, always shown with its disclaimer
- time per question
- the weakest Client Needs areas
- a breakdown table for every dimension

In tutor mode, **Submit answer** locks one question and shows its rationale straight away. In exam mode, nothing is revealed until the whole test is submitted. The review page shows every question, or only the incorrect or flagged ones, with the answer marked right or wrong and a "why" for every option. Attempts that run out of time are graded the same way. That covers a late write, the delayed expiry job and the sweeper.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f10`. It branches from `main`, which already contains F07, F08 and F09.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F10_Results
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"   # if not done yet

dotnet run --project src/Nclex.Api          # http://localhost:5080
dotnet run --project src/Nclex.Worker       # grades attempts that expire while the browser is closed
cd ../nclex-fe && npm run dev                # http://localhost:3000
```

**In the browser (the "done when" check):**

1. Sign in as a verified user. F08's walkthrough shows how to verify one through Mailpit or psql.
2. **Tutor mode:** start **Comprehensive Practice 1** on `/practice`. Answer a SATA question with one wrong choice and press **Submit answer**. The question locks, each option is marked "✓ Correct", "✗ Incorrect" or "Correct answer, not selected", and the rationale panel reads, for example, "Partially correct: 1 of 3 points", with a "why" for every option and the educational objective.
3. **Exam mode:** start **Comprehensive Challenge 1**. It has a 65% threshold. Answer a few questions, mark one for review, then press **Submit test**. The dialog says, for example, "37 unanswered questions and 1 question marked for review." **Keep working** has focus and Escape closes the dialog. Press **Submit test** in the dialog.
4. You land on `/attempts/{id}/results`. It shows the score, "Needs more practice (passing threshold 65%)" with the disclaimer, time, the weakest areas, and tables by Client Needs category and subcategory, body system, question type, CJMM step and difficulty.
5. Press **Review incorrect answers**. Blank questions and partial credit are listed. Switch between **All**, **Incorrect** and **Marked for review**.
6. **Expiry:** close a timed attempt's tab and move its deadline into the past:

   ```bash
   docker exec nclex-postgres-1 psql -U nclex -d nclex -c "update attempts set exam_deadline_at = now() - interval '40 seconds' where status = 'in_progress'"
   ```

   Within a minute the sweeper finalizes and grades it. Reopening the attempt goes to its results page.

**With curl:**

```bash
B=http://localhost:5080/api/v1
TOKEN=...; ID=...   # as in F08: a verified user's token and an opened attempt
H=(-H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-laptop")
J=(-H 'Content-Type: application/json')

# Tutor mode: lock, grade and reveal item 1 (the body is optional; it saves a last answer first)
curl -s -X POST $B/attempts/$ID/items/1/submit "${H[@]}" "${J[@]}" -d '{"response":{"selected":"b"},"clientSeq":7}'
# -> {"reveal":{"answerKey":…,"grade":{"points":1.00,"maxPoints":1,"isCorrect":true,"detail":[…]},"rationale":…}}
# Exam mode returns 409 TUTOR_MODE_ONLY.

# Final submit (idempotent: repeat it and the body is identical)
curl -s -X POST $B/attempts/$ID/submit "${H[@]}" "${J[@]}" -d '{"finalResponses":[]}'
curl -s $B/attempts/$ID/result -H "Authorization: Bearer $TOKEN"
curl -s "$B/attempts/$ID/review?filter=incorrect&limit=20" -H "Authorization: Bearer $TOKEN"
```

There are no new test accounts.

## 3. Reading order

1. [nclex-be/src/Nclex.Domain/Grading/ResultCalculator.cs](../../nclex-be/src/Nclex.Domain/Grading/ResultCalculator.cs): totals and the per-dimension breakdown, as pure functions.
2. [nclex-be/src/Nclex.Domain/Grading/ScoreMaths.cs](../../nclex-be/src/Nclex.Domain/Grading/ScoreMaths.cs): how shares and points are rounded, and why they carry their column's scale.
3. [nclex-be/src/Nclex.Domain/Grading/Readiness.cs](../../nclex-be/src/Nclex.Domain/Grading/Readiness.cs): `IReadinessPolicy` and the "likely ready / needs more practice" rule.
4. [nclex-be/src/Nclex.Domain/Grading/RevealRules.cs](../../nclex-be/src/Nclex.Domain/Grading/RevealRules.cs): the one rule for when a candidate may see a key.
5. [nclex-be/src/Nclex.Domain/Grading/WeakestAreas.cs](../../nclex-be/src/Nclex.Domain/Grading/WeakestAreas.cs): the "focus on these" list.
6. [nclex-be/src/Nclex.Domain/Entities/AttemptResult.cs](../../nclex-be/src/Nclex.Domain/Entities/AttemptResult.cs) and [AttemptCategoryScore.cs](../../nclex-be/src/Nclex.Domain/Entities/AttemptCategoryScore.cs): the two new tables.
7. [nclex-be/src/Nclex.Domain/Entities/AttemptItem.cs](../../nclex-be/src/Nclex.Domain/Entities/AttemptItem.cs) (`RecordGrade`) and [Attempt.cs](../../nclex-be/src/Nclex.Domain/Entities/Attempt.cs) (`SubmitItem`).
8. [nclex-be/src/Nclex.Application/Attempts/Grading/GradingService.cs](../../nclex-be/src/Nclex.Application/Attempts/Grading/GradingService.cs): runs the graders, writes the result, the breakdown and the question history, and calls the notification publisher.
9. [nclex-be/src/Nclex.Application/Attempts/AttemptFinalizer.cs](../../nclex-be/src/Nclex.Application/Attempts/AttemptFinalizer.cs): F09's hook, now grading in the same transaction.
10. [nclex-be/src/Nclex.Application/Attempts/AttemptWriteGuard.cs](../../nclex-be/src/Nclex.Application/Attempts/AttemptWriteGuard.cs): the new `CommitAsync`, which grades tutor items locked during a write.
11. [nclex-be/src/Nclex.Application/Attempts/SubmitAttempt/SubmitAttemptHandler.cs](../../nclex-be/src/Nclex.Application/Attempts/SubmitAttempt/SubmitAttemptHandler.cs) and [SubmitItem/SubmitItemHandler.cs](../../nclex-be/src/Nclex.Application/Attempts/SubmitItem/SubmitItemHandler.cs).
12. [nclex-be/src/Nclex.Application/Attempts/Reveal/RevealReader.cs](../../nclex-be/src/Nclex.Application/Attempts/Reveal/RevealReader.cs) and [GetItem/ItemViewReader.cs](../../nclex-be/src/Nclex.Application/Attempts/GetItem/ItemViewReader.cs): the only path from answer keys to a candidate.
13. [nclex-be/src/Nclex.Application/Attempts/Results/GetResultHandler.cs](../../nclex-be/src/Nclex.Application/Attempts/Results/GetResultHandler.cs) and [Review/GetReviewHandler.cs](../../nclex-be/src/Nclex.Application/Attempts/Review/GetReviewHandler.cs).
14. [nclex-be/src/Nclex.Infrastructure/Notifications/NoOpNotificationPublisher.cs](../../nclex-be/src/Nclex.Infrastructure/Notifications/NoOpNotificationPublisher.cs): the F12 seam.
15. [nclex-fe/src/features/attempt/player/useAttemptSession.ts](../../nclex-fe/src/features/attempt/player/useAttemptSession.ts) (`submitItem`, `submitAttempt`) and [SubmitDialog.tsx](../../nclex-fe/src/features/attempt/player/SubmitDialog.tsx).
16. [nclex-fe/src/features/attempt/reveal/RationalePanel.tsx](../../nclex-fe/src/features/attempt/reveal/RationalePanel.tsx) and [renderers/revealMarks.ts](../../nclex-fe/src/features/attempt/renderers/revealMarks.ts).
17. [nclex-fe/src/features/results/ResultsPage.tsx](../../nclex-fe/src/features/results/ResultsPage.tsx) and [nclex-fe/src/features/review/ReviewPage.tsx](../../nclex-fe/src/features/review/ReviewPage.tsx).

## 4. Request flow: the final submit (`POST /api/v1/attempts/{id}/submit`)

1. **UI.** **Submit test** opens `SubmitDialog`, with counts taken from the navigator. Confirming calls `submitAttempt()` in `useAttemptSession`. It flushes the save queue, sends anything still unsent (for example while offline) as `finalResponses`, and calls `postSubmit`.
2. **API.** `AttemptsController.Submit` stays thin. It takes the optional body (`EmptyBodyBehavior.Allow`) and calls `SubmitAttemptHandler`.
3. **Application.**
   - `SubmitAttemptHandler` opens a transaction, and `AttemptWriteGuard.LockAndApplyTimersAsync` runs `SELECT … FOR UPDATE`.
   - Past the deadline, the guard finalizes the attempt as `time_expired` and the call fails with `ATTEMPT_TIME_EXPIRED`.
   - Already submitted: the stored result comes back. This is the idempotent path, and it needs no lease.
   - Paused: `ATTEMPT_NOT_ACTIVE`.
   - Otherwise the lease is checked, the final responses are validated (`ResponseValidator`) and saved like autosaves, and `AttemptFinalizer.FinalizeAsync` runs.
4. **Domain.**
   - The finalizer records which items were seen, then calls `Attempt.Finalize`, which locks every item.
   - `GradingService.FinalizeAttemptAsync` grades every ungraded item with its module's pure `Grade` (`AttemptItem.RecordGrade`).
   - It builds `GradedItem`s with their tags, calls `ResultCalculator.Totals` and `Breakdown`, and asks `IReadinessPolicy` for the estimate.
5. **Database.** In the same transaction:
   - `attempt_items` gets the points, detail and `graded_at`
   - one `attempt_results` row and one `attempt_category_scores` row per dimension key are inserted
   - `user_question_history` is upserted for the seen questions
   - `INotificationPublisher.PublishAsync` is called (a no-op until F12)
   - `guard.CommitAsync` saves and commits
6. **Back.**
   - The handler returns `{ attemptId, status, submitReason, result: { score, time, readiness } }`.
   - The hook clears the save queue and sets `submitted`, and the player pushes `/attempts/{id}/results`.
   - `ResultsPage` reads `GET /result`. That is one indexed read of the stored rows, with names looked up from the taxonomy.

## 5. Database changes

Migration `F10_Results` adds the two tables of database.md 2.7, exactly as in the DDL:

- **`attempt_results`**: one row per submitted attempt. The primary key is `attempt_id`, which cascades from `attempts` and is also what makes a retried submit unable to write twice.
  - The score columns are `numeric(8,2)` and `numeric(5,4)`.
  - `pass_threshold`, `passed` and `readiness_label` are null when the set has no threshold.
  - `cat_theta` and `cat_se` are created but stay empty until CAT mode.
  - `graded_at` defaults to `now()`.
- **`attempt_category_scores`**: the breakdown, stored at submit so the results page never re-grades (architecture.md 5.5).
  - The primary key is `(attempt_id, dimension, dimension_key)`, which also serves the results read.
  - It has the DDL's `dimension` CHECK, named `attempt_category_scores_dimension_check`, and cascades with the attempt.

There are no column changes. `attempt_items` already had the grading columns (F08), and `user_question_history` already existed (F07). F10 is the first feature to write both.

## 6. Key decisions and deviations

These are the decisions agreed in the plan:

1. **Tutor-mode items are graded the moment they lock.** That covers Submit answer, an item timer running out, and moving forward with back-navigation off. Every write now ends with `AttemptWriteGuard.CommitAsync`, which grades the attempt's tracked items that are locked but not graded (`DbSet.Local`) before saving. The open, pause, resume, navigate, save and mark handlers switched from `SaveChanges` plus `Commit` to this call.
2. **Submitting after the deadline** gets `409 ATTEMPT_TIME_EXPIRED`, like any late write, and its `finalResponses` are discarded. A retry returns `200` with the `time_expired` result. The player sends that one submit after the 2-second grace and goes to results either way. This replaces F09's "make one write" check.
3. **Submitting while paused** returns `409 ATTEMPT_NOT_ACTIVE`.
4. **Score and time:**
   - `pct` is points earned over the maximum, so partial credit counts.
   - Total time is the sum of each item's `time_spent_ms`.
   - The average is that total divided by the number of items served.
5. **All six dimensions are stored**, including `difficulty`. Items without a CJMM step are left out of `cjmm_skill`. A top-level leaf such as PSI is its own `client_needs_parent`. Names are not stored: they are looked up when the result is read.
6. **Weakest areas** are the three `client_needs` subcategories with the lowest share of points, ties broken by more items.
7. **Question history** counts only items the candidate saw, so unreached items stay "unused". A seen but blank item counts as incorrect (`last_correct = false`), so it shows up in the builder's "incorrect" pool. The doc comment on `UserQuestionHistory.LastCorrect` said "null when never answered", and was changed to match.
8. **Reviewing an unsubmitted attempt:** a tutor attempt in progress lists only its locked items. An exam attempt returns `409 ATTEMPT_NOT_ACTIVE`, and a paused one returns `423`, because content is hidden during a pause.

These go beyond the plan, or differ slightly from the design docs:

- **A new error code, `TUTOR_MODE_ONLY` (409).** api.md says the item submit "returns 409 in exam mode" but names no code, and none in section 1 fits. Please add it to api.md's table if you agree.
- **`IGradingService` signatures differ from interfaces.md 3.** They take the tracked, locked `Attempt` and its items rather than ids, because the caller already holds them inside its transaction:
  - `GradeItemAsync(attemptId, position)` became `GradeLockedTutorItemsAsync(attempt)`. It covers every lock path, not only Submit answer.
  - `FinalizeAttemptAsync` takes the items and the positions seen before locking, because `Attempt.Finalize` turns unseen items into locked ones and that information would be lost.
- **`INotificationPublisher` and `NewNotification`.** They are defined with only what result notifications need: type, set title, score, weakest names and deep link. F12 may widen them.
- **The readiness block is `null` when there is no threshold.** api.md's example always shows it. The disclaimer goes with it.
- **The breakdown JSON keys are snake_case** (`client_needs`…), exactly as api.md shows, through `[JsonPropertyName]`. The rest of the API is camelCase.
- **Scores carry their column's scale.** For example `1.00` and `0.4000`, even before they are stored (`ScoreMaths`). Without this, the first submit returned `1` and a retry returned `1.00`. The values were equal, but the bodies were not byte-identical.
- **`GET /review` reads `position` as a starting position.** Items from that position on are returned, with `cursor` for further pages. api.md lists the parameter without saying what it does.
- **An attempt finalized before F10 has no stored result.** `GET /result` returns `404`. Its reveals are graded on the fly, which is safe because graders are pure.
- **The worker now registers the item-type modules and grading,** and checks the modules at start-up. Its expiry jobs grade. Module registration became idempotent (`TryAddEnumerable`), because both the importer and grading add the modules.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test          # 853 passed (Docker must be running for Testcontainers)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # 219 passed
```

- **Domain** (`tests/Nclex.Domain.Tests/Grading/`):
  - totals with partial credit and rounding
  - the breakdown for every dimension, including a top-level leaf and a missing CJMM step, and each dimension adding up to the totals
  - readiness at and below the threshold, and with no threshold
  - weakest-area order and ties
  - `SubmitItem`, `RecordGrade` (locked only, once) and the reveal rules
- **Integration** (`tests/Nclex.Api.IntegrationTests/Attempts/GradingTests.cs`):
  - tutor submit returning the key, grade, detail, rationale and tags, with `ITEM_LOCKED` afterwards
  - other items staying hidden
  - a final answer sent with the item submit
  - resubmitting a locked item (same reveal, no change)
  - grades kept at the final submit
  - exam mode: `TUTOR_MODE_ONLY`, no reveal and no review before submit, a reveal after
  - SATA partial credit (2 of 5 = 0.4)
  - the result's breakdown rows, names and weakest areas
  - readiness pass, fail and none
  - `finalResponses`, including a stale one; an invalid one rolls back
  - two simultaneous submits and a third from another device, giving identical bodies, one `attempt_results` row and the right number of category rows
  - lease and paused errors
  - draft results and another user's attempt
  - question history (seen and blank counts as incorrect, unseen absent, a second attempt increments)
  - the review filters and the cursor, tutor-in-progress review, and a bad filter
- **Expiry** (`AttemptTimerTests`, in the same class as the sweeper test on purpose):
  - a late write, the delayed job and the sweeper each grade, with blanks scoring 0
  - a submit after the deadline gets `409`, then the retry returns the `time_expired` result without the late answer
  - a tutor item locked by its timer is graded and revealed at once
- **Schema** (`MigrationTests`): F10 column types, nullability and defaults, primary keys, the dimension CHECK, and cascade deletes.
- **Frontend:**
  - `AttemptPlayerSubmit.test.tsx`: confirmation counts and focus, Keep working and Escape, submit leading to results, an unsent answer sent as a final response, submit failure, tutor Submit answer with rationale and marks, no item submit in exam mode
  - `RationalePanel.test.tsx`
  - `ResultsPage.test.tsx`: score, time, readiness and disclaimer, no estimate, tables, weakest areas, links, time-expired note, not-submitted error, duration formatting
  - `ReviewPage.test.tsx`: marks and rationale, filter from the link and switching, paging, empty and blank states, the exam-mode message
  - F09's two time-up tests now expect the submit and the redirect to results

I didn't click through the pages in a real browser. The checks above are automated tests against the real API and database, plus component tests with the network mocked.

## 8. Follow-ups and known limitations

- **F12:** replace `NoOpNotificationPublisher` with the notifications table, the outbox email (per preferences) and the Redis publish after commit. Use the `notif_result_once` index for the case where grading is retried.
- **F13:** enqueue `SnapshotRefreshJob` after the commit (architecture.md 5.5) and add history and charts. `IJobScheduler.EnqueueSnapshotRefresh` doesn't exist yet.
- **F11:** the other MVP renderers should accept `reveal` and show marks. Until then the review shows their rationale panel, but their answers only show the "unsupported" notice.
- If the same user's two attempts that share a question are finalized at the same moment (for example the sweeper and a manual submit), the second insert into `user_question_history` can hit its primary key and roll that submit back. A retry succeeds. An `INSERT … ON CONFLICT` upsert would remove this. It is rare, so it was left as it is.
- The dashboard doesn't link to past results yet. History is F13. The results URL is reachable from the player and from the review page.
- The submit dialog counts unanswered questions from the navigator. An item locked by its timer while blank counts as answered there, though it scores 0.
- The modal dialogs don't trap Tab focus, as with F08's dialog. A shared dialog primitive would fix both.
- Add `TUTOR_MODE_ONLY` and the review `position` meaning to api.md, if you accept them.

## 9. Try it yourself

1. **Show five weakest areas instead of three.** Change `WeakestAreas.Count`, then update `WeakestAreasTests` and the `weakest` assertion in `GradingTests`. Why is the rule in Nclex.Domain and not in the results handler?
2. **Add "time per question" rows to the results page.** Return each item's `timeSpentMs` from `GET /result` (a new list on `AttemptResultView`). Regenerate `schema.d.ts` with `npm run gen:api` while the API runs, and render a table in `ResultsPage`. Note which layer each change lands in.
3. **Make seen-but-blank questions stay out of the "incorrect" pool.** In `GradingService.RecordHistoryAsync`, pass `item.IsAnswered ? item.IsCorrect : null` as `correct`. Which integration test fails, and what does that tell you about the decision in section 6?
