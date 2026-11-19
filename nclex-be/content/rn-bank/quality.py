"""Quality gates for the RN bank: catch questions a test-wise candidate can answer without nursing knowledge.

The real NCLEX-RN is written so that every option is plausible and nothing but clinical judgement points
to the key. A bank whose key is usually the longest option, or whose wrong options are the only ones
that say "always", lets a weak candidate score well here and still fail the real exam (it happened: see
REVIEW-LOG.md, 19 November 2026). These checks measure those tells.

- report(): one row per file for QUALITY.md, for every batch.
- gate(): the hard rules a batch marked HARDENED must pass, or the build stops.
- duplicates(): near-identical stems across the whole bank.
"""
import re
import statistics

# Words that test-taking courses teach candidates to distrust in an option.
ABSOLUTE = re.compile(r"\b(always|never|all|none|only|every|must)\b", re.I)
# Option openings that "assess first" test-taking advice picks.
ASSESS = re.compile(r"^(assess|check|monitor|obtain|evaluate|ask|auscultate|measure|review|reassess|palpate|inspect)\b", re.I)
STOP = set("""a an the and or but of to in on at for with by from as is are was were be been being this that these
those it its into than then their there which who whom what when where how why does do did has have had not
no can could should would will shall may might nurse client clients patient nurse's client's""".split())

# Limits for hardened batches (see the hardening plan in docs/features/content-hardening-01.md).
MAX_KEY_LONGEST_SHARE = 0.30      # key is the single longest option; chance with 4 options is 0.25
MAX_KEY_LENGTH_RATIO = 1.20       # key length / longest wrong option, per question
MAX_SELECT_LENGTH_RATIO = 1.25    # SATA/select N: mean correct length / mean wrong length
MAX_TRICK_SCORE = 0.35            # best test-wise trick on a batch's multiple-choice questions
MAX_ABSOLUTE_ONLY_WRONG = 0.15    # share of MC where only wrong options use absolute words
# Words of clinical picture (stem plus answer choices: in "which client first" items the data is in the
# options, as on the real exam) for a standalone item without exhibits.
MIN_SCENARIO_WORDS = {"moderate": 45, "difficult": 60}
MAX_EASY_SHARE = 0.15
MIN_DIFFICULT_SHARE = 0.30
DUPLICATE_SIMILARITY = 0.85


def _words(text):
    return re.findall(r"[a-z0-9]+", text.lower())


def _content_words(text):
    return {w for w in _words(text) if len(w) >= 4 and w not in STOP}


def _mc(q):
    """(options, key id) for a multiple-choice question."""
    return q["content"]["options"], q["key"]["correct"]


def _share(scores, key):
    """Expected accuracy of a trick that splits its pick evenly among the options it scores highest."""
    top = max(scores.values())
    picks = [i for i, s in scores.items() if s == top]
    return (1.0 / len(picks)) if key in picks else 0.0


def tricks(q):
    """How well each test-taking trick answers one multiple-choice question (0 to 1, ties split)."""
    options, key = _mc(q)
    length = {o["id"]: len(o["text"]) for o in options}
    stem = _content_words(q["stem"])
    overlap = {o["id"]: len(stem & _content_words(o["text"])) for o in options}
    no_absolute = {o["id"]: (0 if ABSOLUTE.search(o["text"]) else 1) * 1000 + length[o["id"]] for o in options}
    assess = {o["id"]: 1 if ASSESS.search(o["text"].lstrip("\"'")) else 0 for o in options}
    return {
        "longest": _share(length, key),
        # Writers who trim keys to beat "longest" can make the key the shortest instead.
        "shortest": _share({i: -n for i, n in length.items()}, key),
        "stem words": _share(overlap, key),
        "skip absolutes": _share(no_absolute, key),
        "assess first": _share(assess, key),
    }


