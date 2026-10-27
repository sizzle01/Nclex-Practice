# F04: Taxonomy, question model and item-type framework (MC and SATA)

## 1. Summary

The platform can now store questions. It has the NCLEX test-plan taxonomy for RN and PN: the Client Needs tree with its weights, plus body systems and CJMM skills. Questions are versioned and immutable, so a past attempt always shows what the candidate actually saw. Each item format is a pluggable module that validates authored items, validates responses, works out max points, grades, and shuffles options with a fixed seed. This feature builds the framework and two modules: Multiple Choice (0/1) and Select All That Apply (+/− with a floor of 0). A content importer, run as a worker command, loads a JSON file of questions, rejects the whole file if any item is invalid, and publishes the rest; importing the same file again changes nothing. `GET /api/v1/taxonomy?track=RN|PN` returns the tag tree that the test builder and authoring screens will use.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f04` (see section 6).

```bash
docker compose up -d                 # Postgres, Redis, MinIO, Mailpit
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F04_Taxonomy (taxonomy is seeded here)

# Import the 20 sample RN questions (10 MC, 10 SATA).
# Pass an absolute path: dotnet run starts the worker in its own project folder.
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   Imported 20 RN question(s): 20 created, 0 updated, 0 unchanged.
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
#   Imported 20 RN question(s): 0 created, 0 updated, 20 unchanged.

dotnet run --project src/Nclex.Api   # http://localhost:5080
```

A file with problems is rejected as a whole, with exit code 1:

```
Nothing was imported. 2 problem(s) in /…/bad.json:
  questions[3].rationale.perOption.b: MISSING_RATIONALE - Option "b" needs a rationale.
  questions[5].clientNeeds: UNKNOWN_TAG - "PHI" is not a Client Needs subcategory of this track.
```

Call the taxonomy endpoint with any signed-in account (sign up at `/signup` or with curl):

```bash
ACCESS=$(curl -s -X POST http://localhost:5080/api/v1/auth/signup -H 'content-type: application/json' \
  -d '{"email":"ada@example.com","password":"correct-horse-battery","track":"RN"}' | jq -r .accessToken)

curl -s "http://localhost:5080/api/v1/taxonomy?track=RN" -H "Authorization: Bearer $ACCESS" | jq '.clientNeeds[0]'
# { "id": 1, "code": "SECE", "name": "Safe & Effective Care Environment", "children": [
#   { "id": 2, "code": "SECE.MOC", "name": "Management of Care", "weightMin": 15.00, "weightMax": 21.00 }, … ] }

curl -s "http://localhost:5080/api/v1/taxonomy?track=XN" -H "Authorization: Bearer $ACCESS"
# 400 {"code":"VALIDATION_FAILED","errors":[{"field":"track","code":"INVALID_VALUE"}],…}
```

Look at what was imported:

```bash
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "select q.id, v.version, v.status, v.item_type, v.max_points, left(v.stem, 50) from questions q
   join question_versions v on v.id = q.current_version_id order by q.id limit 5;"
