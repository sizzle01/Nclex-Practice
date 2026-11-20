"""Batch 38: NGN unfolding case studies III - major burn with inhalation injury; anterior STEMI.

Rewritten to NCLEX-RN difficulty on 20 November 2026 (see quality.py)."""
from qb import *

S = "integumentary"
CV = "cardiovascular"
BURN = "burn-care"
ACS = "acute-coronary-syndrome"

# ---- Case 1: major burn --------------------------------------------------------------------------
burn_tabs = [
    ("burn-notes", "Nurses' notes", [
        note("Emergency department, 0200",
             "A 35-year-old, weight 70 kg, was rescued from a house fire. Partial- and full-thickness burns to the chest, abdomen and both arms, estimated at 40% TBSA. Singed nasal hairs, a hoarse voice and soot-tinged sputum. Blisters on both forearms. Rates pain 8 out of 10.", id="burn-note-0200"),
        note("1000",
             "The voice is more hoarse, and the nurse hears a new high-pitched sound on inspiration.", id="burn-note-1000")]),
    ("burn-vitals", "Vital signs and output", [
        flow("Vital signs and urine output", ["0200", "0600", "1000"], [
            ("Heart rate (/min)", ["124", "130", "110"]),
            ("Blood pressure (mm Hg)", ["102/64", "98/60", "110/66"]),
            ("Respiratory rate (/min)", ["26", "28", "30"]),
            ("SpO2 (%)", ["93", "92", "90"]),
            ("Urine output (mL/hr)", ["-", "18", "40"])], id="burn-flow")]),
    ("burn-labs", "Laboratory results", [
        table("Laboratory results", ["0600"], [
            ("Potassium (mEq/L)", ["5.6"]),
            ("Hematocrit (%)", ["54"]),
            ("Carboxyhemoglobin (%)", ["12"])], id="burn-lab-table")]),
]

burn_items = [
hl_text(M("PHI.PA", BURN, "recognize_cues", "moderate", S, "ms"),
   "Highlight the findings in the 0200 note that suggest an inhalation injury.",
   ["The client has ",
    ("singed nasal hairs", True, "This suggests inhaled heat and smoke."),
    ", ",
    ("a hoarse voice", True, "Hoarseness suggests airway swelling."),
    " and ",
    ("soot-tinged sputum", True, "Soot in the sputum shows smoke inhalation."),
    ". There are ",
    ("blisters on both forearms", False, "These show partial-thickness burns, not an airway injury."),
    ", and pain is rated ",
    ("8 out of 10", False, "Pain needs treatment but isn't a sign of inhalation injury."),
    "."],
   "Facial burns, singed hair, hoarseness and carbonaceous sputum signal inhalation injury.",
   "Recognize inhalation injury."),

calc(M("PHI.PPT", "medication-dose-calculations", "analyze_cues", "difficult", "pharmacology", "ms"),
   "Using the Parkland formula (4 mL × kg × %TBSA), at what rate (mL/hr) should lactated Ringer's run during the first 8 hours for this 70 kg client with 40% TBSA burns? (Assume the 8 hours start at the time of the burn.)",
   "mL/hr", 0, "Infusion rate", 700,
   "Total = 4 × 70 × 40 = 11,200 mL in 24 hours. Half (5,600 mL) in the first 8 hours = 5,600 ÷ 8 = 700 mL/hr.",
   "Calculate a burn resuscitation rate."),

mc(M("PHI.PA", BURN, "prioritize_hypotheses", "difficult", S, "ms"),
   "At 0200, the client is talking in a hoarse voice, the SpO2 is 93% on a nonrebreather mask, the blood pressure is 102/64 mm Hg and fluid resuscitation is starting. Which problem is the priority for this client?",
   [("Risk of airway obstruction from inhalation injury", True, "Airway swelling can progress over hours to complete obstruction; early intubation may be needed."),
    ("Hypovolemia from burn shock and large fluid shifts", False, "Fluids are started, but a closing airway kills faster than burn shock."),
    ("Carbon monoxide poisoning from the house fire", False, "This is treated with 100% oxygen, which is already given; the airway threat is greater."),
    ("Acute pain from partial-thickness burns", False, "Pain matters but comes after the airway and circulation.")],
   "Airway first in burns, especially with signs of inhalation injury.",
   "Prioritize the airway in burns."),

matrix_mc(M("PHI.PA", BURN, "generate_solutions", "moderate", S, "ms"),
   "For each intervention, indicate whether it is indicated or not indicated now.",
   ["Indicated", "Not indicated"],
   [("Lactated Ringer's using the Parkland formula", 0, ["Large fluid shifts need resuscitation.", "Fluids are essential."]),
    ("IV opioid analgesia", 0, ["IV is reliable during burn shock.", "Pain control is needed."]),
    ("Indwelling urinary catheter for hourly output", 0, ["Urine output guides fluid titration.", "This is indicated."]),
    ("Tetanus prophylaxis if not up to date", 0, ["Burns are tetanus-prone wounds.", "This is indicated."]),
    ("IM morphine", 1, ["Absorption is unreliable in burn shock.", "IV is preferred."]),
    ("Restricting fluids to reduce edema", 1, ["This causes burn shock and kidney injury.", "This isn't indicated."])],
   "Burn care: airway, fluids, IV analgesia, monitoring and tetanus prophylaxis.",
   "Select interventions for a major burn."),

mc(M("PHI.PA", BURN, "take_action", "moderate", S, "ms"),
   "At 0600 the client's urine output is 18 mL/hr, the heart rate is 130/min and the hematocrit is 54%. The lactated Ringer's is running at the calculated rate. What should the nurse do?",
   [("Increase the lactated Ringer's rate per the burn protocol", True, "Output below 0.5 mL/kg/hr (35 mL/hr for 70 kg) with a high hematocrit means under-resuscitation."),
    ("Ask the provider for IV furosemide to increase the urine output", False, "A diuretic worsens hypovolemia and hides the true volume status."),
    ("Keep the current rate, since it matches the Parkland formula", False, "The formula is a starting point; fluids are titrated to urine output."),
    ("Flush the urinary catheter to check that it is not blocked", False, "Low output with tachycardia and hemoconcentration points to hypovolemia.")],
   "Titrate burn fluids hourly to a target urine output.",
   "Respond to low urine output in burn resuscitation."),

matrix_mc(M("PHI.PA", BURN, "evaluate_outcomes", "difficult", S, "ms"),
   "Review the 1000 findings. For each, indicate whether it shows improvement or needs immediate follow-up.",
   ["Improvement", "Needs immediate follow-up"],
   [("Urine output 40 mL/hr", 0, ["This now meets the target.", "This is improving."]),
    ("Heart rate 110/min", 0, ["Tachycardia is improving with fluids.", "This is improving."]),
    ("Blood pressure 110/66 mm Hg", 0, ["Perfusion is improving.", "This is improving."]),
    ("More hoarse voice with new stridor and SpO2 90%", 1, ["The airway is worsening.", "Progressive airway swelling needs immediate intubation."])],
   "Even as circulation improves, the airway can deteriorate; reassess often.",
   "Evaluate outcomes in burn resuscitation."),
]

