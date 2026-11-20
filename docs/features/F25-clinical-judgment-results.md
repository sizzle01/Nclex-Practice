# F25: Clinical judgment results

## 1. Summary
Every results page now has a "Clinical judgment" panel. It shows:
- each step of the NCSBN Clinical Judgment Measurement Model in model order, with its score and its standard
  (below, near or above passing, using F23's 70% and 86% marks);
- the first half of the test against the second half, with time per question;
- each step's score in each half;
- a note when the second half clearly drops.

The Performance page gains a grid of each step across the last five timed tests, laid out like the candidate's
NCLEX performance report. The candidate's reports showed recognize and analyze cues holding while the later steps
fell below standard on attempts 4 and 5, a pattern that fits fatigue, pacing or anxiety. This feature makes that
pattern visible in practice.

## 2. How to try it
```bash
cd nclex-be && dotnet build && dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --project src/Nclex.Api        # http://localhost:5080
cd ../nclex-fe && npm run dev             # http://localhost:3000
```
Import the RN bank as in the F24 walkthrough, section 2. Then, as a verified candidate:
1. Take any set whose questions carry CJMM steps, for example a timed case-study block, and submit.
2. The results page shows "Clinical judgment" under the score. With at least 10 questions in each half and a
   second half 10 or more points lower, it also shows the drop note.
3. Open `/performance`. "Clinical judgment in timed tests" lists the six steps across your last timed tests.

Sample requests:
```bash
curl -H "Authorization: Bearer $TOKEN" http://localhost:5080/api/v1/attempts/$ATTEMPT/result   # see clinicalJudgment
curl -H "Authorization: Bearer $TOKEN" http://localhost:5080/api/v1/me/performance/clinical-judgment
```

## 3. Reading order
1. `nclex-be/src/Nclex.Domain/Grading/ClinicalJudgment.cs`: the pure calculation. It covers step order, the half
   split, points-based scores, average times and the clear-drop rule.
2. `nclex-be/src/Nclex.Application/Attempts/Results/ClinicalJudgmentView.cs`: the API shape, with step names.
3. `nclex-be/src/Nclex.Application/Attempts/Results/GetResultHandler.cs`: `ServedItemsAsync` reads the stored
   item grades and times, then calls the domain.
4. `nclex-be/src/Nclex.Application/Performance/GetClinicalJudgmentTrend/GetClinicalJudgmentTrendHandler.cs`: the
   last five timed attempts and their stored step scores.
5. `nclex-be/src/Nclex.Api/Controllers/PerformanceController.cs`: the `clinical-judgment` action.
6. `nclex-fe/src/features/results/ClinicalJudgmentPanel.tsx`: the results panel and the standard wording.
7. `nclex-fe/src/features/performance/components/ClinicalJudgmentTrendTable.tsx`: the Performance grid.

## 4. Request flow
1. **Results page.** `/attempts/[id]/results` calls `GET /api/v1/attempts/{id}/result`.
2. **API to Application.** `GetResultHandler` loads the result and category scores as before. It then reads the
   attempt's items (position, points earned and possible, time spent), joined to each question version's CJMM
   step.
3. **Domain.** `ClinicalJudgment.For` orders the items by position and splits them; the first half takes the odd
   item. It scores each half and each step by points, so partial credit counts. It sets `SecondHalfDrop` when both
   halves have at least 10 items and the second is at least 10 shown points lower.
4. **Back to the page.** `ClinicalJudgmentView` adds the step names, and the response carries `clinicalJudgment`.
   `ClinicalJudgmentPanel` renders the halves, the drop note and the step table. Each score shows in band colour
   with its band named for screen readers, plus the standard in words.
5. **Performance grid.** `GET /api/v1/me/performance/clinical-judgment` reads the user's last five submitted
   attempts whose settings were timed. It takes their stored `cjmm_skill` category scores and returns six rows,
   with null where an attempt had no item on a step.

## 5. Database changes
None. Halves are calculated when the result is read, from `attempt_items` (stored grades and times) and the
question versions' `cjmm_skill`. The trend reads the existing `attempt_category_scores`.

## 6. Key decisions and deviations
- **Branch base.** The branch is stacked on `feature/f24-timed-case-blocks`, which already contains main with F23,
  so the API types regenerate once and F24's blocks feed the trend. Merge order: wave 2, case conversion, F24,
  F25.
- **Standard wording.** "Below / Near / Above passing standard" follows the candidate's NCLEX report, on F23's
  bands: below 70%, 70% to 85%, 86% and above.
- **Drop rule.** At least 10 items a half and at least 10 shown points lower. Fewer items make the difference
  mostly chance.
- **Timed attempts only in the trend,** because the pattern being tracked appears under time pressure. Untimed
  practice still shows the panel on its own results page.
- **Calculated on read, not stored.** The results page is read rarely, the data is small, and this needs no
  migration.

## 7. Tests
- Domain, `ClinicalJudgmentTests` (11 tests):
  - step order;
  - partial credit;
  - the half split;
  - items without a step;
  - a one-item attempt;
  - the drop rule table.
- Integration, `ClinicalJudgmentResultTests`: the result's steps, halves and drop flag for a real submitted
  attempt.
- Integration, `ClinicalJudgmentTrendTests`:
  - untimed attempts are left out;
  - oldest first;
  - only the last five;
  - six empty rows for a new user.
- Frontend:
  - `ClinicalJudgmentPanel.test.tsx`: step order, standard words, half scores and the "No questions" dash, the
    drop note and times;
  - `ResultsPage.test.tsx`: the panel appears;
  - `PerformancePage.test.tsx`: the grid and its empty state.

To run them:
```bash
cd nclex-be && caffeinate -i dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```
Results at commit time: 1,448 backend tests passed; lint and typecheck clean; 503 frontend tests passed.

## 8. Follow-ups and known limitations
- The trend counts all timed tests together. Separate rows for case-study blocks and the adaptive exam could help
  once there is more history.
- Quarters instead of halves would show where in a long exam the drop starts.
- Very fast answers late in a test (rushing) are visible in the times but not flagged. A pace note could follow
  the same pattern as the drop note.
- The dashboard does not yet show the clinical judgment summary.

## 9. Try it yourself
1. Change `DropPercentagePoints` to 5 in `ClinicalJudgment.cs`. Which rows of the `Drops` test table change their
   expected answer?
2. Add a third "half": split into thirds and return a `MiddleThird`. What has to change in the view, the API types
   and the panel?
3. Make the Performance grid show 10 attempts. Where is the number, and what does the caption say afterwards?
