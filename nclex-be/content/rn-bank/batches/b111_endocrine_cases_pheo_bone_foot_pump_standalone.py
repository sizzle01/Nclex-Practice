"""Batch 111 (case conversion, chunk 3: endocrine): four full NGN case studies and four standalone items
(CASE-MAP.md).

Cases: pheochromocytoma; osteoporosis with a vertebral compression fracture; a diabetic foot ulcer with
osteomyelitis; insulin pump failure leading to DKA in type 1 diabetes. Each case walks the six CJMM steps in order,
unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
ENDO, MSK = "endocrine", "musculoskeletal"
ADRENAL = "adrenal-disorders"
CV_MEDS = "cardiovascular-medications"
BONE = "bone-health-and-osteoporosis"
ENDO_MEDS = "endocrine-medications"
INJURY = "injury-prevention"
WOUND = "wound-care"
ABX = "antimicrobial-therapy"
INS = "insulin-therapy"
CALC = "medication-dose-calculations"
DKA = "diabetic-ketoacidosis"
DSM = "diabetes-self-management"
HOME = "equipment-and-home-safety"
COLLAB = "collaboration-and-referrals"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
PAIN = "pain-assessment-and-comfort"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PPT, INS, "prioritize_hypotheses", "difficult", ENDO, "ada ph"),
   "Use the glucose log to fill in the diagram: the problem this client most likely has, 2 actions for the nurse, "
   "and 2 parameters to track.",
   [("Nocturnal hypoglycemia with morning rebound (Somogyi effect)", True,
     "A 0300 low followed by a high fasting glucose after an evening NPH increase."),
    ("Dawn phenomenon", False, "In dawn phenomenon, the 0300 glucose is normal or high."),
    ("Insulin resistance from weight gain", False, "Would not cause a 0300 low."),
    ("Missed evening insulin", False, "Would not cause a 0300 low.")],
   [("Ask for the evening NPH dose to be reduced", True, "Less overnight insulin stops the low and the rebound."),
    ("Teach a bedtime snack meanwhile", True, "Reduces overnight lows."),
    ("Increase the evening NPH dose as the client asked", False, "Would deepen the 0300 low."),
    ("Move all of the insulin doses to the morning", False, "Not a standard fix."),
    ("Stop checking the glucose during the night", False, "The 0300 check is how the problem was found.")],
   [("Glucose at 0300", True, "Confirms the pattern and the fix."),
    ("Fasting morning glucose", True, "Should fall as the rebound stops."),
    ("Weekly body weight", False, "Not the main guide."),
    ("Blood pressure", False, "Not the main guide."),
    ("Urine ketones daily", False, "Not the main guide.")],
   "A 0300 low with a high fasting glucose after more evening insulin is the Somogyi effect: reduce the evening dose, "
   "not increase it.",
   "Distinguish the Somogyi effect from the dawn phenomenon.",
   ex=[flow("Glucose log, type 1 diabetes", ["Night 1", "Night 2", "Night 3"], [
       ("Evening NPH", ["14 units", "16 units", "18 units"]),
       ("2200 glucose", ["168", "160", "152"]),
       ("0300 glucose", ["62", "54", "48"]),
       ("0700 fasting glucose", ["228", "246", "262"]),
       ("Client report", ["Night sweats, nightmares", "Night sweats", "Headache on waking"])], id="somogyi-log")]),

dtable(M(PPT, INS, "evaluate_outcomes", "difficult", ENDO, "ada"),
   "A client with DKA is on an insulin infusion. For each finding at hour 14, choose how the nurse should "
   "interpret it.",
   ["Finding", "Hour 14", "Interpretation"],
   [(["Anion gap", "11 (was 28)"], [
       ("Closed: the ketoacidosis has resolved", True, "A gap of 12 or less with bicarbonate of 18 or more marks resolution."),
       ("Still open", False, "It is in the normal range."),
       ("Not useful for DKA", False, "It is the key marker.")]),
    (["Glucose", "176 mg/dL on D5 with insulin"], [
       ("On target while insulin continues", True, "Dextrose keeps glucose up so insulin can clear ketones."),
       ("Too high; increase the insulin", False, "The target is about 150 to 200 during treatment."),
       ("Hypoglycemia", False, "It is not low.")]),
    (["Eating", "Tolerating breakfast"], [
       ("Ready to change to subcutaneous insulin", True, "Gap closed and eating are the criteria."),
       ("Not ready", False, "The criteria are met."),
       ("Needs to stay NPO", False, "No reason.")]),
    (["Plan", "Stop the infusion now and give basal insulin at 2100"], [
       ("Unsafe: give basal insulin 1 to 2 hours before stopping the infusion", True,
        "IV insulin lasts minutes; without overlap, ketosis returns."),
       ("Safe", False, "There would be hours with no insulin."),
       ("Unnecessary to give basal insulin", False, "Type 1 always needs basal insulin.")])],
   "When the gap closes and the client eats, give basal insulin 1 to 2 hours before stopping the infusion.",
   "Evaluate readiness to transition from IV insulin in DKA.",
   ex=[flow("DKA trend", ["Hour 0", "Hour 8", "Hour 14"], [
       ("Anion gap", ["28", "19", "11"]),
       ("Bicarbonate", ["8", "14", "20"]),
       ("Glucose", ["520", "210", "176"]),
       ("Intake", ["NPO", "Sips", "Breakfast"])], id="dka-trend")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", ENDO, "mgmt ms"),
   "On an endocrine unit, the nurse has four new findings at the same time. Which client should the nurse go to "
   "first?",
   [("A client with Addison disease whose morning hydrocortisone is 30 minutes late and who feels tired", False,
     "Give the dose soon, but the client is stable."),
    ("A client 6 hours after thyroidectomy who says their lips feel numb and tingly", False,
     "Early hypocalcemia needs assessment soon; the airway is not yet threatened."),
    ("A client on an insulin infusion whose glucose is 62 mg/dL and who is sweating and confused", True,
     "Symptomatic hypoglycemia on IV insulin can deteriorate within minutes."),
    ("A client with diabetes insipidus who has finished a jug of water and is asking for another", False,
     "Thirst is protective; replace the water and continue monitoring.")],
   "Symptomatic hypoglycemia on an insulin infusion is the most immediate threat.",
   "Prioritize clients on an endocrine unit."),

sata(M(MOC, DELEG, "take_action", "difficult", ENDO, "mgmt"),
   "Which tasks can the RN delegate to the unlicensed assistive personnel (UAP) on an endocrine unit?",
   [("Check and record a fingerstick glucose on a stable client", True, "A routine measurement after training."),
    ("Measure and record urine output every hour for a client with DI", True, "Measuring and recording."),
    ("Report a glucose below 70 mg/dL to the RN at once", True, "Reporting against stated limits."),
    ("Decide whether a client with a glucose of 62 mg/dL needs juice or IV dextrose", False, "Needs RN judgment."),
    ("Adjust an insulin infusion rate", False, "Medication titration is the RN's role."),
    ("Teach a client how to use an insulin pen", False, "Teaching is an RN role.")],
   "UAPs can measure, record and report; decisions, medication changes and teaching stay with the RN.",
   "Delegate safely on an endocrine unit."),
]

# ================================================================================================
# Case 1: Pheochromocytoma
# ================================================================================================
pheo_tabs = [
    ("pheo-notes", "Nurses' notes", [note("Emergency department",
        "A 42-year-old client has had episodes of pounding headache, palpitations and drenching sweats for 6 months. "
        "During an episode today the client is pale and tremulous. A CT for kidney stones last month showed a 4 cm "
        "left adrenal mass. The client's mother had a similar tumor.", id="pheo-note")]),
    ("pheo-data", "Vital signs and labs", [table("Vital signs and labs", ["Now", "Reference"], [
        ("Blood pressure", ["238/130 mm Hg", "-"]),
        ("Heart rate", ["138/min", "-"]),
        ("Glucose", ["212 mg/dL", "70 to 110"]),
        ("Plasma free metanephrines", ["Six times the upper limit", "-"])], id="pheo-table")]),
]

pheo_qs = [
hl_text(M(PA, ADRENAL, "recognize_cues", "moderate", ENDO, "ms"),
   "Highlight the findings that point to a catecholamine-secreting tumor.",
   ["The client has episodes of ",
    ("pounding headache, palpitations and sweating", True, "The classic triad."),
    ". Today the client is ",
    ("pale and tremulous", True, "Vasoconstriction and adrenergic surge."),
    ", with a blood pressure of ",
    ("238/130 mm Hg", True, "Severe paroxysmal hypertension."),
    ". A CT showed ",
    ("a 4 cm left adrenal mass", True, "An adrenal tumor."),
    ". The client ",
    ("had a CT for kidney stones", False, "The reason for the scan, not a sign of the tumor."),
    ". The plasma metanephrines are ",
    ("six times the upper limit", True, "Confirms excess catecholamines."),
    "."],
   "Episodic headache, palpitations and sweating with severe hypertension and high metanephrines mean "
   "pheochromocytoma.",
   "Recognize cues of pheochromocytoma."),

matrix_mc(M(PA, ADRENAL, "analyze_cues", "difficult", ENDO, "ms"),
   "For each finding, indicate the main receptor effect responsible.",
   ["Alpha-1 (vasoconstriction)", "Beta-1 (heart)", "Metabolic effect of catecholamines"],
   [("Blood pressure 238/130 mm Hg", 0, "Intense vasoconstriction."),
    ("Heart rate 138/min with palpitations", 1, "Increased rate and contractility."),
    ("Pale skin", 0, "Skin vessels constrict."),
    ("Glucose 212 mg/dL", 2, "Catecholamines raise glucose."),
    ("Sweating", 2, "Sympathetic activation of sweat glands.")],
   "Excess catecholamines act on alpha and beta receptors and on metabolism.",
   "Analyze catecholamine effects."),

cloze(M(PPT, CV_MEDS, "prioritize_hypotheses", "difficult", ENDO, "ph ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client is in {{b1}}. Giving a beta blocker before an alpha blocker would {{b2}} because {{b3}}.",
   [[("a catecholamine hypertensive crisis", True, "Risks stroke, MI and heart failure."),
     ("a panic attack", False, "The labs and mass show a tumor."),
     ("thyroid storm", False, "Not supported."),
     ("hypoglycemia", False, "Glucose is high.")],
    [("raise the blood pressure further", True, "Unopposed alpha stimulation."),
     ("safely lower the blood pressure", False, "Dangerous."),
     ("cure the tumor", False, "Surgery removes the tumor."),
     ("have no effect", False, "It is harmful.")],
    [("blocking beta-2 vasodilation leaves alpha vasoconstriction unopposed", True, "Alpha blockade must come first."),
     ("beta blockers raise heart rate", False, "They lower it."),
     ("beta blockers release catecholamines", False, "Not the mechanism."),
     ("the tumor is on the left", False, "Not relevant.")]],
   "Alpha blockade always comes before beta blockade in pheochromocytoma.",
   "Prioritize drug sequencing in pheochromocytoma."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", ENDO, "ph ms"),
   "For each part of the plan, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Phentolamine IV for the current crisis", 0, "A short-acting alpha blocker."),
    ("Phenoxybenzamine for 10 to 14 days before surgery", 0, "Preoperative alpha blockade."),
    ("A high-salt diet and fluids once alpha-blocked", 0, "Restores the contracted blood volume and prevents hypotension."),
    ("Metoprolol now for the heart rate", 1, "Before alpha blockade it can cause a crisis."),
    ("Deep palpation of the abdomen to feel the mass", 1, "Can release catecholamines."),
    ("Metoclopramide for nausea", 1, "Can trigger a crisis.")],
   "Alpha blockade and volume expansion first; avoid early beta blockers, palpation and triggering drugs.",
   "Generate the plan for pheochromocytoma.",
   ex=[note("Plan", "Admit. Phentolamine as needed. Start phenoxybenzamine. Adrenalectomy in 2 weeks.", id="pheo-plan")]),

sata(M(PA, ADRENAL, "take_action", "difficult", ENDO, "ms"),
   "The client had a left adrenalectomy. Review the findings 3 hours after surgery. Which actions should the "
   "nurse take?",
   [("Treat the glucose of 52 mg/dL per protocol", True, "Insulin rebounds when catecholamines fall suddenly."),
    ("Give the prescribed fluid bolus for the low blood pressure", True, "Vessels dilate after the catecholamine source is gone."),
    ("Check glucose every hour for the next 24 hours", True, "Hypoglycemia can recur."),
    ("Notify the surgeon of the blood pressure and glucose", True, "Both need review."),
    ("Restart phenoxybenzamine to prevent a crisis", False, "The tumor is gone; this would worsen hypotension."),
    ("Raise the head of the bed to 90 degrees", False, "Can worsen hypotension.")],
   "After adrenalectomy for pheochromocytoma, hypotension and rebound hypoglycemia are common.",
   "Take action after adrenalectomy.",
   ex=[flow("3 hours after surgery", ["Post-op hour 3"], [
       ("Blood pressure", ["84/50 mm Hg"]),
       ("Heart rate", ["104/min"]),
       ("Glucose", ["52 mg/dL"]),
       ("Client", ["Drowsy, sweating"])], id="pheo-postop")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", ENDO, "ms mgmt"),
   "Review the plan at discharge. For each item, indicate whether it is complete or needs follow-up.",
   ["Complete", "Needs follow-up"],
   [("Blood pressure 124/76 mm Hg off all antihypertensives", 0, "Cure of the hypertension."),
    ("Plasma metanephrines to be repeated in 2 to 6 weeks", 0, "Confirms complete removal."),
    ("Genetic counseling not offered despite the mother's tumor", 1, "Hereditary syndromes need genetic referral and family screening."),
    ("Annual metanephrine testing planned", 0, "Tumors can recur."),
    ("The client has not been told to report recurring headaches and palpitations", 1, "Recurrence warning signs.")],
   "Cure is confirmed by testing; family history calls for genetic referral, and recurrence teaching is needed.",
   "Evaluate discharge planning after pheochromocytoma surgery.",
   ex=[note("Discharge, day 4", "Glucose stable. Off IV fluids.", id="pheo-dc")]),
]

# ================================================================================================
# Case 2: Osteoporosis with a vertebral compression fracture
# ================================================================================================
osteo_tabs = [
    ("osteo-notes", "Nurses' notes", [note("Orthopedic clinic",
        "A 74-year-old client felt sudden mid-back pain while lifting a bag of groceries. Height is 4 cm less than "
        "5 years ago, and the upper back is rounded. The client smokes, drinks 3 glasses of wine daily, has taken "
        "omeprazole for 6 years, rarely goes outside, and takes no supplements. The client has fallen twice this "
        "year, once on a loose rug.", id="osteo-note")]),
    ("osteo-data", "Results", [table("Results", ["Result", "Reference"], [
        ("Spine x-ray", ["Compression fracture of T12", "-"]),
        ("DXA T-score (hip)", ["-3.1", "-1.0 or higher is normal"]),
        ("25-hydroxy vitamin D", ["14 ng/mL", "30 to 100"]),
        ("Calcium", ["9.0 mg/dL", "8.5 to 10.5"]),
        ("Creatinine clearance", ["52 mL/min", "-"])], id="osteo-table")]),
]

osteo_qs = [
hl_table(M(HPM, BONE, "recognize_cues", "moderate", MSK, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["Spine x-ray", "T12 compression fracture"], True, "A fragility fracture."),
    (["T-score", "-3.1"], True, "Osteoporosis (-2.5 or lower)."),
    (["Vitamin D", "14 ng/mL"], True, "Deficiency."),
    (["Falls", "Twice this year"], True, "High fracture risk."),
    (["Calcium", "9.0 mg/dL"], False, "Normal; needed before starting a bisphosphonate."),
    (["Creatinine clearance", "52 mL/min"], False, "Adequate for oral bisphosphonates (above 35).")],
   "A fragility fracture, a low T-score, low vitamin D and falls all need action.",
   "Recognize cues of osteoporosis."),

matrix_mc(M(HPM, BONE, "analyze_cues", "difficult", MSK, "ms"),
   "For each risk factor, indicate whether it can be changed.",
   ["Modifiable", "Not modifiable"],
   [("Smoking", 0, "Can stop."),
    ("Three glasses of wine daily", 0, "Can reduce to one or fewer."),
    ("Long-term omeprazole", 0, "Can be reviewed; PPIs reduce calcium absorption."),
    ("Low vitamin D", 0, "Can be supplemented."),
    ("Age 74", 1, "Cannot change."),
    ("Loose rugs at home", 0, "Home hazards can be removed.")],
   "Most of this client's risks can be changed.",
   "Analyze osteoporosis risk factors."),

cloze(M(HPM, BONE, "prioritize_hypotheses", "difficult", MSK, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client's greatest future risk is {{b1}}, because {{b2}}.",
   [[("a hip fracture", True, "A vertebral fracture plus falls predicts hip fracture."),
     ("kidney stones", False, "Not the main risk."),
     ("hypercalcemia", False, "Calcium is normal."),
     ("rheumatoid arthritis", False, "Not related.")],
    [("one fragility fracture plus falls greatly raises the risk of another", True, "The strongest predictors."),
     ("the calcium is normal", False, "Not the reason."),
     ("the kidney function is reduced", False, "Not the main reason."),
     ("the client is still working", False, "Not stated.")]],
   "A prior fragility fracture and falls make another fracture, especially of the hip, likely.",
   "Prioritize fracture risk."),

matrix_mc(M(PPT, ENDO_MEDS, "generate_solutions", "difficult", MSK, "ph"),
   "Alendronate 70 mg weekly is prescribed. For each instruction, indicate whether it is correct or incorrect.",
   ["Correct", "Incorrect"],
   [("Take it first thing in the morning with a full glass of plain water", 0, "Absorption needs an empty stomach and water."),
    ("Stay upright for at least 30 minutes after taking it", 0, "Prevents esophagitis."),
    ("Wait 30 minutes before eating, drinking or other medicines", 0, "Food blocks absorption."),
    ("Take it with orange juice or coffee", 1, "Reduces absorption."),
    ("Take calcium at the same time", 1, "Calcium binds alendronate; take it later in the day."),
    ("Tell the dentist before any extraction", 0, "Risk of osteonecrosis of the jaw.")],
   "Correct alendronate use protects the esophagus and absorption; calcium and food are separated.",
   "Generate correct bisphosphonate instructions.",
   ex=[note("Plan", "Alendronate 70 mg weekly. Vitamin D3 2,000 units daily. Calcium 1,200 mg daily from diet and "
       "supplement. Physical therapy. Pain control.", id="osteo-plan")]),

grouping(M(SIC, INJURY, "take_action", "difficult", MSK, "fund"),
   "The nurse plans a home safety visit. For each area, select the actions that reduce fall risk.",
   [("Floors", [
       ("Remove loose rugs", True, "A cause of the client's fall."),
       ("Keep walkways clear of cords", True, "Trip hazard."),
       ("Use wax polish so floors are easy to clean", False, "Slippery.")]),
    ("Bathroom", [
       ("Install grab bars by the toilet and shower", True, "Support during transfers."),
       ("Use a nonslip mat in the shower", True, "Prevents slips."),
       ("Lock the bathroom door at night for privacy", False, "Delays help after a fall.")]),
    ("Night", [
       ("Use night-lights from bedroom to bathroom", True, "Most falls happen at night."),
       ("Sit on the bed edge before standing", True, "Prevents orthostatic dizziness."),
       ("Take a sleeping pill nightly to avoid getting up", False, "Sedatives increase falls.")])],
   "Removing hazards, adding supports and night lighting reduce falls.",
   "Take action to prevent falls at home.",
   ex=[note("Home visit", "The client lives alone in a two-story house.", id="osteo-home")]),

matrix_mc(M(HPM, BONE, "evaluate_outcomes", "difficult", MSK, "ms ph"),
   "At the 3-month visit, for each statement or finding, indicate whether it shows the plan is working or needs "
   "follow-up.",
   ["Working", "Needs follow-up"],
   [("\"I take alendronate on Sunday mornings with water and stay up for 30 minutes.\"", 0, "Correct use."),
    ("Vitamin D 34 ng/mL", 0, "Corrected."),
    ("\"I've cut back to one glass of wine.\"", 0, "Risk reduced."),
    ("\"It burns behind my breastbone when I swallow.\"", 1, "Esophagitis; report and review the drug."),
    ("No falls since the rugs were removed", 0, "Home changes are working."),
    ("Still smoking a pack a day", 1, "Offer cessation support.")],
   "Medication, vitamin D, alcohol and falls are improving; esophageal symptoms and smoking need follow-up.",
   "Evaluate osteoporosis management.",
   ex=[note("3-month visit", "Back pain 2 of 10. Walking daily with a cane.", id="osteo-m3")]),
]

# ================================================================================================
# Case 3: Diabetic foot ulcer with osteomyelitis
# ================================================================================================
foot_tabs = [
    ("foot-notes", "Nurses' notes", [note("Wound clinic",
        "A 61-year-old client with type 2 diabetes noticed a sock stained with drainage 2 weeks ago. There is a 2 cm "
        "ulcer under the first metatarsal head. The client feels no pain there. A sterile probe touches bone. The "
        "toe is red and swollen. Monofilament sensation is absent in both feet. Pedal pulses are weak. The client is "
        "afebrile.", id="foot-note")]),
    ("foot-data", "Results", [table("Results", ["Result", "Reference"], [
        ("HbA1c", ["10.4%", "below 7% goal"]),
        ("ESR", ["88 mm/hour", "below 20"]),
        ("WBC", ["10,200/mm3", "5,000 to 10,000"]),
        ("Ankle-brachial index", ["0.82", "0.9 to 1.3"]),
        ("MRI", ["Osteomyelitis of the first metatarsal head", "-"])], id="foot-table")]),
]

foot_qs = [
hl_table(M(BCC, WOUND, "recognize_cues", "moderate", ENDO, "ms ada"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["Probe", "Touches bone"], True, "Strongly suggests osteomyelitis."),
    (["MRI", "Osteomyelitis"], True, "Confirms bone infection."),
    (["Sensation", "Absent monofilament both feet"], True, "Loss of protective sensation; explains the painless ulcer."),
    (["ABI", "0.82"], True, "Peripheral arterial disease slows healing."),
    (["HbA1c", "10.4%"], True, "Poor control impairs healing and immunity."),
    (["Temperature", "Afebrile"], False, "Common even with deep infection in diabetes; not reassuring.")],
   "Neuropathy, arterial disease and poor control set up a deep infection that may not cause fever.",
   "Recognize cues of a diabetic foot infection."),

cloze(M(BCC, WOUND, "analyze_cues", "difficult", ENDO, "ms ada"),
   "Complete the sentences by choosing from the lists of options.",
   "The ulcer was not noticed because of {{b1}}. A raised ESR with a positive probe-to-bone test means {{b2}}.",
   [[("peripheral neuropathy", True, "Loss of pain sensation."),
     ("good circulation", False, "Circulation is reduced."),
     ("the client's age", False, "Not the reason."),
     ("dry skin", False, "Not the reason.")],
    [("infection of the bone", True, "Osteomyelitis needs weeks of treatment."),
     ("a superficial skin infection", False, "The probe reached bone."),
     ("gout", False, "Not supported."),
     ("normal healing", False, "Not normal.")]],
   "Neuropathy hides injury; probe-to-bone with a raised ESR means osteomyelitis.",
   "Analyze diabetic foot findings."),

matrix_mc(M(BCC, WOUND, "prioritize_hypotheses", "difficult", ENDO, "ms"),
   "For each risk, indicate its priority.",
   ["High priority", "Lower priority"],
   [("Spreading infection and amputation", 0, "Osteomyelitis with arterial disease threatens the limb."),
    ("Poor healing from high glucose", 0, "Glucose control is part of treatment."),
    ("Pressure on the ulcer with walking", 0, "Continued pressure stops healing."),
    ("Dry skin on the other foot", 1, "Needs care but is less urgent.")],
   "Infection, glucose and pressure on the ulcer are the priorities.",
   "Prioritize diabetic foot ulcer risks."),

sata(M(BCC, WOUND, "generate_solutions", "difficult", ENDO, "ms ada"),
   "Which parts of the plan should the nurse anticipate?",
   [("Offloading with a total contact cast or special boot", True, "Removes pressure from the ulcer."),
    ("Bone biopsy or deep culture before antibiotics when possible", True, "Guides weeks of therapy."),
    ("Intensify insulin therapy", True, "Better control helps healing."),
    ("Vascular surgery review", True, "An ABI of 0.82 may need revascularization."),
    ("Soak the foot daily in warm water", False, "Macerates skin; burns are a risk with neuropathy."),
    ("Walk barefoot at home to toughen the skin", False, "Causes more injury.")],
   "Offload, culture, treat infection for weeks, control glucose and check circulation.",
   "Generate the plan for an infected diabetic foot ulcer.",
   ex=[note("Plan", "Admit for bone biopsy and IV antibiotics. Basal-bolus insulin. Vascular consult.", id="foot-plan")]),

calc(M(PPT, CALC, "take_action", "difficult", ENDO, "ph ada"),
   "The correction insulin order is: 1 unit of insulin lispro for every 30 mg/dL above 150 mg/dL, rounded down to "
   "the nearest whole unit, added to the mealtime dose. The client's prelunch glucose is 312 mg/dL. How many units "
   "of correction insulin should the nurse give? Record your answer as a whole number.",
   "units", 0, "Correction insulin", 5,
   "312 - 150 = 162. 162 / 30 = 5.4, rounded down to 5 units, given with the mealtime dose.",
   "Calculate correction insulin.",
   ex=[note("Prelunch", "Glucose 312 mg/dL. Mealtime lispro 6 units ordered.", id="foot-lunch")]),

matrix_mc(M(HPM, DSM, "evaluate_outcomes", "difficult", ENDO, "ada"),
   "Before discharge, the client describes foot care. For each statement, indicate whether it shows understanding "
   "or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll check both feet every day, using a mirror for the soles.\"", 0, "Finds injuries early."),
    ("\"I'll test bath water with my elbow, not my foot.\"", 0, "Prevents burns."),
    ("\"I'll cut corns off with a razor blade.\"", 1, "A podiatrist treats corns."),
    ("\"I'll wear the offloading boot even for short trips to the bathroom.\"", 0, "Every step on the ulcer delays healing."),
    ("\"I'll put lotion between my toes.\"", 1, "Moisture between toes promotes fungal infection."),
    ("\"I'll always wear shoes and check inside them first.\"", 0, "Prevents injury.")],
   "Daily inspection, safe temperature checks, constant offloading and professional care of corns protect the feet.",
   "Evaluate diabetic foot care teaching.",
   ex=[note("Discharge", "6 weeks of IV antibiotics through a PICC line. HbA1c recheck in 3 months.", id="foot-dc")]),
]

# ================================================================================================
# Case 4: Insulin pump failure leading to DKA
# ================================================================================================
pump_tabs = [
    ("pump-notes", "Nurses' notes", [note("Emergency department, 0900",
        "A 26-year-old client with type 1 diabetes uses an insulin pump with rapid-acting insulin and no long-acting "
        "insulin. The infusion set was changed 3 days ago. The client woke with nausea and vomiting; the glucose was "
        "high and kept rising despite two correction boluses through the pump. The tubing has a kink near the site.",
        id="pump-note")]),
    ("pump-labs", "Labs", [table("Labs", ["0900", "Reference"], [
        ("Glucose", ["412 mg/dL", "70 to 110"]),
        ("Beta-hydroxybutyrate", ["4.1 mmol/L", "below 0.6"]),
        ("pH / bicarbonate", ["7.21 / 12 mEq/L", "-"]),
        ("Potassium", ["5.4 mEq/L", "3.5 to 5.0"]),
        ("Anion gap", ["24", "8 to 12"])], id="pump-table")]),
]

pump_qs = [
hl_text(M(PA, DKA, "recognize_cues", "moderate", ENDO, "ada"),
   "Highlight the findings that explain the client's DKA.",
   ["The client uses ",
    ("an insulin pump with rapid-acting insulin only", True, "With no long-acting insulin, any interruption causes ketosis within hours."),
    ". The infusion set was ",
    ("changed 3 days ago", True, "Sites older than 2 to 3 days absorb poorly and fail."),
    ". The glucose ",
    ("kept rising despite two pump boluses", True, "Insulin was not reaching the client."),
    ", and ",
    ("the tubing has a kink", True, "Blocked delivery."),
    ". The client ",
    ("is 26 years old", False, "Not a cause."),
    "."],
   "Pump users have no insulin depot, so a blocked or old site leads quickly to DKA.",
   "Recognize cues of insulin pump failure."),

matrix_mc(M(PA, DKA, "analyze_cues", "difficult", ENDO, "ada lab"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("The potassium of 5.4 means total body potassium is high", 1, "Acidosis and insulin lack shift potassium out of cells; body stores are low."),
    ("Potassium will fall once insulin starts", 0, "Insulin moves potassium into cells."),
    ("A pump correction bolus that does not lower glucose suggests a delivery problem", 0, "The rule is to inject by pen or syringe and change the set."),
    ("An anion gap of 24 shows ketoacidosis", 0, "High gap from ketones."),
    ("DKA in pump users develops more slowly than in injection users", 1, "Faster, because there is no long-acting insulin.")],
   "Serum potassium is misleading in DKA; pump failure causes rapid ketosis.",
   "Analyze DKA in a pump user."),

cloze(M(PA, DKA, "prioritize_hypotheses", "difficult", ENDO, "ada"),
   "Complete the sentence by choosing from the lists of options.",
   "The priorities are {{b1}} and {{b2}}.",
   [[("restoring circulating volume with IV fluids", True, "Vomiting and osmotic diuresis cause dehydration."),
     ("restarting the pump with the same set", False, "The set has failed."),
     ("a sliding-scale injection only", False, "DKA needs an infusion."),
     ("oral fluids only", False, "The client is vomiting.")],
    [("IV insulin to stop ketone production, with potassium monitoring", True, "Insulin plus close potassium checks."),
     ("bicarbonate infusion", False, "Not needed at pH 7.21."),
     ("holding insulin until glucose is normal", False, "Insulin is the treatment."),
     ("glucagon", False, "Wrong direction.")]],
   "Fluids, then IV insulin with potassium monitoring.",
   "Prioritize DKA treatment."),

sata(M(SIC, HOME, "generate_solutions", "difficult", ENDO, "ada fund"),
   "The client asks to keep the pump on in hospital. Which safety measures should the nurse plan?",
   [("Disconnect the pump while the insulin infusion is running", True, "Avoids double insulin."),
    ("Follow the hospital's pump policy, including a self-management agreement once stable", True, "Clarifies responsibilities."),
    ("Remove the pump before any MRI or x-ray", True, "Magnets and radiation can damage it."),
    ("Record pump settings in the medical record", True, "Lets staff resume correct basal rates."),
    ("Let the client resume the pump with the same infusion set when eating", False, "A new set and site are needed."),
    ("Leave the pump on during the MRI if the client is careful", False, "Unsafe for the device.")],
   "Hospital pump safety covers avoiding double insulin, a policy-based agreement, device protection and documented "
   "settings.",
   "Generate safety measures for an insulin pump in hospital.",
   ex=[note("1000", "Insulin infusion running. The client asks, \"Can I keep my pump on so I don't lose my settings?\"",
       id="pump-1000")]),

sata(M(PPT, INS, "take_action", "difficult", ENDO, "ada"),
   "The anion gap has closed and the client is eating. Which actions are correct for restarting the pump?",
   [("Insert a new infusion set at a new site", True, "Prevents repeat failure."),
    ("Restart the pump basal rate and give a meal bolus", True, "Restores insulin delivery."),
    ("Stop the IV insulin 30 to 60 minutes after the pump restarts", True, "Overlap prevents a gap with no insulin."),
    ("Stop the IV insulin first, then restart the pump when convenient", False, "A gap causes rapid ketosis."),
    ("Use the old site because it is already healed in", False, "The old site failed."),
    ("Check glucose only once at bedtime", False, "Check every 1 to 2 hours during the transition.")],
   "New set and site, restart the pump and overlap with IV insulin, then check glucose closely.",
   "Take action to restart an insulin pump after DKA.",
   ex=[flow("Hour 12", ["Hour 12"], [
       ("Anion gap", ["10"]),
       ("Glucose", ["168 mg/dL"]),
       ("Potassium", ["4.1 mEq/L"]),
       ("Intake", ["Eating lunch"])], id="pump-h12")]),

matrix_mc(M(HPM, DSM, "evaluate_outcomes", "difficult", ENDO, "ada"),
   "For each statement about pump safety at home, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"If a correction bolus doesn't work in 2 hours, I'll inject with a pen and change the set.\"", 0, "The key pump rule."),
    ("\"I'll check ketones if my glucose is over 250 and I feel sick.\"", 0, "Early detection of DKA."),
    ("\"I'll keep long-acting insulin and pens at home in case the pump fails.\"", 0, "A backup plan."),
    ("\"I'll change my infusion set every 5 days to save supplies.\"", 1, "Usually every 2 to 3 days."),
    ("\"If I'm vomiting, I'll stop my basal rate because I'm not eating.\"", 1, "Basal insulin is always needed; call for sick-day guidance."),
    ("\"I'll wear a medical alert ID.\"", 0, "Helps in an emergency.")],
   "Pump safety rules cover failed corrections, ketone checks, backup insulin, regular set changes and never "
   "stopping basal insulin.",
   "Evaluate insulin pump sick-day teaching.",
   ex=[note("Discharge", "Pump restarted. Glucose 120 to 190 mg/dL overnight.", id="pump-dc")]),
]

cases = [
    case("Pheochromocytoma", pheo_tabs, pheo_qs),
    case("Osteoporosis with a vertebral compression fracture", osteo_tabs, osteo_qs),
    case("Diabetic foot ulcer with osteomyelitis", foot_tabs, foot_qs),
    case("Insulin pump failure leading to DKA", pump_tabs, pump_qs),
]

BUILT = build(111, "endocrine-cases-pheo-bone-foot-pump-standalone",
              "Endocrine case studies: pheochromocytoma, osteoporosis, diabetic foot osteomyelitis and insulin pump "
              "DKA, with four standalone items",
              items, cases, case_conversion=True)
