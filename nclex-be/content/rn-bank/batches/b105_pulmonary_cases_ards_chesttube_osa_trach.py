"""Batch 105 (case conversion, chunk 2: pulmonary): four full NGN case studies (CASE-MAP.md).

Cases: ARDS on mechanical ventilation with prone positioning; hemorrhage and an air leak after lobectomy;
opioid-induced respiratory depression in obstructive sleep apnea; a tracheostomy that is blocked and then
dislodged. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a
management of care or safety question."""
from qb import *

RRP, PA, PPT, HPM, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.MOC", "SECE.SIC"
RESP = "respiratory"
VENT = "mechanical-ventilation"
CHEST = "chest-tube-management"
TRACH = "tracheostomy-and-suctioning"
IMPAIRED = "impaired-oxygenation"
OPIOID = "opioid-safety"
CALC = "medication-dose-calculations"
POSTOP = "postoperative-care"
LABS = "laboratory-values-and-monitoring"
HAI = "healthcare-associated-infection-prevention"
MEDSAFE = "medication-safety-systems"
DELEG = "delegation-and-supervision"
HOME = "equipment-and-home-safety"
ANTIDOTE = "antidotes-and-reversal-agents"

items = []

# ================================================================================================
# Case 1: ARDS on mechanical ventilation
# ================================================================================================
ards_tabs = [
    ("ards-notes", "Nurses' notes", [note("ICU, day 2, 0600",
        "A 55-year-old client admitted with severe acute pancreatitis and sepsis was intubated overnight for worsening "
        "hypoxemia. Height 170 cm; actual weight 98 kg; ideal body weight 66 kg. Chest x-ray: new bilateral "
        "infiltrates. An echocardiogram shows normal left ventricular function. Ventilator: volume control, tidal "
        "volume 650 mL, rate 18/min, PEEP 8 cm H2O, FiO2 0.80. Sedated on propofol.", id="ards-note")]),
    ("ards-data", "ABG and ventilator", [table("ABG and ventilator readings", ["0600"], [
        ("pH", ["7.31"]),
        ("PaCO2", ["49 mm Hg"]),
        ("PaO2", ["62 mm Hg"]),
        ("SpO2", ["90%"]),
        ("Plateau pressure", ["36 cm H2O"]),
        ("Peak inspiratory pressure", ["42 cm H2O"])], id="ards-table")]),
]

ards_qs = [
hl_table(M(PA, VENT, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["PaO2 on FiO2 0.80", "62 mm Hg"], True, "Severe hypoxemia despite high oxygen."),
    (["Chest x-ray", "New bilateral infiltrates"], True, "With normal heart function, this fits ARDS."),
    (["Tidal volume", "650 mL (about 10 mL/kg of ideal body weight)"], True,
     "Large tidal volumes overstretch injured lungs; ARDS uses about 6 mL/kg of ideal body weight."),
    (["Plateau pressure", "36 cm H2O"], True, "Above 30 cm H2O raises the risk of lung injury and barotrauma."),
    (["Echocardiogram", "Normal left ventricular function"], False,
     "Reassuring: it shows the infiltrates are not from heart failure."),
    (["Sedation", "Propofol infusion"], False, "Expected on the ventilator.")],
   "Severe hypoxemia with bilateral infiltrates and normal heart function is ARDS; high tidal volume and plateau "
   "pressure need correcting.",
   "Recognize cues of ARDS on the ventilator."),

