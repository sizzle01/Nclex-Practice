# F28: Collapsible sections on the Practice page

## 1. Summary
Each section on the Practice page ("Start here", "Timed case-study blocks", "Mock exams: timed", the review-book
sections under Topics, and so on) can now be collapsed and expanded. Click the section heading, or press Enter or
Space when it has focus, to fold its cards away or bring them back. An "Expand all / Collapse all" button sits
above the sections. The Topics tab, with over 100 topic tests, starts collapsed so it reads as a list of book
sections; every other tab starts expanded. The browser remembers which sections a candidate opened or closed.

## 2. How to try it
Frontend only; no new data is needed.
```bash
docker compose up -d
cd nclex-be && dotnet run --project src/Nclex.Api      # API on :5080
cd nclex-fe && npm run dev                              # http://localhost:3000
```
1. Sign in as a verified candidate and open `/practice?filter=timed`.
2. Click "Timed case-study blocks (20)". The cards fold away and the chevron points right. Click it again to reopen.
3. Press Tab until the heading has focus, then press Enter or Space to toggle it.
4. Click "Collapse all", then reload the page: the sections stay collapsed.
5. Open the **Topics** tab: only the book sections show until you open one.

## 3. Reading order
1. `nclex-fe/src/features/practice/sectionCollapse.ts`: pure helpers: section keys, defaults, reading and saving the
   state in localStorage.
2. `nclex-fe/src/features/practice/useSectionCollapse.ts`: the hook that exposes `isOpen`, `toggle` and `setAll`,
   built on `useSyncExternalStore` like `useTheme`.
3. `nclex-fe/src/features/practice/components/SetSectionList.tsx`: the disclosure heading buttons and the
   Expand all / Collapse all button.
4. `nclex-fe/src/features/practice/components/PracticeCatalog.tsx`: passes `openByDefault={filter !== "topics"}`.
5. `nclex-fe/src/features/practice/components/practice.module.css`: toggle button, chevron and `.list[hidden]`.
6. `nclex-fe/src/features/practice/components/SetSectionList.test.tsx`: the behaviour, test by test.

## 4. Request flow
No requests are involved. A click on a section heading:
1. `SetSectionList` calls `toggle(key)`, where `key` is the section id ("timedCaseBlocks") or, for a topic section,
   `topic:<book section name>`.
2. `useSectionCollapse` reads the saved map from localStorage (`nclex-practice-sections`), flips that key relative to
   its current value (saved choice or tab default), saves it and notifies its subscribers.
3. `useSyncExternalStore` re-reads the snapshot and every `SetSectionList` re-renders. The `<ul>` gets `hidden`,
   and the button gets `aria-expanded="false"`.

## 5. Database changes
None.

## 6. Key decisions and deviations
- **WAI-ARIA disclosure pattern.** The `<button>` sits inside the `<h2>`, so screen-reader users can still jump
  between sections by heading, and the button carries `aria-expanded` and `aria-controls` (interfaces.md 4).
- **Defaults.** Every section starts open except under Topics (7 sections, 100+ tests).
- **Remembered per browser, not per account.** It is a viewing preference, not progress, so localStorage is enough
  (same reasoning as the theme in `themePreference.ts`). Only toggled sections are stored, so new sections follow
  their default. Every storage access is guarded; when storage is blocked, toggling still works for that page view.
- **Shared across tabs.** "Timed case-study blocks" collapsed on the Timed tab is collapsed on the All tab too,
  since it is the same section.
- **Topic keys use the name, not the id.** Topic section ids are index-based (`topic-0`) and would shift when a new
  book section gets tests.
- **The details panel is unaffected.** A set opened with `?set=` stays open even if its section is collapsed.
- **Expand all / Collapse all** appears only when a tab has more than one section.

## 7. Tests
- `SetSectionList.test.tsx` (new, 7 tests): defaults open and closed, click toggle with `aria-controls`, Enter and
  Space, Collapse all / Expand all, remembered across remounts, invalid saved value ignored.
- `PracticeCatalog.test.tsx`: the Topics test now checks that topics start collapsed and open on click.

Run: `cd nclex-fe && npm test` (also `npm run lint`, `npm run typecheck`).

## 8. Follow-ups and known limitations
- **Card order is alphabetical, not numeric:** "Timed case study block 1, 10, 11 … 19, 2, 20". The API sorts by
  title as text (`ListTestSetsHandler.cs`, `.ThenBy(s => s.Title)`); a natural sort (or a sort order column) would put block 2 after block 1.
- No expand and collapse animation (deliberately; the `hidden` attribute keeps the DOM and accessibility simple).
- Not yet checked in Playwright end-to-end tests.

## 9. Try it yourself
1. Make "Mock exams: untimed" start collapsed on every tab, while the other sections keep their default.
2. Show how many sections are open in the toolbar, e.g. "3 of 7 open".
3. Add a test that a second `SetSectionList` mounted at the same time follows a toggle in the first (hint: the
   shared listeners in `useSectionCollapse.ts`).
