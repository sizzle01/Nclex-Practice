"""Batch 109 (case conversion, chunk 3: endocrine): four full NGN case studies on thyroid and parathyroid care
(CASE-MAP.md).

Cases: thyroid storm; myxedema coma; hematoma and hypocalcemia after thyroidectomy; levothyroxine over-replacement
in an older adult. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes
a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
ENDO = "endocrine"
THY = "thyroid-and-parathyroid-disorders"
ENDO_MEDS = "endocrine-medications"
POSTTHY = "post-thyroidectomy-complications"
LABS = "laboratory-values-and-monitoring"
LYTES = "electrolyte-imbalances"
ENDO_DX = "renal-and-endocrine-diagnostics"
AGING = "aging-and-older-adult-health"
TEACH = "health-teaching-and-behavior-change"
PRIORITY = "prioritizing-client-care"
INJURY = "injury-prevention"
CONSENT = "informed-consent"

items = []

# ================================================================================================
# Case 1: Thyroid storm
# ================================================================================================
storm_tabs = [
    ("storm-notes", "Nurses' notes", [note("Emergency department, 2000",
        "A 34-year-old client with Graves disease stopped methimazole 2 months ago. After 3 days of a chest "
        "infection, the client is agitated, sweating and confused, with vomiting and diarrhea. The client has a "
        "goiter and bulging eyes. Weight 60 kg.", id="storm-note")]),
    ("storm-vitals", "Vital signs and labs", [table("Vital signs and labs", ["2000"], [
        ("Temperature", ["40.3 C (104.5 F)"]),
        ("Heart rate", ["164/min, atrial fibrillation"]),
        ("Blood pressure", ["150/60 mm Hg"]),
        ("Respirations", ["28/min, crackles at both bases"]),
        ("Free T4", ["6.2 ng/dL (0.8 to 1.8)"]),
        ("TSH", ["below 0.01 mIU/L"]),
        ("Glucose", ["168 mg/dL"])], id="storm-table")]),
]

storm_qs = [
hl_table(M(PA, THY, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Temperature", "40.3 C (104.5 F)"], True, "Severe hyperthermia."),
    (["Heart rate", "164/min, atrial fibrillation"], True, "Rapid rate with AF threatens cardiac output."),
    (["Mental status", "Agitated, confused"], True, "CNS dysfunction is a key feature of storm."),
    (["Lungs", "Crackles at both bases"], True, "Heart failure from the high-output state."),
    (["Eyes", "Bulging (exophthalmos)"], False, "A chronic Graves sign, not acute."),
    (["Glucose", "168 mg/dL"], False, "Mild stress hyperglycemia.")],
   "Hyperthermia, tachydysrhythmia, delirium and heart failure after stopping antithyroid drugs fit thyroid storm.",
   "Recognize cues of thyroid storm."),

matrix_mc(M(PA, THY, "analyze_cues", "difficult", ENDO, "ms"),
   "For each finding, indicate how it relates to thyroid storm.",
   ["Trigger", "Effect of excess thyroid hormone", "Unrelated"],
   [("Stopping methimazole", 0, "Untreated Graves disease leaves hormone production unchecked."),
    ("Chest infection", 0, "Infection is a common precipitant."),
    ("Wide pulse pressure (150/60)", 1, "High-output state with low vascular resistance."),
    ("Diarrhea and vomiting", 1, "Increased gut motility."),
    ("Fever of 40.3 C (104.5 F)", 1, "Increased metabolic rate and heat production."),
    ("Bulging eyes", 2, "From Graves disease itself, not the storm.")],
   "Storm follows a trigger in untreated hyperthyroidism; its effects are hypermetabolic and adrenergic.",
   "Analyze the causes and effects of thyroid storm."),

cloze(M(PA, THY, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most immediate threat to life is {{b1}}, made worse by {{b2}}.",
   [[("cardiovascular collapse from high-output heart failure and AF", True, "Storm kills mainly through heart failure and dysrhythmia."),
     ("thyroid eye disease", False, "Chronic."),
     ("hyperglycemia", False, "Mild."),
     ("dehydration alone", False, "Present, but not the main threat.")],
    [("hyperthermia increasing oxygen demand", True, "Every degree adds to the heart's workload."),
     ("the goiter", False, "Not the main factor."),
     ("the age of the client", False, "Not the main factor."),
     ("the glucose", False, "Not the main factor.")]],
   "Heart failure and dysrhythmia, driven by fever and hypermetabolism, are the main threats.",
   "Prioritize threats in thyroid storm."),

matrix_mc(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Propranolol to control the heart rate", 0, "Blocks adrenergic effects and reduces T4 to T3 conversion."),
    ("Propylthiouracil (PTU) first", 0, "Blocks new hormone synthesis and T4 to T3 conversion."),
    ("Potassium iodide given at least 1 hour after PTU", 0, "Iodine blocks hormone release; given first, it would feed new hormone synthesis."),
    ("Hydrocortisone IV", 0, "Treats relative adrenal insufficiency and reduces conversion."),
    ("Aspirin for the fever", 1, "Aspirin displaces thyroid hormone from binding proteins and raises free hormone."),
    ("Cooling blankets and acetaminophen", 0, "Lowers temperature safely.")],
   "Beta blocker, thionamide then iodine an hour later, steroids, and cooling with acetaminophen, not aspirin.",
   "Generate the treatment plan for thyroid storm.",
   ex=[note("Orders, 2020", "Propranolol 60 mg PO every 4 hours. PTU 500 mg loading dose. SSKI 5 drops every 6 hours. "
       "Hydrocortisone 100 mg IV every 8 hours. Acetaminophen. Cooling blanket.", id="storm-orders")]),

order(M(PPT, ENDO_MEDS, "take_action", "difficult", ENDO, "ph"),
   "The orders arrive together. Place the medicines in the order the nurse should give them.",
   [("Propylthiouracil (PTU) loading dose", "Starts blocking new hormone synthesis."),
    ("Propranolol", "Can be given right after PTU to control the heart rate."),
    ("Hydrocortisone IV", "Given early in storm."),
    ("Potassium iodide (SSKI), at least 1 hour after PTU", "Iodine last, once synthesis is blocked.")],
   "Thionamide before iodine is the rule that matters; the beta blocker and steroid come early.",
   "Take action to sequence thyroid storm drugs.",
   ex=[flow("2030", ["2030"], [
       ("Heart rate", ["168/min"]),
       ("Temperature", ["40.4 C (104.7 F)"]),
       ("IV access", ["Two lines"]),
       ("Swallowing", ["Able to take tablets with sips"])], id="storm-2030")]),

matrix_mc(M(SIC, INJURY, "evaluate_outcomes", "difficult", ENDO, "ms fund"),
   "Review the next day's findings. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Heart rate 102/min, still in AF", 0, "Rate control is improving."),
    ("Temperature 37.8 C (100 F)", 0, "Fever is settling."),
    ("Oriented x3, calm", 0, "Delirium resolving."),
    ("The client got up alone at night and was found dizzy on the floor beside the bed", 1,
     "A fall; review fall precautions, orthostatic pressure and the beta blocker dose."),
    ("ALT 220 units/L (normal on admission)", 1, "PTU can injure the liver; report it."),
    ("Bed alarm off because \"it kept going off\"", 1, "The safety measure must stay on for a client at risk.")],
   "The storm is settling, but a fall, a disabled bed alarm and liver injury from PTU need action.",
   "Evaluate recovery and safety in thyroid storm.",
   ex=[flow("Next day, 0800", ["0800"], [
       ("Heart rate / rhythm", ["102/min, AF"]),
       ("Temperature", ["37.8 C (100 F)"]),
       ("Mental status", ["Oriented x3"]),
       ("Night event", ["Found on the floor beside the bed; bed alarm switched off"]),
       ("ALT", ["220 units/L"])], id="storm-day2")]),
]

# ================================================================================================
# Case 2: Myxedema coma
# ================================================================================================
myx_tabs = [
    ("myx-notes", "Nurses' notes", [note("Medical unit, 0700",
        "An 80-year-old client with hypothyroidism has not taken levothyroxine for months. The client was admitted "
        "2 days ago with pneumonia and was given oxycodone for chest pain overnight. This morning the client is very "
        "hard to wake. Skin is cool and dry, the face and eyelids are puffy, and the tongue is thick.",
        id="myx-note")]),
    ("myx-data", "Vital signs and labs", [table("Vital signs and labs", ["0700", "Reference"], [
        ("Temperature", ["34.2 C (93.6 F)", "-"]),
        ("Heart rate", ["46/min", "-"]),
        ("Blood pressure", ["86/54 mm Hg", "-"]),
        ("Respirations", ["8/min", "-"]),
        ("PaCO2", ["68 mm Hg", "35 to 45"]),
        ("Sodium", ["121 mEq/L", "135 to 145"]),
        ("Glucose", ["58 mg/dL", "70 to 110"]),
        ("TSH / free T4", ["68 mIU/L / 0.2 ng/dL", "0.4 to 4.0 / 0.8 to 1.8"])], id="myx-table")]),
]

myx_qs = [
hl_table(M(PA, THY, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Level of consciousness", "Very hard to wake"], True, "Coma is the defining feature."),
    (["Temperature", "34.2 C (93.6 F)"], True, "Hypothermia."),
    (["Respirations / PaCO2", "8/min / 68 mm Hg"], True, "Hypoventilation with CO2 retention."),
    (["Blood pressure / heart rate", "86/54 / 46/min"], True, "Hypotension and bradycardia."),
    (["Glucose", "58 mg/dL"], True, "Hypoglycemia."),
    (["Face", "Puffy face and eyelids"], False, "Chronic myxedema; not an emergency by itself.")],
   "Coma, hypothermia, hypoventilation, hypotension and hypoglycemia in untreated hypothyroidism are myxedema coma.",
   "Recognize cues of myxedema coma."),

matrix_mc(M(PA, THY, "analyze_cues", "difficult", ENDO, "ms ph"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Oxycodone likely helped tip the client into coma", 0, "Hypothyroid clients clear sedatives slowly and are very sensitive to them."),
    ("Pneumonia is a common trigger", 0, "Infection precipitates myxedema coma."),
    ("The low sodium is from too much salt loss in sweat", 1, "It is from impaired free water excretion."),
    ("The low temperature should be corrected with an active warming blanket", 1, "Active external warming dilates vessels and can cause shock; passive warming is used."),
    ("Adrenal insufficiency may coexist", 0, "So steroids are given before or with thyroid hormone.")],
   "Sedatives and infection trigger myxedema coma; hyponatremia is dilutional; warming is passive.",
   "Analyze myxedema coma."),

cloze(M(PA, THY, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The first priority is {{b1}} because {{b2}}.",
   [[("supporting ventilation", True, "A rate of 8 with a PaCO2 of 68 is respiratory failure."),
     ("rewarming", False, "Important, but breathing comes first."),
     ("correcting the sodium", False, "Done slowly, after airway and breathing."),
     ("starting oral levothyroxine", False, "IV hormone is given, after the airway is secured.")],
    [("hypoventilation threatens life most quickly", True, "Respiratory failure is the immediate threat."),
     ("the sodium is 121", False, "Serious, but not the first priority."),
     ("the client is 80 years old", False, "Not the reason."),
     ("the TSH is very high", False, "Explains the cause, not the priority.")]],
   "Airway and breathing come first in myxedema coma.",
   "Prioritize care in myxedema coma."),

matrix_mc(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Hydrocortisone IV before or with thyroid hormone", 0, "Covers possible adrenal insufficiency."),
    ("Levothyroxine IV loading dose", 0, "Oral absorption is unreliable in myxedema coma."),
    ("Passive rewarming with blankets and a warm room", 0, "Safe rewarming."),
    ("Further oxycodone for pneumonia pain", 1, "Sedatives worsen coma and hypoventilation."),
    ("Dextrose for the glucose of 58 mg/dL", 0, "Treats hypoglycemia."),
    ("Rapid 3% saline to normalize the sodium today", 1, "Too fast a rise risks osmotic demyelination; correct slowly.")],
   "Steroids and IV thyroid hormone, passive warming, glucose, and no sedatives or rapid sodium correction.",
   "Generate the treatment plan for myxedema coma.",
   ex=[note("Orders, 0730", "BiPAP; intubate if no improvement. Hydrocortisone 100 mg IV. Levothyroxine 200 mcg IV. "
       "D10 infusion. Hold oxycodone. Passive rewarming.", id="myx-orders")]),

sata(M(PA, THY, "take_action", "difficult", ENDO, "ms ph"),
   "Review the findings 2 hours after IV levothyroxine. Which actions should the nurse take?",
   [("Report the chest pain and ST changes now", True, "Thyroid hormone raises myocardial oxygen demand; older clients may have ischemia."),
    ("Obtain a 12-lead ECG", True, "Documents ischemia."),
    ("Keep the client on continuous cardiac monitoring", True, "Dysrhythmias and ischemia can occur."),
    ("Ask whether the next levothyroxine dose should be reduced", True, "Dosing is slowed in older adults and those with heart disease."),
    ("Give the next levothyroxine dose early to speed recovery", False, "Faster replacement raises cardiac risk."),
    ("Apply a heated blanket to speed warming", False, "Active warming is avoided.")],
   "Thyroid hormone can cause ischemia in an older adult; report, monitor and slow the replacement.",
   "Take action for cardiac effects of thyroid replacement.",
   ex=[flow("0930", ["0930"], [
       ("Mental status", ["Opens eyes to voice"]),
       ("Heart rate", ["62/min"]),
       ("Client report", ["Pressure in the chest"]),
       ("ECG", ["New ST depression in V4 to V6"])], id="myx-0930")]),

matrix_mc(M(MOC, PRIORITY, "evaluate_outcomes", "difficult", ENDO, "mgmt ms"),
   "Review the findings on day 3. For each finding, indicate whether it is improving or expected, or needs follow-up.",
   ["Improving or expected", "Needs follow-up"],
   [("Temperature 36.4 C (97.5 F)", 0, "Normothermia restored."),
    ("Off BiPAP; respirations 16/min", 0, "Ventilation recovered."),
    ("Sodium 127 mEq/L (121 two days ago)", 0, "Rising slowly and safely."),
    ("The client lives alone and missed levothyroxine for months because of cost", 1,
     "Needs case management or social work for affordable medicine and follow-up."),
    ("Oxycodone still on the active order list", 1, "Should be discontinued or reviewed."),
    ("TSH not yet rechecked", 0, "TSH is rechecked in weeks, not days; this is expected.")],
   "Physiology is recovering; the cost barrier and an unsafe active order need follow-up before discharge.",
   "Evaluate recovery and discharge needs after myxedema coma.",
   ex=[flow("Day 3", ["Day 3"], [
       ("Temperature", ["36.4 C (97.5 F)"]),
       ("Breathing", ["Off BiPAP, 16/min"]),
       ("Sodium", ["127 mEq/L"]),
       ("Social", ["Lives alone; stopped levothyroxine because of cost"]),
       ("Orders", ["Oxycodone 5 mg every 4 hours as needed still active"])], id="myx-day3")]),
]

# ================================================================================================
# Case 3: Hematoma and hypocalcemia after thyroidectomy
# ================================================================================================
thx_tabs = [
    ("thx-notes", "Nurses' notes", [note("Surgical unit",
        "A 45-year-old client had a total thyroidectomy for a large goiter at 0900. A tracheostomy tray, suction "
        "and calcium gluconate are at the bedside per protocol.", id="thx-note")]),
    ("thx-flow", "Flowsheet", [flow("Postoperative checks", ["1300", "1700", "2100"], [
        ("Voice", ["Slightly hoarse", "Slightly hoarse", "Hoarse"]),
        ("Neck", ["Dressing dry", "Dressing dry", "Swelling, tight dressing, blood at the back of the neck"]),
        ("Breathing", ["Easy", "Easy", "Noisy, RR 26/min"]),
        ("SpO2", ["98%", "97%", "92%"]),
        ("Client report", ["Sore throat", "Sore throat", "\"It feels tight when I swallow.\""])], id="thx-flow")]),
]

thx_qs = [
hl_table(M(RRP, "postoperative-care", "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings at 2100 that require immediate action.",
   ["Finding", "2100"],
   [(["Neck", "Swelling, tight dressing"], True, "A neck hematoma can compress the airway."),
    (["Dressing", "Blood at the back of the neck"], True, "Blood pools posteriorly; the front can look dry."),
    (["Breathing", "Noisy, RR 26/min"], True, "Early airway compromise."),
    (["Swallowing", "Feels tight"], True, "Pressure from a hematoma."),
    (["Voice", "Hoarse"], False, "Mild hoarseness is common after thyroid surgery; new or worsening aphonia would be concerning."),
    (["SpO2", "92%"], True, "Falling saturation.")],
   "Neck swelling, posterior blood, noisy breathing and tightness on swallowing suggest an expanding hematoma.",
   "Recognize cues of a post-thyroidectomy hematoma."),

cloze(M(PA, POSTTHY, "analyze_cues", "difficult", ENDO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The findings suggest {{b1}}. Hoarseness alone after this surgery is most often from {{b2}}.",
   [[("a hematoma compressing the trachea", True, "Swelling, posterior blood and noisy breathing."),
     ("hypocalcemia", False, "Would cause tingling and spasms."),
     ("thyroid storm", False, "Not supported."),
     ("an allergic reaction", False, "Not supported.")],
    [("irritation or injury of the recurrent laryngeal nerve or swelling from the breathing tube", True,
      "Usually temporary; bilateral nerve injury can cause airway obstruction."),
     ("hypocalcemia", False, "Laryngospasm, not simple hoarseness."),
     ("a hematoma", False, "A hematoma gives swelling and stridor."),
     ("infection", False, "Too early.")]],
   "A neck hematoma threatens the airway; hoarseness alone is usually nerve or tube related.",
   "Analyze airway findings after thyroidectomy."),

matrix_mc(M(RRP, "postoperative-care", "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "For each complication, indicate its urgency now.",
   ["Emergency now", "Watch for later"],
   [("Airway obstruction from the hematoma", 0, "Can close the airway within minutes."),
    ("Hypocalcemia from parathyroid injury", 1, "Usually appears 24 to 72 hours after surgery."),
    ("Thyroid storm from handling of the gland", 1, "Rare with modern preparation."),
    ("Wound infection", 1, "Later.")],
   "The expanding hematoma is the emergency; hypocalcemia usually comes later.",
   "Prioritize complications after thyroidectomy."),

sata(M(RRP, "postoperative-care", "generate_solutions", "difficult", ENDO, "ms"),
   "Which actions should the nurse anticipate?",
   [("Call the surgeon immediately", True, "The hematoma may need evacuation at the bedside or in surgery."),
    ("Raise the head of the bed", True, "Reduces swelling and eases breathing."),
    ("Prepare to open the incision at the bedside if the airway closes, per protocol", True, "Releasing the hematoma can save the airway."),
    ("Have the tracheostomy tray and suction ready", True, "Airway backup."),
    ("Apply a tighter pressure dressing to stop the bleeding", False, "Increases pressure on the airway."),
    ("Give a sedative to ease the client's anxiety", False, "Can depress breathing.")],
   "Get the surgeon, keep the head up, prepare airway equipment and the protocol for opening the wound.",
   "Generate solutions for a neck hematoma.",
   ex=[note("2105", "The surgeon is 10 minutes away and asks the nurse to prepare.", id="thx-2105")]),

sata(M(PA, LYTES, "take_action", "difficult", ENDO, "ms ph"),
   "The hematoma was evacuated. On day 2, review the findings. Which actions should the nurse take?",
   [("Give calcium gluconate IV slowly as prescribed", True, "Treats symptomatic hypocalcemia."),
    ("Place the client on a cardiac monitor during the infusion", True, "Calcium affects heart rhythm."),
    ("Check the IV site often", True, "Calcium irritates veins and damages tissue if it leaks."),
    ("Give the calcium as a rapid IV push", False, "Too fast causes dysrhythmias."),
    ("Wait for the next routine calcium level in the morning", False, "Symptoms need treatment now.")],
   "Symptomatic hypocalcemia after thyroidectomy needs slow IV calcium with monitoring.",
   "Take action for post-thyroidectomy hypocalcemia.",
   ex=[flow("Day 2, 1000", ["1000"], [
       ("Client report", ["Tingling around the mouth and fingers"]),
       ("Chvostek sign", ["Positive"]),
       ("Trousseau sign", ["Positive"]),
       ("Calcium", ["7.1 mg/dL (8.5 to 10.5); albumin normal"]),
       ("Voice", ["Hoarse, no stridor"])], id="thx-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", ENDO, "ms ph"),
   "Before discharge, the client describes the plan. For each statement, indicate whether it shows understanding "
   "or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take levothyroxine for the rest of my life.\"", 0, "No thyroid tissue remains."),
    ("\"I'll take it on an empty stomach, 30 to 60 minutes before breakfast.\"", 0, "Improves absorption."),
    ("\"I'll take my calcium tablets with the levothyroxine to save time.\"", 1, "Calcium blocks absorption; separate by 4 hours."),
    ("\"I'll call if my lips or fingers start tingling.\"", 0, "Early sign of hypocalcemia."),
    ("\"I can stop calcium as soon as the tingling goes away.\"", 1, "The surgeon decides based on calcium levels."),
    ("\"My voice may stay hoarse for a few weeks.\"", 0, "Usually temporary; persistent change is reported.")],
   "Teaching covers lifelong levothyroxine taken alone, separating calcium, and hypocalcemia warning signs.",
   "Evaluate teaching after thyroidectomy.",
   ex=[note("Discharge plan", "Levothyroxine 125 mcg daily. Calcium carbonate 1,000 mg three times daily and "
       "calcitriol. Calcium level in 1 week.", id="thx-dc")]),
]

# ================================================================================================
# Case 4: Levothyroxine over-replacement in an older adult
# ================================================================================================
lt4_tabs = [
    ("lt4-notes", "Clinic notes", [note("Primary care clinic",
        "An 81-year-old client with hypothyroidism has had palpitations, poor sleep and weight loss for 2 months. The "
        "client's daughter refills the pill organizer weekly. Two months ago the levothyroxine dose was raised from "
        "75 to 100 mcg; the old bottle was not thrown out. Bone density last year: T-score -2.4 at the hip.",
        id="lt4-note")]),
    ("lt4-data", "Findings", [table("Findings", ["Result", "Reference"], [
        ("Heart rate", ["108/min, irregularly irregular", "-"]),
        ("TSH", ["0.03 mIU/L", "0.4 to 4.0"]),
        ("Free T4", ["2.6 ng/dL", "0.8 to 1.8"]),
        ("Weight", ["Down 3 kg", "-"]),
        ("Pill organizer", ["Contains both a 75 mcg and a 100 mcg tablet each morning", "-"])], id="lt4-table")]),
]

lt4_qs = [
hl_table(M(PPT, ENDO_MEDS, "recognize_cues", "moderate", ENDO, "ph"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["Heart rate", "108/min, irregularly irregular"], True, "New atrial fibrillation."),
    (["TSH / free T4", "0.03 / 2.6"], True, "Over-replacement."),
    (["Pill organizer", "Both 75 and 100 mcg each morning"], True, "The client is taking 175 mcg daily by mistake."),
    (["Weight", "Down 3 kg"], True, "Hypermetabolism."),
    (["Bone density", "T-score -2.4"], True, "Low bone mass; excess thyroid hormone speeds bone loss."),
    (["Age", "81"], False, "A context factor: older adults need lower doses, not a finding to act on.")],
   "A medication error causing iatrogenic hyperthyroidism with AF and bone risk.",
   "Recognize cues of levothyroxine over-replacement."),

matrix_mc(M(PPT, ENDO_MEDS, "analyze_cues", "difficult", ENDO, "ph"),
   "For each consequence, indicate whether excess levothyroxine causes or worsens it.",
   ["Caused or worsened", "Not related"],
   [("Atrial fibrillation", 0, "Thyroid excess increases atrial irritability."),
    ("Faster bone loss", 0, "Increases bone turnover."),
    ("Insomnia and anxiety", 0, "Adrenergic effects."),
    ("Constipation", 1, "Hypothyroidism causes constipation; excess causes frequent stools."),
    ("Angina in a client with coronary disease", 0, "Raises myocardial oxygen demand.")],
   "Excess thyroid hormone affects heart rhythm, bone, sleep and the coronary circulation.",
   "Analyze effects of thyroid hormone excess."),

matrix_mc(M(PA, THY, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "For each risk, indicate its priority.",
   ["High priority", "Lower priority"],
   [("Stroke from new atrial fibrillation", 0, "Needs stroke-risk review and anticoagulation discussion."),
    ("Ongoing double dosing", 0, "The error continues every day."),
    ("Hip fracture from falls and bone loss", 0, "Age, low bone density and AF-related dizziness."),
    ("Weight loss", 1, "Will reverse with correct dosing.")],
   "The error, the AF and fracture risk are the priorities.",
   "Prioritize risks of over-replacement."),

sata(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Remove the old 75 mcg bottle from the home supply", True, "Prevents the error recurring."),
    ("Hold or reduce levothyroxine as the provider directs", True, "Lets hormone levels fall."),
    ("A beta blocker for rate control", True, "Controls AF symptoms while levels fall."),
    ("Recheck TSH in 6 to 8 weeks after the dose change", True, "TSH takes weeks to settle."),
    ("Discuss anticoagulation based on the client's stroke risk", True, "New AF in an 81-year-old carries a high stroke risk."),
    ("Recheck TSH tomorrow to confirm the change", False, "Too soon to be useful."),
    ("Switch to taking levothyroxine with breakfast to slow absorption", False, "Inconsistent timing makes levels erratic.")],
   "Fix the supply error, adjust the dose, control the rate and recheck TSH in 6 to 8 weeks.",
   "Generate a plan for levothyroxine over-replacement.",
   ex=[note("Provider plan", "Hold levothyroxine 1 week, then 75 mcg daily. Metoprolol. Assess stroke risk.",
       id="lt4-plan")]),

sata(M(SIC, "medication-safety-systems", "take_action", "difficult", ENDO, "ph mgmt"),
   "Which actions should the nurse take to prevent another dosing error at home?",
   [("Do a brown-bag review of every medicine bottle with the daughter", True, "Finds duplicates and discontinued drugs."),
    ("Give a single updated medication list to the client, daughter and pharmacy", True, "Everyone works from one list."),
    ("Ask the pharmacy to cancel the old 75 mcg prescription", True, "Stops refills of the old strength."),
    ("Use teach-back while the daughter fills the organizer", True, "Confirms understanding."),
    ("Tell the client to keep the old bottle in case the dose goes back down", False, "Keeps the risk."),
    ("Rely on the client remembering which tablet is new", False, "Not safe.")],
   "Medication reconciliation, one shared list, cancelled old prescriptions and teach-back prevent repeat errors.",
   "Take action to prevent a home medication error.",
   ex=[note("Visit", "The daughter brought all the bottles in a bag.", id="lt4-visit")]),

matrix_mc(M(HPM, AGING, "evaluate_outcomes", "difficult", ENDO, "ph ms"),
   "Review the 8-week follow-up. For each finding, indicate whether it shows the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("TSH 1.8 mIU/L", 0, "Within range."),
    ("Back in sinus rhythm at 72/min", 0, "AF resolved with correct dosing."),
    ("Sleeping better, weight stable", 0, "Symptoms resolved."),
    ("No bone health plan discussed", 1, "With a T-score of -2.4, review calcium, vitamin D and fall prevention."),
    ("Still taking metoprolol with a heart rate of 52 and dizziness", 1, "May no longer be needed; report."),
    ("Pill organizer filled correctly at the visit", 0, "Error prevention is working.")],
   "Thyroid and rhythm are corrected; bone health and the now-excess beta blocker need follow-up.",
   "Evaluate outcomes after correcting over-replacement.",
   ex=[flow("8 weeks", ["Week 8"], [
       ("TSH", ["1.8 mIU/L"]),
       ("Rhythm", ["Sinus 72/min at rest; 52/min after metoprolol with dizziness"]),
       ("Symptoms", ["Sleeping well; weight stable"]),
       ("Bone health", ["Not discussed"]),
       ("Organizer", ["Filled correctly"])], id="lt4-w8")]),
]

cases = [
    case("Thyroid storm", storm_tabs, storm_qs),
    case("Myxedema coma", myx_tabs, myx_qs),
    case("Hematoma and hypocalcemia after thyroidectomy", thx_tabs, thx_qs),
    case("Levothyroxine over-replacement in an older adult", lt4_tabs, lt4_qs),
]

BUILT = build(109, "endocrine-cases-thyroid",
              "Endocrine case studies: thyroid storm, myxedema coma, thyroidectomy complications and levothyroxine "
              "over-replacement",
              items, cases, case_conversion=True)
