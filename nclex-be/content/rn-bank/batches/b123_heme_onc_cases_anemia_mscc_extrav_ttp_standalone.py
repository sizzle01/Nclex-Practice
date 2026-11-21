"""Batch 123 (case conversion, chunk 6: renal, hematologic, oncologic): four full NGN case studies and four
standalone items (CASE-MAP.md, book_topics.py).

Cases: pernicious anemia compared with iron deficiency (hematopoietic medications: cyanocobalamin and iron); malignant
spinal cord compression; vesicant chemotherapy extravasation with hazardous-drug safety; thrombotic thrombocytopenic
purpura with plasma exchange. Standalone: a hyperkalemia bowtie in kidney failure, a trend table for a client after
chemotherapy, a priority question on an oncology unit and a delegation question on a renal unit. Each case walks the
six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health
promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
HEME = "hematologic-oncologic"
RENAL = "renal-urinary"
ANEMIA = "anemias"
HEME_MEDS = "hematologic-medications"
ONC_EM = "oncologic-emergencies"
CANCER = "cancer-care"
CHEMO = "chemotherapy-and-radiation-care"
HAZMAT = "hazardous-materials-and-radiation-safety"
BLEED = "bleeding-and-clotting-disorders"
LABS = "laboratory-values-and-monitoring"
LYTES = "electrolyte-imbalances"
PAIN = "pain-assessment-and-comfort"
OPIOID = "opioid-safety"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
NUTR = "nutrition-and-therapeutic-diets"
ADVOCACY = "client-rights-and-advocacy"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, LYTES, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "After reading the note, complete the diagram with the condition the client is most likely experiencing, the 2 "
   "actions the nurse should take first, and the 2 parameters to monitor.",
   [("Severe hyperkalemia", True, "Potassium 7.2 with peaked T waves and a wide QRS."),
    ("Hypocalcemia", False, "Would not explain the ECG pattern this strongly."),
    ("Fluid overload", False, "Not the immediate threat."),
    ("Uremic pericarditis", False, "No rub or chest pain.")],
   [("Give calcium gluconate IV as prescribed", True, "Stabilizes the heart membrane in minutes."),
    ("Give regular insulin with dextrose IV as prescribed", True, "Shifts potassium into cells."),
    ("Give sodium polystyrene as the only treatment", False, "Too slow alone for these ECG changes."),
    ("Encourage the client to drink orange juice", False, "Adds potassium."),
    ("Start lactated Ringer's at a fast rate", False, "Contains potassium and adds fluid.")],
   [("Cardiac rhythm", True, "Detects dysrhythmias."),
    ("Glucose after insulin", True, "Hypoglycemia risk in kidney failure."),
    ("Hemoglobin level each morning", False, "Not related."),
    ("Body weight once a week", False, "Not urgent."),
    ("Urine specific gravity each shift", False, "Not useful here.")],
   "Peaked T waves and a wide QRS with potassium 7.2 need calcium and an insulin shift now.",
   "Recognize and act on severe hyperkalemia.",
   ex=[note("Renal unit", "A client with kidney failure missed dialysis for 4 days. Potassium 7.2 mEq/L. ECG: peaked T waves "
       "and a widened QRS. Heart rate 52/min. Glucose 104 mg/dL.", id="sa-k-note")]),

dtable(M(RRP, LABS, "evaluate_outcomes", "difficult", HEME, "ms lab"),
   "A client receiving chemotherapy has blood counts each week. For each trend, choose the interpretation.",
   ["Measure", "Days 1, 7, 10, 14", "Interpretation"],
   [(["ANC (cells/mm3)", "4,200, 1,100, 380, 900"], [
       ("Nadir around day 10; neutropenic precautions now", True, "The lowest point is when infection risk peaks."),
       ("Recovered; no precautions needed", False, "380 is severe neutropenia."),
       ("A lab error", False, "An expected nadir pattern.")]),
    (["Platelets (per mm3)", "240,000, 96,000, 42,000, 70,000"], [
       ("Bleeding precautions, near the nadir", True, "Below 50,000 bleeding risk rises."),
       ("Safe for IM injections", False, "Avoid injections."),
       ("Requires platelet transfusion now", False, "Usually only below 10,000 or with bleeding.")]),
    (["Hemoglobin (g/dL)", "12.8, 11.9, 10.6, 10.2"], [
       ("A slow fall; monitor for fatigue", True, "Red cells live longer, so they fall slowly."),
       ("Acute hemorrhage", False, "Too slow and gradual."),
       ("A transfusion is required now", False, "Not at 10.2 without symptoms.")])],
   "Neutrophils and platelets reach their nadir around days 7 to 14; precautions match the counts.",
   "Evaluate blood counts after chemotherapy.",
   ex=[flow("Weekly counts", ["Day 1", "Day 7", "Day 10", "Day 14"], [
       ("ANC", ["4,200", "1,100", "380", "900"]),
       ("Platelets", ["240,000", "96,000", "42,000", "70,000"]),
       ("Hemoglobin", ["12.8", "11.9", "10.6", "10.2"])], id="sa-nadir")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", HEME, "mgmt ms"),
   "On an oncology unit at the start of the shift, four clients have needs. Whose bedside should the nurse go to "
   "first?",
   [("A client with leukemia whose platelet count is 18,000/mm3 and who has a few petechiae on the shins", False,
     "Expected with this count; maintain bleeding precautions."),
    ("A client with lung cancer who reports new back pain and leg heaviness since the night", True,
     "Possible spinal cord compression; delay causes permanent paralysis."),
    ("A client with lymphoma whose antiemetic is due before the 0900 chemotherapy", False,
     "Time-sensitive, but there is time."),
    ("A client with colon cancer who asks to talk about stopping treatment and going home", False,
     "Important; arrange time after the urgent client.")],
   "New back pain with leg symptoms in a client with cancer is cord compression until proven otherwise.",
   "Prioritize clients on an oncology unit."),

sata(M(MOC, DELEG, "take_action", "difficult", RENAL, "mgmt"),
   "Which tasks can the RN delegate to the UAP on a renal unit?",
   [("Weighing clients on the same scale before breakfast", True, "Routine measurement."),
    ("Measuring and recording urine output", True, "Routine measurement."),
    ("Reporting a client's report of a missing fistula thrill", False, "Assessing the thrill is the RN's role."),
    ("Teaching a client on fluid restriction how to spread the allowance across the day", False, "Teaching stays with the RN."),
    ("Removing the peritoneal dialysis effluent bag and judging whether it is cloudy", False, "Judging effluent is assessment.")],
   "UAPs weigh clients and measure output; assessment and teaching stay with the RN.",
   "Delegate on a renal unit."),
]

# ================================================================================================
# Case 1: Pernicious anemia compared with iron deficiency
# ================================================================================================
b12_tabs = [
    ("b12-notes", "Clinic notes", [note("Primary care",
        "A 68-year-old client reports 4 months of tiredness, a sore tongue, and numbness and tingling in both feet. "
        "The client has stumbled twice in the dark and has been more forgetful. The client takes metformin and "
        "omeprazole and has autoimmune hypothyroidism.", id="b12-note")]),
    ("b12-labs", "Labs and exam", [table("Findings", ["Result"], [
        ("Hemoglobin", ["9.4 g/dL"]),
        ("MCV", ["114 fL (80 to 100)"]),
        ("Vitamin B12", ["110 pg/mL (low)"]),
        ("Ferritin", ["140 ng/mL (normal)"]),
        ("Intrinsic factor antibodies", ["Positive"]),
        ("Tongue", ["Smooth, beefy red"]),
        ("Romberg test", ["Positive"]),
        ("Vibration sense in the feet", ["Reduced"])], id="b12-table")]),
]

b12_qs = [
hl_table(M(PA, ANEMIA, "recognize_cues", "moderate", HEME, "ms lab"),
   "Highlight the findings that point to vitamin B12 deficiency.",
   ["Finding", "Result"],
   [(["MCV", "114 fL"], True, "Large red cells (macrocytic)."),
    (["Vitamin B12", "110 pg/mL"], True, "Low."),
    (["Tongue", "Smooth, beefy red"], True, "Glossitis."),
    (["Feet", "Numbness, poor vibration sense, positive Romberg"], True, "Nerve damage from B12 deficiency."),
    (["Ferritin", "140 ng/mL"], False, "Normal iron stores.")],
   "Macrocytic anemia with neurologic signs and glossitis is B12 deficiency.",
   "Recognize cues of vitamin B12 deficiency."),

matrix_mr(M(PA, ANEMIA, "analyze_cues", "difficult", HEME, "ms"),
   "For each finding, select the anemias it is typical of. Each row may have more than one answer.",
   ["Iron deficiency", "Vitamin B12 deficiency", "Folate deficiency"],
   [("Small red cells (low MCV)", {0}, "Microcytic."),
    ("Large red cells (high MCV)", {1, 2}, "Megaloblastic."),
    ("Numbness and poor balance", {1}, "Only B12 damages nerves."),
    ("Smooth, sore tongue", {0, 1, 2}, "All three can cause glossitis."),
    ("Spoon-shaped nails and craving ice", {0}, "Iron deficiency.")],
   "Neurologic signs separate B12 from folate deficiency.",
   "Analyze the types of anemia."),

cloze(M(PA, ANEMIA, "prioritize_hypotheses", "difficult", HEME, "ms ph"),
   "Complete the sentences by choosing from the lists of options.",
   "The most likely cause is {{b1}}, made worse by {{b2}}. Giving folic acid alone would be dangerous because it "
   "{{b3}}.",
   [[("pernicious anemia (no intrinsic factor)", True, "Positive antibodies and autoimmune history."),
     ("a vegan diet", False, "Not described."),
     ("blood loss", False, "Ferritin is normal."),
     ("chronic kidney disease", False, "Would be normocytic.")],
    [("metformin and omeprazole, which reduce B12 absorption", True, "Both lower absorption."),
     ("thyroid replacement", False, "Not a cause."),
     ("a high-iron diet", False, "Not a cause."),
     ("too much vitamin C", False, "Not a cause.")],
    [("corrects the anemia while the nerve damage keeps progressing", True, "Masks B12 deficiency."),
     ("causes bleeding", False, "Not the issue."),
     ("raises potassium", False, "Not the issue."),
     ("causes iron overload", False, "Not the issue.")]],
   "Pernicious anemia needs B12 for life; folate alone hides the anemia while nerves worsen.",
   "Prioritize the cause of the anemia."),

matrix_mc(M(PPT, HEME_MEDS, "generate_solutions", "difficult", HEME, "ph"),
   "For each order, indicate whether it is appropriate or not appropriate.",
   ["Appropriate", "Not appropriate"],
   [("Cyanocobalamin 1,000 mcg IM daily for a week, then weekly, then monthly", 0, "Bypasses the missing intrinsic factor."),
    ("Lifelong B12 replacement", 0, "Pernicious anemia does not go away."),
    ("Potassium level in the first days of treatment", 0, "New red cell production can lower potassium."),
    ("Oral iron three times daily", 1, "Iron stores are normal."),
    ("Fall prevention with a night light", 0, "Poor position sense in the dark."),
    ("Oral B12 at a standard daily dose as the only treatment", 1, "Absorption is poor without intrinsic factor.")],
   "IM B12 for life, potassium monitoring and fall prevention.",
   "Generate the pernicious anemia plan.",
   ex=[note("Diagnosis", "Pernicious anemia. Start cyanocobalamin IM.", id="b12-dx")]),

sata(M(PPT, HEME_MEDS, "take_action", "difficult", HEME, "ph"),
   "On day 3 of B12 injections, the client reports muscle weakness and palpitations. Review the findings. Which "
   "actions should the nurse take?",
   [("Report the potassium level", True, "Hypokalemia from rapid red cell production."),
    ("Place the client on a cardiac monitor as prescribed", True, "Risk of dysrhythmias."),
    ("Expect potassium replacement", True, "Corrects the shift."),
    ("Stop the B12 permanently because of the reaction", False, "This is an expected effect of recovery, not an allergy."),
    ("Give an extra B12 injection to speed up recovery of the nerves", False, "Not indicated.")],
   "Red cell production uses potassium; replace it and monitor.",
   "Take action on hypokalemia during B12 replacement.",
   ex=[flow("Day 3", ["Day 0", "Day 3"], [
       ("Potassium", ["4.1", "3.0 mEq/L"]),
       ("Reticulocytes", ["0.4%", "6.8%"]),
       ("ECG", ["Normal", "U waves, occasional PVCs"])], id="b12-day3")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", HEME, "ph"),
   "At 3 months, for each statement or finding, indicate whether it shows improvement or understanding, or needs "
   "follow-up.",
   ["Improvement or understanding", "Needs follow-up"],
   [("Hemoglobin 12.6 g/dL, MCV 92 fL", 0, "Anemia corrected."),
    ("\"My tongue feels normal now.\"", 0, "Resolved."),
    ("\"I'll stop the shots once my blood is normal.\"", 1, "Lifelong treatment."),
    ("\"Some numbness is still there, but my balance is better.\"", 0, "Nerve recovery is slow and may be incomplete."),
    ("\"I'll keep up the gastroscopy my doctor suggested.\"", 0, "Pernicious anemia raises gastric cancer risk.")],
   "Blood counts and symptoms improved; B12 is lifelong.",
   "Evaluate pernicious anemia treatment.",
   ex=[note("3 months", "Monthly B12 injections.", id="b12-3m")]),
]

# ================================================================================================
# Case 2: Malignant spinal cord compression
# ================================================================================================
mscc_tabs = [
    ("mscc-notes", "Oncology notes", [note("Phone call and arrival",
        "A 63-year-old client with prostate cancer that has spread to the bones calls about 2 weeks of worsening "
        "mid-back pain that is worse lying down and at night. Today both legs feel heavy and the client has not "
        "passed urine since the morning.", id="mscc-note")]),
    ("mscc-exam", "Exam", [table("Findings", ["Result"], [
        ("Back pain", ["8/10 at the T8 level, worse lying flat and with coughing"]),
        ("Leg strength", ["4/5 both legs (5/5 last week)"]),
        ("Sensation", ["Reduced below the umbilicus"]),
        ("Bladder scan", ["780 mL"]),
        ("Rectal tone", ["Reduced"]),
        ("Vital signs", ["Stable"])], id="mscc-table")]),
]

mscc_qs = [
hl_table(M(PA, ONC_EM, "recognize_cues", "moderate", HEME, "ms"),
   "Highlight the findings that require emergency action.",
   ["Finding", "Result"],
   [(["Back pain", "Worse lying flat and with coughing"], True, "Classic for spinal metastasis."),
    (["Leg strength", "4/5, new"], True, "Motor loss."),
    (["Sensation", "Reduced below the umbilicus"], True, "A sensory level."),
    (["Bladder scan", "780 mL"], True, "Urinary retention."),
    (["Rectal tone", "Reduced"], True, "Autonomic loss."),
    (["Vital signs", "Stable"], False, "Reassuring but does not change the urgency.")],
   "Back pain with leg weakness, a sensory level and retention is spinal cord compression.",
   "Recognize cues of malignant spinal cord compression."),

cloze(M(PA, ONC_EM, "analyze_cues", "difficult", HEME, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's {{b1}} is the earliest symptom, while {{b2}} are late signs. The best outcome is predicted by {{b3}}.",
   [[("back pain", True, "Present in most clients weeks before weakness."),
     ("urinary retention", False, "A late sign."),
     ("leg weakness", False, "Later."),
     ("loss of rectal tone", False, "Late.")],
    [("bladder and bowel dysfunction", True, "Autonomic signs are late."),
     ("night pain", False, "Early."),
     ("pain with coughing", False, "Early."),
     ("stable vital signs", False, "Not a sign.")],
    [("the ability to walk when treatment starts", True, "Clients who walk in usually walk out."),
     ("the type of pain medicine", False, "Not predictive."),
     ("the age of the client", False, "Less important."),
     ("the bladder volume", False, "Not the main predictor.")]],
   "Early recognition from back pain preserves walking.",
   "Analyze spinal cord compression."),

matrix_mc(M(PA, ONC_EM, "prioritize_hypotheses", "difficult", HEME, "ms"),
   "For each action, indicate its priority.",
   ["Do first", "Do soon", "Not indicated"],
   [("Give dexamethasone IV as prescribed", 0, "Reduces cord swelling."),
    ("Arrange a whole-spine MRI within 24 hours, ideally sooner", 0, "Confirms the level."),
    ("Insert a urinary catheter for the retention", 1, "Relieves the bladder."),
    ("Ask the client to walk to the bathroom to test strength", 2, "Unstable spine; keep flat until assessed."),
    ("Plan radiation or surgery after the MRI", 1, "Definitive treatment.")],
   "Steroids and MRI first, then bladder care and definitive treatment.",
   "Prioritize spinal cord compression care."),

sata(M(SIC, "safe-client-handling", "generate_solutions", "difficult", HEME, "ms"),
   "Which interventions should the nurse include until the spine is shown to be stable?",
   [("Flat bed rest with log rolling", True, "Prevents further cord injury."),
    ("Neurologic checks of leg strength and sensation every 4 hours", True, "Detects worsening."),
    ("VTE prophylaxis as prescribed", True, "Immobility."),
    ("Raise the head of the bed to 60 degrees for comfort", False, "Spine may be unstable."),
    ("Let the client sit on the edge of the bed to eat meals", False, "Unsafe until the spine is cleared.")],
   "Spinal precautions, neurologic checks and VTE prophylaxis.",
   "Generate the spinal precautions plan.",
   ex=[note("MRI", "Compression at T8 by a vertebral metastasis. Spine possibly unstable. Radiation planned.", id="mscc-mri")]),

sata(M(PPT, "endocrine-medications", "take_action", "difficult", HEME, "ph"),
   "The client receives dexamethasone 16 mg daily. Which actions should the nurse take?",
   [("Check blood glucose regularly", True, "Steroids raise glucose."),
    ("Give a proton pump inhibitor as prescribed", True, "Protects the stomach."),
    ("Give the dose in the morning", True, "Less insomnia."),
    ("Watch for mood changes", True, "Steroid psychosis."),
    ("Stop the dexamethasone suddenly after radiation", False, "It must be tapered."),
    ("Expect the steroid to reverse any weakness in a day without radiation", False, "Radiation or surgery is still needed.")],
   "High-dose steroids need glucose, stomach and mood monitoring and a taper.",
   "Take action with high-dose dexamethasone.",
   ex=[note("Day 2", "Radiation started. Catheter in place.", id="mscc-day2")]),

matrix_mc(M(PA, ONC_EM, "evaluate_outcomes", "difficult", HEME, "ms"),
   "After radiation, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Leg strength 5/5", 0, "Function preserved."),
    ("Back pain 3/10", 0, "Improved."),
    ("Glucose 312 mg/dL", 1, "Steroid-induced hyperglycemia."),
    ("Walking with a frame", 0, "Mobility kept."),
    ("New numbness in both hands", 1, "A new level needs evaluation."),
    ("Voiding after the catheter is removed", 0, "Bladder recovery.")],
   "Function and pain improved; hyperglycemia and new numbness need follow-up.",
   "Evaluate spinal cord compression treatment.",
   ex=[note("Day 10", "Radiation completed. Dexamethasone taper started.", id="mscc-day10")]),
]

# ================================================================================================
# Case 3: Vesicant extravasation and hazardous-drug safety
# ================================================================================================
ext_tabs = [
    ("ext-notes", "Infusion center notes", [note("Chemotherapy chair",
        "A 47-year-old client with breast cancer is receiving doxorubicin by IV push through a peripheral IV in the "
        "left forearm because the port is blocked. Halfway through, the client says the site burns.", id="ext-note")]),
    ("ext-site", "IV site", [table("IV site", ["Before", "Now"], [
        ("Blood return", ["Brisk", "None"]),
        ("Site", ["No swelling", "Swollen, pale, cool"]),
        ("Client", ["Comfortable", "Burning, stinging"]),
        ("Volume given", ["-", "About half the syringe"])], id="ext-table")]),
]

ext_qs = [
hl_table(M(PPT, CHEMO, "recognize_cues", "moderate", HEME, "ph"),
   "Highlight the findings that suggest extravasation.",
   ["Finding", "Now"],
   [(["Blood return", "None"], True, "The cannula may be out of the vein."),
    (["Site", "Swollen, pale, cool"], True, "Fluid in the tissues."),
    (["Client", "Burning, stinging"], True, "Vesicant injury."),
    (["Drug", "Doxorubicin"], True, "A vesicant that can cause tissue death.")],
   "Burning, swelling and loss of blood return with a vesicant is extravasation.",
   "Recognize cues of extravasation."),

matrix_mc(M(PPT, CHEMO, "analyze_cues", "difficult", HEME, "ph"),
   "For each drug, indicate whether it is a vesicant or not a vesicant.",
   ["Vesicant", "Not a vesicant"],
   [("Doxorubicin", 0, "Anthracycline vesicant."),
    ("Vincristine", 0, "Vinca alkaloid vesicant."),
    ("Mechlorethamine", 0, "A vesicant."),
    ("Ondansetron", 1, "Antiemetic."),
    ("Cyclophosphamide", 1, "Not a vesicant.")],
   "Anthracyclines and vinca alkaloids are vesicants.",
   "Analyze vesicant risk."),

order(M(PPT, CHEMO, "prioritize_hypotheses", "difficult", HEME, "ph"),
   "Place the nurse's actions in order.",
   ["Stop the injection",
    "Leave the cannula in and try to aspirate the drug",
    "Remove the cannula",
    "Apply a cold pack as prescribed for doxorubicin",
    "Notify the provider and document the site with a photograph"],
   "Stop, aspirate through the cannula, remove it, apply the correct compress and notify.",
   "Prioritize extravasation steps."),

sata(M(PPT, CHEMO, "generate_solutions", "difficult", HEME, "ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Dexrazoxane IV within 6 hours as prescribed", True, "The antidote for anthracycline extravasation."),
    ("Elevate the arm for 48 hours", True, "Reduces swelling."),
    ("Mark the site and check it daily", True, "Tracks tissue damage."),
    ("Warm compresses for doxorubicin", False, "Heat is used for vinca alkaloids, not anthracyclines."),
    ("Restart the doxorubicin in the same arm below the site", False, "Use another site.")],
   "Dexrazoxane, cold, elevation and monitoring for an anthracycline.",
   "Generate the extravasation plan.",
   ex=[note("Orders", "Dexrazoxane. Restart chemotherapy through a new site in the right arm.", id="ext-orders")]),

sata(M(SIC, HAZMAT, "take_action", "difficult", HEME, "ph"),
   "During cleanup, a small amount of doxorubicin spills on the floor. Which actions should the nurse take?",
   [("Wear two pairs of chemotherapy gloves and a gown", True, "Protection."),
    ("Use the chemotherapy spill kit", True, "Correct equipment."),
    ("Discard the waste in the chemotherapy waste container", True, "Hazardous waste."),
    ("Wipe it up with paper towels and put them in the regular trash", False, "Contaminates staff and the environment."),
    ("Ask a pregnant coworker to help clean the spill", False, "Avoid exposing a pregnant worker.")],
   "Personal protection, a spill kit and hazardous waste disposal.",
   "Take action for a chemotherapy spill.",
   ex=[note("After", "The syringe tip dripped on the floor when the cannula was removed.", id="ext-spill")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", HEME, "ph"),
   "At follow-up, for each statement, indicate whether it shows understanding or needs more teaching about home "
   "safety after chemotherapy.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll flush the toilet twice with the lid down for 48 hours after treatment.\"", 0, "Body fluids carry the drug."),
    ("\"I'll wash my clothes with body fluids on them separately.\"", 0, "Correct."),
    ("\"I'll call if the site blisters or the pain worsens.\"", 0, "Tissue damage can develop over days."),
    ("\"I'll put a heating pad on the arm to soothe it.\"", 1, "Heat is wrong for doxorubicin."),
    ("\"My partner and I don't need any protection during sex this week.\"", 1, "Use condoms; the drug is in body fluids.")],
   "Home precautions for body fluids and site care.",
   "Evaluate home chemotherapy safety teaching.",
   ex=[note("Day 7", "Mild redness at the site, no blistering.", id="ext-day7")]),
]

# ================================================================================================
# Case 4: Thrombotic thrombocytopenic purpura
# ================================================================================================
ttp_tabs = [
    ("ttp-notes", "Emergency notes", [note("Emergency department",
        "A 32-year-old client comes in with tiredness, bruises on the legs and a headache. The partner says the "
        "client has been confused since the morning and had a brief episode of slurred speech.", id="ttp-note")]),
    ("ttp-labs", "Labs", [table("Labs", ["Result"], [
        ("Platelets", ["14,000/mm3"]),
        ("Hemoglobin", ["7.9 g/dL"]),
        ("Blood smear", ["Many schistocytes (fragmented red cells)"]),
        ("LDH", ["1,450 units/L (high)"]),
        ("Creatinine", ["1.6 mg/dL"]),
        ("PT/INR and fibrinogen", ["Normal"]),
        ("Temperature", ["38.1 C"])], id="ttp-table")]),
]

ttp_qs = [
hl_table(M(PA, BLEED, "recognize_cues", "moderate", HEME, "ms"),
   "Highlight the findings that require urgent follow-up.",
   ["Finding", "Result"],
   [(["Platelets", "14,000/mm3"], True, "Severe thrombocytopenia."),
    (["Blood smear", "Schistocytes"], True, "Red cells broken by microclots."),
    (["Neurologic", "Confusion, slurred speech"], True, "Microclots in the brain."),
    (["Creatinine", "1.6 mg/dL"], True, "Kidney involvement."),
    (["Coagulation", "Normal PT/INR and fibrinogen"], False, "Normal; this separates TTP from DIC.")],
   "Low platelets, hemolysis and neurologic signs with normal clotting tests suggest TTP.",
   "Recognize cues of TTP."),

matrix_mc(M(PA, BLEED, "analyze_cues", "difficult", HEME, "ms"),
   "For each finding, indicate whether it is typical of TTP, DIC or both.",
   ["TTP", "DIC", "Both"],
   [("Low platelets", 2, "Both consume platelets."),
    ("Prolonged PT/INR and low fibrinogen", 1, "Clotting factors are consumed in DIC."),
    ("Schistocytes on the smear", 2, "Both can fragment cells."),
    ("Low ADAMTS13 activity", 0, "The cause of TTP."),
    ("Neurologic signs with normal clotting tests", 0, "Typical of TTP.")],
   "Normal clotting tests and low ADAMTS13 distinguish TTP.",
   "Analyze TTP versus DIC."),

cloze(M(PPT, TRANS := "blood-transfusion-reactions", "prioritize_hypotheses", "difficult", HEME, "ms ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority treatment is {{b1}}, and platelet transfusion is {{b2}}.",
   [[("urgent plasma exchange", True, "Removes the antibodies and replaces ADAMTS13."),
     ("vitamin K", False, "Not the problem."),
     ("heparin", False, "Not the treatment."),
     ("an antibiotic", False, "Not infection.")],
    [("generally avoided unless there is life-threatening bleeding", True, "Can feed more clots."),
     ("the first treatment", False, "Can worsen thrombosis."),
     ("given daily to a normal count", False, "Not the approach."),
     ("required before plasma exchange", False, "Not routine.")]],
   "TTP is treated with urgent plasma exchange; platelets can make clots worse.",
   "Prioritize TTP treatment."),

sata(M(PA, BLEED, "generate_solutions", "difficult", HEME, "ms ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Insert a dialysis-type central catheter for plasma exchange", True, "Large-bore access."),
    ("Methylprednisolone as prescribed", True, "Reduces the antibody."),
    ("Bleeding and neurologic checks", True, "Monitors complications."),
    ("Caplacizumab as prescribed", True, "Blocks platelet clumping."),
    ("Transfuse platelets to reach 50,000 before any procedure", False, "Avoided unless bleeding."),
    ("Give aspirin for the headache", False, "Raises bleeding risk.")],
   "Plasma exchange, steroids, caplacizumab and monitoring.",
   "Generate the TTP plan.",
   ex=[note("Result", "ADAMTS13 activity below 5%.", id="ttp-adamts")]),

sata(M(PPT, TRANS, "take_action", "difficult", HEME, "ph"),
   "During plasma exchange, the client reports tingling around the mouth and fingertips and feels light-headed. "
   "Review the findings. Which actions should the nurse take?",
   [("Slow the exchange and notify the apheresis team", True, "Citrate binds calcium."),
    ("Give calcium as prescribed", True, "Corrects citrate toxicity."),
    ("Check the ionized calcium", True, "Confirms hypocalcemia."),
    ("Speed up the exchange to finish sooner", False, "More citrate."),
    ("Give a sedative for the anxiety", False, "Does not fix the cause.")],
   "Tingling during plasma exchange is citrate-induced hypocalcemia.",
   "Take action for citrate toxicity.",
   ex=[flow("During exchange", ["Start", "45 minutes"], [
       ("Blood pressure", ["118/72", "104/66 mm Hg"]),
       ("Ionized calcium", ["1.18", "0.92 mmol/L"]),
       ("Client", ["Comfortable", "Tingling lips and fingers"])], id="ttp-exchange")]),

matrix_mc(M(PA, BLEED, "evaluate_outcomes", "difficult", HEME, "ms"),
   "On day 5, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Platelets 142,000/mm3", 0, "Recovery."),
    ("LDH falling", 0, "Less hemolysis."),
    ("Alert and oriented", 0, "Neurologic recovery."),
    ("Redness and drainage at the catheter site", 1, "Line infection."),
    ("Creatinine 0.9 mg/dL", 0, "Kidney recovery."),
    ("Temperature 38.6 C", 1, "Possible line infection.")],
   "Counts and organ function recovered; a line infection needs attention.",
   "Evaluate TTP treatment.",
   ex=[note("Day 5", "Daily plasma exchange x5.", id="ttp-day5")]),
]

cases = [
    case("Pernicious anemia compared with iron deficiency", b12_tabs, b12_qs),
    case("Malignant spinal cord compression", mscc_tabs, mscc_qs),
    case("Vesicant chemotherapy extravasation and hazardous-drug safety", ext_tabs, ext_qs),
    case("Thrombotic thrombocytopenic purpura", ttp_tabs, ttp_qs),
]

BUILT = build(123, "heme-onc-cases-anemia-mscc-extrav-ttp-standalone",
              "Hematologic and oncologic case studies: pernicious anemia, spinal cord compression, extravasation and "
              "TTP, with four standalone items",
              items, cases, case_conversion=True)
