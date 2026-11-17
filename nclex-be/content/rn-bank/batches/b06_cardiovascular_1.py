"""Batch 6: Cardiovascular I - ACS, dysrhythmias, anticoagulation, vascular disorders.

The first 20 questions were written before the generator existed; they are read from
data/cardiovascular-seed.json and rebuilt here so they get the bank's ids and checks.
"""
import json
from pathlib import Path

from qb import *

SEED_REFS = {"Medical-surgical": "ms", "Nursing pharmacology": "ph", "American Heart": "aha", "NCSBN": "mgmt"}


def from_seed(q):
    refs = [k for r in q["rationale"]["references"] for prefix, k in SEED_REFS.items() if r.startswith(prefix)]
    item = {k: q[k] for k in ("itemType", "clientNeeds", "bodySystem", "subtopics", "cjmmSkill", "difficulty",
                              "stem", "content", "key", "educationalObjective")}
    item.update({"_why": q["rationale"]["correct"], "_per": q["rationale"]["perOption"], "_refs": refs or ["ms"]})
    if "exhibits" in q:
        item["exhibits"] = q["exhibits"]
    return item


seed = json.loads((Path(__file__).parent / "data" / "cardiovascular-seed.json").read_text(encoding="utf-8"))
items = [from_seed(q) for q in seed["questions"]]

CV = "cardiovascular"
items += [
mc(M("PHI.RRP", "monitoring-after-contrast-dye", "recognize_cues", "moderate", CV, "ms ph"),
   "A client with type 2 diabetes is scheduled for a cardiac catheterization with contrast dye tomorrow. Which medication in the client's history should the nurse report to the provider before the procedure?",
   [("Metformin", True, "Metformin is usually held around iodinated contrast procedures because contrast-induced kidney injury can lead to metformin accumulation and lactic acidosis."),
    ("Atorvastatin", False, "Statins don't interact with contrast dye."),
    ("Aspirin", False, "Aspirin is usually continued before cardiac catheterization."),
    ("Omeprazole", False, "Omeprazole doesn't affect contrast safety.")],
   "Contrast can impair kidney function; metformin is held per protocol and restarted after kidney function is confirmed (usually after 48 hours).",
   "Identify medications to hold before a contrast procedure."),

sata(M("PHI.RRP", "post-procedure-complications", "generate_solutions", "moderate", CV, "ms"),
   "A client has returned from a cardiac catheterization through the right femoral artery. Which actions should the nurse include in the plan of care?",
   [("Keep the right leg straight for the prescribed time", True, "Keeping the leg straight prevents bleeding at the arterial puncture."),
    ("Check the pulses, color and temperature of the right foot", True, "Distal checks detect arterial occlusion or a thrombus."),
    ("Check the puncture site for bleeding or hematoma", True, "Bleeding is the most common complication."),
    ("Encourage oral fluids unless restricted", True, "Fluids help flush the contrast and protect the kidneys."),
    ("Raise the head of the bed to 90 degrees right away", False, "Sharp hip flexion can cause bleeding; the head of the bed is usually kept low (30 degrees or less) at first."),
    ("Encourage walking within the first hour", False, "Bed rest is needed for several hours after femoral access.")],
   "After femoral access: keep the leg straight, check the site and distal circulation, encourage fluids and limit hip flexion.",
   "Plan care after femoral cardiac catheterization."),

mc(M("PHI.PA", "cardiac-dysrhythmias", "analyze_cues", "moderate", CV, "ms"),
   "The cardiac monitor shows a regular ventricular rate of 75/min with sawtooth-shaped waves between the QRS complexes and a 4:1 conduction ratio. How should the nurse interpret this rhythm?",
   [("Atrial fibrillation", False, "Atrial fibrillation shows chaotic, wavy baseline activity and an irregularly irregular rhythm."),
    ("Atrial flutter", True, "Sawtooth flutter waves with a fixed conduction ratio are typical of atrial flutter."),
    ("Ventricular tachycardia", False, "VT has wide QRS complexes at a fast rate."),
    ("Sinus rhythm with premature atrial contractions", False, "Sinus rhythm has one upright P wave before each QRS.")],
   "Atrial flutter: sawtooth flutter waves, often with a regular ventricular response.",
   "Identify atrial flutter on a rhythm strip."),

mc(M("PHI.PA", "cardiac-dysrhythmias", "take_action", "moderate", CV, "aha"),
   "A client's heart rate is 38/min in sinus bradycardia. The client is dizzy and pale, with a blood pressure of 78/44 mm Hg. Which medication should the nurse prepare to give first?",
   [("Atropine 1 mg IV", True, "Atropine is the first-line drug for symptomatic bradycardia."),
    ("Adenosine 6 mg rapid IV push", False, "Adenosine slows conduction and is used for supraventricular tachycardia."),
    ("Metoprolol 5 mg IV", False, "A beta-blocker would slow the heart rate further."),
    ("Amiodarone 150 mg IV", False, "Amiodarone treats tachydysrhythmias.")],
   "Symptomatic bradycardia: atropine 1 mg IV (repeat every 3 to 5 minutes, up to 3 mg). If it isn't effective, use transcutaneous pacing or dopamine or epinephrine infusions.",
   "Treat symptomatic bradycardia."),

mc(M("PHI.PA", "cardiac-dysrhythmias", "evaluate_outcomes", "moderate", CV, "ms"),
   "The nurse is teaching a client who had a permanent pacemaker inserted yesterday. Which statement by the client shows a need for more teaching?",
   [("\"I'll avoid raising my arm on the pacemaker side above my shoulder for a few weeks.\"", False, "Limiting arm movement keeps the new leads from moving out of place."),
    ("\"I'll carry my pacemaker identification card.\"", False, "The card informs providers and security staff."),
    ("\"I'll keep my cell phone in the shirt pocket over my pacemaker.\"", True, "Phones should be kept about 15 cm (6 inches) from the device and held to the ear on the opposite side."),
    ("\"I'll check my pulse and report it if it's below the set rate.\"", False, "A rate below the set rate may mean the pacemaker isn't working properly.")],
   "Pacemaker teaching: limit arm movement on that side early, carry the ID card, keep magnets and phones away from the device and check the pulse.",
   "Evaluate teaching after pacemaker insertion."),
]

BUILT = build(6, "cardiovascular-1", "Cardiovascular I: ACS, dysrhythmias, anticoagulation and vascular disorders", items)
