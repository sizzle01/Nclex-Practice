"""Batch 134 (case conversion, chunk 9: mental health part 2): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: first-episode schizophrenia on olanzapine with metabolic syndrome and tardive dyskinesia, moving to a
long-acting injection; bipolar disorder on valproate and lamotrigine with hepatotoxicity, a rash and pregnancy
planning; generalized anxiety disorder with buspirone and an SSRI discontinuation syndrome; smoking cessation with
varenicline in a client with depression. Standalone: a delirium tremens bowtie, a CIWA-Ar trend table, a priority
question in a psychiatric emergency service and a client rights question about voluntary admission. Each case walks
the six CJMM steps in order, unfolds with new data in steps 4 to 6, and includes a management of care, safety or
health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                          "PSI")
MH = "mental-health"
PSYCHOTIC = "psychotic-disorders"
MOOD = "mood-disorders"
ANXIETY = "anxiety-and-related-disorders"
SUBSTANCE = "substance-use-disorders"
WITHDRAWAL = "alcohol-withdrawal"
PSYCH_MEDS = "psychiatric-medications"
AED = "antiseizure-medications"
COMM = "therapeutic-communication"
RIGHTS = "client-rights-and-advocacy"
PRIORITY = "prioritizing-client-care"
LABS = "laboratory-values-and-monitoring"
TEACH = "health-teaching-and-behavior-change"
TECHNIQUE = "medication-administration-technique"
COLLAB = "collaboration-and-referrals"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PSI, WITHDRAWAL, "prioritize_hypotheses", "difficult", MH, "psych ms"),
   "A client on a surgical unit has new problems on day 3. Using the note, fill in the bowtie: the most likely "
   "condition in the middle, two actions to take now on the left and two parameters to monitor on the right.",
   [("Alcohol withdrawal delirium (delirium tremens)", True, "Day 3, confusion, hallucinations, fever and autonomic overactivity."),
    ("Wernicke encephalopathy", False, "No eye signs; marked autonomic overactivity points to withdrawal."),
    ("Postoperative pain alone", False, "Does not explain hallucinations and fever."),
    ("Opioid overdose", False, "Would cause slow breathing and pinpoint pupils.")],
   [("Give IV lorazepam per protocol", True, "Treats withdrawal."),
    ("Give IV thiamine first", True, "Prevents Wernicke."),
    ("Apply four-point restraints as the very first step to protect the staff", False, "Increases injury and agitation."),
    ("Give haloperidol alone for the hallucinations", False, "Lowers seizure threshold and does not treat withdrawal."),
    ("Withhold IV fluids to prevent fluid overload", False, "Clients are often dehydrated.")],
   [("CIWA-Ar", True, "Guides dosing."),
    ("Breathing, sedation", True, "Benzodiazepine safety."),
    ("Hemoglobin each morning", False, "Not related."),
    ("Body weight once a week", False, "Not urgent."),
    ("Urine specific gravity each shift", False, "Not the main guide.")],
   "Delirium tremens needs benzodiazepines, thiamine and close monitoring.",
   "Recognize and act on delirium tremens.",
   ex=[note("Day 3 after a hip fracture repair", "Drinks a bottle of wine daily (told the team on day 2). Now "
       "confused, picking at the air, seeing insects on the wall. T 38.6 C, HR 132/min, BP 172/98, drenched in sweat, "
       "coarse tremor.", id="sa-dts")]),

dtable(M(PSI, WITHDRAWAL, "evaluate_outcomes", "difficult", MH, "psych"),
   "A client in alcohol withdrawal has CIWA-Ar scores every 2 hours on a symptom-triggered protocol "
   "(lorazepam for scores of 10 or more). For each time, choose the action.",
   ["Time", "Score and findings", "Action"],
   [(["0800", "18; tremor, sweating, anxious"], [
       ("Give lorazepam per protocol and reassess in 1 hour", True, "Score above 10."),
       ("No medicine; reassess in 4 hours", False, "Undertreats."),
       ("Call a code", False, "Not an arrest.")]),
    (["1000", "8; mild tremor"], [
       ("No dose; reassess per protocol", True, "Below the threshold."),
       ("Give lorazepam anyway", False, "Oversedation."),
       ("Stop all monitoring", False, "Withdrawal can rebound.")]),
    (["1200", "6; drowsy, respiratory rate 9/min"], [
       ("Hold benzodiazepines and notify the provider", True, "Oversedation."),
       ("Give lorazepam", False, "Dangerous."),
       ("Encourage a walk", False, "Unsafe.")])],
   "Symptom-triggered dosing treats high scores and watches for oversedation.",
   "Evaluate a CIWA-Ar trend.",
   ex=[flow("CIWA-Ar", ["0800", "1000", "1200"], [
       ("Score", ["18", "8", "6"]),
       ("Respiratory rate", ["22", "16", "9"]),
       ("Sedation", ["Alert, anxious", "Calm", "Drowsy, hard to rouse"])], id="sa-ciwa")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", MH, "mgmt psych"),
   "In a psychiatric emergency service, four clients arrive within 10 minutes. Which client should the nurse "
   "assess first?",
   [("A client with schizophrenia who says the voices are back but is calm and asking for a sandwich", False,
     "Stable; assess soon."),
    ("A client who took an unknown number of acetaminophen tablets 3 hours ago and says, \"I'm fine now\"", True,
     "A potentially fatal overdose that looks well early; the antidote works best within 8 hours."),
    ("A client with panic disorder who is breathing fast, clutching the chest and saying, \"I'm dying\"", False,
     "Distressing but rarely dangerous; check vital signs soon."),
    ("A client with bipolar disorder who is loud, pacing and demanding to see the doctor right now", False, "Needs limits but is not in danger.")],
   "An acetaminophen overdose looks well early but is time-critical.",
   "Prioritize in a psychiatric emergency service."),

sata(M(MOC, RIGHTS, "take_action", "difficult", MH, "mgmt psych"),
   "A client admitted voluntarily for depression asks to leave the unit today. Which actions are appropriate?",
   [("Tell the client of the right to request discharge", True, "Voluntary status."),
    ("Ask the client to put the request in writing, per policy and state law", True, "Many states require it."),
    ("Notify the provider so the client can be assessed before leaving", True, "Risk review."),
    ("Tell the client that voluntary clients can never leave without a court order", False, "Untrue."),
    ("Lock the client's belongings away until the client changes their mind", False, "Coercion and false imprisonment.")],
   "Voluntary clients may request discharge; the team assesses risk and follows the law.",
   "Take action on client rights in mental health."),
]

# ================================================================================================
# Case 1: Olanzapine, metabolic syndrome and tardive dyskinesia
# ================================================================================================
olz_tabs = [
    ("olz-notes", "Clinic notes", [note("Early psychosis clinic",
        "A 24-year-old client with first-episode schizophrenia has taken olanzapine 20 mg daily for 9 months. "
        "Voices are mostly gone, but the client often forgets doses and was readmitted once. The parent notices "
        "lip smacking and tongue movements. The client says, \"I'm always hungry.\"", id="olz-note")]),
    ("olz-data", "Results", [table("Results", ["Today", "Before olanzapine"], [
        ("Weight", ["94 kg", "76 kg"]),
        ("Waist", ["104 cm", "88 cm"]),
        ("Fasting glucose", ["118 mg/dL", "88"]),
        ("Triglycerides", ["260 mg/dL", "120"]),
        ("Blood pressure", ["138/88 mm Hg", "118/72"]),
        ("AIMS score", ["8 (new)", "0"])], id="olz-table")]),
]

olz_qs = [
hl_table(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph psych"),
   "Highlight the findings that need follow-up.",
   ["Finding", "Today"],
   [(["Weight", "Up 18 kg"], True, "Metabolic effect."),
    (["Fasting glucose", "118 mg/dL"], True, "Prediabetes."),
    (["Triglycerides", "260 mg/dL"], True, "Dyslipidemia."),
    (["AIMS", "8, with lip smacking"], True, "Tardive dyskinesia."),
    (["Missed doses", "Readmission"], True, "Adherence problem.")],
   "Metabolic syndrome, tardive dyskinesia and nonadherence all need action.",
   "Recognize cues of antipsychotic adverse effects."),

matrix_mc(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each movement problem, indicate its description.",
   ["Acute dystonia", "Akathisia", "Pseudoparkinsonism", "Tardive dyskinesia"],
   [("Sudden neck twisting or eyes rolling up within days", 0, "Dystonia."),
    ("Inner restlessness and unable to sit still", 1, "Akathisia."),
    ("Shuffling gait, tremor and mask-like face after weeks", 2, "Parkinsonism."),
    ("Lip smacking and tongue thrusting after months to years", 3, "Tardive dyskinesia; may be permanent.")],
   "Timing and type separate extrapyramidal effects.",
   "Analyze extrapyramidal effects."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "For tardive dyskinesia, the provider may {{b1}}, and anticholinergic drugs such as benztropine {{b2}}.",
   [[("add valbenazine and review the antipsychotic", True, "A VMAT2 inhibitor treats TD."),
     ("double the olanzapine", False, "Can mask then worsen TD."),
     ("stop all treatment without a plan", False, "Relapse risk."),
     ("ignore it because it is harmless", False, "Can be disabling.")],
    [("do not help and can worsen it", True, "Unlike other EPS."),
     ("cure it", False, "They do not."),
     ("are the first-line treatment", False, "Not for TD."),
     ("prevent it completely", False, "Not true.")]],
   "VMAT2 inhibitors treat TD; anticholinergics do not.",
   "Prioritize tardive dyskinesia."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph psych"),
   "Which parts of the plan should the nurse include?",
   [("AIMS assessment at every visit", True, "Tracks TD."),
    ("Weight, waist, glucose and lipids on a schedule", True, "Metabolic monitoring."),
    ("Nutrition and exercise support", True, "Metabolic risk."),
    ("Discuss a long-acting injectable to support adherence", True, "Prevents relapse."),
    ("Stop olanzapine without a replacement because of the weight", False, "Relapse risk."),
    ("Blame the client for the weight gain", False, "A drug effect.")],
   "Monitoring, lifestyle support and adherence planning.",
   "Generate the antipsychotic monitoring plan.",
   ex=[note("Plan", "Switch to aripiprazole long-acting injection monthly. Valbenazine started. Metformin considered.",
       id="olz-plan")]),

sata(M(PPT, TECHNIQUE, "take_action", "difficult", MH, "ph"),
   "The nurse prepares the first long-acting aripiprazole injection. Which actions are correct?",
   [("Confirm oral tolerance first", True, "Tests tolerability."),
    ("Give it deep IM", True, "Correct route."),
    ("Rotate and record sites", True, "Tissue care."),
    ("Massage the site firmly afterward to spread the drug", False, "Can force the drug into tissue."),
    ("Give it IV to avoid a painful injection", False, "Never IV.")],
   "Oral tolerance, deep IM, site rotation.",
   "Take action with a long-acting injection.",
   ex=[note("Clinic", "Oral aripiprazole for 2 weeks without problems.", id="olz-oral")]),

matrix_mc(M(PSI, PSYCHOTIC, "evaluate_outcomes", "difficult", MH, "psych ph"),
   "At 6 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("No readmissions on the monthly injection", 0, "Adherence."),
    ("Weight down 5 kg", 0, "Metabolic improvement."),
    ("AIMS 3", 0, "TD improving."),
    ("\"I missed last month's shot because I had no bus fare.\"", 1, "Barrier; arrange transport."),
    ("Started part-time work", 0, "Functional recovery.")],
   "Recovery with a practical barrier to solve.",
   "Evaluate long-term schizophrenia care.",
   ex=[note("6 months", "Attending a recovery group.", id="olz-6m")]),
]

# ================================================================================================
# Case 2: Valproate and lamotrigine in bipolar disorder
# ================================================================================================
vpa_tabs = [
    ("vpa-notes", "Clinic notes", [note("Mood disorders clinic",
        "A 30-year-old client with bipolar I disorder takes valproate 1,000 mg twice daily. Lamotrigine was added "
        "3 weeks ago for depressive episodes, and the dose was raised quickly last week. The client hopes to become "
        "pregnant next year. Today the client has a fever, a spreading rash and sore lips.", id="vpa-note")]),
    ("vpa-data", "Findings", [table("Findings", ["Result"], [
        ("Temperature", ["38.4 C"]),
        ("Skin", ["Red-purple patches on the trunk with blisters"]),
        ("Mouth", ["Crusted lips, painful erosions"]),
        ("Eyes", ["Red and gritty"]),
        ("ALT", ["142 units/L"]),
        ("Valproate level", ["118 mcg/mL (50 to 100)"])], id="vpa-table")]),
]

vpa_qs = [
hl_table(M(PPT, AED, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the findings that require emergency action.",
   ["Finding", "Result"],
   [(["Rash", "Blisters, spreading"], True, "Possible Stevens-Johnson syndrome."),
    (["Mouth and eyes", "Erosions, red eyes"], True, "Mucous membranes involved."),
    (["Fever", "38.4 C"], True, "Systemic."),
    (["ALT", "142"], True, "Liver injury."),
    (["Valproate level", "118"], True, "Above range.")],
   "A blistering rash with mucosal involvement on lamotrigine is a medical emergency.",
   "Recognize cues of a severe drug reaction."),

matrix_mr(M(PPT, AED, "analyze_cues", "difficult", MH, "ph"),
   "For each finding, select the likely causes. Each row may have more than one answer.",
   ["Lamotrigine", "Valproate"],
   [("Blistering rash", {0}, "SJS risk, especially with fast titration."),
    ("Raised ALT", {1}, "Hepatotoxicity."),
    ("Higher lamotrigine levels", {1}, "Valproate doubles lamotrigine levels."),
    ("Neural tube defects in pregnancy", {1}, "Valproate is highly teratogenic."),
    ("Need for slow dose increases", {0}, "Reduces rash risk.")],
   "Valproate raises lamotrigine levels, which increases rash risk.",
   "Analyze mood stabilizer adverse effects."),

cloze(M(PPT, AED, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse should tell the client to {{b1}} and {{b2}}.",
   [[("stop lamotrigine now", True, "Any rash with systemic signs."),
     ("take an antihistamine and continue", False, "Dangerous."),
     ("double the dose", False, "Worsens."),
     ("apply steroid cream only", False, "Not enough.")],
    [("go to the emergency department today", True, "SJS needs urgent care."),
     ("book a routine appointment", False, "Too slow."),
     ("wait 48 hours", False, "Too slow."),
     ("restart lamotrigine when the rash fades", False, "Never rechallenge.")]],
   "Stop the drug and seek emergency care.",
   "Prioritize a lamotrigine rash."),

sata(M(PPT, AED, "generate_solutions", "difficult", MH, "ph"),
   "After recovery, the team plans long-term treatment. Which parts of the plan should the nurse anticipate?",
   [("Record lamotrigine as a severe reaction; never restart it", True, "Prevents re-exposure."),
    ("Plan to switch from valproate before pregnancy", True, "Teratogenic."),
    ("Folic acid supplementation", True, "Reduces neural tube risk."),
    ("Liver tests and platelet counts while on valproate", True, "Hepatotoxicity and thrombocytopenia."),
    ("Continue valproate through pregnancy as the safest choice", False, "Highest-risk mood stabilizer."),
    ("Stop all mood stabilizers when pregnant", False, "Relapse risk; switch safely.")],
   "No rechallenge, pre-pregnancy planning and monitoring.",
   "Generate the mood stabilizer plan.",
   ex=[note("Hospital", "Diagnosis: Stevens-Johnson syndrome. Recovered after 10 days.", id="vpa-sjs")]),

sata(M(MOC, COLLAB, "take_action", "difficult", MH, "mgmt ph"),
   "Which actions support safe preconception planning?",
   [("Refer to perinatal psychiatry", True, "Specialist plan."),
    ("Coordinate with the obstetric provider", True, "Shared plan."),
    ("Discuss effective contraception until the switch is complete", True, "Prevents exposure."),
    ("Advise stopping valproate on the client's own the day pregnancy is confirmed", False, "Abrupt stop risks relapse; plan earlier."),
    ("Keep the plan from the obstetrician for privacy", False, "Care needs coordination with consent.")],
   "Specialist, obstetric coordination and contraception.",
   "Take action on preconception planning.",
   ex=[note("Clinic", "Client wants to try for a baby in 6 months.", id="vpa-plan")]),

matrix_mc(M(PPT, AED, "evaluate_outcomes", "difficult", MH, "ph"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Switched to lithium with level 0.7 mEq/L", 0, "Safer in pregnancy than valproate."),
    ("Mood stable", 0, "Good."),
    ("\"I'm taking folic acid every day.\"", 0, "Correct."),
    ("\"My sister has lamotrigine; I could use hers if I get low.\"", 1, "Must never retake."),
    ("ALT 28 units/L", 0, "Normal.")],
   "Safe switch; never retake lamotrigine.",
   "Evaluate mood stabilizer changes.",
   ex=[note("3 months", "Valproate tapered off.", id="vpa-3m")]),
]

# ================================================================================================
# Case 3: GAD, buspirone and SSRI discontinuation
# ================================================================================================
gad_tabs = [
    ("gad-notes", "Clinic notes", [note("Primary care",
        "A 41-year-old client with generalized anxiety disorder stopped paroxetine 3 days ago without telling "
        "anyone because of weight gain. The client now has dizziness, \"electric shock\" feelings in the head, "
        "nausea, irritability and vivid dreams.", id="gad-note")]),
    ("gad-data", "Findings", [table("Findings", ["Result"], [
        ("GAD-7", ["16"]),
        ("Vital signs", ["Normal"]),
        ("Mood", ["Anxious; no suicidal thoughts"]),
        ("Neurologic exam", ["Normal"]),
        ("Other medicines", ["None"])], id="gad-table")]),
]

gad_qs = [
hl_text(M(PPT, PSYCH_MEDS, "recognize_cues", "moderate", MH, "ph"),
   "Highlight the findings that suggest SSRI discontinuation syndrome.",
   ["The client stopped ",
    ("paroxetine suddenly 3 days ago", True, "Short half-life SSRI."),
    " and now has ",
    ("dizziness", True, "Common."),
    ", ",
    ("electric shock sensations", True, "Classic."),
    ", ",
    ("nausea", True, "Common."),
    " and ",
    ("vivid dreams", True, "Common."),
    ". The ",
    ("neurologic exam is normal", False, "Reassuring; not a symptom."),
    "."],
   "Abruptly stopping a short half-life SSRI causes discontinuation symptoms.",
   "Recognize cues of SSRI discontinuation."),

matrix_mc(M(PPT, PSYCH_MEDS, "analyze_cues", "difficult", MH, "ph"),
   "For each antidepressant, indicate its risk of discontinuation symptoms when stopped suddenly.",
   ["Higher risk (short half-life)", "Lower risk (long half-life)"],
   [("Paroxetine", 0, "Short."),
    ("Venlafaxine", 0, "Short."),
    ("Fluoxetine", 1, "Very long."),
    ("Sertraline", 0, "Moderate; still taper.")],
   "Short half-life drugs cause more discontinuation symptoms.",
   "Analyze discontinuation risk."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The symptoms are {{b1}}, and the usual plan is to {{b2}}.",
   [[("uncomfortable but not dangerous", True, "Self-limiting."),
     ("a stroke", False, "Exam is normal."),
     ("addiction", False, "Not addiction."),
     ("serotonin syndrome", False, "Opposite situation.")],
    [("restart the drug and taper slowly, or switch with a plan", True, "Relieves symptoms."),
     ("wait without treatment for months", False, "Unnecessary suffering."),
     ("add a benzodiazepine long term", False, "Dependence."),
     ("start an MAOI tomorrow", False, "Dangerous.")]],
   "Restart and taper, or switch with a plan.",
   "Prioritize discontinuation syndrome."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "The provider plans a cross-taper to buspirone with an SSRI that causes less weight gain. Which teaching about "
   "buspirone should the nurse include?",
   [("It takes 2 to 4 weeks to work", True, "Delayed effect."),
    ("Take it the same way each day, with or without food", True, "Consistent absorption."),
    ("It does not cause dependence", True, "Unlike benzodiazepines."),
    ("Avoid large amounts of grapefruit juice", True, "Raises levels."),
    ("Take a dose only when anxious", False, "Not an as-needed drug."),
    ("It works within 30 minutes for panic", False, "Slow onset.")],
   "Buspirone is slow, daily and non-addictive.",
   "Generate buspirone teaching.",
   ex=[note("Plan", "Restart paroxetine 10 mg, taper over 4 weeks; start escitalopram and buspirone.", id="gad-plan")]),

sata(M(PSI, ANXIETY, "take_action", "difficult", MH, "psych"),
   "Which non-drug strategies should the nurse teach for GAD?",
   [("Cognitive behavioral therapy", True, "First-line."),
    ("Scheduled worry time", True, "Contains rumination."),
    ("Regular exercise", True, "Reduces anxiety."),
    ("Progressive muscle relaxation", True, "Reduces tension."),
    ("Avoid all situations that cause worry", False, "Avoidance maintains anxiety."),
    ("Drink alcohol in the evening to relax", False, "Worsens anxiety.")],
   "CBT, worry scheduling, exercise and relaxation.",
   "Take action with anxiety coping skills.",
   ex=[note("Week 2", "Discontinuation symptoms resolved.", id="gad-wk2")]),

matrix_mc(M(PSI, ANXIETY, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("GAD-7 7", 0, "Improved."),
    ("Weight stable", 0, "Concern addressed."),
    ("\"I'll talk to you before stopping any medicine.\"", 0, "Learned."),
    ("\"I double up on buspirone on bad days.\"", 1, "Not as-needed; side effects."),
    ("Practices relaxation nightly", 0, "Skill use.")],
   "Progress; dose misuse needs teaching.",
   "Evaluate GAD treatment.",
   ex=[note("3 months", "Off paroxetine.", id="gad-3m")]),
]

# ================================================================================================
# Case 4: Smoking cessation with varenicline
# ================================================================================================
var_tabs = [
    ("var-notes", "Clinic notes", [note("Primary care",
        "A 48-year-old client with depression (stable on sertraline) and COPD smokes 25 cigarettes a day and wants "
        "to quit. The client has tried nicotine gum before and relapsed after 2 weeks. The client smokes the first "
        "cigarette within 5 minutes of waking.", id="var-note")]),
    ("var-data", "Findings", [table("Findings", ["Result"], [
        ("Pack-years", ["30"]),
        ("FEV1", ["62% predicted"]),
        ("PHQ-9", ["5"]),
        ("Readiness", ["9/10"]),
        ("Quit date", ["In 1 week"])], id="var-table")]),
]

var_qs = [
hl_table(M(PSI, SUBSTANCE, "recognize_cues", "moderate", MH, "psych"),
   "Highlight the findings that show high nicotine dependence or readiness.",
   ["Finding", "Detail"],
   [(["First cigarette", "Within 5 minutes of waking"], True, "Strong dependence."),
    (["Daily use", "25 cigarettes"], True, "Heavy."),
    (["Readiness", "9/10"], True, "Ready to act."),
    (["Previous relapse", "After 2 weeks"], True, "Needs a stronger plan."),
    (["PHQ-9", "5"], False, "Mild; mood is stable.")],
   "Heavy use and early morning smoking show high dependence; readiness is high.",
   "Recognize cues in tobacco dependence."),

matrix_mc(M(PSI, SUBSTANCE, "analyze_cues", "difficult", MH, "psych"),
   "For each statement, indicate the stage of change.",
   ["Precontemplation", "Contemplation", "Preparation", "Action"],
   [("\"I don't see any reason to quit.\"", 0, "Not considering."),
    ("\"I know I should, maybe someday.\"", 1, "Ambivalent."),
    ("\"I've set a quit date next week.\"", 2, "Planning."),
    ("\"I haven't smoked for 3 weeks.\"", 3, "Changed behavior.")],
   "Stages of change guide interventions.",
   "Analyze stages of change."),

cloze(M(PPT, PSYCH_MEDS, "prioritize_hypotheses", "difficult", MH, "ph"),
   "Complete the sentence by choosing from the lists of options.",
   "Varenicline is started {{b1}} before the quit date, and the most effective plan combines it with {{b2}}.",
   [[("1 week", True, "Builds up the dose."),
     ("on the day of", False, "Too late."),
     ("3 months", False, "Too early."),
     ("after relapse", False, "Not the plan.")],
    [("behavioral counseling", True, "Doubles success."),
     ("cutting to 10 cigarettes only", False, "Not quitting."),
     ("e-cigarettes as the main plan", False, "Not first-line."),
     ("nothing else", False, "Less effective.")]],
   "Start a week before with counseling.",
   "Prioritize the cessation plan."),

sata(M(PPT, PSYCH_MEDS, "generate_solutions", "difficult", MH, "ph"),
   "Which teaching about varenicline should the nurse include?",
   [("Take it after food with a full glass of water", True, "Reduces nausea."),
    ("Vivid dreams and nausea are common", True, "Expected effects."),
    ("Report any change in mood, agitation or suicidal thoughts", True, "Neuropsychiatric monitoring."),
    ("Be careful driving until you know how it affects you", True, "Drowsiness reported."),
    ("Stop sertraline while taking varenicline", False, "No need; mood must stay treated."),
    ("Drink alcohol freely; there is no interaction", False, "Alcohol may increase reactions.")],
   "Food, common effects and mood monitoring.",
   "Generate varenicline teaching.",
   ex=[note("Prescription", "Varenicline 0.5 mg daily, titrate to 1 mg twice daily. Quitline referral.", id="var-rx")]),

sata(M(PSI, SUBSTANCE, "take_action", "difficult", MH, "psych ph"),
   "In week 3, the client calls: \"I feel really down and irritable, and I'm not sure it's worth going on.\" Which "
   "actions should the nurse take?",
   [("Ask directly about suicidal thoughts and plans", True, "Assess risk."),
    ("Tell the client to stop varenicline and notify the provider", True, "Neuropsychiatric effect."),
    ("Arrange a same-day assessment", True, "Safety."),
    ("Reassure that these are just nicotine withdrawal feelings and continue", False, "May be a drug effect or depression."),
    ("Suggest smoking a few cigarettes to feel better", False, "Not a treatment.")],
   "Assess risk, stop the drug and see the client today.",
   "Take action on mood changes during cessation.",
   ex=[note("Week 3", "Smoke-free for 2 weeks.", id="var-wk3")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", MH, "psych"),
   "At 3 months, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Smoke-free for 10 weeks on nicotine patches after stopping varenicline", 0, "Success."),
    ("PHQ-9 6", 0, "Mood back to baseline."),
    ("Less breathless climbing stairs", 0, "Lung benefit."),
    ("\"I smoked one at a party, so I failed. I might as well start again.\"", 1, "A lapse is not a relapse; relapse prevention needed."),
    ("Uses the quitline weekly", 0, "Support.")],
   "Success; a lapse needs relapse prevention.",
   "Evaluate smoking cessation.",
   ex=[note("3 months", "Switched to nicotine patches after the mood change.", id="var-3m")]),
]

cases = [
    case("Olanzapine with metabolic syndrome and tardive dyskinesia", olz_tabs, olz_qs),
    case("Bipolar disorder on valproate and lamotrigine with a severe rash", vpa_tabs, vpa_qs),
    case("Generalized anxiety disorder with SSRI discontinuation and buspirone", gad_tabs, gad_qs),
    case("Smoking cessation with varenicline", var_tabs, var_qs),
]

BUILT = build(134, "mh-cases-olanzapine-mood-gad-smoking-standalone",
              "Mental health case studies: olanzapine and tardive dyskinesia, valproate and lamotrigine, GAD and "
              "smoking cessation, with four standalone items",
              items, cases, case_conversion=True)
