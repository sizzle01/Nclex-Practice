"""Progress of the RN bank toward 5,000 questions spread like the NCLEX-RN test plan (PROGRESS.md).

The bank grows in chunks of about 100 questions (4 batch files built with build(..., wave=True)), five
chunks to a wave. Each chunk follows the gap that is left in every Client Needs category, so the bank gets
closer to the test plan with every chunk and is usable if the growth stops part way.

build_all.py calls write() after the build; PROGRESS.md is generated, do not edit it by hand.
"""
import collections

import book_topics

from catalog import CATEGORY_NAMES, TEST_PLAN_MIDPOINTS

TARGET_TOTAL = 5000
CHUNK_SIZE = 100
CHUNKS_PER_WAVE = 5
# Which batch numbers make up each chunk; add a line when a chunk is committed.
CHUNKS = {
    1: (68, 69, 70, 71),
    2: (72, 73, 74, 75),
    3: (76, 77, 78, 79),
    4: (80, 81, 82, 83),
    5: (84, 85, 86, 87),
    6: (88, 89, 90, 91),
    7: (92, 93, 94, 95),
    8: (96, 97, 98, 99),
}
# Case conversion (CASE-MAP.md, plan of 2026-11-20): the bank is rebuilt around full NGN case studies, topic by
# topic, and the wave chunks above are paused. Add a line when a conversion chunk is committed.
CONVERSION_CHUNKS = {
    1: ("Cardiac, pacemaker, hemodynamic monitoring and shock", (100, 101, 102, 103)),
    2: ("Pulmonary disorders with respiratory pharmacology", (104, 105, 106, 107)),
    3: ("Endocrine disorders with insulin, antidiabetic, thyroid and corticosteroid drugs", (108, 109, 110, 111)),
    4: ("Gastrointestinal disorders with PUD, GERD and antiemetic drugs", (112, 113, 114, 115)),
    5: ("Neurologic disorders, head-to-toe and cranial nerve assessment, neuro and cholinergic drugs", (116, 117, 118, 119)),
    6: ("Renal, hematologic and oncologic disorders with hematopoietic drugs and thrombolytics", (120, 121, 122, 123)),
    7: ("Immunity, infection, STI, musculoskeletal and trauma with antimicrobial, antifungal and antiparasitic drugs",
        (124, 125, 126, 127)),
    8: ("Mental health disorders with psychiatric medications", (128, 129, 130, 131)),
    9: ("Mental health part 2: abuse and violence, perinatal mood, dementia, alcohol, ECT, grief and remaining drugs",
        (132, 133, 134)),
    10: ("Maternity part 1: bleeding, diabetes, Rh, labor and fetal monitoring, prenatal care, newborn and postpartum",
         (135, 136, 137, 138)),
    11: ("Maternity part 2: hyperemesis, PPROM, HELLP, screening, intrapartum emergencies, newborn and postpartum",
         (139, 140, 141, 142)),
    12: ("Pediatrics part 1: cardiac, renal, neurologic and blood disorders", (143, 144, 145, 146)),
}
CONVERSION_NEXT = ("Pediatrics part 2: growth and development by age, respiratory and GI disorders and the vaccination "
                   "schedule (CASE-MAP.md, book_topics.py)")
CASE_SHARE_GOAL = 85    # percent of the bank inside full case studies

# Item-type mix for a chunk of new questions (plan of 2026-11-20): fewer plain multiple-choice questions
# than the first 1,110, and the newer NGN formats the real exam uses.
CHUNK_TYPE_MIX = [
    ("multiple choice", 55),
    ("multiple response, matrix, cloze, ordering, calculation", 23),
    ("bowtie", 6),
    ("highlight", 4),
    ("case-study items (2 cases of 6)", 12),
]


