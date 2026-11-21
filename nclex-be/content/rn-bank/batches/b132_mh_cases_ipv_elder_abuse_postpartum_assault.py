"""Batch 132 (case conversion, chunk 9: mental health part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: intimate partner violence disclosed in the emergency department, with safety planning; elder abuse and
neglect of a client with dementia by an overwhelmed caregiver, with mandatory reporting; postpartum psychosis compared
with postpartum depression; sexual assault with trauma-informed forensic care and prophylaxis. Each case walks the six
CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health
promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
IPV = "intimate-partner-and-sexual-violence"
ABUSE = "abuse-and-neglect"
REPORT = "mandatory-reporting"
PERINATAL = "perinatal-mood-disorders"
CRISIS = "crisis-intervention"
COMM = "therapeutic-communication"
CONFID = "confidentiality-and-information-security"
SUICIDE = "suicide-risk-and-safety"
PSYCH_MEDS = "psychiatric-medications"
DEMENTIA = "dementia-and-delirium"
ABX = "antimicrobial-therapy"
TEACH = "health-teaching-and-behavior-change"
COLLAB = "collaboration-and-referrals"
WOUND = "wound-care"
PRESSURE = "pressure-injury-prevention"

items = []

# ================================================================================================
# Case 1: Intimate partner violence
# ================================================================================================
ipv_tabs = [
    ("ipv-notes", "Emergency notes", [note("Emergency department",
        "A 29-year-old client, 20 weeks pregnant, says a fall down the stairs caused a wrist injury. The partner "
        "answers most questions and will not leave the room. The client avoids eye contact. This is the third "
        "injury visit this year.", id="ipv-note")]),
    ("ipv-data", "Exam", [table("Exam", ["Finding"], [
        ("Wrist", ["Swollen; x-ray shows a spiral fracture"]),
        ("Upper arms", ["Oval bruises in different colors"]),
        ("Neck", ["Faint linear marks"]),
        ("Fetal heart rate", ["146/min"]),
        ("Story", ["Changed twice during triage"])], id="ipv-table")]),
]

ipv_qs = [
hl_table(M(PSI, IPV, "recognize_cues", "moderate", MH, "psych ob"),
   "Highlight the findings that suggest intimate partner violence.",
   ["Finding", "Detail"],
   [(["Spiral fracture", "Twisting injury"], True, "Does not fit a fall."),
    (["Bruises", "Different ages, oval"], True, "Repeated grabbing."),
    (["Neck marks", "Linear"], True, "Strangulation, a strong predictor of homicide."),
    (["Partner", "Answers for the client, will not leave"], True, "Control."),
    (["Fetal heart rate", "146/min"], False, "Normal.")],
   "Injuries that do not fit the story, controlling behavior and strangulation marks suggest IPV.",
   "Recognize cues of intimate partner violence."),

matrix_mc(M(PSI, IPV, "analyze_cues", "difficult", MH, "psych ob"),
   "For each statement, indicate whether it is accurate or inaccurate about intimate partner violence.",
   ["Accurate", "Inaccurate"],
   [("Violence often starts or worsens during pregnancy", 0, "A high-risk time."),
    ("Prior strangulation greatly raises the risk of homicide", 0, "A key lethality factor."),
    ("Leaving the relationship is the most dangerous time", 0, "Risk peaks at separation."),
    ("A client who stays is choosing the abuse", 1, "Many barriers prevent leaving."),
    ("Abuse follows a cycle of tension, violence and reconciliation", 0, "The cycle of violence.")],
   "Pregnancy, strangulation and separation raise danger.",
   "Analyze intimate partner violence."),

cloze(M(PSI, IPV, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first step is to {{b1}}, then {{b2}}.",
   [[("interview the client alone, away from the partner", True, "Safe disclosure."),
     ("ask the partner what happened", False, "The abuser controls the story."),
     ("confront the partner about the injuries", False, "Raises danger."),
     ("call the police immediately without telling the client", False, "Adult IPV reporting depends on state law and the client's wishes.")],
    [("ask directly and privately about safety at home", True, "Direct, nonjudgmental screening."),
     ("tell the client to leave the partner today", False, "The client decides; leaving can be dangerous."),
     ("document only the fall", False, "Inaccurate."),
     ("discharge quickly", False, "Misses safety.")]],
   "Separate, then ask directly.",
   "Prioritize IPV screening."),

sata(M(PSI, IPV, "generate_solutions", "difficult", MH, "psych"),
   "The client discloses abuse but says, \"I can't leave. I have nowhere to go.\" Which actions should the nurse "
   "include?",
   [("Build a safety plan with the client", True, "Client-led."),
    ("Give hotline and shelter information in a safe form", True, "Hidden or memorized."),
    ("Document injuries with a body map, photos with consent and the client's own words", True, "Legal record."),
    ("Complete a lethality assessment", True, "Strangulation, weapons, threats."),
    ("Insist that the client go to a shelter tonight", False, "The client decides."),
    ("Give printed pamphlets the partner might find", False, "Can provoke violence.")],
   "Safety plan, discreet resources, accurate documentation and lethality assessment.",
   "Generate the IPV plan.",
   ex=[note("Private interview", "The client says the partner choked them last month and owns a gun.", id="ipv-disclose")]),

sata(M(MOC, CONFID, "take_action", "difficult", MH, "mgmt psych"),
   "The partner demands to see the chart and asks what the client told staff. Which actions should the nurse take?",
   [("Decline to share any information without the client's consent", True, "Confidentiality."),
    ("Call security if the partner becomes threatening", True, "Safety."),
    ("Mark the client's record so information is not given by phone", True, "Protects the client."),
    ("Tell the partner what the client disclosed to calm things down", False, "Endangers the client."),
    ("Discharge the client with the partner to avoid conflict", False, "Unsafe.")],
   "Protect confidentiality and safety.",
   "Take action to protect a client experiencing IPV.",
   ex=[note("Later", "Partner raising voice at the nurses' station.", id="ipv-partner")]),

matrix_mc(M(PSI, IPV, "evaluate_outcomes", "difficult", MH, "psych"),
   "Before discharge, for each statement, indicate whether it shows a safer plan or needs follow-up.",
   ["Safer plan", "Needs follow-up"],
   [("\"I memorized the hotline number and deleted it from my phone.\"", 0, "Discreet."),
    ("\"I have a bag with documents at my sister's house.\"", 0, "Ready to leave."),
    ("\"My code word with my sister means call 911.\"", 0, "Emergency signal."),
    ("\"I'll tell him tonight that I'm leaving.\"", 1, "Announcing separation raises danger."),
    ("\"I'll keep my prenatal appointments where I can talk alone.\"", 0, "Ongoing contact.")],
   "Discreet resources and a plan; announcing departure is dangerous.",
   "Evaluate the safety plan.",
   ex=[note("Discharge", "Advocate met the client privately.", id="ipv-dc")]),
]

# ================================================================================================
# Case 2: Elder abuse and neglect
# ================================================================================================
elder_tabs = [
    ("elder-notes", "Home health notes", [note("Home visit",
        "An 84-year-old client with moderate Alzheimer disease lives with a son who is the only caregiver and lost "
        "his job recently. The son says, \"I can't do this anymore. She wanders all night.\" The client's pension "
        "is paid into the son's account.", id="elder-note")]),
    ("elder-data", "Findings", [table("Findings", ["Finding"], [
        ("Weight", ["Down 6 kg in 3 months"]),
        ("Sacrum", ["Stage 3 pressure injury, unreported"]),
        ("Wrists", ["Bruises in a band pattern on both wrists"]),
        ("Hygiene", ["Soiled clothing, strong urine odor"]),
        ("Medicines", ["Donepezil bottle full; filled 2 months ago"]),
        ("Client", ["Flinches when the son raises his voice"])], id="elder-table")]),
]

elder_qs = [
hl_table(M(PSI, ABUSE, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that suggest abuse or neglect.",
   ["Finding", "Detail"],
   [(["Weight loss", "6 kg"], True, "Neglect."),
    (["Pressure injury", "Stage 3, unreported"], True, "Neglect."),
    (["Wrist bruises", "Band pattern"], True, "Possible restraint."),
    (["Medicines", "Not given"], True, "Neglect."),
    (["Pension to son's account", "With job loss"], True, "Possible financial exploitation."),
    (["Flinching", "When the son shouts"], True, "Fear.")],
   "Several findings suggest physical abuse, neglect and financial exploitation.",
   "Recognize cues of elder abuse."),

matrix_mc(M(PSI, ABUSE, "analyze_cues", "difficult", MH, "psych"),
   "For each finding, indicate the type of elder mistreatment it suggests.",
   ["Physical abuse", "Neglect", "Financial exploitation"],
   [("Band bruises on the wrists", 0, "Restraint."),
    ("Untreated pressure injury", 1, "Failure to provide care."),
    ("Missed medicines", 1, "Neglect."),
    ("Pension used by the caregiver", 2, "Financial."),
    ("Weight loss and soiled clothes", 1, "Neglect.")],
   "Elder mistreatment has several forms.",
   "Analyze types of elder abuse."),

cloze(M(MOC, REPORT, "prioritize_hypotheses", "difficult", MH, "mgmt"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse must {{b1}}, because nurses are {{b2}}.",
   [[("report the suspected abuse to adult protective services", True, "Required."),
     ("wait for proof before reporting", False, "Suspicion is enough."),
     ("ask the son's permission to report", False, "Not needed."),
     ("only document it", False, "Reporting is required.")],
    [("mandated reporters of suspected elder abuse", True, "A legal duty."),
     ("allowed to report only with the client's consent", False, "Not when the client lacks capacity."),
     ("not involved in family matters", False, "Wrong."),
     ("required to investigate it themselves", False, "Agencies investigate.")]],
   "Suspected elder abuse is reported.",
   "Prioritize mandatory reporting."),

sata(M(PSI, ABUSE, "generate_solutions", "difficult", MH, "psych"),
   "Which parts of the plan should the nurse include?",
   [("Wound care and nutrition referral", True, "Physical needs."),
    ("Respite care and an adult day program", True, "Reduces caregiver strain."),
    ("Caregiver support group referral", True, "Support."),
    ("Door alarms for wandering instead of restraints", True, "Safe alternative."),
    ("Teach the son to tie the client's wrists at night", False, "Abuse."),
    ("Leave the situation as it is because the son is family", False, "Unsafe.")],
   "Meet care needs, support the caregiver and use safe alternatives.",
   "Generate the elder care plan.",
   ex=[note("APS", "Report made. Social worker visiting tomorrow.", id="elder-aps")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The son says, \"I know I've lost my temper. I'm so tired.\" Which responses are therapeutic?",
   [("\"Caring for her every night is exhausting.\"", True, "Empathy and help."),
    ("\"What happens when you lose your temper?\"", True, "Explores safely."),
    ("\"You should be ashamed of yourself for treating your mother like this.\"", False, "Judgmental."),
    ("\"Don't worry, everyone does that sometimes.\"", False, "Minimizes harm."),
    ("\"I won't tell anyone about this, it can stay between us.\"", False, "A false promise; reporting is required.")],
   "Empathy with honesty about reporting.",
   "Take action with the caregiver.",
   ex=[note("Visit 2", "Son meets the nurse in the kitchen.", id="elder-son")]),

matrix_mc(M(PSI, ABUSE, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 1 month, for each finding, indicate whether it shows a safer situation or needs follow-up.",
   ["Safer", "Needs follow-up"],
   [("Weight up 1.5 kg", 0, "Nutrition improving."),
    ("Pressure injury smaller", 0, "Healing."),
    ("Adult day program 3 days a week", 0, "Respite."),
    ("New bruise on the cheek", 1, "Possible ongoing abuse."),
    ("Pension now managed by a court-appointed guardian", 0, "Financial protection.")],
   "Care improved; a new injury needs a new report.",
   "Evaluate elder abuse interventions.",
   ex=[note("1 month", "Home health visits twice weekly.", id="elder-1m")]),
]

# ================================================================================================
# Case 3: Postpartum psychosis
# ================================================================================================
pp_tabs = [
    ("pp-notes", "Postpartum unit and phone notes", [note("Day 6 after birth",
        "A 26-year-old client with a history of bipolar disorder (stopped lithium in pregnancy) gave birth 6 days ago. "
        "The partner calls: the client has not slept for 3 nights, is talking fast, and says the baby \"has the devil "
        "in him and needs to be saved.\"", id="pp-note")]),
    ("pp-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Sleep", ["None for 3 nights"]),
        ("Mood", ["Elated, then terrified"]),
        ("Perceptions", ["Hears God telling her to save the baby"]),
        ("Orientation", ["Unsure of the date"]),
        ("Baby", ["At home with the client and partner"]),
        ("EPDS", ["Not able to complete"])], id="pp-table")]),
]

pp_qs = [
hl_table(M(PSI, PERINATAL, "recognize_cues", "moderate", MH, "psych ob"),
   "Highlight the findings that make this a psychiatric emergency.",
   ["Finding", "Detail"],
   [(["Delusion about the baby", "\"Needs to be saved\""], True, "Risk of infanticide."),
    (["Command hallucinations", "God telling her"], True, "High risk."),
    (["No sleep", "3 nights"], True, "Mania."),
    (["Confusion", "Unsure of date"], True, "Postpartum psychosis often has confusion."),
    (["Bipolar disorder, off lithium", "History"], True, "The strongest risk factor.")],
   "Postpartum psychosis with delusions about the baby is an emergency.",
   "Recognize cues of postpartum psychosis."),

matrix_mc(M(PSI, PERINATAL, "analyze_cues", "difficult", MH, "psych ob"),
   "For each feature, indicate whether it fits postpartum blues, postpartum depression or postpartum psychosis.",
   ["Postpartum blues", "Postpartum depression", "Postpartum psychosis"],
   [("Tearful days 3 to 5, resolves within 2 weeks", 0, "Blues."),
    ("Low mood for weeks with guilt and poor bonding", 1, "Depression."),
    ("Hallucinations and confusion in the first 2 weeks", 2, "Psychosis."),
    ("Intrusive fear of harming the baby that horrifies the mother", 1, "Ego-dystonic thoughts in depression or OCD."),
    ("Belief that harming the baby is the right thing", 2, "Delusional.")],
   "Timing, duration and psychosis separate the three.",
   "Analyze postpartum mood disorders."),

cloze(M(PSI, PERINATAL, "prioritize_hypotheses", "difficult", MH, "psych ob"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should tell the partner to {{b1}} and {{b2}}.",
   [[("keep the baby with another adult and not leave the client alone", True, "Protects both."),
     ("let the client rest with the baby", False, "Dangerous."),
     ("wait to see if it improves with sleep", False, "Too dangerous."),
     ("give the client an over-the-counter sleep aid", False, "Not enough.")],
    [("bring the client to the emergency department now or call 911", True, "Emergency evaluation."),
     ("book a routine appointment next week", False, "Too slow."),
     ("call back tomorrow", False, "Too slow."),
     ("stop breastfeeding only", False, "Does not address safety.")]],
   "Separate the baby and get emergency help.",
   "Prioritize postpartum psychosis."),

sata(M(PSI, PERINATAL, "generate_solutions", "difficult", MH, "psych ob ph"),
   "The client is admitted. Which parts of the plan should the nurse anticipate?",
   [("One-to-one observation", True, "Safety."),
    ("Supervised visits with the baby when stable", True, "Bonding with safety."),
    ("Mood stabilizer and antipsychotic", True, "Treatment."),
    ("Help with expressing breast milk if safe", True, "Supports feeding goals."),
    ("Unsupervised rooming-in with the baby to promote bonding", False, "Unsafe."),
    ("An antidepressant alone as the first treatment", False, "Can worsen mania.")],
   "Observation, supervised contact and mood stabilization.",
   "Generate the postpartum psychosis plan.",
   ex=[note("Admission", "Mother-baby psychiatric unit. Lithium and quetiapine started.", id="pp-admit")]),

sata(M(MOC, COLLAB, "take_action", "difficult", MH, "mgmt ob"),
   "Which actions should the nurse take to coordinate care?",
   [("Inform the pediatric team and arrange the baby's care", True, "Infant safety."),
    ("Include the partner in planning with the client's consent", True, "Support."),
    ("Arrange perinatal psychiatry follow-up", True, "Continuity."),
    ("Discharge as soon as the client sleeps one night", False, "Too early."),
    ("Tell the partner the client will never be safe with the baby", False, "Most recover fully.")],
   "Coordinate infant care, family support and specialist follow-up.",
   "Take action to coordinate perinatal care.",
   ex=[note("Day 4", "Sleeping 6 hours; no hallucinations.", id="pp-day4")]),

matrix_mc(M(PSI, PERINATAL, "evaluate_outcomes", "difficult", MH, "psych ob"),
   "At discharge, for each statement or finding, indicate whether it shows readiness or needs follow-up.",
   ["Readiness", "Needs follow-up"],
   [("Sleeps 7 hours with partner doing night feeds", 0, "Protects sleep."),
    ("No psychotic symptoms for 10 days", 0, "Remission."),
    ("\"I'll keep taking lithium and get my levels checked.\"", 0, "Adherence."),
    ("\"I feel so guilty, maybe the baby is better off without me.\"", 1, "Depression and suicide risk."),
    ("Home visits from a perinatal nurse arranged", 0, "Support.")],
   "Remission with support; guilt and hopelessness need follow-up.",
   "Evaluate postpartum psychosis recovery.",
   ex=[note("Day 14", "Planning discharge.", id="pp-day14")]),
]

# ================================================================================================
# Case 4: Sexual assault
# ================================================================================================
sa_tabs = [
    ("sa-notes", "Emergency notes", [note("Emergency department",
        "A 20-year-old client arrives with a friend 6 hours after being sexually assaulted by an acquaintance at a "
        "party. The client says, \"I think something was put in my drink. I can't remember most of it.\" The client "
        "is shaking and calm by turns.", id="sa-note")]),
    ("sa-data", "Findings", [table("Findings", ["Finding"], [
        ("Mental status", ["Alert, oriented, memory gaps"]),
        ("Injuries", ["Bruises on the inner thighs"]),
        ("Showered", ["No"]),
        ("Last menstrual period", ["2 weeks ago; no contraception"]),
        ("Pregnancy test", ["Negative"]),
        ("Client wishes", ["Unsure about reporting to police"])], id="sa-table")]),
]

sa_qs = [
hl_table(M(PSI, IPV, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that guide the nurse's priorities.",
   ["Finding", "Detail"],
   [(["Memory gaps", "Possible drug in drink"], True, "Toxicology timing."),
    (["Not showered", "Yes"], True, "Evidence can still be collected."),
    (["Mid-cycle, no contraception", "LMP 2 weeks ago"], True, "Emergency contraception."),
    (["Unsure about reporting", "Yes"], True, "Evidence can be collected and held."),
    (["Bruises", "Inner thighs"], True, "Document.")],
   "Timing, evidence, pregnancy and STI risk shape care.",
   "Recognize cues after sexual assault."),

matrix_mc(M(PSI, IPV, "analyze_cues", "difficult", MH, "psych"),
   "For each reaction, indicate whether it is common after sexual assault or should prompt concern.",
   ["Common reaction", "Should prompt concern"],
   [("Appearing calm and controlled", 0, "A controlled response is common."),
    ("Crying and shaking", 0, "An expressed response."),
    ("Self-blame", 0, "Common; needs gentle correction."),
    ("Saying, \"I want to die\"", 1, "Suicide risk."),
    ("Memory gaps after a drink", 1, "Possible drug-facilitated assault.")],
   "Responses vary; suicidal statements and drugging need action.",
   "Analyze responses to sexual assault."),

cloze(M(PSI, CRISIS, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "After medical stability, the priority is to {{b1}}, and the client {{b2}}.",
   [[("restore a sense of control and safety", True, "Trauma-informed care."),
     ("question the client about what they were wearing", False, "Blaming."),
     ("insist on a police report", False, "The client decides."),
     ("discharge quickly", False, "Misses needs.")],
    [("chooses each part of the exam and can stop at any time", True, "Consent throughout."),
     ("must complete every step", False, "Not required."),
     ("cannot have a support person present", False, "May have one."),
     ("must decide about police before the exam", False, "Evidence can be held.")]],
   "Trauma-informed care gives control back.",
   "Prioritize sexual assault care."),

sata(M(PPT, ABX, "generate_solutions", "difficult", MH, "ph cdc"),
   "Which treatments should the nurse anticipate offering?",
   [("Emergency contraception", True, "Within 120 hours."),
    ("Ceftriaxone, doxycycline and metronidazole for STI prophylaxis", True, "Gonorrhea, chlamydia, trichomonas."),
    ("HIV post-exposure prophylaxis within 72 hours", True, "Prevents HIV."),
    ("Hepatitis B vaccine if not immune", True, "Prevents hepatitis B."),
    ("A sedative to help the client forget", False, "Not appropriate."),
    ("Wait for STI test results before treatment", False, "Prophylaxis is given now.")],
   "Pregnancy, STI, HIV and hepatitis B prevention.",
   "Generate post-assault prophylaxis.",
   ex=[note("SANE", "A sexual assault nurse examiner is on the way.", id="sa-sane")]),

sata(M(SIC, "client-identification-and-error-prevention", "take_action", "difficult", MH, "psych"),
   "Which actions protect the evidence and the client?",
   [("Ask the client not to drink, eat, wash or urinate until samples are taken, if possible", True, "Preserves evidence."),
    ("Collect clothing in paper bags", True, "Plastic degrades evidence."),
    ("Keep the chain of custody", True, "Legal requirement."),
    ("Draw blood and urine for toxicology with consent", True, "Possible drugging."),
    ("Store the clothing in a sealed plastic bag", False, "Moisture destroys DNA."),
    ("Leave the evidence kit at the desk while the nurse takes a break", False, "Breaks the chain of custody.")],
   "Preserve evidence and keep the chain of custody.",
   "Take action on forensic evidence.",
   ex=[note("Exam", "Client consents to evidence collection but not to police contact yet.", id="sa-consent")]),

matrix_mc(M(PSI, CRISIS, "evaluate_outcomes", "difficult", MH, "psych"),
   "At discharge, for each statement, indicate whether it shows readiness or needs follow-up.",
   ["Readiness", "Needs follow-up"],
   [("\"I know the kit is stored if I decide to report later.\"", 0, "Informed."),
    ("\"My friend will stay with me tonight.\"", 0, "Support."),
    ("\"I'll take all 28 days of the HIV medicine.\"", 0, "Adherence."),
    ("\"It was my fault for going to that party.\"", 1, "Self-blame needs counseling."),
    ("Has the rape crisis center number and a follow-up visit", 0, "Continuity.")],
   "Informed, supported and linked; self-blame needs follow-up.",
   "Evaluate sexual assault care.",
   ex=[note("Discharge", "Advocate from the rape crisis center present.", id="sa-dc")]),
]

cases = [
    case("Intimate partner violence in pregnancy", ipv_tabs, ipv_qs),
    case("Elder abuse and neglect of a client with dementia", elder_tabs, elder_qs),
    case("Postpartum psychosis", pp_tabs, pp_qs),
    case("Sexual assault with trauma-informed forensic care", sa_tabs, sa_qs),
]

BUILT = build(132, "mh-cases-ipv-elder-abuse-postpartum-assault",
              "Mental health case studies: intimate partner violence, elder abuse, postpartum psychosis and sexual "
              "assault",
              items, cases, case_conversion=True)
