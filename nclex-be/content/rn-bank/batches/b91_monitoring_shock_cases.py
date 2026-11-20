"""Batch 91 (wave 2, chunk 6): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - sickle cell pain crisis and rising intracranial pressure after head injury (PHI.RRP, PHI.PA, PHI.PPT).

Wave 2 keeps chunk 5's depth and uses fewer plain multiple-choice items.
Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
POSTPROC = "post-procedure-complications"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
LABS = "laboratory-values-and-monitoring"
GIDX = "gi-diagnostic-procedures"
RESP_DX = "respiratory-diagnostic-procedures"
SPEC = "specimen-collection"
OXY = "oxygen-therapy-and-devices"
NEURO_DX = "neurological-assessment-and-procedures"
HHS = "hyperosmolar-hyperglycemic-state"
SHOCK = "shock-and-sepsis"
COPD = "asthma-and-copd-management"
BURN = "burn-care"
CARDMED = "heart-failure-interventions"
SICKLE = "sickle-cell-disease"
OPI = "opioid-safety"
ICP = "intracranial-pressure-and-head-injury"
NEUROMED = "neurological-medications"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, POSTPROC, "prioritize_hypotheses", "difficult", "endocrine", "ms"),
   "A client had a total parathyroidectomy 18 hours ago. The client reports numbness around the mouth and in the fingertips and muscle cramps in the hands. When the nurse taps in front of the ear, the corner of the mouth twitches. Serum calcium is 6.8 mg/dL, and the ECG shows a prolonged QT interval. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Acute hypocalcemia", True, "Perioral numbness, cramps, a positive Chvostek sign, low calcium and a long QT after parathyroid surgery show acute hypocalcemia."),
    ("Hyperkalemia", False, "Hyperkalemia causes peaked T waves, not these neuromuscular signs."),
    ("Thyroid storm", False, "Thyroid storm causes fever and tachycardia."),
    ("Anxiety with hyperventilation", False, "Hyperventilation can cause tingling, but the low calcium and Chvostek sign point to hypocalcemia.")],
   [("Notify the provider and prepare IV calcium gluconate as prescribed", True, "IV calcium treats symptomatic hypocalcemia."),
    ("Keep emergency airway equipment at the bedside", True, "Laryngospasm can occur with severe hypocalcemia."),
    ("Give a potassium supplement for the muscle cramps", False, "Potassium is not the problem."),
    ("Encourage the client to breathe into a paper bag", False, "The symptoms are from low calcium, not hyperventilation."),
    ("Restrict dairy foods until the calcium returns to normal", False, "Calcium intake is needed, not restricted.")],
   [("Serum calcium levels", True, "Guide replacement and show response."),
    ("Laryngospasm and cardiac rhythm", True, "Airway and rhythm problems are the dangerous complications."),
    ("Color and clarity of the urine", False, "Not a guide to hypocalcemia."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant."),
    ("Body weight measured once a week", False, "Not relevant.")],
   "After parathyroid or thyroid surgery, watch for hypocalcemia: tingling, cramps, Chvostek and Trousseau signs, laryngospasm and long QT; treat with IV calcium.",
   "Recognize and treat hypocalcemia after parathyroidectomy."),

matrix_mc(M(RRP, CV_DX, "take_action", "difficult", "cardiovascular", "aha"),
   "The nurse is monitoring several clients on telemetry. For each rhythm and situation, indicate the first action the nurse should expect.",
   ["Treat the underlying cause", "Defibrillate", "Start CPR and give epinephrine", "Try vagal maneuvers"],
   [("Sinus tachycardia at 124/min in a client with a fever of 39 °C (102.2 °F)", 0,
     ["Sinus tachycardia is treated by finding and treating the cause, such as fever.", "Not a shockable rhythm.", "The client has a pulse.", "Vagal maneuvers are not used for sinus tachycardia."]),
    ("Ventricular fibrillation in a client who is unresponsive with no pulse", 1,
     ["Not the first action in cardiac arrest.", "VF is treated with immediate defibrillation.", "CPR continues, but defibrillation is the key first step.", "Not used in cardiac arrest."]),
    ("Asystole confirmed in two leads in a client with no pulse", 2,
     ["Not the first action.", "Asystole is not shockable.", "CPR and epinephrine are the treatment for asystole.", "Not used in cardiac arrest."]),
    ("Narrow-complex regular tachycardia at 192/min in a stable, alert client", 3,
     ["Not sinus tachycardia.", "Not a pulseless rhythm.", "The client has a pulse.", "Stable SVT is first treated with vagal maneuvers, then adenosine."])],
   "Treat sinus tachycardia's cause; defibrillate VF or pulseless VT; give CPR and epinephrine for asystole and PEA; try vagal maneuvers then adenosine for stable SVT.",
   "Match rhythms to first actions."),

hl_table(M(RRP, LABS, "analyze_cues", "difficult", F, "ms lab"),
   "On the first morning after a hip replacement, the nurse reviews the client's laboratory results. Highlight the results that need prompt reporting.",
   ["Test", "Result (reference)"],
   [(["Hemoglobin", "7.4 g/dL (12-16), was 11.9 before surgery"], True, "A large drop suggests significant blood loss."),
    (["Potassium", "3.9 mEq/L (3.5-5.0)"], False, "Normal."),
    (["Creatinine", "2.1 mg/dL (0.6-1.2), was 0.9 before surgery"], True, "A doubling of creatinine suggests acute kidney injury."),
    (["Glucose", "152 mg/dL (70-110)"], False, "Mild stress hyperglycemia after surgery is common."),
    (["Sodium", "124 mEq/L (135-145)"], True, "Significant hyponatremia can cause confusion and seizures.")],
   "After surgery, report significant drops in hemoglobin, rising creatinine and significant electrolyte abnormalities; mild stress hyperglycemia is common.",
   "Identify postoperative laboratory results to report."),

order(M(RRP, GIDX, "take_action", "moderate", "gastrointestinal", "fund"),
   "The nurse is inserting a nasogastric tube for decompression. Place the steps in order.",
   [("Measure the tube from the tip of the nose to the earlobe to the xiphoid process and mark it", "Estimates the length to reach the stomach."),
    ("Raise the head of the bed to high Fowler's and have the client flex the neck slightly", "Positioning eases passage into the esophagus."),
    ("Insert the tube while the client sips water or swallows, advancing to the mark", "Swallowing helps direct the tube into the esophagus."),
    ("Confirm placement as policy directs, such as pH testing of aspirate and x-ray", "Placement is confirmed before use.")],
   "NG insertion: measure, position upright with the neck flexed, advance with swallowing and confirm placement.",
   "Insert a nasogastric tube."),

sata(M(RRP, RESP_DX, "take_action", "moderate", "respiratory", "ms"),
   "A client is scheduled for a bronchoscopy with moderate sedation this morning. Which actions should the nurse take before the procedure?",
   [("Confirm the client has been NPO for the required time", True, "Reduces aspiration risk."),
    ("Check that informed consent is signed", True, "Required for an invasive procedure."),
    ("Ask about anticoagulant use and bleeding problems", True, "Biopsies during bronchoscopy can bleed."),
    ("Give the client a large breakfast to keep up their strength", False, "The client must be NPO."),
    ("Remove dentures and check the airway", True, "Prepares for the scope and sedation."),
    ("Tell the client they will be awake and can talk throughout", False, "The client is sedated and cannot speak with the scope in place.")],
   "Before bronchoscopy: NPO, consent, anticoagulant review, remove dentures and prepare for sedation monitoring.",
   "Prepare a client for bronchoscopy."),

mc(M(RRP, GIDX, "take_action", "moderate", "gastrointestinal", "ms"),
   "A client has just had a percutaneous liver biopsy. How should the nurse position the client?",
   [("Left side-lying with the head of the bed raised", False,
     "Left side-lying does not compress the biopsy site."),
    ("Right side-lying with a pillow under the ribs", True,
     "Lying on the right side presses on the puncture site and helps prevent bleeding."),
    ("Prone with the arms above the head", False,
     "Prone is not the standard position after liver biopsy."),
    ("High Fowler's with the legs dangling", False,
     "Upright positioning does not compress the liver.")],
   "After liver biopsy, place the client on the right side for several hours to compress the site and watch for bleeding.",
   "Position a client after liver biopsy."),

mc(M(RRP, SPEC, "take_action", "moderate", "cardiovascular", "ms lab"),
   "Before drawing an arterial blood gas from a client's radial artery, the nurse performs a modified Allen test. The hand stays pale for 12 seconds after the ulnar artery is released. What should the nurse do?",
   [("Draw the sample from the radial artery as planned", False,
     "Poor ulnar flow means the hand may lose blood supply if the radial artery is damaged."),
    ("Do not use that radial artery; choose another site", True,
     "A delayed return of color shows poor collateral flow; another site should be used."),
    ("Apply a warm compress for 10 minutes and then draw from the same artery", False,
     "Warming does not fix inadequate collateral circulation."),
    ("Repeat the test on the same hand until the color returns faster", False,
     "Repeating does not change the anatomy.")],
   "A positive modified Allen test (color returns after more than about 5 to 10 seconds) means poor ulnar flow; avoid that radial artery.",
   "Interpret the modified Allen test."),

cloze(M(RRP, LABS, "analyze_cues", "difficult", "renal-urinary", "ms lab"),
   "A 70-year-old client with watery diarrhea for 5 days and dry mucous membranes has arterial blood gases: pH 7.32, PaCO2 30 mm Hg, HCO3 15 mEq/L. Complete the sentence.",
   "These results show {{b1}} that is {{b2}}, most likely caused by {{b3}}.",
   [[("metabolic acidosis", True, "Low pH with low bicarbonate is metabolic acidosis."),
     ("respiratory acidosis", False, "PaCO2 is low, not high."),
     ("metabolic alkalosis", False, "pH and bicarbonate are low.")],
    [("partly compensated", True, "PaCO2 has fallen as the lungs compensate, but pH is still low."),
     ("fully compensated", False, "pH is still below 7.35."),
     ("uncompensated", False, "The low PaCO2 shows the lungs are compensating.")],
    [("loss of bicarbonate in the stool", True, "Diarrhea removes bicarbonate."),
     ("vomiting stomach acid", False, "Vomiting causes alkalosis."),
     ("hypoventilation", False, "Breathing is fast to compensate.")]],
   "Diarrhea causes bicarbonate loss and metabolic acidosis; the lungs compensate by blowing off CO2.",
   "Interpret a partly compensated metabolic acidosis."),

# ---- physiological adaptation ----
bowtie(M(PA, HHS, "prioritize_hypotheses", "difficult", "endocrine", "ms ada"),
   "A 78-year-old client with type 2 diabetes is brought in after a week of poor intake and increasing confusion. Blood glucose is 912 mg/dL, serum osmolality 362 mOsm/kg, sodium 152 mEq/L (corrected higher), pH 7.36, bicarbonate 23 mEq/L and urine ketones are trace. Blood pressure is 92/54 mm Hg and heart rate 118/min. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Hyperosmolar hyperglycemic state", True, "Very high glucose and osmolality, severe dehydration and confusion without significant acidosis or ketones is HHS."),
    ("Diabetic ketoacidosis", False, "pH and bicarbonate are near normal and ketones are minimal."),
    ("Acute ischemic stroke", False, "Confusion here is explained by hyperosmolality."),
    ("Severe hypoglycemia", False, "Glucose is very high.")],
   [("Give large volumes of IV isotonic fluid as prescribed", True, "Severe dehydration is the main problem; fluids come first."),
    ("Start an insulin infusion after fluids are under way, as prescribed", True, "Insulin is given after fluid replacement has started."),
    ("Give a bolus of subcutaneous long-acting insulin and fluids later", False, "IV fluids come first, and insulin is given by infusion."),
    ("Restrict fluids to prevent cerebral edema", False, "The client is severely dehydrated."),
    ("Give sodium bicarbonate to correct the pH", False, "There is no significant acidosis.")],
   [("Glucose and serum osmolality", True, "Gradual correction lowers the risk of cerebral edema."),
    ("Mental status and urine output", True, "Show response to fluids and treatment."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant."),
    ("Body weight measured once a week", False, "Not frequent enough."),
    ("Pupil size and reaction to light", False, "Not the key guide.")],
   "HHS: severe dehydration and hyperosmolality without major ketoacidosis; give fluids first, then insulin, and correct glucose and osmolality gradually.",
   "Recognize and manage HHS."),

matrix_mc(M(PA, SHOCK, "analyze_cues", "difficult", "cardiovascular", "ms"),
   "For each set of findings, indicate the type of shock it most likely represents.",
   ["Hypovolemic", "Cardiogenic", "Septic (early)", "Neurogenic"],
   [("Cool, pale skin, low CVP, tachycardia and a recent GI bleed", 0,
     ["Blood loss with low filling pressures is hypovolemic shock.", "Cardiogenic shock has high filling pressures.", "Early sepsis causes warm skin.", "Neurogenic shock causes bradycardia."]),
    ("Crackles, distended neck veins and low blood pressure after a large MI", 1,
     ["Volume is not low.", "Pump failure with congestion is cardiogenic shock.", "No infection.", "No spinal injury."]),
    ("Fever, warm flushed skin, wide pulse pressure and low blood pressure", 2,
     ["Not volume loss.", "No congestion.", "Early septic shock causes vasodilation with warm skin.", "No spinal injury."]),
    ("Hypotension, heart rate 48/min and warm dry skin after a cervical spine injury", 3,
     ["Volume loss causes tachycardia.", "Not pump failure.", "No infection.", "Loss of sympathetic tone causes neurogenic shock."])],
   "Shock types: hypovolemic (cool, tachycardic, low CVP), cardiogenic (congestion), early septic (warm, wide pulse pressure) and neurogenic (warm, bradycardic).",
   "Distinguish types of shock."),

mc(M(PA, COPD, "take_action", "difficult", "respiratory", "ms"),
   "A client with severe COPD and a history of CO2 retention is admitted with an exacerbation. SpO2 is 84% on room air, and the client is alert. Which oxygen plan should the nurse expect?",
   [("High-flow oxygen by non-rebreather mask to bring the SpO2 to 100%", False,
     "Too much oxygen in CO2 retainers can worsen hypercapnia."),
    ("Controlled oxygen to an SpO2 of 88% to 92%", True,
     "A target of 88% to 92% corrects hypoxemia while limiting CO2 retention."),
    ("No oxygen, because oxygen stops the client's drive to breathe", False,
     "Hypoxemia must be treated; oxygen is titrated, not withheld."),
    ("Oxygen only at night, while the client is sleeping", False,
     "The client is hypoxemic now.")],
   "In COPD with CO2 retention, titrate oxygen to an SpO2 of 88% to 92%, watch for rising CO2 and drowsiness and never withhold oxygen for hypoxemia.",
   "Choose an oxygen target in COPD."),

mc(M(PA, BURN, "recognize_cues", "difficult", "integumentary", "ms"),
   "A client has full-thickness circumferential burns of the chest from a house fire. Over 2 hours, breathing becomes shallow and the ventilator pressures rise, although the airway is clear on suctioning. What should the nurse anticipate?",
   [("Increasing the sedation so the client fights the ventilator less", False,
     "The problem is the stiff burned skin restricting chest movement, not agitation."),
    ("An escharotomy to release the tight chest eschar", True,
     "Circumferential chest eschar restricts expansion; escharotomy restores ventilation."),
    ("Giving a diuretic to reduce fluid in the lungs", False,
     "The chest wall restriction is the cause."),
    ("Changing the endotracheal tube to a larger size", False,
     "The airway is clear.")],
   "Circumferential full-thickness burns of the chest or limbs can restrict breathing or blood flow; escharotomy is needed.",
   "Recognize the need for escharotomy."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, CARDMED, "take_action", "difficult", "cardiovascular", "ph aha"),
   "A stable, alert client in supraventricular tachycardia at 196/min did not convert with vagal maneuvers. The provider prescribes adenosine 6 mg IV. How should the nurse give it?",
   [("Dilute it in 100 mL of saline and infuse it over 30 minutes by pump", False,
     "Adenosine has a half-life of seconds and must be given as a rapid push."),
    ("Push it rapidly in a proximal vein, then flush fast with saline", True,
     "A rapid push and flush gets the drug to the heart before it breaks down; a brief pause in rhythm is expected."),
    ("Push it slowly over 5 minutes while watching the monitor closely", False,
     "A slow push lets the drug break down before reaching the heart."),
    ("Give it intramuscularly into the deltoid", False,
     "Adenosine is given IV only.")],
   "Adenosine: rapid IV push in a proximal vein followed by a rapid saline flush; warn the client about a brief feeling of chest pressure and expect a short pause.",
   "Give adenosine correctly."),
]

# ---- Case 1: sickle cell pain crisis ---------------------------------------------------------
scd_tabs = [
    ("scd-notes", "Nurses' notes", [note("Emergency department, 1300",
        "A 22-year-old client with sickle cell disease (HbSS) has severe pain in the lower back, chest and both legs "
        "that started yesterday after a long flight. The client rates pain 9 of 10 and says it feels like previous crises. "
        "Since this morning the client has had a cough and sharp chest pain on breathing in.", id="scd-note")]),
    ("scd-vitals", "Vital signs", [flow("Vital signs", ["1300", "1500"], [
        ("Temperature", ["38.0 °C (100.4 °F)", "38.6 °C (101.5 °F)"]),
        ("Heart rate", ["108/min", "118/min"]),
        ("Respiratory rate", ["22/min", "28/min"]),
        ("SpO2", ["94% on room air", "89% on room air"])], id="scd-flow")]),
    ("scd-labs", "Laboratory results", [table("Laboratory results, 1430", ["Result", "Reference"], [
        ("Hemoglobin", ["6.4 g/dL (baseline 8.2)", "12-16 g/dL"]),
        ("WBC", ["17,800/mm3", "5,000-10,000/mm3"]),
        ("Chest x-ray", ["New infiltrate in the right lower lobe", "Clear"])], id="scd-lab")]),
]

scd_qs = [
hl_text(M(PA, SICKLE, "recognize_cues", "moderate", "hematologic-oncologic", "ms"),
   "Review the 1500 findings. Highlight the findings that require immediate follow-up.",
   ["The client has ",
    ("sharp chest pain on breathing in and a cough", True, "Chest symptoms in sickle cell disease may be acute chest syndrome."),
    ". ",
    ("Pain rated 9 of 10 in the back and legs", False, "Expected in a vaso-occlusive crisis and needs treatment, but chest findings are more dangerous."),
    ". SpO2 ",
    ("89% on room air", True, "Falling oxygen saturation suggests acute chest syndrome."),
    ". The chest x-ray shows ",
    ("a new infiltrate in the right lower lobe", True, "A new infiltrate with chest symptoms defines acute chest syndrome."),
    ". Hemoglobin ",
    ("6.4 g/dL, down from 8.2", True, "A falling hemoglobin may need transfusion."),
    "."],
   "In sickle cell crisis, new chest pain, hypoxemia and a new infiltrate signal acute chest syndrome, a leading cause of death.",
   "Recognize acute chest syndrome."),

cloze(M(PA, SICKLE, "analyze_cues", "moderate", "hematologic-oncologic", "ms"),
   "Complete the sentence based on the client's data.",
   "The client is having a vaso-occlusive crisis complicated by {{b1}}, likely triggered by {{b2}}.",
   [[("acute chest syndrome", True, "Chest pain, fever, hypoxemia and a new infiltrate define acute chest syndrome."),
     ("an asthma attack", False, "There is no wheezing, and the infiltrate points to acute chest syndrome."),
     ("a heart attack", False, "The picture fits acute chest syndrome.")],
    [("dehydration and low oxygen during a long flight", True, "Dehydration and hypoxia promote sickling."),
     ("eating a high-iron meal", False, "Diet is not a trigger here."),
     ("too much exercise", False, "Nothing suggests strenuous exercise.")]],
   "Dehydration, hypoxia, cold, infection and stress trigger sickling; acute chest syndrome is a serious complication.",
   "Analyze triggers and complications of sickle cell crisis."),

matrix_mc(M(PA, SICKLE, "prioritize_hypotheses", "difficult", "hematologic-oncologic", "ms"),
   "For each complication, indicate whether the client is at high or low immediate risk based on the current data.",
   ["High immediate risk", "Low immediate risk"],
   [("Respiratory failure", 0, ["Falling SpO2 and rising respiratory rate with an infiltrate show high risk.", "The data show high risk."]),
    ("Worsening anemia needing transfusion", 0, ["Hemoglobin has fallen below baseline.", "The data show high risk."]),
    ("Sepsis", 0, ["Fever and a high WBC in a client with functional asplenia show high risk.", "The data show high risk."]),
    ("Hyperglycemia", 1, ["Nothing in the data suggests high glucose.", "Low risk."])],
   "Acute chest syndrome threatens breathing; anemia and infection risk are high because of hemolysis and loss of spleen function.",
   "Prioritize complications in sickle cell crisis."),

sata(M(RRP, OXY, "generate_solutions", "difficult", "hematologic-oncologic", "ms"),
   "Which interventions should the nurse anticipate for this client?",
   [("Oxygen to keep the SpO2 at or above 95% or per protocol", True, "Hypoxemia worsens sickling."),
    ("IV fluids for hydration", True, "Hydration reduces sickling, with care to avoid overload."),
    ("Incentive spirometry every 1 to 2 hours while awake", True, "Prevents atelectasis that worsens acute chest syndrome."),
    ("Blood cultures and broad-spectrum antibiotics", True, "Fever and an infiltrate need antibiotics."),
    ("Cold packs on the painful legs", False, "Cold causes vasoconstriction and worsens sickling."),
    ("Strict bed rest without changing position", False, "Immobility worsens atelectasis.")],
   "Acute chest syndrome care: oxygen, hydration, incentive spirometry, antibiotics, pain control and possible transfusion; avoid cold.",
   "Plan care for acute chest syndrome."),

mc(M(PPT, OPI, "take_action", "moderate", "hematologic-oncologic", "ph"),
   "The client still rates pain 9 of 10 one hour after arrival. Which pain management approach is most appropriate?",
   [("Give oral acetaminophen and wait an hour to see whether it is enough", False,
     "Severe crisis pain needs strong analgesia."),
    ("Give IV opioids promptly, then scheduled or by PCA", True,
     "Sickle cell crisis pain needs rapid, scheduled IV opioids with close monitoring."),
    ("Give meperidine every 3 hours as the preferred opioid", False,
     "Meperidine can cause seizures and is avoided."),
    ("Withhold opioids because the client might be seeking drugs", False,
     "Assuming drug seeking leads to undertreated pain.")],
   "Treat sickle cell crisis pain quickly with IV opioids (scheduled or PCA), reassess often and avoid meperidine.",
   "Manage pain in sickle cell crisis."),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "moderate", "hematologic-oncologic", "ms lab"),
   "On day 3, the nurse reviews the client's progress. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("SpO2 96% on 2 L/min", 0, ["Oxygenation has improved.", "This shows improvement."]),
    ("Pain 4 of 10 on scheduled oral opioids", 0, ["Pain is controlled.", "This shows improvement."]),
    ("Temperature 37.2 °C (99 °F)", 0, ["The fever has resolved.", "This shows improvement."]),
    ("Hemoglobin 5.8 g/dL", 1, ["Hemoglobin is still falling.", "This needs follow-up for possible transfusion."])],
   "Improving oxygenation, pain and fever show response; a falling hemoglobin needs follow-up.",
   "Evaluate recovery from acute chest syndrome."),
]

# ---- Case 2: rising intracranial pressure -------------------------------------------------------
tbi_tabs = [
    ("tbi-notes", "Nurses' notes", [note("Neurosurgical ICU, 0200",
        "A 34-year-old client was admitted 10 hours ago after a fall from a ladder with a right frontal contusion. "
        "At 2200 the client was drowsy but opened eyes to voice, obeyed commands and was oriented. At 0200 the client "
        "opens eyes only to pain, makes incomprehensible sounds and withdraws from pain. The right pupil is 6 mm and "
        "sluggish; the left is 3 mm and brisk.", id="tbi-note")]),
    ("tbi-vitals", "Vital signs", [flow("Vital signs", ["2200", "0200"], [
        ("Blood pressure", ["132/78 mm Hg", "172/68 mm Hg"]),
        ("Heart rate", ["84/min", "58/min"]),
        ("Respiratory rate", ["16/min", "10/min, irregular"]),
        ("Temperature", ["37.0 °C (98.6 °F)", "37.8 °C (100 °F)"])], id="tbi-flow")]),
]

tbi_qs = [
hl_table(M(RRP, NEURO_DX, "recognize_cues", "moderate", "neurological", "ms"),
   "Compare the 2200 and 0200 data. Highlight the findings that require immediate action.",
   ["Assessment", "0200 finding"],
   [(["Glasgow Coma Scale", "8 (from 14)"], True, "A drop of 6 points shows serious neurological decline."),
    (["Right pupil", "6 mm and sluggish"], True, "A dilating, sluggish pupil on the side of injury suggests herniation."),
    (["Blood pressure", "172/68 mm Hg, wide pulse pressure"], True, "Widening pulse pressure is part of Cushing's triad."),
    (["Temperature", "37.8 °C (100 °F)"], False, "Mild temperature rise; manage, but it is not the most urgent."),
    (["Respirations", "10/min, irregular"], True, "Irregular breathing is part of Cushing's triad.")],
   "A falling GCS, a dilating pupil and Cushing's triad (widening pulse pressure, bradycardia, irregular breathing) signal rising ICP and possible herniation.",
   "Recognize cues of rising intracranial pressure."),

cloze(M(PA, ICP, "analyze_cues", "moderate", "neurological", "ms"),
   "Complete the sentence based on the client's data.",
   "The client's findings show {{b1}}, and the right-sided pupil change suggests {{b2}}.",
   [[("rising intracranial pressure", True, "A falling GCS and Cushing's triad show rising ICP."),
     ("a seizure", False, "No seizure activity is described."),
     ("hypoglycemia", False, "The pattern points to ICP.")],
    [("pressure on the right third cranial nerve from herniation", True, "A dilating pupil on the side of injury suggests uncal herniation."),
     ("a normal effect of opioid pain medication", False, "Opioids cause small pupils."),
     ("an eye injury from the fall", False, "The neurological pattern points to herniation.")]],
   "Rising ICP shows as a falling level of consciousness, pupil changes and Cushing's triad; a dilating pupil on one side suggests herniation.",
   "Analyze cues of brain herniation."),

bowtie(M(PA, ICP, "prioritize_hypotheses", "difficult", "neurological", "ms ph"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Rising intracranial pressure with impending herniation", True, "Falling GCS, a dilating pupil and Cushing's triad point to herniation."),
    ("Postictal state after a seizure", False, "No seizure occurred."),
    ("Opioid oversedation", False, "Opioids cause pinpoint pupils, not a dilated one."),
    ("Hypovolemic shock", False, "Blood pressure is high, not low.")],
   [("Notify the provider now and prepare for treatment", True, "Herniation needs immediate intervention."),
    ("Raise the head of the bed to 30 degrees, head midline", True, "Improves venous drainage and lowers ICP."),
    ("Lay the client flat to improve blood flow to the brain", False, "Lying flat raises ICP."),
    ("Cluster all care together to finish quickly", False, "Clustering many activities can raise ICP; spacing care is better."),
    ("Suction the airway every 30 minutes routinely", False, "Suctioning raises ICP and is done only when needed.")],
   [("Glasgow Coma Scale and pupil checks", True, "Show neurological change."),
    ("Blood pressure, heart rate and breathing pattern", True, "Track Cushing's triad."),
    ("Fingerstick blood glucose before meals", False, "Not the key guide."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant."),
    ("Color and clarity of the urine", False, "Not the key guide.")],
   "Impending herniation: notify immediately, raise the head of the bed to 30 degrees with the head midline and expect hyperosmolar therapy; monitor GCS, pupils and vital signs.",
   "Prioritize care for rising intracranial pressure."),

sata(M(RRP, NEURO_DX, "generate_solutions", "difficult", "neurological", "ms"),
   "Which measures help lower intracranial pressure for this client?",
   [("Keep the head and neck in a neutral, midline position", True, "Prevents compression of the jugular veins."),
    ("Keep the head of the bed at 30 degrees", True, "Helps venous drainage."),
    ("Maintain normal oxygen and carbon dioxide levels", True, "Low oxygen or high CO2 raises ICP."),
    ("Keep the room quiet and space out care activities", True, "Stimulation raises ICP."),
    ("Encourage the client to cough forcefully every hour", False, "Coughing raises ICP."),
    ("Bend the hips sharply with pillows under the knees", False, "Hip flexion raises intra-abdominal and intracranial pressure."),
    ("Treat fever promptly as prescribed", True, "Fever raises metabolic demand and ICP.")],
   "Lower ICP by keeping the head midline and raised 30 degrees, maintaining normal gases, reducing stimulation and avoiding coughing, straining and hip flexion.",
   "Use measures to lower intracranial pressure."),

mc(M(PPT, NEUROMED, "take_action", "difficult", "neurological", "ph"),
   "The provider prescribes IV mannitol for the rising ICP. Which assessment is most important while the client receives this drug?",
   [("Checking the blood glucose every hour", False,
     "Glucose is not the main concern with mannitol."),
    ("Urine output, osmolality and electrolytes", True,
     "Mannitol causes osmotic diuresis; urine output, osmolality and electrolytes guide safe use."),
    ("Measuring the abdominal girth at the umbilicus each shift", False,
     "Not related to mannitol."),
    ("Checking the client's hearing in both ears once each shift", False,
     "Hearing is not the main concern.")],
   "Mannitol pulls water from the brain and causes diuresis; monitor urine output, osmolality, electrolytes and signs of fluid imbalance.",
   "Monitor mannitol therapy."),

matrix_mc(M(RRP, NEURO_DX, "evaluate_outcomes", "moderate", "neurological", "ms"),
   "After emergency treatment, the nurse reassesses the client at 0400. For each finding, indicate whether it shows improvement or needs immediate action.",
   ["Shows improvement", "Needs immediate action"],
   [("GCS 11, opens eyes to voice", 0, ["Consciousness has improved.", "This shows improvement."]),
    ("Pupils 4 mm and reactive on both sides", 0, ["Pupils have equalized.", "This shows improvement."]),
    ("Blood pressure 138/80 mm Hg, heart rate 82/min, regular breathing", 0, ["Cushing's triad has resolved.", "This shows improvement."]),
    ("Clear fluid draining from the nose that tests positive for glucose", 1, ["This may be cerebrospinal fluid from a skull fracture.", "This needs reporting."])],
   "Improving GCS, equal pupils and normal vital signs show response; clear nasal drainage after head injury may be CSF and needs reporting.",
   "Evaluate the response to ICP treatment."),
]

cases = [
    case("Sickle cell crisis with acute chest syndrome", scd_tabs, scd_qs),
    case("Rising intracranial pressure after a head injury", tbi_tabs, tbi_qs),
]

BUILT = build(91, "monitoring-shock-cases",
              "Reduction of risk and physiological adaptation, with case studies on sickle cell crisis and rising intracranial pressure",
              items, cases, wave=True)
