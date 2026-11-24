"""Batch 141 (case conversion, chunk 11: maternity part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: meconium-stained fluid at 42 weeks with meconium aspiration syndrome; a late-preterm newborn with transient
tachypnea, cold stress and hypoglycemia; the newborn head-to-toe discharge exam with cord care, safe sleep,
circumcision and car seat teaching; a vulvar hematoma after a forceps birth, with rubella and Tdap vaccination and
Rho(D) immune globulin. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and
includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
NEWBORN = "newborn-care"
NEWBORN_COMP = "newborn-complications"
POSTPARTUM = "postpartum-care"
INTRAPARTUM = "intrapartum-complications"
IMMZ = "immunizations"
OB_MEDS = "obstetric-medications"
OXY = "oxygen-therapy-and-devices"
LABS = "laboratory-values-and-monitoring"
TEACH = "health-teaching-and-behavior-change"
HOME = "equipment-and-home-safety"
PAIN = "pain-assessment-and-comfort"
DELEG = "delegation-and-supervision"

items = []

# ================================================================================================
# Case 1: Meconium aspiration
# ================================================================================================
mas_tabs = [
    ("mas-notes", "Labor and birth notes", [note("42 weeks",
        "A 34-year-old client at 42 weeks has artificial rupture of membranes; the fluid is thick and dark green. "
        "The FHR shows recurrent late decelerations. The newborn is born limp and not breathing.", id="mas-note")]),
    ("mas-data", "Newborn findings", [flow("Newborn", ["Birth", "30 minutes"], [
        ("Heart rate", ["80/min", "160/min"]),
        ("Breathing", ["Apneic", "Grunting, RR 80/min"]),
        ("Chest", ["-", "Barrel-shaped, coarse crackles"]),
        ("SpO2 (right hand)", ["-", "82% on 40% oxygen"]),
        ("Skin", ["Stained green, peeling", "Stained"])], id="mas-flow")]),
]

mas_qs = [
hl_table(M(PA, NEWBORN_COMP, "recognize_cues", "moderate", OB, "peds"),
   "Highlight the findings that suggest meconium aspiration syndrome.",
   ["Finding", "30 minutes"],
   [(["Breathing", "Grunting, RR 80"], True, "Respiratory distress."),
    (["Chest", "Barrel-shaped, crackles"], True, "Air trapping."),
    (["SpO2", "82% on 40%"], True, "Hypoxemia."),
    (["History", "Thick meconium, post-term, late decels"], True, "Risk."),
    (["Heart rate", "160/min"], False, "Normal now.")],
   "Thick meconium with respiratory distress and air trapping is meconium aspiration.",
   "Recognize cues of meconium aspiration."),

matrix_mc(M(PA, NEWBORN_COMP, "analyze_cues", "difficult", OB, "peds"),
   "For each effect of aspirated meconium, indicate its mechanism.",
   ["Airway obstruction", "Chemical inflammation", "Surfactant inactivation"],
   [("Air trapping and pneumothorax risk", 0, "Ball-valve effect."),
    ("Chemical pneumonitis", 1, "Inflammation."),
    ("Atelectasis and stiff lungs", 2, "Surfactant loss."),
    ("Barrel chest", 0, "Air trapping.")],
   "Meconium obstructs, inflames and inactivates surfactant.",
   "Analyze meconium aspiration."),

cloze(M(PA, NEWBORN_COMP, "prioritize_hypotheses", "difficult", OB, "peds aha"),
   "Complete the sentence by choosing from the lists of options.",
   "For a non-vigorous newborn with meconium, current guidance is to {{b1}}, and the newborn is at risk for "
   "{{b2}}.",
   [[("start routine resuscitation with positive-pressure ventilation within the first minute", True, "Routine suctioning below the cords is no longer recommended."),
     ("delay breathing support for tracheal suctioning first", False, "Delays ventilation."),
     ("stimulate for 3 minutes before any support", False, "Too long."),
     ("give naloxone first", False, "Not indicated.")],
    [("persistent pulmonary hypertension", True, "Hypoxia keeps pulmonary vessels constricted."),
     ("hyperglycemia", False, "Not the main risk."),
     ("polycythemia only", False, "Not the main risk."),
     ("jaundice only", False, "Not the main risk.")]],
   "Ventilate promptly; watch for pulmonary hypertension.",
   "Prioritize meconium aspiration."),

sata(M(PA, NEWBORN_COMP, "generate_solutions", "difficult", OB, "peds"),
   "Which parts of the NICU plan should the nurse anticipate?",
   [("Pre- and post-ductal SpO2", True, "Detects pulmonary hypertension shunting."),
    ("Surfactant as prescribed", True, "Replaces inactivated surfactant."),
    ("Minimal handling, warmth", True, "Reduces oxygen demand."),
    ("Watch for pneumothorax", True, "Air leak."),
    ("Frequent vigorous suctioning of the mouth and nose every hour", False, "Agitation worsens hypoxia."),
    ("Start bottle feeding now to keep up the glucose", False, "Aspiration risk with RR 80.")],
   "Dual saturations, surfactant, minimal handling and air leak watch.",
   "Generate the NICU plan.",
   ex=[note("NICU", "CPAP started. Chest x-ray: patchy infiltrates, hyperinflation.", id="mas-nicu")]),

sata(M(PA, NEWBORN_COMP, "take_action", "difficult", OB, "peds"),
   "Two hours later, review the findings. Which actions should the nurse take?",
   [("Notify the provider at once", True, "Possible tension pneumothorax."),
    ("Prepare for needle decompression or a chest tube", True, "Treatment."),
    ("Transilluminate the chest if ordered", True, "Rapid bedside check."),
    ("Increase oxygen and wait to see if it settles", False, "Delays care."),
    ("Bathe the newborn to remove the meconium", False, "Not now.")],
   "Sudden deterioration with unequal breath sounds suggests pneumothorax.",
   "Take action on a newborn pneumothorax.",
   ex=[flow("2 hours", ["Before", "Now"], [
       ("SpO2", ["93%", "74%"]),
       ("Breath sounds", ["Equal", "Absent on the left"]),
       ("Heart rate", ["150", "90/min"]),
       ("Chest", ["Symmetric", "Left side bulging"])], id="mas-2h")]),

matrix_mc(M(PA, NEWBORN_COMP, "evaluate_outcomes", "difficult", OB, "peds"),
   "On day 3, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("SpO2 95% on 30% oxygen", 0, "Improving."),
    ("Chest tube draining with no air leak", 0, "Resolving."),
    ("Pre-ductal 96%, post-ductal 84%", 1, "Shunting; pulmonary hypertension."),
    ("Parents holding the baby skin to skin", 0, "Bonding."),
    ("Seizure-like movements of the arm", 1, "Possible hypoxic injury.")],
   "Improving; shunting and seizures need follow-up.",
   "Evaluate meconium aspiration care.",
   ex=[note("Day 3", "Weaning from CPAP.", id="mas-day3")]),
]

# ================================================================================================
# Case 2: Late-preterm newborn
# ================================================================================================
lp_tabs = [
    ("lp-notes", "Nursery notes", [note("Nursery",
        "A newborn born at 36 weeks by cesarean without labor is 2 hours old. The newborn is sleepy and feeding "
        "poorly at the breast.", id="lp-note")]),
    ("lp-data", "Findings", [table("Findings", ["Result"], [
        ("Respiratory rate", ["78/min"]),
        ("Breathing", ["Mild grunting, nasal flaring"]),
        ("SpO2", ["93% on room air"]),
        ("Temperature (axillary)", ["36.0 C"]),
        ("Glucose", ["34 mg/dL"]),
        ("Chest x-ray", ["Fluid in the fissures, clears over hours"])], id="lp-table")]),
]

lp_qs = [
hl_table(M(PA, NEWBORN_COMP, "recognize_cues", "moderate", OB, "peds"),
   "Highlight the findings that require action.",
   ["Finding", "Result"],
   [(["Respiratory rate", "78/min"], True, "Tachypnea."),
    (["Grunting and flaring", "Yes"], True, "Distress."),
    (["Temperature", "36.0 C"], True, "Cold stress."),
    (["Glucose", "34 mg/dL"], True, "Hypoglycemia."),
    (["Feeding", "Poor"], True, "Late-preterm risk.")],
   "Late-preterm newborns are at risk for breathing, temperature and glucose problems.",
   "Recognize cues in a late-preterm newborn."),

matrix_mc(M(PA, NEWBORN_COMP, "analyze_cues", "difficult", OB, "peds"),
   "For each finding, indicate whether it fits transient tachypnea or respiratory distress syndrome.",
   ["Transient tachypnea of the newborn", "Respiratory distress syndrome"],
   [("Cesarean without labor, fluid retained in the lungs", 0, "TTN."),
    ("Surfactant deficiency in a preterm newborn", 1, "RDS."),
    ("Ground-glass lungs with air bronchograms", 1, "RDS."),
    ("Improves within 24 to 72 hours", 0, "TTN."),
    ("Worsens over the first hours and may need surfactant", 1, "RDS.")],
   "TTN is retained fluid and resolves; RDS is surfactant deficiency.",
   "Analyze newborn respiratory distress."),

cloze(M(PA, NEWBORN_COMP, "prioritize_hypotheses", "difficult", OB, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, because cold stress {{b2}}.",
   [[("warming and treating the low glucose", True, "Linked problems."),
     ("a bath", False, "More heat loss."),
     ("waiting for the next feed in 3 hours", False, "Hypoglycemia."),
     ("discharge planning", False, "Not now.")],
    [("raises oxygen and glucose use, worsening distress and hypoglycemia", True, "A vicious cycle."),
     ("has no effect on glucose", False, "It does."),
     ("improves breathing", False, "Worsens."),
     ("lowers metabolic rate", False, "Raises it.")]],
   "Warm and treat glucose.",
   "Prioritize late-preterm problems."),

sata(M(PA, NEWBORN_COMP, "generate_solutions", "difficult", OB, "peds"),
   "Which interventions should the nurse anticipate?",
   [("Skin to skin or a radiant warmer", True, "Warmth."),
    ("Dextrose gel and a feed", True, "Hypoglycemia."),
    ("Oxygen if SpO2 falls", True, "Support."),
    ("Gavage feeds if RR stays high", True, "Aspiration risk."),
    ("Bathe the newborn now to remove the vernix and blood", False, "Heat loss."),
    ("Delay all feeding until 12 hours of age to rest the lungs", False, "Hypoglycemia.")],
   "Warmth, glucose, oxygen and safe feeding.",
   "Generate the late-preterm plan.",
   ex=[note("Plan", "Dextrose gel given. Recheck glucose in 30 minutes.", id="lp-plan")]),

sata(M(HPM, TEACH, "take_action", "difficult", OB, "peds"),
   "Before discharge on day 4, which teaching is especially important for a late-preterm newborn?",
   [("Wake the baby to feed at least every 3 hours", True, "Sleepy feeders."),
    ("Watch for yellow skin; late-preterm babies are at higher risk", True, "Jaundice."),
    ("Keep the baby warm with a hat and layers", True, "Thermoregulation."),
    ("Car seat challenge before discharge", True, "Apnea in the car seat."),
    ("Feed on demand only; let the baby sleep long stretches", False, "Too little intake."),
    ("Place the baby prone to sleep for better breathing", False, "SIDS risk.")],
   "Feeding, jaundice, warmth and car seat safety.",
   "Take action with late-preterm discharge teaching.",
   ex=[note("Day 4", "Breathing normally since day 1. Weight down 8%.", id="lp-day4")]),

matrix_mc(M(PA, NEWBORN_COMP, "evaluate_outcomes", "difficult", OB, "peds"),
   "At the 48-hour follow-up visit, for each finding, indicate whether it is reassuring or needs follow-up.",
   ["Reassuring", "Needs follow-up"],
   [("Six wet diapers and three yellow stools a day", 0, "Good intake."),
    ("Weight back to 7% below birth weight", 0, "Improving."),
    ("Yellow to the abdomen, bilirubin 17 mg/dL", 1, "High for a late-preterm baby."),
    ("Temperature 36.8 C", 0, "Normal."),
    ("Breastfeeding 8 to 10 times a day", 0, "Good.")],
   "Feeding is good; the bilirubin needs treatment.",
   "Evaluate late-preterm follow-up.",
   ex=[note("Clinic", "Day 6 of life.", id="lp-clinic")]),
]

# ================================================================================================
# Case 3: Newborn discharge exam and teaching
# ================================================================================================
dc_tabs = [
    ("dc-notes", "Nursery notes", [note("Day 2",
        "A term male newborn is being prepared for discharge with first-time parents. A circumcision was done "
        "this morning.", id="dc-note")]),
    ("dc-data", "Head-to-toe exam", [table("Exam", ["Finding"], [
        ("Head", ["Swelling crossing the suture lines (caput)"]),
        ("Fontanels", ["Anterior soft and flat"]),
        ("Skin", ["Small white spots on the nose; red blotchy rash on the trunk"]),
        ("Hips", ["Clunk felt with the Ortolani maneuver on the left"]),
        ("Cord", ["Drying, no odor"]),
        ("Circumcision", ["Yellow film on the glans, no bleeding"]),
        ("Reflexes", ["Moro, rooting, sucking present"])], id="dc-table")]),
]

dc_qs = [
hl_table(M(HPM, NEWBORN, "recognize_cues", "moderate", OB, "peds"),
   "Highlight the finding that needs follow-up before discharge.",
   ["Finding", "Result"],
   [(["Hips", "Positive Ortolani"], True, "Developmental dysplasia of the hip."),
    (["Head", "Caput"], False, "Resolves in days."),
    (["Skin", "Milia and erythema toxicum"], False, "Normal."),
    (["Circumcision", "Yellow film"], False, "Normal healing."),
    (["Fontanel", "Soft, flat"], False, "Normal.")],
   "A hip clunk needs evaluation; the others are normal.",
   "Recognize cues on the newborn exam."),

matrix_mc(M(HPM, NEWBORN, "analyze_cues", "difficult", OB, "peds"),
   "For each finding, indicate whether it is normal or needs follow-up.",
   ["Normal", "Needs follow-up"],
   [("Caput succedaneum crossing sutures", 0, "Normal."),
    ("Cephalohematoma not crossing sutures", 0, "Normal, but raises jaundice risk."),
    ("Bulging anterior fontanel when calm", 1, "Raised pressure."),
    ("Absent Moro on one side", 1, "Nerve injury or fracture."),
    ("Pseudomenstruation in a female newborn", 0, "Maternal hormones.")],
   "Many newborn findings are normal variants.",
   "Analyze newborn findings."),

cloze(M(HPM, NEWBORN, "prioritize_hypotheses", "difficult", OB, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The hip finding needs {{b1}}, and the most important safety teaching is {{b2}}.",
   [[("an ultrasound and orthopedic referral", True, "Early treatment with a Pavlik harness."),
     ("tight swaddling with the legs straight", False, "Worsens hip dysplasia."),
     ("no action", False, "Untreated causes limp."),
     ("an x-ray in 5 years", False, "Too late.")],
    [("safe sleep: alone, on the back, in a bare crib", True, "Prevents SIDS."),
     ("using baby powder", False, "Not important."),
     ("avoiding all visitors", False, "Not necessary."),
     ("a daily bath", False, "Not needed.")]],
   "Hip referral and safe sleep.",
   "Prioritize discharge needs."),

sata(M(HPM, TEACH, "generate_solutions", "difficult", OB, "peds"),
   "Which discharge teaching should the nurse include?",
   [("Keep the cord stump clean and dry and fold the diaper below it", True, "Cord care."),
    ("Petroleum jelly on the circumcision with each diaper change for a few days", True, "Protects healing."),
    ("A rear-facing car seat in the back seat", True, "Car safety."),
    ("Call for a temperature of 38 C rectal or higher", True, "Sepsis risk."),
    ("Put alcohol on the cord three times a day", False, "Dry care is recommended."),
    ("Remove the yellow film on the circumcision by scrubbing", False, "It is normal healing.")],
   "Cord care, circumcision care, car seat and fever.",
   "Generate discharge teaching.",
   ex=[note("Ultrasound", "Left hip dysplasia; Pavlik harness planned.", id="dc-us")]),

sata(M(MOC, DELEG, "take_action", "difficult", OB, "mgmt peds"),
   "Which discharge tasks can the nurse delegate to the UAP?",
   [("Check that the infant and parent ID bands match before the baby leaves", True, "Routine safety check with a protocol."),
    ("Take the newborn's vital signs", True, "Routine."),
    ("Teach the parents how to use the Pavlik harness", False, "Teaching stays with the RN."),
    ("Assess the circumcision for bleeding and decide if it is normal", False, "Assessment."),
    ("Help the parents carry belongings to the car", True, "Non-clinical.")],
   "UAPs do routine tasks; teaching and assessment stay with the RN.",
   "Delegate at newborn discharge.",
   ex=[note("Discharge", "Parents ready to leave.", id="dc-ready")]),

matrix_mc(M(HPM, NEWBORN, "evaluate_outcomes", "difficult", OB, "peds"),
   "For each statement by the parents, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"We'll keep the harness on all the time unless told otherwise.\"", 0, "Correct."),
    ("\"The baby will sleep in our bed so we can watch him.\"", 1, "Bed-sharing raises SIDS risk."),
    ("\"We'll put the car seat in the front so we can see him.\"", 1, "Airbag danger."),
    ("\"Six wet diapers a day means he's getting enough.\"", 0, "Correct."),
    ("\"We'll give sponge baths until the cord falls off.\"", 0, "Correct.")],
   "Safe sleep and car seat teaching need reinforcing.",
   "Evaluate discharge teaching.",
   ex=[note("Teach-back", "Parents explain the plan.", id="dc-teachback")]),
]

# ================================================================================================
# Case 4: Vulvar hematoma after forceps, with vaccines
# ================================================================================================
hem_tabs = [
    ("hem-notes", "Postpartum notes", [note("2 hours after birth",
        "A 26-year-old client (O negative, rubella non-immune, did not get Tdap during pregnancy) had a rapid "
        "forceps-assisted birth. The baby is O positive. The client now has severe perineal pain and pressure.",
        id="hem-note")]),
    ("hem-data", "Findings", [flow("Findings", ["1 hour", "2 hours"], [
        ("Fundus", ["Firm, midline at the umbilicus", "Firm, midline"]),
        ("Lochia", ["Moderate", "Moderate"]),
        ("Pain", ["3/10", "9/10 deep rectal pressure"]),
        ("Perineum", ["Episiotomy intact", "Tense purple swelling on the right labium"]),
        ("Heart rate", ["84", "118/min"]),
        ("Blood pressure", ["118/72", "98/58 mm Hg"])], id="hem-flow")]),
]

hem_qs = [
hl_table(M(HPM, POSTPARTUM, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest a hematoma.",
   ["Finding", "2 hours"],
   [(["Pain", "Severe, rectal pressure"], True, "Classic."),
    (["Perineum", "Tense purple swelling"], True, "Hematoma."),
    (["Heart rate and BP", "118, 98/58"], True, "Concealed blood loss."),
    (["Fundus", "Firm"], False, "Rules out atony."),
    (["Lochia", "Moderate"], False, "Bleeding is concealed.")],
   "Severe pain and shock with a firm fundus and normal lochia suggest a hematoma.",
   "Recognize cues of a postpartum hematoma."),

matrix_mc(M(HPM, POSTPARTUM, "analyze_cues", "difficult", OB, "ob"),
   "For each finding, indicate the most likely source of postpartum bleeding.",
   ["Uterine atony", "Laceration", "Hematoma"],
   [("Boggy fundus with heavy flow and clots", 0, "Atony."),
    ("Firm fundus with a steady trickle of bright red blood", 1, "Laceration."),
    ("Firm fundus, normal flow, severe pain and swelling", 2, "Hematoma."),
    ("Bleeding that slows with fundal massage", 0, "Atony.")],
   "Fundal tone and pain point to the source.",
   "Analyze sources of postpartum bleeding."),

cloze(M(HPM, POSTPARTUM, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, then {{b2}}.",
   [[("notify the provider and apply ice to the perineum", True, "Reduces swelling."),
     ("massage the fundus harder", False, "The fundus is firm."),
     ("give an opioid and wait", False, "Masks shock."),
     ("sit the client on a hard chair", False, "Worsens pain.")],
    [("start an IV and prepare for drainage of the hematoma", True, "Large hematomas need incision and drainage."),
     ("discharge", False, "Unsafe."),
     ("encourage walking", False, "Unsafe."),
     ("give a laxative", False, "Not now.")]],
   "Notify, ice, IV and prepare for drainage.",
   "Prioritize a hematoma."),

sata(M(PPT, OB_MEDS, "generate_solutions", "difficult", OB, "ph acip"),
   "After the hematoma is drained, which postpartum medicines should the nurse plan to give before discharge?",
   [("Rho(D) immune globulin within 72 hours", True, "Rh-negative mother, Rh-positive baby."),
    ("MMR vaccine for rubella non-immunity", True, "Given postpartum."),
    ("Tdap vaccine", True, "Protects the newborn from pertussis."),
    ("MMR during the next pregnancy instead", False, "Live vaccine is contraindicated in pregnancy."),
    ("Skip Rho(D) because the baby is O positive like the mother's type", False, "Rh factor matters, not ABO.")],
   "Rho(D), MMR and Tdap postpartum.",
   "Generate postpartum medicines.",
   ex=[note("Day 1", "Hematoma drained in the operating room. Hemoglobin 8.6 g/dL, stable.", id="hem-day1")]),

sata(M(HPM, IMMZ, "take_action", "difficult", OB, "acip"),
   "Which teaching should the nurse give with the MMR vaccine?",
   [("Avoid pregnancy for 4 weeks after the vaccine", True, "Live vaccine."),
    ("Breastfeeding can continue", True, "Safe."),
    ("Rho(D) immune globulin may reduce the vaccine's response; check immunity later", True, "Rubella titer later."),
    ("Mild fever or joint aches may occur", True, "Common."),
    ("Stop breastfeeding for 2 weeks", False, "Not needed."),
    ("No contraception is needed while breastfeeding", False, "Breastfeeding is not reliable contraception.")],
   "Avoid pregnancy for 4 weeks; breastfeeding is safe.",
   "Take action with postpartum vaccine teaching.",
   ex=[note("Day 2", "Both vaccines and Rho(D) given.", id="hem-day2")]),

matrix_mc(M(HPM, POSTPARTUM, "evaluate_outcomes", "difficult", OB, "ob"),
   "At discharge, for each finding or statement, indicate whether it shows recovery or understanding, or needs "
   "follow-up.",
   ["Recovery or understanding", "Needs follow-up"],
   [("Pain 3/10 with sitz baths", 0, "Improving."),
    ("Voiding without difficulty", 0, "Normal."),
    ("\"I'll use condoms or another method for at least a month.\"", 0, "Correct."),
    ("New increasing swelling and pain on day 3", 1, "Re-accumulation."),
    ("Dizzy on standing with hemoglobin 7.2 g/dL", 1, "Anemia.")],
   "Recovery; swelling or worsening anemia needs follow-up.",
   "Evaluate postpartum recovery after a hematoma.",
   ex=[note("Day 3", "Discharge planned.", id="hem-day3")]),
]

cases = [
    case("Meconium aspiration in a post-term newborn", mas_tabs, mas_qs),
    case("Late-preterm newborn with transient tachypnea and hypoglycemia", lp_tabs, lp_qs),
    case("Newborn discharge exam and teaching", dc_tabs, dc_qs),
    case("Postpartum vulvar hematoma with postpartum vaccines", hem_tabs, hem_qs),
]

BUILT = build(141, "ob-cases-meconium-late-preterm-discharge-hematoma",
              "Maternity case studies: meconium aspiration, late-preterm newborn, newborn discharge and postpartum "
              "hematoma",
              items, cases, case_conversion=True)
