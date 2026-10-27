# F06: Question banks and prebuilt test sets

## 1. Summary

Candidates now have a catalog. The new `/practice` page lists the curated test sets for their track as cards. Each card shows the mode, the number of questions and the timing, plus Sample and Locked badges. "View details" opens a panel with all of the set's settings and a **Start** button, which stays disabled until attempts arrive in F08. Behind the page are three read-only endpoints: `GET /question-banks`, `GET /test-sets` and `GET /test-sets/{id}`. Premium content shows `locked: true` to free accounts, and a reusable plan-tier guard is ready for F08 to refuse starting it. The content importer now also loads banks and curated sets. The sample file seeds one bank and the three sets from PRD 5.5: **RN Sample Set** (10 items), **Comprehensive Practice 1** (tutor, untimed) and **Comprehensive Challenge 1** (exam, 80 minutes, pass estimate at 65%).

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f06`. It was branched from `feature/f05-remaining-graders` (see section 6).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F06_TestSets

dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   Imported 40 RN question(s): 40 created, 0 updated, 0 unchanged.   (or 40 unchanged after F05)
#   Imported 1 bank(s): 1 created, 0 updated, 0 unchanged.
#   Imported 3 test set(s): 3 created, 0 updated, 0 unchanged.
# Running it again prints "unchanged" on every line.

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm run dev                # http://localhost:3000
```

**In the browser:** sign up at http://localhost:3000/signup with track RN, then open http://localhost:3000/practice. You can also go there from the user menu ("Practice") or the dashboard ("Browse practice sets"). The three sets appear in this order: Sample, Practice, Challenge. Choosing "View details" adds `?set=<id>` to the URL, so a reload or a shared link reopens the same panel. A PN account sees an empty state, because there is no PN content yet.

**With curl:**

```bash
TOKEN=$(curl -s -X POST http://localhost:5080/api/v1/auth/signup -H 'Content-Type: application/json' \
  -d '{"email":"f06-demo@example.com","password":"correct-horse-battery","track":"RN"}' \
  | python3 -c "import json,sys;print(json.load(sys.stdin)['accessToken'])")

curl -s "http://localhost:5080/api/v1/test-sets?kind=prebuilt" -H "Authorization: Bearer $TOKEN"
curl -s "http://localhost:5080/api/v1/test-sets/f06a0000-0000-4000-8000-000000000003" -H "Authorization: Bearer $TOKEN"
curl -s "http://localhost:5080/api/v1/question-banks" -H "Authorization: Bearer $TOKEN"
curl -s "http://localhost:5080/api/v1/test-sets?track=PN" -H "Authorization: Bearer $TOKEN"   # {"items":[],"nextCursor":null}
```

**To see a Locked badge:** there is no payment flow, and all seeded content is free. Make a set premium (or edit `accessTier` in the content file and import it again):

```bash
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "update test_sets set access_tier = 'premium' where title = 'Comprehensive Challenge 1';"
# ...and to unlock it for one account:
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "update users set plan_tier = 'premium' where email = 'f06-demo@example.com';"
```

There are no new test accounts.

## 3. Reading order

Domain (`nclex-be/src/Nclex.Domain/`):
1. `Entities/TestSetKind.cs`, `TestMode.cs`, `RevealMode.cs`: the three new Postgres enums.
2. `Entities/TestSetSettings.cs`: everything a candidate sees about a set, plus `HasConsistentTiming`, the DDL timer rule.
3. `Entities/TestSet.cs` and `Entities/TestSetItem.cs`: the set and its numbered items. Includes `SampleQuestionCount = 10`.
4. `Entities/PausePolicy.cs`: the `pause_policy` JSON column.
5. `Access/PlanAccess.cs`: the one-line tier rule.

Infrastructure:

6. `Persistence/Configurations/TestSetConfiguration.cs` and `TestSetItemConfiguration.cs`: the mapping, CHECKs, partial indexes, and the owned-JSON pause policy.
7. `Persistence/Migrations/*_F06_TestSets.cs`: the migration. It has two hand edits (section 6).

Application (`nclex-be/src/Nclex.Application/Catalog/`):

