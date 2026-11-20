"""Builds every batch in batches/ (in batch-number order) and checks ids are unique across the whole bank.

    cd nclex-be/content/rn-bank && python3 -I build_all.py
"""
import collections
import importlib.util
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import catalog  # noqa: E402
import coverage  # noqa: E402
import qb  # noqa: E402
import quality  # noqa: E402

all_ids, by_cn, by_type, total = set(), collections.Counter(), collections.Counter(), 0
all_questions = []
per_file = []
def batch_number(path):
    """b7_x.py -> 7. Number order, not name order, so b100 comes after b99 (and the catalog lands in
    the highest-numbered file, which deploy.sh imports last)."""
    return int(path.stem[1:].split("_", 1)[0])


for path in sorted((HERE / "batches").glob("b*.py"), key=batch_number):
    # Dead conditional expressions hide which tag a question really has; keep batch files plain.
    if " if False" in path.read_text(encoding="utf-8"):
        raise SystemExit(f"{path.name}: remove the 'if False' expression")
    spec = importlib.util.spec_from_file_location(path.stem, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    out, questions = module.BUILT
    dupes = all_ids & {q["id"] for q in questions}
    if dupes:
        raise SystemExit(f"{path.name}: ids already used by another batch: {sorted(dupes)[:3]}")
    all_ids |= {q["id"] for q in questions}
    all_questions += questions
    per_file.append((out.name, questions))
    for q in questions:
        by_cn[q["clientNeeds"]] += 1
        by_type[q["itemType"]] += 1
    total += len(questions)
    print(f"{out.name}: {len(questions)} questions")

# Quality report (QUALITY.md) for every file, and near-duplicate stems involving a hardened question.
hardened_ids = {q["id"] for name, qs in per_file if name in qb.HARDENED_FILES for q in qs}
dupes = quality.duplicates(all_questions, hardened_ids)
if dupes:
    raise SystemExit("Near-duplicate stems (rewrite one of each pair):\n  " + "\n  ".join(f"{a} ~ {b}" for a, b in dupes))
rows = [(name.removeprefix("rn-bank-").removesuffix(".rn.json"), name in qb.HARDENED_FILES, quality.summary(qs))
        for name, qs in per_file]
(HERE / "QUALITY.md").write_text(quality.markdown(rows, quality.summary(all_questions)), encoding="utf-8")
whole = quality.summary(all_questions)
print(f"Quality: {len(qb.HARDENED_FILES)} hardened file(s); whole bank key longest {whole['key_longest']:.0%}, "
      f"best trick {whole['best_trick_score']:.0%} ({whole['best_trick']}). See QUALITY.md.")

# The catalog (test sets and the admin bank) lists questions from every batch, so it goes into the
# last file, which is imported after all the others (see catalog.py).
banks, sets = catalog.attach(out, all_questions)
print(f"{out.name}: + catalog with {len(sets)} test sets and {len(banks)} bank")

coverage.write(HERE / "PROGRESS.md", all_questions)
print(f"\nTotal: {total} questions (progress toward {coverage.TARGET_TOTAL}: PROGRESS.md)")
for cn, count in sorted(by_cn.items()):
    print(f"  {cn:9} {count:4}  {100 * count / total:5.1f}%")
print("Item types:", dict(by_type))
