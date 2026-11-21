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
- Timed case-study blocks (F24): BLOCK_CASES whole cases per block under exam rules, timed at the NCLEX-RN
  pace, no going back, rationales at the end and no Pass/Fail outcome (outcomeHidden). A block is published only when it is full
  and is then recorded in timed-blocks.json, so its cases never change as the bank grows.
- Topic tests (F27): one untimed set per review-book subtopic (book_topics.py), with its case studies kept whole and
  then its standalone questions, in parts of up to TOPIC_PART_SIZE, answers and the result at the end. A topic with
  fewer than TOPIC_MIN_QUESTIONS questions stays unpublished until the bank grows.
- The adaptive exam (F19, architecture.md 5.8): mode cat, no question list; each attempt picks
  85 to 150 questions from the whole bank, and the candidate chooses timed (5 h) or untimed.
"""
import json
import re
from pathlib import Path

import book_topics

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
BLOCK_BASE = 3000            # set number of timed case-study blocks: base + block number

# Timed case-study blocks (F24). The candidate's report showed clinical judgment falling away late in the exam,
# so blocks build stamina under real conditions: the NCLEX-RN allows 5 hours for up to 150 questions, which is
# 120 seconds a question, and its optional breaks run on the exam clock (one short pause here).
BLOCK_CASES = 10
BLOCK_MAX_PER_SYSTEM = 3     # so every block mixes at least four body systems; leftover cases wait for later chunks
NCLEX_SECONDS_PER_QUESTION = 120
BLOCK_PAUSE_POLICY = {"maxPauses": 1, "maxPauseSec": 600}
BLOCKS_FILE = Path(__file__).resolve().parent / "timed-blocks.json"

# Topic tests (F27). Set number = TOPIC_BASE + subtopic index * TOPIC_STRIDE + part, so each subtopic keeps its set ids
# as the bank grows; book_topics.BOOK must only ever be appended to.
TOPIC_BASE = 5000
TOPIC_STRIDE = 20
TOPIC_PART_SIZE = 50
TOPIC_MIN_QUESTIONS = 10
# Every topic test ever written to an import file, with its last question list (append-only). The importer never
# unpublishes a set that drops out of the file, so when a topic shrinks to fewer parts the missing part is written
# back unpublished from this record.
TOPIC_SETS_FILE = Path(__file__).resolve().parent / "topic-sets.json"


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
    """Returns (banks, testSets) for every active question in the bank.

    Retired questions (F26) are left out of every set, frozen ones included: an attempt stores its own items, so a
    finished attempt keeps its history when a set loses questions. A practice part left with no active questions is
    unpublished and keeps its original list (the importer accepts retired questions only in unpublished sets); the
    unpublished challenges keep their original lists too."""
    retired = {q["id"] for q in questions if q.get("status") == "retired"}
    # Parts are cut from every standalone question, retired ones included, so each part keeps its id and its
    # membership as the bank changes; retired questions are then removed inside each part.
    all_standalone = [q for q in questions if not q.get("caseStudyId")]
    questions = [q for q in questions if q["id"] not in retired]
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
        added = [q["id"] for q in all_standalone if q["clientNeeds"] == cn and q["id"] not in listed]
        parts = [(p["id"], p["questionIds"]) for p in old]
        parts += [(_set_id(NEW_TOPIC_BASE + ci * 100 + k), added[i:i + PART_SIZE])
                  for k, i in enumerate(range(0, len(added), PART_SIZE), len(old) + 1)]
        # Numbering counts only parts that still have active questions, so the titles read "part 2 of 9".
        live = [(set_id, [qid for qid in part if qid not in retired]) for set_id, part in parts]
        shown = [set_id for set_id, part in live if part]
        for (set_id, original), (_, part) in zip(parts, live):
            if not part:
                sets.append({**_practice(0, f"{name} (retired part)",
                                         "Its questions were replaced by case studies.", original),
                             "id": set_id, "isPublished": False})
                continue
            p = shown.index(set_id) + 1
            suffix = f", part {p} of {len(shown)}" if len(shown) > 1 else ""
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

    # ---- timed case-study blocks (F24) ----
    sets += _timed_blocks(cases)

    # ---- topic tests (F27) ----
    sets += _topic_sets(questions, cases)

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


def _case_system(items):
    """A case's body system: the one most of its questions are tagged with (ties go to the first item's)."""
    systems = [q["bodySystem"] for q in items]
    return max(systems, key=lambda s: (systems.count(s), -systems.index(s)))


def _choose_block(unassigned, systems):
    """Picks BLOCK_CASES cases from unassigned (case ids in order) so the block mixes body systems, as the real
    exam does: each pick is the earliest case whose system is least used so far, and no system passes
    BLOCK_MAX_PER_SYSTEM. Returns None when the pool cannot fill a mixed block yet."""
    block, used = [], {}
    pool = list(unassigned)
    while len(block) < BLOCK_CASES:
        allowed = [cid for cid in pool if used.get(systems[cid], 0) < BLOCK_MAX_PER_SYSTEM]
        if not allowed:
            return None
        pick = min(allowed, key=lambda cid: (used.get(systems[cid], 0), pool.index(cid)))
        pool.remove(pick)
        block.append(pick)
        used[systems[pick]] = used.get(systems[pick], 0) + 1
    return block


def _timed_blocks(cases):
    """The timed blocks: those already recorded in timed-blocks.json, then a new block for every further
    BLOCK_CASES cases not yet in one. New blocks are written back to the file, so a published block keeps the same
    cases and questions for good (earlier attempts keep their set and history)."""
    recorded = json.loads(BLOCKS_FILE.read_text(encoding="utf-8")) if BLOCKS_FILE.exists() else []
    order = {cid: [q["id"] for q in sorted(items, key=lambda q: q["casePosition"])] for cid, items in cases.items()}
    for block in recorded:
        for cid, ids in zip(block["caseIds"], block["questionIds"]):
            if order.get(cid) != ids:
                raise SystemExit(f"timed block {block['number']}: case {cid} changed or was removed; published "
                                 "blocks must keep their questions (see catalog.py, F24)")
    assigned = {cid for block in recorded for cid in block["caseIds"]}
    unassigned = [cid for cid in sorted(cases) if cid not in assigned]
    systems = {cid: _case_system(items) for cid, items in cases.items()}
    while (chosen := _choose_block(unassigned, systems)) is not None:
        unassigned = [cid for cid in unassigned if cid not in chosen]
        recorded.append({"number": len(recorded) + 1, "caseIds": chosen, "questionIds": [order[cid] for cid in chosen]})
    BLOCKS_FILE.write_text(json.dumps(recorded, indent=1) + "\n", encoding="utf-8")

    sets = []
    for block in recorded:
        ids = [qid for case_ids in block["questionIds"] for qid in case_ids]
        budget = len(ids) * NCLEX_SECONDS_PER_QUESTION
        minutes = budget // 60
        sets.append({
            "id": _set_id(BLOCK_BASE + block["number"]),
            "title": f"Timed case study block {block['number']}",
            "description": (f"{len(block['caseIds'])} unfolding case studies ({len(ids)} questions) from mixed body "
                            f"systems, at the NCLEX-RN pace: {minutes // 60} h {minutes % 60:02d} min, one 10-minute "
                            "break, no going back. Answers, rationales and your clinical judgment results at the end."),
            "mode": "challenge", "revealMode": "exam", "timed": True, "timeBudgetSec": budget,
            "allowBackNav": False, "pausePolicy": BLOCK_PAUSE_POLICY, "outcomeHidden": True, "accessTier": "free",
            "questionIds": ids})
    return sets


def _case_titles():
    """Case study id -> title, from every built bank file."""
    here = Path(__file__).resolve().parent
    return {c["id"]: c["title"] for f in here.glob("rn-bank-*.rn.json")
            for c in json.loads(f.read_text(encoding="utf-8")).get("caseStudies", [])}


def _matches(q, tags, keywords, both):
    tagged = any(t in tags for t in q["subtopics"])
    worded = bool(keywords and re.search(keywords, q["stem"], re.I))
    return (tagged and worded) if both else (tagged or worded)


def _topic_sets(questions, cases):
    """One untimed topic test per review-book subtopic, split into parts of up to TOPIC_PART_SIZE."""
    titles = _case_titles()
    by_title = {titles[cid]: [q["id"] for q in sorted(items, key=lambda q: q["casePosition"])]
                for cid, items in cases.items() if cid in titles}
    standalone = [q for q in questions if not q.get("caseStudyId")]
    sets = []
    for index, (section, subtopic, case_titles) in enumerate(book_topics.BOOK):
        tags, keywords, both = book_topics.match_rule(section, subtopic)
        units = [by_title[t] for t in case_titles if t in by_title]
        units += [[q["id"]] for q in standalone if _matches(q, tags, keywords, both)]
        # Even parts: as many as TOPIC_PART_SIZE needs, each about the same size, so no part is tiny.
        size = sum(len(u) for u in units)
        n_parts = max(1, -(-size // TOPIC_PART_SIZE))
        target = -(-size // n_parts)
        parts, current = [], []
        for unit in units:
            if current and len(current) + len(unit) > target:
                parts.append(current)
                current = []
            current = current + unit
        if current:
            parts.append(current)
        if len(parts) >= TOPIC_STRIDE:
            raise SystemExit(f"topic {subtopic!r} needs more than {TOPIC_STRIDE - 1} parts; raise TOPIC_STRIDE carefully")
        total = sum(len(p) for p in parts)
        n_cases = sum(1 for t in case_titles if t in by_title)
        for k, part in enumerate(parts, 1):
            suffix = f", part {k} of {len(parts)}" if len(parts) > 1 else ""
            sets.append({
                "id": _set_id(TOPIC_BASE + index * TOPIC_STRIDE + k),
                "title": f"{subtopic}{suffix}",
                "description": (f"{len(part)} questions on {subtopic.lower()} ({n_cases} case studies and "
                                f"{total - n_cases * 6} single questions in all). Untimed; answers, rationales and your "
                                "result at the end."),
                "mode": "practice", "revealMode": "exam", "timed": False, "allowBackNav": True,
                "accessTier": "free", "topicSection": section, "questionIds": part,
                "isPublished": total >= TOPIC_MIN_QUESTIONS})
    return sets + _dropped_topic_parts(sets)


def _dropped_topic_parts(sets):
    """Unpublished copies of earlier topic parts that the current split no longer produces, and an updated record."""
    shipped = json.loads(TOPIC_SETS_FILE.read_text(encoding="utf-8")) if TOPIC_SETS_FILE.exists() else {}
    current = {s["id"] for s in sets}
    dropped = [{**old, "isPublished": False, "title": f"{old['title']} (retired part)",
                "description": "A topic part that no longer exists after the topic was re-split. Kept unpublished "
                               "so past attempts keep their history."}
               for set_id, old in sorted(shipped.items()) if set_id not in current]
    for s in sets:
        shipped[s["id"]] = s
    TOPIC_SETS_FILE.write_text(json.dumps(dict(sorted(shipped.items())), indent=1) + "\n", encoding="utf-8")
    return dropped


def attach(path, questions):
    """Writes the catalog into the import file at path (the last batch file)."""
    banks, sets = build_catalog(questions)
    data = json.loads(path.read_text(encoding="utf-8"))
    data["banks"], data["testSets"] = banks, sets
    data["notice"] += " Also carries the bank-wide catalog (catalog.py): the RN Question Bank and every curated test set."
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return banks, sets
