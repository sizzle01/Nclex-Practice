# F07: Custom test builder

## 1. Summary

Candidates can now build their own practice test at `/practice/custom`. They choose:
- Client Needs categories, from a tree where a category's box selects all of its subcategories
- body systems, question types and difficulty
- which questions to draw from: all, unused, incorrect or marked for review
- how many questions, the mode, when answers are revealed, and the timing

While they choose, a live count shows how many questions match. It updates once the filters have stayed unchanged for 400 ms.

On submit, the backend builds the set:
1. It runs a hand-written SQL pool query over published questions on the user's track and plan tier.
2. When several categories are chosen, it samples in proportion to the NCLEX test-plan weights.
3. It fills any shortfall at random.
4. It keeps each case study's items together.

The new set is saved as the user's own custom set, and the page confirms it with the item count. Starting it arrives with F08.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f07`. It was branched from `main`, which includes F06.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F07_QuestionHistory
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"   # if not done for F06
dotnet run --project src/Nclex.Api                    # http://localhost:5080
cd ../nclex-fe && npm run dev                         # http://localhost:3000
```

**In the browser:**
1. Sign up with track RN.
2. Open http://localhost:3000/practice and choose **Build a custom test**, or go straight to `/practice/custom`.
3. Tick **Physiological Integrity**. Its four subcategories tick too, and the count reads "29 questions match your filters."
4. Set 20 questions, choose Challenge, tick Timed with 30 minutes, and press **Create test**.

The confirmation shows "Custom test: 20 questions". Ask for more than match and the count turns amber with a hint.

**With curl:**

```bash
TOKEN=$(curl -s -X POST http://localhost:5080/api/v1/auth/signup -H 'Content-Type: application/json' \
  -d '{"email":"f07-demo@example.com","password":"correct-horse-battery","track":"RN"}' \
  | python3 -c "import json,sys;print(json.load(sys.stdin)['accessToken'])")
B=http://localhost:5080/api/v1/test-sets/custom

curl -s "$B/available-count?clientNeedsIds=6" -H "Authorization: Bearer $TOKEN"                      # {"available":29}
curl -s "$B/available-count?itemTypes=multiple_choice&difficulties=moderate" -H "Authorization: Bearer $TOKEN"   # {"available":6}

curl -s -X POST $B -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"clientNeedsIds":[6],"questionCount":20,"mode":"challenge","revealMode":"exam","timing":{"timed":true,"timeBudgetSec":1800}}'
# 201 {"testSet":{"kind":"custom","title":"Custom test: 20 questions","questionCount":20,"passThreshold":0.65,...},"attempt":null}

curl -s -X POST $B -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"difficulties":["difficult"],"questionCount":30,"mode":"practice","revealMode":"tutor"}'
# 422 {"code":"INSUFFICIENT_QUESTIONS","available":6,...}
```

**See the test-plan proportions** in a set you created (replace the id):

```bash
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "select c.code, count(*) from test_set_items i join questions q on q.id = i.question_id
   join question_versions v on v.id = q.current_version_id join client_needs_categories c on c.id = v.client_needs_id
   where i.test_set_id = '<id>' group by 1 order by 1;"
