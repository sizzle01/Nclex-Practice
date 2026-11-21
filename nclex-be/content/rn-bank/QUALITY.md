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
| 06-cardiovascular-1 | 25 | yes | 17 | 18% | 29% (skip absolutes) | 6% | 34 | 1/16/8 |
| 07-cardiovascular-2 | 24 | yes | 18 | 11% | 31% (shortest) | 6% | 31.5 | 1/15/8 |
| 08-respiratory-1 | 24 | yes | 19 | 21% | 32% (shortest) | 5% | 32 | 2/14/8 |
| 09-respiratory-2 | 24 | yes | 19 | 11% | 30% (stem words) | 11% | 31 | 3/13/8 |
| 10-fluids-electrolytes-acid-base | 26 | yes | 20 | 20% | 30% (skip absolutes) | 5% | 33.5 | 2/16/8 |
| 11-renal-urinary | 26 | yes | 23 | 4% | 27% (assess first) | 9% | 36 | 2/16/8 |
| 12-endocrine-diabetes | 24 | yes | 19 | 16% | 29% (assess first) | 11% | 33 | 3/12/9 |
| 13-endocrine-thyroid-adrenal-pituitary | 23 | yes | 19 | 5% | 24% (assess first) | 11% | 32 | 0/16/7 |
| 14-neuro-1 | 25 | yes | 21 | 5% | 29% (assess first) | 0% | 31 | 2/15/8 |
| 15-neuro-2-sensory | 25 | yes | 23 | 13% | 27% (assess first) | 13% | 31 | 3/14/8 |
| 16-gi-1 | 25 | yes | 22 | 14% | 25% (assess first) | 9% | 32 | 1/16/8 |
| 17-gi-2-liver-pancreas | 24 | yes | 20 | 10% | 24% (assess first) | 15% | 28 | 2/14/8 |
| 18-hematology | 23 | yes | 18 | 17% | 25% (assess first) | 0% | 35 | 0/16/7 |
| 19-oncology-palliative | 24 | yes | 22 | 23% | 25% (longest) | 9% | 32 | 1/15/8 |
| 20-immune-infectious | 25 | yes | 23 | 17% | 29% (stem words) | 4% | 30 | 2/15/8 |
| 21-musculoskeletal | 24 | yes | 21 | 14% | 25% (assess first) | 14% | 29 | 2/14/8 |
| 22-integumentary-burns | 25 | yes | 20 | 5% | 25% (assess first) | 10% | 31 | 2/15/8 |
| 23-perioperative | 23 | yes | 20 | 10% | 29% (assess first) | 10% | 32 | 2/14/7 |
| 24-basic-care-comfort | 23 | yes | 20 | 15% | 25% (shortest) | 10% | 31 | 2/14/7 |
| 25-health-promotion-lifespan | 24 | yes | 21 | 10% | 23% (assess first) | 5% | 33 | 1/15/8 |
| 26-maternity-antepartum | 25 | yes | 23 | 4% | 27% (shortest) | 4% | 43 | 3/12/10 |
| 27-maternity-intrapartum | 24 | yes | 22 | 9% | 25% (shortest) | 5% | 43 | 3/12/9 |
| 28-maternity-postpartum-newborn | 22 | yes | 20 | 0% | 24% (assess first) | 10% | 38.5 | 2/12/8 |
| 29-pediatrics-1 | 22 | yes | 19 | 0% | 26% (shortest) | 5% | 35.5 | 3/12/7 |
| 30-pediatrics-2 | 23 | yes | 22 | 9% | 24% (assess first) | 0% | 41 | 3/11/9 |
| 31-pediatrics-3 | 21 | yes | 20 | 25% | 28% (shortest) | 0% | 41 | 3/10/8 |
| 32-mental-health-1 | 21 | yes | 19 | 26% | 31% (longest) | 0% | 39 | 3/10/8 |
| 33-mental-health-2 | 22 | yes | 19 | 21% | 25% (longest) | 5% | 39 | 2/12/8 |
| 34-mental-health-3 | 22 | yes | 20 | 15% | 30% (assess first) | 5% | 41.5 | 2/12/8 |
| 35-pharm-psych-misc | 20 | yes | 19 | 16% | 29% (assess first) | 0% | 38 | 0/13/7 |
| 36-ngn-cases-1 | 12 | yes | 2 | 0% | 50% (shortest) | 0% | 16 | 0/7/5 |
| 37-ngn-cases-2 | 12 | yes | 3 | 33% | 33% (longest) | 0% | 14.5 | 0/8/4 |
| 38-ngn-cases-3 | 12 | yes | 4 | 0% | 31% (stem words) | 0% | 19.5 | 0/7/5 |
| 60-ngn-cases-1 | 24 | yes | 5 | 20% | 40% (stem words) | 0% | 14 | 0/15/9 |
| 61-management-of-care-settings | 33 | yes | 24 | 25% | 31% (skip absolutes) | 4% | 26 | 3/19/11 |
| 62-safety-systems | 32 | yes | 24 | 12% | 25% (assess first) | 12% | 31 | 4/18/10 |
| 63-management-of-care-legal-it | 30 | yes | 25 | 20% | 25% (assess first) | 12% | 35 | 3/18/9 |
| 64-safety-infection-injury | 29 | yes | 24 | 17% | 25% (assess first) | 12% | 29 | 3/17/9 |
| 65-management-of-care-specialty | 27 | yes | 18 | 17% | 28% (stem words) | 11% | 27 | 2/15/10 |
| 66-risk-reduction-basic-care | 31 | yes | 27 | 7% | 29% (assess first) | 15% | 32 | 4/17/10 |
| 67-moc-sic-ngn | 34 | yes | 23 | 0% | 23% (shortest) | 9% | 32 | 0/23/11 |
| 68-moc-priorities-delegation-legal | 25 | yes | 17 | 29% | 33% (stem words) | 12% | 33 | 2/13/10 |
| 69-safety-health-promotion-psychosocial | 25 | yes | 17 | 24% | 32% (skip absolutes) | 6% | 33 | 1/15/9 |
| 70-pharmacology-basic-care | 25 | yes | 17 | 24% | 32% (assess first) | 12% | 40 | 2/12/11 |
| 71-risk-reduction-adaptation-cases | 25 | yes | 11 | 9% | 30% (stem words) | 9% | 34 | 1/14/10 |
| 72-moc-ethics-continuity-safety | 25 | yes | 19 | 0% | 20% (stem words) | 5% | 31 | 2/13/10 |
| 73-infection-lifespan-mental-health | 25 | yes | 18 | 0% | 25% (stem words) | 0% | 36 | 2/14/9 |
| 74-medications-comfort-care | 25 | yes | 17 | 6% | 24% (stem words) | 0% | 32 | 2/13/10 |
| 75-procedures-emergencies-cases | 25 | yes | 11 | 0% | 32% (stem words) | 0% | 26 | 0/15/10 |
| 76-moc-resources-legal-safety | 25 | yes | 20 | 10% | 32% (shortest) | 0% | 30 | 2/12/11 |
| 77-precautions-prevention-psychosocial | 25 | yes | 20 | 15% | 25% (assess first) | 5% | 30 | 2/14/9 |
| 78-drug-therapy-basic-needs | 25 | yes | 19 | 5% | 34% (shortest) | 0% | 33 | 2/12/11 |
| 79-monitoring-complications-cases | 25 | yes | 11 | 9% | 30% (assess first) | 0% | 26 | 0/15/10 |
| 80-moc-staffing-rights-safety | 25 | yes | 20 | 10% | 30% (shortest) | 0% | 29 | 2/12/11 |
| 81-infection-development-coping | 25 | yes | 19 | 5% | 31% (shortest) | 0% | 29 | 2/14/9 |
| 82-medication-safety-daily-care | 25 | yes | 18 | 11% | 28% (assess first) | 0% | 28 | 2/13/10 |
| 83-diagnostics-acute-care-cases | 25 | yes | 12 | 8% | 31% (assess first) | 0% | 26 | 1/14/10 |
| 84-moc-judgment-safety | 25 | yes | 19 | 5% | 26% (assess first) | 0% | 31 | 1/8/16 |
| 85-prevention-lifespan-mental-health | 25 | yes | 19 | 0% | 28% (shortest) | 0% | 28 | 1/10/14 |
| 86-pharmacotherapy-comfort-depth | 25 | yes | 18 | 17% | 28% (assess first) | 0% | 31 | 1/11/13 |
| 87-hemodynamics-critical-cases | 25 | yes | 11 | 9% | 27% (shortest) | 0% | 26 | 0/9/16 |
| 88-moc-team-systems-safety | 25 | yes | 10 | 10% | 15% (shortest) | 0% | 22 | 0/10/15 |
| 89-infection-screening-psychosocial | 25 | yes | 11 | 9% | 30% (assess first) | 0% | 29 | 0/14/11 |
| 90-medications-care-measures | 25 | yes | 12 | 8% | 31% (stem words) | 0% | 36 | 1/13/11 |
| 91-monitoring-shock-cases | 25 | yes | 7 | 0% | 29% (shortest) | 0% | 21 | 0/11/14 |
| 92-moc-triage-disclosure-safety | 25 | yes | 11 | 0% | 30% (shortest) | 0% | 25 | 0/12/13 |
| 93-asepsis-newborn-mental-health | 25 | yes | 11 | 9% | 32% (assess first) | 0% | 25 | 0/15/10 |
| 94-drug-safety-daily-function | 25 | yes | 13 | 8% | 25% (assess first) | 8% | 26 | 0/14/11 |
| 95-diagnostics-emergencies-cases | 25 | yes | 7 | 0% | 25% (stem words) | 0% | 21 | 0/11/14 |
| 96-moc-law-levels-of-care-safety | 25 | yes | 11 | 0% | 27% (shortest) | 0% | 27 | 0/14/11 |
| 97-disinfection-development-mental-health | 25 | yes | 13 | 8% | 23% (shortest) | 0% | 26 | 0/15/10 |
| 98-medication-technique-comfort | 25 | yes | 12 | 0% | 30% (stem words) | 0% | 24 | 0/14/11 |
| 99-procedures-neuro-cardiac-cases | 25 | yes | 7 | 29% | 29% (longest) | 0% | 21 | 0/12/13 |
| 100-cardiac-cases-acs-hf-af-htn | 24 | yes | 0 | 0% | 0% (-) | 0% | 17.5 | 0/4/20 |
| 101-cardiac-cases-vascular-coagulation | 24 | yes | 0 | 0% | 0% (-) | 0% | 20 | 0/4/20 |
| 102-cardiac-cases-pacing-vt-shock | 24 | yes | 0 | 0% | 0% (-) | 0% | 16 | 0/4/20 |
| 103-cardiac-cases-shock-vascular-standalone | 28 | yes | 1 | 0% | 25% (stem words) | 0% | 14.5 | 0/4/24 |
| 104-pulmonary-cases-pneumonia-asthma-copd-tb | 24 | yes | 0 | 0% | 0% (-) | 0% | 12 | 0/4/20 |
| 105-pulmonary-cases-ards-chesttube-osa-trach | 24 | yes | 0 | 0% | 0% (-) | 0% | 15.5 | 0/4/20 |
| 106-pulmonary-cases-cf-aspiration-flu-effusion | 24 | yes | 1 | 0% | 33% (stem words) | 0% | 12 | 0/4/20 |
| 107-pulmonary-cases-co-flail-hfnc-otc-standalone | 28 | yes | 1 | 0% | 50% (longest) | 0% | 13.5 | 0/4/24 |
| 108-endocrine-cases-hhs-edka-hypo-t2dm | 24 | yes | 0 | 0% | 0% (-) | 0% | 13 | 0/4/20 |
| 109-endocrine-cases-thyroid | 24 | yes | 0 | 0% | 0% (-) | 0% | 10 | 0/4/20 |
| 110-endocrine-cases-adrenal-pituitary | 24 | yes | 0 | 0% | 0% (-) | 0% | 10 | 0/4/20 |
| 111-endocrine-cases-pheo-bone-foot-pump-standalone | 28 | yes | 1 | 0% | 25% (stem words) | 0% | 14.5 | 0/4/24 |
| **Whole bank** | 2210 | | 1335 | 12% | 25% (assess first) | 5% | 29 | 118/1104/988 |
