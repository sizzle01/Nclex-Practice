"""Batch 116 (case conversion, chunk 5: neurologic): four full NGN case studies (CASE-MAP.md, book_topics.py).

Cases: myasthenic crisis versus cholinergic crisis (PSNS and cholinergic medications); Guillain-Barre syndrome with
respiratory failure; intracerebral hemorrhage on warfarin; cervical spinal cord injury with neurogenic shock and then
autonomic dysreflexia. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and
includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
NEURO = "neurological"
DEGEN = "degenerative-neurological-disorders"
NEURO_MEDS = "neurological-medications"
NEURO_DX = "neurological-assessment-and-procedures"
STROKE = "stroke-care"
SCI = "spinal-cord-injury"
ANTIDOTE = "antidotes-and-reversal-agents"
VENT = "mechanical-ventilation"
ADVERSE = "adverse-effects-of-medications"
TEACH = "health-teaching-and-behavior-change"
HANDOFF = "handoff-and-sbar-communication"
IMMOBILE = "immobility-and-positioning"
INJURY = "injury-prevention"
COLLAB = "collaboration-and-referrals"

items = []

# ================================================================================================
# Case 1: Myasthenic crisis versus cholinergic crisis
# ================================================================================================
mg_tabs = [
    ("mg-notes", "Nurses' notes", [note("Neurology unit, 1400",
        "A 46-year-old client with myasthenia gravis takes pyridostigmine 60 mg four times daily. A week ago the "
        "client started ciprofloxacin for a urinary infection. Today the client has drooping eyelids, double vision, "
        "slurred and nasal speech, and trouble swallowing saliva. The client cannot lift the arms for long and says "
        "breathing feels weak. The client took two extra pyridostigmine tablets this morning \"to help.\"",
        id="mg-note")]),
    ("mg-data", "Respiratory measures", [flow("Respiratory measures", ["1000", "1400"], [
        ("Vital capacity", ["1.9 L", "1.1 L (about 15 mL/kg)"]),
        ("Negative inspiratory force", ["-38 cm H2O", "-22 cm H2O"]),
        ("Respirations", ["18/min", "28/min, shallow"]),
        ("SpO2", ["97%", "95% on room air"]),
        ("Heart rate", ["84/min", "96/min"]),
        ("Secretions", ["Normal", "Pooling in the mouth"])], id="mg-flow")]),
]

mg_qs = [
hl_table(M(PA, DEGEN, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Vital capacity", "1.1 L, falling from 1.9 L"], True, "Approaching the level where ventilation is needed."),
    (["Negative inspiratory force", "-22 cm H2O"], True, "Weaker than -20 to -25 signals respiratory muscle failure."),
    (["Swallowing", "Cannot manage saliva"], True, "Aspiration risk and bulbar weakness."),
    (["Respirations", "28/min, shallow"], True, "Rapid shallow breathing from weak muscles."),
    (["SpO2", "95%"], False, "Normal oxygen saturation is falsely reassuring; it falls late in neuromuscular failure."),
    (["Eyelids", "Drooping"], False, "A typical sign of myasthenia, but not itself an emergency.")],
   "In neuromuscular weakness, vital capacity, inspiratory force and swallowing show danger long before SpO2 falls.",
   "Recognize cues of impending respiratory failure in myasthenia gravis."),

matrix_mc(M(PPT, NEURO_MEDS, "analyze_cues", "difficult", NEURO, "ph ms"),
   "For each finding, indicate whether it is more typical of myasthenic crisis or cholinergic crisis.",
   ["Myasthenic crisis (too little acetylcholine effect)", "Cholinergic crisis (too much)"],
   [("Weakness that improved briefly after the dose, then worsened", 0, "Undertreatment or a trigger."),
    ("Small pupils, sweating, drooling and abdominal cramps", 1, "Muscarinic excess from too much cholinesterase inhibitor."),
    ("Slow heart rate and wheezing", 1, "Excess acetylcholine at muscarinic receptors."),
    ("A new fluoroquinolone antibiotic", 0, "Fluoroquinolones block neuromuscular transmission and trigger crisis."),
    ("Muscle twitching (fasciculations) with weakness", 1, "Depolarizing block from excess acetylcholine."),
    ("Fever and infection", 0, "Common triggers of myasthenic crisis.")],
   "Both cause weakness; muscarinic signs (SLUDGE, small pupils, bradycardia) point to cholinergic crisis.",
   "Analyze myasthenic versus cholinergic crisis."),

cloze(M(PA, DEGEN, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client most likely has {{b1}}, triggered by {{b2}}. The greatest immediate risk is {{b3}}.",
   [[("myasthenic crisis", True, "No muscarinic signs; a known trigger; weakness despite extra doses."),
     ("cholinergic crisis", False, "No small pupils, sweating or bradycardia."),
     ("a stroke", False, "Fatigable, bilateral weakness fits myasthenia."),
     ("Guillain-Barre syndrome", False, "Not with this history.")],
    [("ciprofloxacin", True, "Fluoroquinolones worsen neuromuscular transmission."),
     ("the urinary infection alone", False, "Possible contributor, but the drug is the key trigger."),
     ("the extra pyridostigmine", False, "Too much would cause cholinergic signs."),
     ("dehydration", False, "Not supported.")],
    [("respiratory failure and aspiration", True, "Falling vital capacity and pooled secretions."),
     ("hypertension", False, "Not expected."),
     ("a seizure", False, "Not expected."),
     ("kidney injury", False, "Not expected.")]],
   "Myasthenic crisis from a fluoroquinolone threatens breathing and the airway.",
   "Prioritize myasthenic crisis."),

matrix_mc(M(PPT, NEURO_MEDS, "generate_solutions", "difficult", NEURO, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Stop ciprofloxacin and choose another antibiotic", 0, "Removes the trigger."),
    ("Plasmapheresis or IV immune globulin", 0, "Rapidly lowers acetylcholine receptor antibodies."),
    ("Vital capacity and inspiratory force every 2 to 4 hours", 0, "Tracks respiratory muscle strength."),
    ("Prepare for intubation if vital capacity falls below about 15 to 20 mL/kg", 0, "Early, controlled airway."),
    ("Another dose of pyridostigmine now to strengthen breathing", 1,
     "Usually held in crisis while the client is ventilated or until the crisis type is clear; more can cause cholinergic crisis."),
    ("Magnesium sulfate for the urinary symptoms", 1, "Magnesium worsens neuromuscular blockade.")],
   "Remove the trigger, give immune therapy, watch the respiratory measures and secure the airway early.",
   "Generate solutions for myasthenic crisis.",
   ex=[note("Plan, 1430", "ICU transfer. IVIG 2 g/kg over 5 days. Stop ciprofloxacin; nitrofurantoin instead. Hold "
       "pyridostigmine if intubated. Neurology to see the client.", id="mg-plan")]),

sata(M(PA, DEGEN, "take_action", "difficult", NEURO, "ms"),
   "Review the 1700 findings. Which actions should the nurse take?",
   [("Call the rapid response or anesthesia team for intubation", True, "Vital capacity under 15 mL/kg with tiring."),
    ("Suction the mouth and position the head up", True, "Protects the airway from pooled secretions."),
    ("Keep the client NPO", True, "Aspiration risk and likely intubation."),
    ("Avoid sedatives and neuromuscular blockers unless the airway team directs them", True,
     "Clients with myasthenia are very sensitive to them."),
    ("Wait until the SpO2 falls below 90% before calling", False, "SpO2 falls late; waiting risks arrest."),
    ("Give a sip of water to test swallowing", False, "Aspiration risk.")],
   "Act on vital capacity and secretions, not SpO2: protect the airway and call for intubation.",
   "Take action for respiratory failure in myasthenia gravis.",
   ex=[flow("1700", ["1700"], [
       ("Vital capacity", ["0.9 L (about 12 mL/kg)"]),
       ("Negative inspiratory force", ["-16 cm H2O"]),
       ("SpO2", ["94%"]),
       ("Client", ["Cannot finish a sentence; drooling"])], id="mg-1700")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", NEURO, "ph ms"),
   "After recovery, the client describes the home plan. For each statement, indicate whether it shows understanding "
   "or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take pyridostigmine on time, about 30 to 60 minutes before meals.\"", 0, "Strength peaks for chewing and swallowing."),
    ("\"I'll check with my neurologist before taking any new antibiotic.\"", 0, "Several antibiotics worsen myasthenia."),
    ("\"If I'm weaker, I'll take extra doses until I feel better.\"", 1, "Extra doses can cause cholinergic crisis; call instead."),
    ("\"I'll wear a medical alert bracelet.\"", 0, "Alerts responders."),
    ("\"Drooling, sweating and cramps after a dose mean I should call.\"", 0, "Signs of too much medicine."),
    ("\"I'll rest in hot baths to relax my muscles.\"", 1, "Heat worsens weakness.")],
   "Safe self-care means timed doses, avoiding trigger drugs, not taking extra doses, and recognizing both crises.",
   "Evaluate myasthenia gravis teaching.",
   ex=[note("Day 9", "Extubated on day 6. Swallow test passed.", id="mg-day9")]),
]

# ================================================================================================
# Case 2: Guillain-Barre syndrome
# ================================================================================================
gbs_tabs = [
    ("gbs-notes", "Nurses' notes", [note("Medical unit, admission",
        "A 38-year-old client had a diarrheal illness 3 weeks ago. Over 4 days, tingling began in the feet and "
        "weakness has climbed to the thighs and now the hands. Knee and ankle reflexes are absent. The client is "
        "alert and anxious. Lumbar puncture: high protein with a normal cell count.", id="gbs-note")]),
    ("gbs-data", "Monitoring", [flow("Monitoring", ["Admission", "Day 2"], [
        ("Vital capacity", ["2.8 L", "1.6 L"]),
        ("Heart rate", ["88/min", "48 to 132/min, swinging"]),
        ("Blood pressure", ["128/80", "88/50 to 186/104 mm Hg, swinging"]),
        ("Strength", ["Legs 3/5, arms 5/5", "Legs 1/5, arms 3/5"]),
        ("Cough", ["Strong", "Weak"])], id="gbs-flow")]),
]

gbs_qs = [
hl_text(M(PA, DEGEN, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that support Guillain-Barre syndrome.",
   ["The client had ",
    ("a diarrheal illness 3 weeks ago", True, "Campylobacter infection is a common trigger."),
    ". Tingling and weakness ",
    ("began in the feet and climbed upward", True, "Ascending, symmetric weakness is typical."),
    ". The client has ",
    ("absent knee and ankle reflexes", True, "Areflexia is a key sign."),
    " and is ",
    ("alert and anxious", False, "The mind is unaffected; anxiety is expected."),
    ". The spinal fluid shows ",
    ("high protein with a normal cell count", True, "Albuminocytologic dissociation."),
    "."],
   "Ascending weakness, areflexia and high CSF protein after an infection point to Guillain-Barre syndrome.",
   "Recognize cues of Guillain-Barre syndrome."),

matrix_mc(M(PA, DEGEN, "analyze_cues", "difficult", NEURO, "ms"),
   "For each day 2 finding, indicate what it reflects.",
   ["Respiratory muscle weakness", "Autonomic dysfunction", "Motor nerve involvement"],
   [("Vital capacity 1.6 L, down from 2.8 L", 0, "The diaphragm is weakening."),
    ("Heart rate swinging from 48 to 132/min", 1, "Autonomic nerves are affected."),
    ("Blood pressure swinging widely", 1, "Autonomic instability."),
    ("Weakness spreading to the arms", 2, "Ascending motor involvement."),
    ("A weak cough", 0, "Weak expiratory muscles; secretions and aspiration risk.")],
   "GBS threatens breathing and the autonomic nervous system as weakness climbs.",
   "Analyze findings in GBS."),

cloze(M(PA, DEGEN, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The two most immediate threats are {{b1}} and {{b2}}.",
   [[("respiratory failure", True, "Vital capacity is falling fast."),
     ("pressure injuries", False, "A risk, but later."),
     ("depression", False, "Important, but later."),
     ("contractures", False, "Later.")],
    [("dysrhythmias from autonomic instability", True, "Bradycardia and asystole can occur."),
     ("hyperglycemia", False, "Not expected."),
     ("seizures", False, "Not part of GBS."),
     ("kidney stones", False, "Not relevant.")]],
   "Breathing and autonomic instability are the life threats in GBS.",
   "Prioritize threats in GBS."),

sata(M(PA, DEGEN, "generate_solutions", "difficult", NEURO, "ms ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("IV immune globulin or plasma exchange", True, "Shortens the illness."),
    ("ICU care with cardiac monitoring", True, "Autonomic dysrhythmias."),
    ("Vital capacity checks every 4 hours", True, "Early airway control."),
    ("Heparin and compression devices", True, "Immobility causes clots."),
    ("High-dose IV corticosteroids each day", False, "Not effective in GBS."),
    ("Encourage walking to keep strength up", False, "Unsafe with severe weakness.")],
   "Immune therapy, ICU monitoring, early airway decisions and clot prevention.",
   "Generate solutions for GBS.",
   ex=[note("Day 2 plan", "IVIG started. ICU transfer.", id="gbs-plan")]),

sata(M(PA, DEGEN, "take_action", "difficult", NEURO, "ms"),
   "On day 3, the client is intubated. The client blinks to answer questions and is clearly frightened. Which "
   "actions should the nurse take?",
   [("Explain every procedure before doing it", True, "The client is fully aware and cannot move."),
    ("Set up a reliable way to communicate, such as a blink code or letter board", True, "Reduces fear and isolation."),
    ("Give pain medicine for the nerve pain as prescribed", True, "GBS is often painful."),
    ("Turn every 2 hours and protect the heels", True, "Prevents pressure injury."),
    ("Talk about the client in the third person at the bedside", False, "The client hears everything."),
    ("Assume the client cannot feel pain because they cannot move", False, "Sensation and pain are often present.")],
   "An aware, paralyzed client needs communication, explanation, pain control and skin care.",
   "Take action for psychosocial and physical care in GBS.",
   ex=[flow("Day 3", ["Day 3"], [
       ("Ventilation", ["Intubated; ventilator"]),
       ("Strength", ["Arms and legs 0/5; eyes and blinking intact"]),
       ("Pain", ["Grimaces with turning"]),
       ("Heart rate", ["54 to 120/min"])], id="gbs-day3")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", NEURO, "ms mgmt"),
   "Review the findings at week 3. For each finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Vital capacity 2.2 L; extubated", 0, "Breathing muscles recovering."),
    ("Arm strength 4/5", 0, "Recovering in reverse order."),
    ("Heart rate steady at 70 to 90/min", 0, "Autonomic function improving."),
    ("Not yet referred to inpatient rehabilitation", 1, "Needs physical and occupational therapy and a rehab plan."),
    ("Left calf swollen and tender", 1, "Possible DVT despite prophylaxis."),
    ("Says, \"I'm scared it will happen again.\"", 1, "Needs emotional support and accurate information; recurrence is uncommon.")],
   "Recovery is under way; rehabilitation, a possible DVT and fear need follow-up.",
   "Evaluate recovery in GBS.",
   ex=[note("Week 3", "Moved to the neurology ward.", id="gbs-week3")]),
]

# ================================================================================================
# Case 3: Intracerebral hemorrhage on warfarin
# ================================================================================================
ich_tabs = [
    ("ich-notes", "Nurses' notes", [note("Emergency department, 0915",
        "A 74-year-old client who takes warfarin for atrial fibrillation had a sudden severe headache at 0830, then "
        "vomiting and right-sided weakness. The client is drowsy. CT: a 30 mL left basal ganglia hemorrhage.",
        id="ich-note")]),
    ("ich-data", "Assessment", [table("Assessment and labs", ["0915"], [
        ("Blood pressure", ["212/118 mm Hg"]),
        ("Heart rate", ["64/min, irregular"]),
        ("Glasgow Coma Scale", ["13 (E3 V4 M6)"]),
        ("Pupils", ["Equal, 3 mm, reactive"]),
        ("INR", ["3.4"]),
        ("Glucose", ["142 mg/dL"]),
        ("Platelets", ["210,000/mm3"])], id="ich-table")]),
]

ich_qs = [
hl_table(M(PA, STROKE, "recognize_cues", "moderate", NEURO, "ms aha"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["CT", "30 mL left basal ganglia hemorrhage"], True, "Hemorrhagic stroke."),
    (["INR", "3.4 on warfarin"], True, "The bleed can expand until the INR is reversed."),
    (["Blood pressure", "212/118 mm Hg"], True, "High pressure drives expansion of the bleed."),
    (["Glasgow Coma Scale", "13"], True, "Reduced consciousness."),
    (["Pupils", "Equal and reactive"], False, "Reassuring for now; a baseline for later checks."),
    (["Platelets", "210,000/mm3"], False, "Normal.")],
   "A brain bleed on warfarin with high pressure needs urgent reversal and blood pressure control.",
   "Recognize cues of anticoagulant-related intracerebral hemorrhage."),

cloze(M(PA, STROKE, "analyze_cues", "difficult", NEURO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "Right-sided weakness is explained by {{b1}}. The headache and vomiting suggest {{b2}}. Alteplase is {{b3}}.",
   [[("a left-sided brain lesion, because motor pathways cross", True, "The left hemisphere controls the right body."),
     ("a right-sided lesion", False, "The CT shows the left."),
     ("spinal cord compression", False, "Not supported."),
     ("a peripheral nerve injury", False, "Not supported.")],
    [("rising intracranial pressure", True, "Mass effect from the hematoma."),
     ("migraine", False, "The CT shows a bleed."),
     ("food poisoning", False, "Not supported."),
     ("low blood sugar", False, "Glucose is 142.")],
    [("contraindicated", True, "Thrombolysis is never given for a hemorrhagic stroke."),
     ("indicated within 4.5 hours", False, "Only for ischemic stroke without bleeding."),
     ("optional", False, "It would worsen bleeding."),
     ("indicated at half dose", False, "Never in hemorrhage.")]],
   "A left basal ganglia bleed causes right weakness; ICP is rising; thrombolysis is contraindicated.",
   "Analyze intracerebral hemorrhage findings."),

matrix_mc(M(PA, STROKE, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "For each risk, indicate its priority in the next 6 hours.",
   ["High priority", "Lower priority"],
   [("Hematoma expansion", 0, "Most expansion happens in the first hours, worse with anticoagulation."),
    ("Herniation from rising ICP", 0, "A falling GCS and pupil change would show it."),
    ("Aspiration from drowsiness and vomiting", 0, "Airway protection."),
    ("Long-term stroke prevention for AF", 1, "Decided later.")],
   "Expansion, herniation and aspiration are the early threats.",
   "Prioritize risks in intracerebral hemorrhage."),

matrix_mc(M(PPT, ANTIDOTE, "generate_solutions", "difficult", NEURO, "ph aha"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Four-factor prothrombin complex concentrate and IV vitamin K", 0, "Reverses warfarin quickly and lastingly."),
    ("Nicardipine infusion to a systolic pressure of about 140 mm Hg", 0, "Limits expansion."),
    ("Head of the bed at 30 degrees", 0, "Supports venous drainage."),
    ("Neurologic checks every 15 to 30 minutes at first", 0, "Detects deterioration."),
    ("Aspirin 325 mg for stroke", 1, "Worsens bleeding."),
    ("Fresh frozen plasma alone as first-line reversal", 1, "Slower and needs large volumes; PCC is preferred.")],
   "Reverse warfarin with PCC and vitamin K, control pressure, position and monitor; no antiplatelets.",
   "Generate solutions for warfarin-associated hemorrhage.",
   ex=[note("Orders", "Four-factor PCC. Vitamin K 10 mg IV. Nicardipine to SBP 140. Neurosurgery consult.", id="ich-orders")]),

sata(M(PA, STROKE, "take_action", "difficult", NEURO, "ms"),
   "Review the 1130 findings. Which actions should the nurse take?",
   [("Notify the provider and neurosurgery now", True, "Signs of expansion and herniation."),
    ("Prepare for an urgent repeat CT", True, "Confirms the change."),
    ("Keep the head midline and the head of the bed at 30 degrees", True, "Supports venous drainage."),
    ("Lower the head of the bed flat to improve brain blood flow", False, "Lying flat raises ICP."),
    ("Give a sedative because the client is restless", False, "Masks neurologic changes.")],
   "A falling GCS with a newly dilated pupil means expansion or herniation: escalate, image and protect the airway.",
   "Take action for neurologic deterioration.",
   ex=[flow("Neurologic checks", ["0915", "1030", "1130"], [
       ("GCS", ["13", "12", "9 (E2 V2 M5)"]),
       ("Left pupil", ["3 mm reactive", "3 mm reactive", "5 mm, sluggish"]),
       ("Right pupil", ["3 mm reactive", "3 mm reactive", "3 mm reactive"]),
       ("Blood pressure", ["212/118", "148/86", "176/70 mm Hg"]),
       ("Heart rate", ["64", "62", "50/min"])], id="ich-1130")]),

matrix_mc(M(MOC, HANDOFF, "evaluate_outcomes", "difficult", NEURO, "mgmt ms"),
   "After clot evacuation, the ICU nurse receives a handoff. For each item, indicate whether it is adequate or needs "
   "correction.",
   ["Adequate", "Needs correction"],
   [("GCS trend with times and pupil sizes", 0, "Lets the next nurse detect change."),
    ("\"INR is normal now\" without the value or time", 1, "Give the value and time."),
    ("The blood pressure target and current nicardipine rate", 0, "Clear plan."),
    ("No mention that the client failed the swallow screen", 1, "A safety-critical item."),
    ("The family's question about goals of care is passed on with who will answer it", 0, "Continuity."),
    ("Handoff given at the bedside with a joint neuro check", 0, "Confirms a shared baseline.")],
   "A safe neuro handoff gives trended neurologic data, lab values with times, the BP plan and safety issues.",
   "Evaluate a neurologic handoff.",
   ex=[note("Postoperative day 1", "GCS 12, pupils equal. INR 1.2 at 0600.", id="ich-pod1")]),
]

# ================================================================================================
# Case 4: Cervical spinal cord injury, neurogenic shock, autonomic dysreflexia
# ================================================================================================
sci_tabs = [
    ("sci-notes", "Nurses' notes", [note("Trauma ICU",
        "A 24-year-old client dove into a shallow lake and has a complete C6 spinal cord injury. The client is in a "
        "cervical collar. Lungs have scattered crackles. The client can shrug the shoulders and bend the elbows but "
        "cannot move the hands or legs.", id="sci-note")]),
    ("sci-vitals", "Vital signs", [flow("Vital signs", ["Admission"], [
        ("Blood pressure", ["78/40 mm Hg"]),
        ("Heart rate", ["46/min"]),
        ("Skin", ["Warm, dry, flushed below the injury"]),
        ("Temperature", ["35.8 C (96.4 F)"]),
        ("Vital capacity", ["1.4 L"])], id="sci-flow")]),
]

sci_qs = [
hl_table(M(PA, SCI, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Blood pressure", "78/40 mm Hg"], True, "Hypotension."),
    (["Heart rate", "46/min"], True, "Bradycardia with hypotension is neurogenic shock."),
    (["Skin", "Warm and dry below the injury"], True, "Loss of sympathetic tone, unlike hypovolemic shock."),
    (["Vital capacity", "1.4 L"], True, "C6 injury weakens the respiratory muscles."),
    (["Movement", "Shrugs shoulders and bends elbows"], False, "Expected for a C6 level."),
    (["Temperature", "35.8 C"], True, "Poor temperature control below the injury.")],
   "Hypotension with bradycardia and warm skin is neurogenic shock; breathing is also at risk.",
   "Recognize cues of neurogenic shock."),

matrix_mc(M(PA, SCI, "analyze_cues", "difficult", NEURO, "ms"),
   "For each finding, indicate whether it fits neurogenic shock or hypovolemic shock.",
   ["Neurogenic shock", "Hypovolemic shock"],
   [("Bradycardia", 0, "Unopposed vagal tone."),
    ("Warm, dry skin", 0, "Vasodilation without sympathetic tone."),
    ("Cool, clammy skin with tachycardia", 1, "Compensation for blood loss."),
    ("Low blood pressure that does not respond to fluids alone", 0, "Needs vasopressors."),
    ("A falling hemoglobin", 1, "Blood loss.")],
   "Neurogenic shock is low pressure with a slow heart rate and warm skin.",
   "Analyze neurogenic versus hypovolemic shock."),

cloze(M(PA, SCI, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priorities are {{b1}} and {{b2}}.",
   [[("breathing, because the injury weakens the diaphragm and chest muscles", True, "C3 to C5 supply the diaphragm; edema can rise."),
     ("pressure injury prevention", False, "Important but less immediate."),
     ("bladder training", False, "Later."),
     ("nutrition", False, "Later.")],
    [("perfusion of the injured cord, by treating hypotension", True, "A MAP of 85 to 90 is often targeted."),
     ("pain control alone", False, "Not the main priority."),
     ("early walking", False, "Not possible."),
     ("lowering blood pressure", False, "Pressure is low.")]],
   "Protect breathing and cord perfusion first.",
   "Prioritize care in acute spinal cord injury."),

matrix_mc(M(PA, SCI, "generate_solutions", "difficult", NEURO, "ms ph"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Norepinephrine to keep the MAP at 85 to 90 mm Hg", 0, "Supports cord perfusion."),
    ("Atropine for symptomatic bradycardia", 0, "Treats vagal bradycardia, especially with suctioning."),
    ("Keep the spine aligned and use a log roll", 0, "Prevents further injury."),
    ("Vital capacity every 4 hours", 0, "Tracks respiratory function."),
    ("Large fluid boluses until the pressure is normal", 1, "Fluid alone does not fix vasodilation and risks overload."),
    ("Remove the collar to improve comfort", 1, "The spine must be protected.")],
   "Vasopressors and atropine, spinal precautions and respiratory monitoring.",
   "Generate solutions for neurogenic shock.",
   ex=[note("Plan", "Norepinephrine to MAP 85 to 90 for 7 days. Atropine at the bedside. Surgery for stabilization.",
       id="sci-plan")]),

sata(M(PA, SCI, "take_action", "difficult", NEURO, "ms"),
   "Six weeks later, on the rehabilitation unit, review the findings. Which actions should the nurse take first?",
   [("Sit the client upright with the legs down", True, "Lowers blood pressure by pooling blood."),
    ("Loosen tight clothing and the leg bag straps", True, "Removes a trigger."),
    ("Check the urinary catheter for kinks and the bladder for distension", True, "The bladder is the most common trigger."),
    ("Check blood pressure every 2 to 5 minutes", True, "Tracks response."),
    ("Lay the client flat and raise the legs", False, "Would raise blood pressure further."),
    ("Wait for the provider before doing anything", False, "Autonomic dysreflexia is an emergency the nurse acts on at once.")],
   "Autonomic dysreflexia: sit up, loosen clothing, find and remove the trigger (bladder, bowel, skin), monitor BP.",
   "Take action for autonomic dysreflexia.",
   ex=[flow("Rehabilitation, week 6", ["0900"], [
       ("Blood pressure", ["204/112 mm Hg (usual 96/60)"]),
       ("Heart rate", ["52/min"]),
       ("Client report", ["Pounding headache, stuffy nose"]),
       ("Skin", ["Flushed and sweating above the injury; cool below"]),
       ("Catheter", ["No urine in the bag for 3 hours"])], id="sci-ad")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "Before discharge, the client and partner describe how to prevent and treat autonomic dysreflexia. For each "
   "statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"We'll keep a schedule for bladder and bowel care.\"", 0, "Prevents the commonest triggers."),
    ("\"If a headache starts, I'll sit up and check my catheter first.\"", 0, "Correct first steps."),
    ("\"We'll carry the emergency card that explains dysreflexia.\"", 0, "Helps clinicians who may not recognize it."),
    ("\"A blood pressure of 150/90 is normal for me.\"", 1, "Their baseline is about 96/60; 150/90 may be dysreflexia."),
    ("\"I'll lie down flat if I feel a headache.\"", 1, "Sit upright instead."),
    ("\"We'll check my skin twice a day for pressure areas.\"", 0, "Skin injury is a trigger and a risk.")],
   "Prevention routines, the first steps, the emergency card and knowing the baseline BP.",
   "Evaluate autonomic dysreflexia teaching.",
   ex=[note("Discharge planning", "The client will live at home with a partner and attendant care.", id="sci-dc")]),
]

cases = [
    case("Myasthenic crisis versus cholinergic crisis", mg_tabs, mg_qs),
    case("Guillain-Barre syndrome with respiratory failure", gbs_tabs, gbs_qs),
    case("Intracerebral hemorrhage on warfarin", ich_tabs, ich_qs),
    case("Cervical spinal cord injury, neurogenic shock and autonomic dysreflexia", sci_tabs, sci_qs),
]

BUILT = build(116, "neuro-cases-mg-gbs-ich-sci",
              "Neurologic case studies: myasthenic crisis, Guillain-Barre syndrome, intracerebral hemorrhage and spinal "
              "cord injury",
              items, cases, case_conversion=True)
