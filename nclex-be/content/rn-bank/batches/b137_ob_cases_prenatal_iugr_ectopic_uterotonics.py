"""Batch 137 (case conversion, chunk 10: maternity): four full NGN case studies (CASE-MAP.md, book_topics.py).

Cases: the first prenatal visit (signs of pregnancy, GTPAL terminology, Naegele rule, danger signs); fetal growth
restriction found by fundal height, with fetal development, ultrasound and antenatal testing; ruptured ectopic
pregnancy compared with methotrexate treatment; postpartum hemorrhage needing second-line uterotonics in a client
with asthma and hypertension (medications used during labor and delivery). Each case walks the six CJMM steps in
order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
PRENATAL = "preconception-and-prenatal-care"
PRENATAL_TEST = "prenatal-testing"
COMPLICATIONS = "pregnancy-complications"
OB_MEDS = "obstetric-medications"
PPH = "postpartum-hemorrhage"
TEACH = "health-teaching-and-behavior-change"
NUTR = "nutrition-and-therapeutic-diets"
CALC = "medication-dose-calculations"
SHOCK = "shock-and-sepsis"
GRIEF = "grief-and-loss"
MED_SAFETY = "medication-safety-systems"
IPV = "intimate-partner-and-sexual-violence"

items = []

# ================================================================================================
# Case 1: First prenatal visit
# ================================================================================================
pn_tabs = [
    ("pn-notes", "Prenatal clinic notes", [note("First prenatal visit",
        "A 29-year-old client comes for a first prenatal visit. The first day of the last menstrual period was "
        "March 10. The client has had a son born at 39 weeks, twins born at 34 weeks (both living), and one "
        "miscarriage at 8 weeks. The client smokes 5 cigarettes a day and takes no vitamins.", id="pn-note")]),
    ("pn-data", "Findings", [table("Findings", ["Result"], [
        ("Breasts", ["Tender, darker areolae"]),
        ("Nausea", ["Mornings"]),
        ("Cervix", ["Bluish (Chadwick sign); softened (Goodell sign)"]),
        ("Urine hCG", ["Positive"]),
        ("Transvaginal ultrasound", ["Fetal heart activity seen"]),
        ("Blood pressure", ["112/68 mm Hg"])], id="pn-table")]),
]

pn_qs = [
matrix_mc(M(HPM, PRENATAL, "recognize_cues", "moderate", OB, "ob"),
   "For each finding, indicate the type of sign of pregnancy.",
   ["Presumptive", "Probable", "Positive"],
   [("Breast tenderness and nausea", 0, "Felt by the client."),
    ("Chadwick and Goodell signs", 1, "Seen by the examiner."),
    ("Positive urine hCG", 1, "Other conditions can cause it."),
    ("Fetal heart activity on ultrasound", 2, "Only the fetus causes it."),
    ("Missed period", 0, "Many causes.")],
   "Only signs produced by the fetus are positive signs.",
   "Recognize signs of pregnancy."),

dtable(M(HPM, PRENATAL, "analyze_cues", "difficult", OB, "ob"),
   "Using the history, choose the correct values.",
   ["Measure", "Detail", "Value"],
   [(["Gravida", "Current pregnancy plus all previous"], [
       ("4", True, "Son, twins (one pregnancy), miscarriage, and now."),
       ("5", False, "Twins count as one pregnancy."),
       ("3", False, "Misses the current pregnancy.")]),
    (["Term births", "Born at 37 weeks or later"], [
       ("1", True, "The son."),
       ("2", False, "Twins were preterm."),
       ("3", False, "Twins are one birth.")]),
    (["Preterm births", "Born 20 to 36 6/7 weeks"], [
       ("1", True, "The twin birth."),
       ("2", False, "Twins are one birth event."),
       ("0", False, "Twins were at 34 weeks.")]),
    (["Living children", "Number alive"], [
       ("3", True, "Son plus twins."),
       ("2", False, "Twins are two children."),
       ("1", False, "Undercounts.")])],
   "GTPAL: G4 T1 P1 A1 L3.",
   "Analyze pregnancy terminology.",
   ex=[note("History", "Son at 39 weeks; twins at 34 weeks, both living; miscarriage at 8 weeks.", id="pn-hx")]),

cloze(M(HPM, PRENATAL, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "Using the Naegele rule, the estimated due date is {{b1}}, and the most important change to make now is {{b2}}.",
   [[("December 17", True, "March 10 + 7 days = March 17, minus 3 months = December 17."),
     ("December 10", False, "Forgot to add 7 days."),
     ("January 17", False, "Wrong month."),
     ("November 17", False, "Wrong month.")],
    [("stopping smoking", True, "Linked to growth restriction, abruption and SIDS."),
     ("stopping all exercise", False, "Exercise is encouraged."),
     ("starting a low-calorie diet", False, "Unsafe."),
     ("avoiding all fish", False, "Low-mercury fish is encouraged.")]],
   "Add 7 days, subtract 3 months; smoking is the key risk.",
   "Prioritize first-visit care."),

sata(M(HPM, PRENATAL, "generate_solutions", "difficult", OB, "ob"),
   "Which parts of the first-visit plan should the nurse include?",
   [("Prenatal vitamin with folic acid", True, "Neural tube prevention."),
    ("Smoking cessation counseling", True, "Fetal growth."),
    ("Routine prenatal blood tests", True, "Routine labs."),
    ("Private IPV screening", True, "Routine."),
    ("Avoid all caffeine completely for the whole pregnancy", False, "Up to 200 mg a day is acceptable."),
    ("Raw sushi and unpasteurized soft cheese are fine to eat", False, "Listeria and parasites.")],
   "Vitamins, smoking cessation, routine labs and IPV screening.",
   "Generate the first-visit plan.",
   ex=[note("Plan", "Return every 4 weeks until 28 weeks.", id="pn-plan")]),

sata(M(HPM, TEACH, "take_action", "difficult", OB, "ob"),
   "Which danger signs should the nurse teach the client to report at once?",
   [("Vaginal bleeding", True, "Miscarriage, previa or abruption."),
    ("Severe headache or vision changes", True, "Preeclampsia."),
    ("Fluid leaking from the vagina", True, "Rupture of membranes."),
    ("Fewer fetal movements later in pregnancy", True, "Fetal compromise."),
    ("Mild ankle swelling at the end of the day", False, "Common."),
    ("Heartburn after large meals", False, "Common.")],
   "Bleeding, headache, fluid and reduced movement are danger signs.",
   "Take action with danger-sign teaching.",
   ex=[note("Visit end", "Client asks what is normal and what is not.", id="pn-end")]),

matrix_mc(M(HPM, PRENATAL, "evaluate_outcomes", "difficult", OB, "ob"),
   "At the 16-week visit, for each statement or finding, indicate whether it shows understanding or progress, or "
   "needs follow-up.",
   ["Understanding or progress", "Needs follow-up"],
   [("\"I take my prenatal vitamin with orange juice to absorb the iron.\"", 0, "Vitamin C aids absorption."),
    ("Down to 1 cigarette a day with a quit date", 0, "Progress."),
    ("\"I eat liver twice a week for iron.\"", 1, "Too much vitamin A."),
    ("Weight gain 2 kg so far", 0, "Appropriate."),
    ("\"I'll take the cat litter out myself every day.\"", 1, "Toxoplasmosis risk.")],
   "Good progress; liver and cat litter need teaching.",
   "Evaluate prenatal teaching.",
   ex=[note("16 weeks", "Fetal heart rate 150/min.", id="pn-16")]),
]

# ================================================================================================
# Case 2: Fetal growth restriction
# ================================================================================================
fgr_tabs = [
    ("fgr-notes", "Prenatal clinic notes", [note("32 weeks",
        "A 34-year-old client at 32 weeks by early ultrasound has chronic hypertension and smokes. The client says "
        "the baby moves less than before.", id="fgr-note")]),
    ("fgr-data", "Measurements", [flow("Fundal height", ["24 weeks", "28 weeks", "32 weeks"], [
        ("Fundal height (cm)", ["24", "25", "27"]),
        ("Blood pressure", ["132/84", "138/88", "142/92 mm Hg"]),
        ("Weight gain (kg)", ["5", "5.5", "6"]),
        ("Fetal movement", ["Normal", "Normal", "Reduced"])], id="fgr-flow")]),
]

fgr_qs = [
hl_table(M(RRP, PRENATAL_TEST, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest fetal growth restriction.",
   ["Finding", "32 weeks"],
   [(["Fundal height", "27 cm at 32 weeks"], True, "More than 2 cm less than weeks of gestation."),
    (["Slow rise", "Only 3 cm in 8 weeks"], True, "Poor growth."),
    (["Fetal movement", "Reduced"], True, "Possible compromise."),
    (["Risk factors", "Hypertension, smoking"], True, "Placental insufficiency."),
    (["Weight gain", "6 kg"], False, "Low but not diagnostic.")],
   "Fundal height in cm should be within 2 of the weeks from 20 to 36 weeks.",
   "Recognize cues of growth restriction."),

matrix_mc(M(HPM, PRENATAL, "analyze_cues", "difficult", OB, "ob"),
   "For each gestational age, indicate the expected fetal development milestone.",
   ["8 weeks", "20 weeks", "24 weeks", "36 weeks"],
   [("All major organs formed; most vulnerable to teratogens before this", 0, "Embryonic period ends."),
    ("Mother feels quickening; fundus at the umbilicus", 1, "Midpoint."),
    ("Surfactant production begins; age of viability", 2, "Viability."),
    ("Lungs mostly mature", 3, "Late preterm.")],
   "Fetal milestones guide counseling and care.",
   "Analyze fetal growth and development."),

cloze(M(RRP, PRENATAL_TEST, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The next step is {{b1}}, and the test that best shows placental resistance is {{b2}}.",
   [[("an ultrasound for estimated fetal weight and amniotic fluid", True, "Confirms growth restriction."),
     ("a repeat fundal height in 4 weeks", False, "Too slow."),
     ("reassurance", False, "Not appropriate."),
     ("an amniocentesis", False, "Not first.")],
    [("umbilical artery Doppler", True, "Measures blood flow resistance."),
     ("glucose tolerance test", False, "Not relevant."),
     ("Kleihauer-Betke", False, "Not relevant."),
     ("maternal urine culture", False, "Not relevant.")]],
   "Ultrasound with Doppler.",
   "Prioritize suspected growth restriction."),

matrix_mc(M(RRP, PRENATAL_TEST, "generate_solutions", "difficult", OB, "ob"),
   "For each test result, indicate whether it is reassuring or nonreassuring.",
   ["Reassuring", "Nonreassuring"],
   [("Reactive NST: two accelerations of 15 beats for 15 seconds in 20 minutes", 0, "Reactive."),
    ("Biophysical profile 4/10", 1, "Low; consider birth."),
    ("Amniotic fluid index 4 cm", 1, "Oligohydramnios."),
    ("Absent end-diastolic flow on umbilical Doppler", 1, "Severe placental resistance."),
    ("Biophysical profile 8/10 with normal fluid", 0, "Reassuring.")],
   "NST, BPP, fluid and Doppler guide timing of birth.",
   "Generate interpretations of antenatal tests.",
   ex=[note("Ultrasound", "Estimated fetal weight below the 5th percentile.", id="fgr-us")]),

sata(M(HPM, TEACH, "take_action", "difficult", OB, "ob"),
   "Which teaching should the nurse give?",
   [("Count kicks daily; report fewer than 10 movements in 2 hours", True, "Fetal monitoring at home."),
    ("Keep twice-weekly testing appointments", True, "Surveillance."),
    ("Stop smoking completely", True, "Improves growth."),
    ("Lie on the left side when resting", True, "Improves flow."),
    ("Eat a very high-calorie diet to make the baby grow", False, "Does not fix placental insufficiency."),
    ("Skip blood pressure medicines to avoid harming the baby", False, "Uncontrolled hypertension worsens it.")],
   "Kick counts, testing, no smoking and rest positions.",
   "Take action with fetal surveillance teaching.",
   ex=[note("Plan", "NST and amniotic fluid twice weekly. Betamethasone given. Birth planned by 37 weeks or sooner.", id="fgr-plan")]),

matrix_mc(M(RRP, PRENATAL_TEST, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 34 weeks, for each finding, indicate whether it supports continuing the pregnancy or needs prompt action.",
   ["Supports continuing", "Needs prompt action"],
   [("Reactive NST", 0, "Reassuring."),
    ("AFI 3 cm (was 7)", 1, "Falling fluid."),
    ("Reversed end-diastolic flow", 1, "Severe; birth is indicated."),
    ("Kick counts normal", 0, "Reassuring."),
    ("Blood pressure 168/112 mm Hg with headache", 1, "Superimposed preeclampsia.")],
   "Worsening Doppler, fluid or blood pressure means birth.",
   "Evaluate fetal surveillance.",
   ex=[note("34 weeks", "Surveillance continuing.", id="fgr-34")]),
]

# ================================================================================================
# Case 3: Ruptured ectopic pregnancy
# ================================================================================================
ect_tabs = [
    ("ect-notes", "Emergency notes", [note("Emergency department",
        "A 26-year-old client with a history of pelvic inflammatory disease has had spotting and right lower "
        "abdominal pain for 2 days. The last period was 7 weeks ago. Today the pain became sudden and severe, and "
        "the client has right shoulder pain and fainted at home.", id="ect-note")]),
    ("ect-data", "Findings", [table("Findings", ["Result"], [
        ("Blood pressure", ["86/52 mm Hg"]),
        ("Heart rate", ["128/min"]),
        ("Abdomen", ["Rigid, rebound tenderness"]),
        ("Serum hCG", ["2,800 mIU/mL"]),
        ("Ultrasound", ["Empty uterus; free fluid in the pelvis"]),
        ("Blood type", ["B negative"])], id="ect-table")]),
]

ect_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest a ruptured ectopic pregnancy.",
   ["Finding", "Result"],
   [(["Shock", "BP 86/52, HR 128"], True, "Hemorrhage."),
    (["Shoulder pain", "Right"], True, "Blood irritating the diaphragm."),
    (["Ultrasound", "Empty uterus, free fluid"], True, "Ectopic with bleeding."),
    (["History", "PID"], True, "Tubal scarring."),
    (["Blood type", "B negative"], False, "Needs Rho(D) immune globulin but does not suggest rupture.")],
   "Shock, shoulder pain and an empty uterus with free fluid mean rupture.",
   "Recognize cues of ruptured ectopic pregnancy."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob ph"),
   "For each finding, indicate whether it suits methotrexate treatment or needs surgery.",
   ["Methotrexate may be used", "Surgery is needed"],
   [("Stable vital signs, unruptured, hCG below 5,000", 0, "Candidate."),
    ("Signs of rupture and shock", 1, "Surgery."),
    ("Fetal heart activity in the tube", 1, "Less likely to respond."),
    ("Client unable to return for hCG follow-up", 1, "Methotrexate needs follow-up."),
    ("Small mass with no free fluid", 0, "Candidate.")],
   "Methotrexate is only for stable, unruptured cases with follow-up.",
   "Analyze ectopic pregnancy treatment."),

cloze(M(PA, SHOCK, "prioritize_hypotheses", "difficult", OB, "ob ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, followed by {{b2}}.",
   [[("treating hemorrhagic shock with IV access, fluids and blood", True, "Life-threatening bleeding."),
     ("starting methotrexate", False, "Contraindicated with rupture."),
     ("a repeat hCG in 48 hours", False, "Too slow."),
     ("pain control only", False, "Not enough.")],
    [("emergency laparoscopy or laparotomy", True, "Stops bleeding."),
     ("observation", False, "Unsafe."),
     ("dilation and curettage", False, "The pregnancy is not in the uterus."),
     ("discharge", False, "Unsafe.")]],
   "Resuscitate and operate.",
   "Prioritize ruptured ectopic pregnancy."),

sata(M(PA, SHOCK, "generate_solutions", "difficult", OB, "ob ms"),
   "Which actions should the nurse anticipate before surgery?",
   [("Two large-bore IVs", True, "Resuscitation."),
    ("Type and crossmatch", True, "Blood."),
    ("NPO", True, "Surgery."),
    ("Rho(D) immune globulin after surgery", True, "Rh negative."),
    ("A heating pad on the abdomen for pain", False, "Masks and is not indicated."),
    ("Methotrexate IM in the emergency department", False, "Rupture requires surgery.")],
   "Access, blood, NPO and Rh prophylaxis.",
   "Generate the preoperative plan.",
   ex=[note("Plan", "To the operating room for laparoscopic salpingectomy.", id="ect-plan")]),

sata(M(PSI, GRIEF, "take_action", "difficult", OB, "psych ob"),
   "After surgery, the client cries and says, \"I lost my baby and maybe my chance to have more.\" Which responses "
   "are therapeutic?",
   [("\"You've had a real loss. I'm here with you.\"", True, "Acknowledges grief."),
    ("\"Would you like me to call someone to be with you?\"", True, "Support."),
    ("\"Many conceive with one tube; let's ask your doctor.\"", True, "Accurate information."),
    ("\"At least it was early, before you got too attached.\"", False, "Minimizes the loss."),
    ("\"You can always try again soon, and next time will be fine.\"", False, "Dismissive.")],
   "Acknowledge the loss and give support and accurate information.",
   "Take action with grief support.",
   ex=[note("Post-op", "Right tube removed. Left tube normal.", id="ect-postop")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "At follow-up, for each finding or statement, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("hCG falling to zero", 0, "No remaining tissue."),
    ("\"I got the Rh shot before leaving the hospital.\"", 0, "Correct."),
    ("\"I'll see my provider early in any future pregnancy.\"", 0, "Higher repeat ectopic risk."),
    ("hCG plateaued at 400 for 2 weeks", 1, "Persistent tissue."),
    ("Low mood and not sleeping 6 weeks later", 1, "Screen for depression.")],
   "Recovery; persistent hCG or low mood needs follow-up.",
   "Evaluate ectopic pregnancy recovery.",
   ex=[note("2 weeks", "Incisions healing.", id="ect-2wk")]),
]

# ================================================================================================
# Case 4: Second-line uterotonics in a client with asthma and hypertension
# ================================================================================================
ut_tabs = [
    ("ut-notes", "Postpartum notes", [note("15 minutes after a vaginal birth",
        "A 37-year-old client (G5 P5) with asthma and chronic hypertension had a long labor with oxytocin "
        "augmentation and a 4,300 g baby. Oxytocin 40 units in 1 L is running and fundal massage was done, but "
        "bleeding continues.", id="ut-note")]),
    ("ut-data", "Findings", [flow("Findings", ["Birth", "15 minutes"], [
        ("Estimated blood loss", ["300 mL", "1,100 mL"]),
        ("Fundus", ["Firm", "Boggy, above the umbilicus"]),
        ("Blood pressure", ["154/96", "112/70 mm Hg"]),
        ("Heart rate", ["90", "118/min"]),
        ("Bladder", ["-", "Distended"]),
        ("Lung sounds", ["Clear", "Clear"])], id="ut-flow")]),
]

ut_qs = [
hl_table(M(PA, PPH, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that require immediate action.",
   ["Finding", "15 minutes"],
   [(["Blood loss", "1,100 mL"], True, "Postpartum hemorrhage."),
    (["Fundus", "Boggy, high"], True, "Uterine atony."),
    (["Heart rate", "118/min"], True, "Compensation."),
    (["Bladder", "Distended"], True, "Prevents contraction."),
    (["Blood pressure", "112/70"], True, "Fell from 154/96; relative hypotension.")],
   "Atony with heavy loss despite oxytocin needs escalation.",
   "Recognize cues of postpartum hemorrhage."),

matrix_mc(M(PPT, OB_MEDS, "analyze_cues", "difficult", OB, "ph ob"),
   "For each uterotonic, indicate whether it is safe for this client.",
   ["Safe to give", "Contraindicated for this client"],
   [("Methylergonovine IM", 1, "Contraindicated with hypertension."),
    ("Carboprost IM", 1, "Contraindicated with asthma (bronchospasm)."),
    ("Misoprostol rectally or under the tongue", 0, "Safe with asthma and hypertension."),
    ("Tranexamic acid IV within 3 hours", 0, "Reduces bleeding deaths."),
    ("More oxytocin per protocol", 0, "Continue.")],
   "Avoid methylergonovine in hypertension and carboprost in asthma.",
   "Analyze uterotonic contraindications."),

cloze(M(PA, PPH, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, because {{b2}}.",
   [[("empty the bladder with a catheter while continuing fundal massage", True, "A full bladder stops contraction."),
     ("give methylergonovine", False, "Contraindicated."),
     ("wait for the next vital signs", False, "Delay."),
     ("give a diuretic", False, "Harmful.")],
    [("a distended bladder displaces the uterus and prevents it contracting", True, "Correct."),
     ("it relieves pain", False, "Not the reason."),
     ("it prevents infection", False, "Not the reason."),
     ("it raises blood pressure", False, "Not the reason.")]],
   "Empty the bladder and massage.",
   "Prioritize postpartum hemorrhage."),

calc(M(PPT, CALC, "generate_solutions", "difficult", OB, "ph"),
   "Tranexamic acid 1 g IV over 10 minutes is prescribed. It is mixed in 100 mL. At what rate should the nurse set "
   "the pump? Record your answer in mL/hr.",
   "mL/hr", 0, "Rate", 600,
   "100 mL in 10 minutes = 100 / (10/60) = 600 mL/hr.",
   "Calculate a tranexamic acid rate.",
   ex=[note("Orders", "Misoprostol 800 mcg. Tranexamic acid 1 g IV over 10 minutes. Activate the hemorrhage protocol.", id="ut-orders")]),

sata(M(SIC, MED_SAFETY, "take_action", "difficult", OB, "ph ob"),
   "A second nurse brings carboprost from the hemorrhage kit. Which actions should the nurse take?",
   [("Hold it; state the asthma", True, "Prevents bronchospasm."),
    ("Check the history first", True, "Safety check."),
    ("Suggest a safer drug", True, "Alternative."),
    ("Give it because hemorrhage outweighs everything", False, "Safer options exist."),
    ("Give half the usual dose to lower the risk of bronchospasm", False, "Still dangerous.")],
   "Speak up and use a safer drug.",
   "Take action to prevent a medication error.",
   ex=[note("20 minutes", "Blood loss 1,300 mL. Bladder emptied: 600 mL.", id="ut-20")]),

matrix_mc(M(PA, PPH, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 1 hour, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Fundus firm at the umbilicus", 0, "Contracted."),
    ("Bleeding slowed to a moderate flow", 0, "Improving."),
    ("Heart rate 96/min", 0, "Improving."),
    ("Blood pressure 158/100 mm Hg", 1, "Hypertension needs treatment."),
    ("Shivering and temperature 38.6 C after misoprostol", 1, "A common side effect; still assess for infection.")],
   "Hemorrhage controlled; blood pressure and fever need follow-up.",
   "Evaluate hemorrhage treatment.",
   ex=[note("1 hour", "Total loss 1,500 mL. 2 units red cells given.", id="ut-1h")]),
]

cases = [
    case("First prenatal visit with pregnancy terminology", pn_tabs, pn_qs),
    case("Fetal growth restriction found by fundal height", fgr_tabs, fgr_qs),
    case("Ruptured ectopic pregnancy", ect_tabs, ect_qs),
    case("Postpartum hemorrhage needing second-line uterotonics", ut_tabs, ut_qs),
]

BUILT = build(137, "ob-cases-prenatal-iugr-ectopic-uterotonics",
              "Maternity case studies: first prenatal visit, fetal growth restriction, ruptured ectopic pregnancy and "
              "second-line uterotonics",
              items, cases, case_conversion=True)
