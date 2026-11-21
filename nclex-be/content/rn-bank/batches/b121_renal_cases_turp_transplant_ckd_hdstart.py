"""Batch 121 (case conversion, chunk 6: renal, hematologic, oncologic): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: TURP with continuous bladder irrigation, clot retention and TUR syndrome; kidney transplant with acute rejection
and tacrolimus toxicity; anemia and mineral bone disease of CKD treated with epoetin, iron and binders (hematopoietic
medications); starting hemodialysis with disequilibrium syndrome and fistula care. Each case walks the six CJMM steps
in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
RENAL = "renal-urinary"
UTI = "urinary-tract-and-prostate-disorders"
POSTOP = "postoperative-care"
CATHETER = "urinary-elimination-and-catheter-care"
LYTES = "electrolyte-imbalances"
KIDNEY = "kidney-injury-and-disease"
DIALYSIS = "dialysis-care"
IMMUNO = "immunosuppressant-therapy"
HEME_MEDS = "hematologic-medications"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
TEACH = "health-teaching-and-behavior-change"
HAI = "healthcare-associated-infection-prevention"
DELEG = "delegation-and-supervision"
ID_ERR = "client-identification-and-error-prevention"
NUTR = "nutrition-and-therapeutic-diets"

items = []

# ================================================================================================
# Case 1: TURP, continuous bladder irrigation and TUR syndrome
# ================================================================================================
turp_tabs = [
    ("turp-notes", "Nurses' notes", [note("Urology unit, 2 hours after surgery",
        "A 72-year-old client had a transurethral resection of the prostate (TURP) this afternoon. A three-way "
        "catheter with continuous bladder irrigation (CBI) is in place, with 0.9% sodium chloride irrigation.",
        id="turp-note")]),
    ("turp-flow", "Irrigation flowsheet", [flow("Irrigation and output", ["1600", "1700", "1800"], [
        ("Irrigation infused (mL)", ["1,000", "1,000", "1,000"]),
        ("Total drained (mL)", ["1,250", "1,100", "650"]),
        ("Drainage color", ["Light pink", "Pink with small clots", "Dark red with clots"]),
        ("Client report", ["Comfortable", "Mild bladder spasms", "Severe lower abdominal pain, urge to void"]),
        ("Bladder", ["Not palpable", "Not palpable", "Firm and distended above the pubis"])], id="turp-flowsheet")]),
]

turp_qs = [
hl_table(M(RRP, POSTOP, "recognize_cues", "moderate", RENAL, "ms"),
   "Highlight the findings at 1800 that require immediate action.",
   ["Finding", "1800"],
   [(["Drained", "650 mL with 1,000 mL infused"], True, "Output less than input means fluid is retained."),
    (["Color", "Dark red with clots"], True, "Bleeding with clots."),
    (["Bladder", "Firm and distended"], True, "Clot retention."),
    (["Client", "Severe pain, urge to void"], True, "Bladder distension, which also worsens bleeding."),
    (["Irrigant", "0.9% sodium chloride"], False, "Correct isotonic irrigant.")],
   "Clots blocking the catheter cause retention, pain and more bleeding.",
   "Recognize cues of clot retention after TURP."),

calc(M(BCC, CATHETER, "analyze_cues", "difficult", RENAL, "ms"),
   "True urine output = total drained minus irrigation infused. Using the 1600 and 1700 rows, what was the client's "
   "true urine output over those 2 hours? Record your answer in mL.",
   "mL", 0, "True urine output", 350,
   "(1,250 + 1,100) - (1,000 + 1,000) = 2,350 - 2,000 = 350 mL over 2 hours. At 1800, drainage (650) is less than "
   "the irrigation (1,000), so 350 mL of irrigant is retained in the bladder.",
   "Calculate true urine output during bladder irrigation."),

cloze(M(RRP, POSTOP, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority problem is {{b1}}, and the nurse's first action is to {{b2}}.",
   [[("catheter obstruction by clots", True, "Drainage has fallen with distension and pain."),
     ("bladder spasms only", False, "Spasms do not stop drainage."),
     ("an infection", False, "Too early."),
     ("TUR syndrome", False, "Not suggested yet.")],
    [("stop the irrigation and irrigate the catheter manually as prescribed", True, "Clears clots; inflow is stopped so the bladder is not overfilled."),
     ("increase the irrigation rate", False, "Overfills a blocked bladder."),
     ("give oxybutynin and wait", False, "Does not clear the block."),
     ("remove the catheter", False, "Not without an order and would not help.")]],
   "Blocked outflow is treated by stopping inflow and clearing the catheter.",
   "Prioritize clot retention."),

sata(M(BCC, CATHETER, "generate_solutions", "difficult", RENAL, "ms"),
   "Which parts of the plan should the nurse include for CBI?",
   [("Adjust the irrigation rate to keep drainage light pink", True, "Clears blood without overfilling."),
    ("Record irrigation in and drainage out to calculate true urine output", True, "Detects retention."),
    ("Keep traction on the catheter as prescribed", True, "Compresses the prostate bed to reduce bleeding."),
    ("Use only isotonic irrigation fluid", True, "Hypotonic fluid can cause hemolysis and hyponatremia."),
    ("Encourage the client to try to void around the catheter", False, "Increases spasms."),
    ("Let the irrigation bag run dry between changes", False, "Clots form when flow stops.")],
   "Titrate irrigation to color, track true output, maintain traction and use isotonic fluid.",
   "Generate the CBI plan.",
   ex=[note("1815", "The catheter was irrigated and 120 mL of clots removed; drainage resumed.", id="turp-1815")]),

sata(M(PA, LYTES, "take_action", "difficult", RENAL, "ms"),
   "At 2300, the client is confused and nauseated. Review the findings. Which actions should the nurse take?",
   [("Notify the surgeon now", True, "Hyponatremia with neurologic signs suggests TUR syndrome."),
    ("Prepare hypertonic saline or a loop diuretic as prescribed", True, "Corrects fluid and sodium."),
    ("Institute seizure precautions", True, "Severe hyponatremia can cause seizures."),
    ("Monitor neurologic status and blood pressure closely", True, "Tracks change."),
    ("Offer free water by mouth to clear the confusion and nausea", False, "Worsens hyponatremia."),
    ("Increase the irrigation rate to flush any remaining clots", False, "More absorption.")],
   "Confusion, bradycardia, hypertension and a low sodium after TURP is TUR syndrome from absorbed fluid.",
   "Take action for TUR syndrome.",
   ex=[flow("2300", ["2300"], [
       ("Mental status", ["New confusion"]),
       ("Blood pressure", ["172/96 mm Hg"]),
       ("Heart rate", ["54/min"]),
       ("Sodium", ["121 mEq/L (139 before surgery)"]),
       ("Surgery note", ["Large prostate; long resection time"])], id="turp-2300")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RENAL, "ms"),
   "Before discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"Some blood in the urine for a few weeks can be normal, but I'll call if it gets thick or has clots.\"", 0, "Correct."),
    ("\"I'll avoid straining, heavy lifting and driving for a few weeks.\"", 0, "Prevents bleeding."),
    ("\"I'll drink 2 to 3 liters of fluid daily unless told otherwise.\"", 0, "Flushes the bladder."),
    ("\"Dribbling should improve; I'll do pelvic floor exercises.\"", 0, "Correct."),
    ("\"I'll take aspirin daily for the pain.\"", 1, "Raises bleeding risk; use acetaminophen unless told otherwise."),
    ("\"I won't need any follow-up now that the prostate is removed.\"", 1, "Prostate tissue remains; follow-up and screening continue.")],
   "Teaching covers bleeding signs, activity limits, fluids, continence and avoiding aspirin.",
   "Evaluate post-TURP teaching.",
   ex=[note("Day 2", "Sodium 134 mEq/L. Catheter removed; voiding.", id="turp-day2")]),
]

# ================================================================================================
# Case 2: Kidney transplant rejection and tacrolimus toxicity
# ================================================================================================
tx_tabs = [
    ("tx-notes", "Clinic notes", [note("Transplant clinic",
        "A 42-year-old client had a deceased-donor kidney transplant 4 months ago and takes tacrolimus, "
        "mycophenolate and prednisone. The client started clarithromycin from an urgent care clinic 5 days ago and "
        "missed 2 days of mycophenolate last week because of nausea. Today the client has tenderness over the "
        "transplanted kidney, less urine and a tremor.", id="tx-note")]),
    ("tx-data", "Results", [table("Results", ["Today", "Last month"], [
        ("Creatinine", ["2.4 mg/dL", "1.2 mg/dL"]),
        ("Tacrolimus trough", ["18 ng/mL (target 6 to 10)", "8 ng/mL"]),
        ("Potassium", ["5.8 mEq/L", "4.5 mEq/L"]),
        ("Glucose", ["212 mg/dL", "128 mg/dL"]),
        ("Temperature", ["37.9 C (100.2 F)", "-"]),
        ("Blood pressure", ["164/98 mm Hg", "128/80 mm Hg"]),
        ("Weight", ["Up 2 kg in a week", "-"])], id="tx-table")]),
]

tx_qs = [
hl_table(M(PA, KIDNEY, "recognize_cues", "moderate", RENAL, "ms"),
   "Highlight the findings that suggest a threat to the transplant.",
   ["Finding", "Today"],
   [(["Creatinine", "2.4 mg/dL (doubled)"], True, "Graft dysfunction."),
    (["Graft", "Tender, less urine"], True, "Signs of rejection."),
    (["Tacrolimus trough", "18 ng/mL"], True, "Toxic level; tacrolimus itself harms the kidney."),
    (["Missed mycophenolate", "2 days"], True, "Under-immunosuppression raises rejection risk."),
    (["Weight", "Up 2 kg in a week"], True, "Fluid retention."),
    (["Temperature", "37.9 C"], True, "Low-grade fever with rejection or infection.")],
   "Rising creatinine, a tender graft and a toxic tacrolimus level mean both rejection and toxicity are possible.",
   "Recognize cues of graft dysfunction."),

matrix_mr(M(PPT, IMMUNO, "analyze_cues", "difficult", RENAL, "ph"),
   "For each finding, select the likely causes. Each row may have more than one answer.",
   ["Tacrolimus toxicity", "Acute rejection", "Prednisone effect"],
   [("Tremor", {0}, "Neurotoxicity of calcineurin inhibitors."),
    ("Raised creatinine", {0, 1}, "Both cause it; a biopsy separates them."),
    ("Hyperkalemia", {0}, "Tacrolimus reduces potassium excretion."),
    ("Hyperglycemia", {0, 2}, "Both raise glucose."),
    ("Graft tenderness and fever", {1}, "Inflammation of the graft.")],
   "Clarithromycin raised tacrolimus to toxic levels, while missed mycophenolate risked rejection.",
   "Analyze tacrolimus toxicity versus rejection."),

cloze(M(PPT, IMMUNO, "prioritize_hypotheses", "difficult", RENAL, "ph ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The tacrolimus level rose because {{b1}}, and the plan must {{b2}}.",
   [[("clarithromycin inhibits CYP3A4, which clears tacrolimus", True, "A major interaction."),
     ("the client took extra doses", False, "Not described."),
     ("prednisone raises tacrolimus", False, "Not the main issue."),
     ("the kidney stopped clearing it", False, "Tacrolimus is mainly cleared by the liver.")],
    [("stop clarithromycin, adjust tacrolimus and evaluate for rejection with a biopsy", True, "Treats both problems."),
     ("stop all immunosuppressants", False, "Would guarantee rejection."),
     ("double the mycophenolate", False, "Not the answer."),
     ("wait 2 weeks to see if the creatinine improves", False, "The graft is at risk.")]],
   "A drug interaction caused toxicity; rejection must also be ruled out.",
   "Prioritize graft dysfunction."),

grouping(M(PPT, IMMUNO, "generate_solutions", "difficult", RENAL, "ph ms"),
   "For each area, select the actions the plan should include.",
   [("Medicines", [
       ("Stop clarithromycin; choose an antibiotic without the interaction", True, "Removes the cause."),
       ("Hold or reduce tacrolimus and recheck the trough", True, "Bring the level back to target."),
       ("Stop prednisone suddenly", False, "Risks adrenal crisis and rejection.")]),
    ("Diagnosis", [
       ("Kidney transplant ultrasound and biopsy", True, "Confirms rejection."),
       ("Urine culture", True, "Excludes infection."),
       ("A contrast CT as the first test", False, "Contrast is nephrotoxic.")]),
    ("Monitoring", [
       ("Potassium and glucose", True, "Both are abnormal."),
       ("Daily weight and intake and output", True, "Fluid status.")])],
   "Fix the interaction, find out whether rejection is present and monitor closely.",
   "Generate a plan for graft dysfunction.",
   ex=[note("Plan", "Admit. Biopsy tomorrow. Hold tacrolimus today. Stop clarithromycin.", id="tx-plan")]),

sata(M(SIC, ID_ERR, "take_action", "difficult", RENAL, "ph mgmt"),
   "The biopsy shows acute cellular rejection; IV methylprednisolone pulses are started. Which actions should the "
   "nurse take to keep the client safe?",
   [("Draw tacrolimus troughs just before the morning dose", True, "Correct timing for a trough."),
    ("Give tacrolimus at the same times daily, the same way with food", True, "Keeps levels steady."),
    ("Record the clarithromycin interaction in the chart", True, "Prevents a repeat."),
    ("Teach the client to tell every prescriber about the transplant", True, "Prevents interactions."),
    ("Draw the tacrolimus level 2 hours after the morning dose", False, "That is a peak, not a trough."),
    ("Allow grapefruit juice with breakfast if the client prefers it", False, "Raises tacrolimus levels.")],
   "Correct trough timing, consistent dosing and interaction alerts keep immunosuppression safe.",
   "Take action for safe immunosuppressant use.",
   ex=[note("Day 2", "Biopsy: acute cellular rejection, Banff grade 1A.", id="tx-biopsy")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RENAL, "ph ms"),
   "At discharge, for each statement or result, indicate whether it shows understanding or improvement, or needs "
   "follow-up.",
   ["Shows understanding or improvement", "Needs follow-up"],
   [("Creatinine 1.5 mg/dL", 0, "Improving."),
    ("Tacrolimus 8 ng/mL", 0, "Back in range."),
    ("\"If I'm nauseated and can't take my pills, I'll call the transplant team that day.\"", 0, "Prevents missed doses."),
    ("\"I'll check with the transplant pharmacist before any new medicine, even over the counter.\"", 0, "Correct."),
    ("\"I can skip my sunscreen; my skin is fine.\"", 1, "Immunosuppression raises skin cancer risk."),
    ("\"I'll avoid people with colds and wash my hands often.\"", 0, "Infection prevention.")],
   "Kidney function and levels improved; sun protection teaching is needed.",
   "Evaluate transplant teaching.",
   ex=[note("Day 5", "Three methylprednisolone pulses completed.", id="tx-day5")]),
]

# ================================================================================================
# Case 3: Anemia and mineral bone disease of CKD (hematopoietic medications)
# ================================================================================================
ckd_tabs = [
    ("ckd-notes", "Clinic notes", [note("Nephrology clinic",
        "A 64-year-old client with stage 4 chronic kidney disease (eGFR 22) is tired and short of breath on stairs. "
        "The client has itchy skin and leg cramps. Medicines: lisinopril, furosemide, calcium carbonate with meals.",
        id="ckd-note")]),
    ("ckd-labs", "Labs", [table("Labs", ["Result", "Reference"], [
        ("Hemoglobin", ["8.6 g/dL", "12 to 16"]),
        ("Ferritin", ["68 ng/mL", "goal above 100 in CKD"]),
        ("Transferrin saturation", ["14%", "goal above 20% in CKD"]),
        ("Phosphorus", ["6.4 mg/dL", "2.5 to 4.5"]),
        ("Calcium", ["8.1 mg/dL", "8.5 to 10.5"]),
        ("PTH", ["420 pg/mL", "raised"]),
        ("Blood pressure", ["148/88 mm Hg", "-"])], id="ckd-table")]),
]

ckd_qs = [
hl_table(M(PA, KIDNEY, "recognize_cues", "moderate", RENAL, "ms lab"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Result"],
   [(["Hemoglobin", "8.6 g/dL"], True, "Anemia of CKD."),
    (["Iron studies", "Ferritin 68, saturation 14%"], True, "Iron deficiency must be corrected before epoetin works."),
    (["Phosphorus", "6.4 mg/dL"], True, "Hyperphosphatemia drives itching and bone disease."),
    (["Calcium and PTH", "Low calcium, high PTH"], True, "Secondary hyperparathyroidism."),
    (["Blood pressure", "148/88 mm Hg"], True, "Above target, and epoetin can raise it further.")],
   "CKD causes anemia and mineral bone disease; both need treatment.",
   "Recognize cues of CKD complications."),

matrix_mc(M(PA, KIDNEY, "analyze_cues", "difficult", RENAL, "ms"),
   "For each finding, indicate its main cause in CKD.",
   ["Low erythropoietin from the kidneys", "Poor phosphate excretion", "Low active vitamin D"],
   [("Anemia", 0, "The kidneys make erythropoietin."),
    ("High phosphorus", 1, "The kidneys excrete phosphate."),
    ("Itchy skin", 1, "Phosphate and calcium deposits."),
    ("Low calcium", 2, "Less calcium absorbed from the gut."),
    ("High PTH", 2, "Low calcium and vitamin D drive the parathyroid glands.")],
   "Failing kidneys cause anemia and mineral bone disease.",
   "Analyze CKD complications."),

cloze(M(PPT, HEME_MEDS, "prioritize_hypotheses", "difficult", RENAL, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Before starting an erythropoiesis-stimulating agent, the client needs {{b1}}, and the hemoglobin target is "
   "{{b2}}.",
   [[("iron replacement", True, "Epoetin cannot make red cells without iron stores."),
     ("a blood transfusion", False, "Not needed at this level without symptoms of instability."),
     ("vitamin K", False, "Not relevant."),
     ("a higher potassium diet", False, "Not relevant.")],
    [("about 10 to 11 g/dL, not above 11.5", True, "Higher targets raise stroke and thrombosis risk."),
     ("13 to 15 g/dL", False, "Too high; harmful."),
     ("above 16 g/dL", False, "Dangerous."),
     ("7 g/dL", False, "Too low to relieve symptoms.")]],
   "Replace iron first; aim for a hemoglobin of 10 to 11 with an ESA.",
   "Prioritize anemia treatment in CKD."),

matrix_mc(M(PPT, HEME_MEDS, "generate_solutions", "difficult", RENAL, "ph"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("IV iron sucrose", 0, "Replenishes iron; well absorbed by IV in CKD."),
    ("Epoetin alfa subcutaneously once iron is replaced", 0, "Treats erythropoietin deficiency."),
    ("Sevelamer with meals instead of calcium carbonate", 0, "A non-calcium phosphate binder when calcium-phosphate product is high."),
    ("Calcitriol as prescribed", 0, "Active vitamin D lowers PTH."),
    ("Magnesium-containing antacids for indigestion", 1, "Magnesium builds up in kidney failure."),
    ("Raise epoetin to reach a hemoglobin of 14 g/dL", 1, "Harmful target.")],
   "Iron, ESA, phosphate binder and active vitamin D, while avoiding magnesium and high hemoglobin targets.",
   "Generate the CKD anemia and bone plan.",
   ex=[note("Orders", "Iron sucrose 200 mg IV weekly x5. Then epoetin alfa. Sevelamer 800 mg three times daily with meals. "
       "Calcitriol.", id="ckd-orders")]),

sata(M(PPT, HEME_MEDS, "take_action", "difficult", RENAL, "ph"),
   "Six weeks into epoetin, review the findings. Which actions should the nurse take?",
   [("Hold the epoetin dose and notify the provider", True, "Hemoglobin rose too fast and too high."),
    ("Report the blood pressure", True, "ESAs raise blood pressure."),
    ("Ask about headache, chest pain or leg swelling", True, "Signs of thrombosis or hypertensive effects."),
    ("Give the dose as scheduled because the client feels better", False, "Rising above target is dangerous."),
    ("Shake the vial well before drawing it up", False, "Shaking can denature the protein.")],
   "A rapid rise above target with raised blood pressure means holding the ESA.",
   "Take action on ESA adverse effects.",
   ex=[flow("Epoetin follow-up", ["Week 0", "Week 6"], [
       ("Hemoglobin", ["8.6", "12.1 g/dL"]),
       ("Rise in last 2 weeks", ["-", "1.4 g/dL"]),
       ("Blood pressure", ["148/88", "172/98 mm Hg"]),
       ("Client", ["Tired", "More energy; mild headache"])], id="ckd-6wk")]),

matrix_mc(M(BCC, NUTR, "evaluate_outcomes", "difficult", RENAL, "ms"),
   "At 3 months, for each statement, indicate whether it shows understanding or needs more teaching about the CKD "
   "diet and medicines.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I take sevelamer with each meal, not between meals.\"", 0, "Binds dietary phosphate."),
    ("\"I'm limiting dark sodas, processed cheese and processed meats.\"", 0, "High in phosphate additives."),
    ("\"I use a salt substitute to cut sodium.\"", 1, "Most contain potassium."),
    ("\"I keep protein moderate as the dietitian advised.\"", 0, "Slows progression before dialysis."),
    ("\"I'll start eating more bananas and oranges for energy.\"", 1, "High in potassium."),
    ("\"I'll protect my non-dominant arm veins for a future fistula.\"", 0, "Vein preservation.")],
   "Taking binders with meals and avoiding phosphate additives and potassium-rich substitutes.",
   "Evaluate CKD diet and medication teaching.",
   ex=[note("3-month visit", "Hemoglobin 10.6 g/dL. Phosphorus 4.9 mg/dL.", id="ckd-3m")]),
]

# ================================================================================================
# Case 4: Starting hemodialysis, disequilibrium syndrome and fistula care
# ================================================================================================
dds_tabs = [
    ("dds-notes", "Dialysis unit notes", [note("Hemodialysis unit, first treatment",
        "A 59-year-old client with kidney failure (BUN 168 mg/dL) begins hemodialysis today through a new left upper "
        "arm arteriovenous fistula created 3 months ago. In the last hour of a 3-hour run, the client reports a "
        "headache and nausea and becomes restless and confused.", id="dds-note")]),
    ("dds-data", "During dialysis", [flow("Dialysis run", ["Hour 1", "Hour 2", "Hour 3"], [
        ("BUN (pre and during)", ["168", "-", "92 mg/dL"]),
        ("Blood pressure", ["158/90", "150/88", "166/96 mm Hg"]),
        ("Mental status", ["Alert", "Alert", "Restless, confused"]),
        ("Client report", ["Fine", "Mild headache", "Severe headache, nausea, blurred vision"])], id="dds-flow")]),
]

dds_qs = [
hl_table(M(PA, DIALYSIS, "recognize_cues", "moderate", RENAL, "ms"),
   "Highlight the findings in hour 3 that require immediate action.",
   ["Finding", "Hour 3"],
   [(["BUN", "Fell from 168 to 92 mg/dL"], True, "A fast fall in a first run."),
    (["Mental status", "Restless, confused"], True, "Cerebral edema."),
    (["Client report", "Severe headache, nausea, blurred vision"], True, "Disequilibrium symptoms."),
    (["Blood pressure", "166/96 mm Hg"], False, "Raised, part of the picture but not the trigger.")],
   "Neurologic symptoms with a rapid BUN fall in a first dialysis suggest disequilibrium syndrome.",
   "Recognize cues of dialysis disequilibrium syndrome."),

cloze(M(PA, DIALYSIS, "analyze_cues", "difficult", RENAL, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "Disequilibrium syndrome happens because {{b1}}, causing {{b2}}. It is most likely in {{b3}}.",
   [[("urea is removed from the blood faster than from the brain", True, "Water moves into brain cells."),
     ("potassium rises during dialysis", False, "Potassium falls."),
     ("the fistula clots", False, "Not related."),
     ("air enters the circuit", False, "That is an air embolism.")],
    [("cerebral edema", True, "Headache, confusion, seizures."),
     ("hypoglycemia", False, "Not the mechanism."),
     ("hemorrhage", False, "Not the mechanism."),
     ("dehydration", False, "Not the mechanism.")],
    [("first treatments with a very high BUN", True, "Large urea gradients."),
     ("long-established dialysis clients", False, "Less common."),
     ("peritoneal dialysis", False, "Slower and gentler."),
     ("clients with low BUN", False, "Small gradient.")]],
   "Fast urea removal in first treatments draws water into the brain.",
   "Analyze disequilibrium syndrome."),

matrix_mc(M(PA, DIALYSIS, "prioritize_hypotheses", "difficult", RENAL, "ms"),
   "For each possible complication during this run, indicate how likely it is.",
   ["Most likely", "Possible", "Unlikely"],
   [("Dialysis disequilibrium syndrome", 0, "Classic timing and symptoms."),
    ("Hypotension", 2, "Blood pressure is high."),
    ("Air embolism", 2, "Would cause sudden chest pain and breathlessness."),
    ("Seizure if it worsens", 1, "A severe form of disequilibrium.")],
   "Disequilibrium is most likely; seizure is the danger if it progresses.",
   "Prioritize dialysis complications."),

sata(M(PA, DIALYSIS, "generate_solutions", "difficult", RENAL, "ms"),
   "Which actions should the nurse anticipate?",
   [("Slow the blood flow or stop the treatment early as directed", True, "Reduces the urea gradient."),
    ("Notify the nephrologist", True, "Changes the plan."),
    ("Seizure precautions", True, "Risk of seizure."),
    ("Plan shorter, gentler treatments for the next few sessions", True, "Prevents recurrence."),
    ("Increase the blood flow to finish faster", False, "Worsens the gradient."),
    ("Give a large free water bolus", False, "Worsens cerebral edema.")],
   "Slow or stop dialysis, notify, protect from seizures and plan gentler treatments.",
   "Generate solutions for disequilibrium syndrome.",
   ex=[note("Plan", "Next sessions: 2 hours, lower blood flow, on consecutive days.", id="dds-plan")]),

sata(M(MOC, DELEG, "take_action", "difficult", RENAL, "mgmt ms"),
   "Before discharge home, which tasks related to the fistula can the RN delegate to the UAP?",
   [("Post a sign: no blood pressures or draws in the left arm", True, "Routine safety task."),
    ("Report bleeding from the needle sites to the RN at once", True, "Reporting."),
    ("Assess the fistula for a thrill and bruit at the start of each shift", False, "Assessment stays with the RN."),
    ("Teach the client how to check the fistula for a thrill every day", False, "Teaching is an RN role."),
    ("Hold pressure on a bleeding needle site and call the RN", True, "An emergency first-aid action within their training.")],
   "UAPs can post signs, report and hold pressure; assessment and teaching stay with the RN.",
   "Delegate fistula care.",
   ex=[note("After the run", "Symptoms resolved after the run was stopped at 2.5 hours.", id="dds-after")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RENAL, "ms"),
   "For each statement about fistula care, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll feel for the buzz (thrill) every day.\"", 0, "Checks patency."),
    ("\"I won't wear tight sleeves or a watch on that arm.\"", 0, "Prevents compression."),
    ("\"I'll carry heavy grocery bags on that arm to strengthen it.\"", 1, "Avoid heavy lifting on the fistula arm."),
    ("\"I'll sleep on that arm so it stays warm.\"", 1, "Avoid pressure on the fistula."),
    ("\"If the buzz stops, I'll call the dialysis unit right away.\"", 0, "Possible clot."),
    ("\"I'll wash the arm before each dialysis.\"", 0, "Infection prevention.")],
   "Daily checks and avoiding pressure or heavy lifting protect the fistula.",
   "Evaluate fistula teaching.",
   ex=[note("Week 2", "Tolerating 3.5-hour treatments.", id="dds-2wk")]),
]

cases = [
    case("TURP with bladder irrigation and TUR syndrome", turp_tabs, turp_qs),
    case("Kidney transplant rejection and tacrolimus toxicity", tx_tabs, tx_qs),
    case("Anemia and mineral bone disease of chronic kidney disease", ckd_tabs, ckd_qs),
    case("Starting hemodialysis with disequilibrium syndrome", dds_tabs, dds_qs),
]

BUILT = build(121, "renal-cases-turp-transplant-ckd-hdstart",
              "Renal case studies: TURP, kidney transplant rejection, CKD anemia and starting hemodialysis",
              items, cases, case_conversion=True)
