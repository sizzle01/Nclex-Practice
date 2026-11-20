# F24: Timed case-study blocks

## 1. Summary
Candidates get a new Practice section, "Timed case-study blocks". Each block is 10 whole NGN case studies
(60 questions) taken under exam rules:
- one timer at the NCLEX-RN pace: 2 hours for 60 questions;
- one 10-minute break;
- no going back;
- answers and rationales only at the end, with no pass estimate.

The feature exists because the candidate's five-attempt NCLEX report showed clinical judgment falling away in the
second half of the exam. That points to stamina and pressure rather than knowledge, and untimed tutor-mode case
sets cannot train for it. Blocks mix body systems, and once published a block never changes, so finished attempts
keep their history as the bank grows.

## 2. How to try it
Build the bank (this also updates `timed-blocks.json`), then import it into a local database:
```bash
cd nclex-be/content/rn-bank && python3 -I build_all.py
cd ../.. && export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_f24;Username=nclex;Password=nclex"
dotnet build && dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
for f in $(ls content/rn-bank/rn-bank-*.rn.json | sort -V); do
  dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/$f"
done
```
Next:
1. Run the API and `npm run dev` in nclex-fe.
2. Sign in with any verified candidate account.
3. Open **Practice**. "Timed case-study blocks" sits between "Case studies" and "Mock exams: timed".
4. Start "Timed case study block 1". The top bar shows a 2:00:00 exam timer and has no Previous button.

## 3. Reading order
1. `nclex-be/content/rn-bank/catalog.py`: `_timed_blocks`, `_choose_block` and `_case_system`, with the
   constants `BLOCK_CASES`, `BLOCK_MAX_PER_SYSTEM`, `NCLEX_SECONDS_PER_QUESTION` and `BLOCK_PAUSE_POLICY`.
   These build the blocks.
2. `nclex-be/content/rn-bank/timed-blocks.json`: the published blocks (case ids and question ids), written by the
   build. Never edit it by hand.
3. `nclex-fe/src/features/practice/setCategories.ts`: `sectionOf` puts a timed case-study challenge in the new
   section.
4. `nclex-be/tests/Nclex.Api.IntegrationTests/Catalog/TimedCaseBlockTests.cs`: a block imported and taken as a
   candidate.

## 4. Request flow
1. **Build.** `python3 -I build_all.py` builds every batch. `catalog.build_catalog` groups case questions by case
   and calls `_timed_blocks`, which:
   - re-checks every recorded block against the current cases, and stops the build if a case in a published
     block changed or disappeared;
   - forms new blocks from the cases not yet in one, choosing the earliest case from the least-used body system,
     with at most 3 cases per system;
   - writes the result back to `timed-blocks.json`.
2. **Import.** Each block becomes a test set with `mode: challenge`, `revealMode: exam`, `timed: true`,
   `timeBudgetSec` = questions × 120, `allowBackNav: false` and `pausePolicy: {maxPauses: 1, maxPauseSec: 600}`.
   It goes into the last batch file, and the importer checks that each case stays whole and in order.
3. **Practice page.** The UI lists the sets as before. `sectionOf` sees a timed challenge whose title contains
   "case stud" and shows it under "Timed case-study blocks".
4. **Taking a block.** Start, autosave, the exam timer, expiry and grading are the existing challenge-mode path
   (architecture.md 5.1 to 5.5). Answers and rationales appear on the review page after submission.

## 5. Database changes
None. Blocks are ordinary curated test sets.

## 6. Key decisions and deviations
- **Branch base.** The plan said "from main". The branch is stacked on `content/case-conversion` because blocks
  are built from the bank's cases, which include the 16 new cardiac cases that are not merged yet. Merge order:
  `content/rn-bank-wave-02`, then `content/case-conversion`, then this branch.
- **Up to 3 cases per body system.** The first version filled blocks in case order, and block 4 came out all
  cardiac, because the conversion writes topic by topic. A block now forms only when it can mix at least four
  systems. Today the bank's 42 cases make 3 blocks. The other 12, mostly cardiac, wait for later conversion
  chunks.
- **No pass estimate** (decided by the user): the score and breakdowns point to where judgment breaks down.
  F25 adds the clinical judgment panel.
- **One 10-minute pause.** NCLEX breaks are optional and run on the exam clock. In this app a pause freezes the
  timer (architecture.md 5.1), so the break does not use exam time.
- **Cases appear in tutor sets too.** A case can be in both a tutor case set and a timed block, so a candidate may
  meet a case they have already seen.

## 7. Tests
- Backend, in `TimedCaseBlockTests`:
  - a block starts with exam rules, no going back, the NCLEX time budget and no pass threshold;
  - it serves each case whole and in order;
  - it stores the one-pause policy.
- Frontend: `setCategories.test.ts` checks the new section, its place in the order, and that an untimed
  case-study challenge stays with the untimed exams.
- The build checks that published blocks never change.
- Real data: a trial import of the whole bank created 3 blocks with the expected settings.

To run them:
```bash
cd nclex-be && caffeinate -i dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```
Results at commit time: 1,424 backend tests passed; lint and typecheck clean; 477 frontend tests passed.

## 8. Follow-ups and known limitations
- F25 adds the clinical judgment results panel: each step's score, first half versus second half, and time per
  question.
- Breaks that run on the exam clock, as on the NCLEX, would need a challenge-mode option in the attempt engine.
- Blocks have no standalone questions. The real exam mixes cases with standalone items, which the adaptive exam
  covers.
- Show a candidate which blocks they have finished, and suggest the next one.

## 9. Try it yourself
1. Change `BLOCK_MAX_PER_SYSTEM` to 2 and rebuild in a scratch copy. How many blocks can the bank make now? Delete
   `timed-blocks.json` first, or the recorded blocks stay.
2. Change `NCLEX_SECONDS_PER_QUESTION` to 150, a gentler pace for a first attempt, and check the description and
   `timeBudgetSec` in the last batch file.
3. Edit the stem of a question in block 1, rebuild and confirm the build still passes. Then move that question to
   another case position and watch the build stop. Why is one change allowed and not the other?
