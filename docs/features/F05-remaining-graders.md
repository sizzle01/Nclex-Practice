# F05: Remaining MVP item-type graders

## 1. Summary

Every MVP item format now has a module, so questions of all twelve MVP types can be authored, imported, validated and scored. F05 adds ten modules on top of the F04 framework:
- Select N
- Matrix Multiple Choice and Matrix Multiple Response
- Multiple Response Grouping
- Drop-Down Cloze and Drop-Down Table
- Highlight Text and Highlight Table
- Fill in the Blank (Calculation)
- Ordered Response

Each module checks authored items (including a rationale for every selectable id), checks candidate responses on autosave, works out max points, and grades with the scoring rule from interfaces.md section 2. Types that score the same way share one grader, as the design asks. The API and the importer now refuse to start if any MVP type is missing a module. The sample import file gains two questions of each new type (40 in total). This feature is backend only.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f05`, branched from `feature/f04-taxonomy-item-types` (see section 6).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # F04_Taxonomy; F05 adds no migration

dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   Imported 40 RN question(s): 40 created, 0 updated, 0 unchanged.
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   Imported 40 RN question(s): 0 created, 0 updated, 40 unchanged.
```

If you already imported F04's 20 questions, the first run prints `20 created, 0 updated, 20 unchanged`.

See the max points each module worked out:

```bash
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "select v.item_type, count(*), string_agg(v.max_points::text, ',') from questions q
   join question_versions v on v.id = q.current_version_id group by 1 order by 1;"
#  select_n         | 2 | 3,1    <- per option (select 3), then the all_or_nothing override
#  ordered_response | 2 | 1,5    <- all_or_nothing, then per_position with 5 items
```

`GET /api/v1/taxonomy?track=RN` (see the F04 walkthrough for a token) now reports `"available": true` for all twelve MVP types and `false` for the five phase 2 types.

A broken item is rejected with the path of the problem, for example a Select N key that lists 2 options when `selectCount` is 3:

```
questions[20].key.correct: MISMATCH - The key must list exactly 3 options, the select count.
```

There are no new endpoints or screens, and no new test accounts. The player renderers come in F11, and grading attempts comes in F10.

## 3. Reading order

Shared pieces (`nclex-be/src/Nclex.Domain/ItemTypes/`):
1. `IdRules.cs`: id checks for rows, blanks, segments and groups, plus `CheckRationale` for any list of selectable ids.
2. `PlusMinusScoring.cs`: the NCSBN +/− rule with a floor of 0, used by highlight items and by each group of the grouped types.
3. `ItemPayloads.cs`: the four new `ValidationIssueCodes`.
4. `ItemTypeRegistry.cs`: `MissingModules` and `EnsureEveryTypeHasAModule`, the startup check.

The modules, simplest first:

5. `SelectN/`: payloads (with `SelectNScoringOverride`) and the module.
6. `Matrix/MatrixPayloads.cs`, `Matrix/MatrixRules.cs` (grid checks, `row:column` cell ids), then `Matrix/MatrixMcModule.cs`.
7. `Grouped/GroupedPayloads.cs` and `Grouped/GroupedGrader.cs`: the shared grouped grader. Then `Matrix/MatrixMrModule.cs` (groups are rows or columns) and `Grouped/MultipleResponseGroupingModule.cs`.
8. `Dropdown/DropdownPayloads.cs` and `Dropdown/BlankGrader.cs`: the shared blank grader. Then `DropdownClozeModule.cs` and `DropdownTableModule.cs`, which differ only in where blanks are placed.
9. `Highlight/HighlightPayloads.cs`, `HighlightRules.cs`, then the text and table modules.
10. `FillBlankCalc/CalcInputParser.cs` (what counts as a number), then `FillBlankCalcModule.cs` (tolerance, rounding).
11. `OrderedResponse/`: payloads and the module.

Wiring:

