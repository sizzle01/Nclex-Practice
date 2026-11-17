"""Builds the RN question bank's import files from compact Python batch files.

Each batch file in batches/ calls the helpers below and then build(). The helpers keep every
option, whether it is correct and its rationale together, so a key cannot drift away from its
explanation. build() checks the rules the importer enforces (docs/interfaces.md section 2,
QuestionImportFile.cs) and writes rn-bank-NN-<slug>.rn.json next to this file.

Run every batch:  python3 -I build_all.py
"""
import json
import random
from pathlib import Path

HERE = Path(__file__).resolve().parent
# Review status shown to candidates under each rationale. The full record (who, when, scope) is in
# REVIEW-LOG.md; it deliberately claims no clinical credential, because none was recorded.
REVIEW_STATUS = "Reviewed by the content team on 17 November 2026."

REFS = {
    "ms": "Medical-surgical nursing texts (e.g. Lewis; Ignatavicius)",
    "fund": "Fundamentals of nursing texts (e.g. Potter & Perry)",
    "ph": "Nursing pharmacology texts (e.g. Lehne; McCuistion) and the drug's prescribing information",
    "aha": "American Heart Association Guidelines for CPR and Emergency Cardiovascular Care",
    "mgmt": "NCSBN National Guidelines for Nursing Delegation; LaCharity, Prioritization, Delegation & Assignment",
    "cdc": "CDC infection control and isolation precautions guidance",
    "ob": "Maternal-newborn nursing texts (e.g. Perry; Lowdermilk)",
    "peds": "Pediatric nursing texts (e.g. Hockenberry, Wong's Essentials of Pediatric Nursing)",
    "psych": "Psychiatric-mental health nursing texts (e.g. Varcarolis; Halter)",
    "ati": "NCSBN NCLEX-RN Test Plan",
    "lab": "Laboratory reference ranges as used in common NCLEX review texts; follow the facility's ranges",
    "ada": "American Diabetes Association Standards of Care",
    "acip": "CDC/ACIP immunization schedules",
    "uspstf": "U.S. Preventive Services Task Force recommendations",
}

# One registry for every subtopic in the bank. A file that declares a slug renames it in the
# database, so all files must agree on names; keeping them here guarantees that.
SUBTOPICS = {
    # existing sample subtopics (names must match the stored ones)
    "prioritizing-client-care": ("SECE.MOC", "Prioritizing client care"),
    "delegation-and-supervision": ("SECE.MOC", "Delegation and supervision"),
    "transmission-based-precautions": ("SECE.SIC", "Transmission-based precautions"),
    "fire-safety-response": ("SECE.SIC", "Fire safety response"),
    "preconception-and-prenatal-care": ("HPM", "Preconception and prenatal care"),
    "infant-growth-and-development": ("HPM", "Infant growth and development"),
    "heart-failure-self-care-teaching": ("HPM", "Heart failure self-care teaching"),
    "suicide-risk-and-safety": ("PSI", "Suicide risk and safety"),
    "alcohol-withdrawal": ("PSI", "Alcohol withdrawal"),
    "tube-feeding-care": ("PHI.BCC", "Tube feeding care"),
    "pressure-injury-prevention": ("PHI.BCC", "Pressure injury prevention"),
    "pre-administration-medication-checks": ("PHI.PPT", "Pre-administration medication checks"),
    "adverse-effects-of-medications": ("PHI.PPT", "Adverse effects of medications"),
    "anticoagulant-therapy-and-monitoring": ("PHI.PPT", "Anticoagulant therapy and monitoring"),
    "potassium-medication-safety": ("PHI.PPT", "Potassium medication safety"),
    "blood-transfusion-reactions": ("PHI.PPT", "Blood transfusion reactions"),
    "medication-dose-calculations": ("PHI.PPT", "Medication dose calculations"),
    "medication-administration-technique": ("PHI.PPT", "Medication administration technique"),
    "heart-failure-interventions": ("PHI.PPT", "Heart failure interventions"),
    "monitoring-after-contrast-dye": ("PHI.RRP", "Monitoring after contrast dye"),
    "post-procedure-complications": ("PHI.RRP", "Post-procedure complications"),
    "recognizing-hypoglycemia": ("PHI.RRP", "Recognizing hypoglycemia"),
    "electrolyte-and-glucose-lab-results": ("PHI.RRP", "Electrolyte and glucose lab results"),
    "chest-tube-management": ("PHI.PA", "Chest tube management"),
    "hypoglycemia-treatment": ("PHI.PA", "Hypoglycemia treatment"),
    "heart-failure-assessment": ("PHI.PA", "Heart failure assessment"),
    "heart-failure-treatment-response": ("PHI.PA", "Heart failure treatment response"),
    "acute-pulmonary-edema": ("PHI.PA", "Acute pulmonary edema"),
    "diabetic-ketoacidosis": ("PHI.PA", "Diabetic ketoacidosis"),
    "post-thyroidectomy-complications": ("PHI.PA", "Post-thyroidectomy complications"),
    "postpartum-hemorrhage": ("PHI.PA", "Postpartum hemorrhage"),
    "impaired-oxygenation": ("PHI.PA", "Impaired oxygenation"),
    "infant-dehydration": ("PHI.PA", "Infant dehydration"),
    "basic-life-support": ("PHI.PA", "Basic life support"),
    # added by the RN bank
    "acute-coronary-syndrome": ("PHI.PA", "Acute coronary syndrome"),
    "cardiac-dysrhythmias": ("PHI.PA", "Cardiac dysrhythmias"),
    "vascular-disorders": ("PHI.PA", "Vascular disorders"),
    "venous-thromboembolism": ("PHI.PA", "Venous thromboembolism"),
    "hypertension-management": ("HPM", "Hypertension management"),
}

