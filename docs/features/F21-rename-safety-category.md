# F21: Rename the RN safety category

## 1. Summary

The NCLEX-RN Client Needs category "Safety and Infection Control" is now called **"Safety and Infection Prevention and Control"**, the wording of the 2026 NCLEX-RN test plan (in force April 2026 to March 2029). The new name appears wherever candidates see categories: the custom builder, dashboard, results, performance and weak-area cards. The RN bank's two Safety practice sets get the new name too. Only the name changes: the code `SECE.SIC`, the 10–16% weight and the id stay the same, so tagged questions, scores and history are untouched. The PN taxonomy keeps its old name.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f21`, on `feature/f21-rename-safety-category`, branched from `main` at 4152517.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api     # applies F21_RenameSafetyCategory
for f in content/rn-bank/rn-bank-*.rn.json; do dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$f"; done

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000
```

On a database that already has the bank, only the last file needs re-importing. It holds the catalog and reports `32 test set(s): 0 created, 2 updated, 30 unchanged`:

```bash
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/rn-bank/rn-bank-67-moc-sic-ngn.rn.json"
```

Check it:

```bash
curl -s "http://localhost:5080/api/v1/taxonomy?track=RN" | jq '.clientNeeds[0].children[1].name'
# "Safety and Infection Prevention and Control"
```

In the app, open **Practice**: the sets read "Safety and Infection Prevention and Control, part 1 of 2" and "part 2 of 2". On **/practice/custom** the category checkbox has the new name.

**Deploying:** `deploy/vm/deploy.sh deploy` runs the migration. The renamed sets need a re-import too, so use `deploy/vm/deploy.sh deploy --seed`, which imports the sample file and every bank file; unchanged questions are skipped.

## 3. Reading order

1. `nclex-be/src/Nclex.Infrastructure/Persistence/Seed/TaxonomySeedData.cs`: row 3 with the new name.
2. `nclex-be/src/Nclex.Infrastructure/Persistence/Migrations/20261119031041_F21_RenameSafetyCategory.cs`: one `UpdateData` on `client_needs_categories`, with a `Down` that restores the old name.
3. `nclex-be/tests/Nclex.Api.IntegrationTests/MigrationTests.cs` (`F21_migration_…`): the RN row is renamed and the PN row is not.
4. `nclex-be/content/rn-bank/catalog.py` (`CATEGORY_NAMES`): where the practice-set titles come from.
5. `nclex-be/content/rn-bank/rn-bank-67-moc-sic-ngn.rn.json`: the rebuilt catalog with the renamed sets.

## 4. Request flow

`GET /api/v1/taxonomy?track=RN`, which the custom builder and every category label depend on:

1. **UI.** The custom builder's `CategoryTree` asks TanStack Query for the taxonomy.
2. **API.** `TaxonomyController` passes the request to `GetTaxonomyHandler`.
3. **Application.** The handler reads `client_needs_categories` for the track and builds the tree.
4. **Database.** Row 3's `name` is now the new name, set by the F21 migration.
5. **Back to the UI.** The checkbox is labelled with the name the API returns. Dashboard, results and weak-area cards join their scores to the same row by id, so they show the new name without any code change.

## 5. Database changes

- Migration **`F21_RenameSafetyCategory`**: `client_needs_categories` row 3 (RN, `SECE.SIC`) gets `name = 'Safety and Infection Prevention and Control'`. `Down` restores "Safety and Infection Control". The column is `text`, so the longer name fits.
- No schema change. No table stores a copy of the category name: scores, snapshots and questions refer to it by id.

## 6. Key decisions and deviations

- **RN only.** The PN row (13) keeps "Safety and Infection Control" until the 2026 PN test plan's wording is checked. `database.md` says so.
- **Only category labels changed in the content.** The catalog, review sheet, batch titles and docstrings use the new name. One question rationale in batch 66 says "Safety and infection control." as plain prose; I left it, because editing it would publish a new version of that question for no clinical reason.
- **Rebuilt files were diffed:** only the two Safety sets' titles and descriptions and three file notices changed. A trial import into a scratch database renamed the two sets in place (same ids), and no new question versions were created.
- **The weights stay at 10–16%.** The 2026 test plan kept every percentage from 2023.

## 7. Tests

- **Integration:** `MigrationTests.F21_migration_uses_the_2026_name_for_the_rn_safety_category_only`. `TaxonomyTests` and `GradingTests` (score breakdown) expect the new name.
- **Domain:** a comment in `ItemSelectorTests` only.
- **Frontend:** test data in the custom builder, dashboard and weak-areas tests uses the new name.

Run everything: `cd nclex-be && dotnet build && dotnet test`, then `cd nclex-fe && npm run lint && npm run typecheck && npm test`.

Results on this branch: `dotnet build` 0 warnings and 0 errors; `dotnet test` 1,421 passed; `npm run lint` and `npm run typecheck` clean; `npm test` 475 passed (76 files).

## 8. Follow-ups and known limitations

- **Check the 2026 PN test plan** and rename PN row 13 the same way if it changed.
- **The other 2026 plan changes are wording only** (the same eight categories and weights), so nothing else in the taxonomy needs updating.
- **Next:** the content waves that grow the RN bank toward 5,000 questions.

## 9. Try it yourself

1. **Roll it back and forward.** Run `dotnet ef database update F17_Subtopics -p src/Nclex.Infrastructure -s src/Nclex.Api` (the migration before F21), check the name in the taxonomy, then `dotnet ef database update` again.
2. **Rename a set title only.** Change the `description` wording in `catalog.py`'s `_practice` call, run `python3 -I build_all.py`, and re-import file 67. Check the import says "2 updated" or more, and "0 created".
3. **Write the PN migration.** Change row 13 in `TaxonomySeedData.cs`, run `dotnet ef migrations add Try_PnRename …`, and read what EF generates. Then remove it with `dotnet ef migrations remove`.
