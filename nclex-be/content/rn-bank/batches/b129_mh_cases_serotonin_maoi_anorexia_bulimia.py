"""Batch 129 (case conversion, chunk 8: mental health disorders with psychiatric medications): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: serotonin syndrome from an SSRI with tramadol; hypertensive crisis on an MAOI after aged cheese; anorexia
nervosa with medical instability and admission; bulimia nervosa with hypokalemia and dental and esophageal damage.
Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care,
safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
MOOD = "mood-disorders"
EATING = "eating-disorders"
PSYCH_MEDS = "psychiatric-medications"
COMM = "therapeutic-communication"
MILIEU = "behavior-management-and-milieu"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
NUTR = "nutrition-and-therapeutic-diets"
TEACH = "health-teaching-and-behavior-change"
MED_SAFETY = "medication-safety-systems"
DELEG = "delegation-and-supervision"
CALC = "medication-dose-calculations"
DYSRHYTHMIA = "cardiac-dysrhythmias"

items = []

# ================================================================================================
# Case 1: Serotonin syndrome
# ================================================================================================
ss_tabs = [
    ("ss-notes", "Emergency notes", [note("Emergency department",
        "A 46-year-old client takes fluoxetine 40 mg daily for depression. After a dental extraction 2 days ago, the "
        "client was prescribed tramadol and also took an over-the-counter cough syrup with dextromethorphan. Since "
        "this morning the client has been restless, sweating and shaking.", id="ss-note")]),
    ("ss-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["38.9 C"]),
        ("Heart rate", ["124/min"]),
        ("Blood pressure", ["164/96 mm Hg"]),
        ("Pupils", ["Dilated"]),
        ("Reflexes", ["Brisk, with sustained ankle clonus"]),
        ("Gut", ["Diarrhea, hyperactive bowel sounds"]),
        ("Mental status", ["Agitated, anxious"])], id="ss-table")]),
]

ss_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the findings that suggest serotonin syndrome.",
   ["Finding", "Result"],
   [(["Reflexes", "Clonus, hyperreflexia"], True, "The key sign."),
    (["Temperature", "38.9 C"], True, "Hyperthermia."),
    (["Pupils", "Dilated"], True, "Autonomic."),
    (["Gut", "Diarrhea"], True, "Serotonin in the gut."),
    (["Mental status", "Agitated"], True, "Mental status change.")],
   "Clonus with agitation, fever and diarrhea after adding serotonergic drugs is serotonin syndrome.",
   "Recognize cues of serotonin syndrome."),

matrix_mc(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each drug, indicate whether it adds to serotonin syndrome risk with fluoxetine.",
   ["Adds to the risk", "Does not add to the risk"],
   [("Tramadol", 0, "Serotonergic opioid."),
    ("Dextromethorphan", 0, "Serotonergic."),
    ("Linezolid", 0, "A weak MAO inhibitor."),
    ("St John's wort", 0, "Serotonergic herb."),
    ("Acetaminophen", 1, "No serotonin effect.")],
   "Many non-psychiatric drugs add serotonin.",
   "Analyze serotonin syndrome drug interactions."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The first step is {{b1}}; if symptoms persist, the provider may give {{b2}}.",
   [[("stopping all serotonergic drugs and giving supportive care", True, "Most cases resolve in 24 hours."),
     ("adding a second SSRI", False, "Worsens it."),
     ("physical restraint", False, "Worsens hyperthermia."),
     ("giving more tramadol for the shaking", False, "Worsens it.")],
    [("cyproheptadine and a benzodiazepine for agitation", True, "A serotonin antagonist."),
     ("haloperidol", False, "Can worsen hyperthermia."),
     ("bromocriptine", False, "Worsens serotonin syndrome."),
     ("propranolol as the main treatment", False, "Not recommended.")]],
   "Stop the drugs, support and give cyproheptadine if needed.",
   "Prioritize serotonin syndrome."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "Which interventions should the nurse anticipate?",
   [("Lorazepam IV for agitation", True, "Reduces muscle activity and heat."),
    ("IV fluids", True, "Losses from sweating and diarrhea."),
    ("Cardiac monitoring", True, "Tachycardia."),
    ("External cooling", True, "Hyperthermia."),
    ("Acetaminophen as the main cooling method", False, "Fever is from muscle activity, not the brain's set point."),
    ("Restraints for the shaking", False, "Increase heat and muscle breakdown.")],
   "Benzodiazepines, fluids, monitoring and cooling.",
   "Generate the serotonin syndrome plan.",
   ex=[note("Orders", "Stop fluoxetine, tramadol, dextromethorphan. Lorazepam IV. Cyproheptadine if needed.", id="ss-orders")]),

sata(M(SIC, MED_SAFETY, "take_action", "difficult", MH, "ph"),
   "Which actions should the nurse take to prevent a repeat event?",
   [("Add the interaction to the medication record", True, "Alerts future prescribers."),
    ("Teach the client to check with a pharmacist before any OTC cough or cold remedy", True, "Dextromethorphan is common."),
    ("Report the dental prescription interaction to the prescriber", True, "Closes the loop."),
    ("Tell the client to restart fluoxetine tomorrow with tramadol at half dose", False, "The combination is the problem."),
    ("Record fluoxetine as an allergy", False, "This is an interaction, not an allergy.")],
   "Document the interaction, teach and inform prescribers.",
   "Take action to prevent drug interactions.",
   ex=[note("6 hours", "Clonus resolving after cyproheptadine.", id="ss-6h")]),

matrix_mc(M(PPT, PSYCH_MEDS, "evaluate_outcomes", "difficult", MH, "ph"),
   "At 24 hours, for each finding or statement, indicate whether it shows improvement or understanding, or needs "
   "follow-up.",
   ["Improvement or understanding", "Needs follow-up"],
   [("Temperature 37.2 C", 0, "Resolved."),
    ("No clonus", 0, "Resolved."),
    ("\"I'll use acetaminophen for my tooth pain.\"", 0, "Safe."),
    ("\"I'll start St John's wort to replace the fluoxetine.\"", 1, "Serotonergic."),
    ("Low mood returning with poor sleep", 1, "Depression needs a plan; fluoxetine has a long half-life before any new drug.")],
   "Recovered; depression needs a planned restart.",
   "Evaluate serotonin syndrome recovery.",
   ex=[note("24 hours", "Psychiatry review arranged.", id="ss-24h")]),
]

# ================================================================================================
# Case 2: MAOI hypertensive crisis
# ================================================================================================
maoi_tabs = [
    ("maoi-notes", "Emergency notes", [note("Emergency department",
        "A 58-year-old client with treatment-resistant depression takes phenelzine. Two hours after a wine-tasting "
        "dinner with aged cheeses and cured salami, the client has a pounding headache in the back of the head, a "
        "stiff neck and palpitations.", id="maoi-note")]),
    ("maoi-data", "Findings", [table("Findings", ["Result"], [
        ("Blood pressure", ["228/124 mm Hg"]),
        ("Heart rate", ["58/min"]),
        ("Skin", ["Flushed, sweaty"]),
        ("Pupils", ["Dilated, light-sensitive"]),
        ("Neurologic", ["Alert, no weakness"]),
        ("Chest", ["No chest pain"])], id="maoi-table")]),
]

maoi_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the findings that suggest a hypertensive crisis from the MAOI.",
   ["Finding", "Result"],
   [(["Blood pressure", "228/124 mm Hg"], True, "Severe hypertension."),
    (["Headache", "Occipital, pounding"], True, "Classic."),
    (["Neck", "Stiff"], True, "Classic."),
    (["Pupils", "Dilated, light-sensitive"], True, "Sympathetic surge."),
    (["Neurologic", "No weakness"], False, "Reassuring; no stroke yet.")],
   "A tyramine meal on an MAOI caused a hypertensive crisis.",
   "Recognize cues of MAOI hypertensive crisis."),

matrix_mc(M(BCC, NUTR, "analyze_cues", "difficult", MH, "ph"),
   "For each food, indicate whether it is high in tyramine or safe in usual amounts.",
   ["High in tyramine (avoid)", "Safe in usual amounts"],
   [("Aged cheddar", 0, "Aged cheese."),
    ("Cured salami", 0, "Cured meat."),
    ("Soy sauce and miso", 0, "Fermented."),
    ("Draft beer", 0, "Unpasteurized beer."),
    ("Fresh cottage cheese", 1, "Not aged."),
    ("Fresh chicken", 1, "Fresh meat.")],
   "Aged, fermented and cured foods are high in tyramine.",
   "Analyze tyramine in foods."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The client is at greatest risk for {{b1}}, so the goal is to {{b2}}.",
   [[("hemorrhagic stroke", True, "Very high pressure."),
     ("hypoglycemia", False, "Not related."),
     ("serotonin syndrome", False, "A different reaction."),
     ("anaphylaxis", False, "Not allergic.")],
    [("lower the blood pressure promptly but in a controlled way", True, "About 25% in the first hour."),
     ("bring it to normal within minutes", False, "Can cause stroke from low perfusion."),
     ("wait for it to settle", False, "Dangerous."),
     ("give another phenelzine dose", False, "Worsens it.")]],
   "Controlled lowering prevents stroke.",
   "Prioritize hypertensive crisis."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "Which interventions should the nurse anticipate?",
   [("IV phentolamine or another short-acting agent as prescribed", True, "Blocks alpha receptors."),
    ("Continuous blood pressure monitoring", True, "Titration."),
    ("Neurologic checks", True, "Detect stroke."),
    ("Hold phenelzine", True, "Stop the drug."),
    ("Nifedipine capsule punctured under the tongue", False, "Uncontrolled drops cause stroke."),
    ("Pseudoephedrine for nasal congestion", False, "A sympathomimetic that worsens the crisis.")],
   "Short-acting IV agents with monitoring.",
   "Generate the hypertensive crisis plan.",
   ex=[note("Orders", "Phentolamine 5 mg IV. Arterial line.", id="maoi-orders")]),

sata(M(SIC, MED_SAFETY, "take_action", "difficult", MH, "ph"),
   "The psychiatrist plans to switch the client from phenelzine to sertraline. Which actions should the nurse "
   "take?",
   [("Confirm a 14-day washout first", True, "Prevents serotonin syndrome."),
    ("Keep the tyramine diet for 2 more weeks", True, "MAO takes time to regenerate."),
    ("Teach which medicines to avoid", True, "Safety."),
    ("Start sertraline the day after phenelzine stops to avoid a relapse", False, "Serotonin syndrome risk."),
    ("Give meperidine for the headache while the MAOI is still active", False, "A dangerous interaction.")],
   "A 14-day washout and continued diet are needed.",
   "Take action on switching antidepressants.",
   ex=[note("4 hours", "BP 168/94 mm Hg. Headache easing.", id="maoi-4h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", MH, "ph"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"For 2 weeks I'll still avoid aged cheese, cured meat and tap beer.\"", 0, "Correct."),
    ("\"I won't take decongestants or cough medicines without asking.\"", 0, "Correct."),
    ("\"I'll start the new antidepressant tomorrow.\"", 1, "Wait the full washout."),
    ("\"A severe headache means I should go to the emergency department.\"", 0, "Correct."),
    ("\"Fresh fruit and fresh meat are fine.\"", 0, "Correct.")],
   "Diet, interactions and washout timing.",
   "Evaluate MAOI teaching.",
   ex=[note("Day 2", "BP 132/80 mm Hg. Phenelzine stopped.", id="maoi-day2")]),
]

# ================================================================================================
# Case 3: Anorexia nervosa with medical instability
# ================================================================================================
an_tabs = [
    ("an-notes", "Clinic notes", [note("Primary care",
        "A 17-year-old client is brought by a parent because of weight loss over 8 months. The client runs 10 km a "
        "day, eats \"only salad\" and says, \"I'm still too fat around my stomach.\" The client has not had a period "
        "for 5 months and fainted at school yesterday.", id="an-note")]),
    ("an-data", "Findings", [table("Findings", ["Result"], [
        ("BMI", ["14.8 (weight 40 kg)"]),
        ("Heart rate", ["38/min lying; 74/min standing"]),
        ("Blood pressure", ["82/50 mm Hg"]),
        ("Temperature", ["35.4 C"]),
        ("Potassium", ["3.1 mEq/L"]),
        ("Phosphorus", ["2.9 mg/dL"]),
        ("ECG", ["Sinus bradycardia, QTc 490 ms"]),
        ("Skin", ["Fine hair on the arms and back"])], id="an-table")]),
]

an_qs = [
hl_table(M(PSI, EATING, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that indicate the client needs hospital admission.",
   ["Finding", "Result"],
   [(["Heart rate", "38/min lying"], True, "Severe bradycardia."),
    (["Orthostatic change", "36 beats/min rise"], True, "Marked instability."),
    (["Temperature", "35.4 C"], True, "Hypothermia."),
    (["QTc", "490 ms"], True, "Dysrhythmia risk."),
    (["BMI", "14.8"], True, "Severe malnutrition."),
    (["Fine body hair", "Lanugo"], False, "A sign of starvation but not an admission criterion.")],
   "Bradycardia, orthostasis, hypothermia, long QTc and very low BMI need admission.",
   "Recognize cues of medical instability in anorexia."),

matrix_mc(M(PSI, EATING, "analyze_cues", "difficult", MH, "psych"),
   "For each finding, indicate whether it fits anorexia nervosa, bulimia nervosa or both.",
   ["Anorexia nervosa", "Bulimia nervosa", "Both"],
   [("Very low body weight", 0, "Anorexia."),
    ("Normal weight with binge and purge cycles", 1, "Bulimia."),
    ("Low potassium", 2, "Purging occurs in both."),
    ("Intense fear of weight gain and body image distortion", 2, "Both."),
    ("Calluses on the knuckles", 1, "Self-induced vomiting.")],
   "Weight separates the two; body image and electrolytes are shared.",
   "Analyze eating disorders."),

cloze(M(PSI, EATING, "prioritize_hypotheses", "difficult", MH, "psych ms"),
   "Complete the sentence by choosing from the lists of options.",
   "During the first week of feeding, the most dangerous complication is {{b1}}, shown by {{b2}}.",
   [[("refeeding syndrome", True, "Shifts of phosphate, potassium and magnesium."),
     ("weight gain", False, "The goal."),
     ("constipation", False, "Common, not dangerous."),
     ("bloating", False, "Common.")],
    [("falling phosphorus with heart failure or dysrhythmias", True, "Hypophosphatemia."),
     ("rising albumin", False, "Not a danger sign."),
     ("better sleep", False, "Not a danger sign."),
     ("increased appetite", False, "Not a danger sign.")]],
   "Refeeding syndrome is the first-week danger.",
   "Prioritize refeeding risk."),

sata(M(PSI, EATING, "generate_solutions", "difficult", MH, "psych"),
   "Which parts of the admission plan should the nurse include?",
   [("Daily phosphorus, potassium and magnesium for the first week", True, "Refeeding."),
    ("Weigh at the same time in a gown after voiding, back to the scale", True, "Accurate, without trickery or distress."),
    ("Observe the client for 1 hour after meals", True, "Prevents purging or exercising."),
    ("Cardiac monitoring", True, "Bradycardia and QTc."),
    ("Allow the client to choose to skip meals if anxious", False, "Undermines the plan."),
    ("Let the client exercise to burn off anxiety", False, "Worsens weight loss.")],
   "Electrolytes, careful weighing, supervision and monitoring.",
   "Generate the admission plan.",
   ex=[note("Admission", "Nutrition starts at 1,600 kcal/day and increases. Thiamine and phosphate supplements.", id="an-admit")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "At lunch, the client says, \"If I eat this, I'll be huge. You just want me to get fat.\" Which responses are "
   "therapeutic?",
   [("\"This meal is part of your treatment. I'll sit with you while you eat.\"", True, "Calm, firm support."),
    ("\"It sounds like eating feels very frightening right now.\"", True, "Acknowledges the feeling."),
    ("\"Your body needs this food to keep your heart working.\"", True, "Focuses on health, not appearance."),
    ("\"You look skeletal; you need to eat.\"", False, "Focuses on appearance and shames."),
    ("\"If you don't finish, you'll lose your phone privileges for a week.\"", False, "Punitive power struggle.")],
   "Firm support, empathy and a health focus.",
   "Take action at meals.",
   ex=[note("Day 3", "Client cut food into tiny pieces and hid some in a napkin at breakfast.", id="an-day3")]),

matrix_mc(M(RRP, LABS, "evaluate_outcomes", "difficult", MH, "lab ms"),
   "On day 5, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Heart rate 52/min lying", 0, "Improving."),
    ("Weight up 0.8 kg", 0, "Steady gain."),
    ("Phosphorus 1.4 mg/dL", 1, "Refeeding hypophosphatemia."),
    ("New ankle swelling and crackles", 1, "Refeeding heart failure."),
    ("Temperature 36.4 C", 0, "Improved."),
    ("Eats 75% of meals with support", 0, "Progress.")],
   "Progress, but low phosphorus and edema signal refeeding syndrome.",
   "Evaluate refeeding.",
   ex=[note("Day 5", "Calories increased to 2,000 yesterday.", id="an-day5")]),
]

# ================================================================================================
# Case 4: Bulimia nervosa with hypokalemia
# ================================================================================================
bn_tabs = [
    ("bn-notes", "Emergency notes", [note("Emergency department",
        "A 24-year-old client of normal weight comes in with palpitations and muscle cramps. The client admits to "
        "binge eating then vomiting up to six times a day and taking 20 laxative tablets a day for a year.",
        id="bn-note")]),
    ("bn-data", "Findings", [table("Findings", ["Result"], [
        ("Potassium", ["2.4 mEq/L"]),
        ("Bicarbonate", ["34 mEq/L"]),
        ("Chloride", ["88 mEq/L"]),
        ("ECG", ["U waves, frequent PVCs"]),
        ("Parotid glands", ["Swollen"]),
        ("Teeth", ["Eroded enamel"]),
        ("Knuckles", ["Calluses"])], id="bn-table")]),
]

bn_qs = [
hl_table(M(PA, LYTES, "recognize_cues", "moderate", MH, "ms psych"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Result"],
   [(["Potassium", "2.4 mEq/L"], True, "Severe hypokalemia."),
    (["ECG", "U waves, frequent PVCs"], True, "Dysrhythmia risk."),
    (["Bicarbonate", "34 mEq/L"], True, "Metabolic alkalosis from vomiting worsens hypokalemia."),
    (["Teeth", "Eroded enamel"], False, "Needs dental care, but not urgent."),
    (["Parotid glands", "Swollen"], False, "Common with vomiting; not urgent.")],
   "Severe hypokalemia with ectopy is the emergency.",
   "Recognize cues of purging complications."),

matrix_mc(M(PA, LYTES, "analyze_cues", "difficult", MH, "ms"),
   "For each finding, indicate its cause.",
   ["Vomiting", "Laxative abuse", "Both"],
   [("Metabolic alkalosis and low chloride", 0, "Loss of stomach acid."),
    ("Low potassium", 2, "Both cause losses."),
    ("Calluses on the knuckles", 0, "Russell sign."),
    ("Chronic diarrhea and dependence for bowel movements", 1, "Laxatives."),
    ("Dental erosion", 0, "Stomach acid.")],
   "Vomiting causes alkalosis and dental damage; both cause potassium loss.",
   "Analyze purging complications."),

cloze(M(PPT, "potassium-medication-safety", "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The client needs {{b1}}, given {{b2}}.",
   [[("IV and oral potassium replacement on a cardiac monitor", True, "Severe with ECG changes."),
     ("an IV push of potassium", False, "Fatal."),
     ("a banana and discharge", False, "Too little."),
     ("a laxative to clear the bowel", False, "Worsens loss.")],
    [("no faster than about 10 mEq/hr through a peripheral line", True, "Safe rate."),
     ("as a bolus over 2 minutes", False, "Causes cardiac arrest."),
     ("in plain water", False, "Not an IV method."),
     ("only after the client agrees to stop vomiting", False, "Treat now.")]],
   "Replace potassium safely on a monitor.",
   "Prioritize hypokalemia treatment."),

calc(M(PPT, CALC, "generate_solutions", "difficult", MH, "ph"),
   "Potassium chloride 20 mEq in 100 mL is to be infused over 2 hours through a peripheral line. At what rate "
   "should the nurse set the pump? Record your answer in mL/hr.",
   "mL/hr", 0, "Rate", 50,
   "100 mL / 2 hours = 50 mL/hr, which gives 10 mEq/hr.",
   "Calculate a potassium infusion rate.",
   ex=[note("Orders", "KCl 20 mEq in 100 mL over 2 hours x3. Oral KCl 40 mEq. Magnesium level.", id="bn-orders")]),

sata(M(PSI, MILIEU, "take_action", "difficult", MH, "psych"),
   "The client is admitted to an eating disorder unit. Which actions should the nurse take?",
   [("Supervise the bathroom for 1 to 2 hours after meals", True, "Prevents purging."),
    ("Agree on a regular meal schedule with the client", True, "Breaks the binge-purge cycle."),
    ("Explore feelings that come before a binge", True, "Identifies triggers."),
    ("Taper laxatives under medical supervision", True, "Prevents severe constipation."),
    ("Allow laxatives to continue to avoid constipation", False, "Continues the abuse."),
    ("Weigh the client several times a day to show progress", False, "Increases weight focus.")],
   "Supervision, regular meals, trigger work and laxative tapering.",
   "Take action in the eating disorder unit.",
   ex=[note("Day 2", "Potassium 3.4 mEq/L. Monitor removed.", id="bn-day2")]),

matrix_mc(M(PSI, EATING, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 4 weeks, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Purging down from six times a day to once a week", 0, "Major progress."),
    ("\"When I feel lonely at night, I call a friend instead of bingeing.\"", 0, "A new coping skill."),
    ("Potassium 4.0 mEq/L", 0, "Normal."),
    ("\"I brush my teeth right after I vomit.\"", 1, "Rinse with water first; brushing straight after spreads acid."),
    ("\"I'm still using a few laxatives at night.\"", 1, "Ongoing abuse."),
    ("Attends cognitive behavioral therapy weekly", 0, "First-line therapy.")],
   "Fewer purges and new coping; ongoing laxatives and dental care need attention.",
   "Evaluate bulimia treatment.",
   ex=[note("4 weeks", "Fluoxetine 60 mg started.", id="bn-4wk")]),
]

cases = [
    case("Serotonin syndrome from fluoxetine with tramadol", ss_tabs, ss_qs),
    case("Hypertensive crisis on an MAOI", maoi_tabs, maoi_qs),
    case("Anorexia nervosa with medical instability", an_tabs, an_qs),
    case("Bulimia nervosa with severe hypokalemia", bn_tabs, bn_qs),
]

BUILT = build(129, "mh-cases-serotonin-maoi-anorexia-bulimia",
              "Mental health case studies: serotonin syndrome, MAOI hypertensive crisis, anorexia and bulimia",
              items, cases, case_conversion=True)
