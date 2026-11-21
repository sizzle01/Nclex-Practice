"""Batch 120 (case conversion, chunk 6: renal, hematologic, oncologic): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: hyperkalemia and fluid overload after missed hemodialysis; peritonitis on peritoneal dialysis; acute kidney
injury from an NSAID, an ACE inhibitor and a diuretic; an infected obstructing kidney stone. Each case walks the six
CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health
promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
RENAL = "renal-urinary"
KIDNEY = "kidney-injury-and-disease"
DIALYSIS = "dialysis-care"
LYTES = "electrolyte-imbalances"
POTASSIUM = "potassium-medication-safety"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
ABX = "antimicrobial-therapy"
ASEPSIS = "surgical-asepsis"
ADVERSE = "adverse-effects-of-medications"
UTI = "urinary-tract-and-prostate-disorders"
SHOCK = "shock-and-sepsis"
NUTR = "nutrition-and-therapeutic-diets"
TEACH = "health-teaching-and-behavior-change"
CASE_MGMT = "case-management-and-resources"
PAIN = "pain-assessment-and-comfort"
MEDREC = "medication-safety-systems"

items = []

# ================================================================================================
# Case 1: Missed hemodialysis with hyperkalemia
# ================================================================================================
hd_tabs = [
    ("hd-notes", "Nurses' notes", [note("Emergency department, 0700",
        "A 58-year-old client with end-stage kidney disease on hemodialysis Monday, Wednesday and Friday missed "
        "Friday and Monday sessions because of transport problems. The client reports weakness, numb hands and "
        "shortness of breath lying down. Crackles to mid-lung fields, 3+ ankle edema, jugular veins distended. Left "
        "forearm arteriovenous fistula. Weight 84 kg (dry weight 79 kg).", id="hd-note")]),
    ("hd-data", "Labs and ECG", [table("Labs and ECG", ["0700", "Reference"], [
        ("Potassium", ["7.4 mEq/L", "3.5 to 5.0"]),
        ("Bicarbonate", ["15 mEq/L", "22 to 26"]),
        ("BUN / creatinine", ["112 / 11.8 mg/dL", "-"]),
        ("Glucose", ["108 mg/dL", "-"]),
        ("ECG", ["Peaked T waves, wide QRS 0.14 s, flattened P waves", "-"]),
        ("SpO2", ["89% on room air", "-"])], id="hd-table")]),
]

hd_qs = [
hl_table(M(PA, LYTES, "recognize_cues", "moderate", RENAL, "ms lab"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Potassium", "7.4 mEq/L"], True, "Severe hyperkalemia."),
    (["ECG", "Peaked T, wide QRS, flat P"], True, "Cardiac toxicity: arrest can follow."),
    (["SpO2 and crackles", "89%; crackles to mid-lung"], True, "Pulmonary edema from fluid overload."),
    (["Weight", "5 kg above dry weight"], True, "About 5 L of extra fluid."),
    (["Fistula", "Left forearm"], False, "Access for dialysis; protect it."),
    (["Glucose", "108 mg/dL"], False, "Normal; a baseline before insulin.")],
   "Hyperkalemia with ECG changes and pulmonary edema after missed dialysis is an emergency.",
   "Recognize cues after missed hemodialysis."),

order(M(PPT, POTASSIUM, "analyze_cues", "difficult", RENAL, "ph ms"),
   "Place the hyperkalemia treatments in order of how quickly they act on the heart and the serum potassium, from "
   "fastest to slowest.",
   [("IV calcium gluconate: stabilizes the heart muscle within minutes", "Protects the heart but does not lower potassium."),
    ("IV regular insulin with dextrose: shifts potassium into cells within 15 to 30 minutes", "Temporary shift."),
    ("Nebulized albuterol: also shifts potassium into cells", "Adds to insulin."),
    ("Sodium zirconium cyclosilicate: binds potassium in the gut over hours", "Removes potassium slowly."),
    ("Hemodialysis: removes potassium from the body", "Definitive in kidney failure.")],
   "Calcium protects the heart first, insulin and albuterol shift potassium, binders and dialysis remove it.",
   "Analyze how hyperkalemia treatments work."),

cloze(M(PA, LYTES, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most immediate threat is {{b1}}; the next is {{b2}}; both are fixed definitively by {{b3}}.",
   [[("a fatal dysrhythmia from hyperkalemia", True, "Wide QRS can progress to a sine wave and arrest."),
     ("uremic encephalopathy", False, "Possible, but not the most immediate."),
     ("anemia", False, "Chronic."),
     ("hypertension", False, "Not the main threat.")],
    [("hypoxemia from pulmonary edema", True, "SpO2 89%."),
     ("hypoglycemia", False, "Glucose is normal."),
     ("infection of the fistula", False, "Not present."),
     ("hypokalemia", False, "Potassium is high.")],
    [("urgent hemodialysis", True, "Removes potassium and fluid."),
     ("IV furosemide alone", False, "Little effect without kidney function."),
     ("oral fluids", False, "Wrong direction."),
     ("a potassium-rich diet", False, "Wrong direction.")]],
   "Hyperkalemia threatens the heart first; urgent dialysis removes potassium and fluid.",
   "Prioritize threats after missed dialysis."),

calc(M(PPT, CALC, "generate_solutions", "difficult", RENAL, "ph"),
   "The protocol orders regular insulin 0.1 units/kg (maximum 10 units) IV with 25 g of dextrose. The client weighs "
   "84 kg. How many units of insulin should the nurse prepare? Round to the nearest whole unit.",
   "units", 0, "Insulin", 8,
   "0.1 x 84 = 8.4 units, rounded to 8 units per the protocol; below the 10-unit maximum. Many protocols now use 5 "
   "units in kidney failure to reduce hypoglycemia, and glucose is checked hourly for 4 to 6 hours.",
   "Calculate insulin for hyperkalemia.",
   ex=[note("Hyperkalemia protocol", "Calcium gluconate 10% 30 mL IV over 10 minutes. Regular insulin 0.1 units/kg "
       "IV (round to the nearest whole unit, maximum 10) with dextrose 50% 50 mL. Albuterol 10 mg nebulized. Emergency "
       "hemodialysis. Weight 84 kg.", id="hd-protocol")]),

sata(M(PPT, POTASSIUM, "take_action", "difficult", RENAL, "ph ms"),
   "Which actions should the nurse take while dialysis is arranged?",
   [("Give calcium gluconate IV on a cardiac monitor", True, "Stabilizes the heart."),
    ("Check glucose every hour after the insulin", True, "Hypoglycemia is common, especially in kidney failure."),
    ("Raise the head of the bed and give oxygen", True, "Pulmonary edema."),
    ("Avoid blood pressure checks and IV lines in the fistula arm", True, "Protects the access."),
    ("Give potassium-free IV fluid rapidly", False, "Worsens fluid overload."),
    ("Give the client a banana for the hypoglycemia risk", False, "High in potassium.")],
   "Stabilize the heart, watch glucose after insulin, treat the edema and protect the fistula.",
   "Take action for hyperkalemia and fluid overload.",
   ex=[flow("0730", ["0730"], [
       ("Treatments given", ["Calcium gluconate, insulin with dextrose, albuterol"]),
       ("Glucose", ["Check due"]),
       ("SpO2", ["91% on 4 L/min"])], id="hd-0730")]),

matrix_mc(M(MOC, CASE_MGMT, "evaluate_outcomes", "difficult", RENAL, "mgmt ms"),
   "After dialysis, review the findings. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Potassium 4.6 mEq/L; ECG normal", 0, "Corrected."),
    ("Weight 79.5 kg; lungs clear", 0, "Fluid removed."),
    ("Glucose 58 mg/dL 3 hours after insulin", 1, "Hypoglycemia; treat and keep checking."),
    ("No reliable transport to dialysis", 1, "A case manager or social worker must arrange transport to prevent a repeat."),
    ("Thrill and bruit present over the fistula", 0, "The access is working.")],
   "Potassium and fluid are corrected; hypoglycemia and the transport barrier need action.",
   "Evaluate outcomes after emergency dialysis.",
   ex=[note("After dialysis", "4 hours of dialysis completed.", id="hd-after")]),
]

# ================================================================================================
# Case 2: Peritonitis on peritoneal dialysis
# ================================================================================================
pd_tabs = [
    ("pdx-notes", "Nurses' notes", [note("Home dialysis clinic",
        "A 49-year-old client does continuous ambulatory peritoneal dialysis (four exchanges a day). Since yesterday "
        "the client has had abdominal pain and the drained fluid looks cloudy. The client admits doing exchanges in "
        "the kitchen with the window open and the dog nearby, and sometimes skipping the mask. The exit site is red "
        "with crusting.", id="pdx-note")]),
    ("pdx-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["38.3 C (100.9 F)"]),
        ("Effluent white cells", ["640/mm3, 70% neutrophils (peritonitis: above 100, more than 50% neutrophils)"]),
        ("Effluent Gram stain", ["Gram-positive cocci"]),
        ("Drain volume", ["Less than the volume instilled for 2 days"]),
        ("Blood pressure", ["142/88 mm Hg"])], id="pdx-table")]),
]

pd_qs = [
hl_text(M(PA, DIALYSIS, "recognize_cues", "moderate", RENAL, "ms"),
   "Highlight the findings that require follow-up.",
   ["The client has ",
    ("abdominal pain", True, "Peritonitis."),
    " and ",
    ("cloudy drained fluid", True, "The earliest sign of peritonitis."),
    ". Exchanges are done ",
    ("in the kitchen with the window open and the dog nearby", True, "Breaks in aseptic technique."),
    ". The client ",
    ("sometimes skips the mask", True, "Contamination risk."),
    ". The client uses ",
    ("four exchanges a day", False, "The usual schedule."),
    ". The exit site is ",
    ("red with crusting", True, "Exit-site infection."),
    "."],
   "Cloudy effluent with pain after breaks in technique is peritonitis.",
   "Recognize cues of PD peritonitis."),

matrix_mc(M(PA, DIALYSIS, "analyze_cues", "difficult", RENAL, "ms"),
   "For each finding, indicate what it shows.",
   ["Peritonitis", "Fluid retention", "Expected on PD"],
   [("Effluent white cells 640 with 70% neutrophils", 0, "Diagnostic."),
    ("Gram-positive cocci", 0, "Often skin organisms from touch contamination."),
    ("Draining less than instilled for 2 days", 1, "Inflamed membrane absorbs more; fluid builds up."),
    ("Raised blood pressure", 1, "Fluid overload."),
    ("Some glucose absorption from the dialysate", 2, "Expected; contributes to weight gain.")],
   "Peritonitis inflames the membrane, so ultrafiltration falls and fluid is retained.",
   "Analyze PD findings."),

cloze(M(PA, DIALYSIS, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client most likely has {{b1}} from {{b2}}, and without prompt treatment the risk is {{b3}}.",
   [[("bacterial peritonitis", True, "Cloudy fluid, pain, high neutrophils."),
     ("a bowel perforation", False, "Would show many gut organisms."),
     ("pancreatitis", False, "Not supported."),
     ("normal catheter irritation", False, "Not with these findings.")],
    [("touch contamination during exchanges", True, "Skin bacteria entering the system."),
     ("eating too much potassium", False, "Not a cause."),
     ("missed exchanges", False, "Not the cause."),
     ("the antihypertensive", False, "Not the cause.")],
    [("loss of the peritoneal membrane and catheter, ending PD", True, "Severe or repeated infection scars the membrane."),
     ("kidney stones", False, "Not related."),
     ("hyperthyroidism", False, "Not related."),
     ("anemia correction", False, "Not related.")]],
   "Touch contamination caused peritonitis, which threatens the ability to continue PD.",
   "Prioritize PD peritonitis."),

sata(M(PPT, ABX, "generate_solutions", "difficult", RENAL, "ph ms"),
   "Which orders should the nurse anticipate?",
   [("Intraperitoneal antibiotics", True, "Standard empiric treatment."),
    ("Heparin in the dialysate for fibrin", True, "Prevents catheter blockage."),
    ("Repeat effluent cell count", True, "Tracks response."),
    ("Exit-site care", True, "Treats the exit-site infection."),
    ("Stop all dialysis for one week to rest the membrane", False, "Dialysis continues."),
    ("Remove the peritoneal catheter today", False, "Usually only for failed treatment or certain organisms.")],
   "Intraperitoneal antibiotics, heparin for fibrin, follow-up counts and exit-site care.",
   "Generate solutions for PD peritonitis.",
   ex=[note("Orders", "Vancomycin and ceftazidime intraperitoneally. Heparin 500 units/L in bags. Retrain technique.",
       id="pdx-orders")]),

order(M(SIC, ASEPSIS, "take_action", "difficult", RENAL, "fund ms"),
   "The nurse retrains the client on a manual exchange. Place the steps in order.",
   [("Close windows, remove pets and clean the work surface", "Prepares a clean environment."),
    ("Put on a mask and perform hand hygiene", "Prevents contamination."),
    ("Check the bag: concentration, expiry, clarity and leaks", "Prevents errors."),
    ("Connect the new bag using aseptic technique without touching the connectors", "Avoids touch contamination."),
    ("Drain the used fluid, then fill with the new solution", "Drain before fill."),
    ("Inspect the drained fluid and record the volume", "Detects cloudiness and fluid balance.")],
   "Clean environment, mask and hands, check the bag, aseptic connection, drain then fill, inspect.",
   "Take action to teach a safe PD exchange.",
   ex=[note("Day 2", "The client is admitted for 2 days for treatment and retraining.", id="pdx-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RENAL, "ms"),
   "At the 2-week visit, for each finding, indicate whether the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("Drained fluid clear; effluent white cells 40/mm3", 0, "Peritonitis resolved."),
    ("Exit site clean and dry", 0, "Healed."),
    ("Client demonstrates an exchange correctly with a mask", 0, "Technique corrected."),
    ("Weight 2 kg above target with ankle edema", 1, "Fluid retention persists; review the regimen."),
    ("\"I'll call right away if the fluid is ever cloudy.\"", 0, "Early reporting.")],
   "Infection and technique are fixed; fluid balance needs review.",
   "Evaluate PD peritonitis treatment.",
   ex=[note("2-week visit", "Antibiotics completed.", id="pdx-2wk")]),
]

# ================================================================================================
# Case 3: Acute kidney injury from NSAID + ACE inhibitor + diuretic
# ================================================================================================
aki_tabs = [
    ("aki-notes", "Nurses' notes", [note("Medical unit",
        "A 76-year-old client with hypertension takes lisinopril and hydrochlorothiazide. For a week the client took "
        "ibuprofen 600 mg three times daily for knee pain and had diarrhea for 3 days. The client is dizzy and has "
        "passed little urine.", id="aki-note")]),
    ("aki-labs", "Labs and output", [table("Labs and output", ["Day 1", "Reference"], [
        ("Creatinine", ["3.2 mg/dL (1.0 last month)", "0.6 to 1.2"]),
        ("BUN", ["64 mg/dL", "10 to 20"]),
        ("Potassium", ["5.9 mEq/L", "3.5 to 5.0"]),
        ("Urine output", ["180 mL in 12 hours", "-"]),
        ("Urine sodium", ["12 mEq/L", "below 20 suggests prerenal"]),
        ("Blood pressure", ["94/56 mm Hg lying", "-"])], id="aki-table")]),
]

aki_qs = [
hl_table(M(PA, KIDNEY, "recognize_cues", "moderate", RENAL, "ms lab"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Creatinine", "3.2 mg/dL from 1.0"], True, "Stage 3 acute kidney injury."),
    (["Urine output", "180 mL in 12 hours"], True, "Oliguria."),
    (["Potassium", "5.9 mEq/L"], True, "Hyperkalemia."),
    (["Blood pressure", "94/56 mm Hg"], True, "Low perfusion pressure."),
    (["Urine sodium", "12 mEq/L"], False, "Supports a prerenal cause; diagnostic rather than an action point.")],
   "Rising creatinine, oliguria, hyperkalemia and low pressure.",
   "Recognize cues of acute kidney injury."),

matrix_mr(M(PPT, ADVERSE, "analyze_cues", "difficult", RENAL, "ph"),
   "For each factor, indicate how it reduced kidney perfusion. Each row may have more than one answer.",
   ["Reduces blood volume", "Constricts the afferent arteriole", "Dilates the efferent arteriole"],
   [("Hydrochlorothiazide", {0}, "Diuresis."),
    ("Diarrhea", {0}, "Fluid loss."),
    ("Ibuprofen", {1}, "Blocks vasodilating prostaglandins."),
    ("Lisinopril", {2}, "Blocks angiotensin II, which normally keeps filtration pressure up.")],
   "Volume loss, NSAIDs and ACE inhibitors together (the triple whammy) collapse filtration pressure.",
   "Analyze the causes of prerenal AKI."),

cloze(M(PA, KIDNEY, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "This is {{b1}} acute kidney injury; if perfusion is not restored, it may progress to {{b2}}.",
   [[("prerenal", True, "Low urine sodium and a clear cause of low perfusion."),
     ("postrenal", False, "No obstruction is described."),
     ("intrinsic from glomerulonephritis", False, "Not supported."),
     ("chronic", False, "Creatinine was normal last month.")],
    [("acute tubular necrosis", True, "Prolonged ischemia damages the tubules."),
     ("kidney stones", False, "Not the progression."),
     ("nephrotic syndrome", False, "Not related."),
     ("polycystic kidney disease", False, "Genetic.")]],
   "Prerenal AKI can become intrinsic ATN if perfusion stays low.",
   "Prioritize AKI."),

matrix_mc(M(PPT, ADVERSE, "generate_solutions", "difficult", RENAL, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Hold lisinopril, hydrochlorothiazide and ibuprofen", 0, "Remove the causes."),
    ("IV isotonic fluid bolus, then reassess", 0, "Restores perfusion."),
    ("Strict intake and output with daily weights", 0, "Tracks fluid and kidney response."),
    ("Ketorolac for the knee pain", 1, "Another NSAID."),
    ("Potassium chloride in the IV fluid", 1, "Potassium is high."),
    ("Acetaminophen for knee pain within daily limits", 0, "Safer analgesic.")],
   "Stop the culprits, restore volume, monitor closely and use safer analgesia.",
   "Generate solutions for prerenal AKI.",
   ex=[note("Orders", "0.9% sodium chloride 1 L, then 100 mL/hour. Hold home antihypertensives and NSAIDs.", id="aki-orders")]),

sata(M(PA, KIDNEY, "take_action", "difficult", RENAL, "ms"),
   "On day 4, urine output rises sharply. Review the findings. Which actions should the nurse take?",
   [("Replace fluid losses as prescribed and monitor weight", True, "The diuretic phase can cause dehydration."),
    ("Check sodium and potassium often", True, "Electrolytes can fall quickly."),
    ("Watch for orthostatic hypotension and falls", True, "Volume loss."),
    ("Restart lisinopril now that urine output is high", False, "Wait until kidney function recovers."),
    ("Restrict fluids to 1 L a day", False, "Dehydration risk.")],
   "The recovery (diuretic) phase of AKI needs fluid and electrolyte replacement.",
   "Take action in the diuretic phase of AKI.",
   ex=[flow("Recovery", ["Day 2", "Day 4"], [
       ("Urine output", ["400 mL/24 h", "4,200 mL/24 h"]),
       ("Creatinine", ["3.6", "2.4 mg/dL"]),
       ("Potassium", ["5.6", "3.3 mEq/L"]),
       ("Sodium", ["138", "132 mEq/L"])], id="aki-day4")]),

matrix_mc(M(SIC, MEDREC, "evaluate_outcomes", "difficult", RENAL, "ph mgmt"),
   "Before discharge, for each item, indicate whether it is safe or needs correction.",
   ["Safe", "Needs correction"],
   [("Creatinine 1.3 mg/dL", 0, "Near baseline."),
    ("Discharge list still includes ibuprofen as needed", 1, "Must be removed."),
    ("Client told to stop lisinopril and diuretic if vomiting or diarrhea (sick-day rules)", 0, "Prevents a repeat."),
    ("Lisinopril restarted at a lower dose with a creatinine check in 1 week", 0, "Careful restart."),
    ("No one told the client's pharmacy about the NSAID problem", 1, "The pharmacy should flag NSAIDs.")],
   "Kidney function recovered; the medication list and pharmacy need correcting.",
   "Evaluate medication safety after AKI.",
   ex=[note("Day 6", "Planned discharge tomorrow.", id="aki-day6")]),
]

# ================================================================================================
# Case 4: Infected obstructing kidney stone
# ================================================================================================
stone_tabs = [
    ("stone-notes", "Nurses' notes", [note("Emergency department",
        "A 45-year-old client has severe, colicky right flank pain spreading to the groin, nausea, and blood in the "
        "urine. Since this morning the client has had shaking chills. CT: a 7 mm stone in the right ureter with "
        "hydronephrosis.", id="stone-note")]),
    ("stone-data", "Vital signs and labs", [table("Vital signs and labs", ["Admission", "Reference"], [
        ("Temperature", ["39.2 C (102.6 F)", "-"]),
        ("Blood pressure", ["92/54 mm Hg", "-"]),
        ("Heart rate", ["118/min", "-"]),
        ("WBC", ["19,800/mm3", "-"]),
        ("Lactate", ["3.1 mmol/L", "-"]),
        ("Creatinine", ["1.6 mg/dL", "0.6 to 1.2"]),
        ("Urinalysis", ["Nitrites, many white cells and bacteria", "-"])], id="stone-table")]),
]

stone_qs = [
hl_table(M(PA, UTI, "recognize_cues", "moderate", RENAL, "ms"),
   "Highlight the findings that make this a urologic emergency.",
   ["Finding", "Value"],
   [(["CT", "Stone with hydronephrosis"], True, "Obstruction."),
    (["Fever and chills", "39.2 C"], True, "Infection behind the stone."),
    (["Blood pressure / heart rate", "92/54; 118/min"], True, "Septic shock developing."),
    (["Lactate", "3.1 mmol/L"], True, "Hypoperfusion."),
    (["Pain", "Colicky, to the groin"], False, "Typical of a stone; needs control but is not what makes it an emergency."),
    (["Hematuria", "Present"], False, "Expected with a stone.")],
   "An obstructed, infected kidney with sepsis needs urgent drainage.",
   "Recognize cues of an obstructed infected kidney."),

matrix_mc(M(PA, UTI, "analyze_cues", "difficult", RENAL, "ms"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Antibiotics alone will not clear infection behind an obstruction", 0, "Pus must be drained."),
    ("A 7 mm stone may pass on its own once infection is treated", 1, "Stones over 5 to 6 mm often need intervention."),
    ("Raised creatinine can be from obstruction (postrenal) plus sepsis", 0, "Both contribute."),
    ("Straining all urine helps identify the stone type", 0, "Guides prevention."),
    ("The client should restrict fluids to reduce pain", 1, "Fluids are not restricted; the focus is drainage.")],
   "Infected obstruction needs decompression as well as antibiotics.",
   "Analyze an obstructed infected kidney."),

cloze(M(PA, SHOCK, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client has {{b1}}, and the priority is {{b2}}.",
   [[("urosepsis from an obstructed kidney", True, "Infection plus obstruction plus shock."),
     ("simple renal colic", False, "Fever and shock say otherwise."),
     ("pyelonephritis without obstruction", False, "CT shows obstruction."),
     ("appendicitis", False, "CT shows a stone.")],
    [("sepsis care plus urgent decompression with a stent or nephrostomy tube", True, "Source control."),
     ("tamsulosin and discharge", False, "Unsafe with infection."),
     ("shock wave lithotripsy today", False, "Not with active infection."),
     ("pain control only", False, "Not enough.")]],
   "Urosepsis from obstruction needs sepsis care and urgent decompression.",
   "Prioritize urosepsis with obstruction."),

sata(M(PA, SHOCK, "generate_solutions", "difficult", RENAL, "ms ph"),
   "Which orders should the nurse anticipate in the first hour?",
   [("Blood and urine cultures before antibiotics", True, "Identify the organism."),
    ("IV broad-spectrum antibiotics", True, "Within the hour."),
    ("30 mL/kg crystalloid for hypotension and lactate", True, "Sepsis bundle."),
    ("Urology or interventional radiology for urgent drainage", True, "Source control."),
    ("Hold IV fluids to avoid flushing the stone", False, "Fluids are needed."),
    ("Oral tamsulosin as the main treatment", False, "Not for infected obstruction.")],
   "The sepsis bundle plus urgent drainage.",
   "Generate solutions for urosepsis.",
   ex=[note("1 hour", "Weight 80 kg.", id="stone-1h")]),

sata(M(RRP, "post-procedure-complications", "take_action", "difficult", RENAL, "ms"),
   "A right nephrostomy tube was placed. Six hours later, review the findings. Which actions should the nurse take?",
   [("Check the tube and drainage bag for kinks or clots", True, "No drainage suggests blockage."),
    ("Report the stopped drainage and new flank pain", True, "Blocked tube or dislodgement."),
    ("Never clamp the nephrostomy tube", True, "Clamping causes back pressure."),
    ("Flush the tube with a large volume of saline on your own", False, "Only gentle, small flushes if prescribed."),
    ("Pull the tube back a little to free it", False, "Could dislodge it.")],
   "A nephrostomy that stops draining is checked for kinks and reported.",
   "Take action for a nephrostomy problem.",
   ex=[flow("Nephrostomy", ["Hour 2", "Hour 6"], [
       ("Drainage", ["90 mL/hour, bloody then clearing", "0 mL in 2 hours"]),
       ("Flank pain", ["2 of 10", "7 of 10"]),
       ("Dressing", ["Dry", "Wet with urine"])], id="stone-6h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RENAL, "ms"),
   "The stone was calcium oxalate. Before discharge, for each statement, indicate whether it shows understanding or "
   "needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll drink enough to pass 2.5 liters of urine a day.\"", 0, "Dilutes stone-forming minerals."),
    ("\"I'll cut out all calcium foods.\"", 1, "Low calcium raises oxalate absorption; normal dietary calcium is advised."),
    ("\"I'll eat less salt and fewer high-oxalate foods like spinach.\"", 0, "Reduces risk."),
    ("\"I'll strain my urine and bring any stone in.\"", 0, "Analysis guides prevention."),
    ("\"I'll take vitamin C 2 grams daily for my immune system.\"", 1, "High-dose vitamin C raises oxalate.")],
   "Fluids, normal calcium, less salt and oxalate, and avoiding high-dose vitamin C prevent stones.",
   "Evaluate kidney stone prevention teaching.",
   ex=[note("Discharge", "Stone removed by ureteroscopy after the infection cleared.", id="stone-dc")]),
]

cases = [
    case("Missed hemodialysis with hyperkalemia and fluid overload", hd_tabs, hd_qs),
    case("Peritonitis on peritoneal dialysis", pd_tabs, pd_qs),
    case("Acute kidney injury from an NSAID, ACE inhibitor and diuretic", aki_tabs, aki_qs),
    case("Infected obstructing kidney stone with urosepsis", stone_tabs, stone_qs),
]

BUILT = build(120, "renal-cases-hd-pd-aki-stone",
              "Renal case studies: missed hemodialysis, PD peritonitis, triple-whammy AKI and an infected kidney stone",
              items, cases, case_conversion=True)
