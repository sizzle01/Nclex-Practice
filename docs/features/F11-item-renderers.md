# F11: Item renderers, exhibits and case studies

## 1. Summary

Every MVP item type can now be answered in the exam player and read back in review. That covers multiple choice, SATA, select N, both matrix types, grouping, both drop-down types, both highlight types, calculation and ordered response. Each renderer has a keyboard model and shows its reveal marks as ✓/✗ text, not by colour alone. A question can bring exhibits: tables, trend flowsheets, images with alt text, and audio or video with captions. Questions in an unfolding case study show the case's tabs (nurses' notes, vitals, orders…) beside the question on wide screens and in a drawer on phones. Candidates can strike options through as a note to self. Five sample questions now carry exhibits, including a real image and an audio clip with captions and a transcript. Those media come in through the content importer and are served to the player through short-lived signed links. The importer can also create case studies, and three sample heart-failure questions now form one. A development-only gallery at `/dev/items` shows every renderer with sample data.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f11`. **It is stacked on `feature/f10-grading-results`**, because F10 was not yet merged into `main` when F11 started. Merge F10 first, then F11 fast-forwards on top of it.

There is no migration: `media_assets` already exists from F04. `nclex_f11` was created as a separate local database, following the per-feature databases already there (`nclex_f05`, `nclex_f08`…), so the shared `nclex` database is left alone. Using the main `nclex` database works too, provided it has F10's migrations.

**Re-run the import.** The sample file now has exhibits, a `media` list and a `caseStudies` list. The importer stores the media files in `nclex-be/.local/media`, which git ignores and which the API reads from. Both hosts set `MediaStorage:LocalRoot` to `../../.local/media`. The API also needs `MediaStorage:SigningKey`, which is in `appsettings.Development.json` for development only.

```bash
docker compose up -d
cd nclex-be
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_f11;Username=nclex;Password=nclex"
dotnet tool restore && dotnet build
dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F11_MediaTranscripts
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   -> … Imported 2 media file(s) … Imported 1 case study(ies): 1 created …
#      (questions and sets "updated" if you imported before F11: exhibits, case fields and set order changed)
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

**Exhibits, media and the case study** (question numbers in **Comprehensive Practice 1**; the pressure-injury, heparin and hypoglycemia questions are also in the **RN Sample Set**):

