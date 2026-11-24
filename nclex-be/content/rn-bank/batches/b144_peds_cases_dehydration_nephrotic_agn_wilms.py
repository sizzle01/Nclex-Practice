"""Batch 144 (case conversion, chunk 12: pediatrics part 1): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: an infant with gastroenteritis and moderate to severe dehydration (oral rehydration, maintenance fluid
calculation and contact precautions); nephrotic syndrome on prednisone; acute post-streptococcal glomerulonephritis
with hypertension; Wilms tumor (nephroblastoma) before surgery. Each case walks the six CJMM steps in order, unfolds
with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
DEHYDRATION = "infant-dehydration"
GU = "pediatric-gi-gu-disorders"
HEME_ONC = "pediatric-hematology-oncology"
BASICS = "pediatric-care-basics"
CALC = "medication-dose-calculations"
TBP = "transmission-based-precautions"
STEROIDS = "endocrine-medications"
TEACH = "health-teaching-and-behavior-change"
NUTR = "infant-and-child-nutrition"
CHEMO = "chemotherapy-and-radiation-care"
COMM = "therapeutic-communication"

items = []

# ================================================================================================
# Case 1: Infant dehydration
# ================================================================================================
dh_tabs = [
    ("dh-notes", "Emergency notes", [note("Emergency department",
        "A 9-month-old infant (usual weight 9 kg) has had vomiting and watery diarrhea for 2 days. Rotavirus is "
        "spreading at the infant's daycare. The infant has had one wet diaper in 12 hours.", id="dh-note")]),
    ("dh-data", "Findings", [table("Findings", ["Result"], [
        ("Weight today", ["8.2 kg"]),
        ("Heart rate", ["176/min"]),
        ("Capillary refill", ["3 seconds"]),
        ("Anterior fontanel", ["Sunken"]),
        ("Mucous membranes", ["Dry; no tears when crying"]),
        ("Behavior", ["Irritable, then sleepy"]),
        ("Sodium", ["137 mEq/L"])], id="dh-table")]),
]

dh_qs = [
hl_table(M(PA, DEHYDRATION, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of significant dehydration.",
   ["Finding", "Result"],
   [(["Weight", "Lost 0.8 kg (about 9%)"], True, "Moderate to severe."),
    (["Heart rate", "176/min"], True, "Compensation."),
    (["Capillary refill", "3 seconds"], True, "Poor perfusion."),
    (["Fontanel and tears", "Sunken, none"], True, "Fluid deficit."),
    (["Urine", "One diaper in 12 hours"], True, "Low output."),
    (["Sodium", "137"], False, "Normal; isotonic dehydration.")],
   "A 9% weight loss with tachycardia, sunken fontanel and poor output is moderate to severe dehydration.",
   "Recognize cues of infant dehydration."),

calc(M(PA, DEHYDRATION, "analyze_cues", "difficult", PEDS, "peds"),
   "Using the Holliday-Segar method (100 mL/kg/day for the first 10 kg), what is this infant's daily maintenance "
   "fluid requirement based on the usual weight of 9 kg? Record your answer in mL/day.",
   "mL/day", 0, "Maintenance", 900,
   "100 mL x 9 kg = 900 mL/day, about 38 mL/hr, before replacing the deficit.",
   "Calculate maintenance fluids."),

cloze(M(PA, DEHYDRATION, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "Because the infant is becoming sleepy with slow capillary refill, the priority is {{b1}}, and the best fluid "
   "is {{b2}}.",
   [[("IV fluid resuscitation", True, "Severe signs."),
     ("oral rehydration only at home", False, "Not with lethargy."),
     ("clear juice", False, "Too much sugar, too little sodium."),
     ("withholding all fluids", False, "Dangerous.")],
    [("a 20 mL/kg bolus of 0.9% sodium chloride", True, "Isotonic bolus."),
     ("D5W bolus", False, "Hypotonic; hyponatremia."),
     ("plain water", False, "Hyponatremia."),
     ("a sports drink", False, "Wrong composition.")]],
   "IV isotonic bolus for severe signs.",
   "Prioritize dehydration treatment."),

sata(M(PA, DEHYDRATION, "generate_solutions", "difficult", PEDS, "peds"),
   "After the bolus, the infant is more alert. Which parts of the plan should the nurse include?",
   [("ORS in small frequent sips", True, "Tolerated."),
    ("Weigh diapers to measure output", True, "1 g = 1 mL."),
    ("Resume usual feeds early", True, "Early refeeding."),
    ("Add potassium after voiding", True, "Kidney function first."),
    ("Give clear apple juice and flat soda to settle the stomach", False, "High sugar worsens diarrhea."),
    ("Give loperamide to slow down and stop the diarrhea", False, "Not for infants.")],
   "ORS in small amounts, diaper weights, early feeding and potassium after voiding.",
   "Generate the rehydration plan.",
   ex=[note("1 hour", "Heart rate 140/min. Refill 2 seconds. Voided once.", id="dh-1h")]),

sata(M(SIC, TBP, "take_action", "difficult", PEDS, "peds"),
   "Which infection control actions should the nurse take for rotavirus?",
   [("Contact precautions with gown and gloves", True, "Spread by stool."),
    ("Hand washing with soap and water", True, "Effective."),
    ("Dedicated equipment such as a stethoscope", True, "Prevents spread."),
    ("Teach that rotavirus vaccine prevents severe disease in siblings", True, "Prevention."),
    ("Airborne precautions with an N95", False, "Not airborne."),
    ("Share toys with other children in the playroom", False, "Spreads the virus.")],
   "Contact precautions, soap and water, dedicated equipment and vaccination.",
   "Take action for rotavirus infection control.",
   ex=[note("Day 1", "Stool test: rotavirus positive. Infant missed the rotavirus vaccine.", id="dh-rota")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll give ORS after each loose stool.\"", 0, "Replaces losses."),
    ("\"I'll come back if there are fewer than 4 wet diapers a day.\"", 0, "Warning sign."),
    ("\"I'll keep feeding the usual formula.\"", 0, "Correct."),
    ("\"I'll dilute the formula with extra water to be gentle.\"", 1, "Hyponatremia."),
    ("\"I'll give homemade salt water if we run out of ORS.\"", 1, "Wrong concentration.")],
   "ORS replacement and warning signs; never dilute formula.",
   "Evaluate dehydration teaching.",
   ex=[note("Discharge", "Weight 8.8 kg. Tolerating feeds.", id="dh-dc")]),
]

# ================================================================================================
# Case 2: Nephrotic syndrome
# ================================================================================================
ns_tabs = [
    ("ns-notes", "Pediatric unit notes", [note("Admission",
        "A 4-year-old child has had puffy eyes in the morning for a week and now has swelling of the legs and "
        "scrotum. The child is tired and has a poor appetite.", id="ns-note")]),
    ("ns-data", "Results", [table("Results", ["Result"], [
        ("Urine protein", ["4+ (more than 2 g/day)"]),
        ("Serum albumin", ["1.6 g/dL"]),
        ("Cholesterol", ["420 mg/dL"]),
        ("Blood pressure", ["98/62 mm Hg"]),
        ("Urine", ["Frothy, dark, no blood"]),
        ("Weight", ["Up 2.5 kg"])], id="ns-table")]),
]

ns_qs = [
hl_table(M(PA, GU, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of nephrotic syndrome.",
   ["Finding", "Result"],
   [(["Protein", "Heavy proteinuria"], True, "Main feature."),
    (["Albumin", "1.6 g/dL"], True, "Hypoalbuminemia."),
    (["Cholesterol", "420 mg/dL"], True, "Hyperlipidemia."),
    (["Edema", "Eyes, legs, scrotum"], True, "Low oncotic pressure."),
    (["Blood pressure", "98/62"], False, "Normal; unlike nephritic syndrome.")],
   "Massive proteinuria, low albumin, high lipids and edema.",
   "Recognize cues of nephrotic syndrome."),

matrix_mc(M(PA, GU, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding, indicate whether it fits nephrotic syndrome or acute glomerulonephritis.",
   ["Nephrotic syndrome", "Acute glomerulonephritis"],
   [("Heavy protein, no blood in urine", 0, "Nephrotic."),
    ("Cola-colored urine with red cells", 1, "Nephritic."),
    ("Hypertension", 1, "Fluid retention."),
    ("Very low albumin and high cholesterol", 0, "Nephrotic."),
    ("Recent strep throat", 1, "Post-infectious.")],
   "Protein and edema versus blood and hypertension.",
   "Analyze nephrotic versus nephritic syndrome."),

cloze(M(PPT, STEROIDS, "prioritize_hypotheses", "difficult", PEDS, "peds ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The main treatment is {{b1}}, and the child is at risk for {{b2}}.",
   [[("oral prednisone", True, "Most children respond."),
     ("antibiotics", False, "Not infection."),
     ("dialysis", False, "Not usually."),
     ("a high-protein diet only", False, "Not the treatment.")],
    [("infections and blood clots", True, "Loss of antibodies and anticoagulant proteins."),
     ("hypertensive crisis", False, "Not typical."),
     ("hyperkalemia", False, "Not typical."),
     ("bleeding", False, "Clots, not bleeding.")]],
   "Prednisone; watch for infection and clots.",
   "Prioritize nephrotic syndrome."),

sata(M(BCC, BASICS, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the plan should the nurse include?",
   [("Daily weight and abdominal girth", True, "Fluid status."),
    ("Urine dipstick for protein each morning", True, "Response."),
    ("Skin care and turning, supporting the scrotum", True, "Edematous skin breaks down."),
    ("A no-added-salt diet", True, "Reduces edema."),
    ("Strict fluid restriction to 500 mL", False, "Only if severe; not routine."),
    ("Room with a child who has chickenpox", False, "Infection risk on steroids.")],
   "Weights, dipsticks, skin care and salt limits.",
   "Generate the nephrotic plan.",
   ex=[note("Orders", "Prednisone 2 mg/kg/day. No-added-salt diet. Daily weight.", id="ns-orders")]),

sata(M(PA, GU, "take_action", "difficult", PEDS, "peds"),
   "On day 5, review the findings. Which actions should the nurse take?",
   [("Notify the provider", True, "Peritonitis is a known complication."),
    ("Prepare for cultures and IV antibiotics", True, "Infection."),
    ("Check vital signs and perfusion often", True, "Sepsis."),
    ("Give acetaminophen and reassess tomorrow", False, "Masks fever."),
    ("Increase the prednisone dose without an order", False, "Not appropriate.")],
   "Fever with abdominal pain in nephrotic syndrome suggests spontaneous peritonitis.",
   "Take action on infection in nephrotic syndrome.",
   ex=[flow("Day 5", ["Day 4", "Day 5"], [
       ("Temperature", ["37.0", "38.9 C"]),
       ("Abdomen", ["Distended, soft", "Tender, guarding"]),
       ("Heart rate", ["100", "142/min"])], id="ns-day5")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds ph"),
   "At discharge, for each statement by the parent, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll test the urine each morning and call if it shows protein for 3 days.\"", 0, "Relapse."),
    ("\"I'll give prednisone in the morning with food.\"", 0, "Correct."),
    ("\"We'll avoid live vaccines while on high-dose prednisone.\"", 0, "Correct."),
    ("\"I'll stop the prednisone when the swelling goes.\"", 1, "Must be tapered."),
    ("\"Increased appetite and a round face are side effects of the medicine.\"", 0, "Correct.")],
   "Home testing, correct steroid use and no live vaccines.",
   "Evaluate nephrotic syndrome teaching.",
   ex=[note("Day 10", "Urine protein negative. Weight down 2 kg.", id="ns-day10")]),
]

# ================================================================================================
# Case 3: Acute post-streptococcal glomerulonephritis
# ================================================================================================
agn_tabs = [
    ("agn-notes", "Clinic notes", [note("Pediatric clinic",
        "A 7-year-old child had a sore throat 2 weeks ago that was not tested. Today the child has puffy eyes, a "
        "headache and dark urine.", id="agn-note")]),
    ("agn-data", "Results", [table("Results", ["Result"], [
        ("Urine", ["Cola-colored, red cells and red cell casts"]),
        ("Urine protein", ["1+"]),
        ("Blood pressure", ["148/96 mm Hg (above the 99th percentile)"]),
        ("ASO titer", ["Raised"]),
        ("Complement C3", ["Low"]),
        ("Urine output", ["Low"]),
        ("Weight", ["Up 1.5 kg"])], id="agn-table")]),
]

agn_qs = [
hl_table(M(PA, GU, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings of acute post-streptococcal glomerulonephritis.",
   ["Finding", "Result"],
   [(["Urine", "Cola-colored, casts"], True, "Hematuria."),
    (["Blood pressure", "148/96"], True, "Hypertension."),
    (["Recent strep", "2 weeks ago; ASO raised"], True, "Post-infectious."),
    (["C3", "Low"], True, "Complement consumed."),
    (["Protein", "1+"], False, "Mild; not the main feature.")],
   "Hematuria, hypertension and edema after strep is APSGN.",
   "Recognize cues of APSGN."),

matrix_mc(M(PA, GU, "analyze_cues", "difficult", PEDS, "peds"),
   "For each finding, indicate its cause in APSGN.",
   ["Immune complexes in the glomeruli", "Fluid and sodium retention"],
   [("Red cells and casts in the urine", 0, "Inflamed glomeruli."),
    ("High blood pressure", 1, "Volume."),
    ("Periorbital edema", 1, "Volume."),
    ("Low complement", 0, "Consumed by immune complexes."),
    ("Headache", 1, "Hypertension.")],
   "Immune injury reduces filtration and causes fluid retention.",
   "Analyze APSGN."),

cloze(M(PA, GU, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The greatest risk now is {{b1}}, so the nurse should {{b2}}.",
   [[("hypertensive encephalopathy with seizures", True, "Severe hypertension with headache."),
     ("dehydration", False, "Fluid overload is present."),
     ("hypoglycemia", False, "Not related."),
     ("bleeding", False, "Not related.")],
    [("check blood pressure and neurologic status often", True, "Early detection."),
     ("encourage extra fluids", False, "Worsens overload."),
     ("give a high-sodium diet", False, "Worsens."),
     ("start strenuous activity", False, "Not now.")]],
   "Watch blood pressure and neurologic signs.",
   "Prioritize APSGN."),

sata(M(PA, GU, "generate_solutions", "difficult", PEDS, "peds ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Sodium and fluid restriction", True, "Fluid overload."),
    ("Furosemide", True, "Removes fluid."),
    ("An antihypertensive such as amlodipine", True, "BP control."),
    ("Daily weights and intake and output", True, "Fluid."),
    ("High-potassium foods for energy", False, "Low output raises potassium."),
    ("Steroids as first-line treatment", False, "APSGN is self-limiting; steroids are not routine.")],
   "Fluid and salt limits, diuretic, BP control and weights.",
   "Generate the APSGN plan.",
   ex=[note("Orders", "Admit. Fluid restriction to insensible losses plus output. Furosemide.", id="agn-orders")]),

sata(M(PA, GU, "take_action", "difficult", PEDS, "peds"),
   "On day 2, the child says the light hurts the eyes and vomits. Review the findings. Which actions should the "
   "nurse take?",
   [("Notify the provider now", True, "Hypertensive encephalopathy."),
    ("Seizure precautions", True, "Risk."),
    ("Keep the room quiet and dim", True, "Comfort."),
    ("Give an IV antihypertensive as prescribed", True, "Lower BP."),
    ("Encourage the child to drink to replace the vomit", False, "Fluid overload."),
    ("Let the child play in the playroom", False, "Unsafe.")],
   "Headache, photophobia and vomiting with high BP suggest encephalopathy.",
   "Take action on hypertensive encephalopathy.",
   ex=[flow("Day 2", ["Day 1", "Day 2"], [
       ("Blood pressure", ["148/96", "168/112 mm Hg"]),
       ("Headache", ["Mild", "Severe"]),
       ("Potassium", ["4.6", "5.6 mEq/L"])], id="agn-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At 2 weeks, for each finding or statement, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Blood pressure 104/66 mm Hg", 0, "Resolved."),
    ("Urine output normal", 0, "Recovered."),
    ("Microscopic blood in the urine", 0, "Can persist for months."),
    ("\"Next time, we'll have sore throats tested and finish antibiotics.\"", 0, "Prevention."),
    ("Rising creatinine", 1, "Not expected; may not be APSGN.")],
   "Most children recover fully; rising creatinine needs evaluation.",
   "Evaluate APSGN recovery.",
   ex=[note("2 weeks", "Activity back to normal.", id="agn-2wk")]),
]

# ================================================================================================
# Case 4: Wilms tumor
# ================================================================================================
wt_tabs = [
    ("wt-notes", "Pediatric clinic notes", [note("Clinic",
        "A parent noticed a firm swelling on one side of a 3-year-old child's abdomen while bathing the child. The "
        "child is otherwise playful.", id="wt-note")]),
    ("wt-data", "Findings", [table("Findings", ["Result"], [
        ("Abdomen", ["Firm, smooth mass on the right flank; does not cross the midline"]),
        ("Blood pressure", ["124/80 mm Hg (high for age)"]),
        ("Urine", ["Microscopic blood"]),
        ("Eyes", ["No iris present (aniridia) in the right eye"]),
        ("CT", ["Right kidney mass, suspected Wilms tumor"])], id="wt-table")]),
]

wt_qs = [
hl_table(M(PA, HEME_ONC, "recognize_cues", "moderate", PEDS, "peds ms"),
   "Highlight the findings that support Wilms tumor.",
   ["Finding", "Result"],
   [(["Abdominal mass", "Firm, one side"], True, "Typical."),
    (["Blood pressure", "High"], True, "Renin release."),
    (["Hematuria", "Microscopic"], True, "Kidney involvement."),
    (["Aniridia", "Yes"], True, "Associated syndrome."),
    (["Playful", "Yes"], False, "Children often look well.")],
   "A firm flank mass in a well-appearing preschooler suggests Wilms tumor.",
   "Recognize cues of Wilms tumor."),

matrix_mc(M(PA, HEME_ONC, "analyze_cues", "difficult", PEDS, "peds"),
   "For each feature, indicate whether it fits Wilms tumor or neuroblastoma.",
   ["Wilms tumor", "Neuroblastoma"],
   [("Smooth flank mass that does not cross the midline", 0, "Wilms."),
    ("Irregular mass that crosses the midline", 1, "Neuroblastoma."),
    ("Raised urine catecholamines", 1, "Neuroblastoma."),
    ("Associated with aniridia", 0, "Wilms."),
    ("Bruising around the eyes from orbital spread", 1, "Neuroblastoma.")],
   "Features help separate the two common abdominal tumors.",
   "Analyze pediatric abdominal tumors."),

cloze(M(SIC, "safe-client-handling", "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The most important nursing rule before surgery is {{b1}}, because {{b2}}.",
   [[("do not palpate the abdomen; post a sign at the bed", True, "Prevents rupture."),
     ("palpate the mass each shift to measure it", False, "Risk of rupture."),
     ("bathe the child with vigorous scrubbing", False, "Pressure on the mass."),
     ("encourage rough play", False, "Rupture risk.")],
    [("rupture of the capsule can spread tumor cells", True, "Seeding."),
     ("it causes pain only", False, "Not the main reason."),
     ("it raises blood pressure", False, "Not the main reason."),
     ("it causes infection", False, "Not the main reason.")]],
   "No palpation; handle gently.",
   "Prioritize preoperative Wilms care."),

sata(M(PA, HEME_ONC, "generate_solutions", "difficult", PEDS, "peds ms"),
   "Which parts of the preoperative plan should the nurse include?",
   [("Blood pressure monitoring", True, "Hypertension."),
    ("Careful bathing and handling", True, "Avoid pressure."),
    ("Age-appropriate preparation with dolls or pictures", True, "Preschool understanding."),
    ("Baseline kidney function tests", True, "One kidney will remain."),
    ("Abdominal palpation teaching for the parents", False, "Avoid palpation."),
    ("Delay surgery until after a month of play therapy", False, "Surgery is prompt.")],
   "BP, gentle handling, preparation and kidney tests.",
   "Generate the Wilms preoperative plan.",
   ex=[note("Plan", "Nephrectomy in 2 days, then chemotherapy.", id="wt-plan")]),

sata(M(PSI, COMM, "take_action", "difficult", PEDS, "peds psych"),
   "The parent cries and says, \"I should have noticed it sooner.\" Which responses are therapeutic?",
   [("\"These tumors are often silent; you found it.\"", True, "Accurate, reduces guilt."),
    ("\"It sounds like you're feeling guilty.\"", True, "Reflects."),
    ("\"Would you like our family support team?\"", True, "Resources."),
    ("\"Yes, a careful parent would have seen it earlier at bath time.\"", False, "Blames."),
    ("\"Don't cry in here; your child will see you upset and get scared.\"", False, "Dismisses feelings.")],
   "Reduce guilt, reflect and offer support.",
   "Take action with parent support.",
   ex=[note("Day 1", "Parent at the bedside.", id="wt-day1")]),

matrix_mc(M(PA, HEME_ONC, "evaluate_outcomes", "difficult", PEDS, "peds ms"),
   "After nephrectomy, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Urine output 1.5 mL/kg/hr", 0, "Adequate."),
    ("Bowel sounds absent with a distended, tender abdomen on day 3", 1, "Possible obstruction."),
    ("Pain controlled with scheduled medicines", 0, "Expected."),
    ("Blood pressure back to normal", 0, "Tumor removed."),
    ("Temperature 38.9 C during neutropenia from chemotherapy", 1, "Febrile neutropenia.")],
   "Recovery; bowel obstruction and fever with neutropenia need action.",
   "Evaluate post-nephrectomy care.",
   ex=[note("Post-op", "Stage II Wilms tumor; favorable histology.", id="wt-postop")]),
]

cases = [
    case("Infant dehydration from rotavirus", dh_tabs, dh_qs),
    case("Nephrotic syndrome in a preschooler", ns_tabs, ns_qs),
    case("Acute post-streptococcal glomerulonephritis", agn_tabs, agn_qs),
    case("Wilms tumor before nephrectomy", wt_tabs, wt_qs),
]

BUILT = build(144, "peds-cases-dehydration-nephrotic-agn-wilms",
              "Pediatric case studies: infant dehydration, nephrotic syndrome, glomerulonephritis and Wilms tumor",
              items, cases, case_conversion=True)
