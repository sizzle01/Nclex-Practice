"""Batch 133 (case conversion, chunk 9: mental health part 2): four full NGN case studies (CASE-MAP.md,
book_topics.py).

Cases: Alzheimer disease with sundowning, cholinesterase inhibitor therapy and caregiver support; alcohol use
disorder with Wernicke encephalopathy, denial and relapse-prevention drugs (naltrexone, acamprosate, disulfiram);
electroconvulsive therapy for severe depression with catatonia; complicated grief in an older widower with suicide
risk. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management
of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
DEMENTIA = "dementia-and-delirium"
SUBSTANCE = "substance-use-disorders"
DEFENSE = "defense-mechanisms"
MOOD = "mood-disorders"
GRIEF = "grief-and-loss"
SUICIDE = "suicide-risk-and-safety"
PSYCH_MEDS = "psychiatric-medications"
NEURO_MEDS = "neurological-medications"
COMM = "therapeutic-communication"
CONSENT = "informed-consent"
POSTOP = "post-procedure-complications"
TEACH = "health-teaching-and-behavior-change"
HOME = "equipment-and-home-safety"
NUTR = "nutrition-and-therapeutic-diets"
COLLAB = "collaboration-and-referrals"

items = []

# ================================================================================================
# Case 1: Alzheimer disease with sundowning
# ================================================================================================
alz_tabs = [
    ("alz-notes", "Memory care notes", [note("Long-term care admission",
        "A 79-year-old client with moderate Alzheimer disease moved into memory care yesterday. Each afternoon "
        "around 1600 the client becomes restless, tries to leave to \"pick up the children from school\" and "
        "pushes staff away. The client takes donepezil 10 mg at night. A new order reads haloperidol 2 mg IM as "
        "needed for agitation.", id="alz-note")]),
    ("alz-data", "Assessment", [table("Assessment", ["Finding"], [
        ("MMSE", ["14/30"]),
        ("Sleep", ["Up most of the night"]),
        ("Bowels", ["No stool for 4 days"]),
        ("Urine", ["Not checked"]),
        ("Pain", ["Grimaces when standing; history of knee arthritis"]),
        ("Heart rate", ["52/min"])], id="alz-table")]),
]

alz_qs = [
hl_table(M(PSI, DEMENTIA, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that could be driving the afternoon agitation.",
   ["Finding", "Detail"],
   [(["Constipation", "4 days"], True, "Discomfort shows as agitation."),
    (["Pain", "Grimaces when standing"], True, "Untreated pain."),
    (["Sleep", "Up most of the night"], True, "Fatigue worsens sundowning."),
    (["New setting", "Moved yesterday"], True, "Relocation stress."),
    (["MMSE", "14/30"], False, "Describes severity, not a trigger.")],
   "Pain, constipation, poor sleep and relocation can trigger sundowning.",
   "Recognize cues of triggers for agitation in dementia."),

matrix_mc(M(PSI, DEMENTIA, "analyze_cues", "difficult", MH, "psych"),
   "For each feature, indicate whether it fits dementia or delirium.",
   ["Dementia", "Delirium"],
   [("Gradual decline over years", 0, "Dementia."),
    ("Sudden onset over hours to days", 1, "Delirium."),
    ("Attention fluctuates through the day", 1, "Delirium."),
    ("Usually reversible when the cause is treated", 1, "Delirium."),
    ("Alert, with memory loss", 0, "Dementia.")],
   "Onset, course and attention separate the two; delirium can sit on top of dementia.",
   "Analyze dementia versus delirium."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, because antipsychotics in older adults with dementia {{b2}}.",
   [[("look for and treat causes such as pain, constipation and infection", True, "Non-drug causes first."),
     ("give the haloperidol at 1600 every day", False, "Routine dosing is not appropriate."),
     ("apply a belt restraint to the chair", False, "Restraints increase injury and agitation."),
     ("move the client to a busier room", False, "Adds stimulation.")],
    [("carry a boxed warning for increased death and stroke", True, "FDA boxed warning."),
     ("are the first-line treatment", False, "Last resort."),
     ("improve memory", False, "They do not."),
     ("have no side effects", False, "Falls, sedation and EPS.")]],
   "Find causes first; antipsychotics raise mortality in dementia.",
   "Prioritize agitation in dementia."),

sata(M(PSI, DEMENTIA, "generate_solutions", "difficult", MH, "psych"),
   "Which interventions should the nurse include in the plan?",
   [("Scheduled acetaminophen for knee pain", True, "Treats a trigger."),
    ("A bowel regimen", True, "Treats a trigger."),
    ("A calm, well-lit room in the late afternoon", True, "Reduces sundowning."),
    ("A walking route and a meaningful activity at 1530", True, "Redirects energy."),
    ("Reorient firmly that the children are grown", False, "Arguing with the reality increases distress."),
    ("Daytime naps of 3 hours", False, "Worsens night waking.")],
   "Treat pain and constipation, adjust the environment and plan activity.",
   "Generate the dementia care plan.",
   ex=[note("Plan", "Haloperidol order discontinued after team review.", id="alz-plan")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "At 1600, the client stands at the exit saying, \"I have to get the children from school.\" Which responses are "
   "appropriate?",
   [("\"Tell me about your children.\"", True, "Validation."),
    ("\"Let's walk together and get a snack first.\"", True, "Redirection."),
    ("Approach from the front at eye level, speaking slowly", True, "Reduces fear."),
    ("\"Your children are adults now; you're confused.\"", False, "Confrontation."),
    ("Block the door and hold the client's arms", False, "Escalates.")],
   "Validate the feeling and redirect.",
   "Take action with validation and redirection.",
   ex=[note("1600", "Client pacing near the exit.", id="alz-1600")]),

matrix_mc(M(PPT, NEURO_MEDS, "evaluate_outcomes", "difficult", MH, "ph psych"),
   "At 2 weeks, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Agitation episodes down from daily to twice a week", 0, "Plan working."),
    ("Daily bowel movement", 0, "Constipation treated."),
    ("Heart rate 46/min with a near-fall", 1, "Donepezil can cause bradycardia and syncope."),
    ("Sleeping 6 hours at night", 0, "Improved."),
    ("Joins the afternoon music group", 0, "Engagement.")],
   "Agitation improved; bradycardia on donepezil needs review.",
   "Evaluate dementia care.",
   ex=[note("2 weeks", "Memantine added last week.", id="alz-2wk")]),
]

# ================================================================================================
# Case 2: Alcohol use disorder with Wernicke encephalopathy
# ================================================================================================
aud_tabs = [
    ("aud-notes", "Medical unit notes", [note("Medical unit",
        "A 52-year-old client is admitted with pancreatitis. The client drinks a liter of vodka daily, eats little "
        "and says, \"I don't have a problem. I can stop any time; my job just stresses me out.\" On day 2, an IV "
        "dextrose infusion is started for vomiting.", id="aud-note")]),
    ("aud-data", "Day 2 findings", [table("Day 2", ["Finding"], [
        ("Mental status", ["Confused, cannot recall the day"]),
        ("Eyes", ["Nystagmus; cannot move the eyes fully to the side"]),
        ("Gait", ["Wide-based, unsteady"]),
        ("CIWA-Ar", ["6"]),
        ("Magnesium", ["1.2 mg/dL"]),
        ("Thiamine given", ["Not yet"])], id="aud-table")]),
]

aud_qs = [
hl_table(M(PSI, SUBSTANCE, "recognize_cues", "moderate", MH, "psych ms"),
   "Highlight the findings that suggest Wernicke encephalopathy.",
   ["Finding", "Day 2"],
   [(["Confusion", "New"], True, "Part of the triad."),
    (["Eye movements", "Nystagmus, gaze palsy"], True, "Part of the triad."),
    (["Gait", "Ataxic"], True, "Part of the triad."),
    (["Dextrose without thiamine", "Yes"], True, "Glucose uses up remaining thiamine."),
    (["CIWA-Ar", "6"], False, "Low; withdrawal is not the cause.")],
   "Confusion, eye signs and ataxia after glucose without thiamine is Wernicke encephalopathy.",
   "Recognize cues of Wernicke encephalopathy."),

matrix_mc(M(PSI, DEFENSE, "analyze_cues", "difficult", MH, "psych"),
   "For each statement by the client, indicate the defense mechanism.",
   ["Denial", "Rationalization", "Projection"],
   [("\"I don't have a problem.\"", 0, "Refuses reality."),
    ("\"I drink because my job is stressful.\"", 1, "An excuse."),
    ("\"My wife is the one who drinks too much.\"", 2, "Attributes it to another."),
    ("\"I only drink what everyone else drinks.\"", 1, "Excuse."),
    ("\"The lab test is wrong.\"", 0, "Denial.")],
   "Denial and rationalization are common in alcohol use disorder.",
   "Analyze defense mechanisms in substance use."),

cloze(M(PPT, "parenteral-nutrition", "prioritize_hypotheses", "difficult", MH, "ph ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should {{b1}}, because untreated Wernicke encephalopathy can progress to {{b2}}.",
   [[("give IV thiamine now as prescribed and before more glucose", True, "Prevents progression."),
     ("increase the dextrose rate", False, "Worsens it."),
     ("give lorazepam", False, "Not withdrawal."),
     ("wait for an MRI", False, "Delays treatment.")],
    [("Korsakoff syndrome with permanent memory loss and confabulation", True, "Irreversible."),
     ("hepatic encephalopathy", False, "A different disease."),
     ("delirium tremens", False, "Withdrawal."),
     ("alcohol poisoning", False, "Not the issue.")]],
   "Thiamine before glucose prevents Korsakoff syndrome.",
   "Prioritize Wernicke encephalopathy."),

matrix_mc(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "For each relapse-prevention medicine, indicate its main caution.",
   ["Cannot be used with opioids", "Avoid with severe kidney disease", "Severe reaction with any alcohol"],
   [("Naltrexone", 0, "Opioid antagonist."),
    ("Acamprosate", 1, "Renally cleared."),
    ("Disulfiram", 2, "Flushing, vomiting, hypotension with alcohol, even in mouthwash.")],
   "Each drug has a key caution.",
   "Generate relapse-prevention choices.",
   ex=[note("Day 5", "Confusion improving on thiamine. Lipase falling. Client agrees to talk about drinking.", id="aud-day5")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The client says, \"Maybe I drink a bit too much.\" Which responses use motivational interviewing?",
   [("\"What worries you most about your drinking?\"", True, "Open question."),
    ("\"From 0 to 10, how ready are you?\"", True, "Readiness ruler."),
    ("\"Part of you wants to change.\"", True, "Reflects ambivalence."),
    ("\"You're an alcoholic, and the first step is that you need to admit it.\"", False, "Labeling causes resistance."),
    ("\"If you keep drinking like this, your pancreas will fail and you will die.\"", False, "Confrontation.")],
   "Open questions, readiness and reflecting ambivalence.",
   "Take action with motivational interviewing.",
   ex=[note("Day 6", "Client asks what treatment options exist.", id="aud-day6")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", MH, "ph psych"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll take thiamine every day.\"", 0, "Correct."),
    ("\"I'll go to my first AA meeting on Thursday.\"", 0, "Support."),
    ("\"On naltrexone, I can't take my old oxycodone for my back.\"", 0, "Correct."),
    ("\"I'll start disulfiram tonight after my last beer.\"", 1, "Needs at least 12 hours without alcohol."),
    ("\"I'll avoid cough syrups and mouthwash with alcohol if I take disulfiram.\"", 0, "Correct.")],
   "Thiamine, support groups and correct medicine use.",
   "Evaluate alcohol use disorder teaching.",
   ex=[note("Discharge", "Naltrexone prescribed; disulfiram discussed as an alternative.", id="aud-dc")]),
]

# ================================================================================================
# Case 3: Electroconvulsive therapy
# ================================================================================================
ect_tabs = [
    ("ect-notes", "Mental health unit notes", [note("Mental health unit",
        "A 67-year-old client with severe major depression with catatonia has stopped eating and drinking for 4 "
        "days, lies mute and rigid, and holds odd postures. Two antidepressant trials failed. Lorazepam IV gave "
        "only brief improvement. ECT is recommended.", id="ect-note")]),
    ("ect-data", "Findings", [table("Findings", ["Result"], [
        ("Intake", ["Less than 300 mL a day"]),
        ("Sodium", ["149 mEq/L"]),
        ("Creatinine", ["1.7 mg/dL (baseline 0.9)"]),
        ("Weight", ["Down 5 kg in 3 weeks"]),
        ("Skin", ["Reddened sacrum"]),
        ("Capacity", ["Unable to discuss treatment"])], id="ect-table")]),
]

ect_qs = [
hl_table(M(PSI, MOOD, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that make this depression life-threatening.",
   ["Finding", "Result"],
   [(["Intake", "Under 300 mL a day"], True, "Dehydration."),
    (["Sodium and creatinine", "149, 1.7"], True, "Dehydration and kidney injury."),
    (["Weight", "Down 5 kg"], True, "Starvation."),
    (["Skin", "Reddened sacrum"], True, "Immobility."),
    (["Capacity", "Unable to discuss"], False, "Affects consent, not the severity itself.")],
   "Refusing food and fluids with organ effects makes this an emergency.",
   "Recognize cues of life-threatening depression."),

matrix_mc(M(PSI, MOOD, "analyze_cues", "difficult", MH, "psych"),
   "For each statement, indicate whether it is accurate or inaccurate about ECT.",
   ["Accurate", "Inaccurate"],
   [("It works quickly in catatonia and severe depression", 0, "Fast response."),
    ("It is done under brief general anesthesia with a muscle relaxant", 0, "Modern ECT."),
    ("Short-term memory loss is the most common side effect", 0, "Usually improves."),
    ("It is a painful punishment for refusing treatment", 1, "Untrue."),
    ("A recent stroke or raised intracranial pressure is a major concern", 0, "High-risk conditions.")],
   "ECT is a safe, fast treatment with mainly memory effects.",
   "Analyze ECT."),

cloze(M(MOC, CONSENT, "prioritize_hypotheses", "difficult", MH, "mgmt psych"),
   "Complete the sentence by choosing from the lists of options.",
   "Because the client lacks capacity, consent for ECT comes from {{b1}}, and before the first session the nurse "
   "must {{b2}}.",
   [[("a legally authorized representative or court, as state law requires", True, "Substitute decision."),
     ("the nurse", False, "Not allowed."),
     ("any family member who visits", False, "Must be legally authorized."),
     ("no one; it is an emergency", False, "Consent is still required.")],
    [("confirm signed consent and NPO status", True, "Pre-procedure."),
     ("give a large breakfast", False, "Aspiration risk."),
     ("give the evening lorazepam dose as usual", False, "Benzodiazepines raise the seizure threshold."),
     ("remove the ID band", False, "Needed.")]],
   "Legal consent and pre-procedure checks.",
   "Prioritize ECT consent and preparation."),

sata(M(PSI, MOOD, "generate_solutions", "difficult", MH, "psych ms"),
   "Which parts of the plan should the nurse include for ECT days?",
   [("Nothing by mouth from midnight", True, "Anesthesia."),
    ("Remove dentures and jewelry", True, "Safety."),
    ("Empty the bladder before the procedure", True, "Prevents incontinence."),
    ("IV fluids and nutrition support between sessions", True, "Dehydration."),
    ("Hold all morning medicines without checking with the team", False, "Some are given with sips."),
    ("Expect ECT once only", False, "Usually a course of 6 to 12.")],
   "NPO, safety preparation and medical support.",
   "Generate the ECT plan.",
   ex=[note("Orders", "ECT three times weekly. Court authorization obtained.", id="ect-orders")]),

sata(M(RRP, POSTOP, "take_action", "difficult", MH, "psych ms"),
   "After the first session, the client is confused in recovery. Which actions should the nurse take?",
   [("Side-lying position until fully awake", True, "Airway."),
    ("Check vital signs and oxygen saturation", True, "Recovery."),
    ("Reorient calmly and often", True, "Post-ECT confusion."),
    ("Raise side rails and supervise the first walk", True, "Falls."),
    ("Offer breakfast while still drowsy", False, "Aspiration."),
    ("Tell the client the confusion means brain damage", False, "Untrue and frightening.")],
   "Airway, vital signs, reorientation and fall safety.",
   "Take action after ECT.",
   ex=[note("Recovery", "Awake 20 minutes after the session; asks where they are.", id="ect-recovery")]),

matrix_mc(M(PSI, MOOD, "evaluate_outcomes", "difficult", MH, "psych"),
   "After 6 sessions, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Eating 75% of meals", 0, "Recovering."),
    ("Talking and walking", 0, "Catatonia resolved."),
    ("Creatinine 1.0 mg/dL", 0, "Kidneys recovered."),
    ("Cannot remember the last two weeks", 1, "Expected but should be monitored and explained."),
    ("\"I feel hopeless at night and wish I could sleep forever.\"", 1, "Suicide risk can rise as energy returns.")],
   "Recovery under way; memory and suicide risk need follow-up.",
   "Evaluate ECT response.",
   ex=[note("Week 2", "Six sessions completed. Sertraline restarted.", id="ect-wk2")]),
]

# ================================================================================================
# Case 4: Complicated grief in an older widower
# ================================================================================================
grief_tabs = [
    ("grief-notes", "Primary care notes", [note("Clinic visit",
        "An 81-year-old client's wife of 55 years died 14 months ago. The client still sets her place at the table, "
        "has stopped seeing friends and says, \"There's no point going on without her.\" The client lives alone, "
        "drinks whiskey each night and keeps a hunting rifle.", id="grief-note")]),
    ("grief-data", "Screening", [table("Screening", ["Result"], [
        ("PHQ-9", ["19"]),
        ("Weight", ["Down 7 kg"]),
        ("Sleep", ["Wakes at 3 am"]),
        ("Columbia scale", ["Wish to be dead; thinking about how"]),
        ("Support", ["Daughter lives 2 hours away"])], id="grief-table")]),
]

grief_qs = [
hl_table(M(PSI, SUICIDE, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that raise suicide risk.",
   ["Finding", "Detail"],
   [(["Older man living alone, widowed", "Yes"], True, "Highest-risk group."),
    (["Firearm at home", "Rifle"], True, "Lethal means."),
    (["Alcohol", "Nightly whiskey"], True, "Disinhibition."),
    (["Thoughts of how", "Yes"], True, "Planning."),
    (["Social withdrawal", "Stopped seeing friends"], True, "Isolation.")],
   "Older widowed men with firearms and alcohol have very high suicide rates.",
   "Recognize cues of suicide risk in an older adult."),

matrix_mc(M(PSI, GRIEF, "analyze_cues", "difficult", MH, "psych"),
   "For each feature, indicate whether it fits normal grief or prolonged (complicated) grief with depression.",
   ["Normal grief", "Prolonged grief or depression"],
   [("Waves of sadness triggered by reminders, with good days between", 0, "Normal."),
    ("Intense yearning more than a year later that stops daily life", 1, "Prolonged grief."),
    ("Persistent worthlessness and wish to die", 1, "Depression."),
    ("Laughing at memories with family", 0, "Normal."),
    ("Still setting her place at the table after 14 months", 1, "Inability to accept the loss.")],
   "Duration, impairment and hopelessness separate the two.",
   "Analyze grief."),

cloze(M(PSI, SUICIDE, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority is {{b1}}, which includes {{b2}}.",
   [[("immediate safety", True, "Active ideation with means."),
     ("grief counseling next month", False, "Too slow."),
     ("a sleep aid", False, "Not the priority."),
     ("a dietitian referral", False, "Not the priority.")],
    [("a same-day crisis evaluation and removing the rifle from the home", True, "Means restriction."),
     ("telling him time heals", False, "Dismissive."),
     ("asking him to promise not to hurt himself", False, "Not protective."),
     ("sending him home with a pamphlet", False, "Unsafe.")]],
   "Safety and means restriction come first.",
   "Prioritize suicide risk in grief."),

sata(M(PSI, GRIEF, "generate_solutions", "difficult", MH, "psych"),
   "After the crisis evaluation, the client agrees to outpatient care. Which parts of the plan should the nurse "
   "include?",
   [("Complicated grief therapy or a bereavement group", True, "Targeted therapy."),
    ("Antidepressant treatment for the major depression", True, "Treats depression."),
    ("Daughter stores the rifle at her home", True, "Means safety."),
    ("Senior center activities and a weekly check-in call", True, "Reduces isolation."),
    ("Advise him to clear out all her belongings this week", False, "Forced; may deepen distress."),
    ("A nightly drink to help sleep", False, "Worsens mood and risk.")],
   "Therapy, treatment, means safety and connection.",
   "Generate the grief plan.",
   ex=[note("Crisis team", "Daughter collected the rifle today. Safety plan written.", id="grief-crisis")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "The client says, \"Everyone says I should be over it by now.\" Which responses are therapeutic?",
   [("\"Grief has no timetable. Tell me about her.\"", True, "Validates and invites."),
    ("\"You feel pressured and alone.\"", True, "Reflects feelings."),
    ("\"She's in a better place now, and she would want you to be happy.\"", False, "A cliché that closes the conversation."),
    ("\"You need to move on for your daughter's sake.\"", False, "Pressures."),
    ("\"I know exactly how you feel; I lost my own father last year.\"", False, "Presumes.")],
   "Validate and explore.",
   "Take action with grief communication.",
   ex=[note("Week 2", "First therapy session attended.", id="grief-wk2")]),

matrix_mc(M(PSI, GRIEF, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("PHQ-9 9", 0, "Improved."),
    ("Joins a widowers' group each week", 0, "Connection."),
    ("\"I visited her grave and told her about the grandkids.\"", 0, "Healthy continuing bond."),
    ("\"I bought a new gun for protection.\"", 1, "Means safety must be revisited."),
    ("Weight up 3 kg", 0, "Eating better.")],
   "Progress; a new firearm needs urgent review.",
   "Evaluate grief treatment.",
   ex=[note("3 months", "Sertraline 50 mg.", id="grief-3m")]),
]

cases = [
    case("Alzheimer disease with sundowning", alz_tabs, alz_qs),
    case("Alcohol use disorder with Wernicke encephalopathy", aud_tabs, aud_qs),
    case("Electroconvulsive therapy for catatonic depression", ect_tabs, ect_qs),
    case("Complicated grief in an older widower", grief_tabs, grief_qs),
]

BUILT = build(133, "mh-cases-dementia-aud-ect-grief",
              "Mental health case studies: Alzheimer sundowning, alcohol use disorder with Wernicke encephalopathy, "
              "ECT and complicated grief",
              items, cases, case_conversion=True)