| Question | Exhibit |
|---|---|
| 6 (MC, digoxin) | Vital-signs flowsheet and a lab table, as two tabs |
| 15 (SATA, pressure injuries) | Image of sacral skin, with alt text |
| 19 to 21 (case study "Heart failure exacerbation") | The case's four tabs (nurses' notes, vital signs, labs, orders) on all three items |
| 19 (SATA, left-sided heart failure; case item 1) | Also audio of lung crackles, with captions (turn them on in the player's controls) and a **Read the transcript** link |
| 20 (matrix, left or right; case item 2), 21 (grouping, discharge teaching; case item 3) | Only the case's tabs |
| 25 (matrix, heparin) | aPTT flowsheet |
| 29 (drop-down, hypoglycemia) | Nurse's note |

In **Comprehensive Challenge 1** the case is questions 36 to 38.

From 1024px the exhibits sit beside the question; below that, open them with **Show exhibits**. Tab to the tab list and move between tabs with the arrow keys.

**With curl** (strike-through uses F08's PATCH):
```bash
curl -s -X PATCH $B/attempts/$ID/items/3 "${H[@]}" "${J[@]}" -d '{"struckOptions":["a","c"]}'

# Media: details and signed links (needs a token), then the file itself (needs none)
curl -s $B/media/f11a0000-0000-4000-8000-000000000002 -H "Authorization: Bearer $TOKEN"
# -> {"kind":"audio","url":"/api/v1/media/…/file?exp=…&sig=…","captionsUrl":"…/captions?…","transcriptUrl":"…/transcript?…",…}
curl -s -o clip.wav "http://localhost:5080<url from above>"              # 200 audio/wav
curl -s "http://localhost:5080<transcriptUrl>"                           # 200 text/plain; charset=utf-8

# The case study comes with each of its items
curl -s $B/attempts/$ID/items/20 "${H[@]}" | jq '.caseStudy | {title, position, tabs: [.scenario.tabs[].title]}'
# -> {"title":"Heart failure exacerbation","position":2,"tabs":["Nurses' notes","Vital signs","Laboratory results","Orders"]}
curl -s -H "Range: bytes=0-99" -o /dev/null -w "%{http_code}\n" "http://localhost:5080<url>"   # 206
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

Media, backend then frontend:

17. `nclex-be/src/Nclex.Domain/Content/ExhibitRules.cs` and `MediaRules.cs`: what a valid exhibit and media asset are (pure functions).
18. `Nclex.Domain/Entities/MediaAsset.cs`: `Create`, `Update` and `MediaDetails` (with `TranscriptKey`).
19. `Nclex.Application/Content/ImportQuestions/QuestionImportFile.cs` (`ImportedMedia`), then `MediaImporter.cs` and the changes in `ImportQuestionsHandler.cs`.
20. `Nclex.Application/Abstractions/IMediaStorage.cs` and `IMediaUrlSigner.cs`, then `Media/GetMedia/GetMediaHandler.cs`, `Media/OpenMediaFile/OpenMediaFileHandler.cs` and `Media/MediaPaths.cs`.
21. `Nclex.Infrastructure/Media/LocalDiskMediaStorage.cs`, `HmacMediaUrlSigner.cs` and `MediaStorageOptions.cs`, then `Content/DirectoryContentFiles.cs`.
22. `Nclex.Api/Controllers/MediaController.cs`: the four endpoints (details, file, captions, transcript).
23. `nclex-fe/src/features/player/media/mediaApi.ts` and `MediaContext.tsx` (`useMedia`), then `exhibits/ExhibitMedia.tsx`.
24. `nclex-be/content/sample-questions.rn.json` (`media`, `caseStudies`, the five `exhibits` and the three questions with `caseStudyId`) and `content/media/`.

Case studies:

25. `Nclex.Domain/Content/CaseStudyRules.cs`: a valid scenario, complete positions, and a set that keeps each case whole (pure functions).
26. `Nclex.Domain/Entities/CaseStudy.cs` (`Create`, `Update`, `CasePlacement`), then `AuthoredItem.Case` and how `QuestionVersion.CreateImported` and `Matches` use it.
27. `Nclex.Application/Content/ImportQuestions/CaseStudyImporter.cs`: checks the cases and placements, works out each case's members after the import (`CaseMembership`), and writes the rows.
28. `CatalogImporter.CheckCaseStudies`: the curated-set rule.

## 4. Request flow: ticking a matrix cell, then submitting it

1. **UI.** The candidate focuses a cell in `MatrixMrRenderer` and presses Space. Its `select(rowId, columnId)` toggles `"r1:c4"` inside its group (the row, because `groupAxis` is `row`), keeps the content's order and drops empty groups. It calls `onChange({ byGroup: { r1: ["r1:c1", "r1:c4"] } })`.
2. **Player.** `AttemptPlayer` passes `onChange` as `session.answer`. That enqueues the response in the save queue (debounced 750 ms) and updates the cached item and the navigator at once. The renderer never calls the API.
3. **API.** The save queue sends `PUT /api/v1/attempts/{id}/items/{position}/response` with `{ response, clientSeq }` and the `X-Attempt-Client` lease header.
4. **Application and Domain.** The autosave handler asks the matrix MR module (`MatrixMrModule.ValidateResponse`, via `GroupedGrader.CheckResponse`) to check that every group and cell exists. A malformed shape would be a 400. Then it stores the response in `attempt_items`.
5. **Submit (tutor mode).** **Submit answer** calls `POST …/items/{position}/submit`. `GroupedGrader.Grade` scores each row +/- with a floor of 0 and returns one `GradeDetail` per cell (`target: "r1:c4"`, `delta`, `selected`, `missed`).
6. **Back in the UI.** The item comes back with `reveal`. `choiceMarks("matrix_mr", reveal)` maps each detail to a mark, and the cell shows "✓ Correct", "✗ Incorrect" or "Correct answer, not selected" beside its checkbox. `RationalePanel` lists each cell's "why" under the name "Warfarin: Black, tarry stools", which comes from `choiceLabels`.

**An image exhibit**, from import to screen:
1. **Import.** `import-questions` reads `media[0]` from the sample file. `MediaImporter` reads `media/sacral-skin.png` through `DirectoryContentFiles`, checks it with `MediaRules` (alt text present, `image/png` matches `image`), stores it as `media/f11a…0001.png` with `LocalDiskMediaStorage`, and inserts the `media_assets` row. `ExhibitRules` checks question 15's exhibit, and its `mediaId` must name that asset, of the same kind.
2. **Item.** The player's `GET …/items/15` returns the exhibit unchanged: `{ kind: "image", title, mediaId }`.
3. **Links.** `ExhibitMedia` calls `useMedia(mediaId)`, a TanStack query for `GET /api/v1/media/{id}`. `GetMediaHandler` loads the row and has `HmacMediaUrlSigner` sign a link valid for 10 minutes: `/api/v1/media/{id}/file?exp=…&sig=…`.
4. **File.** The `<img>` loads that link without a token. `OpenMediaFileHandler` checks the signature before touching the database, then streams the file with `Content-Type: image/png`. The alt text comes from the row.
5. **Expired link.** If the browser is refused (a link older than 10 minutes), `ExhibitMedia` asks once for fresh links. A second failure shows "can't be shown right now".

**A case-study item**, from import to screen:
1. **Import.** `CaseStudyImporter.ValidateAsync` checks `caseStudies[0]` with `CaseStudyRules.CheckScenario` (each tab's exhibits go through `ExhibitRules`) and the three questions' `caseStudyId`/`casePosition`. It then works out the case's members after the import (the file's questions plus published ones it doesn't mention) and checks their positions run 1, 2, 3. `CatalogImporter` checks that both comprehensive sets hold all three, back to back, in order.
2. **Write.** Inside the import transaction: media, then the `case_studies` row (`CaseStudy.Create`), then a question version per question with `case_study_id` and `case_position` set from `AuthoredItem.Case`.
3. **Item.** `GET …/items/20` goes through `ItemViewReader` (F08), which loads the case and returns `caseStudy: { id, title, scenario, position: 2 }`.
4. **Screen.** `ExhibitPanel` shows the case title and its four tabs, then the item's own exhibits as more tabs.

The strike-through tool follows a shorter path. `toggleStrike` updates the cached item straight away, then sends `PATCH …/items/{position}` with the full `struckOptions` list. If the server refuses, the strike is put back.

## 5. Database changes

One migration, `F11_MediaTranscripts`:
- `media_assets.transcript_key text` (nullable), added to the DDL in database.md. It holds the storage key of the clip's plain-text transcript, `media/{id}.txt`. Audio and video need one before import, like captions, because captions only help someone who can play the clip.

No other schema change:
- `case_studies` (F04) gets its first rows, from the importer. F11 settles the JSON shape of `scenario` (see section 6).
- `question_versions.case_study_id` and `case_position` (F04) are now set by the importer. The DDL's CHECK (both or neither, position 1 to 6) was already there.
- `media_assets` (F04) gets its first rows, from the importer. `storage_key` is `media/{id}.{ext}`; `captions_key` is `media/{id}.vtt`; `created_by` is the content importer's account.

## 6. Key decisions and deviations

- **Stacked on F10.** F10 wasn't merged into `main` yet, and F11 builds on its player, reveal marks and review page.
- **Renderers moved** from `features/attempt/renderers` (F08 and F10) to `features/player/renderers`, as the feature asked. The player shell, save queue and timers stay in `features/attempt`.
- **The case-study `scenario` shape** was undocumented ("tabs: nurses' notes, vitals, labs, orders, history"). It is now `{ tabs: [{ id, title, exhibits: Exhibit[] }] }`, which reuses `Exhibit`, and is recorded in interfaces.md section 1. The importer refuses any other shape; the player still shows no tabs instead of crashing if one gets in some other way.
- **Media delivery (new API, api.md section 10).**
  - `GET /media/{id}` (signed in) returns the alt text and **signed links** to the file and captions. The file links need no token, because `<img>` and `<audio>` can't send the in-memory access token.
  - The signature (HMAC-SHA256 over the id, the part and the expiry) does the job a presigned S3 URL would. Links last 10 minutes, so the frontend reuses them for 5.
  - The links are relative and go through the Next.js `/api` proxy.
- **Local-disk storage behind `IMediaStorage`.** It needs no new dependency. The S3 or MinIO version that architecture.md describes can replace it behind the same interface, and `GetMediaHandler` would then return the presigned URLs instead.
- **Media in the import file.** The `media` list's paths are relative to the import file. A path that leaves its folder is treated as missing. Files are written every import (so a changed file replaces the old one); rows count as updated only when their description changes.
- **Exhibits are now validated on import** (`ExhibitRules`): a known kind, a title, a table whose rows match its columns, a body for text, and a `mediaId` of an existing asset of the same kind. Graphic options' `mediaId`s are checked too. Before F11 the importer only checked that `exhibits` was an array, but no file had exhibits, so nothing that imported before is now refused.
- **Transcripts are required for audio and video**, as captions are. interfaces.md section 4 asks for a transcript link, and for audio a transcript is the text alternative (WCAG 1.2.1). The import file names a `transcriptFile` (UTF-8 plain text; anything else is refused), and `GET /media/{id}/transcript` serves it through a signed link (api.md section 10). The player shows **Read the transcript** when `transcriptUrl` is set.
- **Case studies in the import file** (interfaces.md section 1, after `CaseStudyScenario`):
  - A top-level `caseStudies` list, on the file's track. A question joins one with `caseStudyId` and `casePosition` (1 to 6, both or neither). It may join a case from this file or one imported before.
  - Positions are checked for the case as it will be after the import, including published questions the file doesn't mention. They must run 1, 2, 3… with no gap or repeat. So moving a question out of the middle of a case is refused until the others are renumbered in the same file.
  - Joining, leaving or moving within a case gives the question a new version, because versions are immutable. The case itself is edited in place: its scenario only sets the scene, and past attempts keep their answers.
  - A case-study tab and one of its items' own exhibits can't share an id, because the player puts them in one tab list.
- **Curated sets keep each case whole.** A prebuilt set that uses any item of a case must hold all of them, back to back, in case order (`CASE_STUDY_SPLIT` otherwise). The custom builder already works this way (F07's `SetSampler`), and otherwise the player would show a case's tabs with items missing or out of order. Banks need no rule, because the builder drops incomplete cases from a pool. Sets already in the database are not re-checked when a later file changes a case; the next import of that set is.
- **The sample case** reuses the three existing heart-failure questions (SATA left-sided findings, matrix left or right, grouping discharge teaching) instead of new ones, so it has 3 items rather than the usual 6. The new scenario text (notes, vitals, labs, orders) is sample content like the rest of the file, not reviewed by a nurse educator. To keep the case whole, the two comprehensive sets were reordered, and the **RN Sample Set** now uses the heparin matrix question (also matrix MC) instead of the case's matrix question. F04's walkthrough says case studies can't be imported; that is now out of date.
- **The gallery** keeps its own local files (`public/dev-media`) through `MediaProvider`, so it works without signing in.
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
cd nclex-be && dotnet build && dotnet test          # 943 passed (Docker must be running)
cd nclex-fe && npm run lint && npm run typecheck && npm test   # 312 passed (49 files)
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
- **Media, backend:**
  - Domain (`tests/Nclex.Domain.Tests/Content/`): each exhibit kind, each malformed exhibit with its path, graphic options found anywhere in the content, and each media rule (alt text, captions, transcript, MIME type of another kind, unknown kind).
  - Import (`Content/ImportMediaTests.cs`): files and rows stored, a re-import unchanged, a changed alt text updating the row, and media from an earlier file usable. These problems each write nothing: no alt text, no captions, a wrong MIME type, a missing file, a path outside the folder, captions that aren't WebVTT, no transcript, a missing transcript file, a transcript that isn't text. Also: an unknown or wrong-kind `mediaId`, a malformed exhibit, and an unknown graphic-option media.
  - Sample file (`ImportQuestionsTests`): 2 media and 5 questions with exhibits, with the files (and the transcript) in storage, and the case study with its three items in order.
  - Endpoints (`Media/MediaEndpointsTests.cs`):
    - 401 when signed out, and 404 for an unknown asset
    - an image with alt text and a token-free link, with the right bytes, type and `Cache-Control: private`
    - audio with captions as `text/vtt`, a transcript as `text/plain; charset=utf-8`, and a range request answered with 206
    - 403 for a tampered signature, another asset's or part's (captions or transcript) signature, a moved expiry, or no signature
    - 403 once the link expires (manual clock)
    - an item's exhibits reaching the candidate
- **Case studies, backend:**
  - Domain (`CaseStudyRulesTests`): a valid scenario and its media, each malformed scenario with its path, tab ids, complete and incomplete positions, sets that keep or split a case, and `CaseStudy.Update` changing only when something changed.
  - Import (`Content/ImportCaseStudiesTests.cs`): the case and its versions stored and a re-import unchanged; a changed scenario updating in place; a later file adding a question to an imported case; leaving a case giving a new version; leaving a gap refused. Each of these writes nothing: an unknown case, a position without a case or a case without a position, position 7, a repeated position, a gap, no tabs, a scenario that isn't an object, a malformed exhibit or unknown media in a tab, an exhibit id that is also a tab id, no title. A curated set with only part of a case, out of order, or not back to back is refused. An item in a case returns its case study with title, tabs and position.
- **Media, frontend** (`ExhibitPanel.test.tsx`): loading from the API with the signed link and alt text, an audio clip's captions and transcript links from the API, a 404 shown as "can't be shown", and one refresh after a refused link, then giving up.
- **Media end to end** (run by hand, not in the suite): through the Next.js proxy, every exhibit of Comprehensive Practice 1 arrived. The image and audio links worked without a token, captions came back as `text/vtt`, a range request got 206, and a tampered link got 403.
- **Transcripts and the case study against the real API** (by hand, after the follow-up): the worker imported the sample file into a database that had the earlier F11 content (3 questions and 3 sets updated, 1 case study created, the transcript stored). Items 19 to 21 of Comprehensive Practice 1 returned the case with its four tabs and positions 1 to 3, with no answer key; the transcript link returned `text/plain; charset=utf-8`.
- **Against the real API** (run once by hand while building F11, not part of the suite): a script answered all 90 items of the three sample sets with the shapes the renderers emit. There were no 400s from autosave or tutor submit, and the grade details had the targets the reveal marks expect.

## 8. Follow-ups and known limitations

- **Premium media.** Any signed-in user can get links for any asset id. Gating by the item's access tier, and serving premium media through signed CDN URLs (architecture.md), is still to do.
- **S3/MinIO storage** in place of local disk for deployed environments, behind `IMediaStorage`.
- **A full six-item sample case.** The sample case has 3 items; a reviewed 6-item case (recognize cues through evaluate outcomes) is content work for later.
- **Sets already in the database** aren't re-checked when a later import changes a case they use (see section 6).
- **A link that expires mid-playback.** A clip that has already loaded keeps playing; one that needs a new range request after 10 minutes gets one fresh link, then shows the note.
- **Markdown** in stems, options and exhibit bodies is still shown as plain text.
- **Browser checks.** The keyboard-only pass in a real browser (dnd-kit's keyboard sensor needs layout, which jsdom lacks) should become a Playwright test in F16.
- `public/dev-media` is also served in production builds. It holds only small sample files, but it could move behind the gallery's development check.
- **Case position.** The item payload doesn't carry the case length, so the panel shows "Question 2" rather than "Question 2 of 6".

## 9. Try it yourself

1. **Change a breakpoint.** Make the matrix stack from 768px instead of 640px. Change `narrowScreenQuery` in `src/lib/browser/useMediaQuery.ts`, then update the matrix test that stubs `matchMedia`, if it needs it.
2. **A new exhibit kind.** Add a `"link"` exhibit that shows a titled external reference. Add it to `Exhibit.kind` in `exhibitTypes.ts` and to the switch in `ExhibitView.tsx`, then add a gallery sample and a test. Remember interfaces.md is the source of truth, so the docs change comes first.
3. **Strike-through in highlight items.** Should candidates be able to strike highlight segments? If so, `HighlightTextRenderer` needs a strike button beside each segment, as `ChoiceOption` has. Try it and see what it does to the keyboard order.
