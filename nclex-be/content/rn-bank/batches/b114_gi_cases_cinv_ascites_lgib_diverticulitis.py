"""Batch 114 (case conversion, chunk 4: gastrointestinal): four full NGN case studies (CASE-MAP.md).

Cases: chemotherapy-induced nausea and vomiting managed with antiemetics; cirrhosis with ascites, paracentesis and
spontaneous bacterial peritonitis; a lower GI bleed in a client taking apixaban; acute diverticulitis with an
abscess. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a
management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
GI = "gastrointestinal"
GI_MEDS = "gi-medications"
CHEMO = "chemotherapy-and-radiation-care"
LYTES = "electrolyte-imbalances"
LIVER = "liver-disorders"
GI_DX = "gi-diagnostic-procedures"
UPPER = "upper-gi-disorders-and-bleeding"
ANTIDOTE = "antidotes-and-reversal-agents"
BOWEL = "bowel-disorders"
NUTR = "nutrition-and-therapeutic-diets"
TEACH = "health-teaching-and-behavior-change"
PRIORITY = "prioritizing-client-care"
ADVERSE = "adverse-effects-of-medications"
TRANSFUSION = "blood-transfusion-reactions"
DISCHARGE = "discharge-planning-and-continuity"
CALC = "medication-dose-calculations"

items = []

# ================================================================================================
# Case 1: Chemotherapy-induced nausea and vomiting
# ================================================================================================
cinv_tabs = [
    ("cinv-notes", "Nurses' notes", [note("Oncology unit, day 2 after cisplatin",
        "A 52-year-old client received cisplatin for head and neck cancer yesterday. Antiemetics: ondansetron 8 mg "
        "every 8 hours, dexamethasone and aprepitant on day 1. Since this morning the client has vomited 7 times and "
        "cannot keep fluids down. The client also takes citalopram and was given metoclopramide 10 mg IV twice "
        "overnight. The client now has restlessness and cannot sit still.", id="cinv-note")]),
    ("cinv-data", "Vital signs and labs", [table("Vital signs and labs", ["Day 2", "Reference"], [
        ("Heart rate", ["112/min", "-"]),
        ("Blood pressure", ["102/64 mm Hg lying, 84/52 standing", "-"]),
        ("Potassium", ["2.9 mEq/L", "3.5 to 5.0"]),
        ("Magnesium", ["1.2 mg/dL", "1.5 to 2.5"]),
        ("Creatinine", ["1.8 mg/dL (0.9 before)", "-"]),
        ("QTc", ["512 ms", "below 470"])], id="cinv-table")]),
]

cinv_qs = [
hl_table(M(PPT, CHEMO, "recognize_cues", "moderate", GI, "ph"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Vomiting", "7 times; no fluids kept down"], True, "Uncontrolled delayed emesis."),
    (["Orthostatic BP", "Falls 18 mm Hg"], True, "Volume depletion."),
    (["Potassium / magnesium", "2.9 / 1.2"], True, "Losses plus cisplatin wasting."),
    (["Creatinine", "1.8 mg/dL"], True, "Cisplatin nephrotoxicity worsened by dehydration."),
    (["QTc", "512 ms"], True, "Prolonged by ondansetron, citalopram and low K and Mg."),
    (["Restlessness, cannot sit still", "After metoclopramide"], True, "Akathisia, an extrapyramidal effect.")],
   "Delayed vomiting after cisplatin has caused dehydration, electrolyte loss, kidney injury, QT prolongation and "
   "akathisia.",
   "Recognize cues of chemotherapy-induced nausea and vomiting complications."),

matrix_mr(M(PPT, GI_MEDS, "analyze_cues", "difficult", GI, "ph"),
   "For each antiemetic, indicate the adverse effects the nurse should watch for. Each row may have more than one "
   "answer.",
   ["QT prolongation", "Extrapyramidal effects", "Hyperglycemia", "Constipation"],
   [("Ondansetron", {0, 3}, "A serotonin antagonist: QT and constipation."),
    ("Metoclopramide", {1}, "A dopamine antagonist: akathisia, dystonia, tardive dyskinesia."),
    ("Dexamethasone", {2}, "A steroid raises glucose."),
    ("Prochlorperazine", {0, 1}, "A phenothiazine: EPS and QT.")],
   "Antiemetics have predictable effects: serotonin antagonists and phenothiazines prolong QT; dopamine "
   "antagonists cause EPS; steroids raise glucose.",
   "Analyze antiemetic adverse effects."),

cloze(M(PPT, CHEMO, "prioritize_hypotheses", "difficult", GI, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The most dangerous problem now is {{b1}}, because {{b2}}.",
   [[("a risk of torsades de pointes", True, "A QTc above 500 ms with low potassium and magnesium."),
     ("akathisia", False, "Distressing but not life-threatening."),
     ("hair loss", False, "Not urgent."),
     ("mouth sores", False, "Not mentioned.")],
    [("several QT-prolonging drugs combine with low potassium and magnesium", True, "Each adds risk."),
     ("cisplatin causes akathisia", False, "Metoclopramide does."),
     ("the heart rate is 112", False, "Tachycardia alone does not cause torsades."),
     ("the client is 52", False, "Not the reason.")]],
   "Combined QT risks make torsades the most dangerous problem.",
   "Prioritize risks in CINV."),

matrix_mc(M(PPT, GI_MEDS, "generate_solutions", "difficult", GI, "ph"),
   "For each order, indicate whether it is indicated or contraindicated now.",
   ["Indicated", "Contraindicated"],
   [("IV potassium and magnesium replacement", 0, "Lowers QT risk and replaces losses."),
    ("IV isotonic fluids", 0, "Treats dehydration and protects the kidneys."),
    ("Diphenhydramine IV for akathisia", 0, "Treats extrapyramidal symptoms."),
    ("A further dose of metoclopramide", 1, "Worsens akathisia."),
    ("Olanzapine at bedtime as prescribed for breakthrough nausea", 0, "Effective for delayed CINV; QT is monitored."),
    ("Ondansetron 16 mg IV now for faster relief", 1, "Higher IV doses prolong QT further.")],
   "Replace fluid and electrolytes, treat akathisia, avoid more metoclopramide and high-dose ondansetron.",
   "Generate solutions for CINV complications.",
   ex=[note("Oncology orders", "Hold citalopram today. Cardiac monitoring. Recheck electrolytes in 6 hours.",
       id="cinv-orders")]),

calc(M(PPT, CALC, "take_action", "difficult", GI, "ph"),
   "Potassium chloride 40 mEq in 250 mL 0.9% sodium chloride is prescribed to run at 10 mEq/hour through a "
   "peripheral line. At what rate should the nurse set the pump? Record your answer as a whole number.",
   "mL/hour", 0, "Rate", 63,
   "40 mEq / 250 mL = 0.16 mEq/mL. 10 mEq/hour / 0.16 mEq/mL = 62.5 mL/hour, about 63 mL/hour, over 4 hours. "
   "Potassium is never given by IV push.",
   "Calculate a potassium infusion rate.",
   ex=[note("Order", "Potassium chloride 40 mEq in 250 mL 0.9% sodium chloride at 10 mEq/hour. Cardiac monitor.",
       id="cinv-k")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", GI, "ph"),
   "Before the next cycle, the client describes the plan for nausea. For each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take the antiemetics on schedule for 3 days, even if I feel fine.\"", 0, "Prevents delayed emesis."),
    ("\"I'll eat small, bland meals and avoid strong smells.\"", 0, "Reduces nausea."),
    ("\"I'll call if I can't keep fluids down for 24 hours.\"", 0, "Dehydration warning."),
    ("\"I'll wait until I'm sick before taking anything.\"", 1, "Prevention works better than rescue."),
    ("\"If my legs feel restless, I'll take extra metoclopramide.\"", 1, "Restlessness is a side effect to report."),
    ("\"I'll drink 2 to 3 liters a day to protect my kidneys.\"", 0, "Supports kidney function.")],
   "Scheduled prevention, diet changes and early reporting prevent delayed CINV complications.",
   "Evaluate CINV teaching.",
   ex=[note("Day 4", "Vomiting stopped. QTc 452 ms. Creatinine 1.1 mg/dL.", id="cinv-day4")]),
]

# ================================================================================================
# Case 2: Ascites, paracentesis and spontaneous bacterial peritonitis
# ================================================================================================
asc_tabs = [
    ("asc-notes", "Nurses' notes", [note("Medical unit",
        "A 57-year-old client with cirrhosis has a tense, distended abdomen and shortness of breath when lying flat. "
        "The client has mild abdominal tenderness and a temperature of 38.1 C (100.6 F). Medicines: spironolactone "
        "100 mg and furosemide 40 mg daily, propranolol. Weight 82 kg (74 kg a month ago).", id="asc-note")]),
    ("asc-labs", "Labs", [table("Labs", ["Admission", "Reference"], [
        ("Sodium", ["129 mEq/L", "135 to 145"]),
        ("Potassium", ["5.3 mEq/L", "3.5 to 5.0"]),
        ("Creatinine", ["1.4 mg/dL (0.9 last month)", "-"]),
        ("Albumin", ["2.4 g/dL", "3.5 to 5.0"]),
        ("Platelets", ["68,000/mm3", "-"]),
        ("INR", ["1.7", "-"])], id="asc-table")]),
]

asc_qs = [
hl_table(M(PA, LIVER, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Abdomen", "Tense, distended, mildly tender"], True, "Tense ascites; tenderness raises concern for infection."),
    (["Temperature", "38.1 C (100.6 F)"], True, "Fever in ascites suggests spontaneous bacterial peritonitis."),
    (["Breathing", "Short of breath lying flat"], True, "Ascites pushes on the diaphragm."),
    (["Creatinine", "1.4 mg/dL (0.9)"], True, "Kidney injury; risk of hepatorenal syndrome."),
    (["Potassium", "5.3 mEq/L"], True, "Spironolactone keeps potassium."),
    (["Platelets / INR", "68,000 / 1.7"], False, "Expected in cirrhosis; paracentesis is still safe without correction.")],
   "Tense ascites with fever, tenderness and rising creatinine needs investigation for infection and kidney injury.",
   "Recognize cues in decompensated cirrhosis."),

cloze(M(RRP, GI_DX, "analyze_cues", "difficult", GI, "ms lab"),
   "The paracentesis fluid shows 480 neutrophils/mm3. Complete the sentences by choosing from the lists of options.",
   "This result means {{b1}}. The client is at risk for {{b2}} because of {{b3}}.",
   [[("spontaneous bacterial peritonitis", True, "250 or more neutrophils/mm3 in ascitic fluid."),
     ("normal ascitic fluid", False, "Above the cutoff."),
     ("cancer in the abdomen", False, "Not shown by this result."),
     ("a perforated bowel", False, "Would show many organisms and other findings.")],
    [("hepatorenal syndrome", True, "SBP often triggers kidney failure in cirrhosis."),
     ("hypoglycemia", False, "Not the main risk."),
     ("gallstones", False, "Not related."),
     ("hypertension", False, "Not related.")],
    [("infection and rising creatinine in cirrhosis", True, "Circulatory changes reduce kidney blood flow."),
     ("the low sodium alone", False, "A marker, not the cause."),
     ("the propranolol", False, "Can contribute to low pressure, but infection is the main trigger."),
     ("the platelets", False, "Not related.")]]
   ,
   "SBP is diagnosed by ascitic neutrophils; it commonly leads to hepatorenal syndrome.",
   "Analyze ascitic fluid results."),

matrix_mc(M(PA, LIVER, "prioritize_hypotheses", "difficult", GI, "ms"),
   "For each problem, indicate its priority.",
   ["High priority", "Lower priority"],
   [("Spontaneous bacterial peritonitis", 0, "Mortality is high without prompt treatment."),
    ("Acute kidney injury", 0, "Can progress to hepatorenal syndrome."),
    ("Breathing difficulty from tense ascites", 0, "Relieved by paracentesis."),
    ("Mild hyponatremia (129)", 1, "Monitored; usually not treated urgently.")],
   "Infection, kidney injury and breathing are the priorities.",
   "Prioritize problems in SBP."),

matrix_mc(M(PPT, GI_MEDS, "generate_solutions", "difficult", GI, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("IV ceftriaxone", 0, "Treats SBP."),
    ("IV albumin on day 1 and day 3", 0, "Reduces kidney failure and death in SBP."),
    ("Hold spironolactone and furosemide", 0, "Diuretics worsen kidney injury and potassium."),
    ("Ibuprofen for abdominal pain", 1, "NSAIDs cause kidney failure in cirrhosis."),
    ("Large-volume paracentesis with albumin replacement", 0, "Relieves breathing; albumin prevents circulatory problems."),
    ("Platelet transfusion before every paracentesis", 1, "Not needed routinely.")],
   "Antibiotics, albumin, holding diuretics and avoiding NSAIDs in SBP.",
   "Generate solutions for SBP.",
   ex=[note("Orders", "Ceftriaxone 2 g IV daily. Albumin 1.5 g/kg day 1 and 1 g/kg day 3. Hold diuretics. "
       "Therapeutic paracentesis of 5 L with albumin.", id="asc-orders")]),

sata(M(RRP, GI_DX, "take_action", "difficult", GI, "ms"),
   "During the 5 L paracentesis, which actions should the nurse take?",
   [("Have the client empty the bladder before the procedure", True, "Prevents bladder puncture."),
    ("Position the client upright or semi-Fowler's", True, "Fluid pools in the lower abdomen."),
    ("Monitor blood pressure and heart rate during and after", True, "Fluid shifts can cause hypotension."),
    ("Give albumin as prescribed after the drainage", True, "Prevents circulatory dysfunction."),
    ("Check the site for leaking fluid", True, "A common complication."),
    ("Drain as quickly as possible to shorten the procedure", False, "Rapid removal without albumin risks hypotension.")],
   "Safe paracentesis: empty bladder, upright position, monitoring, albumin and site checks.",
   "Take action during paracentesis.",
   ex=[flow("During paracentesis", ["Start", "4 L removed"], [
       ("Blood pressure", ["112/68", "98/60 mm Hg"]),
       ("Heart rate", ["92", "104/min"]),
       ("Breathing", ["Short of breath", "Easier"])], id="asc-para")]),

matrix_mc(M(MOC, DISCHARGE, "evaluate_outcomes", "difficult", GI, "ms mgmt"),
   "Review the findings on day 5. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Afebrile; repeat ascitic neutrophils 90/mm3", 0, "SBP treated."),
    ("Creatinine 1.0 mg/dL", 0, "Kidney injury resolved."),
    ("No plan for SBP prevention after discharge", 1, "Clients after SBP usually take a preventive antibiotic."),
    ("\"I'll use salt substitute to make food taste better.\"", 1, "Salt substitutes contain potassium; risky with spironolactone."),
    ("Weight down 6 kg", 0, "Ascites reduced."),
    ("Referral to the liver transplant team", 0, "Decompensated cirrhosis warrants evaluation.")],
   "Infection and kidney function improved; prevention and diet safety need follow-up before discharge.",
   "Evaluate discharge readiness after SBP.",
   ex=[note("Day 5", "Diuretics restarted at lower doses. Low-sodium diet teaching started.", id="asc-day5")]),
]

# ================================================================================================
# Case 3: Lower GI bleed on apixaban
# ================================================================================================
lgib_tabs = [
    ("lgib-notes", "Nurses' notes", [note("Emergency department, 1900",
        "A 79-year-old client with atrial fibrillation takes apixaban 5 mg twice daily (last dose at 0800) and "
        "aspirin 81 mg. Since this afternoon the client has passed large amounts of bright red and maroon blood from "
        "the rectum four times and feels faint. History of diverticulosis.", id="lgib-note")]),
    ("lgib-data", "Vital signs and labs", [table("Vital signs and labs", ["1900", "Reference"], [
        ("Blood pressure", ["88/52 mm Hg", "-"]),
        ("Heart rate", ["124/min", "-"]),
        ("Hemoglobin", ["7.6 g/dL (12.4 last month)", "-"]),
        ("Platelets", ["210,000/mm3", "-"]),
        ("BUN / creatinine", ["24 / 1.2 mg/dL", "-"]),
        ("Type and crossmatch", ["Sent", "-"])], id="lgib-table")]),
]

lgib_qs = [
hl_table(M(PA, UPPER, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Blood pressure / heart rate", "88/52; 124/min"], True, "Hemorrhagic shock."),
    (["Hemoglobin", "7.6 g/dL"], True, "Large blood loss."),
    (["Stool", "Large bright red and maroon blood, four times"], True, "Active brisk bleeding."),
    (["Anticoagulant", "Apixaban, last dose 0800"], True, "Still active; needs a reversal plan."),
    (["Aspirin", "81 mg"], True, "Adds bleeding risk."),
    (["BUN", "24 mg/dL"], False, "Only mildly raised; a high BUN suggests an upper source, so this fits lower GI.")],
   "Shock from a brisk lower GI bleed while on an active anticoagulant.",
   "Recognize cues of a major lower GI bleed."),

matrix_mc(M(PA, UPPER, "analyze_cues", "difficult", GI, "ms"),
   "For each finding, indicate whether it points to a lower or an upper GI source.",
   ["Lower GI source", "Upper GI source"],
   [("Bright red blood from the rectum", 0, "Usually colonic, though a brisk upper bleed can do this."),
    ("History of diverticulosis", 0, "Diverticular bleeding is a common cause in older adults."),
    ("Normal BUN-to-creatinine ratio", 0, "Upper GI blood is digested and raises BUN."),
    ("Coffee-ground vomit", 1, "Upper GI source."),
    ("Black, tarry stool with a high BUN", 1, "Digested blood from above.")],
   "Bright red rectal blood, diverticulosis and a normal BUN favor a lower source.",
   "Analyze the source of GI bleeding."),

cloze(M(PA, UPPER, "prioritize_hypotheses", "difficult", GI, "ms ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The client is in {{b1}}, and the bleeding will be harder to stop because {{b2}}.",
   [[("hemorrhagic shock", True, "Hypotension and tachycardia with blood loss."),
     ("septic shock", False, "No infection."),
     ("cardiogenic shock", False, "Not supported."),
     ("anaphylactic shock", False, "Not supported.")],
    [("apixaban is still active", True, "Taken about 11 hours ago."),
     ("the platelets are low", False, "They are normal."),
     ("the client is dehydrated", False, "Not the main factor."),
     ("the hemoglobin is low", False, "A result of bleeding, not a cause.")]],
   "Hemorrhagic shock with an active factor Xa inhibitor.",
   "Prioritize a GI bleed on anticoagulation."),

matrix_mc(M(PPT, ANTIDOTE, "generate_solutions", "difficult", GI, "ph"),
   "For each option, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Two large-bore IV lines and red cell transfusion", 0, "Restores volume and oxygen carriage."),
    ("Andexanet alfa or four-factor PCC as prescribed", 0, "Reverses factor Xa inhibitors in life-threatening bleeding."),
    ("Vitamin K IV to reverse apixaban", 1, "Vitamin K reverses warfarin, not apixaban."),
    ("Protamine sulfate", 1, "Reverses heparin."),
    ("Hold apixaban and aspirin", 0, "Removes ongoing effects."),
    ("CT angiography to locate the bleeding", 0, "Finds an active bleed for embolization.")],
   "Transfuse, reverse with a factor Xa-specific agent or PCC, hold antithrombotics and locate the source.",
   "Generate solutions for a bleed on apixaban.",
   ex=[note("Orders", "Transfuse 2 units packed red cells. Four-factor PCC 50 units/kg. CT angiography. "
       "GI and interventional radiology consults.", id="lgib-orders")]),

sata(M(PPT, TRANSFUSION, "take_action", "difficult", GI, "ph fund"),
   "Fifteen minutes into the first unit, review the findings. Which actions should the nurse take?",
   [("Stop the transfusion", True, "Possible acute hemolytic reaction."),
    ("Keep the line open with 0.9% sodium chloride through new tubing", True, "Keeps access without giving more blood."),
    ("Notify the provider and blood bank", True, "Reaction workup."),
    ("Send the blood bag, tubing and a blood and urine sample as policy requires", True, "Confirms the reaction."),
    ("Slow the transfusion and give acetaminophen", False, "Back pain with fever and hypotension is not a minor reaction."),
    ("Recheck the label and restart if it matches", False, "Do not restart.")],
   "Fever, back pain and falling pressure during a transfusion mean stopping it and starting the reaction protocol.",
   "Take action for a transfusion reaction.",
   ex=[flow("1945", ["Start", "15 minutes"], [
       ("Temperature", ["36.8 C", "38.9 C (102 F)"]),
       ("Blood pressure", ["94/58", "80/46 mm Hg"]),
       ("Client", ["Faint", "Low back pain, chills"]),
       ("Urine", ["-", "Dark red"])], id="lgib-1945")]),

matrix_mc(M(MOC, PRIORITY, "evaluate_outcomes", "difficult", GI, "ms mgmt"),
   "The bleeding vessel was embolized. Review day 2. For each finding, indicate whether it shows improvement or "
   "needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("No further bleeding; hemoglobin stable at 9.1 g/dL", 0, "Bleeding controlled."),
    ("Blood pressure 128/74 mm Hg, heart rate 84/min", 0, "Stable."),
    ("Creatinine 1.9 mg/dL after contrast and the transfusion reaction", 1, "Kidney injury."),
    ("No plan yet for restarting stroke prevention", 1, "AF needs a plan for anticoagulation restart with the team."),
    ("Aspirin still listed as a home medicine", 1, "Should be reviewed; adds bleeding without clear benefit with apixaban."),
    ("Walking with assistance", 0, "Recovering.")],
   "Bleeding is controlled; kidney injury and the antithrombotic plan need follow-up.",
   "Evaluate outcomes after a lower GI bleed.",
   ex=[note("Day 2", "Interventional radiology embolized a sigmoid diverticular artery.", id="lgib-day2")]),
]

# ================================================================================================
# Case 4: Diverticulitis with an abscess
# ================================================================================================
div_tabs = [
    ("div-notes", "Nurses' notes", [note("Emergency department",
        "A 64-year-old client has had left lower abdominal pain for 3 days, fever and constipation. The left lower "
        "abdomen is tender with guarding. The client eats little fiber. CT: sigmoid diverticulitis with a 5 cm "
        "abscess.", id="div-note")]),
    ("div-data", "Vital signs and labs", [table("Vital signs and labs", ["Admission", "Reference"], [
        ("Temperature", ["38.6 C (101.5 F)", "-"]),
        ("Heart rate", ["104/min", "-"]),
        ("Blood pressure", ["128/78 mm Hg", "-"]),
        ("WBC", ["17,200/mm3", "-"]),
        ("CRP", ["160 mg/L", "below 10"])], id="div-table")]),
]

div_qs = [
hl_text(M(PA, BOWEL, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require follow-up.",
   ["The client has ",
    ("left lower abdominal pain for 3 days", True, "Typical of sigmoid diverticulitis."),
    ", ",
    ("fever", True, "Infection."),
    " and ",
    ("tenderness with guarding", True, "Localized peritoneal irritation."),
    ". The client ",
    ("eats little fiber", False, "A long-term risk factor, not an acute finding."),
    ". The CT shows ",
    ("a 5 cm abscess", True, "An abscess this size usually needs drainage."),
    "."],
   "Left lower pain, fever, guarding and an abscess mean complicated diverticulitis.",
   "Recognize cues of complicated diverticulitis."),

matrix_mc(M(PA, BOWEL, "analyze_cues", "difficult", GI, "ms"),
   "For each feature, indicate whether it describes uncomplicated or complicated diverticulitis.",
   ["Uncomplicated", "Complicated"],
   [("Localized inflammation only on CT", 0, "Often treated as an outpatient."),
    ("An abscess", 1, "Complicated."),
    ("Free air in the abdomen", 1, "Perforation."),
    ("A fistula to the bladder with air in the urine", 1, "Complicated."),
    ("Mild pain, able to eat and drink", 0, "Outpatient care may be possible.")],
   "Abscess, perforation, fistula, obstruction or bleeding make diverticulitis complicated.",
   "Analyze diverticulitis severity."),

cloze(M(PA, BOWEL, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is to {{b1}}, and the complication to watch for is {{b2}}.",
   [[("drain the abscess and give IV antibiotics", True, "Source control and treatment."),
     ("start a high-fiber diet today", False, "Fiber is for prevention, not acute illness."),
     ("give a laxative for constipation", False, "Can worsen inflammation."),
     ("discharge on oral antibiotics", False, "An abscess needs admission.")],
    [("perforation with generalized peritonitis", True, "Signs: diffuse rigidity and shock."),
     ("hypoglycemia", False, "Not expected."),
     ("hepatitis", False, "Not related."),
     ("gallstones", False, "Not related.")]],
   "Drainage and IV antibiotics, watching for perforation.",
   "Prioritize complicated diverticulitis."),

sata(M(RRP, GI_DX, "generate_solutions", "difficult", GI, "ms"),
   "Which parts of the plan should the nurse anticipate?",
   [("CT-guided percutaneous drainage of the abscess", True, "Source control without surgery."),
    ("IV antibiotics covering gut organisms", True, "For example, piperacillin-tazobactam."),
    ("Bowel rest with clear liquids, advancing as tolerated", True, "Rests the bowel."),
    ("Pain control", True, "Comfort."),
    ("A barium enema to confirm the diagnosis", False, "Contraindicated in acute diverticulitis."),
    ("A colonoscopy this week", False, "Delayed 6 to 8 weeks to avoid perforation.")],
   "Drain, give antibiotics, rest the bowel and avoid early colonoscopy or barium studies.",
   "Generate the plan for complicated diverticulitis.",
   ex=[note("Plan", "Interventional radiology drain today. Piperacillin-tazobactam IV. Clear liquids.", id="div-plan")]),

sata(M(PA, BOWEL, "take_action", "difficult", GI, "ms"),
   "On day 2, review the findings. Which actions should the nurse take?",
   [("Notify the surgeon immediately", True, "Signs of perforation and peritonitis."),
    ("Keep the client NPO", True, "Surgery may be needed."),
    ("Start the prescribed fluid bolus", True, "Supports perfusion."),
    ("Prepare for emergency surgery", True, "Generalized peritonitis needs surgery."),
    ("Flush the drain vigorously to clear it", False, "Not the problem."),
    ("Give a laxative for the abdominal bloating", False, "Contraindicated.")],
   "Diffuse rigidity, shock and free air mean perforation and emergency surgery.",
   "Take action for perforation.",
   ex=[flow("Day 2", ["Day 1", "Day 2"], [
       ("Abdomen", ["Left lower tenderness", "Rigid throughout"]),
       ("Heart rate", ["104", "132/min"]),
       ("Blood pressure", ["128/78", "92/54 mm Hg"]),
       ("Drain output", ["Pus, 40 mL", "Scant"]),
       ("X-ray", ["-", "Free air under the diaphragm"])], id="div-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", GI, "ms"),
   "The client had a sigmoid colectomy with a temporary colostomy. Before discharge, for each statement, indicate "
   "whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"Once I've healed, I'll slowly increase fiber to 25 to 35 grams a day.\"", 0, "Prevents recurrence."),
    ("\"I'll drink plenty of fluids with the extra fiber.\"", 0, "Prevents constipation."),
    ("\"I must avoid nuts and seeds forever.\"", 1, "Current evidence does not support avoiding them."),
    ("\"I'll stay active and walk daily.\"", 0, "Lowers risk."),
    ("\"I'll have a colonoscopy in 6 to 8 weeks.\"", 0, "Rules out cancer."),
    ("\"Straining is fine if I'm constipated.\"", 1, "Straining raises colonic pressure.")],
   "Fiber with fluids, activity, follow-up colonoscopy and avoiding straining prevent recurrence.",
   "Evaluate diverticular disease teaching.",
   ex=[note("Discharge", "Colostomy reversal planned in 3 months.", id="div-dc")]),
]

cases = [
    case("Chemotherapy-induced nausea and vomiting", cinv_tabs, cinv_qs),
    case("Ascites, paracentesis and spontaneous bacterial peritonitis", asc_tabs, asc_qs),
    case("Lower GI bleed on apixaban", lgib_tabs, lgib_qs),
    case("Diverticulitis with an abscess", div_tabs, div_qs),
]

BUILT = build(114, "gi-cases-cinv-ascites-lgib-diverticulitis",
              "Gastrointestinal case studies: chemotherapy nausea, ascites with SBP, lower GI bleed and diverticulitis",
              items, cases, case_conversion=True)
