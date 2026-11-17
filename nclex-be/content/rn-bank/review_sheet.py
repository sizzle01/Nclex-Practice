"""Builds the clinical review sheet for every RN bank file, for the RN or nurse educator who signs off.

    cd nclex-be/content/rn-bank && python3 -I review_sheet.py

Writes review/rn-bank-review.csv (one row per question, opens in Excel or Google Sheets, with empty
columns for the reviewer's decision) and review/rn-bank-review.html (a printable packet with the
same content, grouped by batch). Only the standard library is used, so no install is needed.
"""
import collections
import csv
import html
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
OUT = HERE / "review"

CLIENT_NEEDS = {
    "SECE.MOC": "Management of Care", "SECE.SIC": "Safety and Infection Control",
    "HPM": "Health Promotion and Maintenance", "PSI": "Psychosocial Integrity",
    "PHI.BCC": "Basic Care and Comfort", "PHI.PPT": "Pharmacological and Parenteral Therapies",
    "PHI.RRP": "Reduction of Risk Potential", "PHI.PA": "Physiological Adaptation",
}
# NCSBN 2023 NCLEX-RN test plan ranges, shown next to the bank's own mix.
TEST_PLAN = {"SECE.MOC": "15-21%", "SECE.SIC": "10-16%", "HPM": "6-12%", "PSI": "6-12%",
             "PHI.BCC": "6-12%", "PHI.PPT": "13-19%", "PHI.RRP": "9-15%", "PHI.PA": "11-17%"}
REVIEW_COLUMNS = ["Decision (Approve / Revise / Reject)", "Clinical accuracy problem",
                  "Suggested change", "Reviewer", "Date"]


def answer_lines(q):
    """The item as a reviewer needs to see it: every choice, with the key marked and its rationale."""
    c, k, per, t = q["content"], q["key"], q["rationale"]["perOption"], q["itemType"]
    lines = []
    if t in ("multiple_choice", "sata", "select_n"):
        correct = {k["correct"]} if t == "multiple_choice" else set(k["correct"])
        if t == "select_n":
            lines.append(f"Select {c['selectCount']}.")
        for o in c["options"]:
            mark = "CORRECT" if o["id"] in correct else "wrong"
            lines.append(f"{o['id']}) {o['text']}  [{mark}]  - {per.get(o['id'], '')}")
    elif t == "ordered_response":
        text = {i["id"]: i["text"] for i in c["items"]}
        lines.append("Shown to the candidate: " + " | ".join(i["text"] for i in c["items"]))
        lines.append("Correct order:")
        for n, i in enumerate(k["order"], 1):
            lines.append(f"{n}. {text[i]}  - {per.get(i, '')}")
    elif t in ("matrix_mc", "matrix_mr"):
        cols = {col["id"]: col["text"] for col in c["columns"]}
        for r in c["rows"]:
            if t == "matrix_mc":
                right = [k["byRow"][r["id"]]]
            else:
                right = [cell.split(":")[1] for cell in k["byGroup"][r["id"]]]
            lines.append(f"{r['text']} -> {', '.join(cols[x] for x in right)}  - "
                         + " ".join(per.get(f"{r['id']}:{x}", "") for x in right))
    elif t in ("dropdown_cloze", "dropdown_table"):
        if t == "dropdown_cloze":
            lines.append("Sentence: " + c["template"])
        else:
            lines.append("Columns: " + " | ".join(c["columns"]))
            for r in c["rows"]:
                lines.append("Row: " + " | ".join(cell.get("text", "[" + cell.get("blankId", "") + "]")
                                                  for cell in r["cells"]))
        for b in c["blanks"]:
            right = k["byBlank"][b["id"]]
            for o in b["options"]:
                mark = "CORRECT" if o["id"] == right else "wrong"
                lines.append(f"{b['id']}: {o['text']}  [{mark}]  - {per.get(b['id'] + ':' + o['id'], '')}")
    elif t == "highlight_text":
        for s in c["segments"]:
            if s["selectable"]:
                mark = "HIGHLIGHT" if s["id"] in k["correct"] else "leave"
                lines.append(f"\"{s['text']}\"  [{mark}]  - {per.get(s['id'], '')}")
    elif t == "highlight_table":
        for r in c["rows"]:
            if r["selectable"]:
                mark = "HIGHLIGHT" if r["id"] in k["correct"] else "leave"
                lines.append(" | ".join(r["cells"]) + f"  [{mark}]  - {per.get(r['id'], '')}")
    elif t == "multiple_response_grouping":
        for g in c["groups"]:
            lines.append(f"{g['label']}:")
            for o in g["options"]:
                mark = "CORRECT" if o["id"] in k["byGroup"][g["id"]] else "wrong"
                lines.append(f"  {o['text']}  [{mark}]  - {per.get(o['id'], '')}")
    elif t == "fill_blank_calc":
        lines.append(f"Answer: {k['value']} {c['unit']} ({c['decimals']} decimal places)")
    else:
        lines.append(json.dumps({"content": c, "key": k}, ensure_ascii=False))
    return lines


def exhibit_text(exhibits):
    parts = []
    for e in exhibits or []:
        if e.get("table"):
            tbl = e["table"]
            rows = [" | ".join(tbl["columns"])] + [r["label"] + ": " + " | ".join(r["cells"]) for r in tbl["rows"]]
            parts.append(e["title"] + "\n" + "\n".join(rows))
        else:
            parts.append(e["title"] + "\n" + e.get("body", ""))
    return "\n\n".join(parts)


