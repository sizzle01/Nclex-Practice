"""Batch 148 (case conversion, chunk 13: pediatrics part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: a school-age child with a ruptured appendix (school-age development, pain assessment, peritonitis); an
adolescent after spinal fusion for scoliosis (body image, PCA, log rolling); a confidential adolescent visit using
HEADSS screening with contraception and depression screening; an adolescent with type 1 diabetes missing insulin
doses (adolescent development, sick-day rules and impending DKA). Each case walks the six CJMM steps in order,
unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
PEDS = "pediatrics"
ADOLESCENT = "adolescent-health"
GROWTH = "infant-growth-and-development"
GI = "pediatric-gi-gu-disorders"
BASICS = "pediatric-care-basics"
PAIN = "pain-assessment-and-comfort"
POSTOP = "postoperative-care"
CONFID = "confidentiality-and-information-security"
CONTRACEPTION = "family-planning-and-contraception"
SUICIDE = "suicide-risk-and-safety"
INSULIN = "endocrine-medications"
TEACH = "health-teaching-and-behavior-change"
COPING = "coping-and-adjustment"
OPIOID = "opioid-safety"

items = []

# ================================================================================================
# Case 1: Ruptured appendix in a school-age child
# ================================================================================================
ap_tabs = [
    ("ap-notes", "Emergency notes", [note("Emergency department",
        "A 9-year-old child has had pain around the belly button for a day that moved to the right lower abdomen. "
        "An hour ago the pain suddenly eased, but now the child lies very still with the knees drawn up.",
        id="ap-note")]),
    ("ap-data", "Findings", [flow("Findings", ["Morning", "Now"], [
        ("Pain", ["7/10 right lower quadrant", "Briefly better, now 9/10 all over"]),
        ("Abdomen", ["Tender at McBurney point", "Rigid, rebound tenderness"]),
        ("Temperature", ["38.0 C", "39.4 C"]),
        ("Heart rate", ["110", "142/min"]),
        ("WBC", ["14,500", "21,000/mm3"]),
        ("Behavior", ["Talking", "Lying still, refusing to move"])], id="ap-flow")]),
]

ap_qs = [
hl_table(M(PA, GI, "recognize_cues", "moderate", PEDS, "peds"),
   "Highlight the findings that suggest the appendix has ruptured.",
   ["Finding", "Now"],
   [(["Sudden relief of pain, then worse", "Yes"], True, "Classic rupture pattern."),
    (["Abdomen", "Rigid, rebound"], True, "Peritonitis."),
    (["Fever", "39.4 C"], True, "Infection."),
    (["Heart rate", "142/min"], True, "Sepsis."),
    (["Lying still", "Yes"], True, "Movement hurts with peritonitis.")],
   "Sudden pain relief followed by rigidity and fever means rupture with peritonitis.",
   "Recognize cues of a ruptured appendix."),

matrix_mc(M(BCC, PAIN, "analyze_cues", "difficult", PEDS, "peds"),
   "For each child, indicate the most appropriate pain scale.",
   ["FLACC (behavior)", "FACES (Wong-Baker)", "Numeric 0 to 10"],
   [("A 2-year-old who cannot describe pain", 0, "Behavior-based."),
    ("A 4-year-old who can point to faces", 1, "Age 3 and up."),
    ("A 9-year-old who understands numbers", 2, "School age."),
    ("A child with severe cognitive impairment who cannot self-report", 0, "Behavior-based.")],
   "Pain scales match age and ability.",
   "Analyze pediatric pain assessment."),

cloze(M(PA, GI, "prioritize_hypotheses", "difficult", PEDS, "peds"),
   "Complete the sentence by choosing from the lists of options.",
   "Before surgery, the nurse must not {{b1}}, and the child needs {{b2}}.",
   [[("apply heat to the abdomen or give a laxative or enema", True, "Can spread infection."),
     ("give IV fluids", False, "Needed."),
     ("keep the child NPO", False, "Needed."),
     ("give pain medicine", False, "Pain relief is appropriate.")],
    [("IV antibiotics, fluids and surgery", True, "Treats peritonitis."),
     ("discharge with oral antibiotics", False, "Unsafe."),
     ("a heating pad and observation", False, "Dangerous."),
     ("a high-fiber diet", False, "Not relevant.")]],
   "No heat or enemas; antibiotics and surgery.",
   "Prioritize appendicitis care."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", PEDS, "peds"),
   "After laparoscopic appendectomy for a ruptured appendix, which parts of the plan should the nurse include?",
   [("IV antibiotics", True, "Peritonitis."),
    ("Semi-Fowler position", True, "Drains the abdomen."),
    ("Early walking", True, "Prevents complications."),
    ("Watch for an abscess", True, "Complication."),
    ("Nothing by mouth for a week", False, "Diet advances with bowel sounds."),
    ("Strict bed rest for the first few days", False, "Increases complications.")],
   "Antibiotics, positioning, walking and abscess watch.",
   "Generate post-appendectomy care.",
   ex=[note("Post-op", "Ruptured appendix found; drain placed.", id="ap-postop")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "Which actions fit this school-age child's development?",
   [("Explain the body and procedures with simple diagrams", True, "Concrete thinking; likes to learn."),
    ("Respect privacy during care", True, "Modesty."),
    ("Arrange homework and contact with classmates", True, "Industry and peers."),
    ("Let the child help with care, such as recording drinks", True, "Sense of accomplishment."),
    ("Keep the child away from friends to rest", False, "Isolation."),
    ("Treat the child like a toddler with a pacifier", False, "Not age-appropriate.")],
   "Information, privacy, school and participation.",
   "Take action with school-age care.",
   ex=[note("Day 2", "Child bored and asks about missing school.", id="ap-day2")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "On day 4, for each finding, indicate whether it shows recovery or needs follow-up.",
   ["Recovery", "Needs follow-up"],
   [("Afebrile for 24 hours", 0, "Recovery."),
    ("Eating a regular diet", 0, "Recovery."),
    ("Passing gas and stool", 0, "Bowel function."),
    ("New fever of 39 C with diarrhea and rectal pressure", 1, "Pelvic abscess."),
    ("Walking the hallway", 0, "Recovery.")],
   "Recovery; new fever with rectal pressure suggests an abscess.",
   "Evaluate appendicitis recovery.",
   ex=[note("Day 4", "Drain removed.", id="ap-day4")]),
]

# ================================================================================================
# Case 2: Spinal fusion for adolescent scoliosis
# ================================================================================================
sc_tabs = [
    ("sc-notes", "Orthopedic notes", [note("Day 1 after spinal fusion",
        "A 14-year-old with idiopathic scoliosis (curve 52 degrees) had a posterior spinal fusion yesterday. The "
        "teen has a PCA pump with morphine.", id="sc-note")]),
    ("sc-data", "Findings", [flow("Findings", ["Evening", "Morning"], [
        ("Pain", ["6/10", "3/10"]),
        ("Respiratory rate", ["16", "8/min"]),
        ("Sedation", ["Awake", "Hard to rouse"]),
        ("SpO2", ["97%", "88%"]),
        ("Leg movement and feeling", ["Normal", "Normal"]),
        ("PCA use", ["Pressed by the teen", "Parent pressed it overnight"])], id="sc-flow")]),
]

sc_qs = [
hl_table(M(PPT, OPIOID, "recognize_cues", "moderate", PEDS, "peds ph"),
   "Highlight the morning findings that require immediate action.",
   ["Finding", "Morning"],
   [(["Respiratory rate", "8/min"], True, "Respiratory depression."),
    (["Sedation", "Hard to rouse"], True, "Sedation comes before respiratory depression."),
    (["SpO2", "88%"], True, "Hypoxemia."),
    (["PCA by proxy", "Parent pressed it"], True, "Removes the safety of patient control."),
    (["Legs", "Normal"], False, "Neurologically intact.")],
   "Sedation and slow breathing after PCA by proxy is opioid overdose.",
   "Recognize cues of opioid toxicity."),

matrix_mc(M(PPT, OPIOID, "analyze_cues", "difficult", PEDS, "ph"),
   "For each statement, indicate whether it is accurate or inaccurate about PCA safety.",
   ["Accurate", "Inaccurate"],
   [("Only the patient should press the button", 0, "A sleepy patient will not press it."),
    ("Increasing sedation is the earliest warning sign", 0, "Before respiratory depression."),
    ("Naloxone should be available", 0, "Antidote."),
    ("Family can press the button while the patient sleeps", 1, "Overdose risk."),
    ("Continuous pulse oximetry helps detect problems", 0, "Monitoring.")],
   "PCA works because a sedated patient cannot press the button.",
   "Analyze PCA safety."),

cloze(M(PPT, OPIOID, "prioritize_hypotheses", "difficult", PEDS, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, then {{b2}}.",
   [[("stop the PCA and stimulate the teen while giving oxygen", True, "Stop the drug; support breathing."),
     ("increase the PCA dose for comfort", False, "Worsens."),
     ("let the teen sleep", False, "Dangerous."),
     ("give a sedative", False, "Worsens.")],
    [("give naloxone as prescribed and notify the provider", True, "Reverses opioids."),
     ("wait 1 hour and recheck", False, "Delay."),
     ("encourage the parent to keep pressing", False, "Cause."),
     ("give flumazenil", False, "For benzodiazepines.")]],
   "Stop, support, naloxone.",
   "Prioritize opioid toxicity."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", PEDS, "peds"),
   "Which parts of the post-fusion plan should the nurse include?",
   [("Log roll with a pillow between the legs", True, "Keeps the spine aligned."),
    ("Neurovascular checks of the legs", True, "Spinal cord injury."),
    ("Incentive spirometry", True, "Lung expansion."),
    ("Bowel regimen", True, "Opioids and immobility."),
    ("Twisting to reach the bedside table", False, "Stresses the fusion."),
    ("Bed rest for 2 weeks", False, "Walking starts within a day or two.")],
   "Log rolling, neuro checks, breathing and bowel care.",
   "Generate post-fusion care.",
   ex=[note("After naloxone", "RR 16, awake. PCA restarted with teaching.", id="sc-after")]),

sata(M(PSI, COPING, "take_action", "difficult", PEDS, "peds psych"),
   "The teen says, \"I'll look weird with this scar. My friends can't visit me like this.\" Which actions fit "
   "adolescent development?",
   [("Listen privately", True, "Identity."),
    ("Encourage friend visits", True, "Peers matter."),
    ("Let the teen wear their own clothes", True, "Identity and control."),
    ("Involve the teen in decisions", True, "Autonomy."),
    ("Tell the teen that looks don't matter at this age", False, "Dismissive."),
    ("Keep friends away until discharge so the teen can rest", False, "Isolation.")],
   "Privacy, peers, identity and control.",
   "Take action with adolescent-appropriate care.",
   ex=[note("Day 2", "Teen tearful, refusing to look in the mirror.", id="sc-day2")]),

matrix_mc(M(HPM, ADOLESCENT, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At discharge, for each statement by the teen, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"No bending, lifting heavy things or twisting for a few months.\"", 0, "Correct."),
    ("\"I'll walk every day.\"", 0, "Correct."),
    ("\"I'll be back to soccer next week.\"", 1, "Several months."),
    ("\"I'll call if I get numbness in my legs or a fever.\"", 0, "Correct."),
    ("\"I can ride in the car using the seat belt.\"", 0, "Correct.")],
   "Activity limits and warning signs.",
   "Evaluate post-fusion teaching.",
   ex=[note("Day 4", "Walking with a physical therapist.", id="sc-day4")]),
]

# ================================================================================================
# Case 3: Confidential adolescent visit (HEADSS)
# ================================================================================================
hd_tabs = [
    ("hd-notes", "Clinic notes", [note("Adolescent clinic",
        "A 16-year-old comes for a sports physical with a parent. When the parent steps out, the teen says, \"Can "
        "I ask you something without my mom knowing?\"", id="hd-note")]),
    ("hd-data", "HEADSS screening", [table("HEADSS", ["Finding"], [
        ("Home", ["Parents divorcing; arguing at home"]),
        ("Education", ["Grades dropped this term"]),
        ("Activities", ["Quit the band"]),
        ("Drugs", ["Vapes nicotine daily; drinks at parties"]),
        ("Sexuality", ["Sexually active with one partner; condoms sometimes"]),
        ("Suicide and mood", ["PHQ-A 14; says sometimes it would be easier not to wake up"])], id="hd-table")]),
]

hd_qs = [
hl_table(M(PSI, SUICIDE, "recognize_cues", "moderate", PEDS, "psych peds"),
   "Highlight the findings that need follow-up today.",
   ["Area", "Finding"],
   [(["Mood", "PHQ-A 14, passive death wish"], True, "Needs a suicide risk assessment."),
    (["Sexuality", "Inconsistent condoms"], True, "Pregnancy and STI risk."),
    (["Drugs", "Daily vaping, drinking"], True, "Substance use."),
    (["Changes", "Grades dropped, quit band"], True, "Withdrawal, a depression sign."),
    (["Home", "Conflict"], True, "Stress.")],
   "HEADSS finds risks in several areas, the most urgent being mood and suicide risk.",
   "Recognize cues on HEADSS screening."),

matrix_mc(M(MOC, CONFID, "analyze_cues", "difficult", PEDS, "mgmt peds"),
   "For each situation, indicate whether the nurse may keep it confidential from the parent or must share it.",
   ["May keep confidential", "Must share or report"],
   [("Request for contraception", 0, "Minors can consent in most states."),
    ("STI testing and treatment", 0, "Minors can consent."),
    ("A plan to kill oneself", 1, "Safety overrides confidentiality."),
    ("Being abused by an adult", 1, "Mandatory report."),
    ("Occasional alcohol use", 0, "Counsel; usually confidential.")],
   "Confidentiality has limits when safety is at risk.",
   "Analyze adolescent confidentiality."),

cloze(M(PSI, SUICIDE, "prioritize_hypotheses", "difficult", PEDS, "psych peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first priority is to {{b1}}, and before asking sensitive questions the nurse should have {{b2}}.",
   [[("ask directly about suicidal thoughts, plans and means", True, "Risk assessment."),
     ("discuss the sports physical", False, "Not the priority."),
     ("tell the parent immediately", False, "Assess first."),
     ("give a pamphlet", False, "Not enough.")],
    [("explained the limits of confidentiality", True, "Builds trust honestly."),
     ("promised to keep everything secret", False, "Cannot promise."),
     ("asked the parent to stay", False, "Teens disclose more alone."),
     ("finished the physical exam", False, "Not needed first.")]],
   "Assess suicide risk; explain the limits of confidentiality first.",
   "Prioritize adolescent risk."),

sata(M(HPM, CONTRACEPTION, "generate_solutions", "difficult", PEDS, "peds"),
   "After the risk assessment shows no plan or intent, which parts of the plan should the nurse include?",
   [("Offer long-acting reversible contraception and condoms for STI protection", True, "Effective contraception."),
    ("STI and HIV testing", True, "Sexually active."),
    ("HPV vaccine if not complete", True, "Prevention."),
    ("Referral for counseling for depression", True, "PHQ-A 14."),
    ("A safety plan with crisis numbers", True, "Passive ideation."),
    ("Require parental consent before any contraception", False, "Usually not required.")],
   "Contraception, STI testing, HPV, counseling and a safety plan.",
   "Generate the adolescent plan.",
   ex=[note("Assessment", "No plan, no access to firearms, no prior attempts. Agrees to counseling.", id="hd-risk")]),

sata(M(PSI, "therapeutic-communication", "take_action", "difficult", PEDS, "psych peds"),
   "Which approaches help the nurse communicate with this teen?",
   [("Interview the teen alone for part of the visit", True, "Privacy."),
    ("Use open, nonjudgmental questions", True, "Trust."),
    ("Encourage the teen to involve a trusted adult", True, "Support."),
    ("Respect the teen's ideas about the plan", True, "Autonomy."),
    ("Lecture about the dangers of sex and drugs", False, "Shuts down communication."),
    ("Use slang to seem relatable", False, "May seem insincere.")],
   "Privacy, open questions and respect.",
   "Take action with adolescent communication.",
   ex=[note("Visit", "Teen talks more once the parent leaves.", id="hd-visit")]),

matrix_mc(M(HPM, ADOLESCENT, "evaluate_outcomes", "difficult", PEDS, "peds"),
   "At the 4-week follow-up, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Attending counseling weekly", 0, "Progress."),
    ("PHQ-A 8", 0, "Improved."),
    ("Has a contraceptive implant and uses condoms", 0, "Protected."),
    ("\"I told my mom about the counseling, and she's supportive.\"", 0, "Support."),
    ("\"I've been vaping more to calm down.\"", 1, "Substance use needs help."),
    ("\"I found my dad's gun and keep it in my room.\"", 1, "Lethal means; urgent.")],
   "Progress; vaping and firearm access need action.",
   "Evaluate adolescent follow-up.",
   ex=[note("4 weeks", "Return visit.", id="hd-4wk")]),
]

# ================================================================================================
# Case 4: Adolescent with type 1 diabetes missing insulin
# ================================================================================================
dm_tabs = [
    ("dm-notes", "Clinic notes", [note("Diabetes clinic",
        "A 15-year-old with type 1 diabetes for 6 years has an HbA1c of 11.2% (was 7.4%). The teen has been skipping "
        "lunchtime insulin at school because friends ask questions. Today the teen has a stomach virus and vomited "
        "twice.", id="dm-note")]),
    ("dm-data", "Findings", [table("Findings", ["Result"], [
        ("Glucose", ["342 mg/dL"]),
        ("Urine ketones", ["Moderate"]),
        ("Breathing", ["Normal, no fruity odor"]),
        ("Mental status", ["Alert"]),
        ("Last basal insulin", ["Skipped last night because \"I wasn't eating\""])], id="dm-table")]),
]

dm_qs = [
hl_table(M(PA, "diabetic-ketoacidosis", "recognize_cues", "moderate", PEDS, "peds ada"),
   "Highlight the findings that show risk of DKA.",
   ["Finding", "Result"],
   [(["Glucose", "342 mg/dL"], True, "High."),
    (["Ketones", "Moderate"], True, "Early ketosis."),
    (["Skipped basal insulin", "Yes"], True, "Main cause."),
    (["Vomiting", "Twice"], True, "Illness raises insulin needs and dehydrates."),
    (["Mental status", "Alert"], False, "Reassuring for now.")],
   "Skipped basal insulin during illness with ketones is impending DKA.",
   "Recognize cues of impending DKA."),

matrix_mc(M(HPM, ADOLESCENT, "analyze_cues", "difficult", PEDS, "peds"),
   "For each factor, indicate whether it explains the rising HbA1c in this teen.",
   ["Explains it", "Does not explain it"],
   [("Wanting to fit in with peers", 0, "Identity versus role confusion."),
    ("Puberty hormones raising insulin needs", 0, "Growth hormone."),
    ("Feeling invincible about long-term risks", 0, "Adolescent thinking."),
    ("Having had diabetes for 6 years", 1, "Not itself a cause."),
    ("Skipping school insulin", 0, "Direct cause.")],
   "Adolescent development and puberty affect diabetes control.",
   "Analyze adolescent diabetes."),

cloze(M(PPT, INSULIN, "prioritize_hypotheses", "difficult", PEDS, "ph ada"),
   "Complete the sentence by choosing from the lists of options.",
   "Sick-day rules say the teen should {{b1}} and {{b2}}.",
   [[("never skip basal insulin, even when not eating", True, "Prevents DKA."),
     ("stop insulin when vomiting", False, "Causes DKA."),
     ("halve all insulin doses", False, "Usually more is needed."),
     ("only take insulin with food", False, "Basal is needed regardless.")],
    [("check glucose and ketones every 2 to 4 hours and drink fluids", True, "Monitoring and hydration."),
     ("check once a day", False, "Too infrequent."),
     ("avoid drinking", False, "Dehydration."),
     ("wait until morning", False, "Too late.")]],
   "Keep basal insulin and monitor often.",
   "Prioritize sick-day management."),

sata(M(PPT, INSULIN, "generate_solutions", "difficult", PEDS, "ph ada"),
   "Which parts of the sick-day plan should the nurse include?",
   [("Basal plus correction insulin", True, "Clears ketones."),
    ("Sip fluids often", True, "Hydration."),
    ("Check ketones every 2 to 4 hours", True, "Monitoring."),
    ("Seek care for large ketones", True, "DKA."),
    ("Skip all insulin until vomiting stops", False, "DKA."),
    ("Exercise hard to burn off the high glucose", False, "Can worsen ketosis.")],
   "Insulin, fluids, ketone checks and when to seek care.",
   "Generate the sick-day plan.",
   ex=[note("Plan", "Basal given now plus correction insulin. Recheck in 2 hours by phone.", id="dm-plan")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", PEDS, "mgmt peds"),
   "Which actions address the missed school insulin?",
   [("Work with the school nurse on a private place to check glucose and dose", True, "Privacy."),
    ("Discuss a pump or continuous glucose monitor to reduce visible injections", True, "Fits lifestyle."),
    ("Connect the teen with a diabetes peer group or camp", True, "Peer support."),
    ("Involve the teen in setting goals", True, "Autonomy."),
    ("Threaten the teen with hospitalization if doses are missed", False, "Fear tactics backfire."),
    ("Take over all diabetes care from the teen", False, "Undermines independence.")],
   "Privacy, technology, peers and shared goals.",
   "Take action on adolescent adherence.",
   ex=[note("Next day", "Ketones negative. Glucose 180 mg/dL.", id="dm-next")]),

matrix_mc(M(HPM, ADOLESCENT, "evaluate_outcomes", "difficult", PEDS, "peds ada"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("HbA1c 8.1%", 0, "Improving."),
    ("Using a CGM and checking at lunch in the nurse's office", 0, "Adherence."),
    ("\"I never skip basal, even when sick.\"", 0, "Correct."),
    ("\"I skip insulin sometimes to lose weight.\"", 1, "Possible disordered eating (diabulimia)."),
    ("Joined a diabetes teen group", 0, "Support.")],
   "Progress; insulin omission for weight loss needs mental health follow-up.",
   "Evaluate adolescent diabetes care.",
   ex=[note("3 months", "Clinic visit.", id="dm-3m")]),
]

cases = [
    case("School-age child with a ruptured appendix", ap_tabs, ap_qs),
    case("Adolescent spinal fusion for scoliosis", sc_tabs, sc_qs),
    case("Confidential adolescent visit with HEADSS screening", hd_tabs, hd_qs),
    case("Adolescent with type 1 diabetes missing insulin", dm_tabs, dm_qs),
]

BUILT = build(148, "peds-cases-appendix-scoliosis-headss-t1dm",
              "Pediatric case studies: ruptured appendix, scoliosis surgery, adolescent HEADSS visit and adolescent "
              "type 1 diabetes",
              items, cases, case_conversion=True)
