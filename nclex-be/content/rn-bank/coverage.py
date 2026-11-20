"""Progress of the RN bank toward 5,000 questions spread like the NCLEX-RN test plan (PROGRESS.md).

The bank grows in chunks of about 100 questions (4 batch files built with build(..., wave=True)), five
chunks to a wave. Each chunk follows the gap that is left in every Client Needs category, so the bank gets
closer to the test plan with every chunk and is usable if the growth stops part way.

build_all.py calls write() after the build; PROGRESS.md is generated, do not edit it by hand.
"""
import collections

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
}
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
    next_n = max(CHUNKS) + 1 if CHUNKS else 1
    first = max(b for bs in CHUNKS.values() for b in bs) + 1 if CHUNKS else 68
    out += ["", f"**Next: chunk {next_n}** (wave {(next_n - 1) // CHUNKS_PER_WAVE + 1}), "
                f"batch files b{first}–b{first + 3}, with the category counts above and this item-type mix:", ""]
    out += [f"- {label}: about {n}" for label, n in CHUNK_TYPE_MIX]

    out += ["", "## Item types in the whole bank", "", "| Type | Questions | Share |", "|---|---|---|"]
    for t, n in types.most_common():
        out.append(f"| {t} | {n} | {100 * n / total:.0f}% |")
    return "\n".join(out) + "\n"


def write(path, questions):
    path.write_text(markdown(questions), encoding="utf-8")
