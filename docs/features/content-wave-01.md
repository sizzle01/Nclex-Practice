# Content wave 1: growing the RN bank toward 5,000 questions

## 1. Summary

The RN bank grows from 1,110 to 5,000 questions in chunks of 100. Each chunk is four batch files of 25, and five chunks make a wave. Each chunk follows the gap left in every test plan category, so the bank moves closer to the NCLEX-RN test plan with every chunk. Wave 1 adds 500 questions in five chunks (b68 to b87), taking the bank from 1,110 to 1,610. Chunks 1 to 3 were merged early at the user's request; chunks 4 and 5 complete the wave. All new questions are written to the same hardened standard as the existing bank and include the bank's first 30 bowtie questions and 10 new case studies. Chunk 5 deliberately raises depth: 59% of its questions are rated difficult, and most ask which of several reasonable actions comes first, using combined clinical data.

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
for n in $(seq 68 87); do
  dotnet run --project src/Nclex.Worker -- import-questions "$PWD/$(ls content/rn-bank/rn-bank-$n-*.rn.json)"
done
```

Every new file reports "25 created", and the last file (b87) also updates the catalog. In the app, open **Practice**. Each category now has a new last part (for example *Management of Care, part 4 of 4*), and *NGN case studies, set 3* and *set 4* hold the new cases.

## 3. Reading order

1. `nclex-be/content/rn-bank/coverage.py`: targets per category, the next chunk's counts and the `CHUNKS` list. It writes `PROGRESS.md`.
2. `nclex-be/content/rn-bank/qb.py`: `bowtie()`, `WAVE_STATUS` and `build(..., wave=True)`, plus the new empty-segment check in `_check`.
3. `nclex-be/content/rn-bank/catalog.py` and `catalog-frozen.json`: how published sets keep their ids while the bank grows.
4. `nclex-be/content/rn-bank/quality.py`: the bowtie additions (wing length check and scenario word count).
5. `nclex-be/content/rn-bank/batches/b71_risk_reduction_adaptation_cases.py`: a typical chunk file with bowtie questions and two case studies.

## 4. Request flow

`build_all.py` runs every batch file. Each wave batch calls `build(..., wave=True)`, which checks the importer's rules and the quality gates, then writes `rn-bank-NN-*.rn.json`. `catalog.attach()` puts the test sets into the last file (now b87), and `coverage.write()` writes `PROGRESS.md`. The worker's `import-questions` command creates each new question with its first version and updates the test sets by id. Practice sets and the adaptive exam then serve the new questions.

## 5. Database changes

No schema change. Trial import into a scratch database (main's bank, then this branch):
- **Chunks 1 to 3 (merged 661332e):** questions 1,156 to 1,456; case studies 11 to 17; 10 test sets created, 23 retitled, 9 unchanged (the frozen challenges and the adaptive exam).
- **Chunks 4 and 5 on top of main:** questions 1,456 to 1,656; case studies 17 to 21; 3 test sets created, 17 updated, 25 unchanged. No existing question changed.

## 6. Key decisions and deviations

- **Published test sets are frozen.** `catalog.py` used to number sets in order and re-split each category evenly. Adding questions would have changed set ids, including the adaptive exam's, and broken attempt history. The 32 sets published before growth are now kept in `catalog-frozen.json` with their ids and question lists. New questions fill new parts of up to 60 in file order, with set numbers 1000 + category × 100 + part, and new case sets use 2000 + n. A newly added part can be small (a few questions) until later chunks fill it.
- **New review status line.** Wave questions show `WAVE_STATUS` ("Written to NCLEX-RN difficulty...") so the existing questions keep their own line and do not get new versions.
- **Merged in two steps.** Chunks 1 to 3 were merged early (main 661332e) at the user's request and loaded on the test VM; chunks 4 and 5 follow at the end of the wave.
- **Deeper questions in chunk 5.** At the user's request, chunk 5 targets in-depth knowledge: 59 of 100 questions are difficult (against 40 in earlier chunks), with at most one easy question per file, and most stems combine several data points and ask for the first of several reasonable actions.
- **Empty highlight segments are rejected by the build.** One question in b72 ended with an empty segment, which the importer rejects; the build now catches it.
- **Item mix.** Wave 1 has 333 multiple choice, 41 SATA, 32 matrix, 30 bowtie, 25 highlight, 16 drop-down, 11 ordering, 6 calculations, 5 select-N and 1 grouping. Multiple choice is 67% of the new questions, above the 55% plan, because case-study and standalone items were counted together; later waves should shift further toward NGN formats. Across the whole bank, multiple choice fell from 79% to 75%.

## 7. Tests

| Wave 1 (500 questions, b68 to b87) | |
|---|---|
| By category | MOC 96, SIC 70, HPM 45, PSI 50, BCC 50, PPT 80, RRP 65, PA 44 |
| Easy / moderate / difficult | 28 / 253 / 219 (chunk 5: 3 / 38 / 59) |
| Key is the longest option (MC, per file) | 0% to 24% |
| Whole bank after wave 1 (1,610) | key longest 13%, best trick 25% (assess first), absolutes 6%, tiers 117 / 901 / 592 |

Checks run on this branch:
- `python3 -I build_all.py`: passes; 58 hardened files.
- Trial imports into scratch databases: chunks 1 to 3 gave 300 created; chunks 4 and 5 on top of main gave 200 created; 0 problems.
- `dotnet build`: 0 warnings, 0 errors; `dotnet test`: passed.
- `npm run lint`, `npm run typecheck`: clean; `npm test`: 475 passed (76 files). The frontend is unchanged by this branch.

## 8. Follow-ups and known limitations

- **Wave 2 (b88 onward).** Keep chunk 5's depth and push more items into NGN formats.
- **Calibration.** Difficulty is author-judged until real answers allow `irt_b` estimates; the adaptive pass line stays at θ 0.5.
- **Licensed review.** No RN credential is recorded for these questions.
- **Small new practice parts.** Some new parts hold only a few questions until later chunks fill them.
- **Duplicate check speed.** `quality.duplicates()` compares every pair of stems; at 5,000 questions it will take longer and may need an index.

## 9. Try it yourself

1. **See the next chunk's plan.** Change one category's count in a new batch, rebuild, and read how `PROGRESS.md`'s "Next chunk" column shifts.
2. **Break a bowtie.** In `b70`, mark a third action as correct and run `python3 -I lengths.py 70`. Read the assertion from `qb.bowtie()`, then undo it.
3. **Check frozen ids.** Compare the first 32 set ids in `rn-bank-79-*.rn.json` with `catalog-frozen.json`, then add a dummy batch and confirm they stay the same.
