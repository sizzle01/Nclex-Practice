# F23: Pass marks (Pass, Close to pass, Fail)

## 1. Summary

Every fixed-length test is now graded against one fixed pass mark: **86% and above is a Pass**,
**70% to 85% is Close to pass**, and **below 70% is a Fail**. This covers ready-made sets, custom
tests and weak-area practice. The candidate sees the outcome in green, amber or red on the results
page, the history list and the dashboard's recent results. A close-to-pass result gets its own
notification and email wording. Past results are re-graded by the migration so old and new
results agree. Adaptive (CAT) exams keep the pass or fail of their ability estimate.

## 2. How to try it

```bash
docker compose up -d
cd nclex-be
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F23_PassMarks (re-grades past results)
dotnet run --project src/Nclex.Api                                         # http://localhost:5080
cd ../nclex-fe && npm run dev                                              # http://localhost:3000
```

Sign in with a candidate account (see the F01 walkthrough, section 2), then:

- `/practice`: open any set. The detail panel shows **Pass mark: 86% of points (70% or more is close to pass)**.
- Take a short set and submit. The results page shows the score in its outcome colour, then
  **Pass**, **Close to pass** or **Fail** with "(pass mark 86%)" and the estimate disclaimer.
- `/history` and `/dashboard`: each finished test shows its outcome next to the score.
- The bell (and Mailpit at http://localhost:8025) shows "You're close to passing" for a 70–85% score.
- Check the re-grade:
  ```sql
  SELECT round(score_pct * 100) AS shown, passed, readiness_label, pass_threshold
  FROM attempt_results WHERE cat_theta IS NULL ORDER BY graded_at DESC LIMIT 20;
  ```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Grading/Readiness.cs`: `ReadinessPolicy`, the whole rule and its two marks.
2. `nclex-be/src/Nclex.Domain/Grading/ScoreMaths.cs`: `WholePercent`, the rounding shared by the rule and the screen.
3. `nclex-be/src/Nclex.Domain/Entities/ReadinessLabel.cs`: the three labels and how each is stored.
4. `nclex-be/src/Nclex.Application/Attempts/Grading/GradingService.cs` (`ScoreResult`): grades every non-adaptive attempt against the pass mark.
5. `nclex-be/src/Nclex.Infrastructure/Persistence/Configurations/AttemptResultConfiguration.cs`: label ↔ text column.
6. `nclex-be/src/Nclex.Infrastructure/Persistence/Migrations/20261120180618_F23_PassMarks.cs`: the re-grade (Up) and the old rule (Down).
7. `nclex-be/src/Nclex.Application/Notifications/ResultNotifications.cs` and `Nclex.Domain/Notifications/NotificationCopy.cs`: the close-to-pass wording.
8. `nclex-be/src/Nclex.Application/Attempts/ListAttempts/AttemptSummary.cs`: `readinessLabel` on history rows.
9. `nclex-fe/src/features/results/ResultOutcome.tsx`: the pass marks on the client, the words, and the coloured label.
10. `nclex-fe/src/features/results/ResultsPage.tsx`, `history/components/HistoryPage.tsx`, `dashboard/components/RecentResults.tsx`: where the outcome is shown.
11. `nclex-fe/src/features/practice/components/TestSetDetailPanel.tsx`: the pass mark before starting.

## 4. Request flow

Submitting a fixed-length test:

1. **UI:** the player calls `POST /api/v1/attempts/{id}/submit`.
2. **API:** `AttemptsController` hands it to the submit handler, which locks the attempt and calls `GradingService`.
3. **Application:** `GradingService.ScoreResult` totals the points (e.g. 4 of 5 = 0.8000), then calls
   `readiness.Evaluate(0.8000)` and `AttemptResult.Create(..., ReadinessPolicy.PassMark, ...)`.
4. **Domain:** `ReadinessPolicy` rounds to the shown percentage (80), which is ≥ 70 and < 86, so it returns
   `Readiness(false, CloseToPass)`.
5. **Database:** the `attempt_results` row is written with `pass_threshold = 0.8600`, `passed = false`,
   `readiness_label = 'close_to_pass'`. In the same transaction `ResultNotifications.For` adds a
   `result_fail` notification with the close-to-pass wording and a `result_close` email outbox row.
6. **Back:** the response's `result.readiness` is `{ passThreshold: 0.86, passed: false, label: "close_to_pass" }`.
   The results page renders `<ResultOutcome label="close_to_pass" />`, which shows "Close to pass" in amber.

## 5. Database changes

No schema change. `readiness_label` is a text column, so the new value `close_to_pass` needs no DDL.

Migration `F23_PassMarks` is data only:
- **Up:** re-grades every non-adaptive row (`cat_theta IS NULL`). It sets `pass_threshold = 0.86`,
  `passed`, and `readiness_label` from `round(score_pct * 100)`.
- **Down:** restores the F10 rule from each attempt's settings snapshot (`settings->>'passThreshold'`):
  two labels, and no estimate when the set had no threshold.

## 6. Key decisions and deviations

- **One fixed rule replaces the per-set threshold** (architecture.md 5.5 updated). `test_sets.pass_threshold`
  stays in the schema and API but no longer affects grading. The admin editor no longer shows the field;
  saving a set writes back whatever value it already had.
- **Edges are judged on the shown whole percentage**, half rounding up (C# `AwayFromZero`, Postgres `round`,
  JS `Math.round`), so 85.5% reads "86%" and passes, and 69.5% reads "70%" and is close to pass.
- **Stored label values are unchanged.** Pass is still `likely_ready` and fail is still
  `needs_more_practice`; only `close_to_pass` is new. Existing rows, the CAT result and the API stay
  compatible. The UI words are Pass, Close to pass and Fail.
- **History rows return `readinessLabel`**, the same values as the result endpoint. The plan said a new
  `outcome` field with `pass | close_to_pass | fail`; reusing the existing enum avoids a second vocabulary.
- **Close to pass keeps the `result_fail` notification type**, so the Postgres `notification_type`
  enum and the one-result-per-attempt index are unchanged. Only the wording and the email template
  (`result_close`) differ.
- **The practice set panel now shows the fixed pass mark.** This wasn't in the plan, but it used to show
  each set's own threshold (65%), which would contradict the results.
- **Performance page colours now match** (amber starts at 70%, not 71%). They read the same
  `PASS_MARK` and `CLOSE_TO_PASS_MARK` constants.
- **Feature number F23:** another session is building F22 (CAT case studies).

## 7. Tests

Backend (`cd nclex-be && dotnet test`; integration tests need Docker, run with `caffeinate -i`):
- `ReadinessPolicyTests`: the edges 0, 69.49%, 69.5%, 70%, 85%, 85.49%, 85.5%, 86%, 100%.
- `NotificationCopyTests`: close-to-pass wording, with and without weak categories.
- `GradingTests`: a 40% score on a set asking 40% now fails, 80% is close to pass, and 100% passes, all
  against 0.86. Also the migration: an old 72% "likely ready" row is re-graded to close to pass, and
  Down (inside a rolled-back transaction) restores it.
- `ResultNotificationTests`: close to pass sends `result_fail` with the new wording and the `result_close` email.
- `PerformanceEndpointTests`: history rows carry `passed` and `readinessLabel`.

Frontend (`cd nclex-fe && npm run lint && npm run typecheck && npm test`): results page outcomes for each
label, history and dashboard outcome labels, the practice panel pass mark, and the Performance band edges.

Results on this branch: `dotnet build` 0 warnings, 0 errors; `dotnet test` 1,429 passed, 0 failed;
`has-pending-model-changes` clean; lint clean; typecheck clean; `npm test` 77 files, 492 tests passed.

## 8. Follow-ups and known limitations

- **VM deploy:** the migration re-grades results on the test VM when it is deployed. Old notifications
  and emails keep their original wording.
- **`result_ready` is now unused for new results:** every result has a pass/fail estimate. The type,
  template and copy remain for old rows; they could be removed later.
- **The `test_sets.pass_threshold` column and API field** still exist. Removing them (and the content
  import's `passThreshold`) would need its own migration and API change.
- **The marks are constants in two places**, `ReadinessPolicy` (server) and `ResultOutcome.tsx` (client).
  If they ever need to change per track (PN) or per set, they belong in configuration served by the API.

## 9. Try it yourself

1. Change the pass mark to 85% in `ReadinessPolicy` and `ResultOutcome.tsx`, then see which tests fail
   and why (`ReadinessPolicyTests`, `GradingTests`, the practice panel test).
2. Add a fourth outcome, "Strong pass" for 95% and above: a new `ReadinessLabel`, its stored text, the
   word and colour in `ResultOutcome`, and an edge test.
3. Roll the migration back and forward with `dotnet ef database update F21_RenameSafetyCategory` and
   `dotnet ef database update`, and watch the SQL query from section 2 change.
