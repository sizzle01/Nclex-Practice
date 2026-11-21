# F26: Retiring questions replaced by case studies

## 1. Summary
The bank is being rebuilt around full NGN case studies, one topic at a time (CASE-MAP.md). Until F26, the old
isolated questions for a converted topic stayed live beside the new cases. A candidate therefore still met mostly
single questions, which is the pattern the candidate's NCLEX report warned against.

F26 retires those old questions:
- Content files can now mark a question as retired, and the importer retires it for good.
- A committed registry, `retired-questions.json`, records which questions were retired and which conversion chunk
  replaced them.
- The practice catalog leaves retired questions out of every set.

For chunks 1 to 4 (cardiac, pulmonary, endocrine, GI), 368 clinical questions are retired. The active bank falls
from 2,310 to about 1,942 questions, and the share inside full case studies rises from 23% to 28%.

## 2. How to try it
```bash
cd nclex-be/content/rn-bank
python3 -I retirement.py 1 2 3 4      # adds chunks 1-4 to retired-questions.json; writes review/retirement-review.csv
python3 -I build_all.py               # marks those questions "status": "retired" and filters the catalog
cd ../..
for f in $(ls content/rn-bank/rn-bank-*.rn.json | sort -V); do
  dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/$f"   # prints "... N retired."
done
```
Then:
1. Open `/practice`. The topic parts show smaller question counts.
2. Start a part. Retired questions never appear.
3. In the admin question search, filter by status "retired" to see them.

## 3. Reading order
1. `nclex-be/content/rn-bank/retirement.py`: which questions are chosen, and why.
2. `nclex-be/content/rn-bank/retired-questions.json`: the registry. Entries are only ever added.
3. `nclex-be/content/rn-bank/qb.py` (`RETIRED`, `_finish`): sets `"status": "retired"` in the output files.
4. `nclex-be/content/rn-bank/catalog.py` (`build_catalog`): removes retired questions from every set; a part left
   empty is unpublished.
5. `nclex-be/src/Nclex.Application/Content/ImportQuestions/QuestionImportFile.cs` (`ImportedQuestion.Status`).
6. `nclex-be/src/Nclex.Application/Content/ImportQuestions/ImportQuestionsHandler.cs` (`WriteAsync`,
   `CheckRetiredNotListed`): retires the question and records an audit entry.
7. `nclex-be/src/Nclex.Application/Content/ImportQuestions/CatalogImporter.cs` (`LoadRetiredQuestionIdsAsync`):
   unpublished sets may keep listing retired questions.
8. `nclex-be/tests/Nclex.Api.IntegrationTests/Content/ImportRetiredQuestionsTests.cs`.

## 4. Request flow
1. **Selection.** `retirement.py` reads the built files and picks the questions to retire. It appends them to
   `retired-questions.json` and writes a spot-check CSV.
2. **Build.** `build_all.py` builds every batch. `qb._finish` adds `"status": "retired"` to each registered question.
   `catalog.build_catalog` cuts practice parts from all standalone questions, retired ones included, so a part's id
   and membership stay stable. It then removes retired questions from each part. A part left empty is published as
   hidden, with its original list.
3. **Import.**
   - The worker reads each file.
   - `ImportQuestionsHandler` validates the file, then refuses it if a published set or bank in the same file lists
     a question that the file retires.
   - It publishes content as before. For each question marked retired and still active, it calls
     `Question.Retire(now)` and writes a `question.retire` audit event with the content importer as the actor.
   - It never brings a retired question back.
   - `CatalogImporter` accepts retired questions only in unpublished sets.
4. **Use.** Existing code already skips retired questions:
   - attempt start leaves them out of curated sets (`StartAttemptHandler.CurrentVersionsAsync`);
   - the adaptive exam never picks them (`AdaptivePicker`);
   - the custom test builder never includes them (`QuestionPool`).
   Attempts already taken keep their items and history.

## 5. Database changes
None. `questions.status` and `retired_at` already exist (database.md). The import-file format gains an optional
question `status` field (`active` or `retired`).

## 6. Key decisions and deviations
- **What is retired.** Old questions (batches below 100) that are not inside a case study, belong to a converted
  topic, and are clinical (Physiological Integrity or Health Promotion).
  - Management of care, safety and psychosocial questions stay. The cases do not replace them, and they are areas
    the candidate needs.
  - The first draft also matched keywords in answer options. That pulled in unrelated management and safety
    questions (an ICU bed shortage, a hurricane shelter), so matching now uses the stem only.
- **Kept as standalone.** Bowtie items stay. Priority and delegation items are kept because their category stays.
- **The 26 older case studies stay** for a later review against the six-step standard (user decision).
- **Only one way.** Neither the importer nor the registry can un-retire a question; an admin would have to act. This
  stops a later `deploy --seed` from bringing retired questions back.
- **Practice parts are not rebalanced yet.** Some parts shrink to 22 to 43 questions. Rebalancing comes at the end
  of the conversion (user decision).
- **Unpublished sets may list retired questions.** The fixed-length challenges and any emptied practice part keep
  their original lists for history.

## 7. Tests
- `ImportRetiredQuestionsTests`:
  - retiring through a file, with the audit event;
  - never bringing a question back;
  - refusing a published set that lists a question the same file retires;
  - an earlier set skipping a question retired later;
  - an unpublished set keeping a retired question.
- Trial import over a copy of today's bank: 368 retired, 1,988 active (including the 46 sample questions), 66
  published sets, no empty parts.

To run them:
```bash
cd nclex-be && caffeinate -i dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```
Results at commit time: 1,453 backend tests passed; lint and typecheck clean; 503 frontend tests passed.

## 8. Follow-ups and known limitations
- Each new conversion chunk runs `retirement.py <chunk>` in the same commit as its cases.
- Rebalance the practice parts at the end of the conversion.
- Review the 26 older case studies against the six-step standard.
- Keyword matching can still miss or include an odd question. The CSV in `review/` is for spot-checks.
- Re-importing old batch files on the VM overwrites any admin edits to those questions. Check for admin edits
  before shipping.

## 9. Try it yourself
1. Run `python3 -I retirement.py 5` before chunk 5 exists. What does it retire, and why is that the wrong order?
2. Change `CLINICAL` to include `SECE.SIC` and compare the counts. Which safety questions would be lost?
3. In a scratch database, import a file that marks a question retired, then import it again without the status.
   Check `questions.status`. Why does the importer behave this way?
