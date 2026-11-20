"""Batch 60: NGN unfolding case studies I - urosepsis, pediatric asthma, preeclampsia, alcohol withdrawal.

Each case walks the six CJMM steps in order: recognize cues, analyze cues, prioritize hypotheses,
generate solutions, take action, evaluate outcomes (NCSBN Clinical Judgment Measurement Model).

Rewritten to NCLEX-RN difficulty on 20 November 2026 (see quality.py).
"""
from qb import *

PA, PPT, PSI_ = "PHI.PA", "PHI.PPT", "PSI"

# ---- Case 1: urosepsis in an older adult ------------------------------------------------------
sepsis_tabs = [
    ("sepsis-notes", "Nurses' notes", [note("Emergency department, 14:00",
        "A 78-year-old client is brought in by their daughter, who says the client has been "
        "\"more confused since yesterday\" and has eaten little. History: type 2 diabetes, hypertension, "
        "benign prostatic hyperplasia. The client is oriented to person only. Skin is warm and flushed; "
        "mucous membranes are dry. The client reports burning on urination for 3 days.", id="sepsis-note")]),
    ("sepsis-vitals", "Vital signs", [flow("Vital signs", ["14:05", "15:30"], [
        ("Temperature", ["38.9 °C (102 °F)", "38.6 °C (101.5 °F)"]),
        ("Heart rate", ["116/min", "122/min"]),
        ("Respiratory rate", ["24/min", "26/min"]),
        ("Blood pressure", ["88/52 mm Hg (baseline 142/84)", "84/48 mm Hg"]),
        ("SpO2", ["94% on room air", "93% on room air"])], id="sepsis-flow")]),
    ("sepsis-labs", "Laboratory results", [table("Laboratory results, 14:30", ["Result", "Reference"], [
        ("WBC", ["18,400/mm3", "5,000-10,000/mm3"]),
        ("Lactate", ["3.8 mmol/L", "below 2 mmol/L"]),
        ("Creatinine", ["1.9 mg/dL (baseline 1.0)", "0.6-1.2 mg/dL"]),
        ("Glucose", ["264 mg/dL", "70-110 mg/dL"]),
        ("Urinalysis", ["Positive leukocyte esterase and nitrites, many bacteria", "Negative"])], id="sepsis-lab")]),
]

