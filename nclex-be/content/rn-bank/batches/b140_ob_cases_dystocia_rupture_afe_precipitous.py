"""Batch 140 (case conversion, chunk 11: maternity part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: shoulder dystocia with a macrosomic baby of a client with gestational diabetes; uterine rupture during a trial
of labor after cesarean; amniotic fluid embolism with cardiac arrest and DIC; precipitous birth with the nurse alone,
transition-phase behavior and a nuchal cord. Each case walks the six CJMM steps in order, unfolds with new data in
steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
INTRAPARTUM = "intrapartum-complications"
LABOR = "labor-and-birth"
FHR = "fetal-heart-rate-monitoring"
NEWBORN_COMP = "newborn-complications"
NEWBORN = "newborn-care"
BLEED = "bleeding-and-clotting-disorders"
BLS = "basic-life-support"
DOCUMENT = "quality-improvement-and-incident-reporting"
HANDOFF = "handoff-and-sbar-communication"
COMM = "therapeutic-communication"
TEACH = "health-teaching-and-behavior-change"

items = []

# ================================================================================================
# Case 1: Shoulder dystocia
# ================================================================================================
sd_tabs = [
    ("sd-notes", "Labor notes", [note("Second stage",
        "A 32-year-old client with gestational diabetes on insulin is pushing at 40 weeks. Estimated fetal weight "
        "is 4,600 g. The second stage has lasted 2.5 hours.", id="sd-note")]),
    ("sd-data", "At birth", [table("Birth", ["Finding"], [
        ("Head", ["Delivered, then pulled back against the perineum (turtle sign)"]),
        ("Restitution", ["Did not occur"]),
        ("Next contraction", ["No further progress with gentle traction"]),
        ("FHR before birth", ["Category I"]),
        ("Time since head delivered", ["60 seconds"])], id="sd-table")]),
]

sd_qs = [
hl_table(M(PA, INTRAPARTUM, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings of shoulder dystocia.",
   ["Finding", "Detail"],
   [(["Turtle sign", "Head retracts"], True, "Classic."),
    (["No restitution", "Yes"], True, "Shoulders stuck."),
    (["No progress with traction", "Yes"], True, "Impaction."),
    (["Risk factors", "GDM, 4,600 g, long second stage"], True, "Macrosomia."),
    (["FHR before birth", "Category I"], False, "Reassuring.")],
   "The turtle sign after the head delivers means shoulder dystocia.",
   "Recognize cues of shoulder dystocia."),

matrix_mc(M(PA, INTRAPARTUM, "analyze_cues", "difficult", OB, "ob"),
   "For each injury, indicate who is mainly at risk after shoulder dystocia.",
   ["Mother", "Newborn"],
   [("Brachial plexus injury (Erb palsy)", 1, "Stretched nerves."),
    ("Fractured clavicle", 1, "Common."),
    ("Postpartum hemorrhage", 0, "Atony and tears."),
    ("Fourth-degree laceration", 0, "Maneuvers."),
    ("Hypoxic brain injury", 1, "Delay.")],
   "Both mother and newborn are at risk.",
   "Analyze shoulder dystocia injuries."),

cloze(M(PA, INTRAPARTUM, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, and must never {{b2}}.",
   [[("call for help and assist with the McRoberts maneuver", True, "First maneuver."),
     ("push on the top of the uterus", False, "Fundal pressure worsens impaction."),
     ("pull hard on the head", False, "Nerve injury."),
     ("leave to get equipment", False, "Stay.")],
    [("apply fundal pressure", True, "Worsens it."),
     ("time the event", False, "Should be done."),
     ("apply suprapubic pressure", False, "This is correct."),
     ("help the client flex the thighs", False, "This is McRoberts.")]],
   "McRoberts with suprapubic pressure; never fundal pressure.",
   "Prioritize shoulder dystocia."),

order(M(PA, INTRAPARTUM, "generate_solutions", "difficult", OB, "ob"),
   "Place the steps in the usual order.",
   ["Call for help and note the time",
    "McRoberts maneuver: flex the thighs sharply onto the abdomen",
    "Suprapubic pressure above the pubic bone",
    "Internal rotational maneuvers or delivery of the posterior arm by the provider",
    "Prepare for neonatal resuscitation"],
   "Help, McRoberts, suprapubic pressure, internal maneuvers, newborn readiness.",
   "Generate shoulder dystocia steps.",
   ex=[note("90 seconds", "Team arriving.", id="sd-90")]),

sata(M(MOC, DOCUMENT, "take_action", "difficult", OB, "mgmt ob"),
   "After the birth, which documentation actions are appropriate?",
   [("Record the time the head and body delivered", True, "Head-to-body interval."),
    ("List each maneuver in order and who did it", True, "Accurate record."),
    ("Record the newborn's arm movement at birth", True, "Baseline."),
    ("Write that the provider was too slow", False, "Opinion and blame."),
    ("Leave the event out of the chart to avoid legal issues", False, "Falsifies the record.")],
   "Objective, timed documentation.",
   "Take action with documentation.",
   ex=[note("Birth", "Delivered after McRoberts and suprapubic pressure; head-to-body 3 minutes.", id="sd-birth")]),

matrix_mc(M(PA, NEWBORN_COMP, "evaluate_outcomes", "difficult", OB, "ob peds"),
   "For each newborn finding, indicate whether it is reassuring or needs follow-up.",
   ["Reassuring", "Needs follow-up"],
   [("Apgar 6 and 9", 0, "Recovered."),
    ("Right arm limp at the side with an absent Moro on the right", 1, "Erb palsy."),
    ("Glucose 32 mg/dL at 1 hour", 1, "Hypoglycemia from maternal diabetes."),
    ("Crepitus over the left clavicle", 1, "Fracture."),
    ("Strong cry, pink", 0, "Reassuring.")],
   "Nerve injury, fracture and hypoglycemia need follow-up.",
   "Evaluate the newborn after shoulder dystocia.",
   ex=[note("1 hour", "Weight 4,650 g.", id="sd-1h")]),
]

# ================================================================================================
# Case 2: Uterine rupture during TOLAC
# ================================================================================================
ur_tabs = [
    ("ur-notes", "Labor notes", [note("Trial of labor after cesarean",
        "A 30-year-old client with one previous low transverse cesarean is in labor at 7 cm with oxytocin "
        "augmentation. The client suddenly screams with tearing abdominal pain between contractions.", id="ur-note")]),
    ("ur-data", "Findings", [flow("Findings", ["Before", "Now"], [
        ("FHR", ["140/min", "Prolonged deceleration to 70/min"]),
        ("Contractions", ["Every 3 minutes", "Stopped"]),
        ("Station", ["0", "-3 (presenting part moved up)"]),
        ("Maternal heart rate", ["90", "128/min"]),
        ("Maternal BP", ["122/78", "90/54 mm Hg"]),
        ("Abdomen", ["Normal", "Fetal parts easily felt under the skin"])], id="ur-flow")]),
]

ur_qs = [
hl_table(M(PA, INTRAPARTUM, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings of uterine rupture.",
   ["Finding", "Now"],
   [(["FHR", "Prolonged deceleration"], True, "Often the first sign."),
    (["Contractions", "Stopped"], True, "Loss of uterine integrity."),
    (["Station", "Lost"], True, "Fetus moves into the abdomen."),
    (["Shock", "HR 128, BP 90/54"], True, "Hemorrhage."),
    (["Fetal parts palpable", "Yes"], True, "Fetus outside the uterus.")],
   "Sudden pain, FHR collapse, lost station and shock during TOLAC mean rupture.",
   "Recognize cues of uterine rupture."),

matrix_mc(M(PA, INTRAPARTUM, "analyze_cues", "difficult", OB, "ob"),
   "For each factor, indicate whether it raises the risk of uterine rupture.",
   ["Raises the risk", "Does not raise the risk"],
   [("Previous classical (vertical) cesarean", 0, "Highest risk."),
    ("Oxytocin or prostaglandin induction", 0, "Raises risk."),
    ("Birth less than 18 months after the cesarean", 0, "Raises risk."),
    ("Previous low transverse incision with spontaneous labor", 1, "Lowest risk; TOLAC is reasonable."),
    ("Misoprostol for ripening after a cesarean", 0, "Contraindicated.")],
   "Scar type and uterotonics affect rupture risk.",
   "Analyze uterine rupture risk."),

cloze(M(PA, INTRAPARTUM, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should {{b1}} and call for {{b2}}.",
   [[("stop the oxytocin", True, "Remove the stimulant."),
     ("increase the oxytocin", False, "Harmful."),
     ("perform a vaginal exam to confirm", False, "Delays."),
     ("encourage pushing", False, "Not dilated.")],
    [("an emergency cesarean and blood products", True, "Immediate surgery."),
     ("an epidural", False, "Not the priority."),
     ("a repeat ultrasound tomorrow", False, "Too slow."),
     ("a chaplain only", False, "Not the priority.")]],
   "Stop oxytocin and prepare for surgery.",
   "Prioritize uterine rupture."),

sata(M(PA, INTRAPARTUM, "generate_solutions", "difficult", OB, "ob"),
   "Which actions should the nurse take while the operating room is prepared?",
   [("Two large-bore IVs and rapid fluids", True, "Shock."),
    ("Type and crossmatch, massive transfusion protocol", True, "Hemorrhage."),
    ("Insert a urinary catheter if time allows", True, "Bladder injury and surgery."),
    ("Neonatal team to the operating room", True, "Newborn resuscitation."),
    ("Wait for the FHR to recover before moving", False, "Delays."),
    ("Give terbutaline to relax the uterus", False, "Not appropriate.")],
   "Resuscitate, blood, catheter and newborn team.",
   "Generate the rupture plan.",
   ex=[note("Now", "Decision for emergency laparotomy.", id="ur-now")]),

sata(M(MOC, HANDOFF, "take_action", "difficult", OB, "mgmt ob"),
   "The nurse hands the client over to the operating room team. Which information is most important to include?",
   [("Time of onset and FHR since then", True, "Urgency."),
    ("Previous cesarean type and oxytocin dose", True, "Risk and cause."),
    ("Current vital signs and fluids given", True, "Resuscitation status."),
    ("Allergies and blood type", True, "Safety."),
    ("The client's preferred birth playlist", False, "Not now."),
    ("A detailed history of the first pregnancy's prenatal visits", False, "Not urgent.")],
   "A focused handoff in an emergency.",
   "Take action with an emergency handoff.",
   ex=[note("OR", "Transferring now.", id="ur-or")]),

matrix_mc(M(PA, INTRAPARTUM, "evaluate_outcomes", "difficult", OB, "ob"),
   "After surgery, for each finding, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Uterus repaired; hemoglobin 8.9 g/dL stable", 0, "Stable."),
    ("Hematuria with clots in the catheter", 1, "Possible bladder injury."),
    ("Newborn in NICU on cooling therapy", 1, "Hypoxic injury; family support."),
    ("Blood pressure 116/72 mm Hg", 0, "Stable."),
    ("\"Can I try a vaginal birth next time?\"", 1, "Future births need repeat cesarean before labor.")],
   "Recovery; bladder injury, newborn and future counseling need follow-up.",
   "Evaluate uterine rupture outcomes.",
   ex=[note("Post-op", "Baby born 14 minutes after rupture.", id="ur-postop")]),
]

# ================================================================================================
# Case 3: Amniotic fluid embolism
# ================================================================================================
afe_tabs = [
    ("afe-notes", "Labor notes", [note("Labor and delivery",
        "A 38-year-old client at 39 weeks had artificial rupture of membranes 5 minutes ago. The client suddenly "
        "says, \"Something is wrong, I can't breathe,\" and becomes agitated.", id="afe-note")]),
    ("afe-data", "Findings", [flow("Findings", ["0 minutes", "3 minutes"], [
        ("SpO2", ["98%", "78%"]),
        ("Blood pressure", ["128/80", "64/30 mm Hg"]),
        ("Mental status", ["Alert", "Unresponsive"]),
        ("Pulse", ["90", "Not palpable"]),
        ("FHR", ["135/min", "Bradycardia 60/min"])], id="afe-flow")]),
]

afe_qs = [
hl_table(M(PA, INTRAPARTUM, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that suggest amniotic fluid embolism.",
   ["Finding", "3 minutes"],
   [(["Sudden hypoxia", "SpO2 78%"], True, "Respiratory collapse."),
    (["Hypotension then no pulse", "Yes"], True, "Cardiovascular collapse."),
    (["Sense of doom", "\"Something is wrong\""], True, "Early warning."),
    (["Timing", "Minutes after membrane rupture"], True, "Typical timing."),
    (["FHR", "Bradycardia"], True, "Fetal hypoxia.")],
   "Sudden hypoxia and collapse around rupture or birth is amniotic fluid embolism.",
   "Recognize cues of amniotic fluid embolism."),

matrix_mc(M(PA, INTRAPARTUM, "analyze_cues", "difficult", OB, "ob"),
   "For each phase, indicate the main problem in amniotic fluid embolism.",
   ["Cardiopulmonary collapse", "Coagulopathy"],
   [("Sudden hypoxia and pulmonary hypertension", 0, "Early."),
    ("Right then left heart failure", 0, "Early."),
    ("Bleeding from IV sites and the uterus", 1, "DIC."),
    ("Low fibrinogen", 1, "DIC."),
    ("Cardiac arrest", 0, "Early.")],
   "Collapse comes first, then DIC.",
   "Analyze amniotic fluid embolism."),

cloze(M(PA, BLS, "prioritize_hypotheses", "difficult", OB, "ob aha"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should start CPR with {{b1}}, and if there is no return of circulation by 4 minutes, prepare for "
   "{{b2}}.",
   [[("manual left uterine displacement", True, "Relieves aortocaval compression."),
     ("the client turned fully prone", False, "Not possible for CPR."),
     ("the head of the bed raised", False, "Ineffective CPR."),
     ("no changes from usual CPR", False, "Displacement is needed.")],
    [("perimortem cesarean birth within 5 minutes of arrest", True, "Improves maternal and fetal survival."),
     ("waiting 30 minutes", False, "Too late."),
     ("stopping CPR", False, "Not appropriate."),
     ("vaginal birth", False, "Not possible.")]],
   "Left uterine displacement and perimortem cesarean.",
   "Prioritize maternal cardiac arrest."),

sata(M(PA, BLEED, "generate_solutions", "difficult", OB, "ob"),
   "Circulation returns after the cesarean, and the client starts to bleed from IV sites. Which orders should the "
   "nurse anticipate?",
   [("Massive transfusion with plasma, platelets and cryoprecipitate", True, "DIC."),
    ("Tranexamic acid", True, "Reduces bleeding."),
    ("Uterotonics for atony", True, "Uterine bleeding."),
    ("Fibrinogen, platelets and coagulation tests often", True, "Guide products."),
    ("Heparin to treat clotting", False, "Bleeding is the problem."),
    ("Hold all blood products until labs return", False, "Delays.")],
   "Balanced products, TXA, uterotonics and frequent labs.",
   "Generate the coagulopathy plan.",
   ex=[note("After ROSC", "Fibrinogen 90 mg/dL. Platelets 60,000.", id="afe-dic")]),

sata(M(PSI, COMM, "take_action", "difficult", OB, "psych ob"),
   "The partner is in the waiting room asking what is happening. Which actions are appropriate?",
   [("Assign a staff member to stay with and update the partner", True, "Support."),
    ("Use simple, honest language", True, "Clarity."),
    ("Offer a chaplain or social worker", True, "Support."),
    ("Tell the partner everything will be fine", False, "False reassurance."),
    ("Avoid speaking to the partner until the client is stable", False, "Leaves them alone.")],
   "Honest, supported communication.",
   "Take action with family support in a crisis.",
   ex=[note("ICU", "Client intubated in the ICU. Baby in the NICU.", id="afe-icu")]),

matrix_mc(M(PA, INTRAPARTUM, "evaluate_outcomes", "difficult", OB, "ob"),
   "On ICU day 3, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Extubated, oxygen 2 L/min", 0, "Improved."),
    ("Fibrinogen 260 mg/dL", 0, "Recovered."),
    ("Follows commands, oriented", 0, "Neurologically intact."),
    ("New nightmares about the birth and panic when nurses come in", 1, "Possible acute stress or PTSD."),
    ("Creatinine 2.4 mg/dL", 1, "Kidney injury.")],
   "Recovery; psychological and kidney effects need follow-up.",
   "Evaluate amniotic fluid embolism recovery.",
   ex=[note("Day 3", "Baby doing well.", id="afe-day3")]),
]

# ================================================================================================
# Case 4: Precipitous birth with the nurse alone
# ================================================================================================
prec_tabs = [
    ("prec-notes", "Labor notes", [note("Labor room",
        "A 29-year-old client (G4 P3) arrived at 8 cm. The provider is 20 minutes away. The client was calm with "
        "breathing but is now irritable, nauseated and shaking, and says, \"I can't do this anymore.\" Minutes "
        "later the client says, \"The baby is coming,\" and the head is crowning.", id="prec-note")]),
    ("prec-data", "Findings", [table("Findings", ["Finding"], [
        ("Cervix", ["Fully dilated"]),
        ("Head", ["Crowning"]),
        ("FHR", ["130/min"]),
        ("Fluid", ["Clear"]),
        ("Help", ["Call light pressed; another nurse coming"])], id="prec-table")]),
]

prec_qs = [
hl_table(M(HPM, LABOR, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that show the transition phase moving to birth.",
   ["Finding", "Detail"],
   [(["Behavior", "Irritable, \"I can't do this\""], True, "Transition."),
    (["Nausea and shaking", "Yes"], True, "Transition."),
    (["Urge to push", "\"The baby is coming\""], True, "Second stage."),
    (["Crowning", "Yes"], True, "Imminent birth."),
    (["FHR", "130/min"], False, "Normal.")],
   "Transition behavior then crowning means imminent birth.",
   "Recognize cues of imminent birth."),

matrix_mc(M(HPM, LABOR, "analyze_cues", "difficult", OB, "ob"),
   "For each phase of the first stage, indicate its typical features.",
   ["Latent phase", "Active phase", "Transition"],
   [("0 to 5 cm, talkative, excited", 0, "Latent."),
    ("6 to 7 cm, focused, more intense contractions", 1, "Active."),
    ("8 to 10 cm, irritable, nausea, shaking, urge to push", 2, "Transition."),
    ("Contractions every 5 to 30 minutes, mild", 0, "Latent."),
    ("Contractions every 2 to 3 minutes, 60 to 90 seconds, strong", 2, "Transition.")],
   "Phases differ in dilation and behavior.",
   "Analyze phases of labor."),

cloze(M(HPM, LABOR, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should {{b1}}, and support the head with {{b2}}.",
   [[("stay with the client and prepare for birth", True, "Never leave."),
     ("leave to find the provider", False, "Unsafe."),
     ("hold the client's legs together", False, "Causes injury."),
     ("tell the client to stop pushing completely", False, "Not possible.")],
    [("gentle pressure to prevent a sudden expulsion", True, "Controlled birth."),
     ("firm traction on the head", False, "Injury."),
     ("no support", False, "Tears."),
     ("pushing the head back in", False, "Harmful.")]],
   "Stay and control the birth.",
   "Prioritize precipitous birth."),

sata(M(HPM, LABOR, "generate_solutions", "difficult", OB, "ob"),
   "After the head delivers, which actions should the nurse take?",
   [("Feel for a cord around the neck", True, "Nuchal cord."),
    ("Slip a loose cord over the head", True, "Allows birth."),
    ("Let the shoulders deliver with the next contraction, guiding gently", True, "Controlled."),
    ("Dry the newborn and place skin to skin", True, "Warmth."),
    ("Cut the cord immediately before the shoulders deliver", False, "Only if tight and cannot be reduced."),
    ("Pull on the cord to deliver the placenta quickly", False, "Uterine inversion.")],
   "Check for a cord, guide the shoulders, warm the newborn.",
   "Generate precipitous birth steps.",
   ex=[note("Birth", "Loose cord around the neck once.", id="prec-cord")]),

sata(M(MOC, DOCUMENT, "take_action", "difficult", OB, "mgmt ob"),
   "After the birth, which actions should the nurse take?",
   [("Record the time of birth and who was present", True, "Documentation."),
    ("Check the fundus and watch for bleeding", True, "Hemorrhage risk."),
    ("Do not pull on the cord; wait for signs of placental separation", True, "Prevents inversion."),
    ("Report the event to the provider on arrival", True, "Handoff."),
    ("Document that the provider was late as the cause of any injury", False, "Blame."),
    ("Leave the client alone to call the family", False, "Unsafe.")],
   "Document, assess bleeding and wait for the placenta.",
   "Take action after a precipitous birth.",
   ex=[note("5 minutes", "Provider arrived. Placenta not yet delivered.", id="prec-5")]),

matrix_mc(M(HPM, LABOR, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 1 hour, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Fundus firm at the umbilicus", 0, "Normal."),
    ("Newborn temperature 36.6 C skin to skin", 0, "Normal."),
    ("Small first-degree tear, no bleeding", 0, "Minor."),
    ("Severe perineal pain with a tense purple swelling", 1, "Hematoma."),
    ("Newborn breastfeeding", 0, "Normal.")],
   "A tense painful swelling after a rapid birth is a hematoma.",
   "Evaluate after a precipitous birth.",
   ex=[note("1 hour", "Placenta delivered intact.", id="prec-1h")]),
]

cases = [
    case("Shoulder dystocia with a macrosomic baby", sd_tabs, sd_qs),
    case("Uterine rupture during a trial of labor after cesarean", ur_tabs, ur_qs),
    case("Amniotic fluid embolism with cardiac arrest", afe_tabs, afe_qs),
    case("Precipitous birth with the nurse alone", prec_tabs, prec_qs),
]

BUILT = build(140, "ob-cases-dystocia-rupture-afe-precipitous",
              "Maternity case studies: shoulder dystocia, uterine rupture, amniotic fluid embolism and precipitous "
              "birth",
              items, cases, case_conversion=True)
