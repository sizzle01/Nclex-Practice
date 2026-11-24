"""Batch 146 (case conversion, chunk 12: pediatrics part 1): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: acute lymphoblastic leukemia at diagnosis through induction (neutropenia, thrombocytopenia, tumor lysis risk
and family coping); hemophilia A with a knee bleed and factor VIII; iron deficiency anemia in a toddler drinking too
much cow's milk, with oral iron; iron tablet poisoning in a toddler. Standalone: a splenic sequestration bowtie in
sickle cell disease, a pediatric vital sign table by age, a priority question on a pediatric unit and a delegation
question on a pediatric unit. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and
includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
HEME_ONC = "pediatric-hematology-oncology"
SICKLE = "sickle-cell-disease"
BASICS = "pediatric-care-basics"
NUTR = "infant-and-child-nutrition"
POISON = "poisoning-and-ingestions"
HEME_MEDS = "hematologic-medications"
NEUTRO = "neutropenic-precautions"
CHEMO = "chemotherapy-and-radiation-care"
CALC = "medication-dose-calculations"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
HOME = "equipment-and-home-safety"
TEACH = "health-teaching-and-behavior-change"
COPING = "coping-and-adjustment"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, SICKLE, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "A toddler with sickle cell disease is brought in by a parent. Using the note, fill in the bowtie: the most "
   "likely problem in the middle, two actions to take now on the left and two parameters to monitor on the right.",
   [("Acute splenic sequestration", True, "Sudden large spleen, falling hemoglobin and shock."),
    ("Vaso-occlusive pain crisis", False, "Does not cause a rapidly enlarging spleen with shock."),
    ("Acute chest syndrome", False, "No chest findings."),
    ("Aplastic crisis", False, "Would show a low reticulocyte count and no big spleen.")],
   [("Give a fluid bolus", True, "Shock."),
    ("Prepare a transfusion", True, "Severe anemia."),
    ("Apply cold packs to the abdomen to ease the pain", False, "Cold causes sickling."),
    ("Encourage the toddler to walk to relieve pain", False, "Unsafe."),
    ("Give a diuretic to reduce the spleen size", False, "Worsens shock.")],
   [("Spleen size", True, "Response."),
    ("Pulse, BP", True, "Shock."),
    ("Body weight once a week", False, "Not urgent."),
    ("Hearing screen result", False, "Not related."),
    ("Bowel sounds every hour", False, "Not the main guide.")],
   "Sudden splenic enlargement with shock in a young child with sickle cell disease is sequestration.",
   "Recognize and act on splenic sequestration.",
   ex=[note("Emergency", "2-year-old, pale, lethargic. Spleen 6 cm below the rib margin (was not palpable last month). "
       "Hemoglobin 4.1 g/dL (baseline 8). HR 182/min, BP 70/40 mm Hg. Reticulocytes high.", id="sa-seq")]),

dtable(M(BCC, BASICS, "analyze_cues", "difficult", PEDS, "peds"),
   "For each child, choose the interpretation of the vital signs.",
   ["Child", "Vital signs", "Interpretation"],
   [(["Newborn, 2 days", "HR 145, RR 48"], [
       ("Normal for age", True, "Newborn HR 120 to 160, RR 30 to 60."),
       ("Tachycardic", False, "Normal for age."),
       ("Bradypneic", False, "Normal for age.")]),
    (["Toddler, 2 years", "HR 70, RR 10, sleepy"], [
       ("Dangerously slow for age; assess now", True, "Toddler HR 90 to 140, RR 24 to 40."),
       ("Normal for sleep", False, "Too slow."),
       ("Expected after a meal", False, "Too slow.")]),
    (["School-age, 8 years", "HR 90, RR 20, BP 104/66"], [
       ("Normal for age", True, "Within range."),
       ("Hypotensive", False, "Normal."),
       ("Tachycardic", False, "Normal.")])],
   "Children's normal vital signs change with age; bradycardia in a child is ominous.",
   "Analyze pediatric vital signs.",
   ex=[note("Unit", "Vital signs from three children on the pediatric unit.", id="sa-vs")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", PEDS, "mgmt peds"),
   "On a pediatric unit, four children need the nurse. Which child should the nurse see first?",
   [("A 6-year-old with leukemia and an ANC of 300 whose temperature just rose to 38.5 C", True,
     "Febrile neutropenia can progress to sepsis within hours."),
    ("A 10-year-old with a femur fracture in traction who rates pain 6 out of 10", False, "Needs analgesia soon."),
    ("A 3-year-old with gastroenteritis who has had one loose stool this shift", False, "Stable."),
    ("A 14-year-old with type 1 diabetes whose glucose before lunch is 210 mg/dL", False, "Correction insulin soon.")],
   "Fever with neutropenia is a time-critical emergency.",
   "Prioritize children on a pediatric unit."),

sata(M(MOC, DELEG, "take_action", "difficult", PEDS, "mgmt peds"),
   "A pediatric nurse is working with an assistant (UAP) this shift. Which of these jobs may the nurse hand over?",
   [("Weigh an infant on the same scale each morning", True, "Routine."),
    ("Record intake and output, including diaper weights", True, "Routine."),
    ("Feed a stable toddler without swallowing problems", True, "Routine."),
    ("Assess a child's pain after an opioid dose", False, "Assessment."),
    ("Teach parents how to give oral iron", False, "Teaching."),
    ("Decide whether a child with a fever needs a blood culture", False, "Clinical judgment.")],
   "UAPs weigh, measure and feed; assessment, teaching and decisions stay with the RN.",
   "Delegate on a pediatric unit."),
]

# ================================================================================================
# Case 1: Acute lymphoblastic leukemia
# ================================================================================================
all_tabs = [
    ("all-notes", "Pediatric oncology notes", [note("Diagnosis",
        "A 5-year-old child has had 3 weeks of tiredness, bone pain that wakes the child, easy bruising and nose "
        "bleeds.", id="all-note")]),
    ("all-data", "Results", [table("Results", ["Result"], [
        ("WBC", ["68,000/mm3 with blasts"]),
        ("ANC", ["400/mm3"]),
        ("Hemoglobin", ["7.1 g/dL"]),
        ("Platelets", ["18,000/mm3"]),
        ("Uric acid", ["8.9 mg/dL"]),
        ("Exam", ["Pale, petechiae, enlarged liver and spleen"])], id="all-table")]),
]

all_qs = [
hl_table(M(PA, HEME_ONC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that guide immediate nursing care.",
   ["Finding", "Result"],
   [(["ANC", "400"], True, "Infection risk."),
    (["Platelets", "18,000"], True, "Bleeding risk."),
    (["Hemoglobin", "7.1"], True, "Fatigue and possible transfusion."),
    (["WBC and uric acid", "68,000 with blasts, 8.9"], True, "Tumor lysis risk at the start of treatment."),
    (["Bone pain", "Yes"], True, "Marrow expansion; needs pain control.")],
   "Bone marrow failure causes infection, bleeding and anemia; high counts raise tumor lysis risk.",
   "Recognize cues of childhood leukemia."),

matrix_mc(M(PA, HEME_ONC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding, indicate the cause in leukemia.",
   ["Low neutrophils", "Low platelets", "Low red cells"],
   [("Fever and infections", 0, "Neutropenia."),
    ("Petechiae and nose bleeds", 1, "Thrombocytopenia."),
    ("Pallor and tiredness", 2, "Anemia."),
    ("Mouth sores that do not heal", 0, "Infection."),
    ("Bruising", 1, "Thrombocytopenia.")],
   "Leukemic cells crowd out normal marrow.",
   "Analyze leukemia findings."),

cloze(M(PA, HEME_ONC, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "As induction chemotherapy starts, the nurse must watch for {{b1}}, prevented by {{b2}}.",
   [[("tumor lysis syndrome", True, "High blast count."),
     ("hyperglycemia only", False, "Can occur with steroids but is not the main risk."),
     ("hypertension only", False, "Not the main risk."),
     ("liver failure", False, "Not the main risk.")],
    [("hyperhydration and allopurinol or rasburicase", True, "Prevents uric acid and electrolyte problems."),
     ("fluid restriction", False, "Worsens."),
     ("a high-potassium diet", False, "Worsens."),
     ("aspirin", False, "Bleeding.")]],
   "Hydrate and lower uric acid.",
   "Prioritize induction risks."),

sata(M(SIC, NEUTRO, "generate_solutions", "difficult", PEDS, "peds"),
   "Which precautions should the nurse include?",
   [("Strict hand hygiene for everyone", True, "Infection."),
    ("No rectal temperatures or suppositories", True, "Bleeding and infection."),
    ("Soft toothbrush or sponge swabs", True, "Bleeding."),
    ("Avoid contact sports and sharp toys", True, "Bleeding."),
    ("Fresh flowers and live plants in the room to cheer the child", False, "Infection."),
    ("Live vaccines for siblings are always forbidden", False, "Most are allowed; check with the team.")],
   "Infection and bleeding precautions.",
   "Generate leukemia precautions.",
   ex=[note("Plan", "Induction started. Central line placed.", id="all-plan")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "Which actions support the child and family during treatment?",
   [("Prepare with play", True, "Preschool development."),
    ("Offer small choices", True, "Control."),
    ("Include siblings", True, "Siblings are affected."),
    ("Child life and school help", True, "Normal development."),
    ("Avoid telling the child anything about the illness", False, "Children fear the unknown."),
    ("Keep the child in bed all day to save energy for healing", False, "Activity as tolerated.")],
   "Play, choices, siblings and school.",
   "Take action with family-centered care.",
   ex=[note("Week 1", "Child afraid of needles; sibling acting out at home.", id="all-wk1")]),

matrix_mc(M(PA, HEME_ONC, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At the end of induction (day 29), for each finding, indicate whether it shows a good response or needs "
   "follow-up.",
   ["Good response", "Needs follow-up"],
   [("No blasts in the bone marrow", 0, "Remission."),
    ("ANC 1,800/mm3", 0, "Recovery."),
    ("Platelets 140,000/mm3", 0, "Recovery."),
    ("Round face and weight gain from steroids", 0, "Expected; explain to the child."),
    ("Temperature 38.4 C with chills when the line is flushed", 1, "Central line infection.")],
   "Remission; a line infection needs treatment.",
   "Evaluate induction response.",
   ex=[note("Day 29", "Bone marrow done.", id="all-day29")]),
]

# ================================================================================================
# Case 2: Hemophilia A with a knee bleed
# ================================================================================================
hem_tabs = [
    ("hem-notes", "Emergency notes", [note("Emergency department",
        "A 9-year-old boy with severe hemophilia A twisted the right knee playing soccer 2 hours ago. The knee "
        "is hot, swollen and very painful, and he cannot bend it.", id="hem-note")]),
    ("hem-data", "Results", [table("Results", ["Result"], [
        ("Factor VIII activity", ["Less than 1% (severe)"]),
        ("aPTT", ["Prolonged"]),
        ("PT", ["Normal"]),
        ("Platelets", ["Normal"]),
        ("Knee", ["Tense, warm, limited movement"]),
        ("Weight", ["30 kg"])], id="hem-table")]),
]

hem_qs = [
hl_table(M(PA, HEME_ONC, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of a joint bleed (hemarthrosis).",
   ["Finding", "Result"],
   [(["Knee", "Hot, swollen, painful"], True, "Hemarthrosis."),
    (["Limited movement", "Cannot bend"], True, "Joint full of blood."),
    (["Factor VIII", "Less than 1%"], True, "Severe hemophilia."),
    (["aPTT", "Prolonged"], True, "Intrinsic pathway."),
    (["Platelets", "Normal"], False, "Platelets are not affected.")],
   "A hot swollen joint after minor injury in hemophilia is a joint bleed.",
   "Recognize cues of hemarthrosis."),

matrix_mc(M(PA, HEME_ONC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each statement, indicate whether it is accurate or inaccurate about hemophilia A.",
   ["Accurate", "Inaccurate"],
   [("It is X-linked recessive, so it mostly affects boys", 0, "Inheritance."),
    ("It prolongs the aPTT but not the PT", 0, "Labs."),
    ("Repeated joint bleeds can cause permanent joint damage", 0, "Hemophilic arthropathy."),
    ("Platelet transfusion is the main treatment", 1, "Factor VIII is replaced."),
    ("A head bleed is an emergency even after a minor bump", 0, "Life-threatening.")],
   "Factor VIII replacement treats hemophilia A.",
   "Analyze hemophilia."),

calc(M(PPT, CALC, "prioritize_hypotheses", "difficult", PEDS, "ph peds"),
   "Factor VIII is dosed to raise the level by 50%. One unit/kg raises the level by about 2%. The child weighs 30 kg. "
   "How many units should the nurse prepare? Record your answer as a whole number.",
   "units", 0, "Dose", 750,
   "Units = weight x desired rise / 2 = 30 x 50 / 2 = 750 units.",
   "Calculate a factor VIII dose."),

sata(M(PA, HEME_ONC, "generate_solutions", "difficult", PEDS, "peds ph"),
   "Which parts of the plan should the nurse include?",
   [("Factor VIII IV now", True, "Stops the bleed."),
    ("RICE", True, "Comfort."),
    ("Acetaminophen for pain", True, "Safe analgesic."),
    ("No IM shots or rectal temps", True, "Bleeding."),
    ("Ibuprofen for pain", False, "Antiplatelet effect."),
    ("Aspirate the joint at the bedside to relieve the pressure", False, "Not routine.")],
   "Factor first, RICE and safe analgesia.",
   "Generate the hemophilia plan.",
   ex=[note("Orders", "Recombinant factor VIII 750 units IV now. Recheck level.", id="hem-orders")]),

sata(M(HPM, TEACH, "take_action", "difficult", PEDS, "peds"),
   "Which teaching should the nurse give the child and parents?",
   [("Learn to give factor at home at the first sign of a bleed", True, "Early treatment."),
    ("Choose low-contact sports such as swimming", True, "Activity safely."),
    ("Wear a medical ID", True, "Emergency care."),
    ("Use an electric razor and soft toothbrush as he grows", True, "Bleeding prevention."),
    ("Avoid all physical activity forever", False, "Activity protects joints."),
    ("Wait to see if a head bump gets worse before calling", False, "Treat and call.")],
   "Home factor, safe sports, ID and bleeding prevention.",
   "Take action with hemophilia teaching.",
   ex=[note("4 hours", "Pain 3/10. Swelling reduced.", id="hem-4h")]),

matrix_mc(M(PA, HEME_ONC, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At follow-up, for each finding or statement, indicate whether it shows a good outcome or needs follow-up.",
   ["Good outcome", "Needs follow-up"],
   [("Full knee movement after physical therapy", 0, "Recovered."),
    ("Joined the swim team", 0, "Safe activity."),
    ("Factor VIII given but bleeding continues, with a new inhibitor test positive", 1, "Inhibitors need a different treatment."),
    ("Parent gives prophylactic factor three times a week", 0, "Prevention."),
    ("\"He fell off his bike and hit his head, but he seems fine.\"", 1, "Needs factor and evaluation.")],
   "Good recovery; inhibitors and head injury need action.",
   "Evaluate hemophilia care.",
   ex=[note("6 weeks", "Prophylaxis started.", id="hem-6wk")]),
]

# ================================================================================================
# Case 3: Iron deficiency anemia in a toddler
# ================================================================================================
ida_tabs = [
    ("ida-notes", "Clinic notes", [note("18-month well visit",
        "An 18-month-old toddler drinks about 1.4 liters (48 ounces) of cow's milk a day from a bottle and eats few "
        "solids. The parent says the child is a \"picky eater\" but \"chubby.\" The child is pale and irritable.",
        id="ida-note")]),
    ("ida-data", "Results", [table("Results", ["Result"], [
        ("Hemoglobin", ["8.4 g/dL"]),
        ("MCV", ["62 fL (low)"]),
        ("Ferritin", ["5 ng/mL (low)"]),
        ("Weight", ["90th percentile"]),
        ("Lead level", ["Normal"]),
        ("Stool", ["Positive for blood"])], id="ida-table")]),
]

ida_qs = [
hl_table(M(HPM, NUTR, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that explain the anemia.",
   ["Finding", "Result"],
   [(["Cow's milk", "1.4 L a day"], True, "Displaces iron-rich food; poor iron."),
    (["Bottle at 18 months", "Yes"], True, "Excess milk."),
    (["Stool blood", "Positive"], True, "Milk can irritate the gut and cause blood loss."),
    (["Low MCV and ferritin", "Yes"], True, "Iron deficiency."),
    (["Lead", "Normal"], False, "Excluded.")],
   "Too much cow's milk causes iron deficiency in toddlers.",
   "Recognize cues of iron deficiency anemia."),

matrix_mc(M(HPM, NUTR, "analyze_cues", "difficult", PEDS, "peds"),
   "For each statement, indicate whether it is accurate or inaccurate about toddler nutrition.",
   ["Accurate", "Inaccurate"],
   [("Toddlers should drink no more than about 16 to 24 ounces of milk a day", 0, "Limit."),
    ("Weaning from the bottle by 12 to 15 months is advised", 0, "Recommendation."),
    ("A heavy child cannot be anemic", 1, "Milk babies are often heavy."),
    ("Iron-rich foods include meat, beans and iron-fortified cereal", 0, "Correct."),
    ("Low-fat milk is recommended at 12 months", 1, "Whole milk until 2 years.")],
   "Limit milk, wean the bottle and add iron-rich foods.",
   "Analyze toddler nutrition."),

cloze(M(PPT, HEME_MEDS, "prioritize_hypotheses", "difficult", PEDS, "ph peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The plan is {{b1}} and to give oral iron {{b2}}.",
   [[("limit milk to about 16 to 20 ounces and add iron-rich foods", True, "Treats the cause."),
     ("stop all milk forever", False, "Not needed."),
     ("give a blood transfusion", False, "Not at this level."),
     ("switch to goat's milk", False, "Not iron-rich.")],
    [("between meals with juice, not with milk", True, "Absorption."),
     ("mixed in a full bottle of milk", False, "Calcium blocks absorption."),
     ("only when the child seems tired", False, "Daily."),
     ("with antacids", False, "Blocks absorption.")]],
   "Limit milk and give iron with vitamin C, not milk.",
   "Prioritize iron deficiency treatment."),

sata(M(PPT, HEME_MEDS, "generate_solutions", "difficult", PEDS, "ph peds"),
   "Which teaching about liquid iron should the nurse give?",
   [("Give toward the back, then brush", True, "Prevents staining."),
    ("Stools will turn dark green or black", True, "Expected."),
    ("Lock it away", True, "Poisoning."),
    ("Continue for months after", True, "Refills stores."),
    ("Stop the iron if the stools turn dark or black", False, "Expected effect."),
    ("Double the dose on days when the child seems pale", False, "Overdose.")],
   "Safe giving, expected stool change, storage and duration.",
   "Generate oral iron teaching.",
   ex=[note("Plan", "Ferrous sulfate 3 mg/kg/day elemental iron. Recheck hemoglobin in 4 weeks.", id="ida-plan")]),

sata(M(SIC, HOME, "take_action", "difficult", PEDS, "peds"),
   "During the visit, which home safety teaching fits an 18-month-old?",
   [("Lock up medicines, vitamins and cleaning products", True, "Poisoning."),
    ("Anchor furniture and use stair gates", True, "Falls."),
    ("Rear-facing car seat as long as possible", True, "Car safety."),
    ("Keep the Poison Control number handy", True, "Emergency."),
    ("Teach the child to swim instead of supervising near water", False, "Constant supervision."),
    ("Give whole grapes and popcorn as snacks", False, "Choking.")],
   "Poisoning, falls, car and water safety.",
   "Take action with toddler safety teaching.",
   ex=[note("Visit", "Parent mentions iron tablets for the parent are kept in a handbag.", id="ida-bag")]),

matrix_mc(M(HPM, NUTR, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At 4 weeks, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Hemoglobin 10.2 g/dL", 0, "Rising."),
    ("Reticulocytes up", 0, "Response."),
    ("\"We switched to a cup and give about 16 ounces of milk.\"", 0, "Correct."),
    ("\"I give the iron in the morning bottle of milk.\"", 1, "Blocks absorption."),
    ("Eating meat and beans most days", 0, "Diet.")],
   "Responding; iron in milk needs correction.",
   "Evaluate iron deficiency treatment.",
   ex=[note("4 weeks", "Less irritable.", id="ida-4wk")]),
]

# ================================================================================================
# Case 4: Iron poisoning
# ================================================================================================
ip_tabs = [
    ("ip-notes", "Emergency notes", [note("Emergency department",
        "A 2-year-old child (12 kg) swallowed an unknown number of a parent's ferrous sulfate 325 mg tablets "
        "(65 mg elemental iron each) about 2 hours ago. Up to 20 tablets are missing.", id="ip-note")]),
    ("ip-data", "Findings", [flow("Findings", ["Arrival", "4 hours"], [
        ("Vomiting", ["Several times, blood-streaked", "Stopped"]),
        ("Diarrhea", ["Bloody", "Stopped"]),
        ("Behavior", ["Lethargic", "Seems better, playing"]),
        ("Serum iron", ["520 mcg/dL", "-"]),
        ("Bicarbonate", ["16 mEq/L", "13 mEq/L"]),
        ("Abdominal x-ray", ["Many tablets in the stomach", "-"])], id="ip-flow")]),
]

ip_qs = [
hl_table(M(PA, POISON, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that show serious iron poisoning.",
   ["Finding", "Result"],
   [(["Bloody vomiting and diarrhea", "Yes"], True, "Corrosive GI injury."),
    (["Serum iron", "520 mcg/dL"], True, "Above 500 is severe."),
    (["Bicarbonate", "Falling"], True, "Metabolic acidosis."),
    (["X-ray", "Many tablets"], True, "More absorption to come."),
    (["Lethargy", "Yes"], True, "Systemic toxicity.")],
   "GI bleeding, acidosis and a high iron level mean severe toxicity.",
   "Recognize cues of iron poisoning."),

calc(M(PA, POISON, "analyze_cues", "difficult", PEDS, "peds"),
   "If the child swallowed all 20 tablets (65 mg elemental iron each), what dose of elemental iron per kg was taken? "
   "The child weighs 12 kg. Record your answer in mg/kg as a whole number.",
   "mg/kg", 0, "Dose", 108,
   "20 x 65 = 1,300 mg; 1,300 / 12 = 108 mg/kg. Above 60 mg/kg is potentially severe.",
   "Calculate an iron ingestion dose."),

cloze(M(PA, POISON, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The child seems better at 4 hours, but this is {{b1}}, and the antidote is {{b2}}.",
   [[("the latent phase before shock and liver failure", True, "False improvement."),
     ("full recovery", False, "Acidosis is worsening."),
     ("a sign the iron was not absorbed", False, "Not true."),
     ("an allergic reaction", False, "Not related.")],
    [("IV deferoxamine", True, "Chelates iron."),
     ("activated charcoal", False, "Does not bind iron."),
     ("syrup of ipecac", False, "Not used."),
     ("N-acetylcysteine", False, "For acetaminophen.")]],
   "False improvement; give deferoxamine.",
   "Prioritize iron poisoning."),

sata(M(PPT, "adverse-effects-of-medications", "generate_solutions", "difficult", PEDS, "ph peds"),
   "Which parts of the plan should the nurse anticipate?",
   [("Deferoxamine IV infusion", True, "Chelation."),
    ("Whole bowel irrigation for tablets in the gut", True, "Removes tablets."),
    ("IV fluids for losses", True, "Hypovolemia."),
    ("Monitor for hypotension during the infusion", True, "Deferoxamine effect."),
    ("Activated charcoal", False, "Does not bind iron."),
    ("Discharge when the child is playing", False, "Latent phase.")],
   "Deferoxamine, bowel irrigation, fluids and BP monitoring.",
   "Generate the iron poisoning plan.",
   ex=[note("Orders", "Deferoxamine 15 mg/kg/hr IV. Whole bowel irrigation. PICU.", id="ip-orders")]),

sata(M(PPT, "adverse-effects-of-medications", "take_action", "difficult", PEDS, "ph peds"),
   "During deferoxamine, the urine turns a red-orange (vin rosé) color and blood pressure falls to 76/44 mm Hg. "
   "Which actions should the nurse take?",
   [("Slow it and notify", True, "Rate-related hypotension."),
    ("Give a fluid bolus", True, "Hypotension."),
    ("Expect the urine color", True, "Expected sign."),
    ("Stop all IV fluids until the pressure recovers", False, "Worsens shock."),
    ("Treat the urine color as a transfusion reaction", False, "Expected.")],
   "Vin rosé urine is expected; hypotension needs a slower rate and fluids.",
   "Take action during deferoxamine.",
   ex=[note("6 hours", "Infusion running.", id="ip-6h")]),

matrix_mc(M(SIC, HOME, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "Before discharge, for each statement by the parent, indicate whether it shows understanding or needs more "
   "teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll keep all pills, including vitamins, in a locked cabinet.\"", 0, "Correct."),
    ("\"I'll call Poison Control right away if it happens again.\"", 0, "Correct."),
    ("\"Child-resistant caps mean the child can't open them.\"", 1, "Resistant, not proof."),
    ("\"I won't call the iron 'candy'.\"", 0, "Correct."),
    ("\"Keeping them in my handbag is safe enough.\"", 1, "Toddlers explore bags.")],
   "Locked storage and Poison Control.",
   "Evaluate poisoning prevention teaching.",
   ex=[note("Day 3", "Iron level normal. Liver tests normal.", id="ip-day3")]),
]

cases = [
    case("Acute lymphoblastic leukemia through induction", all_tabs, all_qs),
    case("Hemophilia A with a knee bleed", hem_tabs, hem_qs),
    case("Iron deficiency anemia from too much cow's milk", ida_tabs, ida_qs),
    case("Iron tablet poisoning in a toddler", ip_tabs, ip_qs),
]

BUILT = build(146, "peds-cases-all-hemophilia-ida-iron-standalone",
              "Pediatric case studies: leukemia, hemophilia, iron deficiency anemia and iron poisoning, with four "
              "standalone items",
              items, cases, case_conversion=True)
