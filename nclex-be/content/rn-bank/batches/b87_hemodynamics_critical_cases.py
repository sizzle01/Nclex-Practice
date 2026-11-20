"""Batch 87 (wave 1, chunk 5): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - acute ischemic stroke in the thrombolytic window and acute pulmonary edema (PHI.RRP, PHI.PA, PHI.PPT).

Chunk 5 raises depth: most items ask which of several reasonable actions comes first, using combined data.
Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
POSTPROC = "post-procedure-complications"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
NEURO_DX = "neurological-assessment-and-procedures"
GIDX = "gi-diagnostic-procedures"
POSTOP = "postoperative-care"
LABS = "laboratory-values-and-monitoring"
SCI = "spinal-cord-injury"
O2 = "impaired-oxygenation"
BURN = "burn-care"
PIT = "pituitary-disorders"
OPI = "opioid-safety"
STROKE = "stroke-care"
ANTIHTN = "heart-failure-interventions"
PULM_EDEMA = "acute-pulmonary-edema"
OXY = "oxygen-therapy-and-devices"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, POSTPROC, "prioritize_hypotheses", "difficult", "respiratory", "ms"),
   "Thirty minutes after a right subclavian central venous catheter is placed at the bedside, a client becomes anxious and short of breath with sharp right chest pain. Respiratory rate is 32/min, SpO2 is 88% on 2 L/min, breath sounds are absent over the right upper chest, and the right chest is hyperresonant. Blood pressure is 124/78 mm Hg, and the trachea is midline. A confirming chest x-ray has not yet been taken. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Iatrogenic pneumothorax", True, "Sudden dyspnea, chest pain, absent breath sounds and hyperresonance on the side of a subclavian line placement point to a pneumothorax; normal blood pressure and a midline trachea suggest it is not yet under tension."),
    ("Air embolism", False, "Air embolism causes sudden hypotension, confusion and a mill-wheel murmur, not one-sided absent breath sounds."),
    ("Tension pneumothorax", False, "Blood pressure is normal and the trachea is midline, so tension has not developed yet."),
    ("Pulmonary embolism", False, "PE does not cause absent breath sounds and hyperresonance on one side.")],
   [("Raise the head of the bed, give oxygen and notify the provider", True, "Supports breathing and gets the provider to the bedside."),
    ("Prepare for a chest x-ray and possible chest tube insertion", True, "Imaging confirms it, and a chest tube may be needed."),
    ("Flush the new central line with 20 mL of saline before using it", False, "This does not address the problem, and the line's position is not yet confirmed."),
    ("Start the prescribed vasopressor through the new central line", False, "The line should not be used until placement is confirmed."),
    ("Lay the client flat to reduce the work of breathing", False, "Lying flat worsens breathing.")],
   [("Respiratory rate, SpO2 and breath sounds", True, "Show whether the pneumothorax is enlarging."),
    ("Blood pressure and tracheal position", True, "Falling pressure or tracheal shift signals tension pneumothorax."),
    ("Urine output measured and recorded each hour", False, "Not a key guide."),
    ("Fingerstick blood glucose before meals and at bedtime", False, "Not relevant."),
    ("Bowel sounds listened for in each of the four quadrants", False, "Not relevant.")],
   "Pneumothorax is a known complication of subclavian and internal jugular line placement; confirm placement by x-ray before use and watch for signs of tension.",
   "Recognize pneumothorax after central line insertion."),

mc(M(RRP, CV_DX, "analyze_cues", "difficult", "cardiovascular", "ms"),
   "A client admitted with an upper GI bleed has a central line. Readings: central venous pressure 2 mm Hg, heart rate 122/min, blood pressure 92/58 mm Hg and urine output 18 mL/hr. How should the nurse interpret these findings?",
   [("Fluid overload from the IV fluids that have been given since admission", False,
     "Fluid overload raises CVP; a CVP of 2 is low."),
    ("Hypovolemia from blood loss, with poor filling of the heart", True,
     "A low CVP with tachycardia, low blood pressure and low urine output shows intravascular volume depletion."),
    ("Right-sided heart failure that is reducing cardiac output", False,
     "Right heart failure raises CVP."),
    ("An expected reading after placement of a new central line", False,
     "These values with signs of shock are not expected.")],
   "CVP reflects right-heart filling (normal about 2 to 8 mm Hg); a low CVP with tachycardia and hypotension points to hypovolemia, a high CVP to overload or right heart failure.",
   "Interpret central venous pressure."),

mc(M(RRP, CV_DX, "take_action", "difficult", "cardiovascular", "aha ms"),
   "A client on telemetry suddenly becomes dizzy and pale. The monitor shows P waves at 78/min and QRS complexes at 34/min with no relationship between them. Blood pressure is 74/40 mm Hg. Atropine has not helped. What should the nurse prepare for next?",
   [("Synchronized cardioversion at 100 joules", False,
     "Cardioversion is for unstable tachycardias, not complete heart block."),
    ("Transcutaneous pacing with pads on the chest", True,
     "Symptomatic third-degree heart block that does not respond to atropine is treated with pacing."),
    ("A dose of IV adenosine given rapidly", False,
     "Adenosine slows conduction and is for supraventricular tachycardia."),
    ("Starting CPR right away", False,
     "The client has a pulse and blood pressure; CPR is for pulselessness.")],
   "Symptomatic bradycardia or complete heart block that does not respond to atropine is treated with transcutaneous pacing or a dopamine or epinephrine infusion.",
   "Respond to symptomatic complete heart block."),

mc(M(RRP, NEURO_DX, "take_action", "moderate", "neurological", "ms"),
   "The nurse is preparing an adult client for a lumbar puncture. How should the nurse position the client?",
   [("Supine with the head of the bed raised to 45 degrees and the knees straight", False,
     "This position does not open the spaces between the lumbar vertebrae."),
    ("On the side with the back curved, knees drawn up and chin tucked to the chest", True,
     "Curling opens the spaces between the vertebrae so the needle can enter the spinal canal."),
    ("Prone with a pillow under the chest and arms raised above the head", False,
     "Prone is not the standard bedside lumbar puncture position."),
    ("Standing and leaning forward over the bedside table with the arms resting on it", False,
     "Standing is unsafe; sitting and leaning forward is an alternative, but not standing.")],
   "For lumbar puncture, place the client side-lying in a curled (fetal) position, or sitting and leaning forward over a table.",
   "Position a client for lumbar puncture."),

mc(M(RRP, GIDX, "take_action", "moderate", "gastrointestinal", "ms"),
   "A client with ascites is scheduled for a bedside paracentesis. Which action should the nurse take right before the procedure?",
   [("Keep the client NPO for 12 hours before the procedure", False,
     "Prolonged fasting is not usually required for paracentesis."),
    ("Have the client empty the bladder right before the procedure", True,
     "An empty bladder lowers the risk of puncturing it with the needle."),
    ("Place the client flat on the left side with the knees bent", False,
     "The client usually sits upright or with the head raised so fluid collects in the lower abdomen."),
    ("Give a large glass of water to keep the client hydrated during the procedure", False,
     "Extra fluid fills the bladder, which raises the risk of puncture.")],
   "Before paracentesis: have the client void, record weight, girth and vital signs, and position upright or semi-Fowler's.",
   "Prepare a client for paracentesis."),

mc(M(RRP, POSTOP, "analyze_cues", "difficult", "gastrointestinal", "ms"),
   "Three days after a bowel resection, a client has abdominal distention, no flatus for 24 hours, nausea and absent bowel sounds. The client has been receiving IV hydromorphone every 3 hours and has walked only once. Vital signs are stable, there is no fever, and the pain is diffuse and crampy rather than sharp. What is the most likely explanation?",
   [("Peritonitis from a leak at the join between the bowel ends", False,
     "Peritonitis usually causes fever, tachycardia, rigidity and severe localized pain."),
    ("Paralytic ileus worsened by opioids and immobility", True,
     "Absent bowel sounds, distention and no flatus after bowel surgery, with stable vital signs and heavy opioid use, fit paralytic ileus."),
    ("Constipation from not eating enough fiber at home", False,
     "Diet before surgery does not explain these postoperative findings."),
    ("Acute appendicitis that developed after the surgery", False,
     "The picture fits ileus, not appendicitis.")],
   "Postoperative ileus causes distention, absent bowel sounds and no flatus; opioids and immobility worsen it. Treatment includes NPO status, NG decompression if vomiting, walking and opioid-sparing analgesia.",
   "Recognize postoperative ileus."),

matrix_mc(M(RRP, LABS, "analyze_cues", "moderate", "pharmacology", "lab ph"),
   "The nurse reviews laboratory results for four clients. For each result, indicate whether it is expected for the client's situation or should be reported now.",
   ["Expected", "Report now"],
   [("INR 2.6 in a client taking warfarin for atrial fibrillation (target 2 to 3)", 0,
     ["This is within the target range.", "No report is needed."]),
    ("Potassium 5.9 mEq/L in a client taking lisinopril and spironolactone", 1,
     ["Both drugs raise potassium; this level is dangerous.", "Hyperkalemia needs prompt reporting."]),
    ("Glucose 46 mg/dL in a client on insulin who is sweating", 1,
     ["Symptomatic hypoglycemia needs immediate treatment.", "This needs action now."]),
    ("Hemoglobin 13.6 g/dL in a client admitted for a knee replacement", 0,
     ["This is a normal value.", "No report is needed."])],
   "Interpret labs in context: in-range values and drug targets are expected; dangerous electrolyte levels and symptomatic hypoglycemia need immediate action.",
   "Interpret laboratory results in context."),

sata(M(RRP, POSTOP, "recognize_cues", "difficult", "integumentary", "ms"),
   "On the fourth day after an open abdominal surgery, which findings suggest a surgical site infection?",
   [("Increasing redness spreading 3 cm from the incision", True, "Spreading redness beyond the edges suggests infection."),
    ("Thick yellow drainage with an odor", True, "Purulent drainage is a sign of infection."),
    ("A slight pink line along well-joined wound edges", False, "Mild pinkness along approximated edges is normal healing."),
    ("A temperature of 38.6 °C (101.5 °F) with increasing incision pain", True, "Fever and worsening pain after day 3 suggest infection."),
    ("A ridge of firm tissue under the incision", False, "A healing ridge forms normally by about day 5 to 9."),
    ("Mild itching around the incision", False, "Itching is common during healing.")],
   "Surgical site infection usually appears 3 to 7 days after surgery: spreading redness, purulent drainage, fever and increasing pain; a healing ridge and mild pinkness are normal.",
   "Recognize surgical site infection."),

# ---- physiological adaptation ----
bowtie(M(PA, SCI, "prioritize_hypotheses", "difficult", "neurological", "ms"),
   "A 19-year-old was admitted 2 hours ago after a diving accident with a complete spinal cord injury at C6. Blood pressure is now 78/42 mm Hg, heart rate 48/min, and the skin is warm, dry and pink below the injury. There is no external bleeding, and the abdomen is soft. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Neurogenic shock", True, "Loss of sympathetic tone after a cervical injury causes hypotension, bradycardia and warm, dry skin."),
    ("Hypovolemic shock from internal bleeding", False, "Bleeding causes tachycardia and cool, pale skin."),
    ("Autonomic dysreflexia", False, "Dysreflexia causes severe hypertension and occurs later, after spinal shock resolves."),
    ("Septic shock", False, "There is no infection source this early, and the pattern fits loss of sympathetic tone.")],
   [("Give IV fluids and prepare prescribed vasopressors", True, "Fluids and vasopressors restore blood pressure and spinal cord perfusion."),
    ("Prepare atropine for symptomatic bradycardia", True, "Unopposed vagal tone slows the heart; atropine treats symptomatic bradycardia."),
    ("Raise the head of the bed to 90 degrees to help breathing", False, "Sitting up worsens hypotension in neurogenic shock."),
    ("Give a large dose of furosemide to protect the kidneys", False, "Diuresis worsens hypotension."),
    ("Apply a warming blanket at a high setting to raise the heart rate", False, "Warming does not treat neurogenic shock and these clients lose temperature control.")],
   [("Blood pressure and heart rate", True, "Show the response to fluids, vasopressors and atropine."),
    ("Respiratory effort and oxygen saturation", True, "A C6 injury affects breathing muscles, and the injury level can rise with swelling."),
    ("Pupil size and response to light once each shift", False, "Not the key guide for this condition."),
    ("Bowel sounds in each of the four quadrants once a shift", False, "Not the key guide."),
    ("Body weight measured on the same scale once a week", False, "Not relevant in the acute phase.")],
   "Neurogenic shock follows injury at T6 or above: hypotension, bradycardia and warm skin. Treat with fluids, vasopressors and atropine, and watch breathing closely.",
   "Recognize and treat neurogenic shock."),

mc(M(PA, O2, "take_action", "difficult", "respiratory", "ms"),
   "A client with ARDS from sepsis is on a ventilator with an FiO2 of 0.8 and PEEP of 12 cm H2O. The PaO2 is still 56 mm Hg. Which intervention should the nurse anticipate?",
   [("Lowering the PEEP to reduce pressure on the lungs", False,
     "Lower PEEP would allow more alveoli to collapse and worsen hypoxemia."),
    ("Prone positioning for long periods each day", True,
     "Prone positioning improves oxygenation and survival in moderate to severe ARDS."),
    ("Giving a large IV fluid bolus to improve oxygen delivery", False,
     "Extra fluid can worsen lung edema in ARDS."),
    ("Removing the ventilator and starting a nasal cannula at 6 L/min", False,
     "The client needs more, not less, respiratory support.")],
   "Refractory hypoxemia in ARDS is managed with lung-protective ventilation, PEEP, conservative fluids and prone positioning.",
   "Anticipate treatment for refractory hypoxemia in ARDS."),

mc(M(PA, BURN, "evaluate_outcomes", "difficult", "integumentary", "ms"),
   "A client who weighs 80 kg is receiving fluid resuscitation 8 hours after a 40% total body surface area burn. Which finding best shows the fluid resuscitation is adequate?",
   [("Blood pressure of 132/84 mm Hg", False,
     "Blood pressure can stay normal while organs are underperfused."),
    ("Urine output of 40 to 50 mL/hr", True,
     "In adults, urine output of about 0.5 mL/kg/hr (about 40 mL/hr for 80 kg) is the main guide to burn resuscitation."),
    ("Heart rate of 112/min that has stayed the same", False,
     "Tachycardia is common in burns but does not confirm adequate resuscitation."),
    ("Absence of thirst when the client is asked", False,
     "Thirst is not a reliable guide.")],
   "In burn resuscitation, titrate fluids to urine output of about 0.5 mL/kg/hr in adults (about 1 mL/kg/hr in small children).",
   "Evaluate burn fluid resuscitation."),

mc(M(PA, PIT, "analyze_cues", "difficult", "endocrine", "ms"),
   "Two days after a traumatic brain injury, a client's urine output is 450 mL/hr for 3 hours. Urine specific gravity is 1.002, serum sodium is 152 mEq/L and the client is very thirsty. What should the nurse anticipate?",
   [("Fluid restriction and an infusion of 3% saline", False,
     "This treats SIADH, which causes low sodium and concentrated urine."),
    ("Desmopressin and replacement of the lost water", True,
     "Large dilute urine output with high sodium after brain injury is diabetes insipidus; desmopressin replaces ADH."),
    ("A loop diuretic such as furosemide to control the sodium", False,
     "Diuretics would worsen the water loss."),
    ("Insulin, since the high urine output suggests hyperglycemia", False,
     "Hyperglycemia causes high specific gravity from glucose, not 1.002.")],
   "Diabetes insipidus: large volumes of dilute urine, high serum sodium and thirst; treat with desmopressin and water replacement, and monitor sodium and output.",
   "Recognize diabetes insipidus after brain injury."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, OPI, "evaluate_outcomes", "difficult", "pharmacology", "ph"),
   "A client who took extended-release oxycodone was given naloxone 0.4 mg IV for a respiratory rate of 6/min. Ten minutes later the client is awake with a respiratory rate of 16/min. What should the nurse do next?",
   [("Document that the overdose has resolved and plan for discharge", False,
     "Naloxone wears off in 30 to 90 minutes, before extended-release oxycodone does."),
    ("Keep monitoring breathing and sedation, ready to repeat naloxone", True,
     "Respiratory depression can return as naloxone wears off; repeat doses or an infusion may be needed."),
    ("Give the client's next scheduled oxycodone dose to prevent withdrawal", False,
     "More opioid would worsen the toxicity."),
    ("Stop the close monitoring now that the client is awake and talking normally", False,
     "Monitoring must continue for several hours.")],
   "Naloxone's effect is shorter than that of many opioids, especially long-acting ones; monitor closely and expect repeat doses or an infusion.",
   "Monitor after opioid reversal."),
]

# ---- Case 1: acute ischemic stroke ------------------------------------------------------------
stroke_tabs = [
    ("stroke-notes", "Nurses' notes", [note("Emergency department, 1015",
        "A 68-year-old client was last known well at 0900 when they spoke normally with a neighbor. At 0945 the spouse "
        "found the client with slurred speech, a right facial droop and weakness of the right arm and leg. History: "
        "hypertension, atrial fibrillation (stopped apixaban 2 weeks ago because of cost) and type 2 diabetes. A CT scan "
        "of the head without contrast shows no bleeding.", id="stroke-note")]),
    ("stroke-vitals", "Vital signs", [flow("Vital signs", ["1005", "1015"], [
        ("Blood pressure", ["196/110 mm Hg", "192/112 mm Hg"]),
        ("Heart rate", ["104/min, irregular", "102/min, irregular"]),
        ("SpO2", ["96% on room air", "96% on room air"]),
        ("Fingerstick glucose", ["148 mg/dL", "-"])], id="stroke-flow")]),
    ("stroke-labs", "Laboratory results", [table("Laboratory results, 1010", ["Result", "Reference"], [
        ("Platelets", ["214,000/mm3", "150,000-400,000/mm3"]),
        ("INR", ["1.1", "0.8-1.1"]),
        ("Creatinine", ["1.0 mg/dL", "0.6-1.2 mg/dL"])], id="stroke-lab")]),
]

stroke_qs = [
hl_text(M(PA, STROKE, "recognize_cues", "moderate", "neurological", "ms aha"),
   "Review the nurse's note. Highlight the findings that are most important for deciding on emergency stroke treatment.",
   ["The client was ",
    ("last known well at 0900", True, "The time last known well sets the window for thrombolytic therapy."),
    ". The client has ",
    ("slurred speech, a right facial droop and right-sided weakness", True, "Sudden focal deficits suggest an acute stroke."),
    ". History includes ",
    ("atrial fibrillation with apixaban stopped 2 weeks ago", True, "Untreated atrial fibrillation is a likely source of an embolic stroke."),
    ". ",
    ("The client lives with a spouse", False, "Important for discharge planning, not the emergency decision."),
    ". The head CT ",
    ("shows no bleeding", True, "Ruling out hemorrhage is required before thrombolysis."),
    "."],
   "Time last known well, sudden focal deficits, a cardioembolic risk factor and a CT without bleeding are key to emergency stroke decisions.",
   "Recognize cues for acute stroke treatment."),

cloze(M(PA, STROKE, "analyze_cues", "moderate", "neurological", "ms aha"),
   "Complete the sentence based on the client's data.",
   "The client is most likely having {{b1}}, probably caused by {{b2}}, and is {{b3}} for thrombolytic therapy based on time.",
   [[("an acute ischemic stroke in the left hemisphere", True, "Right-sided weakness and speech problems point to the left hemisphere; the CT shows no bleed."),
     ("a hemorrhagic stroke", False, "The CT shows no bleeding."),
     ("a transient ischemic attack that has already resolved", False, "The deficits are still present.")],
    [("a clot from the heart related to atrial fibrillation", True, "Stopping anticoagulation in atrial fibrillation raises embolic stroke risk."),
     ("low blood sugar", False, "Glucose is 148 mg/dL."),
     ("a migraine with aura", False, "The history and risk factors point to a cardioembolic stroke.")],
    [("still within the window", True, "Thrombolysis can be given up to 4.5 hours after last known well; 0900 to 1015 is within the window."),
     ("too late", False, "Only about 75 minutes have passed."),
     ("not eligible because of the time", False, "Time does not exclude this client.")]],
   "An embolic ischemic stroke from untreated atrial fibrillation within 4.5 hours of last known well can be treated with a thrombolytic if no contraindication exists.",
   "Analyze cues in acute ischemic stroke."),

matrix_mc(M(RRP, LABS, "prioritize_hypotheses", "difficult", "neurological", "ms aha"),
   "Before thrombolytic therapy, the team reviews the client's data. For each finding, indicate whether it currently prevents giving the thrombolytic.",
   ["Prevents treatment now", "Does not prevent treatment"],
   [("Blood pressure 192/112 mm Hg", 0, ["Blood pressure must be below 185/110 before a thrombolytic is given; it must be lowered first.", "It must be corrected first."]),
    ("INR 1.1 after stopping apixaban 2 weeks ago", 1, ["A normal INR and no recent anticoagulant dose do not prevent treatment.", "This does not prevent treatment."]),
    ("Platelets 214,000/mm3", 1, ["A normal platelet count does not prevent treatment.", "This does not prevent treatment."]),
    ("Glucose 148 mg/dL", 1, ["Glucose is within the acceptable range.", "This does not prevent treatment."])],
   "Before thrombolysis, blood pressure must be lowered below 185/110 mm Hg; normal coagulation, platelets and glucose do not prevent treatment.",
   "Identify barriers to thrombolytic therapy."),

sata(M(RRP, NEURO_DX, "generate_solutions", "difficult", "neurological", "ms aha"),
   "Which actions should the nurse anticipate before the thrombolytic is started?",
   [("Lower the blood pressure below 185/110 mm Hg", True, "Required before thrombolysis."),
    ("Complete a baseline NIH Stroke Scale assessment", True, "A baseline score guides decisions and later comparisons."),
    ("Place any needed tubes and catheters before the infusion", True, "Invasive lines and tubes are placed before thrombolysis when possible to reduce bleeding."),
    ("Give a dose of aspirin by mouth now to start clot prevention early", False, "Antiplatelets are held for 24 hours after a thrombolytic, and the client needs a swallow screen first."),
    ("Keep the client NPO until a swallow screen is done", True, "Stroke clients are at risk of aspiration."),
    ("Restart the apixaban at the client's usual home dose right away", False, "Anticoagulants are not given before or right after thrombolysis.")],
   "Before thrombolysis: blood pressure below 185/110, baseline NIHSS, lines and tubes placed first, NPO until a swallow screen and no antithrombotic drugs.",
   "Prepare a client for thrombolytic therapy."),

mc(M(PPT, ANTIHTN, "take_action", "difficult", "neurological", "ph aha"),
   "The provider prescribes IV labetalol 10 mg now. Ten minutes later, the blood pressure is 178/104 mm Hg and the heart rate is 88/min. The thrombolytic is ready. What should the nurse do?",
   [("Wait 30 minutes to make sure the blood pressure stays down before starting", False,
     "Delay reduces the benefit of thrombolysis; the target has been reached."),
    ("Start the thrombolytic and check the blood pressure often", True,
     "The pressure is now below 185/110; during and after treatment it must stay below 180/105 with frequent checks."),
    ("Give a second dose of labetalol to bring the pressure below 140/90 first", False,
     "Lowering pressure too far can reduce brain perfusion."),
    ("Hold the thrombolytic, since the blood pressure was high on arrival", False,
     "Treated blood pressure that meets the target does not prevent thrombolysis.")],
   "Once blood pressure is below 185/110, start the thrombolytic and keep the pressure below 180/105 with frequent checks for 24 hours.",
   "Manage blood pressure for thrombolysis."),

matrix_mc(M(PA, STROKE, "evaluate_outcomes", "moderate", "neurological", "ms aha"),
   "Two hours after the thrombolytic infusion, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs immediate action.",
   ["Shows improvement", "Needs immediate action"],
   [("Speech clearer and right hand grip stronger", 0, ["Neurological function is improving.", "This shows improvement."]),
    ("NIHSS score down from 12 to 6", 0, ["A lower score shows improvement.", "This shows improvement."]),
    ("New severe headache and vomiting", 1, ["These may signal bleeding in the brain.", "Stop any infusion and notify the provider now."]),
    ("Blood pressure 162/94 mm Hg", 0, ["This is within the post-thrombolytic limit of 180/105.", "This is within the target."])],
   "After thrombolysis, improving deficits and a falling NIHSS show response; a new headache, vomiting or neurological decline suggests hemorrhage and needs immediate action.",
   "Evaluate the response to thrombolytic therapy."),
]

# ---- Case 2: acute pulmonary edema --------------------------------------------------------------
ape_tabs = [
    ("ape-notes", "Nurses' notes", [note("Medical unit, 0330",
        "A 72-year-old client with heart failure (ejection fraction 25%) wakes up gasping and sits on the edge of the bed. "
        "The client is anxious, sweating and coughing up frothy pink sputum. Crackles are heard throughout both lungs. "
        "The client says they ate a large ham dinner and skipped the evening furosemide dose.", id="ape-note")]),
    ("ape-vitals", "Vital signs", [flow("Vital signs", ["2200", "0330"], [
        ("Blood pressure", ["138/82 mm Hg", "186/104 mm Hg"]),
        ("Heart rate", ["84/min", "124/min"]),
        ("Respiratory rate", ["18/min", "34/min"]),
        ("SpO2", ["95% on room air", "84% on room air"])], id="ape-flow")]),
    ("ape-labs", "Laboratory results", [table("Laboratory results, 0345", ["Result", "Reference"], [
        ("BNP", ["1,860 pg/mL", "below 100 pg/mL"]),
        ("Potassium", ["4.2 mEq/L", "3.5-5.0 mEq/L"]),
        ("ABG", ["pH 7.31, PaCO2 49, PaO2 54", "pH 7.35-7.45, PaCO2 35-45, PaO2 80-100"])], id="ape-lab")]),
]

ape_qs = [
hl_table(M(RRP, LABS, "recognize_cues", "moderate", "cardiovascular", "ms lab"),
   "Review the 0345 data. Highlight the findings that require immediate action.",
   ["Assessment", "Finding"],
   [(["SpO2", "84% on room air"], True, "Severe hypoxemia needs immediate oxygen and support."),
    (["ABG", "pH 7.31, PaCO2 49, PaO2 54"], True, "Hypoxemia with rising CO2 shows respiratory failure is developing."),
    (["Potassium", "4.2 mEq/L"], False, "Normal; this matters before diuresis but is not urgent."),
    (["Sputum", "Frothy and pink"], True, "Pink frothy sputum signals fluid flooding the alveoli."),
    (["BNP", "1,860 pg/mL"], False, "Confirms heart failure but does not itself change the immediate action.")],
   "Severe hypoxemia, worsening blood gases and pink frothy sputum show acute pulmonary edema needing immediate treatment.",
   "Recognize cues of acute pulmonary edema."),

cloze(M(PA, PULM_EDEMA, "analyze_cues", "moderate", "cardiovascular", "ms"),
   "Complete the sentence based on the client's data.",
   "The client is having {{b1}}, triggered by {{b2}}.",
   [[("acute pulmonary edema from decompensated heart failure", True, "Sudden dyspnea, crackles, pink frothy sputum and hypoxemia in a client with a low ejection fraction fit pulmonary edema."),
     ("pneumonia", False, "There is no fever, and the onset is sudden with frothy sputum."),
     ("an asthma attack", False, "Crackles and pink sputum point to fluid, not bronchospasm.")],
    [("a high-sodium meal and a missed diuretic dose", True, "Sodium and fluid retention without the diuretic overwhelm the weak heart."),
     ("too much exercise during the day", False, "Nothing suggests exertion."),
     ("a new allergy to a medication", False, "Nothing suggests an allergic reaction.")]],
   "Dietary sodium and missed diuretics are common triggers of acute decompensated heart failure.",
   "Analyze the trigger of acute pulmonary edema."),

bowtie(M(PA, PULM_EDEMA, "prioritize_hypotheses", "difficult", "cardiovascular", "ms ph"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Acute pulmonary edema", True, "Gasping, crackles, pink frothy sputum and hypoxemia show fluid in the alveoli."),
    ("Pulmonary embolism", False, "PE does not cause crackles throughout the lungs with frothy sputum."),
    ("Acute asthma exacerbation", False, "The findings fit fluid overload, not bronchospasm."),
    ("Anxiety attack", False, "Anxiety does not cause hypoxemia and crackles.")],
   [("Sit the client upright and start oxygen or BiPAP as prescribed", True, "Upright positioning reduces venous return, and oxygen or BiPAP improves oxygenation."),
    ("Give IV furosemide and nitroglycerin", True, "Diuresis removes fluid and nitrates reduce preload and afterload."),
    ("Lay the client flat in bed so they can rest and conserve energy", False, "Lying flat increases venous return and worsens dyspnea."),
    ("Give a 500 mL IV fluid bolus to slow the client's fast heart rate", False, "Fluid would worsen the edema."),
    ("Encourage the client to drink a full glass of water to loosen the sputum", False, "Fluid intake adds to overload.")],
   [("Oxygen saturation and work of breathing", True, "Show whether oxygenation improves."),
    ("Urine output and blood pressure", True, "Show the response to diuretics and nitrates and guard against hypotension."),
    ("Fingerstick blood glucose before meals and at bedtime", False, "Not a key guide."),
    ("Bowel sounds in each of the four quadrants once a shift", False, "Not relevant."),
    ("Pupil size and reaction to light each shift", False, "Not relevant.")],
   "Acute pulmonary edema: upright position, oxygen or noninvasive ventilation, IV diuretics and nitrates, with close monitoring of oxygenation, urine output and blood pressure.",
   "Prioritize care for acute pulmonary edema."),

sata(M(RRP, OXY, "generate_solutions", "difficult", "respiratory", "ms"),
   "The provider prescribes noninvasive positive-pressure ventilation (BiPAP). Which actions should the nurse take?",
   [("Fit the mask snugly and check for air leaks", True, "A good seal is needed for effective pressure."),
    ("Keep the head of the bed raised", True, "Upright position helps breathing and lowers aspiration risk."),
    ("Watch for worsening drowsiness or inability to protect the airway", True, "These mean BiPAP is failing and intubation may be needed."),
    ("Give the client a full meal while the mask is on", False, "Eating with the mask on risks aspiration."),
    ("Check the skin on the bridge of the nose for pressure injury", True, "Masks can cause skin breakdown.")],
   "BiPAP care: good mask fit, upright position, close watch for failure (drowsiness, rising CO2), no eating with the mask on and skin protection.",
   "Provide care during noninvasive ventilation."),

mc(M(PPT, ANTIHTN, "take_action", "difficult", "cardiovascular", "ph"),
   "The client is started on a nitroglycerin infusion. Thirty minutes later the blood pressure is 86/50 mm Hg, and the client feels lightheaded. What should the nurse do first?",
   [("Increase the nitroglycerin rate to reduce the preload further", False,
     "A higher rate would worsen hypotension."),
    ("Stop or reduce the nitroglycerin per protocol and notify the provider", True,
     "Hypotension is a common effect of nitroglycerin; stopping or reducing it usually restores the pressure quickly."),
    ("Lay the client completely flat with the legs raised and continue the infusion as it is", False,
     "The infusion must be adjusted, and lying flat may worsen the pulmonary edema."),
    ("Give a 1 L IV fluid bolus to raise the blood pressure", False,
     "A large fluid bolus would worsen pulmonary edema.")],
   "Titrate nitroglycerin to blood pressure; with hypotension, stop or reduce it per protocol and notify the provider.",
   "Respond to hypotension during a nitroglycerin infusion."),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "moderate", "cardiovascular", "ms lab"),
   "At 0800, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("SpO2 94% on 2 L/min by nasal cannula", 0, ["Oxygenation has improved enough to stop BiPAP.", "This shows improvement."]),
    ("Urine output 1,600 mL since 0400", 0, ["Diuresis is working.", "This shows improvement."]),
    ("Crackles only at the lung bases", 0, ["Fluid in the lungs has decreased.", "This shows improvement."]),
    ("Potassium 3.1 mEq/L", 1, ["Diuresis has lowered potassium, raising dysrhythmia risk.", "This needs replacement and follow-up."])],
   "Better oxygenation, strong diuresis and fewer crackles show response; low potassium after diuresis needs replacement.",
   "Evaluate the response to treatment of pulmonary edema."),
]

cases = [
    case("Acute ischemic stroke in the thrombolytic window", stroke_tabs, stroke_qs),
    case("Acute pulmonary edema in heart failure", ape_tabs, ape_qs),
]

BUILT = build(87, "hemodynamics-critical-cases",
              "Reduction of risk and physiological adaptation, with case studies on acute ischemic stroke and acute pulmonary edema",
              items, cases, wave=True)
