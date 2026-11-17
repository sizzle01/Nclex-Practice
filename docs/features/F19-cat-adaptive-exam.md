# F19: Adaptive (CAT) exam

## 1. Summary

The Practice page now has one **NCLEX-RN adaptive exam** in place of the eight fixed-length challenges. Like the real NCLEX-RN, it gives one question at a time. Each answer changes the server's estimate of the candidate's ability, and the next question is the one that tells it most at that level, keeping the test-plan category mix. After 85 questions the exam stops as soon as the result is clear: the 95% confidence interval is wholly above or below the passing standard. Otherwise it goes on to 150, and the final estimate decides. Before starting, the candidate chooses **timed** (5 hours, submits by itself when time runs out; fewer than 85 answers is then a fail) or **untimed**. The results page leads with Pass or Fail and why the exam ended, not a percentage.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f19`, on `feature/f19-cat-adaptive-exam`, branched from `main` at e82ac9c. There is **no migration**: the `cat` enums and the `cat_config`, `cat_state` and `cat_theta`/`cat_se` columns were already in the F00 schema.

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
for f in content/rn-bank/rn-bank-*.rn.json; do dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$f"; done

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000
```

During development I used the database `nclex_f19` and the API on `:5190`.

**As a candidate** (sign up, then verify the email through Mailpit at http://localhost:8025):

1. Open **Practice** → the **Adaptive** tab. The card reads "Adaptive · 85–150 questions · 5 h or untimed". The old "NCLEX-RN challenge 1–4 (timed/untimed)" cards are gone.
2. **View details** → choose **Timed (5 h)** or **Untimed** → **Start**.
3. The player shows "Question 1" with no total, no question list, no Previous, no Mark for review and no Submit. Pick an answer and press **Next ▶**: the answer is final. **Next** with no answer asks you to choose one.
4. Keep going. A strong or weak run stops at question 85 ("Your exam has ended"); a borderline run goes on toward 150.
5. The results page shows **Pass** or **Fail**, the questions answered, why it ended, and the usual breakdown tables and review.
6. Start again while a draft is open: it resumes the draft with the timing you chose first. The dashboard shows the draft as "Adaptive, timed: 12 answered".

**Through the API** (token from `POST /api/v1/auth/login`):

```bash
# the adaptive exam's id
curl -s -H "Authorization: Bearer $TOKEN" "http://localhost:5080/api/v1/test-sets?kind=cat"
# start untimed, then open it with a client id
curl -s -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"testSetId":"<set id>","timed":false}' http://localhost:5080/api/v1/attempts
curl -s -X POST -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-try" \
  http://localhost:5080/api/v1/attempts/<attempt id>/open
# answer question 1 (shape depends on the item type; multiple choice shown)
curl -s -X POST -H "Authorization: Bearer $TOKEN" -H "X-Attempt-Client: c-try" -H "Content-Type: application/json" \
  -d '{"response":{"selected":"b"},"clientSeq":1}' http://localhost:5080/api/v1/attempts/<attempt id>/items/1/answer
```

What I saw against the imported RN bank (simulated candidates answering a fixed share of questions correctly; correct answers built from the stored keys):

| Candidate | Answered | Result | Why it ended | θ ± SE | Share of points |
|---|---|---|---|---|---|
| right 92% of the time, untimed | 85 | Pass | clear result | 3.00 ± 0.33 | 95% |
| right 20%, timed | 85 | Fail | clear result | −2.61 ± 0.29 | 14% |
| right 62%, untimed | 85 | Pass | clear result | 0.88 ± 0.22 | 48% |
| right 50%, untimed (3 runs) | 150 | Pass | maximum | 0.03 to 0.23 ± 0.16 | 55% to 57% |

No answer key reached the candidate before the end in any run.

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Cat/RaschModel.cs`: probability, information, and the tier seeds −1/0/+1.
2. `nclex-be/src/Nclex.Domain/Cat/AbilityEstimator.cs`: the EAP estimate of θ and its standard error.
3. `nclex-be/src/Nclex.Domain/Cat/ItemSelector.cs`: test-plan balancing, then the top 5 most informative, picking one at random.
4. `nclex-be/src/Nclex.Domain/Cat/StoppingRule.cs`: when to stop, and the single pass rule.
5. `nclex-be/src/Nclex.Domain/Cat/CatState.cs`: what an attempt remembers (`attempts.cat_state`) and its JSON.
6. `nclex-be/src/Nclex.Domain/Entities/Attempt.cs`: `StartAdaptive`, `LockAdaptiveAnswer`, `GiveNextAdaptiveItem`.
7. `nclex-be/src/Nclex.Application/Attempts/Adaptive/AdaptivePicker.cs`: loads the pool and the test plan.
8. `nclex-be/src/Nclex.Application/Attempts/StartAttempt/StartAttemptHandler.cs`: the timed choice and the first question.
9. `nclex-be/src/Nclex.Application/Attempts/Adaptive/AnswerAdaptiveItemHandler.cs`: one answer, end to end.
10. `nclex-be/src/Nclex.Application/Attempts/Grading/GradingService.cs` (`ScoreResult`): pass or fail from θ.
11. `nclex-be/src/Nclex.Application/Attempts/Results/AttemptResultView.cs` (`AdaptiveResult`): what the results page gets.
12. `nclex-be/src/Nclex.Application/Content/ImportQuestions/CatalogImporter.cs`: `mode: "cat"` sets and `isPublished`.
13. `nclex-be/content/rn-bank/catalog.py`: the adaptive exam and the unpublished challenges.
14. `nclex-fe/src/features/practice/components/TestSetDetailPanel.tsx`: the Timed / Untimed choice.
15. `nclex-fe/src/features/attempt/player/useAttemptSession.ts` (`answerAdaptive`) and `AttemptPlayer.tsx`: the adaptive player.
16. `nclex-fe/src/features/results/ResultsPage.tsx` (`AdaptiveSummary`): Pass / Fail and why.

## 4. Request flow: pressing Next ▶ on question 40

1. **UI:** `AttemptPlayer` calls `session.answerAdaptive()`. The answer goes into the save queue, which gives it the next `clientSeq` and flushes it as an autosave (`PUT …/items/40/response`). Then the same answer and `clientSeq` go to `POST /api/v1/attempts/{id}/items/40/answer`. Because the seq is the same, the stored answer and the final one can never differ.
2. **API:** `AttemptsController.AnswerAdaptiveItem` passes the request to `AnswerAdaptiveItemHandler`.
3. **Application:** in one transaction, `AttemptWriteGuard.LockAndApplyTimersAsync` takes `SELECT … FOR UPDATE` on the attempt. If the 5 hours ran out, it finalizes the attempt as `time_expired` and returns `409 ATTEMPT_TIME_EXPIRED`. The handler then:
   - checks the attempt is adaptive and writable;
   - treats position 39 as a retry (returns the current state) and any position other than 40 as not found;
   - saves the response and calls `Attempt.LockAdaptiveAnswer`;
   - grades the item at once with `IGradingService.GradeItemsAsync`.
4. **Domain:** `CatState.RecordAnswer(points / maxPoints)` re-estimates θ from all 40 answers (`AbilityEstimator`). `StoppingRule.AfterAnswer` returns null: 40 is below the minimum of 85.
5. **Application → database:** `AdaptivePicker.NextAsync` loads every published standalone RN question not yet given, plus the Client Needs leaves and their weights. `ItemSelector.Choose` takes the leaf furthest behind its share, then one of the 5 most informative questions at θ. `Attempt.GiveNextAdaptiveItem` appends `attempt_items` row 41, starts its time count, bumps `total_items` and `current_position`, and stores the new `cat_state`. The transaction commits.
6. **Back:** the response is `{ ended: false, attempt }`. The player stores the new state, refetches item 40 (now locked) and loads item 41. Focus moves to its text.
7. **At question 85 or later**, when the interval clears the standard, `StoppingRule` returns `ClearResult`. The handler stores it in `cat_state` and calls `AttemptFinalizer`, which locks, grades and writes `attempt_results` with `passed`, `cat_theta` and `cat_se`. The response is `{ ended: true }`, and the player routes to the results page.

## 5. Database changes

None. `dotnet ef migrations has-pending-model-changes` reports no changes. F19 starts using what the F00 schema already had:
- `test_sets.kind = 'cat'`, `mode = 'cat'`, `cat_config = {"minItems":85,"maxItems":150,"passingTheta":0}`, no `test_set_items`, `question_count` = the maximum.
- `attempts.cat_state` (shape in database.md), with `attempt_items` appended one at a time.
- `attempts.settings` (the snapshot) gets `timed:false` and no budget when the candidate picks untimed.
- `attempt_results.cat_theta`, `cat_se`, `passed`, `readiness_label`; `pass_threshold` is null.
- `submit_reason = 'cat_stop'` when the exam ends on its own.

The only content change is in the RN bank's last import file: the 8 challenge sets get `"isPublished": false`, and one adaptive set is added.

## 6. Key decisions and deviations

- **85, not 80, as the minimum** (your choice of my recommendation). It matches the NCLEX-RN since April 2023, and it is one value in `cat_config`.
- **One adaptive set with a Timed / Untimed choice at Start**, instead of two sets (your request). The choice is stored in the attempt's settings snapshot. A draft keeps the timing it started with; the detail panel says so.
- **The 8 fixed challenges are unpublished, not deleted**, so attempts already taken keep their set, results and history. "Comprehensive Challenge 1" from the older `sample-questions.rn.json` (F06) is still published: it isn't one of the 8 RN-bank challenges. Say if it should go too.
- **EAP rather than maximum likelihood** for θ: MLE has no answer while every answer is right (or wrong), which is how every exam starts. The design said "Rasch"; it didn't say which estimator.
- **One pass rule for every ending:** below the minimum is a fail, otherwise θ ≥ the standard. The design lists three ways to stop, and this rule agrees with the interval when the stop is a clear result.
- **Partial credit counts as a fraction** in the estimate. The design didn't say; all-or-nothing would make NGN items much harsher than the NCLEX's own polytomous scoring.
- **Case-study items are left out of the pool** for now (the plan's recommendation). Real NCLEX exams include 3 case studies.
- **Passing standard 0 on uncalibrated difficulties:** with −1/0/+1 seeds, "pass" means answering moderate questions reliably. The results page says so in a disclaimer ("not calibrated on real candidates").
- **No percentage on the adaptive results** (score and breakdown are still stored and shown in the tables): adaptive questions pull everyone toward about 50% correct.
- **A new import field, `isPublished`**, so the content file can hide sets. The importer also accepts `mode: "cat"` with `catConfig`, and checks that it lists no questions, uses exam reveal with no going back, and has no sample flag, item timer or pass threshold.
- **`GET /test-sets` with no `kind`** now returns prebuilt sets and adaptive exams; the Practice page uses that. `kind=prebuilt` still returns only prebuilt sets, and `kind=cat` returns only adaptive exams.
- **New error codes** `ADAPTIVE_NOT_ALLOWED` (Navigate and Submit on an adaptive exam) and `ADAPTIVE_ONLY` (`/answer` on another attempt), added to api.md section 1.
- **`AdaptivePicker` has one virtual method** (`PoolAsync`), only so integration tests on the shared database can keep an exam to their own questions. The server always uses the real pool.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test      # 1,391 passed
cd nclex-fe && npm run lint && npm run typecheck && npm test   # lint and typecheck clean; 461 passed
```

- **Domain** (`tests/Nclex.Domain.Tests/Cat`, `Entities/AdaptiveAttemptTests.cs`):
  - The estimator: prior, symmetry, finite with all answers right, SE shrinking, partial credit.
  - The stopping rule at, below and past the minimum and at the maximum, plus time-out before the minimum.
  - Selection: content balancing, closest difficulty, randomesque spread, same seed giving the same pick.
  - State JSON round trip and the stored config keys.
  - Three simulated whole exams, at true θ of +1.5, −1.5 and 0, stopping at 85 / 85 / 150.
  - The attempt entity: timed and untimed starts, answering, the next item waiting for the answer, finalizing.
- **API integration** (`tests/Nclex.Api.IntegrationTests/Attempts/AdaptiveExamTests.cs`, using sets of 3 to 6 questions):
  - catalog fields; timed and untimed starts; resuming a draft keeps its timing; `timed` refused on other sets;
  - answering locks and serves a new question with no key revealed; retries; a required answer;
  - Navigate and Submit refused; `/answer` refused on other attempts;
  - all right gives a pass, all wrong gives a fail; a clear result at the minimum; an uncertain run to the maximum;
  - time-out before the minimum is a fail; review after the end;
  - importer: `isPublished:false` hides a set, and bad adaptive sets are refused.
- **Two older tests updated:** the list validator now allows `kind=cat`, and an import test now uses a `catConfig` on a practice set as its invalid example.
- **Frontend** (Vitest):
  - the adaptive player (`AttemptPlayerAdaptive.test.tsx`): no navigator, Next sends the answer with the autosave's `clientSeq`, an answer is required, and the end goes to results;
  - the detail panel's choice and the `timed` it sends;
  - the length and timing labels;
  - the adaptive results panel, including a time-out fail;
  - the adaptive draft row;
  - the catalog query with no kind.
- **Not checked in a browser.** The UI is covered by the Vitest tests above, and the API by the real-bank runs in section 2.

## 8. Follow-ups and known limitations

- **Case studies in the adaptive exam.** The real NCLEX-RN includes 3 (18 items), and they need selection by whole case.
- **Calibration.** Difficulties are author tiers. Once enough answers exist, estimate `irt_b` per question and revisit the passing standard.
- **Exposure across attempts:** prefer questions the candidate hasn't seen in earlier adaptive attempts. Right now each attempt only avoids repeats within itself.
- **The admin console** lists and edits only prebuilt sets, so the adaptive set is managed through the content file.
- **Breaks:** the set has no pause limits, and a pause stops the clock, unlike the NCLEX, where optional breaks count against the 5 hours. Decide whether the timed version should follow the NCLEX here.
- **History** shows a dash for an adaptive exam's percentage; the outcome column carries Pass or Fail.
- **Remove the git stash** `f19 frontend with accidental prettier formatting (backup)`, a backup made during development, once you are happy with this branch.

## 9. Try it yourself

1. **Shorter practice exam:** add a second adaptive set in `catalog.py` with `catConfig` `{minItems: 30, maxItems: 60}` and a 90-minute budget. Rebuild with `python3 -I build_all.py`, re-import the last file, and take it. Which tests describe what you should see?
2. **A stricter standard:** change `passingTheta` to `0.5` for the RN exam and rerun the domain simulation in `CatStateTests.A_simulated_exam_stops_where_the_rules_say` with a true θ of 0.4. Does it still pass? Why does it now usually run to 150?
3. **Show progress to the minimum:** in `AttemptPlayer.tsx`, use `session.adaptive.minItems` to show "at least 85 questions" next to "Question n", and write a Vitest test for it. Keep θ hidden, as the NCLEX does.