# ---- Case 2: anterior STEMI ----------------------------------------------------------------------
mi_tabs = [
    ("mi-notes", "Nurses' notes", [
        note("Emergency department, 0900",
             "A 62-year-old has had crushing chest pressure radiating to the left arm for 40 minutes, with sweating and nausea. History of hypertension and smoking. No recent use of phosphodiesterase-5 inhibitors. 12-lead ECG: ST elevation in V1 to V4.", id="mi-note-0900"),
        note("After PCI, 1130",
             "A stent was placed in the left anterior descending artery through the right radial artery. The client reports no chest pain. ST segments are returning toward baseline. The radial site is soft without hematoma. New crackles in both lung bases.", id="mi-note-1130")]),
    ("mi-vitals", "Vital signs", [
        flow("Vital signs", ["0900", "1130"], [
            ("Blood pressure (mm Hg)", ["150/92", "122/74"]),
            ("Heart rate (/min)", ["104", "88"]),
            ("SpO2 (%)", ["95", "89"]),
            ("Respiratory rate (/min)", ["22", "24"])], id="mi-flow")]),
    ("mi-labs", "Laboratory results", [
        table("Laboratory results", ["0900"], [
            ("Troponin I", ["Elevated"]),
            ("Glucose", ["110 mg/dL"]),
            ("Potassium", ["4.1 mEq/L"])], id="mi-lab-table")]),
]

