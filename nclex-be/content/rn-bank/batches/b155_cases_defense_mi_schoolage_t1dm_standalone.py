"""Batch 155 (chunk 14: second cases for review-book subtopics with one case): two full NGN case studies and four
standalone items (CASE-MAP.md, book_topics.py).

Cases: defense mechanisms after a myocardial infarction (denial, displacement, regression, intellectualization) and
how the nurse responds to each; a school-age child newly diagnosed with type 1 diabetes, with school-age development,
school planning and hypoglycemia at school. Standalone: a kidney transplant rejection bowtie, a table of
developmental play by age, a priority question on a mental health unit and a teen driver safety question. Each case
walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety
or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH, PEDS, CV, RENAL = "mental-health", "pediatrics", "cardiovascular", "renal-urinary"
DEFENSE = "defense-mechanisms"
COPING = "coping-and-adjustment"
COMM = "therapeutic-communication"
ACS = "acute-coronary-syndrome"
GROWTH = "infant-growth-and-development"
ADOLESCENT = "adolescent-health"
INSULIN = "endocrine-medications"
HYPO = "hypoglycemia-treatment"
IMMUNO = "immunosuppressant-therapy"
PRIORITY = "prioritizing-client-care"
COLLAB = "collaboration-and-referrals"
TEACH = "health-teaching-and-behavior-change"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PPT, IMMUNO, "prioritize_hypotheses", "difficult", RENAL, "ph ms"),
   "A client who had a kidney transplant 6 weeks ago calls the transplant clinic. Using the note, fill in the "
   "bowtie: the most likely problem in the middle, two actions to take now on the left and two parameters to "
   "monitor on the right.",
   [("Acute rejection", True, "Fever, graft tenderness, less urine and rising creatinine after missed doses."),
    ("Urinary tract infection only", False, "Does not explain a tender graft and missed tacrolimus."),
    ("Dehydration", False, "Weight is up, not down."),
    ("Tacrolimus toxicity", False, "Doses were missed; the level is low.")],
   [("Tell the client to come in today for labs and possible biopsy", True, "Urgent evaluation."),
    ("Notify the transplant nephrologist", True, "Treatment."),
    ("Advise doubling tacrolimus tonight to catch up", False, "Unsafe dosing."),
    ("Advise ibuprofen for the graft pain", False, "Nephrotoxic."),
    ("Reassure and recheck at the routine visit", False, "Delay.")],
   [("Creatinine, output", True, "Graft function."),
    ("Tacrolimus level", True, "Immunosuppression."),
    ("Hearing each shift", False, "Not related."),
    ("Bowel sounds every hour", False, "Not related."),
    ("Height once a week", False, "Not related.")],
   "Missed immunosuppressants with a tender graft and less urine suggests rejection.",
   "Recognize and act on transplant rejection.",
   ex=[note("Phone call", "Missed tacrolimus for 3 days (ran out). Temperature 37.9 C, tender over the graft, passing "
       "less urine, weight up 1.5 kg. Yesterday's creatinine 2.1 (baseline 1.1). Tacrolimus level 2 ng/mL.",
       id="sa-rej")]),

dtable(M(HPM, GROWTH, "analyze_cues", "difficult", PEDS, "peds"),
   "For each age group, choose the most typical type of play.",
   ["Age group", "Example", "Type of play"],
   [(["Infant", "Shakes a rattle and watches it"], [
       ("Solitary, sensory play", True, "Infants explore with senses."),
       ("Cooperative play", False, "School age."),
       ("Associative play", False, "Preschool.")]),
    (["Toddler", "Plays near another child with similar toys, without interacting"], [
       ("Parallel play", True, "Toddler."),
       ("Cooperative play", False, "School age."),
       ("Solitary play only", False, "Infant.")]),
    (["Preschooler", "Plays pretend house with other children loosely"], [
       ("Associative play", True, "Preschool."),
       ("Parallel play", False, "Toddler."),
       ("Competitive team sports", False, "School age.")]),
    (["School-age child", "Joins a team with rules and roles"], [
       ("Cooperative play", True, "School age."),
       ("Parallel play", False, "Toddler."),
       ("Solitary play", False, "Infant.")])],
   "Play develops from solitary to parallel, associative and cooperative.",
   "Analyze play by age.",
   ex=[note("Play room", "Children of different ages on the pediatric unit.", id="sa-play")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", MH, "mgmt psych"),
   "On an inpatient mental health unit at 0700, four clients need the nurse. Which client should the nurse see "
   "first?",
   [("A client on lithium who has vomited twice and has a coarse tremor and slurred speech", True,
     "Signs of lithium toxicity are a medical emergency."),
    ("A client with schizophrenia who refuses breakfast because the food is \"poisoned\"", False, "Address after the urgent client."),
    ("A client with anorexia who wants to skip the morning weigh-in", False, "Address soon."),
    ("A client with depression who is tearful after a phone call", False, "Support soon.")],
   "Lithium toxicity is a medical emergency.",
   "Prioritize clients on a mental health unit."),

sata(M(HPM, ADOLESCENT, "take_action", "difficult", PEDS, "peds"),
   "The school nurse plans teaching for 16-year-olds who recently got driver's licenses. Which topics address the "
   "leading cause of death in this age group?",
   [("Always wear a seat belt, including in the back seat", True, "Prevents death in crashes."),
    ("Do not text or use the phone while driving", True, "Distraction."),
    ("Never drive after alcohol or cannabis, or ride with an impaired driver", True, "Impairment."),
    ("Limit teen passengers as graduated licensing laws require", True, "Peer distraction."),
    ("Speeding is safe on empty roads at night", False, "Speed and night driving raise risk."),
    ("Energy drinks are a safe way to stay awake on long drives", False, "Drowsy driving.")],
   "Motor vehicle crashes lead deaths in teens; belts, no phones, no impairment and passenger limits help.",
   "Teach teen driver safety."),
]

# ================================================================================================
# Case 1: Defense mechanisms after a myocardial infarction
# ================================================================================================
dm_tabs = [
    ("dm-notes", "Cardiac unit notes", [note("Day 2 after an anterior MI",
        "A 52-year-old business owner had a stent placed after an anterior myocardial infarction. The client "
        "walks around the unit making work calls and says, \"It was just indigestion; I'm fine.\"", id="dm-note")]),
    ("dm-data", "Observations", [table("Observations", ["Statement or behavior"], [
        ("Day 2", ["\"I don't need these heart pills; I'll be at work Monday.\""]),
        ("Day 3", ["Shouts at the spouse about the parking ticket"]),
        ("Day 3", ["Talks only about troponin numbers and ejection fraction"]),
        ("Day 4", ["Asks the nurse to feed and bathe them, though able"]),
        ("Ejection fraction", ["35%"])], id="dm-table")]),
]

dm_qs = [
hl_table(M(PSI, DEFENSE, "recognize_cues", "moderate", MH, "psych ms"),
   "Highlight the behaviors that are defense mechanisms.",
   ["Day", "Behavior"],
   [(["Day 2", "\"Just indigestion; I'm fine\""], True, "Denial."),
    (["Day 3", "Shouts at the spouse"], True, "Displacement."),
    (["Day 3", "Talks only about numbers"], True, "Intellectualization."),
    (["Day 4", "Asks to be fed and bathed"], True, "Regression."),
    (["Ejection fraction", "35%"], False, "A clinical finding.")],
   "Several defense mechanisms protect the client from the fear of the MI.",
   "Recognize defense mechanisms."),

matrix_mc(M(PSI, DEFENSE, "analyze_cues", "difficult", MH, "psych"),
   "For each behavior, indicate the defense mechanism.",
   ["Denial", "Displacement", "Intellectualization", "Regression"],
   [("Refuses to believe the heart attack happened", 0, "Denial."),
    ("Takes anger out on the spouse", 1, "Displacement."),
    ("Discusses lab values without showing feeling", 2, "Intellectualization."),
    ("Acts younger and more dependent", 3, "Regression."),
    ("\"I'll be at work Monday\"", 0, "Denial.")],
   "Naming the defense helps choose the response.",
   "Analyze defense mechanisms."),

cloze(M(PSI, DEFENSE, "prioritize_hypotheses", "difficult", MH, "psych ms"),
   "Complete the sentence by choosing from the lists of options.",
   "Denial is a problem when it {{b1}}, so the nurse should {{b2}}.",
   [[("stops the client following life-saving treatment", True, "Unsafe."),
     ("helps the client sleep", False, "Not harmful."),
     ("happens on day 1", False, "Early denial can be protective."),
     ("is mentioned by the spouse", False, "Not the issue.")],
    [("gently present the facts and connect treatment to the client's goals", True, "Supportive reality."),
     ("argue until the client admits the MI", False, "Increases defenses."),
     ("ignore it", False, "Unsafe."),
     ("tell the client they will die soon", False, "Frightening.")]],
   "Address denial when it blocks treatment, gently and with the client's goals.",
   "Prioritize the response to denial."),

sata(M(PSI, COMM, "generate_solutions", "difficult", MH, "psych ms"),
   "Which responses fit the client's defenses?",
   [("\"These medicines help you get back to work.\"", True, "Links to goals."),
    ("\"This has been frightening.\"", True, "Names feelings beneath the defense."),
    ("Encourage self-care", True, "Counters regression."),
    ("Talk with the spouse", True, "Supports the family."),
    ("\"Stop pretending this heart attack didn't happen to you.\"", False, "Confrontational."),
    ("Do all care for the client to reduce stress", False, "Reinforces regression.")],
   "Link to goals, name feelings and encourage independence.",
   "Generate responses to defense mechanisms.",
   ex=[note("Day 4", "Cardiac rehab referral planned.", id="dm-day4")]),

sata(M(MOC, COLLAB, "take_action", "difficult", MH, "mgmt ms"),
   "Which referrals should the nurse arrange before discharge?",
   [("Cardiac rehabilitation", True, "Recovery and education."),
    ("Screening for depression", True, "Common after MI."),
    ("A counselor or support group for coping", True, "Adjustment."),
    ("Smoking cessation if needed", True, "Risk reduction."),
    ("No referrals until the denial resolves", False, "Delays care."),
    ("A referral only for the spouse", False, "The client needs support.")],
   "Rehab, depression screening, coping support and risk reduction.",
   "Take action with post-MI referrals.",
   ex=[note("Discharge planning", "PHQ-9 pending.", id="dm-plan")]),

matrix_mc(M(PSI, COPING, "evaluate_outcomes", "difficult", MH, "psych ms"),
   "At the 2-week visit, for each statement, indicate whether it shows healthy coping or needs follow-up.",
   ["Healthy coping", "Needs follow-up"],
   [("\"I was scared. I'm taking my pills and going to rehab.\"", 0, "Acceptance."),
    ("\"I've cut my work hours while I recover.\"", 0, "Adaptation."),
    ("\"My wife and I talked about how frightened we were.\"", 0, "Shared coping."),
    ("\"I stopped the metoprolol because I feel fine.\"", 1, "Persistent denial."),
    ("\"I can't sleep and don't enjoy anything anymore.\"", 1, "Possible depression.")],
   "Most coping is healthy; stopping medicines and low mood need follow-up.",
   "Evaluate coping after MI.",
   ex=[note("2 weeks", "Clinic visit.", id="dm-2wk")]),
]

# ================================================================================================
# Case 2: School-age child newly diagnosed with type 1 diabetes
# ================================================================================================
sa_tabs = [
    ("sa-notes", "Pediatric unit notes", [note("New diagnosis",
        "An 8-year-old child is admitted with new type 1 diabetes after weight loss and bedwetting. The child loves "
        "soccer and worries about \"being different\" at school. The parents both work.", id="sa-note")]),
    ("sa-data", "Findings", [table("Findings", ["Result"], [
        ("Glucose", ["420 mg/dL on admission"]),
        ("HbA1c", ["11.8%"]),
        ("Ketones", ["Moderate, no acidosis"]),
        ("Weight", ["Down 2 kg"]),
        ("Development", ["Reads well, likes to learn how things work, collects cards"]),
        ("School", ["Has no school nurse on site daily"])], id="sa-table")]),
]

sa_qs = [
hl_table(M(HPM, GROWTH, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that shape teaching for this school-age child.",
   ["Finding", "Detail"],
   [(["Likes to learn how things work", "Yes"], True, "Concrete learning; teach with hands-on practice."),
    (["Worries about being different", "Yes"], True, "Peer acceptance matters."),
    (["Loves soccer", "Yes"], True, "Plan for exercise and hypoglycemia."),
    (["No school nurse daily", "Yes"], True, "Needs a school plan."),
    (["HbA1c", "11.8%"], False, "Confirms diabetes; not a teaching factor.")],
   "School-age children learn by doing and care about fitting in.",
   "Recognize cues for school-age teaching."),

matrix_mc(M(HPM, GROWTH, "analyze_cues", "difficult", PEDS, "peds"),
   "For each diabetes task, indicate who usually does it at age 8 with supervision.",
   ["The child can do it with supervision", "An adult must do it"],
   [("Check own fingerstick glucose", 0, "School-age can learn."),
    ("Recognize and report feeling low", 0, "Can learn."),
    ("Choose snacks from a list", 0, "Can learn."),
    ("Calculate and decide insulin doses", 1, "Too complex."),
    ("Give glucagon when the child is unconscious", 1, "Adult.")],
   "School-age children take on tasks gradually with adult oversight.",
   "Analyze diabetes tasks by age."),

cloze(M(PPT, INSULIN, "prioritize_hypotheses", "difficult", PEDS, "ph ada"),
   "Complete the sentence by choosing from the lists of options.",
   "Before soccer, the child should {{b1}}, because exercise {{b2}}.",
   [[("check glucose and have a carbohydrate snack if needed", True, "Prevents lows."),
     ("take extra insulin", False, "Causes lows."),
     ("skip lunch", False, "Lows."),
     ("avoid all sports", False, "Exercise is healthy.")],
    [("lowers blood glucose for hours afterward", True, "Delayed lows."),
     ("raises glucose always", False, "Usually lowers."),
     ("has no effect", False, "It does."),
     ("cures diabetes", False, "It does not.")]],
   "Check and snack before exercise.",
   "Prioritize exercise safety."),

sata(M(MOC, COLLAB, "generate_solutions", "difficult", PEDS, "mgmt peds ada"),
   "Which parts of the school plan should the nurse help arrange?",
   [("A written diabetes medical management plan for the school", True, "Plan."),
    ("Trained staff who can give glucagon", True, "Emergency."),
    ("Permission to check glucose and eat snacks in class", True, "Safety."),
    ("Fast sugar kept with the child and in the classroom", True, "Hypoglycemia."),
    ("Keep diabetes secret from the teacher", False, "Unsafe."),
    ("Send the child home for every high reading", False, "Not needed.")],
   "Written plan, trained staff and access to snacks and supplies.",
   "Generate the school plan.",
   ex=[note("Plan", "Multiple daily injections; carb counting. CGM ordered.", id="sa-plan")]),

sata(M(PA, HYPO, "take_action", "difficult", PEDS, "peds ada"),
   "At school, the child is pale, sweaty and confused before lunch, glucose 52 mg/dL, and can still swallow. Which "
   "actions should staff take?",
   [("Give 15 g fast sugar", True, "Treat."),
    ("Recheck glucose in 15 minutes", True, "Rule of 15."),
    ("Snack once above 70", True, "Prevents recurrence."),
    ("Send the child alone to the office to rest on the cot", False, "Unsafe."),
    ("Give the lunchtime insulin first so the child can eat", False, "Worsens.")],
   "Fast sugar, recheck, snack and stay.",
   "Take action on school hypoglycemia.",
   ex=[note("Week 3", "Back at school.", id="sa-week3")]),

matrix_mc(M(HPM, GROWTH, "evaluate_outcomes", "difficult", PEDS, "peds ada"),
   "At 3 months, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("\"I check my sugar before soccer and eat a snack.\"", 0, "Self-care."),
    ("HbA1c 7.6%", 0, "Improving."),
    ("\"My friends know what to do if I feel low.\"", 0, "Peer support."),
    ("\"I hide my snacks so no one sees.\"", 1, "Risk of untreated lows."),
    ("Parents give the insulin and check doses", 0, "Appropriate.")],
   "Progress; hiding snacks needs support.",
   "Evaluate school-age diabetes care.",
   ex=[note("3 months", "Clinic visit.", id="sa-3m")]),
]

cases = [
    case("Defense mechanisms after a myocardial infarction", dm_tabs, dm_qs),
    case("School-age child newly diagnosed with type 1 diabetes", sa_tabs, sa_qs),
]

BUILT = build(155, "cases-defense-mi-schoolage-t1dm-standalone",
              "Case studies: defense mechanisms after a heart attack and a school-age child with new type 1 "
              "diabetes, with four standalone items",
              items, cases, case_conversion=True)
