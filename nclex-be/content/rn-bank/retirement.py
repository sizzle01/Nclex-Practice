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
    6: ("Renal, hematologic and oncologic", ("renal-urinary", "hematologic-oncologic"),
        r"dialysis|fistula|transfusion|anemia|epoetin|filgrastim|neutropen|"
        r"chemotherap|platelet|thrombocytop|leukemia|lymphoma|tumor lysis|vesicant|extravasat|\biron\b|cyanocobalamin"),
    7: ("Immunity, infection, musculoskeletal and trauma", ("immune", "musculoskeletal"),
        r"\bhiv\b|antiretrovir|lupus|rheumatoid|methotrexate|adalimumab|fluconazole|amphotericin|antifungal|"
        r"malaria|syphilis|gonorrh|chlamydia|pelvic inflammatory|fracture|traction|amputat|gout|allopurinol|"
        r"colchicine|rhabdomyol|heat stroke|hypotherm|triage"),
    8: ("Mental health", "mental-health",
        r"lithium|haloperidol|clozapine|neuroleptic malignant|serotonin syndrome|phenelzine|maoi|sertraline|"
        r"fluoxetine|methylphenidate|buprenorphine|naloxone|benzodiazepine|lorazepam|anorexia|bulimia"),
    9: ("Mental health part 2", "mental-health",
        r"donepezil|memantine|disulfiram|naltrexone|acamprosate|thiamine|varenicline|buspirone|valpro|lamotrigine|"
        r"olanzapine|tardive|electroconvulsive|\bect\b"),
    10: ("Maternity and newborn", "maternal-newborn",
         r"pregnan|prenatal|postpartum|newborn|neonat|\blabor\b|oxytocin|magnesium sulfate|rho\(d\)|placenta|fundal|"
         r"breastfe|apgar|lochia"),
    11: ("Maternity and newborn part 2", "maternal-newborn",
         r"pregnan|prenatal|postpartum|newborn|neonat|\blabor\b|oxytocin|magnesium sulfate|rho\(d\)|placenta|fundal|"
         r"breastfe|apgar|lochia"),
    12: ("Pediatrics part 1: cardiac, renal, neurologic and blood disorders", "pediatrics", r"(?!)"),
    13: ("Pediatrics part 2: growth and development, respiratory, GI and vaccines", "pediatrics", r"(?!)"),
}
# Pediatric chunks retire only the subtopics their cases teach (growth, respiratory, GI and vaccines come later).
CHUNK_SUBTOPICS = {
    12: {"pediatric-cardiac-disorders", "pediatric-gi-gu-disorders", "pediatric-neurological-disorders",
         "pediatric-hematology-oncology", "infant-dehydration", "poisoning-and-ingestions", "sickle-cell-disease"},
    13: {"infant-growth-and-development", "adolescent-health", "immunizations", "infant-and-child-nutrition",
         "pediatric-respiratory-disorders", "pediatric-gi-gu-disorders", "pediatric-care-basics",
         "childhood-communicable-diseases", "pediatric-neurological-disorders", "poisoning-and-ingestions",
         "infant-dehydration"},
}
# Pediatric GI topics the chunk 12 cases do not teach yet (kept for chunk 13).
KEEP_STEMS_PEDS_GI = re.compile(r"pyloric|intussuscep|hirschsprung|celiac|cleft|appendic|gastroschisis|omphalocele|"
                                r"esophageal atresia|tracheoesophageal|reflux|constipation|encopresis|hernia|"
                                r"lead|acetaminophen|salicylate|button batter|hydrocarbon|vomited forcefully|"
                                r"knees drawn up|not passed meconium|hypospadias|moved from another country|"
                                r"3-week-old|6-month-old is brought in with a temperature", re.I)