CLIENT_NEEDS = {"SECE.MOC", "SECE.SIC", "HPM", "PSI", "PHI.BCC", "PHI.PPT", "PHI.RRP", "PHI.PA"}
BODY_SYSTEMS = {"cardiovascular", "respiratory", "neurological", "gastrointestinal", "renal-urinary",
                "endocrine", "musculoskeletal", "integumentary", "hematologic-oncologic", "immune",
                "reproductive", "maternal-newborn", "pediatrics", "mental-health", "pharmacology",
                "fundamentals"}
SKILLS = {"recognize_cues", "analyze_cues", "prioritize_hypotheses", "generate_solutions",
          "take_action", "evaluate_outcomes"}
DIFFICULTY = {"easy", "moderate", "difficult"}


def add_subtopics(entries):
    """Registers subtopics a batch introduces: {slug: (clientNeeds, name)}."""
    for slug, value in entries.items():
        if slug in SUBTOPICS and SUBTOPICS[slug] != value:
            raise ValueError(f"subtopic {slug} already registered as {SUBTOPICS[slug]}")
        SUBTOPICS[slug] = value


def M(cn, sub, skill, diff, body, refs="ms"):
    """Metadata shared by a question: Client Needs code, subtopic slug(s), CJMM skill, difficulty, body system, reference keys."""
    subs = [sub] if isinstance(sub, str) else list(sub)
    return {"clientNeeds": cn, "bodySystem": body, "subtopics": subs, "cjmmSkill": skill,
            "difficulty": diff, "_refs": refs.split()}


# ---- exhibits -------------------------------------------------------------------------------
def note(title, body, id=None):
    return {"id": id or _slug(title), "kind": "text", "title": title, "body": body}


def flow(title, columns, rows, id=None):
    """rows: [(label, [cell, ...]), ...]"""
    return {"id": id or _slug(title), "kind": "flowsheet", "title": title,
            "table": {"columns": columns, "rows": [{"label": l, "cells": c} for l, c in rows]}}


def table(title, columns, rows, id=None):
    return {"id": id or _slug(title), "kind": "table", "title": title,
            "table": {"columns": columns, "rows": [{"label": l, "cells": c} for l, c in rows]}}


def _slug(s):
    return "".join(ch if ch.isalnum() else "-" for ch in s.lower()).strip("-")[:40] or "x"


# ---- item helpers. Each option is (text, is_correct, rationale). ------------------------------
def _q(meta, item_type, stem, content, key, why, per, obj, ex):
    d = dict(meta)
    d.update({"itemType": item_type, "stem": stem, "content": content, "key": key,
              "_why": why, "_per": per, "educationalObjective": obj})
    if ex:
        d["exhibits"] = ex
    return d


def _options(opts):
    return [{"id": chr(97 + i), "text": o[0]} for i, o in enumerate(opts)]


def mc(meta, stem, opts, why, obj, ex=None):
    correct = [chr(97 + i) for i, o in enumerate(opts) if o[1]]
    assert len(correct) == 1, f"multiple_choice needs exactly one correct option: {stem[:60]}"
    return _q(meta, "multiple_choice", stem, {"options": _options(opts)}, {"correct": correct[0]},
              why, {chr(97 + i): o[2] for i, o in enumerate(opts)}, obj, ex)


