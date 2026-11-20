# Content hardening, passes 2 and 3: the rest of the RN bank

## 1. Summary

Pass 1 rewrote 362 questions after a candidate who had failed the NCLEX-RN four times passed the adaptive exam unprepared (see `content-hardening-01.md`). Passes 2 and 3 rewrite the other 748 questions in the same way:
- the body-system files 06–19 (pass 2);
- files 20–34 and the case-study files 36, 37, 38 and 60 (pass 3).

All 46 RN files (1,110 questions) are now marked hardened and pass the build's quality gates. A test-wise candidate who picks the longest option, or uses any other trick the gates measure, now scores about chance. The adaptive exam keeps the stopgap pass line of θ 0.5 until the difficulty is calibrated from real answers.

## 2. How to try it

Same worktree and branch as pass 1: `/Users/tunde/Codes/Nclex-Practice-harden`, branch `content/rn-bank-hardening`, from `main` at 6ccab4a.

```bash
cd nclex-be/content/rn-bank
python3 -I build_all.py      # every file is hardened, so every file must pass the gates
python3 -I lengths.py 27     # per-question key lengths and gate problems for one batch
```

`QUALITY.md` now says "yes" under **Hardened** for all 46 files.

To load the bank into a database that has main's bank:

```bash
docker compose up -d
cd nclex-be
for f in content/rn-bank/rn-bank-*.rn.json; do dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$f"; done
```

Every RN file reports "N updated, 0 created". On the test VM, run `deploy/vm/deploy.sh deploy --seed`.

In the app, try **Practice → Maternal-newborn**, **Pediatrics** or **Mental health**, or a case study such as *Preeclampsia with severe features at 35 weeks*.

## 3. Reading order

1. `content-hardening-01.md`: the gates and the approach. Nothing about them changed, except the item below.
2. `nclex-be/content/rn-bank/quality.py` (`duplicates`): case-study questions are now left out of the near-duplicate stem check (see section 6).
3. `nclex-be/content/rn-bank/batches/b27_maternity_intrapartum.py`: a typical pass-3 batch.
4. `nclex-be/content/rn-bank/batches/b60_ngn_cases_1.py`: a hardened case-study file. The scenario tabs carry the data, and the multiple-choice items have plausible distractors.
5. `nclex-be/content/rn-bank/QUALITY.md` and `REVIEW-LOG.md`: the results and the review record.

## 4. Request flow

As in pass 1. The build runs `quality.gate()` for each hardened file, and `import-questions` publishes a new `question_versions` row for each changed question. Practice sets and the adaptive picker serve the new version.

## 5. Database changes

None in the schema. Trial import into a scratch database (`nclex_hard`, since dropped), loading main's bank first and then this branch:
- **Question versions:** 1,156 → 2,266. That is 1,110 new versions, one for every RN question; the 46 sample questions are unchanged.
- **Questions:** still 1,156, so no new ids.
- **Test sets:** 1 updated (the adaptive exam, `{"minItems": 85, "maxItems": 150, "passingTheta": 0.5}`), 31 unchanged.

## 6. Key decisions and deviations

- **Same ids and order.** Each batch keeps its item count and order, so questions update in place.
- **One item changed type.** Batch 25, question 12 changed from multiple choice to a calculation item (expected weight at 12 months). `item_type` is a column of `question_versions` (database.md), so the change imports as a normal new version.
- **Case-study items skip the duplicate check.** NGN case items use short standard stems ("Complete the sentence.", "Review the 0900 findings…") that depend on their case's scenario. Once files 36–38 were hardened, these stems matched each other across different cases at the 0.85 similarity threshold, although the questions are different. All non-case questions are still checked.
- **Case files were tightened rather than rewritten.** Files 36, 37, 38 and 60 already followed the clinical judgment model with full scenarios. Their multiple-choice items got realistic competing distractors. For example, in preeclampsia the low platelet count is a distractor against seizure and stroke risk, and in a tet spell or burn case two problems both look urgent. Their difficulty mix now meets the limits: no easy items, and 30% or more difficult.
- **Neutral wording.** Clients are "the client" or use they/them, unless the question itself states otherwise (pregnancy, a named "Mr." or "Ms.", or a quoted family member).
- **Clinical sources.** Items were written against current guidance, for example: ACOG on preeclampsia, GBS and fetal monitoring; AAP on jaundice, infant feeding and croup; AHA on infant choking and STEMI; CDC on varicella isolation; and SAMHSA on buprenorphine and naltrexone starts.

## 7. Tests

| Passes 2 and 3 (748 questions) | Before | After |
|---|---|---|
| Key is the longest option (MC) | 72% | 13% |
| Best test-wise trick (MC) | 73% (longest) | 25% (assess first) |
| Only wrong options use absolute words | 16% | 7% |
| Median stem words | 17 | 33 |
| Easy / moderate / difficult | 136 / 489 / 123 | 58 / 435 / 255 |

| Whole bank (1,110 questions) | Before pass 1 | After pass 3 |
|---|---|---|
| Key is the longest option (MC) | 73% | 14% |
| Best test-wise trick (MC) | 75% (longest) | 25% (assess first) |
| Only wrong options use absolute words | 17% | 7% |
| Median stem words | 17 | 33 |
| Easy / moderate / difficult | 171 / 733 / 206 | 89 / 648 / 373 |

Checks run on this branch:
- `python3 -I build_all.py`: passes, with 46 of 46 files hardened.
- `dotnet build`: 0 warnings, 0 errors; `dotnet test`: 1,421 passed.
- `npm run lint` and `npm run typecheck`: clean; `npm test`: 475 passed (76 files).
- The scratch-database import in section 5.

## 8. Follow-ups and known limitations

- **Calibration.** Difficulty is still author-judged (seeds −1/0/+1). Once candidates answer the new versions, estimate `irt_b` from the attempts and revisit the θ 0.5 stopgap.
- **Licensed review.** No RN credential is recorded for any pass. A licensed RN or nurse educator should review the bank before it is marketed.
- **The checks are heuristics.** They catch tells, not clinical errors.
- **Next, the 5,000-question goal.** New waves should be written with `hardened=True` from the start, so they meet the same gates.

## 9. Try it yourself

1. **Watch a gate catch a tell.** In `b30_pediatrics_2.py`, lengthen the key of the coarctation question to about twice its length and run `python3 -I lengths.py 30`. Then undo the change.
2. **Turn the duplicate check back on for cases.** Remove the `caseStudyId` filter in `quality.duplicates` and run `build_all.py`. Look at which case stems collide, and think about how you would word them to be unique.
3. **Change a difficulty mix.** In `b34_mental_health_3.py`, mark three more questions as easy and rebuild. Read the gate message about the easy limit.
