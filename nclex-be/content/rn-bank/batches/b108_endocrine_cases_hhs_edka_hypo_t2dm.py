"""Batch 108 (case conversion, chunk 3: endocrine): four full NGN case studies (CASE-MAP.md).

Cases: hyperosmolar hyperglycemic state; euglycemic diabetic ketoacidosis on an SGLT2 inhibitor after surgery;
prolonged hypoglycemia from a sulfonylurea in a hospitalized client; newly diagnosed type 2 diabetes starting
metformin, a GLP-1 receptor agonist and an SGLT2 inhibitor. Each case walks the six CJMM steps in order, unfolds
with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
ENDO = "endocrine"
DKA = "diabetic-ketoacidosis"
HHS = "hyperosmolar-hyperglycemic-state"
INS = "insulin-therapy"
NONINS = "non-insulin-diabetes-medications"
HYPO_TX = "hypoglycemia-treatment"
HYPO_REC = "recognizing-hypoglycemia"
DSM = "diabetes-self-management"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
MEDSAFE = "medication-safety-systems"
HANDOFF = "handoff-and-sbar-communication"
PREOP = "preoperative-care"

items = []

# ================================================================================================
# Case 1: Hyperosmolar hyperglycemic state
# ================================================================================================
hhs_tabs = [
    ("hhs-notes", "Nurses' notes", [note("Emergency department, 1100",
        "A 79-year-old client with type 2 diabetes on metformin is brought from home after a week of a urinary "
        "infection, poor intake and increasing drowsiness. The client responds to voice but is disoriented. Mucous "
        "membranes are dry and skin turgor is poor. The client has had a urine output of about 30 mL/hour since "
        "arrival. Weight 60 kg.", id="hhs-note")]),
    ("hhs-labs", "Laboratory results", [table("Laboratory results", ["1100", "Reference range"], [
        ("Glucose", ["980 mg/dL", "70 to 110 mg/dL"]),
        ("Sodium", ["150 mEq/L", "135 to 145 mEq/L"]),
        ("Potassium", ["4.4 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("BUN / creatinine", ["56 / 2.1 mg/dL", "10 to 20 / 0.6 to 1.2 mg/dL"]),
        ("Serum osmolality", ["374 mOsm/kg", "275 to 295 mOsm/kg"]),
        ("pH / bicarbonate", ["7.34 / 20 mEq/L", "7.35 to 7.45 / 22 to 26 mEq/L"]),
        ("Serum ketones (beta-hydroxybutyrate)", ["0.4 mmol/L", "below 0.6 mmol/L"])], id="hhs-lab")]),
]

hhs_qs = [
hl_table(M(PA, HHS, "recognize_cues", "moderate", ENDO, "ms ada lab"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "Value"],
   [(["Glucose", "980 mg/dL"], True, "Extreme hyperglycemia."),
    (["Serum osmolality", "374 mOsm/kg"], True, "Severe hyperosmolality explains the drowsiness."),
    (["Mental status", "Drowsy, disoriented"], True, "Neurologic change tracks osmolality."),
    (["BUN / creatinine", "56 / 2.1 mg/dL"], True, "Prerenal kidney injury from dehydration."),
    (["Serum ketones", "0.4 mmol/L"], False, "Normal: there is no significant ketosis, which helps separate HHS from DKA."),
    (["pH", "7.34"], False, "Only slightly low, from dehydration; not ketoacidosis.")],
   "Extreme glucose, high osmolality, altered mentation and kidney injury with little ketosis define HHS.",
   "Recognize cues of hyperosmolar hyperglycemic state."),

calc(M(RRP, LABS, "analyze_cues", "difficult", ENDO, "lab ms"),
   "High glucose draws water into the blood and lowers the measured sodium. Corrected sodium = measured sodium + "
   "1.6 x ((glucose - 100) / 100). What is the client's corrected sodium? Record your answer as a whole number.",
   "mEq/L", 0, "Corrected sodium", 164,
   "150 + 1.6 x ((980 - 100) / 100) = 150 + 1.6 x 8.8 = 150 + 14.1 = 164 mEq/L. The client's true free water "
   "deficit is larger than the measured sodium suggests.",
   "Calculate and interpret corrected sodium in hyperglycemia."),

cloze(M(PA, HHS, "prioritize_hypotheses", "difficult", ENDO, "ms ada"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's most urgent problem is {{b1}}. The main danger of correcting it too fast is {{b2}}.",
   [[("severe dehydration with hyperosmolality", True, "HHS causes fluid losses of around 9 L in adults."),
     ("ketoacidosis", False, "Ketones are normal."),
     ("hyperkalemia", False, "Potassium is normal."),
     ("a stroke", False, "The drowsiness fits hyperosmolality; no focal signs are described.")],
    [("cerebral edema from a rapid fall in osmolality", True, "Brain cells adapt to high osmolality; a fast fall pulls water into them."),
     ("hypertension", False, "Not the main danger."),
     ("hyperglycemia returning", False, "Not the danger of correcting too fast."),
     ("urinary retention", False, "Not related.")]],
   "Replace fluid first and lower glucose and osmolality gradually to avoid cerebral edema.",
   "Prioritize the problem and its treatment risks in HHS."),

matrix_mc(M(PPT, INS, "generate_solutions", "difficult", ENDO, "ada ph"),
   "For each part of the plan, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("0.9% sodium chloride 1 L in the first hour", 0, "Restores circulating volume first."),
    ("Start the insulin infusion before any IV fluid", 1, "Insulin moves glucose and water into cells and can cause collapse in a dry client."),
    ("Insulin infusion once fluids are running and potassium is at least 3.3 mEq/L", 0, "The usual sequence."),
    ("Aim for a fall in glucose of about 50 to 75 mg/dL per hour", 0, "A gradual fall limits osmotic shifts."),
    ("Add dextrose to the IV fluid when glucose reaches about 300 mg/dL", 0, "Lets insulin continue while preventing a fast fall."),
    ("Continue metformin to help lower the glucose", 1, "Held with kidney injury and dehydration because of lactic acidosis risk.")],
   "Fluids first, then insulin with potassium checked, a slow glucose fall with dextrose added at about 300 mg/dL, "
   "and metformin held.",
   "Generate the treatment plan for HHS.",
   ex=[note("Orders, 1130", "0.9% sodium chloride 1 L over 1 hour, then reassess. Insulin infusion per protocol. "
       "Urine culture. Ceftriaxone IV. Hold metformin.", id="hhs-orders")]),

sata(M(PA, HHS, "take_action", "difficult", ENDO, "ada ms"),
   "Review the 1500 findings. Which actions should the nurse take?",
   [("Tell the provider about the new headache and confusion", True, "Possible early cerebral edema with a fast fall."),
    ("Ask for the insulin rate and fluid tonicity to be reviewed", True, "Slowing the fall in glucose and osmolality is needed."),
    ("Do a full neurologic assessment, including pupils", True, "Tracks cerebral edema."),
    ("Keep the head of the bed at 30 degrees", True, "Supports venous drainage from the brain."),
    ("Increase the insulin rate to bring the glucose to normal faster", False, "Would worsen the problem."),
    ("Switch to 5% dextrose in water at a high rate", False, "A large free-water load lowers osmolality too fast.")],
   "Glucose fell about 175 mg/dL an hour with new neurologic change; slow the correction and assess for cerebral "
   "edema.",
   "Take action when hyperosmolality is corrected too fast.",
   ex=[flow("Hourly checks", ["1200", "1400", "1500"], [
       ("Glucose", ["920", "570", "395"]),
       ("Serum osmolality", ["370", "352", "338"]),
       ("Mental status", ["Drowsy, disoriented", "More alert", "New headache, more confused"]),
       ("Insulin infusion", ["Started 6 units/hour", "6 units/hour", "6 units/hour"])], id="hhs-1500")]),

matrix_mc(M(MOC, HANDOFF, "evaluate_outcomes", "difficult", ENDO, "mgmt ms"),
   "The client is moving to the medical unit the next day. For each part of the handoff, indicate whether it is "
   "adequate or needs correction.",
   ["Adequate", "Needs correction"],
   [("Glucose trend over 24 hours with the time insulin infusion stopped", 0, "Lets the receiving nurse judge stability."),
    ("Basal insulin given 2 hours before the infusion stopped", 0, "The overlap prevents rebound hyperglycemia."),
    ("\"Sodium was high but it's fine now\" with no value", 1, "Give the value and trend."),
    ("Urine culture result and antibiotic plan", 0, "The trigger must be treated."),
    ("No mention of the confusion episode at 1500", 1, "The receiving nurse needs to know about the neurologic change."),
    ("Metformin held; kidney function trend given", 0, "Explains the medication plan.")],
   "A safe handoff gives values and trends, the insulin transition, the trigger's treatment and significant events.",
   "Evaluate a handoff after HHS.",
   ex=[flow("Next day, 0900", ["0900"], [
       ("Glucose", ["168 to 212 mg/dL on subcutaneous insulin"]),
       ("Sodium", ["146 mEq/L"]),
       ("Creatinine", ["1.3 mg/dL"]),
       ("Mental status", ["Oriented x3"])], id="hhs-day2")]),
]

# ================================================================================================
# Case 2: Euglycemic DKA on an SGLT2 inhibitor after surgery
# ================================================================================================
edka_tabs = [
    ("edka-notes", "Nurses' notes", [note("Surgical unit, postoperative day 1",
        "A 58-year-old client with type 2 diabetes had a bowel resection yesterday. Home medicines: empagliflozin "
        "25 mg daily (last dose the morning of surgery), metformin, lisinopril. Since surgery the client has had "
        "only sips of fluid. Today the client has nausea, vomiting, abdominal pain beyond the incision, and deep, "
        "rapid breathing.", id="edka-note")]),
    ("edka-labs", "Laboratory results", [table("Laboratory results", ["Day 1", "Reference range"], [
        ("Glucose", ["192 mg/dL", "70 to 110 mg/dL"]),
        ("pH", ["7.12", "7.35 to 7.45"]),
        ("Bicarbonate", ["9 mEq/L", "22 to 26 mEq/L"]),
        ("Sodium / chloride", ["136 / 101 mEq/L", "-"]),
        ("Potassium", ["3.1 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("Beta-hydroxybutyrate", ["6.2 mmol/L", "below 0.6 mmol/L"]),
        ("Lactate", ["1.4 mmol/L", "0.5 to 2.0 mmol/L"])], id="edka-lab")]),
]

edka_qs = [
hl_table(M(PA, DKA, "recognize_cues", "moderate", ENDO, "ada lab"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "Value"],
   [(["pH / bicarbonate", "7.12 / 9 mEq/L"], True, "Severe metabolic acidosis."),
    (["Beta-hydroxybutyrate", "6.2 mmol/L"], True, "High ketones: ketoacidosis."),
    (["Breathing", "Deep and rapid"], True, "Kussmaul respirations compensate for acidosis."),
    (["Potassium", "3.1 mEq/L"], True, "Low potassium must be corrected before insulin."),
    (["Glucose", "192 mg/dL"], False, "Only mildly raised; this is the trap in euglycemic DKA."),
    (["Lactate", "1.4 mmol/L"], False, "Normal; the acidosis is not from lactate.")],
   "Severe ketoacidosis with a near-normal glucose fits euglycemic DKA.",
   "Recognize cues of euglycemic DKA."),

cloze(M(PA, DKA, "analyze_cues", "difficult", ENDO, "ada ph"),
   "Complete the sentences by choosing from the lists of options.",
   "The anion gap is {{b1}}. The near-normal glucose is explained by {{b2}}, and the trigger was {{b3}}.",
   [[("26", True, "136 - (101 + 9) = 26, a high anion gap."),
     ("12", False, "Recalculate: sodium minus chloride and bicarbonate."),
     ("35", False, "Recalculate."),
     ("9", False, "That is the bicarbonate.")],
    [("empagliflozin making the kidneys excrete glucose", True, "SGLT2 inhibitors lower glucose while ketones build up."),
     ("good control of diabetes", False, "The client is in ketoacidosis."),
     ("metformin", False, "Metformin does not cause this pattern."),
     ("a lab error", False, "The picture is consistent.")],
    [("surgery and fasting while the SGLT2 inhibitor was still active", True,
      "Stress and low carbohydrate intake push the body to make ketones."),
     ("the lisinopril", False, "Not a trigger."),
     ("too much IV fluid", False, "Not a trigger."),
     ("an allergy to anesthesia", False, "Not supported.")]],
   "A high-gap ketoacidosis with near-normal glucose after surgery and fasting is SGLT2 inhibitor-associated "
   "euglycemic DKA.",
   "Analyze euglycemic DKA."),

matrix_mc(M(PA, DKA, "prioritize_hypotheses", "difficult", ENDO, "ada"),
   "For each risk, indicate its priority now.",
   ["High priority", "Lower priority"],
   [("Worsening acidosis because it was missed while glucose looked normal", 0, "Delay is the main danger."),
    ("Cardiac dysrhythmia from low potassium, worsened by insulin", 0, "Insulin drives potassium into cells."),
    ("Hypoglycemia once insulin starts", 0, "Glucose is only 192, so dextrose is needed with the insulin."),
    ("Wound infection", 1, "Monitored, but not the immediate threat.")],
   "The priorities are treating the acidosis without causing hypokalemia or hypoglycemia.",
   "Prioritize risks in euglycemic DKA."),

matrix_mc(M(PPT, INS, "generate_solutions", "difficult", ENDO, "ada ph"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Potassium replacement before starting insulin", 0, "Insulin is held until potassium is at least 3.3 mEq/L."),
    ("Insulin infusion with 5% dextrose from the start", 0, "Insulin stops ketone production; dextrose prevents hypoglycemia."),
    ("Stop empagliflozin", 0, "The cause is stopped."),
    ("Hold insulin because the glucose is almost normal", 1, "Insulin is needed to stop ketosis whatever the glucose."),
    ("Sodium bicarbonate infusion for a pH of 7.12", 1, "Not recommended above a pH of about 7.0."),
    ("Check beta-hydroxybutyrate or anion gap every 2 to 4 hours", 0, "Tracks resolution of the ketoacidosis.")],
   "Replace potassium, then insulin with dextrose, stop the SGLT2 inhibitor and follow the ketones and anion gap.",
   "Generate solutions for euglycemic DKA.",
   ex=[note("Orders", "Potassium chloride 20 mEq/hour IV. Insulin infusion 0.1 units/kg/hour once potassium is 3.3 "
       "mEq/L or higher, with D5 0.45% sodium chloride. Stop empagliflozin.", id="edka-orders")]),

sata(M(PPT, INS, "take_action", "difficult", ENDO, "ada"),
   "Review the data 8 hours into treatment. Which actions should the nurse take?",
   [("Increase the dextrose as prescribed to keep glucose 150 to 200 mg/dL", True, "Lets insulin continue."),
    ("Continue the insulin infusion until the anion gap closes", True, "Stopping insulin early restarts ketosis."),
    ("Continue potassium replacement and recheck in 2 hours", True, "Potassium is still low-normal while insulin runs."),
    ("Stop the insulin because the glucose is 128 mg/dL", False, "The acidosis has not resolved."),
    ("Give an extra dose of empagliflozin to lower the glucose", False, "It caused the problem."),
    ("Start oral metformin now that the client is improving", False, "The client is still acidotic and not eating.")],
   "In euglycemic DKA, keep the insulin running and raise the dextrose; insulin stops when the gap closes.",
   "Take action during treatment of euglycemic DKA.",
   ex=[flow("8 hours into treatment", ["Hour 8"], [
       ("Glucose", ["128 mg/dL"]),
       ("Anion gap", ["18"]),
       ("Potassium", ["3.8 mEq/L"]),
       ("Infusions", ["Insulin 0.1 units/kg/hour; D5 0.45% at 150 mL/hour"])], id="edka-h8")]),

matrix_mc(M(SIC, MEDSAFE, "evaluate_outcomes", "difficult", ENDO, "ph mgmt"),
   "The client recovers. Before discharge, the team reviews how to prevent this. For each statement, indicate "
   "whether it shows a safe plan or needs correction.",
   ["Safe plan", "Needs correction"],
   [("\"Stop empagliflozin 3 days before any planned surgery.\"", 0, "Recommended to prevent euglycemic DKA."),
    ("\"Restart it only when eating and drinking normally.\"", 0, "Reduces risk."),
    ("\"If I'm vomiting, my sugar is normal, so I don't need to check ketones.\"", 1,
     "Ketones can be high with normal glucose; check ketones when sick."),
    ("The preoperative medication checklist now flags SGLT2 inhibitors", 0, "A system fix."),
    ("\"It's fine to keep taking it while I'm fasting for my religious holiday.\"", 1, "Fasting raises the DKA risk; discuss a plan first.")],
   "Prevention combines holding the drug around surgery and fasting, checking ketones when ill, and system flags.",
   "Evaluate prevention of SGLT2 inhibitor-associated DKA.",
   ex=[note("Day 5", "Anion gap closed on day 2. Eating normally. Glucose 140 to 180 mg/dL on basal insulin.",
       id="edka-day5")]),
]

# ================================================================================================
# Case 3: Prolonged hypoglycemia from a sulfonylurea
# ================================================================================================
hypo_tabs = [
    ("hypo-notes", "Nurses' notes", [note("Medical unit, 0630",
        "A 74-year-old client with type 2 diabetes and chronic kidney disease (eGFR 28) is NPO for an endoscopy. "
        "Medicines given last evening: insulin glargine 30 units, glipizide 10 mg, metoprolol 50 mg. At 0630 the "
        "client is sweaty, confused and trying to climb out of bed. The client has a saline lock in the left arm.",
        id="hypo-note")]),
    ("hypo-data", "Point-of-care tests", [flow("Glucose checks", ["2200", "0630"], [
        ("Capillary glucose", ["132 mg/dL", "42 mg/dL"]),
        ("Heart rate", ["68/min", "72/min"]),
        ("Mental status", ["Alert", "Confused, agitated"])], id="hypo-flow")]),
]

hypo_qs = [
hl_text(M(RRP, HYPO_REC, "recognize_cues", "moderate", ENDO, "ada ph"),
   "Highlight the findings that explain or show the client's problem.",
   ["The client ",
    ("is NPO for an endoscopy", True, "No food intake while glucose-lowering drugs continue."),
    " and received ",
    ("insulin glargine 30 units", True, "Full basal dose while fasting."),
    " and ",
    ("glipizide 10 mg", True, "A sulfonylurea causes long hypoglycemia, worse in kidney disease."),
    ". The client has ",
    ("an eGFR of 28", True, "Reduced clearance prolongs the sulfonylurea's effect."),
    ". At 0630 the client is ",
    ("sweaty and confused", True, "Signs of hypoglycemia."),
    ". The client's heart rate is ",
    ("72/min", False, "Not raised: metoprolol blunts the fast heart rate of hypoglycemia, but sweating remains."),
    "."],
   "Fasting with a full basal dose and a sulfonylurea in kidney disease caused severe hypoglycemia.",
   "Recognize cues of drug-induced hypoglycemia."),

matrix_mc(M(PPT, NONINS, "analyze_cues", "difficult", ENDO, "ph"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Glipizide-induced hypoglycemia can recur for 24 hours or more", 0, "Its effect outlasts one treatment."),
    ("Metoprolol may hide the tremor and fast heart rate of hypoglycemia", 0, "Beta blockers mask adrenergic signs; sweating persists."),
    ("Because the client is confused, oral glucose is the safest first treatment", 1, "Oral treatment risks aspiration when confused."),
    ("Kidney disease shortens the action of glipizide", 1, "Reduced clearance prolongs it."),
    ("The basal insulin dose is usually reduced when a client is NPO", 0, "Often by about 20% or as prescribed.")],
   "Sulfonylurea hypoglycemia is prolonged, beta blockers mask warning signs, and confused clients need IV or IM "
   "treatment.",
   "Analyze the factors in drug-induced hypoglycemia."),

cloze(M(PA, HYPO_TX, "prioritize_hypotheses", "difficult", ENDO, "ada"),
   "Complete the sentences by choosing from the lists of options.",
   "The priority is to {{b1}}. After the first treatment, the main risk is {{b2}}.",
   [[("raise the glucose at once by IV route", True, "The client has IV access and is confused."),
     ("give 4 oz of juice", False, "Unsafe with confusion."),
     ("recheck the glucose in an hour", False, "Delays treatment."),
     ("call the endoscopy unit", False, "Treat first.")],
    [("recurrent hypoglycemia as the glipizide continues to act", True, "Needs ongoing glucose checks and dextrose."),
     ("hyperkalemia", False, "Not expected."),
     ("rebound hypertension", False, "Not the main risk."),
     ("aspiration of juice", False, "Not given.")]],
   "Treat severe hypoglycemia by IV now, then watch for recurrence.",
   "Prioritize treatment of severe hypoglycemia."),

matrix_mc(M(PA, HYPO_TX, "generate_solutions", "difficult", ENDO, "ada ph"),
   "For each option, indicate whether it is indicated, not indicated, or contraindicated now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Dextrose 50% 25 mL IV push", 0, "Gives 12.5 g glucose quickly through the saline lock."),
    ("Glucagon 1 mg IM", 1, "Used when there is no IV access; the client has one."),
    ("Recheck glucose 15 minutes after treatment", 0, "Confirms the response."),
    ("Juice by mouth while confused", 2, "Aspiration risk."),
    ("A continuous dextrose infusion if hypoglycemia recurs", 0, "Covers the long sulfonylurea effect."),
    ("Octreotide if hypoglycemia keeps recurring, as prescribed", 0, "Reduces sulfonylurea-driven insulin release.")],
   "IV dextrose now, rechecks, an infusion for recurrence and octreotide for persistent sulfonylurea hypoglycemia.",
   "Generate solutions for sulfonylurea hypoglycemia.",
   ex=[note("Hypoglycemia protocol", "Glucose below 70 mg/dL and unable to swallow safely: D50 25 mL IV or glucagon "
       "1 mg IM if no IV. Recheck in 15 minutes. Notify the provider.", id="hypo-protocol")]),

sata(M(PA, HYPO_TX, "take_action", "difficult", ENDO, "ada"),
   "After D50, the client's glucose is 128 mg/dL at 0645 and 61 mg/dL at 0830. Which actions should the nurse take?",
   [("Treat the low glucose again per protocol", True, "Recurrent hypoglycemia."),
    ("Notify the provider for a continuous dextrose infusion", True, "Recurrence after a bolus is expected with glipizide."),
    ("Check glucose every hour", True, "Close monitoring."),
    ("Ask the provider to stop glipizide and review the insulin dose", True, "Removes the causes."),
    ("Send the client for the endoscopy as planned", False, "Unsafe while hypoglycemia is recurring."),
    ("Give the morning glipizide dose with the dextrose", False, "Continues the cause.")],
   "Recurrent hypoglycemia needs retreatment, an infusion, hourly checks and removal of the drug.",
   "Take action for recurrent hypoglycemia.",
   ex=[flow("Rechecks", ["0645", "0730", "0830"], [
       ("Glucose", ["128", "94", "61"]),
       ("Mental status", ["Alert", "Alert", "Sweaty, slow to answer"])], id="hypo-recheck")]),

matrix_mc(M(SIC, MEDSAFE, "evaluate_outcomes", "difficult", ENDO, "ph mgmt"),
   "The team reviews the event. For each finding, indicate whether it is a safe practice or a gap to fix.",
   ["Safe practice", "Gap to fix"],
   [("The full glargine dose was given the night before an NPO procedure", 1, "Basal insulin is usually reduced when fasting."),
    ("Glipizide was continued in a client with an eGFR of 28", 1, "Sulfonylureas are risky in kidney disease; alternatives exist."),
    ("Glucose was checked at 2200 and not again until 0630", 1, "Fasting clients on insulin need checks overnight."),
    ("The nurse used IV dextrose because the client was confused", 0, "Correct route."),
    ("The hypoglycemia protocol was posted on the unit", 0, "Supports rapid treatment."),
    ("The event was reported through the incident system", 0, "Allows system learning.")],
   "Fasting orders for insulin, sulfonylurea use in kidney disease and overnight monitoring were the gaps.",
   "Evaluate a hypoglycemia event for system gaps.",
   ex=[note("Day 2", "Glipizide stopped. Dextrose infusion stopped after 20 hours. Glargine reduced to 20 units.",
       id="hypo-day2")]),
]

# ================================================================================================
# Case 4: Newly diagnosed type 2 diabetes
# ================================================================================================
t2_tabs = [
    ("t2-notes", "Clinic notes", [note("Primary care clinic",
        "A 54-year-old client has been tired and thirsty and urinates often. BMI 34. History: hypertension, heart "
        "failure with reduced ejection fraction, and a father with medullary thyroid cancer. The client drinks "
        "4 or 5 beers on weekends.", id="t2-note")]),
    ("t2-labs", "Laboratory results", [table("Laboratory results", ["Result", "Reference range"], [
        ("HbA1c", ["8.6%", "below 5.7%"]),
        ("Fasting glucose", ["188 mg/dL", "70 to 99 mg/dL"]),
        ("eGFR", ["55 mL/min", "above 60"]),
        ("Urine albumin-to-creatinine ratio", ["120 mg/g", "below 30 mg/g"]),
        ("LDL cholesterol", ["146 mg/dL", "below 100 mg/dL"])], id="t2-lab")]),
]

t2_qs = [
hl_table(M(RRP, LABS, "recognize_cues", "moderate", ENDO, "ada lab"),
   "Highlight the findings that will shape the choice of diabetes medicines.",
   ["Finding", "Value"],
   [(["Heart failure", "Reduced ejection fraction"], True, "SGLT2 inhibitors lower heart failure admissions."),
    (["Urine albumin", "120 mg/g"], True, "Kidney disease favors an SGLT2 inhibitor."),
    (["Family history", "Medullary thyroid cancer in the father"], True, "GLP-1 receptor agonists are contraindicated with this history."),
    (["eGFR", "55 mL/min"], True, "Metformin is used, with a dose check below 45."),
    (["Alcohol", "4 or 5 beers on weekends"], True, "Binge alcohol raises lactic acidosis and hypoglycemia risk."),
    (["BMI", "34"], False, "Important, but the drug choice here turns on the heart, kidney and thyroid history.")],
   "Coexisting heart failure, kidney disease and a family history of medullary thyroid cancer guide drug choice.",
   "Recognize cues that guide diabetes medication choice."),

matrix_mr(M(PPT, NONINS, "analyze_cues", "difficult", ENDO, "ph ada"),
   "For each drug class, indicate the statements that apply. Each row may have more than one answer.",
   ["Protects the heart or kidneys", "Main risk to teach", "Contraindicated for this client"],
   [("Metformin", {1}, "Teach GI upset, holding before contrast and when ill, and B12 checks; it is first-line."),
    ("SGLT2 inhibitor (empagliflozin)", {0, 1}, "Benefits heart failure and kidney disease; teach genital infections, dehydration and DKA."),
    ("GLP-1 receptor agonist (semaglutide)", {2}, "Contraindicated with a personal or family history of medullary thyroid cancer."),
    ("Sulfonylurea (glipizide)", {1}, "Teach hypoglycemia, especially with alcohol.")],
   "Coexisting conditions decide which drugs protect and which are unsafe.",
   "Analyze diabetes drug classes for a specific client."),

cloze(M(PPT, NONINS, "prioritize_hypotheses", "difficult", ENDO, "ada ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The best plan combines metformin with {{b1}}, and the nurse should be most concerned about {{b2}}.",
   [[("empagliflozin", True, "It benefits both the heart failure and the kidney disease."),
     ("semaglutide", False, "Contraindicated with the family history."),
     ("glipizide", False, "No heart or kidney benefit and a hypoglycemia risk."),
     ("pioglitazone", False, "Causes fluid retention and worsens heart failure.")],
    [("weekend binge drinking", True, "Raises hypoglycemia, lactic acidosis and ketoacidosis risk."),
     ("the client's age", False, "Not the main concern."),
     ("the LDL alone", False, "Treated with a statin, but not the main medicine safety issue."),
     ("tiredness", False, "Expected to improve with control.")]],
   "Metformin plus an SGLT2 inhibitor fits heart failure and kidney disease; alcohol is the main safety concern.",
   "Prioritize the diabetes drug plan."),

sata(M(HPM, DSM, "generate_solutions", "difficult", ENDO, "ada"),
   "Which goals and teaching topics should the nurse include in the plan?",
   [("An HbA1c goal of below 7% for most adults", True, "Reduces complications."),
    ("Hold metformin and empagliflozin on sick days with vomiting or poor intake", True, "Sick-day rules prevent lactic acidosis and DKA."),
    ("Daily foot inspection", True, "Detects injuries early."),
    ("Annual dilated eye examination", True, "Screens for retinopathy."),
    ("Limit carbohydrates to under 20 g a day", False, "Not standard; individualized meal planning is used."),
    ("Stop taking medicines once glucose readings are normal", False, "Treatment continues.")],
   "Plans include individual targets, sick-day rules and complication screening.",
   "Generate a diabetes self-management plan.",
   ex=[note("Plan", "Metformin 500 mg twice daily, increased weekly. Empagliflozin 10 mg daily. Atorvastatin. "
       "Diabetes education referral.", id="t2-plan")]),

sata(M(HPM, DSM, "take_action", "difficult", ENDO, "ada ph"),
   "Which instructions should the nurse give about the new medicines?",
   [("Take metformin with meals to reduce stomach upset", True, "GI effects are common at first."),
    ("Drink fluids and keep the genital area clean and dry", True, "SGLT2 inhibitors cause glucose in the urine."),
    ("Check ketones when sick, even if glucose is normal", True, "Euglycemic DKA can occur."),
    ("Hold metformin for 48 hours after iodine contrast", True, "Prevents lactic acidosis."),
    ("Take the empagliflozin at bedtime so that it works overnight", False, "Taken in the morning to avoid night urination."),
    ("Stop the statin yourself if any muscle aches occur, without calling", False, "Report muscle pain; stopping is decided with the provider.")],
   "Teaching covers how to take each drug, its main risks and when to hold it.",
   "Teach a client starting diabetes medicines.",
   ex=[note("Follow-up visit, 2 weeks", "Some loose stools on metformin. No hypoglycemia. Asks about a yeast infection.",
       id="t2-2wk")]),

matrix_mc(M(HPM, DSM, "evaluate_outcomes", "difficult", ENDO, "ada"),
   "Review the 3-month visit. For each finding, indicate whether it shows the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("HbA1c 7.1%", 0, "A large fall toward goal."),
    ("Urine albumin-to-creatinine ratio 70 mg/g", 0, "Falling albuminuria."),
    ("Weight down 3 kg", 0, "Helps glucose and blood pressure."),
    ("Two yeast infections treated at urgent care", 1, "A known SGLT2 inhibitor effect; hygiene and treatment plan needed."),
    ("Dizziness on standing, BP 98/60 mm Hg", 1, "Volume depletion; review diuretics and fluids."),
    ("Has not yet had the eye examination", 1, "Annual screening is needed.")],
   "Glucose, kidney and weight goals are improving; infections, low pressure and missed screening need follow-up.",
   "Evaluate diabetes outcomes at 3 months.",
   ex=[flow("3-month visit", ["Month 3"], [
       ("HbA1c", ["7.1%"]),
       ("Urine albumin-to-creatinine", ["70 mg/g"]),
       ("Weight", ["Down 3 kg"]),
       ("Blood pressure", ["98/60 mm Hg standing, dizzy"]),
       ("Other", ["Two yeast infections; eye exam not done"])], id="t2-m3")]),
]

cases = [
    case("Hyperosmolar hyperglycemic state", hhs_tabs, hhs_qs),
    case("Euglycemic DKA on an SGLT2 inhibitor after surgery", edka_tabs, edka_qs),
    case("Prolonged hypoglycemia from a sulfonylurea", hypo_tabs, hypo_qs),
    case("Newly diagnosed type 2 diabetes", t2_tabs, t2_qs),
]

BUILT = build(108, "endocrine-cases-hhs-edka-hypo-t2dm",
              "Endocrine case studies: hyperosmolar hyperglycemic state, euglycemic DKA, sulfonylurea hypoglycemia and "
              "new type 2 diabetes",
              items, cases, case_conversion=True)
