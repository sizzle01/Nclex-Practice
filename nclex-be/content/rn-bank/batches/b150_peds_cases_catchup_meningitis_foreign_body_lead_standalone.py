"""Batch 150 (case conversion, chunk 13: pediatrics part 2): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: catch-up vaccination for a newly arrived 4-year-old with true and false contraindications (vaccination
schedule); bacterial meningitis in an infant with droplet precautions and prophylaxis for contacts; foreign body
aspiration in a toddler; lead poisoning with chelation and home hazard teaching. Standalone: an epiglottitis bowtie,
a developmental stage table (Erikson and Piaget), a priority question in a pediatric emergency department and care
after hypospadias repair. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and
includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
IMMZ = "immunizations"
GROWTH = "infant-growth-and-development"
NEURO = "pediatric-neurological-disorders"
RESP = "pediatric-respiratory-disorders"
GU = "pediatric-gi-gu-disorders"
POISON = "poisoning-and-ingestions"
COMMUNICABLE = "childhood-communicable-diseases"
TBP = "transmission-based-precautions"
PRIORITY = "prioritizing-client-care"
HOME = "equipment-and-home-safety"
TEACH = "health-teaching-and-behavior-change"
CULTURE = "cultural-and-spiritual-care"
BLS = "basic-life-support"
POSTOP = "postoperative-care"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, RESP, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "A child arrives at the emergency department with sudden breathing trouble. Using the note, fill in the bowtie: "
   "the most likely problem in the middle, two actions to take now on the left and two parameters to monitor on "
   "the right.",
   [("Epiglottitis", True, "Sudden high fever, drooling, tripod position, muffled voice, unvaccinated."),
    ("Croup", False, "Gradual, barking cough, no drooling."),
    ("Asthma attack", False, "Wheezing, not drooling."),
    ("Foreign body aspiration", False, "No choking episode and high fever.")],
   [("Keep calm and upright", True, "Crying can close the airway."),
    ("Call the airway team", True, "Airway emergency."),
    ("Look in the throat with a tongue blade to see the epiglottis", False, "Can cause complete obstruction."),
    ("Lay the child flat on the stretcher for a neck x-ray", False, "Airway closure."),
    ("Take a throat culture now before antibiotics start", False, "Wait until the airway is secure.")],
   [("Work of breathing", True, "Obstruction."),
    ("SpO2", True, "Hypoxia."),
    ("Bowel sounds every hour", False, "Not relevant."),
    ("Body weight once a week", False, "Not relevant."),
    ("Urine color each void", False, "Not relevant.")],
   "Drooling, tripod position and high fever in an unvaccinated child is epiglottitis; protect the airway.",
   "Recognize and act on epiglottitis.",
   ex=[note("Emergency department", "4-year-old, no Hib vaccine. Fever 40 C for 6 hours, drooling, sitting forward "
       "on the hands, muffled voice, soft stridor. Refuses to lie down.", id="sa-epi")]),

dtable(M(HPM, GROWTH, "analyze_cues", "difficult", PEDS, "peds psych"),
   "For each age, choose Erikson's developmental task.",
   ["Age group", "Typical behavior", "Erikson's task"],
   [(["Infant", "Cries and is comforted by a consistent caregiver"], [
       ("Trust versus mistrust", True, "Infancy."),
       ("Initiative versus guilt", False, "Preschool."),
       ("Industry versus inferiority", False, "School age.")]),
    (["Toddler", "Says \"no\" and wants to do things alone"], [
       ("Autonomy versus shame and doubt", True, "Toddler."),
       ("Trust versus mistrust", False, "Infant."),
       ("Identity versus role confusion", False, "Adolescent.")]),
    (["Preschooler", "Makes up games and asks \"why\""], [
       ("Initiative versus guilt", True, "Preschool."),
       ("Industry versus inferiority", False, "School age."),
       ("Autonomy versus shame", False, "Toddler.")]),
    (["School-age child", "Collects things, likes to finish projects"], [
       ("Industry versus inferiority", True, "School age."),
       ("Initiative versus guilt", False, "Preschool."),
       ("Intimacy versus isolation", False, "Young adult.")]),
    (["Adolescent", "Tries out styles and values; peers matter most"], [
       ("Identity versus role confusion", True, "Adolescence."),
       ("Industry versus inferiority", False, "School age."),
       ("Generativity versus stagnation", False, "Middle adult.")])],
   "Erikson's stages guide age-appropriate care.",
   "Analyze developmental stages.",
   ex=[note("Unit", "Children of different ages on the pediatric unit.", id="sa-erikson")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", PEDS, "mgmt peds"),
   "In a pediatric emergency department, four children arrive at the same time. Which child should the nurse see "
   "first?",
   [("A 3-year-old who choked on a grape and now cannot speak or cry", True,
     "Complete airway obstruction needs immediate action."),
    ("A 7-year-old with an obvious forearm deformity after falling off a swing at school", False, "Painful but stable."),
    ("A 10-month-old with fever of 39 C who is feeding and smiling", False, "Well-appearing."),
    ("A 5-year-old with a 2 cm scalp cut that is still oozing blood", False, "Apply pressure; stable.")],
   "A child who cannot speak or cry after choking has complete obstruction.",
   "Prioritize children in the emergency department."),

sata(M(RRP, POSTOP, "take_action", "difficult", PEDS, "peds"),
   "A 12-month-old boy had hypospadias repair with a urinary stent. Which actions should the nurse take?",
   [("Use double diapers so the stent drains into the outer diaper", True, "Protects the stent."),
    ("Watch for kinking or blockage of the stent", True, "Urine flow."),
    ("Encourage fluids", True, "Keeps the stent flowing."),
    ("Avoid straddle toys and tub baths until cleared", True, "Protects the repair."),
    ("Remove the stent if it looks loose", False, "Only the provider removes it."),
    ("Circumcise at the same time to save another surgery", False, "Foreskin is needed for repair.")],
   "Stent care, fluids and activity limits.",
   "Take action after hypospadias repair."),
]

# ================================================================================================
# Case 1: Catch-up vaccines
# ================================================================================================
cu_tabs = [
    ("cu-notes", "Clinic notes", [note("Pediatric clinic",
        "A 4-year-old child arrived from another country a month ago with no vaccine records. The child has a mild "
        "cold with a temperature of 37.6 C. The parent says the child had a rash after eating eggs once. A "
        "younger sibling is getting chemotherapy for leukemia.", id="cu-note")]),
    ("cu-data", "History", [table("History", ["Finding"], [
        ("Vaccine records", ["None"]),
        ("Egg allergy", ["Hives once"]),
        ("Current illness", ["Mild cold, 37.6 C"]),
        ("Sibling", ["On chemotherapy"]),
        ("Pregnancy", ["Mother is 20 weeks pregnant"]),
        ("Previous reactions to vaccines", ["Unknown"])], id="cu-table")]),
]

cu_qs = [
matrix_mc(M(HPM, IMMZ, "recognize_cues", "moderate", PEDS, "peds acip"),
   "For each factor, indicate whether it is a true reason to delay vaccines today.",
   ["Reason to delay", "Not a reason to delay"],
   [("Mild cold with a low fever", 1, "Mild illness is not a contraindication."),
    ("Egg allergy with hives", 1, "Flu and MMR can be given."),
    ("Sibling on chemotherapy", 1, "MMR and varicella are safe; avoid contact with a varicella rash."),
    ("Mother is pregnant", 1, "Children of pregnant mothers can get live vaccines."),
    ("Anaphylaxis to a previous dose", 0, "True contraindication.")],
   "Mild illness, egg allergy and household contacts are not reasons to delay.",
   "Recognize true and false vaccine contraindications."),

matrix_mc(M(HPM, IMMZ, "analyze_cues", "difficult", PEDS, "peds acip"),
   "For each vaccine, indicate whether it is a live vaccine.",
   ["Live", "Not live"],
   [("MMR", 0, "Live."),
    ("Varicella", 0, "Live."),
    ("DTaP", 1, "Inactivated."),
    ("IPV (polio shot)", 1, "Inactivated."),
    ("Rotavirus", 0, "Live oral (infants only)."),
    ("Hepatitis B", 1, "Recombinant.")],
   "Live vaccines have special timing and contraindication rules.",
   "Analyze live vaccines."),

cloze(M(HPM, IMMZ, "prioritize_hypotheses", "difficult", PEDS, "peds acip"),
   "Complete the sentence by choosing from the lists of options.",
   "Without records, the child should be {{b1}}, and two live vaccines not given the same day must be {{b2}}.",
   [[("vaccinated as if never vaccinated, following the catch-up schedule", True, "Records cannot be assumed."),
     ("assumed fully vaccinated", False, "Unsafe."),
     ("tested for every antibody before any vaccine", False, "Not required."),
     ("vaccinated only at school entry", False, "Too late.")],
    [("given at least 4 weeks apart", True, "Interference."),
     ("given 1 week apart", False, "Too close."),
     ("never given", False, "Wrong."),
     ("mixed in one syringe", False, "Never mix.")]],
   "Start the catch-up schedule; space live vaccines 4 weeks if not same day.",
   "Prioritize catch-up vaccination."),

sata(M(HPM, IMMZ, "generate_solutions", "difficult", PEDS, "peds acip"),
   "Which vaccines can the nurse give today at the first catch-up visit for a 4-year-old?",
   [("DTaP", True, "Catch-up."),
    ("IPV", True, "Catch-up."),
    ("MMR", True, "Live but allowed."),
    ("Varicella", True, "Live but allowed."),
    ("Hepatitis B", True, "Catch-up."),
    ("Rotavirus", False, "Not given after 8 months of age.")],
   "Most vaccines can be given together; rotavirus has an age limit.",
   "Generate the catch-up plan.",
   ex=[note("Plan", "Multiple vaccines today; return in 4 weeks.", id="cu-plan")]),

sata(M(PSI, CULTURE, "take_action", "difficult", PEDS, "psych peds"),
   "The parent speaks little English and seems worried. Which actions should the nurse take?",
   [("Use a professional medical interpreter", True, "Accurate communication."),
    ("Give vaccine information statements in the family's language", True, "Informed consent."),
    ("Ask what the parent knows and worries about vaccines", True, "Respect."),
    ("Ask the 9-year-old sibling to interpret", False, "Children should not interpret."),
    ("Give the vaccines without discussion to save time", False, "Consent required.")],
   "Professional interpreters and respectful consent.",
   "Take action with culturally safe care.",
   ex=[note("Visit", "Parent asks how many shots today.", id="cu-shots")]),

matrix_mc(M(HPM, IMMZ, "evaluate_outcomes", "difficult", PEDS, "peds acip"),
   "After the vaccines, for each statement by the parent, indicate whether it shows understanding or needs more "
   "teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"A rash or fever 1 to 2 weeks after MMR can happen.\"", 0, "Correct."),
    ("\"If a chickenpox-like rash appears, we'll keep the child away from the sibling on chemotherapy.\"", 0, "Rare spread."),
    ("\"We'll come back in 4 weeks for the next doses.\"", 0, "Correct."),
    ("\"One visit finishes all the vaccines.\"", 1, "Several visits."),
    ("\"I'll use ibuprofen or acetaminophen for fever, not aspirin.\"", 0, "Correct.")],
   "Reactions, sibling safety and follow-up.",
   "Evaluate catch-up vaccine teaching.",
   ex=[note("After", "Five vaccines given.", id="cu-after")]),
]

# ================================================================================================
# Case 2: Bacterial meningitis in an infant
# ================================================================================================
men_tabs = [
    ("men-notes", "Emergency notes", [note("Emergency department",
        "A 6-month-old infant has had a fever of 39.6 C for a day, poor feeding and vomiting. The infant is "
        "irritable and cries more when picked up. The infant attends daycare.", id="men-note")]),
    ("men-data", "Findings", [table("Findings", ["Result"], [
        ("Anterior fontanel", ["Bulging"]),
        ("Behavior", ["High-pitched cry; paradoxical irritability"]),
        ("Skin", ["No rash"]),
        ("Heart rate", ["188/min"]),
        ("Capillary refill", ["3 seconds"]),
        ("CSF", ["Cloudy, high WBC, low glucose, high protein"])], id="men-table")]),
]

men_qs = [
hl_table(M(PA, NEURO, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of bacterial meningitis in this infant.",
   ["Finding", "Result"],
   [(["Bulging fontanel", "Yes"], True, "Raised ICP."),
    (["Paradoxical irritability", "Cries when held"], True, "Meningeal irritation."),
    (["High-pitched cry", "Yes"], True, "CNS."),
    (["CSF", "Cloudy, low glucose"], True, "Bacterial."),
    (["Neck stiffness", "Not listed"], False, "Often absent in infants.")],
   "Infants with meningitis show a bulging fontanel and irritability rather than neck stiffness.",
   "Recognize cues of infant meningitis."),

matrix_mc(M(PA, NEURO, "analyze_cues", "difficult", PEDS, "peds"),
   "For each CSF finding, indicate whether it fits bacterial or viral meningitis.",
   ["Bacterial", "Viral"],
   [("Cloudy fluid", 0, "Bacterial."),
    ("Low glucose", 0, "Bacteria consume glucose."),
    ("Normal glucose", 1, "Viral."),
    ("Mostly lymphocytes", 1, "Viral."),
    ("Mostly neutrophils with high protein", 0, "Bacterial.")],
   "Glucose and cell type separate bacterial and viral meningitis.",
   "Analyze CSF results."),

cloze(M(PA, NEURO, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse must give {{b1}} without delay, and place the infant on {{b2}}.",
   [[("IV antibiotics", True, "Each hour of delay worsens outcomes."),
     ("oral antibiotics", False, "Too slow."),
     ("antivirals only", False, "Bacterial."),
     ("acetaminophen only", False, "Not treatment.")],
    [("droplet precautions for 24 hours of antibiotics", True, "Spread."),
     ("airborne precautions", False, "Not airborne."),
     ("no precautions", False, "Spread."),
     ("contact precautions only", False, "Droplet.")]],
   "IV antibiotics and droplet precautions.",
   "Prioritize infant meningitis."),

sata(M(PA, NEURO, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse include?",
   [("Measure head circumference daily", True, "Hydrocephalus."),
    ("Seizure precautions", True, "Risk."),
    ("A quiet, dim room", True, "Reduces stimulation."),
    ("Neurologic checks", True, "ICP."),
    ("Hold the infant upright on the shoulder for long periods", False, "Painful; paradoxical irritability."),
    ("Hearing test only if parents ask", False, "All need a hearing test.")],
   "Head size, seizures, quiet room and neuro checks.",
   "Generate infant meningitis care.",
   ex=[note("Culture", "Streptococcus pneumoniae. Infant missed PCV doses.", id="men-culture")]),

sata(M(SIC, TBP, "take_action", "difficult", PEDS, "peds"),
   "If the cause had been Neisseria meningitidis, which actions would the nurse take for contacts?",
   [("Prophylaxis for close contacts", True, "Prevents spread."),
    ("Report to public health", True, "Reportable."),
    ("Mask until 24 hours of antibiotics", True, "Droplet."),
    ("Treat contacts only if they get a fever", False, "Too late."),
    ("Close the hospital unit to new admissions", False, "Not needed.")],
   "Prophylaxis, reporting and droplet precautions.",
   "Take action for meningococcal contacts.",
   ex=[note("Teaching", "Nurse discusses what would change with meningococcus.", id="men-teach")]),

matrix_mc(M(PA, NEURO, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "On day 5, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Afebrile for 48 hours", 0, "Responding."),
    ("Feeding well", 0, "Recovery."),
    ("Fontanel soft and flat", 0, "ICP resolved."),
    ("Does not startle to loud sounds", 1, "Hearing loss."),
    ("Head circumference rising faster than expected", 1, "Hydrocephalus.")],
   "Recovery; hearing loss and hydrocephalus need follow-up.",
   "Evaluate meningitis recovery.",
   ex=[note("Day 5", "Antibiotics continue for 10 to 14 days.", id="men-day5")]),
]

# ================================================================================================
# Case 3: Foreign body aspiration
# ================================================================================================
fb_tabs = [
    ("fb-notes", "Emergency notes", [note("Emergency department",
        "A 2-year-old was eating peanuts and suddenly coughed and gagged 2 hours ago. The coughing settled, but the "
        "child now wheezes on one side.", id="fb-note")]),
    ("fb-data", "Findings", [flow("Findings", ["Arrival", "30 minutes"], [
        ("Breath sounds", ["Decreased on the right with wheeze", "Decreased on the right"]),
        ("Cough", ["Intermittent", "Strong, can cry"]),
        ("SpO2", ["95%", "94%"]),
        ("Chest x-ray", ["Right lung over-inflated on expiration", "-"]),
        ("Voice", ["Crying normally", "Crying normally"])], id="fb-flow")]),
]

fb_qs = [
hl_table(M(PA, RESP, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that suggest foreign body aspiration.",
   ["Finding", "Result"],
   [(["Sudden choking while eating nuts", "Yes"], True, "Classic history."),
    (["One-sided wheeze", "Right"], True, "Partial obstruction."),
    (["Decreased breath sounds", "Right"], True, "Air trapping."),
    (["Over-inflation on expiration", "Right"], True, "Ball-valve effect."),
    (["Crying normally", "Yes"], False, "Shows the airway is not completely blocked.")],
   "Choking history with one-sided wheeze is foreign body aspiration.",
   "Recognize cues of foreign body aspiration."),

matrix_mc(M(PA, BLS, "analyze_cues", "difficult", PEDS, "peds aha"),
   "For each situation, indicate the correct first aid.",
   ["Encourage coughing and watch", "Abdominal thrusts", "Back blows and chest thrusts"],
   [("A child who is coughing hard and can speak or cry", 0, "Partial obstruction."),
    ("A conscious 2-year-old who cannot cough, speak or cry", 1, "Complete obstruction in a child."),
    ("A conscious 6-month-old who cannot cry", 2, "Infant: 5 back blows, 5 chest thrusts."),
    ("A conscious 8-year-old clutching the throat, silent", 1, "Complete obstruction.")],
   "Coughing children are left to cough; infants get back blows and chest thrusts.",
   "Analyze choking first aid."),

cloze(M(PA, RESP, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "Because the child can cough and cry, the nurse should {{b1}}, and the child needs {{b2}}.",
   [[("keep the child calm and not perform blind finger sweeps or back blows", True, "Partial obstruction."),
     ("do abdominal thrusts now", False, "Not with an effective cough."),
     ("give food to push it down", False, "Dangerous."),
     ("lay the child flat", False, "Worsens.")],
    [("rigid bronchoscopy to remove the object", True, "Definitive."),
     ("antibiotics only", False, "Does not remove it."),
     ("a cough suppressant", False, "Harmful."),
     ("discharge", False, "Object remains.")]],
   "Calm, no blind sweeps, bronchoscopy.",
   "Prioritize foreign body aspiration."),

sata(M(PA, RESP, "generate_solutions", "difficult", PEDS, "peds"),
   "Which actions should the nurse include before bronchoscopy?",
   [("Keep the child NPO", True, "Anesthesia."),
    ("Let the child sit upright with the parent", True, "Comfort and airway."),
    ("Have suction and airway equipment ready", True, "Obstruction can worsen."),
    ("Continuous pulse oximetry", True, "Monitoring."),
    ("Give a drink to soothe the throat", False, "Aspiration."),
    ("Leave the child alone to rest", False, "Needs observation.")],
   "NPO, position, equipment and monitoring.",
   "Generate pre-bronchoscopy care.",
   ex=[note("Plan", "Bronchoscopy in 1 hour.", id="fb-plan")]),

sata(M(PA, RESP, "take_action", "difficult", PEDS, "peds aha"),
   "While waiting, the child suddenly cannot cough, speak or cry and turns blue. Which actions should the nurse "
   "take?",
   [("Call for help", True, "Emergency."),
    ("Give abdominal thrusts", True, "Complete obstruction in a child."),
    ("CPR if unresponsive", True, "Remove a visible object."),
    ("Do a blind finger sweep", False, "Pushes the object deeper."),
    ("Wait for the bronchoscopy team to arrive", False, "Delay.")],
   "Complete obstruction needs abdominal thrusts now.",
   "Take action for complete airway obstruction.",
   ex=[note("45 minutes", "The peanut fragment moved.", id="fb-45")]),

matrix_mc(M(SIC, HOME, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "After removal, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"No whole nuts, popcorn or whole grapes until age 4 or 5.\"", 0, "Choking foods."),
    ("\"I'll cut grapes and hot dogs into small pieces.\"", 0, "Correct."),
    ("\"My child should sit while eating.\"", 0, "Correct."),
    ("\"Balloons are safe toys for toddlers.\"", 1, "Latex balloons are a top choking hazard."),
    ("\"I'll take an infant and child CPR class.\"", 0, "Preparedness.")],
   "Choking foods, safe eating and CPR.",
   "Evaluate choking prevention teaching.",
   ex=[note("After bronchoscopy", "Peanut removed. Breath sounds equal.", id="fb-after")]),
]

# ================================================================================================
# Case 4: Lead poisoning
# ================================================================================================
pb_tabs = [
    ("pb-notes", "Clinic notes", [note("2-year well visit",
        "A 2-year-old lives in a house built in 1940 that is being renovated. The child eats paint chips from the "
        "windowsill (pica). The parent says the child is more irritable, has stomach aches and has stopped using "
        "new words.", id="pb-note")]),
    ("pb-data", "Results", [table("Results", ["Result"], [
        ("Blood lead level", ["48 mcg/dL"]),
        ("Hemoglobin", ["9.8 g/dL, small cells"]),
        ("Development", ["Speech regression"]),
        ("Abdominal x-ray", ["Small bright flecks in the bowel"]),
        ("Sibling", ["Age 4, not tested"])], id="pb-table")]),
]

pb_qs = [
hl_table(M(PA, POISON, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of lead poisoning.",
   ["Finding", "Result"],
   [(["Lead level", "48 mcg/dL"], True, "Needs chelation (45 or more)."),
    (["Old house being renovated", "Yes"], True, "Lead dust."),
    (["Pica", "Paint chips"], True, "Ingestion."),
    (["Speech regression", "Yes"], True, "Neurotoxicity."),
    (["Anemia", "Small cells"], True, "Lead blocks hemoglobin synthesis.")],
   "A high lead level with exposure and developmental change is lead poisoning.",
   "Recognize cues of lead poisoning."),

matrix_mc(M(PA, POISON, "analyze_cues", "difficult", PEDS, "peds"),
   "For each lead level, indicate the usual response.",
   ["Education and repeat test", "Case management and home inspection", "Chelation therapy"],
   [("3.5 to 9 mcg/dL", 0, "Above the reference value."),
    ("20 to 44 mcg/dL", 1, "Environmental investigation."),
    ("45 mcg/dL or more", 2, "Chelation."),
    ("Above 70 mcg/dL with encephalopathy", 2, "Emergency inpatient chelation.")],
   "Higher levels need stronger action.",
   "Analyze lead levels."),

cloze(M(PA, POISON, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The most important step is {{b1}}, along with {{b2}}.",
   [[("removing the child from the lead source", True, "Stops more exposure."),
     ("chelation while staying in the house", False, "Ongoing exposure."),
     ("an iron supplement alone", False, "Not enough."),
     ("waiting to retest in a year", False, "Too slow.")],
    [("oral succimer chelation", True, "For 45 to 69 mcg/dL without encephalopathy."),
     ("activated charcoal", False, "Does not bind lead."),
     ("vitamin K", False, "Not relevant."),
     ("deferoxamine", False, "For iron.")]],
   "Remove exposure and chelate.",
   "Prioritize lead poisoning."),

sata(M(SIC, HOME, "generate_solutions", "difficult", PEDS, "peds"),
   "Which home actions should the nurse teach?",
   [("Wet-mop and wipe sills", True, "Reduces dust."),
    ("Wash hands and toys", True, "Reduces ingestion."),
    ("Use cold tap water", True, "Less lead from pipes."),
    ("Iron and calcium foods", True, "Reduces lead absorption."),
    ("Dry-sweep the paint chips and dust every day", False, "Spreads dust."),
    ("Stay in the house during renovation", False, "High exposure.")],
   "Dust control, hygiene, cold water and nutrition.",
   "Generate lead hazard teaching.",
   ex=[note("Plan", "Family moves to a relative's home during abatement. Succimer started.", id="pb-plan")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", PEDS, "mgmt peds"),
   "Which actions should the nurse coordinate?",
   [("Test the 4-year-old sibling", True, "Shared exposure."),
    ("Report the level to the health department", True, "Reportable; home inspection."),
    ("Refer for early intervention for speech", True, "Developmental delay."),
    ("Check that the house is safe before return", True, "Abatement."),
    ("Wait to see if the speech improves on its own", False, "Delay."),
    ("Let the family return home as soon as chelation starts", False, "Re-exposure.")],
   "Sibling testing, reporting, early intervention and safe housing.",
   "Take action to coordinate lead care.",
   ex=[note("Week 1", "Succimer day 7.", id="pb-wk1")]),

matrix_mc(M(PA, POISON, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At 6 weeks, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Lead level 22 mcg/dL", 0, "Falling."),
    ("Hemoglobin 11 g/dL", 0, "Improving."),
    ("Attending speech therapy", 0, "Intervention."),
    ("Sibling's lead level 31 mcg/dL", 1, "Needs management."),
    ("Rebound rise in lead level after chelation", 1, "Lead leaving bone; recheck.")],
   "Falling level; sibling exposure and rebound need follow-up.",
   "Evaluate lead treatment.",
   ex=[note("6 weeks", "Home abatement complete.", id="pb-6wk")]),
]

cases = [
    case("Catch-up vaccination for a newly arrived child", cu_tabs, cu_qs),
    case("Bacterial meningitis in an infant", men_tabs, men_qs),
    case("Foreign body aspiration in a toddler", fb_tabs, fb_qs),
    case("Lead poisoning in a toddler", pb_tabs, pb_qs),
]

BUILT = build(150, "peds-cases-catchup-meningitis-foreign-body-lead-standalone",
              "Pediatric case studies: catch-up vaccines, infant meningitis, foreign body aspiration and lead "
              "poisoning, with four standalone items",
              items, cases, case_conversion=True)