sepsis_qs = [
hl_text(M(PA, "shock-and-sepsis", "recognize_cues", "moderate", "renal-urinary", "ms ati"),
   "Review the nurse's note. Highlight the findings that require immediate follow-up.",
   ["A 78-year-old client is ",
    ("more confused since yesterday", True, "Acute confusion in an older adult can be the first sign of sepsis and poor brain perfusion."),
    ". History of ",
    ("benign prostatic hyperplasia", False, "A risk factor for urinary tract infection, but not an acute finding."),
    ". Blood pressure ",
    ("88/52 mm Hg, down from a baseline of 142/84", True, "A large drop from baseline suggests septic shock."),
    "; heart rate ",
    ("116/min", True, "Tachycardia is a compensatory response to infection and low blood pressure."),
    ". Lactate ",
    ("3.8 mmol/L", True, "A lactate above 2 mmol/L shows tissue hypoperfusion."),
    ". SpO2 ",
    ("94% on room air", False, "Acceptable for now; continue to monitor."),
    "."],
   "Acute confusion, hypotension, tachycardia and a high lactate show sepsis with organ hypoperfusion.",
   "Recognize the cues of sepsis in an older adult."),

cloze(M(PA, "shock-and-sepsis", "analyze_cues", "moderate", "renal-urinary", "ms ati"),
   "Complete the sentence based on the client's data.",
   "The client's findings are most consistent with {{b1}} caused by {{b2}}.",
   [[("septic shock", True, "Infection with hypotension and a lactate above 2 mmol/L meets the picture of septic shock."),
     ("hypovolemic shock from poor intake alone", False, "Poor intake contributes, but fever, leukocytosis and a positive urinalysis point to sepsis."),
     ("hyperosmolar hyperglycemic state", False, "A glucose of 264 mg/dL is far below the levels seen in HHS.")],
    [("a urinary tract infection", True, "Dysuria and a urinalysis with nitrites and leukocyte esterase identify the source."),
     ("pneumonia", False, "Breath sounds and oxygenation don't point to a lung source."),
     ("an acute stroke", False, "Confusion is explained by sepsis, and there are no focal deficits.")]],
   "An older adult with UTI symptoms, fever, hypotension and a high lactate has urosepsis with shock.",
   "Analyze cues to identify urosepsis."),

matrix_mc(M(PA, "shock-and-sepsis", "prioritize_hypotheses", "difficult", "renal-urinary", "ms"),
   "For each complication, indicate whether the client is at high or low immediate risk based on the current data.",
   ["High immediate risk", "Low immediate risk"],
   [("Acute kidney injury", 0, ["Creatinine has nearly doubled from baseline with hypotension.", "Rising creatinine shows the kidneys are already affected."]),
    ("Worsening shock and multiorgan failure", 0, ["Blood pressure is still falling and lactate is high.", "Falling blood pressure shows shock is progressing."]),
    ("Acute angle-closure glaucoma", 1, ["Nothing in the data suggests eye disease.", "No eye symptoms are reported."]),
    ("Falls and injury from confusion", 0, ["Acute confusion in an older adult raises fall risk.", "Delirium is a strong fall risk factor."])],
   "Shock threatens the kidneys and other organs, and delirium raises injury risk.",
   "Prioritize the most likely complications of septic shock."),

sata(M(PA, "shock-and-sepsis", "generate_solutions", "difficult", "renal-urinary", "ms ati"),
   "Which prescriptions should the nurse anticipate for this client?",
   [("Two sets of blood cultures before antibiotics", True, "Cultures identify the organism and should be drawn before the first dose."),
    ("Broad-spectrum IV antibiotic within 1 hour", True, "Early antibiotics lower deaths from sepsis."),
    ("IV crystalloid 30 mL/kg", True, "Recommended for hypotension or a lactate of 4 mmol/L or more; this client is hypotensive."),
    ("Repeat lactate", True, "A falling lactate shows that perfusion is improving."),
    ("Encourage 240 mL of oral fluids every hour", False, "Oral intake cannot correct septic shock in a confused client."),
    ("Hold the insulin until the blood pressure is normal", False, "Glucose still needs management; hyperglycemia worsens outcomes.")],
   "Hour-1 sepsis bundle: lactate, cultures, antibiotics, fluids, then vasopressors if hypotension persists.",
   "Plan sepsis bundle care."),

order(M(PA, "shock-and-sepsis", "take_action", "difficult", "renal-urinary", "ms"),
   "Place the nurse's actions in the order they should be done.",
   [("Insert two large-bore IV catheters", "Access is needed for fluids and medications."),
    ("Draw two sets of blood cultures", "Cultures are drawn before antibiotics."),
    ("Start the IV crystalloid bolus", "Restoring perfusion is urgent."),
    ("Give the first dose of IV antibiotic", "Antibiotics are started within the first hour, after cultures."),
    ("Measure hourly urine output", "Urine output shows whether kidney perfusion is improving.")],
   "Access, cultures, fluids and antibiotics come first; monitoring follows.",
   "Sequence nursing actions in septic shock.", seed=60),

matrix_mc(M(PA, "shock-and-sepsis", "evaluate_outcomes", "moderate", "renal-urinary", "ms"),
   "Four hours later the nurse reassesses the client. For each finding, indicate whether it shows improvement or no improvement.",
   ["Improved", "Not improved"],
   [("Blood pressure 118/70 mm Hg", 0, ["Perfusion pressure is restored.", "This is improvement."]),
    ("Lactate 1.6 mmol/L", 0, ["Lactate has cleared to normal.", "This is improvement."]),
    ("Urine output 15 mL/hr", 1, ["Output below 0.5 mL/kg/hr isn't improvement.", "The kidneys are still poorly perfused; report it."]),
    ("Oriented to person, place and time", 0, ["Mental status has returned to baseline.", "This is improvement."])],
   "Blood pressure, lactate and mental status have improved; low urine output still needs follow-up.",
   "Evaluate the response to sepsis treatment."),
]