def targets():
    return {cn: TARGET_TOTAL * pct // 100 for cn, pct in TEST_PLAN_MIDPOINTS.items()}


def next_chunk(have):
    """Questions per category for the next chunk: the remaining gap, scaled to CHUNK_SIZE (largest remainder)."""
    gap = {cn: max(0, t - have.get(cn, 0)) for cn, t in targets().items()}
    left = sum(gap.values())
    if left == 0:
        return {cn: 0 for cn in gap}
    size = min(CHUNK_SIZE, left)
    raw = {cn: size * g / left for cn, g in gap.items()}
    counts = {cn: int(v) for cn, v in raw.items()}
    for cn in sorted(raw, key=lambda c: raw[c] - counts[c], reverse=True)[: size - sum(counts.values())]:
        counts[cn] += 1
    return counts


def _batch(qid):
    return int(qid[3:8])


def markdown(questions):
    have = collections.Counter(q["clientNeeds"] for q in questions)
    types = collections.Counter(q["itemType"] for q in questions)
    cases = {q["caseStudyId"] for q in questions if q.get("caseStudyId")}
    total = len(questions)
    goal = targets()
    nxt = next_chunk(have)
    by_batch = collections.Counter(_batch(q["id"]) for q in questions)

    out = [
        "# RN bank progress toward 5,000 questions",
        "",
        "Written by `python3 -I build_all.py` (coverage.py); do not edit by hand.",
        "",
        f"**{total} of {TARGET_TOTAL} questions** ({100 * total / TARGET_TOTAL:.0f}%), {len(cases)} case studies, "
        f"{types['bowtie']} bowtie questions. Multiple choice is {100 * types['multiple_choice'] / total:.0f}% of the bank.",
        "",
        "## By test plan category",
        "",
        "| Category | Test plan | Target | Have | To write | Next chunk |",
        "|---|---|---|---|---|---|",
    ]
    for cn, name in CATEGORY_NAMES.items():
        out.append(f"| {name} | {TEST_PLAN_MIDPOINTS[cn]}% | {goal[cn]} | {have[cn]} | "
                   f"{max(0, goal[cn] - have[cn])} | {nxt[cn]} |")
    out.append(f"| **Total** | 100% | {TARGET_TOTAL} | {total} | "
               f"{sum(max(0, goal[c] - have[c]) for c in goal)} | {sum(nxt.values())} |")

    out += ["", "## Chunks", "", "| Chunk | Wave | Batch files | Questions |", "|---|---|---|---|"]
    for n, batches in CHUNKS.items():
        count = sum(by_batch[b] for b in batches)
        files = ", ".join(f"b{b}" for b in batches)
        out.append(f"| {n} | {(n - 1) // CHUNKS_PER_WAVE + 1} | {files} | {count} |")
    out += ["", "The wave chunks are paused (2026-11-20) while the bank is rebuilt around case studies."]

    in_cases = sum(1 for q in questions if q.get("caseStudyId"))
    retired = sum(1 for q in questions if q.get("status") == "retired")
    active = total - retired
    out += ["", "## Case conversion (CASE-MAP.md)", "",
            f"**{in_cases} of {active} active questions ({100 * in_cases / active:.0f}%) are inside full case "
            f"studies**; the goal is {CASE_SHARE_GOAL}%. {retired} older questions are retired (F26, "
            "retired-questions.json): their topics were rebuilt as case studies.", "",
            "| Chunk | Topic | Batch files | Questions | Case studies |", "|---|---|---|---|---|"]
    case_batch = collections.Counter(_batch(q["id"]) for q in questions if q.get("caseStudyId"))
    for n, (topic, batches) in CONVERSION_CHUNKS.items():
        count = sum(by_batch[b] for b in batches)
        n_cases = sum(case_batch[b] for b in batches) // 6
        out.append(f"| {n} | {topic} | {', '.join(f'b{b}' for b in batches)} | {count} | {n_cases} |")
    out += ["", f"**Next: conversion chunk {max(CONVERSION_CHUNKS) + 1}**: {CONVERSION_NEXT}."]

    out += book_topics.markdown()

    out += ["", "## Item types in the whole bank", "", "| Type | Questions | Share |", "|---|---|---|"]
    for t, n in types.most_common():
        out.append(f"| {t} | {n} | {100 * n / total:.0f}% |")
    return "\n".join(out) + "\n"


def write(path, questions):
    path.write_text(markdown(questions), encoding="utf-8")
