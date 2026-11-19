# RN bank quality report

Written by `python3 -I build_all.py`; do not edit by hand. See `quality.py` for the checks.

- **Key longest:** share of multiple-choice questions whose key is the single longest option. Chance is about 25%.
- **Best trick:** the best score a test-wise candidate gets on the multiple-choice questions without any nursing
  knowledge (pick the longest or the shortest option, the one repeating the stem's words, avoid absolute words, or pick "assess").
  Chance is about 25%; hardened files must stay at or below 35%.
- **Absolutes:** share of multiple-choice questions where only wrong options say always, never, all, only and so on.
- **Difficulty:** easy / moderate / difficult.

| File | Questions | Hardened | MC | Key longest | Best trick | Absolutes | Median stem words | Difficulty |
|---|---|---|---|---|---|---|---|---|
| 01-safety-infection-control | 25 | yes | 18 | 11% | 25% (shortest) | 6% | 32 | 3/14/8 |
| 02-management-of-care-priorities | 25 | yes | 17 | 24% | 32% (skip absolutes) | 6% | 27 | 2/14/9 |
| 03-management-of-care-legal-ethical | 26 | yes | 21 | 24% | 26% (longest) | 5% | 35 | 3/15/8 |
| 04-pharm-high-alert | 26 | yes | 19 | 26% | 34% (stem words) | 11% | 37 | 1/17/8 |
| 05-pharm-calculations-technique | 24 | yes | 11 | 18% | 32% (longest) | 0% | 48.5 | 3/13/8 |
| 06-cardiovascular-1 | 25 |  | 17 | 53% | 59% (longest) | 0% | 27 | 3/17/5 |
| 07-cardiovascular-2 | 24 |  | 18 | 56% | 56% (longest) | 6% | 21.5 | 2/16/6 |
| 08-respiratory-1 | 24 |  | 19 | 47% | 50% (longest) | 21% | 19 | 4/17/3 |
| 09-respiratory-2 | 24 |  | 19 | 68% | 73% (longest) | 11% | 17 | 3/14/7 |
| 10-fluids-electrolytes-acid-base | 26 |  | 20 | 55% | 55% (longest) | 5% | 20.5 | 3/16/7 |
| 11-renal-urinary | 26 |  | 23 | 65% | 65% (longest) | 4% | 20 | 8/16/2 |
| 12-endocrine-diabetes | 24 |  | 19 | 68% | 71% (longest) | 26% | 17.5 | 4/11/9 |
| 13-endocrine-thyroid-adrenal-pituitary | 23 |  | 19 | 63% | 66% (longest) | 5% | 19 | 1/18/4 |
| 14-neuro-1 | 25 |  | 21 | 76% | 76% (longest) | 14% | 19 | 3/16/6 |
| 15-neuro-2-sensory | 25 |  | 23 | 83% | 83% (longest) | 17% | 18 | 3/18/4 |
| 16-gi-1 | 25 |  | 22 | 64% | 70% (skip absolutes) | 18% | 15 | 7/17/1 |
| 17-gi-2-liver-pancreas | 24 |  | 20 | 80% | 80% (longest) | 10% | 14 | 8/14/2 |
| 18-hematology | 23 |  | 18 | 78% | 81% (longest) | 0% | 17 | 1/17/5 |
| 19-oncology-palliative | 24 |  | 22 | 82% | 82% (longest) | 27% | 17 | 5/17/2 |
| 20-immune-infectious | 25 |  | 23 | 74% | 74% (longest) | 22% | 16 | 4/20/1 |
| 21-musculoskeletal | 24 |  | 21 | 76% | 79% (longest) | 29% | 15.5 | 8/14/2 |
| 22-integumentary-burns | 25 |  | 20 | 90% | 90% (longest) | 20% | 15 | 7/14/4 |
| 23-perioperative | 23 |  | 20 | 90% | 90% (longest) | 15% | 19 | 7/14/2 |
| 24-basic-care-comfort | 23 |  | 20 | 65% | 68% (longest) | 10% | 13 | 10/12/1 |
| 25-health-promotion-lifespan | 24 |  | 22 | 68% | 77% (skip absolutes) | 27% | 13 | 11/13/0 |
| 26-maternity-antepartum | 25 |  | 23 | 52% | 57% (skip absolutes) | 26% | 19 | 5/15/5 |
| 27-maternity-intrapartum | 24 |  | 22 | 68% | 73% (longest) | 0% | 18.5 | 4/12/8 |
| 28-maternity-postpartum-newborn | 22 |  | 20 | 70% | 75% (skip absolutes) | 25% | 16 | 4/15/3 |
| 29-pediatrics-1 | 22 |  | 19 | 63% | 66% (longest) | 16% | 13.5 | 5/15/2 |
| 30-pediatrics-2 | 23 |  | 22 | 82% | 82% (longest) | 23% | 15 | 3/17/3 |
| 31-pediatrics-3 | 21 |  | 20 | 75% | 75% (longest) | 20% | 15 | 4/14/3 |
| 32-mental-health-1 | 21 |  | 19 | 84% | 87% (longest) | 32% | 17 | 4/17/0 |
| 33-mental-health-2 | 22 |  | 19 | 89% | 89% (longest) | 16% | 18 | 1/18/3 |
| 34-mental-health-3 | 22 |  | 20 | 75% | 78% (longest) | 35% | 16 | 2/17/3 |
| 35-pharm-psych-misc | 20 | yes | 19 | 16% | 29% (assess first) | 0% | 38 | 0/13/7 |
| 36-ngn-cases-1 | 12 |  | 2 | 50% | 62% (stem words) | 0% | 15.5 | 1/6/5 |
| 37-ngn-cases-2 | 12 |  | 3 | 100% | 100% (longest) | 0% | 13.5 | 0/9/3 |
| 38-ngn-cases-3 | 12 |  | 4 | 100% | 100% (longest) | 0% | 14 | 1/6/5 |
| 60-ngn-cases-1 | 24 |  | 5 | 80% | 80% (longest) | 0% | 13 | 0/17/7 |
| 61-management-of-care-settings | 33 | yes | 24 | 25% | 31% (skip absolutes) | 4% | 26 | 3/19/11 |
| 62-safety-systems | 32 | yes | 24 | 12% | 25% (assess first) | 12% | 31 | 4/18/10 |
| 63-management-of-care-legal-it | 30 |  | 25 | 88% | 88% (longest) | 24% | 20 | 0/22/8 |
| 64-safety-infection-injury | 29 |  | 24 | 92% | 92% (longest) | 33% | 16 | 1/24/4 |
| 65-management-of-care-specialty | 27 |  | 18 | 83% | 86% (longest) | 11% | 15 | 0/14/13 |
| 66-risk-reduction-basic-care | 31 |  | 27 | 93% | 94% (longest) | 26% | 17 | 1/26/4 |
| 67-moc-sic-ngn | 34 |  | 23 | 87% | 89% (longest) | 17% | 18 | 0/25/9 |
| **Whole bank** | 1110 | | 874 | 65% | 67% (longest) | 15% | 18.5 | 157/723/230 |
