"""Batch 110 (case conversion, chunk 3: endocrine): four full NGN case studies on adrenal and pituitary disorders
(CASE-MAP.md).

Cases: adrenal crisis in primary adrenal insufficiency; Cushing syndrome from long-term prednisone; SIADH with
hyponatremic seizure; diabetes insipidus after transsphenoidal pituitary surgery. Each case walks the six CJMM steps
in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
ENDO = "endocrine"
ADRENAL = "adrenal-disorders"
PIT = "pituitary-disorders"
ENDO_MEDS = "endocrine-medications"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
TEACH = "health-teaching-and-behavior-change"
INJURY = "injury-prevention"
SEIZURE = "seizure-management"
NEURO_DX = "neurological-assessment-and-procedures"
HAI = "healthcare-associated-infection-prevention"
COLLAB = "collaboration-and-referrals"

items = []

# ================================================================================================
# Case 1: Adrenal crisis
# ================================================================================================
add_tabs = [
    ("add-notes", "Nurses' notes", [note("Emergency department, 0400",
        "A 38-year-old client with Addison disease takes hydrocortisone 15 mg in the morning and 5 mg in the "
        "afternoon, and fludrocortisone 0.1 mg daily. For 2 days the client has had vomiting and diarrhea and could "
        "not keep tablets down. The client did not have an injectable hydrocortisone kit. Now the client is weak, "
        "confused and has severe abdominal pain. Skin is darkly pigmented in the palm creases and gums.",
        id="add-note")]),
    ("add-labs", "Vital signs and labs", [table("Vital signs and labs", ["0400", "Reference"], [
        ("Blood pressure", ["74/40 mm Hg", "-"]),
        ("Heart rate", ["126/min", "-"]),
        ("Temperature", ["38.2 C (100.8 F)", "-"]),
        ("Sodium", ["124 mEq/L", "135 to 145"]),
        ("Potassium", ["6.2 mEq/L", "3.5 to 5.0"]),
        ("Glucose", ["54 mg/dL", "70 to 110"]),
        ("BUN / creatinine", ["42 / 1.6 mg/dL", "10 to 20 / 0.6 to 1.2"])], id="add-table")]),
]

add_qs = [
hl_table(M(PA, ADRENAL, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Blood pressure", "74/40 mm Hg"], True, "Shock."),
    (["Potassium", "6.2 mEq/L"], True, "Hyperkalemia from aldosterone deficiency; dysrhythmia risk."),
    (["Glucose", "54 mg/dL"], True, "Hypoglycemia from cortisol deficiency."),
    (["Sodium", "124 mEq/L"], True, "Sodium loss from aldosterone deficiency."),
    (["Mental status", "Confused"], True, "Hypoperfusion and hypoglycemia."),
    (["Skin", "Dark pigment in palm creases and gums"], False, "A chronic sign of primary adrenal insufficiency, not acute.")],
   "Shock, hyperkalemia, hypoglycemia and hyponatremia after missed steroid doses is adrenal crisis.",
   "Recognize cues of adrenal crisis."),

matrix_mc(M(PA, ADRENAL, "analyze_cues", "difficult", ENDO, "ms"),
   "For each finding, indicate the missing hormone mainly responsible.",
   ["Cortisol deficiency", "Aldosterone deficiency", "Excess ACTH"],
   [("Hypoglycemia", 0, "Cortisol supports gluconeogenesis."),
    ("Hyperkalemia", 1, "Aldosterone makes the kidney excrete potassium."),
    ("Hyponatremia and volume loss", 1, "Aldosterone keeps sodium and water."),
    ("Hypotension that does not respond to fluids alone", 0, "Cortisol is needed for vessels to respond to catecholamines."),
    ("Darkened skin", 2, "ACTH stimulates melanocytes in primary adrenal failure.")],
   "Primary adrenal insufficiency lacks both cortisol and aldosterone, and high ACTH darkens the skin.",
   "Analyze hormone deficits in Addison disease."),

cloze(M(PA, ADRENAL, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client is in {{b1}}, triggered by {{b2}}, and the most immediate threats are {{b3}}.",
   [[("adrenal crisis", True, "Acute life-threatening cortisol deficiency."),
     ("septic shock alone", False, "Infection may contribute, but the electrolytes point to adrenal crisis."),
     ("diabetic ketoacidosis", False, "Glucose is low."),
     ("SIADH", False, "Potassium would not be high.")],
    [("gastroenteritis with missed hydrocortisone doses", True, "Illness raises cortisol needs while doses were lost."),
     ("too much fludrocortisone", False, "Would cause the opposite."),
     ("stress from the ED visit", False, "Not the cause."),
     ("a high-salt diet", False, "Not supported.")],
    [("shock and hyperkalemic dysrhythmia", True, "Both can kill quickly."),
     ("skin changes", False, "Chronic."),
     ("abdominal pain", False, "A symptom, not the main threat."),
     ("fever", False, "Not the main threat.")]],
   "Adrenal crisis from missed doses during illness; shock and hyperkalemia are the immediate threats.",
   "Prioritize adrenal crisis."),

matrix_mc(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Hydrocortisone 100 mg IV now", 0, "Replaces cortisol; at this dose it also has mineralocorticoid effect."),
    ("0.9% sodium chloride 1 L rapidly", 0, "Restores volume and sodium."),
    ("Dextrose for the glucose of 54 mg/dL", 0, "Treats hypoglycemia."),
    ("Wait for a cortisol level before giving steroids", 1, "Draw blood if possible, but never delay treatment."),
    ("Potassium chloride with the fluids", 1, "Potassium is already high."),
    ("Cardiac monitoring", 0, "Hyperkalemia can cause dysrhythmias.")],
   "Give IV hydrocortisone at once, saline and dextrose, and monitor the heart; do not delay for tests.",
   "Generate the treatment for adrenal crisis.",
   ex=[note("ECG", "Peaked T waves. Sinus tachycardia.", id="add-ecg")]),

sata(M(PA, ADRENAL, "take_action", "difficult", ENDO, "ms ph"),
   "Review the findings 4 hours after treatment. Which actions should the nurse take?",
   [("Continue hydrocortisone every 6 hours as prescribed", True, "Stress doses continue until recovery."),
    ("Recheck glucose, sodium and potassium", True, "Tracks correction."),
    ("Report the sodium rise of 12 mEq/L in 4 hours", True, "Too-fast correction risks osmotic demyelination."),
    ("Stop the steroid now that blood pressure is normal", False, "Stopping would bring the crisis back."),
    ("Give fludrocortisone instead of hydrocortisone tonight", False, "High-dose hydrocortisone already covers mineralocorticoid needs.")],
   "Continue stress-dose steroids, keep checking electrolytes, and report a sodium rise that is too fast.",
   "Take action during recovery from adrenal crisis.",
   ex=[flow("0800", ["0400", "0800"], [
       ("Blood pressure", ["74/40", "112/68 mm Hg"]),
       ("Sodium", ["124", "136 mEq/L"]),
       ("Potassium", ["6.2", "4.9 mEq/L"]),
       ("Glucose", ["54", "118 mg/dL"])], id="add-0800")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", ENDO, "ms ph"),
   "Before discharge, the client describes sick-day rules. For each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"With a fever, I'll double my hydrocortisone doses.\"", 0, "Stress dosing for illness."),
    ("\"If I'm vomiting and can't keep tablets down, I'll inject the emergency hydrocortisone and go to the ED.\"", 0,
     "The emergency kit prevents a crisis."),
    ("\"I'll wear a medical alert bracelet.\"", 0, "Alerts responders."),
    ("\"I'll double the fludrocortisone too when I'm sick.\"", 1, "Stress dosing applies to hydrocortisone, not fludrocortisone."),
    ("\"If I feel well, I can skip the afternoon dose.\"", 1, "Doses are taken every day."),
    ("\"I'll tell any surgeon or dentist that I take steroids.\"", 0, "Procedures need extra steroid cover.")],
   "Sick-day rules: increase hydrocortisone, inject when vomiting, wear ID, never skip doses.",
   "Evaluate adrenal insufficiency sick-day teaching.",
   ex=[note("Discharge", "Prescriptions: hydrocortisone tablets, fludrocortisone, hydrocortisone 100 mg injection "
       "kit. Endocrinology follow-up in 2 weeks.", id="add-dc")]),
]

# ================================================================================================
# Case 2: Cushing syndrome from long-term prednisone
# ================================================================================================
cush_tabs = [
    ("cush-notes", "Nurses' notes", [note("Medical unit, admission",
        "A 62-year-old client has taken prednisone 30 mg daily for 8 months for polymyalgia. The client is admitted "
        "with a productive cough but no fever. Findings: round face, fat pad between the shoulders, thin skin with "
        "bruises, purple stretch marks on the abdomen, and weak thigh muscles (cannot rise from a chair without using "
        "the arms). The client fell at home last week.", id="cush-note")]),
    ("cush-labs", "Vital signs and labs", [table("Vital signs and labs", ["Admission", "Reference"], [
        ("Blood pressure", ["164/96 mm Hg", "-"]),
        ("Temperature", ["37.2 C (99 F)", "-"]),
        ("Glucose", ["286 mg/dL", "70 to 110"]),
        ("Potassium", ["3.1 mEq/L", "3.5 to 5.0"]),
        ("WBC", ["16,800/mm3", "5,000 to 10,000"]),
        ("Chest x-ray", ["Right lower lobe infiltrate", "-"])], id="cush-table")]),
]

cush_qs = [
hl_text(M(PA, ADRENAL, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that are effects of long-term glucocorticoid therapy.",
   ["The client has ",
    ("a round face and a fat pad between the shoulders", True, "Fat redistribution."),
    ", ",
    ("thin, bruised skin with purple stretch marks", True, "Protein breakdown in skin."),
    ", ",
    ("weak thigh muscles", True, "Steroid myopathy."),
    ", a ",
    ("productive cough", False, "From pneumonia, not a steroid effect, though steroids raise infection risk."),
    ", and ",
    ("a glucose of 286 mg/dL", True, "Steroid-induced hyperglycemia."),
    ". The client ",
    ("fell at home last week", False, "A consequence of myopathy and a safety concern, rather than a direct sign."),
    "."],
   "Fat redistribution, skin fragility, proximal myopathy and hyperglycemia are signs of Cushing syndrome.",
   "Recognize cues of iatrogenic Cushing syndrome."),

matrix_mc(M(PPT, ENDO_MEDS, "analyze_cues", "difficult", ENDO, "ph ms"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("The normal temperature rules out a serious infection", 1, "Steroids blunt fever; the WBC and x-ray show pneumonia."),
    ("The low potassium is a steroid effect", 0, "Mineralocorticoid activity wastes potassium."),
    ("The client's own adrenal glands are probably suppressed", 0, "Months of prednisone suppress ACTH and cortisol production."),
    ("Prednisone can be stopped now because of the infection", 1, "Stopping abruptly risks adrenal crisis."),
    ("The WBC of 16,800 may partly reflect the steroid", 0, "Steroids raise neutrophil counts, so infection is judged on the whole picture.")],
   "Steroids mask fever, waste potassium, suppress the adrenals and raise the WBC.",
   "Analyze the effects of chronic steroid therapy."),

matrix_mc(M(PA, ADRENAL, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "For each risk, indicate its priority during this admission.",
   ["High priority", "Lower priority"],
   [("Adrenal crisis if a dose is missed while ill", 0, "Illness raises the need for steroid."),
    ("Worsening pneumonia hidden by the steroid", 0, "Signs are muted."),
    ("Falls and fracture", 0, "Myopathy and possible osteoporosis."),
    ("Hyperglycemia", 0, "Worsens infection and healing."),
    ("Cosmetic changes", 1, "Distressing, but not urgent.")],
   "Adrenal suppression, masked infection, falls and hyperglycemia are the main risks.",
   "Prioritize risks of chronic steroid therapy."),

sata(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Continue prednisone; the provider may increase it during the illness", True, "Avoids adrenal crisis."),
    ("Correctional insulin before meals", True, "Steroid hyperglycemia peaks in the afternoon."),
    ("Potassium replacement", True, "Corrects hypokalemia."),
    ("A bone density scan and calcium with vitamin D", True, "Steroids cause osteoporosis."),
    ("Stop prednisone at once to help fight the infection", False, "Risks adrenal crisis."),
    ("Rely on the temperature to judge whether the antibiotic is working", False, "Steroids blunt fever.")],
   "Continue or stress-dose the steroid and treat its effects: glucose, potassium and bone.",
   "Generate a plan for a client on long-term steroids.",
   ex=[note("Orders", "Ceftriaxone and azithromycin. Prednisone 30 mg daily. Insulin correction scale. Potassium "
       "chloride 40 mEq PO.", id="cush-orders")]),

grouping(M(SIC, INJURY, "take_action", "difficult", ENDO, "fund"),
   "For each safety risk, select the actions the nurse should take.",
   [("Falls from muscle weakness", [
       ("A raised chair or toilet seat and grab bars", True, "Reduces the effort of standing."),
       ("Physical therapy referral", True, "Strengthening and safe transfers."),
       ("Keep the client on bed rest to avoid falls", False, "Bed rest worsens weakness.")]),
    ("Fragile skin", [
       ("Use a lift sheet when repositioning", True, "Prevents shearing and tears."),
       ("Pad the bed rails and avoid tape on the skin", True, "Prevents skin tears."),
       ("Massage the bruised areas firmly", False, "Can cause further injury.")]),
    ("Infection", [
       ("Strict hand hygiene by staff and visitors", True, "Reduces exposure."),
       ("Avoid rooming the client with someone who has an infection", True, "Limits spread."),
       ("Give live vaccines while in hospital", False, "Live vaccines are avoided on high-dose steroids.")])],
   "Prevent falls, protect fragile skin and reduce infection exposure.",
   "Take action to keep a client on long-term steroids safe.",
   ex=[note("Day 2", "Able to walk with a walker and one assist. Skin tear on the forearm from a blood pressure cuff.",
       id="cush-day2")]),

matrix_mc(M(PPT, ENDO_MEDS, "evaluate_outcomes", "difficult", ENDO, "ph"),
   "The rheumatologist plans to taper the prednisone. For each statement by the client, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll lower the dose slowly on the schedule, not stop it suddenly.\"", 0, "Tapering lets the adrenals recover."),
    ("\"If I feel weak, dizzy or sick while tapering, I'll call.\"", 0, "Signs of adrenal insufficiency."),
    ("\"I'll take prednisone in the morning with food.\"", 0, "Matches the body's rhythm and protects the stomach."),
    ("\"Once I'm off it, my face will go back to normal within a week.\"", 1, "Changes fade over months."),
    ("\"I'll skip my dose if my blood sugar is high.\"", 1, "Never skip; call about high glucose."),
    ("\"I'll tell any doctor I've taken steroids in the past year.\"", 0, "Stress dosing may still be needed.")],
   "Teaching covers slow tapering, warning signs, morning dosing with food and never skipping.",
   "Evaluate steroid taper teaching.",
   ex=[note("Day 5", "Pneumonia improving. Glucose 140 to 220 mg/dL. Potassium 3.9 mEq/L.", id="cush-day5")]),
]

# ================================================================================================
# Case 3: SIADH with a hyponatremic seizure
# ================================================================================================
siadh_tabs = [
    ("siadh-notes", "Nurses' notes", [note("Oncology unit, 1400",
        "A 66-year-old client with small cell lung cancer has had nausea and headache for 3 days and is now confused. "
        "Weight is up 2 kg with no edema. At 1400 the client has a generalized seizure lasting 90 seconds. Weight "
        "70 kg.", id="siadh-note")]),
    ("siadh-labs", "Laboratory results", [table("Laboratory results", ["1300", "Reference"], [
        ("Serum sodium", ["116 mEq/L", "135 to 145"]),
        ("Serum osmolality", ["242 mOsm/kg", "275 to 295"]),
        ("Urine osmolality", ["560 mOsm/kg", "-"]),
        ("Urine sodium", ["62 mEq/L", "-"]),
        ("TSH and cortisol", ["Normal", "-"])], id="siadh-table")]),
]

siadh_qs = [
hl_table(M(PA, PIT, "recognize_cues", "moderate", ENDO, "ms lab"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Seizure", "Generalized, 90 seconds"], True, "Severe symptomatic hyponatremia."),
    (["Serum sodium", "116 mEq/L"], True, "Severe hyponatremia."),
    (["Mental status", "Confused"], True, "Cerebral edema."),
    (["Urine osmolality", "560 mOsm/kg"], False, "Supports the diagnosis, but it is not acted on directly."),
    (["TSH and cortisol", "Normal"], False, "Rules out other causes."),
    (["Weight", "Up 2 kg"], False, "Water retention; context.")],
   "A seizure with a sodium of 116 is an emergency.",
   "Recognize cues of severe hyponatremia."),

matrix_mc(M(RRP, LABS, "analyze_cues", "difficult", ENDO, "lab ms"),
   "For each finding, indicate whether it supports SIADH.",
   ["Supports SIADH", "Does not support SIADH"],
   [("Low serum osmolality", 0, "Excess water dilutes the blood."),
    ("Concentrated urine (osmolality 560)", 0, "ADH keeps the kidneys holding water."),
    ("High urine sodium (62)", 0, "The kidneys keep excreting sodium."),
    ("Normal thyroid and adrenal function", 0, "Required before diagnosing SIADH."),
    ("Edema and crackles", 1, "SIADH causes water gain without edema; edema suggests heart failure or cirrhosis.")],
   "SIADH: dilute blood, inappropriately concentrated urine, high urine sodium, no edema, normal thyroid and "
   "adrenals.",
   "Analyze lab results in SIADH."),

cloze(M(PA, PIT, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The SIADH is most likely caused by {{b1}}. The priority now is {{b2}}, and the sodium should rise by no more "
   "than {{b3}} in the first 24 hours.",
   [[("the small cell lung cancer", True, "These tumors often secrete ADH."),
     ("drinking too much water", False, "Not supported."),
     ("heart failure", False, "No edema."),
     ("adrenal insufficiency", False, "Cortisol is normal.")],
    [("a small rise in sodium to stop seizures", True, "A few mEq/L reduces cerebral edema."),
     ("normalizing the sodium today", False, "Too fast."),
     ("starting chemotherapy", False, "Later."),
     ("fluid restriction alone", False, "Too slow for a seizure.")],
    [("8 to 10 mEq/L", True, "Faster correction risks osmotic demyelination."),
     ("20 mEq/L", False, "Far too fast."),
     ("2 mEq/L", False, "Too little for symptomatic hyponatremia."),
     ("30 mEq/L", False, "Dangerous.")]],
   "Tumor ADH causes SIADH; raise sodium a little to stop seizures, then correct slowly.",
   "Prioritize treatment in symptomatic SIADH."),

matrix_mc(M(PA, LYTES, "generate_solutions", "difficult", ENDO, "ms ph"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("3% sodium chloride 100 mL IV bolus, repeated if seizures continue", 0, "Rapid small rise in sodium."),
    ("Sodium checks every 2 hours", 0, "Guards against overcorrection."),
    ("Seizure precautions", 0, "Safety."),
    ("Fluid restriction once stable", 0, "Ongoing SIADH treatment."),
    ("0.9% sodium chloride 2 L rapidly", 1, "In SIADH, the sodium is excreted and the water kept, which can lower sodium further."),
    ("Free water flushes with tube feeds", 1, "Adds free water.")],
   "Hypertonic saline in small boluses with close sodium checks; isotonic saline and free water can worsen SIADH.",
   "Generate solutions for symptomatic SIADH.",
   ex=[note("Orders, 1410", "3% sodium chloride 100 mL over 10 minutes; may repeat twice. Sodium every 2 hours. "
       "Seizure precautions. Fluid restriction 1 L/day when stable.", id="siadh-orders")]),

sata(M(PA, LYTES, "take_action", "difficult", ENDO, "ms"),
   "Review the sodium trend. Which actions should the nurse take?",
   [("Report the sodium rise to the provider now", True, "A rise of 12 mEq/L in 10 hours is above the safe limit."),
    ("Stop the hypertonic saline", True, "Prevents further rise."),
    ("Measure urine output hourly", True, "Large urine output can raise sodium quickly."),
    ("Anticipate D5W or desmopressin to slow or reverse the rise", True, "Used to re-lower sodium when overcorrection occurs."),
    ("Continue the current plan because the client is awake", False, "Overcorrection must be stopped."),
    ("Give a second 3% bolus to reach normal sodium", False, "Dangerous.")],
   "Overcorrection of chronic hyponatremia must be stopped and sometimes reversed to prevent osmotic demyelination.",
   "Take action for overcorrection of hyponatremia.",
   ex=[flow("Sodium trend", ["1300", "1600", "2000", "2300"], [
       ("Sodium", ["116", "120", "125", "128"]),
       ("Urine output", ["40 mL/h", "60 mL/h", "250 mL/h", "300 mL/h"]),
       ("Mental status", ["Seizure at 1400", "Drowsy", "Awake", "Awake"])], id="siadh-trend")]),

matrix_mc(M(SIC, INJURY, "evaluate_outcomes", "difficult", ENDO, "ms fund"),
   "Review the findings on day 3. For each finding, indicate whether it is safe and expected or needs follow-up.",
   ["Safe and expected", "Needs follow-up"],
   [("Sodium 131 mEq/L", 0, "Corrected slowly."),
    ("No further seizures; oriented", 0, "Recovered."),
    ("The client drinks from the sink when thirsty", 1, "Breaks the fluid restriction; discuss strategies such as ice chips and mouth care."),
    ("The client has trouble swallowing and slurred speech that began today", 1,
     "Possible osmotic demyelination after correction; report urgently."),
    ("Bed in low position with side rails padded", 0, "Seizure precautions continue appropriately.")],
   "Sodium is recovering, but fluid restriction lapses and new neurologic signs need follow-up.",
   "Evaluate outcomes in SIADH.",
   ex=[flow("Day 3", ["Day 3"], [
       ("Sodium", ["131 mEq/L"]),
       ("Neurologic", ["Oriented; new slurred speech and trouble swallowing today"]),
       ("Intake", ["Seen drinking from the sink"]),
       ("Safety", ["Bed low, rails padded"])], id="siadh-day3")]),
]

# ================================================================================================
# Case 4: Diabetes insipidus after pituitary surgery
# ================================================================================================
di_tabs = [
    ("di-notes", "Nurses' notes", [note("Neurosurgical unit, postoperative day 1",
        "A 47-year-old client had transsphenoidal removal of a pituitary adenoma yesterday. Nasal packing is in "
        "place. Since midnight the client has passed large amounts of pale urine and is very thirsty. The client "
        "reports clear, watery drip from the nose when leaning forward.", id="di-note")]),
    ("di-data", "Intake, output and labs", [table("Intake, output and labs", ["0000 to 0600", "Reference"], [
        ("Urine output", ["350 to 450 mL/hour", "-"]),
        ("Urine specific gravity", ["1.002", "1.005 to 1.030"]),
        ("Serum sodium", ["151 mEq/L", "135 to 145"]),
        ("Serum osmolality", ["306 mOsm/kg", "275 to 295"]),
        ("Glucose", ["112 mg/dL", "70 to 110"])], id="di-table")]),
]

di_qs = [
hl_table(M(PA, PIT, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Urine output", "350 to 450 mL/hour"], True, "Polyuria."),
    (["Urine specific gravity", "1.002"], True, "Very dilute urine."),
    (["Serum sodium", "151 mEq/L"], True, "Hypernatremia from water loss."),
    (["Nose", "Clear, watery drip when leaning forward"], True, "Possible CSF leak after transsphenoidal surgery."),
    (["Glucose", "112 mg/dL"], False, "Not high enough to cause polyuria; rules out osmotic diuresis."),
    (["Thirst", "Very thirsty"], False, "An expected response to water loss; protective while the client can drink.")],
   "Dilute polyuria with rising sodium is diabetes insipidus; clear nasal drip may be CSF.",
   "Recognize cues of diabetes insipidus and CSF leak."),

matrix_mc(M(PA, PIT, "analyze_cues", "difficult", ENDO, "ms"),
   "For each finding, indicate whether it fits diabetes insipidus or SIADH.",
   ["Diabetes insipidus", "SIADH"],
   [("Large volumes of dilute urine", 0, "Too little ADH."),
    ("Serum sodium 151 mEq/L", 0, "Water loss concentrates sodium."),
    ("Concentrated urine with low serum sodium", 1, "Too much ADH."),
    ("Weight gain without edema", 1, "Water retention."),
    ("Intense thirst", 0, "Driven by high osmolality.")],
   "DI loses water (high sodium, dilute urine); SIADH keeps water (low sodium, concentrated urine).",
   "Analyze DI against SIADH."),

cloze(M(PA, PIT, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client has {{b1}} from {{b2}}. The clear nasal drip should be tested for {{b3}}.",
   [[("central diabetes insipidus", True, "Surgery near the posterior pituitary disrupts ADH."),
     ("nephrogenic diabetes insipidus", False, "That is a kidney problem."),
     ("SIADH", False, "The sodium is high."),
     ("osmotic diuresis", False, "Glucose is normal.")],
    [("disruption of ADH release after pituitary surgery", True, "Often temporary."),
     ("too much IV fluid", False, "Would not raise sodium."),
     ("lithium", False, "Not taking it."),
     ("diabetes mellitus", False, "Glucose is normal.")],
    [("glucose (or beta-2 transferrin), which shows cerebrospinal fluid", True, "Nasal mucus has little glucose."),
     ("bacteria", False, "Not the first test."),
     ("protein only", False, "Not specific."),
     ("blood type", False, "Not relevant.")]],
   "Central DI after pituitary surgery, and a possible CSF leak that needs testing.",
   "Prioritize problems after transsphenoidal surgery."),

sata(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", ENDO, "ph ms"),
   "Which orders should the nurse anticipate?",
   [("Desmopressin as prescribed", True, "Replaces ADH."),
    ("Hourly urine output and specific gravity", True, "Tracks response."),
    ("Serum sodium every 4 to 6 hours", True, "Guards against both hypernatremia and overcorrection."),
    ("Let the client drink to thirst", True, "Thirst protects the awake client."),
    ("Fluid restriction to 1 L a day", False, "Dangerous in DI."),
    ("Encourage the client to blow the nose to clear the drip", False, "Raises pressure and worsens a CSF leak.")],
   "Desmopressin with close output and sodium monitoring; let the client drink, and protect the surgical site.",
   "Generate the plan for postoperative DI.",
   ex=[note("Orders, 0700", "Desmopressin 1 mcg IV. Test nasal drainage for glucose. Head of bed 30 degrees.",
       id="di-orders")]),

sata(M(PA, PIT, "take_action", "difficult", ENDO, "ms"),
   "Which instructions and actions should the nurse give the client now?",
   [("Do not blow your nose, cough hard or bend over", True, "Prevents pressure on the surgical site."),
    ("Sneeze with your mouth open", True, "Lowers pressure in the nose."),
    ("Do not use a straw or brush your teeth until cleared", True, "Protects the incision under the upper lip."),
    ("Report a headache or stiff neck", True, "Possible meningitis with a CSF leak."),
    ("Lie flat to stop the drip", False, "The head of the bed is raised."),
    ("Use a nasal decongestant spray if the nose feels blocked", False, "Not without the surgeon's order.")],
   "Protect the transsphenoidal site and watch for meningitis while a CSF leak is assessed.",
   "Take action to protect the transsphenoidal surgery site.",
   ex=[flow("1000", ["1000"], [
       ("Nasal drainage glucose", ["Positive"]),
       ("Urine output", ["120 mL/hour after desmopressin"]),
       ("Sodium", ["146 mEq/L"])], id="di-1000")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", ENDO, "ms mgmt"),
   "Review the findings on day 3. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Urine output 80 to 120 mL/hour; specific gravity 1.015", 0, "DI controlled."),
    ("Nasal drip has stopped", 0, "The leak has sealed."),
    ("Sodium 128 mEq/L after two more desmopressin doses", 1, "Overcorrection: hold desmopressin and tell the provider."),
    ("Temperature 38.5 C (101.3 F), headache and neck stiffness", 1, "Possible meningitis; urgent review."),
    ("Endocrinology has reviewed the cortisol and thyroid plan", 0, "Pituitary surgery can affect other hormones; collaboration is in place.")],
   "DI and the leak have improved, but low sodium from desmopressin and signs of meningitis need urgent action.",
   "Evaluate outcomes after pituitary surgery.",
   ex=[flow("Day 3", ["Day 3"], [
       ("Urine", ["80 to 120 mL/hour, SG 1.015"]),
       ("Nose", ["No drip"]),
       ("Sodium", ["128 mEq/L"]),
       ("Temperature", ["38.5 C (101.3 F)"]),
       ("Neurologic", ["Headache, neck stiffness"]),
       ("Consults", ["Endocrinology reviewed hydrocortisone and thyroid plan"])], id="di-day3")]),
]

cases = [
    case("Adrenal crisis in Addison disease", add_tabs, add_qs),
    case("Cushing syndrome from long-term prednisone", cush_tabs, cush_qs),
    case("SIADH with a hyponatremic seizure", siadh_tabs, siadh_qs),
    case("Diabetes insipidus after pituitary surgery", di_tabs, di_qs),
]

BUILT = build(110, "endocrine-cases-adrenal-pituitary",
              "Endocrine case studies: adrenal crisis, Cushing syndrome, SIADH and diabetes insipidus",
              items, cases, case_conversion=True)
