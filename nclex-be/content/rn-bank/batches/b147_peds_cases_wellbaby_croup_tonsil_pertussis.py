"""Batch 147 (case conversion, chunk 13: pediatrics part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: a 6-month well-baby visit (growth, milestones, vaccines, solids and safety); a hospitalized toddler with croup
compared with epiglottitis, with toddler development and separation anxiety; a preschooler bleeding after
tonsillectomy, with preschool fears; pertussis in an unvaccinated infant with cocooning and the vaccination schedule.
Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of
care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
GROWTH = "infant-growth-and-development"
IMMZ = "immunizations"
NUTR = "infant-and-child-nutrition"
RESP = "pediatric-respiratory-disorders"
BASICS = "pediatric-care-basics"
COMMUNICABLE = "childhood-communicable-diseases"
POSTOP = "post-procedure-complications"
HOME = "equipment-and-home-safety"
TEACH = "health-teaching-and-behavior-change"
COPING = "coping-and-adjustment"
CALC = "medication-dose-calculations"
RESP_MEDS = "respiratory-medications"

items = []

# ================================================================================================
# Case 1: 6-month well-baby visit
# ================================================================================================
wb_tabs = [
    ("wb-notes", "Clinic notes", [note("6-month visit",
        "A 6-month-old infant born at term is brought for a well visit. The infant is exclusively breastfed. The "
        "parent asks about starting solids and says the baby \"isn't sitting yet.\"", id="wb-note")]),
    ("wb-data", "Findings", [table("Findings", ["Result"], [
        ("Birth weight", ["3.4 kg"]),
        ("Weight today", ["6.9 kg"]),
        ("Development", ["Rolls both ways, sits with support, babbles, transfers toys hand to hand"]),
        ("Reflexes", ["Moro reflex still strong"]),
        ("Vaccines so far", ["Birth, 2 months and 4 months on time"]),
        ("Teeth", ["None yet"])], id="wb-table")]),
]

wb_qs = [
hl_table(M(HPM, GROWTH, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the finding that needs follow-up.",
   ["Finding", "Result"],
   [(["Moro reflex", "Still strong at 6 months"], True, "Should fade by about 4 to 6 months; check neurologic development."),
    (["Weight", "About double birth weight"], False, "Expected doubling by 5 to 6 months."),
    (["Sitting", "With support"], False, "Sits alone at 6 to 8 months."),
    (["Transfers toys", "Yes"], False, "Expected."),
    (["No teeth", "Yes"], False, "Normal; teeth erupt 6 to 12 months.")],
   "Most findings are normal; a persistent Moro reflex needs evaluation.",
   "Recognize cues at a 6-month visit."),

matrix_mc(M(HPM, GROWTH, "analyze_cues", "difficult", PEDS, "peds"),
   "For each milestone, indicate the age it is usually reached.",
   ["6 months", "9 months", "12 months", "15 months"],
   [("Birth weight doubled; sits with support", 0, "6 months."),
    ("Pincer grasp and stranger anxiety", 1, "9 months."),
    ("Birth weight tripled; walks holding furniture; says mama and dada with meaning", 2, "12 months."),
    ("Walks alone well; stacks two blocks", 3, "15 months."),
    ("Object permanence develops", 1, "Around 9 months.")],
   "Milestones guide well visits.",
   "Analyze infant milestones."),

cloze(M(HPM, NUTR, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The first solid food should be {{b1}}, and new foods are added {{b2}}.",
   [[("iron-fortified cereal or pureed meat", True, "Iron stores fall around 6 months."),
     ("cow's milk", False, "Not before 12 months."),
     ("honey on fruit", False, "Botulism before 12 months."),
     ("whole nuts", False, "Choking.")],
    [("one at a time, a few days apart", True, "Detects allergies."),
     ("several at once to save time", False, "Hard to identify allergies."),
     ("only after teeth appear", False, "Not needed."),
     ("in the bottle mixed with milk", False, "Choking and overfeeding.")]],
   "Iron-rich foods first, one at a time.",
   "Prioritize starting solids."),

matrix_mc(M(HPM, IMMZ, "generate_solutions", "difficult", PEDS, "peds acip"),
   "For each vaccine, indicate whether it is due at the 6-month visit for this infant.",
   ["Due today", "Not due today"],
   [("DTaP dose 3", 0, "2, 4, 6 months."),
    ("Pneumococcal conjugate dose 3", 0, "2, 4, 6 months."),
    ("Influenza (first season, dose 1 of 2)", 0, "From 6 months."),
    ("MMR", 1, "12 to 15 months."),
    ("Varicella", 1, "12 to 15 months."),
    ("Hepatitis B dose 3 (6 to 18 months)", 0, "Can be given now.")],
   "DTaP, PCV, flu and hepatitis B; live vaccines wait until 12 months.",
   "Generate the 6-month vaccine plan.",
   ex=[note("Plan", "Neurology referral for the persistent Moro reflex.", id="wb-plan")]),

sata(M(SIC, HOME, "take_action", "difficult", PEDS, "peds"),
   "Which safety teaching fits a 6-month-old?",
   [("Lower the crib mattress before the baby pulls to stand", True, "Falls."),
    ("Never leave the baby alone on a bed or changing table", True, "Rolling falls."),
    ("Keep small objects off the floor", True, "Choking with mouthing."),
    ("Keep the car seat rear-facing", True, "Car safety."),
    ("Use a baby walker to help the baby learn to walk", False, "Falls down stairs."),
    ("Put the baby to sleep on the tummy now that the baby can roll", False, "Back to sleep; let the baby roll.")],
   "Falls, choking and car seat safety.",
   "Take action with infant safety teaching.",
   ex=[note("Visit", "Parents have stairs at home.", id="wb-stairs")]),

matrix_mc(M(HPM, IMMZ, "evaluate_outcomes", "difficult", PEDS, "peds acip"),
   "For each statement by the parent after the vaccines, indicate whether it shows understanding or needs more "
   "teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"A low fever and a sore leg for a day or two are common.\"", 0, "Correct."),
    ("\"I'll call if the baby cries without stopping for 3 hours or has a seizure.\"", 0, "Correct."),
    ("\"We'll come back in a month for the second flu dose.\"", 0, "Two doses the first season."),
    ("\"I'll give aspirin for the fever.\"", 1, "Reye syndrome."),
    ("\"Breastfeeding during the shots can help with pain.\"", 0, "Correct.")],
   "Common reactions, warning signs and no aspirin.",
   "Evaluate vaccine teaching.",
   ex=[note("After vaccines", "Vaccines given.", id="wb-after")]),
]

# ================================================================================================
# Case 2: Croup compared with epiglottitis in a hospitalized toddler
# ================================================================================================
cr_tabs = [
    ("cr-notes", "Pediatric unit notes", [note("Admission",
        "A 2-year-old toddler has had a cold for 2 days and woke at night with a barking cough and noisy breathing. "
        "The toddler screams and clings to the parent when the nurse approaches.", id="cr-note")]),
    ("cr-data", "Findings", [flow("Findings", ["Arrival", "2 hours"], [
        ("Stridor", ["Only when crying", "At rest"]),
        ("Retractions", ["Mild", "Moderate"]),
        ("SpO2", ["96%", "91%"]),
        ("Temperature", ["38.0 C", "38.1 C"]),
        ("Drooling", ["No", "No"]),
        ("Behavior", ["Crying", "Restless, anxious"])], id="cr-flow")]),
]

cr_qs = [
hl_table(M(PA, RESP, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the 2-hour findings that show worsening croup.",
   ["Finding", "2 hours"],
   [(["Stridor", "At rest"], True, "Moderate to severe."),
    (["Retractions", "Moderate"], True, "Work of breathing."),
    (["SpO2", "91%"], True, "Hypoxemia is a late sign."),
    (["Restlessness", "Yes"], True, "Hypoxia."),
    (["Drooling", "No"], False, "Argues against epiglottitis.")],
   "Stridor at rest with retractions and falling oxygen means worsening croup.",
   "Recognize cues of worsening croup."),

matrix_mc(M(PA, RESP, "analyze_cues", "difficult", PEDS, "peds"),
   "For each feature, indicate whether it fits croup or epiglottitis.",
   ["Croup (laryngotracheobronchitis)", "Epiglottitis"],
   [("Barking cough after a cold, gradual onset", 0, "Croup."),
    ("Sudden high fever, drooling, sitting forward (tripod)", 1, "Epiglottitis."),
    ("Muffled voice and refuses to swallow", 1, "Epiglottitis."),
    ("Steeple sign on neck x-ray", 0, "Croup."),
    ("Unvaccinated for Hib", 1, "Risk.")],
   "Drooling, tripod position and toxic look suggest epiglottitis.",
   "Analyze croup versus epiglottitis."),

cloze(M(PPT, RESP_MEDS, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The toddler needs {{b1}} now, plus {{b2}}.",
   [[("nebulized racemic epinephrine", True, "Rapid airway swelling relief."),
     ("a throat swab", False, "Not now."),
     ("an albuterol inhaler only", False, "Not effective for croup."),
     ("a cough suppressant", False, "Not effective.")],
    [("a single dose of dexamethasone", True, "Reduces swelling for hours."),
     ("antibiotics", False, "Viral."),
     ("a sedative", False, "Masks distress."),
     ("IV fluids only", False, "Not the treatment.")]],
   "Racemic epinephrine and dexamethasone.",
   "Prioritize croup treatment."),

sata(M(PA, RESP, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse include?",
   [("Keep the toddler calm on the parent's lap", True, "Crying worsens stridor."),
    ("Observe at least 2 to 4 hours after racemic epinephrine", True, "Rebound."),
    ("Oxygen as needed", True, "Hypoxemia."),
    ("Continuous pulse oximetry", True, "Monitoring."),
    ("Separate the toddler from the parent for a full exam", False, "Increases distress."),
    ("Examine the throat with a tongue blade if epiglottitis is suspected", False, "Can cause airway obstruction.")],
   "Calm, observe, oxygen and monitoring.",
   "Generate the croup plan.",
   ex=[note("After treatment", "Stridor only with crying. SpO2 96%.", id="cr-after")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "Which actions fit the toddler's development during the hospital stay?",
   [("Parent rooms in", True, "Separation anxiety."),
    ("Keep home routines", True, "Ritual and routine."),
    ("Favorite blanket", True, "Comfort object."),
    ("Explain procedures days in advance", False, "Toddlers live in the present; explain just before."),
    ("Scold the toddler firmly each time the toddler says no", False, "Negativism is normal autonomy.")],
   "Rooming-in, routine, comfort objects and accepting regression.",
   "Take action with toddler-appropriate care.",
   ex=[note("Night 1", "Toddler cries when the parent leaves to shower.", id="cr-night")]),

matrix_mc(M(PA, RESP, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "Before discharge, for each finding or statement, indicate whether it supports discharge or needs follow-up.",
   ["Supports discharge", "Needs follow-up"],
   [("No stridor at rest for 4 hours", 0, "Stable."),
    ("Drinking well", 0, "Hydrated."),
    ("\"If the noisy breathing comes back at rest, we'll come in.\"", 0, "Correct."),
    ("New drooling and refusing to swallow", 1, "Possible bacterial tracheitis or epiglottitis."),
    ("SpO2 97% on room air", 0, "Stable.")],
   "Most findings support discharge; drooling needs urgent assessment.",
   "Evaluate croup recovery.",
   ex=[note("Morning", "Breakfast eaten.", id="cr-am")]),
]

# ================================================================================================
# Case 3: Post-tonsillectomy bleeding in a preschooler
# ================================================================================================
ton_tabs = [
    ("ton-notes", "Day surgery notes", [note("Recovery",
        "A 4-year-old child had a tonsillectomy and adenoidectomy this morning for sleep apnea. In the afternoon the "
        "child is sleepy and keeps swallowing.", id="ton-note")]),
    ("ton-data", "Findings", [flow("Findings", ["1 hour", "4 hours"], [
        ("Swallowing", ["Occasional", "Frequent"]),
        ("Heart rate", ["110", "148/min"]),
        ("Mouth", ["Dried blood on the teeth", "Bright red blood at the back of the throat"]),
        ("Vomit", ["None", "Dark, coffee-ground"]),
        ("Blood pressure", ["98/60", "94/58 mm Hg"]),
        ("Behavior", ["Sleepy", "Restless"])], id="ton-flow")]),
]

ton_qs = [
hl_table(M(RRP, POSTOP, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the 4-hour findings that suggest bleeding.",
   ["Finding", "4 hours"],
   [(["Frequent swallowing", "Yes"], True, "Swallowing blood."),
    (["Heart rate", "148/min"], True, "Early sign of blood loss in children."),
    (["Bright red blood", "In the throat"], True, "Active bleeding."),
    (["Restlessness", "Yes"], True, "Hypovolemia."),
    (["Blood pressure", "94/58"], False, "Children keep BP until late.")],
   "Frequent swallowing and rising heart rate after tonsillectomy mean bleeding.",
   "Recognize cues of post-tonsillectomy bleeding."),

matrix_mc(M(RRP, POSTOP, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding, indicate whether it is expected after tonsillectomy or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Dark old blood on the teeth", 0, "Expected."),
    ("Bad breath and a white coating in the throat on day 3", 0, "Normal healing."),
    ("Ear pain", 0, "Referred pain."),
    ("Bright red bleeding on day 7 to 10", 1, "Secondary bleed when the scab falls off."),
    ("Frequent swallowing while asleep", 1, "Bleeding.")],
   "Old blood and white scabs are normal; fresh bleeding is not.",
   "Analyze post-tonsillectomy findings."),

cloze(M(RRP, POSTOP, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, then {{b2}}.",
   [[("turn the child on the side and look in the throat with a light", True, "Airway and source."),
     ("give a cold drink with a straw", False, "Straws cause bleeding."),
     ("encourage coughing to clear the throat", False, "Worsens bleeding."),
     ("give an opioid and let the child sleep", False, "Masks signs.")],
    [("notify the surgeon and prepare for a return to the operating room", True, "Active bleed."),
     ("discharge home", False, "Unsafe."),
     ("give ibuprofen", False, "Increases bleeding in some protocols."),
     ("suction deeply with a hard catheter", False, "Trauma.")]],
   "Side-lying, look, notify.",
   "Prioritize post-tonsillectomy bleeding."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", PEDS, "peds"),
   "After the bleed is controlled, which parts of the plan should the nurse include?",
   [("Cool, soft foods and clear fluids", True, "Comfort."),
    ("No straws", True, "Suction causes bleeding."),
    ("No red or brown drinks", True, "Mistaken for blood."),
    ("Scheduled acetaminophen for pain", True, "Comfort improves drinking."),
    ("Crunchy foods like chips", False, "Scratch the throat."),
    ("Gargling and vigorous tooth brushing", False, "Bleeding.")],
   "Soft cool foods, no straws, no red drinks, pain control.",
   "Generate post-tonsillectomy care.",
   ex=[note("Return to OR", "Bleeding vessel cauterized.", id="ton-or")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "Which actions fit the preschooler's development?",
   [("Show it with a puppet", True, "Play."),
    ("Stress that the body is fixed", True, "Preschoolers fear body damage."),
    ("Say it is not a punishment", True, "Magical thinking."),
    ("Offer simple choices", True, "Initiative."),
    ("Use plain words like \"cut\" and \"take out\" so the child understands", False, "Frightening."),
    ("Give long, logical explanations of the surgery", False, "Too abstract.")],
   "Play, reassurance and simple choices.",
   "Take action with preschool-appropriate care.",
   ex=[note("Evening", "Child asks if the doctor will take out more parts.", id="ton-evening")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll call if there is bright red blood, especially 7 to 10 days after surgery.\"", 0, "Secondary bleed."),
    ("\"I'll offer popsicles but not red ones.\"", 0, "Correct."),
    ("\"Frequent swallowing during sleep means I should check on my child.\"", 0, "Correct."),
    ("\"My child can go back to preschool and rough play tomorrow.\"", 1, "Rest for about 2 weeks."),
    ("\"If my child won't drink, I'll wait a few days.\"", 1, "Dehydration.")],
   "Bleeding signs, fluids and activity limits.",
   "Evaluate tonsillectomy teaching.",
   ex=[note("Next day", "Drinking well.", id="ton-next")]),
]

# ================================================================================================
# Case 4: Pertussis in an unvaccinated infant
# ================================================================================================
per_tabs = [
    ("per-notes", "Pediatric unit notes", [note("Admission",
        "A 7-week-old infant has had a runny nose for a week and now has coughing fits that end in a gasp and "
        "vomiting. The parents chose to delay vaccines. A teenage sibling has had a cough for 3 weeks.", id="per-note")]),
    ("per-data", "Findings", [flow("Findings", ["Between fits", "During a fit"], [
        ("SpO2", ["97%", "78%"]),
        ("Color", ["Pink", "Blue around the lips"]),
        ("Heart rate", ["150", "80/min"]),
        ("Breathing", ["Normal", "Pause of 20 seconds after the fit"]),
        ("WBC", ["32,000 with lymphocytes", "-"])], id="per-flow")]),
]

per_qs = [
hl_table(M(SIC, COMMUNICABLE, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that make this a pertussis emergency.",
   ["Finding", "During a fit"],
   [(["SpO2", "78%"], True, "Severe hypoxia."),
    (["Apnea", "20-second pause"], True, "Infants may have apnea instead of the whoop."),
    (["Bradycardia", "80/min"], True, "Hypoxia."),
    (["Age", "7 weeks, unvaccinated"], True, "Highest death risk."),
    (["WBC", "32,000 lymphocytes"], True, "Severe disease marker.")],
   "Young infants with pertussis have apnea and hypoxia and can die.",
   "Recognize cues of infant pertussis."),

matrix_mc(M(SIC, COMMUNICABLE, "analyze_cues", "difficult", PEDS, "peds"),
   "For each disease, indicate the transmission-based precautions needed.",
   ["Droplet", "Airborne", "Contact"],
   [("Pertussis", 0, "Droplet."),
    ("Measles", 1, "Airborne."),
    ("Varicella", 1, "Airborne and contact."),
    ("Mumps", 0, "Droplet."),
    ("RSV", 2, "Contact.")],
   "Know the precautions for childhood infections.",
   "Analyze precautions for childhood infections."),

cloze(M(PPT, "antimicrobial-therapy", "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The infant needs {{b1}}, and close contacts including the sibling need {{b2}}.",
   [[("azithromycin and continuous monitoring for apnea", True, "Treats and protects."),
     ("amoxicillin", False, "Not effective."),
     ("cough syrup", False, "Not effective."),
     ("discharge with a home monitor only", False, "Unsafe.")],
    [("antibiotic prophylaxis", True, "Stops spread."),
     ("nothing if they feel well", False, "Spread."),
     ("only a chest x-ray", False, "Not enough."),
     ("isolation for 3 months", False, "Not needed.")]],
   "Azithromycin for the infant and contacts.",
   "Prioritize pertussis treatment."),

sata(M(SIC, COMMUNICABLE, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse include?",
   [("Droplet precautions", True, "Infectious."),
    ("Suction and oxygen ready", True, "Fits."),
    ("Small feeds after fits", True, "Vomiting."),
    ("Continuous monitoring", True, "Apnea."),
    ("Cough suppressants to reduce the fits", False, "Not effective in infants."),
    ("A shared room with an infant with RSV", False, "Cross-infection.")],
   "Droplet precautions, suction, feeding and monitoring.",
   "Generate pertussis care.",
   ex=[note("PCR", "Pertussis confirmed.", id="per-pcr")]),

sata(M(HPM, IMMZ, "take_action", "difficult", PEDS, "peds acip"),
   "The parents ask how to protect the baby and future children. Which teaching should the nurse give?",
   [("DTaP at 2, 4, 6 and 15 to 18 months, and at 4 to 6 years", True, "Schedule."),
    ("Tdap for the teenage sibling and adults around the baby", True, "Cocooning."),
    ("Tdap in every pregnancy between 27 and 36 weeks", True, "Passes antibodies to the baby."),
    ("Wait until age 2 to start vaccines so the immune system is stronger", False, "Leaves infants unprotected."),
    ("Natural infection gives better protection, so vaccines are not needed", False, "Dangerous.")],
   "DTaP schedule, cocooning and maternal Tdap.",
   "Take action with pertussis vaccine teaching.",
   ex=[note("Day 4", "Fewer fits. No apnea for 24 hours.", id="per-day4")]),

matrix_mc(M(HPM, IMMZ, "evaluate_outcomes", "difficult", PEDS, "peds acip"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"The cough may last weeks, even though the antibiotic is done.\"", 0, "The 100-day cough."),
    ("\"We'll start the 2-month vaccines this week.\"", 0, "Catch up."),
    ("\"The rest of the family took the antibiotic too.\"", 0, "Prophylaxis."),
    ("\"Now that the baby had pertussis, there's no need for the DTaP.\"", 1, "Immunity from infection wanes."),
    ("\"We'll call if the baby turns blue or stops breathing.\"", 0, "Correct.")],
   "Long cough, catch-up vaccines and prophylaxis.",
   "Evaluate pertussis teaching.",
   ex=[note("Day 7", "Feeding well.", id="per-day7")]),
]

cases = [
    case("Six-month well-baby visit", wb_tabs, wb_qs),
    case("Hospitalized toddler with croup", cr_tabs, cr_qs),
    case("Preschooler bleeding after tonsillectomy", ton_tabs, ton_qs),
    case("Pertussis in an unvaccinated infant", per_tabs, per_qs),
]

BUILT = build(147, "peds-cases-wellbaby-croup-tonsil-pertussis",
              "Pediatric case studies: well-baby visit, croup, tonsillectomy bleeding and pertussis",
              items, cases, case_conversion=True)