12. `Nclex.Application/DependencyInjection.cs`: `AddItemTypes` registers all twelve modules, and `EnsureItemTypeModules` sets the enabled phase.
13. `Nclex.Api/Program.cs` and `Nclex.Worker/ImportQuestionsCommand.cs`: where the startup check runs.
14. `nclex-be/content/sample-questions.rn.json`: the twenty `f05a…` questions show every payload shape by example.

Tests (`nclex-be/tests`):

15. `Nclex.Domain.Tests/ItemTypes/<Type>/fixtures/scoring.json` and `GradingFixtureTests.cs`: the golden cases.
16. `Nclex.Domain.Tests/ItemTypes/*ModuleTests.cs`: validation tests for each module.
17. `Nclex.Application.Tests/ItemTypes/ItemTypeRegistrationTests.cs`: the real DI registration covers every MVP type.

## 4. Request flow: importing and grading a matrix multiple response item

No endpoint grades answers yet; F10 adds that. The two paths that run today are the importer, which uses every authoring check, and `IItemTypeModule.Grade`, which F10 will call.

**Import** (`import-questions content/sample-questions.rn.json`, question `f05a…05`):
1. **Worker.** `ImportQuestionsCommand` builds a host with `AddContentImport()`, then calls `host.Services.EnsureItemTypeModules()`. If an MVP type had no module, the command would stop here with an exception that names the missing types.
2. **Application.** `ImportQuestionsHandler` resolves the module with `ItemTypeRegistry.TryGet(ItemType.MatrixMr)` and calls `ValidateAuthoring(content, key, rationale)`.
3. **Domain.** The `ItemTypeModule` base class reads the JSON strictly into `MatrixMrContent` and `GroupedKey`. A missing `groupAxis` becomes an `INVALID_SHAPE` issue, not an exception. `MatrixMrModule` then:
   - checks the grid with `MatrixRules.CheckGrid`
   - builds one `ChoiceGroup` per row, holding cells `r1:c1 … r1:c4`
   - checks the key with `GroupedGrader.CheckKey`: every row needs at least one correct cell, and only cells from that row
   - checks that `rationale.perOption` has all 12 cell ids with `IdRules.CheckRationale`
4. **Max points.** `GroupedGrader.MaxPoints` adds up the correct cells (2 + 1 + 1 = 4). The handler stores that in `question_versions.max_points`.
5. **Database.** Same as F04: rows in `questions` and `question_versions`, created then published in one transaction.

**Grade** (what F10 will call with a saved response):
1. `IItemTypeModule.Grade(content, key, response)`. A null response, meaning blank or timed out, scores 0.
2. `MatrixMrModule.Grade` passes its row groups to `GroupedGrader.Grade`.
3. For each group, `PlusMinusScoring.Score` gives +1 for each correct cell selected, −1 for each wrong one, and `Missed` for each correct cell left out. That group's total is floored at 0.
4. The group points are added up, the details are concatenated in display order, and the result comes back as a `GradeResult` with `IsCorrect` only when every point is earned.

## 5. Database changes

None. F04's `item_type` enum already lists every type. `question_versions.max_points` holds what each module computes, and the column's `CHECK (max_points > 0)` holds because every module's max is at least 1.

## 6. Key decisions and deviations

- **Branch base and worktree.** F05 starts from the F04 branch (`c911bb6`), which includes F03. It lives in `../Nclex-Practice-f05` so it doesn't disturb the F03 and F04 checkouts. If F04 changes before it's merged, rebase this branch onto it.
- **Select N override (agreed in the plan).** interfaces.md 2.3 says to set `scoringOverride: 'all_or_nothing'` "on the key", but `SelectNKey` in the doc has no such field. F05 adds `scoringOverride?: 'all_or_nothing'` to the key:
  - **Default:** one point per correct selection. Wrong selections cost nothing and show in the detail with `Selected`. Max points equals `selectCount`.
  - **Override:** 1 point only when exactly the correct options are selected, so max points is 1.