def load():
    files = []
    for path in sorted(HERE.glob("rn-bank-*.rn.json")):
        data = json.loads(path.read_text(encoding="utf-8"))
        cases = {c["id"]: c for c in data.get("caseStudies", [])}
        files.append((path.name, data, cases))
    return files


def write_csv(files):
    header = ["File", "Question ID", "Item type", "Client Needs", "Subtopics", "Body system", "CJMM skill",
              "Difficulty", "Case study", "Exhibits", "Stem", "Choices, key and per-choice rationale",
              "Overall rationale", "Educational objective", "References"] + REVIEW_COLUMNS
    with (OUT / "rn-bank-review.csv").open("w", newline="", encoding="utf-8-sig") as f:
        w = csv.writer(f)
        w.writerow(header)
        for name, data, cases in files:
            for q in data["questions"]:
                case = cases.get(q.get("caseStudyId"))
                case_text = ""
                if case:
                    case_text = f"{case['title']} (item {q['casePosition']})\n" + "\n\n".join(
                        exhibit_text(t["exhibits"]) for t in case["scenario"]["tabs"])
                w.writerow([name, q["id"], q["itemType"], f"{q['clientNeeds']} {CLIENT_NEEDS[q['clientNeeds']]}",
                            ", ".join(q["subtopics"]), q["bodySystem"], q.get("cjmmSkill", ""), q["difficulty"],
                            case_text, exhibit_text(q.get("exhibits")), q["stem"], "\n".join(answer_lines(q)),
                            q["rationale"]["correct"], q["educationalObjective"],
                            "\n".join(q["rationale"].get("references", []))] + [""] * len(REVIEW_COLUMNS))


def write_html(files):
    e = html.escape
    total = sum(len(d["questions"]) for _, d, _ in files)
    by_cn = collections.Counter(q["clientNeeds"] for _, d, _ in files for q in d["questions"])
    summary = "".join(
        f"<tr><td>{e(cn)} {e(CLIENT_NEEDS[cn])}</td><td>{by_cn[cn]}</td>"
        f"<td>{100 * by_cn[cn] / total:.1f}%</td><td>{TEST_PLAN[cn]}</td></tr>" for cn in CLIENT_NEEDS)
    body = []
    for name, data, cases in files:
        body.append(f"<h2>{e(name)} <small>({len(data['questions'])} questions)</small></h2>")
        shown_cases = set()
        for n, q in enumerate(data["questions"], 1):
            case = cases.get(q.get("caseStudyId"))
            if case and case["id"] not in shown_cases:
                shown_cases.add(case["id"])
                body.append(f"<div class='case'><h3>Case study: {e(case['title'])}</h3>"
                            f"<pre>{e(chr(10).join(exhibit_text(t['exhibits']) for t in case['scenario']['tabs']))}</pre></div>")
            meta = (f"{e(q['itemType'])} · {e(q['clientNeeds'])} · {e(', '.join(q['subtopics']))} · "
                    f"{e(q['difficulty'])} · {e(q.get('cjmmSkill', ''))}")
            ex = exhibit_text(q.get("exhibits"))
            body.append(
                "<section class='q'>"
                f"<div class='meta'>#{n} · <code>{e(q['id'])}</code> · {meta}</div>"
                + (f"<pre>{e(ex)}</pre>" if ex else "")
                + f"<p class='stem'>{e(q['stem'])}</p>"
                f"<ul>{''.join('<li>' + e(l) + '</li>' for l in answer_lines(q))}</ul>"
                f"<p><b>Rationale:</b> {e(q['rationale']['correct'])}</p>"
                f"<p><b>Objective:</b> {e(q['educationalObjective'])}</p>"
                "<p class='decision'>☐ Approve &nbsp; ☐ Revise &nbsp; ☐ Reject &nbsp; "
                "Notes: ________________________________ &nbsp; Initials: ______</p>"
                "</section>")
    page = f"""<!doctype html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1"><title>RN Bank Review</title>
<style>
body{{font:14px/1.45 system-ui,sans-serif;margin:24px auto;max-width:900px;padding:0 16px;color:#111;background:#fff}}
h1{{font-size:22px}} h2{{font-size:18px;margin-top:32px;border-bottom:2px solid #333}}
table{{border-collapse:collapse}} td,th{{border:1px solid #999;padding:4px 8px;text-align:left}}
.q{{border:1px solid #ccc;border-radius:6px;padding:10px 14px;margin:12px 0;break-inside:avoid}}
.meta{{font-size:12px;color:#555}} .stem{{font-weight:600}} pre{{white-space:pre-wrap;background:#f4f4f4;padding:8px}}
.case{{border-left:4px solid #333;padding-left:10px}} .decision{{font-size:13px}}
</style></head><body>
<h1>RN question bank: clinical review packet</h1>
<p>{total} questions in {len(files)} files. Use this packet for each review round; past rounds are recorded in REVIEW-LOG.md.
Check that the key is correct and current, that each rationale is accurate, and that the wording is clear and fair.
Record decisions in the CSV (rn-bank-review.csv) or on this printout.</p>
<table><tr><th>Client Needs</th><th>Questions</th><th>Bank</th><th>NCLEX test plan</th></tr>{summary}</table>
{''.join(body)}
</body></html>"""
    (OUT / "rn-bank-review.html").write_text(page, encoding="utf-8")


def main():
    OUT.mkdir(exist_ok=True)
    files = load()
    write_csv(files)
    write_html(files)
    print(f"Review sheet: {sum(len(d['questions']) for _, d, _ in files)} questions from {len(files)} files "
          f"-> {OUT / 'rn-bank-review.csv'} and rn-bank-review.html")


if __name__ == "__main__":
    main()