# ---- Case 2: pediatric asthma exacerbation ----------------------------------------------------
asthma_tabs = [
    ("asthma-notes", "Nurses' notes", [note("Pediatric emergency department, 19:20",
        "A 7-year-old with asthma has had a cold for 2 days. Tonight the child is coughing and wheezing; "
        "the parent gave albuterol twice at home with little relief. The child is sitting upright, speaking "
        "in short phrases, with intercostal and subcostal retractions. Expiratory wheezes in all lung fields.",
        id="asthma-note")]),
    ("asthma-vitals", "Vital signs", [flow("Vital signs", ["19:25"], [
        ("Temperature", ["37.6 °C (99.7 °F)"]), ("Heart rate", ["138/min"]), ("Respiratory rate", ["38/min"]),
        ("SpO2", ["89% on room air"]), ("Peak flow", ["45% of personal best"])], id="asthma-flow")]),
]

asthma_qs = [
select_n(M(PA, "asthma-and-copd-management", "recognize_cues", "moderate", "pediatrics", "peds"),
   "Select the 4 findings that show the child's asthma exacerbation is severe.",
   [("Speaking in short phrases", True, "Limited speech shows serious breathlessness."),
    ("Intercostal and subcostal retractions", True, "Accessory muscle use shows increased work of breathing."),
    ("SpO2 89% on room air", True, "Hypoxemia below 90% marks a severe exacerbation."),
    ("Peak flow 45% of personal best", True, "Below 50% indicates a severe exacerbation."),
    ("Temperature 37.6 °C (99.7 °F)", False, "A low-grade temperature isn't a severity sign."),
    ("A cold for 2 days", False, "The cold is the likely trigger, not a sign of severity.")],
   "Severity signs: limited speech, retractions, hypoxemia and low peak flow.",
   "Recognize a severe pediatric asthma exacerbation."),

cloze(M(PA, "asthma-and-copd-management", "analyze_cues", "moderate", "pediatrics", "peds"),
   "Complete the sentence.",
   "The child's wheezing and hypoxemia are caused by {{b1}}, most likely triggered by {{b2}}.",
   [[("bronchoconstriction and airway inflammation", True, "Asthma narrows and inflames the lower airways."),
     ("fluid in the alveoli", False, "That describes pneumonia or pulmonary edema."),
     ("upper airway obstruction", False, "Upper airway obstruction causes inspiratory stridor, not expiratory wheezes.")],
    [("a viral respiratory infection", True, "Viral colds are the most common trigger of asthma flares in children."),
     ("exercise", False, "No exercise was reported."),
     ("a food allergy", False, "No food exposure was reported.")]],
   "A viral infection triggered airway inflammation and bronchospasm.",
   "Analyze the cause of an asthma exacerbation."),

mc(M(PA, "asthma-and-copd-management", "prioritize_hypotheses", "difficult", "pediatrics", "peds"),
   "The child is frightened and clinging to the parent, has had little to drink since the afternoon and used the inhaler without a spacer at home. Which problem is the nurse's priority for this child?",
   [("Impaired gas exchange from narrowed airways", True, "An SpO2 of 89% from narrowed airways is the most immediate threat."),
    ("Anxiety that is making the breathing worse", False, "Fear adds to the distress but is secondary to the hypoxemia."),
    ("Risk for dehydration from rapid breathing", False, "Possible with tachypnea and poor intake, but not the most urgent."),
    ("Ineffective inhaler technique without a spacer", False, "This likely contributed, but teaching comes after the child is stable.")],
   "Oxygenation comes first.",
   "Prioritize gas exchange in acute asthma."),

grouping(M(PPT, "respiratory-medications", "generate_solutions", "difficult", "pediatrics", "peds ph"),
   "For each category, select the actions the nurse should anticipate in the emergency department.",
   [("Medications", [
        ("Nebulized albuterol", True, "A short-acting beta2 agonist relieves bronchospasm quickly."),
        ("Nebulized ipratropium", True, "Added to albuterol in severe exacerbations to improve bronchodilation."),
        ("Systemic corticosteroid", True, "Reduces airway inflammation; the effect begins within hours."),
        ("Oral codeine cough syrup", False, "Cough suppressants and opioids can depress breathing and aren't used.")]),
    ("Monitoring", [
        ("Continuous pulse oximetry", True, "Tracks oxygenation during treatment."),
        ("Repeat respiratory assessment after each treatment", True, "Shows the response to bronchodilators."),
        ("Daily weight only", False, "Not useful in the acute phase.")])],
   "Acute asthma care: oxygen, short-acting bronchodilators with ipratropium, systemic steroids and close monitoring.",
   "Plan acute asthma treatment for a child."),

mc(M(PA, "asthma-and-copd-management", "take_action", "moderate", "pediatrics", "peds"),
   "The provider has prescribed oxygen, nebulized albuterol with ipratropium, oral dexamethasone and a chest x-ray. Which action should the nurse take first?",
   [("Apply oxygen and start the albuterol-ipratropium nebulizer", True, "This treats the hypoxemia and bronchospasm right away."),
    ("Give the oral dexamethasone so it can start working sooner", False, "Steroids take hours to work; oxygen and bronchodilators act in minutes."),
    ("Send the child for the chest x-ray before treatments begin", False, "Imaging does not treat the problem and should not delay treatment."),
    ("Weigh the child to confirm the doses of each medicine", False, "A recent or estimated weight can be used; treatment should not wait.")],
   "Treat breathing first.",
   "Take action in an acute pediatric asthma exacerbation."),

matrix_mc(M(PA, "asthma-and-copd-management", "evaluate_outcomes", "moderate", "pediatrics", "peds"),
   "One hour after treatment, the nurse reassesses the child. For each finding, indicate whether it shows improvement or decline.",
   ["Improvement", "Decline"],
   [("Speaking in full sentences", 0, ["Breathlessness has eased.", "This is improvement."]),
    ("SpO2 95% on 2 L/min", 0, ["Oxygenation has improved.", "This is improvement."]),
    ("Drowsy and difficult to wake", 1, ["This isn't improvement.", "Drowsiness may signal CO2 retention and impending respiratory failure."]),
    ("Peak flow 70% of personal best", 0, ["Airflow has improved.", "This is improvement."])],
   "Drowsiness during an asthma attack is a danger sign, not a sign of rest.",
   "Evaluate the response to asthma treatment."),
]