- **`rationale.perOption` keys by type.** The design only spells these out for drop-downs. Every selectable id needs one, and no other ids are allowed:

  | Type | Keys |
  |---|---|
  | `select_n`, `multiple_response_grouping`, `ordered_response` | option or item ids. Grouping option ids must be unique across groups for this reason. |
  | `matrix_mc`, `matrix_mr` | every cell, `rowId:columnId` |
  | `dropdown_cloze`, `dropdown_table` | `blankId:optionId` for every option of every blank |
  | `highlight_text`, `highlight_table` | only the *selectable* segment or row ids |
  | `fill_blank_calc` | none; `perOption` must be empty |

- **No `:` in row, column or blank ids.** Cell and rationale keys are joined with `:`. Without this rule, row `a:b` + column `c` and row `a` + column `b:c` would both become `a:b:c`.
- **Calculation rules (agreed in the plan):**
  - When a tolerance is set, it decides on its own: |answer − value| ≤ tolerance. Otherwise `round` compares both numbers rounded to `content.decimals`, and `exact` needs equal values (`125.0` equals `125`).
  - Rounding is half away from zero (0.25 → 0.3), the convention taught for dosage maths, not .NET's default banker's rounding (0.25 → 0.2). Sample question 18 depends on this: 8.65 mL → 8.7 mL.
  - The parser accepts only an optional sign, digits and `.`, with surrounding spaces ignored. `12,5`, `1,000`, `1e3` and `12 mL` are all `NOT_A_NUMBER`.
- **An empty calculation box is a valid autosave.** This differs from the plan's wording, which listed an empty value as rejected. Clearing the box is a normal thing to do mid-exam, so autosave accepts it, and it scores 0. The parser itself still treats an empty string as "not a number".
- **Rules the design doesn't state, chosen to keep items meaningful:**
  - Select N needs at least 3 options, and `selectCount` from 1 to options − 1.
  - Matrix items need at least 1 row and 2 columns.
  - Every matrix MR or grouping group needs at least 1 correct choice, so no group is worth 0.
  - Grouping groups and drop-down blanks need at least 2 options.
  - Highlight items need at least 2 selectable parts and 1 correct part.
  - Ordered response needs at least 3 items.
- **Partial answers on autosave.** Matrix MC rows and drop-down blanks may be null or absent. Grouped responses may leave groups out. An ordered response must always be a full permutation, because the player always holds every item.
- **Ordered response detail.** There is one entry per position, targeted at the item that belongs there, with delta 1 when it's in place. With `all_or_nothing` scoring the deltas still show which positions were right, but only a fully correct order earns the single point. So the deltas don't add up to the points, just as SATA's floor can make them differ.
- **Present (seeded shuffle).** `select_n`, `ordered_response` and grouping options (shuffled within each group, groups kept in order) are shuffled, like MC and SATA. Matrix, drop-down, highlight and calculation items keep the authored order, because their layout carries meaning: a passage, a table, a sentence.
- **Folder layout.** architecture.md 5.4 shows one folder per type. Types that share a grader share a folder instead (`Matrix/`, `Grouped/`, `Dropdown/`, `Highlight/`), so the shared code and its users sit together. Test fixtures still have one folder per type.
- **Where the startup check runs.** It runs in the API and the import command, the two hosts that load item types today. The worker's background host doesn't register item types yet, so it gets the check when it starts grading (F10).
- **Small changes to F04 code:**
  - `OptionRules.CheckOptions` takes an optional `path`, so ordered response (`content.items`) and drop-down blanks can reuse it.
  - Three F04 tests were updated because more types are available now: the taxonomy "available" list, the sample-import counts (20 → 40), and the "unsupported type" import case, which now uses `bowtie` instead of `select_n`.
