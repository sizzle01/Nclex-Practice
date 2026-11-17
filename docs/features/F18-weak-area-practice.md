# F18: Practice your weak areas

## 1. Summary

Candidates now get a **Practice your weak areas** card on the dashboard and on every results page. It lists the Client Needs areas where they scored under 65% in their last 10 tests, counting only areas with at least 5 questions. One button then starts a 25-question practice test in tutor mode (rationales after each answer, no timer). The test picks questions the candidate last missed first, then ones they have never seen, and only then ones they got right. If nothing is weak, the card says so instead of inventing a weakness. If the areas have fewer than 25 questions, the card says the test will be shorter before it starts.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f18`, and branches from `main` (F00–F17 plus the RN bank, 5fabf61). There is **no migration**.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
# questions: the sample, then the RN bank (see docs/features/RN-question-bank.md)
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
for f in content/rn-bank/rn-bank-*.rn.json; do dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$f"; done

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000
```

During development I used the database `nclex_f18` and the API on `:5098`.

**As a candidate:**

1. Sign up and open the dashboard. Before any test, the card says "Finish a test and we'll point out the areas to practise."
2. Take the **RN Sample Set** and get most answers wrong. The sample set has 10 items spread over several areas, so you may need two or three attempts before an area reaches 5 questions.
3. Go back to the dashboard (or stay on the results page). The card lists the weak areas, worst first, with their scores, and a **Practice 25 questions** button.
4. Click it. The player opens in tutor mode on a set called "Weak areas practice: 25 questions". All its questions come from the listed areas.
5. Get everything right in a later test or two: once no area is under 65%, the card says "No weak areas in your last N tests…" and the button disappears.

**With curl:**

```bash
B=http://localhost:5080/api/v1
curl -s -H "Authorization: Bearer $TOKEN" $B/me/weak-areas
# {"track":"RN","areas":[{"clientNeedsId":8,"code":"PHI.PPT","name":"Pharmacological and Parenteral Therapies",
#   "pct":0.0,"items":9}, …],"attemptsConsidered":3,"questionsAvailable":550,"questionCount":25}

curl -s -X POST -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' -d '{}' $B/test-sets/weak-areas
# 201 {"testSet":{"title":"Weak areas practice: 25 questions","mode":"practice","revealMode":"tutor",…},"attempt":{…}}
# 422 NO_WEAK_AREAS when nothing qualifies
```

