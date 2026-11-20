"""Batch 95 (wave 2, chunk 7): Reduction of risk potential and physiological adaptation, with two NGN unfolding
cases - status epilepticus and postpartum hemorrhage (PHI.RRP, PHI.PA, PHI.PPT).

Wave 2 keeps chunk 5's depth and uses fewer plain multiple-choice items.
Each case walks the six CJMM steps in order (NCSBN Clinical Judgment Measurement Model).

Written to NCLEX-RN difficulty on 20 November 2026 (see quality.py and PROGRESS.md)."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
F = "fundamentals"
CONTRAST = "monitoring-after-contrast-dye"
LYTE_LAB = "electrolyte-and-glucose-lab-results"
POSTOP = "postoperative-care"
SPEC = "specimen-collection"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
LABS = "laboratory-values-and-monitoring"
NEURO_DX = "neurological-assessment-and-procedures"
EYE = "eye-and-ear-disorders"
BURN = "burn-care"
ACS = "acute-coronary-syndrome"
INF_DEHY = "infant-dehydration"
ANTIARR = "heart-failure-interventions"
SEIZ = "seizure-management"
SEIZMED = "antiseizure-medications"
PPH = "postpartum-hemorrhage"
OBMED = "obstetric-medications"

items = [
# ---- reduction of risk potential ----
bowtie(M(RRP, CONTRAST, "prioritize_hypotheses", "difficult", "immune", "ms"),
   "Five minutes after IV iodinated contrast is given for a CT scan, a client reports itching and a tight throat. The nurse sees hives on the chest and hears wheezing. Blood pressure has dropped from 132/80 to 84/50 mm Hg, and heart rate is 124/min. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Anaphylactic reaction to the contrast", True, "Hives, wheezing, throat tightness and hypotension minutes after contrast show a severe reaction."),
    ("Contrast-associated kidney injury", False, "Kidney injury develops over 24 to 72 hours, not minutes."),
    ("Vasovagal reaction from anxiety", False, "Vasovagal reactions cause a slow heart rate without hives or wheezing."),
    ("Panic attack in the scanner", False, "Panic does not cause hives and hypotension.")],
   [("Stop the contrast and give IM epinephrine as prescribed", True, "Epinephrine is first-line for anaphylaxis."),
    ("Call for help and give oxygen and IV fluids", True, "Support breathing and circulation."),
    ("Give oral diphenhydramine and watch for 30 minutes", False, "Antihistamines alone do not treat anaphylaxis."),
    ("Finish the scan quickly before treating the reaction", False, "The reaction must be treated at once."),
    ("Sit the client upright with the legs hanging down", False, "Hypotension is treated with the client lying with the legs raised unless breathing needs otherwise.")],
   [("Blood pressure and heart rate", True, "Show the response to treatment."),
    ("Airway, breathing and SpO2", True, "Airway swelling can worsen quickly."),
    ("Urine color in the bag", False, "Not the key guide."),
    ("Blood glucose before meals", False, "Not the key guide."),
    ("Bowel sounds in each quadrant", False, "Not the key guide.")],
   "Contrast anaphylaxis: stop the contrast, give IM epinephrine, support airway and circulation and document the reaction for future premedication or alternatives.",
   "Recognize and treat a contrast reaction."),

matrix_mc(M(RRP, LYTE_LAB, "analyze_cues", "moderate", F, "lab"),
   "The nurse reviews the morning electrolytes of a client with heart failure who takes furosemide and is on a fluid restriction. For each result, indicate whether it is low, normal or high.",
   ["Low", "Normal", "High"],
   [("Sodium 128 mEq/L", 0, ["Below 135 mEq/L is low.", "Not normal.", "Not high."]),
    ("Potassium 4.1 mEq/L", 1, ["Not low.", "Within 3.5 to 5.0 mEq/L.", "Not high."]),
    ("Magnesium 1.2 mg/dL", 0, ["Below about 1.5 mg/dL is low.", "Not normal.", "Not high."]),
    ("Calcium 11.4 mg/dL", 2, ["Not low.", "Not normal.", "Above about 10.5 mg/dL is high."])],
   "Usual ranges: sodium 135 to 145 mEq/L, potassium 3.5 to 5.0 mEq/L, magnesium about 1.5 to 2.5 mg/dL and calcium about 8.5 to 10.5 mg/dL.",
   "Interpret electrolyte results."),

hl_table(M(RRP, POSTOP, "recognize_cues", "difficult", F, "ms"),
   "On the second day after a bowel resection, the nurse reviews the vital sign trend. Highlight the findings that suggest early sepsis.",
   ["Time", "Finding"],
   [(["0800", "Temperature 37.2 °C (99 °F), HR 84, RR 16, BP 128/76"], False, "Normal."),
    (["1200", "Temperature 38.3 °C (100.9 °F), HR 104"], True, "New fever and tachycardia."),
    (["1600", "RR 24, client newly confused about the date"], True, "Rapid breathing and new confusion are sepsis warning signs."),
    (["1600", "BP 112/70, urine output 45 mL/hr"], False, "Still acceptable."),
    (["2000", "BP 96/58, urine output 20 mL/hr"], True, "Falling blood pressure and low urine output suggest worsening sepsis.")],
   "Early sepsis: fever, tachycardia, rapid breathing, new confusion, falling blood pressure and low urine output; act on the trend.",
   "Recognize early sepsis from a vital sign trend."),

order(M(RRP, SPEC, "take_action", "moderate", "respiratory", "fund lab"),
   "The nurse is collecting a sputum specimen for culture. Place the steps in order.",
   [("Plan the collection for early morning, before breakfast", "Secretions build up overnight."),
    ("Have the client rinse the mouth with water", "Reduces contamination from mouth bacteria."),
    ("Have the client take deep breaths and cough deeply into a sterile container", "Brings up sputum from the lungs."),
    ("Label the specimen and send it to the lab promptly", "Prevents overgrowth and mislabeling.")],
   "Sputum culture: early morning, rinse with water, deep cough into a sterile cup, label at the bedside and send promptly.",
   "Collect a sputum specimen."),

sata(M(RRP, CV_DX, "take_action", "difficult", "cardiovascular", "ms"),
   "A client with type 2 diabetes is scheduled for a cardiac catheterization in the morning. Which actions should the nurse take before the procedure?",
   [("Ask about earlier reactions to contrast dye", True, "Previous reactions guide premedication."),
    ("Check the serum creatinine", True, "Kidney function affects contrast risk."),
    ("Ask the provider about holding metformin around the procedure", True, "Metformin is held in at-risk clients to prevent lactic acidosis."),
    ("Mark and record the pedal pulses", True, "A baseline helps detect circulation problems afterward."),
    ("Give a large high-fat breakfast to prevent hypoglycemia", False, "The client is usually NPO before the procedure."),
    ("Apply a heating pad to the groin to dilate the artery", False, "This is not part of preparation.")],
   "Before catheterization: contrast allergy history, creatinine, metformin plan, NPO status, consent and baseline pulses.",
   "Prepare a client for cardiac catheterization."),

mc(M(RRP, CV_DX, "analyze_cues", "moderate", "cardiovascular", "ms"),
   "A client's telemetry shows a new irregularly irregular rhythm at 132/min with no clear P waves. The client feels palpitations but is alert, with a blood pressure of 124/78 mm Hg. Which risk should the nurse keep in mind when planning care?",
   [("Rapid progression to complete heart block", False,
     "Atrial fibrillation does not usually lead to heart block."),
    ("Stroke from a clot that forms in the atria", True,
     "Atrial fibrillation lets blood pool in the atria and form clots that can travel to the brain."),
    ("Hypoglycemia from a rapid heart rate", False,
     "Not a typical effect."),
    ("Pneumonia from reduced lung expansion during the fast rhythm", False,
     "Not a typical effect.")],
   "New atrial fibrillation raises stroke risk; rate control, anticoagulation assessment and monitoring for symptoms are key.",
   "Recognize the stroke risk of atrial fibrillation."),

mc(M(RRP, POSTOP, "take_action", "difficult", "renal-urinary", "ms"),
   "On the first day after a hysterectomy, a client's urine output has been 20 mL/hr for 2 hours through an indwelling catheter. Heart rate is 96/min and blood pressure 108/66 mm Hg. What should the nurse do first?",
   [("Increase the IV fluid rate to 250 mL/hr", False,
     "The nurse needs a prescription and should first check the cause."),
    ("Look for a kinked or blocked catheter, then assess fluid status", True,
     "A blocked catheter is a quick, fixable cause; then the nurse assesses for hypovolemia and reports."),
    ("Give a dose of IV furosemide to increase the client's urine output", False,
     "Diuretics can worsen hypovolemia and need a prescription."),
    ("Document the output and recheck it at the end of the shift", False,
     "Low output for 2 hours needs action now.")],
   "Low urine output after surgery: check the catheter first, then assess volume status and report; never give diuretics without assessing for hypovolemia.",
   "Respond to low urine output after surgery."),

cloze(M(RRP, LABS, "analyze_cues", "moderate", "hematologic-oncologic", "lab ph"),
   "A client taking warfarin for a mechanical heart valve (target INR 2.5 to 3.5) has these results: INR 4.6 and platelets 48,000/mm3. Complete the sentences.",
   "The INR is {{b1}} the client's target range. The platelet count means the nurse should {{b2}}.",
   [[("above", True, "4.6 is above the 2.5 to 3.5 target."),
     ("within", False, "4.6 is outside the range."),
     ("below", False, "4.6 is above the range.")],
    [("use bleeding precautions, such as a soft toothbrush and no IM injections", True, "Low platelets increase bleeding risk."),
     ("encourage contact sports to raise the platelets", False, "Activity does not raise platelets and raises injury risk."),
     ("give aspirin to prevent clots", False, "Aspirin increases bleeding.")]],
   "High INR and low platelets both raise bleeding risk; report them and use bleeding precautions.",
   "Interpret coagulation results."),

# ---- physiological adaptation ----
bowtie(M(PA, EYE, "prioritize_hypotheses", "difficult", "neurological", "ms"),
   "A 64-year-old client comes to the emergency department after watching a movie in a dark theater. The client has sudden severe pain in the right eye, blurred vision with halos around lights, a headache, nausea and vomiting. The right eye is red, and the pupil is mid-dilated and does not react to light. Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Acute angle-closure glaucoma", True, "Sudden eye pain, halos, a red eye, a fixed mid-dilated pupil and vomiting after time in the dark point to angle-closure glaucoma."),
    ("Migraine with aura", False, "Migraine does not cause a red eye with a fixed pupil."),
    ("Conjunctivitis", False, "Conjunctivitis does not cause severe pain and a fixed pupil."),
    ("Retinal detachment", False, "Detachment causes painless flashes and a curtain over vision.")],
   [("Notify the provider and prepare drugs to lower eye pressure", True, "This is an emergency that can cause blindness within hours."),
    ("Give the prescribed antiemetic and analgesic", True, "Treating nausea and pain lowers pressure from vomiting and eases distress."),
    ("Dilate the pupil further with prescribed drops to help the eye drain", False, "Dilating drops worsen angle closure."),
    ("Patch the eye and send the client home to rest in a dark room", False, "This delays emergency treatment."),
    ("Give an oral decongestant for the headache and stuffy nose", False, "Some decongestants can worsen angle closure.")],
   [("Eye pain and visual acuity", True, "Show whether pressure is falling."),
    ("Intraocular pressure readings", True, "Directly show the response to treatment."),
    ("Fingerstick blood glucose before meals", False, "Not the key guide."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant."),
    ("Urine output measured each hour", False, "Not the key guide.")],
   "Acute angle-closure glaucoma is an emergency: lower eye pressure quickly with drops and systemic drugs, avoid dilating drugs and prepare for laser treatment.",
   "Recognize acute angle-closure glaucoma."),

matrix_mc(M(PA, BURN, "analyze_cues", "difficult", "integumentary", "ms"),
   "A client has burns of different depths after a kitchen fire. For each area, indicate the burn depth.",
   ["Superficial", "Superficial partial-thickness", "Deep partial-thickness", "Full-thickness"],
   [("Red, dry, painful skin on the forearm that blanches, with no blisters", 0,
     ["Redness without blisters is superficial.", "These have blisters.", "These are deeper.", "These are painless and leathery."]),
    ("Moist, pink skin with large blisters on the hand that is very painful", 1,
     ["Superficial burns have no blisters.", "Moist, blistered and very painful is superficial partial-thickness.", "Deep partial burns are paler and less moist.", "Full-thickness burns are not painful."]),
    ("Pale, mottled skin on the chest with reduced sensation and slow capillary refill", 2,
     ["Too deep.", "These are moist and pink.", "Pale, mottled skin with reduced sensation is deep partial-thickness.", "Full-thickness burns are leathery and insensate."]),
    ("White, leathery, dry skin on the leg with no pain when touched", 3,
     ["Too deep.", "Too deep.", "Too deep.", "Leathery, painless skin is full-thickness."])],
   "Burn depth: superficial (red, no blisters), superficial partial (moist, blistered, painful), deep partial (pale, less sensation), full-thickness (leathery, insensate).",
   "Classify burn depth."),

mc(M(PA, ACS, "take_action", "difficult", "cardiovascular", "aha ms"),
   "A 59-year-old client arrives at the emergency department with chest pressure that started 30 minutes ago and spreads to the left arm. The client is sweating and anxious. Which action should the nurse take first?",
   [("Draw a troponin level and wait for the result before doing anything else", False,
     "Troponin may be normal early; the ECG is needed first."),
    ("Get a 12-lead ECG within 10 minutes of the client's arrival", True,
     "A rapid ECG identifies STEMI, which needs emergency reperfusion."),
    ("Give a large glass of water and have the client lie flat", False,
     "This does not treat or diagnose the problem."),
    ("Send the client for a chest x-ray to look for pneumonia", False,
     "The ECG takes priority.")],
   "Suspected acute coronary syndrome: 12-lead ECG within 10 minutes, aspirin unless contraindicated, oxygen if hypoxemic, and rapid reperfusion for STEMI.",
   "Act first for suspected acute coronary syndrome."),

mc(M(PA, INF_DEHY, "recognize_cues", "difficult", "pediatrics", "peds"),
   "A 9-month-old who attends day care has had vomiting and watery diarrhea for 2 days and has refused the bottle since this morning. Which group of findings indicates severe dehydration?",
   [("Moist mouth, tears present and a soft, flat, normal fontanelle", False,
     "These suggest little or no dehydration."),
    ("Sunken fontanelle, no tears, lethargy and slow refill", True,
     "These show severe dehydration with poor perfusion."),
    ("Slightly dry mouth and fewer wet diapers than usual", False,
     "These suggest mild dehydration."),
    ("Normal heart rate, alert and drinking eagerly", False,
     "These suggest mild or no dehydration.")],
   "Severe dehydration in infants: lethargy, sunken fontanelle and eyes, no tears, very dry mouth, cool mottled skin and delayed capillary refill.",
   "Recognize severe dehydration in an infant."),

# ---- pharmacological and parenteral therapies ----
mc(M(PPT, ANTIARR, "evaluate_outcomes", "difficult", "cardiovascular", "ph"),
   "A client with stable ventricular tachycardia with a pulse is receiving an IV amiodarone infusion. Which finding should the nurse report right away?",
   [("Heart rate has slowed from 168/min to 92/min in sinus rhythm", False,
     "This shows the drug is working."),
    ("Blood pressure has fallen to 78/44 mm Hg with lightheadedness", True,
     "IV amiodarone can cause hypotension and bradycardia; this needs action."),
    ("The client reports a metallic taste in the mouth", False,
     "This is a minor effect."),
    ("The IV site is slightly red along the vein without any swelling", False,
     "Monitor the site, but hypotension is more urgent.")],
   "IV amiodarone can cause hypotension, bradycardia and phlebitis; monitor blood pressure and rhythm closely.",
   "Monitor IV amiodarone."),
]

# ---- Case 1: status epilepticus ----------------------------------------------------------------
se_tabs = [
    ("se-notes", "Nurses' notes", [note("Medical unit, 1400",
        "A 41-year-old client with epilepsy was admitted with pneumonia. The client has missed two doses of "
        "levetiracetam because of vomiting. At 1355 the client began a generalized tonic-clonic seizure. The seizure "
        "is still going at 1402, and the client is cyanotic around the lips with drooling.", id="se-note")]),
    ("se-vitals", "Vital signs", [flow("Vital signs", ["1340", "1402"], [
        ("Heart rate", ["96/min", "142/min"]),
        ("Blood pressure", ["124/76 mm Hg", "168/94 mm Hg"]),
        ("SpO2", ["95% on 2 L/min", "84% on 2 L/min"]),
        ("Fingerstick glucose", ["-", "118 mg/dL"])], id="se-flow")]),
]

se_qs = [
hl_text(M(PA, SEIZ, "recognize_cues", "moderate", "neurological", "ms"),
   "Review the 1402 findings. Highlight the findings that require immediate action.",
   ["The seizure ",
    ("has lasted 7 minutes", True, "A seizure longer than 5 minutes is status epilepticus."),
    ". The client is ",
    ("cyanotic around the lips with an SpO2 of 84%", True, "Hypoxemia needs airway support and oxygen."),
    ". ",
    ("Fingerstick glucose is 118 mg/dL", False, "Normal; hypoglycemia is ruled out."),
    ". The client ",
    ("missed two doses of levetiracetam", True, "Missed antiseizure doses are a likely trigger."),
    "."],
   "A seizure lasting more than 5 minutes, hypoxemia and missed antiseizure doses need immediate action.",
   "Recognize status epilepticus."),

cloze(M(PA, SEIZ, "analyze_cues", "moderate", "neurological", "ms"),
   "Complete the sentence based on the client's data.",
   "The client is in {{b1}}, most likely triggered by {{b2}}.",
   [[("status epilepticus", True, "Seizure activity longer than 5 minutes is status epilepticus."),
     ("a simple febrile seizure", False, "Febrile seizures occur in young children and are brief."),
     ("a syncopal episode", False, "Syncope is brief and does not cause prolonged convulsions.")],
    [("missed levetiracetam doses and an acute infection", True, "Missed doses and illness lower the seizure threshold."),
     ("low blood sugar", False, "Glucose is normal."),
     ("too much oxygen", False, "Oxygen does not trigger seizures.")]],
   "Missed antiseizure medication and acute illness are common triggers of status epilepticus.",
   "Analyze the cause of status epilepticus."),

matrix_mc(M(RRP, NEURO_DX, "prioritize_hypotheses", "difficult", "neurological", "ms lab"),
   "For each laboratory test, indicate whether it is a priority to obtain now to find a reversible cause of the seizure.",
   ["Priority now", "Not a priority now"],
   [("Serum sodium", 0, ["Low sodium can cause seizures.", "It is a priority."]),
    ("Levetiracetam level or medication history", 0, ["Low drug levels are a likely cause.", "It is a priority."]),
    ("Serum cholesterol", 1, ["Not related to the acute seizure.", "Not a priority now."]),
    ("Calcium and magnesium", 0, ["Low levels can cause seizures.", "They are a priority."])],
   "In status epilepticus, check glucose, sodium, calcium, magnesium, drug levels and toxicology to find reversible causes.",
   "Prioritize tests in status epilepticus."),

sata(M(RRP, NEURO_DX, "generate_solutions", "difficult", "neurological", "ms"),
   "Which actions should the nurse take right away?",
   [("Turn the client onto the side and suction the mouth as needed", True, "Protects the airway."),
    ("Give oxygen by non-rebreather mask", True, "Corrects hypoxemia."),
    ("Note the time the seizure started and its features", True, "Guides treatment and diagnosis."),
    ("Place a bite block between the teeth", False, "Nothing goes in the mouth during a seizure."),
    ("Hold the arms and legs down to stop the movements", False, "Restraining causes injury.")],
   "During status epilepticus: protect the airway, give oxygen, prevent injury, time the seizure and prepare emergency drugs.",
   "Take immediate action in status epilepticus."),

mc(M(PPT, SEIZMED, "take_action", "difficult", "neurological", "ph"),
   "The client has an IV in place. Which medication should the nurse expect to give first to stop the seizure?",
   [("IV fosphenytoin as the first drug", False,
     "Fosphenytoin is a second-line drug after a benzodiazepine."),
    ("IV lorazepam, given right away as the first drug", True,
     "A benzodiazepine such as IV lorazepam is the first-line treatment for status epilepticus."),
    ("Oral levetiracetam through a nasogastric tube", False,
     "Oral drugs act too slowly and are unsafe during a seizure."),
    ("IV haloperidol to calm the jerking movements quickly", False,
     "Haloperidol lowers the seizure threshold.")],
   "Status epilepticus: benzodiazepine first (IV lorazepam, or IM midazolam), then a second-line drug such as fosphenytoin, levetiracetam or valproate.",
   "Choose first-line treatment for status epilepticus."),

matrix_mc(M(PA, SEIZ, "evaluate_outcomes", "moderate", "neurological", "ms"),
   "One hour after treatment, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("No further seizure activity", 0, ["The seizure has stopped.", "This shows improvement."]),
    ("SpO2 96% on 2 L/min", 0, ["Oxygenation has recovered.", "This shows improvement."]),
    ("Drowsy but arouses to voice", 0, ["Postictal drowsiness is expected.", "This is expected."]),
    ("Respiratory rate 8/min and difficult to arouse", 1, ["This may be oversedation from the benzodiazepine.", "This needs follow-up."])],
   "After status epilepticus treatment, expect postictal drowsiness; slow breathing and difficulty arousing may mean oversedation.",
   "Evaluate the response to seizure treatment."),
]

# ---- Case 2: postpartum hemorrhage -------------------------------------------------------------
pph_tabs = [
    ("pph-notes", "Nurses' notes", [note("Labor and delivery, 30 minutes after birth",
        "A client gave birth vaginally to a 4.3 kg (9.5 lb) infant after a 22-hour labor with oxytocin augmentation. "
        "The client has asthma and had magnesium sulfate earlier for preeclampsia, which has been stopped. The nurse "
        "finds the fundus soft and 2 cm above the umbilicus, and the pad is soaked with bright red blood and clots.", id="pph-note")]),
    ("pph-vitals", "Vital signs", [flow("Vital signs", ["15 min after birth", "30 min after birth"], [
        ("Heart rate", ["88/min", "122/min"]),
        ("Blood pressure", ["132/84 mm Hg", "94/56 mm Hg"]),
        ("Quantitative blood loss", ["350 mL", "1,100 mL"])], id="pph-flow")]),
]

pph_qs = [
hl_table(M(RRP, LABS, "recognize_cues", "moderate", "maternal-newborn", "ob"),
   "Review the 30-minute findings. Highlight the findings that require immediate action.",
   ["Assessment", "Finding"],
   [(["Fundus", "Soft, 2 cm above the umbilicus"], True, "A soft, high fundus means uterine atony."),
    (["Quantitative blood loss", "1,100 mL"], True, "Blood loss of 1,000 mL or more is a postpartum hemorrhage."),
    (["Heart rate", "122/min"], True, "Tachycardia signals blood loss."),
    (["Infant weight", "4.3 kg"], False, "A risk factor, not a current finding to act on."),
    (["Blood pressure", "94/56 mm Hg"], True, "Falling blood pressure shows hypovolemia.")],
   "A soft, high fundus, blood loss of 1,000 mL or more, tachycardia and hypotension show postpartum hemorrhage.",
   "Recognize postpartum hemorrhage."),

cloze(M(PA, PPH, "analyze_cues", "moderate", "maternal-newborn", "ob"),
   "Complete the sentence based on the client's data.",
   "The bleeding is most likely caused by {{b1}}, and the client's risk factors include {{b2}}.",
   [[("uterine atony", True, "A soft, boggy uterus is the most common cause of postpartum hemorrhage."),
     ("a cervical laceration", False, "Lacerations bleed with a firm uterus."),
     ("a clotting disorder", False, "A boggy uterus points to atony.")],
    [("a large baby, long labor, oxytocin and magnesium", True, "These overstretch or tire the uterus or relax its muscle."),
     ("the client's asthma", False, "Asthma is not a risk factor for atony."),
     ("a short labor with no medications", False, "This does not match the history.")]],
   "Risk factors for uterine atony include a large baby, long or induced labor, oxytocin use, magnesium and overdistention.",
   "Analyze the cause of postpartum hemorrhage."),

bowtie(M(PA, PPH, "prioritize_hypotheses", "difficult", "maternal-newborn", "ob ph"),
   "Complete the diagram by placing the condition the client is most likely experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Postpartum hemorrhage from uterine atony", True, "A boggy uterus with heavy bleeding and signs of hypovolemia."),
    ("Amniotic fluid embolism", False, "This causes sudden collapse and breathing failure."),
    ("Retained placenta", False, "The placenta has been delivered; atony is the more likely cause."),
    ("Uterine rupture", False, "Rupture usually happens during labor with severe pain.")],
   [("Massage the fundus firmly", True, "Massage helps the uterus contract."),
    ("Increase the oxytocin infusion as prescribed", True, "Oxytocin is the first-line uterotonic."),
    ("Give methylergonovine, the fastest available drug", False, "It is contraindicated with the client's preeclampsia history."),
    ("Give carboprost as the first drug", False, "Carboprost is contraindicated with asthma."),
    ("Wait for the next pad check in 15 minutes", False, "Delay worsens blood loss.")],
   [("Fundal tone and amount of bleeding", True, "Show whether the uterus is contracting and bleeding is slowing."),
    ("Heart rate and blood pressure", True, "Show the response to treatment."),
    ("Deep tendon reflexes at the knees", False, "Magnesium has been stopped; reflexes are not the main guide now."),
    ("Weight of the infant at the next feeding", False, "Not relevant."),
    ("Bowel sounds in each of the four quadrants", False, "Not relevant.")],
   "PPH from atony: fundal massage, oxytocin, other uterotonics chosen around contraindications, IV fluids and blood, with monitoring of tone, blood loss and vital signs.",
   "Prioritize care for postpartum hemorrhage."),

sata(M(RRP, LABS, "generate_solutions", "moderate", "maternal-newborn", "ob lab"),
   "Which actions should the nurse anticipate while the team treats the hemorrhage?",
   [("Send blood for a type and crossmatch", True, "Blood may be needed."),
    ("Draw a complete blood count and clotting studies", True, "Guides replacement and checks for coagulopathy."),
    ("Weigh pads and drapes to measure blood loss", True, "Quantitative blood loss is more accurate than estimates."),
    ("Insert a second large-bore IV catheter", True, "Allows fluids and blood to be given quickly."),
    ("Delay labs until the bleeding stops", False, "Labs guide treatment now."),
    ("Empty the bladder with a catheter if it is full", True, "A full bladder prevents the uterus from contracting.")],
   "PPH care includes type and crossmatch, CBC and clotting studies, measured blood loss, two large IVs and bladder emptying.",
   "Plan supportive care for postpartum hemorrhage."),

mc(M(PPT, OBMED, "take_action", "difficult", "maternal-newborn", "ob ph"),
   "Bleeding continues despite fundal massage and oxytocin. Which additional uterotonic is safest for this client?",
   [("Methylergonovine 0.2 mg IM", False,
     "Contraindicated with hypertension and preeclampsia history."),
    ("Misoprostol as prescribed", True,
     "Misoprostol can be used with asthma and hypertension."),
    ("Carboprost 250 mcg IM", False,
     "Carboprost can cause bronchospasm in clients with asthma."),
    ("Magnesium sulfate restarted at 2 g/hr", False,
     "Magnesium relaxes the uterus and worsens atony.")],
   "Choose uterotonics around contraindications: avoid methylergonovine with hypertension and carboprost with asthma; misoprostol and tranexamic acid are options.",
   "Choose a uterotonic safely."),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "moderate", "maternal-newborn", "ob"),
   "One hour later, the nurse reassesses the client. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Fundus firm at the umbilicus", 0, ["The uterus is contracting.", "This shows improvement."]),
    ("Heart rate 94/min and BP 112/70 mm Hg", 0, ["Vital signs are recovering.", "This shows improvement."]),
    ("Hemoglobin 7.2 g/dL after 2 units of red cells", 1, ["Hemoglobin is still low.", "More replacement may be needed."]),
    ("Pad with a small amount of lochia in the last 30 minutes", 0, ["Bleeding has slowed.", "This shows improvement."])],
   "A firm fundus, stable vital signs and reduced bleeding show response; a low hemoglobin needs follow-up.",
   "Evaluate the response to treatment of postpartum hemorrhage."),
]

cases = [
    case("Status epilepticus after missed antiseizure doses", se_tabs, se_qs),
    case("Postpartum hemorrhage from uterine atony", pph_tabs, pph_qs),
]

BUILT = build(95, "diagnostics-emergencies-cases",
              "Reduction of risk and physiological adaptation, with case studies on status epilepticus and postpartum hemorrhage",
              items, cases, wave=True)
