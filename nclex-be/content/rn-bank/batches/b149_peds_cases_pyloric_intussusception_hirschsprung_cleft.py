"""Batch 149 (case conversion, chunk 13: pediatrics part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: pyloric stenosis with hypochloremic metabolic alkalosis; intussusception with an air enema; Hirschsprung disease
with enterocolitis risk and a temporary colostomy; cleft lip and palate with feeding and postoperative care. Each case
walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety
or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
GI = "pediatric-gi-gu-disorders"
DEHYDRATION = "infant-dehydration"
LYTES = "electrolyte-imbalances"
NUTR = "infant-and-child-nutrition"
POSTOP = "postoperative-care"
OSTOMY = "ostomy-care"
BASICS = "pediatric-care-basics"
TEACH = "health-teaching-and-behavior-change"
COLLAB = "collaboration-and-referrals"
COPING = "coping-and-adjustment"

items = []

# ================================================================================================
# Case 1: Pyloric stenosis
# ================================================================================================
py_tabs = [
    ("py-notes", "Emergency notes", [note("Emergency department",
        "A 5-week-old first-born boy has vomited forcefully across the room after most feeds for 4 days. The vomit "
        "is milky without bile. He is hungry right after vomiting.", id="py-note")]),
    ("py-data", "Findings", [table("Findings", ["Result"], [
        ("Abdomen", ["Olive-shaped mass in the right upper abdomen; waves moving left to right after feeds"]),
        ("Weight", ["Below birth weight"]),
        ("Chloride", ["84 mEq/L"]),
        ("Potassium", ["3.0 mEq/L"]),
        ("Bicarbonate", ["34 mEq/L"]),
        ("Fontanel", ["Sunken"]),
        ("Wet diapers", ["2 in 24 hours"])], id="py-table")]),
]

py_qs = [
hl_table(M(PA, GI, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of pyloric stenosis.",
   ["Finding", "Result"],
   [(["Projectile non-bilious vomiting", "Yes"], True, "Classic."),
    (["Olive-shaped mass", "Yes"], True, "Hypertrophied pylorus."),
    (["Visible peristalsis", "Left to right"], True, "Classic."),
    (["Hungry after vomiting", "Yes"], True, "Classic."),
    (["First-born boy, 5 weeks", "Yes"], True, "Typical age and sex.")],
   "Projectile non-bilious vomiting with an olive mass is pyloric stenosis.",
   "Recognize cues of pyloric stenosis."),

matrix_mc(M(PA, LYTES, "analyze_cues", "difficult", PEDS, "peds"),
   "For each lab result, indicate its cause.",
   ["Loss of stomach acid", "Kidney response to dehydration", "Both"],
   [("Low chloride", 0, "HCl loss."),
    ("High bicarbonate (metabolic alkalosis)", 0, "Acid loss."),
    ("Low potassium", 2, "Vomiting and kidney sodium-potassium exchange."),
    ("Concentrated urine", 1, "Dehydration.")],
   "Vomiting stomach acid causes hypochloremic metabolic alkalosis.",
   "Analyze pyloric stenosis labs."),

cloze(M(PA, GI, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "Before surgery, the priority is {{b1}}, because {{b2}}.",
   [[("correcting dehydration and electrolytes with IV fluids", True, "Surgery is not an emergency."),
     ("immediate surgery tonight", False, "Correct alkalosis first."),
     ("thickened feeds", False, "Not a treatment."),
     ("an antiemetic", False, "Not effective.")],
    [("alkalosis raises the risk of apnea after anesthesia", True, "Safety."),
     ("the pylorus will open on its own", False, "It will not."),
     ("feeds must continue", False, "NPO before surgery."),
     ("it prevents jaundice", False, "Not related.")]],
   "Correct fluids and electrolytes before pyloromyotomy.",
   "Prioritize pyloric stenosis care."),

sata(M(PA, DEHYDRATION, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the preoperative plan should the nurse anticipate?",
   [("NPO with IV fluids", True, "Rehydration."),
    ("Potassium after voiding", True, "Kidney function first."),
    ("NG tube if ordered", True, "Decompression."),
    ("Diaper and daily weights", True, "Fluid."),
    ("Continue breastfeeding on demand until the surgery", False, "Vomiting and aspiration."),
    ("Potassium chloride IV push to correct the low level", False, "Never IV push.")],
   "NPO, IV fluids, potassium after voiding, NG and monitoring.",
   "Generate the preoperative plan.",
   ex=[note("Day 2", "Chloride 102, bicarbonate 26. Pyloromyotomy planned today.", id="py-day2")]),

sata(M(RRP, POSTOP, "take_action", "difficult", PEDS, "peds"),
   "After pyloromyotomy, which actions should the nurse take?",
   [("Early, slow feeds", True, "Early feeding."),
    ("Expect some vomiting", True, "Common."),
    ("Burp, hold upright", True, "Reduces vomiting."),
    ("Apnea monitoring", True, "Anesthesia."),
    ("Keep the infant NPO for 3 days to rest the stomach", False, "Too long."),
    ("Lay the infant flat on the back right after feeds", False, "Aspiration.")],
   "Early, slow feeds with burping and monitoring.",
   "Take action after pyloromyotomy.",
   ex=[note("Post-op", "Laparoscopic pyloromyotomy done.", id="py-postop")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each finding or statement, indicate whether it shows readiness or needs follow-up.",
   ["Readiness", "Needs follow-up"],
   [("Tolerating full feeds for 24 hours", 0, "Ready."),
    ("Six wet diapers in 24 hours", 0, "Hydrated."),
    ("\"Projectile vomiting again after a week means I call.\"", 0, "Correct."),
    ("Redness and drainage at a port site", 1, "Infection."),
    ("\"I'll keep the incisions clean and dry.\"", 0, "Correct.")],
   "Ready; wound infection needs follow-up.",
   "Evaluate pyloric stenosis recovery.",
   ex=[note("Day 2", "Feeding well.", id="py-dc")]),
]

# ================================================================================================
# Case 2: Intussusception
# ================================================================================================
in_tabs = [
    ("in-notes", "Emergency notes", [note("Emergency department",
        "An 8-month-old has had episodes every 15 to 20 minutes of sudden screaming with the knees drawn up, "
        "followed by quiet periods. The infant vomited and passed a stool like red currant jelly.", id="in-note")]),
    ("in-data", "Findings", [flow("Findings", ["Arrival", "3 hours"], [
        ("Abdomen", ["Sausage-shaped mass in the right upper abdomen", "Distended, tender, rigid"]),
        ("Behavior", ["Comfortable between episodes", "Lethargic"]),
        ("Heart rate", ["150", "188/min"]),
        ("Temperature", ["37.4 C", "38.9 C"]),
        ("Stool", ["Currant jelly", "Normal brown stool passed"])], id="in-flow")]),
]

in_qs = [
hl_table(M(PA, GI, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of intussusception at arrival.",
   ["Finding", "Arrival"],
   [(["Episodic screaming with knees drawn up", "Yes"], True, "Colicky pain."),
    (["Currant jelly stool", "Yes"], True, "Blood and mucus."),
    (["Sausage-shaped mass", "Yes"], True, "Telescoped bowel."),
    (["Age", "8 months"], True, "Typical age."),
    (["Vomiting", "Yes"], True, "Obstruction.")],
   "Colicky pain, a sausage mass and currant jelly stool are intussusception.",
   "Recognize cues of intussusception."),

matrix_mc(M(PA, GI, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding, indicate whether it fits intussusception, pyloric stenosis or Hirschsprung disease.",
   ["Intussusception", "Pyloric stenosis", "Hirschsprung disease"],
   [("Currant jelly stool", 0, "Intussusception."),
    ("Projectile non-bilious vomiting", 1, "Pyloric."),
    ("No meconium in the first 48 hours", 2, "Hirschsprung."),
    ("Ribbon-like foul stools in a toddler", 2, "Hirschsprung."),
    ("Olive-shaped mass", 1, "Pyloric.")],
   "Key signs separate pediatric GI obstructions.",
   "Analyze pediatric GI obstruction."),

cloze(M(PA, GI, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The first treatment is {{b1}}, unless {{b2}}.",
   [[("an air or contrast enema to reduce the intussusception", True, "Non-surgical reduction."),
     ("a laxative", False, "Harmful."),
     ("waiting for it to resolve", False, "Bowel death risk."),
     ("antibiotics alone", False, "Not enough.")],
    [("there are signs of perforation or peritonitis", True, "Then surgery."),
     ("the infant is hungry", False, "Not a reason."),
     ("the parents prefer surgery", False, "Not the indication."),
     ("the stool is red", False, "Expected.")]],
   "Enema reduction unless perforated.",
   "Prioritize intussusception."),

sata(M(PA, GI, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse include before the enema?",
   [("IV access and fluids", True, "Dehydration."),
    ("NPO", True, "Possible surgery."),
    ("Prepare the parents for what will happen", True, "Anxiety."),
    ("Surgery team aware in case of perforation", True, "Backup."),
    ("Give a laxative to clear the bowel", False, "Dangerous."),
    ("Feed to calm the infant", False, "Aspiration.")],
   "IV, NPO, preparation and surgical backup.",
   "Generate the pre-enema plan.",
   ex=[note("Ultrasound", "Target sign confirmed.", id="in-us")]),

sata(M(PA, GI, "take_action", "difficult", PEDS, "peds"),
   "Three hours later, before the enema, review the findings. Which actions should the nurse take?",
   [("Notify the provider now", True, "Perforation and peritonitis."),
    ("Prepare for surgery", True, "Enema is contraindicated with perforation."),
    ("Weigh the stool against the other signs", True, "Interpret in context."),
    ("Proceed with the air enema as planned because the stool is normal", False, "Perforation."),
    ("Give a sedative for the lethargy and irritability", False, "Masks shock.")],
   "A rigid abdomen with fever and shock means perforation.",
   "Take action on suspected perforation.",
   ex=[note("3 hours", "Abdominal x-ray: free air.", id="in-xray")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "After surgery with a small bowel resection, for each finding, indicate whether it shows recovery or needs "
   "follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Bowel sounds returned on day 2", 0, "Recovery."),
    ("Tolerating feeds", 0, "Recovery."),
    ("Afebrile", 0, "Recovery."),
    ("Screaming episodes with currant jelly stool on day 4", 1, "Recurrence."),
    ("Urine output 1 mL/kg/hr", 0, "Adequate.")],
   "Recovery; recurrence needs urgent evaluation.",
   "Evaluate intussusception recovery.",
   ex=[note("Surgery", "Perforated segment resected.", id="in-surgery")]),
]

# ================================================================================================
# Case 3: Hirschsprung disease
# ================================================================================================
hi_tabs = [
    ("hi-notes", "NICU notes", [note("Newborn, 48 hours",
        "A term newborn has not passed meconium at 48 hours. The abdomen is distended, the newborn refuses feeds and "
        "has bile-stained vomiting.", id="hi-note")]),
    ("hi-data", "Findings", [table("Findings", ["Result"], [
        ("Rectal exam", ["Tight anus; explosive stool and gas when the finger is removed"]),
        ("Contrast enema", ["Narrow rectum with a wide colon above"]),
        ("Rectal biopsy", ["No ganglion cells"]),
        ("Temperature", ["37.0 C"]),
        ("Abdominal girth", ["Increasing"])], id="hi-table")]),
]

hi_qs = [
hl_table(M(PA, GI, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of Hirschsprung disease.",
   ["Finding", "Result"],
   [(["No meconium", "At 48 hours"], True, "Classic."),
    (["Explosive stool after rectal exam", "Yes"], True, "Classic."),
    (["Transition zone", "Narrow rectum, wide colon"], True, "Aganglionic segment."),
    (["Biopsy", "No ganglion cells"], True, "Diagnostic."),
    (["Temperature", "37.0 C"], False, "Normal now.")],
   "Delayed meconium with an aganglionic rectum is Hirschsprung disease.",
   "Recognize cues of Hirschsprung disease."),

matrix_mc(M(PA, GI, "analyze_cues", "difficult", PEDS, "peds"),
   "For each statement, indicate whether it is accurate or inaccurate about Hirschsprung disease.",
   ["Accurate", "Inaccurate"],
   [("Missing nerve cells stop the bowel from relaxing", 0, "Functional obstruction."),
    ("Enterocolitis is the most serious complication", 0, "Can be fatal."),
    ("Rectal temperatures are safe", 1, "Avoid rectal procedures."),
    ("It is linked to Down syndrome", 0, "Association."),
    ("Laxatives cure it", 1, "Surgery is required.")],
   "Aganglionic bowel needs surgery; watch for enterocolitis.",
   "Analyze Hirschsprung disease."),

cloze(M(PA, GI, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The most dangerous complication is {{b1}}, shown by {{b2}}.",
   [[("enterocolitis", True, "Life-threatening."),
     ("constipation", False, "Expected."),
     ("jaundice", False, "Not related."),
     ("diaper rash", False, "Minor.")],
    [("fever, explosive foul diarrhea and abdominal distension", True, "Classic."),
     ("a firm, soft stool", False, "Not a sign."),
     ("hunger", False, "Not a sign."),
     ("normal weight gain", False, "Not a sign.")]],
   "Fever with explosive diarrhea and distension is enterocolitis.",
   "Prioritize Hirschsprung complications."),

sata(M(BCC, OSTOMY, "generate_solutions", "difficult", PEDS, "peds"),
   "The newborn has a temporary colostomy. Which parts of the plan should the nurse include?",
   [("Check stoma color", True, "Healthy stoma."),
    ("Daily girth", True, "Distension."),
    ("Skin barrier", True, "Breakdown."),
    ("Teach pouch care", True, "Home care."),
    ("Expect a pale or dusky stoma in the first weeks", False, "Ischemia; report."),
    ("Rectal temperatures to check for fever", False, "Avoid.")],
   "Stoma checks, girth, skin and parent teaching.",
   "Generate colostomy care.",
   ex=[note("Surgery", "Leveling colostomy. Pull-through planned at 6 to 12 months.", id="hi-surgery")]),

sata(M(MOC, COLLAB, "take_action", "difficult", PEDS, "mgmt peds"),
   "Before discharge, which referrals and supports should the nurse arrange?",
   [("Wound and ostomy nurse", True, "Teaching."),
    ("Home health nursing", True, "Support."),
    ("Supplies and a list of when to call", True, "Safety."),
    ("Parent support group", True, "Coping."),
    ("No follow-up until the next surgery", False, "Needs regular visits."),
    ("Formula changes to stop the stool", False, "Not appropriate.")],
   "Ostomy nurse, home health, supplies and support.",
   "Take action to coordinate home care.",
   ex=[note("Day 7", "Parents nervous about the pouch.", id="hi-day7")]),

matrix_mc(M(PA, GI, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At a home visit at 2 months, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Stoma pink and moist", 0, "Healthy."),
    ("Gaining weight", 0, "Good."),
    ("Mild redness around the stoma treated with barrier powder", 0, "Managed."),
    ("Fever, lethargy and foul watery output", 1, "Enterocolitis."),
    ("Parents changing the pouch with confidence", 0, "Coping.")],
   "Thriving; enterocolitis signs need urgent care.",
   "Evaluate Hirschsprung home care.",
   ex=[note("Home visit", "2 months old.", id="hi-home")]),
]

# ================================================================================================
# Case 4: Cleft lip and palate
# ================================================================================================
cl_tabs = [
    ("cl-notes", "Newborn notes", [note("Newborn nursery",
        "A term newborn has a complete unilateral cleft lip and palate. The parent is distressed and asks, \"Did I "
        "cause this?\" The newborn coughs and chokes during the first bottle and milk comes out of the nose.",
        id="cl-note")]),
    ("cl-data", "Findings", [table("Findings", ["Result"], [
        ("Feeding", ["Takes 45 minutes; milk through the nose"]),
        ("Weight", ["3.2 kg at birth"]),
        ("SpO2 during feeds", ["Drops to 90%"]),
        ("Ears", ["Normal"]),
        ("Parent", ["Tearful, avoiding looking at the face"])], id="cl-table")]),
]

cl_qs = [
hl_table(M(HPM, NUTR, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that need nursing action now.",
   ["Finding", "Result"],
   [(["Choking and milk through the nose", "Yes"], True, "Aspiration risk."),
    (["Long feeds", "45 minutes"], True, "Fatigue and poor intake."),
    (["SpO2 drop", "90% during feeds"], True, "Aspiration or fatigue."),
    (["Parent distress", "Avoiding the face"], True, "Bonding."),
    (["Ears", "Normal"], False, "Normal now; ear infections are common later.")],
   "Feeding safety and bonding are the first needs.",
   "Recognize cues in cleft lip and palate."),

matrix_mc(M(HPM, NUTR, "analyze_cues", "difficult", PEDS, "peds"),
   "For each problem, indicate whether it is linked to cleft palate.",
   ["Linked", "Not linked"],
   [("Ear infections from eustachian tube problems", 0, "Common."),
    ("Speech problems", 0, "Common."),
    ("Dental problems", 0, "Common."),
    ("Aspiration during feeds", 0, "Risk."),
    ("Cyanotic heart disease in every case", 1, "Not always.")],
   "Cleft palate affects feeding, ears, speech and teeth.",
   "Analyze cleft palate."),

cloze(M(HPM, NUTR, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The best feeding method is {{b1}}, holding the newborn {{b2}}.",
   [[("a special cleft bottle or squeeze bottle with a long nipple", True, "Needs no suction."),
     ("a regular bottle with a hole cut larger", False, "Aspiration."),
     ("feeding by spoon only", False, "Too slow."),
     ("NPO until surgery", False, "Unnecessary.")],
    [("upright, with frequent burping", True, "Reduces aspiration and swallowed air."),
     ("flat on the back", False, "Aspiration."),
     ("face down", False, "Unsafe."),
     ("with the head lower than the body", False, "Aspiration.")]],
   "Special bottle, upright and frequent burping.",
   "Prioritize cleft feeding."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", PEDS, "peds"),
   "After cleft lip repair at 3 months, which parts of the plan should the nurse include?",
   [("Back or side-lying", True, "Protects the repair."),
    ("Elbow restraints", True, "Keeps hands away."),
    ("Clean the suture line", True, "Prevents crusting."),
    ("Good pain control", True, "Comfort."),
    ("Let the infant suck on a pacifier for comfort", False, "Strains the repair."),
    ("Place the infant on the stomach to sleep", False, "Rubs the repair.")],
   "Position, restraints, suture care and pain control.",
   "Generate post-repair care.",
   ex=[note("3 months", "Cleft lip repaired. Palate repair at 9 to 12 months.", id="cl-repair")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "Which responses support the parent at birth?",
   [("\"This is not caused by anything you did. Let's look at your baby together.\"", True, "Reduces guilt and models acceptance."),
    ("Show before-and-after photos of repaired clefts", True, "Hope."),
    ("Praise the baby's features, such as the eyes", True, "Bonding."),
    ("Connect the family with the cleft team", True, "Support."),
    ("\"Don't worry, no one will notice after surgery.\"", False, "False reassurance."),
    ("Cover the baby's face with a blanket when visitors come", False, "Shame.")],
   "Reduce guilt, model acceptance and connect to the team.",
   "Take action with parent support.",
   ex=[note("Day 1", "Parent holding the baby.", id="cl-day1")]),

matrix_mc(M(HPM, NUTR, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At the 2-week visit, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Back to birth weight", 0, "Good intake."),
    ("Feeds take 20 minutes", 0, "Efficient."),
    ("\"I hold the baby upright and burp often.\"", 0, "Correct."),
    ("Weight 200 g below birth weight", 1, "Poor intake."),
    ("\"I feel more comfortable showing the baby to family now.\"", 0, "Bonding.")],
   "Progress in feeding and bonding.",
   "Evaluate cleft feeding.",
   ex=[note("2 weeks", "Visit with the cleft team.", id="cl-2wk")]),
]

cases = [
    case("Pyloric stenosis with metabolic alkalosis", py_tabs, py_qs),
    case("Intussusception with perforation", in_tabs, in_qs),
    case("Hirschsprung disease with a colostomy", hi_tabs, hi_qs),
    case("Cleft lip and palate", cl_tabs, cl_qs),
]

BUILT = build(149, "peds-cases-pyloric-intussusception-hirschsprung-cleft",
              "Pediatric GI case studies: pyloric stenosis, intussusception, Hirschsprung disease and cleft lip and "
              "palate",
              items, cases, case_conversion=True)