# ---- Case 3: preeclampsia with severe features ------------------------------------------------
pe_tabs = [
    ("pe-notes", "Nurses' notes", [note("Labor and delivery triage, 10:10",
        "A 24-year-old client, gravida 1, at 35 weeks reports a headache for 6 hours that acetaminophen "
        "didn't relieve and \"seeing spots.\" Swelling of the face and hands. Fetal heart rate 145/min with "
        "moderate variability.", id="pe-note")]),
    ("pe-vitals", "Vital signs", [flow("Vital signs", ["10:10", "10:25"], [
        ("Blood pressure", ["164/112 mm Hg", "168/110 mm Hg"]), ("Heart rate", ["92/min", "94/min"]),
        ("Respiratory rate", ["18/min", "18/min"]), ("Deep tendon reflexes", ["3+ with 1 beat of clonus", "3+"])],
        id="pe-flow")]),
    ("pe-labs", "Laboratory results", [table("Laboratory results, 10:45", ["Result", "Reference"], [
        ("Platelets", ["96,000/mm3", "150,000-400,000/mm3"]), ("AST", ["88 U/L", "0-35 U/L"]),
        ("Creatinine", ["1.0 mg/dL", "0.5-1.1 mg/dL"]), ("Urine protein-to-creatinine ratio", ["0.6", "below 0.3"])],
        id="pe-lab")]),
]