```

Test accounts: none. The importer creates the system account `content-import@system.local` (role `author`, no password, so it can't log in) and records it as the author of imported versions.

## 3. Reading order

Backend (`nclex-be/src`):
1. `Nclex.Domain/ItemTypes/ItemType.cs` and `Phase.cs`: the 17 item formats and their product phase.
2. `Nclex.Domain/ItemTypes/ItemPayloads.cs`: `Option`, `Rationale`, `GradeDetail`, `GradeResult`, `ValidationIssue`, the shared shapes from interfaces.md section 1.
3. `Nclex.Domain/ItemTypes/IItemTypeModule.cs`: the plug-in contract.
4. `Nclex.Domain/ItemTypes/ItemTypeModule.cs`: the typed base class. It reads JSON once (strictly, via `ItemJson`) and delegates to typed overrides.
5. `Nclex.Domain/ItemTypes/OptionRules.cs`: option, rationale-coverage and selection checks shared by option-list types.
6. `Nclex.Domain/ItemTypes/MultipleChoice/` and `Sata/`: payload records plus the two modules. **Grading lives here.**
7. `Nclex.Domain/ItemTypes/SeededShuffle.cs`: SHA-256-seeded Fisher–Yates used by `Present`.
8. `Nclex.Domain/ItemTypes/ItemTypeRegistry.cs` and `ItemTypeCatalog.cs`: module lookup, and labels and phases for all 17 types.
9. `Nclex.Domain/Entities/ClientNeedsCategory.cs`, `BodySystem.cs`: taxonomy rows.
10. `Nclex.Domain/Entities/AuthoredItem.cs`, `QuestionVersion.cs`, `Question.cs`: the versioned question. `Question.Publish` holds the "supersede the old version" rule.
11. `Nclex.Domain/Entities/MediaAsset.cs`, `CaseStudy.cs`, `QuestionBank.cs`, `QuestionBankItem.cs`: tables only for now.
12. `Nclex.Infrastructure/Persistence/Seed/TaxonomySeedData.cs`: the RN and PN test plans and the body systems.
13. `Nclex.Infrastructure/Persistence/Configurations/Question*.cs`, `ClientNeedsCategoryConfiguration.cs` and the rest, then `Migrations/*_F04_Taxonomy.cs`.
14. `Nclex.Application/DependencyInjection.cs`: `AddContentImport` and `AddItemTypes` (module registration).
15. `Nclex.Application/Taxonomy/`: `CjmmSkillCatalog`, then `GetTaxonomy/` (request, validator, response, handler).
16. `Nclex.Api/Controllers/TaxonomyController.cs`.
17. `Nclex.Application/Content/ImportQuestions/`: `QuestionImportFile` (file format), `ImportResult`, `ImportQuestionsHandler`.
18. `Nclex.Worker/ImportQuestionsCommand.cs` and the first lines of `Program.cs`.
19. `nclex-be/content/sample-questions.rn.json`: the sample content and the file format by example.

Tests (`nclex-be/tests`):

20. `Nclex.Domain.Tests/ItemTypes/*/fixtures/scoring.json` and `GradingFixtureTests.cs`: the golden grading cases.

Frontend: only `nclex-fe/src/lib/api/schema.d.ts` changed (regenerated). There are no screens in this feature.

## 4. Request flow: `GET /api/v1/taxonomy?track=RN`

1. **UI.** No screen yet. The test builder (F07) and authoring (F14) will call this through the generated client (`paths["/api/v1/taxonomy"]`).
2. **API.** JWT bearer authentication; the controller is `[Authorize]`. MVC binds `?track=RN` into `GetTaxonomyRequest(ExamTrack? Track)`.
   - An unparseable value such as `XN` fails model binding, and `ValidationProblems.FromModelState` returns `400 INVALID_VALUE` on `track`.
   - A missing value reaches `FluentValidationFilter`, and `GetTaxonomyRequestValidator` returns `400 REQUIRED`.
3. **Application.** `GetTaxonomyHandler`:
   - loads the track's `client_needs_categories` ordered by `sort_order`, and all `body_systems`, with `AsNoTracking`
   - builds the tree in memory: parents get `children`, leaves keep their weights, and null fields are left out of the JSON
4. **Domain.** Item types come from `ItemTypeCatalog.All`. `available` is `ItemTypeRegistry.IsAvailable(type)`, so it's true only for types with a registered module (today `multiple_choice` and `sata`). CJMM skills come from `CjmmSkillCatalog`.
5. **Database.** Two small reads of seed tables (20 + 16 rows in total).
6. **Back out.** 200 with `TaxonomyResponse`. Enums are serialised in snake_case (`"type": "multiple_choice"`, `"key": "recognize_cues"`).

**Importing a file** follows a similar path: `ImportQuestionsCommand` → `QuestionImportFile.Parse` → `ImportQuestionsHandler`.
1. Load the tags and any existing questions with the file's ids.
2. Validate everything: file-level rules, tags, then `module.ValidateAuthoring` for each item. Any issue returns without writing.
3. In one transaction: get or create the importer account, then for each question choose *create*, *new version* or *unchanged* using `QuestionVersion.Matches`. Matching compares JSON by value, because jsonb reorders keys.
4. Save, then call `Question.Publish` for each new version (superseding the old one), save again, and commit. Two saves are needed because `questions.current_version_id` and `question_versions.question_id` reference each other.

## 5. Database changes

Migration `F04_Taxonomy`:

| Change | Why |
|---|---|
| Enums `item_type`, `difficulty_tier`, `cjmm_skill`, `version_status`, `question_status`, in DDL order | database.md 2.1. The annotation is hand-edited because EF sorts labels alphabetically (as in F01 and F02). |
| `client_needs_categories` (smallserial, self-FK `parent_id`, `numeric(5,2)` weights, `UNIQUE (track, code)`) | The test plan tree (database.md 2.3) |
| `body_systems` (smallserial, unique `slug`) | Body-system tags |
| Seed rows: RN ids 1–10, PN ids 11–20, body systems 1–16 | Fixed ids so parents can be referenced. Npgsql moves the sequences past the seeded ids at the end of the migration. |
| `media_assets` with `media_assets_kind_check`, unique `storage_key` | database.md 2.4. Used from F14. |
| `case_studies` | database.md 2.4. Used by case-study items later. |
| `questions`, and `question_versions` with every DDL column, default and CHECK (`time_allotment_sec > 0`, `max_points > 0`, case study and position set together) | The versioned question model |
| `questions_current_version_fk`, added after both tables exist | The circular FK in the DDL |
| `qv_filter_idx` (partial, `status = 'published'`), `qv_question_idx (question_id, version DESC)`, `qv_review_queue_idx` (partial, `in_review`), unique `(question_id, version)` | Builder pool query, version lookup, review queue, and version numbering |
| `question_banks`, `question_bank_items` (composite PK, cascade on bank delete) | database.md 2.4. Managed from F14. |
| FK indexes EF adds automatically (`ix_question_versions_author_id`, `ix_questions_current_version_id`, …) | Not in the DDL. Same convention as earlier features. |

## 6. Key decisions and deviations

- **Branch base and worktree.** F03 isn't finished or merged, and another session was committing F03 work in the main folder while this feature was being built. To keep the two apart, this branch:
  - starts from F03's latest commit (`348b785`), which keeps the EF migration chain linear (F03_Audit → F04_Taxonomy)
  - lives in a separate git worktree at `../Nclex-Practice-f04`

  When F03 changes, rebase this branch onto it.
- **Weights and codes.** The PRD isn't in the repo, so the RN and PN values come from the 2023 NCSBN test plans. They match api.md's MOC and SIC example. Codes: `SECE`, `SECE.MOC`/`SECE.CC`, `SECE.SIC`, `HPM`, `PSI`, `PHI`, `PHI.BCC`, `PHI.PPT`/`PHI.PT`, `PHI.RRP`, `PHI.PA`. **Please check these against PRD 2.4.**
- **Body systems.** No list exists in the design, so I seeded the 16 proposed in the plan.
- **CJMM prompts.** Only "What matters most?" is in api.md. The other five prompts are mine (`CjmmSkillCatalog`).
- **Import identity.** Each question in a file carries a fixed UUID. This makes re-import idempotent without adding an `external_key` column to the DDL.
- **Importer author and review.** `question_versions.author_id` is NOT NULL, so imports are attributed to a system account with no password. Imported versions are created `approved`, with `review_notes = 'Imported'` and no reviewer, then published. The file stands in for review until F14 brings the real workflow.
- **Importer as a worker command.** It's a worker command, not a separate project, so the solution keeps the architecture.md section 4 layout. It registers only `AddContentImport()` (item types and the handler), because `AddApplication()` also pulls in auth services that only the API host provides.
- **No startup "every enabled type has a module" check yet.** interfaces.md section 3 asks for one, but with only MC and SATA built it would fail. It belongs in F05, when every MVP module exists.
- **`Option` keeps its name.** The analyzer (CA1716) flags it as a VB keyword. It's suppressed with a justification, because the name comes from interfaces.md.
- **`GradeDetail.Selected`.** It's filled in for selected options in both modules, so the results UI can tell "chose a wrong option" (delta 0 in MC) from "didn't choose". This adds to the interfaces.md sample, which leaves the field out.
- **MC option count.** At least 2 options, with no upper limit; the design only says "typically 4". SATA enforces 5–10, as in the design.
- **Weights in JSON** are `15.00` rather than `15`, because `numeric(5,2)` maps to C# `decimal`, which keeps the scale. It's the same number to any JSON reader.

## 7. Tests

Run everything:

```bash
cd nclex-be && dotnet build && dotnet test           # needs Docker for Testcontainers
cd ../nclex-fe && npm run lint && npm run typecheck && npm test
```

Results on this branch (with F03's commits included):
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet test`: 337 passed, 0 failed (Domain, Application and API integration).
- `npm run lint`: clean. `npm run typecheck`: clean. `npm test`: 18 files, 76 tests passed.

