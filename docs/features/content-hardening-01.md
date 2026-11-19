# Content hardening, pass 1: rewrite the RN bank to NCLEX difficulty

## 1. Summary

A candidate who has failed the NCLEX-RN four times passed this app's adaptive exam after about 85 questions, with no preparation. The bank was too easy to answer by test-taking tricks. In 77% of the multiple-choice questions in these files the right answer was the longest option, so "pick the longest" scored 78% with no nursing knowledge. Pass 1 rewrites 362 questions (the Management of Care, Safety and Pharmacology files plus file 66) to real NCLEX-RN difficulty:
- clinical stems with vital signs, labs and history;
- every wrong option plausible;
- no length or wording tells;
- honest difficulty tiers.

The build now measures these tells for every file and refuses to build a rewritten file that fails. Until all three passes are done, the adaptive exam's pass line is raised from θ 0 to θ 0.5, and its description says results are a practice estimate.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-harden`, on `content/rn-bank-hardening`, branched from `main` at 6ccab4a.

```bash
cd nclex-be/content/rn-bank
python3 -I build_all.py          # builds every file, checks the gates, writes QUALITY.md
python3 -I lengths.py 02         # while rewriting: per-question key lengths and gate problems for one batch
```

Open `nclex-be/content/rn-bank/QUALITY.md` to see every file's numbers. The 13 rewritten files say "yes" under **Hardened**.

To load the rewritten bank into a database that already has the old one:

```bash
docker compose up -d
cd nclex-be
for f in content/rn-bank/rn-bank-*.rn.json; do dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$f"; done
```

Rewritten files report "25 updated" (or similar). Untouched files report "0 updated", and the catalog file reports "1 updated" (the adaptive exam). On the test VM, use `deploy/vm/deploy.sh deploy --seed`.

In the app, start **Practice → Management of Care** (or Safety, or Pharmacology) and compare with the old questions. Each rationale ends with "Rewritten to NCLEX-RN difficulty and reviewed by the content team on 19 November 2026."

## 3. Reading order

1. `nclex-be/content/rn-bank/quality.py`: the checks.
   - **Tricks:** longest option, shortest option, repeating the stem's words, avoiding absolute words, and picking "assess".
   - **Per-question rules:** how much longer the key is than the wrong options, and how many words of clinical picture the question gives.
   - **Per-file limits** and the near-duplicate check.