def sata(meta, stem, opts, why, obj, ex=None):
    correct = [chr(97 + i) for i, o in enumerate(opts) if o[1]]
    assert len(correct) >= 1, stem[:60]
    if "Select all that apply" not in stem:
        stem = stem.rstrip() + " Select all that apply."
    return _q(meta, "sata", stem, {"options": _options(opts)}, {"correct": correct},
              why, {chr(97 + i): o[2] for i, o in enumerate(opts)}, obj, ex)


def select_n(meta, stem, opts, why, obj, ex=None):
    correct = [chr(97 + i) for i, o in enumerate(opts) if o[1]]
    return _q(meta, "select_n", stem, {"options": _options(opts), "selectCount": len(correct)},
              {"correct": correct}, why, {chr(97 + i): o[2] for i, o in enumerate(opts)}, obj, ex)


def order(meta, stem, steps, why, obj, ex=None, seed=0):
    """steps: [(text, rationale)] in the CORRECT order; shown shuffled."""
    items = [{"id": chr(97 + i), "text": s[0]} for i, s in enumerate(steps)]
    shown = items[:]
    rnd = random.Random(seed or len(stem))
    while shown == items:
        rnd.shuffle(shown)
    return _q(meta, "ordered_response", stem, {"items": shown},
              {"order": [i["id"] for i in items], "scoring": "all_or_nothing"},
              why, {chr(97 + i): s[1] for i, s in enumerate(steps)}, obj, ex)


def matrix_mc(meta, stem, columns, rows, why, obj, ex=None):
    """rows: [(text, correct_column_index, [rationale per column])]; one answer per row."""
    content = {"rows": [{"id": f"r{i+1}", "text": r[0]} for i, r in enumerate(rows)],
               "columns": [{"id": f"c{j+1}", "text": c} for j, c in enumerate(columns)]}
    key = {"byRow": {f"r{i+1}": f"c{r[1]+1}" for i, r in enumerate(rows)}}
    per = {}
    for i, r in enumerate(rows):
        assert len(r[2]) == len(columns), f"row {i+1} needs a rationale per column: {stem[:50]}"
        for j in range(len(columns)):
            per[f"r{i+1}:c{j+1}"] = r[2][j]
    return _q(meta, "matrix_mc", stem, content, key, why, per, obj, ex)


def matrix_mr(meta, stem, columns, rows, why, obj, ex=None):
    """rows: [(text, {correct column indexes}, [rationale per column])]; each row is a group."""
    content = {"rows": [{"id": f"r{i+1}", "text": r[0]} for i, r in enumerate(rows)],
               "columns": [{"id": f"c{j+1}", "text": c} for j, c in enumerate(columns)],
               "groupAxis": "row"}
    key = {"byGroup": {f"r{i+1}": [f"r{i+1}:c{j+1}" for j in sorted(r[1])] for i, r in enumerate(rows)}}
    per = {}
    for i, r in enumerate(rows):
        assert r[1], f"row {i+1} needs at least one correct column"
        assert len(r[2]) == len(columns), f"row {i+1} needs a rationale per column: {stem[:50]}"
        for j in range(len(columns)):
            per[f"r{i+1}:c{j+1}"] = r[2][j]
    return _q(meta, "matrix_mr", stem, content, key, why, per, obj, ex)


def cloze(meta, stem, template, blanks, why, obj, ex=None):
    """template uses {{b1}}, {{b2}}...; blanks: [[(text, correct, rationale), ...], ...]"""
    content = {"template": template, "blanks": []}
    key, per = {"byBlank": {}}, {}
    for n, opts in enumerate(blanks, 1):
        bid = f"b{n}"
        assert "{{" + bid + "}}" in template, f"{bid} missing from template"
        content["blanks"].append({"id": bid, "options": _options(opts)})
        correct = [chr(97 + i) for i, o in enumerate(opts) if o[1]]
        assert len(correct) == 1, f"{bid} needs one correct choice"
        key["byBlank"][bid] = correct[0]
        for i, o in enumerate(opts):
            per[f"{bid}:{chr(97 + i)}"] = o[2]
    return _q(meta, "dropdown_cloze", stem, content, key, why, per, obj, ex)


def dtable(meta, stem, columns, rows, why, obj, ex=None):
    """rows: [([fixed cell texts], [(choice, correct, rationale), ...])]; the blank is the last column."""
    content = {"columns": columns, "rows": [], "blanks": []}
    key, per = {"byBlank": {}}, {}
    for n, (cells, opts) in enumerate(rows, 1):
        bid = f"b{n}"
        content["rows"].append({"id": f"r{n}", "cells": [{"text": c} for c in cells] + [{"blankId": bid}]})
        content["blanks"].append({"id": bid, "options": _options(opts)})
        correct = [chr(97 + i) for i, o in enumerate(opts) if o[1]]
        assert len(correct) == 1
        key["byBlank"][bid] = correct[0]
        for i, o in enumerate(opts):
            per[f"{bid}:{chr(97 + i)}"] = o[2]
        assert len(cells) + 1 == len(columns)
    return _q(meta, "dropdown_table", stem, content, key, why, per, obj, ex)


