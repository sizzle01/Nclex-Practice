# RN question bank review log

Each review round is recorded here. Candidates see only the short status line under each rationale
(`qb.REVIEW_STATUS`); this log holds who reviewed what, and on what basis.

| Date | Reviewer | Scope | Decision | Basis |
|---|---|---|---|---|
| 2026-11-17 | Muritala T. Aliyu (content team) | All 1,110 questions in batches b01-b38 and b60-b67, including 10 case studies | Approved | Content review using the review packet (`review_sheet.py`). No clinical licence or credential was recorded for this round. |

## How to record the next round
1. Run `python3 -I review_sheet.py` and review with `review/rn-bank-review.csv` or `.html`.
2. Fix or retire questions in `batches/`, then run `python3 -I build_all.py`.
3. Add a row above. If a licensed RN or nurse educator signs off, give their name and credentials, and update
   `REVIEW_STATUS` in `qb.py` to say so.
