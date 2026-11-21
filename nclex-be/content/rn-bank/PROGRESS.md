# RN bank progress toward 5,000 questions

Written by `python3 -I build_all.py` (coverage.py); do not edit by hand.

**2310 of 5000 questions** (46%), 90 case studies, 55 bowtie questions. Multiple choice is 58% of the bank.

## By test plan category

| Category | Test plan | Target | Have | To write | Next chunk |
|---|---|---|---|---|---|
| Management of Care | 18% | 900 | 355 | 545 | 20 |
| Safety and Infection Prevention and Control | 13% | 650 | 258 | 392 | 15 |
| Health Promotion and Maintenance | 9% | 450 | 178 | 272 | 10 |
| Psychosocial Integrity | 9% | 450 | 148 | 302 | 11 |
| Basic Care and Comfort | 9% | 450 | 161 | 289 | 11 |
| Pharmacological and Parenteral Therapies | 16% | 800 | 396 | 404 | 15 |
| Reduction of Risk Potential | 12% | 600 | 254 | 346 | 13 |
| Physiological Adaptation | 14% | 700 | 560 | 140 | 5 |
| **Total** | 100% | 5000 | 2310 | 2690 | 100 |

## Chunks

| Chunk | Wave | Batch files | Questions |
|---|---|---|---|
| 1 | 1 | b68, b69, b70, b71 | 100 |
| 2 | 1 | b72, b73, b74, b75 | 100 |
| 3 | 1 | b76, b77, b78, b79 | 100 |
| 4 | 1 | b80, b81, b82, b83 | 100 |
| 5 | 1 | b84, b85, b86, b87 | 100 |
| 6 | 2 | b88, b89, b90, b91 | 100 |
| 7 | 2 | b92, b93, b94, b95 | 100 |
| 8 | 2 | b96, b97, b98, b99 | 100 |

The wave chunks are paused (2026-11-20) while the bank is rebuilt around case studies.

## Case conversion (CASE-MAP.md)

**540 of 1942 active questions (28%) are inside full case studies**; the goal is 85%. 368 older questions are retired (F26, retired-questions.json): their topics were rebuilt as case studies.

| Chunk | Topic | Batch files | Questions | Case studies |
|---|---|---|---|---|
| 1 | Cardiac, pacemaker, hemodynamic monitoring and shock | b100, b101, b102, b103 | 100 | 16 |
| 2 | Pulmonary disorders with respiratory pharmacology | b104, b105, b106, b107 | 100 | 16 |
| 3 | Endocrine disorders with insulin, antidiabetic, thyroid and corticosteroid drugs | b108, b109, b110, b111 | 100 | 16 |
| 4 | Gastrointestinal disorders with PUD, GERD and antiemetic drugs | b112, b113, b114, b115 | 100 | 16 |

**Next: conversion chunk 5**: Neurologic disorders with antiseizure and Parkinson drugs and cranial nerve assessment (CASE-MAP.md).

## Item types in the whole bank

| Type | Questions | Share |
|---|---|---|
| multiple_choice | 1336 | 58% |
| matrix_mc | 272 | 12% |
| sata | 203 | 9% |
| dropdown_cloze | 103 | 4% |
| highlight_table | 84 | 4% |
| highlight_text | 62 | 3% |
| bowtie | 55 | 2% |
| ordered_response | 54 | 2% |
| fill_blank_calc | 49 | 2% |
| dropdown_table | 32 | 1% |
| select_n | 25 | 1% |
| multiple_response_grouping | 21 | 1% |
| matrix_mr | 14 | 1% |
