# RN question bank (content)

## 1. Summary
The platform now ships with an NCLEX-RN practice bank of 1,110 original questions in 46 import files under
`nclex-be/content/rn-bank/`. It covers every Client Needs category of the NCSBN test plan, all 12 MVP item types,
10 six-item NGN unfolding case studies, and 197 study subtopics that drive "study this next" (F17).
Every question is a **draft**: it carries a DRAFT reference line and must be approved by an RN or nurse educator
before candidates rely on it. A generated review sheet (CSV and printable HTML) supports that sign-off.

Two sessions built it: `feature/rn-question-bank` (batches b01-b38) and `feature/rn-question-bank-gaps`
(b60-b67, the review sheet, and a merge of b01-b38).

## 2. How to try it
```bash
docker compose up -d postgres
cd nclex-be
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_qbank;Username=nclex;Password=nclex"
dotnet build && dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api
# sample first (it creates the original subtopics), then every bank file in name order
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
for f in content/rn-bank/rn-bank-*.rn.json; do
  dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/$f"
done
```
Rebuild the JSON from the batch sources and print the category mix: `cd content/rn-bank && python3 -I build_all.py`.
Build the review sheet: `python3 -I review_sheet.py` (writes `review/rn-bank-review.csv` and `.html`, gitignored).

## 3. Reading order
1. `nclex-be/content/rn-bank/qb.py` - helpers for every item type, the shared subtopic registry, and the checks the importer enforces.
2. `batches/b02_management_of_care_priorities.py` - a typical batch: options, keys and rationales kept together.
3. `batches/b60_ngn_cases_1.py` - how `case()` builds NGN unfolding cases with tabs and six CJMM-ordered items.
4. `build_all.py` - builds every batch and checks ids are unique across the bank.
5. `review_sheet.py` - renders each item type in plain language for the clinical reviewer.

## 4. Request flow
Batch file (Python) -> `qb.build()` checks and writes `rn-bank-NN-*.rn.json` -> `import-questions` (Nclex.Worker)
-> `QuestionImportFile.Parse` -> `ImportQuestionsHandler` (item-type validators, `SubtopicImporter`, `CaseStudyImporter`,
one transaction per file) -> `questions`, `question_versions`, `case_studies`, `subtopics`, `question_subtopics`.
Candidates then meet the questions through the custom builder (F07) and attempt engine (F08).

## 5. Database changes
None. Content only; it uses the tables from F04, F11 and F17.

## 6. Key decisions and deviations
- **Original content.** No question is copied or paraphrased from UWorld, Saunders, Kaplan or ATI.
- **One subtopic registry** (`qb.SUBTOPICS`): a file that declares a slug renames it in the database, so all files must agree.
- **Ids** are `c1a{batch:05d}-...`; b60+ were reserved for the second session so the two never collided.
- **Table exhibits:** `columns` excludes the row-label column; the real importer rejected the opposite, and `qb._check` now enforces it.
- **Review sheet uses only the standard library** (no spreadsheet dependency added).
- **Category balance** (1,110 questions): MOC 15.1%, SIC 10.1%, HPM 7.4%, PSI 6.0%, BCC 6.5%, PPT 16.0%, RRP 9.0%,
  PA 29.8%. Every category is at or above its test-plan minimum; PA is above its 11-17% range, so prebuilt tests should
  sample by test-plan weights rather than at random.

## 7. Tests
- `python3 -I build_all.py` validates every item (keys, per-option rationales, item-type size limits, subtopic category, exhibit cells, ids).
- Real import into a fresh database: 1,154 questions (44 sample + 1,110 bank), 11 case studies, 197 subtopics, 0 failures.
- `caffeinate -i dotnet test`: 1,303 passed, 0 failed (no code changed; run as a regression check).

## 8. Follow-ups and known limitations
- **Clinical review is required** before release. Use the review sheet; record decisions; fix or retire items.
- Some topics are covered by both sessions (for example classic prioritization); a reviewer may retire near-duplicates.
- PA is over-represented; future content should go to PSI, HPM and BCC.
- Few `matrix_mr` and grouping items; add more NGN item types over time.
- No question banks or curated test sets are defined for this content yet.

## 9. Try it yourself
1. Add a `select_n` question to a new batch `b68_*.py`, run `build_all.py`, and import only its file.
2. Change one rationale, rebuild, re-import the file, and confirm the importer reports "1 updated".
3. Extend `review_sheet.py` to add a column with each question's word count, to spot overlong stems.
