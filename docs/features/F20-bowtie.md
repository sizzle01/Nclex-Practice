# F20: Bowtie item type

## 1. Summary

Candidates can now answer **bowtie** questions, a Next Generation NCLEX format the real exam uses. The screen shows a bowtie diagram with one slot in the centre for the condition the client is most likely experiencing, two slots on the left for actions to take and two on the right for parameters to monitor. Below the diagram are the three lists to choose from. Candidates fill the slots by dragging, or by picking an option and then a slot (keyboard or tap). Each correct slot earns a point, out of 5, and a wrong pick costs nothing. After the reveal, each slot is marked right or wrong, and the correct options the candidate left out are marked in their lists. Authors can create and edit bowties in the admin console. The sample file now has two bowtie questions, so bowtie can be tried straight away.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f20`, on `feature/f20-bowtie`, branched from `main` at 3ab993f. There is **no migration**: `bowtie` has been in the `item_type` enum since F06/F08.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000
```

**See every state without signing in:** open http://localhost:3000/dev/items (the dev-only item gallery) and scroll to **bowtie**. Fill it in, watch the response JSON update, then tick **Show the reveal** to see the marks.

**As a candidate** (sign up, then verify the email through Mailpit at http://localhost:8025):

1. Go to http://localhost:3000/practice/custom. Under **Question types**, leave only **Bowtie** ticked, set the number of questions to 2, and press **Create and start**. The two sample bowties are about DKA and postpartum hemorrhage.
2. **With the keyboard:** Tab to "Diabetic ketoacidosis" and press Enter. Focus jumps to the centre slot; press Enter again to place it. Escape cancels a pick.
3. **With a mouse or finger:** drag "Start an infusion of 0.9% sodium chloride" onto an action slot, or tap it and then tap the slot.
4. Use **Remove** on a filled slot to take an option back. Choosing a filled slot while an option is picked replaces it.
5. Submit. On the results and review pages each slot shows "✓ Correct" or "✗ Incorrect", and correct options left out read "Correct answer, not selected".

**As an author:** make an account an admin with `dotnet run --project src/Nclex.Worker -- set-role you@example.com admin` (see F14), then go to **Admin → Questions → New question** and pick **Bowtie**. The form starts with 4 conditions, 5 actions and 5 parameters. Choose one correct condition (a radio button) and two correct actions and parameters (a third checkbox is disabled until you untick one). Every option needs a rationale.

**Through the API** (an open attempt whose item 1 is a bowtie):

```bash
curl -s -X PUT -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-1" -H "Content-Type: application/json" \
  "http://localhost:5080/api/v1/attempts/$ATTEMPT/items/1/response" \
  -d '{"response":{"condition":"c1","actions":["a1","a4"],"parameters":["p1"]},"clientSeq":1}'
# 3 actions in one wing → 400 VALIDATION_FAILED with field response.actions, code TOO_MANY
```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/ItemTypes/Bowtie/BowtiePayloads.cs`: the content, key and response records (interfaces.md 2.10).
2. `nclex-be/src/Nclex.Domain/ItemTypes/Bowtie/BowtieModule.cs`: authoring and response checks, grading (one point per slot) and the per-list shuffle.
3. `nclex-be/src/Nclex.Application/DependencyInjection.cs` (`AddItemTypes`): registering the module is what makes bowtie importable, gradable and "available" in the taxonomy.
4. `nclex-be/tests/Nclex.Domain.Tests/ItemTypes/BowtieModuleTests.cs` and `Bowtie/fixtures/scoring.json`: the rules, as tests and golden scoring cases.
5. `nclex-be/tests/Nclex.Api.IntegrationTests/Attempts/BowtieAttemptTests.cs`: a bowtie shown, saved, graded and revealed through the real API.
6. `nclex-fe/src/features/player/renderers/BowtieRenderer.tsx`: the diagram, the lists, pick-and-place, dragging and the live region.
7. `nclex-fe/src/features/player/renderers/renderers.module.css` (the "Bowtie (F20)" section): the bowtie layout, one column on narrow screens.
8. `nclex-fe/src/features/player/renderers/ItemAnswer.tsx` and `choiceLabels.ts`: the renderer registered for `bowtie`, and the "Condition: …" labels the rationale panel uses.
9. `nclex-fe/src/features/admin/item-forms/BowtieForm.tsx`, plus `payloads.ts` and `templates.ts`: authoring.
10. `nclex-be/content/sample-questions.rn.json`: the two sample bowties (ids `f20a…0001` and `f20a…0002`).

## 4. Request flow

Saving a half-filled bowtie, then submitting it in tutor mode:

1. **UI.** The candidate picks "Start an infusion of 0.9% sodium chloride" in `BowtieRenderer`. The renderer stores the pick in local state and moves focus to "Action to take, slot 1 of 2". Choosing the slot calls `place`, which builds `{ condition: null, actions: ["a1"], parameters: [] }` and passes it to `onChange`. The player's save queue (F08) sends it to `PUT /attempts/{id}/items/1/response` with the next `clientSeq`.
2. **API.** `AttemptsController` passes the request to the save handler in `Nclex.Application/Attempts`, which checks the lease and the item's state.
3. **Application → Domain.** The handler looks up the module for the item's type in the `ItemTypeRegistry` and calls `ValidateResponse`. `BowtieModule` checks that every id comes from its own list, with no repeats and no more than 2 per wing. A problem becomes a 400 `VALIDATION_FAILED` with field paths such as `response.actions`.
4. **Database.** The response is stored as JSON in `attempt_items.response`.
5. **Submit.** `POST /attempts/{id}/items/1/submit` runs `GradingService`, which calls `BowtieModule.Grade`. The grade has one detail per option: `+1` for a correct option placed, `0` with `selected` for a wrong one, and `missed` for a correct option left out. Points and max points (5) go to `attempt_items`.
6. **Back to the UI.** The reveal (answer key, grade, rationale) returns in the submit response. `optionMarks` turns the detail into marks: the renderer shows them in each slot and on missed options in the lists.

## 5. Database changes

None. `bowtie` was already a value of the `item_type` enum, and items keep their payloads in the existing JSON columns. `question_versions.max_points` is 5 for every bowtie, from `BowtieModule.MaxPoints`.

## 6. Key decisions and deviations

- **Bowtie stays a Phase 2 type in `ItemTypeCatalog`.** The startup check still only requires the MVP types; bowtie is available because its module is registered. Tests that compared "available" with "MVP" now expect MVP types plus bowtie.
- **Authoring rules (filling a gap in interfaces.md 2.10, now documented there):** each list has 4 to 8 options, ids are unique across all three lists (`rationale.perOption` is keyed by id alone), and the key needs exactly 2 actions and 2 parameters from the right lists.
- **A half-filled bowtie is a valid answer.** Saving is allowed with empty slots, and it earns the points of the slots that are right, like a partly answered matrix. The response is null only when every slot is empty.
- **Keyboard model (new row in interfaces.md section 4):** pick-then-place, as the design says for drag-and-drop types. Picking moves focus to the first free slot of the right kind, so a placement takes two keypresses. dnd-kit only handles pointer drags here, not keyboard drags, so there is one keyboard model instead of two.
- **No strike-through tool on bowtie options.** Options are buttons that pick, so a strike button beside each would double the controls in a dense layout. The real exam has none on bowties either.
- **The sample file gained two questions** (now 46) and the sample bank lists them. Import and bank-count tests were updated to match.
- **Tests that used bowtie as their "not built yet" example now use hotspot** (importer, player and custom builder tests).

## 7. Tests

- **Domain** (`dotnet test tests/Nclex.Domain.Tests`): 20 `BowtieModuleTests` cover authoring rules, response limits, grading, grade detail order and the shuffle, plus 6 golden cases in `Bowtie/fixtures/scoring.json`, run by `GradingFixtureTests`.
- **Application** (`dotnet test tests/Nclex.Application.Tests`): `ItemTypeRegistrationTests` now checks that bowtie is the only Phase 2 type with a module.
- **Integration** (`dotnet test tests/Nclex.Api.IntegrationTests`, needs Docker): `BowtieAttemptTests` covers the item shown without its key, tutor submit and reveal (3 of 5), exam submit (5 of 5) and refused responses. `ImportQuestionsTests` checks the sample bowties import with max points 5, and `TaxonomyTests` checks bowtie is available.
- **Frontend** (`cd nclex-fe && npm test`): `BowtieRenderer.test.tsx` covers keyboard and tap placement, focus moving to the slot, replacing, removing, the wrong-kind hint, Escape, saved answers, reveal marks, and axe checks before and after the reveal. `ItemForm.test.tsx` covers the bowtie form's limits and id prefixes, and the gallery test now includes bowtie.

Run everything: `cd nclex-be && dotnet build && dotnet test`, then `cd nclex-fe && npm run lint && npm run typecheck && npm test`.

Results on this branch: `dotnet build` 0 warnings and 0 errors; `dotnet test` 1,420 passed; `npm run lint` and `npm run typecheck` clean; `npm test` 475 passed (76 files). I also checked the bowtie in a real browser (Chromium through Playwright, on `/dev/items`). Keyboard pick-and-place and a mouse drag both saved the expected response, and at 390 px the diagram and lists stack into one column.

## 8. Follow-ups and known limitations

- **No bowtie questions in the RN bank yet.** The content waves add about 150, with a `qb.py` helper so batch files can write bowties.
- **The adaptive exam can now pick the two sample bowties,** because it draws from every standalone published RN question. It counts a bowtie as right only with all 5 slots right, as it does for SATA. The RN bank waves will make bowties a normal part of the adaptive pool.
- **Dragging is not tested automatically.** jsdom can't simulate dnd-kit pointer drags; the pick-and-place path, which gives the same response, is tested. A Playwright test could cover dragging.
- **The dev item gallery scrolls sideways at phone width** because of the drop-down table sample (446 px wide at 390 px), not the bowtie. It is older than F20.
- **`next dev` printed a Next.js security advisory** for the installed version (16.4.2). Upgrading changes dependencies, so it waits for your decision.
- **The other Phase 2 types** (drag-and-drop cloze and rationale, drop-down rationale, hotspot) still show the "can't be answered yet" notice.

## 9. Try it yourself

1. **Change the slot names.** In `BowtieRenderer.tsx`, change `kindInfo.actions.slot` to "Nursing action". Run `npx vitest run src/features/player/renderers/BowtieRenderer.test.tsx` and fix the tests that relied on the old name.
2. **Allow 3 to 8 options per list.** Change `MinOptionsPerList` in `BowtieModule.cs` to 3, update `Each_list_needs_at_least_four_options` to match, and run the domain tests. Then decide whether the admin form's hint should change too.
3. **Add a golden case.** Add a case to `Bowtie/fixtures/scoring.json` where the candidate places only the two correct parameters, with `expectedPoints` 2. It runs with no code changes.
