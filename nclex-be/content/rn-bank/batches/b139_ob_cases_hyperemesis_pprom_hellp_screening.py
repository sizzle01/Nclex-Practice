"""Batch 139 (case conversion, chunk 11: maternity part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: hyperemesis gravidarum with dehydration, hypokalemia and thiamine; preterm prelabor rupture of membranes with
chorioamnionitis; HELLP syndrome; prenatal screening after an elevated AFP with amniocentesis, folic acid and
alcohol counseling. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a
management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
COMPLICATIONS = "pregnancy-complications"
PRENATAL = "preconception-and-prenatal-care"
PRENATAL_TEST = "prenatal-testing"
OB_MEDS = "obstetric-medications"
LYTES = "electrolyte-imbalances"
FLUIDS = "fluid-volume-imbalances"
ABX = "antimicrobial-therapy"
LABS = "laboratory-values-and-monitoring"
CONSENT = "informed-consent"
TEACH = "health-teaching-and-behavior-change"
COMM = "therapeutic-communication"
ETHICS = "ethical-principles-and-dilemmas"

items = []

# ================================================================================================
# Case 1: Hyperemesis gravidarum
# ================================================================================================
hg_tabs = [
    ("hg-notes", "Emergency notes", [note("10 weeks",
        "A 24-year-old client at 10 weeks with twins has vomited 10 to 15 times a day for a week and cannot keep "
        "fluids down. The client feels dizzy when standing.", id="hg-note")]),
    ("hg-data", "Findings", [table("Findings", ["Result"], [
        ("Weight", ["Down 4 kg (more than 5% of pre-pregnancy weight)"]),
        ("Heart rate", ["118/min"]),
        ("Blood pressure", ["98/60 mm Hg, falls to 84/52 standing"]),
        ("Urine ketones", ["3+"]),
        ("Urine specific gravity", ["1.032"]),
        ("Potassium", ["2.9 mEq/L"]),
        ("TSH", ["Low (from high hCG)"])], id="hg-table")]),
]

hg_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that distinguish hyperemesis gravidarum from usual morning sickness.",
   ["Finding", "Result"],
   [(["Weight loss", "More than 5%"], True, "A defining feature."),
    (["Ketonuria", "3+"], True, "Starvation."),
    (["Orthostatic hypotension", "84/52 standing"], True, "Dehydration."),
    (["Potassium", "2.9 mEq/L"], True, "Electrolyte loss."),
    (["Twins", "Yes"], False, "A risk factor, not a distinguishing finding.")],
   "Weight loss over 5%, ketosis, dehydration and electrolyte loss define hyperemesis.",
   "Recognize cues of hyperemesis gravidarum."),

matrix_mc(M(PA, LYTES, "analyze_cues", "difficult", OB, "ob ms"),
   "For each finding, indicate its cause.",
   ["Loss of stomach acid", "Starvation", "Volume loss"],
   [("Metabolic alkalosis", 0, "HCl loss."),
    ("Ketones in the urine", 1, "Fat breakdown."),
    ("High urine specific gravity", 2, "Concentrated urine."),
    ("Orthostatic hypotension", 2, "Low volume."),
    ("Low potassium", 0, "Gastric and renal losses with alkalosis.")],
   "Vomiting causes alkalosis, ketosis and dehydration.",
   "Analyze hyperemesis complications."),

cloze(M(PPT, "parenteral-nutrition", "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Before giving dextrose-containing fluids, the nurse should give {{b1}} to prevent {{b2}}.",
   [[("thiamine", True, "Prolonged vomiting depletes thiamine."),
     ("vitamin K", False, "Not the concern."),
     ("iron", False, "Not urgent."),
     ("folic acid alone", False, "Not the concern.")],
    [("Wernicke encephalopathy", True, "Glucose without thiamine can trigger it."),
     ("hyperglycemia", False, "Not the main risk."),
     ("anemia", False, "Not the main risk."),
     ("hypertension", False, "Not the main risk.")]],
   "Thiamine before dextrose.",
   "Prioritize hyperemesis treatment."),

sata(M(PPT, OB_MEDS, "generate_solutions", "difficult", OB, "ob ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("IV fluids with added potassium", True, "Volume and potassium."),
    ("IV antiemetics", True, "Antiemetics."),
    ("Small bland meals later", True, "Gradual refeeding."),
    ("Daily weight and intake and output", True, "Monitoring."),
    ("Potassium chloride IV push to correct the level quickly", False, "Never IV push."),
    ("Large fatty meals to regain weight quickly", False, "Worsens nausea.")],
   "Fluids with potassium, antiemetics, gradual refeeding and monitoring.",
   "Generate the hyperemesis plan.",
   ex=[note("Orders", "Thiamine 100 mg IV, then LR with KCl 20 mEq/L at 150 mL/hr. Ondansetron IV.", id="hg-orders")]),

sata(M(PSI, COMM, "take_action", "difficult", OB, "psych ob"),
   "The client cries and says, \"I can't do this for 30 more weeks. Maybe I should end the pregnancy.\" Which "
   "responses are therapeutic?",
   [("\"This sounds exhausting. Tell me more.\"", True, "Explores."),
    ("\"Most improve by 20 weeks.\"", True, "Accurate hope."),
    ("\"I can arrange a talk about your options.\"", True, "Respects autonomy."),
    ("\"You'll feel terrible later if you do that to your babies.\"", False, "Judgmental."),
    ("\"It's just morning sickness; everyone has it in the first trimester.\"", False, "Minimizes.")],
   "Validate, offer realistic hope and respect choices.",
   "Take action with emotional support in hyperemesis.",
   ex=[note("Day 2", "Vomiting twice a day. Ketones trace.", id="hg-day2")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "On day 4, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Potassium 4.0 mEq/L", 0, "Corrected."),
    ("Ketones negative", 0, "Resolved."),
    ("Tolerating crackers and soup", 0, "Refeeding."),
    ("New QT prolongation on ECG after several ondansetron doses", 1, "Ondansetron prolongs QT."),
    ("Orthostatic changes resolved", 0, "Rehydrated.")],
   "Improvement; QT prolongation needs review.",
   "Evaluate hyperemesis treatment.",
   ex=[note("Day 4", "Planning discharge with oral antiemetics.", id="hg-day4")]),
]

# ================================================================================================
# Case 2: PPROM with chorioamnionitis
# ================================================================================================
pp_tabs = [
    ("pp-notes", "Antepartum notes", [note("33 weeks",
        "A 23-year-old client at 33 weeks had a gush of clear fluid 2 days ago and is on the antepartum unit. "
        "Today the client feels unwell.", id="pp-note")]),
    ("pp-data", "Findings", [flow("Findings", ["Day 1", "Day 3"], [
        ("Maternal temperature", ["37.0", "38.6 C"]),
        ("Maternal heart rate", ["82", "118/min"]),
        ("Fetal heart rate baseline", ["140", "172/min"]),
        ("Uterus", ["Nontender", "Tender between contractions"]),
        ("Fluid", ["Clear", "Cloudy, foul-smelling"]),
        ("WBC", ["11,000", "19,500/mm3"])], id="pp-flow")]),
]

pp_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the day 3 findings that suggest chorioamnionitis.",
   ["Finding", "Day 3"],
   [(["Maternal temperature", "38.6 C"], True, "Fever."),
    (["Fetal tachycardia", "172/min"], True, "Fetal response to infection."),
    (["Uterine tenderness", "Yes"], True, "Infection of the uterus."),
    (["Fluid", "Foul, cloudy"], True, "Infection."),
    (["Maternal tachycardia", "118/min"], True, "Systemic response.")],
   "Fever with fetal tachycardia, uterine tenderness and foul fluid after PPROM is chorioamnionitis.",
   "Recognize cues of chorioamnionitis."),

matrix_mc(M(RRP, PRENATAL_TEST, "analyze_cues", "difficult", OB, "ob"),
   "For each finding, indicate whether it confirms rupture of membranes.",
   ["Supports rupture", "Does not support rupture"],
   [("Pooling of fluid in the vagina on sterile speculum exam", 0, "Confirms."),
    ("Nitrazine paper turns blue (pH above 6.5)", 0, "Amniotic fluid is alkaline."),
    ("Ferning on a dried slide", 0, "Amniotic fluid crystals."),
    ("Urine leak with coughing and acidic pH", 1, "Urine."),
    ("Low amniotic fluid on ultrasound", 0, "Supports.")],
   "Pooling, nitrazine and ferning confirm rupture.",
   "Analyze rupture of membranes."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "Because of chorioamnionitis, the plan is {{b1}}, and tocolytics are {{b2}}.",
   [[("broad-spectrum IV antibiotics and birth", True, "Infection requires birth."),
     ("antibiotics and keep the pregnancy to 37 weeks", False, "Infection worsens."),
     ("observation without antibiotics", False, "Sepsis risk."),
     ("a cerclage", False, "Not indicated.")],
    [("contraindicated", True, "Keeps the infected fetus in utero."),
     ("first-line", False, "No."),
     ("required for 48 hours", False, "No."),
     ("given with antibiotics", False, "No.")]],
   "Antibiotics and birth; no tocolysis.",
   "Prioritize chorioamnionitis."),

sata(M(PPT, ABX, "generate_solutions", "difficult", OB, "ph ob"),
   "Which orders should the nurse anticipate?",
   [("IV ampicillin and gentamicin", True, "Standard."),
    ("Induction of labor with oxytocin", True, "Birth."),
    ("Acetaminophen for fever", True, "Fever harms the fetal brain."),
    ("Notify the neonatal team for the birth", True, "Septic newborn risk."),
    ("Frequent digital cervical exams", False, "Increase infection."),
    ("Delay antibiotics until after birth", False, "Treat now.")],
   "Antibiotics, birth, antipyretic and neonatal team.",
   "Generate the chorioamnionitis plan.",
   ex=[note("Orders", "Ampicillin 2 g IV every 6 hours. Gentamicin. Oxytocin induction.", id="pp-orders")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", OB, "mgmt ob"),
   "Which actions help coordinate care around the birth?",
   [("Give the neonatal team the maternal temperature, antibiotic times and gestational age", True, "Newborn risk."),
    ("Arrange a NICU tour or meeting with the neonatologist for the family", True, "Preparation."),
    ("Document antibiotic administration times accurately", True, "Newborn sepsis evaluation depends on it."),
    ("Promise the parents the baby will not need the NICU", False, "Cannot promise."),
    ("Omit the fever from the handoff to avoid worrying the team", False, "Unsafe.")],
   "Share key data and prepare the family.",
   "Take action to coordinate perinatal care.",
   ex=[note("Labor", "Active labor at 6 cm.", id="pp-labor")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "After birth, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Maternal temperature 37.2 C at 24 hours", 0, "Resolved."),
    ("Uterus firm, lochia without odor", 0, "Resolving."),
    ("Newborn on IV antibiotics in the NICU, stable", 0, "Treated."),
    ("Maternal temperature 39 C on day 2 with tender uterus", 1, "Postpartum endometritis."),
    ("Newborn with new grunting and poor feeding", 1, "Possible sepsis.")],
   "Improvement; persistent fever or newborn signs need follow-up.",
   "Evaluate chorioamnionitis care.",
   ex=[note("Birth", "Vaginal birth at 33 weeks.", id="pp-birth")]),
]

# ================================================================================================
# Case 3: HELLP syndrome
# ================================================================================================
hellp_tabs = [
    ("hellp-notes", "Triage notes", [note("33 weeks",
        "A 31-year-old client at 33 weeks has nausea, malaise and pain under the right ribs. The client thought it "
        "was the flu.", id="hellp-note")]),
    ("hellp-data", "Results", [table("Results", ["Result"], [
        ("Blood pressure", ["156/98 mm Hg"]),
        ("Platelets", ["64,000/mm3"]),
        ("AST / ALT", ["310 / 280 units/L"]),
        ("LDH", ["820 units/L"]),
        ("Hemoglobin", ["9.8 g/dL, schistocytes on the smear"]),
        ("Urine protein", ["2+"])], id="hellp-table")]),
]

hellp_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings of HELLP syndrome.",
   ["Finding", "Result"],
   [(["Hemolysis", "LDH 820, schistocytes"], True, "H."),
    (["Elevated liver enzymes", "AST 310"], True, "EL."),
    (["Low platelets", "64,000"], True, "LP."),
    (["Right upper quadrant pain", "Yes"], True, "Liver swelling."),
    (["Nausea and malaise", "Flu-like"], True, "Often mistaken for flu.")],
   "Hemolysis, elevated liver enzymes and low platelets.",
   "Recognize cues of HELLP syndrome."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob"),
   "For each complication, indicate whether HELLP syndrome raises the risk.",
   ["Raises the risk", "Does not raise the risk"],
   [("Liver hematoma or rupture", 0, "Capsule stretching."),
    ("Placental abruption", 0, "Risk rises."),
    ("DIC", 0, "Risk rises."),
    ("Eclamptic seizures", 0, "Risk rises."),
    ("Gestational diabetes", 1, "Not related.")],
   "HELLP is a severe form of preeclampsia with serious complications.",
   "Analyze HELLP complications."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The definitive treatment is {{b1}}, and {{b2}} is given to prevent seizures.",
   [[("birth of the baby", True, "Cure."),
     ("bed rest until term", False, "Dangerous."),
     ("platelet transfusion alone", False, "Not definitive."),
     ("antibiotics", False, "Not infection.")],
    [("magnesium sulfate", True, "Seizure prophylaxis."),
     ("phenytoin", False, "Not first-line."),
     ("lorazepam", False, "Not prophylaxis."),
     ("oxytocin", False, "Not for seizures.")]],
   "Birth with magnesium.",
   "Prioritize HELLP syndrome."),

sata(M(PA, COMPLICATIONS, "generate_solutions", "difficult", OB, "ob ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Magnesium sulfate infusion", True, "Seizures."),
    ("Labetalol or hydralazine for severe blood pressure", True, "Prevents stroke."),
    ("Betamethasone if time allows", True, "Fetal lungs."),
    ("Platelets ready if a cesarean is needed with very low counts", True, "Bleeding."),
    ("IM injections for pain", False, "Bleeding with low platelets."),
    ("Delay birth for 2 weeks to give steroids more time", False, "Too dangerous.")],
   "Magnesium, BP control, steroids if possible and blood products.",
   "Generate the HELLP plan.",
   ex=[note("Orders", "Magnesium 4 g load, then 2 g/hr. Labetalol IV. Plan birth within 24 to 48 hours.", id="hellp-orders")]),

sata(M(PA, COMPLICATIONS, "take_action", "difficult", OB, "ob"),
   "Six hours later, the client has sudden severe right shoulder pain and becomes pale. Review the findings. Which "
   "actions should the nurse take?",
   [("Call the rapid response or obstetric emergency team", True, "Possible liver rupture."),
    ("Avoid any abdominal palpation", True, "Can worsen rupture."),
    ("Prepare for emergency surgery and massive transfusion", True, "Hemorrhage."),
    ("Give an opioid and reassess in an hour", False, "Delays care."),
    ("Massage the abdomen to locate the pain", False, "Dangerous.")],
   "Shoulder pain with shock in HELLP suggests liver capsule rupture.",
   "Take action on liver rupture.",
   ex=[flow("6 hours", ["Start", "6 hours"], [
       ("Blood pressure", ["156/98", "88/50 mm Hg"]),
       ("Heart rate", ["96", "136/min"]),
       ("Hemoglobin", ["9.8", "7.1 g/dL"])], id="hellp-6h")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob"),
   "On postpartum day 2, for each finding, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Platelets 112,000 and rising", 0, "Recovering."),
    ("AST falling", 0, "Liver recovering."),
    ("Blood pressure 158/104 with a headache", 1, "Postpartum preeclampsia can worsen."),
    ("Urine output 15 mL/hr", 1, "Kidney injury or magnesium risk."),
    ("Ambulating with help", 0, "Recovery.")],
   "Labs recovering; blood pressure and output need follow-up.",
   "Evaluate HELLP recovery.",
   ex=[note("Day 2", "Laparotomy repaired a liver hematoma; cesarean birth. Magnesium continued 24 hours.", id="hellp-day2")]),
]

# ================================================================================================
# Case 4: Prenatal screening
# ================================================================================================
scr_tabs = [
    ("scr-notes", "Prenatal notes", [note("16 weeks",
        "A 31-year-old client at 16 weeks had a previous pregnancy affected by a neural tube defect. The client did "
        "not take the high-dose folic acid recommended before conception and drank wine at a party before knowing "
        "about the pregnancy.", id="scr-note")]),
    ("scr-data", "Results", [table("Results", ["Result"], [
        ("Maternal serum AFP", ["2.8 MoM (elevated)"]),
        ("Dating", ["Confirmed by first-trimester ultrasound"]),
        ("Twins", ["No"]),
        ("Cell-free DNA", ["Low risk for trisomies"]),
        ("Rh status", ["A negative"])], id="scr-table")]),
]

scr_qs = [
hl_table(M(RRP, PRENATAL_TEST, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that need follow-up.",
   ["Finding", "Result"],
   [(["AFP", "Elevated"], True, "Possible open neural tube or abdominal wall defect."),
    (["History", "Previous neural tube defect"], True, "Higher recurrence risk."),
    (["No preconception folic acid", "Yes"], True, "Risk."),
    (["Rh negative", "A negative"], True, "Needs Rho(D) if amniocentesis is done."),
    (["Cell-free DNA", "Low risk"], False, "Reassuring for trisomies.")],
   "An elevated AFP with prior NTD needs a detailed ultrasound.",
   "Recognize cues of abnormal prenatal screening."),

matrix_mc(M(RRP, PRENATAL_TEST, "analyze_cues", "difficult", OB, "ob"),
   "For each test, indicate whether it is a screening or a diagnostic test.",
   ["Screening", "Diagnostic"],
   [("Maternal serum AFP", 0, "Screening."),
    ("Cell-free DNA", 0, "Screening."),
    ("Amniocentesis with karyotype and amniotic AFP", 1, "Diagnostic."),
    ("Chorionic villus sampling", 1, "Diagnostic."),
    ("Nuchal translucency", 0, "Screening.")],
   "Screening finds risk; diagnostic tests confirm.",
   "Analyze prenatal tests."),

cloze(M(RRP, PRENATAL_TEST, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The next step is {{b1}}, and AFP can also be falsely high with {{b2}}.",
   [[("a detailed (level II) ultrasound", True, "Looks for defects."),
     ("immediate termination", False, "Not a diagnosis."),
     ("no follow-up", False, "Unsafe."),
     ("a glucose test", False, "Not related.")],
    [("wrong dates or a multiple pregnancy", True, "Common causes."),
     ("Rh-negative blood", False, "Not a cause."),
     ("drinking water", False, "Not a cause."),
     ("a normal pregnancy at 16 weeks", False, "Not a cause.")]],
   "Ultrasound next; dating and twins can raise AFP.",
   "Prioritize abnormal AFP."),

sata(M(MOC, CONSENT, "generate_solutions", "difficult", OB, "mgmt ob"),
   "The ultrasound shows a small lumbar spina bifida. The client is offered amniocentesis. Which actions are part "
   "of a sound plan?",
   [("Refer for genetic counseling before the test", True, "Informed choice."),
    ("Explain the procedure risk of miscarriage is small", True, "Accurate."),
    ("Give Rho(D) immune globulin after the procedure", True, "Rh negative."),
    ("Respect the client's choice whatever the decision", True, "Autonomy."),
    ("Tell the client which choice the nurse would make", False, "Imposes values."),
    ("Schedule the test without consent because it is routine", False, "Requires consent.")],
   "Counseling, accurate risk, Rh prophylaxis and respect.",
   "Generate the prenatal diagnosis plan.",
   ex=[note("Ultrasound", "Open lumbar spina bifida; head and heart normal.", id="scr-us")]),

sata(M(HPM, TEACH, "take_action", "difficult", OB, "ob"),
   "After amniocentesis, which instructions should the nurse give?",
   [("Report fluid leaking, bleeding or cramping", True, "Complications."),
    ("Report fever", True, "Infection."),
    ("Rest for the remainder of the day and avoid heavy lifting", True, "Comfort."),
    ("Expect mild cramping that settles within hours", True, "Common."),
    ("Douche to prevent infection", False, "Not recommended."),
    ("Ignore decreased fetal movement for a week", False, "Report.")],
   "Report leaking, bleeding, fever and reduced movement.",
   "Take action with post-amniocentesis teaching.",
   ex=[note("After the test", "Rho(D) immune globulin given.", id="scr-after")]),

matrix_mc(M(HPM, PRENATAL, "evaluate_outcomes", "difficult", OB, "ob"),
   "At follow-up, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll plan birth at a center with neurosurgery.\"", 0, "Correct."),
    ("\"Next time I'll start 4 mg of folic acid at least a month before trying.\"", 0, "High-dose folic acid for prior NTD."),
    ("\"No amount of alcohol is known to be safe, so I've stopped.\"", 0, "Correct."),
    ("\"The one glass of wine caused the spina bifida.\"", 1, "Not the cause; addresses guilt."),
    ("\"I'll ask about fetal surgery options.\"", 0, "Appropriate.")],
   "Planning and prevention; guilt needs gentle correction.",
   "Evaluate prenatal screening teaching.",
   ex=[note("20 weeks", "Amniocentesis confirmed an isolated NTD; chromosomes normal.", id="scr-20")]),
]

cases = [
    case("Hyperemesis gravidarum", hg_tabs, hg_qs),
    case("Preterm prelabor rupture of membranes with chorioamnionitis", pp_tabs, pp_qs),
    case("HELLP syndrome with liver hematoma", hellp_tabs, hellp_qs),
    case("Prenatal screening after an elevated AFP", scr_tabs, scr_qs),
]

BUILT = build(139, "ob-cases-hyperemesis-pprom-hellp-screening",
              "Maternity case studies: hyperemesis, PPROM with chorioamnionitis, HELLP syndrome and prenatal "
              "screening",
              items, cases, case_conversion=True)
