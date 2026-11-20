"""Builds the RN bank's catalog: the curated test sets candidates see on the Practice page, and the
admin question bank. build_all.py calls attach() after every batch is built.

The importer refuses a file without questions, so the catalog travels in the last batch file
(imported last, after every question it lists is published). Everything is deterministic: the same
questions always give the same sets, so re-importing updates sets in place.

Sets (PRD 5.5, api.md section 3):
- Topic practice: every standalone question, grouped by Client Needs category, in parts of up to
  PART_SIZE, tutor mode, untimed.
- Case study practice: every NGN unfolding case, CASES_PER_SET cases per set, cases kept whole.
- Four fixed-length challenges of 85 items, each as a timed and an untimed challenge. F19 unpublished
  them (isPublished false) in favour of the adaptive exam; they stay in the file with the same ids and
  items so earlier attempts keep their set and history.
The sets published before the bank started growing (2026-11-20) are frozen in catalog-frozen.json: their
ids and items never change. New questions fill new parts after them.
- The adaptive exam (F19, architecture.md 5.8): mode cat, no question list; each attempt picks
  85 to 150 questions from the whole bank, and the candidate chooses timed (5 h) or untimed.
"""
import json
from pathlib import Path

PART_SIZE = 60
CASES_PER_SET = 5
ADAPTIVE_SECONDS = 18000    # 5 hours, the real NCLEX-RN's limit
# The pass line sits half a logit above "moderate" until the bank is rewritten to real NCLEX difficulty
# and calibrated: the 2026-11-17 questions were easier than their tiers, which inflated ability (θ).
ADAPTIVE_CONFIG = {"minItems": 85, "maxItems": 150, "passingTheta": 0.5}

CATEGORY_NAMES = {
    "SECE.MOC": "Management of Care", "SECE.SIC": "Safety and Infection Prevention and Control",
    "HPM": "Health Promotion and Maintenance", "PSI": "Psychosocial Integrity",
    "PHI.BCC": "Basic Care and Comfort", "PHI.PPT": "Pharmacological and Parenteral Therapies",
    "PHI.RRP": "Reduction of Risk Potential", "PHI.PA": "Physiological Adaptation",
}
# NCLEX-RN test plan, middle of each Client Needs range (percent); sums to 100. coverage.py uses it.
TEST_PLAN_MIDPOINTS = {"SECE.MOC": 18, "SECE.SIC": 13, "HPM": 9, "PSI": 9, "PHI.BCC": 9,
                       "PHI.PPT": 16, "PHI.RRP": 12, "PHI.PA": 14}

BANK_ID = "c1f00000-0000-4000-8000-000000000001"
ADAPTIVE_SET_NUMBER = 32     # the adaptive exam's set id; it must never change (attempt history)
NEW_TOPIC_BASE = 1000        # set number of new topic parts: base + category index * 100 + part
NEW_CASE_BASE = 2000         # set number of new case-study sets: base + set number


def _set_id(n):
    return f"c1e00000-0000-4000-8000-{n:012d}"


def _practice(n, title, description, question_ids):
    return {"id": _set_id(n), "title": title, "description": description, "mode": "practice",
            "revealMode": "tutor", "timed": False, "accessTier": "free", "questionIds": question_ids}


def _frozen():
    """The sets published before the bank grew past the 1,110 hardened questions (catalog-frozen.json).

    Their ids and question lists never change, so a candidate's finished sets stay finished and earlier
    attempts keep their history. Questions added since go into new parts after them."""
    return json.loads((Path(__file__).resolve().parent / "catalog-frozen.json").read_text(encoding="utf-8"))


def build_catalog(questions):
    """Returns (banks, testSets) for every question in the bank."""
    frozen = _frozen()
    standalone = [q for q in questions if not q.get("caseStudyId")]
    cases = {}
    for q in questions:
        if q.get("caseStudyId"):
            cases.setdefault(q["caseStudyId"], []).append(q)
    case_blocks = [[q["id"] for q in sorted(items, key=lambda q: q["casePosition"])]
                   for _, items in sorted(cases.items())]

    sets = []
    # ---- topic practice: the frozen parts first, then new questions in file order, PART_SIZE per part.
    # Filling parts in order (not re-splitting evenly) keeps every earlier part the same as the bank grows.
    for ci, (cn, name) in enumerate(CATEGORY_NAMES.items()):
        old = [p for p in frozen["topicParts"] if p["clientNeeds"] == cn]
        listed = {qid for p in old for qid in p["questionIds"]}
        added = [q["id"] for q in standalone if q["clientNeeds"] == cn and q["id"] not in listed]
        parts = [(p["id"], p["questionIds"]) for p in old]
        parts += [(_set_id(NEW_TOPIC_BASE + ci * 100 + k), added[i:i + PART_SIZE])
                  for k, i in enumerate(range(0, len(added), PART_SIZE), len(old) + 1)]
        for p, (set_id, part) in enumerate(parts, 1):
            suffix = f", part {p} of {len(parts)}" if len(parts) > 1 else ""
            sets.append({**_practice(0, f"{name}{suffix}",
                                     f"{len(part)} {name} questions with the answer and rationale after each one.", part),
                         "id": set_id})

    # ---- case study practice: frozen sets, then new cases CASES_PER_SET per set ----
    listed = {qid for s in frozen["caseSets"] for qid in s["questionIds"]}
    added = [b for b in case_blocks if b[0] not in listed]
    groups = [(s["id"], s["questionIds"]) for s in frozen["caseSets"]]
    groups += [(_set_id(NEW_CASE_BASE + k), [qid for b in added[i:i + CASES_PER_SET] for qid in b])
               for k, i in enumerate(range(0, len(added), CASES_PER_SET), len(groups) + 1)]
    for g, (set_id, ids) in enumerate(groups, 1):
        sets.append({**_practice(0, f"NGN case studies, set {g} of {len(groups)}",
                                 f"{len(ids) // 6} unfolding case studies of 6 items each, with rationales after each item.", ids),
                     "id": set_id})

    # ---- the fixed-length challenges: unpublished since F19 and frozen with their original items ----
    sets += frozen["challenges"]
    n = ADAPTIVE_SET_NUMBER - 1

    # ---- the adaptive exam (F19) ----
    n += 1
    sets.append({
        "id": _set_id(n), "title": "NCLEX-RN adaptive exam",
        "description": ("Works like the real NCLEX-RN: each question depends on your answers so far, and the exam "
                        "stops once your result is clear, after 85 to 150 questions. Choose timed (5 hours) or "
                        "untimed. No going back; answers and rationales at the end. The questions are being rewritten "
                        "to real NCLEX difficulty and recalibrated, so treat your result as a practice estimate."),
        "mode": "cat", "revealMode": "exam", "timed": True, "timeBudgetSec": ADAPTIVE_SECONDS,
        "allowBackNav": False, "accessTier": "free", "questionIds": [], "catConfig": ADAPTIVE_CONFIG})

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
