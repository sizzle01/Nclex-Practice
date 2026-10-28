# F11: Item renderers, exhibits and case studies

## 1. Summary

Every MVP item type can now be answered in the exam player and read back in review. That covers multiple choice, SATA, select N, both matrix types, grouping, both drop-down types, both highlight types, calculation and ordered response. Each renderer has a keyboard model and shows its reveal marks as ✓/✗ text, not by colour alone. A question can bring exhibits: tables, trend flowsheets, images with alt text, and audio or video with captions. Questions in an unfolding case study show the case's tabs (nurses' notes, vitals, orders…) beside the question on wide screens and in a drawer on phones. Candidates can strike options through as a note to self. A development-only gallery at `/dev/items` shows every renderer with sample data.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f11`. **It is stacked on `feature/f10-grading-results`**, because F10 was not yet merged into `main` when F11 started. Merge F10 first, then F11 fast-forwards on top of it.

There are no backend changes and no migration. `nclex_f11` was created as a separate local database, following the per-feature databases already there (`nclex_f05`, `nclex_f08`…), so the shared `nclex` database is left alone. Using the main `nclex` database works too, provided it has F10's migrations.

```bash
docker compose up -d
cd nclex-be
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_f11;Username=nclex;Password=nclex"
dotnet tool restore && dotnet build
dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
dotnet run --project src/Nclex.Api                    # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev          # http://localhost:3000
```

**The gallery (no sign-in needed):** open <http://localhost:3000/dev/items>.
- Each sample has its own answer and strikes.
- **Response sent to the save queue** shows the exact JSON the renderer emits.
- **Show the reveal** shows a partly wrong answer with its marks and the rationale.
- The last two samples show a trend item (flowsheet, image, audio with captions and a transcript) and a matrix item inside a case study.
- Narrow the window below 640px to see the matrix stack into cards. Below 1024px, the exhibits become a drawer.
- A production build (`npm run build && npm start`) answers 404 for `/dev/items`.

**In the player, keyboard only (the "done when" check):**
1. Sign in as a verified user (see F08's walkthrough). Start **Comprehensive Practice 1** (tutor mode, 40 items, all 12 MVP types).
2. For each question, use Tab, the arrow keys, Space and Enter only:
   - **Multiple choice:** arrow keys.
   - **SATA, select N, grouping:** Space. Select N disables the rest once N are ticked.
   - **Matrix:** arrow keys move between cells, Space selects.
   - **Drop-downs:** native selects.
   - **Highlight:** Tab to each segment or row button, then Space.
   - **Calculation:** type the number.
   - **Ordered:** Tab to **Move up** or **Move down** and press Enter. Or Tab to the ⠿ handle, press Space, use the arrows, then Space again.
3. Strike an option with its **Strike** button (Tab after the option, then Enter). It stays struck after reloading the page.
4. Press **Submit answer**. Each target now shows its mark ("✓ Correct", "✗ Incorrect", "Correct answer, not selected", "✓ Correct position", "Correct answer: 125 mL/hr"…). The rationale names each choice, e.g. "Warfarin: Unusual bleeding or bruising" or "Blank 1: hypoglycemia".
5. Submit the test and open **Review**. The same renderers show every answer, locked and marked.

**With curl** (strike-through uses F08's PATCH):
```bash
curl -s -X PATCH $B/attempts/$ID/items/3 "${H[@]}" "${J[@]}" -d '{"struckOptions":["a","c"]}'
```

There are no new test accounts.

## 3. Reading order

1. `nclex-fe/src/features/player/renderers/types.ts`: `ItemRendererProps`, the contract every renderer implements (now with `onToggleStrike`).
2. `renderers/ItemAnswer.tsx`: the registry (`renderers: Record<ItemType, …>`). Phase 2 types map to `UnsupportedRenderer`.
3. `renderers/revealMarks.ts`: turns a grade's `detail` into marks. `choiceMarks` handles the types graded per slot (matrix MC rows, drop-down blanks, ordered positions).
4. `renderers/ChoiceOption.tsx`: the option row with its strike button, shared by MC, SATA, select N and grouping.
5. `renderers/SataRenderer.tsx`, then `SelectNRenderer.tsx`: the simplest renderers. `selection.ts` holds their helpers.
6. `renderers/MatrixRenderer.tsx`: the table with arrow-key cell movement, and the stacked layout for narrow screens.
7. `renderers/BlankSelect.tsx`, then `DropdownClozeRenderer.tsx` and `DropdownTableRenderer.tsx`: one native select per blank, named by its context.
8. `renderers/HighlightTextRenderer.tsx` and `HighlightTableRenderer.tsx`: `aria-pressed` toggle buttons.
9. `renderers/CalcRenderer.tsx`: the raw text, with the unit and decimals in the description.
10. `renderers/OrderedResponseRenderer.tsx`: dnd-kit sorting, Move up/down buttons and the live region.
11. `renderers/choiceLabels.ts`: readable names for rationale keys, used by `attempt/reveal/RationalePanel.tsx`.
12. `player/exhibits/exhibitTypes.ts`, then `ExhibitPanel.tsx`, `ExhibitTabs.tsx`, `ExhibitView.tsx`, `FlowsheetTable.tsx` and `ExhibitMedia.tsx`.
13. `player/media/MediaContext.tsx`: how a `mediaId` becomes a URL (see Follow-ups).
14. `attempt/player/useAttemptSession.ts` (`toggleStrike`) and `attempt/api/attemptApi.ts` (`setStruckOptions`).
15. `attempt/player/AttemptPlayer.tsx`: the split view with `ExhibitPanel` beside the question.
16. `player/gallery/ItemGallery.tsx` and `sampleItems.ts`, then `src/app/dev/items/page.tsx`.

## 4. Request flow: ticking a matrix cell, then submitting it

1. **UI.** The candidate focuses a cell in `MatrixMrRenderer` and presses Space. Its `select(rowId, columnId)` toggles `"r1:c4"` inside its group (the row, because `groupAxis` is `row`), keeps the content's order and drops empty groups. It calls `onChange({ byGroup: { r1: ["r1:c1", "r1:c4"] } })`.
2. **Player.** `AttemptPlayer` passes `onChange` as `session.answer`. That enqueues the response in the save queue (debounced 750 ms) and updates the cached item and the navigator at once. The renderer never calls the API.
3. **API.** The save queue sends `PUT /api/v1/attempts/{id}/items/{position}/response` with `{ response, clientSeq }` and the `X-Attempt-Client` lease header.
4. **Application and Domain.** The autosave handler asks the matrix MR module (`MatrixMrModule.ValidateResponse`, via `GroupedGrader.CheckResponse`) to check that every group and cell exists. A malformed shape would be a 400. Then it stores the response in `attempt_items`.
5. **Submit (tutor mode).** **Submit answer** calls `POST …/items/{position}/submit`. `GroupedGrader.Grade` scores each row +/- with a floor of 0 and returns one `GradeDetail` per cell (`target: "r1:c4"`, `delta`, `selected`, `missed`).
6. **Back in the UI.** The item comes back with `reveal`. `choiceMarks("matrix_mr", reveal)` maps each detail to a mark, and the cell shows "✓ Correct", "✗ Incorrect" or "Correct answer, not selected" beside its checkbox. `RationalePanel` lists each cell's "why" under the name "Warfarin: Black, tarry stools", which comes from `choiceLabels`.

The strike-through tool follows a shorter path. `toggleStrike` updates the cached item straight away, then sends `PATCH …/items/{position}` with the full `struckOptions` list. If the server refuses, the strike is put back.

## 5. Database changes

None. `case_studies.scenario` already exists as `jsonb`. F11 only settles the JSON shape inside it (see section 6).

## 6. Key decisions and deviations

- **Stacked on F10.** F10 wasn't merged into `main` yet, and F11 builds on its player, reveal marks and review page.
- **Renderers moved** from `features/attempt/renderers` (F08 and F10) to `features/player/renderers`, as the feature asked. The player shell, save queue and timers stay in `features/attempt`.
- **The case-study `scenario` shape** was undocumented ("tabs: nurses' notes, vitals, labs, orders, history"). It is now `{ tabs: [{ id, title, exhibits: Exhibit[] }] }`, which reuses `Exhibit`, and is recorded in interfaces.md section 1. A scenario in any other shape shows no tabs instead of crashing. Nothing authors case studies yet, so no data has to change.
- **Media.** Exhibits and graphic options carry only a `mediaId`. No candidate endpoint turns that into a URL, alt text and captions; those live in `media_assets`. Renderers ask a `MediaResolver` from React context. In the player it resolves nothing, so a media exhibit says "this image can't be shown right now". The gallery serves local files from `public/dev-media`. A real endpoint is a follow-up.
- **Panel layout.** The exhibit panel uses tabs: the case study's tabs first, then one tab per item exhibit. With a single tab there is no tab list.
- **Matrix keyboard model.** The arrow keys move focus without selecting, and Space selects, for both radio and checkbox matrices. Native radio buttons would select as they move, and only within their row.
- **Breakpoints.** The matrix stacks below 640px; exhibits become a drawer below 1024px (interfaces.md 5.3). Both use `useMediaQuery`. jsdom has no `matchMedia`, so tests get the wide layout unless they stub it.
- **Select N** disables the unticked boxes once N are chosen, and the instructions say "You have selected 3; untick one to change your answer". The design only said the validator caps the length.
- **Ordered response.**
  - The response stays `null` until the candidate moves an item or presses **Keep this order**. That way the shuffled starting order never counts as an answer.
  - Touch users reorder by dragging the handle (dnd-kit's pointer sensor) or with the Move buttons. Instead of a separate tap-an-item-then-tap-a-place mode, the Move up/down buttons are the tap path.
  - dnd-kit keeps its own live region for drags, and the renderer has one for button moves, so there are two `role="status"` regions.
- **Drop-down blanks** are named "Blank 1 of 2: The client is most likely experiencing blank, as shown by …." Each name is built from the blank's sentence, which gives the sentence context interfaces.md section 4 asks for.
- **Text is shown as plain text**, as in F08 and F10. The `Markdown` fields (stems, options, exhibit bodies) are not rendered as Markdown yet.
- **Strike-through** shows on option-based types (MC, SATA, select N, grouping). It is hidden once an item is locked or revealed. Struck options stay crossed out in review.
- **New dependencies:** `@dnd-kit/core`, `@dnd-kit/sortable` and `@dnd-kit/utilities` (in the stack), plus the dev-only `axe-core`. It was proposed as question 2 of the plan, and the reply was "go". The axe helper turns off `color-contrast`, because jsdom can't compute colours; contrast is checked by eye in the gallery.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test          # 853 passed (backend unchanged; Docker must be running)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # 309 passed (49 files)
```

