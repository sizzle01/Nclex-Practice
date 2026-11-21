"""Batch 104 (case conversion, chunk 2: pulmonary): four full NGN case studies (CASE-MAP.md).

Cases: severe community-acquired pneumonia in an older adult; status asthmaticus in an adult; COPD with
hypercapnia made worse by too much oxygen; active pulmonary tuberculosis. Each case walks the six CJMM steps in
order, unfolds with new data in steps 4 to 6, and includes at least one management of care, safety or health
promotion question (plan of 2026-11-20, from the candidate's NCLEX report)."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
RESP = "respiratory"
PNEU = "pneumonia-care"
RESP_MEDS = "respiratory-medications"
ACID = "acid-base-imbalances"
OXY = "oxygen-therapy-and-devices"
ASTHMA = "asthma-and-copd-management"
TBP = "transmission-based-precautions"
ABX = "antimicrobial-therapy"
LABS = "laboratory-values-and-monitoring"
RESP_DX = "respiratory-diagnostic-procedures"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
HAI = "healthcare-associated-infection-prevention"

items = []

# ================================================================================================
# Case 1: Severe community-acquired pneumonia in an older adult
# ================================================================================================
cap_tabs = [
    ("cap-notes", "Nurses' notes", [note("Emergency department, 0900",
        "An 82-year-old client is brought in by a daughter, who says the client has been \"not making sense\" since "
        "last night and has had a cough with rust-colored sputum for 3 days. The client lives alone and takes "
        "citalopram 20 mg daily, lisinopril 10 mg daily and metformin. Crackles and bronchial breath sounds over "
        "the right lower lobe. Oriented to person only. Chest x-ray: right lower lobe consolidation.",
        id="cap-note")]),
    ("cap-vitals", "Vital signs", [flow("Vital signs", ["0900"], [
        ("Temperature", ["38.6 C (101.5 F)"]),
        ("Blood pressure", ["96/58 mm Hg"]),
        ("Heart rate", ["112/min"]),
        ("Respirations", ["28/min"]),
        ("SpO2", ["88% on room air"])], id="cap-flow")]),
    ("cap-labs", "Laboratory results", [table("Laboratory results", ["0900", "Reference range"], [
        ("WBC", ["16,800/mm3", "5,000 to 10,000/mm3"]),
        ("BUN", ["28 mg/dL", "10 to 20 mg/dL"]),
        ("Creatinine", ["1.3 mg/dL", "0.6 to 1.2 mg/dL"]),
        ("Lactate", ["2.4 mmol/L", "0.5 to 2.0 mmol/L"]),
        ("Glucose", ["188 mg/dL", "70 to 110 mg/dL"])], id="cap-lab")]),
]

cap_qs = [
hl_table(M(PA, PNEU, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Assessment", "Finding"],
   [(["Mental status", "New confusion, oriented to person only"], True,
     "New confusion in an older adult with infection signals sepsis or hypoxemia."),
    (["SpO2", "88% on room air"], True, "Hypoxemia needs oxygen now."),
    (["Respirations", "28/min"], True, "Tachypnea is an early sign of respiratory failure and sepsis."),
    (["Blood pressure", "96/58 mm Hg"], True, "Low pressure with infection suggests sepsis."),
    (["Lactate", "2.4 mmol/L"], True, "Raised lactate shows tissue hypoperfusion."),
    (["Glucose", "188 mg/dL"], False, "Stress hyperglycemia needs monitoring but is not the priority."),
    (["Creatinine", "1.3 mg/dL"], False, "A slight rise to watch; it matters for drug dosing but is not urgent.")],
   "Confusion, hypoxemia, tachypnea, low pressure and raised lactate show severe pneumonia with sepsis.",
   "Recognize cues of severe pneumonia in an older adult."),

matrix_mc(M(PA, PNEU, "analyze_cues", "difficult", RESP, "ms"),
   "The provider uses the CURB-65 score to decide where the client is treated. For each finding, indicate what it "
   "contributes.",
   ["Adds a CURB-65 point", "Supports pneumonia but is not a CURB-65 point"],
   [("New confusion", 0, "C: confusion."),
    ("BUN 28 mg/dL", 0, "U: urea (BUN) above 19 mg/dL."),
    ("Respirations 28/min", 1, "R counts a rate of 30 or more, so 28 adds no point, though the fast rate still shows severe illness."),
    ("Blood pressure 96/58 mm Hg", 0, "B: systolic below 90 or diastolic 60 or below; 58 counts."),
    ("Age 82", 0, "65: age 65 or older."),
    ("Crackles with bronchial breath sounds", 1, "Consolidation signs support pneumonia but are not in the score."),
    ("WBC 16,800/mm3", 1, "Supports bacterial infection; not in the score.")],
   "CURB-65 counts confusion, urea, respiratory rate of 30 or more, low blood pressure and age 65 or more. This "
   "client scores 4, which calls for intensive care.",
   "Analyze pneumonia severity with CURB-65."),

cloze(M(PA, PNEU, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's priority problem is {{b1}}. With a CURB-65 score of {{b2}}, the client should be treated "
   "{{b3}}.",
   [[("impaired gas exchange with sepsis", True, "Hypoxemia plus low pressure and lactate threaten oxygen delivery."),
     ("acute kidney injury", False, "Creatinine is only slightly raised."),
     ("dementia", False, "The confusion is new, so it is delirium from illness."),
     ("dehydration alone", False, "Fluid loss contributes but does not explain the hypoxemia.")],
    [("4", True, "Confusion, BUN, blood pressure and age; the rate of 28 is below 30."),
     ("5", False, "The respiratory rate is below 30."),
     ("2", False, "Too low; four criteria are met."),
     ("3", False, "Four criteria are met.")],
    [("in the intensive care unit", True, "A score of 4 or 5 means high mortality and calls for ICU care."),
     ("at home with oral antibiotics", False, "Home care suits a score of 0 or 1."),
     ("in an observation unit overnight", False, "Too little monitoring."),
     ("in a skilled nursing facility", False, "Not for an acutely unstable client.")]],
   "Severe pneumonia with sepsis threatens gas exchange; CURB-65 of 4 calls for intensive care.",
   "Prioritize problems and level of care in severe pneumonia."),

matrix_mc(M(SIC, TBP, "generate_solutions", "difficult", RESP, "cdc fund"),
   "The influenza PCR is pending. For each safety measure, indicate whether it is indicated or contraindicated now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Droplet precautions until the influenza result is back", 0,
     "Influenza spreads by droplets; precautions continue until it is ruled out."),
    ("A negative-pressure airborne isolation room", 1, "Bacterial and influenza pneumonia do not need airborne isolation."),
    ("A low bed with the bed alarm on", 0, "Delirium raises the risk of falls."),
    ("Soft wrist restraints to stop the client pulling at the oxygen", 2,
     "Restraints are a last resort; reorientation, family presence and close observation come first, and restraints "
     "worsen delirium."),
    ("Head of the bed at 30 degrees or more during care and meals", 0, "Reduces aspiration in a confused older adult."),
    ("A sitter or video monitoring if confusion increases", 0, "Close observation is less restrictive than restraint.")],
   "Isolate for a possible droplet infection, prevent falls and aspiration, and use the least restrictive measures "
   "for delirium.",
   "Generate safety and infection-control measures for a confused client with pneumonia.",
   ex=[note("Admission orders, 0930",
       "Influenza and COVID-19 PCR sent. Admit to ICU. Fall precautions. The client has pulled at the nasal "
       "cannula twice.", id="cap-orders")]),

sata(M(PPT, ABX, "take_action", "difficult", RESP, "ph ms"),
   "Review the 0945 data. Which actions should the nurse take?",
   [("Draw blood cultures from two sites before the first antibiotic", True,
     "Cultures first, so the organism can be found, without delaying antibiotics past the first hour."),
    ("Give the ceftriaxone within the first hour", True, "Early antibiotics lower deaths in sepsis."),
    ("Hold the azithromycin and tell the provider the QTc", True,
     "Azithromycin and citalopram both prolong the QT; with a QTc of 510 ms, another drug such as doxycycline is safer."),
    ("Titrate oxygen to an SpO2 of 92% to 96%", True, "Corrects hypoxemia without excess oxygen."),
    ("Give metformin with breakfast", False,
     "Metformin is held in sepsis with hypoperfusion and a raised lactate because of lactic acidosis risk."),
    ("Wait for the sputum culture before starting antibiotics", False, "Culture results take days.")],
   "In severe pneumonia with sepsis: cultures, then antibiotics within the hour, oxygen to target, and a check "
   "for drug interactions and unsafe drugs before giving them.",
   "Take action in the first hour of severe pneumonia.",
   ex=[flow("0945", ["0945"], [
       ("12-lead ECG", ["Sinus tachycardia, QTc 510 ms"]),
       ("Antibiotics prescribed", ["Ceftriaxone 1 g IV and azithromycin 500 mg IV"]),
       ("Blood cultures", ["Not yet drawn"]),
       ("SpO2", ["90% on 2 L/min nasal cannula"])], id="cap-0945")]),

matrix_mc(M(SIC, HAI, "evaluate_outcomes", "difficult", RESP, "cdc ms"),
   "Review the findings on day 3. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Temperature 37.4 C (99.3 F); oriented x3", 0, "The infection and delirium are resolving."),
    ("SpO2 94% on 1 L/min nasal cannula", 0, "Oxygen need is falling."),
    ("Influenza PCR negative", 0, "Droplet precautions can stop."),
    ("Six loose, watery stools since midnight", 1,
     "After antibiotics, this suggests C. difficile: contact precautions, soap-and-water hand hygiene and a stool test."),
    ("Staff using alcohol gel after leaving the room", 1, "Alcohol does not kill C. difficile spores; soap and water is needed."),
    ("Blood cultures: Streptococcus pneumoniae, sensitive to ceftriaxone", 0, "Treatment is matched to the organism.")],
   "Pneumonia is improving; new diarrhea after antibiotics needs C. difficile precautions, including soap-and-water "
   "hand hygiene.",
   "Evaluate outcomes and infection risks during pneumonia treatment.",
   ex=[flow("Day 3", ["0800"], [
       ("Temperature", ["37.4 C (99.3 F)"]),
       ("Mental status", ["Oriented x3"]),
       ("SpO2", ["94% on 1 L/min"]),
       ("Influenza PCR", ["Negative"]),
       ("Stools", ["Six loose, watery stools since midnight"]),
       ("Observation", ["Staff use alcohol gel when leaving the room"]),
       ("Blood cultures", ["S. pneumoniae, sensitive to ceftriaxone"])], id="cap-day3")]),
]

# ================================================================================================
# Case 2: Status asthmaticus in an adult
# ================================================================================================
asth_tabs = [
    ("asth-notes", "Nurses' notes", [note("Emergency department, 2200",
        "A 24-year-old client with asthma has been short of breath for 2 days and used an albuterol inhaler \"every "
        "hour or so\" today. The client was intubated for asthma 2 years ago and takes no controller medicine. The "
        "client sits forward with hands on the knees, speaks 2 or 3 words at a time, and uses the neck muscles to "
        "breathe. Faint wheezes are heard, with poor air movement at both bases. The systolic pressure falls 20 mm Hg "
        "on inspiration. Peak flow is 30% of the client's best.", id="asth-note")]),
    ("asth-abg", "Vital signs and ABGs", [table("Vital signs and arterial blood gases", ["2200", "2245"], [
        ("Respirations", ["32/min", "30/min, shallower"]),
        ("Heart rate", ["128/min", "136/min"]),
        ("SpO2", ["89% on room air", "90% on 6 L/min"]),
        ("pH", ["7.47", "7.33"]),
        ("PaCO2", ["32 mm Hg", "48 mm Hg"]),
        ("PaO2", ["58 mm Hg", "62 mm Hg"]),
        ("HCO3-", ["23 mEq/L", "24 mEq/L"]),
        ("Mental status", ["Anxious, alert", "Tired, closes eyes between questions"])], id="asth-table")]),
]

asth_qs = [
hl_text(M(PA, ASTHMA, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that show a life-threatening asthma attack.",
   ["The client used albuterol ",
    ("every hour or so today", True, "Heavy rescue use shows the attack is not responding."),
    " and ",
    ("was intubated for asthma 2 years ago", True, "Prior intubation is the strongest risk factor for fatal asthma."),
    ". The client ",
    ("speaks 2 or 3 words at a time", True, "Being unable to speak in sentences shows severe airflow limitation."),
    " and ",
    ("uses the neck muscles to breathe", True, "Accessory muscle use shows a severe attack."),
    ". There are ",
    ("faint wheezes with poor air movement", True, "Less wheezing with poor air movement means very little air is moving."),
    ". The client is ",
    ("24 years old", False, "Age is not a severity marker here."),
    "."],
   "Heavy rescue use, prior intubation, words-only speech, accessory muscles and a quiet chest mark a "
   "life-threatening attack.",
   "Recognize cues of severe asthma."),

dtable(M(RRP, LABS, "analyze_cues", "difficult", RESP, "ms lab"),
   "For each finding, choose the best interpretation.",
   ["Finding", "Interpretation"],
   [(["ABG at 2200: pH 7.47, PaCO2 32 mm Hg"], [
       ("Respiratory alkalosis from hyperventilation, expected early in an attack", True,
        "Early in asthma, fast breathing blows off CO2."),
       ("Metabolic alkalosis", False, "HCO3- is normal."),
       ("Respiratory acidosis", False, "The pH is high."),
       ("A normal ABG", False, "The pH and PaCO2 are both abnormal.")]),
    (["ABG at 2245: pH 7.33, PaCO2 48 mm Hg"], [
       ("Respiratory acidosis from tiring respiratory muscles", True,
        "A rising PaCO2 in a severe attack means the client can no longer move enough air."),
       ("Improvement, because the pH is closer to normal", False, "The pH has passed normal into acidosis."),
       ("Metabolic acidosis", False, "HCO3- is normal and PaCO2 is high."),
       ("Compensated respiratory alkalosis", False, "The CO2 is high.")]),
    (["A systolic fall of 20 mm Hg on inspiration"], [
       ("Pulsus paradoxus from severe air trapping", True, "Large pressure swings in the chest with severe obstruction."),
       ("Cardiac tamponade", False, "Possible in other settings; here it reflects air trapping."),
       ("Hypovolemia", False, "Not the cause in this picture."),
       ("A measurement error", False, "It fits the clinical picture.")])],
   "A PaCO2 that rises from low to normal or high during a severe attack means exhaustion, not recovery.",
   "Analyze ABGs in a severe asthma attack."),

cloze(M(PA, ASTHMA, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client is developing {{b1}}, shown by {{b2}}. The nurse should prepare for {{b3}}.",
   [[("hypercapnic respiratory failure", True, "CO2 retention with fatigue means the client cannot keep up."),
     ("anxiety-related hyperventilation", False, "The client is now tiring, not hyperventilating."),
     ("pneumonia", False, "Not supported."),
     ("pulmonary embolism", False, "Not supported.")],
    [("a rising PaCO2 with drowsiness", True, "The key signs of tiring respiratory muscles."),
     ("the heart rate of 128/min", False, "Expected with distress and albuterol."),
     ("the PaO2 of 62 mm Hg", False, "Low, but CO2 retention is the warning sign."),
     ("the client's age", False, "Not relevant.")],
    [("intubation and mechanical ventilation", True, "Exhaustion with rising CO2 needs airway support."),
     ("discharge with an oral steroid", False, "Unsafe."),
     ("a chest CT", False, "Would delay care."),
     ("pulmonary function testing", False, "Not in an emergency.")]],
   "Fatigue with rising CO2 in severe asthma is impending respiratory failure; prepare to intubate.",
   "Prioritize impending respiratory failure in asthma."),

matrix_mc(M(PPT, RESP_MEDS, "generate_solutions", "difficult", RESP, "ph"),
   "For each prescription, indicate whether it is indicated, not indicated, or contraindicated now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Continuous nebulized albuterol", 0, "A short-acting beta2 agonist is the first bronchodilator."),
    ("Ipratropium with the albuterol", 0, "An anticholinergic adds bronchodilation in severe attacks."),
    ("Methylprednisolone IV", 0, "Steroids reduce airway inflammation; they act over hours."),
    ("Magnesium sulfate 2 g IV over 20 minutes", 0, "Relaxes bronchial smooth muscle in severe attacks."),
    ("Lorazepam IV for anxiety", 2, "A sedative can stop a tiring client from breathing."),
    ("Propranolol for the heart rate of 136/min", 2, "A nonselective beta blocker causes bronchospasm."),
    ("Salmeterol inhaler for rescue", 1, "A long-acting beta agonist is too slow and is never used alone for rescue.")],
   "Bronchodilators, steroids and magnesium treat severe asthma; sedatives and nonselective beta blockers are "
   "dangerous; long-acting agonists are not rescue drugs.",
   "Generate solutions for status asthmaticus.",
   ex=[note("Prescriptions, 2250",
       "Albuterol 10 mg/hour continuous nebulizer. Ipratropium 0.5 mg nebulized every 20 minutes x3. "
       "Methylprednisolone 125 mg IV. Magnesium sulfate 2 g IV over 20 minutes. Lorazepam 1 mg IV as needed for "
       "anxiety. Propranolol 10 mg PO. Salmeterol 1 puff as needed.", id="asth-orders")]),

sata(M(PA, ASTHMA, "take_action", "difficult", RESP, "ms"),
   "Review the 2315 findings. Which actions should the nurse take?",
   [("Call the rapid response or airway team now", True, "Worsening CO2 retention and drowsiness need airway experts."),
    ("Stay with the client and keep the head of the bed high", True, "Upright posture helps breathing; the client must not be left."),
    ("Give 100% oxygen by non-rebreather while the team comes", True, "Supports oxygenation before intubation."),
    ("Prepare the intubation drugs and equipment", True, "Readiness speeds a safe intubation."),
    ("Leave to collect supplies from the stockroom", False, "A deteriorating client is never left alone."),
    ("Give the lorazepam now to calm the client", False, "Sedation could stop breathing."),
    ("Switch to a nasal cannula for comfort", False, "Lowers oxygen delivery.")],
   "Impending respiratory arrest: get the airway team, stay, oxygenate, keep upright and prepare to intubate.",
   "Take action for impending respiratory failure.",
   ex=[flow("2315", ["2315"], [
       ("Respirations", ["24/min, shallow"]),
       ("SpO2", ["86% on 6 L/min"]),
       ("PaCO2", ["58 mm Hg"]),
       ("Mental status", ["Drowsy, opens eyes to voice"]),
       ("Wheezes", ["Barely heard"])], id="asth-2315")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", RESP, "ph ms"),
   "The client recovered after 2 days of ventilation. Before discharge, the nurse checks the client's "
   "understanding. For each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll use my steroid inhaler every day, even when I feel fine.\"", 0,
     "An inhaled corticosteroid is a controller taken daily."),
    ("\"I'll rinse my mouth after the steroid inhaler.\"", 0, "Prevents oral thrush."),
    ("\"If I need albuterol more than twice a week, my asthma is under control.\"", 1,
     "Rescue use more than twice a week means poor control; the plan needs review."),
    ("\"I'll stop the prednisone tablets as soon as I can breathe.\"", 1,
     "The course is finished as prescribed; longer courses are tapered."),
    ("\"If my peak flow is under 50% of my best, I'll use albuterol and get emergency care.\"", 0,
     "The red zone of an action plan."),
    ("\"I'll use a spacer with my inhalers.\"", 0, "A spacer improves drug delivery to the lungs.")],
   "Effective teaching covers daily controllers, rescue limits, finishing steroids and the action plan zones.",
   "Evaluate asthma discharge teaching.",
   ex=[note("Discharge plan",
       "Budesonide-formoterol inhaler daily and as needed, prednisone 40 mg daily for 5 days, written asthma action "
       "plan, peak flow meter, follow-up in 1 week.", id="asth-plan")]),
]

# ================================================================================================
# Case 3: COPD with hypercapnia worsened by too much oxygen
# ================================================================================================
copd_tabs = [
    ("copd-notes", "Nurses' notes", [note("Medical unit, 0400",
        "A 68-year-old client with severe COPD, on 2 L/min home oxygen, was admitted yesterday with an exacerbation "
        "and green sputum. At 0200 the client said they were \"short of breath\", and the unlicensed assistive "
        "personnel (UAP) turned the oxygen up to 6 L/min by nasal cannula. At 0400 the client is hard to wake and "
        "mumbles. Skin is warm and flushed. Prescribed oxygen: titrate to SpO2 88% to 92%.", id="copd-note")]),
    ("copd-abg", "ABGs and vital signs", [table("Arterial blood gases and vital signs", ["Admission (2 L/min)", "0400 (6 L/min)"], [
        ("pH", ["7.37", "7.24"]),
        ("PaCO2", ["58 mm Hg", "78 mm Hg"]),
        ("HCO3-", ["33 mEq/L", "33 mEq/L"]),
        ("PaO2", ["58 mm Hg", "92 mm Hg"]),
        ("SpO2", ["90%", "98%"]),
        ("Respirations", ["24/min", "12/min, shallow"])], id="copd-table")]),
]

copd_qs = [
hl_table(M(PA, ASTHMA, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings at 0400 that require immediate action.",
   ["Finding", "0400"],
   [(["Mental status", "Hard to wake, mumbling"], True, "Drowsiness in COPD signals CO2 narcosis."),
    (["PaCO2", "78 mm Hg (58 on admission)"], True, "A sharp rise in CO2 shows acute hypoventilation."),
    (["pH", "7.24"], True, "Acidemia from acute CO2 retention."),
    (["Respirations", "12/min, shallow"], True, "Falling respiratory drive."),
    (["SpO2", "98% on 6 L/min"], True, "Above the 88% to 92% target; excess oxygen is part of the problem."),
    (["HCO3-", "33 mEq/L"], False, "Unchanged; it reflects the chronic compensation, not the new problem.")],
   "Drowsiness, a rising CO2, acidemia, slow breathing and an SpO2 above target point to oxygen-induced "
   "hypercapnia.",
   "Recognize cues of CO2 narcosis in COPD."),

dtable(M(RRP, LABS, "analyze_cues", "difficult", RESP, "lab ms"),
   "For each ABG, choose the interpretation.",
   ["ABG", "Interpretation"],
   [(["Admission: pH 7.37, PaCO2 58, HCO3- 33"], [
       ("Compensated respiratory acidosis, the client's chronic baseline", True,
        "The kidneys have kept bicarbonate high, so the pH is normal despite a high CO2."),
       ("Uncompensated respiratory acidosis", False, "The pH is normal."),
       ("Metabolic alkalosis", False, "The primary change is the CO2."),
       ("Normal ABG", False, "The PaCO2 and HCO3- are both abnormal.")]),
    (["0400: pH 7.24, PaCO2 78, HCO3- 33"], [
       ("Acute respiratory acidosis on top of chronic", True,
        "A new rise in CO2 outpaces the existing compensation, so the pH falls."),
       ("Metabolic acidosis", False, "HCO3- is high."),
       ("Fully compensated respiratory acidosis", False, "The pH is low."),
       ("Respiratory alkalosis", False, "The CO2 is high.")])],
   "Chronic CO2 retention is compensated; a further acute rise causes acidemia and narcosis.",
   "Analyze ABGs in COPD."),

cloze(M(PA, ASTHMA, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's change is most likely caused by {{b1}}. The greatest immediate risk is {{b2}}.",
   [[("worsened hypoventilation and ventilation-perfusion mismatch from excess oxygen", True,
      "High oxygen in some COPD clients reduces ventilation drive and worsens V/Q mismatch, so CO2 rises."),
     ("a stroke", False, "No focal signs; the ABG explains the change."),
     ("hypoglycemia", False, "Not supported."),
     ("an opioid overdose", False, "No opioid was given.")],
    [("respiratory arrest", True, "Rising CO2 with falling respiratory rate can progress to apnea."),
     ("hypertension", False, "Not the threat."),
     ("pulmonary embolism", False, "Not supported."),
     ("urinary retention", False, "Not relevant.")]],
   "Excess oxygen in a CO2 retainer can cause narcosis and respiratory arrest.",
   "Prioritize the cause and risk in CO2 narcosis."),

matrix_mc(M(PA, ASTHMA, "generate_solutions", "difficult", RESP, "ms ph"),
   "For each intervention, indicate whether it is indicated or contraindicated now.",
   ["Indicated", "Contraindicated"],
   [("Bilevel positive airway pressure (BiPAP)", 0, "Noninvasive ventilation lowers CO2 and often avoids intubation."),
    ("Titrate oxygen down to an SpO2 of 88% to 92%", 0, "The prescribed target for this client."),
    ("Stop all oxygen until the client wakes", 1, "Removing oxygen causes dangerous hypoxemia."),
    ("Nebulized albuterol and ipratropium", 0, "Bronchodilators reduce air trapping."),
    ("Prednisone 40 mg daily for 5 days", 0, "Short steroid courses shorten COPD exacerbations."),
    ("Morphine IV for dyspnea", 1, "Opioids would further depress breathing in narcosis.")],
   "Ventilate with BiPAP, bring oxygen back to target and treat the exacerbation; do not stop oxygen or sedate.",
   "Generate solutions for hypercapnic respiratory failure in COPD.",
   ex=[note("Provider orders, 0415",
       "BiPAP IPAP 14, EPAP 5 cm H2O. Oxygen through BiPAP to SpO2 88% to 92%. Albuterol-ipratropium nebulizer every "
       "4 hours. Prednisone 40 mg daily. Doxycycline 100 mg twice daily. Repeat ABG in 1 hour.", id="copd-orders")]),

sata(M(MOC, DELEG, "take_action", "difficult", RESP, "mgmt"),
   "BiPAP is started. Later, the nurse speaks with the UAP who turned up the oxygen. Which actions are appropriate?",
   [("Explain that changing oxygen flow is not within the UAP's role", True, "Oxygen is a prescribed therapy."),
    ("Ask the UAP to report breathlessness to the nurse instead of acting on it", True,
     "Reporting lets the RN assess and act within the prescription."),
    ("Discuss the event privately, not in front of the client", True, "Feedback is given respectfully and privately."),
    ("Complete an incident report according to policy", True, "Supports a review of the system, not blame."),
    ("Document in the client's record that the UAP made an error", False,
     "The record documents the client's care and findings; incident reports are not referenced in it."),
    ("Stop delegating vital signs to this UAP", False, "The issue was acting outside the role, not measuring.")],
   "Correct the role boundary privately, set clear reporting expectations and report the event through the "
   "incident system.",
   "Take action after a delegation error.",
   ex=[flow("0530, on BiPAP", ["0530"], [
       ("Mental status", ["Awake, answers questions"]),
       ("SpO2", ["91%"]),
       ("Event", ["The UAP says, \"I just wanted to help them breathe.\""])], id="copd-0530")]),

matrix_mc(M(RRP, OXY, "evaluate_outcomes", "difficult", RESP, "ms lab"),
   "Review the findings after 2 hours of BiPAP. For each finding, indicate whether it shows improvement or needs "
   "follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("pH 7.33, PaCO2 64 mm Hg", 0, "CO2 is falling and the pH is rising."),
    ("Alert and oriented", 0, "Narcosis is resolving."),
    ("Respirations 20/min, deeper", 0, "Ventilation is improving."),
    ("SpO2 97%", 1, "Above the 88% to 92% target; oxygen should be turned down."),
    ("Red skin on the bridge of the nose under the mask", 1, "Early pressure injury; pad the area and refit the mask."),
    ("The client asks for a drink of water", 1,
     "Needs a swallow and aspiration check; drinking on BiPAP is planned during short breaks off the mask.")],
   "Falling CO2 and better alertness show BiPAP is working; oxygen above target, mask pressure and safe drinking "
   "need attention.",
   "Evaluate the response to BiPAP in COPD.",
   ex=[flow("0615, after 2 hours of BiPAP", ["0615"], [
       ("ABG", ["pH 7.33, PaCO2 64 mm Hg"]),
       ("Mental status", ["Alert and oriented"]),
       ("Respirations", ["20/min, deeper"]),
       ("SpO2", ["97%"]),
       ("Skin", ["Red area on the bridge of the nose"]),
       ("Request", ["Asks for a drink of water"])], id="copd-0615")]),
]

# ================================================================================================
# Case 4: Active pulmonary tuberculosis
# ================================================================================================
tb_tabs = [
    ("tb-notes", "Nurses' notes", [note("Clinic, then admission",
        "A 45-year-old client who works at a homeless shelter has had a cough for 5 weeks, with blood-streaked sputum, "
        "night sweats and a 6 kg weight loss. The client lives with a partner and two school-age children. The "
        "client uses oral contraceptives and wears soft contact lenses. Chest x-ray: a cavity in the right upper "
        "lobe.", id="tb-note")]),
    ("tb-tests", "Test results", [table("Test results", ["Result"], [
        ("Tuberculin skin test", ["14 mm induration at 48 hours"]),
        ("Interferon-gamma release assay", ["Positive"]),
        ("Sputum AFB smear", ["3+ (many acid-fast bacilli)"]),
        ("Sputum culture", ["Pending"]),
        ("HIV test", ["Negative"]),
        ("ALT", ["28 units/L (reference 7 to 56)"])], id="tb-table")]),
]

tb_qs = [
hl_text(M(PA, PNEU, "recognize_cues", "moderate", RESP, "cdc ms"),
   "Highlight the findings that suggest active, infectious tuberculosis.",
   ["The client has had ",
    ("a cough for 5 weeks", True, "A cough lasting 3 weeks or more is a TB symptom."),
    " with ",
    ("blood-streaked sputum", True, "Hemoptysis fits cavitary disease."),
    ", ",
    ("night sweats", True, "A classic systemic symptom."),
    " and ",
    ("a 6 kg weight loss", True, "Weight loss is common in active TB."),
    ". The client ",
    ("wears soft contact lenses", False, "Not a sign of TB, though it matters for teaching about rifampin."),
    ". The x-ray shows ",
    ("a cavity in the right upper lobe", True, "Upper-lobe cavitation is typical of reactivation TB and very infectious."),
    "."],
   "Prolonged cough, hemoptysis, night sweats, weight loss and an upper-lobe cavity point to active pulmonary TB.",
   "Recognize cues of active tuberculosis."),

matrix_mc(M(RRP, RESP_DX, "analyze_cues", "difficult", RESP, "cdc lab"),
   "For each test result, indicate what it shows.",
   ["Shows TB infection, not whether it is active", "Shows active, infectious disease", "Confirms the diagnosis and drug sensitivity"],
   [("Tuberculin skin test 14 mm", 0, "A positive skin test shows infection at some time; it cannot tell latent from active."),
    ("Positive interferon-gamma release assay", 0, "Also shows infection, not activity."),
    ("Sputum AFB smear 3+", 1, "Many bacilli in sputum means the client is highly infectious."),
    ("Cavity on chest x-ray", 1, "Cavitary disease is active and infectious."),
    ("Sputum culture", 2, "Culture confirms M. tuberculosis and tests drug sensitivity; it takes weeks.")],
   "Skin tests and IGRAs show infection; smears and cavities show active infectious disease; culture confirms and "
   "guides drugs.",
   "Analyze TB test results."),

cloze(M(SIC, TBP, "prioritize_hypotheses", "difficult", RESP, "cdc"),
   "Complete the sentences by choosing from the lists of options.",
   "The nurse's first priority is to {{b1}}. Staff entering the room must wear {{b2}}, and when the client leaves "
   "the room for tests the client should wear {{b3}}.",
   [[("place the client in an airborne infection isolation room", True,
      "A negative-pressure room stops droplet nuclei spreading."),
     ("start the four-drug regimen before anything else", False, "Treatment starts promptly, but isolation comes first to protect others."),
     ("test the client's household contacts", False, "Needed soon, but after isolating the client."),
     ("arrange a bronchoscopy", False, "Smears are already positive.")],
    [("a fit-tested N95 respirator", True, "Filters airborne particles."),
     ("a surgical mask", False, "Does not protect against airborne particles."),
     ("gloves and a gown only", False, "TB is airborne."),
     ("no protection, if they had a BCG vaccine", False, "BCG does not protect staff reliably.")],
    [("a surgical mask", True, "A mask on the client traps droplets at the source."),
     ("an N95 respirator", False, "Clients wear surgical masks; respirators protect the wearer."),
     ("nothing, if the trip is short", False, "Any time out of the room needs a mask."),
     ("a face shield", False, "Not adequate.")]]
   ,
   "Isolate first in a negative-pressure room; staff wear N95 respirators; the client wears a surgical mask outside "
   "the room.",
   "Prioritize airborne precautions for active TB."),

matrix_mr(M(PPT, RESP_MEDS, "generate_solutions", "difficult", RESP, "ph"),
   "The four-drug regimen is prescribed. For each drug, indicate the monitoring it needs. Each row may have more "
   "than one answer.",
   ["Liver function tests", "Vision (acuity and red-green color)", "Uric acid or joint pain", "Numbness or tingling"],
   [("Isoniazid", {0, 3}, "Hepatotoxic, and causes peripheral neuropathy that pyridoxine (vitamin B6) prevents."),
    ("Rifampin", {0}, "Hepatotoxic; it also turns body fluids orange and lowers the effect of oral contraceptives."),
    ("Pyrazinamide", {0, 2}, "Hepatotoxic, and raises uric acid, which can cause gout."),
    ("Ethambutol", {1}, "Can cause optic neuritis, first seen as loss of red-green color vision.")],
   "Three of the four first-line drugs are hepatotoxic; isoniazid also needs neuropathy checks, pyrazinamide uric "
   "acid and ethambutol vision checks.",
   "Generate the monitoring plan for TB drugs.",
   ex=[note("Prescriptions",
       "Isoniazid 300 mg, rifampin 600 mg, pyrazinamide 1,500 mg and ethambutol 1,200 mg daily, with pyridoxine 25 mg "
       "daily. Directly observed therapy.", id="tb-orders")]),

sata(M(HPM, TEACH, "take_action", "difficult", RESP, "ph cdc"),
   "Which instructions should the nurse include in the client's teaching?",
   [("Take the medicines every day for at least 6 months, even after feeling better", True,
     "Stopping early leads to relapse and drug resistance."),
    ("Use a barrier or other nonhormonal method of contraception", True, "Rifampin makes oral contraceptives unreliable."),
    ("Expect orange urine, sweat and tears", True, "A harmless rifampin effect."),
    ("Wear glasses rather than soft contact lenses", True, "Rifampin can permanently stain soft lenses."),
    ("Avoid alcohol", True, "Alcohol adds to liver injury."),
    ("Stop the medicines if the urine turns orange", False, "This is expected."),
    ("Return to work as soon as the cough settles", False,
     "The client returns to work only when no longer infectious, usually after 3 negative smears.")],
   "Teaching covers finishing therapy, rifampin's effects on fluids, lenses and contraception, avoiding alcohol and "
   "when it is safe to return to work.",
   "Teach a client starting TB treatment.",
   ex=[note("Before discharge home on day 5",
       "The client will continue isolation at home with directly observed therapy by the health department. The "
       "client asks, \"When can I go back to the shelter?\"", id="tb-day5")]),

matrix_mc(M(MOC, "collaboration-and-referrals", "evaluate_outcomes", "difficult", RESP, "cdc ph"),
   "Review the findings at the 3-week visit. For each finding, indicate whether it is expected or needs follow-up.",
   ["Expected or improving", "Needs follow-up"],
   [("Weight up 1.5 kg; night sweats gone", 0, "Treatment is working."),
    ("Orange urine", 0, "Expected with rifampin."),
    ("ALT 240 units/L, nausea and yellow sclerae", 1, "Drug-induced hepatitis; the drugs are held and the provider told now."),
    ("The partner and children have not yet been screened", 1,
     "Close contacts need testing; the health department leads contact investigation."),
    ("Normal red-green color vision", 0, "No ethambutol eye toxicity."),
    ("Sputum smears still positive", 1, "The client stays in isolation; the team checks adherence and drug sensitivity.")],
   "Symptoms are improving, but liver injury, unscreened contacts and positive smears each need action and "
   "collaboration with the health department.",
   "Evaluate outcomes of TB treatment.",
   ex=[flow("3-week visit", ["Week 3"], [
       ("Weight", ["Up 1.5 kg"]),
       ("Symptoms", ["No night sweats; nausea for 4 days"]),
       ("Eyes", ["Yellow sclerae; red-green color vision normal"]),
       ("ALT", ["240 units/L"]),
       ("Sputum smears", ["Positive"]),
       ("Contacts", ["Partner and children not yet screened"])], id="tb-week3")]),
]

cases = [
    case("Severe community-acquired pneumonia in an older adult", cap_tabs, cap_qs),
    case("Status asthmaticus in an adult", asth_tabs, asth_qs),
    case("COPD with CO2 narcosis from too much oxygen", copd_tabs, copd_qs),
    case("Active pulmonary tuberculosis", tb_tabs, tb_qs),
]

BUILT = build(104, "pulmonary-cases-pneumonia-asthma-copd-tb",
              "Pulmonary case studies: severe pneumonia, status asthmaticus, COPD with CO2 narcosis and active "
              "tuberculosis",
              items, cases, case_conversion=True)
