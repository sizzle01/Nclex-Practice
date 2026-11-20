"""Batch 83 (wave 1, chunk 4): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - sepsis from an anastomotic leak and tension pneumothorax after chest trauma (PHI.RRP, PHI.PA, PHI.PPT).

Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
POSTPROC = "post-procedure-complications"
SPEC = "specimen-collection"
GIDX = "gi-diagnostic-procedures"
LABS = "laboratory-values-and-monitoring"
CONTRAST = "monitoring-after-contrast-dye"
ANES = "anesthesia-and-sedation"
POSTOP = "postoperative-care"
RENDO_DX = "renal-and-endocrine-diagnostics"
THYROID = "thyroid-and-parathyroid-disorders"
DIAL = "dialysis-care"
ASTHMA = "asthma-and-copd-management"
INS = "insulin-therapy"
TECH = "medication-administration-technique"
SEPSIS = "shock-and-sepsis"
ADV = "adverse-effects-of-medications"
CHEST = "chest-trauma-and-pneumothorax"
CT = "chest-tube-management"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, POSTPROC, "prioritize_hypotheses", "difficult", "pediatrics", "peds"),
   "A 6-year-old child had a tonsillectomy this morning. At 1600 the nurse notes the child is swallowing every few seconds and has vomited a small amount of bright red blood. The child is restless and pale. Heart rate has risen from 98 to 136/min, and blood pressure is 96/58 mm Hg. Complete the diagram by placing the condition the child is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Hemorrhage after tonsillectomy", True, "Frequent swallowing, bright red vomit, restlessness and tachycardia after tonsillectomy point to bleeding from the surgical site."),
    ("Expected swallowing of old blood after surgery", False, "Old blood is dark brown; bright red blood with tachycardia is active bleeding."),
    ("Postoperative nausea from anesthesia", False, "Nausea does not explain frequent swallowing and rising heart rate."),
    ("Dehydration from poor oral intake", False, "Dehydration does not cause bright red vomit.")],
   [("Notify the surgeon right away", True, "Active bleeding may need a return to the operating room."),
    ("Position the child on the side with the head slightly down", True, "Side-lying lets blood drain from the mouth and protects the airway."),
    ("Have the child gargle with cold water to slow the bleeding", False, "Gargling can dislodge clots and worsen bleeding."),
    ("Suction deep into the back of the throat to see where the blood is coming from", False, "Deep suction can injure the surgical site."),
    ("Give a dose of ibuprofen for pain and recheck in an hour", False, "NSAIDs can increase bleeding, and waiting is unsafe.")],
   [("Frequency of swallowing and amount of blood", True, "Shows whether bleeding is continuing."),
    ("Heart rate and blood pressure", True, "Rising heart rate is an early sign of blood loss in children; hypotension is late."),
    ("Oral temperature checked every 4 hours", False, "Not a guide to bleeding."),
    ("Bowel sounds in each of the four quadrants", False, "Not related to post-tonsillectomy bleeding."),
    ("Urine specific gravity with each void", False, "Not a guide to acute bleeding.")],
   "After tonsillectomy, frequent swallowing, bright red blood and tachycardia signal hemorrhage; position on the side, notify the surgeon and watch swallowing and vital signs.",
   "Recognize and respond to post-tonsillectomy hemorrhage."),

mc(M(RRP, SPEC, "take_action", "moderate", "respiratory", "ms lab"),
   "The nurse has just drawn an arterial blood gas sample from a client's radial artery. What should the nurse do next?",
   [("Apply firm pressure to the site for at least 5 minutes", True,
     "Arterial punctures bleed more than venous ones; pressure for 5 minutes or longer (longer with anticoagulants) prevents a hematoma."),
    ("Shake the syringe hard to mix the blood with the heparin", False,
     "Vigorous shaking can damage cells; the sample is rolled gently."),
    ("Leave an air bubble in the syringe so the sample keeps its oxygen level", False,
     "Air bubbles change the oxygen and carbon dioxide values and must be removed."),
    ("Send the sample at room temperature at the end of the shift with other specimens", False,
     "Gas samples must be analyzed promptly, usually within minutes, or on ice per policy.")],
   "After an ABG: hold firm pressure for at least 5 minutes, remove air bubbles, label and send promptly.",
   "Handle an arterial blood gas sample correctly."),

mc(M(RRP, GIDX, "recognize_cues", "difficult", "gastrointestinal", "ms"),
   "Four hours after a colonoscopy with removal of several polyps, a client reports severe abdominal pain that is getting worse. The abdomen is rigid and tender, temperature is 38.4 °C (101.1 °F) and heart rate is 118/min. What should the nurse suspect?",
   [("Gas pain from the air used to inflate the colon during the procedure", False,
     "Mild cramping from air is common, but it eases and does not cause rigidity and fever."),
    ("Perforation of the bowel wall with leakage of contents", True,
     "Severe, worsening pain, a rigid abdomen, fever and tachycardia after polyp removal suggest perforation and peritonitis."),
    ("A reaction to the bowel preparation solution taken the night before", False,
     "Bowel prep effects occur before the procedure and do not cause peritonitis."),
    ("Expected discomfort from the sedation as it wears off over the evening", False,
     "Sedation does not cause abdominal rigidity or fever.")],
   "After colonoscopy, severe or worsening abdominal pain, rigidity, fever or tachycardia suggest perforation and need urgent reporting.",
   "Recognize perforation after colonoscopy."),

mc(M(RRP, LABS, "analyze_cues", "moderate", "endocrine", "ada lab"),
   "A client with type 2 diabetes has a hemoglobin A1C of 9.1%. The client says their morning fingerstick readings have been \"pretty good.\" How should the nurse interpret the A1C?",
   [("It shows glucose has been well controlled, since the morning readings are good", False,
     "An A1C of 9.1% shows high average glucose regardless of the morning readings."),
    ("It shows average glucose over about 3 months has been well above the usual target", True,
     "A1C reflects average glucose over 2 to 3 months; 9.1% is well above the usual goal of under 7%."),
    ("It reflects just the glucose level on the day the blood sample was drawn at the lab", False,
     "A1C reflects the past 2 to 3 months, not one day."),
    ("It is a test of kidney function that is affected by diabetes over time", False,
     "A1C measures glucose attached to hemoglobin, not kidney function.")],
   "A1C reflects average glucose over 2 to 3 months; most adults with diabetes aim for under 7%. Good morning readings can hide high levels later in the day.",
   "Interpret hemoglobin A1C."),

mc(M(RRP, CONTRAST, "analyze_cues", "moderate", "renal-urinary", "ms lab"),
   "A client is scheduled for a CT scan of the abdomen with IV contrast this afternoon. Which finding should the nurse report to the provider before the scan?",
   [("The client had an allergic reaction to shrimp as a child", False,
     "Shellfish allergy does not predict a reaction to iodinated contrast."),
    ("A serum creatinine of 2.4 mg/dL, up from 1.1 last month", True,
     "Worsening kidney function raises the risk of contrast-associated kidney injury; the provider may change the plan or give fluids."),
    ("The client ate a light breakfast at 0700 this morning", False,
     "Fasting rules vary, and a light early meal is often acceptable; it is less important than kidney function."),
    ("The client is anxious and asks how long the scan will take", False,
     "Anxiety is addressed with explanation; it does not change safety.")],
   "Before IV contrast, check kidney function, earlier contrast reactions and metformin use; shellfish allergy is not a contraindication.",
   "Identify risks before IV contrast."),

mc(M(RRP, ANES, "recognize_cues", "difficult", "neurological", "ms"),
   "A client who had a thoracotomy 2 days ago has an epidural catheter for pain control and receives enoxaparin for clot prevention. The client now reports new back pain and cannot lift either leg off the bed. What should the nurse suspect?",
   [("Expected motor block from the epidural medication", False,
     "Low-dose epidural analgesia usually spares motor function; new weakness with back pain is a warning sign."),
    ("An epidural hematoma that is pressing on the spinal cord", True,
     "Back pain with new leg weakness in a client with an epidural and an anticoagulant suggests a hematoma, a surgical emergency."),
    ("Muscle stiffness from lying in bed for 2 days after surgery", False,
     "Stiffness does not cause inability to lift the legs."),
    ("A urinary tract infection spreading to the kidneys", False,
     "Kidney infection causes flank pain and fever, not leg weakness.")],
   "With an epidural catheter and anticoagulation, new back pain or leg weakness or numbness suggests epidural hematoma; notify the provider urgently, as decompression may be needed.",
   "Recognize epidural hematoma."),

sata(M(RRP, POSTOP, "generate_solutions", "moderate", F, "ms"),
   "A client is on the first day after an open bowel resection. Which interventions help prevent postoperative complications?",
   [("Walking in the hall at least three times today", True, "Early walking prevents clots, pneumonia and ileus."),
    ("Using the incentive spirometer 10 times an hour while awake", True, "Deep breathing prevents atelectasis."),
    ("Keeping the legs crossed in bed for comfort", False, "Crossing the legs slows venous return and raises clot risk."),
    ("Wearing sequential compression devices while in bed", True, "Compression devices reduce the risk of deep vein thrombosis."),
    ("Splinting the incision with a pillow when coughing", True, "Splinting reduces pain and lets the client cough effectively."),
    ("Staying in bed until the incision is fully healed", False, "Prolonged bed rest increases complications.")],
   "Postoperative prevention: early walking, incentive spirometry, compression devices, splinted coughing and avoiding leg crossing and prolonged bed rest.",
   "Prevent postoperative complications."),

mc(M(RRP, RENDO_DX, "analyze_cues", "moderate", "endocrine", "ms lab"),
   "A client reports fatigue, weight gain, constipation and feeling cold. Laboratory results show a TSH of 18 mU/L (high) and a free T4 of 0.4 ng/dL (low). How should the nurse interpret these results?",
   [("They suggest primary hypothyroidism from an underactive thyroid", True,
     "A high TSH with a low T4 shows the thyroid is underactive and the pituitary is trying to stimulate it."),
    ("They suggest hyperthyroidism from Graves' disease", False,
     "Graves' disease causes low TSH and high T4."),
    ("They suggest a pituitary tumor causing too much growth hormone", False,
     "Growth hormone excess does not cause this pattern."),
    ("They are normal results for an older adult and need no follow-up", False,
     "These results are clearly abnormal.")],
   "Primary hypothyroidism: high TSH and low free T4, with fatigue, weight gain, cold intolerance and constipation.",
   "Interpret thyroid function tests."),

# ---- physiological adaptation ----
bowtie(M(PA, THYROID, "prioritize_hypotheses", "difficult", "endocrine", "ms ph"),
   "A client with untreated Graves' disease is admitted with pneumonia. Today the client is agitated and confused, with a temperature of 40.2 °C (104.4 °F), heart rate 168/min in atrial fibrillation, blood pressure 164/62 mm Hg, vomiting and diarrhea. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Thyroid storm", True, "High fever, rapid atrial fibrillation, agitation and GI symptoms in untreated Graves' disease triggered by infection are thyroid storm."),
    ("Septic shock from pneumonia alone", False, "Sepsis causes fever, but a widened pulse pressure, rapid AF and agitation with Graves' disease point to thyroid storm."),
    ("Myxedema coma", False, "Myxedema coma causes low temperature and slow heart rate."),
    ("Heat stroke", False, "There is no history of heat exposure.")],
   [("Give the prescribed beta-blocker such as propranolol", True, "Beta-blockers control heart rate and adrenergic symptoms."),
    ("Give the prescribed antithyroid drug, followed later by iodine as ordered", True, "Antithyroid drugs block hormone production; iodine is given after to block release."),
    ("Give aspirin by mouth or rectally to bring down the high fever", False, "Aspirin can raise free thyroid hormone levels; acetaminophen is used instead."),
    ("Wrap the client in warm blankets to stop the shivering and chills", False, "Warming worsens hyperthermia."),
    ("Give the client's prescribed levothyroxine dose as soon as possible", False, "Thyroid hormone would worsen the crisis.")],
   [("Heart rate and rhythm", True, "Rapid AF and heart failure are major dangers."),
    ("Core temperature", True, "Shows whether cooling and treatment are working."),
    ("Body weight measured once a week", False, "Not useful in an acute crisis."),
    ("Visual acuity with an eye chart each day", False, "Not a key parameter in thyroid storm."),
    ("Serum albumin and total protein levels", False, "Not a guide to thyroid storm.")],
   "Thyroid storm: beta-blocker, antithyroid drug then iodine, steroids, cooling with acetaminophen (not aspirin) and close cardiac and temperature monitoring.",
   "Recognize and manage thyroid storm."),

mc(M(PA, DIAL, "analyze_cues", "difficult", "renal-urinary", "ms"),
   "A client on home peritoneal dialysis calls the clinic and says the fluid draining out this morning looks cloudy. The client also has mild abdominal pain and feels warm. What should the nurse suspect?",
   [("A normal change in the dialysate after a large meal", False,
     "Cloudy effluent is not a normal finding."),
    ("Peritonitis, an infection of the lining of the abdomen", True,
     "Cloudy effluent with abdominal pain and fever is the classic sign of peritonitis, the main complication of peritoneal dialysis."),
    ("Too much fluid being removed during each exchange", False,
     "Excess removal causes dehydration and low blood pressure, not cloudy fluid."),
    ("A blocked catheter that needs to be flushed at home", False,
     "A blocked catheter slows drainage but does not make it cloudy.")],
   "Cloudy peritoneal dialysis effluent is peritonitis until proven otherwise; the client needs a sample sent and antibiotics, often added to the dialysate.",
   "Recognize peritonitis in peritoneal dialysis."),

mc(M(PA, ASTHMA, "recognize_cues", "difficult", "respiratory", "ms"),
   "A client with a severe asthma attack has been wheezing loudly for an hour despite albuterol treatments. The client is now quieter, very drowsy, and the nurse hears almost no wheezing or air movement. SpO2 is 86%. What does this change most likely mean?",
   [("The bronchodilator has worked and the attack is ending", False,
     "Improvement would bring better air movement and alertness, not drowsiness and low oxygen."),
    ("The client is tiring and air movement is too poor to produce wheezing", True,
     "A \"silent chest\" with drowsiness signals impending respiratory failure; the client may need intubation."),
    ("The client has developed a pneumothorax on one side", False,
     "Pneumothorax causes absent sounds on one side, but this picture fits exhaustion and severe obstruction."),
    ("The client is falling asleep because the anxiety of the attack has finally resolved", False,
     "Drowsiness with low oxygen is a danger sign, not relief.")],
   "In severe asthma, decreasing wheezing with drowsiness and poor air movement is a silent chest, an emergency signaling respiratory failure.",
   "Recognize impending respiratory failure in asthma."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, INS, "take_action", "moderate", "pharmacology", "ph ada"),
   "A client with type 1 diabetes is prescribed insulin lispro before meals. The breakfast tray arrives at 0800. When should the nurse give the lispro?",
   [("Within 15 minutes before the client starts to eat the meal", True,
     "Rapid-acting insulin starts working within about 15 minutes, so it is given just before eating."),
    ("One hour before breakfast so the insulin is at its peak when the tray arrives", False,
     "Giving it an hour early can cause hypoglycemia before the meal."),
    ("At bedtime, so it covers the glucose rise overnight", False,
     "Lispro is a mealtime insulin, not an overnight insulin."),
    ("Two hours after breakfast, once the blood glucose has risen", False,
     "Giving it late leaves the meal uncovered and then risks hypoglycemia.")],
   "Rapid-acting insulins (lispro, aspart, glulisine) are given within 15 minutes before a meal; the tray should be present.",
   "Time rapid-acting insulin."),

mc(M(PPT, TECH, "take_action", "easy", "pharmacology", "ph"),
   "A client has a prescription for a new transdermal nitroglycerin patch each morning. Which action is correct?",
   [("Remove the old patch and use a different hairless site", True,
     "Removing the old patch prevents overdose, and rotating sites prevents skin irritation."),
    ("Place the new patch on top of the old one so the dose stays steady", False,
     "Layering patches causes an overdose."),
    ("Cut the patch in half if the client gets a headache", False,
     "Patches should not be cut unless the product allows it."),
    ("Apply the new patch to the same spot each day so it sticks better", False,
     "Using the same site irritates the skin.")],
   "Transdermal patches: remove the old patch, apply to clean, hairless, rotated sites, do not cut and dispose of safely.",
   "Apply a transdermal patch correctly."),
]

# ---- Case 1: sepsis from an anastomotic leak --------------------------------------------------
leak_tabs = [
    ("leak-notes", "Nurses' notes", [note("Surgical unit, day 4 after surgery, 0600",
        "A 64-year-old client is 4 days after a sigmoid colectomy with primary anastomosis for diverticular disease. "
        "Overnight, the client developed increasing abdominal pain and became restless. The abdomen is distended and "
        "tender throughout, and the client has not passed flatus since yesterday. The drain near the incision now has "
        "cloudy, foul-smelling fluid.", id="leak-note")]),
    ("leak-vitals", "Vital signs", [flow("Vital signs", ["2200", "0600"], [
        ("Temperature", ["37.8 °C (100 °F)", "39.1 °C (102.4 °F)"]),
        ("Heart rate", ["98/min", "126/min"]),
        ("Blood pressure", ["124/72 mm Hg", "88/48 mm Hg"]),
        ("Respiratory rate", ["20/min", "28/min"]),
        ("Urine output", ["45 mL/hr", "15 mL/hr"])], id="leak-flow")]),
    ("leak-labs", "Laboratory results", [table("Laboratory results, 0530", ["Result", "Reference"], [
        ("WBC", ["21,600/mm3", "5,000-10,000/mm3"]),
        ("Lactate", ["4.6 mmol/L", "below 2 mmol/L"]),
        ("Creatinine", ["1.8 mg/dL (baseline 0.9)", "0.6-1.2 mg/dL"])], id="leak-lab")]),
]

leak_qs = [
hl_text(M(RRP, POSTOP, "recognize_cues", "moderate", "gastrointestinal", "ms"),
   "Review the 0600 findings. Highlight the findings that require immediate follow-up.",
   ["The drain has ",
    ("cloudy, foul-smelling fluid", True, "Cloudy, foul drainage suggests a leak of bowel contents and infection."),
    ". ",
    ("The incision edges are together", False, "An intact incision is expected."),
    ". Blood pressure ",
    ("88/48 mm Hg", True, "Hypotension suggests septic shock."),
    ", temperature ",
    ("39.1 °C (102.4 °F)", True, "High fever signals infection."),
    ". Lactate ",
    ("4.6 mmol/L", True, "A lactate above 4 mmol/L shows severe hypoperfusion."),
    ". ",
    ("The client is wearing compression stockings", False, "Expected clot prevention."),
    "."],
   "Hypotension, fever, high lactate and foul drain fluid after bowel surgery suggest septic shock from an anastomotic leak.",
   "Recognize cues of sepsis after bowel surgery."),

cloze(M(PA, SEPSIS, "analyze_cues", "moderate", "gastrointestinal", "ms"),
   "Complete the sentence based on the client's data.",
   "The client is most likely experiencing {{b1}} caused by {{b2}}.",
   [[("septic shock", True, "Infection with hypotension and a lactate above 4 mmol/L fits septic shock."),
     ("hypovolemic shock from bleeding", False, "The drain fluid is cloudy and foul, not bloody, and there is fever."),
     ("cardiogenic shock", False, "Nothing points to heart failure.")],
    [("leakage at the bowel anastomosis", True, "Day 4 pain, distention and foul drain fluid after colectomy suggest anastomotic leak."),
     ("a urinary tract infection", False, "The drain and abdominal findings point to the bowel."),
     ("a reaction to the anesthetic", False, "Anesthetic effects do not appear on day 4 with fever and foul drainage.")]],
   "An anastomotic leak typically appears 3 to 7 days after bowel surgery with pain, fever, ileus and signs of sepsis.",
   "Analyze the cause of postoperative sepsis."),

matrix_mc(M(PA, SEPSIS, "prioritize_hypotheses", "difficult", "gastrointestinal", "ms"),
   "For each potential problem, indicate whether the client is at high or low immediate risk based on the current data.",
   ["High immediate risk", "Low immediate risk"],
   [("Acute kidney injury", 0, ["Urine output 15 mL/hr and creatinine doubled show the kidneys are affected.", "The data show high risk."]),
    ("Worsening shock and organ failure", 0, ["Hypotension and a high lactate show shock is progressing.", "The data show high risk."]),
    ("Wound dehiscence of the skin incision", 1, ["The incision edges are together.", "Nothing suggests dehiscence now."]),
    ("Respiratory failure", 0, ["Tachypnea from sepsis and abdominal distention can lead to respiratory failure.", "The rising respiratory rate shows high risk."])],
   "Septic shock threatens the kidneys, lungs and circulation; the intact incision is a lower immediate concern.",
   "Prioritize complications of septic shock."),

sata(M(PA, SEPSIS, "generate_solutions", "difficult", "gastrointestinal", "ms"),
   "Which actions should the nurse anticipate within the first hour?",
   [("Draw blood cultures before giving antibiotics", True, "Cultures first, without delaying antibiotics."),
    ("Give broad-spectrum IV antibiotics as prescribed", True, "Early antibiotics lower mortality."),
    ("Give a 30 mL/kg crystalloid bolus as prescribed", True, "Fluids treat hypotension and hypoperfusion."),
    ("Prepare the client for possible return to surgery", True, "A leak often needs surgical source control."),
    ("Start a regular diet to improve the client's strength", False, "The client needs to be NPO with an ileus and possible surgery."),
    ("Hold all IV fluids to avoid fluid overload", False, "The client is in shock and needs fluids."),
    ("Repeat the lactate level after the fluid bolus", True, "A falling lactate shows perfusion is improving.")],
   "Sepsis bundle: cultures, antibiotics, fluids, lactate and source control, such as surgery for an anastomotic leak.",
   "Plan initial care in septic shock."),

mc(M(PPT, ADV, "take_action", "moderate", "cardiovascular", "ph ms"),
   "After 2 L of crystalloid, the client's mean arterial pressure is 58 mm Hg, and norepinephrine is prescribed. The client has a peripheral IV in the forearm and a central line is being placed. While the infusion runs peripherally, the nurse sees the skin around the IV is cool, pale and swollen. What should the nurse do first?",
   [("Increase the rate of the norepinephrine to raise the pressure faster", False,
     "Extravasation needs the infusion stopped at that site, not increased."),
    ("Stop the infusion at that site and follow the extravasation protocol", True,
     "Norepinephrine extravasation can cause tissue necrosis; the infusion is moved and an antidote such as phentolamine may be given."),
    ("Apply a warm compress and keep the infusion running at the same rate", False,
     "The infusion must be stopped in that vein."),
    ("Elevate the arm and recheck the site again in 1 hour", False,
     "Waiting allows tissue damage to continue.")],
   "Vasopressors are given through a central line when possible; with peripheral extravasation, stop the infusion at that site and use the protocol, which may include phentolamine.",
   "Respond to vasopressor extravasation."),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "moderate", "gastrointestinal", "ms"),
   "On day 6, after surgical repair of the leak and antibiotics, the nurse reviews the client's status. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Mean arterial pressure 74 mm Hg off norepinephrine", 0, ["Blood pressure is stable without vasopressors.", "This shows improvement."]),
    ("Lactate 1.4 mmol/L", 0, ["Tissue perfusion has recovered.", "This shows improvement."]),
    ("Urine output 55 mL/hr", 0, ["Kidney perfusion has returned.", "This shows improvement."]),
    ("Temperature 38.9 °C (102 °F) with new chills", 1, ["A new fever spike may mean an abscess or new infection.", "This needs follow-up."])],
   "Stable blood pressure without vasopressors, normal lactate and good urine output show recovery; a new fever needs evaluation.",
   "Evaluate recovery from septic shock."),
]

# ---- Case 2: tension pneumothorax after chest trauma ------------------------------------------
ptx_tabs = [
    ("ptx-notes", "Nurses' notes", [note("Emergency department, 2140",
        "A 32-year-old client was the driver in a car crash and hit the steering wheel. The client has right-sided "
        "chest pain, bruising across the chest and is becoming more anxious and short of breath. Breath sounds are absent "
        "on the right. The trachea is shifted to the left, and the neck veins are distended.", id="ptx-note")]),
    ("ptx-vitals", "Vital signs", [flow("Vital signs", ["2125", "2140"], [
        ("Heart rate", ["108/min", "138/min"]),
        ("Blood pressure", ["124/80 mm Hg", "82/50 mm Hg"]),
        ("Respiratory rate", ["24/min", "36/min"]),
        ("SpO2", ["93% on 4 L/min", "82% on non-rebreather mask"])], id="ptx-flow")]),
]

ptx_qs = [
hl_table(M(PA, CHEST, "recognize_cues", "moderate", "respiratory", "ms"),
   "Review the 2140 findings. Highlight the findings that require immediate action.",
   ["Assessment", "Finding"],
   [(["Trachea", "Shifted to the left"], True, "Tracheal shift away from the injured side signals tension pneumothorax."),
    (["Blood pressure", "82/50 mm Hg"], True, "Falling blood pressure shows obstructed venous return."),
    (["SpO2", "82% on non-rebreather mask"], True, "Severe hypoxemia despite high-flow oxygen."),
    (["Chest wall", "Bruising across the chest"], False, "An expected finding from the injury; it does not change the immediate plan."),
    (["Breath sounds", "Absent on the right"], True, "Absent sounds show the right lung has collapsed.")],
   "Tracheal deviation, hypotension, severe hypoxemia and absent breath sounds point to tension pneumothorax.",
   "Recognize cues of tension pneumothorax."),

cloze(M(PA, CHEST, "analyze_cues", "moderate", "respiratory", "ms"),
   "Complete the sentence based on the client's data.",
   "The client's low blood pressure is caused by {{b1}}, which happens because {{b2}}.",
   [[("reduced venous return to the heart", True, "Pressure in the chest compresses the great veins and heart."),
     ("bleeding into the abdomen", False, "The chest findings explain the low blood pressure."),
     ("a heart attack caused by the crash", False, "Nothing suggests MI; the mechanism is obstruction.")],
    [("air is trapped and building pressure in the right chest", True, "A one-way leak lets air in but not out, shifting the mediastinum."),
     ("the left lung has collapsed", False, "The right side is affected."),
     ("the client is hyperventilating from anxiety", False, "Anxiety does not cause tracheal shift and hypotension.")]],
   "Tension pneumothorax traps air under pressure, collapsing the lung and pushing the mediastinum, which blocks venous return and causes shock.",
   "Analyze the mechanism of tension pneumothorax."),

bowtie(M(PA, CHEST, "prioritize_hypotheses", "difficult", "respiratory", "ms"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should anticipate, and 2 parameters the nurse should monitor.",
   [("Tension pneumothorax", True, "Absent breath sounds, tracheal shift, distended neck veins and hypotension after chest trauma indicate tension pneumothorax."),
    ("Simple pneumothorax", False, "A simple pneumothorax does not cause tracheal shift and shock."),
    ("Cardiac tamponade", False, "Tamponade causes muffled heart sounds without tracheal shift or absent breath sounds on one side."),
    ("Flail chest", False, "Flail chest causes paradoxical chest movement, not tracheal shift.")],
   [("Prepare for immediate needle decompression", True, "Needle decompression releases the trapped air and restores circulation."),
    ("Prepare for chest tube insertion after decompression", True, "A chest tube provides definitive drainage."),
    ("Wait for a chest x-ray to confirm before any treatment", False, "Tension pneumothorax is a clinical diagnosis; waiting is dangerous."),
    ("Lay the client flat with the legs raised", False, "Lying flat worsens breathing."),
    ("Give a sedative to calm the client's anxiety", False, "Sedation can worsen breathing and does not treat the cause.")],
   [("Oxygen saturation and respiratory rate", True, "Show whether the lung re-expands."),
    ("Blood pressure and heart rate", True, "Show whether circulation recovers after decompression."),
    ("Fingerstick blood glucose every 6 hours", False, "Not a guide to pneumothorax."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant."),
    ("Color and clarity of the urine", False, "Not relevant.")],
   "Tension pneumothorax is treated by immediate needle decompression, then a chest tube, with close monitoring of oxygenation and blood pressure.",
   "Prioritize care for tension pneumothorax."),

sata(M(RRP, POSTPROC, "take_action", "moderate", "respiratory", "ms"),
   "A chest tube is inserted on the right and connected to a drainage system with suction. Which actions should the nurse take?",
   [("Keep the drainage system below the level of the chest", True, "This prevents backflow into the pleural space."),
    ("Check that the water seal chamber has water at the correct level", True, "The water seal prevents air from returning to the chest."),
    ("Clamp the tube routinely each time the client is turned", False, "Routine clamping can cause a tension pneumothorax."),
    ("Keep sterile petroleum gauze and tape at the bedside", True, "Needed if the tube is accidentally pulled out."),
    ("Tape all connections securely", True, "Prevents accidental disconnection."),
    ("Strip the tubing every hour to keep it open", False, "Stripping creates high negative pressure and is not recommended.")],
   "Chest tube care: system below the chest, water seal at the right level, secure connections, emergency supplies at the bedside and no routine clamping or stripping.",
   "Manage a chest tube drainage system."),

mc(M(RRP, POSTPROC, "analyze_cues", "difficult", "respiratory", "ms"),
   "Two hours after the chest tube is placed, the nurse notes continuous bubbling in the water seal chamber that was not there before. The client's breathing is unchanged. What should the nurse do first?",
   [("Increase the suction to clear the extra air faster", False,
     "Changing suction does not find the source of a new leak."),
    ("Check the tubing and connections from the dressing down to find an air leak", True,
     "New continuous bubbling often means a leak in the system; checking from the client to the drainage unit finds it."),
    ("Clamp the chest tube near the client for an hour to stop the bubbling", False,
     "Prolonged clamping can cause a tension pneumothorax."),
    ("Remove the chest tube, since the bubbling shows that the lung has now fully re-expanded", False,
     "Bubbling does not mean the lung has re-expanded, and nurses do not remove chest tubes without orders.")],
   "Continuous bubbling in the water seal means an air leak; check the system for loose connections, then report if it persists, since it may come from the lung.",
   "Troubleshoot a chest tube air leak."),

matrix_mc(M(RRP, POSTPROC, "evaluate_outcomes", "moderate", "respiratory", "ms"),
   "On the next day, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Breath sounds heard on the right side", 0, ["The lung has re-expanded.", "This shows improvement."]),
    ("Tidaling in the water seal that rises and falls with breathing", 0, ["Tidaling shows the system is open and working.", "This is expected."]),
    ("SpO2 96% on 2 L/min", 0, ["Oxygenation has improved.", "This shows improvement."]),
    ("New crackling under the skin spreading up the neck", 1, ["Spreading subcutaneous emphysema may mean the tube is blocked or misplaced.", "This needs follow-up."])],
   "Re-expansion, tidaling and good oxygenation show recovery; spreading subcutaneous emphysema needs reporting.",
   "Evaluate the response to chest tube treatment."),
]

cases = [
    case("Septic shock from an anastomotic leak after colectomy", leak_tabs, leak_qs),
    case("Tension pneumothorax after a car crash", ptx_tabs, ptx_qs),
]

BUILT = build(83, "diagnostics-acute-care-cases",
              "Reduction of risk and physiological adaptation, with case studies on postoperative sepsis and tension pneumothorax",
              items, cases, wave=True)
