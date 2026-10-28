# F13: Performance analytics, history and dashboard

## 1. Summary

Candidates can now see how they are doing across every test they have finished, not just one result at a time. The dashboard lists their latest results and shows their overall score and three weakest Client Needs categories. A new **Performance** page has the totals, a breakdown table for any dimension (Client Needs category or subcategory, body system, question type, clinical judgment step, difficulty) and two trend charts, overall and for one chosen category, by week or by day in the candidate's own time zone. Every chart has its numbers in a data table beside it. A new **History** page lists every finished test with its score and links to its results and review. Behind the scenes, a worker job keeps a per-user summary table (`performance_snapshots`) up to date after every submit, and an admin can rebuild it from the stored results.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f13`. It branches from `main`, which contains F10. F11 and F12 are not merged, so they are not included (see section 6).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F13_PerformanceSnapshots
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"   # if not done yet

dotnet run --project src/Nclex.Api          # http://localhost:5080
dotnet run --project src/Nclex.Worker       # runs SnapshotRefreshJob after each submit (Hangfire)
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000 (npm install adds recharts)
```

During development I used a separate database and ports so as not to disturb other branches: `nclex_f13`, the API on `:5093` and the frontend on `:3013`, with `ConnectionStrings__Postgres=...Database=nclex_f13...` and `API_URL=http://localhost:5093`.

**In the browser (the "done when" check):**

1. Sign up (the free **RN Sample Set** needs no verified email) and finish the sample set three times with different answers.
2. Open `/dashboard`. **Recent results** lists the three tests with their scores, newest first. **Performance** shows, for example, "16% overall on 30 questions from 3 tests" and the three weakest categories. A category needs at least 3 questions before it can be called weak.
3. Open **Performance** from the user menu. The four stat tiles show the same numbers as the dashboard. The **Overall score** trend has one point for this week, and its data table shows the same score and question count. Change **Break down by** and **Category to follow**, and switch **Show trends by** to **Day**.
4. Open **History**. Each row has the score, the readiness estimate when the set has a threshold, and **Results** and **Review** links.
5. Open `/practice` and the sample set's detail: `myLastAttempt` now holds your latest score.
6. Time zones: set your time zone (`PATCH /me {"timezone":"America/Los_Angeles"}`). Trend buckets then follow your local calendar.

What I checked locally: three real attempts went through the proxy and the worker. Its log shows the first job adding all three attempts and the later two adding none (see section 4). Summary, breakdown, trend and history all agreed: 10 of 63 points = 16%, 30 questions. I also checked screenshots at 1280 px (light and dark) and at 390 px.

**With curl:**

```bash
B=http://localhost:5080/api/v1
H=(-H "Authorization: Bearer $TOKEN")

curl -s "${H[@]}" $B/me/performance/summary
# -> {"track":"RN","attemptsCount":3,"itemsAnswered":30,"itemsCorrect":4,"accuracy":0.1587,"avgTimeMs":…,"weakest":[{"key":"PHI.PA","name":"Physiological Adaptation",…}]}
curl -s "${H[@]}" "$B/me/performance/breakdown?dimension=client_needs_parent"
curl -s "${H[@]}" "$B/me/performance/trend?bucket=week"                                  # overall
curl -s "${H[@]}" "$B/me/performance/trend?dimension=client_needs&key=PHI.PPT&bucket=day&from=2026-10-01&to=2026-10-31"
curl -s "${H[@]}" "$B/attempts?status=submitted&limit=20"                               # history rows now carry scorePct and passed

# Admin only (403 for anyone else, 404 for an unknown user)
curl -s -o /dev/null -w '%{http_code}\n' -X POST $B/admin/performance-snapshots/rebuild \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H 'Content-Type: application/json' -d "{\"userId\":\"$USER_ID\"}"   # 202
```