# In one run of the 20-question PHI set above: BCC 2, PA 5, PPT 8, RRP 5
```

What those numbers mean:
- The weights (the midpoints of each leaf's range) give BCC 4, PPT 6, RRP 5 and PA 5.
- BCC has only 2 questions in the content, so its quota is capped at 2.
- The 2 missing places were filled at random. In this run both landed on PPT, giving 8.

There are no new test accounts. The "unused" and "incorrect" pools depend on `user_question_history`, which grading fills in F10. Until then, "unused" is every question and "incorrect" is none.

## 3. Reading order

Domain (`nclex-be/src/Nclex.Domain/`):
1. `Builder/PoolItem.cs`: one pool row (question, leaf, case-study position) and `LeafWeight`.
2. `Builder/TestPlanAllocation.cs`: proportional quotas using the largest-remainder method, capped at what each leaf has available.
3. `Builder/SetSampler.cs`: quotas, then random fill, then case-study blocks, then a final shuffle that keeps each case together.
4. `Entities/UserQuestionHistory.cs` and `Entities/TestSet.cs`: the new table, and `TestSet.CreateCustom`.

Application (`nclex-be/src/Nclex.Application/Catalog/CustomSets/`):

5. `PoolFilters.cs`, `PoolMode.cs` and `PoolFiltersValidation.cs`: the filters both endpoints share.
6. `QuestionPool.cs`: the hand-written SQL, parent-to-leaf expansion, leaf weights, and dropping incomplete case studies.
7. `GetAvailableCountHandler.cs`.
8. `CreateCustomSetRequest.cs`, then `CreateCustomSetRequestValidator.cs`, then `CreateCustomSetHandler.cs`.
9. `../../Common/AppException.cs` (the new `details`) and `../CatalogErrors.cs` (`INSUFFICIENT_QUESTIONS`).

API and infrastructure:

10. `Nclex.Api/Controllers/TestSetsController.cs`: the two `custom` actions.
11. `Nclex.Api/ModelBinding/SnakeCaseEnumModelBinder.cs`: lets `?itemTypes=multiple_choice` bind (section 6).
12. `Nclex.Infrastructure/Persistence/Configurations/UserQuestionHistoryConfiguration.cs` and the `F07_QuestionHistory` migration.

Frontend (`nclex-fe/src/features/custom-builder/`):

13. `api/builderApi.ts`: taxonomy, count and create, plus their hooks.
14. `schema.ts` and `useDebouncedValue.ts`.
15. `components/CategoryTree.tsx`, `CheckboxGroup.tsx` and `AvailableCount.tsx`.
16. `components/CustomBuilder.tsx` (the form), then `CreatedSetCard.tsx`.
17. `src/app/practice/custom/page.tsx`.

## 4. Request flow: `POST /api/v1/test-sets/custom`

1. **UI:** `CustomBuilder` validates the form with Zod (5 to 150 questions, and a timer when the test is timed). `toRequest` turns minutes into `timeBudgetSec` and adds the user's track and `startNow: false`. `useMutation` calls `createCustomSet`, which calls `apiClient.POST("/api/v1/test-sets/custom")`, and the Next.js rewrite forwards it to the API.
2. **API:** `TestSetsController.CreateCustom` binds `CreateCustomSetRequest`. `FluentValidationFilter` runs `CreateCustomSetRequestValidator`, which checks:
   - the count is from 5 to 150
   - the mode is practice or challenge
   - the reveal mode is present
   - the timing is consistent
   - every enum value in the filters is valid
3. **Application:** `CreateCustomSetHandler`:
   - Loads the viewer (track and plan) through `PlanTierGuard`.
   - Loads the pool through `QuestionPool.LoadAsync`, which:
     - expands parent categories to their leaves and works out each leaf's weight (the midpoint of its range)
     - checks that the body-system ids exist; unknown ids become 400 field errors
     - runs the SQL (step 4)
     - drops any case study with items missing from the result
4. **Database:** one query joins `questions` to their `current_version_id` in `question_versions`. It filters by:
   - track
   - `status='active'` and `published`
   - tier (`access_tier='free' OR premium user`)
   - leaf, body-system, item-type and difficulty arrays, where an empty array means "any"
   - the pool mode, through `user_question_history` (`NOT EXISTS` for unused, `last_correct = false` for incorrect)
5. **Domain:** `SetSampler.Sample(pool, count, leaves, Random.Shared)` returns the ordered ids. If there are fewer than `count`, the handler throws `INSUFFICIENT_QUESTIONS` with `available` (422).
6. **Save:** `TestSet.CreateCustom` sets:
   - `kind=custom`
   - the owner
   - `is_published=false`
   - `generator_criteria` holding the filters as chosen
   - a generated title
   - `pass_threshold` 0.65 for challenge sets

   The set row and its `test_set_items` (positions 1..N) are saved in one `SaveChanges`.
7. **Back:** the response is `201 Created` with `Location: /api/v1/test-sets/{id}` and `{ testSet: TestSetDetail, attempt: null }`. `CreatedSetCard` replaces the form and moves focus to "Your test is ready".

The live count follows the same path through `GetAvailableCountHandler` and returns `{ available: pool.Items.Count }`. The frontend sends it only after the filters have stayed unchanged for 400 ms. `keepPreviousData` keeps the old number on screen until the new one arrives.

## 5. Database changes

Migration **`F07_QuestionHistory`** adds `user_question_history` exactly as database.md 2.7 defines it:
- **Key:** `(user_id, question_id)`.
- **Columns:** `times_seen int NOT NULL DEFAULT 0`, `last_correct boolean`, `last_seen_at timestamptz NOT NULL`.
- **Foreign keys:** `ON DELETE CASCADE` to `users`, and a plain FK to `questions`. EF also adds `ix_user_question_history_question_id` for that FK.

The table powers the "unused" and "incorrect" pools. Nothing writes to it yet; F10's grading will.

No index was added for the pool query. It filters on the current version reached through `questions.current_version_id`, and the sample content is small. Revisit with `EXPLAIN` once the question bank is real-sized (see section 8).

## 6. Key decisions and deviations

- **The "flagged" pool is empty until F08.** We agreed it means questions the user marked for review in an attempt (`attempt_items.flagged`). That table doesn't exist yet, so the SQL returns nothing for this mode, and the form labels it "Marked for review".
- **Case studies:**
  - A case study is used only when every one of its items passes the filters, so a set never has part of a case.
  - Cases take part only in the random fill, not the proportional step, because their items can span several leaves.
  - A case that doesn't fit the remaining space is skipped.

  The sample content has no case studies. Integration tests create one directly in the database, because the importer can't author them yet (F14).
- **Test-plan weights** are the midpoint of each leaf's `weight_min` to `weight_max` range. Shares are rounded with the largest-remainder method, so they always add up to the requested count. Ties go to the lower category id, which makes results repeatable.
- **`available` on 422** is the number of questions the sampler could actually put together. That is usually the pool size. With case studies it can be lower, because a 6-item block can't fill 5 places.
- **Challenge sets get `pass_threshold` 0.65,** as agreed, matching the seeded Comprehensive Challenge. Practice sets get none. The title is generated as "Custom test: N questions".
- **`startNow`** is accepted, and `attempt` is always null until F08. The form sends `false`.
- **Track** defaults to the user's own track when the request leaves it out, as in F06.
- **Custom sets** use `access_tier='free'`, because the questions were already limited to the user's tier. They are unpublished and visible only to their owner through `GET /test-sets/{id}`. They aren't listed anywhere yet.
- **Bug fix for query-string enums.** MVC binds enums in query strings by their C# names, so `?itemTypes=multiple_choice` returned 400, while `sata` happened to work. The new `SnakeCaseEnumModelBinder` accepts the API's own names (snake_case, or an explicit name such as `RN`), the same ones JSON uses. Every query-string enum is affected, but the existing F04 and F06 tests still pass.
- **`IAppDbContext.SqlQuery<T>`** was added so the Application layer can run the hand-written pool SQL. The snake_case naming convention applies to these results too, so the SQL aliases its columns in snake_case (`question_id`, and so on).
- **Errors:**
  - `AppException` gained `details`, extra top-level fields in the error body, used here for `available`.
  - `AppErrorKind` gained `Unprocessable` (422).
- **Randomness:** production uses `Random.Shared`. The sampler takes a `Random`, so unit tests pass a fixed seed, and sorting the pool first makes a seed give the same set whatever order the rows arrive in.
- **Test setup:** the integration-test host now creates the content-importer account when it starts (section 8).

## 7. Tests

| Where | What |
|---|---|
| `Nclex.Domain.Tests/Builder/TestPlanAllocationTests.cs` | Exact shares for the RN PHI weights, capping, an empty leaf, tie-breaking |
| `Nclex.Domain.Tests/Builder/SetSamplerTests.cs` (seed 20261027) | Proportions (31 split 18:13), random top-up for a short leaf, no duplicates, same seed gives the same set whatever the pool order, case study kept whole and in order, a case that doesn't fit left out, a small pool returns everything it has |
| `Nclex.Domain.Tests/Entities/TestSetTests.cs` | `CreateCustom`: owner, unpublished, criteria |
| `Nclex.Application.Tests/Catalog/CreateCustomSetRequestValidatorTests.cs` | Required fields, 5 to 150 inclusive, no CAT, timing rules, undefined enum values, shared filter rules |
| `Nclex.Api.IntegrationTests/Catalog/QuestionPoolTests.cs` | Parent expands to leaves (and the weights), body-system, difficulty and item-type filters, tier, track, retired questions, unused/incorrect/flagged pools from history, case studies only when complete, unknown ids as field errors |
| `Nclex.Api.IntegrationTests/Catalog/CustomSetsTests.cs` | Exact live count, **set created with the right number of items** (owner, positions 1..N, unique items, criteria, `Location`), only the owner sees it, **`INSUFFICIENT_QUESTIONS` with `available`**, 400 field errors, unknown category, 401 |
| `Nclex.Api.IntegrationTests/MigrationTests.cs` | `user_question_history` columns, defaults and cascade |
| `nclex-fe/.../CategoryTree.test.tsx` | A parent selects and clears its leaves, mixed state, a category without children counts as a leaf, keyboard use |
| `nclex-fe/.../schema.test.ts` | Count limits, timed tests need a timer |
| `nclex-fe/.../CustomBuilder.test.tsx` | Count for the user's track, **one count request after quick clicks (debounce)**, only servable item types, "too many" warning, submit then confirmation with the item count and the exact request body, client-side timer error, 422 message |

The pool and endpoint tests share one database with every other test. So they check which of their own questions are in or out, or they build from a fresh user's "incorrect" pool, where every number is exact.

```bash
cd nclex-be && dotnet build && dotnet test        # 616 passed, 0 failed (Docker must be running)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # clean, clean, 118 tests in 25 files passed
```

I also checked it end to end against a scratch Postgres loaded with the sample content:
- The counts were right.
- A 20-question PHI challenge set was created with the proportions shown in section 2.
- The 422 response included `available`.
- `npm run build` succeeded, and `/practice/custom` served 200 through the proxy.

I didn't drive the form in a real browser; Playwright arrives in F16.

## 8. Follow-ups and known limitations

- **F08:**
  - honour `startNow` and return the attempt
  - make the "flagged" pool read `attempt_items.flagged`
  - enable **Start** on the confirmation card
- **F10:** grading must create and update `user_question_history` (`times_seen`, `last_correct`, `last_seen_at`). `UserQuestionHistory.FirstSeen` is the starting point.
- **Importer race (from F04):** `ImportQuestionsHandler` creates the content-importer user on first use. Two first imports at the same moment both try, and one fails on `ix_users_email`. The new parallel tests hit this, so the test host now creates the account up front. The product code should handle it too, for example with `INSERT … ON CONFLICT DO NOTHING`.
- **"My custom sets"** has no list yet, and nothing offers "retake similar" from `generator_criteria`.
- **`Idempotency-Key`** for `POST /test-sets/custom` (api.md section 1) isn't implemented; there's no idempotency infrastructure yet.
- **Pool query performance:** check it with `EXPLAIN` on a realistic bank. A partial index on `question_versions (id) WHERE status = 'published'`, or filtering through `qv_filter_idx`, may help.
- **Generated types for `/taxonomy`** mark `children`, `weightMin` and `weightMax` as required. The API leaves them out when null, so test fixtures spell out `null`. The OpenAPI document should mark them optional.
- **PN:** the builder works for PN users, but there's no PN content yet, so their count is 0.

## 9. Try it yourself

1. **Show each category's weight in the tree.** In `CategoryTree.tsx`, add "(15–21%)" after each subcategory name, using `weightMin` and `weightMax`. Update `CategoryTree.test.tsx` to expect it.
2. **Change how weights are worked out.** In `QuestionPool.Midpoint`, use `weight_max` instead of the midpoint. Predict the new quotas for 20 PHI questions by hand, then check them with `TestPlanAllocationTests`.
3. **Make case studies take part in the proportional step.** In `SetSampler`, give each case study the leaf of its first item and let it fill a quota when it fits. Write a seeded test where a case study fills one leaf's whole quota.