def hl_text(meta, stem, parts, why, obj, ex=None):
    """parts: plain strings (not selectable) and (text, correct, rationale) tuples (selectable)."""
    segs, key, per = [], [], {}
    for n, p in enumerate(parts, 1):
        sid = f"s{n}"
        if isinstance(p, str):
            segs.append({"id": sid, "text": p, "selectable": False})
        else:
            segs.append({"id": sid, "text": p[0], "selectable": True})
            per[sid] = p[2]
            if p[1]:
                key.append(sid)
    assert key
    return _q(meta, "highlight_text", stem, {"segments": segs}, {"correct": key}, why, per, obj, ex)


def hl_table(meta, stem, columns, rows, why, obj, ex=None):
    """rows: ([cells], correct or None when not selectable, rationale)."""
    content = {"columns": columns, "rows": []}
    key, per = [], {}
    for n, (cells, correct, r) in enumerate(rows, 1):
        rid = f"r{n}"
        content["rows"].append({"id": rid, "cells": cells, "selectable": correct is not None})
        if correct is not None:
            per[rid] = r
            if correct:
                key.append(rid)
    return _q(meta, "highlight_table", stem, content, {"correct": key}, why, per, obj, ex)


def grouping(meta, stem, groups, why, obj, ex=None):
    """groups: [(label, [(text, correct, rationale), ...])]; option ids run a, b, c... across groups."""
    content, key, per, n = {"groups": []}, {"byGroup": {}}, {}, 0
    for g, (label, opts) in enumerate(groups, 1):
        gid, options, correct = f"g{g}", [], []
        for o in opts:
            oid = chr(97 + n)
            n += 1
            options.append({"id": oid, "text": o[0]})
            per[oid] = o[2]
            if o[1]:
                correct.append(oid)
        content["groups"].append({"id": gid, "label": label, "options": options})
        key["byGroup"][gid] = correct
    return _q(meta, "multiple_response_grouping", stem, content, key, why, per, obj, ex)


def calc(meta, stem, unit, decimals, label, value, why, obj, ex=None):
    return _q(meta, "fill_blank_calc", stem, {"unit": unit, "decimals": decimals, "inputLabel": label},
              {"value": value, "rounding": "exact"}, why, {}, obj, ex)


def case(title, tabs, questions):
    """An NGN unfolding case: tabs [(id, title, [exhibits])], questions in case order (up to 6)."""
    return {"title": title,
            "scenario": {"tabs": [{"id": t[0], "title": t[1], "exhibits": t[2]} for t in tabs]},
            "questions": questions}


# ---- build ------------------------------------------------------------------------------------
def _qid(batch, n):
    return f"c1a{batch:05d}-0000-4000-8000-{n:012d}"


def _cid(batch, n):
    return f"c1c{batch:05d}-0000-4000-8000-{n:012d}"