cloze(M(PA, VENT, "analyze_cues", "difficult", RESP, "ms lab"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's PaO2/FiO2 ratio is {{b1}}, which is {{b2}} ARDS. The most likely trigger is {{b3}}.",
   [[("about 78", True, "62 divided by 0.80 is 77.5."),
     ("about 50", False, "Check the calculation: 62 / 0.80."),
     ("about 310", False, "That would be a normal or mild value."),
     ("about 160", False, "Too high for these numbers.")],
    [("severe", True, "A ratio of 100 or less is severe ARDS."),
     ("mild", False, "Mild is 201 to 300."),
     ("moderate", False, "Moderate is 101 to 200."),
     ("not", False, "The ratio, timing and bilateral infiltrates without heart failure meet the definition.")],
    [("systemic inflammation from pancreatitis and sepsis", True, "Both are common indirect causes of ARDS."),
     ("left-sided heart failure", False, "The echocardiogram is normal."),
     ("a pneumothorax", False, "Not shown on the x-ray."),
     ("the propofol infusion", False, "Not a cause of ARDS.")]],
   "The PaO2/FiO2 ratio grades ARDS; this client has severe ARDS from pancreatitis and sepsis.",
   "Analyze ARDS severity with the PaO2/FiO2 ratio."),

matrix_mc(M(PA, VENT, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "For each risk, indicate its priority in the next 24 hours.",
   ["High priority", "Lower priority"],
   [("Ventilator-induced lung injury from high volumes and pressures", 0, "Current settings are injuring the lungs."),
    ("Barotrauma, such as pneumothorax", 0, "A plateau pressure above 30 raises this risk."),
    ("Refractory hypoxemia", 0, "The PaO2 is barely adequate on 80% oxygen."),
    ("Constipation from immobility", 1, "Real but less urgent."),
    ("Deconditioning", 1, "Addressed once the client is stable.")],
   "In severe ARDS, the priorities are protecting the lungs and keeping oxygenation adequate.",
   "Prioritize risks in severe ARDS."),

calc(M(PA, VENT, "generate_solutions", "difficult", RESP, "ms"),
   "The provider orders lung-protective ventilation at 6 mL/kg of ideal body weight. What tidal volume should the "
   "ventilator be set to? Record your answer as a whole number.",
   "mL", 0, "Tidal volume", 396,
   "Tidal volume is based on ideal body weight, not actual weight, because lung size follows height. 66 kg x 6 mL/kg "
   "= 396 mL. Using the actual weight of 98 kg would give 588 mL and overstretch the lungs.",
   "Calculate a lung-protective tidal volume.",
   ex=[note("New orders, 0630",
       "Lung-protective ventilation: tidal volume 6 mL/kg of ideal body weight, plateau pressure 30 cm H2O or less, "
       "PEEP 14 cm H2O. Prone positioning for 16 hours a day. Ideal body weight 66 kg; actual weight 98 kg.",
       id="ards-orders")]),

sata(M(PA, VENT, "take_action", "difficult", RESP, "ms aha"),
   "At 1100 the high-pressure alarm sounds. Review the findings. Which actions should the nurse take?",
   [("Disconnect and ventilate with a bag-valve device on 100% oxygen", True,
     "Removes the ventilator as a cause and supports oxygenation."),
    ("Call for emergency needle decompression", True,
     "Absent breath sounds on one side, tracheal shift and hypotension mean tension pneumothorax."),
    ("Check the tube is still at 23 cm at the lips", True,
     "Rules out displacement into a mainstem bronchus as part of the DOPE check."),
    ("Silence the alarm and recheck the client in 10 minutes", False, "The alarm shows a life-threatening change."),
    ("Increase the PEEP setting to improve the oxygenation", False, "More pressure worsens a tension pneumothorax."),
    ("Suction the airway and wait to see if the SpO2 improves", False, "Suctioning does not treat tension pneumothorax.")],
   "A sudden high-pressure alarm with one-sided absent breath sounds, tracheal shift and hypotension is tension "
   "pneumothorax: bag on 100%, check the tube and get decompression now.",
   "Take action for barotrauma on the ventilator.",
   ex=[flow("1100", ["1100"], [
       ("Peak pressure", ["58 cm H2O (alarm)"]),
       ("SpO2", ["78%"]),
       ("Blood pressure", ["76/40 mm Hg"]),
       ("Breath sounds", ["Absent on the left"]),
       ("Trachea", ["Shifted to the right"]),
       ("Endotracheal tube", ["23 cm at the lips, as documented"])], id="ards-1100")]),

matrix_mc(M(SIC, HAI, "evaluate_outcomes", "difficult", RESP, "cdc ms"),
   "A chest tube was placed, and the client has had 16 hours of prone positioning. The nurse checks the care "
   "bundle. For each finding, indicate whether it meets the standard or needs correction.",
   ["Meets the standard", "Needs correction"],
   [("PaO2/FiO2 ratio up from 78 to 165", 0, "Proning has improved oxygenation."),
    ("Head of the bed raised 30 degrees (reverse Trendelenburg while prone)", 0, "Reduces aspiration and VAP risk."),
    ("Endotracheal tube cuff pressure 14 cm H2O", 1, "Below 20 cm H2O lets secretions leak past the cuff; keep 20 to 30."),
    ("Daily sedation interruption and readiness check done", 0, "Shortens ventilation and lowers VAP."),
    ("Eyes taped and lubricated while prone", 0, "Protects the corneas."),
    ("Red, non-blanching area on the forehead and chin", 1, "A pressure injury from proning; reposition and pad the face.")],
   "Oxygenation improved with proning; the cuff pressure and facial pressure injury need correcting.",
   "Evaluate a ventilator care bundle and prone positioning.",
   ex=[flow("Day 3, 0800", ["0800"], [
       ("PaO2/FiO2 ratio", ["165"]),
       ("Bed position", ["Reverse Trendelenburg 30 degrees, prone"]),
       ("Cuff pressure", ["14 cm H2O"]),
       ("Sedation interruption", ["Done at 0600"]),
       ("Eyes", ["Taped and lubricated"]),
       ("Skin", ["Non-blanching redness on the forehead and chin"])], id="ards-day3")]),
]

# ================================================================================================
# Case 2: Hemorrhage and an air leak after lobectomy
# ================================================================================================
lob_tabs = [
    ("lob-notes", "Nurses' notes", [note("Surgical unit, postoperative day 0, 2200",
        "A 62-year-old client had a right upper lobectomy for lung cancer at 1400. A right chest tube is connected to a "
        "drainage system with water seal and -20 cm H2O suction. The client has morphine patient-controlled "
        "analgesia (PCA).", id="lob-note")]),
    ("lob-flow", "Flowsheet", [flow("Vital signs and chest tube", ["1800", "2000", "2200"], [
        ("Blood pressure", ["132/78", "118/70", "102/64"]),
        ("Heart rate", ["88", "102", "118"]),
        ("SpO2", ["95%", "94%", "92%"]),
        ("Drainage, mL/hour", ["60, dark red", "150, bright red", "160, bright red"]),
        ("Water seal chamber", ["Tidaling; bubbles on coughing", "Tidaling; bubbles on coughing", "Continuous vigorous bubbling"]),
        ("Skin around the insertion site", ["Normal", "Normal", "Crackles under the skin spreading up the neck"])],
        id="lob-flowsheet")]),
]

lob_qs = [
hl_table(M(PA, CHEST, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings at 2200 that require follow-up.",
   ["Finding", "2200"],
   [(["Drainage", "150 to 160 mL/hour, bright red, for 2 hours"], True,
     "More than 100 mL/hour of bright red drainage suggests active bleeding."),
    (["Heart rate and blood pressure", "118/min; 102/64 mm Hg"], True, "Compensation for blood loss."),
    (["Water seal", "Continuous vigorous bubbling"], True, "A new large air leak."),
    (["Skin", "Crackles spreading up the neck"], True, "Expanding subcutaneous emphysema."),
    (["Water seal", "Tidaling"], False, "Rise and fall with breathing shows the tube is patent."),
    (["Analgesia", "Morphine PCA"], False, "Expected after thoracic surgery.")],
   "Heavy bright red drainage with falling pressure, a new large air leak and spreading emphysema need follow-up.",
   "Recognize cues of complications after lobectomy."),

matrix_mc(M(PA, CHEST, "analyze_cues", "difficult", RESP, "ms"),
   "For each chest drainage finding after lobectomy, indicate whether it is expected or a problem.",
   ["Expected", "A problem"],
   [("Tidaling in the water seal chamber", 0, "Shows the tube is open to the pleural space."),
    ("Bubbles in the water seal when the client coughs", 0, "A small air leak from the lung surface is common after lobectomy."),
    ("Continuous vigorous bubbling in the water seal", 1, "A large leak from the lung or the system."),
    ("Drainage of 160 mL/hour of bright red blood", 1, "Suggests bleeding needing surgical review."),
    ("Gentle continuous bubbling in a wet suction-control chamber", 0, "Shows suction is applied.")],
   "Tidaling and small cough-related bubbling are expected; continuous vigorous water seal bubbling and heavy "
   "bright red drainage are not.",
   "Analyze chest drainage findings."),

cloze(M(PA, CHEST, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most urgent problem is {{b1}}, shown by {{b2}}.",
   [[("hemorrhage into the chest", True, "Blood loss with shock signs is the most immediate threat."),
     ("the air leak", False, "Needs assessment, but the client is still oxygenating."),
     ("subcutaneous emphysema", False, "Uncomfortable but rarely dangerous by itself."),
     ("pain", False, "Not the most urgent.")],
    [("drainage over 100 mL/hour with a rising heart rate and falling blood pressure", True,
      "These together point to active bleeding."),
     ("tidaling in the water seal", False, "Expected."),
     ("an SpO2 of 92%", False, "Slightly low but not the main threat."),
     ("use of the PCA", False, "Not the cause.")]],
   "Active bleeding with hemodynamic change is the priority after lobectomy.",
   "Prioritize hemorrhage after lobectomy."),

grouping(M(MOC, DELEG, "generate_solutions", "difficult", RESP, "mgmt"),
   "The RN plans care while waiting for the surgeon. For each team member, select the tasks that fit their role.",
   [("RN", [
       ("Assess the chest tube system, dressing and lung sounds", True, "Assessment of an unstable client stays with the RN."),
       ("Call the surgeon with an SBAR report", True, "Communicating a change in condition is the RN's role."),
       ("Empty the drainage unit to measure it", False, "A chest drainage unit is never emptied; output is marked on it.")]),
    ("LPN", [
       ("Mark the drainage level on the unit every hour and report it", True, "A routine measurement with clear parameters."),
       ("Decide whether the air leak comes from the client or the tubing", False, "That needs RN assessment.")]),
    ("UAP", [
       ("Take vital signs every 15 minutes and report a systolic below 100", True, "Measuring and reporting is within the role."),
       ("Strip the chest tube to keep it draining", False, "Stripping is not done routinely and is not delegated.")])],
   "The RN assesses and communicates; the LPN measures marked drainage; the UAP takes vital signs against stated "
   "limits. No one empties the drainage unit or strips the tube routinely.",
   "Plan delegation for a client with a chest tube complication.",
   ex=[note("Staffing, 2210",
       "The RN is working with an LPN and a UAP. The surgeon will arrive in 30 minutes. Type and crossmatch sent.",
       id="lob-staff")]),

sata(M(SIC, HOME, "take_action", "difficult", RESP, "ms"),
   "At 2240, while being turned, the tubing pulls apart from the drainage unit. Which actions should the nurse take?",
   [("Put the tube end 2 cm under sterile water", True,
     "This makes a temporary water seal, so air cannot enter the chest."),
    ("Connect a new sterile drainage system", True, "Restores the closed system."),
    ("Assess breathing, SpO2 and breath sounds", True, "Checks for a developing pneumothorax."),
    ("Clamp the chest tube until the surgeon arrives", False,
     "Clamping for long periods with an air leak can cause tension pneumothorax."),
    ("Leave the chest tube open to the air for now", False, "Air will enter the pleural space."),
    ("Reconnect the contaminated end without cleaning it", False, "Increases infection risk; use a new sterile system.")],
   "A disconnected chest tube needs a temporary water seal and a new system, not prolonged clamping.",
   "Take action for a disconnected chest tube.",
   ex=[note("2240", "The drainage tubing separates from the unit. The client is coughing.", id="lob-2240")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", RESP, "ms"),
   "The client returned to surgery for a bleeding vessel. Review the findings the next morning. For each finding, "
   "indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Drainage 30 mL/hour, serous", 0, "Bleeding has stopped."),
    ("Heart rate 86/min, blood pressure 124/72 mm Hg", 0, "Volume is restored."),
    ("Bubbling only on coughing", 0, "The large leak is sealed."),
    ("Crackles under the skin of the neck, not spreading", 0, "Stable emphysema is reabsorbed over days."),
    ("Uses the incentive spirometer twice in 8 hours, citing pain", 1,
     "Splinting from pain risks atelectasis and pneumonia; pain control and hourly spirometry are needed."),
    ("Respirations 8/min, sleepy, PCA used 30 times overnight", 1, "Opioid-induced respiratory depression; stop the PCA and assess.")],
   "Bleeding and the air leak are controlled, but poor lung expansion and opioid-induced hypoventilation need "
   "action.",
   "Evaluate outcomes after a return to surgery.",
   ex=[flow("Postoperative day 1, 0700", ["0700"], [
       ("Drainage", ["30 mL/hour, serous"]),
       ("Vital signs", ["HR 86/min, BP 124/72 mm Hg, RR 8/min"]),
       ("Water seal", ["Bubbles only on coughing"]),
       ("Skin", ["Neck crepitus, not spreading"]),
       ("Incentive spirometer", ["Used twice since midnight; says it hurts"]),
       ("PCA", ["30 doses overnight; client sleepy"])], id="lob-pod1")]),
]

# ================================================================================================
# Case 3: Opioid-induced respiratory depression in obstructive sleep apnea
# ================================================================================================
osa_tabs = [
    ("osa-notes", "Nurses' notes", [note("Surgical unit, 0300",
        "A 48-year-old client with a BMI of 44 and obstructive sleep apnea had a laparoscopic hernia repair "
        "yesterday. The client's CPAP machine was left at home. Pain plan: morphine PCA 1 mg demand with a 1 mg/hour "
        "basal rate, gabapentin 300 mg three times daily, and diphenhydramine 50 mg at 2200 for sleep. Oxygen 4 L/min "
        "by nasal cannula. At 0300 the client is snoring loudly with pauses of 20 seconds and is hard to wake.",
        id="osa-note")]),
    ("osa-vitals", "Monitoring", [flow("Monitoring", ["2300", "0300"], [
        ("Respirations", ["14/min", "7/min"]),
        ("SpO2", ["97%", "93% on 4 L/min"]),
        ("End-tidal CO2", ["42 mm Hg", "64 mm Hg"]),
        ("Sedation (Pasero scale)", ["2 (slightly drowsy, easily aroused)", "4 (somnolent, minimal response)"]),
        ("Pain", ["4 of 10", "Unable to rate"])], id="osa-flow")]),
]

osa_qs = [
hl_table(M(PPT, OPIOID, "recognize_cues", "moderate", RESP, "ph ms"),
   "Highlight the findings at 0300 that require immediate action.",
   ["Finding", "0300"],
   [(["Respirations", "7/min with 20-second pauses"], True, "Bradypnea with apnea."),
    (["Sedation", "Pasero 4"], True, "Somnolence with minimal response means opioids must stop."),
    (["End-tidal CO2", "64 mm Hg"], True, "Hypoventilation: CO2 is rising."),
    (["SpO2", "93% on 4 L/min"], False,
     "It looks acceptable, but supplemental oxygen masks hypoventilation; capnography and sedation tell the truth."),
    (["Pain", "Unable to rate"], False, "Reflects sedation, not pain control.")],
   "Sedation and breathing, not SpO2 on oxygen, reveal opioid-induced respiratory depression.",
   "Recognize cues of opioid-induced respiratory depression."),

matrix_mc(M(PPT, OPIOID, "analyze_cues", "difficult", RESP, "ph"),
   "For each factor, indicate whether it raises the client's risk of respiratory depression.",
   ["Raises the risk", "Does not raise the risk"],
   [("Untreated obstructive sleep apnea without CPAP", 0, "Airway collapse during sleep worsens with sedatives."),
    ("A basal (continuous) PCA rate", 0, "Opioid is given even while the client sleeps."),
    ("Gabapentin", 0, "Adds sedation and respiratory depression with opioids."),
    ("Diphenhydramine at bedtime", 0, "A sedating antihistamine adds to CNS depression."),
    ("Oxygen at 4 L/min", 0, "It does not cause depression, but it hides it from pulse oximetry and delays detection."),
    ("A laparoscopic rather than open incision", 1, "Less pain, not more risk.")],
   "Several sedating drugs, a basal opioid infusion and untreated sleep apnea add up; oxygen hides the result.",
   "Analyze risk factors for opioid-induced respiratory depression."),

cloze(M(PPT, OPIOID, "prioritize_hypotheses", "difficult", RESP, "ph"),
   "Complete the sentences by choosing from the lists of options.",
   "The client has {{b1}}. The best ongoing measure of breathing for this client is {{b2}}.",
   [[("opioid-induced respiratory depression with hypercapnia", True, "Low rate, high CO2 and deep sedation."),
     ("expected postoperative sleep", False, "Pasero 4 and an EtCO2 of 64 are not normal sleep."),
     ("a pulmonary embolism", False, "Not supported."),
     ("a reaction to the anesthetic gas", False, "Too late after surgery.")],
    [("continuous capnography (end-tidal CO2)", True, "Detects hypoventilation even when SpO2 is normal on oxygen."),
     ("pulse oximetry alone", False, "Misleading on supplemental oxygen."),
     ("blood pressure every 4 hours", False, "Not a breathing measure."),
     ("pain scores", False, "Not a breathing measure.")]],
   "Capnography and sedation scores detect opioid-induced hypoventilation earlier than SpO2.",
   "Prioritize opioid-induced respiratory depression and its monitoring."),

matrix_mc(M(PPT, ANTIDOTE, "generate_solutions", "difficult", RESP, "ph"),
   "For each intervention, indicate whether it is indicated or contraindicated now.",
   ["Indicated", "Contraindicated"],
   [("Stop the PCA and remove the demand button", 0, "Stops further opioid."),
    ("Naloxone 0.04 mg IV, repeated every 2 to 3 minutes until breathing improves", 0,
     "Small, titrated doses reverse depression without causing severe pain or withdrawal."),
    ("Naloxone 2 mg IV push all at once", 1,
     "A full dose can cause sudden severe pain, hypertension, pulmonary edema and dysrhythmias in a postoperative client."),
    ("Let the client sleep, since the SpO2 is 93%", 1, "Sleep with an EtCO2 of 64 is dangerous."),
    ("Raise the head of the bed and apply CPAP from the respiratory therapy department", 0, "Opens the airway."),
    ("Give the 0600 gabapentin dose as scheduled", 1, "Hold sedating drugs until the provider reviews them.")],
   "Stop the opioid, reverse it carefully with small doses, open the airway with CPAP, and hold other sedatives.",
   "Generate solutions for opioid-induced respiratory depression.",
   ex=[note("Standing protocol",
       "For Pasero sedation 4 or respirations below 8/min: stop opioids, give naloxone 0.04 mg IV every 2 minutes as "
       "needed, call the rapid response team.", id="osa-protocol")]),

sata(M(PPT, OPIOID, "take_action", "difficult", RESP, "ph"),
   "After two doses of naloxone, the client's respirations are 14/min. Which actions should the nurse take next?",
   [("Monitor sedation and EtCO2 closely for the next 2 hours", True,
     "Naloxone wears off in 30 to 90 minutes, before morphine does, so depression can return."),
    ("Tell the provider and ask for a non-opioid pain plan", True, "Pain will return once the opioid is reversed."),
    ("Keep the client on CPAP while asleep", True, "Treats the sleep apnea that added to the risk."),
    ("Restart the PCA at the same settings once awake", False, "The same settings caused the event."),
    ("Stop monitoring once the client is talking", False, "Recurrent depression is likely."),
    ("Give diphenhydramine to help the client go back to sleep", False, "Another sedative would add to the risk.")],
   "Naloxone is short-acting: watch for recurrence, change the pain plan and treat the sleep apnea.",
   "Take action after naloxone reversal.",
   ex=[flow("0320", ["0320"], [
       ("Respirations", ["14/min"]),
       ("Sedation", ["Pasero 2"]),
       ("End-tidal CO2", ["46 mm Hg"]),
       ("Pain", ["7 of 10"])], id="osa-0320")]),

matrix_mc(M(SIC, MEDSAFE, "evaluate_outcomes", "difficult", RESP, "ph mgmt"),
   "Review the findings at 0500. For each finding, indicate whether it shows a safe situation or needs follow-up.",
   ["Safe", "Needs follow-up"],
   [("Ketorolac and scheduled acetaminophen started for pain", 0, "Multimodal non-opioid analgesia."),
    ("CPAP on while asleep", 0, "Treats obstruction."),
    ("Respirations 12/min, EtCO2 48 mm Hg, Pasero 2", 0, "Breathing is adequate."),
    ("The client's partner says, \"I pressed the pain button for them while they slept\"", 1,
     "PCA by proxy defeats its safety: only the client may press the button. Teach and report."),
    ("The incident has not been reported", 1, "An event needing naloxone is reported so the system can be reviewed."),
    ("The PCA order still includes a basal rate", 1, "Ask the provider to remove the basal infusion before any restart.")],
   "Breathing has stabilized, but PCA by proxy, the basal rate and the unreported event are safety gaps.",
   "Evaluate safety after an opioid-related event.",
   ex=[note("0500", "The client's partner arrived at 0100 and stayed at the bedside.", id="osa-0500")]),
]

# ================================================================================================
# Case 4: Tracheostomy blocked, then dislodged
# ================================================================================================
trach_tabs = [
    ("trach-notes", "Nurses' notes", [note("Medical unit, day 10 after tracheostomy",
        "A 70-year-old client had a cuffed tracheostomy placed 10 days ago after prolonged ventilation; the stoma "
        "is now well formed. The client has a spare tracheostomy tube of the same size and one size smaller, an "
        "obturator and suction at the bedside. Humidified oxygen at 35% by trach collar.", id="trach-note")]),
    ("trach-vitals", "Assessment", [flow("Assessment", ["0800", "1000"], [
        ("Respirations", ["18/min", "30/min"]),
        ("SpO2", ["95%", "86%"]),
        ("Breath sounds", ["Coarse crackles", "Very faint; high-pitched sound at the tube"]),
        ("Behavior", ["Calm", "Restless, pulling at the collar"]),
        ("Secretions", ["Thick, tan", "Unable to cough any out"])], id="trach-flow")]),
]

trach_qs = [
hl_table(M(PA, TRACH, "recognize_cues", "moderate", RESP, "ms"),
   "Highlight the findings at 1000 that require immediate action.",
   ["Finding", "1000"],
   [(["SpO2", "86%"], True, "Hypoxemia."),
    (["Respirations", "30/min"], True, "Respiratory distress."),
    (["Breath sounds", "Very faint; high-pitched sound at the tube"], True, "Little air is moving through the tube."),
    (["Behavior", "Restless, pulling at the collar"], True, "Restlessness is an early sign of hypoxia."),
    (["Secretions", "Unable to cough any out"], True, "Thick secretions that will not clear suggest a plug."),
    (["Oxygen", "35% humidified by trach collar, as prescribed"], False, "The prescribed setup; not a new finding."),
    (["Temperature", "37.1 C (98.8 F)"], False, "Normal; infection is not the issue now.")],
   "Distress, hypoxemia and poor air movement in a client with thick secretions suggest an obstructed tube.",
   "Recognize cues of tracheostomy obstruction."),

dtable(M(PA, TRACH, "analyze_cues", "difficult", RESP, "ms"),
   "For each finding, choose the most likely cause.",
   ["Finding", "Most likely cause"],
   [(["A suction catheter passes, but returns a thick plug of mucus and air movement improves"], [
       ("Mucus plug in the tube or airway", True, "Clearing the plug restores flow."),
       ("Tube dislodged from the trachea", False, "A catheter usually cannot pass into the trachea."),
       ("Cuff leak", False, "A leak causes air escaping from the mouth."),
       ("Bronchospasm", False, "Would cause wheezing.")]),
    (["The suction catheter cannot pass, the client can speak, and the tube sits further out than marked"], [
       ("Tube dislodged into the tissues", True, "A displaced tube lies outside the trachea."),
       ("Mucus plug", False, "A plug would not let the client speak around a cuffed tube."),
       ("Normal function", False, "These are danger signs."),
       ("Pneumothorax", False, "Not shown by these findings.")]),
    (["Air and sound escape from the mouth during ventilation"], [
       ("Cuff leak or underinflated cuff", True, "Air passes around the cuff."),
       ("Mucus plug", False, "A plug blocks airflow."),
       ("Correct tube placement", False, "This is a leak."),
       ("Dislodgement", False, "Possible, but a leak is more likely.")])],
   "A passable catheter with a plug means obstruction; a catheter that will not pass with a tube further out means "
   "dislodgement.",
   "Analyze causes of tracheostomy distress."),

matrix_mc(M(PA, TRACH, "prioritize_hypotheses", "difficult", RESP, "ms"),
   "For each possible cause of the client's distress at 1000, indicate how likely it is.",
   ["Most likely", "Possible, check", "Unlikely"],
   [("Mucus plug from thick secretions", 0, "Thick secretions that cannot be cleared plus a high-pitched sound."),
    ("Dislodged tube", 1, "Restlessness and pulling at the collar make this possible; check the tube position."),
    ("Pulmonary embolism", 2, "Not suggested by the airway sounds."),
    ("Anxiety alone", 2, "Hypoxemia has a physical cause here.")],
   "Thick secretions make a mucus plug most likely, but dislodgement must be checked.",
   "Prioritize causes of tracheostomy distress."),

matrix_mc(M(PA, TRACH, "generate_solutions", "difficult", RESP, "ms fund"),
   "The nurse prepares to suction the tracheostomy. For each step, indicate whether it is correct practice.",
   ["Correct", "Incorrect"],
   [("Give 100% oxygen for 30 to 60 seconds before suctioning", 0, "Prevents hypoxemia during suctioning."),
    ("Apply suction only while withdrawing the catheter", 0, "Reduces mucosal damage."),
    ("Limit each pass to 10 seconds", 0, "Longer passes cause hypoxemia."),
    ("Use suction pressure of 80 to 120 mm Hg", 0, "Higher pressures damage the mucosa."),
    ("Instill saline before every pass to loosen secretions", 1, "Routine saline is not recommended; humidification is."),
    ("Make as many passes as needed without a break", 1, "Rest and reoxygenate between passes; limit to about 3.")],
   "Preoxygenate, use suction only on withdrawal, keep passes short and pressures safe, and humidify rather than "
   "instilling saline.",
   "Generate a safe suctioning plan.",
   ex=[note("Plan", "Suction now; humidified oxygen; consider a nebulized mucolytic as prescribed.", id="trach-plan")]),

sata(M(PA, TRACH, "take_action", "difficult", RESP, "ms"),
   "Suctioning cleared a plug. At 1300, during a coughing fit, the whole tube comes out. Which actions should the "
   "nurse take?",
   [("Call for help", True, "A second person and the airway team are needed."),
    ("Insert the spare tube with its obturator, then remove the obturator", True,
     "The stoma is 10 days old and mature, so the nurse can reinsert the spare tube."),
    ("Use the smaller spare tube if the first one will not pass", True, "The stoma may narrow quickly."),
    ("Confirm placement by breath sounds, chest rise and end-tidal CO2", True, "Shows the tube is in the trachea."),
    ("Leave the obturator in the tube after insertion", False, "The obturator blocks the airway."),
    ("Wait for the provider before attempting reinsertion", False, "The stoma may close; a mature stoma can be recannulated by the nurse.")],
   "For a dislodged tube in a mature stoma: call for help, reinsert with the obturator, remove the obturator and "
   "confirm placement.",
   "Take action for a dislodged tracheostomy tube.",
   ex=[flow("1300", ["1300"], [
       ("Event", ["Tube out during coughing"]),
       ("SpO2", ["84%"]),
       ("Stoma", ["Open, mature (day 10)"])], id="trach-1300")]),

matrix_mc(M(SIC, HOME, "evaluate_outcomes", "difficult", RESP, "ms fund"),
   "Before discharge, the client's spouse demonstrates and describes home tracheostomy care. For each statement, "
   "indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll keep a spare tube and the obturator by the bed.\"", 0, "Ready for an emergency."),
    ("\"I'll cover the opening with a shower shield when bathing.\"", 0, "Prevents water entering the airway."),
    ("\"We'll use a humidifier at night.\"", 0, "Keeps secretions thin."),
    ("\"They can go swimming once the stoma heals.\"", 1, "Swimming is not safe with a tracheostomy."),
    ("\"I'll put cotton balls around the stoma to catch the mucus.\"", 1, "Loose fibers can be inhaled; use a precut dressing."),
    ("\"If the tube comes out, I'll call 911 and keep the stoma open.\"", 0, "An emergency plan.")],
   "Home safety covers spare equipment, water protection, humidification, avoiding swimming and loose fibers, and "
   "an emergency plan.",
   "Evaluate home tracheostomy teaching.",
   ex=[note("Discharge teaching", "The spouse will be the main caregiver.", id="trach-teach")]),
]

cases = [
    case("ARDS on mechanical ventilation", ards_tabs, ards_qs),
    case("Hemorrhage and an air leak after lobectomy", lob_tabs, lob_qs),
    case("Opioid-induced respiratory depression in sleep apnea", osa_tabs, osa_qs),
    case("Tracheostomy blocked, then dislodged", trach_tabs, trach_qs),
]

BUILT = build(105, "pulmonary-cases-ards-chesttube-osa-trach",
              "Pulmonary case studies: ARDS, complications after lobectomy, opioid-induced respiratory depression and "
              "tracheostomy emergencies",
              items, cases, case_conversion=True)
