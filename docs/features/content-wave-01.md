# Content wave 1: growing the RN bank toward 5,000 questions (chunks 1 to 3)

## 1. Summary

The RN bank grows from 1,110 to 5,000 questions in chunks of 100. Each chunk is four batch files of 25, and five chunks make a wave. Each chunk follows the gap left in every test plan category, so the bank moves closer to the NCLEX-RN test plan with every chunk. Chunks 1 to 3 add 300 questions (b68 to b79), taking the bank to 1,410, and they were merged before the end of wave 1 at the user's request. All new questions are written to the same hardened standard as the existing bank. They include the bank's first 18 bowtie questions and 6 new case studies.

## 2. How to try it

Worktree `/Users/tunde/Codes/Nclex-Practice-wave01`, branch `content/rn-bank-wave-01`, from `main` at 7342005.

```bash
cd nclex-be/content/rn-bank
python3 -I build_all.py      # builds every file, checks quality, writes QUALITY.md and PROGRESS.md
python3 -I lengths.py 75     # option lengths and gate problems for one batch
```

`PROGRESS.md` shows the counts against target and the next chunk's numbers per category.

To load the new questions into a database that already has main's bank:

```bash
cd nclex-be
for n in 68 69 70 71 72 73 74 75 76 77 78 79; do
  dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$(ls content/rn-bank/rn-bank-$n-*.rn.json)"
done
```

Every new file reports "25 created", and the last file (b79) also updates the catalog. In the app, open **Practice**. Each category now has a new last part (for example *Management of Care, part 4 of 4*), and *NGN case studies, set 3* and *set 4* hold the new cases.

## 3. Reading order

1. `nclex-be/content/rn-bank/coverage.py`: targets per category, the next chunk's counts and the `CHUNKS` list. It writes `PROGRESS.md`.
2. `nclex-be/content/rn-bank/qb.py`: `bowtie()`, `WAVE_STATUS` and `build(..., wave=True)`, plus the new empty-segment check in `_check`.
3. `nclex-be/content/rn-bank/catalog.py` and `catalog-frozen.json`: how published sets keep their ids while the bank grows.
4. `nclex-be/content/rn-bank/quality.py`: the bowtie additions (wing length check and scenario word count).
5. `nclex-be/content/rn-bank/batches/b71_risk_reduction_adaptation_cases.py`: a typical chunk file with bowtie questions and two case studies.

## 4. Request flow

`build_all.py` runs every batch file. Each wave batch calls `build(..., wave=True)`, which checks the importer's rules and the quality gates, then writes `rn-bank-NN-*.rn.json`. `catalog.attach()` puts the test sets into the last file (now b79), and `coverage.write()` writes `PROGRESS.md`. The worker's `import-questions` command creates each new question with its first version and updates the test sets by id. Practice sets and the adaptive exam then serve the new questions.

## 5. Database changes

No schema change. Trial import into a scratch database (main's bank, then this branch):
- **Questions:** 1,156 to 1,456 (300 created, 0 existing questions changed).
- **Case studies:** 11 to 17.
- **Test sets:** 10 created (one new topic part per category, plus two new case-study sets), 23 updated (new "part X of Y" titles), 9 unchanged (the frozen challenges and the adaptive exam).

## 6. Key decisions and deviations

- **Published test sets are frozen.** `catalog.py` used to number sets in order and re-split each category evenly. Adding questions would have changed set ids, including the adaptive exam's, and broken attempt history. The 32 sets published before growth are now kept in `catalog-frozen.json` with their ids and question lists. New questions fill new parts of up to 60 in file order, with set numbers 1000 + category × 100 + part, and new case sets use 2000 + n. A newly added part can be small (a few questions) until later chunks fill it.
- **New review status line.** Wave questions show `WAVE_STATUS` ("Written to NCLEX-RN difficulty...") so the existing questions keep their own line and do not get new versions.
- **Merged mid-wave.** The plan was to merge after five chunks. The user asked to merge after chunk 3, so the full checks and this walkthrough were done now; they will be updated at the end of wave 1.
- **Empty highlight segments are rejected by the build.** One question in b72 ended with an empty segment, which the importer rejects; the build now catches it.
- **Item mix.** New chunks use about 55% multiple choice. Chunks 1 to 3 have 197 multiple choice, 25 SATA, 18 matrix, 18 bowtie, 15 highlight, 12 drop-down, 7 ordering, 4 calculations and 4 other. Multiple choice across the whole bank fell from 79% to 76%.

## 7. Tests

| Chunks 1 to 3 (300 questions) | |
|---|---|
| By category | MOC 58, SIC 42, HPM 27, PSI 30, BCC 30, PPT 48, RRP 39, PA 26 |
| Easy / moderate / difficult | 18 / 162 / 120 |
| Key is the longest option (MC, per file) | 0% to 24% |
| Whole bank after chunk 3 | key longest 13%, best trick 24% (assess first) |

Checks run on this branch:
- `python3 -I build_all.py`: passes; 58 hardened files.
- Trial import into a scratch database: 300 created, 0 problems (section 5).
- `dotnet build`: 0 warnings, 0 errors; `dotnet test`: passed.
- `npm run lint`, `npm run typecheck`: clean; `npm test`: 475 passed (76 files). The frontend is unchanged by this branch.

## 8. Follow-ups and known limitations

- **Chunks 4 and 5 finish wave 1.** Then update this walkthrough with the wave totals.
- **Calibration.** Difficulty is author-judged until real answers allow `irt_b` estimates; the adaptive pass line stays at θ 0.5.
- **Licensed review.** No RN credential is recorded for these questions.
- **Small new practice parts.** Some new parts hold only a few questions until later chunks fill them.
- **Duplicate check speed.** `quality.duplicates()` compares every pair of stems; at 5,000 questions it will take longer and may need an index.

## 9. Try it yourself

1. **See the next chunk's plan.** Change one category's count in a new batch, rebuild, and read how `PROGRESS.md`'s "Next chunk" column shifts.
2. **Break a bowtie.** In `b70`, mark a third action as correct and run `python3 -I lengths.py 70`. Read the assertion from `qb.bowtie()`, then undo it.
3. **Check frozen ids.** Compare the first 32 set ids in `rn-bank-79-*.rn.json` with `catalog-frozen.json`, then add a dummy batch and confirm they stay the same.
