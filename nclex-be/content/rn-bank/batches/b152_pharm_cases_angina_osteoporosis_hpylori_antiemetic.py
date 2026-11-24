"""Batch 152 (chunk 14: second cases for review-book subtopics with one case): four full NGN case studies
(CASE-MAP.md, book_topics.py).

Cases: stable angina with nitroglycerin, a beta blocker and a dangerous sildenafil interaction (angina drugs);
osteoporosis drugs with alendronate esophagitis, then denosumab and hypocalcemia; H. pylori ulcer with quadruple
therapy and long-term PPI effects (PUD and GERD drugs); postoperative nausea with antiemetic adverse effects
(metoclopramide dystonia, ondansetron QT prolongation, promethazine tissue injury). Each case walks the six CJMM
steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health
promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PHARM = "pharmacology"
ADVERSE = "adverse-effects-of-medications"
TECH = "medication-administration-technique"
GI_MEDS = "gi-medications"
ENDO_MEDS = "endocrine-medications"
ACS = "acute-coronary-syndrome"
LYTES = "electrolyte-imbalances"
TEACH = "health-teaching-and-behavior-change"
MED_SAFETY = "medication-safety-systems"
CHECKS = "pre-administration-medication-checks"

items = []

# ================================================================================================
# Case 1: Stable angina drugs
# ================================================================================================
an_tabs = [
    ("an-notes", "Clinic notes", [note("Cardiology clinic",
        "A 62-year-old client has chest pressure when climbing stairs that goes away with rest in 5 minutes. The "
        "client was prescribed sublingual nitroglycerin, metoprolol and isosorbide mononitrate. The client asks "
        "whether sildenafil from a friend is safe.", id="an-note")]),
    ("an-data", "Findings", [table("Findings", ["Result"], [
        ("Blood pressure", ["138/84 mm Hg"]),
        ("Heart rate", ["82/min"]),
        ("Stress test", ["Ischemia at moderate exercise"]),
        ("LDL", ["128 mg/dL"]),
        ("Smoking", ["10 cigarettes a day"])], id="an-table")]),
]

an_qs = [
hl_table(M(PA, ACS, "recognize_cues", "moderate", PHARM, "ms ph"),
   "Highlight the findings that fit stable angina rather than an acute coronary syndrome.",
   ["Finding", "Detail"],
   [(["Pain trigger", "Stairs"], True, "Exertion."),
    (["Relief", "Rest in 5 minutes"], True, "Predictable."),
    (["Stress test", "Ischemia with exercise"], True, "Demand ischemia."),
    (["Smoking", "10 a day"], False, "A risk factor, not a feature."),
    (["LDL", "128"], False, "A risk factor.")],
   "Predictable exertional pain relieved by rest is stable angina.",
   "Recognize cues of stable angina."),

matrix_mc(M(PPT, ADVERSE, "analyze_cues", "difficult", PHARM, "ph"),
   "For each angina drug, indicate how it relieves angina.",
   ["Dilates veins and lowers preload", "Lowers heart rate and contractility", "Blocks late sodium current"],
   [("Nitroglycerin", 0, "Nitrate."),
    ("Isosorbide mononitrate", 0, "Long-acting nitrate."),
    ("Metoprolol", 1, "Beta blocker."),
    ("Ranolazine", 2, "Anti-anginal without changing heart rate.")],
   "Each drug lowers oxygen demand differently.",
   "Analyze angina drugs."),

cloze(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Sildenafil must not be taken with nitrates because it can cause {{b1}}, and it must be avoided for at least "
   "{{b2}} before a nitrate.",
   [[("severe, life-threatening hypotension", True, "Both dilate vessels."),
     ("hypertension", False, "Opposite."),
     ("bleeding", False, "Not the issue."),
     ("hyperkalemia", False, "Not the issue.")],
    [("24 hours (48 hours for tadalafil)", True, "Washout."),
     ("1 hour", False, "Too short."),
     ("10 minutes", False, "Too short."),
     ("it is never a problem", False, "Wrong.")]],
   "PDE5 inhibitors with nitrates cause severe hypotension.",
   "Prioritize nitrate interactions."),

sata(M(PPT, TECH, "generate_solutions", "difficult", PHARM, "ph aha"),
   "Which teaching about sublingual nitroglycerin should the nurse give?",
   [("Sit down before taking it", True, "Dizziness."),
    ("Take one tablet under the tongue at the start of pain", True, "Dose."),
    ("Call 911 if pain is not better 5 minutes after the first tablet", True, "Possible MI."),
    ("Keep tablets in the original dark glass bottle", True, "Light and moisture."),
    ("Swallow the tablet with water", False, "Sublingual."),
    ("A headache means the tablets are dangerous; stop them", False, "Headache is common.")],
   "Sit, one tablet, call 911 at 5 minutes, store properly.",
   "Generate nitroglycerin teaching.",
   ex=[note("Plan", "Nitroglycerin 0.4 mg SL as needed. Isosorbide mononitrate each morning. Metoprolol.", id="an-plan")]),

sata(M(PPT, CHECKS, "take_action", "difficult", PHARM, "ph"),
   "Before giving metoprolol in the clinic, review the findings. Which actions should the nurse take?",
   [("Hold the dose", True, "Heart rate below 50 to 60."),
    ("Notify the provider", True, "Adjust."),
    ("Ask about dizziness or fainting", True, "Symptoms of bradycardia."),
    ("Give the dose because angina is the priority", False, "Unsafe."),
    ("Tell the client to stop metoprolol at home suddenly", False, "Rebound angina.")],
   "Bradycardia means hold and notify; never stop beta blockers suddenly.",
   "Take action before giving a beta blocker.",
   ex=[note("2 weeks later", "Heart rate 48/min, BP 104/60, feels tired.", id="an-2wk")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At 3 months, for each statement or finding, indicate whether it shows understanding or progress, or needs "
   "follow-up.",
   ["Understanding or progress", "Needs follow-up"],
   [("\"I take isosorbide in the morning with a nitrate-free period at night.\"", 0, "Prevents tolerance."),
    ("\"I quit smoking last month.\"", 0, "Risk reduction."),
    ("Angina once a week instead of daily", 0, "Improved."),
    ("\"My chest pain now comes at rest and lasts 20 minutes.\"", 1, "Unstable angina."),
    ("\"I told my friend I can't take the sildenafil.\"", 0, "Correct.")],
   "Improvement; pain at rest means unstable angina.",
   "Evaluate angina management.",
   ex=[note("3 months", "Clinic visit.", id="an-3m")]),
]

# ================================================================================================
# Case 2: Osteoporosis drugs
# ================================================================================================
op_tabs = [
    ("op-notes", "Clinic notes", [note("Primary care",
        "A 70-year-old client started alendronate 70 mg weekly a month ago. The client takes it at bedtime with "
        "milk and lies down. The client now has burning chest pain when swallowing.", id="op-note")]),
    ("op-data", "Findings", [table("Findings", ["Result"], [
        ("DEXA T-score", ["-3.0 (osteoporosis)"]),
        ("Swallowing", ["Painful"]),
        ("Calcium", ["9.0 mg/dL"]),
        ("Vitamin D", ["14 ng/mL (low)"]),
        ("Creatinine clearance", ["28 mL/min"]),
        ("Dental", ["Tooth extraction planned"])], id="op-table")]),
]

op_qs = [
hl_table(M(PPT, ADVERSE, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the findings that need follow-up.",
   ["Finding", "Result"],
   [(["Taking with milk at bedtime and lying down", "Yes"], True, "Wrong method; causes esophagitis and poor absorption."),
    (["Painful swallowing", "Yes"], True, "Esophagitis."),
    (["Vitamin D", "Low"], True, "Must be corrected."),
    (["Creatinine clearance", "28"], True, "Bisphosphonates avoided below 30 to 35."),
    (["Dental extraction", "Planned"], True, "Jaw osteonecrosis risk.")],
   "Wrong administration, low vitamin D, poor kidney function and dental work all matter.",
   "Recognize cues of bisphosphonate problems."),

matrix_mc(M(PPT, ENDO_MEDS, "analyze_cues", "difficult", PHARM, "ph"),
   "For each osteoporosis drug, indicate its key risk.",
   ["Esophagitis and jaw osteonecrosis", "Hypocalcemia and infections", "Blood clots and hot flashes", "Bone tumor risk in animal studies; daily injection"],
   [("Alendronate", 0, "Bisphosphonate."),
    ("Denosumab", 1, "RANKL inhibitor."),
    ("Raloxifene", 2, "SERM."),
    ("Teriparatide", 3, "Bone builder.")],
   "Each drug has a key risk.",
   "Analyze osteoporosis drugs."),

cloze(M(PPT, TECH, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Alendronate should be taken {{b1}} and the client must {{b2}}.",
   [[("first thing in the morning with a full glass of plain water", True, "Absorption."),
     ("at bedtime with milk", False, "Calcium blocks absorption."),
     ("with breakfast and coffee", False, "Food blocks absorption."),
     ("with orange juice", False, "Blocks absorption.")],
    [("stay upright and not eat for 30 minutes", True, "Prevents esophagitis."),
     ("lie down right away", False, "Reflux."),
     ("take an antacid with it", False, "Blocks."),
     ("crush the tablet", False, "Esophageal injury.")]],
   "Morning, water only, upright 30 minutes.",
   "Prioritize bisphosphonate administration."),

sata(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", PHARM, "ph"),
   "Because of the kidney function, the provider switches to denosumab every 6 months. Which parts of the plan "
   "should the nurse include?",
   [("Correct vitamin D and give calcium before starting", True, "Prevents hypocalcemia."),
    ("Check calcium before each dose", True, "Hypocalcemia."),
    ("Dental exam before starting", True, "Jaw osteonecrosis."),
    ("Do not stop or delay doses", True, "Stopping causes rapid bone loss and fractures."),
    ("Avoid all weight-bearing exercise", False, "Exercise helps bones."),
    ("Stop calcium once denosumab starts", False, "Calcium is needed.")],
   "Vitamin D and calcium, dental check and on-time doses.",
   "Generate the denosumab plan.",
   ex=[note("Plan", "Vitamin D 50,000 units weekly x8. Calcium 1,200 mg/day. Denosumab after dental work.", id="op-plan")]),

sata(M(PA, LYTES, "take_action", "difficult", PHARM, "ph"),
   "Two weeks after the first denosumab dose, review the findings. Which actions should the nurse take?",
   [("Notify the provider", True, "Symptomatic hypocalcemia."),
    ("Check for Chvostek and Trousseau signs", True, "Neuromuscular irritability."),
    ("Expect calcium replacement", True, "Treatment."),
    ("Place on a cardiac monitor if ordered", True, "QT prolongation."),
    ("Reassure that tingling is expected", False, "Not expected."),
    ("Give the next denosumab dose early", False, "Worsens.")],
   "Tingling and cramps with low calcium after denosumab need treatment.",
   "Take action on denosumab hypocalcemia.",
   ex=[flow("2 weeks", ["Before", "After"], [
       ("Calcium", ["9.0", "7.2 mg/dL"]),
       ("Client", ["Well", "Tingling lips, muscle cramps"]),
       ("Vitamin D", ["14", "Still low; not taken"])], id="op-2wk")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At 6 months, for each statement or finding, indicate whether it shows understanding or progress, or needs "
   "follow-up.",
   ["Understanding or progress", "Needs follow-up"],
   [("\"I take my calcium and vitamin D every day.\"", 0, "Correct."),
    ("\"I walk 30 minutes most days.\"", 0, "Weight-bearing."),
    ("Calcium 9.2 mg/dL before the second dose", 0, "Safe."),
    ("\"I'm thinking of skipping the next injection since I feel fine.\"", 1, "Rebound bone loss."),
    ("\"I removed the loose rugs at home.\"", 0, "Fall prevention.")],
   "Adherence and fall prevention; skipping doses is dangerous.",
   "Evaluate osteoporosis therapy.",
   ex=[note("6 months", "Clinic visit.", id="op-6m")]),
]

# ================================================================================================
# Case 3: H. pylori ulcer and long-term PPI effects
# ================================================================================================
hp_tabs = [
    ("hp-notes", "Clinic notes", [note("GI clinic",
        "A 58-year-old client has had burning upper abdominal pain that improves with food for 2 months and black "
        "stools twice. The client has taken omeprazole for 6 years for reflux and ibuprofen for back pain.",
        id="hp-note")]),
    ("hp-data", "Results", [table("Results", ["Result"], [
        ("Endoscopy", ["Duodenal ulcer, not bleeding now"]),
        ("H. pylori biopsy", ["Positive"]),
        ("Hemoglobin", ["10.4 g/dL"]),
        ("Magnesium", ["1.2 mg/dL (low)"]),
        ("Vitamin B12", ["Low"]),
        ("DEXA", ["Osteopenia"])], id="hp-table")]),
]

hp_qs = [
hl_table(M(PPT, GI_MEDS, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the findings that need treatment or follow-up.",
   ["Finding", "Result"],
   [(["H. pylori", "Positive"], True, "Must be eradicated."),
    (["Ibuprofen use", "Yes"], True, "Ulcer risk."),
    (["Low magnesium", "1.2"], True, "Long-term PPI effect."),
    (["Low B12", "Yes"], True, "Long-term PPI effect."),
    (["Osteopenia", "Yes"], True, "PPI fracture risk.")],
   "H. pylori and NSAIDs cause ulcers; long-term PPIs cause low magnesium, B12 and bone loss.",
   "Recognize cues in H. pylori ulcer disease."),

matrix_mc(M(PPT, GI_MEDS, "analyze_cues", "difficult", PHARM, "ph"),
   "For each drug, indicate its role or key teaching.",
   ["Reduces acid production", "Coats the ulcer", "Kills H. pylori", "Neutralizes acid quickly"],
   [("Omeprazole", 0, "PPI."),
    ("Sucralfate", 1, "Barrier."),
    ("Clarithromycin, amoxicillin or metronidazole", 2, "Antibiotics."),
    ("Bismuth subsalicylate", 2, "Part of quadruple therapy; black stools."),
    ("Aluminum hydroxide", 3, "Antacid.")],
   "Combination therapy heals ulcers and kills H. pylori.",
   "Analyze ulcer drugs."),

cloze(M(PPT, GI_MEDS, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The plan is {{b1}} for 14 days, and the client should {{b2}}.",
   [[("bismuth quadruple therapy: PPI, bismuth, metronidazole and tetracycline", True, "Effective first-line."),
     ("a PPI alone", False, "Does not eradicate."),
     ("antacids only", False, "Does not eradicate."),
     ("one antibiotic for 3 days", False, "Resistance.")],
    [("stop ibuprofen and use acetaminophen", True, "Removes a cause."),
     ("double the ibuprofen", False, "Worsens."),
     ("add aspirin", False, "Worsens."),
     ("take ibuprofen with the PPI", False, "Still risky.")]],
   "Quadruple therapy and stop NSAIDs.",
   "Prioritize H. pylori treatment."),

sata(M(PPT, GI_MEDS, "generate_solutions", "difficult", PHARM, "ph"),
   "Which teaching about the treatment should the nurse give?",
   [("PPI before breakfast", True, "Best effect."),
    ("Bismuth darkens stool", True, "Expected; not bleeding."),
    ("No alcohol", True, "Disulfiram reaction."),
    ("Sucralfate apart from other drugs", True, "Binds other drugs."),
    ("Stop the antibiotics as soon as the pain eases", False, "Resistance."),
    ("Take tetracycline with a glass of milk to protect the stomach", False, "Calcium blocks absorption.")],
   "Timing, expected black stool and interactions.",
   "Generate ulcer treatment teaching.",
   ex=[note("Plan", "Quadruple therapy 14 days. Stop ibuprofen. Magnesium and B12 replacement.", id="hp-plan")]),

sata(M(PPT, GI_MEDS, "take_action", "difficult", PHARM, "ph"),
   "On day 5, the client calls. Review the findings. Which actions should the nurse take?",
   [("Hemoglobin check today", True, "Possible bleeding."),
    ("Ask about dizziness", True, "Blood loss."),
    ("Explain tarry stool signs", True, "Teaching."),
    ("Tell the client black stool is always from bismuth", False, "Could be bleeding."),
    ("Stop all the ulcer medicines until the stool is normal", False, "Not appropriate.")],
   "Black stool can be bismuth or bleeding; dizziness makes bleeding more likely.",
   "Take action on black stools during treatment.",
   ex=[note("Phone call", "Stools are black and sticky; client feels dizzy when standing.", id="hp-call")]),

matrix_mc(M(PPT, GI_MEDS, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "At 8 weeks, for each finding or statement, indicate whether it shows a good outcome or needs follow-up.",
   ["Good outcome", "Needs follow-up"],
   [("Negative H. pylori breath test (PPI stopped 2 weeks before)", 0, "Eradicated."),
    ("No more pain", 0, "Healed."),
    ("Magnesium 2.0 mg/dL", 0, "Corrected."),
    ("\"I'll stay on omeprazole forever just in case.\"", 1, "Use the lowest dose for the shortest time."),
    ("\"I use acetaminophen for my back now.\"", 0, "Safer.")],
   "Eradicated; long-term PPI use should be reviewed.",
   "Evaluate ulcer treatment.",
   ex=[note("8 weeks", "Follow-up.", id="hp-8wk")]),
]

# ================================================================================================
# Case 4: Antiemetic adverse effects
# ================================================================================================
ae_tabs = [
    ("ae-notes", "Surgical unit notes", [note("Day 1 after bowel surgery",
        "A 24-year-old client has nausea after surgery. Ondansetron was given twice, metoclopramide 10 mg IV an "
        "hour ago, and promethazine was ordered IV push through a hand IV.", id="ae-note")]),
    ("ae-data", "Findings", [flow("Findings", ["Before", "1 hour after metoclopramide"], [
        ("Neck", ["Normal", "Twisted to one side, cannot straighten"]),
        ("Eyes", ["Normal", "Rolling upward"]),
        ("ECG QTc", ["440 ms", "498 ms"]),
        ("Potassium", ["3.6", "3.1 mEq/L"]),
        ("Client", ["Nauseated", "Frightened, restless"])], id="ae-flow")]),
]

ae_qs = [
hl_table(M(PPT, ADVERSE, "recognize_cues", "moderate", PHARM, "ph"),
   "Highlight the findings that are adverse drug effects needing action.",
   ["Finding", "After"],
   [(["Neck twisting, eyes rolling", "Yes"], True, "Acute dystonia from metoclopramide."),
    (["QTc", "498 ms"], True, "Ondansetron and low potassium prolong QT."),
    (["Potassium", "3.1"], True, "Raises dysrhythmia risk."),
    (["Restlessness", "Yes"], True, "Akathisia or fear."),
    (["Nausea before", "Yes"], False, "The original problem.")],
   "Metoclopramide causes dystonia; ondansetron prolongs QT.",
   "Recognize cues of antiemetic adverse effects."),

matrix_mc(M(PPT, GI_MEDS, "analyze_cues", "difficult", PHARM, "ph"),
   "For each antiemetic, indicate its key risk.",
   ["QT prolongation", "Extrapyramidal effects and tardive dyskinesia", "Tissue injury if given IV and sedation", "Drowsiness and dry mouth"],
   [("Ondansetron", 0, "5-HT3 antagonist."),
    ("Metoclopramide", 1, "Dopamine blocker."),
    ("Promethazine", 2, "Vesicant; respiratory depression in children under 2."),
    ("Meclizine", 3, "Antihistamine.")],
   "Each antiemetic has a key risk.",
   "Analyze antiemetic risks."),

cloze(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", PHARM, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, and for the dystonia expect {{b2}}.",
   [[("hold further metoclopramide and ondansetron and notify the provider", True, "Stop the causes."),
     ("give more metoclopramide", False, "Worsens."),
     ("give the promethazine push now", False, "Adds risk."),
     ("restrain the neck", False, "Harmful.")],
    [("diphenhydramine or benztropine IV", True, "Reverses dystonia."),
     ("haloperidol", False, "Worsens."),
     ("naloxone", False, "Not an opioid problem."),
     ("calcium", False, "Not the treatment.")]],
   "Stop the drugs; give an anticholinergic.",
   "Prioritize antiemetic toxicity."),

sata(M(SIC, MED_SAFETY, "generate_solutions", "difficult", PHARM, "ph"),
   "The nurse is asked to give promethazine IV push through the hand IV. Which actions are safest?",
   [("Question the order", True, "Severe tissue injury risk."),
    ("If IV: large vein, dilute, slow", True, "Safety measures."),
    ("Stop for burning", True, "Extravasation."),
    ("Give it IV push quickly into the hand vein as ordered", False, "Risk of gangrene."),
    ("Give it into an artery for a faster effect on the nausea", False, "Never intra-arterial.")],
   "Avoid IV promethazine when possible; if used, take precautions.",
   "Generate safe promethazine administration.",
   ex=[note("Order", "Promethazine 25 mg IV push now via hand IV.", id="ae-order")]),

sata(M(PA, LYTES, "take_action", "difficult", PHARM, "ph"),
   "For the QT prolongation, which actions should the nurse take?",
   [("Place on continuous cardiac monitoring", True, "Torsades risk."),
    ("Expect potassium and magnesium replacement", True, "Correct electrolytes."),
    ("Review other QT-prolonging drugs", True, "Additive."),
    ("Give another ondansetron dose", False, "Worsens."),
    ("Ignore it because the client is young", False, "Unsafe.")],
   "Monitor, replace electrolytes and remove QT drugs.",
   "Take action on QT prolongation.",
   ex=[note("After diphenhydramine", "Neck relaxed.", id="ae-after")]),

matrix_mc(M(PPT, GI_MEDS, "evaluate_outcomes", "difficult", PHARM, "ph"),
   "The next day, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("QTc 446 ms", 0, "Normalized."),
    ("Potassium 4.1 mEq/L", 0, "Corrected."),
    ("No further dystonia", 0, "Resolved."),
    ("Nausea controlled with dexamethasone and scopolamine patch", 0, "Alternative plan."),
    ("Chart still lists metoclopramide as a PRN option", 1, "Should be flagged.")],
   "Improved; the chart must reflect the reaction.",
   "Evaluate antiemetic changes.",
   ex=[note("Day 2", "Eating clear fluids.", id="ae-day2")]),
]

cases = [
    case("Stable angina drugs and a nitrate interaction", an_tabs, an_qs),
    case("Osteoporosis drugs from alendronate to denosumab", op_tabs, op_qs),
    case("H. pylori ulcer and long-term PPI effects", hp_tabs, hp_qs),
    case("Antiemetic adverse effects after surgery", ae_tabs, ae_qs),
]

BUILT = build(152, "pharm-cases-angina-osteoporosis-hpylori-antiemetic",
              "Pharmacology case studies: angina drugs, osteoporosis drugs, H. pylori and PPIs, and antiemetic "
              "adverse effects",
              items, cases, case_conversion=True)