- **Every renderer** (`src/features/player/renderers/*.test.tsx`) is checked for:
  - its roles and names, e.g. a table with row and column headers, radios named "Orthopnea: Right-sided", blanks named by sentence
  - keyboard-only operation that emits the exact response shape of interfaces.md section 2
  - clearing to `null`
  - reveal marks as text
  - an axe check

  The tests use content from the F04/F05 sample questions, through the gallery's `sampleItems`.
- **Type-specific cases:**
  - Matrix: arrow keys move without selecting, the stacked layout with `matchMedia` stubbed, and axe in both layouts.
  - Select N: disabling at N.
  - Ordered response: Keep this order, the live-region announcement, and focus kept on the moved item at the top of the list.
  - Calculation: the unit and decimals in the description.
- **Reveal helpers** (`revealMarks.test.ts`): slot marks for drop-downs and matrix MC, position marks, and readable choice labels.
- **Exhibits** (`ExhibitPanel.test.tsx`, `exhibitTypes.test.ts`):
  - flowsheet headers
  - tabs with the arrow keys, Home and End
  - image alt text, captions on audio and video, the transcript link, and media that can't be shown
  - case-study tabs and the narrow-screen drawer
  - axe
- **Player** (`AttemptPlayer.test.tsx`): striking sends the full list with the lease header, shows at once and is undone if the server refuses; exhibits appear beside the question.
- **Gallery** (`ItemGallery.test.tsx`): all 12 MVP types have a real renderer; the response JSON and the reveal show.
- **Against the real API** (run once by hand while building F11, not part of the suite): a script answered all 90 items of the three sample sets with the shapes the renderers emit. There were no 400s from autosave or tutor submit, and the grade details had the targets the reveal marks expect.

