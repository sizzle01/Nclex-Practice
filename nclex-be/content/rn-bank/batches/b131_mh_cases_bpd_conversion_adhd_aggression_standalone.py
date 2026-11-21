"""Batch 131 (case conversion, chunk 8: mental health disorders with psychiatric medications): four full NGN case
studies and four standalone items (CASE-MAP.md, book_topics.py).

Cases: borderline personality disorder with self-harm and splitting; functional neurological symptom (conversion)
disorder with somatic symptoms; a child with ADHD and oppositional behavior on a stimulant (childhood and
neurodevelopmental disorders); escalating aggression in intermittent explosive disorder with de-escalation, seclusion
and restraint (impulse control). Standalone: an acute dystonia bowtie, a defense mechanism table, a priority question
on a mental health unit and a delegation question on a mental health unit. Each case walks the six CJMM steps in
order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or health promotion
question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH, PEDS = "mental-health", "pediatrics"
PERSONALITY = "personality-disorders"
DEFENSE = "defense-mechanisms"
NEURODEV = "neurodevelopmental-disorders"
COPING = "coping-and-adjustment"
PSYCH_MEDS = "psychiatric-medications"
COMM = "therapeutic-communication"
MILIEU = "behavior-management-and-milieu"
CRISIS = "crisis-intervention"
SUICIDE = "suicide-risk-and-safety"
RESTRAINT = "restraint-safety"
RIGHTS = "client-rights-and-advocacy"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
WOUND = "wound-care"
COLLAB = "collaboration-and-referrals"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph psych"),
   "A client on a mental health unit has a new problem after starting an antipsychotic. Using the note, fill in "
   "the bowtie: the most likely problem in the middle, two actions to take on the left and two things to monitor "
   "on the right.",
   [("Acute dystonia", True, "Sustained muscle spasms within days of a high-potency antipsychotic."),
    ("Tardive dyskinesia", False, "Develops after months to years."),
    ("Neuroleptic malignant syndrome", False, "No fever or rigidity with instability."),
    ("Akathisia", False, "Restlessness, not fixed spasms.")],
   [("Give benztropine or diphenhydramine IM as prescribed", True, "Rapid relief."),
    ("Stay with the client and keep the airway clear", True, "Laryngeal spasm risk."),
    ("Give another haloperidol dose for the distress", False, "Worsens dystonia."),
    ("Tell the client the spasms are voluntary", False, "Untrue and dismissive."),
    ("Apply restraints to hold the neck straight", False, "Harmful.")],
   [("Airway, swallowing", True, "Laryngeal dystonia."),
    ("Response to the dose", True, "Confirms effect."),
    ("Body weight once a week", False, "Not urgent."),
    ("Bowel sounds each shift", False, "Not related."),
    ("Blood glucose before meals", False, "Not related.")],
   "Fixed spasms of the neck and eyes days after haloperidol is acute dystonia; treat with an anticholinergic.",
   "Recognize and act on acute dystonia.",
   ex=[note("Day 2", "22-year-old started haloperidol yesterday. Neck twisted to the left, eyes rolled upward, "
       "frightened, speaking with difficulty. Temperature 37.0 C, no rigidity of the limbs.", id="sa-dystonia")]),

dtable(M(PSI, DEFENSE, "analyze_cues", "difficult", MH, "psych"),
   "The nurse hears these statements on a mental health unit. For each statement, choose the defense mechanism.",
   ["Client", "Statement", "Defense mechanism"],
   [(["A", "After a DUI arrest: \"I don't have a drinking problem. Everyone drinks.\""], [
       ("Denial", True, "Refuses to accept reality."),
       ("Projection", False, "Attributes own feelings to others."),
       ("Sublimation", False, "Healthy redirection.")]),
    (["B", "Angry at the boss, yells at the children at home."], [
       ("Displacement", True, "Feelings moved to a safer target."),
       ("Regression", False, "Earlier behavior."),
       ("Rationalization", False, "Excuses.")]),
    (["C", "\"I failed the test because the questions were unfair.\""], [
       ("Rationalization", True, "A logical-sounding excuse."),
       ("Repression", False, "Unconscious forgetting."),
       ("Undoing", False, "Act to cancel.")]),
    (["D", "Unfaithful client accuses the partner of cheating."], [
       ("Projection", True, "Own impulses placed on others."),
       ("Denial", False, "Not the pattern."),
       ("Identification", False, "Not the pattern.")])],
   "Naming the defense helps the nurse respond therapeutically.",
   "Analyze defense mechanisms.",
   ex=[note("Unit", "Statements recorded during group and individual sessions.", id="sa-defense")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", MH, "mgmt psych"),
   "The nurse on an inpatient mental health unit gets four reports at the same time. Which client should the nurse "
   "see first?",
   [("A client with schizophrenia who is pacing and talking to unseen voices in the hallway, as on most days", False,
     "Baseline behavior; monitor."),
    ("A client with depression who was quiet all week and is now cheerful and asked to have their belongings back", True,
     "A sudden lift in mood with gathering belongings can signal a suicide plan."),
    ("A client with mania who is loudly singing in the dayroom and wants to start a business", False,
     "Needs limits and a calm area, but not the highest risk."),
    ("A client with anorexia who wants to skip the weigh-in this morning", False, "Address after the safety check.")],
   "A sudden improvement in mood in a depressed client is a suicide warning sign.",
   "Prioritize clients on a mental health unit."),

sata(M(MOC, DELEG, "take_action", "difficult", MH, "mgmt psych"),
   "Which tasks on a mental health unit can the RN delegate to the mental health technician (UAP)?",
   [("Doing 15-minute safety checks and recording them", True, "Routine observation."),
    ("Escorting a client with privileges to the cafeteria", True, "Routine."),
    ("Taking vital signs on a client in alcohol withdrawal and reporting them", True, "Measurement and reporting."),
    ("Deciding whether a client on one-to-one can go to every-15-minute checks", False, "A clinical decision."),
    ("Doing the admission suicide risk assessment", False, "Assessment stays with the RN."),
    ("Leading a medication teaching group", False, "Teaching stays with licensed staff.")],
   "Technicians observe, escort and measure; risk decisions, assessment and teaching stay with the RN.",
   "Delegate on a mental health unit."),
]

# ================================================================================================
# Case 1: Borderline personality disorder with self-harm
# ================================================================================================
bpd_tabs = [
    ("bpd-notes", "Unit notes", [note("Mental health unit",
        "A 23-year-old client with borderline personality disorder was admitted after cutting the forearms when a "
        "partner ended the relationship. The client tells the night nurse, \"You're the only one who understands me; "
        "the day nurse is cruel.\" The next day the client tells the day nurse the same about the night nurse.",
        id="bpd-note")]),
    ("bpd-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Forearms", ["Six superficial cuts, two deeper and gaping"]),
        ("Mood", ["Shifts from rage to tears within an hour"]),
        ("History", ["Frequent self-harm to relieve tension; one overdose last year"]),
        ("Relationships", ["Intense, unstable"]),
        ("Statement", ["\"If you leave me alone tonight, I'll do something.\""])], id="bpd-table")]),
]

bpd_qs = [
hl_table(M(PSI, PERSONALITY, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that require nursing action.",
   ["Finding", "Detail"],
   [(["Forearms", "Two deeper gaping cuts"], True, "Need wound care and possible closure."),
    (["Threat", "\"I'll do something\""], True, "Risk must be assessed; never ignored."),
    (["Splitting", "Praises one nurse, condemns another"], True, "Needs a consistent team response."),
    (["History", "Overdose last year"], True, "Raises suicide risk."),
    (["Mood shifts", "Rage to tears"], True, "Affective instability.")],
   "Self-harm, threats and splitting all need a planned response.",
   "Recognize cues in borderline personality disorder."),

matrix_mc(M(PSI, PERSONALITY, "analyze_cues", "difficult", MH, "psych"),
   "For each behavior, indicate the personality disorder cluster it most fits.",
   ["Cluster A (odd, eccentric)", "Cluster B (dramatic, emotional)", "Cluster C (anxious, fearful)"],
   [("Fear of abandonment with self-harm", 1, "Borderline."),
    ("Disregard for others' rights without remorse", 1, "Antisocial."),
    ("Suspicious that others intend harm", 0, "Paranoid."),
    ("Avoids social contact for fear of criticism", 2, "Avoidant."),
    ("Needs others to make everyday decisions", 2, "Dependent.")],
   "Personality disorders group into three clusters.",
   "Analyze personality disorder clusters."),

cloze(M(PSI, PERSONALITY, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, and the team manages splitting by {{b2}}.",
   [[("safety from self-harm and suicide", True, "Highest risk."),
     ("improving relationships with partners", False, "Longer-term."),
     ("teaching about the diagnosis", False, "Secondary."),
     ("weight management", False, "Not relevant.")],
    [("consistent limits agreed by all staff and shared in handoff", True, "Prevents manipulation of staff."),
     ("assigning the favorite nurse every shift", False, "Reinforces splitting."),
     ("avoiding the client", False, "Abandonment fear."),
     ("arguing with the client about the claims", False, "Power struggle.")]],
   "Safety first; consistency defeats splitting.",
   "Prioritize BPD care."),

sata(M(PSI, PERSONALITY, "generate_solutions", "difficult", MH, "psych"),
   "Which parts of the plan should the nurse include?",
   [("A written behavioral contract with clear limits", True, "Consistency."),
    ("Dialectical behavior therapy skills such as distress tolerance", True, "First-line therapy."),
    ("Care for wounds in a neutral, matter-of-fact way", True, "Avoids rewarding self-harm with extra attention."),
    ("Regular short check-ins not tied to self-harm", True, "Meets needs without reinforcing harm."),
    ("Extra attention and special privileges after each cut", False, "Reinforces self-harm."),
    ("Different rules depending on which nurse is working", False, "Feeds splitting.")],
   "Contracts, DBT skills, neutral wound care and scheduled contact.",
   "Generate the BPD plan.",
   ex=[note("Team meeting", "Agreed: one consistent plan, shared in every handoff.", id="bpd-team")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The client says, \"You're the only nurse who cares. Can you give me your phone number for after I leave?\" "
   "Which responses are therapeutic?",
   [("\"I can't share my number. Who else could you call?\"", True, "A clear limit with an alternative."),
    ("\"You sound worried about being alone.\"", True, "Names the feeling."),
    ("\"The whole team cares about you, not just me.\"", True, "Counters splitting."),
    ("\"Sure, I'll write it down for you, but please don't tell anyone.\"", False, "A boundary violation."),
    ("\"That is manipulative behavior, and I won't put up with it on this unit.\"", False, "Judgmental.")],
   "Firm boundaries with empathy and team framing.",
   "Take action with boundaries.",
   ex=[note("Day 5", "Discharge planned in 2 days.", id="bpd-day5")]),

matrix_mc(M(PSI, PERSONALITY, "evaluate_outcomes", "difficult", MH, "psych"),
   "At discharge, for each statement or finding, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("\"When I feel the urge to cut, I hold ice and call my DBT coach.\"", 0, "A distress-tolerance skill."),
    ("No self-harm for 5 days", 0, "Progress."),
    ("Wounds healing without infection", 0, "Healing."),
    ("\"If my ex doesn't call me back, I'll take all my pills.\"", 1, "A suicide threat needs reassessment."),
    ("Has a DBT group start date", 0, "Continuity.")],
   "Skills and no self-harm; a new threat needs reassessment.",
   "Evaluate BPD progress.",
   ex=[note("Day 7", "Safety plan reviewed.", id="bpd-day7")]),
]

# ================================================================================================
# Case 2: Functional neurological symptom (conversion) disorder
# ================================================================================================
fnd_tabs = [
    ("fnd-notes", "Neurology unit notes", [note("Neurology unit",
        "A 30-year-old client was admitted with sudden weakness of both legs that started the day after witnessing a "
        "fatal car crash. The client appears calm about being unable to walk. The client has visited the emergency "
        "department 9 times this year for chest pain and abdominal pain with normal tests and worries constantly "
        "about having a serious illness.", id="fnd-note")]),
    ("fnd-data", "Results", [table("Results", ["Result"], [
        ("MRI brain and spine", ["Normal"]),
        ("Reflexes", ["Normal and equal"]),
        ("Hoover sign", ["Positive (hip extension returns when the other leg flexes)"]),
        ("Bladder and bowel", ["Normal"]),
        ("Muscle bulk", ["Normal"])], id="fnd-table")]),
]

fnd_qs = [
hl_table(M(PSI, COPING, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that support a functional neurological (conversion) disorder.",
   ["Finding", "Result"],
   [(["Onset", "After witnessing a fatal crash"], True, "A stressor before onset."),
    (["Hoover sign", "Positive"], True, "A positive sign of functional weakness."),
    (["MRI and reflexes", "Normal"], True, "No structural cause."),
    (["Calm about the paralysis", "Yes"], True, "Sometimes seen (la belle indifference), though not required."),
    (["Bladder and bowel", "Normal"], True, "Against cord compression.")],
   "Positive functional signs with normal tests after a stressor.",
   "Recognize cues of functional neurological disorder."),

matrix_mc(M(PSI, COPING, "analyze_cues", "difficult", MH, "psych"),
   "For each description, indicate the disorder it fits.",
   ["Functional neurological (conversion) disorder", "Somatic symptom disorder", "Illness anxiety disorder", "Factitious disorder"],
   [("Neurologic symptoms not explained by disease, after stress", 0, "Conversion."),
    ("Real distressing symptoms with excessive time and worry about them", 1, "Somatic symptom."),
    ("Few or no symptoms but intense fear of having a disease", 2, "Illness anxiety."),
    ("Deliberately creating symptoms to take the sick role", 3, "Factitious.")],
   "Somatic disorders differ in symptoms, worry and intent.",
   "Analyze somatic symptom disorders."),

cloze(M(PSI, COPING, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The symptoms are {{b1}}, so the nurse should {{b2}}.",
   [[("real to the client and not consciously produced", True, "Not faking."),
     ("deliberately faked", False, "That is factitious or malingering."),
     ("caused by a hidden stroke", False, "Tests are normal."),
     ("dangerous to ignore with more scans", False, "Tests are done.")],
    [("acknowledge the symptoms while focusing on function and stress", True, "Validates and rehabilitates."),
     ("tell the client nothing is wrong", False, "Invalidating."),
     ("order more scans to reassure", False, "Reinforces illness focus."),
     ("confront the client as faking", False, "Harmful.")]],
   "Symptoms are real; focus on function and coping.",
   "Prioritize functional disorder care."),

sata(M(PSI, COPING, "generate_solutions", "difficult", MH, "psych"),
   "Which parts of the plan should the nurse include?",
   [("Physical therapy focused on retraining movement", True, "Effective for functional weakness."),
    ("Explain the diagnosis as a real, treatable problem", True, "Builds trust."),
    ("Referral for psychotherapy, including trauma-focused work", True, "Addresses the stressor."),
    ("Limit repeated tests unless new signs appear", True, "Reduces illness focus."),
    ("Keep the client on bed rest until strength returns", False, "Worsens disability."),
    ("Do all care for the client to avoid frustration", False, "Promotes dependence.")],
   "Physical therapy, clear explanation, psychotherapy and limiting tests.",
   "Generate the functional disorder plan.",
   ex=[note("Diagnosis", "Functional neurological disorder with functional leg weakness.", id="fnd-dx")]),

sata(M(MOC, COLLAB, "take_action", "difficult", MH, "mgmt psych"),
   "Which actions support coordinated care for this client's frequent emergency visits?",
   [("Arrange one primary care provider for regular scheduled visits", True, "Reduces emergency use."),
    ("Share a care plan with the emergency department", True, "Consistency."),
    ("Refer to a mental health professional", True, "Treats the somatic symptom disorder."),
    ("Tell the client not to come to the emergency department again", False, "New symptoms still need assessment."),
    ("Label the client a drug seeker in the chart", False, "Stigmatizing and inaccurate.")],
   "Scheduled visits, a shared plan and mental health care.",
   "Take action to coordinate care.",
   ex=[note("Day 4", "Standing with a frame.", id="fnd-day4")]),

matrix_mc(M(PSI, COPING, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 6 weeks, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Walking with a cane", 0, "Functional recovery."),
    ("Attends trauma-focused therapy", 0, "Treatment."),
    ("Emergency visits down to one in 6 weeks", 0, "Progress."),
    ("\"I'm sure there's a tumor the scans missed; I want another MRI.\"", 1, "Health anxiety persists."),
    ("\"I'm having nightmares about the crash.\"", 1, "Possible PTSD needs assessment.")],
   "Progress in function; health anxiety and trauma symptoms need follow-up.",
   "Evaluate functional disorder treatment.",
   ex=[note("6 weeks", "Outpatient physical therapy twice weekly.", id="fnd-6wk")]),
]

# ================================================================================================
# Case 3: Child with ADHD on a stimulant
# ================================================================================================
adhd_tabs = [
    ("adhd-notes", "Pediatric clinic notes", [note("Pediatric clinic",
        "An 8-year-old child was diagnosed with ADHD (combined type) 3 months ago and takes extended-release "
        "methylphenidate each morning. The parent says schoolwork is better, but the child eats little at lunch, "
        "has trouble falling asleep and has lost 2 kg. The child argues and refuses rules at home.",
        id="adhd-note")]),
    ("adhd-data", "Findings", [table("Findings", ["Today", "3 months ago"], [
        ("Weight", ["24 kg", "26 kg"]),
        ("Height percentile", ["40th", "45th"]),
        ("Heart rate", ["112/min", "88/min"]),
        ("Blood pressure", ["118/76 mm Hg (95th percentile)", "100/64"]),
        ("Sleep onset", ["11 pm", "9 pm"]),
        ("Teacher rating", ["Improved attention", "Poor attention"])], id="adhd-table")]),
]

adhd_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", PEDS, "ph peds"),
   "Highlight the findings that need follow-up.",
   ["Finding", "Today"],
   [(["Weight", "Lost 2 kg"], True, "Appetite suppression."),
    (["Height percentile", "Falling"], True, "Growth slowing."),
    (["Heart rate and blood pressure", "112/min, 95th percentile"], True, "Cardiovascular effects."),
    (["Sleep onset", "11 pm"], True, "Insomnia."),
    (["Teacher rating", "Improved"], False, "The desired effect.")],
   "Stimulants improve attention but can slow growth and raise heart rate and blood pressure.",
   "Recognize cues of stimulant adverse effects."),

matrix_mc(M(PSI, NEURODEV, "analyze_cues", "difficult", PEDS, "peds psych"),
   "For each behavior, indicate whether it fits ADHD, oppositional defiant disorder or autism spectrum disorder.",
   ["ADHD", "Oppositional defiant disorder", "Autism spectrum disorder"],
   [("Cannot wait for a turn, blurts out answers", 0, "Impulsivity."),
    ("Angry, argues with adults, deliberately annoys", 1, "Oppositional."),
    ("Rigid routines and distress when they change", 2, "Autism."),
    ("Loses homework, easily distracted", 0, "Inattention."),
    ("Limited eye contact and trouble with back-and-forth conversation", 2, "Social communication.")],
   "The three conditions often overlap but differ in core features.",
   "Analyze childhood neurodevelopmental disorders."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", PEDS, "ph peds"),
   "Complete the sentence by choosing from the lists of options.",
   "The most important problem to address with the provider is {{b1}}, and a likely change is {{b2}}.",
   [[("weight loss with slowed growth", True, "Long-term harm."),
     ("better attention", False, "Desired."),
     ("arguing at home", False, "Behavioral therapy target."),
     ("homework", False, "Not a medicine problem.")],
    [("giving the dose with breakfast and adding calorie-dense snacks or adjusting the dose", True, "Protects growth."),
     ("giving the dose at bedtime", False, "Worsens insomnia."),
     ("doubling the dose", False, "Worsens effects."),
     ("stopping all treatment", False, "Not first step.")]],
   "Protect growth by timing doses and adjusting nutrition or dose.",
   "Prioritize stimulant adverse effects."),

sata(M(PSI, NEURODEV, "generate_solutions", "difficult", PEDS, "peds psych"),
   "Which parts of the plan should the nurse include?",
   [("Parent behavior management training", True, "First-line for oppositional behavior."),
    ("Height, weight, heart rate and blood pressure at each visit", True, "Monitoring."),
    ("A consistent bedtime routine without screens", True, "Sleep."),
    ("School accommodations such as a 504 plan", True, "Support."),
    ("Harsh physical punishment for defiance", False, "Worsens behavior."),
    ("Large sugary drinks at night to make up calories", False, "Poor nutrition and sleep.")],
   "Behavior training, monitoring, sleep routine and school supports.",
   "Generate the ADHD plan.",
   ex=[note("Plan", "Dose moved to after breakfast. Evening snack. Recheck in 4 weeks.", id="adhd-plan")]),

sata(M(SIC, "medication-safety-systems", "take_action", "difficult", PEDS, "ph peds"),
   "Which teaching should the nurse give the parent about methylphenidate safety?",
   [("Store it locked up and count the tablets", True, "Misuse and diversion."),
    ("Swallow extended-release tablets whole", True, "Crushing releases the whole dose."),
    ("Report chest pain or fainting at once", True, "Rare cardiac effects."),
    ("Share it with a sibling who has trouble focusing", False, "Illegal and unsafe."),
    ("Give an extra dose in the afternoon for homework if one is missed", False, "Insomnia and overdose.")],
   "Secure storage, swallow whole and report cardiac symptoms.",
   "Take action with stimulant safety teaching.",
   ex=[note("Parent", "\"My teenage son's friends asked about the pills.\"", id="adhd-parent")]),

matrix_mc(M(PSI, NEURODEV, "evaluate_outcomes", "difficult", PEDS, "peds psych"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Weight back to 25.5 kg", 0, "Recovery."),
    ("Asleep by 9:30 pm", 0, "Improved."),
    ("Fewer arguments using a reward chart", 0, "Behavior plan working."),
    ("New eye blinking and throat-clearing tics", 1, "Stimulants can unmask tics."),
    ("Blood pressure 104/66 mm Hg", 0, "Normal."),
    ("\"The child doesn't need the plan at school anymore.\"", 1, "Supports should continue.")],
   "Growth and behavior improved; new tics need review.",
   "Evaluate ADHD treatment.",
   ex=[note("3 months", "Attends parent training weekly.", id="adhd-3m")]),
]

# ================================================================================================
# Case 4: Escalating aggression with de-escalation, seclusion and restraint
# ================================================================================================
agg_tabs = [
    ("agg-notes", "Unit notes", [note("Mental health unit",
        "A 28-year-old client with intermittent explosive disorder and cannabis use was admitted after punching a "
        "wall. Tonight the client was refused a phone call after hours.", id="agg-note")]),
    ("agg-data", "Behavior log", [flow("Behavior", ["2000", "2010", "2015"], [
        ("Speech", ["Loud, swearing", "Shouting threats at a peer", "Screaming"]),
        ("Body", ["Pacing, clenched fists", "Kicked a chair", "Picked up a chair over the head"]),
        ("Response to staff", ["Talks with staff", "Refuses to talk", "Not responding to verbal de-escalation"]),
        ("Peers nearby", ["Two", "Three", "Four, frightened"])], id="agg-flow")]),
]

agg_qs = [
hl_table(M(PSI, MILIEU, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that show escalation toward violence.",
   ["Finding", "Change"],
   [(["Speech", "Loud to screaming threats"], True, "Escalation."),
    (["Body", "Pacing to lifting a chair"], True, "Imminent violence."),
    (["Response", "No longer responds to talk"], True, "De-escalation failing."),
    (["Peers", "Frightened, nearby"], True, "Others at risk."),
    (["Trigger", "Denied phone call"], True, "An identifiable trigger.")],
   "Escalating threats, a weapon and failed de-escalation mean imminent danger.",
   "Recognize cues of escalating aggression."),

order(M(PSI, MILIEU, "analyze_cues", "difficult", MH, "psych"),
   "Place the interventions in order from least to most restrictive.",
   ["Calm verbal de-escalation and active listening",
    "Offer choices and a quiet space",
    "Offer an oral PRN medicine",
    "Seclusion",
    "Physical restraint"],
   "Use the least restrictive measure that keeps everyone safe.",
   "Analyze the least restrictive intervention."),

cloze(M(SIC, RESTRAINT, "prioritize_hypotheses", "difficult", MH, "psych mgmt"),
   "Complete the sentence by choosing from the lists of options.",
   "At 2015, the priority is {{b1}}, and restraint or seclusion is used {{b2}}.",
   [[("the safety of the client, peers and staff", True, "Imminent violence."),
     ("finishing the phone call discussion", False, "Too late."),
     ("documenting first", False, "Safety first."),
     ("waiting for the client to calm down alone", False, "Others at risk.")],
    [("only as a last resort, never as punishment or for convenience", True, "Legal and ethical rule."),
     ("whenever a client swears", False, "Not dangerous."),
     ("to teach a lesson", False, "Punitive."),
     ("because staff are busy", False, "Convenience.")]],
   "Safety first; restraint only as a last resort.",
   "Prioritize safety in aggression."),

sata(M(SIC, RESTRAINT, "generate_solutions", "difficult", MH, "psych mgmt"),
   "Which actions should the team take when placing the client in seclusion?",
   [("Use enough trained staff with one leader", True, "Safe show of force."),
    ("Move other clients away", True, "Protects peers."),
    ("Get a provider order within the required time", True, "Legal requirement."),
    ("Observe the client continuously", True, "Safety."),
    ("Tell the client it is a consequence for bad behavior", False, "Never punitive."),
    ("Leave the client's belt and shoelaces on", False, "Ligature risk.")],
   "Trained team, protect others, order and continuous observation.",
   "Generate a safe seclusion plan.",
   ex=[note("2020", "Client placed in seclusion. Lorazepam IM and haloperidol IM given per order.", id="agg-2020")]),

sata(M(SIC, RESTRAINT, "take_action", "difficult", MH, "psych mgmt"),
   "The client continues to bang the head on the wall and is placed in four-point restraints. Which actions should "
   "the nurse take?",
   [("Check circulation, skin and range of motion at least every 15 minutes", True, "Prevents injury."),
    ("Offer fluids and toileting regularly", True, "Basic needs."),
    ("Release restraints as soon as the client is calm", True, "Least time possible."),
    ("Position the client prone with weight on the back", False, "Asphyxia risk."),
    ("Leave the client alone to avoid stimulation", False, "Continuous observation is required.")],
   "Frequent checks, basic needs and release as soon as possible.",
   "Take action during restraint.",
   ex=[note("2030", "Head-banging in seclusion; forehead bleeding.", id="agg-2030")]),

matrix_mc(M(PSI, MILIEU, "evaluate_outcomes", "difficult", MH, "psych"),
   "The next morning, for each statement or finding, indicate whether it shows a good outcome or needs follow-up.",
   ["Good outcome", "Needs follow-up"],
   [("Restraints removed after 40 minutes; seclusion ended at 2200", 0, "Shortest time."),
    ("A debriefing with the client and staff", 0, "Learning and trust."),
    ("\"Next time I feel it building, I'll ask for the quiet room.\"", 0, "A coping plan."),
    ("Forehead laceration not yet assessed", 1, "Injury care."),
    ("Documentation missing the reason and the less restrictive measures tried", 1, "Legal requirement."),
    ("\"I'm still angry at that nurse and I'll get them later.\"", 1, "Ongoing threat.")],
   "Short restraint and debriefing; injury, documentation and threats need follow-up.",
   "Evaluate the aggression response.",
   ex=[note("Next morning", "Client calm at breakfast.", id="agg-am")]),
]

cases = [
    case("Borderline personality disorder with self-harm and splitting", bpd_tabs, bpd_qs),
    case("Functional neurological symptom disorder after a traumatic event", fnd_tabs, fnd_qs),
    case("Child with ADHD on a stimulant", adhd_tabs, adhd_qs),
    case("Escalating aggression with seclusion and restraint", agg_tabs, agg_qs),
]

BUILT = build(131, "mh-cases-bpd-conversion-adhd-aggression-standalone",
              "Mental health case studies: borderline personality disorder, functional neurological disorder, ADHD "
              "and escalating aggression, with four standalone items",
              items, cases, case_conversion=True)
