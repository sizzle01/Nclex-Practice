"""Builds the RN bank's catalog: the curated test sets candidates see on the Practice page, and the
admin question bank. build_all.py calls attach() after every batch is built.

The importer refuses a file without questions, so the catalog travels in the last batch file
(imported last, after every question it lists is published). Everything is deterministic: the same
questions always give the same sets, so re-importing updates sets in place.

Sets (PRD 5.5, api.md section 3):
- Topic practice: every standalone question, grouped by Client Needs category, in parts of up to
  PART_SIZE, tutor mode, untimed.
- Case study practice: every NGN unfolding case, CASES_PER_SET cases per set, cases kept whole.
- Challenges: EXAM_COUNT exams of EXAM_LENGTH items, each published as a timed and an untimed
  challenge. Each holds CASES_PER_EXAM whole cases and standalone items weighted by the NCLEX-RN
  test plan midpoints, with no item repeated across exams. F19 will make both versions adaptive.
"""
import json
import math
import random

PART_SIZE = 60
CASES_PER_SET = 5
EXAM_COUNT = 4
EXAM_LENGTH = 85            # the real NCLEX-RN's minimum length
CASES_PER_EXAM = 2
EXAM_SECONDS = 10200        # 2 h 50 min: about 2 minutes per item, like the real exam's pace
PASS_THRESHOLD = 0.65

CATEGORY_NAMES = {
    "SECE.MOC": "Management of Care", "SECE.SIC": "Safety and Infection Control",
    "HPM": "Health Promotion and Maintenance", "PSI": "Psychosocial Integrity",
    "PHI.BCC": "Basic Care and Comfort", "PHI.PPT": "Pharmacological and Parenteral Therapies",
    "PHI.RRP": "Reduction of Risk Potential", "PHI.PA": "Physiological Adaptation",
}
# NCSBN 2023 NCLEX-RN test plan, middle of each range (percent); sums to 100.
TEST_PLAN_MIDPOINTS = {"SECE.MOC": 18, "SECE.SIC": 13, "HPM": 9, "PSI": 9, "PHI.BCC": 9,
                       "PHI.PPT": 16, "PHI.RRP": 12, "PHI.PA": 14}

BANK_ID = "c1f00000-0000-4000-8000-000000000001"


def _set_id(n):
    return f"c1e00000-0000-4000-8000-{n:012d}"


def _practice(n, title, description, question_ids):
    return {"id": _set_id(n), "title": title, "description": description, "mode": "practice",
            "revealMode": "tutor", "timed": False, "accessTier": "free", "questionIds": question_ids}


def _split(ids, size):
    """Splits ids into the fewest parts of at most size, as even as possible."""
    parts = math.ceil(len(ids) / size)
    base, extra = divmod(len(ids), parts)
    out, start = [], 0
    for p in range(parts):
        end = start + base + (1 if p < extra else 0)
        out.append(ids[start:end])
        start = end
    return out


def _allocate(total, weights):
    """Largest-remainder split of total across weights."""
    raw = {k: total * w / sum(weights.values()) for k, w in weights.items()}
    counts = {k: int(v) for k, v in raw.items()}
    for k in sorted(raw, key=lambda k: raw[k] - counts[k], reverse=True)[: total - sum(counts.values())]:
        counts[k] += 1
    return counts