What's covered:
- **Golden grading fixtures** (`Nclex.Domain.Tests/ItemTypes/<Type>/fixtures/scoring.json`, one `[Theory]`):
  - MC: correct, wrong, nothing selected, no response.
  - SATA: the +/− examples (2 right + 1 wrong = 1; every option selected = 1), the floor at 0, empty and null responses, and selection order.
- **Authoring validation:**
  - a missing or blank per-option rationale is rejected; so is a rationale for an option that doesn't exist
  - a missing overall rationale is rejected
  - unknown or duplicate key ids, duplicate option ids, and the option-count limits (MC fewer than 2; SATA 4 or 11) are rejected
  - an empty SATA key is rejected
  - wrongly shaped JSON is reported as an issue rather than thrown
- **Response validation:** unknown or repeated ids, a null selection, and a response that isn't an object.
- **Present:** same seed gives the same order; every option is kept; different seeds vary; grading is unchanged after a shuffle; a pinned order for a known seed guards the algorithm.
- **Registry and catalogue:** lookups, unavailable types, and every enum value described once.
- **Publishing rules** (`QuestionTests`): first publish, supersede, a wrong "current" version, another question's version, double publish, and `Matches` ignoring JSON key order.
- **Import integration:**
  - the sample file imports 20 questions, and a second import leaves all 20 unchanged
  - a file with one missing rationale writes nothing
  - unknown tags (including a parent category), duplicate ids and unsupported types are reported
  - an edited question gets version 2, which supersedes version 1
  - a change to the access tier alone adds no new version
  - a question can't move to the other track