LATER_CHUNKS = {"pediatrics"}
CLINICAL = {"PHI.PA", "PHI.PPT", "PHI.RRP", "PHI.BCC", "HPM"}
# Topics the converted cases do not teach, even when a question carries a converted body system (for example, eye
# surgery is tagged neurological). These questions are kept until a case covers them.
KEEP_TAGS = {"eye-and-ear-disorders", "sensory-impairment-care", "urinary-elimination-and-catheter-care",
             "bowel-elimination", "hygiene-and-personal-care",
             # chunk 6 cases do not teach these renal and cancer topics yet
             "urinary-tract-and-prostate-disorders", "adverse-effects-of-medications", "cancer-care",
             "cancer-screening", "sickle-cell-disease", "palliative-and-end-of-life-care",
             "pain-assessment-and-comfort", "specimen-collection", "medication-administration-technique",
             # chunk 7 cases do not teach these
             "mobility-and-assistive-devices", "joint-replacement-care", "immunizations", "aging-and-older-adult-health",
             "immobility-and-positioning", "postoperative-care", "opioid-safety", "bone-health-and-osteoporosis",
             "antimicrobial-therapy"}
# Single questions on topics no case covers yet, kept whatever their tags: superior vena cava syndrome (chunk 6);
# children (pediatric chunks), carpal tunnel, Lyme disease, HIV PrEP, osteoarthritis and Buck's traction (chunk 7).
KEEP_STEMS = re.compile(r"superior vena cava|collar of the shirt|\bchild\b|\bparent\b|carpal|lyme|"
                        r"pre-exposure|\bprep\b|osteoarthritis|buck's|16-year-olds who recently", re.I)
# Chunk 10 (maternity part 1) cases do not yet teach these; maternity part 2 will.
KEEP_STEMS_MATERNITY = re.compile(
    r"vomited several times a day|amniocentesis|alpha-fetoprotein|gush of clear fluid 1 hour|right ribs|"
    r"cramp in the calf|feel faint|\(bmi|drank wine|plan to (try to )?become pregnant|previous pregnancy affected|"
    r"definitely real|four prenatal clients|swollen, hard, warm breasts|chosen not to breastfeed|umbilical cord at home|"
    r"safe infant sleep|head-to-toe assessment of a 12-hour|36 weeks by cesarean|thick and dark green|head delivers|"
    r"previous cesarean|something is wrong|back pain between contractions|alone in the room|leopold|38\.4|rubella|"
    r"forceps|i can't do this anymore|irritable, nauseated and shaking|not voided|terbutaline|four postpartum clients|"
    r"come to the hospital|calls the birthing unit|which stage of labor|gradual|1-hour 50 g|preeclampsia and mild "
    r"intermittent|stillbirth|took the baby's movements|track of the baby's movements", re.I)
# Chunk 11 (maternity part 2) teaches the chunk 10 list except true versus false labor.
KEEP_STEMS_MATERNITY_2 = re.compile(r"come to the hospital|calls the birthing unit", re.I)
# Chunk 8 kept alcohol use drugs, varenicline, SSRI discontinuation, buspirone, valproate, lamotrigine, olanzapine
# and tardive dyskinesia questions; chunk 9 cases teach them, so they are no longer kept.
KEEP_MOC = re.compile(r"\b(first|priorit\w*|delegat\w*|assign\w*)\b", re.I)


def _batch(question_id):
    return int(question_id[3:8])


def _stem(q):
    return q["stem"].lower()


def _topic_of(q, chunks):
    """The first applied chunk whose topic the question belongs to, or None."""
    for chunk in chunks:
        topic, system, keywords = CHUNK_TOPICS[chunk]
        systems = (system,) if isinstance(system, str) else system
        if q["bodySystem"] in systems:
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
        if _batch(q["id"]) >= 100 or q.get("caseStudyId"):
            continue
        if q["bodySystem"] in LATER_CHUNKS and not any(c in CHUNK_SUBTOPICS for c in chunks):
            continue
        allowed = set().union(*(CHUNK_SUBTOPICS.get(c, set()) for c in chunks))
        if q["bodySystem"] == "pediatrics" and not (allowed & set(q["subtopics"])):
            continue
        if 12 in chunks and q["bodySystem"] == "pediatrics" and KEEP_STEMS_PEDS_GI.search(q["stem"]):
            continue
        if q["clientNeeds"] not in CLINICAL:
            continue
        if KEEP_TAGS & set(q["subtopics"]) or KEEP_STEMS.search(q["stem"]):
            continue
        if 11 in chunks and q["bodySystem"] == "maternal-newborn" and KEEP_STEMS_MATERNITY_2.search(q["stem"]):
            continue
        if 10 in chunks and q["bodySystem"] == "maternal-newborn" and KEEP_STEMS_MATERNITY.search(q["stem"]):
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