pe_qs = [
hl_table(M(PA, "pregnancy-complications", "recognize_cues", "moderate", "maternal-newborn", "ob"),
   "Highlight the findings that are severe features of preeclampsia.",
   ["Finding", "Value"],
   [(["Blood pressure on two readings", "164/112 and 168/110 mm Hg"], True, "160/110 mm Hg or higher on two readings is a severe feature."),
    (["Platelets", "96,000/mm3"], True, "Below 100,000/mm3 is a severe feature."),
    (["Headache unrelieved by acetaminophen with visual spots", "Present"], True, "New neurologic and visual symptoms are severe features."),
    (["Facial and hand swelling", "Present"], False, "Edema is common in pregnancy and isn't a diagnostic criterion."),
    (["AST", "88 U/L"], True, "Liver enzymes at least twice the upper limit of normal are a severe feature."),
    (["Fetal heart rate", "145/min, moderate variability"], False, "A reassuring fetal heart rate pattern.")],
   "Severe features include BP of 160/110 or more, platelets under 100,000, raised liver enzymes, and neurologic or visual symptoms.",
   "Recognize severe features of preeclampsia."),

cloze(M(PA, "pregnancy-complications", "analyze_cues", "difficult", "maternal-newborn", "ob"),
   "Complete the sentence.",
   "The client is at highest immediate risk for {{b1}}, as shown by {{b2}}.",
   [[("eclamptic seizures", True, "Cerebral irritability in severe preeclampsia can progress to seizures."),
     ("postpartum hemorrhage", False, "A later risk, not the most immediate one."),
     ("gestational diabetes", False, "Not related to these findings.")],
    [("headache, visual changes and hyperreflexia with clonus", True, "These are warning signs of impending eclampsia."),
     ("a fetal heart rate of 145/min", False, "A normal fetal heart rate."),
     ("a creatinine of 1.0 mg/dL", False, "Within normal limits.")]],
   "Persistent headache, visual changes and clonus warn of impending eclampsia.",
   "Analyze seizure risk in severe preeclampsia."),

mc(M(PA, "pregnancy-complications", "prioritize_hypotheses", "difficult", "maternal-newborn", "ob"),
   "The client is anxious, the face and hands are swollen, the fetal heart rate is reassuring and the platelets are 96,000/mm3. Which problem is the nurse's priority?",
   [("Risk for seizures and stroke from severe hypertension", True, "Eclampsia and stroke are the most immediate threats to the client's life."),
    ("Risk for bleeding related to the low platelet count", False, "Platelets above 50,000 rarely bleed on their own; seizures and stroke are more imminent."),
    ("Risk for fetal compromise from reduced placental flow", False, "The fetal heart rate is reassuring; the maternal threat is more immediate."),
    ("Fluid volume excess shown by the facial edema", False, "Edema is common in pregnancy and secondary here.")],
   "Preventing eclampsia and stroke comes first.",
   "Prioritize problems in severe preeclampsia."),

matrix_mr(M(PA, "pregnancy-complications", "generate_solutions", "difficult", "maternal-newborn", "ob ph"),
   "For each intervention, indicate the goal or goals it supports. Each row may have more than one answer.",
   ["Prevent seizures", "Lower blood pressure", "Monitor fetal well-being"],
   [("Magnesium sulfate infusion", {0}, ["Magnesium is the drug of choice to prevent eclamptic seizures.", "Magnesium isn't used as an antihypertensive.", "Magnesium doesn't monitor the fetus."]),
    ("IV labetalol", {1}, ["Labetalol doesn't prevent seizures directly.", "Labetalol lowers severe-range blood pressure.", "It doesn't monitor the fetus."]),
    ("Continuous electronic fetal monitoring", {2}, ["It doesn't prevent seizures.", "It doesn't lower blood pressure.", "It shows how the fetus tolerates the maternal condition and treatment."]),
    ("Quiet, dimly lit room", {0}, ["Reducing stimulation lowers seizure risk.", "It doesn't directly lower blood pressure.", "It doesn't monitor the fetus."])],
   "Severe preeclampsia: magnesium for seizure prevention, rapid antihypertensives, fetal monitoring and a low-stimulation environment.",
   "Plan care for severe preeclampsia."),

sata(M(PPT, "obstetric-medications", "take_action", "moderate", "maternal-newborn", "ob ph"),
   "Magnesium sulfate is started. Which actions should the nurse take?",
   [("Check respiratory rate and deep tendon reflexes every hour", True, "Loss of reflexes and slow breathing are early signs of toxicity."),
    ("Measure hourly urine output", True, "Magnesium is excreted by the kidneys; low output raises the risk of toxicity."),
    ("Keep calcium gluconate at the bedside", True, "Calcium gluconate is the antidote for magnesium toxicity."),
    ("Use an infusion pump on a secondary line", True, "Pump control prevents accidental overdose."),
    ("Encourage the client to walk to the bathroom", False, "Seizure and fall risk require bed rest with the side rails padded."),
    ("Stop the infusion when the headache goes away", False, "Magnesium usually continues for 24 hours after birth.")],
   "Magnesium sulfate safety: pump, hourly reflexes, breathing and urine output checks, and calcium gluconate ready.",
   "Take action safely during magnesium sulfate therapy."),

matrix_mc(M(PA, "pregnancy-complications", "evaluate_outcomes", "moderate", "maternal-newborn", "ob"),
   "Two hours later, the nurse reassesses. For each finding, indicate whether the treatment is effective or not effective.",
   ["Effective", "Not effective"],
   [("Blood pressure 146/94 mm Hg", 0, ["Out of the severe range.", "This shows control."]),
    ("No seizure activity", 0, ["The goal of magnesium therapy is met.", "This shows effectiveness."]),
    ("Respiratory rate 8/min", 1, ["A slow rate isn't a treatment goal.", "Suggests magnesium toxicity; stop the infusion and notify the provider."]),
    ("Deep tendon reflexes 2+", 0, ["Reflexes are back to normal.", "This shows effectiveness without toxicity."])],
   "Controlled BP, no seizures and normal reflexes show success; a respiratory rate under 12 signals toxicity.",
   "Evaluate preeclampsia treatment."),
]

