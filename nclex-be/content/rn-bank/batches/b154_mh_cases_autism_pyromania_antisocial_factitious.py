"""Batch 154 (chunk 14: second cases for review-book subtopics with one case): four full NGN case studies
(CASE-MAP.md, book_topics.py).

Cases: a child with autism spectrum disorder admitted for surgery (sensory and routine needs, communication);
an adolescent with conduct disorder and fire-setting (impulse control disorders); antisocial personality disorder on
a medical unit with manipulation and limit setting; factitious disorder imposed on another (a child) with mandatory
reporting (somatic symptom and related disorders). Each case walks the six CJMM steps in order, unfolds with new data
in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH, PEDS = "mental-health", "pediatrics"
NEURODEV = "neurodevelopmental-disorders"
MILIEU = "behavior-management-and-milieu"
PERSONALITY = "personality-disorders"
ABUSE = "abuse-and-neglect"
COPING = "coping-and-adjustment"
COMM = "therapeutic-communication"
REPORT = "mandatory-reporting"
COLLAB = "collaboration-and-referrals"
OPIOID = "opioid-safety"
TEACH = "health-teaching-and-behavior-change"
HOME = "equipment-and-home-safety"

items = []

# ================================================================================================
# Case 1: Autism spectrum disorder in hospital
# ================================================================================================
as_tabs = [
    ("as-notes", "Pediatric unit notes", [note("Admission",
        "A 7-year-old child with autism spectrum disorder is admitted for dental surgery under anesthesia. The child "
        "speaks in short phrases, covers the ears at loud sounds and eats only plain pasta. The parent says, \"He "
        "melts down when his routine changes.\"", id="as-note")]),
    ("as-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Communication", ["Uses a picture board"]),
        ("Sensory", ["Distressed by bright lights and beeping alarms"]),
        ("Behavior", ["Rocks and flaps hands when anxious"]),
        ("Routine", ["Bath, story and bed at 1930 every night"]),
        ("Safety", ["Has wandered away from home twice"])], id="as-table")]),
]

as_qs = [
hl_table(M(PSI, NEURODEV, "recognize_cues", "moderate", MH, "psych peds"),
   "Highlight the findings that the care plan must address.",
   ["Finding", "Detail"],
   [(["Sensory sensitivity", "Lights and alarms"], True, "Triggers distress."),
    (["Routine dependence", "Melts down with change"], True, "Hospital disrupts routine."),
    (["Communication", "Picture board"], True, "Needs adapted communication."),
    (["Wandering", "Twice"], True, "Elopement risk."),
    (["Hand flapping when anxious", "Yes"], False, "Self-soothing; should not be stopped.")],
   "Sensory needs, routine, communication and wandering risk shape care.",
   "Recognize cues in autism spectrum disorder."),

matrix_mc(M(PSI, NEURODEV, "analyze_cues", "difficult", MH, "psych peds"),
   "For each behavior, indicate the feature of autism spectrum disorder it shows.",
   ["Social communication difference", "Restricted or repetitive behavior", "Sensory difference"],
   [("Little eye contact and literal understanding", 0, "Social communication."),
    ("Lines up toys and insists on the same route", 1, "Repetitive."),
    ("Covers ears at the sound of a vacuum", 2, "Sensory."),
    ("Repeats phrases from a show (echolalia)", 1, "Repetitive."),
    ("Eats only foods of one texture", 2, "Sensory.")],
   "ASD involves social communication, repetitive behaviors and sensory differences.",
   "Analyze autism features."),

cloze(M(PSI, NEURODEV, "prioritize_hypotheses", "difficult", MH, "psych peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The best approach is to {{b1}}, using {{b2}}.",
   [[("keep routines as close to home as possible and reduce stimulation", True, "Prevents meltdowns."),
     ("change caregivers often so the child gets used to people", False, "Increases distress."),
     ("place the child near the nurses' station for noise", False, "Overstimulating."),
     ("stop the rocking", False, "Self-soothing.")],
    [("the picture board, simple words and a visual schedule", True, "Communication."),
     ("long verbal explanations", False, "Hard to process."),
     ("sudden touch to get attention", False, "Startling."),
     ("sarcasm or jokes", False, "Taken literally.")]],
   "Routine, low stimulation and visual communication.",
   "Prioritize care for a child with autism."),

sata(M(PSI, NEURODEV, "generate_solutions", "difficult", MH, "psych peds"),
   "Which parts of the plan should the nurse include?",
   [("A private, quiet room with dimmed lights", True, "Sensory."),
    ("The same nurse each shift when possible", True, "Consistency."),
    ("Bring familiar items such as a blanket and pasta from home", True, "Comfort."),
    ("Show each step with pictures before it happens", True, "Preparation."),
    ("Door alarm or close observation", True, "Elopement."),
    ("Surprise the child with procedures to avoid anxiety", False, "Increases distress.")],
   "Quiet room, consistency, familiar items, visual prep and safety.",
   "Generate the autism care plan.",
   ex=[note("Plan", "Child life specialist to prepare with a picture story. First case of the day.", id="as-plan")]),

sata(M(MOC, COLLAB, "take_action", "difficult", MH, "mgmt peds"),
   "Which actions help the surgery day go smoothly?",
   [("Schedule as the first case to avoid waiting", True, "Less waiting."),
    ("Let the parent stay until the child is asleep", True, "Comfort."),
    ("Share the child's triggers and calming strategies at every handoff", True, "Consistency."),
    ("Ask the anesthesia team about premedication if needed", True, "Reduces distress."),
    ("Ask the parent to leave early so the child adjusts", False, "Increases distress."),
    ("Use physical restraint as the first approach", False, "Last resort.")],
   "Scheduling, parent presence, handoff and premedication.",
   "Take action to coordinate care.",
   ex=[note("Surgery day", "Child anxious in the waiting area.", id="as-surgery")]),

matrix_mc(M(PSI, NEURODEV, "evaluate_outcomes", "difficult", MH, "psych peds"),
   "After surgery, for each finding, indicate whether it shows the plan worked or needs follow-up.",
   ["Plan worked", "Needs follow-up"],
   [("Calm in recovery with the parent present", 0, "Worked."),
    ("Used the picture board to show pain", 0, "Communication."),
    ("Ate pasta from home", 0, "Familiar food."),
    ("Head banging after an alarm went off", 1, "Self-injury; reduce triggers."),
    ("Went to bed at 1930 with the usual story", 0, "Routine.")],
   "Mostly worked; self-injury after noise needs attention.",
   "Evaluate autism-adapted care.",
   ex=[note("Evening", "Back on the unit.", id="as-evening")]),
]

# ================================================================================================
# Case 2: Adolescent fire-setting (impulse control)
# ================================================================================================
fs_tabs = [
    ("fs-notes", "Adolescent unit notes", [note("Admission",
        "A 14-year-old was admitted after setting a fire in a school trash can. Over a year the teen has set small "
        "fires, stolen from stores, skipped school and hurt the family dog. The teen says, \"It's not a big "
        "deal.\"", id="fs-note")]),
    ("fs-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Remorse", ["None expressed"]),
        ("Home", ["Parental conflict; father in prison"]),
        ("School", ["Suspended twice"]),
        ("Substance use", ["Cannabis on weekends"]),
        ("Suicide screen", ["Negative"]),
        ("On the unit", ["Took a lighter from a visitor's bag"])], id="fs-table")]),
]

fs_qs = [
hl_table(M(PSI, MILIEU, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that point to conduct disorder.",
   ["Finding", "Detail"],
   [(["Fire-setting", "Repeated"], True, "Destruction of property."),
    (["Stealing", "From stores"], True, "Theft."),
    (["Cruelty to animals", "Hurt the dog"], True, "Aggression."),
    (["Lack of remorse", "Yes"], True, "Callous traits."),
    (["Suicide screen", "Negative"], False, "Reassuring; not a feature.")],
   "A pattern of aggression, destruction, theft and rule-breaking is conduct disorder.",
   "Recognize cues of conduct disorder."),

matrix_mc(M(PSI, MILIEU, "analyze_cues", "difficult", MH, "psych"),
   "For each description, indicate the disorder it best fits.",
   ["Conduct disorder", "Oppositional defiant disorder", "Pyromania", "Kleptomania"],
   [("Violates others' rights, with aggression and theft", 0, "Conduct."),
    ("Angry, argumentative with adults, without serious aggression", 1, "ODD."),
    ("Sets fires for pleasure or relief, not for gain or anger", 2, "Pyromania."),
    ("Steals items not needed, with relief afterward", 3, "Kleptomania.")],
   "Impulse control disorders differ in motive and pattern.",
   "Analyze disruptive and impulse control disorders."),

cloze(M(SIC, HOME, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The immediate priority is {{b1}}, so the nurse should {{b2}}.",
   [[("safety of the teen and others", True, "Lighter on the unit."),
     ("improving grades", False, "Later."),
     ("family therapy only", False, "Later."),
     ("teaching about cannabis", False, "Later.")],
    [("remove the lighter and search belongings per policy", True, "Fire risk."),
     ("ignore it to build trust", False, "Unsafe."),
     ("let the teen keep it if promised not to use it", False, "Unsafe."),
     ("call the police first", False, "Remove the hazard first.")]],
   "Remove ignition sources first.",
   "Prioritize safety in fire-setting."),

sata(M(PSI, MILIEU, "generate_solutions", "difficult", MH, "psych"),
   "Which parts of the plan should the nurse include?",
   [("Clear, consistent rules with consequences stated in advance", True, "Structure."),
    ("Praise and privileges for positive behavior", True, "Reinforcement."),
    ("Anger management and problem-solving groups", True, "Skills."),
    ("Family therapy and parent training", True, "Family factors."),
    ("Arguing with the teen until the teen admits fault", False, "Power struggle."),
    ("Changing the rules depending on staff mood", False, "Inconsistent.")],
   "Structure, reinforcement, skills and family work.",
   "Generate the conduct disorder plan.",
   ex=[note("Day 2", "Lighter removed. Teen angry, threatens to burn the unit.", id="fs-day2")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The teen shouts, \"You can't make me follow your stupid rules!\" Which responses are appropriate?",
   [("\"Lights out is at 2200.\"", True, "Calm limit."),
    ("\"You sound angry. We can talk.\"", True, "Acknowledges feelings."),
    ("\"Following the plan earns phone time.\"", True, "Reinforcement."),
    ("\"You're acting like a criminal, just like your father.\"", False, "Shaming."),
    ("\"Fine, do whatever you want tonight; I give up arguing.\"", False, "No limits.")],
   "Calm limits, empathy and earned privileges.",
   "Take action with limit setting.",
   ex=[note("Evening", "Teen refuses to go to the room.", id="fs-evening")]),

matrix_mc(M(PSI, MILIEU, "evaluate_outcomes", "difficult", MH, "psych"),
   "At discharge, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("No aggression for 5 days", 0, "Progress."),
    ("Attends school on the unit daily", 0, "Engagement."),
    ("\"When I'm mad, I go for a run instead of breaking things.\"", 0, "Coping."),
    ("Parents say there are lighters around the house", 1, "Home safety plan needed."),
    ("\"Setting fires is still fun; I just won't get caught.\"", 1, "Ongoing risk.")],
   "Some progress; home safety and ongoing risk need follow-up.",
   "Evaluate conduct disorder care.",
   ex=[note("Day 14", "Outpatient multisystemic therapy arranged.", id="fs-dc")]),
]

# ================================================================================================
# Case 3: Antisocial personality disorder on a medical unit
# ================================================================================================
ap_tabs = [
    ("ap-notes", "Medical unit notes", [note("Medical unit",
        "A 35-year-old client admitted with a leg wound infection has a history of assault and fraud. The client "
        "is charming to some staff, demands extra opioids, and tells a new nurse, \"The other nurse lets me have "
        "two doses. You're my favorite, you'll help me.\" The client has been seen in another client's room.",
        id="ap-note")]),
    ("ap-data", "Findings", [table("Findings", ["Result"], [
        ("Pain", ["Rates 10/10 while laughing on the phone"]),
        ("Opioid orders", ["Oxycodone every 4 hours as needed"]),
        ("Another client", ["Missing wallet reported"]),
        ("Wound", ["Healing well"]),
        ("Urine drug screen", ["Positive for unprescribed benzodiazepines"])], id="ap-table")]),
]

ap_qs = [
hl_table(M(PSI, PERSONALITY, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that suggest manipulative behavior.",
   ["Finding", "Detail"],
   [(["Flattery", "\"You're my favorite\""], True, "Manipulation."),
    (["Splitting staff", "\"The other nurse lets me\""], True, "Manipulation."),
    (["Demands extra opioids", "Yes"], True, "Drug-seeking."),
    (["In another client's room; missing wallet", "Yes"], True, "Possible theft."),
    (["Wound healing", "Yes"], False, "Clinical progress.")],
   "Flattery, splitting and rule-breaking are manipulative behaviors.",
   "Recognize cues of antisocial personality disorder."),

matrix_mc(M(PSI, PERSONALITY, "analyze_cues", "difficult", MH, "psych"),
   "For each feature, indicate whether it fits antisocial or borderline personality disorder.",
   ["Antisocial personality disorder", "Borderline personality disorder"],
   [("Lack of remorse for harming others", 0, "Antisocial."),
    ("Intense fear of abandonment", 1, "Borderline."),
    ("Charming manipulation for personal gain", 0, "Antisocial."),
    ("Self-harm to relieve emotional pain", 1, "Borderline."),
    ("History of conduct disorder before age 15", 0, "Required for antisocial.")],
   "Remorse, motive and abandonment fears separate the two.",
   "Analyze personality disorders."),

cloze(M(PSI, PERSONALITY, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The key nursing approach is {{b1}}, communicated by {{b2}}.",
   [[("firm, consistent limits applied by all staff", True, "Prevents manipulation."),
     ("giving in to avoid conflict", False, "Reinforces."),
     ("arguing about the rules", False, "Power struggle."),
     ("ignoring the behavior", False, "Unsafe.")],
    [("a written care plan shared at every handoff", True, "Consistency."),
     ("each nurse deciding alone", False, "Splitting."),
     ("telling the client different rules", False, "Splitting."),
     ("only verbal reports", False, "Inconsistent.")]],
   "Firm, consistent, written limits.",
   "Prioritize care for antisocial personality disorder."),

sata(M(PPT, OPIOID, "generate_solutions", "difficult", MH, "psych ph"),
   "Which parts of the plan should the nurse include?",
   [("Give opioids exactly as ordered and assess pain objectively", True, "Safety."),
    ("Tell the provider about the unprescribed benzodiazepines", True, "Overdose risk."),
    ("Agree on a behavior contract with clear consequences", True, "Limits."),
    ("Monitor sedation and respiratory rate", True, "Opioid plus benzodiazepine."),
    ("Give an extra dose if the client threatens to complain", False, "Manipulation."),
    ("Withhold all pain medicine as punishment", False, "Pain is still treated.")],
   "Treat pain safely, report the drug screen and set limits.",
   "Generate the care plan.",
   ex=[note("Team meeting", "Behavior contract and a single plan agreed.", id="ap-team")]),

sata(M(MOC, "client-rights-and-advocacy", "take_action", "difficult", MH, "mgmt"),
   "The other client reports the missing wallet. Which actions are appropriate?",
   [("Follow the hospital policy and notify security", True, "Procedure."),
    ("Document objective facts only", True, "Legal record."),
    ("Protect the other client's belongings and safety", True, "Advocacy."),
    ("Search the client's room without following policy", False, "Rights violation."),
    ("Accuse the client in front of others", False, "Unprofessional.")],
   "Follow policy, document facts and protect others.",
   "Take action on suspected theft.",
   ex=[note("Day 3", "Wallet report filed.", id="ap-day3")]),

matrix_mc(M(PSI, PERSONALITY, "evaluate_outcomes", "difficult", MH, "psych"),
   "At discharge, for each finding, indicate whether it shows the plan worked or needs follow-up.",
   ["Plan worked", "Needs follow-up"],
   [("Opioids taken as ordered for 3 days", 0, "Limits held."),
    ("Staff gave the same answers to requests", 0, "Consistency."),
    ("Wound healed enough for outpatient care", 0, "Medical goal."),
    ("Client asks for an early opioid prescription by phone", 1, "Ongoing drug-seeking."),
    ("No further visits to other clients' rooms", 0, "Limits held.")],
   "Limits held; ongoing drug-seeking needs attention.",
   "Evaluate the care plan.",
   ex=[note("Day 5", "Discharge.", id="ap-dc")]),
]

# ================================================================================================
# Case 4: Factitious disorder imposed on another
# ================================================================================================
fd_tabs = [
    ("fd-notes", "Pediatric unit notes", [note("Pediatric unit",
        "A 4-year-old child has had 11 admissions this year for vomiting and 'seizures' that no one but the parent "
        "has seen. Tests are always normal. The parent, a former nursing assistant, is very attentive, knows medical "
        "terms well and enjoys the staff's attention. Symptoms stop when the parent is away.", id="fd-note")]),
    ("fd-data", "Findings", [table("Findings", ["Result"], [
        ("EEGs", ["Normal x4"]),
        ("Vomiting", ["Only when the parent is present"]),
        ("Toxicology", ["Ipecac (emetine) detected in the urine"]),
        ("Weight", ["Dropping"]),
        ("Siblings", ["One sibling died in infancy of unexplained causes"])], id="fd-table")]),
]

fd_qs = [
hl_table(M(PSI, ABUSE, "recognize_cues", "moderate", MH, "psych peds"),
   "Highlight the findings that suggest factitious disorder imposed on another.",
   ["Finding", "Detail"],
   [(["Symptoms seen only by the parent", "Yes"], True, "Classic."),
    (["Symptoms stop when the parent is away", "Yes"], True, "Classic."),
    (["Ipecac in the urine", "Yes"], True, "Induced vomiting."),
    (["Many admissions, normal tests", "11 admissions"], True, "Pattern."),
    (["Unexplained sibling death", "Yes"], True, "Red flag.")],
   "Symptoms that only occur with the caregiver, with poison detected, are child abuse.",
   "Recognize cues of factitious disorder imposed on another."),

matrix_mc(M(PSI, COPING, "analyze_cues", "difficult", MH, "psych"),
   "For each description, indicate the disorder.",
   ["Factitious disorder imposed on another", "Factitious disorder (self)", "Malingering", "Illness anxiety disorder"],
   [("Caregiver creates illness in a child for attention", 0, "Imposed on another."),
    ("Person creates own illness to take the sick role, without external gain", 1, "Factitious."),
    ("Person fakes illness for money or to avoid work", 2, "Malingering."),
    ("Person fears having a disease despite reassurance", 3, "Illness anxiety.")],
   "Intent and gain separate these disorders.",
   "Analyze somatic and factitious disorders."),

cloze(M(MOC, REPORT, "prioritize_hypotheses", "difficult", MH, "mgmt peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's priority is {{b1}}, which requires {{b2}}.",
   [[("the child's safety", True, "Abuse."),
     ("the parent's feelings", False, "Secondary."),
     ("more tests", False, "Tests are normal."),
     ("discharge home", False, "Unsafe.")],
    [("reporting to child protective services", True, "Mandated."),
     ("confronting the parent alone", False, "Unsafe; may flee."),
     ("waiting for a confession", False, "Not required."),
     ("telling the parent first", False, "Risk.")]],
   "Report to protect the child.",
   "Prioritize suspected abuse."),

sata(M(PSI, ABUSE, "generate_solutions", "difficult", MH, "psych peds"),
   "Which parts of the plan should the nurse include?",
   [("Supervised parent visits per the protection team", True, "Safety."),
    ("Objective documentation of what is seen, with times", True, "Legal record."),
    ("Weigh the child daily and record intake", True, "Track recovery."),
    ("Work with the child protection team and social work", True, "Coordinated care."),
    ("Let the parent give all food and medicines", False, "Poisoning risk."),
    ("Discuss the suspicion with other families on the unit", False, "Confidentiality.")],
   "Supervision, documentation, monitoring and team work.",
   "Generate the protection plan.",
   ex=[note("Report", "CPS notified. Supervised visits only.", id="fd-report")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych peds"),
   "The parent says, \"Why can't I be alone with my child? I'm the only one who knows how to care for her.\" "
   "Which responses are appropriate?",
   [("\"The team planned supervised visits.\"", True, "Factual."),
    ("\"This is upsetting for you.\"", True, "Empathy."),
    ("\"The team leader will meet you.\"", True, "Escalate."),
    ("\"We know you've been poisoning her.\"", False, "Accusatory; not the nurse's role."),
    ("\"Just this once, I'll leave the two of you alone for a while.\"", False, "Unsafe.")],
   "Factual, empathetic and escalated, without accusation.",
   "Take action with the parent.",
   ex=[note("Day 3", "Parent angry at the nurses' station.", id="fd-day3")]),

matrix_mc(M(PSI, ABUSE, "evaluate_outcomes", "difficult", MH, "psych peds"),
   "After a week of supervised care, for each finding, indicate whether it shows a safer outcome or needs "
   "follow-up.",
   ["Safer outcome", "Needs follow-up"],
   [("No vomiting for 6 days", 0, "Symptoms stopped."),
    ("Weight up 0.6 kg", 0, "Recovery."),
    ("Playing and eating well", 0, "Recovery."),
    ("Parent tries to bring homemade juice in during a visit", 1, "Possible poisoning attempt."),
    ("Sibling assessed by the protection team", 0, "Safety.")],
   "The child thrives without the parent's care; attempts to bypass rules need action.",
   "Evaluate child protection.",
   ex=[note("Day 7", "Child in kinship care pending court.", id="fd-day7")]),
]

cases = [
    case("Child with autism spectrum disorder in hospital", as_tabs, as_qs),
    case("Adolescent conduct disorder with fire-setting", fs_tabs, fs_qs),
    case("Antisocial personality disorder on a medical unit", ap_tabs, ap_qs),
    case("Factitious disorder imposed on a child", fd_tabs, fd_qs),
]

BUILT = build(154, "mh-cases-autism-pyromania-antisocial-factitious",
              "Mental health case studies: autism in hospital, conduct disorder with fire-setting, antisocial "
              "personality disorder and factitious disorder imposed on another",
              items, cases, case_conversion=True)