Starting a test needs a verified email (403 `EMAIL_NOT_VERIFIED`), as for any custom set. Locally, run `update users set email_verified_at = now() where email = '…'`.

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/WeakAreas/WeakAreaRules.cs`: **the rule.** Which Client Needs leaves are weak, and in what order.
2. `nclex-be/src/Nclex.Domain/Builder/WeakAreaSampler.cs`: **the pick.** Test-plan shares per leaf, then missed, unseen and correct.
3. `nclex-be/src/Nclex.Domain/WeakAreas/WeakAreaPractice.cs`: the test size (25, fewer when short, none under 5).
4. `nclex-be/src/Nclex.Application/WeakAreas/WeakAreaFinder.cs`: loads the last 10 attempts' per-category scores and applies the rule.
5. `nclex-be/src/Nclex.Application/WeakAreas/WeakAreaPool.cs`: reuses the custom builder's `QuestionPool` and adds what the user last did with each question.
6. `nclex-be/src/Nclex.Application/WeakAreas/GetWeakAreas/GetWeakAreasHandler.cs`: the card's preview.
7. `nclex-be/src/Nclex.Application/WeakAreas/StartWeakAreaPractice/StartWeakAreaPracticeHandler.cs`: saves the custom set and starts the attempt in one transaction.
8. `nclex-be/src/Nclex.Api/Controllers/WeakAreasController.cs`: two thin endpoints.
9. `nclex-fe/src/features/weak-areas/api/weakAreasApi.ts` and `WeakAreasCard.tsx`: the card, used by `Dashboard.tsx` and `ResultsPage.tsx`.

## 4. Request flow

Clicking **Practice 25 questions** on the dashboard:

1. `WeakAreasCard` calls `useStartWeakAreaPractice`, which sends `POST /api/v1/test-sets/weak-areas` with `{}` through the generated client.
2. `WeakAreasController.Start` passes the body (validated by `StartWeakAreaPracticeRequestValidator`) to `StartWeakAreaPracticeHandler`.
3. `PlanTierGuard` loads the user's track and plan from the database.
4. `WeakAreaFinder` reads the ids of the user's last 10 submitted attempts on the track (served by `attempts_history_idx`). It then reads their `client_needs` rows from `attempt_category_scores`. `WeakAreaRules.From` sums them per leaf and keeps those under 65% with at least 5 items. If none qualify, the handler returns 422 `NO_WEAK_AREAS`.
5. `WeakAreaPool` asks `QuestionPool` for every published, active question in those leaves within the user's plan. It drops case-study items and reads `user_question_history` to label each question missed, unseen or correct.
6. `WeakAreaPractice.SizeFor` gives 25, or fewer when the pool is short. If under 5, the handler returns 422 `INSUFFICIENT_QUESTIONS`. `WeakAreaSampler` then picks the questions.
7. Inside one transaction, the handler saves a `test_sets` row (`kind = custom`, owned by the user) and its `test_set_items`. It then starts the attempt with `StartAttemptHandler`, the same code as `POST /attempts`, so the email check applies.
8. The 201 body is the same as the custom builder's. The card sends the browser to `/attempts/{id}`.

## 5. Database changes

None. The feature reads `attempts`, `attempt_category_scores`, `user_question_history` and the question tables. It writes ordinary `test_sets` and `test_set_items` rows. `generator_criteria` records `source: "weak_areas"` and the weak leaf ids, with the same other keys as a builder set.

## 6. Key decisions and deviations

- **Client Needs leaves, not F17 subtopics.** The RN bank has 197 subtopics, and about half have 5 questions or fewer, so a subtopic can rarely fill a test or meet a 5-question minimum. The 8 RN leaves have 67 to 339 questions each. F17's "What to study next" (subtopics, one test) stays as it is. The two cards answer different questions.
- **Recent means the last 10 submitted attempts on the track**, read from `attempt_category_scores` rather than `performance_snapshots`. Snapshots are all-time and refreshed by a background job, so they would remember old mistakes for ever and lag a submit by a few seconds.
- **Accuracy is by points over served items**, as in the results breakdown, so the card's percentages agree with what the candidate saw there. Blank answers count as 0. This differs from F17, which ignores blanks.
- **Case studies are left out.** A case study's 6 items span several leaves, so it can't target one area, and it would take a quarter of the test.
- **The served order is shuffled.** Missed-first decides *which* questions are picked, not the order they are served, so the areas are mixed as on the real exam.
- **The size is shown before the click.** `GET /me/weak-areas` returns `questionCount`, so the "only 18 questions" note is on the card rather than a surprise afterwards.
- The design docs now describe all of this: architecture.md 5.7.1, the two endpoints and `NO_WEAK_AREAS` in api.md, and the dashboard and results rows in interfaces.md.

## 7. Tests

```bash
cd nclex-be && caffeinate -i dotnet test     # 1,333 passed
cd nclex-fe && npm run lint && npm run typecheck && npm test   # 436 passed
```

- **Domain** (`WeakAreaRulesTests`, `WeakAreaSamplerTests`): the 65% edge (64.9% weak, 65% not), 4 vs 5 items, summing attempts, partial credit, ordering and test size. The sampler tests cover missed before unseen before correct, test-plan shares, a short leaf handing its places on, a small pool, and a seeded Random giving the same test.
- **Integration** (`WeakAreasTests`, Testcontainers): a new user has nothing and gets `NO_WEAK_AREAS`; strong areas aren't listed. The test is a tutor practice set in the weak leaf containing every missed question. Only the last 10 attempts count. A free user never gets a premium question, even one they missed. Bad track gives 400, and signed-out callers get 401.
- **Frontend** (`WeakAreasCard.test.tsx`): the list, start and navigate, the short-pool note, too few questions, nothing weak, no tests yet, an unverified email, and a load failure. `ResultsPage.test.tsx` now answers the card's request.

## 8. Follow-ups and known limitations

- Body systems and subtopics as targets, once the bank has enough questions per subtopic.
- Case studies in weak-area practice, perhaps a whole case study when most of its items fall in weak leaves.
- The result notification's deep link (`result_fail`) could open the practice test instead of the breakdown.
- Only questions the candidate *reached* go into `user_question_history` (F10), so an item skipped by submitting early counts in the area's score but not as "missed" for picking.
- The card on the results page is the same for every attempt (it is across tests). After a new submit it refreshes when the page loads; nothing invalidates it from the player.

## 9. Try it yourself

1. Change the window from the last 10 attempts to the last 30 days: replace `.Take(WeakAreaRules.RecentAttempts)` in `WeakAreaFinder` with a `SubmittedAt` filter using an injected `TimeProvider`, and adjust the "last N tests" wording on the card.
2. Let the candidate choose 10, 25 or 50 questions: add `questionCount` to `StartWeakAreaPracticeRequest` (with a validator range), pass it to `WeakAreaPractice.SizeFor`, and add a select to the card.
3. Show a "missed before" count on each area: count `PastResult.Missed` candidates per leaf in `GetWeakAreasHandler`, add it to `WeakAreaView`, regenerate `schema.d.ts`, and display it under the score.
