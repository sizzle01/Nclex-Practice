"""Builds every batch in batches/ (in name order) and checks ids are unique across the whole bank.

    cd nclex-be/content/rn-bank && python3 -I build_all.py
"""
import collections
import importlib.util
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import catalog  # noqa: E402
import qb  # noqa: E402

all_ids, by_cn, by_type, total = set(), collections.Counter(), collections.Counter(), 0
all_questions = []
for path in sorted((HERE / "batches").glob("b*.py")):
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
    for q in questions:
        by_cn[q["clientNeeds"]] += 1
        by_type[q["itemType"]] += 1
    total += len(questions)
    print(f"{out.name}: {len(questions)} questions")

# The catalog (test sets and the admin bank) lists questions from every batch, so it goes into the
# last file, which is imported after all the others (see catalog.py).
banks, sets = catalog.attach(out, all_questions)
print(f"{out.name}: + catalog with {len(sets)} test sets and {len(banks)} bank")

print(f"\nTotal: {total} questions")
for cn, count in sorted(by_cn.items()):
    print(f"  {cn:9} {count:4}  {100 * count / total:5.1f}%")
print("Item types:", dict(by_type))
