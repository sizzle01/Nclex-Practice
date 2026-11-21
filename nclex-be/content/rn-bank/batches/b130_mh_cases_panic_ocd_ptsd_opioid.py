"""Batch 130 (case conversion, chunk 8: mental health disorders with psychiatric medications): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: panic disorder with benzodiazepine and SSRI treatment (anxiolytics); obsessive-compulsive disorder with
exposure and response prevention; post-traumatic stress disorder in a veteran with nightmares and prazosin; opioid
overdose with naloxone, then buprenorphine induction and precipitated withdrawal. Each case walks the six CJMM steps
in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
ANXIETY = "anxiety-and-related-disorders"
TRAUMA = "trauma-and-stress-related-disorders"
SUBSTANCE = "substance-use-disorders"
PSYCH_MEDS = "psychiatric-medications"
OPIOID = "opioid-safety"
COMM = "therapeutic-communication"
CRISIS = "crisis-intervention"
MILIEU = "behavior-management-and-milieu"
SUICIDE = "suicide-risk-and-safety"
TEACH = "health-teaching-and-behavior-change"
COLLAB = "collaboration-and-referrals"
RIGHTS = "client-rights-and-advocacy"
DELEG = "delegation-and-supervision"
CULTURE = "cultural-and-spiritual-care"

items = []

# ================================================================================================
# Case 1: Panic disorder
# ================================================================================================
pan_tabs = [
    ("pan-notes", "Emergency notes", [note("Emergency department",
        "A 27-year-old client arrives for the fourth time in 2 months with sudden chest tightness, pounding heart, "
        "shaking, tingling hands and a fear of dying that peaked within 10 minutes. The client now avoids driving "
        "and supermarkets. The client drinks 5 energy drinks a day.", id="pan-note")]),
    ("pan-data", "Results", [table("Results", ["Result"], [
        ("ECG", ["Sinus tachycardia 118/min, otherwise normal"]),
        ("Troponin", ["Negative x2"]),
        ("TSH", ["Normal"]),
        ("Glucose", ["92 mg/dL"]),
        ("Respiratory rate", ["30/min"]),
        ("SpO2", ["99%"]),
        ("Hands", ["Tingling around the mouth and fingers"])], id="pan-table")]),
]

pan_qs = [
hl_table(M(PSI, ANXIETY, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that support panic disorder rather than a medical cause.",
   ["Finding", "Result"],
   [(["Peak", "Within 10 minutes"], True, "Panic attacks peak quickly."),
    (["Recurrent attacks with avoidance", "Driving, supermarkets"], True, "Behavior change from fear of attacks."),
    (["Troponin and ECG", "Normal"], True, "Cardiac cause excluded."),
    (["TSH and glucose", "Normal"], True, "Thyroid and glucose causes excluded."),
    (["Energy drinks", "5 a day"], False, "A trigger that must be addressed, not a support for the diagnosis.")],
   "Recurrent sudden attacks with avoidance and normal tests suggest panic disorder.",
   "Recognize cues of panic disorder."),

matrix_mc(M(PSI, ANXIETY, "analyze_cues", "difficult", MH, "psych"),
   "For each finding during the attack, indicate its cause.",
   ["Hyperventilation lowering CO2", "Sympathetic surge"],
   [("Tingling around the mouth and fingers", 0, "Respiratory alkalosis."),
    ("Pounding heart", 1, "Adrenaline."),
    ("Light-headedness", 0, "Low CO2 narrows brain vessels."),
    ("Shaking and sweating", 1, "Adrenaline."),
    ("Muscle cramps in the hands", 0, "Alkalosis lowers ionized calcium.")],
   "Overbreathing and adrenaline explain the symptoms.",
   "Analyze panic symptoms."),

cloze(M(PSI, ANXIETY, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "During an attack, the nurse's priority is to {{b1}}, using {{b2}}.",
   [[("stay with the client and reduce the anxiety", True, "Safety and calm."),
     ("teach coping skills in detail", False, "Severe anxiety blocks learning."),
     ("leave the client alone to calm down", False, "Increases fear."),
     ("explain the physiology of panic", False, "Cannot be processed during an attack.")],
    [("short, calm sentences and slow breathing together", True, "Simple directions work."),
     ("a paper bag held over the face", False, "Can cause hypoxia."),
     ("a loud, firm voice", False, "Increases fear."),
     ("long explanations", False, "Not absorbed.")]],
   "Stay, speak simply and guide breathing.",
   "Prioritize care during a panic attack."),

matrix_mc(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "For each medicine, indicate its role in panic disorder.",
   ["Long-term first-line treatment", "Short-term relief only", "Not used for panic disorder"],
   [("Sertraline", 0, "SSRIs are first-line."),
    ("Lorazepam", 1, "Fast relief; dependence with long use."),
    ("Buspirone", 2, "Slow; for generalized anxiety, not panic."),
    ("Cognitive behavioral therapy (not a medicine)", 0, "First-line therapy."),
    ("Venlafaxine", 0, "An SNRI option.")],
   "SSRIs or SNRIs with CBT long term; benzodiazepines only short term.",
   "Generate the panic disorder treatment plan.",
   ex=[note("Plan", "Sertraline 25 mg daily, increase slowly. Lorazepam 0.5 mg as needed for 2 weeks. CBT referral.", id="pan-plan")]),

sata(M(PPT, PSYCH_MEDS, "take_action", "difficult", MH, "ph"),
   "Which teaching should the nurse give about the new medicines?",
   [("Sertraline may take 4 to 6 weeks to work", True, "Delayed effect."),
    ("Anxiety may briefly worsen at the start of sertraline", True, "Start low."),
    ("Do not drink alcohol with lorazepam", True, "Respiratory depression."),
    ("Do not stop lorazepam suddenly after regular use", True, "Withdrawal seizures."),
    ("Take extra lorazepam before driving to prevent attacks", False, "Sedation impairs driving."),
    ("Stop sertraline once you feel better after a week", False, "Too early; relapse.")],
   "Delayed SSRI effect, avoiding alcohol and not stopping suddenly.",
   "Take action with anxiolytic teaching.",
   ex=[note("Clinic", "Client asks how fast the medicines work.", id="pan-clinic")]),

matrix_mc(M(PSI, ANXIETY, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 3 months, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("One mild attack this month (was several a week)", 0, "Improved."),
    ("\"I drove to the supermarket with my CBT homework plan.\"", 0, "Facing avoidance."),
    ("\"I cut down to one coffee a day.\"", 0, "Removes a trigger."),
    ("\"I take lorazepam every day now, sometimes three tablets.\"", 1, "Dependence risk."),
    ("\"I use slow breathing when I feel it starting.\"", 0, "Coping skill.")],
   "Fewer attacks and less avoidance; daily benzodiazepine use needs review.",
   "Evaluate panic disorder treatment.",
   ex=[note("3 months", "Sertraline 100 mg.", id="pan-3m")]),
]

# ================================================================================================
# Case 2: Obsessive-compulsive disorder
# ================================================================================================
ocd_tabs = [
    ("ocd-notes", "Unit notes", [note("Mental health unit",
        "A 33-year-old client with obsessive-compulsive disorder is admitted because handwashing now takes 5 hours a "
        "day. The client fears contaminating their baby and checks the stove 30 times before leaving. The client "
        "knows the fears are excessive. The client's hands are cracked and bleeding.", id="ocd-note")]),
    ("ocd-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Y-BOCS score", ["32 (severe)"]),
        ("Hands", ["Fissures, bleeding, redness"]),
        ("Missed meals", ["Often, because of rituals"]),
        ("Insight", ["Good"]),
        ("Mood", ["Low, ashamed"]),
        ("Thoughts of self-harm", ["Denies"])], id="ocd-table")]),
]

ocd_qs = [
hl_table(M(PSI, ANXIETY, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that need nursing action.",
   ["Finding", "Detail"],
   [(["Hands", "Cracked, bleeding"], True, "Skin breakdown and infection risk."),
    (["Missed meals", "Because of rituals"], True, "Nutrition."),
    (["Mood", "Low, ashamed"], True, "Depression is common with OCD."),
    (["Insight", "Good"], False, "A strength that helps therapy."),
    (["Self-harm", "Denies"], False, "Continue to monitor.")],
   "Physical harm from rituals and low mood need action.",
   "Recognize cues in OCD."),

matrix_mc(M(PSI, ANXIETY, "analyze_cues", "difficult", MH, "psych"),
   "For each item, indicate whether it is an obsession or a compulsion.",
   ["Obsession", "Compulsion"],
   [("Intrusive fear of contaminating the baby", 0, "A thought."),
    ("Washing hands for 30 minutes", 1, "A behavior."),
    ("Checking the stove 30 times", 1, "A behavior."),
    ("Unwanted image of the house burning", 0, "A thought."),
    ("Counting silently to 50 to cancel a thought", 1, "A mental compulsion.")],
   "Obsessions are thoughts; compulsions are acts that relieve anxiety.",
   "Analyze obsessions and compulsions."),

cloze(M(PSI, ANXIETY, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "On admission, the nurse should {{b1}}, because stopping rituals suddenly {{b2}}.",
   [[("allow time for rituals while setting gradual limits", True, "Reduces rituals without overwhelming anxiety."),
     ("forbid all handwashing at once", False, "Causes severe anxiety."),
     ("ignore the rituals", False, "Does not help."),
     ("point out that the fears are silly", False, "Shames.")],
    [("causes severe anxiety", True, "Rituals relieve anxiety."),
     ("cures OCD quickly", False, "It does not."),
     ("has no effect", False, "It has an effect."),
     ("reduces depression", False, "Not likely.")]],
   "Gradual limits rather than sudden prohibition.",
   "Prioritize OCD care."),

sata(M(PSI, ANXIETY, "generate_solutions", "difficult", MH, "psych ph"),
   "Which parts of the plan should the nurse include?",
   [("Exposure and response prevention with the therapist", True, "First-line therapy."),
    ("A schedule that builds in time for meals and rituals", True, "Meets basic needs."),
    ("Emollient and gentle skin care for the hands", True, "Skin healing."),
    ("A high-dose SSRI as prescribed", True, "OCD often needs higher doses."),
    ("Reassure the client every time they ask if their hands are clean", False, "Reassurance acts as a compulsion."),
    ("Use alcohol hand rub instead of washing", False, "Worsens cracked skin and is still a ritual.")],
   "ERP, a structured schedule, skin care and an SSRI; avoid reassurance cycles.",
   "Generate the OCD plan.",
   ex=[note("Orders", "Fluvoxamine titrated to 200 mg. ERP daily.", id="ocd-orders")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The client repeatedly asks, \"Are you sure my hands are clean? Are you sure?\" Which responses are therapeutic?",
   [("\"We agreed to sit with the doubt for 10 minutes, together.\"", True, "Supports response prevention."),
    ("\"It sounds really uncomfortable not to be sure.\"", True, "Validates."),
    ("\"Yes, they are definitely clean, I watched you wash them and I promise.\"", False, "Reassurance feeds the cycle."),
    ("\"Please stop asking me that, it is the tenth time this hour.\"", False, "Punitive."),
    ("\"Go and wash them again so you feel better.\"", False, "Reinforces the compulsion.")],
   "Validate and support the agreed plan without reassuring.",
   "Take action with OCD communication.",
   ex=[note("Day 4", "ERP session: touching the door handle without washing for 20 minutes.", id="ocd-day4")]),

matrix_mc(M(PSI, ANXIETY, "evaluate_outcomes", "difficult", MH, "psych"),
   "At discharge, for each finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Handwashing down to 45 minutes a day", 0, "Large reduction."),
    ("Y-BOCS 18", 0, "Moderate; improved."),
    ("Hands healing", 0, "Skin care working."),
    ("\"I'll keep doing ERP homework at home.\"", 0, "Continues therapy."),
    ("\"My partner checks the stove for me now, 20 times.\"", 1, "Family accommodation keeps rituals going.")],
   "Progress; family accommodation needs family teaching.",
   "Evaluate OCD treatment.",
   ex=[note("Week 3", "Discharge with outpatient ERP.", id="ocd-week3")]),
]

# ================================================================================================
# Case 3: PTSD in a veteran
# ================================================================================================
ptsd_tabs = [
    ("ptsd-notes", "Clinic notes", [note("Veterans clinic",
        "A 36-year-old veteran returned from combat 2 years ago. The veteran has nightmares most nights, startles at "
        "loud noises, avoids crowds and fireworks, and says, \"I feel numb, like I'm watching my life on TV.\" The "
        "veteran drinks 8 beers most nights to sleep. The veteran's partner reports angry outbursts.", id="ptsd-note")]),
    ("ptsd-data", "Screening", [table("Screening", ["Result"], [
        ("PCL-5", ["58 (high)"]),
        ("PHQ-9", ["17 (moderately severe)"]),
        ("AUDIT-C", ["10 (high)"]),
        ("Firearm at home", ["Yes, loaded in the bedside drawer"]),
        ("Suicidal thoughts", ["\"Sometimes I think everyone would be better off\""])], id="ptsd-table")]),
]

ptsd_qs = [
hl_table(M(PSI, TRAUMA, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that need urgent attention.",
   ["Finding", "Result"],
   [(["Suicidal thoughts", "Better off without me"], True, "Needs a full risk assessment."),
    (["Firearm", "Loaded at the bedside"], True, "Lethal means."),
    (["Alcohol", "AUDIT-C 10"], True, "Raises risk and worsens PTSD."),
    (["Angry outbursts", "Per partner"], True, "Possible violence risk."),
    (["PCL-5", "58"], True, "Severe PTSD symptoms.")],
   "Suicidal thoughts with a loaded gun and heavy drinking need urgent action.",
   "Recognize cues of risk in PTSD."),

matrix_mc(M(PSI, TRAUMA, "analyze_cues", "difficult", MH, "psych"),
   "For each symptom, indicate the PTSD cluster it belongs to.",
   ["Intrusion", "Avoidance", "Negative mood and thinking", "Arousal and reactivity"],
   [("Nightmares", 0, "Re-experiencing."),
    ("Avoiding crowds and fireworks", 1, "Avoidance."),
    ("Feeling numb and detached", 2, "Negative changes."),
    ("Startling at loud noises", 3, "Hyperarousal."),
    ("Angry outbursts", 3, "Reactivity.")],
   "PTSD has four symptom clusters.",
   "Analyze PTSD symptoms."),

cloze(M(PSI, SUICIDE, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first priority is {{b1}}, including {{b2}}.",
   [[("a full suicide risk assessment", True, "Thoughts plus means."),
     ("starting trauma therapy today", False, "Safety first."),
     ("teaching sleep hygiene", False, "Secondary."),
     ("couples counseling", False, "Secondary.")],
    [("lethal means counseling about the firearm", True, "Storing it away from home reduces risk."),
     ("advice to keep the gun closer for safety", False, "Increases risk."),
     ("telling the veteran to drink less only", False, "Not enough."),
     ("waiting until next visit to discuss it", False, "Delays safety.")]],
   "Risk assessment and means safety first.",
   "Prioritize safety in PTSD."),

sata(M(PSI, TRAUMA, "generate_solutions", "difficult", MH, "psych ph"),
   "Which parts of the plan should the nurse include?",
   [("Trauma-focused therapy such as prolonged exposure or cognitive processing", True, "First-line."),
    ("Sertraline or paroxetine", True, "First-line medicines."),
    ("Prazosin at bedtime for nightmares as prescribed", True, "Reduces nightmares."),
    ("Treatment for alcohol use", True, "Co-occurring."),
    ("Long-term daily benzodiazepines for sleep", False, "Worsen PTSD outcomes and interact with alcohol."),
    ("Ask the veteran to describe the trauma in detail at every visit", False, "Retraumatizes outside of structured therapy.")],
   "Trauma therapy, SSRIs, prazosin and alcohol treatment.",
   "Generate the PTSD plan.",
   ex=[note("Risk assessment", "Thoughts without a plan; agrees to store the gun with a friend. Safety plan written.", id="ptsd-risk")]),

sata(M(PPT, PSYCH_MEDS, "take_action", "difficult", MH, "ph"),
   "The veteran starts prazosin 1 mg at bedtime. Which teaching should the nurse give?",
   [("Take the first dose at bedtime", True, "First-dose hypotension."),
    ("Rise slowly from bed at night", True, "Orthostatic hypotension."),
    ("Avoid alcohol, which worsens dizziness", True, "Additive."),
    ("Stop the medicine if nightmares improve after one night", False, "Effects build over weeks."),
    ("Double the dose if a nightmare occurs", False, "Hypotension risk.")],
   "Bedtime dosing, slow rising and no alcohol.",
   "Take action with prazosin teaching.",
   ex=[note("Prescription", "Prazosin 1 mg at bedtime, increase weekly.", id="ptsd-rx")]),

matrix_mc(M(PSI, TRAUMA, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 3 months, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Nightmares 1 to 2 a week (was nightly)", 0, "Improved."),
    ("Drinking 2 beers on weekends", 0, "Reduced."),
    ("PCL-5 32", 0, "Improved."),
    ("\"I got my gun back from my friend last week.\"", 1, "Means safety must be reviewed."),
    ("Went to a family barbecue for an hour", 0, "Less avoidance."),
    ("\"I still feel cut off from my kids.\"", 1, "Ongoing symptoms need therapy focus.")],
   "Progress; firearm access and detachment need follow-up.",
   "Evaluate PTSD treatment.",
   ex=[note("3 months", "Completed 10 sessions of cognitive processing therapy.", id="ptsd-3m")]),
]

# ================================================================================================
# Case 4: Opioid overdose and buprenorphine induction
# ================================================================================================
op_tabs = [
    ("op-notes", "Emergency notes", [note("Emergency department",
        "A 31-year-old client was found unresponsive in a restroom with a needle nearby. Paramedics gave naloxone "
        "4 mg nasally twice. The client says the last use was \"fentanyl, I think.\"", id="op-note")]),
    ("op-data", "Findings", [flow("Findings", ["On arrival", "45 minutes"], [
        ("Respiratory rate", ["14/min", "6/min"]),
        ("SpO2", ["95% on 2 L", "84% on 2 L"]),
        ("Pupils", ["Normal", "Pinpoint"]),
        ("Mental status", ["Awake, irritable", "Hard to rouse"]),
        ("Client", ["Wants to leave", "Snoring"])], id="op-flow")]),
]

op_qs = [
hl_table(M(PPT, OPIOID, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the 45-minute findings that require immediate action.",
   ["Finding", "45 minutes"],
   [(["Respiratory rate", "6/min"], True, "Respiratory depression."),
    (["SpO2", "84%"], True, "Hypoxemia."),
    (["Pupils", "Pinpoint"], True, "Opioid effect."),
    (["Mental status", "Hard to rouse"], True, "Renarcotization."),
    (["Snoring", "Yes"], True, "Airway obstruction.")],
   "Naloxone wore off before the fentanyl did.",
   "Recognize cues of renarcotization."),

matrix_mc(M(PPT, OPIOID, "analyze_cues", "difficult", MH, "ph"),
   "For each statement, indicate whether it is accurate or inaccurate about naloxone.",
   ["Accurate", "Inaccurate"],
   [("Naloxone lasts 30 to 90 minutes, shorter than many opioids", 0, "Renarcotization."),
    ("It can cause sudden withdrawal in dependent clients", 0, "Agitation, vomiting."),
    ("It has no effect if no opioids are present", 0, "Safe to give."),
    ("One dose always reverses fentanyl permanently", 1, "Repeat doses or an infusion may be needed."),
    ("The goal is to restore breathing, not full alertness", 0, "Titrate.")],
   "Naloxone is short-acting and titrated to breathing.",
   "Analyze naloxone effects."),

cloze(M(PPT, OPIOID, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, then {{b2}}.",
   [[("open the airway and give rescue breaths with a bag-valve mask", True, "Breathing first."),
     ("start a naloxone infusion before ventilating", False, "Ventilate first."),
     ("let the client sleep it off", False, "Fatal."),
     ("draw a urine drug screen", False, "Not a priority.")],
    [("give naloxone in small IV doses and consider an infusion", True, "Restores breathing without severe withdrawal."),
     ("discharge home", False, "Unsafe."),
     ("give flumazenil", False, "For benzodiazepines; seizure risk."),
     ("give a sedative", False, "Worsens breathing.")]],
   "Airway and breathing first, then titrated naloxone.",
   "Prioritize opioid overdose care."),

sata(M(PSI, SUBSTANCE, "generate_solutions", "difficult", MH, "psych ph"),
   "The next day the client says, \"I want to stop using.\" Which parts of the plan should the nurse include?",
   [("Offer buprenorphine starting in the hospital", True, "Medication for opioid use disorder lowers death."),
    ("Give a naloxone kit and teach the family to use it", True, "Harm reduction."),
    ("Link to an outpatient program before discharge", True, "Continuity."),
    ("Screen for hepatitis C and HIV", True, "Injection risk."),
    ("Require 30 days of abstinence before medication", False, "Delays life-saving treatment."),
    ("Advise detoxification without medication as the best choice", False, "Higher relapse and overdose risk.")],
   "Buprenorphine, naloxone, linkage and infection screening.",
   "Generate the opioid use disorder plan.",
   ex=[note("Next day", "Last fentanyl use 18 hours ago. COWS score pending.", id="op-nextday")]),

sata(M(PPT, OPIOID, "take_action", "difficult", MH, "ph"),
   "Before the first buprenorphine dose, review the findings. Which actions should the nurse take?",
   [("Hold the buprenorphine and notify the provider", True, "Too early; COWS is low."),
    ("Explain that starting too early can cause sudden withdrawal", True, "Precipitated withdrawal."),
    ("Reassess the COWS score in a few hours", True, "Wait for moderate withdrawal."),
    ("Give the full dose now because the client asked for it", False, "Precipitated withdrawal."),
    ("Give naloxone to speed up withdrawal", False, "Harmful.")],
   "Buprenorphine starts when withdrawal is moderate; too early causes precipitated withdrawal.",
   "Take action on buprenorphine timing.",
   ex=[table("Before dose", ["Result"], [
       ("COWS score", ["5 (mild)"]),
       ("Hours since fentanyl", ["18"]),
       ("Client", ["Comfortable, slight yawning"])], id="op-cows")]),

matrix_mc(M(PSI, SUBSTANCE, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 1 month, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Taking buprenorphine daily; urine negative for fentanyl", 0, "Progress."),
    ("\"My sister carries naloxone now.\"", 0, "Harm reduction."),
    ("\"I'm also taking my friend's alprazolam to sleep.\"", 1, "Benzodiazepines with opioids raise overdose risk."),
    ("Attending a peer support group weekly", 0, "Support."),
    ("\"I'll stop buprenorphine next week now that I'm better.\"", 1, "Early stopping raises relapse risk.")],
   "Recovery under way; benzodiazepine use and early stopping need follow-up.",
   "Evaluate opioid use disorder treatment.",
   ex=[note("1 month", "Buprenorphine-naloxone 16 mg daily.", id="op-1m")]),
]

cases = [
    case("Panic disorder with anxiolytic treatment", pan_tabs, pan_qs),
    case("Obsessive-compulsive disorder with exposure and response prevention", ocd_tabs, ocd_qs),
    case("Post-traumatic stress disorder in a veteran", ptsd_tabs, ptsd_qs),
    case("Opioid overdose and buprenorphine induction", op_tabs, op_qs),
]

BUILT = build(130, "mh-cases-panic-ocd-ptsd-opioid",
              "Mental health case studies: panic disorder, OCD, PTSD and opioid use disorder",
              items, cases, case_conversion=True)