# ---- Case 4: alcohol withdrawal after surgery -------------------------------------------------
aw_tabs = [
    ("aw-notes", "Nurses' notes", [note("Orthopedic unit, postoperative day 2, 06:00",
        "A 52-year-old client had open reduction and internal fixation of the ankle 2 days ago. Overnight the "
        "client became restless and sweaty, with a tremor of both hands. At 05:45 the client said, "
        "\"There are bugs crawling on the wall.\" The client's spouse says the client drinks about a pint of "
        "vodka a day; this wasn't documented on admission.", id="aw-note")]),
    ("aw-vitals", "Vital signs", [flow("Vital signs", ["22:00", "06:00"], [
        ("Temperature", ["37.2 °C (99 °F)", "38.0 °C (100.4 °F)"]), ("Heart rate", ["88/min", "124/min"]),
        ("Blood pressure", ["134/82 mm Hg", "168/98 mm Hg"]), ("Respiratory rate", ["16/min", "22/min"]),
        ("SpO2", ["97%", "96% on room air"])], id="aw-flow")]),
]

aw_qs = [
select_n(M(PSI_, "alcohol-withdrawal", "recognize_cues", "moderate", "mental-health", "psych"),
   "Select the 4 findings that most suggest alcohol withdrawal.",
   [("Tremor of both hands", True, "Tremor is one of the earliest signs of withdrawal."),
    ("Seeing bugs on the wall", True, "Visual hallucinations occur 12 to 48 hours after the last drink."),
    ("Heart rate up from 88 to 124/min", True, "Autonomic hyperactivity raises the heart rate."),
    ("Sweating", True, "Diaphoresis reflects autonomic hyperactivity."),
    ("Ankle surgery 2 days ago", False, "This explains the timing (no access to alcohol), not a sign itself."),
    ("SpO2 96% on room air", False, "Normal oxygenation.")],
   "Withdrawal signs: tremor, sweating, tachycardia, hypertension, agitation, hallucinations and seizures.",
   "Recognize alcohol withdrawal after surgery."),

dtable(M(PSI_, "alcohol-withdrawal", "analyze_cues", "moderate", "mental-health", "psych"),
   "For each time after the last drink, select the withdrawal finding most typical of that period.",
   ["Time after last drink", "Most typical finding"],
   [(["6 to 24 hours"], [("Tremor, anxiety and sweating", True, "Early withdrawal begins within hours."),
                          ("Delirium tremens", False, "DTs typically begin at 48 to 72 hours."),
                          ("Full recovery", False, "Symptoms are just beginning.")]),
    (["12 to 48 hours"], [("Hallucinations and possible seizures", True, "Alcoholic hallucinosis and withdrawal seizures peak in this window."),
                           ("No symptoms", False, "Symptoms are usually present."),
                           ("Wernicke encephalopathy only", False, "Wernicke's is from thiamine deficiency, not a stage of withdrawal.")]),
    (["48 to 72 hours"], [("Delirium tremens", True, "DTs: confusion, severe autonomic instability and fever."),
                           ("Mild tremor only", False, "By this time severe withdrawal may develop."),
                           ("Withdrawal has always ended", False, "This is when the most dangerous phase can begin.")])],
   "The client's day-2 picture (hallucinations, fever, autonomic signs) fits progression toward delirium tremens.",
   "Analyze the timeline of alcohol withdrawal."),

mc(M(PSI_, "alcohol-withdrawal", "prioritize_hypotheses", "difficult", "mental-health", "psych"),
   "The client keeps trying to stand on the operated ankle, has a temperature of 38.0 °C and pulled at the IV once overnight. Which problem is the nurse's priority?",
   [("Risk for injury from withdrawal seizures and delirium", True, "Withdrawal can progress to seizures and delirium tremens, which can be fatal."),
    ("Risk for infection shown by the new low-grade fever", False, "Fever here fits autonomic hyperactivity of withdrawal; infection is assessed but is less urgent."),
    ("Risk for falls from standing on the operated ankle", False, "Falls are part of the injury risk, but seizures and DTs are the larger threat."),
    ("Ineffective denial of the client's alcohol use", False, "This is addressed after the client is safe.")],
   "Safety from seizures and delirium comes first.",
   "Prioritize safety in alcohol withdrawal."),

sata(M(PSI_, "alcohol-withdrawal", "generate_solutions", "moderate", "mental-health", "psych ph"),
   "Which interventions should the nurse anticipate?",
   [("Benzodiazepine dosed by a withdrawal scale such as CIWA-Ar", True, "Symptom-triggered benzodiazepines are first-line."),
    ("Thiamine before any glucose-containing fluids", True, "Prevents Wernicke encephalopathy."),
    ("Seizure precautions", True, "Withdrawal seizures can occur."),
    ("A dark, quiet room with the door closed", False, "Darkness creates shadows the client may misperceive; a quiet, well-lit room is better."),
    ("Restraints as the first intervention", False, "Restraints are a last resort and can increase agitation."),
    ("Restrict all oral fluids", False, "Clients in withdrawal often need fluids and electrolytes.")],
   "Withdrawal care: benzodiazepines, thiamine, fluids and electrolytes, safety and low stimulation.",
   "Plan alcohol withdrawal care."),

mc(M(PSI_, "alcohol-withdrawal", "take_action", "moderate", "mental-health", "psych ph"),
   "The client's CIWA-Ar score is 22. The client last had lorazepam 2 mg 1 hour ago and is still agitated. What should the nurse do?",
   [("Give the lorazepam dose the protocol lists for this score", True, "A score above about 15 to 20 indicates severe withdrawal; symptom-triggered dosing can repeat hourly."),
    ("Wait until the 4-hour reassessment before giving another dose", False, "Delay risks seizures and delirium tremens."),
    ("Give haloperidol for the agitation instead of lorazepam", False, "Antipsychotics do not treat withdrawal and lower the seizure threshold."),
    ("Apply wrist restraints until the agitation settles", False, "Restraints are a last resort and can increase agitation.")],
   "Symptom-triggered benzodiazepine dosing based on CIWA-Ar score.",
   "Act on a withdrawal assessment score."),

matrix_mc(M(PSI_, "alcohol-withdrawal", "evaluate_outcomes", "moderate", "mental-health", "psych"),
   "Six hours later, for each finding, indicate whether it shows improvement or no improvement.",
   ["Improved", "Not improved"],
   [("Heart rate 92/min", 0, ["Autonomic activity has settled.", "This is improvement."]),
    ("No hallucinations reported", 0, ["Perceptual symptoms have resolved.", "This is improvement."]),
    ("Tremor decreased", 0, ["Symptoms are easing.", "This is improvement."]),
    ("Generalized seizure at 11:00", 1, ["A seizure isn't improvement.", "Shows progression; needs urgent treatment and safety measures."])],
   "Falling heart rate and fewer symptoms show improvement; a seizure is a serious complication.",
   "Evaluate alcohol withdrawal treatment."),
]

cases = [
    case("Older adult with sepsis from a urinary tract infection", sepsis_tabs, sepsis_qs),
    case("Child with a severe asthma exacerbation", asthma_tabs, asthma_qs),
    case("Preeclampsia with severe features at 35 weeks", pe_tabs, pe_qs),
    case("Alcohol withdrawal after orthopedic surgery", aw_tabs, aw_qs),
]

BUILT = build(60, "ngn-cases-1", "NGN unfolding case studies I", [], cases, hardened=True)
