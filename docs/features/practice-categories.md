# Practice page categories

## 1. Summary

The Practice page used to show every curated set as one long grid. With the RN bank there are about 36 sets, and it was hard to tell a topic set from a mock exam. The page now has filter tabs (**All, Practice, Timed, Untimed, Adaptive**), each showing how many sets it holds. Under the tabs, sets are grouped into sections: **Start here** (the sample), **Practice sets**, **Case studies**, **Mock exams: timed**, **Mock exams: untimed** and **Adaptive exams**. The chosen tab is kept in the URL (`/practice?filter=timed`), so a reload or a shared link opens the same tab. This change is frontend only: no migration and no API change.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-categories`, and branches from `main` (F00 to F17 + RN bank).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet build
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
for f in content/rn-bank/rn-bank-*.rn.json; do
  dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/$f"
done
dotnet run --project src/Nclex.Api                 # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev       # http://localhost:3000
```

As a candidate on an RN account (sign up; the sample set needs no verified email):

1. Open <http://localhost:3000/practice>. The **All** tab shows every section. The tab labels read e.g. "Timed 4", "Adaptive 0".
2. Click **Timed**: only the four timed NCLEX-RN challenges are left, under "Mock exams: timed". The URL is now `/practice?filter=timed`. Reload, and the Timed tab is still selected.
3. Open a set's details, then switch tab: the details panel stays open (`?filter=…&set=…`).
4. Click **Adaptive**: it says adaptive exams are coming soon (F19 hasn't been built yet).
5. Keyboard: Tab onto the selected tab, then use Left/Right, Home and End to switch tabs. Tab again goes into the cards.

## 3. Reading order

1. `nclex-fe/src/features/practice/setCategories.ts`: the rules. Which tab shows which set, which section a set belongs to, and section order. These are pure functions.
2. `nclex-fe/src/features/practice/setCategories.test.ts`: the rules as examples.
3. `nclex-fe/src/features/practice/api/practiceApi.ts`: `fetchAllPrebuiltSets` follows `nextCursor` until the last page, 50 sets per request.
4. `nclex-fe/src/features/practice/components/PracticeCatalog.tsx`: the page. It reads `?filter=` and `?set=` from the URL, then shows the tabs, the tab panel and the details panel.
5. `nclex-fe/src/features/practice/components/SetFilterTabs.tsx`: the WAI-ARIA tab list (roving tabindex, arrow keys). Copied from the case-study `ExhibitTabs`, with selection kept in the URL.
6. `nclex-fe/src/features/practice/components/SetSectionList.tsx`: one `<section>` per group, each with an h2 heading, a one-line description and a grid of cards.
7. `nclex-fe/src/features/practice/components/TestSetCard.tsx`: unchanged except that the card title is now an h3, below the section's h2.
8. `nclex-fe/src/features/practice/components/PracticeCatalog.test.tsx`: tests for the page.

## 4. Request flow

Opening `/practice?filter=timed`:

1. **UI:** `PracticeCatalog` calls `usePrebuiltSets()`, a TanStack Query under the key `["test-sets","prebuilt"]`.
2. **API client:** `fetchAllPrebuiltSets` calls `GET /api/v1/test-sets?kind=prebuilt&limit=50`. If the reply has a `nextCursor`, it asks again with `cursor=…` until there is none. Today's RN catalog fits in one request.
3. **API → Application → database:** unchanged from F06. The `ListTestSets` slice reads the published `test_sets` of the user's track and returns `TestSetSummary` items: `mode`, `timed`, `isSample` and so on. It returns no answer keys or rationales.
4. **Back in the UI:** `parseFilter("timed")` picks the tab. `filterSets` keeps the sets where `timed` is true, and `groupSets` puts each one in a section with `sectionOf` and drops empty sections. `SetFilterTabs` counts each tab with `filterSets(sets, filter).length`.
5. **Clicking a tab** calls `router.replace("/practice?filter=…&set=…", { scroll: false })`. That changes the URL without adding a history entry, and the page re-renders from the new search params.

## 5. Database changes

None. Every field the page needs (`mode`, `timed`, `isSample`, `title`) is already in `GET /test-sets`.

## 6. Key decisions and deviations

- **Case studies are recognised by title.** Nothing in the data marks a set as a case study. A practice set whose title contains "case stud" (as in "NGN case studies, set 1 of 2", from `rn-bank/catalog.py`) goes under Case studies. If titles change, or more categories are wanted, add a category column to `test_sets` (migration plus API field) and use it in `sectionOf`. This was the default in the plan, and you said "go".
- **What each tab shows.**
  - **Practice:** every practice-mode set (sample, practice sets, case studies).
  - **Timed / Untimed:** by the `timed` flag, across every section.
  - **Adaptive:** `mode = 'cat'`.
- **Section rules, in order of priority.**
  - A sample set always goes under Start here.
  - An adaptive set always goes under Adaptive, timed or not.
  - Challenges are split into timed and untimed mock exams.
- **No adaptive sets exist yet.** The Adaptive tab is always shown and says "coming soon", so candidates know it's on the way. Its section and tab fill in automatically once F19 publishes `cat` sets.
- **"Show more sets" is gone.** The page now loads every page up front (50 per request, the API's maximum), because grouping half a catalog would leave sections incomplete. That's 1 request for today's 36 sets.
- **Placement and headings.** `setCategories.ts` sits next to `format.ts` at the feature root, not in `components/` as the plan said, because it holds rules, not a component. Card titles became h3 so the heading outline reads page → section → set.
- `docs/interfaces.md` 5.2: the `/practice` row now describes the tabs, sections and `?filter=`.

## 7. Tests

- `setCategories.test.ts` (unit tests):
  - Each kind of set lands in its section; the sample and adaptive sets take priority.
  - Section order, and the API's order within a section.
  - What each filter shows, including Adaptive staying empty until `cat` sets exist.
  - `?filter=` parsing, where an unknown value falls back to All.
- `PracticeCatalog.test.tsx` (React Testing Library):
  - Sections and their headings, and the 50-set page size.
  - Tab counts and the default All tab.
  - `?filter=` selects the tab and hides other sets.
  - Changing tab writes `?filter=` and keeps `?set=`.
  - Arrow keys, Home and End.
  - The Adaptive "coming soon" note.
  - Every page is loaded before grouping.
  - The existing tests for the details panel and the empty track still pass.

Run them with `cd nclex-fe && npx vitest run src/features/practice`.

Results on this branch:
- `npm run lint`: clean
- `npm run typecheck`: clean
- `npm test`: 73 files, 442 tests passed
- `dotnet build`: 0 errors
- `dotnet test`: "Test run summary: Passed!" (no backend code changed)

## 8. Follow-ups and known limitations

- **F19 adaptive exams.** When F19 publishes `cat` sets, check that the Adaptive section's description still matches how they work.
- **Case studies by title** (see section 6). A `category` column on `test_sets`, editable in the admin console, would be sturdier.
- **One section for all topic sets.** The 21 topic sets share a single section. They could later be sub-grouped by Client Needs category ("Management of Care, part 1 of 3" and so on), but that also needs a category field to avoid more title matching.
- The empty-track message and the error message are unchanged.
- I didn't check the page in a browser against a running API; the tests cover the behaviour. Take a look in the browser before merging.

## 9. Try it yourself

1. Add a **Comprehensive** section for sets whose title starts with "Comprehensive". Add a `SectionId`, an entry in `sectionInfo`, and a rule in `sectionOf`, then add a test in `setCategories.test.ts`.
2. Make **Practice** the default tab instead of All. Change `parseFilter`'s fallback and `catalogUrl`'s "leave out the default" check, then update the tests that expect `/practice` with no query.
3. Hide a tab when it has no sets, except Adaptive. Filter `filters` in `SetFilterTabs` using `counts`, and think about what happens when the URL names a hidden tab.