## 8. Follow-ups and known limitations

- **Candidate media endpoint.** Something like `GET /media/{id}` returning a signed URL, alt text and the captions and transcript URLs is needed before media exhibits and graphic options can show in the player. Today they say the media can't be shown.
- **Sample content.** No sample question has exhibits, a case study or media yet. Only the gallery and the tests exercise them.
- **Markdown** in stems, options and exhibit bodies is still shown as plain text.
- **Browser checks.** The keyboard-only pass in a real browser (dnd-kit's keyboard sensor needs layout, which jsdom lacks) should become a Playwright test in F16.
- `public/dev-media` is also served in production builds. It holds only small sample files, but it could move behind the gallery's development check.
- **Case position.** The item payload doesn't carry the case length, so the panel shows "Question 2" rather than "Question 2 of 6".

## 9. Try it yourself

1. **Change a breakpoint.** Make the matrix stack from 768px instead of 640px. Change `narrowScreenQuery` in `src/lib/browser/useMediaQuery.ts`, then update the matrix test that stubs `matchMedia`, if it needs it.
2. **A new exhibit kind.** Add a `"link"` exhibit that shows a titled external reference. Add it to `Exhibit.kind` in `exhibitTypes.ts` and to the switch in `ExhibitView.tsx`, then add a gallery sample and a test. Remember interfaces.md is the source of truth, so the docs change comes first.
3. **Strike-through in highlight items.** Should candidates be able to strike highlight segments? If so, `HighlightTextRenderer` needs a strike button beside each segment, as `ChoiceOption` has. Try it and see what it does to the keyboard order.
