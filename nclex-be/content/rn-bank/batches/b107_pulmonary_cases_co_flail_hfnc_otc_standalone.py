"""Batch 107 (case conversion, chunk 2: pulmonary): four full NGN case studies and four standalone items
(CASE-MAP.md: standalone items are bowtie, trend, multi-client priority and delegation).

Cases: carbon monoxide poisoning with smoke inhalation; flail chest with pulmonary contusion; viral pneumonia on
high-flow nasal oxygen; an older adult harmed by over-the-counter cold medicines (antihistamines, decongestants,
antitussives). Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a
management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
RESP = "respiratory"
POISON = "poisoning-and-ingestions"
BURN = "burn-care"
CHEST_TRAUMA = "chest-trauma-and-pneumothorax"
PNEU = "pneumonia-care"
OXY = "oxygen-therapy-and-devices"
TBP = "transmission-based-precautions"
RESP_MEDS = "respiratory-medications"
ADVERSE = "adverse-effects-of-medications"
HOME = "equipment-and-home-safety"
TEACH = "health-teaching-and-behavior-change"
HANDOFF = "handoff-and-sbar-communication"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
AGING = "aging-and-older-adult-health"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PPT, RESP_MEDS, "prioritize_hypotheses", "difficult", RESP, "ph"),
   "Use the clinic note to fill in the diagram: the problem this client with COPD most likely has, 2 actions for "
   "the nurse, and 2 parameters to track.",
   [("Theophylline toxicity", True, "Ciprofloxacin slows theophylline clearance; nausea, tachycardia, tremor and a level of 28 fit toxicity."),
    ("Asthma exacerbation", False, "Breathing is unchanged."),
    ("Hypoglycemia", False, "Glucose is normal."),
    ("Thyrotoxicosis", False, "Not supported by the history.")],
   [("Hold theophylline and notify the provider", True, "Stops further accumulation."),
    ("Place the client on a cardiac monitor", True, "Toxicity causes dysrhythmias."),
    ("Give the next theophylline dose with food", False, "Food does not reduce toxicity."),
    ("Encourage strong coffee to relieve nausea", False, "Caffeine adds to theophylline's effects."),
    ("Increase the ciprofloxacin to clear the infection faster", False, "Worsens the interaction.")],
   [("Heart rhythm", True, "Tachydysrhythmias are a key toxic effect."),
    ("Theophylline level", True, "Guides when to restart and at what dose."),
    ("Daily body weight on one scale", False, "Not the main guide."),
    ("Bowel sounds in all quadrants", False, "Not the main guide."),
    ("Urine specific gravity each shift", False, "Not the main guide.")],
   "Theophylline has a narrow range (about 10 to 20 mcg/mL); interacting drugs such as ciprofloxacin raise it. Hold "
   "it, monitor the heart and recheck the level.",
   "Recognize and manage theophylline toxicity.",
   ex=[note("Clinic note",
       "A 70-year-old client with COPD takes theophylline ER 300 mg twice daily. Ciprofloxacin was started 4 days "
       "ago for a urinary infection. Today: nausea, vomiting, tremor, HR 128/min, irregular. Theophylline level 28 "
       "mcg/mL (therapeutic 10 to 20). Glucose 96 mg/dL.", id="theo-note")]),

dtable(M(PA, PNEU, "evaluate_outcomes", "difficult", RESP, "ms"),
   "A client with pneumonia has been treated for 3 days. For each trend, choose how the nurse should interpret it.",
   ["Measure", "Day 1 to day 3", "Interpretation"],
   [(["Temperature", "39.1 to 37.3 C (102.4 to 99.1 F)"], [
       ("Improving", True, "Fever is resolving with treatment."),
       ("Worsening", False, "It is falling."),
       ("Needs a change of antibiotic", False, "The response is good.")]),
    (["Oxygen need", "4 L/min to 4 L/min, SpO2 90% to 89%"], [
       ("Not improving; needs reassessment", True, "Oxygenation should improve by day 3; check for effusion, empyema or another cause."),
       ("Improving", False, "The need has not fallen."),
       ("Expected and needs no action", False, "A plateau with a falling SpO2 needs review.")]),
    (["Breath sounds", "Crackles right base to dullness and absent sounds right base"], [
       ("New finding suggesting a pleural effusion", True, "Dullness with absent breath sounds suggests fluid."),
       ("Expected resolution", False, "Resolution brings clearing crackles, not dullness."),
       ("Unrelated", False, "It explains the oxygen plateau.")]),
    (["WBC", "18,000 to 12,500/mm3"], [
       ("Improving", True, "Falling toward normal."),
       ("Worsening", False, "It is falling."),
       ("Unreliable", False, "It is a valid trend.")])],
   "Fever and white count are improving, but a static oxygen need with new dullness suggests a parapneumonic "
   "effusion that needs imaging.",
   "Evaluate a pneumonia treatment trend.",
   ex=[flow("Pneumonia trend", ["Day 1", "Day 2", "Day 3"], [
       ("Temperature", ["39.1 C", "38.2 C", "37.3 C"]),
       ("Oxygen", ["4 L/min, SpO2 90%", "4 L/min, SpO2 90%", "4 L/min, SpO2 89%"]),
       ("Right base", ["Crackles", "Crackles", "Dull, absent breath sounds"]),
       ("WBC", ["18,000", "15,200", "12,500"])], id="pneu-trend")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", RESP, "mgmt ms"),
   "At the start of the day shift on a pulmonary unit, four clients are assigned to the nurse. Whose bedside "
   "should the nurse go to first?",
   [("A client with COPD on 2 L/min whose SpO2 is 89% and who is eating breakfast", False,
     "Within the 88% to 92% target for chronic CO2 retention."),
    ("A client on day 2 of TB treatment in airborne isolation who is asking for a newspaper", False,
     "A stable request that can wait."),
    ("A client with a chest tube that stopped tidaling an hour ago who is now short of breath", True,
     "Loss of tidaling with new dyspnea suggests a blocked tube or a re-expanding problem; it needs assessment now."),
    ("A client with asthma whose peak flow is 75% of personal best before the morning inhaler", False,
     "In the yellow zone, needing the scheduled treatment, but stable.")],
   "New dyspnea with a change in chest tube function is the most unstable situation.",
   "Prioritize clients on a pulmonary unit."),

sata(M(MOC, DELEG, "take_action", "difficult", RESP, "mgmt"),
   "The RN is working with an LPN on a pulmonary unit. Which tasks can the RN assign to the LPN?",
   [("Give scheduled inhalers to a stable client with COPD", True, "Routine medication for a stable client."),
    ("Reinforce the RN's incentive spirometer teaching", True, "LPNs reinforce teaching the RN has given."),
    ("Suction a stable client's 3-month-old tracheostomy", True, "A routine task for a stable client."),
    ("Do the admission assessment of a client who has new hemoptysis", False, "Initial assessment of a new problem stays with the RN."),
    ("Titrate the oxygen for a client on BiPAP whose CO2 is rising", False, "An unstable client needs the RN."),
    ("Create the discharge teaching plan for home oxygen", False, "Planning teaching is the RN's role.")],
   "LPNs care for stable clients with predictable needs and reinforce teaching; assessment, planning and unstable "
   "clients stay with the RN.",
   "Assign tasks on a pulmonary unit."),
]

# ================================================================================================
# Case 1: Carbon monoxide poisoning with smoke inhalation
# ================================================================================================
co_tabs = [
    ("co-notes", "Nurses' notes", [note("Emergency department, 0230",
        "A 52-year-old client was pulled from a house fire after about 15 minutes in a smoke-filled bedroom. The "
        "client has a severe headache, is confused, and vomited once. Soot is in the nose and mouth, nasal hairs are "
        "singed, and the voice is hoarse. No burns to the skin. Oxygen 2 L/min by nasal cannula was started by "
        "paramedics.", id="co-note")]),
    ("co-data", "Monitoring and labs", [table("Monitoring and labs", ["0230", "Reference"], [
        ("SpO2 (pulse oximeter)", ["99%", "-"]),
        ("Carboxyhemoglobin", ["28%", "below 3% (below 10% in smokers)"]),
        ("PaO2", ["96 mm Hg", "80 to 100 mm Hg"]),
        ("Lactate", ["4.1 mmol/L", "0.5 to 2.0 mmol/L"]),
        ("Heart rate", ["118/min", "-"]),
        ("ECG", ["Sinus tachycardia, ST depression in V4 to V6", "-"])], id="co-table")]),
]

co_qs = [
hl_table(M(PA, POISON, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Carboxyhemoglobin", "28%"], True, "Significant CO poisoning."),
    (["Mental status", "Confused, severe headache"], True, "Neurologic signs of CO toxicity."),
    (["Airway", "Singed nasal hairs, soot, hoarse voice"], True, "Signs of inhalation injury; the airway may swell."),
    (["Lactate", "4.1 mmol/L"], True, "Tissue hypoxia."),
    (["ECG", "ST depression in V4 to V6"], True, "Myocardial ischemia from CO."),
    (["SpO2", "99%"], False, "Falsely normal: pulse oximeters read carboxyhemoglobin as oxygenated hemoglobin."),
    (["PaO2", "96 mm Hg"], False, "Normal: dissolved oxygen is normal even when hemoglobin is blocked by CO.")],
   "In CO poisoning, the carboxyhemoglobin level and the clinical picture matter; SpO2 and PaO2 can look normal.",
   "Recognize cues of carbon monoxide poisoning and inhalation injury."),

matrix_mc(M(PA, POISON, "analyze_cues", "difficult", RESP, "ms"),
   "For each statement about the client, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("The SpO2 of 99% shows the tissues are well oxygenated.", 1,
     "Standard pulse oximetry cannot tell carboxyhemoglobin from oxyhemoglobin."),
    ("CO binds hemoglobin about 200 times more strongly than oxygen.", 0, "So little CO blocks much oxygen transport."),
    ("High-flow oxygen shortens the half-life of carboxyhemoglobin.", 0,
     "From about 4 to 5 hours on room air to about 1 hour on 100% oxygen."),
    ("Hoarseness after smoke exposure can progress to airway obstruction within hours.", 0, "Airway edema peaks over 24 hours."),
    ("Without skin burns, inhalation injury is unlikely.", 1, "Inhalation injury occurs without skin burns.")],
   "CO blocks oxygen carriage while SpO2 and PaO2 stay normal; inhalation injury can swell the airway without skin "
   "burns.",
   "Analyze the pathophysiology of CO poisoning."),

cloze(M(PA, BURN, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The two most immediate threats are {{b1}} and {{b2}}.",
   [[("airway obstruction from inhalation injury", True, "Hoarseness and singed hairs signal an airway at risk."),
     ("skin infection", False, "No burns."),
     ("hypovolemic shock", False, "No major fluid loss."),
     ("hypoglycemia", False, "Not supported.")],
    [("tissue hypoxia from carbon monoxide", True, "Confusion, lactate and ECG changes show oxygen starvation."),
     ("pneumonia", False, "A later risk."),
     ("kidney failure", False, "Not immediate."),
     ("anxiety", False, "Not a threat in itself.")]],
   "Protect the airway and reverse CO hypoxia.",
   "Prioritize threats after smoke inhalation."),

matrix_mc(M(PA, POISON, "generate_solutions", "difficult", RESP, "ms"),
   "For each intervention, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("100% oxygen by non-rebreather mask", 0, "Displaces CO from hemoglobin faster."),
    ("Titrate oxygen to keep SpO2 at 94% to 98%", 1, "SpO2 is falsely high; titrating to it would undertreat."),
    ("Consult about hyperbaric oxygen", 0, "Carboxyhemoglobin above 25% with neurologic signs or ischemia is an indication."),
    ("Prepare for early intubation if hoarseness worsens", 0, "Intubating before the airway swells is safer."),
    ("Continue 2 L/min by nasal cannula while waiting for results", 1, "Far too little oxygen.")],
   "Give 100% oxygen regardless of SpO2, consider hyperbaric therapy and protect the airway early.",
   "Generate solutions for CO poisoning.",
   ex=[note("Provider orders, 0240", "100% oxygen by non-rebreather. Hyperbaric medicine consult. Airway "
       "assessment by anesthesia.", id="co-orders")]),

sata(M(PA, BURN, "take_action", "difficult", RESP, "ms"),
   "Review the 0315 findings. Which actions should the nurse take?",
   [("Call the airway team for intubation now", True, "Stridor shows the airway is closing."),
    ("Keep the head of the bed up", True, "Reduces airway swelling."),
    ("Prepare intubation drugs and a smaller tube size", True, "A swollen airway may need a smaller tube."),
    ("Continue 100% oxygen", True, "Treats the CO poisoning."),
    ("Give the client sips of water for the sore throat", False, "Aspiration risk before likely intubation."),
    ("Wait for the hyperbaric team before acting on the airway", False, "The airway comes first.")],
   "Stridor after smoke inhalation needs urgent intubation while oxygen continues.",
   "Take action for an airway threatened by inhalation injury.",
   ex=[flow("0315", ["0315"], [
       ("Voice", ["More hoarse"]),
       ("Breathing", ["Inspiratory stridor"]),
       ("Carboxyhemoglobin", ["16%"]),
       ("Mental status", ["Less confused"])], id="co-0315")]),

matrix_mc(M(SIC, HOME, "evaluate_outcomes", "difficult", RESP, "fund"),
   "Before discharge, the client describes home safety plans. For each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll put a CO detector near the bedrooms.\"", 0, "CO detectors near sleeping areas give early warning."),
    ("\"The smoke alarm will also warn me about carbon monoxide.\"", 1, "Most smoke alarms do not detect CO unless they are combination units."),
    ("\"I'll test the alarms every month.\"", 0, "Monthly testing is recommended."),
    ("\"It's fine to run the generator in the garage with the door open.\"", 1, "Generators must run outside, away from windows and doors."),
    ("\"If I get headaches that improve when I leave the house, I'll get it checked.\"", 0, "A classic sign of CO exposure."),
    ("\"I'll plan two ways out of each room.\"", 0, "Part of a fire escape plan.")],
   "Teaching covers CO detectors, generator safety, alarm testing and an escape plan.",
   "Evaluate home safety teaching after CO poisoning.",
   ex=[note("Day 3", "Carboxyhemoglobin 1%; extubated; memory and concentration normal.", id="co-day3")]),
]

# ================================================================================================
# Case 2: Flail chest with pulmonary contusion
# ================================================================================================
flail_tabs = [
    ("flail-notes", "Nurses' notes", [note("Trauma unit, admission",
        "A 45-year-old client was the driver in a car crash. Left ribs 4 to 7 are each broken in two places. During "
        "inspiration, a section of the left chest wall moves inward while the rest of the chest expands. CT shows a "
        "left lung contusion. The client rates pain 9 of 10, breathes shallowly and will not cough.", id="flail-note")]),
    ("flail-vitals", "Vital signs", [flow("Vital signs", ["Admission"], [
        ("Respirations", ["30/min, shallow"]),
        ("SpO2", ["90% on 4 L/min"]),
        ("Heart rate", ["112/min"]),
        ("Blood pressure", ["138/84 mm Hg"])], id="flail-flow")]),
]

flail_qs = [
hl_text(M(PA, CHEST_TRAUMA, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that require follow-up.",
   ["Ribs 4 to 7 are ",
    ("each broken in two places", True, "Two breaks in three or more adjacent ribs create a flail segment."),
    ". A section of chest wall ",
    ("moves inward on inspiration", True, "Paradoxical movement reduces ventilation."),
    ". CT shows ",
    ("a lung contusion", True, "Bruised lung worsens over 24 to 72 hours."),
    ". The client ",
    ("breathes shallowly and will not cough", True, "Pain splinting leads to atelectasis and pneumonia."),
    ". Blood pressure is ",
    ("138/84 mm Hg", False, "Stable."),
    "."],
   "A flail segment with contusion and pain-limited breathing threatens ventilation.",
   "Recognize cues of flail chest."),

cloze(M(PA, CHEST_TRAUMA, "analyze_cues", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The flail segment moves inward on inspiration because {{b1}}. The client's hypoxemia is likely to {{b2}} over "
   "the next 2 days because {{b3}}.",
   [[("negative pressure in the chest pulls the unsupported segment in", True, "The segment is detached from the rib cage."),
     ("the lung underneath is collapsed", False, "The movement is from the chest wall."),
     ("the diaphragm is paralyzed", False, "Not the mechanism."),
     ("the client is breathing too deeply", False, "Not the mechanism.")],
    [("worsen", True, "Contusions evolve with swelling and bleeding into the lung."),
     ("resolve on its own", False, "Contusions usually worsen first."),
     ("stay the same", False, "They typically progress."),
     ("depend only on pain", False, "Pain contributes, but the contusion progresses.")],
    [("the contused lung fills with blood and edema", True, "Gas exchange falls as alveoli fill."),
     ("the ribs will heal", False, "Takes weeks."),
     ("the client will be more active", False, "Not the reason."),
     ("oxygen will be stopped", False, "Not the reason.")]],
   "A flail segment moves paradoxically; contusion hypoxemia worsens over 24 to 72 hours.",
   "Analyze the course of flail chest and contusion."),

matrix_mc(M(PA, CHEST_TRAUMA, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "For each complication, indicate how likely it is in the first 72 hours.",
   ["Likely", "Unlikely"],
   [("Atelectasis and pneumonia from shallow breathing", 0, "Pain prevents deep breaths and coughing."),
    ("Respiratory failure as the contusion evolves", 0, "Hypoxemia often worsens."),
    ("Pulmonary edema made worse by large IV fluid volumes", 0, "Contused lung leaks fluid easily."),
    ("Hemothorax or pneumothorax from rib ends", 0, "Broken ribs can tear the pleura or vessels."),
    ("Diabetic ketoacidosis", 1, "No relation.")],
   "Pain-limited breathing, evolving contusion, fluid overload and pleural injury are the main risks.",
   "Prioritize complications of chest trauma."),

matrix_mc(M(PA, CHEST_TRAUMA, "generate_solutions", "difficult", RESP, "ms ph"),
   "For each intervention, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Regional analgesia, such as an epidural or nerve block", 0, "Allows deep breathing and coughing."),
    ("Incentive spirometry every hour while awake", 0, "Prevents atelectasis."),
    ("Splinting the chest with a pillow to cough", 0, "Reduces pain during coughing."),
    ("Binding the chest tightly to stop the segment moving", 1, "Restricts expansion and worsens atelectasis."),
    ("Large maintenance IV fluid volumes to keep the urine output high", 1, "Contused lungs are prone to edema; fluids are given carefully."),
    ("Noninvasive ventilation or intubation if respiratory failure develops", 0, "Positive pressure splints the chest internally.")],
   "Good pain control and lung expansion, careful fluids, and ventilatory support if needed.",
   "Generate solutions for flail chest.",
   ex=[note("Orders", "Thoracic epidural. Incentive spirometry. IV fluids 150 mL/hour. Chest x-ray daily.", id="flail-orders")]),

sata(M(PA, CHEST_TRAUMA, "take_action", "difficult", RESP, "ms"),
   "Review the day 2 findings. Which actions should the nurse take?",
   [("Notify the provider of the worsening oxygenation", True, "Evolving contusion and fluid overload need review."),
    ("Raise the head of the bed", True, "Improves ventilation."),
    ("Ask for the IV fluid rate to be reviewed", True, "The positive fluid balance worsens contused lung."),
    ("Prepare for noninvasive ventilation", True, "May prevent intubation."),
    ("Increase the IV fluids to support blood pressure", False, "Pressure is normal; more fluid worsens edema."),
    ("Stop incentive spirometry to let the client rest", False, "Lung expansion is still needed.")],
   "Worsening hypoxemia with crackles and positive fluid balance needs escalation, fluid review and ventilatory "
   "support.",
   "Take action as a lung contusion evolves.",
   ex=[flow("Day 2", ["Day 1", "Day 2"], [
       ("SpO2", ["93% on 4 L/min", "85% on 6 L/min"]),
       ("Respirations", ["24/min", "34/min"]),
       ("Lungs", ["Diminished left base", "Crackles bilaterally"]),
       ("Fluid balance", ["+1.4 L", "+3.2 L"]),
       ("Blood pressure", ["132/80 mm Hg", "134/82 mm Hg"])], id="flail-day2")]),

matrix_mc(M(MOC, HANDOFF, "evaluate_outcomes", "difficult", RESP, "mgmt"),
   "The client is now on noninvasive ventilation. The night nurse gives a handoff report. For each part of the "
   "report, indicate whether it is adequate or needs correction.",
   ["Adequate", "Needs correction"],
   [("\"SpO2 93% on BiPAP 12/5 with 50% oxygen, up from 85% on 6 L.\"", 0, "Specific values and trend."),
    ("\"The client's fine.\"", 1, "Vague; give findings, not a judgment."),
    ("\"Epidural infusing; pain 3 of 10; spirometer 750 mL, up from 400 mL.\"", 0, "Measurable and trended."),
    ("\"Fluid balance is positive, I think.\"", 1, "Give the exact balance and the new fluid orders."),
    ("\"If SpO2 drops below 90% or the client tires, call the team for intubation.\"", 0, "Clear escalation plan."),
    ("The report is given at the nurses' station without checking the client together", 1,
     "Bedside handoff lets both nurses check the client and the equipment.")],
   "A safe handoff gives specific trended data, a clear escalation plan and is done at the bedside.",
   "Evaluate a handoff report.",
   ex=[note("Day 2, 1900 handoff", "Night to day shift, at the nurses' station.", id="flail-handoff")]),
]

# ================================================================================================
# Case 3: Viral pneumonia on high-flow nasal oxygen
# ================================================================================================
hf_tabs = [
    ("hf-notes", "Nurses' notes", [note("Step-down unit, 1000",
        "A 59-year-old client with viral pneumonia from COVID-19 (day 8 of illness) has bilateral infiltrates. "
        "History: obesity, type 2 diabetes. The client is on high-flow nasal cannula at 50 L/min and FiO2 0.60. "
        "The client is anxious and talks in short sentences.", id="hf-note")]),
    ("hf-data", "Monitoring", [flow("Monitoring", ["1000"], [
        ("SpO2", ["92%"]),
        ("Respirations", ["30/min"]),
        ("Heart rate", ["104/min"]),
        ("Glucose", ["248 mg/dL"]),
        ("D-dimer", ["1.8 mcg/mL (below 0.5)"])], id="hf-flow")]),
]

hf_qs = [
hl_table(M(PA, PNEU, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Oxygen need", "50 L/min, FiO2 0.60"], True, "A high oxygen requirement."),
    (["Respirations", "30/min"], True, "Tachypnea shows high work of breathing."),
    (["Speech", "Short sentences"], True, "Breathlessness limits speech."),
    (["Glucose", "248 mg/dL"], True, "Hyperglycemia, likely worse with steroids."),
    (["D-dimer", "1.8 mcg/mL"], True, "Raised clotting activity; COVID-19 raises the risk of clots."),
    (["Day of illness", "Day 8"], False, "Context: the inflammatory phase often peaks now.")],
   "High oxygen need with tachypnea, hyperglycemia and raised D-dimer all need follow-up.",
   "Recognize cues in viral pneumonia on high-flow oxygen."),

calc(M(PA, PNEU, "analyze_cues", "difficult", RESP, "ms"),
   "The ROX index helps predict whether high-flow oxygen will fail. ROX = (SpO2 / FiO2) / respiratory rate. Using "
   "the 1000 values, what is the client's ROX index? Record your answer to one decimal place.",
   "points", 1, "ROX index", 5.1,
   "SpO2 92 / FiO2 0.60 = 153.3. 153.3 / 30 = 5.1. A ROX of 4.88 or more suggests high-flow oxygen is likely to "
   "succeed; below 3.85 suggests failure and a need to escalate.",
   "Calculate and interpret the ROX index."),

cloze(M(PA, PNEU, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "At 1000 the risk of high-flow failure is {{b1}}. The finding that would most strongly suggest the client needs "
   "intubation is {{b2}}.",
   [[("lower, since the ROX is above 4.88, but the client needs close watching", True,
      "A reassuring ROX, but the clinical picture can change quickly."),
     ("very high", False, "The ROX is above 4.88."),
     ("zero", False, "The client is still severely ill."),
     ("impossible to judge", False, "The ROX helps judge it.")],
    [("a falling ROX with rising respiratory rate and accessory muscle use", True, "Shows tiring and failing support."),
     ("a heart rate of 104/min", False, "Expected with illness and anxiety."),
     ("a glucose of 248 mg/dL", False, "Needs treatment but does not indicate intubation."),
     ("a dry cough", False, "Common and not a marker of failure.")]],
   "Trend the ROX and the work of breathing to catch high-flow failure early.",
   "Prioritize the risk of high-flow failure."),

matrix_mc(M(SIC, TBP, "generate_solutions", "difficult", RESP, "cdc"),
   "For each infection-control measure, indicate whether it is indicated or not indicated for this client on "
   "high-flow oxygen.",
   ["Indicated", "Not indicated"],
   [("Fit-tested N95 respirator, eye protection, gown and gloves for staff", 0,
     "High-flow oxygen can aerosolize the virus; airborne-level protection is used."),
    ("An airborne infection isolation room if available", 0, "Aerosol risk."),
    ("A surgical mask over the cannula when others are in the room", 0, "Reduces dispersion."),
    ("Removing PPE outside the room before hand hygiene", 1, "PPE is removed in the anteroom or at the door, followed by hand hygiene."),
    ("Clustering care to reduce room entries", 0, "Limits exposure while meeting needs.")],
   "High-flow oxygen in COVID-19 calls for respirators, eye protection, an isolation room and careful doffing.",
   "Generate infection-control measures for aerosol-generating oxygen therapy.",
   ex=[note("Orders, 1015",
       "Awake prone positioning as tolerated. Dexamethasone 6 mg daily. Enoxaparin 40 mg daily. Insulin correction "
       "scale. Recheck ROX every 2 hours.", id="hf-orders")]),

sata(M(PA, PNEU, "take_action", "difficult", RESP, "ms"),
   "Review the 1400 findings. Which actions should the nurse take?",
   [("Notify the provider and the rapid response team", True, "A falling ROX with tiring means high-flow is failing."),
    ("Prepare for intubation", True, "Delayed intubation in high-flow failure raises mortality."),
    ("Keep the client upright or prone as tolerated", True, "Improves oxygenation while awaiting the team."),
    ("Stay with the client", True, "The client is unstable."),
    ("Give lorazepam to reduce anxiety and respiratory rate", False, "Sedation can cause respiratory arrest."),
    ("Reduce the flow to 30 L/min for comfort", False, "Lowers support when it is needed most.")],
   "A ROX below 3.85 with accessory muscle use means escalation to intubation.",
   "Take action when high-flow oxygen fails.",
   ex=[flow("1400", ["1000", "1400"], [
       ("SpO2", ["92%", "88%"]),
       ("FiO2", ["0.60", "0.90"]),
       ("Respirations", ["30/min", "38/min"]),
       ("ROX index", ["5.1", "2.6"]),
       ("Breathing", ["Short sentences", "Accessory muscles, single words"])], id="hf-1400")]),

matrix_mc(M(PA, PNEU, "evaluate_outcomes", "difficult", RESP, "ms ph"),
   "The client was intubated. Review the findings 3 days later. For each finding, indicate whether it shows "
   "improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("PaO2/FiO2 ratio up from 110 to 190", 0, "Oxygenation improving."),
    ("FiO2 down to 0.45 with PEEP 10", 0, "Less support needed."),
    ("Glucose 320 to 380 mg/dL on dexamethasone", 1, "Steroid hyperglycemia needs an insulin infusion or adjustment."),
    ("New swelling and pain in the left calf", 1, "Possible DVT despite prophylaxis; needs ultrasound."),
    ("Passed the daily spontaneous breathing trial screen", 0, "Moving toward extubation.")],
   "Lung function is improving, but steroid hyperglycemia and a possible DVT need action.",
   "Evaluate outcomes in severe viral pneumonia.",
   ex=[flow("Day 3 on the ventilator", ["0800"], [
       ("PaO2/FiO2", ["190"]),
       ("Ventilator", ["FiO2 0.45, PEEP 10"]),
       ("Glucose", ["320 to 380 mg/dL"]),
       ("Left calf", ["Swollen, tender"]),
       ("Breathing trial screen", ["Passed"])], id="hf-day3")]),
]

# ================================================================================================
# Case 4: Over-the-counter cold medicines in an older adult
# ================================================================================================
otc_tabs = [
    ("otc-notes", "Nurses' notes", [note("Emergency department, 1600",
        "A 78-year-old client with benign prostatic hyperplasia, hypertension, narrow-angle glaucoma and depression "
        "(sertraline 100 mg daily) took a \"nighttime multi-symptom cold\" product four times in 24 hours, plus "
        "extra cough syrup. The product contains pseudoephedrine, diphenhydramine and dextromethorphan. The "
        "client's partner says the client is \"seeing things\" and has not urinated since morning.", id="otc-note")]),
    ("otc-findings", "Assessment", [table("Assessment", ["1600"], [
        ("Mental status", ["Agitated, confused, picking at the air"]),
        ("Temperature", ["38.6 C (101.5 F)"]),
        ("Blood pressure", ["178/96 mm Hg"]),
        ("Heart rate", ["116/min"]),
        ("Neuromuscular", ["Tremor, inducible ankle clonus, brisk reflexes"]),
        ("Skin", ["Sweating"]),
        ("Bladder scan", ["900 mL"]),
        ("Eyes", ["Severe right eye pain, blurred vision, halos, mid-dilated pupil"])], id="otc-table")]),
]

otc_qs = [
hl_text(M(PPT, ADVERSE, "recognize_cues", "moderate", RESP, "ph"),
   "Highlight the findings that require immediate action.",
   ["The client is ",
    ("agitated, confused and picking at the air", True, "Delirium."),
    ", with ",
    ("tremor and inducible clonus", True, "Clonus with hyperthermia and sweating suggests serotonin syndrome."),
    ", a temperature of ",
    ("38.6 C (101.5 F)", True, "Hyperthermia."),
    ", ",
    ("a bladder scan of 900 mL", True, "Acute urinary retention."),
    " and ",
    ("severe eye pain with halos and a mid-dilated pupil", True, "Acute angle-closure glaucoma, which threatens sight."),
    ". The client has taken ",
    ("sertraline for years", False, "Long-standing use is not the problem by itself; the new combination is."),
    "."],
   "Delirium, serotonin toxicity, urinary retention and angle-closure glaucoma each need urgent action.",
   "Recognize cues of harm from over-the-counter cold medicines."),

matrix_mr(M(PPT, RESP_MEDS, "analyze_cues", "difficult", RESP, "ph"),
   "For each finding, indicate the drug or drugs most likely responsible. Each row may have more than one answer.",
   ["Pseudoephedrine", "Diphenhydramine", "Dextromethorphan with sertraline"],
   [("Urinary retention", {0, 1}, "Alpha stimulation tightens the bladder neck; anticholinergic effects weaken the bladder."),
    ("Blood pressure 178/96 mm Hg", {0}, "A sympathomimetic decongestant raises pressure."),
    ("Confusion and visual hallucinations", {1}, "Anticholinergic delirium, common in older adults."),
    ("Clonus, hyperthermia and sweating", {2}, "Serotonin syndrome from two serotonergic drugs."),
    ("Acute angle-closure glaucoma", {0, 1}, "Pupil dilation from either drug can close a narrow angle.")],
   "Combination cold products give older adults several harmful effects at once.",
   "Analyze drug effects of cold medicines."),

matrix_mc(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", RESP, "ph"),
   "For each problem, indicate its urgency.",
   ["Emergency now", "Urgent but less immediate"],
   [("Serotonin syndrome with hyperthermia", 0, "Can progress to seizures and death."),
    ("Acute angle-closure glaucoma", 0, "Vision can be lost within hours."),
    ("Urinary retention of 900 mL", 0, "Needs drainage now to prevent bladder and kidney injury."),
    ("Blood pressure 178/96 mm Hg", 1, "Treated, but expected to fall as the drugs clear.")],
   "Serotonin toxicity, angle closure and retention are emergencies.",
   "Prioritize drug-induced emergencies."),

matrix_mc(M(PPT, ADVERSE, "generate_solutions", "difficult", RESP, "ph"),
   "For each intervention, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Stop the cold product and hold sertraline", 0, "Removes the causes."),
    ("Cyproheptadine as prescribed", 0, "A serotonin antagonist used in serotonin syndrome."),
    ("Lorazepam IV for agitation and muscle activity", 0, "Benzodiazepines calm agitation and reduce heat production."),
    ("Haloperidol IM for the hallucinations", 1, "Anticholinergic and dopamine-blocking effects can worsen this picture."),
    ("Physical restraints for agitation", 1, "Struggling raises temperature and muscle breakdown."),
    ("Insert a urinary catheter", 0, "Drains the bladder.")],
   "Stop the drugs, give a serotonin antagonist and benzodiazepines, drain the bladder; avoid antipsychotics and "
   "restraints.",
   "Generate solutions for serotonin syndrome and anticholinergic toxicity.",
   ex=[note("Orders, 1615", "Ophthalmology to see the client now. Cyproheptadine 12 mg PO or NG. Lorazepam 1 mg IV as "
       "needed. Cooling measures. Urinary catheter.", id="otc-orders")]),

sata(M(PPT, ADVERSE, "take_action", "difficult", RESP, "ph ms"),
   "Which actions should the nurse take now?",
   [("Insert the urinary catheter and record the volume drained", True, "Relieves retention and documents the amount."),
    ("Apply cooling measures", True, "Treats hyperthermia."),
    ("Give the prescribed eye drops as soon as ophthalmology orders them", True, "Lowers eye pressure to save vision."),
    ("Monitor temperature, heart rhythm and clonus closely", True, "Tracks serotonin toxicity."),
    ("Dim the lights and keep the client lying flat", False, "Lying flat does not help; the eye team guides positioning."),
    ("Give acetaminophen as the main treatment for the fever", False, "Fever from muscle activity responds to cooling and benzodiazepines.")],
   "Drain the bladder, cool the client, protect the eye and monitor serotonin toxicity.",
   "Take action for multiple drug-induced emergencies.",
   ex=[flow("1630", ["1630"], [
       ("Temperature", ["38.9 C (102 F)"]),
       ("Clonus", ["Sustained"]),
       ("Eye pressure", ["Right eye 52 mm Hg (normal 10 to 21)"])], id="otc-1630")]),

matrix_mc(M(HPM, AGING, "evaluate_outcomes", "difficult", RESP, "ph"),
   "Before discharge, the client and partner discuss cold remedies. For each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll ask the pharmacist before buying any cold or sleep medicine.\"", 0, "The pharmacist can check interactions."),
    ("\"Nighttime cold products with antihistamines are safe if I take them only at night.\"", 1,
     "Sedating antihistamines are a risk for older adults at any time."),
    ("\"Decongestants are a problem with my prostate and my eyes.\"", 0, "Correct."),
    ("\"Cough medicine with dextromethorphan is fine with my sertraline.\"", 1, "Together they can cause serotonin syndrome."),
    ("\"Saline nasal spray and honey are safer choices for my symptoms.\"", 0, "Non-drug or low-risk options."),
    ("\"I'll take two cold products together if one isn't strong enough.\"", 1, "Doubles ingredients and the risk.")],
   "Older adults should check cold remedies with a pharmacist and avoid sedating antihistamines, decongestants "
   "and serotonergic cough medicines.",
   "Evaluate teaching on over-the-counter medicines in older adults.",
   ex=[note("Day 3", "Delirium resolved; urinating after catheter removal; eye pressure controlled after laser "
       "iridotomy; sertraline restarted.", id="otc-day3")]),
]

cases = [
    case("Carbon monoxide poisoning with smoke inhalation", co_tabs, co_qs),
    case("Flail chest with pulmonary contusion", flail_tabs, flail_qs),
    case("Viral pneumonia on high-flow nasal oxygen", hf_tabs, hf_qs),
    case("Over-the-counter cold medicines in an older adult", otc_tabs, otc_qs),
]

BUILT = build(107, "pulmonary-cases-co-flail-hfnc-otc-standalone",
              "Pulmonary case studies: carbon monoxide poisoning, flail chest, high-flow oxygen failure and cold "
              "medicine toxicity, with four standalone items",
              items, cases, case_conversion=True)