8. `CatalogViewer.cs`: `CatalogViewer` (the user's track and plan) and `PlanTierGuard`, which loads the viewer and has the `EnsureAllowed` check for F08.
9. `TestSetSummary.cs`: the card and detail response records, and `TestSetVisibility` (who can see which sets).
10. `ListTestSets/*`: request, validator, handler and `TestSetPage`. `PageCursor.cs` holds the opaque cursor.
11. `GetTestSet/GetTestSetHandler.cs` and `ListQuestionBanks/*`.
12. `../Content/ImportQuestions/QuestionImportFile.cs` (the new `banks` and `testSets` sections), then `CatalogImporter.cs` (validation and upserts), then the four changed lines in `ImportQuestionsHandler.cs`.

API:

13. `Controllers/TestSetsController.cs` and `QuestionBanksController.cs`: thin controllers.
14. `OpenApi/CamelCaseQueryParameters.cs`: documents `?track=&kind=` in camelCase.

Content:

15. `nclex-be/content/sample-questions.rn.json`: the `banks` and `testSets` sections at the end.

Frontend (`nclex-fe/src/`):

16. `features/practice/api/practiceApi.ts`: fetchers and TanStack Query hooks (an infinite query for pages).
17. `features/practice/format.ts`: labels for mode, timing, percentages and pauses.
18. `features/practice/components/TestSetCard.tsx`, then `TestSetDetailPanel.tsx`, then `PracticeCatalog.tsx`.
19. `app/practice/page.tsx`: the route, behind `RequireAuth`.

## 4. Request flow: `GET /api/v1/test-sets?kind=prebuilt`

1. **UI:** `PracticeCatalog` calls `usePrebuiltSets()`, an infinite query keyed `["test-sets","prebuilt"]`. Its `fetchPrebuiltSets(null)` calls `apiClient.GET("/api/v1/test-sets", { query: { kind, limit: 20 } })`. The auth middleware adds the bearer token, and the Next.js rewrite forwards the call to the .NET API.
2. **API:** `TestSetsController.List` binds the query string to `ListTestSetsRequest`. `FluentValidationFilter` runs `ListTestSetsRequestValidator`, which allows only `kind=prebuilt`, a limit from 1 to 50, and a cursor this API made. Otherwise it returns 400 `VALIDATION_FAILED`.
3. **Application:** `ListTestSetsHandler` asks `PlanTierGuard.LoadViewerAsync` for the user's track and plan. That is one `SELECT` on `users`; a deleted account gets 401. When `track` is missing, the user's own track is used. The handler then builds the query: `TestSetVisibility.VisibleTo(user)` plus track and kind, ordered by `is_sample DESC, mode, title, id`, with `OFFSET` taken from the cursor and `LIMIT limit + 1`.
4. **Database:** the filter on published sets by track and kind matches the partial index `ix_test_sets_track_kind`. Postgres sorts `mode` in enum order (practice, then challenge, then cat), which is why the enum labels must be in DDL order.
5. **Domain:** each row becomes a `TestSetSummary`. `locked` comes from `viewer.IsLocked(set.AccessTier)`, which is `!PlanAccess.Allows(plan, tier)`. `myLastAttempt` is null until F08 and F10.
6. **Back:** `TestSetPage { items, nextCursor }` is serialised as camelCase with snake_case enums. `nextCursor` is set only when the extra row came back. The cards render. "Show more sets" calls `fetchNextPage` with that cursor.

Opening a card calls `router.replace("/practice?set=<id>")`. `TestSetDetailPanel` then loads `GET /test-sets/{id}` and moves focus to its heading. "Close" clears the parameter and puts focus back on the card's button.

## 5. Database changes

Migration **`F06_TestSets`** (database.md 2.1 and 2.5):
- **Enums**:
  - `test_set_kind` (prebuilt, custom, cat, assigned)
  - `test_mode` (practice, challenge, cat)
  - `reveal_mode` (tutor, exam)
- **`test_sets`**: every column, default and type in the DDL.
  - `pass_threshold numeric(5,4)`
  - `pause_policy jsonb` with the default `{"maxPauses":null,"maxPauseSec":null}`
  - `allow_back_nav` default true
  - `generator_criteria` and `cat_config` as nullable `jsonb`, unused until F07 and phase 2
  - Two CHECKs: a custom set needs an owner, and a timed set needs a timer.
- **`test_set_items`**: PK `(test_set_id, position)`, `UNIQUE (test_set_id, question_id)`, and FKs to `test_sets` (`ON DELETE CASCADE`) and `questions`. Items point at the question, not a version. The current published version is chosen when an attempt starts.
- **Indexes**:
  - `ix_test_sets_track_kind ON (track, kind) WHERE is_published`: the catalog listing.
  - `ix_test_sets_owner_user_id ON (owner_user_id) WHERE kind = 'custom'`: "my custom sets" in F07.
  - `ix_test_set_items_question_id`: EF adds it for the FK. It isn't in the DDL; F04's `question_bank_items` has the same one.

`question_banks` and `question_bank_items` already existed from F04 and are unchanged.

## 6. Key decisions and deviations

- **Branch base.** CLAUDE.md says to branch from `main`, but F01 to F05 aren't merged yet and F06 needs their questions and importer. The branch is stacked on `feature/f05-remaining-graders`. Merge in order: F03, F04, F05, then F06.
- **Shorter Comprehensive sets (agreed with you).** The real NCLEX is 85 to 150 items, including 15 unscored pretest items. The content file has only 40 RN questions, so both Comprehensive sets use all 40, and the Challenge timer is 80 minutes (4,800 s). The Challenge set serves the same 40 questions interleaved by type rather than in file order. The importer enforces exactly 10 items for sample sets, but sets no minimum for Comprehensive sets yet.
- **Where the plan-tier check lives.** architecture.md section 4 lists a "PlanTier" policy under `Api/Auth`. An ASP.NET policy based on the resource would need the set loaded in the controller. Instead, the rule is a pure Domain function (`PlanAccess`) and the guard is in Application (`PlanTierGuard.EnsureAllowed`, which returns 403 `PLAN_UPGRADE_REQUIRED`). Controllers stay thin, and F08's start handler can call the guard after loading the set.
- **Premium sets are shown, not refused.** `GET /test-sets/{id}` returns 200 with `locked: true` for a premium set, so the panel can explain why. The 403 is saved for starting an attempt.
- **Cursor format.** api.md asks for an opaque cursor. Here it is base64url of `o:<offset>`. The catalog is small and sorted by sample flag, mode and title, so a keyset cursor over that order would add complexity without benefit. Clients must treat it as opaque, so it can change later without breaking them.
- **`GET /question-banks` isn't paged.** It returns `{ items }`, because banks are few. `questionCount` counts only active questions that have a published version.
- **Default track.** api.md shows `?track=RN`. When it is left out, both list endpoints use the signed-in user's track, which is what `/practice` relies on.
- **`kind` accepts only `prebuilt` for now.** Any other value returns 400 `INVALID_VALUE` on `kind`. Custom sets get listed in F07.
- **Visibility.** Candidates see published non-custom sets, plus their own custom sets (ready for F07). Unpublished and unknown sets return 404 `NOT_FOUND`.
- **Two hand edits to the migration** (as F04 did):
  - The enum labels are listed in DDL order, because EF writes them alphabetically and the order decides sorting.
  - The `pause_policy` default is added, because EF can't express a default on an owned JSON column.
- **OpenAPI query names.** Query strings bound to a request record were documented as `Track` and `Kind`, so the generated client used PascalCase. A small operation transformer documents them in camelCase, as api.md writes them. Binding ignores case, so the API behaves the same either way. This also changes `/taxonomy`'s `track` parameter in the generated client.
- **Import file format.** The file gains optional `banks` and `testSets` sections, and the importer stays all or nothing. Imported sets are always `prebuilt` and published. A bank or set may use questions from the same file, or questions already published on the same track. New issue code: `UNKNOWN_QUESTION`.
- **Navigation.** A "Practice" link in the user menu and a "Browse practice sets" button on the dashboard, so the page can be reached. Neither was listed in the feature, but both are small.

## 7. Tests

| Where | What |
|---|---|
| `Nclex.Domain.Tests/Access/PlanAccessTests.cs` | The tier rule in every combination |
| `Nclex.Domain.Tests/Entities/TestSetTests.cs` | Creating and updating prebuilt sets, timing consistency, item numbering |
| `Nclex.Application.Tests/Catalog/*` | List validator (kind, limit, cursor, track), `PageCursor` round trip, `PlanTierGuard.EnsureAllowed` |
| `Nclex.Api.IntegrationTests/Catalog/TestSetsTests.cs` | Order and card fields, JSON contract (null `myLastAttempt`, no item ids, no keys or rationales), **tier locking** for free and premium users, **track filtering** (default, RN, PN), unpublished sets hidden, cursor paging, detail fields, 404, 400s, 401 |
| `Nclex.Api.IntegrationTests/Catalog/QuestionBanksTests.cs` | Counts that skip retired questions, locking, track filtering, 401 |
| `Nclex.Api.IntegrationTests/Content/ImportCatalogTests.cs` | Create, then unchanged on re-import; reordering and changed settings update in place; reuse of earlier questions; every validation issue; a set can't change track |
| `Nclex.Api.IntegrationTests/Content/ImportQuestionsTests.cs` | The real sample file seeds the 3 sets and the bank with the expected settings |
| `Nclex.Api.IntegrationTests/MigrationTests.cs` | New enum label order, partial indexes, CHECKs and DDL defaults |
| `nclex-fe/.../practice/format.test.ts` | Duration, timing, percentage and pause labels |
| `nclex-fe/.../TestSetCard.test.tsx` | Mode, count, timing, Sample and Locked badges, keyboard opening, `aria-expanded` and `aria-controls` |
| `nclex-fe/.../TestSetDetailPanel.test.tsx` | Settings, focus on the heading, disabled Start with its reason (normal and locked), 404 message, Close |
| `nclex-fe/.../PracticeCatalog.test.tsx` | Lists the three seeded sets in API order, puts `?set=` in the URL, opens the panel from the URL, "Show more sets", empty state |

Run them:

```bash
cd nclex-be && dotnet build && dotnet test      # 571 passed, 0 failed (Docker must be running)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # lint and typecheck clean; 99 tests in 22 files passed
```

I also checked the feature end to end against a scratch Postgres:
- Importing the sample file twice gave "created" and then "unchanged".
- `GET /test-sets` returned the three sets in order.
- `npm run build` succeeded, and `/practice` served 200 through the proxy.

I didn't drive the page in a real browser; the end-to-end browser tests start with Playwright in F16.

## 8. Follow-ups and known limitations

- **F08:** enable **Start**. The start handler should call `PlanTierGuard.EnsureAllowed(viewer, set.AccessTier)` and require a verified email unless `is_sample`.
- **F08 and F10:** fill `myLastAttempt` from the user's latest submitted attempt.
- **Full-length sets:** after more content is written, raise both Comprehensive sets to 85 to 150 items with a 5-hour budget. Then consider a minimum size for prebuilt Comprehensive sets in the importer.
- **Unscored pretest items** (15 per NCLEX) need a way to mark items as unscored, which means a schema change. They need their own feature.
- **PN content:** PN users see an empty `/practice` page until PN questions and sets are imported.
- **Banks have no candidate screen.** `GET /question-banks` exists, but interfaces.md 5.2 lists no candidate route for banks. Admin management comes with F14.
- **The top-bar track badge** from interfaces.md 5.2 is still pending.
- **Flaky existing test:** `OutboxDispatcherTests.An_email_still_within_its_claim_lease_is_left_alone` (F02 and F03) failed once and then passed on the rerun. `ReleaseExpiredClaimsAsync` works on the whole outbox, so a parallel test's expired claim can be counted. Scope the assertion to the test's own row.

## 9. Try it yourself

1. **Add a "Pharmacology Quick 5" set.** Append a set to `testSets` in the content file:
   - five `f04a…`/`f05a…` question ids tagged `PHI.PPT`
   - `"timed": true` and `"perQuestionSec": 90`

   Re-import the file. The card should read "90 s per question". Then try `"timed": true` without a timer and read the importer's error.
2. **Show the reveal mode on the card.** `TestSetCard` lists mode, count and timing. Add `revealModeLabel(set.revealMode)` as a fourth fact, and extend `TestSetCard.test.tsx` to expect "Answers at the end" for the challenge set.
3. **Sort banks by size.** In `ListQuestionBanksHandler`, order by `QuestionCount` descending, then name. Write an integration test with two banks of different sizes. Why does the `OrderBy` have to come after the projection?