def build(batch, slug, title, items, cases=()):
    questions, case_rows, n = [], [], 0
    for item in items:
        n += 1
        questions.append(_finish(item, _qid(batch, n)))
    for c_n, c in enumerate(cases, 1):
        cid = _cid(batch, c_n)
        case_rows.append({"id": cid, "title": c["title"], "scenario": c["scenario"]})
        assert 1 <= len(c["questions"]) <= 6
        for pos, item in enumerate(c["questions"], 1):
            n += 1
            q = _finish(item, _qid(batch, n))
            q["caseStudyId"], q["casePosition"] = cid, pos
            questions.append(q)

    _check(questions, case_rows)
    used = []
    for q in questions:
        for s in q["subtopics"]:
            if s not in used:
                used.append(s)
    out = {
        "notice": f"RN bank batch {batch}: {title}. {REVIEW_STATUS} See REVIEW-LOG.md.",
        "track": "RN",
        "subtopics": [{"clientNeeds": SUBTOPICS[s][0], "slug": s, "name": SUBTOPICS[s][1]} for s in used],
        "questions": questions,
    }
    if case_rows:
        out["caseStudies"] = case_rows
    path = HERE / f"rn-bank-{batch:02d}-{slug}.rn.json"
    path.write_text(json.dumps(out, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return path, questions


def _finish(item, qid):
    q = {"id": qid}
    for k in ("itemType", "clientNeeds", "bodySystem", "subtopics", "cjmmSkill", "difficulty", "stem",
              "content", "key"):
        q[k] = item[k]
    q["rationale"] = {"correct": item["_why"], "perOption": item["_per"],
                      "references": [REFS[r] for r in item["_refs"]] + [REVIEW_STATUS]}
    q["educationalObjective"] = item["educationalObjective"]
    if "exhibits" in item:
        q["exhibits"] = item["exhibits"]
    return q


def _check(questions, cases):
    errors = []
    for q in questions:
        where = f'{q["id"]} ({q["stem"][:50]}...)'
        if q["clientNeeds"] not in CLIENT_NEEDS: errors.append(f"{where}: client needs {q['clientNeeds']}")
        if q["bodySystem"] not in BODY_SYSTEMS: errors.append(f"{where}: body system {q['bodySystem']}")
        if q["cjmmSkill"] not in SKILLS: errors.append(f"{where}: skill {q['cjmmSkill']}")
        if q["difficulty"] not in DIFFICULTY: errors.append(f"{where}: difficulty")
        if not 1 <= len(q["subtopics"]) <= 3: errors.append(f"{where}: 1 to 3 subtopics")
        for s in q["subtopics"]:
            if s not in SUBTOPICS: errors.append(f"{where}: unknown subtopic {s}")
            elif SUBTOPICS[s][0] != q["clientNeeds"]:
                errors.append(f"{where}: subtopic {s} belongs to {SUBTOPICS[s][0]}, not {q['clientNeeds']}")
        for k in ("stem", "educationalObjective"):
            if not q[k].strip(): errors.append(f"{where}: empty {k}")
        for k, v in q["rationale"]["perOption"].items():
            if not v.strip(): errors.append(f"{where}: empty rationale {k}")
        c, t = q["content"], q["itemType"]
        # Size limits from the item-type modules in Nclex.Domain/ItemTypes.
        if t == "sata" and not 5 <= len(c["options"]) <= 10: errors.append(f"{where}: SATA needs 5 to 10 options")
        if t == "multiple_choice" and len(c["options"]) < 2: errors.append(f"{where}: too few options")
        if t == "select_n" and not (len(c["options"]) >= 3 and 1 <= c["selectCount"] < len(c["options"])):
            errors.append(f"{where}: select N needs 3+ options and fewer correct than options")
        if t == "ordered_response" and len(c["items"]) < 3: errors.append(f"{where}: ordering needs 3+ items")
        if t in ("matrix_mc", "matrix_mr") and len(c["columns"]) < 2: errors.append(f"{where}: 2+ columns")
        if t == "multiple_response_grouping" and any(len(g["options"]) < 2 for g in c["groups"]):
            errors.append(f"{where}: 2+ options per group")
        if t in ("dropdown_cloze", "dropdown_table") and any(len(b["options"]) < 2 for b in c["blanks"]):
            errors.append(f"{where}: 2+ choices per blank")
        texts = [o["text"] for o in c.get("options", [])]
        if len(texts) != len(set(texts)): errors.append(f"{where}: duplicate option text")
        ex_ids = [e["id"] for e in q.get("exhibits", [])]
        if len(ex_ids) != len(set(ex_ids)): errors.append(f"{where}: duplicate exhibit ids")
    # The importer requires one cell per column in table and flowsheet exhibits (the row label
    # is separate from the columns), in questions and in case-study tabs alike.
    exhibits = [(q["id"], e) for q in questions for e in q.get("exhibits", [])]
    exhibits += [(c["title"], e) for c in cases for t in c["scenario"]["tabs"] for e in t["exhibits"]]
    for owner, e in exhibits:
        for row in (e.get("table") or {}).get("rows", []):
            if len(row["cells"]) != len(e["table"]["columns"]):
                errors.append(f"{owner}: exhibit '{e['title']}' row '{row['label']}' needs one cell per column")
    ids = [q["id"] for q in questions]
    if len(ids) != len(set(ids)): errors.append("duplicate question ids")
    for c in cases:
        tab_ids = [t["id"] for t in c["scenario"]["tabs"]]
        if len(tab_ids) != len(set(tab_ids)): errors.append(f"case {c['title']}: duplicate tab ids")
        for q in questions:
            if q.get("caseStudyId") == c["id"]:
                clash = set(tab_ids) & {e["id"] for e in q.get("exhibits", [])}
                if clash: errors.append(f"{q['id']}: exhibit ids clash with case tabs {clash}")
    if errors:
        raise SystemExit("Batch has problems:\n  " + "\n  ".join(errors))
