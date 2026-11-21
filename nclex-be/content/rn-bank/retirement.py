"""Chooses which old isolated questions to retire once their topic is rebuilt as full case studies (F26).

    cd nclex-be/content/rn-bank && python3 -I retirement.py 1 2 3 4    # conversion chunks to apply

The case-study conversion (CASE-MAP.md) writes new six-step cases topic by topic. Each converted topic's old
standalone questions are then retired, so candidates practice the reasoning chain instead of isolated recall
(the candidate's NCLEX report, 2026-11-20). Rules:
- Only questions written before the conversion (batches below 100) and not already inside a case study.
- Only clinical questions (Physiological Integrity and Health Promotion). Management of care, safety and
  psychosocial questions are kept in every topic: the cases do not replace them, and they are areas the
  candidate needs.
- A question belongs to a chunk's topic by body system, or, for questions tagged "fundamentals" or
  "pharmacology", by keywords in its stem (options alone are not enough).
- Pediatric, maternal-newborn and mental health questions wait for their own chunks.
- Kept for the 15% standalone share: bowtie items, and management of care items about priority or delegation.

The result is appended to retired-questions.json, which build_all.py reads. Entries are never removed: once shipped,
a retirement cannot be undone through the importer. A spot-check list is written to review/retirement-review.csv.
"""
import collections
import csv
import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
REGISTRY = HERE / "retired-questions.json"
REVIEW = HERE / "review" / "retirement-review.csv"

# Conversion chunk -> (topic, body system, keywords for questions tagged fundamentals or pharmacology).
CHUNK_TOPICS = {
    1: ("Cardiac", "cardiovascular",
        r"myocard|angina|heart failure|atrial fib|digoxin|warfarin|heparin|hypertens|nitroglyc|amiodarone|anticoag|"
        r"antiplatelet|thromboly|statin|beta.?blocker|pacemaker|dysrhythm|cardiac|troponin|telemetry"),
    2: ("Pulmonary", "respiratory",
        r"asthma|copd|albuterol|inhaler|tubercul|isoniazid|pneumon|chest tube|ventilat|tracheost|oxygen therapy|"
        r"nebuliz|bronch|spirometr"),
    3: ("Endocrine", "endocrine",
        r"insulin|diabet|glucose|hypoglyc|thyroid|levothyrox|cortico|prednisone|adrenal|siadh|cushing|addison|"
        r"metformin|osteopor"),
    4: ("Gastrointestinal", "gastrointestinal",
        r"cirrho|pancrea|bowel|peptic|ulcer|gerd|proton pump|antiemetic|ondansetron|ostomy|nasogastric|tube feed|"
        r"hepat|lactulose|parenteral nutrition|colitis|crohn"),
    5: ("Neurologic", "neurological",
        r"stroke|seizure|intracranial|parkinson|multiple sclerosis|myasthenia|guillain|spinal cord|meningitis|"
        r"cranial nerve|glasgow|phenytoin|levetiracetam|carbamazepine|levodopa|head injury"),
}
LATER_CHUNKS = {"pediatrics", "maternal-newborn", "mental-health"}
CLINICAL = {"PHI.PA", "PHI.PPT", "PHI.RRP", "PHI.BCC", "HPM"}
# Topics the converted cases do not teach, even when a question carries a converted body system (for example, eye
# surgery is tagged neurological). These questions are kept until a case covers them.
KEEP_TAGS = {"eye-and-ear-disorders", "sensory-impairment-care", "urinary-elimination-and-catheter-care",
             "bowel-elimination", "hygiene-and-personal-care"}
KEEP_MOC = re.compile(r"\b(first|priorit\w*|delegat\w*|assign\w*)\b", re.I)


def _batch(question_id):
    return int(question_id[3:8])


def _stem(q):
    return q["stem"].lower()


def _topic_of(q, chunks):
    """The first applied chunk whose topic the question belongs to, or None."""
    for chunk in chunks:
        topic, system, keywords = CHUNK_TOPICS[chunk]
        if q["bodySystem"] == system:
            return chunk, topic
        if q["bodySystem"] in ("fundamentals", "pharmacology") and re.search(keywords, _stem(q)):
            return chunk, topic
    return None


def _kept(q):
    """Standalone formats the real exam uses outside cases, kept for the 15% standalone share."""
    if q["itemType"] == "bowtie":
        return "bowtie"
    if q["clientNeeds"] == "SECE.MOC" and KEEP_MOC.search(q["stem"]):
        return "priority or delegation"
    return None


def choose(questions, chunks):
    """(to_retire, kept) for the given conversion chunks: lists of (question, chunk, topic, reason)."""
    to_retire, kept = [], []
    for q in questions:
        if _batch(q["id"]) >= 100 or q.get("caseStudyId") or q["bodySystem"] in LATER_CHUNKS:
            continue
        if q["clientNeeds"] not in CLINICAL:
            continue
        if KEEP_TAGS & set(q["subtopics"]):
            continue
        match = _topic_of(q, chunks)
        if not match:
            continue
        chunk, topic = match
        reason = _kept(q)
        (kept if reason else to_retire).append((q, chunk, topic, reason or "replaced by case studies"))
    return to_retire, kept


def load_registry():
    return json.loads(REGISTRY.read_text(encoding="utf-8")) if REGISTRY.exists() else {}


def main(chunks):
    questions = [q for f in sorted(HERE.glob("rn-bank-*.rn.json")) for q in json.loads(f.read_text(encoding="utf-8"))["questions"]]
    registry = load_registry()
    to_retire, kept = choose(questions, chunks)
    for q, chunk, topic, _ in to_retire:
        registry.setdefault(q["id"], {"chunk": chunk, "topic": topic})
    REGISTRY.write_text(json.dumps(dict(sorted(registry.items())), indent=1) + "\n", encoding="utf-8")

    REVIEW.parent.mkdir(exist_ok=True)
    with REVIEW.open("w", newline="", encoding="utf-8") as out:
        writer = csv.writer(out)
        writer.writerow(["decision", "chunk", "topic", "id", "item type", "client needs", "body system", "stem"])
        for decision, rows in (("retire", to_retire), ("keep", kept)):
            for q, chunk, topic, reason in rows:
                writer.writerow([f"{decision} ({reason})", chunk, topic, q["id"], q["itemType"], q["clientNeeds"],
                                 q["bodySystem"], q["stem"][:160]])

    by_topic = collections.Counter(topic for _, _, topic, _ in to_retire)
    kept_by = collections.Counter(reason for _, _, _, reason in kept)
    print(f"Retire {len(to_retire)}: {dict(by_topic)}")
    print(f"Keep {len(kept)} as standalone: {dict(kept_by)}")
    print(f"Registry now lists {len(registry)} retired questions. Spot-check: {REVIEW.relative_to(HERE)}")


if __name__ == "__main__":
    main([int(a) for a in sys.argv[1:]] or sorted(CHUNK_TOPICS))