To make an admin locally: `update users set role = 'admin' where email = '…'`, then sign in again so the new role is in the token.

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Entities/PerformanceSnapshot.cs`: one row of running totals and its `Add` method.
2. `nclex-be/src/Nclex.Domain/Performance/`: `PerformanceDimension(s)` (overall plus the six breakdown dimensions), `SnapshotTally`, `SnapshotMaths` (folding an attempt into an average), `TrendBucketing` (local day or week, time zones, clock changes), `TrendSeries` (samples to chart points) and `WeakestCategories`.
3. `nclex-be/src/Nclex.Infrastructure/Persistence/Configurations/PerformanceSnapshotConfiguration.cs` and `Migrations/*_F13_PerformanceSnapshots.cs`: the table, including the generated `accuracy` column.
4. `nclex-be/src/Nclex.Application/Attempts/AttemptFinalizer.cs`: the one place every finalize path goes through now asks for a snapshot refresh.
5. `nclex-be/src/Nclex.Infrastructure/Jobs/PendingSnapshotRefreshes.cs` and `EnqueueSnapshotRefreshAfterCommit.cs`: hold that request until the transaction commits, then queue the job.
6. `nclex-be/src/Nclex.Application/Performance/Snapshots/SnapshotCatchUp.cs`: **the heart of the feature.** The high-water mark, the self-healing replay and the incremental upsert.
7. `RefreshSnapshotsHandler.cs`, `RebuildSnapshotsHandler.cs` (same folder) and `Infrastructure/Jobs/SnapshotRefreshJob.cs`, `SnapshotRebuildJob.cs`: transaction, per-user lock and Hangfire wrapper.
8. `nclex-be/src/Nclex.Application/Performance/GetSummary`, `GetBreakdown`, `GetTrend`, `RebuildSnapshots`: the read endpoints and the admin request.
9. `nclex-be/src/Nclex.Api/Controllers/PerformanceController.cs` and `AdminPerformanceSnapshotsController.cs`: thin controllers.
10. `nclex-be/src/Nclex.Application/Attempts/ListAttempts/ListAttemptsHandler.cs` and `Catalog/LastAttempts.cs`: history scores and `myLastAttempt`.
11. `nclex-fe/src/features/performance/api/performanceApi.ts`, then `components/PerformancePage.tsx`, `TrendChart.tsx`, `BreakdownTable.tsx`.
12. `nclex-fe/src/features/history/` and `nclex-fe/src/features/dashboard/components/Dashboard.tsx`, `RecentResults.tsx`, `PerformanceOverview.tsx`.

## 4. Request flow

**Write side: a submit updates the snapshots.**

1. The player sends `POST /attempts/{id}/submit`. `SubmitAttemptHandler` locks the attempt and calls `AttemptFinalizer`, which grades it and writes `attempt_results` and `attempt_category_scores` (F10). The finalizer then calls `ISnapshotRefreshRequests.RequestAfterCommit(userId, attemptId)`. Nothing is queued yet.
2. `AttemptWriteGuard.CommitAsync` commits. EF Core's `TransactionCommittedAsync` hook in `EnqueueSnapshotRefreshAfterCommit` then calls `IJobScheduler.EnqueueSnapshotRefresh`. Hangfire writes the job into its Postgres tables. A rollback discards the request. The expiry job, the sweeper and a late write go through the same finalizer, so they queue the job too.
3. The worker's Hangfire server runs `SnapshotRefreshJob`, which calls `RefreshSnapshotsHandler`. It opens a transaction and takes `pg_advisory_xact_lock` for the user, so two jobs for the same user run one after the other. Then it calls `SnapshotCatchUp.RunAsync(userId, track)`:
   - It lists the user's submitted attempts in that track in submit order, `(submitted_at, id)`.
   - It reads the overall row. Its `last_attempt_id` is the high-water mark: every attempt up to it is already counted.
   - If `attempts_count` equals the number of attempts up to the mark, it adds only the attempts after the mark. Otherwise an attempt landed behind the mark, so it deletes the track's rows and replays them all.
   - For each attempt it adds the result to the `overall`/`all` row and each `attempt_category_scores` row to its dimension's row, creating rows as needed.
4. The handler saves and commits. Running the job again finds nothing after the mark and adds 0. A job that runs late finds that an earlier job already caught up on its attempt.

**Read side: the performance page.**

1. `PerformancePage` mounts. TanStack Query calls `GET /me/performance/summary`, `/breakdown?dimension=client_needs_parent` and `/trend?dimension=overall&bucket=week` through the Next.js proxy.
2. `PerformanceController.Trend` passes the query, already validated by `GetTrendRequestValidator`, to `GetTrendHandler`.
3. The handler loads the user's track and time zone (`PerformanceViewers`) and turns `from`/`to` into UTC bounds with `TrendBucketing.StartOfDay`. It reads one row per attempt: `attempts` joined to `attempt_results`, or to `attempt_category_scores` for a category. The `(user_id, submitted_at)` index serves that read.
4. `TrendSeries.Build` groups the rows into local days or ISO weeks and computes the points-based score and the average time per bucket. The JSON goes back to the page.
5. `TrendChart` draws the Recharts line (labelled as an image) and renders the same numbers in a table beside it.

## 5. Database changes

Migration `F13_PerformanceSnapshots` creates `performance_snapshots` exactly as in database.md 2.7:

- primary key `(user_id, track, dimension, dimension_key)`
- `accuracy numeric(5,4) GENERATED ALWAYS AS (CASE WHEN max_points > 0 THEN points_earned / max_points ELSE 0 END) STORED`
- `points_earned` and `max_points` as `numeric(10,2)`
- `last_attempt_id` (nullable, no foreign key, as in the DDL)
- `updated_at` defaulting to `now()`
- a cascade foreign key to `users`

There are no extra indexes: every read filters on a prefix of the primary key.

`dimension` is free text, as in the DDL. It holds the six breakdown names plus `overall`, whose single key is `all`. Trends need no table (database.md 2.7).

## 6. Key decisions and deviations

- **Idempotency guard (agreed in the plan).** `last_attempt_id` on the overall row is a high-water mark over `(submitted_at, id)`, not "the attempt this job is for". This makes repeated, out-of-order and concurrent jobs all safe. If `attempts_count` doesn't match the attempts up to the mark, the track is replayed from scratch. That covers an attempt that commits behind the mark, a lost job, and manual corruption.
- **After-commit enqueue (agreed).** The job is queued from an EF Core transaction interceptor once the submit commits, as architecture.md 5.5 orders it. AddInfrastructure now hands every registered `IInterceptor` to the DbContext. F12 adds a similar interceptor for notifications with an explicit `AddInterceptors(...)`. When F12 and F13 meet, keep both: register F12's as an `IInterceptor` too, or list both.
- **Admin rebuild (agreed).** `POST /admin/performance-snapshots/rebuild {userId}` returns 202 and queues `SnapshotRebuildJob`. It is added to api.md section 8.
- **Summary.** It has no `streak` (phase 2). It gains `weakest`, the three least accurate Client Needs subcategories with at least 3 questions. This keeps the dashboard to the one call that interfaces.md 5.2 names. api.md section 5 is updated.
- **Item counts.** `items_answered` counts items *served*, answered or not, in both the snapshots and the trend. That matches the results page breakdown, where a blank item counts against the score. The UI says "Questions answered" to stay with the column name; see follow-ups.
- **Empty trend buckets** are left out rather than drawn as 0%, which would read as a score.
- **History** reuses F08's `GET /attempts?status=submitted` and its cursor paging. Rows gain `scorePct` and `passed`. `myLastAttempt` on test-set cards and detail is now filled in; it was always `null` before.
- **Test host startup.** `TimedTestHost` now starts its hosts one at a time. Program.cs swaps Serilog's static bootstrap logger, and with three test classes using the host in parallel, two could start at once and fail with "logger already frozen".
- **Merge notes.** F12 also edits `GradingService`, the model snapshot and `schema.d.ts`. Whichever of F12 and F13 merges second should resolve `AppDbContextModelSnapshot.cs` so it holds both features' tables, then confirm with `dotnet ef migrations has-pending-model-changes`. It should also re-run `npm run gen:api` against the merged API.
- **Dependency.** `recharts` 3.10.1 was added. It is listed in the stack.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test        # Docker must be running (Testcontainers)
cd nclex-fe && npm run lint && npm run typecheck && npm test
```

Results on this branch: `dotnet build` 0 warnings, 0 errors. `dotnet test` 912 passed, 0 failed. Frontend: lint clean, typecheck clean, 235 tests passed.

- **Domain** (`tests/Nclex.Domain.Tests/Performance`):
  - Trend bucketing across time zones: a Sunday-night test in Los Angeles that is Monday in UTC, a Monday-morning test in Auckland that is Sunday in UTC, a week across the US clock change, and Santiago's skipped midnight.
  - Trend series sums, snapshot running totals and average time, dimension names, weakest categories.
- **Application:** the performance request validators.
- **Integration** (`tests/Nclex.Api.IntegrationTests/Performance`, real Postgres):
  - Snapshot jobs: one refresh is queued per submit and none for a repeated submit. Running the job twice gives the same totals. A late job adds nothing. Concurrent jobs add each attempt once. Drifted snapshots are replayed. Rebuild equals incremental.
  - Endpoints: summary (including the zero state and the weakest list), breakdown, trend weeks in UTC versus Los Angeles, local `from`/`to` days, category trend, validation errors, history `scorePct`, `myLastAttempt`, and admin rebuild returning 202, 403 or 404.
  - Migration tests: the generated column and the primary key.
- **Frontend:**
  - Performance page: stats, empty state, breakdown switching, chart data tables, following a category, the day bucket, the error state.
  - History: rows, links, "Show more" with the cursor, empty state.
  - Dashboard cards, and bucket label dates.

## 8. Follow-ups and known limitations

- **Average time drifts by a millisecond or two.** The table stores a rounded average, not a total, so each incremental fold rounds again. A rebuild replays in the same order, so it matches the jobs exactly, but the summary's `avgTimeMs` can differ from the trend's by about 1 ms per attempt at worst. At real answer times (around 60,000 ms) this is invisible once formatted to seconds. With my script's 9 ms answers the summary showed 10 ms and the trend 9 ms. A `total_time_ms` column would remove the drift, but it changes the DDL, so it needs your decision.
- **Snapshots lag a submit by a few seconds**, the time until the worker runs the job. The dashboard and performance page refetch on mount, so they catch up on the next visit.
- **Lost jobs.** If the API dies between commit and enqueue, that attempt waits for the user's next submit, which adds it, or for an admin rebuild. A nightly sweep of users whose `attempts_count` disagrees would close the gap.
- **No admin screen** for the rebuild; it is API only until the admin UI features.
- **Possible column rename.** "Questions answered" counts served items, as on the results page. Renaming to "Questions seen" may read more honestly; it's a copy decision.
- **Out of scope as listed:** streaks, goals and the study planner (phase 2). The trend `track` defaults to the user's track; there is no track switcher on the page.

## 9. Try it yourself

1. **Change what counts as weak.** Set `WeakestCategories.MinItems` to 5 in `Nclex.Domain/Performance/WeakestCategories.cs`. Run the domain tests and the `PerformanceEndpointTests`, and see which expectations need updating and why.
2. **Weeks starting on Sunday.** Make `TrendBucketing.StartOfWeek` start weeks on Sunday. Which `TrendBucketingTests` fail? Would you make this a per-user setting, and where would it be stored?
3. **Watch the self-healing replay.** Finish two tests, then run `update performance_snapshots set attempts_count = 9 where dimension = 'overall'` in psql. Finish a third test and read the worker log: the job reports 3 attempts added, because it replayed the track. The numbers on `/performance` are correct again.