mi_items = [
hl_table(M("PHI.PA", ACS, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that support acute myocardial infarction.",
   ["Finding", "Result"],
   [(["Chest pressure", "Radiating to the left arm for 40 minutes"], True, "Ischemic pain is classic for MI."),
    (["Skin", "Sweaty"], True, "Sweating reflects a sympathetic response to ischemia."),
    (["12-lead ECG", "ST elevation V1 to V4"], True, "ST elevation shows myocardial injury."),
    (["Troponin I", "Elevated"], True, "Troponin confirms myocardial injury."),
    (["SpO2 at 0900", "95%"], False, "This is acceptable."),
    (["Glucose", "110 mg/dL"], False, "This isn't specific.")],
   "Symptoms, ECG changes and troponin together confirm a STEMI.",
   "Recognize the findings of an MI."),

cloze(M("PHI.PA", ACS, "analyze_cues", "moderate", CV, "ms"),
   "Complete the sentence.",
   "ST elevation in leads V1 to V4 indicates an {{b1}} STEMI, usually caused by blockage of the {{b2}}.",
   [[("anterior", True, "Leads V1 to V4 look at the anterior wall."),
     ("inferior", False, "Inferior MI shows in II, III and aVF."),
     ("lateral", False, "Lateral MI shows in I, aVL, V5 and V6.")],
    [("left anterior descending artery", True, "The LAD supplies the anterior wall."),
     ("right coronary artery", False, "The RCA usually supplies the inferior wall."),
     ("circumflex artery", False, "The circumflex usually supplies the lateral wall.")]],
   "Anterior STEMI involves the LAD and carries a high risk of pump failure.",
   "Link ECG leads to the affected artery."),

mc(M("PHI.PA", ACS, "prioritize_hypotheses", "moderate", CV, "aha"),
   "The client arrived at 0855. The chest pressure is 7 out of 10 after one nitroglycerin tablet, and the hospital has a cardiac catheterization lab. What is the priority goal of treatment for this client?",
   [("Reperfusion by PCI within 90 minutes of arrival", True, "Time is muscle; rapid reperfusion limits damage to the anterior wall."),
    ("Complete pain relief with IV morphine before transfer", False, "Pain is treated, but reperfusion is not delayed for it."),
    ("Fibrinolytic therapy within 30 minutes of arrival", False, "Fibrinolysis is used when PCI is not available in time; this hospital has a cath lab."),
    ("Blood pressure control to below 120/80 mm Hg", False, "Blood pressure is managed, but the blocked artery is the priority.")],
   "STEMI: door-to-balloon time of 90 minutes or less, or fibrinolysis within 30 minutes if PCI isn't available.",
   "Prioritize reperfusion in STEMI."),

sata(M("PHI.PA", ACS, "generate_solutions", "moderate", CV, "aha ph"),
   "Which interventions should the nurse anticipate before the client goes to the cardiac catheterization lab?",
   [("Aspirin 162 to 325 mg chewed", True, "Aspirin reduces platelet aggregation."),
    ("Sublingual nitroglycerin if blood pressure allows", True, "This relieves ischemic pain; there's no PDE-5 inhibitor use."),
    ("Continuous ECG monitoring", True, "Dysrhythmias are common early in MI."),
    ("IV access", True, "This is needed for medications and the procedure."),
    ("Oxygen at 10 L/min by nonrebreather for an SpO2 of 95%", False, "Oxygen is given only when SpO2 is below about 90%."),
    ("A treadmill stress test", False, "This is contraindicated in acute MI.")],
   "Pre-PCI: aspirin, antiplatelet and anticoagulant per protocol, nitrates if appropriate, monitoring and rapid transfer.",
   "Plan initial STEMI care."),

mc(M("PHI.PA", "cardiac-dysrhythmias", "take_action", "difficult", CV, "aha"),
   "While waiting for transfer, the client becomes unresponsive, with no pulse, and the monitor shows ventricular fibrillation. What should the nurse do first?",
   [("Start CPR and defibrillate as soon as possible", True, "Early defibrillation is the most important treatment for VF."),
    ("Prepare for synchronized cardioversion at 100 joules", False, "Pulseless VF needs unsynchronized defibrillation."),
    ("Give amiodarone 300 mg IV push right away", False, "Amiodarone is given after shocks per ACLS, not before the first shock."),
    ("Obtain a 12-lead ECG to confirm the rhythm", False, "This delays lifesaving treatment.")],
   "Pulseless VF or VT: CPR and immediate defibrillation, then epinephrine and amiodarone per ACLS.",
   "Respond to ventricular fibrillation."),

matrix_mc(M("PHI.PA", ACS, "evaluate_outcomes", "difficult", CV, "ms"),
   "Review the findings after PCI. For each, indicate whether it shows an expected or improving response or needs follow-up.",
   ["Expected or improving", "Needs follow-up"],
   [("No chest pain", 0, ["Reperfusion relieved the ischemia.", "This is improving."]),
    ("ST segments returning toward baseline", 0, ["This shows successful reperfusion.", "This is expected."]),
    ("Radial site soft without hematoma", 0, ["This shows no bleeding at the access site.", "This is expected."]),
    ("New crackles in both lung bases with SpO2 89%", 1, ["This isn't improvement.", "This suggests heart failure after an anterior MI; notify the provider."])],
   "After an anterior MI, watch for heart failure, dysrhythmias and bleeding.",
   "Evaluate outcomes after PCI."),
]

BUILT = build(38, "ngn-cases-3", "NGN case studies III: major burn and anterior STEMI", [], hardened=True, cases=[
    case("Major burn with inhalation injury", burn_tabs, burn_items),
    case("Anterior STEMI", mi_tabs, mi_items),
])
