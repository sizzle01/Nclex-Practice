"""Batch 115 (case conversion, chunk 4: gastrointestinal): four full NGN case studies and four standalone items
(CASE-MAP.md).

Cases: an anastomotic leak after gastric bypass, then dumping syndrome; acetaminophen overdose with acute liver
injury; refeeding syndrome on parenteral nutrition; esophageal perforation after endoscopic dilation. Each case walks
the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety,
psychosocial or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                         "PSI")
GI = "gastrointestinal"
POSTOP = "postoperative-care"
NUTR = "nutrition-and-therapeutic-diets"
HANDLING = "safe-client-handling"
POISON = "poisoning-and-ingestions"
ANTIDOTE = "antidotes-and-reversal-agents"
CALC = "medication-dose-calculations"
LIVER = "liver-disorders"
SUICIDE = "suicide-risk-and-safety"
COLLAB = "collaboration-and-referrals"
TPN = "parenteral-nutrition"
LYTES = "electrolyte-imbalances"
GI_DX = "gi-diagnostic-procedures"
UPPER = "upper-gi-disorders-and-bleeding"
GI_MEDS = "gi-medications"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
TUBE = "tube-feeding-care"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, UPPER, "prioritize_hypotheses", "difficult", GI, "ms ph"),
   "Use the client's data to fill in the diagram: the problem this client most likely has, 2 actions for the "
   "nurse, and 2 parameters to track.",
   [("Bleeding esophageal varices", True, "Cirrhosis with large-volume bright red hematemesis and shock."),
    ("Bleeding gastric ulcer", False, "Possible, but cirrhosis with portal hypertension makes varices most likely."),
    ("Mallory-Weiss tear", False, "Usually smaller bleeds after retching."),
    ("Esophageal cancer", False, "Not an acute presentation.")],
   [("Start the octreotide infusion as prescribed", True, "Lowers portal pressure."),
    ("Prepare for urgent endoscopic band ligation", True, "Definitive control."),
    ("Insert a nasogastric tube to suction", False, "Risks tearing varices; not routine."),
    ("Give propranolol IV now", False, "Beta blockers prevent rebleeding later; they worsen acute shock."),
    ("Give a large saline bolus to a normal blood pressure", False, "Overfilling raises portal pressure; transfuse to a modest target.")],
   [("Hemoglobin", True, "Guides transfusion, usually to about 7 to 8 g/dL."),
    ("Heart rate and BP", True, "Track ongoing bleeding."),
    ("Daily weight on one scale", False, "Not the immediate guide."),
    ("Serum lipase each morning", False, "Not relevant."),
    ("Urine specific gravity", False, "Not the main guide.")],
   "Variceal bleeding: octreotide, antibiotics, restrictive transfusion and urgent banding; track hemoglobin and "
   "hemodynamics.",
   "Manage acute variceal hemorrhage.",
   ex=[note("Emergency department", "A 55-year-old client with alcohol-related cirrhosis vomited about 1 L of bright "
       "red blood. BP 86/50 mm Hg, HR 128/min. Hemoglobin 6.9 g/dL. Platelets 72,000/mm3.", id="var-note")]),

dtable(M(PPT, TPN, "evaluate_outcomes", "difficult", GI, "ph lab"),
   "A client on parenteral nutrition has weekly labs. For each trend, choose the interpretation.",
   ["Lab", "Week 1 to week 3", "Interpretation"],
   [(["Glucose", "128 to 246 mg/dL"], [
       ("Needs follow-up: adjust insulin in the bag or the dextrose", True, "Hyperglycemia raises infection risk."),
       ("Expected and needs no action", False, "Above target."),
       ("Hypoglycemia", False, "It is high.")]),
    (["Triglycerides", "180 to 520 mg/dL"], [
       ("Needs follow-up: reduce the lipid emulsion", True, "Above 400 risks pancreatitis."),
       ("Normal", False, "Too high."),
       ("Improving", False, "Rising.")]),
    (["Prealbumin", "9 to 17 mg/dL"], [
       ("Improving nutrition", True, "A short half-life protein that responds to feeding."),
       ("Worsening", False, "It is rising."),
       ("Unrelated to nutrition", False, "It tracks protein status.")]),
    (["ALT", "30 to 34 units/L"], [
       ("Stable", True, "No liver injury from PN."),
       ("Severe liver injury", False, "Normal."),
       ("Falling", False, "Stable.")])],
   "Track glucose and triglycerides closely on PN; prealbumin shows response.",
   "Evaluate parenteral nutrition labs.",
   ex=[flow("PN labs", ["Week 1", "Week 2", "Week 3"], [
       ("Glucose", ["128", "190", "246 mg/dL"]),
       ("Triglycerides", ["180", "340", "520 mg/dL"]),
       ("Prealbumin", ["9", "13", "17 mg/dL"]),
       ("ALT", ["30", "32", "34 units/L"])], id="pn-trend")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", GI, "mgmt ms"),
   "On a GI surgical unit, the nurse receives these updates at 0800. Which client should the nurse see first?",
   [("A client 1 day after gastric bypass whose heart rate rose from 96 to 128/min, feeling anxious", True,
     "Sustained tachycardia after bypass is the earliest sign of an anastomotic leak."),
    ("A client with a new colostomy whose stoma is red and moist and who has not yet passed any stool", False,
     "Expected early after surgery."),
    ("A client with cirrhosis on lactulose who reports three soft stools since midnight", False,
     "The intended effect."),
    ("A client the day after laparoscopic cholecystectomy who reports right shoulder pain and asks for a heating pad", False,
     "Referred gas pain after laparoscopy is common.")],
   "New tachycardia after bariatric surgery may be the only early sign of a leak.",
   "Prioritize clients on a GI surgical unit."),

sata(M(MOC, DELEG, "take_action", "difficult", GI, "mgmt"),
   "Which tasks can the RN delegate to the unlicensed assistive personnel (UAP) for a client receiving continuous "
   "enteral tube feeding?",
   [("Keep the head of the bed at 30 degrees or higher", True, "A routine positioning task."),
    ("Measure and record the volume of feeding given", True, "Measuring and recording."),
    ("Report coughing or vomiting during the feeding at once", True, "Reporting against instructions."),
    ("Give mouth care", True, "Routine hygiene."),
    ("Check tube placement before restarting the feeding", False, "Assessment by the nurse."),
    ("Give medications through the feeding tube", False, "Medication administration is not delegated to UAP.")],
   "UAPs can position, measure, report and give mouth care; placement checks and medications stay with licensed "
   "nurses.",
   "Delegate tasks for enteral feeding."),
]

# ================================================================================================
# Case 1: Anastomotic leak after gastric bypass, then dumping syndrome
# ================================================================================================
rny_tabs = [
    ("rny-notes", "Nurses' notes", [note("Bariatric unit, postoperative day 1",
        "A 41-year-old client (BMI 48) had a laparoscopic Roux-en-Y gastric bypass yesterday. This morning the client "
        "says, \"Something is wrong,\" and reports new left shoulder pain and pain in the upper abdomen. The client "
        "is breathing fast. Sips of water started at 0600.", id="rny-note")]),
    ("rny-flow", "Vital signs", [flow("Vital signs", ["2200", "0400", "0800"], [
        ("Heart rate", ["92", "108", "128"]),
        ("Respirations", ["18", "22", "28"]),
        ("Temperature", ["37.2", "37.6", "38.3 C"]),
        ("Blood pressure", ["138/82", "132/80", "124/76 mm Hg"]),
        ("Urine output", ["50 mL/h", "35 mL/h", "20 mL/h"])], id="rny-flowsheet")]),
]

rny_qs = [
hl_table(M(RRP, POSTOP, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "0800"],
   [(["Heart rate", "128/min, rising since 2200"], True, "Sustained tachycardia above 120 is the earliest sign of a leak."),
    (["Respirations", "28/min"], True, "Tachypnea."),
    (["Client statement", "\"Something is wrong\""], True, "A sense of doom often precedes collapse."),
    (["Pain", "New left shoulder and upper abdominal pain"], True, "Referred diaphragmatic irritation."),
    (["Urine output", "20 mL/hour"], True, "Falling perfusion."),
    (["Blood pressure", "124/76 mm Hg"], False, "Still normal; pressure falls late.")],
   "Rising heart rate, tachypnea, new pain, low urine and a sense of doom suggest an anastomotic leak.",
   "Recognize cues of a leak after gastric bypass."),

cloze(M(RRP, POSTOP, "analyze_cues", "difficult", GI, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "These findings most likely mean {{b1}}. In clients with severe obesity, abdominal signs are often {{b2}}, so "
   "{{b3}} is the most reliable early warning.",
   [[("an anastomotic leak with early sepsis", True, "Gastric contents leak into the abdomen."),
     ("normal postoperative pain", False, "Rising heart rate and falling urine are not normal."),
     ("a pulmonary embolism only", False, "Possible, but the pain pattern and fever suggest a leak."),
     ("anxiety", False, "Objective signs are present.")],
    [("hard to detect", True, "Body habitus masks guarding and rigidity."),
     ("exaggerated", False, "The opposite."),
     ("always present", False, "Often absent."),
     ("only on the right", False, "Not the issue.")],
    [("sustained tachycardia", True, "Heart rate above 120 for several hours."),
     ("blood pressure", False, "Falls late."),
     ("temperature alone", False, "Can be mild early."),
     ("bowel sounds", False, "Not reliable.")]],
   "Tachycardia is the key early sign of a leak because the abdomen hides signs.",
   "Analyze early signs of anastomotic leak."),

matrix_mc(M(RRP, POSTOP, "prioritize_hypotheses", "difficult", GI, "ms"),
   "For each possible cause, indicate how likely it is.",
   ["Most likely", "Possible, check", "Unlikely"],
   [("Anastomotic leak", 0, "Classic early signs."),
    ("Pulmonary embolism", 1, "High risk after bariatric surgery; may be checked with CT."),
    ("Bowel obstruction", 2, "Less likely on day 1."),
    ("Dumping syndrome", 2, "Occurs after eating, weeks later.")],
   "A leak is most likely; PE should also be considered.",
   "Prioritize causes after bariatric surgery."),

grouping(M(SIC, HANDLING, "generate_solutions", "difficult", GI, "fund mgmt"),
   "The client will go for a CT and possibly back to surgery. For each area, select the safe actions.",
   [("Moving the client", [
       ("Use a lateral transfer device and enough staff", True, "Prevents injury to client and staff."),
       ("Use a bed and stretcher rated for the client's weight", True, "Equipment must be rated correctly."),
       ("Have two staff lift the client by the arms", False, "Unsafe for client and staff.")]),
    ("Preparation", [
       ("Keep NPO and stop the sips of water", True, "Possible surgery."),
       ("Use an appropriately sized blood pressure cuff", True, "A small cuff gives falsely high readings."),
       ("Wait for the CT before calling the surgeon", False, "The surgeon is called now.")])],
   "Safe handling equipment, correct sizes and prompt escalation keep the client and staff safe.",
   "Generate safe handling and preparation measures.",
   ex=[note("0815", "The surgeon orders a CT with oral contrast and IV antibiotics; the OR is on standby.",
       id="rny-orders")]),

sata(M(BCC, NUTR, "take_action", "difficult", GI, "ms"),
   "The leak was repaired and the client was discharged. At 4 weeks, the client reports cramping, diarrhea, a "
   "racing heart and dizziness 20 minutes after meals. Which teaching should the nurse give?",
   [("Eat six small meals rich in protein", True, "Slows gastric emptying effects."),
    ("Avoid sweets and sugary drinks", True, "Simple sugars trigger dumping."),
    ("Wait 30 minutes after meals before drinking fluids", True, "Fluids with meals speed food into the bowel."),
    ("Drink a large glass of juice with each meal", False, "Worsens dumping."),
    ("Eat a high-carbohydrate snack before bed", False, "Can cause late dumping and hypoglycemia.")],
   "Early dumping is prevented with small protein meals, avoiding sugars and separating fluids from meals.",
   "Take action for dumping syndrome.",
   ex=[note("4-week visit", "Weight down 11 kg. Symptoms after lunch, especially with juice.", id="rny-4wk")]),

matrix_mc(M(HPM, "health-teaching-and-behavior-change", "evaluate_outcomes", "difficult", GI, "ms ph"),
   "At 3 months, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I take my vitamin B12, iron and calcium citrate every day.\"", 0, "Lifelong supplements are needed."),
    ("\"I'll use acetaminophen, not ibuprofen, for headaches.\"", 0, "NSAIDs cause marginal ulcers."),
    ("\"I'll get my labs checked as scheduled.\"", 0, "Monitors deficiencies."),
    ("\"I take my calcium together with my iron.\"", 1, "Calcium blocks iron absorption; separate them."),
    ("\"I'll eat protein first at each meal.\"", 0, "Supports healing and muscle."),
    ("\"I feel shaky 2 hours after a big pasta meal, so I'll eat more pasta.\"", 1, "Late dumping hypoglycemia; reduce simple carbohydrates.")],
   "Lifelong supplements, NSAID avoidance, protein first and recognizing late dumping.",
   "Evaluate bariatric surgery teaching.",
   ex=[note("3-month visit", "Weight down 24 kg. Hemoglobin 11.8 g/dL.", id="rny-3mo")]),
]

# ================================================================================================
# Case 2: Acetaminophen overdose
# ================================================================================================
apap_tabs = [
    ("apap-notes", "Nurses' notes", [note("Emergency department, 0300",
        "A 19-year-old client says they swallowed \"about 40\" acetaminophen 500 mg tablets at about 2300 last night "
        "after a breakup. The client has mild nausea and otherwise feels fine. Weight 60 kg. The client says, \"It "
        "doesn't matter what happens to me.\"", id="apap-note")]),
    ("apap-labs", "Labs", [table("Labs", ["0300 (4 hours after ingestion)", "Reference"], [
        ("Acetaminophen level", ["260 mcg/mL", "treatment line at 4 hours: 150"]),
        ("ALT", ["38 units/L", "7 to 56"]),
        ("INR", ["1.0", "0.8 to 1.1"]),
        ("Glucose", ["96 mg/dL", "-"]),
        ("Salicylate and ethanol", ["Negative", "-"])], id="apap-table")]),
]

apap_qs = [
hl_table(M(PA, POISON, "recognize_cues", "moderate", GI, "ph"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Acetaminophen level at 4 hours", "260 mcg/mL"], True, "Above the treatment line."),
    (["Estimated dose", "About 20 g (333 mg/kg)"], True, "A potentially fatal dose."),
    (["Statement", "\"It doesn't matter what happens to me.\""], True, "Ongoing suicide risk."),
    (["ALT / INR", "Normal"], False, "Normal early; liver injury appears 24 to 72 hours later."),
    (["Symptoms", "Mild nausea only"], False, "Misleading: early acetaminophen poisoning causes few symptoms.")],
   "A level above the treatment line and a large dose need treatment even though the client looks well.",
   "Recognize cues of acetaminophen poisoning."),

matrix_mc(M(PA, POISON, "analyze_cues", "difficult", GI, "ph"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Normal liver tests now rule out serious poisoning", 1, "Injury peaks at 72 to 96 hours."),
    ("Acetylcysteine works best when started within 8 hours", 0, "It replenishes glutathione before injury."),
    ("The toxic metabolite is NAPQI", 0, "Normally neutralized by glutathione."),
    ("Activated charcoal is most useful within about 1 to 2 hours", 0, "Later, it adds less."),
    ("The 4-hour level is plotted on a nomogram to decide treatment", 0, "Levels before 4 hours are not reliable.")],
   "Acetaminophen poisoning looks mild early; the 4-hour level and timing decide treatment.",
   "Analyze acetaminophen toxicity."),

cloze(M(PSI, SUICIDE, "prioritize_hypotheses", "difficult", GI, "psych ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Alongside the antidote, the nurse's priority is {{b1}}, because {{b2}}.",
   [[("continuous one-to-one observation and a safe environment", True, "The client remains at risk of self-harm."),
     ("discharge once the antidote finishes", False, "A psychiatric assessment is needed first."),
     ("asking the family to leave", False, "Family support is useful unless unsafe."),
     ("restraints", False, "Not the first choice.")],
    [("the client expresses hopelessness after an intentional overdose", True, "A high-risk statement."),
     ("the ALT is normal", False, "Not the reason."),
     ("the client is 19", False, "Age is a factor, but the statement drives the priority."),
     ("acetylcysteine causes drowsiness", False, "Not the reason.")]],
   "After an intentional overdose, suicide precautions run alongside medical treatment.",
   "Prioritize safety after intentional overdose."),

calc(M(PPT, CALC, "generate_solutions", "difficult", GI, "ph"),
   "IV acetylcysteine is prescribed: 150 mg/kg over 1 hour, then 50 mg/kg over 4 hours, then 100 mg/kg over 16 "
   "hours. The client weighs 60 kg. How many milligrams are in the first (loading) dose? Record your answer as a "
   "whole number.",
   "mg", 0, "Loading dose", 9000,
   "150 mg/kg x 60 kg = 9,000 mg over 1 hour. The total 21-hour course is 300 mg/kg, or 18,000 mg.",
   "Calculate an acetylcysteine loading dose.",
   ex=[note("Orders, 0315", "Acetylcysteine IV 21-hour protocol. One-to-one observation. Psychiatry consult. "
       "Recheck acetaminophen level, ALT and INR at the end of the infusion.", id="apap-orders")]),

sata(M(PPT, ANTIDOTE, "take_action", "difficult", GI, "ph"),
   "Twenty minutes into the loading dose, the client develops flushing, itching and wheezing. Which actions should "
   "the nurse take?",
   [("Stop the infusion temporarily", True, "A reaction to acetylcysteine, most common in the loading dose."),
    ("Give the prescribed antihistamine and bronchodilator", True, "Treats the reaction."),
    ("Notify the provider", True, "To plan restarting."),
    ("Restart at a slower rate once symptoms resolve, as prescribed", True, "Usually acetylcysteine is continued."),
    ("Discontinue acetylcysteine permanently", False, "The benefit outweighs the reaction risk."),
    ("Leave the client alone to rest during the reaction", False, "One-to-one observation continues.")],
   "Acetylcysteine reactions are treated and the infusion restarted more slowly; it is rarely abandoned.",
   "Take action for an acetylcysteine reaction.",
   ex=[flow("0335", ["0335"], [
       ("Skin", ["Flushed, itchy"]),
       ("Breathing", ["Mild wheeze"]),
       ("Blood pressure", ["118/72 mm Hg"]),
       ("SpO2", ["96%"])], id="apap-0335")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", GI, "ph mgmt"),
   "Review the findings on day 2. For each finding, indicate whether it is expected or needs follow-up.",
   ["Expected or improving", "Needs follow-up"],
   [("Acetaminophen level undetectable", 0, "Cleared."),
    ("ALT 4,200 units/L, INR 3.2", 1, "Severe liver injury; continue acetylcysteine and involve hepatology."),
    ("Glucose 58 mg/dL", 1, "The failing liver cannot keep glucose up."),
    ("New confusion", 1, "Possible hepatic encephalopathy: consider transplant referral."),
    ("Psychiatry has assessed the client", 0, "Collaboration in place."),
    ("One-to-one observation continuing", 0, "Safety maintained.")],
   "Severe liver injury with failing synthesis and encephalopathy needs a transplant center referral.",
   "Evaluate outcomes after acetaminophen poisoning.",
   ex=[flow("Day 2", ["Day 2"], [
       ("Acetaminophen level", ["Undetectable"]),
       ("ALT / INR", ["4,200 units/L / 3.2"]),
       ("Glucose", ["58 mg/dL"]),
       ("Mental status", ["New confusion"]),
       ("Consults", ["Psychiatry done"]),
       ("Observation", ["One-to-one"])], id="apap-day2")]),
]

# ================================================================================================
# Case 3: Refeeding syndrome on parenteral nutrition
# ================================================================================================
ref_tabs = [
    ("ref-notes", "Nurses' notes", [note("Medical unit",
        "A 48-year-old client with alcohol use disorder has eaten almost nothing for 3 weeks. BMI 16. A central "
        "catheter was placed and parenteral nutrition was started yesterday at the full goal rate. Today the client "
        "is weak, short of breath and confused, with new ankle swelling.", id="ref-note")]),
    ("ref-labs", "Labs", [table("Labs", ["Before PN", "Day 2", "Reference"], [
        ("Phosphate", ["2.9", "0.9 mg/dL", "2.5 to 4.5"]),
        ("Potassium", ["3.6", "2.8 mEq/L", "3.5 to 5.0"]),
        ("Magnesium", ["1.7", "1.1 mg/dL", "1.5 to 2.5"]),
        ("Glucose", ["82", "286 mg/dL", "70 to 140"]),
        ("Thiamine given", ["No", "No", "-"])], id="ref-table")]),
]

ref_qs = [
hl_table(M(PPT, TPN, "recognize_cues", "moderate", GI, "ph"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Day 2"],
   [(["Phosphate", "0.9 mg/dL"], True, "Severe hypophosphatemia, the hallmark of refeeding syndrome."),
    (["Potassium / magnesium", "2.8 / 1.1"], True, "Shifted into cells with feeding."),
    (["Mental status", "New confusion"], True, "Possible Wernicke encephalopathy without thiamine."),
    (["Breathing and edema", "Short of breath, ankle swelling"], True, "Fluid retention and heart failure."),
    (["Glucose", "286 mg/dL"], True, "Hyperglycemia."),
    (["BMI", "16"], False, "The underlying risk, not a new finding.")],
   "Falling phosphate, potassium and magnesium with confusion, edema and hyperglycemia after starting full feeding.",
   "Recognize cues of refeeding syndrome."),

cloze(M(PPT, TPN, "analyze_cues", "difficult", GI, "ph ms"),
   "Complete the sentences by choosing from the lists of options.",
   "Refeeding syndrome happens because {{b1}}. The confusion may be {{b2}}, which is prevented by {{b3}} before "
   "feeding.",
   [[("insulin released by carbohydrate drives phosphate, potassium and magnesium into cells", True, "Stores are already depleted."),
     ("the PN contains too much phosphate", False, "Not the cause."),
     ("the kidneys stop working", False, "Not the mechanism."),
     ("the central line is infected", False, "Not the mechanism.")],
    [("Wernicke encephalopathy", True, "Glucose uses up thiamine in a depleted client."),
     ("hepatic encephalopathy", False, "Possible, but no thiamine was given."),
     ("normal sleepiness", False, "Not normal."),
     ("hypernatremia", False, "Not shown.")],
    [("IV thiamine", True, "Given before glucose in at-risk clients."),
     ("vitamin K", False, "Not relevant."),
     ("insulin", False, "Not prevention."),
     ("folic acid alone", False, "Does not prevent Wernicke.")]],
   "Feeding shifts electrolytes into cells and uses up thiamine.",
   "Analyze refeeding syndrome."),

matrix_mc(M(PPT, TPN, "prioritize_hypotheses", "difficult", GI, "ph"),
   "For each risk, indicate its priority now.",
   ["High priority", "Lower priority"],
   [("Cardiac dysrhythmia and heart failure", 0, "Low potassium, magnesium and phosphate with fluid overload."),
    ("Respiratory muscle weakness", 0, "Low phosphate impairs the diaphragm."),
    ("Permanent brain injury from thiamine deficiency", 0, "Needs thiamine at once."),
    ("Long-term weight regain", 1, "Later goal.")],
   "Heart, breathing and brain injury are the immediate risks.",
   "Prioritize risks in refeeding syndrome."),

matrix_mc(M(PPT, TPN, "generate_solutions", "difficult", GI, "ph"),
   "For each order, indicate whether it is indicated or contraindicated.",
   ["Indicated", "Contraindicated"],
   [("Reduce PN to about half the goal rate, then increase slowly", 0, "Slower refeeding."),
    ("IV thiamine now and daily", 0, "Treats and prevents Wernicke."),
    ("IV phosphate, potassium and magnesium replacement", 0, "Corrects deficits."),
    ("Electrolytes every 6 to 12 hours for the first days", 0, "Close monitoring."),
    ("Increase PN to the full rate to correct malnutrition faster", 1, "Worsens refeeding."),
    ("Add the IV potassium directly into the running PN bag on the unit", 1, "Never add to PN outside pharmacy.")],
   "Slow the feed, give thiamine, replace electrolytes and monitor closely.",
   "Generate solutions for refeeding syndrome.",
   ex=[note("Orders", "PN to 50% of goal. Thiamine 200 mg IV three times daily. Sodium phosphate IV. Potassium and "
       "magnesium IV. Cardiac monitor.", id="ref-orders")]),

grouping(M(SIC, "healthcare-associated-infection-prevention", "take_action", "difficult", GI, "ph cdc"),
   "For each area of PN care, select the safe actions.",
   [("The line", [
       ("Use a dedicated lumen for PN", True, "Reduces contamination and incompatibility."),
       ("Scrub the hub before every access", True, "Prevents bloodstream infection."),
       ("Draw blood from the PN lumen to save sticks", False, "Contaminates the line.")]),
    ("The bag and tubing", [
       ("Use the prescribed in-line filter", True, "Removes particles."),
       ("Change the tubing with each new bag, at least every 24 hours", True, "Infection prevention."),
       ("Hang a bag that was out of the fridge for 30 hours", False, "Discard after the safe time.")]),
    ("If the bag runs out early", [
       ("Hang 10% dextrose at the same rate until the new bag arrives", True, "Prevents rebound hypoglycemia."),
       ("Run normal saline only", False, "Risks hypoglycemia.")])],
   "PN safety covers a dedicated scrubbed lumen, a filter, tubing changes, bag timing and dextrose cover.",
   "Take action to give parenteral nutrition safely.",
   ex=[note("Day 3", "The night bag ran out 2 hours early; the new bag will come from pharmacy at 0900.",
       id="ref-day3")]),

matrix_mc(M(PPT, TPN, "evaluate_outcomes", "difficult", GI, "ph"),
   "Review the findings on day 5. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Phosphate 2.8 mg/dL, potassium 4.0 mEq/L", 0, "Corrected."),
    ("Oriented x3", 0, "Confusion resolved with thiamine."),
    ("Edema decreasing, breathing easier", 0, "Fluid overload improving."),
    ("Temperature 38.7 C (101.7 F) with chills when the line is flushed", 1, "Possible line infection: cultures and provider."),
    ("Glucose 140 to 180 mg/dL", 0, "Controlled."),
    ("Eating small amounts of food", 0, "Moving toward oral nutrition.")],
   "Refeeding is corrected; fever with chills on flushing suggests a central line infection.",
   "Evaluate outcomes on parenteral nutrition.",
   ex=[flow("Day 5", ["Day 5"], [
       ("Phosphate / potassium", ["2.8 mg/dL / 4.0 mEq/L"]),
       ("Mental status", ["Oriented x3"]),
       ("Edema", ["Decreasing"]),
       ("Temperature", ["38.7 C, chills when the line is flushed"]),
       ("Glucose", ["140 to 180 mg/dL"]),
       ("Intake", ["Small amounts of food"])], id="ref-day5")]),
]

# ================================================================================================
# Case 4: Esophageal perforation after endoscopic dilation
# ================================================================================================
egd_tabs = [
    ("egd-notes", "Nurses' notes", [note("Endoscopy recovery, 1100",
        "A 66-year-old client had an upper endoscopy with balloon dilation of an esophageal stricture at 0930 under "
        "sedation. At 1100 the client reports severe pain behind the breastbone that goes to the back, and pain on "
        "swallowing. There is a crackling feeling under the skin of the neck when pressed.", id="egd-note")]),
    ("egd-data", "Vital signs and ECG", [table("Vital signs and ECG", ["0930", "1100"], [
        ("Heart rate", ["78/min", "118/min"]),
        ("Temperature", ["36.8 C", "38.2 C (100.8 F)"]),
        ("Blood pressure", ["134/80", "126/74 mm Hg"]),
        ("SpO2", ["98%", "94%"]),
        ("12-lead ECG", ["-", "Sinus tachycardia, no ST changes"])], id="egd-table")]),
]

egd_qs = [
hl_table(M(RRP, GI_DX, "recognize_cues", "moderate", GI, "ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "1100"],
   [(["Pain", "Severe chest pain to the back after dilation"], True, "Suggests perforation."),
    (["Neck", "Crackling under the skin"], True, "Subcutaneous emphysema from air leaking from the esophagus."),
    (["Heart rate / temperature", "118/min; 38.2 C"], True, "Early mediastinitis."),
    (["Swallowing", "Painful"], True, "Esophageal injury."),
    (["ECG", "No ST changes"], False, "Helps rule out MI; not a cause for action itself.")],
   "Chest pain, neck crepitus, fever and tachycardia after dilation suggest esophageal perforation.",
   "Recognize cues of esophageal perforation."),

matrix_mc(M(RRP, GI_DX, "analyze_cues", "difficult", GI, "ms"),
   "For each finding, indicate whether it supports esophageal perforation or acute coronary syndrome.",
   ["Esophageal perforation", "Acute coronary syndrome"],
   [("Pain starting right after esophageal dilation", 0, "Timing points to the procedure."),
    ("Subcutaneous emphysema in the neck", 0, "Air from a torn esophagus."),
    ("ST elevation in two adjacent leads", 1, "Myocardial infarction."),
    ("Fever developing within hours", 0, "Mediastinal infection."),
    ("Pain relieved by sublingual nitroglycerin", 1, "Suggests ischemia, though esophageal spasm can also respond.")],
   "Timing, crepitus and fever point to perforation; the ECG helps exclude ACS.",
   "Analyze chest pain after endoscopy."),

cloze(M(RRP, GI_DX, "prioritize_hypotheses", "difficult", GI, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client most likely has {{b1}}, and the main danger is {{b2}}.",
   [[("a perforated esophagus", True, "A known risk of dilation."),
     ("a myocardial infarction", False, "ECG is unchanged."),
     ("aspiration pneumonia", False, "Possible, but crepitus points to perforation."),
     ("a sedation reaction", False, "Not supported.")],
    [("mediastinitis and sepsis", True, "Gastric and oral contents in the mediastinum."),
     ("hypoglycemia", False, "Not the danger."),
     ("a stroke", False, "Not the danger."),
     ("constipation", False, "Not the danger.")]],
   "Perforation can lead quickly to mediastinitis and sepsis.",
   "Prioritize esophageal perforation."),

sata(M(RRP, GI_DX, "generate_solutions", "difficult", GI, "ms ph"),
   "Which orders should the nurse anticipate?",
   [("NPO, including no oral medications", True, "Prevents leakage."),
    ("Chest CT with water-soluble contrast", True, "Confirms the leak."),
    ("Broad-spectrum IV antibiotics", True, "Treats contamination."),
    ("Thoracic surgery consult", True, "Repair or stent."),
    ("A barium swallow study this afternoon", False, "Barium in the mediastinum causes inflammation."),
    ("Clear liquids by mouth to test the swallowing", False, "Unsafe.")],
   "NPO, imaging with water-soluble contrast, antibiotics and specialist repair.",
   "Generate solutions for esophageal perforation.",
   ex=[note("1115", "Gastroenterologist notified; considering an endoscopic stent.", id="egd-1115")]),

sata(M(RRP, GI_DX, "take_action", "difficult", GI, "ms"),
   "While awaiting the CT, review the findings. Which actions should the nurse take?",
   [("Raise the head of the bed", True, "Eases breathing and reduces reflux."),
    ("Give oxygen to keep SpO2 at 92% or higher", True, "Supports oxygenation."),
    ("Report the falling SpO2 and new left-sided breath sound changes", True, "Possible pleural effusion or pneumothorax."),
    ("Start IV fluids and antibiotics as prescribed", True, "Sepsis care."),
    ("Give the client ice chips for the pain", False, "Nothing by mouth."),
    ("Send the client home to wait for results", False, "Unsafe.")],
   "Support breathing, report new chest findings and start sepsis care while imaging is arranged.",
   "Take action while awaiting imaging for perforation.",
   ex=[flow("1145", ["1145"], [
       ("SpO2", ["91% on room air"]),
       ("Breath sounds", ["Diminished at the left base"]),
       ("Heart rate", ["124/min"]),
       ("Pain", ["8 of 10"])], id="egd-1145")]),

matrix_mc(M(MOC, "quality-improvement-and-incident-reporting", "evaluate_outcomes", "difficult", GI, "mgmt ms"),
   "After an endoscopic stent and antibiotics, the client recovers. For each finding, indicate whether it is "
   "appropriate or needs follow-up.",
   ["Appropriate", "Needs follow-up"],
   [("Afebrile on day 4; leak sealed on repeat imaging", 0, "Successful treatment."),
    ("The recovery nurse's early report of pain led to rapid imaging", 0, "Timely escalation."),
    ("No incident or adverse event report filed", 1, "Serious procedure complications are reported for review."),
    ("The client was told about the complication by the gastroenterologist", 0, "Disclosure is appropriate."),
    ("A plan for the stent removal date is not documented", 1, "Stents must be removed or reviewed on time.")],
   "Recovery and disclosure are appropriate; event reporting and stent follow-up need completion.",
   "Evaluate quality and follow-up after a procedure complication.",
   ex=[note("Day 5", "Starting liquids by mouth.", id="egd-day5")]),
]

cases = [
    case("Leak after gastric bypass, then dumping syndrome", rny_tabs, rny_qs),
    case("Acetaminophen overdose", apap_tabs, apap_qs),
    case("Refeeding syndrome on parenteral nutrition", ref_tabs, ref_qs),
    case("Esophageal perforation after dilation", egd_tabs, egd_qs),
]

BUILT = build(115, "gi-cases-bariatric-apap-refeeding-egd-standalone",
              "Gastrointestinal case studies: bariatric leak and dumping, acetaminophen overdose, refeeding syndrome "
              "and esophageal perforation, with four standalone items",
              items, cases, case_conversion=True)
