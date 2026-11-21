"""Batch 128 (case conversion, chunk 8: mental health disorders with psychiatric medications): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: major depression with rising suicide risk early in SSRI treatment; bipolar mania with lithium toxicity;
schizophrenia with neuroleptic malignant syndrome on haloperidol; treatment-resistant schizophrenia on clozapine with
neutropenia and constipation. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6, and
includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
MOOD = "mood-disorders"
SUICIDE = "suicide-risk-and-safety"
PSYCHOTIC = "psychotic-disorders"
PSYCH_MEDS = "psychiatric-medications"
COMM = "therapeutic-communication"
MILIEU = "behavior-management-and-milieu"
CRISIS = "crisis-intervention"
RIGHTS = "client-rights-and-advocacy"
LEGAL = "legal-and-ethical-practice"
LYTES = "electrolyte-imbalances"
LABS = "laboratory-values-and-monitoring"
RESTRAINT = "restraint-safety"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
NEUTRO = "neutropenic-precautions"
NUTR = "nutrition-and-therapeutic-diets"

items = []

# ================================================================================================
# Case 1: Major depression with suicide risk early in SSRI treatment
# ================================================================================================
dep_tabs = [
    ("dep-notes", "Clinic and unit notes", [note("Admission to the mental health unit",
        "A 19-year-old college student with major depressive disorder started sertraline 12 days ago. Today the "
        "student's roommate brought the student in after finding a goodbye note. The student says, \"I have more "
        "energy now, and I finally know what I need to do.\" The student has given away a guitar and a laptop this "
        "week.", id="dep-note")]),
    ("dep-data", "Assessment", [table("Assessment", ["Finding"], [
        ("Plan", ["Will take a bottle of the roommate's pills"]),
        ("Access to means", ["Roommate's oxycodone in the dorm"]),
        ("Previous attempt", ["One at age 16"]),
        ("Columbia scale", ["Positive for intent and preparatory behavior"]),
        ("Mood", ["\"Calm\"; affect flat"]),
        ("Sleep", ["3 to 4 hours a night"]),
        ("Alcohol", ["Drinking alone most nights for 2 weeks"])], id="dep-table")]),
]

dep_qs = [
hl_table(M(PSI, SUICIDE, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that raise this client's immediate suicide risk.",
   ["Finding", "Detail"],
   [(["Plan and means", "Pills; access to oxycodone"], True, "A specific plan with access to lethal means."),
    (["Giving away belongings", "Guitar, laptop"], True, "Preparatory behavior."),
    (["Sudden calm and energy", "Day 12 of sertraline"], True, "Energy returns before mood lifts; a decision may have been made."),
    (["Previous attempt", "Age 16"], True, "The strongest predictor."),
    (["Alcohol", "Drinking alone"], True, "Disinhibition."),
    (["Age", "19 years"], True, "Antidepressants carry a boxed warning for suicidality under 25.")],
   "A plan, means, prior attempt, alcohol and sudden calm early in treatment signal high risk.",
   "Recognize cues of high suicide risk."),

matrix_mc(M(PSI, SUICIDE, "analyze_cues", "difficult", MH, "psych"),
   "For each statement, indicate whether it is accurate or inaccurate about suicide risk.",
   ["Accurate", "Inaccurate"],
   [("Asking directly about suicide does not plant the idea", 0, "Direct questions are safe and expected."),
    ("Risk can rise in the first weeks of antidepressant treatment", 0, "Energy improves before mood."),
    ("A sudden improvement in mood can mean a decision to die", 0, "A warning sign."),
    ("A no-suicide contract makes the client safe", 1, "Contracts do not prevent suicide."),
    ("Clients who talk about suicide rarely act on it", 1, "Most give warnings.")],
   "Direct questions are safe; contracts are not protective.",
   "Analyze suicide risk myths and facts."),

cloze(M(PSI, SUICIDE, "prioritize_hypotheses", "difficult", MH, "psych"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority nursing problem is {{b1}}, so the client needs {{b2}}.",
   [[("imminent risk of suicide", True, "Plan, intent, means."),
     ("sleep disturbance", False, "Important but secondary."),
     ("social isolation", False, "Secondary."),
     ("knowledge deficit about sertraline", False, "Secondary.")],
    [("one-to-one observation within arm's length", True, "The highest level of observation."),
     ("checks every 30 minutes", False, "Not enough for imminent risk."),
     ("a private room with the door closed", False, "Unsafe."),
     ("an outpatient appointment next week", False, "Unsafe.")]],
   "Imminent risk needs continuous close observation.",
   "Prioritize suicide risk."),

sata(M(SIC, "equipment-and-home-safety", "generate_solutions", "difficult", MH, "psych"),
   "Which interventions should the nurse include on admission?",
   [("Search belongings for items that could be used for self-harm", True, "Removes means."),
    ("Remove belts, cords and shoelaces", True, "Ligature risk."),
    ("Check that the client swallows each medicine", True, "Prevents hoarding."),
    ("Ask the client to promise to tell staff before acting", False, "A no-harm contract is not a safety measure."),
    ("Allow a private bathroom with a locked door for dignity", False, "Unsafe for imminent risk.")],
   "Environmental safety, removal of means and medication checks.",
   "Generate the safety plan.",
   ex=[note("Orders", "One-to-one observation. Sertraline continued. Collaborative safety plan before any discharge.", id="dep-orders")]),

sata(M(PSI, COMM, "take_action", "difficult", MH, "psych"),
   "On day 2, the client says, \"Everyone would be better off without me.\" Which responses by the nurse are "
   "therapeutic?",
   [("\"Are you thinking about killing yourself right now?\"", True, "Direct assessment."),
    ("\"You feel like a burden to the people around you.\"", True, "Reflects the feeling."),
    ("\"I'm going to stay with you. Tell me more about that.\"", True, "Presence and exploration."),
    ("\"Don't say that. Your family loves you very much.\"", False, "Dismisses the feeling."),
    ("\"Why would you think something like that about yourself?\"", False, "\"Why\" questions put the client on the defensive.")],
   "Ask directly, reflect and stay present.",
   "Take action with therapeutic communication.",
   ex=[note("Day 2", "Client sitting alone in the dayroom; one-to-one staff present.", id="dep-day2")]),

matrix_mc(M(PSI, SUICIDE, "evaluate_outcomes", "difficult", MH, "psych"),
   "On day 9, the team considers discharge. For each finding, indicate whether it supports discharge or needs "
   "follow-up first.",
   ["Supports discharge", "Needs follow-up"],
   [("Can name warning signs and three people to call", 0, "Part of the safety plan."),
    ("Roommate has removed the oxycodone from the dorm", 0, "Means restriction."),
    ("Sleeping 7 hours a night", 0, "Improved."),
    ("\"I'll be fine once I'm out. I don't need the crisis line number.\"", 1, "Minimizes risk; needs more work."),
    ("Outpatient appointment within 7 days of discharge", 0, "The period after discharge is high risk."),
    ("Says the thoughts come but leave quickly and has no plan", 0, "Lower risk; continue monitoring.")],
   "A safety plan, means restriction and early follow-up support discharge; refusing the crisis plan does not.",
   "Evaluate readiness for discharge after suicide risk.",
   ex=[note("Day 9", "Mood rating 5/10 (was 2/10). Attends groups.", id="dep-day9")]),
]

# ================================================================================================
# Case 2: Bipolar mania with lithium toxicity
# ================================================================================================
li_tabs = [
    ("li-notes", "Unit notes", [note("Mental health unit, day 6",
        "A 42-year-old client admitted with acute mania takes lithium 900 mg twice daily. The client had a stomach "
        "virus with vomiting and diarrhea for 2 days and started ibuprofen for a sore back. Today the client is "
        "unsteady and has slurred speech.", id="li-note")]),
    ("li-data", "Findings", [flow("Findings", ["Day 3", "Day 6"], [
        ("Lithium level", ["0.9 mEq/L", "2.3 mEq/L"]),
        ("Sodium", ["139", "131 mEq/L"]),
        ("Creatinine", ["0.9", "1.6 mg/dL"]),
        ("Tremor", ["Fine hand tremor", "Coarse tremor, muscle twitching"]),
        ("Gait", ["Steady", "Ataxic"]),
        ("Mental status", ["Pressured speech, grandiose", "Confused, drowsy"])], id="li-flow")]),
]

li_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph psych"),
   "Highlight the day 6 findings that require immediate action.",
   ["Finding", "Day 6"],
   [(["Lithium level", "2.3 mEq/L"], True, "Toxic; therapeutic is about 0.6 to 1.2."),
    (["Tremor", "Coarse, with twitching"], True, "Neurotoxicity."),
    (["Gait", "Ataxic"], True, "Toxicity."),
    (["Mental status", "Confused, drowsy"], True, "Severe toxicity."),
    (["Creatinine", "1.6 mg/dL"], True, "Reduced clearance.")],
   "A level of 2.3 with neurologic signs is serious lithium toxicity.",
   "Recognize cues of lithium toxicity."),

matrix_mr(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each factor, select how it raised the lithium level. Each row may have more than one answer.",
   ["Lowers sodium or fluid volume", "Reduces kidney clearance"],
   [("Vomiting and diarrhea", {0, 1}, "Dehydration and sodium loss make the kidneys hold lithium."),
    ("Ibuprofen", {1}, "NSAIDs reduce lithium clearance."),
    ("Low sodium of 131", {0}, "Kidneys retain lithium with sodium."),
    ("Rising creatinine", {1}, "Less clearance.")],
   "Dehydration, low sodium and NSAIDs all raise lithium.",
   "Analyze causes of lithium toxicity."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should first {{b1}}, and the provider is likely to order {{b2}}.",
   [[("hold the next lithium dose and notify the provider", True, "Stops more drug."),
     ("give the dose with food", False, "Adds to toxicity."),
     ("give the PRN haloperidol for confusion", False, "Masks and adds risk."),
     ("encourage the client to rest", False, "Delays care.")],
    [("IV 0.9% sodium chloride and preparation for hemodialysis if it worsens", True, "Restores volume; dialysis removes lithium."),
     ("activated charcoal", False, "Does not bind lithium."),
     ("a low-sodium diet", False, "Raises lithium."),
     ("furosemide", False, "Worsens volume loss.")]],
   "Hold lithium, give saline, and dialyze severe cases.",
   "Prioritize lithium toxicity."),

matrix_mc(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "For each lithium level, indicate the expected meaning.",
   ["Therapeutic for maintenance", "Early toxicity", "Severe toxicity"],
   [("0.8 mEq/L with a fine tremor", 0, "Fine tremor is a common side effect."),
    ("1.6 mEq/L with vomiting and diarrhea", 1, "GI signs come first."),
    ("2.3 mEq/L with ataxia and confusion", 2, "Neurologic signs."),
    ("3.1 mEq/L with seizures", 2, "Life-threatening.")],
   "Toxicity moves from GI signs to neurologic signs as the level rises.",
   "Generate interpretations of lithium levels.",
   ex=[note("Orders", "Hold lithium. 0.9% sodium chloride 150 mL/hr. Repeat level in 6 hours. Nephrology consult.", id="li-orders")]),

sata(M(SIC, "safe-client-handling", "take_action", "difficult", MH, "ph psych"),
   "Which actions should the nurse take to keep the client safe while toxic?",
   [("Fall precautions", True, "Ataxia."),
    ("Seizure precautions", True, "Severe toxicity can cause seizures."),
    ("Strict intake and output", True, "Fluid status and kidney function."),
    ("Neurologic checks", True, "Tracks progression."),
    ("Allow the client to walk alone to the group room", False, "Ataxia."),
    ("Restrict fluids to 1 liter a day", False, "Worsens dehydration.")],
   "Falls, seizures and fluid status need close attention.",
   "Take action for safety in lithium toxicity.",
   ex=[note("6 hours", "Lithium 1.9 mEq/L. Still unsteady.", id="li-6h")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", MH, "ph"),
   "At discharge, for each statement, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll drink 2 to 3 liters of water a day and more in hot weather.\"", 0, "Prevents dehydration."),
    ("\"I'll keep my salt intake about the same each day.\"", 0, "Stable sodium keeps levels stable."),
    ("\"If I get vomiting or diarrhea, I'll call my provider.\"", 0, "Risk of toxicity."),
    ("\"Ibuprofen is fine for my back pain.\"", 1, "NSAIDs raise lithium; acetaminophen is preferred."),
    ("\"I'll get my thyroid and kidney blood tests as scheduled.\"", 0, "Lithium affects both."),
    ("\"I'll start a low-salt diet to lose weight.\"", 1, "Low sodium raises lithium.")],
   "Steady fluids and salt, avoid NSAIDs and keep lab tests.",
   "Evaluate lithium teaching.",
   ex=[note("Day 10", "Lithium restarted at a lower dose. Level 0.8 mEq/L.", id="li-day10")]),
]

# ================================================================================================
# Case 3: Neuroleptic malignant syndrome
# ================================================================================================
nms_tabs = [
    ("nms-notes", "Unit notes", [note("Mental health unit",
        "A 34-year-old client with schizophrenia was admitted 4 days ago with agitation and received haloperidol "
        "IM several times plus oral haloperidol. Today the client is stiff, sweaty and not answering questions.",
        id="nms-note")]),
    ("nms-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["40.2 C (104.4 F)"]),
        ("Muscle tone", ["Lead-pipe rigidity in all limbs"]),
        ("Blood pressure", ["168/102, then 132/70 an hour later"]),
        ("Heart rate", ["128/min"]),
        ("Mental status", ["Stuporous, mute"]),
        ("CK", ["14,600 units/L"]),
        ("WBC", ["15,400/mm3"])], id="nms-table")]),
]

nms_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph psych"),
   "Highlight the findings that suggest neuroleptic malignant syndrome (NMS).",
   ["Finding", "Result"],
   [(["Temperature", "40.2 C"], True, "Hyperthermia."),
    (["Muscle tone", "Lead-pipe rigidity"], True, "Severe rigidity."),
    (["Blood pressure", "Swinging"], True, "Autonomic instability."),
    (["Mental status", "Stuporous"], True, "Altered consciousness."),
    (["CK", "14,600 units/L"], True, "Muscle breakdown.")],
   "Fever, rigidity, autonomic instability and altered mental status after an antipsychotic is NMS.",
   "Recognize cues of NMS."),

matrix_mc(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each feature, indicate whether it favors NMS or serotonin syndrome.",
   ["NMS", "Serotonin syndrome"],
   [("Onset over days after a dopamine blocker", 0, "Slow."),
    ("Onset within hours of a serotonergic drug", 1, "Fast."),
    ("Lead-pipe rigidity with slow reflexes", 0, "NMS."),
    ("Clonus and hyperreflexia, worse in the legs", 1, "Serotonin syndrome."),
    ("Diarrhea and dilated pupils", 1, "Serotonin syndrome.")],
   "Timing, rigidity versus clonus and drug class separate the two.",
   "Analyze NMS versus serotonin syndrome."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first action is to {{b1}}, and the provider may order {{b2}}.",
   [[("stop the haloperidol and call for urgent medical help", True, "Remove the cause."),
     ("give another haloperidol dose for agitation", False, "Worsens NMS."),
     ("give benztropine and continue the haloperidol", False, "Not enough."),
     ("restrain the client", False, "Worsens hyperthermia and muscle breakdown.")],
    [("dantrolene or bromocriptine with cooling and IV fluids", True, "Treats rigidity and dopamine blockade."),
     ("a second antipsychotic", False, "Dangerous."),
     ("an SSRI", False, "Not relevant."),
     ("lithium", False, "Increases NMS risk.")]],
   "Stop the antipsychotic, cool, hydrate and give specific drugs.",
   "Prioritize NMS treatment."),

sata(M(PA, "fluid-volume-imbalances", "generate_solutions", "difficult", MH, "ms ph"),
   "The client is moved to the ICU. Which interventions should the nurse anticipate?",
   [("Cooling blankets and ice packs", True, "Hyperthermia."),
    ("IV fluids with urine output monitoring", True, "Protects kidneys from myoglobin."),
    ("Cardiac monitoring", True, "Autonomic instability."),
    ("Aspiration precautions", True, "Rigidity and stupor."),
    ("Acetaminophen as the main way to lower the temperature", False, "Central fever does not respond well."),
    ("Restart haloperidol at a lower dose tomorrow", False, "Wait at least 2 weeks and choose another agent.")],
   "Cooling, fluids, monitoring and aspiration precautions.",
   "Generate the NMS plan.",
   ex=[note("ICU", "Dantrolene IV started. Creatinine 1.8 mg/dL.", id="nms-icu")]),

sata(M(MOC, LEGAL, "take_action", "difficult", MH, "mgmt psych"),
   "The client was admitted involuntarily. In the ICU, the client remains confused. Which actions are appropriate?",
   [("Continue emergency treatment of NMS", True, "Emergency treatment is allowed to save life."),
    ("Involve the legally authorized representative in decisions", True, "Client lacks capacity."),
    ("Document the client's capacity assessment", True, "Legal record."),
    ("Assume involuntary admission removes all rights to refuse treatment", False, "Clients keep rights; refusal is handled through legal processes."),
    ("Delay NMS treatment until a court order is obtained", False, "Emergencies are treated.")],
   "Emergency care proceeds; involuntary status does not remove rights.",
   "Take action on legal rights in mental health.",
   ex=[note("ICU day 2", "Client still confused; sister is the legal guardian.", id="nms-guardian")]),

matrix_mc(M(PPT, PSYCH_MEDS, "evaluate_outcomes", "difficult", MH, "ph"),
   "On day 7, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("Temperature 37.1 C", 0, "Resolved."),
    ("CK 1,200 units/L", 0, "Falling."),
    ("Rigidity gone", 0, "Resolved."),
    ("Hearing voices again, pacing", 1, "Psychosis needs a safer antipsychotic plan."),
    ("Creatinine 1.0 mg/dL", 0, "Kidneys recovered."),
    ("Chart lists haloperidol as an adverse reaction", 0, "Prevents re-exposure.")],
   "NMS resolved; the psychosis still needs treatment with a careful choice.",
   "Evaluate NMS recovery.",
   ex=[note("Day 7", "Dantrolene stopped.", id="nms-day7")]),
]

# ================================================================================================
# Case 4: Clozapine for treatment-resistant schizophrenia
# ================================================================================================
cloz_tabs = [
    ("cloz-notes", "Clinic notes", [note("Clozapine clinic",
        "A 29-year-old client with treatment-resistant schizophrenia started clozapine 10 weeks ago. Voices have "
        "quieted for the first time in years. Today the client reports a sore throat and has not had a bowel "
        "movement in 5 days. The client smokes 20 cigarettes a day and plans to quit.", id="cloz-note")]),
    ("cloz-data", "Results", [table("Results", ["Today", "Last week"], [
        ("ANC", ["900/mm3", "2,400/mm3"]),
        ("Temperature", ["38.2 C", "36.8 C"]),
        ("Abdomen", ["Distended, few bowel sounds", "Soft"]),
        ("Heart rate", ["112/min", "96/min"]),
        ("Drooling at night", ["Yes", "Yes"])], id="cloz-table")]),
]

cloz_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the findings that require same-day action.",
   ["Finding", "Today"],
   [(["ANC", "900/mm3"], True, "Moderate neutropenia on clozapine."),
    (["Temperature and sore throat", "38.2 C"], True, "Infection with neutropenia."),
    (["Abdomen", "Distended, few bowel sounds, no stool for 5 days"], True, "Clozapine can cause fatal bowel obstruction."),
    (["Heart rate", "112/min"], True, "Infection, or myocarditis in early treatment."),
    (["Drooling at night", "Yes"], False, "A common, bothersome side effect.")],
   "Neutropenia with fever and severe constipation are clozapine emergencies.",
   "Recognize cues of clozapine adverse effects."),

matrix_mc(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each effect of clozapine, indicate how serious it is.",
   ["Potentially fatal", "Common but not usually dangerous"],
   [("Severe neutropenia (agranulocytosis)", 0, "Fatal infections."),
    ("Myocarditis in the first 2 months", 0, "Fatal heart damage."),
    ("Bowel obstruction from slowed gut", 0, "Can cause death."),
    ("Seizures at higher doses", 0, "Dose-related."),
    ("Nighttime drooling", 1, "Bothersome."),
    ("Weight gain", 1, "Long-term metabolic risk.")],
   "Clozapine has several life-threatening effects.",
   "Analyze clozapine risks."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "With an ANC of 900 and a fever, the nurse should {{b1}}, and the client needs {{b2}}.",
   [[("hold clozapine and notify the prescriber today", True, "Moderate neutropenia with fever."),
     ("double the clozapine to keep the voices away", False, "Dangerous."),
     ("continue clozapine and recheck in a month", False, "Too slow."),
     ("switch to a laxative only", False, "Ignores the neutropenia.")],
    [("urgent assessment for infection and bowel obstruction", True, "Both are emergencies."),
     ("a routine visit next month", False, "Too slow."),
     ("a sleep study", False, "Not relevant."),
     ("a dental cleaning", False, "Not relevant.")]],
   "Hold clozapine and assess for infection and obstruction.",
   "Prioritize clozapine emergencies."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "Which parts of the long-term clozapine plan should the nurse include?",
   [("Regular ANC monitoring per the schedule", True, "Neutropenia detection."),
    ("A bowel regimen with a stimulant laxative", True, "Prevents obstruction."),
    ("Weight, glucose and lipids", True, "Metabolic effects."),
    ("Tell the prescriber before stopping smoking", True, "Smoking lowers clozapine levels; quitting can cause toxicity."),
    ("Use bulk-forming fiber alone for constipation", False, "Can worsen obstruction with slow transit."),
    ("Stop clozapine for 2 weeks if a dose is missed and restart at the old dose", False, "Must be re-titrated after missed days.")],
   "ANC checks, a bowel plan, metabolic checks and alerting the prescriber about smoking changes.",
   "Generate the clozapine plan.",
   ex=[note("Hospital", "Admitted. Clozapine held. Abdominal x-ray: dilated colon, no obstruction yet. Blood cultures sent.", id="cloz-admit")]),

sata(M(MOC, "collaboration-and-referrals", "take_action", "difficult", MH, "mgmt psych"),
   "The client is admitted to a medical unit where staff are unfamiliar with clozapine. Which actions should the "
   "nurse take?",
   [("Tell the team that missed doses longer than 48 hours require re-titration", True, "Seizure and hypotension risk."),
    ("Ask for a psychiatry consult", True, "Specialist plan."),
    ("Include the clozapine risks in the handoff report", True, "Safe communication."),
    ("Let the client restart the home dose without checking", False, "Unsafe after a break."),
    ("Leave the psychiatric history out of the handoff for privacy", False, "Needed for safe care.")],
   "Communicate clozapine risks and involve psychiatry.",
   "Take action to coordinate clozapine care.",
   ex=[note("Day 2", "ANC 1,500/mm3; fever settled on antibiotics. Bowels opened with an enema and senna.", id="cloz-day2")]),

matrix_mc(M(PSI, PSYCHOTIC, "evaluate_outcomes", "difficult", MH, "psych ph"),
   "At follow-up, for each finding or statement, indicate whether it shows a good outcome or needs follow-up.",
   ["Good outcome", "Needs follow-up"],
   [("ANC 2,600/mm3 after restarting clozapine", 0, "Recovered."),
    ("Daily bowel movement on senna", 0, "Bowel plan working."),
    ("\"The voices are quiet and I started a job.\"", 0, "Functional recovery."),
    ("\"I quit smoking last week and feel very sleepy and dizzy.\"", 1, "Clozapine level rising after quitting."),
    ("Weight up 6 kg in 3 months", 1, "Metabolic monitoring and lifestyle support.")],
   "Good recovery; quitting smoking and weight gain need attention.",
   "Evaluate clozapine treatment.",
   ex=[note("Week 16", "Clozapine re-titrated after the admission.", id="cloz-16wk")]),
]

cases = [
    case("Major depression with suicide risk early in SSRI treatment", dep_tabs, dep_qs),
    case("Bipolar mania with lithium toxicity", li_tabs, li_qs),
    case("Schizophrenia with neuroleptic malignant syndrome", nms_tabs, nms_qs),
    case("Treatment-resistant schizophrenia on clozapine", cloz_tabs, cloz_qs),
]

BUILT = build(128, "mh-cases-depression-lithium-nms-clozapine",
              "Mental health case studies: depression with suicide risk, lithium toxicity, NMS and clozapine",
              items, cases, case_conversion=True)