def _key_strictly_longest(q):
    options, key = _mc(q)
    lengths = sorted((len(o["text"]), o["id"]) for o in options)
    return lengths[-1][1] == key and lengths[-1][0] > lengths[-2][0]


def _absolute_only_wrong(q):
    options, key = _mc(q)
    wrong = any(ABSOLUTE.search(o["text"]) for o in options if o["id"] != key)
    right = any(ABSOLUTE.search(o["text"]) for o in options if o["id"] == key)
    return wrong and not right


def _stem_words(q):
    return len(_words(q["stem"]))


def _scenario_words(q):
    c = q["content"]
    texts = [o["text"] for o in c.get("options", [])] + [r["text"] for r in c.get("rows", []) if "text" in r]
    texts += [i["text"] for i in c.get("items", [])]
    texts += [o["text"] for g in c.get("groups", []) for o in g["options"]]
    texts += [cell if isinstance(cell, str) else cell.get("text", "") for r in c.get("rows", []) for cell in r.get("cells", [])]
    texts += [seg["text"] for seg in c.get("segments", [])]
    texts += [c["template"]] if "template" in c else []
    texts += [o["text"] for b in c.get("blanks", []) for o in b["options"]]
    return len(_words(q["stem"])) + sum(len(_words(t)) for t in texts)


def _needs_long_stem(q):
    # Case-study items carry their data in the case tabs, and exhibits carry it beside the stem.
    return "caseStudyId" not in q and not q.get("exhibits")


def summary(questions):
    """The numbers QUALITY.md shows for a group of questions."""
    mc = [q for q in questions if q["itemType"] == "multiple_choice"]
    trick_scores = {}
    for q in mc:
        for name, s in tricks(q).items():
            trick_scores[name] = trick_scores.get(name, 0) + s
    # No "always pick C" trick: the player shuffles options per attempt (SeededShuffle), so the key's
    # position in the file tells a candidate nothing.
    best = max(trick_scores.items(), key=lambda kv: kv[1]) if mc else ("-", 0)
    tiers = {t: sum(q["difficulty"] == t for q in questions) for t in ("easy", "moderate", "difficult")}
    return {
        "questions": len(questions),
        "mc": len(mc),
        "key_longest": (sum(_key_strictly_longest(q) for q in mc) / len(mc)) if mc else 0.0,
        "best_trick": best[0],
        "best_trick_score": (best[1] / len(mc)) if mc else 0.0,
        "absolute_only_wrong": (sum(_absolute_only_wrong(q) for q in mc) / len(mc)) if mc else 0.0,
        "median_stem": statistics.median(_stem_words(q) for q in questions) if questions else 0,
        "tiers": tiers,
    }