def build_catalog(questions):
    """Returns (banks, testSets) for every question in the bank."""
    rnd = random.Random(20261117)
    standalone = [q for q in questions if not q.get("caseStudyId")]
    cases = {}
    for q in questions:
        if q.get("caseStudyId"):
            cases.setdefault(q["caseStudyId"], []).append(q)
    case_blocks = [[q["id"] for q in sorted(items, key=lambda q: q["casePosition"])]
                   for _, items in sorted(cases.items())]

    sets, n = [], 0
    # ---- topic practice ----
    for cn, name in CATEGORY_NAMES.items():
        ids = [q["id"] for q in standalone if q["clientNeeds"] == cn]
        rnd.shuffle(ids)
        parts = _split(ids, PART_SIZE)
        for p, part in enumerate(parts, 1):
            n += 1
            suffix = f", part {p} of {len(parts)}" if len(parts) > 1 else ""
            sets.append(_practice(n, f"{name}{suffix}",
                                  f"{len(part)} {name} questions with the answer and rationale after each one.", part))

    # ---- case study practice ----
    order = case_blocks[:]
    rnd.shuffle(order)
    groups = _split(order, CASES_PER_SET)
    for g, group in enumerate(groups, 1):
        n += 1
        ids = [qid for block in group for qid in block]
        sets.append(_practice(n, f"NGN case studies, set {g} of {len(groups)}",
                              f"{len(group)} unfolding case studies of 6 items each, with rationales after each item.", ids))

    # ---- mock exams ----
    by_cn = {cn: [q["id"] for q in standalone if q["clientNeeds"] == cn] for cn in CATEGORY_NAMES}
    for ids in by_cn.values():
        rnd.shuffle(ids)
    quota = _allocate(EXAM_LENGTH - 6 * CASES_PER_EXAM, TEST_PLAN_MIDPOINTS)
    exam_cases = order[:]
    exams = []
    for e in range(1, EXAM_COUNT + 1):
        items = []
        for cn, count in quota.items():
            if len(by_cn[cn]) < count:
                raise SystemExit(f"catalog: not enough {cn} questions for mock exam {e}")
            items += [by_cn[cn].pop() for _ in range(count)]
        rnd.shuffle(items)
        # Cases go in as whole blocks, spread through the exam.
        chosen = [exam_cases[(e - 1 + k * EXAM_COUNT) % len(exam_cases)] for k in range(CASES_PER_EXAM)]
        for k, block in enumerate(chosen):
            at = (k + 1) * len(items) // (CASES_PER_EXAM + 1) + k * 6
            items[at:at] = block
        assert len(items) == EXAM_LENGTH and len(set(items)) == EXAM_LENGTH
        exams.append(items)

    # Each exam comes as a timed and an untimed challenge with the same items. Both are fixed at
    # EXAM_LENGTH until F19 makes them adaptive (architecture.md 5.8: stop between 85 and 150 items).
    # The timed sets keep the ids they were first published with; the untimed ones come after.
    about = (f"{EXAM_LENGTH} items weighted like the NCLEX-RN test plan, including {CASES_PER_EXAM} case "
             "studies. Exam rules: no going back, answers and a pass estimate at the end. Fixed length for now; "
             "an adaptive version that stops between 85 and 150 items, like the real exam, is planned.")
    clock = f"{EXAM_SECONDS // 3600} h {EXAM_SECONDS % 3600 // 60} min"
    for timed in (True, False):
        for e, items in enumerate(exams, 1):
            n += 1
            sets.append({
                "id": _set_id(n), "title": f"NCLEX-RN challenge {e} ({'timed' if timed else 'untimed'})",
                "description": f"{'Timed (' + clock + ').' if timed else 'No time limit.'} {about}",
                "mode": "challenge", "revealMode": "exam", "timed": timed,
                **({"timeBudgetSec": EXAM_SECONDS} if timed else {}),
                "passThreshold": PASS_THRESHOLD, "allowBackNav": False, "accessTier": "free", "questionIds": items})

    banks =[{"id": BANK_ID, "name": "RN Question Bank",
              "description": f"All {len(questions)} RN bank questions across every NCLEX-RN category and item type.",
              "accessTier": "free", "questionIds": [q["id"] for q in questions]}]
    return banks, sets


def attach(path, questions):
    """Writes the catalog into the import file at path (the last batch file)."""
    banks, sets = build_catalog(questions)
    data = json.loads(path.read_text(encoding="utf-8"))
    data["banks"], data["testSets"] = banks, sets
    data["notice"] += " Also carries the bank-wide catalog (catalog.py): the RN Question Bank and every curated test set."
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return banks, sets
