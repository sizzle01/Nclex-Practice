"""Batch 138 (case conversion, chunk 10: maternity): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: newborn assessment in the first hours with Apgar scoring, thermoregulation and routine medications;
pathologic newborn jaundice from ABO incompatibility with phototherapy; neonatal opioid withdrawal syndrome; postpartum
endometritis and mastitis while breastfeeding. Standalone: a neonatal sepsis bowtie, a postpartum fundus and lochia
trend, a priority question on a labor and delivery unit and a delegation question on a postpartum unit. Each case
walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety
or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
NEWBORN = "newborn-care"
NEWBORN_COMP = "newborn-complications"
POSTPARTUM = "postpartum-care"
BREASTFEEDING = "breastfeeding"
OB_MEDS = "obstetric-medications"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
ID_ERR = "client-identification-and-error-prevention"
ABX = "antimicrobial-therapy"
SUBSTANCE = "substance-use-disorders"
TEACH = "health-teaching-and-behavior-change"
CALC = "medication-dose-calculations"
LABS = "laboratory-values-and-monitoring"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, NEWBORN_COMP, "prioritize_hypotheses", "difficult", OB, "ob peds"),
   "A newborn in the nursery has new findings at 18 hours of age. Using the note, fill in the bowtie: the most "
   "likely condition in the middle, two actions to take now on the left and two parameters to monitor on the right.",
   [("Early-onset neonatal sepsis", True, "GBS-positive mother without adequate prophylaxis, temperature instability and poor feeding."),
    ("Physiologic jaundice", False, "Does not explain temperature instability and grunting."),
    ("Normal transition", False, "Transition is complete by 18 hours."),
    ("Neonatal abstinence syndrome", False, "No substance exposure.")],
   [("Notify the provider and obtain a blood culture as ordered", True, "Sepsis workup."),
    ("Start IV ampicillin and gentamicin as prescribed", True, "Empiric therapy."),
    ("Wait for the culture result before antibiotics", False, "Delays treatment."),
    ("Give a bottle of glucose water and observe", False, "Not enough."),
    ("Bathe the newborn to lower the temperature", False, "Hypothermia, not fever.")],
   [("Temperature and respiratory effort", True, "Instability."),
    ("Blood glucose", True, "Sepsis causes hypoglycemia."),
    ("Weekly head circumference", False, "Not urgent."),
    ("Stool color", False, "Not urgent."),
    ("Hearing screen result", False, "Not related.")],
   "A GBS-exposed newborn with hypothermia, grunting and poor feeding needs a sepsis workup and antibiotics.",
   "Recognize and act on neonatal sepsis.",
   ex=[note("Nursery, 18 hours", "Mother GBS positive; antibiotic given 2 hours before birth (less than the 4 hours "
       "needed). Newborn temperature 36.1 C axillary, grunting, RR 72/min, sleepy, not latching. Glucose 42 mg/dL.",
       id="sa-gbs")]),

dtable(M(HPM, POSTPARTUM, "evaluate_outcomes", "difficult", OB, "ob"),
   "A client is on day 1 after a vaginal birth. For each check, choose the interpretation.",
   ["Time", "Fundus and lochia", "Interpretation"],
   [(["Hour 1", "Firm, at the umbilicus; moderate rubra"], [
       ("Expected", True, "Normal involution."),
       ("Hemorrhage", False, "Not heavy."),
       ("Infection", False, "No signs.")]),
    (["Hour 8", "Boggy, 2 cm above and to the right; heavy rubra"], [
       ("Likely a full bladder; massage and help the client void", True, "Displaced, boggy uterus."),
       ("Expected", False, "Not normal."),
       ("Retained placenta for certain", False, "Check bladder first.")]),
    (["Day 4", "1 cm below umbilicus each day; pinkish-brown serosa"], [
       ("Expected", True, "Normal progression."),
       ("Subinvolution", False, "Descending normally."),
       ("Hemorrhage", False, "No.")])],
   "The fundus descends about 1 cm a day; a displaced boggy fundus suggests a full bladder.",
   "Evaluate a postpartum fundus and lochia trend.",
   ex=[flow("Postpartum checks", ["Hour 1", "Hour 8", "Day 4"], [
       ("Fundus", ["Firm at U", "Boggy, U+2, right", "U-4, firm"]),
       ("Lochia", ["Moderate rubra", "Heavy rubra", "Serosa"])], id="sa-fundus")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", OB, "mgmt ob"),
   "On a labor and delivery unit, four clients call at once. Which client should the nurse see first?",
   [("A client at 39 weeks in early labor who wants an epidural now", False, "Can be arranged shortly."),
    ("A client on magnesium for preeclampsia whose respiratory rate is 10/min and reflexes are absent", True,
     "Magnesium toxicity with respiratory depression is life-threatening."),
    ("A client 2 hours after birth whose fundus is firm and who is asking for a meal", False, "Stable."),
    ("A client at 40 weeks whose membranes ruptured with clear fluid and a reassuring FHR", False,
     "Needs assessment soon but is stable.")],
   "Respiratory depression with absent reflexes on magnesium is the priority.",
   "Prioritize clients on a labor and delivery unit."),

sata(M(MOC, DELEG, "take_action", "difficult", OB, "mgmt ob"),
   "The RN has four mother-baby couplets this morning and a UAP to help. Which tasks are appropriate to hand to the UAP?",
   [("Measure vital signs on a stable client on day 1", True, "Routine."),
    ("Help a client to the bathroom for the first time with the RN's guidance", True, "With instruction on fall risk."),
    ("Change perineal pads and report heavy bleeding", True, "Reporting."),
    ("Assess the fundus and lochia of a client who just had a hemorrhage", False, "Assessment of an unstable client."),
    ("Teach a first-time mother how to latch the baby", False, "Teaching stays with the RN."),
    ("Give the Rho(D) immune globulin injection", False, "Medication.")],
   "UAPs measure, help and report; assessment, teaching and medicines stay with the RN.",
   "Delegate on a postpartum unit."),
]

# ================================================================================================
# Case 1: Newborn assessment and routine medications
# ================================================================================================
nb_tabs = [
    ("nb-notes", "Delivery room notes", [note("Birth at 39 weeks",
        "A term newborn is born vaginally to a mother who is hepatitis B surface antigen negative. At 1 minute the "
        "newborn is placed skin to skin.", id="nb-note")]),
    ("nb-data", "1-minute assessment", [table("1 minute", ["Finding"], [
        ("Heart rate", ["130/min"]),
        ("Respiratory effort", ["Slow, irregular"]),
        ("Muscle tone", ["Some flexion of the limbs"]),
        ("Reflex irritability", ["Grimace when suctioned"]),
        ("Color", ["Body pink, hands and feet blue"]),
        ("Temperature (axillary)", ["36.2 C"])], id="nb-table")]),
]

nb_qs = [
hl_table(M(HPM, NEWBORN, "recognize_cues", "moderate", OB, "ob peds"),
   "Highlight the 1-minute findings that need action now.",
   ["Finding", "1 minute"],
   [(["Respiratory effort", "Slow, irregular"], True, "Stimulate and dry."),
    (["Temperature", "36.2 C"], True, "Cold stress risk."),
    (["Heart rate", "130/min"], False, "Normal."),
    (["Acrocyanosis", "Hands and feet blue"], False, "Normal in the first hours."),
    (["Tone", "Some flexion"], False, "Scores 1.")],
   "Breathing effort and temperature need attention; acrocyanosis is normal.",
   "Recognize cues at birth."),

dtable(M(HPM, NEWBORN, "analyze_cues", "difficult", OB, "ob peds"),
   "Score each Apgar sign at 1 minute.",
   ["Sign", "Finding", "Score"],
   [(["Heart rate", "130/min"], [("2", True, "Above 100."), ("1", False, "Below 100."), ("0", False, "Absent.")]),
    (["Respiratory effort", "Slow, irregular"], [("1", True, "Weak."), ("2", False, "Strong cry."), ("0", False, "Absent.")]),
    (["Muscle tone", "Some flexion"], [("1", True, "Some flexion."), ("2", False, "Active motion."), ("0", False, "Limp.")]),
    (["Reflex irritability", "Grimace"], [("1", True, "Grimace."), ("2", False, "Cry."), ("0", False, "None.")]),
    (["Color", "Acrocyanosis"], [("1", True, "Body pink, extremities blue."), ("2", False, "All pink."), ("0", False, "Pale or blue.")])],
   "Total Apgar 6 at 1 minute: needs stimulation and reassessment at 5 minutes.",
   "Analyze the Apgar score.",
   ex=[note("1 minute", "Scoring now.", id="nb-apgar")]),

cloze(M(HPM, NEWBORN, "prioritize_hypotheses", "difficult", OB, "ob peds"),
   "Complete the sentence by choosing from the lists of options.",
   "With an Apgar of 6, the nurse should {{b1}}, because cold stress can cause {{b2}}.",
   [[("dry, stimulate, keep skin to skin under a warm blanket and reassess at 5 minutes", True, "Routine support."),
     ("start chest compressions", False, "Heart rate is normal."),
     ("bathe the newborn now", False, "Heat loss."),
     ("place the newborn alone in an open crib", False, "Heat loss.")],
    [("hypoglycemia, respiratory distress and acidosis", True, "Brown fat use raises oxygen and glucose demand."),
     ("jaundice only", False, "Not the main effect."),
     ("hyperthermia", False, "Opposite."),
     ("dehydration only", False, "Not the main effect.")]],
   "Warm, dry, stimulate and reassess.",
   "Prioritize newborn transition."),

matrix_mc(M(PPT, OB_MEDS, "generate_solutions", "difficult", OB, "ph peds"),
   "For each routine newborn medicine, indicate its purpose.",
   ["Prevents bleeding", "Prevents eye infection", "Prevents liver infection"],
   [("Vitamin K (phytonadione) 1 mg IM in the vastus lateralis", 0, "Newborns lack gut bacteria that make vitamin K."),
    ("Erythromycin eye ointment", 1, "Gonococcal ophthalmia."),
    ("Hepatitis B vaccine within 24 hours", 2, "Hepatitis B.")],
   "Routine medicines prevent bleeding, eye infection and hepatitis B.",
   "Generate routine newborn medicines.",
   ex=[note("5 minutes", "Apgar 9. Crying, pink body.", id="nb-5min")]),

sata(M(SIC, ID_ERR, "take_action", "difficult", OB, "peds"),
   "Which actions keep the newborn safe in the first hours?",
   [("Apply matching ID bands to the newborn and mother before leaving the room", True, "Identification."),
    ("Apply the security tag", True, "Abduction prevention."),
    ("Teach the parents to give the baby only to staff wearing the unit badge", True, "Abduction prevention."),
    ("Place the newborn on the back to sleep", True, "Safe sleep."),
    ("Let a visitor carry the baby in the hallway", False, "Abduction risk."),
    ("Put a blanket and stuffed toy in the crib", False, "SIDS risk.")],
   "Identification, security and safe sleep.",
   "Take action for newborn safety.",
   ex=[note("1 hour", "Breastfed for 20 minutes.", id="nb-1h")]),

matrix_mc(M(HPM, NEWBORN, "evaluate_outcomes", "difficult", OB, "peds"),
   "At 24 hours, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Passed meconium at 12 hours", 0, "Normal."),
    ("Weight down 4% from birth", 0, "Up to 7 to 10% is normal."),
    ("Yellow skin at 18 hours", 1, "Jaundice before 24 hours is pathologic."),
    ("Bluish spots on the lower back", 0, "Congenital dermal melanocytosis."),
    ("Respiratory rate 48/min", 0, "Normal 30 to 60."),
    ("Temperature 36.0 C axillary", 1, "Low.")],
   "Early jaundice and low temperature need follow-up.",
   "Evaluate the newborn at 24 hours.",
   ex=[note("24 hours", "Breastfeeding every 2 to 3 hours.", id="nb-24h")]),
]

# ================================================================================================
# Case 2: Pathologic jaundice from ABO incompatibility
# ================================================================================================
jau_tabs = [
    ("jau-notes", "Nursery notes", [note("Nursery, 20 hours",
        "A term newborn of an O positive mother looked yellow at 18 hours. The newborn is breastfeeding every 3 "
        "hours but is sleepy.", id="jau-note")]),
    ("jau-data", "Results", [table("Results", ["Result"], [
        ("Total serum bilirubin", ["11.2 mg/dL at 20 hours (high-risk zone)"]),
        ("Newborn blood type", ["A positive"]),
        ("Direct Coombs", ["Positive"]),
        ("Hemoglobin", ["13.1 g/dL"]),
        ("Reticulocytes", ["High"]),
        ("Wet diapers", ["2 in 20 hours"])], id="jau-table")]),
]

jau_qs = [
hl_table(M(PA, NEWBORN_COMP, "recognize_cues", "moderate", OB, "peds"),
   "Highlight the findings that make this jaundice pathologic.",
   ["Finding", "Result"],
   [(["Onset", "Before 24 hours"], True, "Pathologic."),
    (["Bilirubin", "11.2 at 20 hours"], True, "High-risk zone."),
    (["Coombs", "Positive"], True, "Immune hemolysis."),
    (["Blood types", "Mother O, baby A"], True, "ABO incompatibility."),
    (["Breastfeeding", "Every 3 hours"], False, "Appropriate frequency.")],
   "Jaundice before 24 hours with hemolysis is pathologic.",
   "Recognize cues of pathologic jaundice."),

matrix_mc(M(PA, NEWBORN_COMP, "analyze_cues", "difficult", OB, "peds"),
   "For each feature, indicate whether it fits physiologic, breastfeeding or pathologic jaundice.",
   ["Physiologic", "Breastfeeding (suboptimal intake)", "Pathologic"],
   [("Appears after 24 hours, peaks day 3 to 5", 0, "Physiologic."),
    ("First week with few stools and wet diapers", 1, "Low intake."),
    ("Appears within 24 hours with a positive Coombs", 2, "Hemolysis."),
    ("Rising more than 5 mg/dL a day", 2, "Pathologic."),
    ("Resolves by 2 weeks in a well baby", 0, "Physiologic.")],
   "Timing and rate of rise separate the types.",
   "Analyze newborn jaundice."),

cloze(M(PA, NEWBORN_COMP, "prioritize_hypotheses", "difficult", OB, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The newborn is at risk of {{b1}}, so the priority is {{b2}}.",
   [[("bilirubin encephalopathy (kernicterus)", True, "Brain damage."),
     ("hypoglycemia only", False, "Not the main risk."),
     ("dehydration only", False, "Not the main risk."),
     ("heart failure", False, "Not the main risk.")],
    [("starting phototherapy and frequent feeding", True, "Lowers bilirubin."),
     ("stopping breastfeeding", False, "Not needed."),
     ("giving water bottles", False, "Does not help."),
     ("waiting until day 3", False, "Too late.")]],
   "Phototherapy and feeding prevent kernicterus.",
   "Prioritize pathologic jaundice."),

sata(M(PA, NEWBORN_COMP, "generate_solutions", "difficult", OB, "peds"),
   "Which parts of the phototherapy plan should the nurse include?",
   [("Cover the eyes with eye shields", True, "Protects the retina."),
    ("Leave the skin uncovered except for a small diaper", True, "Maximum exposure."),
    ("Check temperature often", True, "Overheating or cooling."),
    ("Feed every 2 to 3 hours and count diapers", True, "Hydration and excretion."),
    ("Apply lotion to protect the skin", False, "Can burn."),
    ("Keep the baby under the lights for every feeding", False, "Short breaks are allowed unless bilirubin is very high.")],
   "Eye shields, exposure, temperature and feeding.",
   "Generate the phototherapy plan.",
   ex=[note("Orders", "Intensive phototherapy. Repeat bilirubin in 4 hours.", id="jau-orders")]),

sata(M(PA, NEWBORN_COMP, "take_action", "difficult", OB, "peds"),
   "Four hours later, review the findings. Which actions should the nurse take?",
   [("Notify the provider at once", True, "Rising despite phototherapy with neurologic signs."),
    ("Prepare for IV immune globulin or exchange transfusion as ordered", True, "Escalation."),
    ("Continue intensive phototherapy", True, "Keep treating."),
    ("Stop phototherapy because it is not working", False, "Continue while escalating."),
    ("Let the newborn sleep through the next feeding", False, "Feeding helps.")],
   "A rising level with a high-pitched cry and arching are early encephalopathy signs.",
   "Take action on rising bilirubin.",
   ex=[flow("4 hours", ["Start", "4 hours"], [
       ("Bilirubin", ["11.2", "14.6 mg/dL"]),
       ("Behavior", ["Sleepy", "High-pitched cry, arching back"]),
       ("Feeding", ["Every 3 hours", "Poor suck"])], id="jau-4h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", OB, "peds"),
   "At discharge on day 4, for each statement or finding, indicate whether it shows readiness or needs follow-up.",
   ["Readiness", "Needs follow-up"],
   [("Bilirubin 9 mg/dL and falling", 0, "Improving."),
    ("\"I'll feed at least 8 to 12 times a day.\"", 0, "Correct."),
    ("\"I'll put the baby in the sun at the window to treat yellow skin.\"", 1, "Not safe or effective."),
    ("Follow-up bilirubin check in 24 to 48 hours", 0, "Needed."),
    ("\"I'll call if the yellow spreads to the legs or the baby won't wake to feed.\"", 0, "Correct.")],
   "Improving with follow-up; sunlight is not a treatment.",
   "Evaluate jaundice discharge teaching.",
   ex=[note("Day 4", "IVIG given once. No exchange needed.", id="jau-day4")]),
]

# ================================================================================================
# Case 3: Neonatal opioid withdrawal syndrome
# ================================================================================================
nas_tabs = [
    ("nas-notes", "Nursery notes", [note("Nursery, 36 hours",
        "A term newborn's mother takes methadone 90 mg daily for opioid use disorder and is in recovery. The "
        "newborn is now irritable with a high-pitched cry.", id="nas-note")]),
    ("nas-data", "Findings", [table("Findings", ["Result"], [
        ("Cry", ["High-pitched, hard to console"]),
        ("Sleep", ["Less than 1 hour after feeds"]),
        ("Tone", ["Increased, tremors when undisturbed"]),
        ("Feeding", ["Frantic sucking, poor coordination"]),
        ("Stools", ["Loose, frequent"]),
        ("Skin", ["Excoriated nose and knees"]),
        ("Sneezing and yawning", ["Frequent"])], id="nas-table")]),
]

nas_qs = [
hl_table(M(PA, NEWBORN_COMP, "recognize_cues", "moderate", OB, "peds"),
   "Highlight the findings of neonatal opioid withdrawal.",
   ["Finding", "Result"],
   [(["Cry", "High-pitched"], True, "CNS irritability."),
    (["Tremors", "When undisturbed"], True, "CNS."),
    (["Feeding", "Frantic, poorly coordinated"], True, "GI and suck."),
    (["Stools", "Loose"], True, "GI."),
    (["Sneezing and yawning", "Frequent"], True, "Autonomic.")],
   "Irritability, tremors, poor feeding and loose stools after opioid exposure is withdrawal.",
   "Recognize cues of neonatal opioid withdrawal."),

matrix_mc(M(PA, NEWBORN_COMP, "analyze_cues", "difficult", OB, "peds"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Methadone withdrawal usually starts 24 to 72 hours or later after birth", 0, "Long half-life."),
    ("Mothers on methadone should be told not to breastfeed", 1, "Breastfeeding is encouraged if stable and HIV negative."),
    ("Rooming-in with the mother reduces the need for medicine", 0, "Eat, Sleep, Console approach."),
    ("The mother should stop methadone after birth", 1, "Relapse risk."),
    ("Withdrawal is assessed by function: eating, sleeping and being consoled", 0, "ESC.")],
   "Rooming-in, breastfeeding and functional assessment improve outcomes.",
   "Analyze neonatal opioid withdrawal."),

cloze(M(PA, NEWBORN_COMP, "prioritize_hypotheses", "difficult", OB, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The first-line treatment is {{b1}}; medicine such as {{b2}} is added if the baby still cannot eat, sleep or be "
   "consoled.",
   [[("non-drug care: low stimulation, swaddling, skin to skin and frequent feeds", True, "First-line."),
     ("a strong sedative", False, "Not first-line."),
     ("separating the baby from the mother", False, "Worsens outcomes."),
     ("formula only", False, "Not required.")],
    [("oral morphine", True, "Standard opioid replacement."),
     ("naloxone", False, "Precipitates severe withdrawal."),
     ("caffeine", False, "Not used."),
     ("ampicillin", False, "Not used.")]],
   "Non-drug care first; morphine if needed.",
   "Prioritize neonatal withdrawal care."),

sata(M(PA, NEWBORN_COMP, "generate_solutions", "difficult", OB, "peds"),
   "Which interventions should the nurse include?",
   [("Dim lights and a quiet room", True, "Low stimulation."),
    ("Swaddle and hold skin to skin", True, "Comfort."),
    ("Small frequent feedings", True, "Coordination."),
    ("Protect excoriated skin with barrier cream", True, "Skin care."),
    ("Keep the baby in the busy nurses' station for observation", False, "Overstimulating."),
    ("Wake the baby every hour for assessment", False, "Disrupts sleep.")],
   "Low stimulation, comfort, feeding and skin care.",
   "Generate the withdrawal care plan.",
   ex=[note("Plan", "Rooming-in with the mother; Eat, Sleep, Console assessments every 3 hours.", id="nas-plan")]),

sata(M(PSI, SUBSTANCE, "take_action", "difficult", OB, "psych peds"),
   "The mother says, \"This is all my fault. I'm a terrible mother.\" Which responses are therapeutic?",
   [("\"Taking methadone was a healthy choice for you and your baby.\"", True, "Accurate and supportive."),
    ("\"You calm the baby better than anyone. Let's work on this together.\"", True, "Empowers."),
    ("\"You sound very worried about your baby.\"", True, "Reflects."),
    ("\"You should have thought about that before.\"", False, "Judgmental."),
    ("\"Don't worry, the nurses will take care of everything.\"", False, "Removes the parent's role.")],
   "Support the mother's recovery and role.",
   "Take action with a mother in recovery.",
   ex=[note("Day 3", "Mother is tearful at the bedside.", id="nas-day3")]),

matrix_mc(M(PA, NEWBORN_COMP, "evaluate_outcomes", "difficult", OB, "peds"),
   "On day 6, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Eats 60 mL per feed", 0, "Eating."),
    ("Sleeps 2 to 3 hours after feeds", 0, "Sleeping."),
    ("Consoles within 10 minutes", 0, "Consoling."),
    ("Weight down 11% from birth", 1, "Excessive loss."),
    ("Mother attends her methadone dosing daily", 0, "Recovery.")],
   "Functional improvement; weight loss needs a feeding plan.",
   "Evaluate neonatal withdrawal care.",
   ex=[note("Day 6", "No medicine needed.", id="nas-day6")]),
]

# ================================================================================================
# Case 4: Postpartum endometritis and mastitis
# ================================================================================================
pp_tabs = [
    ("pp-notes", "Postpartum notes", [note("Postpartum day 3 after cesarean",
        "A 24-year-old client had a cesarean after 22 hours of labor with ruptured membranes for 20 hours. The "
        "client is breastfeeding.", id="pp-note")]),
    ("pp-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["38.8 C"]),
        ("Heart rate", ["112/min"]),
        ("Uterus", ["Tender, soft"]),
        ("Lochia", ["Foul-smelling, brownish"]),
        ("Incision", ["Clean and dry"]),
        ("WBC", ["21,000/mm3"])], id="pp-table")]),
]

pp_qs = [
hl_table(M(HPM, POSTPARTUM, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest endometritis.",
   ["Finding", "Result"],
   [(["Temperature", "38.8 C on day 3"], True, "Postpartum infection."),
    (["Uterus", "Tender"], True, "Endometrial infection."),
    (["Lochia", "Foul-smelling"], True, "Infection."),
    (["Risk", "Cesarean after long labor and rupture"], True, "Highest risk."),
    (["Incision", "Clean"], False, "Argues against a wound infection.")],
   "Fever, uterine tenderness and foul lochia after cesarean is endometritis.",
   "Recognize cues of endometritis."),

matrix_mc(M(HPM, POSTPARTUM, "analyze_cues", "difficult", OB, "ob"),
   "For each finding, indicate the likely postpartum problem.",
   ["Endometritis", "Mastitis", "Deep vein thrombosis", "Wound infection"],
   [("Tender uterus with foul lochia", 0, "Endometritis."),
    ("Red, hot wedge on one breast with flu-like symptoms", 1, "Mastitis."),
    ("One swollen, painful calf", 2, "DVT."),
    ("Red, draining incision", 3, "Wound infection.")],
   "Postpartum fever has several sources.",
   "Analyze postpartum fever."),

cloze(M(PPT, ABX, "prioritize_hypotheses", "difficult", OB, "ph ob"),
   "Complete the sentence by choosing from the lists of options.",
   "Treatment is {{b1}}, and the client {{b2}}.",
   [[("IV clindamycin and gentamicin until afebrile for 24 to 48 hours", True, "Standard."),
     ("oral amoxicillin only", False, "Inadequate."),
     ("no antibiotics", False, "Unsafe."),
     ("an antifungal", False, "Not fungal.")],
    [("can continue breastfeeding", True, "Compatible."),
     ("must stop breastfeeding", False, "Not needed."),
     ("must be isolated from the baby", False, "Not needed."),
     ("should pump and discard all milk", False, "Not needed.")]],
   "IV antibiotics; breastfeeding continues.",
   "Prioritize endometritis treatment."),

sata(M(HPM, BREASTFEEDING, "generate_solutions", "difficult", OB, "ob"),
   "On day 10 at home, the client has a red, tender wedge on the left breast and a fever. Which parts of the plan "
   "should the nurse include?",
   [("Keep breastfeeding or pumping often, starting on the affected side", True, "Empties the breast."),
    ("Check the latch and position", True, "Cause."),
    ("Take the prescribed antibiotic for the full course", True, "Cephalexin or dicloxacillin."),
    ("Use ibuprofen and cold packs for pain", True, "Comfort."),
    ("Stop breastfeeding on the left side until it heals", False, "Milk stasis worsens it."),
    ("Wear a tight bra to reduce milk", False, "Worsens blockage.")],
   "Keep emptying, fix the latch, antibiotics and comfort.",
   "Generate mastitis care.",
   ex=[note("Day 10 phone call", "Discharged on day 6 after endometritis treatment.", id="pp-day10")]),

sata(M(HPM, POSTPARTUM, "take_action", "difficult", OB, "ob"),
   "Which signs should the nurse teach the client to report immediately after discharge?",
   [("Soaking a pad in an hour", True, "Hemorrhage."),
    ("Calf pain or swelling, or chest pain", True, "DVT or PE."),
    ("Severe headache or vision changes", True, "Postpartum preeclampsia."),
    ("Thoughts of harming self or baby", True, "Postpartum mood disorder."),
    ("Lochia changing from red to pink", False, "Normal."),
    ("Mild afterpains while breastfeeding", False, "Normal.")],
   "Postpartum warning signs.",
   "Take action with postpartum warning signs.",
   ex=[note("Discharge", "Client going home with the baby.", id="pp-dc")]),

matrix_mc(M(HPM, POSTPARTUM, "evaluate_outcomes", "difficult", OB, "ob"),
   "At the 2-week visit, for each finding, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Afebrile; breast redness gone", 0, "Resolved."),
    ("Fundus not palpable", 0, "Involution."),
    ("Lochia alba", 0, "Normal."),
    ("A firm, tender breast lump that fluctuates", 1, "Possible abscess."),
    ("EPDS 14", 1, "Possible postpartum depression.")],
   "Recovery; an abscess or depression needs follow-up.",
   "Evaluate postpartum recovery.",
   ex=[note("2 weeks", "Breastfeeding well.", id="pp-2wk")]),
]

cases = [
    case("Newborn assessment and routine medications", nb_tabs, nb_qs),
    case("Pathologic newborn jaundice from ABO incompatibility", jau_tabs, jau_qs),
    case("Neonatal opioid withdrawal syndrome", nas_tabs, nas_qs),
    case("Postpartum endometritis and mastitis", pp_tabs, pp_qs),
]

BUILT = build(138, "ob-cases-newborn-jaundice-nas-postpartum-standalone",
              "Maternity case studies: newborn assessment, pathologic jaundice, neonatal opioid withdrawal and "
              "postpartum infection, with four standalone items",
              items, cases, case_conversion=True)
