"""Batch 127 (case conversion, chunk 7: immunity, infection, STI, musculoskeletal and trauma): four full NGN case
studies and four standalone items (CASE-MAP.md, book_topics.py).

Cases: blunt abdominal trauma with a splenic laceration, hemorrhagic shock and massive transfusion; exertional heat
stroke; accidental hypothermia with rewarming; a mass-casualty incident with triage (management of care). Standalone:
a pelvic fracture hemorrhage bowtie, a neurovascular check trend after casting, a priority question on an
orthopedic trauma unit and a delegation question on an infectious disease unit. Each case walks the six CJMM steps
in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
TRAUMA = "trauma-and-hemorrhage"
ENV = "environmental-emergencies"
add_subtopics({
    TRAUMA: ("PHI.PA", "Trauma and hemorrhage"),
    ENV: ("PHI.PA", "Environmental emergencies (heat, cold, drowning)"),
})
FUND, MSK, IMMUNE = "fundamentals", "musculoskeletal", "immune"
TRANSFUSION = "blood-transfusion-reactions"
SHOCK = "shock-and-sepsis"
LYTES = "electrolyte-imbalances"
FRACTURE = "fracture-complications"
DISASTER = "emergency-and-disaster-response"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
TBP = "transmission-based-precautions"
TEACH = "health-teaching-and-behavior-change"
LABS = "laboratory-values-and-monitoring"
CALC = "medication-dose-calculations"
IMMZ = "immunizations"
DYSRHYTHMIA = "cardiac-dysrhythmias"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, TRAUMA, "prioritize_hypotheses", "difficult", MSK, "ms"),
   "A pedestrian struck by a car is in the trauma bay. Using the trauma note, fill in the bowtie: the likely cause "
   "of the shock in the middle, two priority interventions on the left and two parameters that show the response "
   "on the right.",
   [("Hemorrhage from an unstable pelvic fracture", True, "Hypotension with an unstable pelvis."),
    ("Neurogenic shock", False, "Heart rate is fast, not slow."),
    ("Fat embolism syndrome", False, "Too early, and no hypoxemia or petechiae."),
    ("Cardiac tamponade", False, "No distended neck veins or muffled heart sounds.")],
   [("Apply a pelvic binder", True, "Reduces pelvic volume and bleeding."),
    ("Start blood products", True, "Replaces blood loss."),
    ("Rock the pelvis again to confirm that it is unstable", False, "Repeated rocking dislodges clots."),
    ("Insert a urinary catheter at once to measure output", False, "Blood at the meatus suggests a urethral injury."),
    ("Give a large volume of crystalloid first", False, "Dilutes clotting factors.")],
   [("Blood pressure, pulse", True, "Response to resuscitation."),
    ("Hemoglobin, lactate", True, "Bleeding and perfusion."),
    ("Bowel sounds every hour", False, "Not the priority."),
    ("Body weight each morning", False, "Not useful now."),
    ("Pupil size and reactivity", False, "No head injury described.")],
   "An unstable pelvis with shock needs a binder and blood products.",
   "Recognize and act on pelvic fracture hemorrhage.",
   ex=[note("Trauma bay", "Pedestrian struck by a car. BP 78/44 mm Hg, HR 136/min. The pelvis moved on gentle "
       "compression. Blood at the urinary meatus. Alert, pupils equal.", id="sa-pelvis")]),

dtable(M(RRP, FRACTURE, "evaluate_outcomes", "difficult", MSK, "ms"),
   "A client has a new long-leg cast after a tibia fracture. For each trend, choose the interpretation.",
   ["Measure", "1400 to 2000", "Interpretation"],
   [(["Pain", "4, 5, 8, 10 despite morphine"], [
       ("Rising pain out of proportion; report now", True, "Early compartment syndrome."),
       ("Expected after a fracture", False, "Pain should ease with analgesia."),
       ("Needs a stronger opioid only", False, "Masks the problem.")]),
    (["Toes", "Pink, pink, pale, pale and cool"], [
       ("Worsening perfusion", True, "A late sign."),
       ("Normal variation", False, "A real change."),
       ("Improving", False, "Worse.")]),
    (["Sensation", "Normal, normal, tingling, numb between the first and second toes"], [
       ("Nerve compression", True, "Deep peroneal nerve."),
       ("From the opioid", False, "Opioids do not cause focal numbness."),
       ("Normal after casting", False, "Not normal.")])],
   "Rising pain, pallor and numbness under a cast mean compartment syndrome; the cast may need bivalving.",
   "Evaluate a neurovascular check trend.",
   ex=[flow("Neurovascular checks", ["1400", "1600", "1800", "2000"], [
       ("Pain", ["4", "5", "8", "10"]),
       ("Toes", ["Pink", "Pink", "Pale", "Pale, cool"]),
       ("Sensation", ["Normal", "Normal", "Tingling", "Numb first web space"])], id="sa-nv")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", MSK, "mgmt ms"),
   "The nurse receives handoff on four orthopedic trauma clients at 0700. After handoff, which client should the "
   "nurse assess first?",
   [("A client 1 day after a total knee replacement who rates pain 6 out of 10 and wants the next dose of oxycodone", False,
     "Needs pain relief soon, but is stable."),
    ("A client 2 days after a femur fracture who is newly confused with an SpO2 of 88%", True,
     "Possible fat embolism; hypoxemia is the most urgent."),
    ("A client in a cast who reports itching under the cast and asks for a coat hanger", False,
     "Teaching need; not urgent."),
    ("A client after hip surgery whose drain has 150 mL of blood since the last shift", False,
     "Within expected limits; check after the hypoxic client.")],
   "New confusion with hypoxemia after a long-bone fracture is the priority.",
   "Prioritize clients on an orthopedic trauma unit."),

sata(M(MOC, DELEG, "take_action", "difficult", IMMUNE, "mgmt"),
   "On an infectious disease unit, which tasks can the RN delegate to the UAP?",
   [("Take vital signs on a client in contact precautions for MRSA", True, "Routine; uses the correct precautions."),
    ("Restock gowns and gloves outside isolation rooms", True, "Non-clinical."),
    ("Bathe a client with C. difficile using soap and water for hand hygiene afterward", True, "Routine with correct precautions."),
    ("Assess a client with HIV for new oral lesions", False, "Assessment stays with the RN."),
    ("Teach a client with TB how to take the medicines at home", False, "Teaching stays with the RN."),
    ("Decide when a client's airborne precautions can stop", False, "A clinical decision.")],
   "UAPs do routine care with correct precautions; assessment, teaching and decisions stay with the RN.",
   "Delegate on an infectious disease unit."),
]

# ================================================================================================
# Case 1: Splenic laceration with hemorrhagic shock
# ================================================================================================
spl_tabs = [
    ("spl-notes", "Trauma notes", [note("Emergency department",
        "A 28-year-old client was the driver in a car crash. The client was wearing a seat belt and complains of "
        "left upper abdominal pain spreading to the left shoulder.", id="spl-note")]),
    ("spl-vitals", "Vital signs", [flow("Vital signs", ["Arrival", "20 minutes"], [
        ("Blood pressure", ["112/70", "84/50 mm Hg"]),
        ("Heart rate", ["108", "138/min"]),
        ("Respiratory rate", ["22", "28/min"]),
        ("Skin", ["Pale", "Cold, clammy"]),
        ("Mental status", ["Alert", "Anxious, slow to answer"]),
        ("FAST ultrasound", ["-", "Free fluid around the spleen"]),
        ("Abdomen", ["Seat belt bruise", "Distended, rigid"])], id="spl-flow")]),
]

spl_qs = [
hl_table(M(PA, TRAUMA, "recognize_cues", "moderate", FUND, "ms"),
   "Highlight the findings at 20 minutes that require immediate action.",
   ["Finding", "20 minutes"],
   [(["Blood pressure", "84/50 mm Hg"], True, "Hypotension; class III or IV hemorrhage."),
    (["Heart rate", "138/min"], True, "Compensating."),
    (["FAST", "Free fluid around the spleen"], True, "Intra-abdominal bleeding."),
    (["Shoulder pain", "Left shoulder (Kehr sign)"], True, "Blood irritating the diaphragm."),
    (["Mental status", "Anxious, slow"], True, "Reduced brain perfusion.")],
   "Shock with free fluid after blunt trauma means internal bleeding.",
   "Recognize cues of hemorrhagic shock."),

matrix_mc(M(PA, SHOCK, "analyze_cues", "difficult", FUND, "ms"),
   "For each finding, indicate the class of hemorrhage it best fits in an adult.",
   ["Class I-II (less than 30% loss)", "Class III-IV (30% or more loss)"],
   [("Heart rate 138/min with falling blood pressure", 1, "Decompensation."),
    ("Normal blood pressure with heart rate 105 and anxiety", 0, "Compensated."),
    ("Confusion and very low urine output", 1, "Severe."),
    ("Narrowed pulse pressure with normal systolic pressure", 0, "Early sign.")],
   "Hypotension marks loss of 30% or more.",
   "Analyze hemorrhage severity."),

cloze(M(PA, TRAUMA, "prioritize_hypotheses", "difficult", FUND, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}} with {{b2}}, while preparing for {{b3}}.",
   [[("damage-control resuscitation", True, "Limits dilution and clotting failure."),
     ("aggressive crystalloid boluses to a normal blood pressure", False, "Dilutes clotting and worsens bleeding."),
     ("a CT scan before any treatment", False, "Unstable clients do not go to CT."),
     ("observation", False, "Unsafe.")],
    [("balanced blood products (plasma, platelets and red cells) and tranexamic acid", True, "Massive transfusion protocol."),
     ("albumin only", False, "Not enough."),
     ("vitamin K", False, "Not the issue."),
     ("dextrose 50%", False, "Not the issue.")],
    [("emergency laparotomy or splenic embolization", True, "Source control."),
     ("discharge", False, "Unsafe."),
     ("an outpatient ultrasound", False, "Too slow."),
     ("physiotherapy", False, "Not relevant.")]],
   "Balanced products, TXA and source control.",
   "Prioritize hemorrhagic shock care."),

sata(M(PPT, TRANSFUSION, "generate_solutions", "difficult", FUND, "ph"),
   "The massive transfusion protocol is activated. Which actions should the nurse anticipate?",
   [("Use a blood warmer", True, "Prevents hypothermia."),
    ("Check ionized calcium", True, "Citrate binds calcium."),
    ("Give tranexamic acid within 3 hours of injury", True, "Reduces death from bleeding."),
    ("Monitor potassium", True, "Stored blood raises potassium."),
    ("Give products cold straight from the fridge to save time", False, "Causes hypothermia and coagulopathy."),
    ("Wait for the full crossmatch before any blood", False, "Uncrossmatched O blood is used in emergencies.")],
   "Warm products, replace calcium, give TXA and monitor potassium.",
   "Generate the massive transfusion plan.",
   ex=[note("MTP", "Uncrossmatched O negative red cells, plasma and platelets 1:1:1. TXA 1 g IV.", id="spl-mtp")]),

sata(M(PA, TRAUMA, "take_action", "difficult", FUND, "ms"),
   "After 8 units, review the findings. Which actions should the nurse take?",
   [("Warm the client with a forced-air blanket", True, "Hypothermia worsens bleeding."),
    ("Give calcium as prescribed", True, "Low ionized calcium."),
    ("Report the results to the trauma team", True, "Lethal triad."),
    ("Stop all blood products because of the acidosis", False, "Ongoing bleeding needs products."),
    ("Give sodium bicarbonate to correct the pH as the main fix", False, "Treat the cause: perfusion and bleeding.")],
   "Hypothermia, acidosis and coagulopathy form the lethal triad.",
   "Take action on the lethal triad.",
   ex=[flow("After 8 units", ["Value"], [
       ("Temperature", ["34.9 C"]),
       ("pH", ["7.18"]),
       ("INR", ["1.9"]),
       ("Ionized calcium", ["0.88 mmol/L"]),
       ("Operating room", ["Ready in 10 minutes"])], id="spl-8u")]),

matrix_mc(M(HPM, IMMZ, "evaluate_outcomes", "difficult", FUND, "ms acip"),
   "After a splenectomy, for each statement or finding at discharge, indicate whether it shows understanding or "
   "progress, or needs follow-up.",
   ["Understanding or progress", "Needs follow-up"],
   [("Hemoglobin stable at 9.8 g/dL for 48 hours", 0, "No further bleeding."),
    ("\"I'll get the pneumococcal, meningococcal and Hib vaccines.\"", 0, "Asplenia vaccines."),
    ("\"A fever is no big deal now, I can wait a few days.\"", 1, "Overwhelming infection risk; seek care at once."),
    ("\"I'll wear a medical alert bracelet.\"", 0, "Correct."),
    ("Platelets 820,000/mm3 on day 5", 1, "Reactive thrombocytosis; clot risk is reviewed.")],
   "After splenectomy, vaccines and urgent care for fever are essential.",
   "Evaluate post-splenectomy teaching.",
   ex=[note("Surgery", "Splenectomy. 10 units given in total.", id="spl-op")]),
]

# ================================================================================================
# Case 2: Exertional heat stroke
# ================================================================================================
heat_tabs = [
    ("heat-notes", "Event notes", [note("Medical tent at a summer half-marathon",
        "A 24-year-old runner collapsed at km 19 on a hot, humid day. The runner is confused and combative and "
        "takes a stimulant medicine for ADHD.", id="heat-note")]),
    ("heat-data", "Findings", [table("Findings", ["Result"], [
        ("Rectal temperature", ["41.6 C (106.9 F)"]),
        ("Mental status", ["Confused, combative"]),
        ("Skin", ["Hot, flushed, still sweating"]),
        ("Heart rate", ["148/min"]),
        ("Blood pressure", ["92/54 mm Hg"]),
        ("Glucose", ["96 mg/dL"]),
        ("Sodium", ["138 mEq/L"])], id="heat-table")]),
]

heat_qs = [
hl_table(M(PA, ENV, "recognize_cues", "moderate", FUND, "ms"),
   "Highlight the findings that define heat stroke.",
   ["Finding", "Result"],
   [(["Temperature", "41.6 C"], True, "Above 40 C."),
    (["Mental status", "Confused, combative"], True, "Central nervous system dysfunction."),
    (["Skin", "Still sweating"], False, "Sweating is common in exertional heat stroke and does not rule it out."),
    (["Glucose", "96 mg/dL"], False, "Normal; hypoglycemia is excluded."),
    (["Sodium", "138 mEq/L"], False, "Normal; hyponatremia is excluded.")],
   "A core temperature above 40 C with altered mental status is heat stroke.",
   "Recognize cues of heat stroke."),

matrix_mc(M(PA, ENV, "analyze_cues", "difficult", FUND, "ms"),
   "For each finding, indicate whether it fits heat exhaustion or heat stroke.",
   ["Heat exhaustion", "Heat stroke"],
   [("Temperature 39 C with normal mental status", 0, "Exhaustion."),
    ("Confusion or seizures", 1, "CNS dysfunction."),
    ("Heavy sweating, nausea and headache with a clear mind", 0, "Exhaustion."),
    ("Temperature above 40 C with combative behavior", 1, "Heat stroke.")],
   "Mental status separates heat stroke from heat exhaustion.",
   "Analyze heat illness."),

cloze(M(PA, ENV, "prioritize_hypotheses", "difficult", FUND, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, ideally by {{b2}}.",
   [[("cooling to below 39 C within 30 minutes", True, "Time at high temperature determines organ damage."),
     ("transport first and cool at the hospital", False, "Cool first, transport second."),
     ("antipyretics", False, "Do not work in heat stroke."),
     ("oral fluids only", False, "Not enough and unsafe if confused.")],
    [("cold-water immersion", True, "Fastest cooling."),
     ("a fan alone", False, "Too slow."),
     ("a warm shower", False, "Wrong."),
     ("acetaminophen", False, "Ineffective.")]],
   "Cool first with cold-water immersion.",
   "Prioritize heat stroke treatment."),

sata(M(PA, ENV, "generate_solutions", "difficult", FUND, "ms"),
   "Which actions should the nurse include during cooling?",
   [("Monitor rectal temperature continuously", True, "Accurate core temperature."),
    ("Remove from the water at about 38.6 to 39 C", True, "Prevents overshoot hypothermia."),
    ("Protect the airway and watch for seizures", True, "CNS involvement."),
    ("Give acetaminophen to bring the temperature down", False, "Ineffective and harms the liver."),
    ("Cool until the temperature reaches 35 C", False, "Overshoot hypothermia.")],
   "Continuous core temperature, stop at 39 C and protect the airway.",
   "Generate the cooling plan.",
   ex=[note("Cooling", "Placed in an ice-water tub. Rectal probe in place.", id="heat-cool")]),

sata(M(PA, ENV, "take_action", "difficult", FUND, "ms lab"),
   "At the hospital 6 hours later, review the results. Which actions should the nurse take?",
   [("Report the CK and creatinine", True, "Rhabdomyolysis and kidney injury."),
    ("Expect IV fluids to keep urine output up", True, "Protects the kidneys."),
    ("Monitor for bleeding", True, "DIC risk."),
    ("Check liver enzymes again", True, "Heat injures the liver."),
    ("Discharge home now that the temperature is normal", False, "Organ injury evolves over days."),
    ("Restart the stimulant at the usual dose tonight", False, "Stimulants raise heat production.")],
   "Heat stroke injures muscle, kidneys, liver and clotting.",
   "Take action on heat stroke complications.",
   ex=[table("6 hours", ["Result"], [
       ("Temperature", ["37.8 C"]),
       ("CK", ["18,400 units/L"]),
       ("Creatinine", ["2.0 mg/dL"]),
       ("AST/ALT", ["640 / 580 units/L"]),
       ("Platelets", ["92,000/mm3"])], id="heat-6h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", FUND, "ms"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll return to running gradually after my doctor clears me.\"", 0, "Graded return."),
    ("\"I'll acclimatize to the heat over 1 to 2 weeks before races.\"", 0, "Prevention."),
    ("\"I'll talk to my prescriber about the stimulant before races.\"", 0, "Raises risk."),
    ("\"If I feel confused while running, I'll push through to finish.\"", 1, "Stop and get help."),
    ("\"I'll run at midday because it's when I have time.\"", 1, "Avoid the hottest hours.")],
   "Acclimatize, return gradually, review medicines and avoid the hottest hours.",
   "Evaluate heat stroke teaching.",
   ex=[note("Day 4", "CK falling; creatinine 1.1 mg/dL.", id="heat-day4")]),
]

# ================================================================================================
# Case 3: Accidental hypothermia
# ================================================================================================
cold_tabs = [
    ("cold-notes", "Emergency notes", [note("Emergency department, winter",
        "A 72-year-old client who lives alone was found at home in an unheated room. The client is drowsy and "
        "mumbling. Clothing was damp.", id="cold-note")]),
    ("cold-data", "Findings", [table("Findings", ["Result"], [
        ("Core temperature (esophageal)", ["29.4 C (84.9 F)"]),
        ("Heart rate", ["38/min, atrial fibrillation with Osborn (J) waves"]),
        ("Blood pressure", ["86/50 mm Hg"]),
        ("Respiratory rate", ["8/min"]),
        ("Shivering", ["Absent"]),
        ("Glucose", ["64 mg/dL"]),
        ("Potassium", ["4.8 mEq/L"])], id="cold-table")]),
]

cold_qs = [
hl_table(M(PA, ENV, "recognize_cues", "moderate", FUND, "ms"),
   "Highlight the findings that show moderate to severe hypothermia.",
   ["Finding", "Result"],
   [(["Core temperature", "29.4 C"], True, "Moderate (28 to 32 C), near severe."),
    (["Shivering", "Absent"], True, "Shivering stops below about 32 C."),
    (["ECG", "Bradycardia, AF, J waves"], True, "Cold heart."),
    (["Respiratory rate", "8/min"], True, "Depressed breathing."),
    (["Potassium", "4.8 mEq/L"], False, "Normal.")],
   "No shivering, bradycardia and J waves at 29 C mean moderate hypothermia.",
   "Recognize cues of hypothermia."),

matrix_mc(M(PA, ENV, "analyze_cues", "difficult", FUND, "ms"),
   "For each statement, indicate whether it is accurate or inaccurate about the hypothermic heart.",
   ["Accurate", "Inaccurate"],
   [("Rough handling can trigger ventricular fibrillation", 0, "The cold heart is irritable."),
    ("Defibrillation and drugs may not work below 30 C", 0, "Rewarm while doing CPR."),
    ("A client is not dead until warm and dead", 0, "Prolonged resuscitation is justified."),
    ("Bradycardia should be treated first with atropine", 1, "Usually ineffective; rewarming treats it."),
    ("Placing the client upright quickly helps the circulation", 1, "Can cause collapse; keep horizontal.")],
   "Handle gently, keep horizontal and rewarm.",
   "Analyze hypothermia physiology."),

cloze(M(PA, ENV, "prioritize_hypotheses", "difficult", FUND, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "This client needs {{b1}}, and the nurse should expect {{b2}} as rewarming proceeds.",
   [[("active internal rewarming such as warmed IV fluids and warmed humidified oxygen", True, "Moderate to severe hypothermia."),
     ("only a blanket", False, "Passive rewarming is for mild cases."),
     ("a hot bath", False, "Causes vasodilation and collapse."),
     ("rubbing the limbs", False, "Harms tissue and can cause VF.")],
    [("hypotension as vessels dilate (rewarming shock)", True, "Volume needs rise."),
     ("rising heart rate requiring beta blockers", False, "Not expected."),
     ("permanent bradycardia", False, "Rate improves."),
     ("hypertension", False, "Not typical.")]],
   "Active internal rewarming with fluids ready for rewarming shock.",
   "Prioritize rewarming."),

sata(M(PA, ENV, "generate_solutions", "difficult", FUND, "ms"),
   "Which interventions should the nurse include?",
   [("Remove wet clothing gently", True, "Stops heat loss."),
    ("Warmed IV 0.9% sodium chloride at 40 to 42 C", True, "Internal rewarming."),
    ("Forced-air warming blanket to the trunk", True, "Active external rewarming."),
    ("Treat the low glucose", True, "Hypoglycemia."),
    ("Warm the arms and legs first", False, "Returns cold, acidic blood to the core."),
    ("Move the client briskly onto a stretcher", False, "Can trigger VF.")],
   "Gentle handling, trunk warming and warmed fluids.",
   "Generate the rewarming plan.",
   ex=[note("Orders", "Warm fluids, forced-air blanket, warmed oxygen. Consider extracorporeal rewarming if arrest occurs.", id="cold-orders")]),

sata(M(PA, DYSRHYTHMIA, "take_action", "difficult", FUND, "ms aha"),
   "During transfer, the client becomes pulseless and the monitor shows ventricular fibrillation. The core "
   "temperature is 29.6 C. Which actions should the nurse take?",
   [("Start high-quality CPR", True, "Arrest."),
    ("Defibrillate once as per protocol", True, "One attempt is reasonable."),
    ("Continue active rewarming during CPR", True, "Rewarming is the key treatment."),
    ("Stop resuscitation after 10 minutes because the client is cold", False, "Not dead until warm and dead."),
    ("Give repeated epinephrine every 3 minutes as at normal temperature", False, "Drugs are held or spaced until warmer.")],
   "CPR, one shock and continued rewarming; drugs are limited until warmer.",
   "Take action for cardiac arrest in hypothermia.",
   ex=[note("Transfer", "The client was moved from the stretcher to the bed.", id="cold-arrest")]),

matrix_mc(M(SIC, "equipment-and-home-safety", "evaluate_outcomes", "difficult", FUND, "ms"),
   "After recovery, for each finding or plan, indicate whether it shows progress or a safe plan, or needs follow-up.",
   ["Progress or safe plan", "Needs follow-up"],
   [("Temperature 36.8 C, sinus rhythm", 0, "Recovered."),
    ("Referral to social services for heating assistance", 0, "Addresses the cause."),
    ("Daily phone check from a neighbor arranged", 0, "Safety net."),
    ("The client plans to heat the room with a gas oven", 1, "Carbon monoxide and fire risk."),
    ("Mild cognitive impairment noted on screening", 1, "Affects safety living alone.")],
   "Recovery; home heating safety and cognition need follow-up.",
   "Evaluate discharge safety after hypothermia.",
   ex=[note("Day 3", "ROSC after 18 minutes of CPR and rewarming. Neurologically intact.", id="cold-day3")]),
]

# ================================================================================================
# Case 4: Mass-casualty incident and triage
# ================================================================================================
mci_tabs = [
    ("mci-notes", "Emergency department", [note("Disaster plan activated",
        "A bus has crashed on the highway. The hospital activates its mass-casualty plan. The charge nurse is "
        "triaging arrivals with the START system.", id="mci-note")]),
    ("mci-data", "Arrivals", [table("Arrivals", ["Findings"], [
        ("Client A", ["Walking, laceration on the forearm, bleeding controlled"]),
        ("Client B", ["Not breathing after opening the airway"]),
        ("Client C", ["Respiratory rate 34/min, confused"]),
        ("Client D", ["Respiratory rate 20/min, radial pulse present, obeys commands, open tibia fracture"]),
        ("Client E", ["Capillary refill 4 seconds, no radial pulse, follows commands"])], id="mci-table")]),
]

mci_qs = [
hl_table(M(SIC, DISASTER, "recognize_cues", "moderate", FUND, "mgmt"),
   "Highlight the clients who need immediate (red) care.",
   ["Client", "Findings"],
   [(["Client A", "Walking"], False, "Minor (green)."),
    (["Client B", "Not breathing after airway opening"], False, "Expectant (black) in START."),
    (["Client C", "RR 34, confused"], True, "RR above 30 is immediate."),
    (["Client D", "Stable, obeys, fracture"], False, "Delayed (yellow)."),
    (["Client E", "No radial pulse, refill 4 s"], True, "Poor perfusion is immediate.")],
   "RR above 30, poor perfusion or not following commands is red.",
   "Recognize triage categories."),

matrix_mc(M(SIC, DISASTER, "analyze_cues", "difficult", FUND, "mgmt"),
   "For each client, indicate the START triage category.",
   ["Immediate (red)", "Delayed (yellow)", "Minor (green)", "Expectant (black)"],
   [("Client A", 2, "Walking wounded."),
    ("Client B", 3, "Apneic after repositioning."),
    ("Client C", 0, "RR above 30 and confused."),
    ("Client D", 1, "Stable but cannot walk."),
    ("Client E", 0, "Poor perfusion.")],
   "START uses walking, breathing, perfusion and mental status.",
   "Analyze START triage."),

cloze(M(SIC, DISASTER, "prioritize_hypotheses", "difficult", FUND, "mgmt"),
   "Complete the sentence by choosing from the lists of options.",
   "In a mass-casualty event, the goal of triage changes to {{b1}}, so a client in cardiac arrest is {{b2}}.",
   [[("the greatest good for the greatest number", True, "Utilitarian triage."),
     ("the sickest client first regardless of survival", False, "Daily ED principle."),
     ("first come, first served", False, "Not triage."),
     ("the youngest first", False, "Not triage.")],
    [("expectant, with comfort care if resources are limited", True, "Resources go to salvageable clients."),
     ("always resuscitated first", False, "Uses resources for unlikely survival."),
     ("sent home", False, "Not appropriate."),
     ("immediate", False, "Not in START.")]],
   "Disaster triage aims at the most lives saved.",
   "Prioritize in disaster triage."),

sata(M(MOC, "collaboration-and-referrals", "generate_solutions", "difficult", FUND, "mgmt"),
   "To make beds available, the charge nurse reviews inpatients for early discharge. Which clients are appropriate "
   "to discharge or transfer?",
   [("Day 3 of oral antibiotics for cellulitis, afebrile", True, "Stable; can finish at home."),
    ("Knee arthroscopy this morning, walking and eating", True, "Stable."),
    ("Stable COPD, waiting for home oxygen", True, "Can go when oxygen arrives."),
    ("A client 6 hours after an anterior myocardial infarction with chest pain resolved", False, "Needs monitoring."),
    ("A client with DKA whose insulin infusion is being titrated hourly", False, "Unstable.")],
   "Discharge stable clients to create surge capacity.",
   "Generate surge capacity plans.",
   ex=[note("Hospital command", "Need 12 beds in 1 hour.", id="mci-command")]),

sata(M(SIC, DISASTER, "take_action", "difficult", FUND, "mgmt"),
   "A family member arrives demanding to see a relative and a news reporter asks the nurse about the injured. "
   "Which actions should the nurse take?",
   [("Refer the reporter to the public information officer", True, "Single point of communication."),
    ("Send the family to the family assistance area", True, "Organized information."),
    ("Follow the incident command structure", True, "Coordinated response."),
    ("Give the reporter the names of the injured who have been admitted", False, "Breaches privacy."),
    ("Let the family member into the treatment area to search for the relative", False, "Disrupts care and security.")],
   "Use the incident command roles for media and family.",
   "Take action within incident command.",
   ex=[note("1 hour", "Twenty-two clients received.", id="mci-1h")]),

matrix_mc(M(SIC, DISASTER, "evaluate_outcomes", "difficult", FUND, "mgmt"),
   "One hour later, the charge nurse re-triages. For each finding, indicate whether the category should stay the "
   "same or change.",
   ["Stay the same", "Change category"],
   [("Client A: bleeding controlled, walking", 0, "Still green."),
    ("Client D: now confused, radial pulse absent", 1, "Upgrade to red."),
    ("Client C: intubated and stable in ICU", 0, "Already treated as red."),
    ("Client E: radial pulse back after blood, alert", 1, "Can move to yellow."),
    ("A green client now short of breath with chest pain", 1, "Upgrade.")],
   "Triage is repeated as clients change.",
   "Evaluate re-triage.",
   ex=[note("Re-triage", "Ongoing reassessment every 30 to 60 minutes.", id="mci-retriage")]),
]

cases = [
    case("Splenic laceration with hemorrhagic shock", spl_tabs, spl_qs),
    case("Exertional heat stroke", heat_tabs, heat_qs),
    case("Accidental hypothermia with rewarming", cold_tabs, cold_qs),
    case("Mass-casualty incident with START triage", mci_tabs, mci_qs),
]

BUILT = build(127, "trauma-cases-spleen-heat-cold-mci-standalone",
              "Trauma and environmental case studies: splenic injury, heat stroke, hypothermia and mass-casualty "
              "triage, with four standalone items",
              items, cases, case_conversion=True)