- **Analyzer.** `tests/.editorconfig` turns off CA1861 ("prefer static readonly arrays"). Inline arrays in tests are the test inputs, and moving them to fields would hide each case.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test                  # needs Docker for Testcontainers
cd ../nclex-fe && npm run lint && npm run typecheck && npm test
```

Results on this branch:
- `dotnet build`: 0 warnings, 0 errors. `dotnet format --verify-no-changes`: clean.
- `dotnet test`: 498 passed, 0 failed (Domain 260, Application 47, API integration 191).
- `npm run lint`: clean. `npm run typecheck`: clean. `npm test`: 18 files, 76 tests passed. The frontend is unchanged.
- The real importer loaded the 40-question file into a fresh database twice: 40 created, then 40 unchanged.

What's covered:
- **Golden fixtures** (80 new cases, at least 6 per type, one `[Theory]`). Each type has a blank response and a null response, plus:

  | Type | Edge cases |
  |---|---|
  | Select N | wrong picks don't cost points; `all_or_nothing` in any order; two of three right scoring 0 |
  | Matrix MR | the floor is per row (a row at −1 doesn't lower the other rows); column groups |
  | Grouping | the floor is per group; every option selected |
  | Drop-down | answers swapped between blanks; null and absent blanks |
  | Highlight | right and wrong picks cancelling; the floor at 0 |
  | Calculation | trailing zeros; spaces; round half away from zero; the tolerance edge; decimal comma; a unit typed in the box; an empty box |
  | Ordered | both scoring modes; one swap; reversed; every item shifted by one |

- **Validator tests** for each module:
  - unknown and repeated ids in keys and responses
  - Select N over-selection (`TOO_MANY`)
  - non-numeric calc input (`NOT_A_NUMBER`), with a theory table for the parser
  - non-selectable highlight picks (`NOT_SELECTABLE`)
  - cells or options taken from another group
  - template and table blank placement (`MISMATCH`, `DUPLICATE_ID`, `UNKNOWN_ID`)
  - `row:column` and `blank:option` rationale keys
  - the `:` rule
  - unknown enum values (`INVALID_SHAPE`)
  - a grouping option id repeated across groups
- **Present:** grouping shuffles inside each group only, and ordered response is seeded and keeps every item.
- **Registry:** `GradingFixtureTests.Every_mvp_item_type_has_a_fixtures_folder`, `ItemTypeRegistrationTests` (the real `AddContentImport()` covers every MVP type exactly once and registers no phase 2 modules), and `ItemTypeRegistryTests` (the startup check names the missing types and ignores phase 2).
- **Integration:** the sample import has two of each new type, with Select N max points 3 and 1 and ordered response 1 and 5. Taxonomy availability now lists all MVP types.

## 8. Follow-ups and known limitations

- **F10:** call the modules from `IGradingService`, and run `EnsureItemTypeModules` in the worker host once it grades. Response validation should run on autosave in F08.
- **F11:** the renderers need the payload shapes above. Note that matrix, drop-down and highlight content isn't shuffled.
- **Rationale volume for matrices.** One rationale per cell means a 4 × 4 matrix needs 16. If authors find that too heavy, a design decision could allow one rationale per row instead.
- **Highlight segmentation is trusted.** The module doesn't check that the segments rebuild the original passage; that's the authoring tool's job (F14).
- **The grader assumes a valid key.** Grading an item whose key misses a row or blank throws. Authoring validation and the importer prevent this, so it would mean the data was changed outside those paths.
- **The sample content isn't clinically reviewed**, the same as F04's. Every rationale's `references` says so.
- **Nothing enforces the "at least 6 golden cases" rule.** MC has 4 from F04.

## 9. Try it yourself

1. **Predict a grouped score.** In `tests/Nclex.Domain.Tests/ItemTypes/MatrixMr/fixtures/scoring.json`, copy the first case. Change the response so row 1 selects all four cells and row 3 selects nothing, then write down `expectedPoints` before running `dotnet test --filter GradingFixtureTests`. Hint: each row has its own floor.
2. **Give the drip-rate question a tolerance.** In `content/sample-questions.rn.json`, add `"tolerance": 1` to the key of question `f05a…17`, then import twice. The output shows `1 updated`, then `40 unchanged`. Add a fixture showing that `126` now scores 1 for that key.
3. **See the startup check fail.** In `DependencyInjection.EnsureItemTypeModules`, change `EnabledItemTypePhase` to `Phase.P2`, run `dotnet run --project src/Nclex.Api`, and read the error listing the five phase 2 types. Then change it back.