- **`GET /taxonomy` integration:**
  - the RN tree and weights, the PN plan, body systems, item-type availability and CJMM skills
  - the raw JSON shape (absent weights and children, snake_case enums)
  - 400 for a missing or unknown track, and 401 without a token
- **Migration:** enum label order, the three `qv_*` indexes, `questions_current_version_fk`, and the seeded sequences' next ids.

## 8. Follow-ups and known limitations

- **F05:** the other MVP item-type modules, then the startup check that every enabled type has a module.
- **F14:** admin authoring and the review workflow. It will replace the importer's "approved without a reviewer" shortcut for real content.
- **Importer account and password reset.** The importer account has no password, but "forgot password" would still email a reset link to `content-import@system.local`. Nothing is delivered to `.local`, but an F03 or F14 follow-up should refuse resets for system accounts.
- **Exhibits, media and case studies.** The importer doesn't check exhibit contents or media ids yet. Case-study fields aren't in the file format.
- **Option shuffling.** It can't be switched off per item, for options such as "All of the above".
- **Sample content isn't clinically reviewed.** It says so in its `notice` and in every rationale's `references`. Replace it before any real use.
- **Generated client types.** Numbers and ids appear as `number | string`, because of .NET's OpenAPI number handling. This happens project-wide and is worth a look when the frontend first consumes `/taxonomy`.
- **Questions on two tracks.** If a question needs to exist on both RN and PN, that needs a design decision. Today the importer refuses to move a question between tracks.

## 9. Try it yourself

1. **Add a golden case.** In `nclex-be/tests/Nclex.Domain.Tests/ItemTypes/Sata/fixtures/scoring.json`, add a case where the candidate selects one correct and one wrong option, and predict `expectedPoints` before running `dotnet test --filter GradingFixtureTests`. No C# changes are needed.
2. **Tighten MC authoring.** Make `MultipleChoiceModule` reject items with more than 6 options: pass a `max` to `OptionRules.CheckOptions`. Add a test in `MultipleChoiceModuleTests` next to `Fewer_than_two_options_are_rejected`.
3. **Edit and re-import.** Change the stem of one question in `content/sample-questions.rn.json`, import the file twice, and read the output: "1 updated", then "20 unchanged". Then query `question_versions` for that question id to see version 1 `superseded` and version 2 `published`.
