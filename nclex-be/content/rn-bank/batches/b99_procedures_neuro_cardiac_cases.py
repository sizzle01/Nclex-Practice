"""Batch 99 (wave 2, chunk 8): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - a child with diabetic ketoacidosis and an ST-elevation myocardial infarction (PHI.RRP, PHI.PA, PHI.PPT).

Wave 2 keeps chunk 5's depth and uses fewer plain multiple-choice items.
Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
RENDO_DX = "renal-and-endocrine-diagnostics"
OXY = "oxygen-therapy-and-devices"
LABS = "laboratory-values-and-monitoring"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
POSTOP = "postoperative-care"
IV = "iv-therapy-and-vascular-access"
DEGEN = "degenerative-neurological-disorders"
ACID = "acid-base-imbalances"
INFL = "inflammatory-and-valvular-heart-disease"
NEUROMED = "neurological-medications"
DKA = "diabetic-ketoacidosis"
INS = "insulin-therapy"
ACS = "acute-coronary-syndrome"
POSTPROC = "post-procedure-complications"
ANTIANG = "heart-failure-interventions"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, RENDO_DX, "prioritize_hypotheses", "difficult", "renal-urinary", "ms"),
   "Four hours after a percutaneous kidney biopsy, a client reports new flank pain on the biopsy side. The urine in the collection bag has turned from pink to bright red with clots. Heart rate has risen from 82 to 118/min, and blood pressure has fallen from 136/80 to 98/60 mm Hg. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Bleeding from the biopsy site", True, "Bright red urine with clots, flank pain, tachycardia and falling blood pressure after kidney biopsy point to hemorrhage."),
    ("Urinary tract infection", False, "Infection develops over days with fever, not sudden hemodynamic change."),
    ("Kidney stone passage", False, "The timing after biopsy and the falling blood pressure point to bleeding."),
    ("Expected postprocedure hematuria", False, "Mild pink urine is expected; bright red urine with clots and shock is not.")],
   [("Notify the provider immediately", True, "Bleeding may need imaging, transfusion or embolization."),
    ("Keep the client on bed rest and prepare IV fluids and blood as prescribed", True, "Bed rest limits bleeding, and volume support treats shock."),
    ("Help the client walk to the bathroom to empty the bladder", False, "Activity can worsen bleeding."),
    ("Give ibuprofen for the flank pain", False, "NSAIDs increase bleeding risk."),
    ("Irrigate the catheter forcefully to clear the clots", False, "Forceful irrigation is not done without a prescription.")],
   [("Blood pressure, heart rate and hemoglobin", True, "Show the extent of blood loss."),
    ("Urine color and clots", True, "Shows whether bleeding is continuing."),
    ("Bowel sounds in each of the four quadrants", False, "Not a guide."),
    ("Fingerstick blood glucose before meals", False, "Not a guide."),
    ("Body weight measured once a week", False, "Not a guide.")],
   "After kidney biopsy, watch for bleeding: hematuria with clots, flank pain, tachycardia and hypotension; keep the client on bed rest and notify the provider.",
   "Recognize bleeding after kidney biopsy."),

matrix_mc(M(RRP, OXY, "analyze_cues", "difficult", "respiratory", "ms"),
   "During orientation on a busy step-down unit, a nurse is teaching new staff about the limits of pulse oximetry. For each situation, indicate how the SpO2 reading is most likely affected.",
   ["May read falsely low or fail to read", "May read falsely normal or high", "Usually accurate"],
   [("A client with very cold fingers and poor circulation", 0,
     ["Poor blood flow weakens the signal.", "Not typical.", "Reading may be unreliable."]),
    ("A client with carbon monoxide poisoning", 1,
     ["Not typical.", "Carboxyhemoglobin is read as oxygenated hemoglobin.", "The reading is misleading."]),
    ("A client with warm hands and a good waveform", 2,
     ["No reason for a low reading.", "No reason for a high reading.", "Good perfusion and waveform give a reliable reading."]),
    ("A client with dark nail polish on the probe finger", 0,
     ["Some polish colors can interfere and give low or failed readings.", "Not typical.", "May be unreliable."])],
   "Pulse oximetry is unreliable with poor perfusion, motion, some nail polish and carbon monoxide; check the waveform and correlate with the client.",
   "Recognize pulse oximetry limits."),

hl_table(M(RRP, LABS, "analyze_cues", "moderate", F, "ms lab"),
   "A client is scheduled for an elective laparoscopic cholecystectomy tomorrow. Highlight the preoperative results the nurse should report.",
   ["Test", "Result (reference)"],
   [(["Platelets", "62,000/mm3 (150,000-400,000)"], True, "Low platelets increase bleeding risk."),
    (["INR", "1.9 (0.8-1.1)"], True, "A prolonged INR increases bleeding risk."),
    (["Potassium", "3.1 mEq/L (3.5-5.0)"], True, "Low potassium raises the risk of dysrhythmias under anesthesia."),
    (["Fasting glucose", "104 mg/dL (70-110)"], False, "Normal."),
    (["Hemoglobin", "13.2 g/dL (12-16)"], False, "Normal.")],
   "Before surgery, report abnormal clotting, low platelets and electrolyte abnormalities that raise anesthesia or bleeding risk.",
   "Identify preoperative results to report."),

order(M(RRP, CV_DX, "take_action", "moderate", "cardiovascular", "fund ms"),
   "The nurse is measuring orthostatic vital signs for a client who feels dizzy when standing. Place the steps in order.",
   [("Have the client lie flat for at least 5 minutes, then measure blood pressure and pulse", "Baseline values."),
    ("Help the client sit up and measure again after 1 to 2 minutes", "Sitting values."),
    ("Help the client stand, staying close to prevent a fall", "Safety during position change."),
    ("Measure blood pressure and pulse after 1 and 3 minutes of standing", "Detects orthostatic changes.")],
   "Orthostatic vital signs: lying for 5 minutes, then sitting and standing measurements; a systolic drop of 20 or diastolic drop of 10 mm Hg is positive.",
   "Measure orthostatic vital signs."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", "musculoskeletal", "ms"),
   "A client is on the first night after a lumbar spinal fusion. Which assessments should the nurse include?",
   [("Movement, sensation and circulation in both legs", True, "Detects nerve injury or hematoma."),
    ("Surgical drain output and the dressing", True, "Detects bleeding or a CSF leak."),
    ("Ability to void after the catheter is removed", True, "Urinary retention is common after spinal surgery."),
    ("Pain level and response to analgesia", True, "Guides pain control and early movement."),
    ("Logrolling the client with the spine kept straight", True, "Protects the fusion during turning."),
    ("Bending at the waist to check the back dressing", False, "Twisting and bending stress the fusion.")],
   "After spinal fusion: neurovascular checks, drain and dressing, voiding, pain and logrolling to protect the spine.",
   "Plan assessments after spinal fusion."),

mc(M(RRP, LABS, "analyze_cues", "difficult", "renal-urinary", "ms lab"),
   "A client with vomiting for 3 days has a BUN of 48 mg/dL and a creatinine of 1.4 mg/dL (ratio about 34). Urine is dark and scant. How should the nurse interpret these results?",
   [("Intrinsic kidney damage from a toxic drug", False,
     "Intrinsic injury usually raises BUN and creatinine together, with a ratio closer to 10 to 15."),
    ("Dehydration that is reducing kidney blood flow", True,
     "A BUN-to-creatinine ratio above 20 with dark, scant urine suggests a prerenal cause such as dehydration."),
    ("Normal results for an older adult after illness", False,
     "These are abnormal."),
    ("A urinary blockage from an enlarged prostate", False,
     "Obstruction is less likely with vomiting and dark urine.")],
   "A BUN-to-creatinine ratio above 20 suggests a prerenal cause such as dehydration, GI bleeding or high protein intake.",
   "Interpret BUN and creatinine."),

mc(M(RRP, POSTPROC, "take_action", "moderate", F, "ms"),
   "A peripherally inserted central catheter (PICC) was just placed at the bedside by the IV team. The provider has prescribed IV antibiotics to start now. What should the nurse do before using the PICC?",
   [("Flush it with saline and start the antibiotic right away", False,
     "The tip position must be confirmed first."),
    ("Confirm the tip position by x-ray or per policy", True,
     "The tip must be confirmed in the correct position before the line is used."),
    ("Check that blood can be drawn back easily, then start the infusion", False,
     "Blood return does not confirm tip location."),
    ("Wait 24 hours to let the insertion site heal", False,
     "Waiting is not needed once placement is confirmed.")],
   "Confirm central line tip placement (x-ray or ECG-guided method per policy) before first use.",
   "Verify PICC placement before use."),

cloze(M(RRP, CV_DX, "take_action", "moderate", "cardiovascular", "ms"),
   "A client is going home with a 48-hour Holter monitor to find the cause of palpitations. Complete the sentences about the teaching.",
   "The client should {{b1}} while wearing the monitor, keep a diary of {{b2}} and {{b3}} until it is removed.",
   [[("carry out normal daily activities", True, "The goal is to record the heart during usual activities."),
     ("stay in bed and rest", False, "Rest would not show what triggers symptoms."),
     ("exercise as hard as possible", False, "Normal activity is the goal.")],
    [("symptoms and activities with the times they happened", True, "The diary links symptoms with the recording."),
     ("everything they eat", False, "Diet is not the focus."),
     ("their weight each hour", False, "Not useful.")],
    [("avoid showering or getting the device wet", True, "Water can damage the monitor."),
     ("remove the electrodes at night", False, "Recording should continue overnight."),
     ("unplug it when they feel well", False, "Continuous recording is needed.")]],
   "Holter monitor teaching: normal activities, a symptom diary with times and keeping the device dry.",
   "Teach Holter monitor use."),

# ---- physiological adaptation ----
bowtie(M(PA, DEGEN, "prioritize_hypotheses", "difficult", "neurological", "ms"),
   "A client with myasthenia gravis has had an upper respiratory infection for 3 days. Today the client's speech is slurred, the eyelids droop more, the client chokes on sips of water and says it is hard to breathe. Vital capacity has fallen from 2.4 L to 1.1 L. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Myasthenic crisis", True, "Worsening weakness with breathing and swallowing difficulty after an infection is myasthenic crisis."),
    ("Cholinergic crisis", False, "Cholinergic crisis follows too much medication and causes drooling, cramps and small pupils."),
    ("Guillain-Barré syndrome", False, "GBS causes ascending paralysis, not worsening of known myasthenia."),
    ("Stroke", False, "There are no focal deficits on one side.")],
   [("Prepare to support breathing, including possible intubation", True, "Respiratory failure can develop quickly."),
    ("Keep the client NPO and position upright with suction ready", True, "Swallowing weakness raises aspiration risk."),
    ("Give a large meal to keep up the client's strength", False, "Eating risks aspiration."),
    ("Encourage the client to walk to build strength", False, "Activity worsens weakness."),
    ("Give a sedative to ease anxiety about breathing", False, "Sedation can depress breathing.")],
   [("Vital capacity and negative inspiratory force", True, "Falling values signal impending respiratory failure."),
    ("Swallowing and ability to handle secretions", True, "Shows aspiration risk."),
    ("Fingerstick blood glucose before meals", False, "Not the key guide."),
    ("Bowel sounds in each of the four quadrants", False, "Not the key guide."),
    ("Body weight measured once a week", False, "Not the key guide.")],
   "Myasthenic crisis can cause respiratory failure; monitor vital capacity, protect the airway and prepare for ventilation and treatments such as IVIG or plasmapheresis.",
   "Recognize myasthenic crisis."),

matrix_mc(M(PA, ACID, "analyze_cues", "difficult", "respiratory", "ms lab"),
   "For each arterial blood gas, indicate the acid-base disorder.",
   ["Respiratory acidosis", "Respiratory alkalosis", "Metabolic acidosis", "Metabolic alkalosis"],
   [("pH 7.28, PaCO2 58, HCO3 25 in a client with an opioid overdose", 0,
     ["Low pH with high CO2.", "pH is low.", "HCO3 is normal.", "pH is low."]),
    ("pH 7.51, PaCO2 28, HCO3 23 in an anxious client breathing fast", 1,
     ["pH is high.", "High pH with low CO2.", "pH is high.", "HCO3 is normal."]),
    ("pH 7.22, PaCO2 30, HCO3 12 in a client with DKA", 2,
     ["CO2 is low.", "pH is low.", "Low pH with low HCO3; CO2 is falling to compensate.", "pH is low."]),
    ("pH 7.52, PaCO2 46, HCO3 36 in a client with prolonged vomiting", 3,
     ["pH is high.", "CO2 is not low.", "pH is high.", "High pH with high HCO3."])],
   "Match pH with the system that moved the same direction: CO2 opposite to pH means respiratory; HCO3 in the same direction as pH means metabolic.",
   "Interpret arterial blood gases."),

mc(M(PA, DEGEN, "take_action", "difficult", "neurological", "ms"),
   "A client admitted with Guillain-Barré syndrome had leg weakness 2 days ago. Today the weakness has spread to the arms and chest, and the client's voice is softer. Which assessment is the priority?",
   [("Checking the deep tendon reflexes in the legs", False,
     "Reflexes are already expected to be reduced; breathing is the priority."),
    ("Measuring vital capacity and breathing effort", True,
     "Ascending weakness can reach the breathing muscles; falling vital capacity signals the need for ventilation."),
    ("Testing the client's ability to walk in the hall", False,
     "Walking is unsafe and not the priority."),
    ("Measuring the client's calf circumference on both legs", False,
     "DVT risk matters but is not the priority.")],
   "Guillain-Barré syndrome can cause respiratory failure as weakness ascends; monitor vital capacity and be ready for ventilation.",
   "Prioritize breathing in Guillain-Barré syndrome."),

mc(M(PA, INFL, "recognize_cues", "difficult", "cardiovascular", "ms"),
   "A 63-year-old client 3 days after a myocardial infarction treated with a stent reports sharp chest pain that gets worse when lying flat or breathing deeply and eases when leaning forward. The nurse hears a scratchy sound over the heart. What should the nurse suspect?",
   [("A new myocardial infarction", False,
     "Pain from ischemia does not usually change with position."),
    ("Pericarditis after the infarction", True,
     "Positional, pleuritic chest pain eased by leaning forward with a friction rub suggests pericarditis."),
    ("A pulmonary embolism", False,
     "PE causes pleuritic pain and hypoxemia, but not a rub that changes with position."),
    ("Gastroesophageal reflux", False,
     "Reflux does not cause a friction rub.")],
   "Pericarditis: sharp, positional chest pain relieved by leaning forward and a pericardial friction rub; watch for tamponade.",
   "Recognize pericarditis."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, NEUROMED, "evaluate_outcomes", "moderate", "neurological", "ph"),
   "A client with myasthenia gravis takes pyridostigmine. Which statement shows correct understanding of when to take it?",
   [("\"I'll take it at bedtime so it can work while I'm asleep.\"", False,
     "Doses are timed to cover activities and meals."),
    ("\"I'll take it 30 to 60 minutes before each of my meals.\"", True,
     "Timing doses before meals improves chewing and swallowing."),
    ("\"I'll take it when I feel very weak and tired during the day.\"", False,
     "Doses are taken on schedule."),
    ("\"I'll take extra doses if I feel stronger so I stay that way.\"", False,
     "Extra doses can cause cholinergic crisis.")],
   "Pyridostigmine is taken on schedule, about 30 to 60 minutes before meals, without extra doses.",
   "Teach pyridostigmine timing."),
]

# ---- Case 1: child with diabetic ketoacidosis --------------------------------------------------
dka_tabs = [
    ("dka-notes", "Nurses' notes", [note("Pediatric emergency department, 0900",
        "A 9-year-old with no known history of diabetes has had increased thirst, frequent urination and weight loss for "
        "3 weeks. Since last night the child has vomited three times and has abdominal pain. The child is drowsy but "
        "answers questions, breathing is deep and fast and the breath smells fruity.", id="dka-note")]),
    ("dka-vitals", "Vital signs", [flow("Vital signs", ["0900"], [
        ("Heart rate", ["134/min"]),
        ("Respiratory rate", ["32/min, deep"]),
        ("Blood pressure", ["96/58 mm Hg"]),
        ("Capillary refill", ["3 seconds"])], id="dka-flow")]),
    ("dka-labs", "Laboratory results", [table("Laboratory results, 0930", ["Result", "Reference"], [
        ("Glucose", ["520 mg/dL", "70-110 mg/dL"]),
        ("pH", ["7.12", "7.35-7.45"]),
        ("Bicarbonate", ["8 mEq/L", "22-26 mEq/L"]),
        ("Potassium", ["5.4 mEq/L", "3.5-5.0 mEq/L"]),
        ("Beta-hydroxybutyrate", ["6.2 mmol/L", "below 0.6 mmol/L"])], id="dka-lab")]),
]

dka_qs = [
hl_text(M(PA, DKA, "recognize_cues", "moderate", "pediatrics", "peds ada"),
   "Review the nurse's note and results. Highlight the findings that require immediate follow-up.",
   ["The child is ",
    ("drowsy", True, "Changed alertness in DKA can signal worsening acidosis or cerebral edema."),
    " with ",
    ("deep, fast breathing and fruity breath", True, "Kussmaul breathing and ketone odor show severe ketoacidosis."),
    ". ",
    ("Weight loss over 3 weeks", False, "Explains the new diagnosis but is not an immediate threat."),
    ". The pH is ",
    ("7.12", True, "Severe acidosis."),
    ", and capillary refill is ",
    ("3 seconds", True, "Delayed refill suggests dehydration."),
    "."],
   "Drowsiness, Kussmaul breathing, severe acidosis and poor perfusion show severe DKA in a child.",
   "Recognize severe pediatric DKA."),

cloze(M(PA, DKA, "analyze_cues", "moderate", "pediatrics", "peds ada"),
   "Complete the sentence based on the child's data.",
   "The child has new-onset type 1 diabetes presenting as {{b1}}, and the high potassium is most likely {{b2}}.",
   [[("severe diabetic ketoacidosis", True, "High glucose, pH 7.12, low bicarbonate and high ketones show severe DKA."),
     ("hyperosmolar hyperglycemic state", False, "HHS lacks significant acidosis and ketones."),
     ("gastroenteritis", False, "The glucose and acidosis point to DKA.")],
    [("a shift of potassium out of cells, hiding a total-body deficit", True, "Acidosis and lack of insulin move potassium out of cells; total-body potassium is low."),
     ("a sign of too much potassium in the diet", False, "Diet is not the cause."),
     ("a reason to restrict potassium during treatment", False, "Potassium usually falls quickly with insulin and must be replaced.")]],
   "In DKA, serum potassium may be high despite a total-body deficit; it falls with insulin and fluids and needs close monitoring and replacement.",
   "Analyze cues in pediatric DKA."),

matrix_mc(M(RRP, LABS, "prioritize_hypotheses", "difficult", "pediatrics", "peds ada"),
   "For each complication of treatment, indicate whether the child is at high or low risk.",
   ["High risk", "Low risk"],
   [("Cerebral edema", 0, ["Children with severe DKA, especially new-onset, are at highest risk.", "The risk is high."]),
    ("Hypokalemia as insulin starts", 0, ["Insulin moves potassium into cells, so levels can drop quickly.", "The risk is high."]),
    ("Hypoglycemia if dextrose is not added as glucose falls", 0, ["Insulin continues while glucose falls.", "The risk is high."]),
    ("Hypertensive crisis", 1, ["The child is dehydrated with low-normal blood pressure.", "The risk is low."])],
   "Pediatric DKA treatment risks include cerebral edema, hypokalemia and hypoglycemia.",
   "Prioritize risks during pediatric DKA treatment."),

sata(M(RRP, LABS, "generate_solutions", "difficult", "pediatrics", "peds ada"),
   "Which monitoring should the nurse include during the first hours of treatment?",
   [("Glucose every hour", True, "Guides insulin and dextrose."),
    ("Neurological checks every hour, watching for headache or a falling level of consciousness", True, "Early signs of cerebral edema."),
    ("Electrolytes, including potassium, every 2 to 4 hours", True, "Potassium shifts quickly."),
    ("Strict intake and output", True, "Guides fluid replacement."),
    ("Body weight measured once each week", False, "Not frequent enough."),
    ("Oral fluids as much as the child wants while vomiting", False, "The child is vomiting; fluids are given IV.")],
   "Pediatric DKA monitoring: hourly glucose and neurological checks, frequent electrolytes and strict intake and output.",
   "Plan monitoring in pediatric DKA."),

mc(M(PPT, INS, "take_action", "difficult", "pediatrics", "peds ph"),
   "Which plan for insulin should the nurse expect for this child?",
   [("A large IV insulin bolus followed by an infusion", False,
     "Insulin boluses are avoided in children because of cerebral edema risk."),
    ("A low-dose IV insulin infusion after fluids, with no bolus", True,
     "Pediatric DKA uses a continuous infusion (about 0.05 to 0.1 units/kg/hr) after fluids begin, with no bolus."),
    ("Subcutaneous long-acting insulin given once a day", False,
     "IV insulin is needed for severe DKA."),
    ("Insulin by mouth with small sips of clear fluid", False,
     "Insulin is not given orally.")],
   "In pediatric DKA, start a low-dose continuous insulin infusion after initial fluid replacement, without a bolus, and correct slowly.",
   "Plan insulin therapy in pediatric DKA."),

matrix_mc(M(PA, DKA, "evaluate_outcomes", "moderate", "pediatrics", "peds ada"),
   "Six hours into treatment, the nurse reassesses the child. For each finding, indicate whether it shows improvement or needs immediate action.",
   ["Shows improvement", "Needs immediate action"],
   [("Glucose 260 mg/dL and falling about 70 mg/dL per hour", 0, ["A steady fall is the goal.", "This shows improvement."]),
    ("pH 7.24 and bicarbonate 13 mEq/L", 0, ["Acidosis is improving.", "This shows improvement."]),
    ("New severe headache, slowing heart rate and confusion", 1, ["These suggest cerebral edema.", "This needs immediate action."]),
    ("Breathing slower and less deep", 0, ["Kussmaul breathing eases as acidosis improves.", "This shows improvement."])],
   "Steady improvement in glucose, pH and breathing shows response; headache, bradycardia and confusion suggest cerebral edema and need immediate action.",
   "Evaluate the response to pediatric DKA treatment."),
]

# ---- Case 2: ST-elevation myocardial infarction ------------------------------------------------
mi_tabs = [
    ("mi-notes", "Nurses' notes", [note("Emergency department, 0615",
        "A 61-year-old client has had crushing chest pain spreading to the jaw for 40 minutes, with nausea and sweating. "
        "History: hypertension, smoking and erectile dysfunction treated with sildenafil, last taken last night. "
        "The 12-lead ECG shows ST elevation in leads II, III and aVF.", id="mi-note")]),
    ("mi-vitals", "Vital signs", [flow("Vital signs", ["0615"], [
        ("Blood pressure", ["98/62 mm Hg"]),
        ("Heart rate", ["54/min"]),
        ("SpO2", ["95% on room air"])], id="mi-flow")]),
]

mi_qs = [
hl_table(M(RRP, CV_DX, "recognize_cues", "moderate", "cardiovascular", "aha ms"),
   "Review the data. Highlight the findings that require immediate action.",
   ["Assessment", "Finding"],
   [(["ECG", "ST elevation in II, III and aVF"], True, "ST elevation shows an inferior STEMI."),
    (["Heart rate", "54/min"], True, "Bradycardia is common with inferior MI and may need treatment."),
    (["Blood pressure", "98/62 mm Hg"], True, "Low blood pressure affects drug choices and suggests possible right ventricular involvement."),
    (["SpO2", "95% on room air"], False, "Oxygen is not needed at this saturation."),
    (["Medication history", "Sildenafil last night"], True, "Nitrates are contraindicated after recent sildenafil.")],
   "Inferior STEMI with bradycardia, low blood pressure and recent sildenafil use changes immediate treatment choices.",
   "Recognize cues in an inferior STEMI."),

cloze(M(PA, ACS, "analyze_cues", "moderate", "cardiovascular", "aha ms"),
   "Complete the sentence based on the client's data.",
   "The client is having an {{b1}}, which most often involves blockage of the {{b2}}.",
   [[("inferior ST-elevation myocardial infarction", True, "ST elevation in II, III and aVF shows an inferior STEMI."),
     ("anterior STEMI", False, "Anterior STEMI shows changes in V1 to V4."),
     ("unstable angina", False, "ST elevation indicates infarction.")],
    [("right coronary artery", True, "The right coronary artery usually supplies the inferior wall and the SA and AV nodes."),
     ("left anterior descending artery", False, "The LAD supplies the anterior wall."),
     ("pulmonary artery", False, "Not a coronary artery.")]],
   "Inferior STEMI usually involves the right coronary artery and can cause bradycardia and right ventricular infarction.",
   "Analyze an inferior STEMI."),

bowtie(M(PA, ACS, "prioritize_hypotheses", "difficult", "cardiovascular", "aha ph"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Inferior STEMI with possible right ventricular involvement", True, "ST elevation in the inferior leads with hypotension points to inferior STEMI that may involve the right ventricle."),
    ("Stable angina", False, "Pain lasting 40 minutes with ST elevation is infarction."),
    ("Aortic dissection", False, "Tearing pain to the back and unequal pulses would suggest dissection."),
    ("Pericarditis", False, "Pericarditis causes diffuse ST changes and positional pain.")],
   [("Activate the cath lab team for emergency reperfusion", True, "STEMI needs reperfusion as soon as possible."),
    ("Give aspirin to chew as prescribed", True, "Aspirin reduces clot growth."),
    ("Give sublingual nitroglycerin for the chest pain", False, "Nitrates are contraindicated after sildenafil and with hypotension."),
    ("Give a large dose of furosemide", False, "Diuresis would worsen hypotension, especially with right ventricular involvement."),
    ("Wait for troponin results before calling the cath lab", False, "STEMI treatment does not wait for troponin.")],
   [("Heart rhythm and blood pressure", True, "Bradycardia, heart block and hypotension are common."),
    ("Chest pain level", True, "Shows ongoing ischemia."),
    ("Fingerstick blood glucose before meals", False, "Not the key guide."),
    ("Bowel sounds in each of the four quadrants", False, "Not the key guide."),
    ("Body weight measured once a week", False, "Not the key guide.")],
   "STEMI: rapid reperfusion and aspirin; avoid nitrates after PDE-5 inhibitors and with hypotension or right ventricular infarction; monitor rhythm, pressure and pain.",
   "Prioritize care in an inferior STEMI."),

sata(M(RRP, POSTPROC, "take_action", "moderate", "cardiovascular", "ms"),
   "After a stent is placed through the right femoral artery, which actions should the nurse take?",
   [("Check the groin site for bleeding or a hematoma", True, "Bleeding is the most common complication."),
    ("Check pulses, color and temperature of the right leg", True, "Detects arterial blockage."),
    ("Keep the right leg straight for the prescribed time", True, "Prevents bleeding."),
    ("Raise the head of the bed to 90 degrees right away", False, "Bending the hip can cause bleeding."),
    ("Watch the telemetry for rhythm changes", True, "Reperfusion can cause dysrhythmias."),
    ("Have the client walk within the first hour", False, "Early walking risks bleeding.")],
   "After femoral PCI: check the site, distal pulses and the leg, keep the leg straight and monitor rhythm.",
   "Care for a client after femoral PCI."),

mc(M(PPT, ANTIANG, "take_action", "difficult", "cardiovascular", "ph aha"),
   "The client still has chest pain while waiting for the cath lab. The provider is considering pain relief. Which statement by the nurse is most important to share?",
   [("\"The client is anxious and asking for something to help them calm down.\"", False,
     "Useful, but not the key safety issue."),
    ("\"The client took sildenafil last night; BP is 98/62.\"", True,
     "Nitrates with recent sildenafil and low blood pressure can cause severe hypotension."),
    ("\"The client would like ice chips for a dry mouth.\"", False,
     "Not the key safety issue."),
    ("\"The client's spouse is on the way to the hospital.\"", False,
     "Not the key safety issue.")],
   "Before nitrates, check for PDE-5 inhibitor use (sildenafil within 24 hours, tadalafil within 48 hours), hypotension and right ventricular infarction.",
   "Communicate a nitrate contraindication."),

matrix_mc(M(RRP, CV_DX, "evaluate_outcomes", "moderate", "cardiovascular", "ms"),
   "Two hours after the stent is placed, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Chest pain 0 of 10", 0, ["Ischemia has resolved.", "This shows improvement."]),
    ("ST segments returning toward baseline", 0, ["Reperfusion is working.", "This shows improvement."]),
    ("A growing, firm swelling at the groin site", 1, ["This suggests a hematoma from bleeding.", "This needs follow-up."]),
    ("Heart rate 72/min in sinus rhythm", 0, ["Rhythm has improved.", "This shows improvement."])],
   "After PCI, resolving pain and ST changes show reperfusion; a growing groin swelling suggests bleeding.",
   "Evaluate outcomes after PCI."),
]

cases = [
    case("Diabetic ketoacidosis in a child with new-onset diabetes", dka_tabs, dka_qs),
    case("Inferior ST-elevation myocardial infarction", mi_tabs, mi_qs),
]

BUILT = build(99, "procedures-neuro-cardiac-cases",
              "Reduction of risk and physiological adaptation, with case studies on pediatric DKA and inferior STEMI",
              items, cases, wave=True)
