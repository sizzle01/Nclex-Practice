"""Batch 122 (case conversion, chunk 6: renal, hematologic, oncologic): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: febrile neutropenia after chemotherapy with filgrastim; tumor lysis syndrome at the start of leukemia
treatment; disseminated intravascular coagulation from sepsis with blood products; massive pulmonary embolism treated
with alteplase (thrombolytics). Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6,
and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
HEME = "hematologic-oncologic"
ONC_EM = "oncologic-emergencies"
CANCER = "cancer-care"
CHEMO = "chemotherapy-and-radiation-care"
NEUTRO = "neutropenic-precautions"
HEME_MEDS = "hematologic-medications"
BLEED = "bleeding-and-clotting-disorders"
TRANSFUSION = "blood-transfusion-reactions"
SEPSIS = "shock-and-sepsis"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
VTE = "venous-thromboembolism"
ANTICOAG = "anticoagulant-therapy-and-monitoring"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
CALC = "medication-dose-calculations"

items = []

# ================================================================================================
# Case 1: Febrile neutropenia after chemotherapy
# ================================================================================================
fn_tabs = [
    ("fn-notes", "Emergency department notes", [note("Triage",
        "A 51-year-old client with breast cancer had the second cycle of docetaxel and cyclophosphamide 9 days ago. "
        "The client feels shaky and tired and has a sore mouth. An implanted port is in the right chest. The client "
        "took acetaminophen 2 hours ago for chills.", id="fn-note")]),
    ("fn-vitals", "Vital signs and labs", [table("Findings", ["Result"], [
        ("Temperature", ["38.4 C (101.1 F) despite acetaminophen"]),
        ("Heart rate", ["116/min"]),
        ("Blood pressure", ["98/60 mm Hg (usual 128/78)"]),
        ("Respiratory rate", ["22/min"]),
        ("WBC", ["0.9 x 10^3/mm3"]),
        ("Neutrophils", ["12%"]),
        ("Platelets", ["88,000/mm3"]),
        ("Lactate", ["2.6 mmol/L"]),
        ("Mouth", ["White patches and two shallow ulcers"]),
        ("Port site", ["Faint redness 1 cm around the needle site"])], id="fn-table")]),
]

fn_qs = [
hl_table(M(PA, ONC_EM, "recognize_cues", "moderate", HEME, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "Result"],
   [(["Temperature", "38.4 C despite acetaminophen"], True, "Fever in neutropenia is an emergency; acetaminophen may mask a higher fever."),
    (["Heart rate and blood pressure", "116/min, 98/60 mm Hg"], True, "Early sepsis."),
    (["Lactate", "2.6 mmol/L"], True, "Hypoperfusion."),
    (["Port site", "Faint redness"], True, "Neutropenic clients show little inflammation; faint redness can be a serious infection."),
    (["Platelets", "88,000/mm3"], False, "Low but not the immediate threat.")],
   "Fever, tachycardia, low blood pressure and raised lactate in a neutropenic client is sepsis until proven otherwise.",
   "Recognize cues of febrile neutropenia."),

calc(M(RRP, LABS, "analyze_cues", "difficult", HEME, "lab ms"),
   "Absolute neutrophil count (ANC) = WBC x percentage of neutrophils (segs + bands). Using the results, calculate "
   "the ANC. Record your answer as cells/mm3, as a whole number.",
   "cells/mm3", 0, "ANC", 108,
   "0.9 x 10^3 = 900 cells/mm3; 900 x 0.12 = 108 cells/mm3. An ANC below 500 is severe neutropenia; below 100 is "
   "profound.",
   "Calculate the absolute neutrophil count."),

cloze(M(PA, ONC_EM, "prioritize_hypotheses", "difficult", HEME, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client is most at risk for {{b1}}. The most important goal in the first hour is {{b2}}.",
   [[("septic shock from neutropenic infection", True, "ANC 108 with early sepsis signs."),
     ("bleeding from thrombocytopenia", False, "Platelets 88,000 are not the immediate threat."),
     ("an allergic reaction to chemotherapy", False, "Nine days after the dose."),
     ("tumor lysis syndrome", False, "Not suggested.")],
    [("blood cultures and broad-spectrum IV antibiotics within 60 minutes", True, "Delay increases death in febrile neutropenia."),
     ("waiting for the culture results before antibiotics", False, "Too late."),
     ("giving filgrastim before anything else", False, "It takes days to work."),
     ("a chest x-ray before any treatment", False, "Must not delay antibiotics.")]],
   "Antibiotics within the hour are the priority in febrile neutropenia.",
   "Prioritize febrile neutropenia."),

grouping(M(PA, ONC_EM, "generate_solutions", "difficult", HEME, "ms"),
   "For each area, select the actions the plan should include.",
   [("Cultures", [
       ("Blood cultures from the port and a peripheral vein", True, "Identifies a line infection."),
       ("Urine culture", True, "Look for a source even without symptoms."),
       ("Delay cultures until the fever reaches 39 C", False, "Fever at 38.3 is enough.")]),
    ("Treatment", [
       ("Cefepime or piperacillin-tazobactam IV now", True, "Antipseudomonal coverage."),
       ("30 mL/kg crystalloid for hypotension and lactate", True, "Sepsis bundle."),
       ("A rectal temperature to confirm the fever", False, "Rectal routes are avoided in neutropenia.")]),
    ("Protection", [
       ("Private room and strict hand hygiene", True, "Neutropenic precautions."),
       ("Fresh flowers and fruit at the bedside", False, "Sources of organisms.")])],
   "Culture from line and periphery, antibiotics and fluids now, and protect from new infection.",
   "Generate the febrile neutropenia plan.",
   ex=[note("Orders", "Cultures x2. Cefepime 2 g IV. Lactated Ringer's 30 mL/kg. Neutropenic precautions. Filgrastim "
       "daily.", id="fn-orders")]),

sata(M(MOC, DELEG, "take_action", "difficult", HEME, "mgmt ms"),
   "The client is admitted to the oncology unit. Which tasks can the RN delegate to the UAP?",
   [("Recording vital signs every 2 hours", True, "Routine measurement."),
    ("Helping with oral care using a soft brush", True, "Routine hygiene."),
    ("Reporting a temperature above 38 C at once", True, "Reporting."),
    ("Assessing the port site for spreading redness and pain", False, "Assessment stays with the RN."),
    ("Teaching the family about neutropenic precautions at home", False, "Teaching is an RN role."),
    ("Giving the first filgrastim injection under the RN's watch", False, "Medication administration cannot go to UAPs.")],
   "UAPs measure, help with hygiene and report; assessing, teaching and medicines stay with licensed staff.",
   "Delegate care in neutropenia.",
   ex=[note("Day 1", "Blood pressure 118/70 after fluids. Lactate 1.4. Port culture pending.", id="fn-day1")]),

matrix_mc(M(PPT, HEME_MEDS, "evaluate_outcomes", "difficult", HEME, "ph ms"),
   "On day 4 of filgrastim, for each finding, indicate whether it shows the expected response or needs follow-up.",
   ["Expected response", "Needs follow-up"],
   [("ANC 1,400 cells/mm3", 0, "Recovery."),
    ("Afebrile for 48 hours", 0, "Infection controlled."),
    ("Aching in the lower back and hips", 0, "Bone pain is common as the marrow expands."),
    ("Left upper abdominal pain spreading to the shoulder", 1, "Possible splenic rupture, a rare filgrastim effect."),
    ("New shortness of breath with crackles", 1, "Possible ARDS, a rare filgrastim effect."),
    ("Port culture: no growth", 0, "No line infection.")],
   "Neutrophils recovered; left upper quadrant pain or breathlessness on filgrastim must be reported.",
   "Evaluate the response to filgrastim.",
   ex=[note("Day 4", "WBC 3.8. Mouth ulcers healing.", id="fn-day4")]),
]

# ================================================================================================
# Case 2: Tumor lysis syndrome
# ================================================================================================
tls_tabs = [
    ("tls-notes", "Hematology unit notes", [note("Admission",
        "A 34-year-old client with newly diagnosed Burkitt lymphoma and a WBC of 96,000 started induction "
        "chemotherapy 18 hours ago. Allopurinol was started yesterday. IV fluids run at 150 mL/hr.", id="tls-note")]),
    ("tls-labs", "Labs and output", [flow("Labs", ["Before chemo", "18 hours"], [
        ("Potassium", ["4.6", "6.4 mEq/L"]),
        ("Phosphorus", ["4.2", "7.8 mg/dL"]),
        ("Calcium", ["9.2", "6.9 mg/dL"]),
        ("Uric acid", ["8.1", "13.6 mg/dL"]),
        ("Creatinine", ["0.9", "2.1 mg/dL"]),
        ("Urine output (last 4 hours)", ["-", "80 mL"]),
        ("Client", ["Comfortable", "Muscle cramps, tingling around the mouth"])], id="tls-flow")]),
]

tls_qs = [
hl_table(M(RRP, LABS, "recognize_cues", "moderate", HEME, "lab ms"),
   "Highlight the 18-hour findings that require immediate action.",
   ["Finding", "18 hours"],
   [(["Potassium", "6.4 mEq/L"], True, "Risk of fatal dysrhythmias."),
    (["Phosphorus", "7.8 mg/dL"], True, "Binds calcium."),
    (["Calcium", "6.9 mg/dL"], True, "Tetany and seizures."),
    (["Creatinine and output", "2.1 mg/dL, 80 mL in 4 hours"], True, "Acute kidney injury."),
    (["Uric acid", "13.6 mg/dL"], True, "Crystals block the tubules.")],
   "Rapid cell death releases potassium, phosphate and uric acid, causing kidney injury and low calcium.",
   "Recognize cues of tumor lysis syndrome."),

matrix_mc(M(PA, ONC_EM, "analyze_cues", "difficult", HEME, "ms"),
   "For each finding, indicate its main cause in tumor lysis syndrome.",
   ["Potassium released from cells", "Phosphate binding calcium", "Uric acid crystals in the kidneys"],
   [("Peaked T waves", 0, "Hyperkalemia."),
    ("Tingling around the mouth and cramps", 1, "Hypocalcemia."),
    ("Rising creatinine with low output", 2, "Crystal nephropathy."),
    ("Positive Chvostek sign", 1, "Hypocalcemia."),
    ("Muscle weakness", 0, "Hyperkalemia.")],
   "The electrolyte pattern explains the symptoms.",
   "Analyze tumor lysis syndrome."),

matrix_mc(M(PA, ONC_EM, "prioritize_hypotheses", "difficult", HEME, "ms"),
   "For each problem, indicate its priority now.",
   ["Treat first", "Treat soon", "Monitor"],
   [("Potassium 6.4 mEq/L with ECG changes", 0, "Cardiac arrest risk."),
    ("Falling urine output", 1, "Needs fluids and possibly dialysis."),
    ("Uric acid 13.6 mg/dL", 1, "Rasburicase lowers it quickly."),
    ("Mild nausea", 2, "Symptomatic only.")],
   "Hyperkalemia with ECG changes is treated first.",
   "Prioritize tumor lysis problems.",
   ex=[note("ECG", "Peaked T waves. Rate 92/min.", id="tls-ecg")]),

sata(M(PPT, CHEMO, "generate_solutions", "difficult", HEME, "ph ms"),
   "Which orders should the nurse anticipate?",
   [("Calcium gluconate IV", True, "Protects the heart."),
    ("Regular insulin IV with dextrose", True, "Shifts potassium."),
    ("Rasburicase IV", True, "Breaks down uric acid."),
    ("A nephrology consult for dialysis", True, "For worsening kidney injury."),
    ("Potassium-containing IV fluids such as lactated Ringer's", False, "Adds potassium in hyperkalemia."),
    ("Calcium replacement to fix the low calcium in a client without symptoms", False, "Can cause calcium-phosphate crystals.")],
   "Stabilize the heart, shift potassium, lower uric acid and plan for dialysis.",
   "Generate the tumor lysis plan.",
   ex=[note("History", "G6PD screen: normal.", id="tls-g6pd")]),

order(M(PA, ONC_EM, "take_action", "difficult", HEME, "ms"),
   "Place the nurse's actions in order of priority.",
   ["Place the client on a cardiac monitor",
    "Give calcium gluconate IV as prescribed",
    "Give regular insulin with dextrose IV as prescribed",
    "Check the blood glucose 1 hour after insulin",
    "Recheck the potassium level as ordered"],
   "Monitor, stabilize the heart, shift potassium, check glucose, then recheck the potassium.",
   "Take action for hyperkalemia in TLS.",
   ex=[note("New", "Prescriptions received.", id="tls-rx")]),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "difficult", HEME, "lab ms"),
   "At 36 hours, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Uric acid 3.2 mg/dL", 0, "Rasburicase worked."),
    ("Potassium 4.9 mEq/L", 0, "Back in range."),
    ("Urine output 90 mL/hr", 0, "Kidneys recovering."),
    ("Calcium 6.8 mg/dL with carpopedal spasm", 1, "Symptomatic hypocalcemia now needs replacement."),
    ("Creatinine 1.6 mg/dL", 0, "Improving."),
    ("Phosphorus 6.9 mg/dL", 1, "Still high.")],
   "Uric acid, potassium and output improved; symptomatic low calcium and high phosphate need treatment.",
   "Evaluate the TLS response.",
   ex=[note("36 hours", "Rasburicase given. Insulin and dextrose x2.", id="tls-36h")]),
]

# ================================================================================================
# Case 3: DIC from sepsis
# ================================================================================================
dic_tabs = [
    ("dic-notes", "ICU notes", [note("Intensive care",
        "A 66-year-old client with urosepsis is on norepinephrine. The nurse notes oozing from the IV sites and the "
        "arterial line, blood in the urine and new purple patches on the arms.", id="dic-note")]),
    ("dic-labs", "Coagulation labs", [table("Labs", ["Today", "Yesterday"], [
        ("Platelets", ["38,000/mm3", "142,000/mm3"]),
        ("PT/INR", ["22 seconds / 2.1", "13 / 1.1"]),
        ("aPTT", ["58 seconds", "32"]),
        ("Fibrinogen", ["88 mg/dL", "310"]),
        ("D-dimer", ["Very high", "Mild rise"]),
        ("Hemoglobin", ["7.8 g/dL", "10.4"])], id="dic-table")]),
]

dic_qs = [
hl_text(M(PA, BLEED, "recognize_cues", "moderate", HEME, "ms"),
   "Highlight the findings that suggest a coagulation emergency.",
   ["The client ",
    ("oozes from the IV and arterial line sites", True, "Bleeding from many sites."),
    ", has ",
    ("blood in the urine", True, "Bleeding."),
    " and has ",
    ("new purple patches on the arms", True, "Purpura."),
    ". Platelets ",
    ("fell from 142,000 to 38,000", True, "Consumption."),
    ", ",
    ("fibrinogen is 88 mg/dL", True, "Consumption of fibrinogen."),
    " and the ",
    ("D-dimer is very high", True, "Clot breakdown."),
    ". The client ",
    ("is alert enough to follow commands", False, "Not a coagulation finding."),
    "."],
   "Bleeding from many sites with falling platelets and fibrinogen and a high D-dimer is DIC.",
   "Recognize cues of DIC."),

cloze(M(PA, BLEED, "analyze_cues", "difficult", HEME, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "In DIC, {{b1}} uses up {{b2}}, so the client {{b3}}.",
   [[("widespread clotting triggered by sepsis", True, "Microclots form everywhere."),
     ("a lack of vitamin K", False, "Does not explain the pattern."),
     ("too much heparin", False, "Not on heparin."),
     ("bone marrow failure", False, "Consumption, not production.")],
    [("platelets and clotting factors", True, "Consumption coagulopathy."),
     ("red cells only", False, "Not the main problem."),
     ("albumin", False, "Not the mechanism."),
     ("white cells", False, "Not the mechanism.")],
    [("both clots and bleeds", True, "Microclots damage organs while bleeding occurs."),
     ("only clots", False, "Bleeding is present."),
     ("only bleeds from the IV sites", False, "Bleeding is widespread."),
     ("is protected from clots", False, "Microclots damage organs.")]],
   "Clotting consumes factors, causing both organ-damaging clots and bleeding.",
   "Analyze DIC."),

cloze(M(PA, BLEED, "prioritize_hypotheses", "difficult", HEME, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most important treatment of DIC is {{b1}}, while blood products are given to {{b2}}.",
   [[("treating the underlying sepsis", True, "Removes the trigger."),
     ("giving heparin to all clients", False, "Only in selected cases."),
     ("giving vitamin K alone", False, "Not enough."),
     ("stopping all IV fluids", False, "Worsens shock.")],
    [("treat active bleeding or prepare for procedures", True, "Replace what was consumed."),
     ("reach normal lab values in all clients", False, "Products are guided by bleeding."),
     ("prevent sepsis", False, "Not their role."),
     ("replace the norepinephrine", False, "Not their role.")]],
   "Treat the cause; replace products for bleeding.",
   "Prioritize DIC treatment."),

matrix_mc(M(PPT, TRANSFUSION, "generate_solutions", "difficult", HEME, "ph ms"),
   "For each blood product, indicate the problem it treats.",
   ["Low platelets", "Low fibrinogen", "Low clotting factors (high INR)", "Low hemoglobin"],
   [("Platelet concentrate", 0, "Platelets 38,000 with bleeding."),
    ("Cryoprecipitate", 1, "Rich in fibrinogen."),
    ("Fresh frozen plasma", 2, "Replaces clotting factors."),
    ("Packed red blood cells", 3, "Hemoglobin 7.8 with bleeding.")],
   "Each product replaces a consumed part of the clotting system.",
   "Generate a plan for blood products.",
   ex=[note("Orders", "1 unit apheresis platelets, 10 units cryoprecipitate, 2 units FFP, 2 units PRBCs.", id="dic-orders")]),

sata(M(SIC, "client-identification-and-error-prevention", "take_action", "difficult", HEME, "ph ms"),
   "15 minutes into the first unit of red cells, review the findings. Which actions should the nurse take?",
   [("Stop the transfusion", True, "Suspected reaction."),
    ("Keep the IV open with 0.9% sodium chloride using new tubing", True, "Keeps access without more blood."),
    ("Notify the provider and blood bank", True, "Required for a reaction."),
    ("Send the bag, tubing and a blood sample to the blood bank", True, "Investigation."),
    ("Slow the transfusion and give diphenhydramine before continuing", False, "Fever and back pain are not a mild allergic reaction."),
    ("Flush the remaining blood in the tubing into the client", False, "Gives more of the incompatible blood.")],
   "Fever, back pain and hypotension early in a transfusion is a hemolytic reaction: stop and investigate.",
   "Take action for a transfusion reaction.",
   ex=[flow("15 minutes", ["Before", "15 minutes"], [
       ("Temperature", ["37.6 C", "39.1 C"]),
       ("Blood pressure", ["104/62", "82/48 mm Hg"]),
       ("Client", ["Sedated", "Restless, grimacing, flank pain"]),
       ("Urine", ["Pink", "Dark red-brown"])], id="dic-15min")]),

matrix_mc(M(PA, BLEED, "evaluate_outcomes", "difficult", HEME, "ms"),
   "48 hours later, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Fibrinogen 180 mg/dL", 0, "Rising."),
    ("Platelets 74,000/mm3", 0, "Rising."),
    ("No oozing from line sites", 0, "Bleeding controlled."),
    ("Cold, dusky fingertips on both hands", 1, "Microclots or vasopressor effect; needs assessment."),
    ("Norepinephrine weaned off", 0, "Sepsis improving."),
    ("Urine output 15 mL/hr", 1, "Kidney injury.")],
   "Clotting labs and bleeding improved; limb ischemia and low output need follow-up.",
   "Evaluate DIC treatment.",
   ex=[note("48 hours", "Blood cultures: E. coli, sensitive to the antibiotic given.", id="dic-48h")]),
]

# ================================================================================================
# Case 4: Massive pulmonary embolism and alteplase
# ================================================================================================
pe_tabs = [
    ("pe-notes", "Nurses' notes", [note("Orthopedic unit, day 3 after hip replacement",
        "A 70-year-old client (weight 80 kg) suddenly becomes short of breath and anxious while walking to the "
        "bathroom, and says, \"I feel like I'm going to die.\" The client missed enoxaparin doses yesterday while "
        "nauseated.", id="pe-note")]),
    ("pe-vitals", "Vital signs", [flow("Vital signs", ["0800", "1015"], [
        ("Heart rate", ["84", "128/min"]),
        ("Blood pressure", ["136/80", "84/52 mm Hg"]),
        ("Respiratory rate", ["16", "32/min"]),
        ("SpO2", ["96% on room air", "84% on 4 L/min"]),
        ("Neck veins", ["Flat", "Distended"]),
        ("Left calf", ["Normal", "Swollen and tender"])], id="pe-flow")]),
]

pe_qs = [
hl_table(M(PA, VTE, "recognize_cues", "moderate", HEME, "ms"),
   "Highlight the findings at 1015 that require immediate action.",
   ["Finding", "1015"],
   [(["Heart rate", "128/min"], True, "Compensatory."),
    (["Blood pressure", "84/52 mm Hg"], True, "Obstructive shock."),
    (["SpO2", "84% on 4 L/min"], True, "Severe hypoxemia."),
    (["Neck veins", "Distended"], True, "Right heart strain."),
    (["Left calf", "Swollen and tender"], True, "Source of the clot.")],
   "Sudden breathlessness, hypotension and distended neck veins after surgery suggest a massive PE.",
   "Recognize cues of massive PE."),

matrix_mc(M(PA, VTE, "analyze_cues", "difficult", HEME, "ms"),
   "For each finding, indicate whether it supports a massive (high-risk) PE or does not support it.",
   ["Supports massive PE", "Supports neither"],
   [("Systolic pressure below 90 mm Hg", 0, "Defines massive (high-risk) PE."),
    ("Distended neck veins", 0, "Right ventricular failure."),
    ("Severe hypoxemia", 0, "Large perfusion defect."),
    ("Bibasilar crackles with a third heart sound", 1, "More typical of left heart failure."),
    ("Recent surgery and missed anticoagulant", 0, "Risk factors.")],
   "Hypotension separates massive PE from smaller emboli.",
   "Analyze PE severity."),

cloze(M(PPT, ANTICOAG, "prioritize_hypotheses", "difficult", HEME, "ph ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Because the client has {{b1}}, the team considers {{b2}}, but the recent hip surgery is {{b3}}.",
   [[("a PE with shock", True, "High-risk PE."),
     ("a small PE", False, "Anticoagulation alone is used then."),
     ("a calf clot only", False, "Shock is present."),
     ("pneumonia", False, "Not the picture.")],
    [("thrombolytic therapy or catheter-directed clot removal", True, "Restores pulmonary blood flow quickly."),
     ("aspirin alone", False, "Inadequate."),
     ("compression stockings", False, "Not for a PE."),
     ("waiting for a lung scan tomorrow", False, "Too slow.")],
    [("a relative contraindication that raises bleeding risk", True, "Surgery within 3 weeks raises bleeding risk."),
     ("not relevant", False, "It is relevant."),
     ("an absolute reason never to treat", False, "The benefit may outweigh the risk in shock."),
     ("a reason to give a double dose", False, "Dangerous.")]],
   "Massive PE with shock may need thrombolysis even with relative contraindications.",
   "Prioritize massive PE treatment."),

calc(M(PPT, CALC, "generate_solutions", "difficult", HEME, "ph"),
   "After the team decides on systemic thrombolysis, alteplase 100 mg IV over 2 hours is prescribed. The pharmacy "
   "supplies 100 mg in 100 mL. At what rate should the nurse set the pump? Record your answer in mL/hr.",
   "mL/hr", 0, "Rate", 50,
   "100 mL over 2 hours = 50 mL/hr.",
   "Calculate the alteplase infusion rate.",
   ex=[note("Bedside echo", "Right ventricle dilated with poor function. CT: saddle embolus.", id="pe-echo")]),

sata(M(PPT, ANTICOAG, "take_action", "difficult", HEME, "ph ms"),
   "During the alteplase infusion, which actions should the nurse take?",
   [("Check neurologic status and vital signs often", True, "Detects intracranial bleeding."),
    ("Watch the surgical site for bleeding", True, "Recent surgery."),
    ("Avoid new arterial punctures and IM injections", True, "Bleeding risk."),
    ("Insert a new nasogastric tube for comfort", False, "Invasive procedures cause bleeding during thrombolysis."),
    ("Give enoxaparin at the same time to make up for the missed doses", False, "Anticoagulation is held or adjusted during thrombolysis.")],
   "Monitoring for bleeding and avoiding punctures are key during thrombolysis.",
   "Take action during thrombolytic therapy.",
   ex=[note("1045", "Alteplase started. Heparin held during infusion.", id="pe-1045")]),

matrix_mc(M(PA, VTE, "evaluate_outcomes", "difficult", HEME, "ms ph"),
   "At 1300, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Blood pressure 118/70 mm Hg", 0, "Shock resolved."),
    ("SpO2 94% on 2 L/min", 0, "Oxygenation improved."),
    ("Heart rate 96/min", 0, "Improving."),
    ("New severe headache and vomiting", 1, "Possible intracranial bleeding; stop any thrombolytic and notify."),
    ("Oozing at the hip incision", 1, "Bleeding; apply pressure and notify."),
    ("Neck veins flat", 0, "Right heart strain easing.")],
   "Thrombolysis restored flow; headache or surgical bleeding must be acted on.",
   "Evaluate the response to thrombolysis.",
   ex=[note("1300", "Infusion completed. Heparin restarted as prescribed.", id="pe-1300")]),
]

cases = [
    case("Febrile neutropenia after chemotherapy", fn_tabs, fn_qs),
    case("Tumor lysis syndrome", tls_tabs, tls_qs),
    case("Disseminated intravascular coagulation from sepsis", dic_tabs, dic_qs),
    case("Massive pulmonary embolism treated with alteplase", pe_tabs, pe_qs),
]

BUILT = build(122, "heme-onc-cases-neutropenia-tls-dic-pe",
              "Hematologic and oncologic case studies: febrile neutropenia, tumor lysis, DIC and massive PE",
              items, cases, case_conversion=True)
