"""Batch 145 (case conversion, chunk 12: pediatrics part 1): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: VP shunt malfunction with raised intracranial pressure in a toddler; a simple febrile seizure compared with
status epilepticus and rescue benzodiazepines; a newborn with myelomeningocele (sac protection, latex precautions,
hydrocephalus); abusive head trauma in an infant with mandatory reporting. Each case walks the six CJMM steps in
order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
NEURO = "pediatric-neurological-disorders"
BASICS = "pediatric-care-basics"
AED = "antiseizure-medications"
CALC = "medication-dose-calculations"
REPORT = "mandatory-reporting"
ABUSE = "abuse-and-neglect"
LATEX = "standard-precautions-and-hand-hygiene"
TEACH = "health-teaching-and-behavior-change"
GROWTH = "infant-growth-and-development"

items = []

# ================================================================================================
# Case 1: VP shunt malfunction
# ================================================================================================
vp_tabs = [
    ("vp-notes", "Emergency notes", [note("Emergency department",
        "A 2-year-old child with a VP shunt for hydrocephalus has vomited four times since waking without "
        "diarrhea. The parent says the child is \"not acting right\" and keeps holding the head.", id="vp-note")]),
    ("vp-data", "Findings", [flow("Findings", ["Arrival", "1 hour"], [
        ("Level of consciousness", ["Irritable", "Sleepy, hard to rouse"]),
        ("Heart rate", ["110", "72/min"]),
        ("Blood pressure", ["98/60", "124/58 mm Hg"]),
        ("Respirations", ["24", "Irregular, 14/min"]),
        ("Pupils", ["Equal, reactive", "Right pupil sluggish"]),
        ("Shunt tract", ["Normal", "Fluid swelling along the tubing"])], id="vp-flow")]),
]

vp_qs = [
hl_table(M(PA, NEURO, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the 1-hour findings that require immediate action.",
   ["Finding", "1 hour"],
   [(["Consciousness", "Hard to rouse"], True, "Rising ICP."),
    (["Heart rate", "72/min"], True, "Bradycardia; Cushing response."),
    (["Blood pressure", "124/58, wide pulse pressure"], True, "Cushing response."),
    (["Respirations", "Irregular"], True, "Brainstem compression."),
    (["Pupil", "Sluggish"], True, "Herniation risk.")],
   "Cushing triad with falling consciousness means shunt failure with rising ICP.",
   "Recognize cues of shunt malfunction."),

matrix_mc(M(PA, NEURO, "analyze_cues", "difficult", PEDS, "peds"),
   "For each sign, indicate whether it is an early or late sign of raised ICP in children.",
   ["Early sign", "Late sign"],
   [("Headache and vomiting, especially in the morning", 0, "Early."),
    ("Irritability and poor feeding", 0, "Early."),
    ("Bulging fontanel in an infant", 0, "Early in infants."),
    ("Bradycardia with irregular breathing", 1, "Late."),
    ("Fixed dilated pupil", 1, "Late.")],
   "Cushing triad and pupil changes are late.",
   "Analyze ICP signs in children."),

cloze(M(PA, NEURO, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should {{b1}}, and the child is likely to need {{b2}}.",
   [[("raise the head of the bed and call the neurosurgeon now", True, "Reduces ICP and gets help."),
     ("lay the child flat with legs raised", False, "Raises ICP."),
     ("pump the shunt reservoir repeatedly", False, "Only on a provider's direction."),
     ("give oral fluids for the vomiting", False, "Aspiration.")],
    [("emergency shunt revision", True, "Definitive."),
     ("an antiemetic and discharge", False, "Unsafe."),
     ("a lumbar puncture", False, "Herniation risk."),
     ("observation overnight", False, "Too slow.")]],
   "Head up and neurosurgery.",
   "Prioritize shunt malfunction."),

sata(M(PA, NEURO, "generate_solutions", "difficult", PEDS, "peds"),
   "After shunt revision, which parts of the plan should the nurse include?",
   [("Position as ordered, often flat at first to avoid overdrainage", True, "Prevents subdural."),
    ("Neurologic checks often", True, "Detects change."),
    ("Watch the incision and tract for redness or fluid", True, "Infection or leak."),
    ("Check temperature", True, "Shunt infection."),
    ("Pump the shunt to test it every hour", False, "Only as ordered."),
    ("Lay the child on the shunt side", False, "Pressure on the valve.")],
   "Positioning, neuro checks, site and temperature.",
   "Generate post-shunt-revision care.",
   ex=[note("Surgery", "Distal catheter blocked and replaced.", id="vp-surgery")]),

sata(M(MOC, "handoff-and-sbar-communication", "take_action", "difficult", PEDS, "mgmt peds"),
   "The nurse gives handoff to the PICU nurse. Which information is most important?",
   [("Parent-described baseline", True, "Comparison."),
    ("Surgery time, shunt setting", True, "Clinical."),
    ("Latest vitals and pupils", True, "Trend."),
    ("Past malfunction signs", True, "Early warning."),
    ("The parents' insurance details and billing contact", False, "Not for handoff."),
    ("A list of the child's favorite foods only", False, "Not essential.")],
   "Baseline, surgery details, trends and parent-observed signs.",
   "Take action with an effective handoff.",
   ex=[note("Transfer", "Moving to the PICU.", id="vp-transfer")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"Vomiting without diarrhea, headache or sleepiness means I call right away.\"", 0, "Malfunction."),
    ("\"Fever or redness along the tubing means infection.\"", 0, "Infection."),
    ("\"The shunt will need to be checked as my child grows.\"", 0, "Revisions."),
    ("\"I'll pump the shunt every morning to keep it working.\"", 1, "Only as directed."),
    ("\"My child can't ever play outside.\"", 1, "Normal play with sensible limits.")],
   "Know the signs; normal activity with limits.",
   "Evaluate shunt teaching.",
   ex=[note("Day 4", "Back to baseline.", id="vp-day4")]),
]

# ================================================================================================
# Case 2: Febrile seizure compared with status epilepticus
# ================================================================================================
fs_tabs = [
    ("fs-notes", "Emergency notes", [note("Emergency department",
        "A 20-month-old toddler with an ear infection had a 2-minute generalized seizure at home with a fever of "
        "39.6 C. The toddler is now sleepy but rousable. Fifteen minutes after arrival, the toddler has another "
        "seizure.", id="fs-note")]),
    ("fs-data", "Findings", [flow("Findings", ["Arrival", "Second seizure"], [
        ("Seizure duration", ["2 minutes at home", "Still seizing at 6 minutes"]),
        ("Type", ["Generalized", "Generalized"]),
        ("SpO2", ["97%", "86%"]),
        ("Glucose", ["92 mg/dL", "-"]),
        ("Neck", ["Supple", "-"]),
        ("IV access", ["None", "None"])], id="fs-flow")]),
]

fs_qs = [
hl_table(M(PA, NEURO, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that need immediate action during the second seizure.",
   ["Finding", "Second seizure"],
   [(["Duration", "6 minutes and ongoing"], True, "Treat seizures over 5 minutes."),
    (["SpO2", "86%"], True, "Hypoxia."),
    (["Recurrence", "Second seizure within hours"], True, "Not a simple febrile seizure."),
    (["No IV", "Yes"], True, "Needs a non-IV rescue route."),
    (["Glucose", "92"], False, "Normal.")],
   "A seizure over 5 minutes needs rescue medicine.",
   "Recognize cues of status epilepticus."),

matrix_mc(M(PA, NEURO, "analyze_cues", "difficult", PEDS, "peds"),
   "For each feature, indicate whether it fits a simple or complex febrile seizure.",
   ["Simple febrile seizure", "Complex febrile seizure"],
   [("Generalized, less than 15 minutes", 0, "Simple."),
    ("Focal features", 1, "Complex."),
    ("More than one in 24 hours", 1, "Complex."),
    ("Age 6 months to 5 years with fever", 0, "Typical."),
    ("Longer than 15 minutes", 1, "Complex.")],
   "Simple seizures are brief, generalized and single.",
   "Analyze febrile seizures."),

cloze(M(PPT, AED, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Without IV access, the first rescue medicine is {{b1}}, and if seizures continue, {{b2}} is next.",
   [[("intranasal or buccal midazolam", True, "Fast non-IV route."),
     ("oral acetaminophen", False, "Treats fever, not seizures."),
     ("IM phenytoin", False, "Not given IM."),
     ("a cold bath", False, "Not a treatment.")],
    [("a second benzodiazepine dose, then levetiracetam or fosphenytoin IV/IO", True, "Second-line."),
     ("waiting 30 minutes", False, "Brain injury risk."),
     ("oral diazepam", False, "Not during a seizure."),
     ("aspirin", False, "Reye syndrome.")]],
   "Benzodiazepine first, then a second-line drug.",
   "Prioritize status epilepticus."),

calc(M(PPT, CALC, "generate_solutions", "difficult", PEDS, "ph peds"),
   "Intranasal midazolam 0.2 mg/kg is prescribed. The toddler weighs 12 kg. The concentration is 5 mg/mL. How many "
   "mL should the nurse give? Record your answer to two decimal places.",
   "mL", 2, "Volume", 0.48,
   "0.2 x 12 = 2.4 mg; 2.4 / 5 = 0.48 mL, split between the nostrils.",
   "Calculate a midazolam rescue dose.",
   ex=[note("Orders", "Midazolam 0.2 mg/kg intranasal now. Oxygen. IO access if no IV.", id="fs-orders")]),

sata(M(SIC, "safe-client-handling", "take_action", "difficult", PEDS, "peds"),
   "Which actions should the nurse take during the seizure?",
   [("Turn the child on the side", True, "Airway."),
    ("Give oxygen by mask", True, "Hypoxia."),
    ("Time the seizure", True, "Guides treatment."),
    ("Move hard objects away", True, "Injury prevention."),
    ("Put a finger in the mouth to stop tongue biting", False, "Injury."),
    ("Hold the arms and legs still", False, "Injury.")],
   "Side-lying, oxygen, timing and safety.",
   "Take action during a seizure.",
   ex=[note("8 minutes", "Midazolam given at 7 minutes.", id="fs-8")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll give the nasal midazolam if a seizure lasts more than 5 minutes, then call 911.\"", 0, "Correct."),
    ("\"Fever medicine helps comfort but doesn't prevent febrile seizures.\"", 0, "Correct."),
    ("\"Most children grow out of febrile seizures.\"", 0, "Correct."),
    ("\"I'll put my child in a cold bath during a fever.\"", 1, "Shivering raises temperature."),
    ("\"I'll give aspirin for the fever.\"", 1, "Reye syndrome.")],
   "Rescue plan; no cold baths or aspirin.",
   "Evaluate febrile seizure teaching.",
   ex=[note("Next day", "No further seizures. EEG planned because of the complex features.", id="fs-next")]),
]

# ================================================================================================
# Case 3: Myelomeningocele
# ================================================================================================
mm_tabs = [
    ("mm-notes", "NICU notes", [note("NICU, 2 hours old",
        "A term newborn has a lumbar myelomeningocele found on prenatal ultrasound. The sac is intact. Surgery is "
        "planned within 48 hours.", id="mm-note")]),
    ("mm-data", "Findings", [table("Findings", ["Result"], [
        ("Sac", ["Intact, thin membrane, clear fluid"]),
        ("Legs", ["Little movement below the knees"]),
        ("Anal tone", ["Reduced"]),
        ("Head circumference", ["37 cm (above the 90th percentile)"]),
        ("Anterior fontanel", ["Soft"]),
        ("Bladder", ["Dribbling urine"])], id="mm-table")]),
]

mm_qs = [
hl_table(M(PA, NEURO, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that need ongoing nursing attention.",
   ["Finding", "Result"],
   [(["Sac", "Thin membrane"], True, "Rupture and infection risk."),
    (["Legs", "Little movement"], True, "Nerve damage below the lesion."),
    (["Bladder and anal tone", "Dribbling, reduced"], True, "Neurogenic bladder and bowel."),
    (["Head circumference", "Above 90th percentile"], True, "Hydrocephalus risk."),
    (["Fontanel", "Soft"], False, "Normal now.")],
   "Protect the sac and watch for hydrocephalus and bladder problems.",
   "Recognize cues in myelomeningocele."),

matrix_mc(M(PA, NEURO, "analyze_cues", "difficult", PEDS, "peds"),
   "For each problem, indicate whether it is associated with myelomeningocele.",
   ["Associated", "Not associated"],
   [("Hydrocephalus from Chiari II malformation", 0, "Common."),
    ("Latex allergy", 0, "High risk from repeated exposure."),
    ("Neurogenic bladder", 0, "Common."),
    ("Clubfoot and hip dislocation", 0, "Common."),
    ("Cyanotic heart disease", 1, "Not typical.")],
   "Myelomeningocele affects the brain, bladder, bowel and limbs.",
   "Analyze myelomeningocele."),

cloze(M(BCC, BASICS, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "Before surgery, the newborn should be placed {{b1}}, with the sac covered by {{b2}}.",
   [[("prone with the hips slightly flexed", True, "No pressure on the sac."),
     ("supine", False, "Pressure on the sac."),
     ("sitting in an infant seat", False, "Pressure."),
     ("side-lying on the sac", False, "Pressure.")],
    [("a sterile, moist, non-adherent dressing with saline", True, "Prevents drying and infection."),
     ("a dry gauze pad taped tightly", False, "Adheres and damages."),
     ("a diaper", False, "Contamination."),
     ("nothing", False, "Infection and drying.")]],
   "Prone with a sterile moist dressing.",
   "Prioritize preoperative myelomeningocele care."),

sata(M(SIC, LATEX, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the care plan should the nurse include?",
   [("Latex-free care", True, "Prevents sensitization."),
    ("Daily head circumference", True, "Hydrocephalus."),
    ("Drape below the sac", True, "Infection."),
    ("Check the sac", True, "Rupture and infection."),
    ("Diaper the newborn snugly to keep the sac clean", False, "Pressure and contamination."),
    ("Hold the newborn for feeds in the usual cradle position", False, "Pressure on the sac.")],
   "Latex-free care, head measurement, sac protection.",
   "Generate myelomeningocele care.",
   ex=[note("Day 1", "Surgery tomorrow.", id="mm-day1")]),

sata(M(PA, NEURO, "take_action", "difficult", PEDS, "peds"),
   "On day 5 after closure, review the findings. Which actions should the nurse take?",
   [("Notify the provider", True, "Hydrocephalus."),
    ("Measure the head again and compare", True, "Trend."),
    ("Check for vomiting and irritability", True, "Raised ICP."),
    ("Prepare the family for a possible shunt", True, "Common after closure."),
    ("Give an extra feed for the irritability", False, "Misses ICP."),
    ("Lay the newborn flat on the incision", False, "Pressure.")],
   "A rising head size and bulging fontanel after closure mean hydrocephalus.",
   "Take action on hydrocephalus.",
   ex=[flow("Day 5", ["Day 1", "Day 5"], [
       ("Head circumference", ["37 cm", "39.5 cm"]),
       ("Fontanel", ["Soft", "Bulging, tense"]),
       ("Eyes", ["Normal", "Sunset eyes"])], id="mm-day5")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "Before discharge, for each statement by the parent, indicate whether it shows understanding or needs more "
   "teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"We'll use clean intermittent catheterization as taught.\"", 0, "Bladder."),
    ("\"We'll avoid balloons and latex products at home.\"", 0, "Latex."),
    ("\"We'll check the legs and feet for pressure sores.\"", 0, "No sensation."),
    ("\"We'll take folic acid before our next pregnancy.\"", 0, "Prevention."),
    ("\"The bath water can be as warm as we like; the baby won't feel it.\"", 1, "Burn risk without sensation.")],
   "Bladder care, latex avoidance, skin checks and prevention.",
   "Evaluate myelomeningocele teaching.",
   ex=[note("Week 2", "VP shunt placed.", id="mm-wk2")]),
]

# ================================================================================================
# Case 4: Abusive head trauma
# ================================================================================================
aht_tabs = [
    ("aht-notes", "Emergency notes", [note("Emergency department",
        "A 4-month-old infant is brought by the parent's partner, who says the infant \"rolled off the couch\" "
        "onto a carpet. The infant is not feeding and is very sleepy. The infant cannot yet roll over, according "
        "to the clinic chart.", id="aht-note")]),
    ("aht-data", "Findings", [table("Findings", ["Result"], [
        ("Level of consciousness", ["Lethargic, high-pitched cry"]),
        ("Anterior fontanel", ["Bulging"]),
        ("Eyes", ["Retinal hemorrhages in both eyes"]),
        ("CT", ["Subdural hematomas of different ages"]),
        ("Skin", ["Small bruises on the upper arms"]),
        ("Story", ["Changed when the parent arrived"])], id="aht-table")]),
]

aht_qs = [
hl_table(M(PSI, ABUSE, "recognize_cues", "moderate", PEDS, "peds psych"),
   "Highlight the findings that suggest abusive head trauma.",
   ["Finding", "Result"],
   [(["History", "Rolled off a couch at an age when the infant cannot roll"], True, "Story does not fit development."),
    (["Retinal hemorrhages", "Both eyes"], True, "Shaking."),
    (["Subdurals", "Different ages"], True, "Repeated injury."),
    (["Bruises", "Upper arms"], True, "Grip marks."),
    (["Story changes", "Yes"], True, "Inconsistent.")],
   "Retinal and subdural bleeding with a history that does not fit development suggest abuse.",
   "Recognize cues of abusive head trauma."),

matrix_mc(M(HPM, GROWTH, "analyze_cues", "difficult", PEDS, "peds"),
   "For each milestone, indicate the age it is usually reached.",
   ["2 months", "4 months", "6 months", "9 months"],
   [("Social smile", 0, "2 months."),
    ("Rolls front to back", 1, "About 4 months."),
    ("Rolls back to front and sits with support", 2, "About 6 months."),
    ("Sits alone well and crawls", 3, "About 9 months."),
    ("Holds head steady when upright", 1, "About 4 months.")],
   "Knowing milestones helps judge whether a history fits the injury.",
   "Analyze infant development."),

cloze(M(MOC, REPORT, "prioritize_hypotheses", "difficult", PEDS, "mgmt peds"),
   "Complete the sentence by choosing from the lists of options.",
   "After stabilizing the infant, the nurse must {{b1}}, and the report requires {{b2}}.",
   [[("report suspected abuse to child protective services", True, "Mandated."),
     ("confront the partner", False, "Unsafe."),
     ("wait for the CT to be reviewed by a specialist first", False, "Suspicion is enough."),
     ("ask the parents' permission", False, "Not required.")],
    [("reasonable suspicion, not proof", True, "Legal standard."),
     ("a confession", False, "Not required."),
     ("two witnesses", False, "Not required."),
     ("a police officer present", False, "Not required.")]],
   "Report on suspicion.",
   "Prioritize mandatory reporting."),

sata(M(PA, NEURO, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse anticipate?",
   [("Neurologic checks and seizure precautions", True, "Brain injury."),
    ("Head of bed raised 30 degrees", True, "ICP."),
    ("Skeletal survey for other fractures", True, "Occult injuries."),
    ("Objective documentation with exact quotes from caregivers", True, "Legal record."),
    ("Allow the partner to stay alone with the infant", False, "Safety until assessed."),
    ("Write \"the partner abused the baby\" in the chart", False, "Opinion, not fact.")],
   "Neuro care, skeletal survey and objective documentation.",
   "Generate the AHT plan.",
   ex=[note("Plan", "PICU admission. Child protection team consulted.", id="aht-plan")]),

sata(M(PA, NEURO, "take_action", "difficult", PEDS, "peds"),
   "In the PICU, review the findings. Which actions should the nurse take?",
   [("Call the provider at once", True, "Herniation risk."),
    ("Protect the airway and give oxygen", True, "Apnea."),
    ("Prepare for neurosurgical intervention", True, "Rising ICP."),
    ("Give a feed to calm the infant", False, "Aspiration."),
    ("Lower the head of the bed flat", False, "Raises ICP.")],
   "Apnea, bradycardia and a fixed pupil mean herniation.",
   "Take action on rising ICP in an infant.",
   ex=[flow("PICU", ["Admission", "4 hours"], [
       ("Heart rate", ["150", "80/min"]),
       ("Breathing", ["Regular", "Pauses of 20 seconds"]),
       ("Pupils", ["Equal", "Left fixed and dilated"])], id="aht-picu")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds psych"),
   "When the infant improves, the nurse teaches the family about crying. For each statement, indicate whether it "
   "shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"Crying peaks at about 6 to 8 weeks and gets better after 3 to 4 months.\"", 0, "PURPLE crying."),
    ("\"If I get frustrated, I'll put the baby safely in the crib and walk away for a few minutes.\"", 0, "Safe."),
    ("\"I can ask a trusted person to take over.\"", 0, "Support."),
    ("\"A gentle shake will stop the crying.\"", 1, "Never shake."),
    ("\"Babies cry to make parents angry.\"", 1, "Infants cry to communicate needs.")],
   "Crying education prevents shaking.",
   "Evaluate crying and safety teaching.",
   ex=[note("Week 2", "Infant in foster care per CPS. Family attends education.", id="aht-wk2")]),
]

cases = [
    case("VP shunt malfunction in a toddler", vp_tabs, vp_qs),
    case("Febrile seizure progressing to status epilepticus", fs_tabs, fs_qs),
    case("Newborn with myelomeningocele", mm_tabs, mm_qs),
    case("Abusive head trauma in an infant", aht_tabs, aht_qs),
]

BUILT = build(145, "peds-cases-shunt-seizure-myelo-aht",
              "Pediatric neurologic case studies: shunt malfunction, febrile seizure, myelomeningocele and abusive "
              "head trauma",
              items, cases, case_conversion=True)