def gate(questions):
    """Problems that stop the build for a HARDENED batch; an empty list means it passes."""
    errors = []
    for q in questions:
        where = f'{q["id"]} ({q["stem"][:50]}...)'
        t = q["itemType"]
        if t == "multiple_choice":
            options, key = _mc(q)
            key_len = next(len(o["text"]) for o in options if o["id"] == key)
            longest_wrong = max(len(o["text"]) for o in options if o["id"] != key)
            # Short options (single terms) differ by a few letters; only a real gap is a tell.
            if key_len > MAX_KEY_LENGTH_RATIO * longest_wrong and key_len - longest_wrong >= 10:
                errors.append(f"{where}: key is {key_len / longest_wrong:.2f}x the longest wrong option")
        if t in ("sata", "select_n"):
            right = [len(o["text"]) for o in q["content"]["options"] if o["id"] in q["key"]["correct"]]
            wrong = [len(o["text"]) for o in q["content"]["options"] if o["id"] not in q["key"]["correct"]]
            if wrong and statistics.mean(right) > MAX_SELECT_LENGTH_RATIO * statistics.mean(wrong):
                errors.append(f"{where}: correct options are much longer than wrong ones")
        minimum = MIN_SCENARIO_WORDS.get(q["difficulty"])
        if minimum and _needs_long_stem(q) and _scenario_words(q) < minimum:
            errors.append(f"{where}: {q['difficulty']} item has {_scenario_words(q)} words of stem and options, "
                          f"needs {minimum} (give the clinical picture, or add an exhibit)")

    s = summary(questions)
    if s["mc"] >= 8:
        if s["key_longest"] > MAX_KEY_LONGEST_SHARE:
            errors.append(f"key is the longest option in {s['key_longest']:.0%} of MC (max {MAX_KEY_LONGEST_SHARE:.0%})")
        if s["best_trick_score"] > MAX_TRICK_SCORE:
            errors.append(f"trick '{s['best_trick']}' scores {s['best_trick_score']:.0%} on MC (max {MAX_TRICK_SCORE:.0%})")
        if s["absolute_only_wrong"] > MAX_ABSOLUTE_ONLY_WRONG:
            errors.append(f"only wrong options use absolute words in {s['absolute_only_wrong']:.0%} of MC "
                          f"(max {MAX_ABSOLUTE_ONLY_WRONG:.0%})")
    n = s["questions"]
    if s["tiers"]["easy"] > MAX_EASY_SHARE * n:
        errors.append(f"{s['tiers']['easy']} easy of {n} (max {MAX_EASY_SHARE:.0%})")
    if s["tiers"]["difficult"] < MIN_DIFFICULT_SHARE * n:
        errors.append(f"{s['tiers']['difficult']} difficult of {n} (min {MIN_DIFFICULT_SHARE:.0%})")
    counts = [len(q["key"]["correct"]) for q in questions if q["itemType"] == "sata"]
    if len(counts) >= 3 and len(set(counts)) == 1:
        errors.append(f"every SATA has {counts[0]} correct options; vary the number")
    return errors


def duplicates(questions, hardened_ids):
    """Pairs of near-identical stems where at least one question is hardened."""
    sets = [(q["id"], set(_words(q["stem"]))) for q in questions]
    found = []
    for i, (a, wa) in enumerate(sets):
        for b, wb in sets[i + 1:]:
            if a not in hardened_ids and b not in hardened_ids:
                continue
            union = len(wa | wb)
            if union and len(wa & wb) / union >= DUPLICATE_SIMILARITY:
                found.append((a, b))
    return found


def markdown(rows, total):
    """QUALITY.md: rows are (file name, hardened, summary)."""
    def line(name, hardened, s):
        t = s["tiers"]
        return (f"| {name} | {s['questions']} | {'yes' if hardened else ''} | {s['mc']} | {s['key_longest']:.0%} | "
                f"{s['best_trick_score']:.0%} ({s['best_trick']}) | {s['absolute_only_wrong']:.0%} | "
                f"{s['median_stem']:g} | {t['easy']}/{t['moderate']}/{t['difficult']} |")
    out = [
        "# RN bank quality report",
        "",
        "Written by `python3 -I build_all.py`; do not edit by hand. See `quality.py` for the checks.",
        "",
        "- **Key longest:** share of multiple-choice questions whose key is the single longest option. Chance is about 25%.",
        "- **Best trick:** the best score a test-wise candidate gets on the multiple-choice questions without any nursing",
        "  knowledge (pick the longest or the shortest option, the one repeating the stem's words, avoid absolute words, or pick \"assess\").",
        "  Chance is about 25%; hardened files must stay at or below 35%.",
        "- **Absolutes:** share of multiple-choice questions where only wrong options say always, never, all, only and so on.",
        "- **Difficulty:** easy / moderate / difficult.",
        "",
        "| File | Questions | Hardened | MC | Key longest | Best trick | Absolutes | Median stem words | Difficulty |",
        "|---|---|---|---|---|---|---|---|---|",
    ]
    out += [line(*r) for r in rows]
    out.append(line("**Whole bank**", False, total).replace("|  |", "| |", 1))
    return "\n".join(out) + "\n"
