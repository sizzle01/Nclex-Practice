"""While rewriting a batch: shows each multiple-choice key's length against the wrong options' range,
and the batch's quality-gate problems, without stopping at the first failure.

    python3 -I lengths.py 02
"""
import importlib.util
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import quality  # noqa: E402

problems = []
real_gate = quality.gate
quality.gate = lambda questions: problems.extend(real_gate(questions)) or []
path = next((HERE / "batches").glob(f"b{sys.argv[1]}_*.py"))
spec = importlib.util.spec_from_file_location(path.stem, path)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
for n, q in enumerate(module.BUILT[1], 1):
    if q["itemType"] != "multiple_choice":
        continue
    key = q["key"]["correct"]
    lengths = {o["id"]: len(o["text"]) for o in q["content"]["options"]}
    others = [v for k, v in lengths.items() if k != key]
    flag = "KEY LONGEST" if lengths[key] > max(others) else "KEY SHORTEST" if lengths[key] < min(others) else ""
    print(f"{n:3} key {key} {lengths[key]:4}  wrong {min(others):4}-{max(others):4}  {flag}")
print("\n".join(problems) or "Passes the quality gates.")