2. `nclex-be/content/rn-bank/qb.py` (`build`, `HARDENED_STATUS`): `build(..., hardened=True)` runs the gates and stamps the new review status.
3. `nclex-be/content/rn-bank/build_all.py`: writes `QUALITY.md` and stops on near-duplicate stems.
4. `nclex-be/content/rn-bank/lengths.py`: the helper used while rewriting a batch.
5. `nclex-be/content/rn-bank/batches/b02_management_of_care_priorities.py`: a rewritten batch to read as the standard.
6. `nclex-be/content/rn-bank/catalog.py` (`ADAPTIVE_CONFIG` and the adaptive set's description): the pass-line stopgap.
7. `nclex-be/content/rn-bank/QUALITY.md` and `REVIEW-LOG.md`: the results and the review record.

## 4. Request flow

Content changes don't add a request path. They change what an existing one returns:

1. **Build.** `python3 -I build_all.py` runs each batch.
   - `build(..., hardened=True)` calls `quality.gate()`.
   - The file is written only if every rule passes.
   - Question ids come from each item's position (`_qid`), so a rewrite that keeps the order keeps the ids.
2. **Import.** `import-questions` finds each id already in `questions`. Because the content, key or rationale changed, it publishes a new `question_versions` row (F14 versioning). History and earlier attempts keep pointing at the old version.
3. **Practice and the adaptive exam.** Practice sets list question ids, so they show the new version automatically.
   - The adaptive picker (F19) reads each question's current published version and its difficulty tier. A stronger tier mix (more "difficult" with a +1 seed) now matches what the questions really ask.
   - The catalog sets `passingTheta` to 0.5. `CatalogImporter` applies it to the existing adaptive set through `ApplyAdaptive`, so new attempts use the higher line.

## 5. Database changes

None in the schema. Importing the branch's files into a copy of main's database (scratch database `nclex_hard`, since dropped):
- **Question versions:** 362 new (1,156 → 1,518), one for each rewritten question.
- **Questions:** still 1,156, so no new ids.
- **Test sets:** 1 updated (the adaptive exam, now `{"minItems": 85, "maxItems": 150, "passingTheta": 0.5}` with the new description), 31 unchanged.

## 6. Key decisions and deviations

- **Same ids, same order, same item types.** Each batch keeps its item count and order, so every question updates in place. Changing an item's type was avoided to keep the import a simple new version.
- **Gates apply only to rewritten files.** The other 748 questions are reported in `QUALITY.md` but don't fail the build until passes 2 and 3 rewrite them.
- **The checks are heuristics.** They catch tells, not clinical errors. Every rewritten item was also written against current guidance: CDC precautions, START triage, EMTALA, The Joint Commission's "Do Not Use" list, ADA (DKA), stroke thrombolysis criteria, and transfusion and medication safety.
- **Letter position is not checked.** The player shuffles options for each attempt (`SeededShuffle`), so the key's position in the file tells a candidate nothing.
- **Stem length counts the options too.** In "which client first" questions the clinical data is in the options, as on the real exam. Moderate items need 45 words of stem plus options, difficult items 60, unless an exhibit or case study carries the data.
- **Difficulty mix of the 362:** 31 easy, 213 moderate, 118 difficult, against 35/244/83 before. Each rewritten file has at most 15% easy and at least 30% difficult.
- **Pass-line stopgap (architecture.md 5.8 updated).** θ 0.5 instead of 0 is a judgment call, not a calibration. It is meant to be revisited after pass 3, or when there is real response data to calibrate `irt_b`.
- **Two option wordings were adjusted only to avoid tells** (for example, "every" or "only" in wrong options). Their clinical meaning didn't change.

Before and after, one example (batch 02, question 3):

- **Before:** *"The charge nurse is reviewing updates on four clients. Which client needs the nurse's attention first?"* The key was a head-injured client with a new unequal pupil. The three wrong options were plainly routine.
- **After:** the key is a client 2 hours after a femoral cardiac catheterization with new low back pain and a heart rate of 112/min (retroperitoneal bleeding). The wrong options are a head-injured client whose GCS of 14 is unchanged from admission, intermittent water-seal bubbling with coughing, and a vancomycin trough not yet drawn. Every option sounds worrying, and only the cues separate them.

## 7. Tests

| Pass 1 (362 questions) | Before | After |
|---|---|---|
| Key is the longest option (MC) | 77% | 17% |
| Best test-wise trick (MC) | 78% (longest) | 24% (assess first) |
| Only wrong options use absolute words | 17% | 9% |
| Median stem words | 19 | 31.5 |
| Easy / moderate / difficult | 35 / 244 / 83 | 31 / 213 / 118 |

Whole bank (1,110): key longest 73% → 55%, best trick 75% → 57%. Passes 2 and 3 cover the rest.

Checks run on this branch:
- `python3 -I build_all.py`: passes, with all 13 hardened files within the limits.
- `dotnet build`: 0 errors; `dotnet test`: 1,421 passed.
- `npm run lint` and `npm run typecheck`: clean; `npm test`: 475 passed (76 files).
- The scratch-database import in section 5.

## 8. Follow-ups and known limitations

- **Passes 2 and 3** rewrite the body-system files 06–19 (about 340 questions), then 20–34 with the case-study files 36–38 and 60 (about 410). Until then, the adaptive exam still draws many easy items from those files.
- **Calibration.** Difficulty is still author-judged (seeds −1/0/+1). Real calibration of `irt_b` needs answer data; a later feature can estimate it from attempts.
- **Licensed review.** As in the first round, no RN credential is recorded. A licensed RN or nurse educator should review the bank before it is marketed.
- **A slip in the commit history.** `1d4cbd7` committed batch 64's source while the build was failing, so its JSON wasn't rebuilt in that commit. `c56e141` finished the batch and rebuilt the file. Each commit after that was made only after a passing build.

## 9. Try it yourself

1. **Break a gate.** In `b02_…py`, make the key of the first question twice as long as the other options, and run `python3 -I lengths.py 02`. Read the error, then undo the change.
2. **Add a trick.** In `quality.tricks()`, add "pick the option that mentions the provider" (a common test-taking habit). Run `build_all.py` and see which files score highest on it in `QUALITY.md`.
3. **Move the pass line.** Change `passingTheta` in `catalog.py` to 0.3, rebuild, and import only file 67. The importer should report "1 updated" test set, and nothing else should change.
