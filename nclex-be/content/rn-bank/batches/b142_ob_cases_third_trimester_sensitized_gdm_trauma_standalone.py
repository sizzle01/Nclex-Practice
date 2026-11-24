"""Batch 142 (case conversion, chunk 11: maternity part 2): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: a third-trimester visit with common discomforts, supine hypotension, weight gain by BMI, Leopold maneuvers and
pregnancy terminology; an Rh-sensitized pregnancy with rising titers and fetal anemia; gestational diabetes on
insulin with night-time hypoglycemia and polyhydramnios; placental abruption after a car crash (trauma in
pregnancy). Standalone: an eclamptic seizure bowtie, a breast problem comparison table (engorgement, plugged duct,
mastitis, lactation suppression), a priority question on an antepartum unit and a back-labor comfort question. Each
case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care,
safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
PRENATAL = "preconception-and-prenatal-care"
PRENATAL_TEST = "prenatal-testing"
COMPLICATIONS = "pregnancy-complications"
OB_MEDS = "obstetric-medications"
LABOR = "labor-and-birth"
BREASTFEEDING = "breastfeeding"
FHR = "fetal-heart-rate-monitoring"
PRIORITY = "prioritizing-client-care"
INSULIN = "endocrine-medications"
TEACH = "health-teaching-and-behavior-change"
IPV = "intimate-partner-and-sexual-violence"
TRAUMA = "trauma-and-hemorrhage"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "A client on the postpartum unit has a new emergency. Using the note, fill in the bowtie: the most likely "
   "condition in the middle, two actions to take now on the left and two parameters to monitor on the right.",
   [("Eclampsia", True, "A seizure with severe-range blood pressure after birth."),
    ("Hypoglycemic seizure", False, "Glucose is normal."),
    ("Epilepsy flare", False, "No seizure history; blood pressure is severe."),
    ("Amniotic fluid embolism", False, "Too late after birth; no collapse.")],
   [("Side-lying, pad the rails", True, "Airway and safety."),
    ("Give IV magnesium", True, "Stops and prevents seizures."),
    ("Put a padded tongue blade between the teeth to protect the tongue", False, "Causes injury."),
    ("Restrain the arms and legs to prevent injury during the seizure", False, "Injury."),
    ("Give a large IV fluid bolus to protect the kidneys", False, "Pulmonary edema risk.")],
   [("BP, reflexes", True, "BP control and safety."),
    ("Breathing, SpO2", True, "Postictal and magnesium."),
    ("Bowel sounds every hour", False, "Not urgent."),
    ("Body weight once a week", False, "Not urgent."),
    ("Breast milk supply each shift", False, "Not urgent.")],
   "A postpartum seizure with severe hypertension is eclampsia; give magnesium and protect the airway.",
   "Recognize and act on eclampsia.",
   ex=[note("Postpartum day 2", "Client complained of a severe headache an hour ago. Now a tonic-clonic seizure. "
       "BP 174/112 mm Hg. Glucose 102 mg/dL. No seizure history.", id="sa-eclampsia")]),

dtable(M(HPM, BREASTFEEDING, "analyze_cues", "difficult", OB, "ob"),
   "For each postpartum breast problem, choose the best nursing advice.",
   ["Client", "Findings", "Advice"],
   [(["A, day 4, breastfeeding", "Both breasts hard, warm, swollen; baby struggles to latch"], [
       ("Feed often; express a little milk to soften the areola before latching; cold packs after feeds", True, "Engorgement."),
       ("Stop breastfeeding for 2 days", False, "Worsens engorgement."),
       ("Start antibiotics", False, "Not infection.")]),
    (["B, week 3, breastfeeding", "Tender lump in one breast, no fever"], [
       ("Feed often, starting on that side; gentle massage toward the nipple", True, "Plugged duct."),
       ("Avoid feeding from that side", False, "Worsens it."),
       ("Wear a tight bra", False, "Worsens it.")]),
    (["C, day 3, not breastfeeding", "Painful, swollen breasts"], [
       ("Wear a supportive bra, apply cold packs and avoid stimulating the breasts", True, "Lactation suppression."),
       ("Pump to relieve the fullness", False, "Stimulates more milk."),
       ("Use warm showers on the breasts", False, "Stimulates milk.")])],
   "Engorgement and plugged ducts need frequent emptying; suppression needs no stimulation.",
   "Analyze postpartum breast problems.",
   ex=[note("Phone calls", "Three postpartum clients call the clinic.", id="sa-breast")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", OB, "mgmt ob"),
   "On an antepartum unit, four clients call at once. Which client should the nurse see first?",
   [("A client at 30 weeks with placenta previa who has new bright red bleeding soaking a pad", True,
     "Active hemorrhage threatens mother and fetus."),
    ("A client at 28 weeks with hyperemesis who vomited once after breakfast", False, "Expected; reassess soon."),
    ("A client at 34 weeks with gestational diabetes whose glucose is 128 mg/dL after lunch", False,
     "Slightly high but not urgent."),
    ("A client at 32 weeks on bed rest asking for help to the bathroom", False, "Delegate assistance.")],
   "Active bleeding with previa is the priority.",
   "Prioritize clients on an antepartum unit."),

sata(M(HPM, LABOR, "take_action", "difficult", OB, "ob"),
   "A client at 6 cm without an epidural has intense constant lower back pain, and the fetus is occiput posterior. "
   "The client has not voided for 4 hours. Which actions should the nurse take?",
   [("Apply firm counterpressure to the sacrum", True, "Relieves back pain."),
    ("Encourage hands-and-knees or side-lying positions", True, "Helps rotation."),
    ("Help the client empty the bladder or catheterize if unable", True, "A full bladder slows descent."),
    ("Offer a warm pack or shower", True, "Comfort."),
    ("Keep the client flat on the back", False, "Worsens pain and rotation."),
    ("Tell the client back pain is not part of labor", False, "Untrue.")],
   "Counterpressure, positions, bladder care and warmth.",
   "Take action for back labor."),
]

# ================================================================================================
# Case 1: Third-trimester visit
# ================================================================================================
tt_tabs = [
    ("tt-notes", "Prenatal notes", [note("32 weeks",
        "A 28-year-old client at 32 weeks (pre-pregnancy BMI 31) comes for a routine visit. The client reports leg "
        "cramps at night, heartburn, and feeling faint when lying flat for an ultrasound last week. The client has "
        "had a 12-week pregnancy loss, a stillbirth at 26 weeks and one living child born at 38 weeks.", id="tt-note")]),
    ("tt-data", "Findings", [table("Findings", ["Result"], [
        ("Weight gain so far", ["12 kg"]),
        ("Fundal height", ["32 cm"]),
        ("Blood pressure", ["118/72 mm Hg"]),
        ("Leopold: fundus", ["Soft, irregular part"]),
        ("Leopold: lower pole", ["Hard, round, movable"]),
        ("Ankles", ["Mild swelling at night"])], id="tt-table")]),
]

tt_qs = [
matrix_mc(M(HPM, PRENATAL, "recognize_cues", "moderate", OB, "ob"),
   "For each finding, indicate whether it is an expected discomfort or needs follow-up.",
   ["Expected discomfort", "Needs follow-up"],
   [("Leg cramps at night", 0, "Common."),
    ("Heartburn", 0, "Common."),
    ("Feeling faint lying flat", 0, "Supine hypotension; teach side-lying."),
    ("Weight gain of 12 kg at 32 weeks with BMI 31", 1, "Above the 5 to 9 kg total advised for obesity."),
    ("Mild ankle swelling at night", 0, "Common if BP is normal.")],
   "Most are expected; excess weight gain needs counseling.",
   "Recognize expected discomforts of pregnancy."),

dtable(M(HPM, PRENATAL, "analyze_cues", "difficult", OB, "ob"),
   "Using the history, choose the correct GTPAL values.",
   ["Letter", "Meaning", "Value"],
   [(["G", "Gravida"], [("4", True, "Three past plus this one."), ("3", False, "Misses the current."), ("2", False, "Too few.")]),
    (["T", "Term births"], [("1", True, "38 weeks."), ("2", False, "Stillbirth was preterm."), ("0", False, "Missed.")]),
    (["P", "Preterm births (20 to 36 weeks)"], [("1", True, "Stillbirth at 26 weeks counts as a preterm birth."), ("0", False, "It counts."), ("2", False, "Too many.")]),
    (["A", "Abortions or losses before 20 weeks"], [("1", True, "12-week loss."), ("2", False, "The stillbirth was after 20 weeks."), ("0", False, "Missed.")]),
    (["L", "Living children"], [("1", True, "One child."), ("2", False, "Stillbirth is not living."), ("0", False, "Missed.")])],
   "G4 T1 P1 A1 L1.",
   "Analyze pregnancy terminology.",
   ex=[note("History", "12-week loss; stillbirth at 26 weeks; living child born at 38 weeks.", id="tt-hx")]),

cloze(M(HPM, PRENATAL, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The Leopold findings show the fetus is {{b1}}, so the FHR is best heard {{b2}}.",
   [[("vertex (head down)", True, "Hard round part in the pelvis."),
     ("breech", False, "The soft part is in the fundus."),
     ("transverse", False, "Poles are at the ends."),
     ("face presentation", False, "Not determined this way.")],
    [("in a lower quadrant below the umbilicus", True, "Over the fetal back near the head."),
     ("above the umbilicus", False, "That is for breech."),
     ("at the fundus", False, "Not for vertex."),
     ("only at the umbilicus", False, "Not specific.")]],
   "Vertex position; FHR below the umbilicus.",
   "Prioritize Leopold findings."),

sata(M(HPM, PRENATAL, "generate_solutions", "difficult", OB, "ob"),
   "Which advice should the nurse give for the client's discomforts?",
   [("Lie on the left side, not flat on the back", True, "Supine hypotension."),
    ("Flex the foot upward (dorsiflex) to relieve a leg cramp", True, "Stretches the calf."),
    ("Eat small meals and stay upright after eating", True, "Heartburn."),
    ("Elevate the legs when resting", True, "Edema."),
    ("Point the toes hard during a cramp", False, "Worsens the cramp."),
    ("Take sodium bicarbonate for heartburn", False, "Sodium and alkalosis.")],
   "Side-lying, dorsiflexion, small meals and elevation.",
   "Generate advice for pregnancy discomforts.",
   ex=[note("History", "Stillbirth at 26 weeks; client anxious about fetal movement.", id="tt-anx")]),

sata(M(HPM, PRENATAL, "take_action", "difficult", OB, "ob"),
   "Because of the prior stillbirth, the client asks how to keep track of the baby's movements. Which teaching "
   "should the nurse give?",
   [("Count movements daily at the same time, often after a meal, lying on the side", True, "Consistent method."),
    ("Expect 10 movements within 2 hours", True, "A common threshold."),
    ("Call the same day if movements decrease", True, "Do not wait."),
    ("Movements stop near term, so less movement is normal", False, "Movements do not stop."),
    ("Wait until the next visit to report fewer movements", False, "Delays care.")],
   "Daily counts with same-day reporting.",
   "Take action with kick-count teaching.",
   ex=[note("Plan", "Twice-weekly NSTs from 32 weeks due to prior stillbirth.", id="tt-plan")]),

matrix_mc(M(HPM, PRENATAL, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 36 weeks, for each finding or statement, indicate whether it shows progress or understanding, or needs "
   "follow-up.",
   ["Progress or understanding", "Needs follow-up"],
   [("Weight gain 1 kg in 4 weeks", 0, "Slowed."),
    ("Reactive NSTs", 0, "Reassuring."),
    ("\"I sleep on my side with a pillow behind my back.\"", 0, "Correct."),
    ("\"I had a bad headache and saw spots yesterday.\"", 1, "Possible preeclampsia."),
    ("Fundal height 36 cm", 0, "Appropriate.")],
   "Progress; headache with visual change needs same-day evaluation.",
   "Evaluate third-trimester care.",
   ex=[note("36 weeks", "BP 146/94 mm Hg today.", id="tt-36")]),
]

# ================================================================================================
# Case 2: Rh-sensitized pregnancy
# ================================================================================================
sens_tabs = [
    ("sens-notes", "Maternal-fetal medicine notes", [note("24 weeks",
        "A 33-year-old client (Rh negative) did not receive Rho(D) immune globulin after the birth of an Rh-positive "
        "baby 3 years ago abroad. The current partner is Rh positive.", id="sens-note")]),
    ("sens-data", "Results", [flow("Results", ["16 weeks", "24 weeks"], [
        ("Anti-D antibody titer", ["1:16", "1:64"]),
        ("Middle cerebral artery Doppler", ["Normal", "Peak velocity high (more than 1.5 MoM)"]),
        ("Ultrasound", ["Normal", "Mild fluid around the heart and scalp edema"]),
        ("Fetal heart rate", ["150", "155/min"])], id="sens-flow")]),
]

sens_qs = [
hl_table(M(RRP, PRENATAL_TEST, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest fetal anemia.",
   ["Finding", "24 weeks"],
   [(["Antibody titer", "Rising to 1:64"], True, "Active sensitization."),
    (["MCA Doppler", "High peak velocity"], True, "Anemic blood flows faster."),
    (["Ultrasound", "Fluid around the heart, scalp edema"], True, "Early hydrops."),
    (["FHR", "155/min"], False, "Normal.")],
   "Rising titers, high MCA velocity and edema mean fetal anemia with early hydrops.",
   "Recognize cues of fetal anemia."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob"),
   "For each statement, indicate whether it is accurate or inaccurate about this pregnancy.",
   ["Accurate", "Inaccurate"],
   [("Rho(D) immune globulin now will reverse the sensitization", 1, "Too late once sensitized."),
    ("The antibodies cross the placenta and destroy fetal red cells", 0, "Mechanism."),
    ("Later pregnancies with Rh-positive babies are usually more affected", 0, "Stronger response."),
    ("Hydrops fetalis is fluid build-up from severe anemia", 0, "Heart failure."),
    ("An Rh-negative fetus would not be affected", 0, "No D antigen.")],
   "Sensitization cannot be reversed; the fetus needs monitoring and treatment.",
   "Analyze Rh sensitization."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The expected treatment is {{b1}}, and after birth the newborn is at risk for {{b2}}.",
   [[("intrauterine blood transfusion", True, "Treats fetal anemia."),
     ("Rho(D) immune globulin", False, "Too late."),
     ("immediate birth at 24 weeks", False, "Too preterm."),
     ("maternal iron", False, "Not the problem.")],
    [("severe jaundice and anemia", True, "Ongoing hemolysis."),
     ("hyperglycemia", False, "Not typical."),
     ("macrosomia", False, "Not typical."),
     ("neonatal abstinence", False, "Not related.")]],
   "Intrauterine transfusion; watch for newborn jaundice.",
   "Prioritize fetal anemia."),

sata(M(RRP, PRENATAL_TEST, "generate_solutions", "difficult", OB, "ob"),
   "Which parts of the plan should the nurse anticipate?",
   [("MCA Doppler every 1 to 2 weeks", True, "Monitors anemia."),
    ("Repeat intrauterine transfusions as needed", True, "Treatment."),
    ("Betamethasone before 34 weeks", True, "Preterm birth likely."),
    ("Birth at a center with a NICU", True, "Exchange transfusion may be needed."),
    ("Rho(D) immune globulin at 28 weeks", False, "Does not help once sensitized."),
    ("Routine care with no extra monitoring", False, "Unsafe.")],
   "Doppler, transfusions, steroids and a NICU birth.",
   "Generate the plan for a sensitized pregnancy.",
   ex=[note("25 weeks", "First intrauterine transfusion done.", id="sens-iut")]),

sata(M(PA, COMPLICATIONS, "take_action", "difficult", OB, "ob"),
   "After the intrauterine transfusion, which actions should the nurse take?",
   [("Monitor the FHR continuously for a period", True, "Bradycardia risk."),
    ("Watch for contractions and fluid leaking", True, "Preterm labor and rupture."),
    ("Check for fever", True, "Infection."),
    ("Discharge without monitoring", False, "Unsafe."),
    ("Give Rho(D) immune globulin to prevent more antibodies", False, "Already sensitized.")],
   "Monitor the fetus and watch for labor and infection.",
   "Take action after intrauterine transfusion.",
   ex=[note("Post-procedure", "Fetal hemoglobin raised from 6 to 13 g/dL.", id="sens-post")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob peds"),
   "After birth at 35 weeks, for each newborn finding, indicate whether it is reassuring or needs follow-up.",
   ["Reassuring", "Needs follow-up"],
   [("Hemoglobin 13 g/dL", 0, "Treated in utero."),
    ("Bilirubin rising 1 mg/dL per hour", 1, "Severe hemolysis; exchange transfusion may be needed."),
    ("Direct Coombs positive", 0, "Expected; not a problem in itself."),
    ("Mild edema resolving", 0, "Improving."),
    ("Platelets 60,000", 1, "Can occur after transfusions.")],
   "Severe hemolysis may still need treatment after birth.",
   "Evaluate the newborn after Rh disease.",
   ex=[note("Birth", "Three intrauterine transfusions in total.", id="sens-birth")]),
]

# ================================================================================================
# Case 3: Gestational diabetes on insulin with hypoglycemia and polyhydramnios
# ================================================================================================
gd_tabs = [
    ("gd-notes", "Prenatal notes", [note("34 weeks",
        "A 35-year-old client with gestational diabetes takes NPH insulin at bedtime and rapid-acting insulin with "
        "meals. The partner found the client sweaty and confused at 0300 last night. The abdomen feels tight.",
        id="gd-note")]),
    ("gd-data", "Findings", [table("Findings", ["Result"], [
        ("0300 glucose (partner checked)", ["48 mg/dL"]),
        ("Fasting glucose this week", ["Mostly 72 to 85 mg/dL"]),
        ("After-meal glucose", ["Mostly 110 to 125 mg/dL"]),
        ("Fundal height", ["39 cm at 34 weeks"]),
        ("Amniotic fluid index", ["27 cm (high)"]),
        ("Estimated fetal weight", ["Above the 95th percentile"])], id="gd-table")]),
]

gd_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob ada"),
   "Highlight the findings that need follow-up.",
   ["Finding", "Result"],
   [(["Night hypoglycemia", "48 mg/dL with confusion"], True, "Dangerous."),
    (["Fundal height", "39 cm at 34 weeks"], True, "Large for dates."),
    (["AFI", "27 cm"], True, "Polyhydramnios."),
    (["Fetal weight", "Above 95th percentile"], True, "Macrosomia."),
    (["After-meal glucose", "110 to 125"], False, "At target.")],
   "Night hypoglycemia and fetal overgrowth need action.",
   "Recognize cues in insulin-treated GDM."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob"),
   "For each complication of polyhydramnios, indicate whether it is a risk.",
   ["Risk", "Not a risk"],
   [("Preterm labor", 0, "Overdistension."),
    ("Cord prolapse when membranes rupture", 0, "Gush of fluid."),
    ("Postpartum hemorrhage", 0, "Overstretched uterus."),
    ("Malpresentation", 0, "Room to move."),
    ("Oligohydramnios at birth", 1, "Opposite.")],
   "Polyhydramnios raises several labor risks.",
   "Analyze polyhydramnios risks."),

cloze(M(PPT, INSULIN, "prioritize_hypotheses", "difficult", OB, "ph ada"),
   "Complete the sentence by choosing from the lists of options.",
   "The night hypoglycemia is most likely from {{b1}}, so the provider will likely {{b2}}.",
   [[("the bedtime NPH insulin peaking at night", True, "NPH peaks 4 to 12 hours."),
     ("the rapid-acting insulin at dinner only", False, "Shorter action."),
     ("too much protein", False, "Not the cause."),
     ("placental insufficiency", False, "Not the main cause.")],
    [("reduce the NPH dose and add a bedtime snack", True, "Prevents night lows."),
     ("increase the NPH", False, "Worsens."),
     ("stop all insulin", False, "Hyperglycemia."),
     ("switch to glyburide", False, "Not preferred.")]],
   "NPH peaks overnight; reduce it and add a snack.",
   "Prioritize night hypoglycemia."),

sata(M(PPT, INSULIN, "generate_solutions", "difficult", OB, "ph ada"),
   "Which teaching should the nurse give about hypoglycemia?",
   [("Rule of 15", True, "Rule of 15."),
    ("Partner learns glucagon", True, "Severe lows."),
    ("Bedtime snack", True, "Prevents night lows."),
    ("Check glucose at 0300", True, "Confirms."),
    ("Treat a low with a chocolate bar or a handful of nuts", False, "Fat slows absorption."),
    ("Skip the next meal after a low to avoid a high reading", False, "Rebound and repeat lows.")],
   "Rule of 15, glucagon, bedtime snack and checks.",
   "Generate hypoglycemia teaching.",
   ex=[note("Plan", "NPH reduced by 20%. Weekly ultrasound and twice-weekly NST.", id="gd-plan")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", OB, "mgmt ob"),
   "Planning for birth at 38 to 39 weeks, which actions should the nurse coordinate?",
   [("Tell the newborn team about the GDM and estimated weight", True, "Newborn hypoglycemia and dystocia."),
    ("Plan an insulin and glucose protocol for labor", True, "Labor glucose control."),
    ("Discuss the risk of shoulder dystocia and birth options", True, "Informed planning."),
    ("Arrange a 75 g OGTT 4 to 12 weeks after birth", True, "Screening."),
    ("Plan to continue the same insulin doses after birth", False, "Needs usually stop after the placenta delivers."),
    ("Delay the first newborn feed until 6 hours", False, "Early feeding prevents hypoglycemia.")],
   "Coordinate newborn, labor glucose and follow-up.",
   "Take action to plan birth with GDM.",
   ex=[note("37 weeks", "Estimated fetal weight 4,300 g.", id="gd-37")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "After birth, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Maternal glucose 84 mg/dL without insulin", 0, "GDM resolves."),
    ("Newborn glucose 52 mg/dL after a feed", 0, "Acceptable."),
    ("Heavy bleeding with a boggy uterus at 30 minutes", 1, "Atony from overdistension."),
    ("Newborn weight 4,250 g", 0, "Large but known."),
    ("Newborn jittery with glucose 28 mg/dL at 2 hours", 1, "Hypoglycemia.")],
   "Watch for postpartum hemorrhage and newborn hypoglycemia.",
   "Evaluate birth with GDM.",
   ex=[note("Birth", "Vaginal birth at 39 weeks.", id="gd-birth")]),
]

# ================================================================================================
# Case 4: Abruption after a car crash
# ================================================================================================
mvc_tabs = [
    ("mvc-notes", "Emergency notes", [note("Emergency department",
        "A 27-year-old client at 30 weeks was a restrained driver in a car crash 1 hour ago. The seat belt was "
        "across the abdomen. The client has mild abdominal pain and no bleeding.", id="mvc-note")]),
    ("mvc-data", "Findings", [flow("Findings", ["Arrival", "2 hours"], [
        ("Contractions", ["None", "Every 2 minutes, small and frequent"]),
        ("Uterus", ["Soft", "Tender, firm between contractions"]),
        ("FHR", ["145/min, moderate variability", "Minimal variability, late decelerations"]),
        ("Vaginal bleeding", ["None", "Small, dark"]),
        ("Kleihauer-Betke", ["-", "Positive"]),
        ("Blood type", ["O negative", "-"])], id="mvc-flow")]),
]

mvc_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the 2-hour findings that suggest placental abruption.",
   ["Finding", "2 hours"],
   [(["Contractions", "Small, frequent"], True, "Irritable uterus."),
    (["Uterus", "Tender, firm"], True, "Abruption."),
    (["FHR", "Late decelerations"], True, "Fetal compromise."),
    (["Kleihauer-Betke", "Positive"], True, "Fetal blood in the maternal circulation."),
    (["Bleeding", "Small"], False, "Bleeding may be concealed.")],
   "Abruption can appear hours after minor trauma.",
   "Recognize cues of traumatic abruption."),

matrix_mc(M(PA, TRAUMA, "analyze_cues", "difficult", OB, "ob ms"),
   "For each change of pregnancy, indicate how it affects trauma assessment.",
   ["Can hide shock", "Raises a specific injury risk"],
   [("Blood volume up 40 to 50%", 0, "Large losses before vital signs change."),
    ("Supine hypotension after 20 weeks", 0, "Mimics shock."),
    ("Enlarged uterus out of the pelvis", 1, "Direct injury and abruption."),
    ("Fetus as a sign of maternal perfusion", 0, "FHR changes may come first."),
    ("Displaced bowel", 1, "Different injury pattern.")],
   "Pregnancy changes hide shock and change injury patterns.",
   "Analyze trauma in pregnancy."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "After trauma beyond 20 weeks, the client needs {{b1}}, even with no symptoms at first, and because of the "
   "positive Kleihauer-Betke, {{b2}}.",
   [[("at least 4 to 6 hours of continuous fetal monitoring", True, "Delayed abruption."),
     ("discharge after 30 minutes", False, "Too short."),
     ("an x-ray of the abdomen only", False, "Not enough."),
     ("no monitoring if the seat belt was worn", False, "Belts can cause abruption.")],
    [("Rho(D) immune globulin dosed to the bleed", True, "Rh negative with fetal-maternal bleed."),
     ("no Rho(D) because of the trauma", False, "Needed."),
     ("a blood transfusion for the fetus", False, "Not now."),
     ("iron tablets", False, "Not relevant.")]],
   "Monitoring for hours and Rh prophylaxis.",
   "Prioritize trauma in pregnancy."),

sata(M(PA, COMPLICATIONS, "generate_solutions", "difficult", OB, "ob"),
   "Which actions should the nurse anticipate?",
   [("Left lateral tilt", True, "Aortocaval compression."),
    ("Betamethasone", True, "30 weeks."),
    ("Magnesium if birth is near", True, "Fetal brain."),
    ("Readiness for cesarean", True, "Fetal compromise."),
    ("Tocolytics to stop the frequent contractions", False, "Contraindicated with abruption."),
    ("Discharge home if the bleeding stays small", False, "Unsafe.")],
   "Tilt, steroids, magnesium and readiness for birth.",
   "Generate the trauma abruption plan.",
   ex=[note("Plan", "Admit. Continuous monitoring. Rho(D) per Kleihauer-Betke.", id="mvc-plan")]),

sata(M(PSI, IPV, "take_action", "difficult", OB, "psych ob"),
   "Which actions should the nurse include as part of trauma care in pregnancy?",
   [("Screen for intimate partner violence privately", True, "Trauma in pregnancy is often IPV."),
    ("Teach correct seat belt use: lap belt under the abdomen, shoulder belt between the breasts", True, "Prevention."),
    ("Ask whether anyone at home has hurt the client", True, "Direct screening."),
    ("Screen for violence with the partner in the room", False, "Unsafe."),
    ("Teach that seat belts should not be worn in pregnancy", False, "Belts save lives.")],
   "IPV screening and seat belt teaching.",
   "Take action with trauma prevention in pregnancy.",
   ex=[note("Day 1", "FHR improved after fluids and tilt; no further decelerations.", id="mvc-day1")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "On day 2, for each finding, indicate whether it supports continuing the pregnancy or needs prompt action.",
   ["Supports continuing", "Needs prompt action"],
   [("Contractions stopped, uterus soft", 0, "Settled."),
    ("Reactive NST", 0, "Reassuring."),
    ("No further bleeding", 0, "Stable."),
    ("Fibrinogen falling from 380 to 190 mg/dL", 1, "Worsening abruption."),
    ("Rho(D) immune globulin given", 0, "Done.")],
   "Stable except a falling fibrinogen, which signals a worsening abruption.",
   "Evaluate traumatic abruption.",
   ex=[note("Day 2", "Betamethasone course complete.", id="mvc-day2")]),
]

cases = [
    case("Third-trimester visit with expected discomforts", tt_tabs, tt_qs),
    case("Rh-sensitized pregnancy with fetal anemia", sens_tabs, sens_qs),
    case("Gestational diabetes on insulin with night-time hypoglycemia", gd_tabs, gd_qs),
    case("Placental abruption after a car crash", mvc_tabs, mvc_qs),
]

BUILT = build(142, "ob-cases-third-trimester-sensitized-gdm-trauma-standalone",
              "Maternity case studies: third-trimester visit, Rh sensitization, GDM with hypoglycemia and traumatic "
              "abruption, with four standalone items",
              items, cases, case_conversion=True)
