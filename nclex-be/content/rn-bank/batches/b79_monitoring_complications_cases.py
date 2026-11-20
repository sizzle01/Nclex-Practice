"""Batch 79 (wave 1, chunk 3): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - acute pancreatitis and pulmonary embolism after surgery (PHI.RRP, PHI.PA, PHI.PPT).

Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
CONTRAST = "monitoring-after-contrast-dye"
HYPO = "recognizing-hypoglycemia"
LYTE_LAB = "electrolyte-and-glucose-lab-results"
FHR = "fetal-heart-rate-monitoring"
PRENATAL = "prenatal-testing"
JOINT = "joint-replacement-care"
FX = "fracture-complications"
SPEC = "specimen-collection"
PREOP = "preoperative-care"
LABS = "laboratory-values-and-monitoring"
HTN_CRISIS = "hypertensive-crisis"
MENING = "meningitis-and-encephalitis"
HF_ASSESS = "heart-failure-assessment"
PULM_EDEMA = "acute-pulmonary-edema"
HF_INT = "heart-failure-interventions"
PANC = "pancreas-and-gallbladder-disorders"
OPI = "opioid-safety"
VTE = "venous-thromboembolism"
ANTICOAG = "anticoagulant-therapy-and-monitoring"
POSTOP = "postoperative-care"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, CONTRAST, "prioritize_hypotheses", "difficult", "renal-urinary", "ms ph"),
   "A 71-year-old client with type 2 diabetes, heart failure and a baseline creatinine of 1.4 mg/dL had a CT scan of the abdomen with IV contrast 48 hours ago. The client's metformin was continued. Today the creatinine is 2.6 mg/dL, urine output has been 15 to 20 mL/hr for 6 hours, and the client is nauseated and short of breath with a respiratory rate of 26/min. Venous blood gas shows pH 7.24 and lactate 5.8 mmol/L. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Contrast-associated acute kidney injury with metformin-associated lactic acidosis", True, "A rise in creatinine within 48 hours of contrast, low urine output and lactic acidosis in a client still taking metformin fit this picture."),
    ("Diabetic ketoacidosis", False, "DKA causes marked hyperglycemia and ketones; the high lactate and timing after contrast point elsewhere."),
    ("Delayed allergic reaction to the contrast dye", False, "Allergic reactions cause hives, itching or wheezing, not a rising creatinine and lactic acidosis."),
    ("Acute exacerbation of heart failure alone", False, "Shortness of breath may be fluid, but heart failure alone does not explain the acidosis and timing.")],
   [("Hold the metformin and notify the provider of the results", True, "Metformin builds up when the kidneys fail and worsens lactic acidosis."),
    ("Prepare for treatment of the acidosis, which may include dialysis", True, "Severe metformin-associated lactic acidosis can need dialysis to remove the drug and correct the acidosis."),
    ("Give a large IV fluid bolus quickly to flush the kidneys", False, "The client has heart failure and is short of breath; fluids must be given with care as prescribed."),
    ("Give a dose of oral ibuprofen for the client's discomfort", False, "NSAIDs further reduce kidney blood flow."),
    ("Schedule a repeat CT with contrast to look for the cause", False, "More contrast would worsen the kidney injury.")],
   [("Serum creatinine and urine output", True, "Show whether kidney function is recovering or worsening."),
    ("Arterial pH and lactate", True, "Show whether the acidosis is responding to treatment."),
    ("Blood pressure in both arms", False, "Not a key guide to this condition."),
    ("Pupil size and reaction", False, "Not related to kidney injury or acidosis."),
    ("Skin turgor on the forearm", False, "Skin turgor is unreliable in older adults and does not guide this treatment.")],
   "Contrast can injure the kidneys within 48 to 72 hours, especially with diabetes, CKD and heart failure. Metformin is held around contrast in at-risk clients because kidney failure can cause lactic acidosis.",
   "Recognize kidney injury and lactic acidosis after contrast."),

mc(M(RRP, HYPO, "recognize_cues", "moderate", "endocrine", "ms ada"),
   "A client with type 2 diabetes who received insulin glargine last night has been NPO since midnight for surgery that has been delayed until afternoon. At 1100 the client is sweaty, shaky and says, \"I feel strange and can't think straight.\" What should the nurse do first?",
   [("Check the client's fingerstick blood glucose right away", True,
     "Sweating, shakiness and confusion in a fasting client on insulin suggest hypoglycemia; confirming it guides treatment."),
    ("Give the client a cup of orange juice, since the surgery is delayed anyway", False,
     "Oral intake may be needed, but the client is NPO for surgery and the glucose should be checked first."),
    ("Tell the client the feelings are probably anxiety about the delayed surgery", False,
     "Assuming anxiety can miss dangerous hypoglycemia."),
    ("Notify the surgeon that the client is too unwell to have surgery today", False,
     "The client needs assessment and possible treatment before decisions about surgery.")],
   "Sweating, tremor, confusion and hunger in a client on insulin who is fasting suggest hypoglycemia; check glucose and treat by the protocol for NPO clients.",
   "Recognize hypoglycemia in a fasting client."),

mc(M(RRP, LYTE_LAB, "analyze_cues", "difficult", "cardiovascular", "ms lab"),
   "A client with alcohol use disorder is on telemetry. Morning labs show magnesium 1.1 mg/dL and potassium 3.2 mEq/L. The telemetry strip shows a prolonged QT interval of 0.52 seconds. Which complication is the client at greatest risk for?",
   [("Third-degree heart block with a slow junctional escape rhythm", False,
     "Low magnesium and potassium do not typically cause complete heart block."),
    ("Torsades de pointes, a form of polymorphic ventricular tachycardia", True,
     "Low magnesium and potassium with a long QT interval greatly raise the risk of torsades de pointes."),
    ("Atrial flutter with a regular rate of about 150 beats per minute", False,
     "Electrolyte problems can cause atrial dysrhythmias, but a long QT points to torsades."),
    ("Sinus bradycardia caused by increased vagal tone while the client is asleep at night", False,
     "Bradycardia is not the main risk of these findings.")],
   "Hypomagnesemia and hypokalemia prolong the QT interval and can cause torsades de pointes, which is treated with IV magnesium.",
   "Recognize the risk of torsades de pointes."),

mc(M(RRP, FHR, "take_action", "difficult", "maternal-newborn", "ob"),
   "A client in active labor at 6 cm has a fetal heart rate baseline of 140/min with moderate variability. The nurse sees abrupt, V-shaped drops in the fetal heart rate to 90/min that vary in timing with contractions and return quickly to baseline. What should the nurse do first?",
   [("Increase the oxytocin infusion to speed the labor along", False,
     "Oxytocin increases contraction strength and can worsen cord compression."),
    ("Change the client's position, such as to the side or hands and knees", True,
     "These are variable decelerations from cord compression; changing position often relieves the pressure on the cord."),
    ("Prepare the client for an emergency cesarean birth right away", False,
     "Variable decelerations with moderate variability are first managed with position changes."),
    ("Document the pattern as expected for active labor and keep monitoring", False,
     "Variable decelerations need intervention, even if the variability is reassuring.")],
   "Variable decelerations come from cord compression; reposition the client, stop oxytocin if it is running and decelerations persist, and notify the provider.",
   "Respond to variable decelerations."),

mc(M(RRP, PRENATAL, "analyze_cues", "moderate", "maternal-newborn", "ob"),
   "A client at 36 weeks with gestational hypertension has a nonstress test. Over 20 minutes, the fetal heart rate rises at least 15 beats/min above the baseline for at least 15 seconds three times, each with fetal movement. How should the nurse interpret this result?",
   [("Nonreactive; the fetus may be compromised and needs a biophysical profile", False,
     "These accelerations meet the criteria for a reactive test."),
    ("Reactive; the fetus is likely well oxygenated at this time", True,
     "Two or more accelerations of at least 15 beats/min for 15 seconds in 20 minutes after 32 weeks is a reactive, reassuring result."),
    ("Positive; contractions are causing late decelerations in the fetal heart rate", False,
     "A positive result describes a contraction stress test with late decelerations."),
    ("Inconclusive; the test needs to be repeated after the client eats a meal", False,
     "The test met its criteria and is not inconclusive.")],
   "A reactive nonstress test (accelerations of at least 15 beats/min for 15 seconds, twice in 20 minutes after 32 weeks) is reassuring.",
   "Interpret a nonstress test."),

mc(M(RRP, JOINT, "take_action", "moderate", "musculoskeletal", "ms"),
   "A client had a total knee replacement yesterday. The client's pain is 8 of 10, and the client asks to skip physical therapy today. What should the nurse do?",
   [("Agree that the client can skip therapy until the pain is below 3 of 10", False,
     "Delaying mobility increases stiffness, clots and complications."),
    ("Give the prescribed analgesic about 30 minutes before therapy and encourage the session", True,
     "Pain control before therapy makes exercise possible; early motion is vital to recovery after knee replacement."),
    ("Tell the client that pain after knee surgery is normal and therapy cannot be skipped", False,
     "This dismisses the client's pain without treating it."),
    ("Keep the knee resting on a pillow so that it stays bent and comfortable for the whole day", False,
     "A pillow under the knee promotes a flexion contracture.")],
   "After joint replacement, time analgesia before therapy and promote early mobility; avoid pillows under the knee.",
   "Support mobility after knee replacement."),

mc(M(RRP, FX, "recognize_cues", "difficult", "musculoskeletal", "ms"),
   "A 22-year-old client with a closed femur fracture from a motorcycle crash is in skeletal traction. Thirty-six hours after the injury, the client becomes restless and confused, the respiratory rate is 32/min, SpO2 is 86% and the nurse sees tiny red-purple spots across the chest and in the armpits. What should the nurse suspect?",
   [("Alcohol withdrawal starting 36 hours after the client's last drink", False,
     "Withdrawal can cause confusion and restlessness, but not hypoxemia with a petechial rash."),
    ("Fat embolism syndrome", True,
     "Respiratory distress, confusion and petechiae on the chest and axillae 24 to 72 hours after a long-bone fracture are classic fat embolism."),
    ("A reaction to the opioid that is being given for pain", False,
     "Opioid effects slow breathing and do not cause petechiae."),
    ("Compartment syndrome of the injured thigh", False,
     "Compartment syndrome causes limb pain and changes, not breathing problems and a rash on the chest.")],
   "Fat embolism syndrome follows long-bone fractures within 24 to 72 hours: hypoxemia, confusion and petechiae. Give oxygen, support breathing and notify the provider.",
   "Recognize fat embolism syndrome."),

sata(M(RRP, SPEC, "take_action", "moderate", "renal-urinary", "fund lab"),
   "The nurse is starting a 24-hour urine collection for creatinine clearance at 0800. Which actions are correct?",
   [("Have the client void at 0800, discard that urine and record the start time", True, "The test starts with an empty bladder, so the first void is discarded."),
    ("Keep the collection container on ice or refrigerated as the lab directs", True, "Cold storage prevents breakdown of substances in the urine."),
    ("Save the first void at 0800 to start the collection", False, "Urine made before the start time would add to the total and give a wrong result."),
    ("Ask the client to void at 0800 the next day and add that urine to the container", True, "The final void completes the 24 hours."),
    ("Restart the collection if one void is thrown away by mistake", True, "A missed sample makes the test inaccurate."),
    ("Collect only the urine passed during the day while the client is awake", False, "The test needs all urine over 24 hours.")],
   "24-hour urine: discard the first void, collect all urine for 24 hours, keep it cold, include the final void and restart if any is lost.",
   "Collect a 24-hour urine specimen."),

mc(M(RRP, PREOP, "take_action", "moderate", F, "ms ph"),
   "During a preoperative interview for an elective hip replacement in 1 week, a client says they take ginkgo biloba, garlic capsules and fish oil every day. What should the nurse do?",
   [("Tell the client that natural supplements are safe and can be continued", False,
     "Several supplements affect bleeding and anesthesia."),
    ("Report the supplements to the surgeon, since they can increase bleeding risk", True,
     "Ginkgo, garlic and fish oil can increase bleeding; they are often stopped before surgery as directed."),
    ("Tell the client to double the fish oil before surgery to protect the heart", False,
     "Extra fish oil would increase bleeding risk."),
    ("Write the supplements down but leave them out of the client's medication list", False,
     "Supplements belong on the medication list because they interact with drugs and surgery.")],
   "Ask about herbal supplements before surgery; ginkgo, garlic, ginseng, fish oil and vitamin E can increase bleeding and are reported to the surgeon.",
   "Identify supplements that affect surgery."),

# ---- physiological adaptation ----
bowtie(M(PA, HTN_CRISIS, "prioritize_hypotheses", "difficult", "cardiovascular", "ms ph"),
   "A 56-year-old client who ran out of clonidine 3 days ago comes to the emergency department with a severe headache, blurred vision and confusion. Blood pressure is 232/134 mm Hg in both arms, and the client vomited twice. There is no weakness or facial droop, and a CT of the head shows no bleeding. Urine shows protein and blood. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Hypertensive emergency with encephalopathy", True, "Severe hypertension with headache, confusion, vision changes and signs of kidney injury is a hypertensive emergency; stopping clonidine causes rebound hypertension."),
    ("Hypertensive urgency without organ damage", False, "Confusion, vision changes and protein in the urine show organ damage, which makes this an emergency."),
    ("Acute ischemic stroke", False, "There are no focal deficits, and the CT is clear; the confusion is from hypertensive encephalopathy."),
    ("Migraine with aura", False, "Migraine does not cause a blood pressure of 232/134 with kidney changes.")],
   [("Start a titrated IV antihypertensive such as nicardipine as prescribed", True, "IV drugs allow controlled, steady lowering of blood pressure."),
    ("Lower the mean arterial pressure by no more than about 25% in the first hour", True, "Lowering pressure too fast can cause stroke or kidney and heart ischemia."),
    ("Give sublingual nifedipine to bring the pressure down to normal quickly", False, "Rapid, unpredictable drops can cause stroke and are avoided."),
    ("Wait for the blood pressure to settle on its own after the client rests", False, "Organ damage is happening now; treatment must start at once."),
    ("Give the missed clonidine doses all at once to catch up", False, "Large catch-up doses are unsafe and do not provide controlled lowering.")],
   [("Blood pressure every 5 to 15 minutes, by arterial line if available", True, "Close monitoring prevents both overshoot and undertreatment."),
    ("Neurological status, including level of consciousness and vision", True, "Shows whether the encephalopathy is improving or a stroke is developing."),
    ("Oral temperature checked every 4 hours with the routine vital signs", False, "Not a key parameter for hypertensive emergency."),
    ("Bowel sounds listened for in each of the four abdominal quadrants", False, "Not related to the emergency."),
    ("Body weight measured on the same scale each morning before breakfast", False, "Daily weight does not guide acute blood pressure treatment.")],
   "Hypertensive emergency means severe hypertension with organ damage. Lower the pressure in a controlled way with titrated IV drugs (about 25% in the first hour) and monitor blood pressure and neurological status.",
   "Recognize and manage a hypertensive emergency."),

mc(M(PA, MENING, "take_action", "difficult", "neurological", "ms cdc"),
   "A 19-year-old college student is admitted with fever, severe headache, a stiff neck and a purple rash on the legs that does not blanch. Bacterial meningitis is suspected. What should the nurse do first?",
   [("Wait to start antibiotics until the lumbar puncture results confirm the organism", False,
     "Delaying antibiotics in suspected bacterial meningitis increases death and disability."),
    ("Place the client on droplet precautions and prepare to give IV antibiotics right away", True,
     "A petechial rash suggests meningococcal disease; droplet precautions protect others and antibiotics are given without delay."),
    ("Put the client in a brightly lit room so the nurse can watch the rash closely", False,
     "Clients with meningitis are often light-sensitive; a dim, quiet room is better."),
    ("Ask the client's roommates to visit so they can help keep the client calm and oriented", False,
     "Close contacts need assessment for prophylaxis, not visits during the infectious period.")],
   "Suspected bacterial meningitis with a petechial rash: droplet precautions, immediate IV antibiotics, seizure precautions and prophylaxis for close contacts.",
   "Start care for suspected meningococcal meningitis."),

matrix_mc(M(PA, HF_ASSESS, "analyze_cues", "moderate", "cardiovascular", "ms"),
   "The nurse is assessing clients with heart failure. For each finding, indicate whether it is more typical of left-sided or right-sided heart failure.",
   ["Left-sided", "Right-sided"],
   [("Crackles in both lung bases", 0,
     ["Fluid backs up into the lungs when the left ventricle fails.", "Right-sided failure causes systemic, not lung, congestion."]),
    ("Swelling of the ankles and legs", 1,
     ["Leg swelling is mainly from systemic venous congestion.", "Right-sided failure backs blood up into the body's veins."]),
    ("Needing three pillows to breathe at night", 0,
     ["Orthopnea comes from fluid in the lungs.", "Orthopnea is a sign of pulmonary congestion."]),
    ("Distended neck veins with the head raised 45 degrees", 1,
     ["Jugular distention reflects high right-sided pressure.", "Right-sided failure raises venous pressure."]),
    ("Enlarged, tender liver and loss of appetite", 1,
     ["These come from venous congestion of the liver and gut.", "Right-sided failure congests the abdominal organs."])],
   "Left-sided failure affects the lungs (crackles, dyspnea, orthopnea); right-sided failure affects the body (edema, distended neck veins, hepatomegaly).",
   "Distinguish left- from right-sided heart failure."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, HF_INT, "take_action", "moderate", "cardiovascular", "ph"),
   "A client with acute pulmonary edema has a prescription for furosemide 80 mg IV. Which action is most important when the nurse gives this dose?",
   [("Push the full dose rapidly over 10 seconds so it works faster", False,
     "Rapid IV furosemide can cause hearing loss; it is given slowly, about 20 mg/min or less."),
    ("Give it slowly, about 20 mg per minute or less, and check the potassium", True,
     "A slow rate protects hearing, and furosemide causes potassium loss that must be monitored."),
    ("Mix the dose into a liter of IV fluid and run it over 8 hours", False,
     "This would deliver extra fluid and delay the diuretic effect the client needs now."),
    ("Hold the dose if the client's blood pressure is above 140/90 mm Hg on the next check", False,
     "High blood pressure is not a reason to hold furosemide in pulmonary edema.")],
   "IV furosemide: give slowly to prevent ototoxicity, monitor potassium, blood pressure, urine output and weight.",
   "Give IV furosemide safely."),
]

# ---- Case 1: acute pancreatitis -----------------------------------------------------------------
panc_tabs = [
    ("panc-notes", "Nurses' notes", [note("Medical unit, 0700",
        "A 47-year-old client was admitted last night with severe epigastric pain that goes through to the back, after "
        "a weekend of heavy drinking. The client is lying curled on the side and has vomited three times. The abdomen is "
        "tender with guarding and decreased bowel sounds. The nurse notes a bluish discoloration around the umbilicus.", id="panc-note")]),
    ("panc-vitals", "Vital signs", [flow("Vital signs", ["2300", "0700"], [
        ("Temperature", ["37.9 °C (100.2 °F)", "38.3 °C (100.9 °F)"]),
        ("Heart rate", ["108/min", "124/min"]),
        ("Blood pressure", ["118/72 mm Hg", "96/58 mm Hg"]),
        ("Respiratory rate", ["22/min", "28/min"]),
        ("Urine output", ["50 mL/hr", "20 mL/hr"])], id="panc-flow")]),
    ("panc-labs", "Laboratory results", [table("Laboratory results, 0600", ["Result", "Reference"], [
        ("Lipase", ["1,840 units/L", "0-160 units/L"]),
        ("Calcium", ["7.4 mg/dL", "8.6-10.2 mg/dL"]),
        ("Glucose", ["228 mg/dL", "70-110 mg/dL"]),
        ("WBC", ["17,200/mm3", "5,000-10,000/mm3"]),
        ("Hematocrit", ["49%", "37-47%"])], id="panc-lab")]),
]

panc_qs = [
hl_text(M(PA, PANC, "recognize_cues", "moderate", "gastrointestinal", "ms"),
   "Review the 0700 findings. Highlight the findings that require immediate follow-up.",
   ["The client has ",
    ("bluish discoloration around the umbilicus", True, "Cullen's sign suggests bleeding into the abdomen in severe pancreatitis."),
    ". ",
    ("Lying curled on the side", False, "A position of comfort that is common in pancreatitis."),
    ". Blood pressure ",
    ("96/58 mm Hg", True, "Falling blood pressure suggests fluid loss into the abdomen."),
    ", heart rate ",
    ("124/min", True, "Rising heart rate is compensation for hypovolemia."),
    ". Urine output ",
    ("20 mL/hr", True, "Low urine output shows poor kidney perfusion."),
    ". ",
    ("Lipase 1,840 units/L", False, "Confirms the diagnosis but does not itself need a new action."),
    "."],
   "Hypotension, tachycardia, low urine output and Cullen's sign point to severe pancreatitis with fluid shifts and bleeding.",
   "Recognize cues of severe acute pancreatitis."),

cloze(M(PA, PANC, "analyze_cues", "moderate", "gastrointestinal", "ms"),
   "Complete the sentence based on the client's data.",
   "The client's falling blood pressure and rising hematocrit are most likely caused by {{b1}}, and the low calcium is a sign of {{b2}}.",
   [[("fluid shifting out of the blood vessels into the abdomen", True, "Inflammation makes vessels leaky; fluid loss concentrates the blood and lowers blood pressure."),
     ("a reaction to the pain medication", False, "Pain medication does not raise the hematocrit."),
     ("an acute myocardial infarction", False, "The picture fits fluid loss from pancreatitis.")],
    [("severe pancreatitis, as fat breakdown binds calcium", True, "Fatty acids released in pancreatitis bind calcium, and low calcium predicts more severe disease."),
     ("a dietary calcium deficiency", False, "Acute hypocalcemia here is from the disease process, not diet."),
     ("parathyroid gland overactivity", False, "Overactive parathyroids raise calcium.")]],
   "Severe pancreatitis causes third-spacing of fluid (hypovolemia, rising hematocrit) and hypocalcemia from fat necrosis.",
   "Analyze cues in acute pancreatitis."),

matrix_mc(M(RRP, LABS, "prioritize_hypotheses", "difficult", "gastrointestinal", "ms lab"),
   "For each potential complication, indicate whether the client is at high or low immediate risk based on the current data.",
   ["High immediate risk", "Low immediate risk"],
   [("Hypovolemic shock", 0, ["Falling blood pressure, tachycardia and low urine output show shock is developing.", "The data show a high risk."]),
    ("Tetany or seizures from low calcium", 0, ["Calcium of 7.4 mg/dL can cause tetany and dysrhythmias.", "The low calcium makes this risk high."]),
    ("Acute respiratory distress syndrome", 0, ["Rising respiratory rate in severe pancreatitis can signal ARDS.", "The tachypnea makes this risk high."]),
    ("Hypoglycemia", 1, ["The glucose is high, not low.", "Glucose is 228 mg/dL, so the immediate risk is low."])],
   "Severe pancreatitis threatens circulation, breathing and electrolyte balance; glucose is usually high, not low.",
   "Prioritize complications of acute pancreatitis."),

sata(M(PA, PANC, "generate_solutions", "difficult", "gastrointestinal", "ms"),
   "Which interventions should the nurse anticipate for this client?",
   [("Aggressive IV fluid replacement with isotonic crystalloid as prescribed", True, "Fluid restores volume lost to third-spacing and protects the kidneys."),
    ("Keep the client NPO and give IV opioids for pain as prescribed", True, "Resting the gut and treating severe pain are core treatments."),
    ("Check for Chvostek's and Trousseau's signs and have IV calcium available", True, "These signs detect hypocalcemia that may need replacement."),
    ("Start a high-fat oral diet to restore calories quickly", False, "Fat stimulates the pancreas and worsens pain."),
    ("Monitor oxygen saturation and breathing closely", True, "ARDS and pleural effusions are complications."),
    ("Give a dose of the prescribed alcohol-containing cough syrup at bedtime", False, "Alcohol worsens pancreatitis.")],
   "Pancreatitis care: fluids, NPO with early enteral feeding as tolerated, pain control, monitoring calcium, glucose and breathing, and no alcohol.",
   "Plan care for acute pancreatitis."),

mc(M(PPT, OPI, "take_action", "moderate", "gastrointestinal", "ph"),
   "The client rates pain 9 of 10 and has a prescription for hydromorphone 1 mg IV every 2 hours as needed. The last dose was 2 hours ago. The respiratory rate is 28/min, and the client is alert. What should the nurse do?",
   [("Hold the dose because opioids will worsen the client's pancreatitis", False,
     "Opioids are appropriate for pancreatitis pain; uncontrolled pain increases stress and oxygen demand."),
    ("Give the hydromorphone and reassess pain and sedation within 15 to 30 minutes", True,
     "The client is alert with a fast respiratory rate; treating severe pain and reassessing is appropriate."),
    ("Offer acetaminophen instead, since the pain is not yet severe enough for an opioid", False,
     "Pain of 9 of 10 is severe and needs stronger analgesia."),
    ("Wait until the pain reaches 10 of 10 before giving the next dose of IV hydromorphone to the client", False,
     "Waiting allows pain to escalate and is harder to control.")],
   "Severe pancreatitis pain is treated with IV opioids; reassess pain, sedation and breathing after each dose.",
   "Manage pain in acute pancreatitis."),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "moderate", "gastrointestinal", "ms lab"),
   "On day 3, the nurse reviews the client's progress. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Blood pressure 118/74 mm Hg and urine output 60 mL/hr", 0, ["Perfusion has returned.", "This shows improvement."]),
    ("Pain 3 of 10 and bowel sounds returning", 0, ["Inflammation is settling.", "This shows improvement."]),
    ("Calcium 8.8 mg/dL", 0, ["Calcium is back in range.", "This shows improvement."]),
    ("SpO2 88% on 4 L/min with new crackles at both bases", 1, ["Worsening oxygenation may signal ARDS or fluid overload.", "This needs prompt follow-up."])],
   "Improving perfusion, pain and calcium show recovery; falling oxygen saturation with crackles needs urgent evaluation.",
   "Evaluate the response to treatment of pancreatitis."),
]

# ---- Case 2: pulmonary embolism after surgery ----------------------------------------------------
pe_tabs = [
    ("pe-notes", "Nurses' notes", [note("Surgical unit, day 4 after surgery, 1400",
        "A 63-year-old client is 4 days after a total abdominal hysterectomy for cancer. The client has walked little "
        "because of pain and refused enoxaparin injections twice. The client suddenly reports sharp chest pain that is "
        "worse with breathing in and feels anxious, saying \"something is wrong.\" The left calf is swollen and tender.", id="pe-note")]),
    ("pe-vitals", "Vital signs", [flow("Vital signs", ["1000", "1400"], [
        ("Heart rate", ["84/min", "122/min"]),
        ("Respiratory rate", ["16/min", "30/min"]),
        ("Blood pressure", ["128/76 mm Hg", "108/68 mm Hg"]),
        ("SpO2", ["96% on room air", "86% on room air"]),
        ("Temperature", ["37.2 °C (99 °F)", "37.6 °C (99.7 °F)"])], id="pe-flow")]),
]

pe_qs = [
hl_table(M(RRP, POSTOP, "recognize_cues", "moderate", "respiratory", "ms"),
   "Review the 1400 findings. Highlight the findings that require immediate follow-up.",
   ["Assessment", "Finding"],
   [(["SpO2", "86% on room air"], True, "Sudden hypoxemia needs immediate oxygen and evaluation."),
    (["Heart rate", "122/min"], True, "New tachycardia with chest pain is a warning sign."),
    (["Chest pain", "Sharp, worse on inspiration"], True, "Pleuritic chest pain with hypoxemia suggests pulmonary embolism."),
    (["Temperature", "37.6 °C (99.7 °F)"], False, "Low-grade temperature is not the priority."),
    (["Left calf", "Swollen and tender"], True, "Signs of deep vein thrombosis, the usual source of a pulmonary embolism.")],
   "Sudden pleuritic chest pain, hypoxemia, tachycardia and a swollen calf after surgery point to pulmonary embolism.",
   "Recognize cues of pulmonary embolism."),

cloze(M(PA, VTE, "analyze_cues", "moderate", "respiratory", "ms"),
   "Complete the sentence based on the client's data.",
   "The client's findings are most consistent with {{b1}}, and the client's risk factors include {{b2}}.",
   [[("pulmonary embolism", True, "Sudden dyspnea, pleuritic pain, hypoxemia and tachycardia with a DVT fit PE."),
     ("postoperative pneumonia", False, "Pneumonia develops more gradually with fever and productive cough."),
     ("an anxiety attack", False, "Anxiety does not cause an SpO2 of 86% or calf swelling.")],
    [("cancer, recent pelvic surgery, immobility and missed enoxaparin", True, "These are major risk factors for venous thromboembolism."),
     ("the client's age alone", False, "Age adds risk, but surgery, cancer and immobility are stronger factors here."),
     ("a low-grade temperature", False, "Temperature is not a risk factor for VTE.")]],
   "Cancer, pelvic surgery, immobility and missed prophylaxis are major risk factors for PE.",
   "Analyze cues and risk factors for pulmonary embolism."),

bowtie(M(PA, VTE, "prioritize_hypotheses", "difficult", "respiratory", "ms ph"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Pulmonary embolism", True, "Sudden pleuritic pain, hypoxemia, tachycardia and a DVT after surgery indicate PE."),
    ("Myocardial infarction", False, "Chest pain is present, but pleuritic pain with a DVT and hypoxemia points to PE."),
    ("Pneumothorax", False, "Nothing suggests a punctured lung, and the calf findings point to a clot."),
    ("Atelectasis", False, "Atelectasis does not cause sudden severe hypoxemia with pleuritic pain on day 4.")],
   [("Give oxygen and raise the head of the bed", True, "Oxygen corrects hypoxemia and sitting up eases breathing."),
    ("Notify the provider or rapid response team at once", True, "PE is an emergency that needs imaging and anticoagulation."),
    ("Massage the left calf to relieve the tenderness", False, "Massage could dislodge more clot."),
    ("Help the client walk in the hall to improve breathing", False, "Walking with an acute PE and hypoxemia is unsafe."),
    ("Give the prescribed oral pain medicine and reassess in an hour", False, "This delays emergency care.")],
   [("Oxygen saturation and respiratory rate", True, "Show whether oxygenation is improving or worsening."),
    ("Heart rate and blood pressure", True, "Falling blood pressure can signal a massive PE."),
    ("Bowel sounds in each of the four abdominal quadrants", False, "Not relevant to PE."),
    ("Amount and color of drainage on the abdominal incision dressing", False, "Not a guide to PE."),
    ("Color and clarity of the urine in the collection bag", False, "Not a guide to PE.")],
   "Suspected PE: oxygen, head of bed up, rapid response, anticoagulation as prescribed, and close monitoring of oxygenation and blood pressure.",
   "Prioritize care for pulmonary embolism."),

sata(M(RRP, POSTOP, "generate_solutions", "moderate", "respiratory", "ms"),
   "Which interventions should the nurse anticipate after the provider evaluates the client?",
   [("A CT pulmonary angiogram to confirm the diagnosis", True, "CT angiography is the main test for PE."),
    ("Starting an anticoagulant such as a heparin infusion as prescribed", True, "Anticoagulation stops the clot from growing."),
    ("Continuous pulse oximetry and cardiac monitoring", True, "Monitoring detects worsening oxygenation and rhythm changes."),
    ("Applying a sequential compression device to the left leg", False, "Compression on a leg with an acute DVT may dislodge clot."),
    ("Keeping the client on bed rest with the legs raised high on pillows", False, "Strict bed rest with raised legs is not standard; activity is guided by the provider once anticoagulated.")],
   "PE workup and treatment: CT angiography, anticoagulation and monitoring; avoid compression devices on a leg with acute DVT.",
   "Plan care after pulmonary embolism is suspected."),

mc(M(PPT, ANTICOAG, "take_action", "difficult", "hematologic-oncologic", "ph"),
   "The client is started on a weight-based heparin infusion. Six hours later, the aPTT is 118 seconds; the protocol target is 60 to 90 seconds. There is no bleeding. What should the nurse do?",
   [("Continue the infusion at the same rate and recheck the aPTT in the morning", False,
     "An aPTT above the target increases bleeding risk and needs adjustment now."),
    ("Follow the protocol, which may mean pausing the infusion and lowering the rate", True,
     "Heparin protocols direct the nurse to hold briefly and reduce the rate when the aPTT is above range."),
    ("Give protamine sulfate right away to reverse the heparin completely", False,
     "Protamine is for serious bleeding or overdose, not a high aPTT without bleeding."),
    ("Increase the rate, since a higher aPTT means the clot is dissolving faster", False,
     "Heparin does not dissolve clots; a high aPTT means too much anticoagulation.")],
   "Adjust heparin infusions by the aPTT or anti-Xa protocol; above-range results usually mean a brief hold and a lower rate, with a recheck.",
   "Adjust a heparin infusion by protocol."),

matrix_mc(M(PA, VTE, "evaluate_outcomes", "moderate", "respiratory", "ms"),
   "Two days later, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("SpO2 95% on 2 L/min", 0, ["Oxygenation has improved.", "This shows improvement."]),
    ("Heart rate 88/min", 0, ["Tachycardia has resolved.", "This shows improvement."]),
    ("Chest pain 2 of 10 with deep breathing", 0, ["Pleuritic pain is easing.", "This shows improvement."]),
    ("New dark, tarry stool", 1, ["Black stools suggest GI bleeding from anticoagulation.", "This needs prompt reporting."])],
   "Better oxygenation, heart rate and pain show recovery; signs of bleeding on anticoagulation need prompt reporting.",
   "Evaluate recovery from pulmonary embolism."),
]

cases = [
    case("Severe acute pancreatitis", panc_tabs, panc_qs),
    case("Pulmonary embolism after pelvic surgery", pe_tabs, pe_qs),
]

BUILT = build(79, "monitoring-complications-cases",
              "Reduction of risk and physiological adaptation, with case studies on acute pancreatitis and pulmonary embolism",
              items, cases, wave=True)
