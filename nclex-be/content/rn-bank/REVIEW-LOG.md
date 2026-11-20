# RN question bank review log

Each review round is recorded here. Candidates see only the short status line under each rationale
(`qb.REVIEW_STATUS`); this log holds who reviewed what, and on what basis.

| Date | Reviewer | Scope | Decision | Basis |
|---|---|---|---|---|
| 2026-11-17 | Muritala T. Aliyu (content team) | All 1,110 questions in batches b01-b38 and b60-b67, including 10 case studies | Approved | Content review using the review packet (`review_sheet.py`). No clinical licence or credential was recorded for this round. |
| 2026-11-19 | Content team (hardening pass 1) | 362 questions in b01-b05, b35 and b61-b67 rewritten to NCLEX-RN difficulty: clinical stems, plausible distractors, balanced option lengths, honest difficulty tiers | Approved | Rewritten after a candidate who had failed the NCLEX-RN four times passed the adaptive exam unprepared. Each batch passes the quality gates in `quality.py` (QUALITY.md). Candidates see `qb.HARDENED_STATUS`. No clinical licence or credential was recorded for this round. |
| 2026-11-20 | Content team (hardening passes 2 and 3) | 748 questions in b06-b34, b36-b38 and b60 rewritten to NCLEX-RN difficulty; all 46 RN files are now hardened | Approved | Same method and gates as pass 1 (QUALITY.md: whole bank key longest 14%, best trick 25%). Case-study items are left out of the duplicate stem check. No clinical licence or credential was recorded for this round. |

## How to record the next round
1. Run `python3 -I review_sheet.py` and review with `review/rn-bank-review.csv` or `.html`.
2. Fix or retire questions in `batches/`, then run `python3 -I build_all.py`.
3. Add a row above. If a licensed RN or nurse educator signs off, give their name and credentials, and update
   `REVIEW_STATUS` in `qb.py` to say so.
