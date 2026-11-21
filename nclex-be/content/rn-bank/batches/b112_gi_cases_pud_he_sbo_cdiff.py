"""Batch 112 (case conversion, chunk 4: gastrointestinal): four full NGN case studies (CASE-MAP.md).

Cases: a perforated duodenal ulcer; hepatic encephalopathy in cirrhosis; small bowel obstruction with
strangulation; severe Clostridioides difficile colitis progressing to toxic megacolon. Each case walks the six CJMM
steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
GI = "gastrointestinal"
UPPER = "upper-gi-disorders-and-bleeding"
LIVER = "liver-disorders"
BOWEL = "bowel-disorders"
GI_MEDS = "gi-medications"
GI_DX = "gi-diagnostic-procedures"
POSTOP = "postoperative-care"
PREOP = "preoperative-care"
CONSENT = "informed-consent"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
INJURY = "injury-prevention"
HOME = "equipment-and-home-safety"
TBP = "transmission-based-precautions"
HAI = "healthcare-associated-infection-prevention"
TEACH = "health-teaching-and-behavior-change"
ABX = "antimicrobial-therapy"

items = []

# ================================================================================================
# Case 1: Perforated duodenal ulcer
# ================================================================================================
pud_tabs = [
    ("pud-notes", "Nurses' notes", [note("Emergency department, 1500",
        "A 58-year-old client has had burning upper abdominal pain for months that eased after meals. The client "
        "smokes and takes ibuprofen 800 mg three times daily for back pain. An hour ago the client had sudden, "
        "severe pain across the upper abdomen spreading to the right shoulder. The client lies very still with the "
        "knees drawn up. The abdomen is rigid and board-like, with rebound tenderness; bowel sounds are absent.",
        id="pud-note")]),
    ("pud-data", "Vital signs and tests", [table("Vital signs and tests", ["1500", "Reference"], [
        ("Blood pressure", ["98/60 mm Hg", "-"]),
        ("Heart rate", ["124/min", "-"]),
        ("Temperature", ["38.4 C (101.1 F)", "-"]),
        ("WBC", ["17,600/mm3", "5,000 to 10,000"]),
        ("Lactate", ["3.0 mmol/L", "0.5 to 2.0"]),
        ("Upright chest x-ray", ["Free air under the diaphragm", "-"])], id="pud-table")]),
]

pud_qs = [
hl_text(M(PA, UPPER, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require immediate action.",
   ["The client had ",
    ("sudden, severe pain spreading to the right shoulder", True, "Perforation irritates the diaphragm and refers pain to the shoulder."),
    " and lies ",
    ("very still with the knees drawn up", True, "Any movement hurts in peritonitis."),
    ". The abdomen is ",
    ("rigid and board-like with rebound tenderness", True, "Signs of peritonitis."),
    ". The client ",
    ("smokes and takes ibuprofen daily", False, "Risk factors that explain the ulcer, not signs needing action now."),
    ". The x-ray shows ",
    ("free air under the diaphragm", True, "Confirms perforation."),
    ", and the lactate is ",
    ("3.0 mmol/L", True, "Hypoperfusion."),
    "."],
   "Sudden severe pain, a rigid abdomen, free air and rising lactate mean perforation with peritonitis.",
   "Recognize cues of a perforated ulcer."),

matrix_mc(M(PA, UPPER, "analyze_cues", "difficult", GI, "ms"),
   "For each finding, indicate what it shows.",
   ["Chemical then bacterial peritonitis", "Hypovolemia and early sepsis", "History of duodenal ulcer"],
   [("Pain that used to ease after meals", 2, "Duodenal ulcer pain is relieved by food."),
    ("Absent bowel sounds", 0, "Peritonitis paralyzes the bowel."),
    ("Heart rate 124/min and BP 98/60 mm Hg", 1, "Fluid shifts into the peritoneum."),
    ("Temperature 38.4 C and WBC 17,600", 1, "Infection is developing."),
    ("Rebound tenderness", 0, "Peritoneal irritation.")],
   "The history fits a duodenal ulcer; the exam shows peritonitis; the vital signs show early sepsis.",
   "Analyze findings of a perforated ulcer."),

cloze(M(PA, UPPER, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client needs {{b1}} because {{b2}}.",
   [[("emergency surgery", True, "A perforation needs closure and washout."),
     ("an oral proton pump inhibitor and discharge", False, "Life-threatening."),
     ("a barium swallow to confirm", False, "Barium leaking into the abdomen is harmful."),
     ("observation overnight", False, "Delay increases deaths.")],
    [("gastric contents are leaking into the peritoneum and sepsis is developing", True, "Source control is needed."),
     ("the pain is severe", False, "Pain is not the indication."),
     ("the client smokes", False, "Not the indication."),
     ("the client is 58", False, "Not the indication.")]],
   "Perforation with peritonitis needs emergency surgery.",
   "Prioritize perforated ulcer care."),

matrix_mc(M(PPT, GI_MEDS, "generate_solutions", "difficult", GI, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("NPO and a nasogastric tube to low intermittent suction", 0, "Reduces leakage."),
    ("IV pantoprazole", 0, "Reduces acid."),
    ("Broad-spectrum IV antibiotics", 0, "Treats peritonitis."),
    ("IV fluids with isotonic crystalloid", 0, "Restores volume."),
    ("Oral antacids for the pain", 1, "Nothing by mouth with a perforation."),
    ("Ketorolac IV for the pain", 1, "An NSAID worsens ulcers and kidney perfusion; use an opioid.")],
   "NPO, decompress, acid suppression, antibiotics, fluids and opioid analgesia before surgery; no NSAIDs or oral "
   "intake.",
   "Generate preoperative solutions for perforation.",
   ex=[note("Orders, 1520", "To OR as soon as available. Hydromorphone 0.5 mg IV given at 1515.", id="pud-orders")]),

sata(M(MOC, CONSENT, "take_action", "difficult", GI, "mgmt"),
   "The surgeon arrives at 1540 to obtain consent. The client received hydromorphone at 1515. Which actions are "
   "appropriate?",
   [("Tell the surgeon the opioid time and how alert the client is", True,
     "The surgeon judges capacity; the nurse provides the information."),
    ("Witness the signature if the client can restate the risks", True,
     "A small opioid dose does not remove capacity if the client understands."),
    ("Document the time and the client's understanding", True, "Records the process."),
    ("Explain the risks of the surgery yourself before the surgeon arrives", False, "Explaining the procedure is the surgeon's role."),
    ("Withhold all further analgesia until the consent form is signed", False, "Pain control is not withheld to obtain consent.")],
   "Consent is the surgeon's duty; the nurse reports medication and alertness, witnesses and documents.",
   "Take action for consent in an emergency.",
   ex=[note("1540", "The client is drowsy but answers questions correctly and asks about a colostomy.", id="pud-1540")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", GI, "ph ms"),
   "After repair, H. pylori was found. Before discharge, the client describes the plan. For each statement, "
   "indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take all the antibiotics for the full 14 days with my omeprazole.\"", 0, "Completing therapy eradicates H. pylori."),
    ("\"I'll switch from ibuprofen to acetaminophen for my back.\"", 0, "Avoids NSAID injury."),
    ("\"The bismuth may turn my stools black.\"", 0, "Expected; tarry stools with dizziness would be reported."),
    ("\"I'll keep smoking because it helps my stress.\"", 1, "Smoking delays ulcer healing; offer cessation help."),
    ("\"I'll be retested after treatment to make sure the infection is gone.\"", 0, "Confirms eradication."),
    ("\"I can stop omeprazole once the pain goes.\"", 1, "Take for the prescribed course.")],
   "Teaching covers completing eradication therapy, avoiding NSAIDs and smoking, expected bismuth effects and retesting.",
   "Evaluate ulcer discharge teaching.",
   ex=[note("Day 5", "Eating a soft diet. H. pylori biopsy positive. Bismuth quadruple therapy prescribed.", id="pud-day5")]),
]

# ================================================================================================
# Case 2: Hepatic encephalopathy
# ================================================================================================
he_tabs = [
    ("he-notes", "Nurses' notes", [note("Medical unit, 0800",
        "A 61-year-old client with alcohol-related cirrhosis is confused and sleepy this morning, after being alert "
        "yesterday. The client has had no bowel movement for 3 days and was given lorazepam 1 mg last night for "
        "sleep. When the arms are held out with wrists extended, the hands flap. Breath has a sweet, musty odor.",
        id="he-note")]),
    ("he-labs", "Labs", [table("Labs", ["0600", "Reference"], [
        ("Ammonia", ["118 mcmol/L", "15 to 45"]),
        ("Potassium", ["3.0 mEq/L", "3.5 to 5.0"]),
        ("Sodium", ["131 mEq/L", "135 to 145"]),
        ("Hemoglobin", ["11.8 g/dL (12.0 yesterday)", "-"]),
        ("INR", ["1.9", "-"]),
        ("Glucose", ["94 mg/dL", "-"])], id="he-table")]),
]

he_qs = [
hl_table(M(PA, LIVER, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that suggest hepatic encephalopathy or its triggers.",
   ["Finding", "Value"],
   [(["Mental status", "New confusion and sleepiness"], True, "Encephalopathy."),
    (["Hands", "Flapping tremor (asterixis)"], True, "A classic sign."),
    (["Breath", "Sweet, musty odor"], True, "Fetor hepaticus."),
    (["Bowels", "No stool for 3 days"], True, "Constipation increases ammonia absorption."),
    (["Lorazepam", "1 mg last night"], True, "Sedatives trigger encephalopathy."),
    (["Hemoglobin", "11.8 g/dL (12.0 yesterday)"], False, "Stable; no sign of GI bleeding as a trigger."),
    (["Glucose", "94 mg/dL"], False, "Normal; hypoglycemia is not the cause.")],
   "Confusion, asterixis and fetor with constipation and a benzodiazepine point to hepatic encephalopathy.",
   "Recognize cues of hepatic encephalopathy."),

matrix_mc(M(PA, LIVER, "analyze_cues", "difficult", GI, "ms ph"),
   "For each factor, indicate whether it can trigger or worsen hepatic encephalopathy.",
   ["Trigger or worsens it", "Does not"],
   [("Constipation", 0, "More time for gut bacteria to make ammonia."),
    ("Benzodiazepines", 0, "Sedation in a sensitive brain."),
    ("Hypokalemia", 0, "Low potassium increases kidney ammonia production."),
    ("GI bleeding", 0, "Blood protein is broken down into ammonia."),
    ("Infection", 0, "A common trigger."),
    ("Lactulose", 1, "Treats it.")],
   "Constipation, sedatives, low potassium, bleeding and infection all raise ammonia or brain sensitivity.",
   "Analyze triggers of hepatic encephalopathy."),

cloze(M(PA, LIVER, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Because the client is drowsy, the most immediate risks are {{b1}} and {{b2}}.",
   [[("aspiration", True, "Drowsiness impairs airway protection."),
     ("hypertension", False, "Not expected."),
     ("hyperglycemia", False, "Not expected."),
     ("urinary retention", False, "Not the main risk.")],
    [("falls", True, "Confusion with a raised INR makes a fall dangerous."),
     ("pressure injury", False, "Less immediate."),
     ("constipation", False, "Already present and being treated."),
     ("dehydration", False, "Less immediate.")]],
   "Drowsiness creates aspiration and fall risks, made worse by a high INR.",
   "Prioritize safety risks in hepatic encephalopathy."),

matrix_mc(M(PPT, GI_MEDS, "generate_solutions", "difficult", GI, "ph ms"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Lactulose titrated to 2 or 3 soft stools a day", 0, "Traps ammonia in the gut and speeds its removal."),
    ("Rifaximin", 0, "Reduces ammonia-producing gut bacteria."),
    ("Potassium replacement", 0, "Corrects a trigger."),
    ("Stop lorazepam", 0, "Removes a trigger."),
    ("Restrict dietary protein to 20 g a day", 1, "Severe restriction worsens malnutrition; normal protein is recommended."),
    ("Haloperidol for confusion", 1, "Sedating drugs worsen encephalopathy.")],
   "Lactulose, rifaximin, potassium and removing sedatives; protein is not severely restricted.",
   "Generate solutions for hepatic encephalopathy.",
   ex=[note("Orders", "Lactulose 30 mL every 2 hours until stooling, then three times daily. Rifaximin 550 mg twice "
       "daily. Potassium chloride 40 mEq. Discontinue lorazepam.", id="he-orders")]),

sata(M(SIC, INJURY, "take_action", "difficult", GI, "fund ms"),
   "Which safety actions should the nurse take?",
   [("Keep the head of the bed up and check the swallow before oral lactulose", True, "Prevents aspiration."),
    ("Low bed, bed alarm and frequent rounding", True, "Fall prevention."),
    ("Keep a bedside commode close and help with toileting", True, "Lactulose causes urgency; rushing causes falls."),
    ("Protect the skin around the anus", True, "Frequent stools irritate skin."),
    ("Apply wrist restraints to stop the client getting up", False, "Restraints increase agitation and injury."),
    ("Give the lactulose through a straw while the client lies flat", False, "Aspiration risk.")],
   "Prevent aspiration and falls, help with frequent toileting and protect the skin.",
   "Take action for safety in hepatic encephalopathy.",
   ex=[flow("1000", ["1000"], [
       ("Mental status", ["Drowsy, follows simple commands"]),
       ("Swallow", ["Coughs on thin water"]),
       ("Lactulose", ["First dose due"])], id="he-1000")]),

matrix_mc(M(PPT, GI_MEDS, "evaluate_outcomes", "difficult", GI, "ph ms"),
   "Review the findings on day 3. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Oriented x3; asterixis gone", 0, "Encephalopathy resolved."),
    ("Ammonia 48 mcmol/L", 0, "Falling."),
    ("Nine watery stools in 24 hours", 1, "Too much lactulose risks dehydration and low sodium; reduce the dose."),
    ("Sodium 127 mEq/L and dizziness on standing", 1, "Fluid loss from excess lactulose."),
    ("Potassium 3.9 mEq/L", 0, "Corrected."),
    ("\"I'll stop lactulose at home once my head is clear.\"", 1, "It is continued to prevent recurrence.")],
   "Mental status improved, but excess lactulose has caused diarrhea, low sodium and dizziness, and the client "
   "plans to stop it.",
   "Evaluate lactulose therapy.",
   ex=[flow("Day 3", ["Day 3"], [
       ("Mental status", ["Oriented x3; no asterixis"]),
       ("Ammonia", ["48 mcmol/L"]),
       ("Stools", ["Nine watery in 24 hours"]),
       ("Sodium / potassium", ["127 / 3.9 mEq/L"]),
       ("Orthostatic", ["Dizzy on standing"])], id="he-day3")]),
]

# ================================================================================================
# Case 3: Small bowel obstruction with strangulation
# ================================================================================================
sbo_tabs = [
    ("sbo-notes", "Nurses' notes", [note("Surgical unit, admission 0600",
        "A 70-year-old client who had a hysterectomy and an appendectomy years ago has had cramping abdominal pain "
        "that comes in waves for 18 hours, with repeated vomiting. The abdomen is distended. High-pitched, tinkling "
        "bowel sounds are heard. No stool or flatus since yesterday. CT: small bowel obstruction from adhesions.",
        id="sbo-note")]),
    ("sbo-labs", "Labs", [table("Labs", ["0600", "Reference"], [
        ("Potassium", ["3.1 mEq/L", "3.5 to 5.0"]),
        ("Chloride", ["91 mEq/L", "98 to 106"]),
        ("Bicarbonate", ["33 mEq/L", "22 to 26"]),
        ("BUN / creatinine", ["38 / 1.5 mg/dL", "-"]),
        ("Lactate", ["1.4 mmol/L", "0.5 to 2.0"]),
        ("WBC", ["11,200/mm3", "5,000 to 10,000"])], id="sbo-table")]),
]

sbo_qs = [
hl_table(M(PA, BOWEL, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Value"],
   [(["Pain", "Cramping, in waves"], True, "Colicky pain from bowel trying to push past the blockage."),
    (["Vomiting", "Repeated"], True, "Fluid and electrolyte loss."),
    (["Bowel sounds", "High-pitched, tinkling"], True, "Early mechanical obstruction."),
    (["Potassium / chloride / bicarbonate", "3.1 / 91 / 33"], True, "Hypokalemic, hypochloremic metabolic alkalosis from vomiting."),
    (["BUN / creatinine", "38 / 1.5"], True, "Dehydration."),
    (["Lactate", "1.4 mmol/L"], False, "Normal for now; a rise would suggest ischemia.")],
   "Colicky pain, vomiting, obstructive bowel sounds and the electrolyte pattern of vomiting.",
   "Recognize cues of small bowel obstruction."),

cloze(M(RRP, LABS, "analyze_cues", "difficult", GI, "ms lab"),
   "Complete the sentences by choosing from the lists of options.",
   "The electrolyte pattern is {{b1}}, caused by {{b2}}. A rise in lactate with constant pain would suggest {{b3}}.",
   [[("hypokalemic, hypochloremic metabolic alkalosis", True, "Low potassium and chloride with high bicarbonate."),
     ("metabolic acidosis", False, "Bicarbonate is high."),
     ("respiratory alkalosis", False, "The primary change is metabolic."),
     ("hyperkalemia", False, "Potassium is low.")],
    [("loss of stomach acid and fluid with vomiting", True, "Loss of hydrochloric acid."),
     ("diarrhea", False, "Diarrhea causes acidosis."),
     ("kidney failure", False, "Not the cause."),
     ("too much IV saline", False, "Not given yet.")],
    [("strangulation with bowel ischemia", True, "Blood supply is cut off."),
     ("the obstruction resolving", False, "Resolution reduces pain and lactate."),
     ("dehydration only", False, "Not with constant pain."),
     ("anxiety", False, "Not supported.")]],
   "Vomiting causes metabolic alkalosis; rising lactate and constant pain suggest strangulation.",
   "Analyze labs in bowel obstruction."),

matrix_mc(M(PA, BOWEL, "prioritize_hypotheses", "difficult", GI, "ms"),
   "For each complication, indicate how urgent it is to watch for.",
   ["Watch closely now", "Less urgent"],
   [("Strangulation and bowel ischemia", 0, "Needs surgery within hours."),
    ("Hypovolemia and kidney injury", 0, "Ongoing losses."),
    ("Aspiration of vomit", 0, "A distended bowel and vomiting."),
    ("Pressure injury", 1, "Less urgent.")],
   "Strangulation, hypovolemia and aspiration are the urgent risks.",
   "Prioritize complications of bowel obstruction."),

grouping(M(SIC, HOME, "generate_solutions", "difficult", GI, "fund ms"),
   "A nasogastric (NG) tube is ordered to low intermittent suction. For each part of NG care, select the safe "
   "actions.",
   [("Placement", [
       ("Confirm initial placement by x-ray", True, "The standard for first placement."),
       ("Mark the tube at the nostril and check the mark each shift", True, "Detects migration."),
       ("Confirm placement by injecting air and listening", False, "Not reliable.")]),
    ("Function", [
       ("Keep the blue vent (pigtail) open and above the stomach", True, "Prevents suction damage to the stomach lining."),
       ("Measure and record drainage every shift", True, "Guides fluid replacement."),
       ("Clamp the vent to stop it leaking", False, "Causes the tube to stick to the stomach wall.")]),
    ("Comfort and skin", [
       ("Give mouth and nostril care regularly", True, "Prevents breakdown and dryness."),
       ("Tape the tube tightly against the nostril edge", False, "Causes pressure injury.")])],
   "Safe NG care: x-ray placement, mark checks, an open vent, measured drainage and skin care.",
   "Generate safe nasogastric tube care.",
   ex=[note("Orders", "NPO. NG tube to low intermittent suction. 0.9% sodium chloride with 20 mEq KCl/L at 150 mL/hour. "
       "Serial abdominal exams. Surgery consult.", id="sbo-orders")]),

sata(M(PA, BOWEL, "take_action", "difficult", GI, "ms"),
   "Review the 1400 findings. Which actions should the nurse take?",
   [("Notify the surgeon now", True, "Signs of strangulation."),
    ("Keep the client NPO and prepare for surgery", True, "Surgery is likely."),
    ("Give IV fluids as prescribed and check the blood pressure often", True, "Supports perfusion."),
    ("Give metoclopramide for the vomiting", False, "Prokinetics are contraindicated in mechanical obstruction."),
    ("Give a laxative to clear the blockage", False, "Contraindicated.")],
   "Constant pain, fever, tachycardia and rising lactate mean strangulation: call the surgeon and prepare.",
   "Take action for suspected strangulation.",
   ex=[flow("1400", ["0600", "1400"], [
       ("Pain", ["Cramping in waves", "Constant and severe"]),
       ("Temperature", ["37.4 C", "38.6 C (101.5 F)"]),
       ("Heart rate", ["98/min", "126/min"]),
       ("Lactate", ["1.4", "4.2 mmol/L"]),
       ("Bowel sounds", ["High-pitched", "Absent"])], id="sbo-1400")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", GI, "ms"),
   "The client had a small bowel resection. Review postoperative day 3. For each finding, indicate whether it shows "
   "improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Passing flatus; bowel sounds present", 0, "Return of bowel function."),
    ("NG output down to 150 mL in 8 hours", 0, "Less output means the bowel is working."),
    ("Potassium 3.9 mEq/L", 0, "Corrected."),
    ("Walking in the hall three times a day", 0, "Helps bowel recovery and prevents clots."),
    ("Incision red with purulent drainage, temperature 38.5 C", 1, "Wound infection."),
    ("Calf pain and swelling on the left", 1, "Possible DVT.")],
   "Bowel function and electrolytes are recovering, but wound infection and a possible DVT need action.",
   "Evaluate recovery after bowel resection.",
   ex=[flow("Postoperative day 3", ["Day 3"], [
       ("Bowel", ["Flatus; bowel sounds present"]),
       ("NG output", ["150 mL in 8 hours"]),
       ("Potassium", ["3.9 mEq/L"]),
       ("Activity", ["Walking three times a day"]),
       ("Incision", ["Red, purulent drainage"]),
       ("Temperature", ["38.5 C (101.3 F)"]),
       ("Left calf", ["Pain and swelling"])], id="sbo-pod3")]),
]

# ================================================================================================
# Case 4: Severe C. difficile colitis and toxic megacolon
# ================================================================================================
cdi_tabs = [
    ("cdi-notes", "Nurses' notes", [note("Medical unit, day 1",
        "A 72-year-old client was treated with clindamycin 2 weeks ago for a skin infection and takes omeprazole "
        "daily. The client now has 12 watery, foul-smelling stools a day, cramping and a distended abdomen. The "
        "client shares a bathroom with another client.", id="cdi-note")]),
    ("cdi-labs", "Labs", [table("Labs", ["Day 1", "Reference"], [
        ("C. difficile toxin", ["Positive", "-"]),
        ("WBC", ["28,000/mm3", "5,000 to 10,000"]),
        ("Creatinine", ["2.0 mg/dL (1.0 last month)", "-"]),
        ("Albumin", ["2.4 g/dL", "3.5 to 5.0"]),
        ("Temperature", ["38.9 C (102 F)", "-"])], id="cdi-table")]),
]

cdi_qs = [
hl_table(M(PA, BOWEL, "recognize_cues", "moderate", GI, "cdc ms"),
   "Highlight the findings that show severe C. difficile infection or its risk factors.",
   ["Finding", "Value"],
   [(["WBC", "28,000/mm3"], True, "Above 15,000 marks severe infection."),
    (["Creatinine", "2.0 mg/dL"], True, "Above 1.5 marks severe infection."),
    (["Recent antibiotic", "Clindamycin"], True, "A high-risk antibiotic."),
    (["Omeprazole", "Daily"], True, "Acid suppression raises the risk."),
    (["Shared bathroom", "With another client"], True, "Spreads spores."),
    (["Stool odor", "Foul-smelling"], False, "Common but not a severity marker.")],
   "A high WBC and rising creatinine mark severe C. difficile; antibiotics and PPIs are risks.",
   "Recognize cues of severe C. difficile infection."),

matrix_mc(M(SIC, HAI, "analyze_cues", "difficult", GI, "cdc"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Alcohol hand rub kills C. difficile spores", 1, "Spores resist alcohol; wash with soap and water."),
    ("Spores survive on surfaces for months", 0, "So rooms are cleaned with a sporicidal agent such as bleach."),
    ("The client needs a private room with its own toilet", 0, "Prevents spread."),
    ("Contact precautions can stop as soon as the diarrhea slows", 1, "Usually continued until 48 hours after diarrhea resolves."),
    ("Dedicated equipment such as a stethoscope stays in the room", 0, "Prevents spread on equipment.")],
   "C. difficile spores need soap-and-water hand washing, sporicidal cleaning, a private bathroom and dedicated equipment.",
   "Analyze C. difficile infection control."),

cloze(M(PA, BOWEL, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client has {{b1}} C. difficile infection, and the complication to watch for most closely is {{b2}}.",
   [[("severe", True, "WBC above 15,000 or creatinine above 1.5."),
     ("mild", False, "The labs show severe disease."),
     ("recurrent", False, "This is the first episode."),
     ("carrier-only", False, "The client has symptoms.")],
    [("toxic megacolon", True, "Severe colitis can dilate and perforate the colon."),
     ("hypertension", False, "Not expected."),
     ("pancreatitis", False, "Not related."),
     ("gallstones", False, "Not related.")]],
   "Severe C. difficile infection risks toxic megacolon.",
   "Prioritize severe C. difficile infection."),

matrix_mc(M(PPT, ABX, "generate_solutions", "difficult", GI, "ph cdc"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Oral vancomycin 125 mg four times daily", 0, "First-line oral treatment; oral is used because IV vancomycin does not reach the colon."),
    ("IV vancomycin instead of oral", 1, "IV vancomycin does not treat colitis."),
    ("Stop omeprazole if there is no strong reason for it", 0, "Removes a risk factor."),
    ("Loperamide for the diarrhea", 1, "Slowing the bowel keeps toxin in and risks megacolon."),
    ("IV fluids and electrolyte replacement", 0, "Replaces losses."),
    ("Probiotics as the main treatment", 1, "Not a treatment for severe infection.")],
   "Oral vancomycin, stopping acid suppression, fluids, and no antidiarrheals.",
   "Generate solutions for severe C. difficile.",
   ex=[note("Orders", "Vancomycin 125 mg PO four times daily. Contact precautions. Private room. IV fluids.",
       id="cdi-orders")]),

sata(M(PA, BOWEL, "take_action", "difficult", GI, "ms"),
   "Review the day 3 findings. Which actions should the nurse take?",
   [("Notify the provider immediately", True, "Fewer stools with a distended, tender abdomen suggests megacolon, not improvement."),
    ("Keep the client NPO", True, "Possible surgery or ileus."),
    ("Prepare for an urgent abdominal x-ray or CT", True, "Measures colon diameter."),
    ("Monitor for signs of perforation and shock", True, "Rigid abdomen, hypotension."),
    ("Record the fewer stools as treatment success", False, "Dangerous misreading."),
    ("Give a tap water enema to relieve the distension", False, "Could perforate the colon.")],
   "Diarrhea that stops while the abdomen swells and the client worsens is toxic megacolon.",
   "Take action for suspected toxic megacolon.",
   ex=[flow("Day 3", ["Day 1", "Day 3"], [
       ("Stools", ["12 a day", "1 today"]),
       ("Abdomen", ["Distended", "Very distended, tender, quiet"]),
       ("Heart rate", ["104/min", "128/min"]),
       ("Blood pressure", ["118/70", "92/56 mm Hg"]),
       ("Lactate", ["-", "3.8 mmol/L"])], id="cdi-day3")]),

matrix_mc(M(SIC, HAI, "evaluate_outcomes", "difficult", GI, "cdc"),
   "The nurse audits infection control on the unit. For each observation, indicate whether it meets the standard "
   "or needs correction.",
   ["Meets the standard", "Needs correction"],
   [("Staff wash hands with soap and water after leaving the room", 0, "Removes spores."),
    ("The room is cleaned daily with a bleach-based product", 0, "Kills spores."),
    ("A visitor wears a gown and gloves in the room", 0, "Contact precautions."),
    ("The blood pressure cuff is taken to the next room after use", 1, "Equipment must stay in the room or be disinfected."),
    ("The former roommate who shared the bathroom is not being monitored", 1, "An exposed client should be watched for diarrhea."),
    ("The client's antibiotic for a urinary infection is being reviewed by stewardship", 0, "Limits unnecessary antibiotics.")],
   "Soap-and-water hand hygiene, bleach cleaning, contact precautions and antibiotic stewardship meet the standard; "
   "shared equipment and unmonitored contacts need correction.",
   "Evaluate C. difficile infection control on a unit.",
   ex=[note("Unit audit, day 4", "The client went to surgery on day 3 for colectomy.", id="cdi-audit")]),
]

cases = [
    case("Perforated duodenal ulcer", pud_tabs, pud_qs),
    case("Hepatic encephalopathy in cirrhosis", he_tabs, he_qs),
    case("Small bowel obstruction with strangulation", sbo_tabs, sbo_qs),
    case("Severe C. difficile colitis and toxic megacolon", cdi_tabs, cdi_qs),
]

BUILT = build(112, "gi-cases-pud-he-sbo-cdiff",
              "Gastrointestinal case studies: perforated ulcer, hepatic encephalopathy, bowel obstruction and severe "
              "C. difficile",
              items, cases, case_conversion=True)
