"""Batch 136 (case conversion, chunk 10: maternity): four full NGN case studies (CASE-MAP.md, book_topics.py).

Cases: oxytocin induction with tachysystole and recurrent late decelerations (electronic fetal monitoring and
intrauterine resuscitation); umbilical cord prolapse after rupture of membranes; preterm labor with nifedipine,
betamethasone and magnesium for neuroprotection; labor with an epidural, the stages of labor and maternal
hypotension. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a
management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
OB = "maternal-newborn"
FHR = "fetal-heart-rate-monitoring"
LABOR = "labor-and-birth"
COMPLICATIONS = "pregnancy-complications"
OB_MEDS = "obstetric-medications"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
HANDOFF = "handoff-and-sbar-communication"
CALC = "medication-dose-calculations"
PAIN = "pain-assessment-and-comfort"
TEACH = "health-teaching-and-behavior-change"
SAFE = "safe-client-handling"

items = []

# ================================================================================================
# Case 1: Oxytocin induction with tachysystole and late decelerations
# ================================================================================================
oxy_tabs = [
    ("oxy-notes", "Labor notes", [note("Labor and delivery, 41 weeks",
        "A 30-year-old client (G1 P0) is being induced for post-term pregnancy. Oxytocin was increased to "
        "12 milliunits/min an hour ago. The cervix is 5 cm dilated.", id="oxy-note")]),
    ("oxy-data", "Monitor strip summary", [flow("Last 30 minutes", ["Value"], [
        ("Contractions", ["7 in 10 minutes, each 90 seconds"]),
        ("Resting tone", ["Firm between contractions"]),
        ("Baseline FHR", ["160/min"]),
        ("Variability", ["Minimal (3 to 5 beats/min)"]),
        ("Decelerations", ["Gradual drops starting after each contraction peak, returning after it ends"]),
        ("Maternal BP", ["118/74 mm Hg"])], id="oxy-flow")]),
]

oxy_qs = [
hl_table(M(RRP, FHR, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Contractions", "7 in 10 minutes"], True, "Tachysystole (more than 5 in 10)."),
    (["Resting tone", "Firm"], True, "No recovery between contractions."),
    (["Decelerations", "Late"], True, "Uteroplacental insufficiency."),
    (["Variability", "Minimal"], True, "Reduced fetal reserve."),
    (["Baseline", "160/min"], True, "Upper limit; trending to tachycardia."),
    (["Maternal BP", "118/74"], False, "Normal.")],
   "Tachysystole with late decelerations and minimal variability is a Category II to III pattern.",
   "Recognize cues on the fetal monitor."),

matrix_mc(M(RRP, FHR, "analyze_cues", "difficult", OB, "ob"),
   "For each deceleration or pattern, indicate its usual cause (VEAL CHOP).",
   ["Cord compression", "Head compression", "Uteroplacental insufficiency", "Fetal well-being"],
   [("Variable decelerations", 0, "V = cord."),
    ("Early decelerations", 1, "E = head."),
    ("Accelerations", 3, "A = OK."),
    ("Late decelerations", 2, "L = placenta.")],
   "VEAL CHOP links each pattern to a cause.",
   "Analyze fetal heart rate patterns."),

cloze(M(PPT, OB_MEDS, "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first action is to {{b1}}, because {{b2}}.",
   [[("stop the oxytocin infusion", True, "Removes the cause."),
     ("increase the oxytocin to speed birth", False, "Worsens hypoxia."),
     ("prepare for an epidural", False, "Not the priority."),
     ("perform a cervical exam", False, "Not first.")],
    [("frequent strong contractions reduce placental blood flow", True, "Tachysystole causes late decelerations."),
     ("the cervix is not dilating", False, "Not the issue."),
     ("the client is tired", False, "Not the issue."),
     ("the FHR is normal", False, "It is not.")]],
   "Stop the oxytocin first.",
   "Prioritize oxytocin tachysystole."),

order(M(RRP, FHR, "generate_solutions", "difficult", OB, "ob"),
   "Place the intrauterine resuscitation steps in order after stopping the oxytocin.",
   ["Turn the client to the left side",
    "Give an IV fluid bolus of lactated Ringer's",
    "Give oxygen only if the mother is hypoxic, per current guidance",
    "Give terbutaline subcutaneously if tachysystole continues, as prescribed",
    "Notify the provider and document the response"],
   "Reposition, fluids, oxygen if needed, tocolytic if needed, then notify and document.",
   "Generate intrauterine resuscitation steps.",
   ex=[note("Now", "Oxytocin stopped.", id="oxy-stop")]),

sata(M(MOC, HANDOFF, "take_action", "difficult", OB, "mgmt ob"),
   "The nurse calls the provider using SBAR. Which statements belong in the report?",
   [("\"Situation: recurrent late decelerations on oxytocin.\"", True, "Situation."),
    ("\"Background: G1 at 41 weeks, induction, 5 cm.\"", True, "Background."),
    ("\"Assessment: tachysystole; oxytocin off, bolus given.\"", True, "Assessment and actions."),
    ("\"Recommendation: please come to evaluate now.\"", True, "Clear request."),
    ("\"I think the baby is probably fine, so there is no rush to come.\"", False, "Inaccurate and vague."),
    ("\"The resident ordered too much oxytocin again, like last week.\"", False, "Blame, not facts.")],
   "SBAR gives a clear, factual report with a request.",
   "Take action with SBAR.",
   ex=[note("10 minutes later", "Contractions 4 in 10 minutes. Late decelerations still present.", id="oxy-10")]),

matrix_mc(M(RRP, FHR, "evaluate_outcomes", "difficult", OB, "ob"),
   "At 30 minutes, for each finding, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Contractions 3 in 10 minutes, soft between", 0, "Tachysystole resolved."),
    ("Moderate variability returned", 0, "Fetal reserve recovered."),
    ("Accelerations with fetal scalp stimulation", 0, "Reassuring."),
    ("Recurrent late decelerations with absent variability", 1, "Category III; prepare for birth."),
    ("Baseline 140/min", 0, "Normal.")],
   "Recovery allows the induction to restart carefully; absent variability with lates means emergency birth.",
   "Evaluate intrauterine resuscitation.",
   ex=[note("30 minutes", "Provider at the bedside.", id="oxy-30")]),
]

# ================================================================================================
# Case 2: Umbilical cord prolapse
# ================================================================================================
cord_tabs = [
    ("cord-notes", "Labor notes", [note("Labor and delivery, 38 weeks",
        "A 27-year-old client (G3 P2) with polyhydramnios and a high, unengaged fetal head has a sudden gush of "
        "fluid when the membranes rupture. The FHR drops at once.", id="cord-note")]),
    ("cord-data", "Findings", [table("Findings", ["Result"], [
        ("FHR", ["Dropped from 140 to 70/min, sustained"]),
        ("Vaginal exam", ["A pulsating cord felt in the vagina"]),
        ("Cervix", ["6 cm"]),
        ("Station", ["-3"]),
        ("Fluid", ["Clear, large volume"])], id="cord-table")]),
]

cord_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that show a cord prolapse emergency.",
   ["Finding", "Result"],
   [(["FHR", "Sustained 70/min"], True, "Cord compression."),
    (["Vaginal exam", "Pulsating cord"], True, "Overt prolapse."),
    (["Station", "-3, unengaged"], True, "Risk factor."),
    (["Polyhydramnios", "Large gush"], True, "Risk factor."),
    (["Fluid color", "Clear"], False, "Not a sign.")],
   "Rupture with a high head and a cord in the vagina is prolapse.",
   "Recognize cues of cord prolapse."),

matrix_mc(M(PA, COMPLICATIONS, "analyze_cues", "difficult", OB, "ob"),
   "For each factor, indicate whether it raises the risk of cord prolapse.",
   ["Raises the risk", "Does not raise the risk"],
   [("Breech or transverse lie", 0, "Poor fit."),
    ("Rupture of membranes with an unengaged head", 0, "Space for the cord."),
    ("Polyhydramnios", 0, "Gush carries the cord."),
    ("Preterm fetus", 0, "Small presenting part."),
    ("Vertex at +2 station", 1, "Engaged head fills the pelvis.")],
   "An unengaged or small presenting part allows the cord to slip.",
   "Analyze cord prolapse risk."),

cloze(M(PA, COMPLICATIONS, "prioritize_hypotheses", "difficult", OB, "ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first action is to {{b1}}, while {{b2}}.",
   [[("lift the presenting part off the cord with a gloved hand", True, "Relieves compression."),
     ("push the cord back into the uterus", False, "Causes spasm and is not done."),
     ("leave the room to call for help", False, "Stay; call from the bedside."),
     ("start oxytocin", False, "Harmful.")],
    [("calling for help and preparing for emergency cesarean", True, "Birth is the cure."),
     ("waiting for full dilation", False, "Too slow."),
     ("documenting the event first", False, "Later."),
     ("removing the hand to check the FHR", False, "Keep pressure off.")]],
   "Relieve compression and prepare for immediate birth.",
   "Prioritize cord prolapse."),

sata(M(PA, COMPLICATIONS, "generate_solutions", "difficult", OB, "ob"),
   "Which other actions should the nurse anticipate?",
   [("Knee-chest or steep Trendelenburg position", True, "Gravity lifts the fetus."),
    ("Cover exposed cord with warm moist gauze", True, "Prevents spasm."),
    ("Fill the bladder with saline if ordered", True, "Lifts the presenting part."),
    ("Keep the hand in place until birth", True, "Ongoing relief."),
    ("Massage the cord gently to restore its pulsation", False, "Causes spasm."),
    ("Ask the client to push hard with the next contraction", False, "Not fully dilated.")],
   "Positioning, cord protection and continued elevation.",
   "Generate cord prolapse actions.",
   ex=[note("Now", "Team called. Operating room preparing.", id="cord-now")]),

sata(M(MOC, DELEG, "take_action", "difficult", OB, "mgmt ob"),
   "The RN keeps the presenting part elevated. Which tasks can be given to other staff?",
   [("Another RN: monitor the fetal heart rate and maternal vital signs", True, "Within RN scope."),
    ("UAP: bring the warm saline and the emergency cart", True, "Non-clinical."),
    ("UAP: call the operating room team using the emergency number", True, "A communication task."),
    ("UAP: remove the RN's hand so the RN can document", False, "Unsafe."),
    ("UAP: decide when to move the client to the operating room", False, "A clinical decision.")],
   "The RN stays; others help.",
   "Delegate in an obstetric emergency.",
   ex=[note("Transfer", "Moving to the operating room on the bed.", id="cord-transfer")]),

matrix_mc(M(PA, COMPLICATIONS, "evaluate_outcomes", "difficult", OB, "ob peds"),
   "After an emergency cesarean, for each finding, indicate whether it is reassuring or needs follow-up.",
   ["Reassuring", "Needs follow-up"],
   [("Apgar 4 at 1 minute, 8 at 5 minutes", 0, "Recovered."),
    ("Cord arterial pH 7.21", 0, "Mild acidosis."),
    ("Newborn breathing room air at 1 hour", 0, "Stable."),
    ("Client says, \"Did I do something wrong?\"", 1, "Needs debriefing and reassurance."),
    ("Newborn tremors and poor feeding at 6 hours", 1, "Possible encephalopathy or hypoglycemia.")],
   "Good recovery; emotional needs and newborn signs need follow-up.",
   "Evaluate cord prolapse outcome.",
   ex=[note("Birth", "Born 12 minutes after diagnosis.", id="cord-birth")]),
]

# ================================================================================================
# Case 3: Preterm labor
# ================================================================================================
ptl_tabs = [
    ("ptl-notes", "Triage notes", [note("Triage, 29 weeks",
        "A 22-year-old client (G1 P0) at 29 weeks has low back ache and tightening every 6 minutes for 3 hours. "
        "The client had a urinary tract infection last week that was not treated.", id="ptl-note")]),
    ("ptl-data", "Findings", [table("Findings", ["Result"], [
        ("Contractions", ["Every 5 to 6 minutes, 40 seconds"]),
        ("Cervix", ["3 cm, 80% effaced"]),
        ("Fetal fibronectin", ["Positive"]),
        ("Cervical length", ["18 mm"]),
        ("Urinalysis", ["Nitrites positive"]),
        ("Membranes", ["Intact"])], id="ptl-table")]),
]

ptl_qs = [
hl_table(M(PA, COMPLICATIONS, "recognize_cues", "moderate", OB, "ob"),
   "Highlight the findings that confirm preterm labor.",
   ["Finding", "Result"],
   [(["Contractions", "Regular"], True, "Labor."),
    (["Cervix", "3 cm, 80% effaced"], True, "Cervical change."),
    (["Fetal fibronectin", "Positive"], True, "Predicts birth."),
    (["Cervical length", "18 mm"], True, "Short."),
    (["UTI", "Untreated"], True, "A trigger.")],
   "Regular contractions with cervical change before 37 weeks is preterm labor.",
   "Recognize cues of preterm labor."),

matrix_mc(M(PPT, OB_MEDS, "analyze_cues", "difficult", OB, "ph ob"),
   "For each medicine, indicate its purpose in preterm labor.",
   ["Delays birth (tocolytic)", "Matures fetal lungs", "Protects the fetal brain", "Treats the trigger"],
   [("Nifedipine", 0, "Calcium channel blocker."),
    ("Betamethasone", 1, "Corticosteroid."),
    ("Magnesium sulfate before 32 weeks", 2, "Reduces cerebral palsy."),
    ("Cephalexin", 3, "Treats the UTI."),
    ("Indomethacin before 32 weeks", 0, "NSAID tocolytic.")],
   "Each drug has a different role.",
   "Analyze preterm labor medicines."),

cloze(M(PPT, OB_MEDS, "prioritize_hypotheses", "difficult", OB, "ph ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The main purpose of tocolysis is to {{b1}} so that {{b2}}.",
   [[("delay birth for about 48 hours", True, "Short-term goal."),
     ("prevent preterm birth until 40 weeks", False, "Not realistic."),
     ("treat the infection", False, "Not its role."),
     ("dilate the cervix", False, "Opposite.")],
    [("betamethasone can work and the client can be moved to a center with a NICU", True, "The real goal."),
     ("the client can go home", False, "Not the goal."),
     ("labor will never return", False, "Not true."),
     ("the baby gains weight", False, "Too short.")]],
   "Tocolysis buys 48 hours for steroids and transfer.",
   "Prioritize preterm labor treatment."),

calc(M(PPT, CALC, "generate_solutions", "difficult", OB, "ph"),
   "Magnesium sulfate 4 g IV loading dose over 20 minutes is ordered. The bag contains 4 g in 100 mL. At what rate "
   "should the nurse set the pump? Record your answer in mL/hr.",
   "mL/hr", 0, "Rate", 300,
   "100 mL over 20 minutes = 100 / (20/60) = 300 mL/hr.",
   "Calculate a magnesium loading rate.",
   ex=[note("Orders", "Magnesium 4 g IV over 20 minutes, then 1 g/hr. Betamethasone 12 mg IM. Nifedipine.", id="ptl-orders")]),

sata(M(PPT, OB_MEDS, "take_action", "difficult", OB, "ph ob"),
   "Two hours into magnesium, review the findings. Which actions should the nurse take?",
   [("Stop the magnesium infusion", True, "Toxicity."),
    ("Notify the provider", True, "Change the plan."),
    ("Prepare calcium gluconate", True, "Antidote."),
    ("Check a magnesium level", True, "Confirms."),
    ("Increase the infusion to 2 g/hr", False, "Worsens toxicity."),
    ("Encourage oral fluids to flush it out", False, "Not the treatment.")],
   "Absent reflexes, slow breathing and low output mean magnesium toxicity.",
   "Take action for magnesium toxicity.",
   ex=[flow("2 hours", ["Start", "2 hours"], [
       ("Deep tendon reflexes", ["2+", "Absent"]),
       ("Respiratory rate", ["18", "10/min"]),
       ("Urine output", ["50", "18 mL/hr"]),
       ("Client", ["Warm, flushed", "Drowsy, slurred speech"])], id="ptl-2h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", OB, "ob"),
   "After labor stops and the client goes home at 31 weeks, for each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll come in for contractions more than 4 to 6 an hour.\"", 0, "Correct."),
    ("\"Fluid leaking or low back pain means I should call.\"", 0, "Correct."),
    ("\"I'll finish all of my antibiotic.\"", 0, "Treats the trigger."),
    ("\"Strict bed rest will keep the baby in.\"", 1, "Not recommended; raises clot risk."),
    ("\"I'll drink plenty of water.\"", 0, "Dehydration can trigger contractions.")],
   "Warning signs and treating triggers.",
   "Evaluate preterm labor teaching.",
   ex=[note("Day 3", "Contractions stopped. Betamethasone course complete.", id="ptl-day3")]),
]

# ================================================================================================
# Case 4: Epidural and the stages of labor
# ================================================================================================
epi_tabs = [
    ("epi-notes", "Labor notes", [note("Labor and delivery",
        "A 31-year-old client (G2 P1) in active labor at 6 cm requests an epidural. The nurse gave a 500 mL "
        "lactated Ringer's bolus before placement. Fifteen minutes after the epidural is started, the client says, "
        "\"I feel dizzy and sick.\"", id="epi-note")]),
    ("epi-data", "Findings", [flow("Findings", ["Before epidural", "15 minutes after"], [
        ("Blood pressure", ["124/78", "84/48 mm Hg"]),
        ("Heart rate", ["88", "112/min"]),
        ("FHR", ["140/min, moderate variability", "100/min, prolonged deceleration"]),
        ("Position", ["Sitting", "Flat on the back"]),
        ("Pain", ["8/10", "1/10"])], id="epi-flow")]),
]

epi_qs = [
hl_table(M(PPT, OB_MEDS, "recognize_cues", "moderate", OB, "ob ph"),
   "Highlight the findings after the epidural that require immediate action.",
   ["Finding", "15 minutes after"],
   [(["Blood pressure", "84/48"], True, "Sympathetic block hypotension."),
    (["FHR", "Prolonged deceleration to 100"], True, "Reduced placental flow."),
    (["Position", "Flat supine"], True, "Aortocaval compression."),
    (["Client", "Dizzy, nauseated"], True, "Hypotension."),
    (["Pain", "1/10"], False, "Desired effect.")],
   "Epidural hypotension with supine position reduces placental flow.",
   "Recognize cues of epidural hypotension."),

matrix_mc(M(HPM, LABOR, "analyze_cues", "difficult", OB, "ob"),
   "For each description, indicate the stage of labor.",
   ["First stage", "Second stage", "Third stage", "Fourth stage"],
   [("From full dilation to birth of the baby", 1, "Second."),
    ("From onset of labor to full dilation", 0, "First."),
    ("From birth of the baby to delivery of the placenta", 2, "Third."),
    ("First 1 to 4 hours after the placenta", 3, "Recovery."),
    ("Latent, active and transition phases", 0, "Phases of the first stage.")],
   "Labor has four stages.",
   "Analyze the stages of labor."),

cloze(M(PPT, OB_MEDS, "prioritize_hypotheses", "difficult", OB, "ob ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, then {{b2}}.",
   [[("turn the client to the left side", True, "Relieves aortocaval compression."),
     ("raise the head of the bed to 90 degrees", False, "Worsens hypotension."),
     ("stop the epidural catheter and remove it", False, "Not first."),
     ("give oxytocin", False, "Harmful.")],
    [("give an IV fluid bolus and phenylephrine or ephedrine as prescribed", True, "Restores pressure."),
     ("wait 30 minutes", False, "Fetal risk."),
     ("give a diuretic", False, "Worsens it."),
     ("give naloxone", False, "Not an opioid problem.")]],
   "Reposition, fluids and a vasopressor.",
   "Prioritize epidural hypotension."),

sata(M(HPM, LABOR, "generate_solutions", "difficult", OB, "ob"),
   "Which parts of the plan should the nurse include for a client with an epidural?",
   [("Blood pressure every 5 minutes after a dose and then per policy", True, "Hypotension risk."),
    ("Check the bladder; catheterize if distended", True, "Retention."),
    ("Turn side to side every hour", True, "Pressure and labor progress."),
    ("Fall precautions; no walking alone", True, "Leg weakness."),
    ("Let the client walk to the bathroom alone", False, "Fall risk."),
    ("Keep the client flat on the back", False, "Aortocaval compression.")],
   "Monitoring, bladder care, position changes and fall safety.",
   "Generate the epidural care plan.",
   ex=[note("10 minutes later", "BP 116/70 after left side, bolus and phenylephrine. FHR 135/min.", id="epi-recover")]),

sata(M(SIC, SAFE, "take_action", "difficult", OB, "ob"),
   "The client is fully dilated and the head is at +1. Which actions should the nurse take during the second "
   "stage?",
   [("Coach pushing with contractions when the urge comes (laboring down)", True, "Reduces exhaustion."),
    ("Check the FHR after each contraction", True, "Second-stage monitoring."),
    ("Watch for bulging of the perineum and crowning", True, "Birth approaching."),
    ("Tell the client to push continuously between contractions", False, "Exhausts and harms the fetus."),
    ("Push hard on the top of the uterus to speed birth", False, "Fundal pressure is unsafe.")],
   "Supported pushing, monitoring and watching for crowning.",
   "Take action in the second stage.",
   ex=[note("3 hours later", "10 cm, +1 station, client feels pressure.", id="epi-2nd")]),

matrix_mc(M(HPM, LABOR, "evaluate_outcomes", "difficult", OB, "ob"),
   "In the fourth stage, for each finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Fundus firm at the umbilicus, midline", 0, "Normal."),
    ("Lochia rubra, one pad in an hour", 0, "Normal."),
    ("Fundus boggy and 2 cm above the umbilicus, deviated right", 1, "Full bladder or atony."),
    ("Shaking chills with a normal temperature", 0, "Common after birth."),
    ("Legs still numb at 4 hours", 1, "Epidural block should be wearing off.")],
   "Firm fundus and normal lochia are expected; a boggy, deviated fundus needs action.",
   "Evaluate the fourth stage.",
   ex=[note("Birth", "Vaginal birth after 1 hour of pushing. Placenta delivered intact.", id="epi-birth")]),
]

cases = [
    case("Oxytocin induction with tachysystole and late decelerations", oxy_tabs, oxy_qs),
    case("Umbilical cord prolapse", cord_tabs, cord_qs),
    case("Preterm labor with tocolysis and magnesium", ptl_tabs, ptl_qs),
    case("Labor epidural with hypotension and the stages of labor", epi_tabs, epi_qs),
]

BUILT = build(136, "ob-cases-fhr-prolapse-preterm-epidural",
              "Maternity case studies: fetal monitoring on oxytocin, cord prolapse, preterm labor and the labor "
              "epidural",
              items, cases, case_conversion=True)
