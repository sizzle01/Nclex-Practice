# RN question bank (content)

## 1. Summary
The platform now ships with an NCLEX-RN practice bank of 1,110 original questions in 46 import files under
`nclex-be/content/rn-bank/`. It covers every Client Needs category of the NCSBN test plan, all 12 MVP item types,
10 six-item NGN unfolding case studies, and 197 study subtopics that drive "study this next" (F17).
On 17 November 2026 the content team (Muritala T. Aliyu) reviewed and approved all 1,110 questions; each rationale now
ends with "Reviewed by the content team on 17 November 2026." That round recorded no clinical credential, so it is not
an RN sign-off. `nclex-be/content/rn-bank/REVIEW-LOG.md` records each review round, and a generated review sheet
(CSV and printable HTML) supports later rounds.

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

### What candidates see
`catalog.py` (run by `build_all.py`) builds the curated test sets shown on the Practice page and the admin
"RN Question Bank" (all 1,110). It writes them into the last batch file, because the importer refuses a file without
questions and the last file imports after every question it lists:
- 21 topic practice sets: every standalone question, by Client Needs category, in parts of up to 60 (tutor mode, untimed).
- 2 case study sets: the 10 NGN cases, 5 per set, each kept whole and in order.
- 8 NCLEX-RN challenges: 4 exams of 85 items (2 whole cases + 73 standalone items weighted by test-plan
  midpoints; no item appears in two exams), each published as a timed (2 h 50 min) and an untimed challenge.
  Both use exam rules: no going back, answers and a 65% pass estimate at the end.
- **Since F19** these 8 challenges are unpublished (`isPublished: false`, same ids and items, so earlier attempts
  keep their history). One **NCLEX-RN adaptive exam** replaces them: 85 to 150 questions picked from the whole
  bank as the candidate answers, taken timed (5 hours) or untimed as the candidate chooses (architecture.md 5.8,
  docs/features/F19-cat-adaptive-exam.md).
The custom builder (`/practice/custom`) draws from every published question, so it never needed a set.

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
- **Clinical sign-off is still recommended.** The 2026-11-17 approval was a content review without a recorded
  licence. A licensed RN or nurse educator should review the bank; record that round in REVIEW-LOG.md and update
  `qb.REVIEW_STATUS`.
- Some topics are covered by both sessions (for example classic prioritization); a reviewer may retire near-duplicates.
- PA is over-represented; future content should go to PSI, HPM and BCC.
- Few `matrix_mr` and grouping items; add more NGN item types over time.
- No question banks or curated test sets are defined for this content yet.

## 9. Try it yourself
1. Add a `select_n` question to a new batch `b68_*.py`, run `build_all.py`, and import only its file.
2. Change one rationale, rebuild, re-import the file, and confirm the importer reports "1 updated".
3. Extend `review_sheet.py` to add a column with each question's word count, to spot overlong stems.
