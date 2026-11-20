"""Batch 100 (case conversion, chunk 1: cardiac): four full NGN case studies.

The bank is being rebuilt around full case studies (CASE-MAP.md, plan of 2026-11-20): each case walks the
six CJMM steps in order and unfolds, with new data in steps 4 to 6, so the candidate has to generate
solutions, take action and evaluate outcomes on a changing client. Pharmacology is tested inside the
disease (hold parameters, interactions, antidotes, monitoring).

Cases: NSTEMI with a retroperitoneal bleed after catheterization; heart failure with digoxin toxicity;
new atrial fibrillation with a rapid ventricular rate from thyrotoxicosis; hypertensive emergency after
stopping clonidine."""
from qb import *

RRP, PA, PPT, HPM = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM"
CV = "cardiovascular"
ACS = "acute-coronary-syndrome"
DYS = "cardiac-dysrhythmias"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
POSTPROC = "post-procedure-complications"
CONTRAST = "monitoring-after-contrast-dye"
ANTICOAG = "anticoagulant-therapy-and-monitoring"
ADVERSE = "adverse-effects-of-medications"
HF_RX = "heart-failure-interventions"
HF_ASSESS = "heart-failure-assessment"
LYTES = "electrolyte-imbalances"
ANTIDOTE = "antidotes-and-reversal-agents"
HTN_CRISIS = "hypertensive-crisis"
HTN = "hypertension-management"
THYROID = "thyroid-and-parathyroid-disorders"
TEACH = "health-teaching-and-behavior-change"
LABS = "laboratory-values-and-monitoring"
CV_MEDS = "cardiovascular-medications"
add_subtopics({CV_MEDS: (PPT, "Cardiovascular medications")})

items = []

# ================================================================================================
# Case 1: NSTEMI, then a retroperitoneal bleed after femoral catheterization
# ================================================================================================
nstemi_tabs = [
    ("nstemi-notes", "Nurses' notes", [note("Emergency department, 0700",
        "A 67-year-old client reports pressure in the center of the chest at rest for the past 2 hours, with "
        "sweating and shortness of breath. History: type 2 diabetes, chronic kidney disease stage 3, "
        "hypertension, 40 pack-years of smoking. Home medications: metformin 1,000 mg twice daily, lisinopril "
        "20 mg daily, atorvastatin 10 mg daily, and over-the-counter ibuprofen 400 mg three times daily for "
        "knee pain. The 12-lead ECG shows 2 mm ST depression and T-wave inversion in V4 to V6. Cardiac "
        "catheterization is planned for this afternoon.", id="nstemi-note")]),
    ("nstemi-vitals", "Vital signs", [flow("Vital signs", ["0700", "0900"], [
        ("Blood pressure", ["158/94 mm Hg", "150/90 mm Hg"]),
        ("Heart rate", ["104/min", "100/min"]),
        ("Respirations", ["22/min", "20/min"]),
        ("SpO2", ["93% on room air", "94% on room air"]),
        ("Temperature", ["37.0 C (98.6 F)", "37.0 C (98.6 F)"]),
        ("Chest pain", ["6 of 10", "4 of 10"])], id="nstemi-flow")]),
    ("nstemi-labs", "Laboratory results", [table("Laboratory results", ["0700", "0900", "Reference range"], [
        ("Troponin I", ["0.9 ng/mL", "2.4 ng/mL", "< 0.04 ng/mL"]),
        ("Creatinine", ["1.6 mg/dL", "-", "0.6 to 1.2 mg/dL"]),
        ("Potassium", ["4.1 mEq/L", "-", "3.5 to 5.0 mEq/L"]),
        ("Glucose", ["212 mg/dL", "-", "70 to 110 mg/dL"]),
        ("Hemoglobin", ["13.6 g/dL", "-", "13.5 to 17.5 g/dL"]),
        ("Platelets", ["230,000/mm3", "-", "150,000 to 400,000/mm3"])], id="nstemi-lab")]),
]

nstemi_qs = [
hl_table(M(RRP, CV_DX, "recognize_cues", "moderate", CV, "aha ms lab"),
   "Review the data in the client's record. Highlight the findings that require follow-up by the nurse.",
   ["Assessment", "Finding"],
   [(["12-lead ECG", "ST depression and T-wave inversion in V4 to V6"], True,
     "ST depression with T-wave inversion shows subendocardial ischemia; with rising troponin it is an NSTEMI."),
    (["Troponin I", "0.9 then 2.4 ng/mL"], True,
     "A rising troponin confirms ongoing myocardial cell death, not stable angina."),
    (["SpO2", "93% on room air"], False,
     "Current ACS guidance gives oxygen only when SpO2 is below 90%; 93% does not need oxygen."),
    (["Creatinine", "1.6 mg/dL with metformin at home"], True,
     "Reduced kidney function plus iodinated contrast raises the risk of contrast-associated kidney injury, "
     "and metformin then risks lactic acidosis."),
    (["Home medication", "Ibuprofen 400 mg three times daily"], True,
     "NSAIDs raise the risk of further cardiovascular events and bleeding with antiplatelets, and harm the kidneys."),
    (["Temperature", "37.0 C (98.6 F)"], False, "Normal; no follow-up needed."),
    (["Platelets", "230,000/mm3"], False, "Normal; a baseline count before heparin, not a problem now.")],
   "The cues that matter are those showing infarction (ECG, rising troponin) and those that change the safety "
   "of treatment (kidney function with metformin and contrast, daily NSAID use).",
   "Recognize the cues of NSTEMI and of procedure risk."),

matrix_mc(M("SECE.MOC", "informed-consent", "analyze_cues", "difficult", CV, "mgmt fund"),
   "At 1230 the nurse prepares the client for catheterization. For each finding, indicate whether the consent "
   "process is in order or the nurse must act before the procedure.",
   ["In order", "Nurse must act"],
   [("The client signed the consent at 1000, alert and before any opioid was given", 0,
     "Consent signed while alert and before sedating drugs is valid."),
    ("The cardiologist explained the risks, including bleeding and kidney injury, and the client restated them", 0,
     "Explaining the procedure and risks is the provider's duty; teach-back shows understanding."),
    ("The client now says, \"I'm not sure what they're going to do to my heart\"", 1,
     "New doubt means consent may no longer be informed; the nurse stops and tells the cardiologist, who re-explains."),
    ("The client's son asks to sign a new form because the client is \"too stressed\"", 1,
     "A capable adult consents for themselves; the nurse explains this and does not accept the son's signature."),
    ("The nurse signed the form as a witness to the client's signature", 0,
     "The nurse's role is to witness that the client signed voluntarily."),
    ("The client received morphine 2 mg IV at 1200", 0,
     "Consent was obtained before the opioid, so it stays valid; the dose is documented for the procedure team.")],
   "The nurse witnesses consent and checks it is still informed and voluntary; the provider explains risks. New "
   "doubt or a family member trying to sign for a capable client means the nurse must act.",
   "Analyze whether informed consent is valid before a procedure."),

cloze(M(PA, ACS, "prioritize_hypotheses", "difficult", CV, "aha ms"),
   "Complete the sentences by choosing from the lists of options.",
   "Before catheterization, the client's priority problem is {{b1}}, shown most clearly by {{b2}}. After the "
   "catheterization, the nurse should be most alert for {{b3}} because of {{b4}}.",
   [[("ongoing myocardial ischemia", True, "Rest pain with ST depression and rising troponin is active ischemia."),
     ("acute decompensated heart failure", False, "There are no crackles, edema or hypoxemia below 90% to support it."),
     ("acute pericarditis", False, "Pericarditis gives diffuse ST elevation and pain that changes with position."),
     ("hypertensive emergency", False, "A blood pressure of 158/94 is high but not in the emergency range.")],
    [("the rising troponin with ST depression", True, "These show the infarction is evolving."),
     ("the blood pressure of 158/94 mm Hg", False, "Raised pressure adds workload but does not prove ischemia."),
     ("the blood glucose of 212 mg/dL", False, "Stress hyperglycemia is common and needs treatment, but it is not the evidence of ischemia."),
     ("the respiratory rate of 22/min", False, "A mild rise in rate is not specific to ischemia.")],
    [("acute kidney injury", True, "Contrast plus baseline kidney disease, diabetes and NSAIDs make kidney injury the key risk."),
     ("an allergic reaction to contrast", False, "Possible, but nothing in the history points to a contrast allergy."),
     ("hypoglycemia", False, "The client's glucose is high, and metformin alone rarely causes hypoglycemia."),
     ("hyperthyroidism", False, "Iodine-induced thyroid problems are rare and late.")],
    [("the creatinine of 1.6 mg/dL with diabetes and daily NSAID use", True,
      "Each of these lowers kidney reserve before the contrast load."),
     ("the heart rate of 104/min", False, "Tachycardia does not predict kidney injury from contrast."),
     ("the platelet count of 230,000/mm3", False, "This is normal."),
     ("the history of smoking", False, "Smoking raises coronary risk, not contrast kidney injury.")]],
   "The first priority is ongoing ischemia; the next predictable danger is contrast-associated kidney injury in "
   "a client with CKD, diabetes and NSAID use.",
   "Prioritize hypotheses before and after catheterization."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "aha ph"),
   "The provider writes the prescriptions shown. For each prescription, indicate whether it is indicated, "
   "not indicated, or contraindicated for the client at this time.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Aspirin 324 mg chewed now", 0,
     "Chewed aspirin acts fastest and reduces death in acute coronary syndrome."),
    ("Unfractionated heparin by weight-based bolus and infusion", 0,
     "Anticoagulation limits clot growth until the coronary anatomy is known."),
    ("Oxygen at 2 L/min by nasal cannula", 1,
     "With SpO2 at 93 to 94%, extra oxygen gives no benefit and may cause vasoconstriction."),
    ("Ketorolac 30 mg IV for knee pain", 2,
     "NSAIDs raise the risk of reinfarction and bleeding and worsen kidney function before contrast."),
    ("Metformin at the scheduled 1800 dose", 2,
     "Metformin is held on the day of contrast and for 48 hours after in a client with reduced kidney "
     "function, until creatinine is rechecked, because of the risk of lactic acidosis."),
    ("Furosemide 40 mg IV before the contrast study", 1,
     "Diuretics do not prevent contrast kidney injury and may worsen it by reducing volume; IV isotonic saline is used."),
    ("Atorvastatin 80 mg daily", 0,
     "High-intensity statin therapy is started early in acute coronary syndrome whatever the cholesterol level.")],
   "Indicated: chewed aspirin, heparin and a high-intensity statin. Not indicated: oxygen above 90% and diuretics for "
   "kidney protection. Contraindicated: NSAIDs, and metformin around contrast with reduced kidney function.",
   "Generate solutions for NSTEMI, including drugs to hold.",
   ex=[note("Prescriptions, 0930",
       "Aspirin 324 mg chewed now. Heparin 60 units/kg IV bolus, then 12 units/kg/hour. Oxygen 2 L/min by nasal "
       "cannula. Ketorolac 30 mg IV every 6 hours as needed for knee pain. Continue metformin 1,000 mg twice daily. "
       "Furosemide 40 mg IV before the contrast study. Atorvastatin 80 mg daily.", id="nstemi-orders")]),

sata(M(RRP, POSTPROC, "take_action", "difficult", CV, "ms aha"),
   "Review the 1530 update. Which actions should the nurse take now?",
   [("Stop the heparin infusion and call the provider", True,
     "Back pain with falling pressure and hemoglobin after femoral access is a retroperitoneal bleed; anticoagulation stops."),
    ("Apply firm pressure just above the puncture site", True,
     "Pressure over the femoral artery above the skin puncture compresses the arterial entry point, where the bleeding starts."),
    ("Keep the client flat with the right leg straight", True,
     "Flexing the hip can reopen the arterial puncture; flat positioning also supports perfusion."),
    ("Give IV 0.9% sodium chloride as a bolus as prescribed", True,
     "Isotonic fluid supports perfusion while blood is typed and crossmatched."),
    ("Raise the head of the bed to 45 degrees to ease back pain", False,
     "Hip flexion increases bleeding from the femoral artery."),
    ("Give the prescribed morphine for the back pain first", False,
     "Opioids lower blood pressure further and hide the sign that tracks the bleed."),
    ("Recheck the hemoglobin in 4 hours before calling the provider", False,
     "Hemoglobin lags behind blood loss; the vital signs already show shock developing.")],
   "Retroperitoneal bleeding shows as back or flank pain, hypotension and tachycardia with a clean groin dressing. "
   "Stop anticoagulants, compress above the puncture, keep the leg straight, support volume and call the provider.",
   "Take action for a retroperitoneal bleed after femoral catheterization.",
   ex=[flow("Update after catheterization (right femoral approach, stent placed)", ["1330", "1530"], [
       ("Blood pressure", ["138/80 mm Hg", "92/58 mm Hg"]),
       ("Heart rate", ["88/min", "122/min"]),
       ("Client report", ["No pain", "New low back and right flank pain, 7 of 10; restless"]),
       ("Groin dressing", ["Dry, no hematoma", "Dry, no hematoma"]),
       ("Right pedal pulses", ["2+", "2+"]),
       ("Hemoglobin", ["13.1 g/dL", "9.8 g/dL"]),
       ("Heparin infusion", ["Running", "Running"])], id="nstemi-1530")]),

matrix_mc(M(RRP, CONTRAST, "evaluate_outcomes", "difficult", CV, "ms lab"),
   "The heparin was stopped, pressure was held and 2 units of red blood cells were given. Review the 2200 "
   "findings. For each finding, indicate whether it shows the treatment is working or needs follow-up.",
   ["Treatment working", "Needs follow-up"],
   [("Blood pressure 118/72 mm Hg, heart rate 96/min", 0,
     "Pressure and rate have recovered, so blood loss has slowed and volume has been restored."),
    ("Back pain 2 of 10", 0, "Falling pain suggests the hematoma is no longer expanding."),
    ("Hemoglobin 9.6 g/dL after 2 units", 0,
     "Each unit should raise hemoglobin about 1 g/dL; 9.6 after the low point of about 8 is the expected rise."),
    ("Creatinine 2.3 mg/dL (1.6 before the procedure)", 1,
     "A rise of 0.3 mg/dL or more within 48 hours after contrast is acute kidney injury; it needs reporting and fluid review."),
    ("Right foot cooler than the left with a weak dorsalis pedis pulse", 1,
     "A new pulse deficit suggests arterial blockage or compression from the hematoma; this threatens the limb."),
    ("Urine output 45 mL/hour", 0, "Output above 0.5 mL/kg/hour shows adequate kidney perfusion for now.")],
   "Recovering vital signs, less pain and the expected transfusion rise show the bleed is controlled. Rising "
   "creatinine and a new pulse deficit are new problems that need the provider now.",
   "Evaluate outcomes after a retroperitoneal bleed and contrast exposure.",
   ex=[flow("Reassessment", ["1530", "2200"], [
       ("Blood pressure", ["92/58 mm Hg", "118/72 mm Hg"]),
       ("Heart rate", ["122/min", "96/min"]),
       ("Back pain", ["7 of 10", "2 of 10"]),
       ("Hemoglobin", ["9.8 g/dL (low point 8.1)", "9.6 g/dL"]),
       ("Creatinine", ["1.6 mg/dL", "2.3 mg/dL"]),
       ("Right foot", ["Warm, pulses 2+", "Cooler than left, dorsalis pedis 1+"]),
       ("Urine output", ["20 mL/hour", "45 mL/hour"])], id="nstemi-2200")]),
]

# ================================================================================================
# Case 2: Heart failure with digoxin toxicity, hypokalemia and a drug interaction
# ================================================================================================
dig_tabs = [
    ("dig-notes", "Nurses' notes", [note("Medical unit admission, 1000",
        "A 78-year-old client with heart failure (ejection fraction 25%) and chronic atrial fibrillation is "
        "admitted with 4 days of nausea, poor appetite and fatigue. The client says, \"Everything looks yellow, "
        "and there are rings around the lights.\" The client's daughter reports new confusion since yesterday. "
        "Furosemide was increased from 40 mg daily to 40 mg twice daily 10 days ago, and clarithromycin was "
        "started 5 days ago for bronchitis. Other medications: digoxin 0.25 mg daily, carvedilol 12.5 mg twice "
        "daily, lisinopril 10 mg daily. Lungs clear, 1+ ankle edema (the client says this is usual), weight 62 kg "
        "(64 kg one week ago). Monitor: slow atrial fibrillation with frequent premature ventricular contractions.",
        id="dig-note")]),
    ("dig-vitals", "Vital signs", [flow("Vital signs", ["1000"], [
        ("Blood pressure", ["102/60 mm Hg (sitting); 84/50 mm Hg (standing)"]),
        ("Apical pulse", ["48/min, irregular"]),
        ("Respirations", ["18/min"]),
        ("SpO2", ["96% on room air"])], id="dig-flow")]),
    ("dig-labs", "Laboratory results", [table("Laboratory results", ["1000", "Reference range"], [
        ("Digoxin", ["3.1 ng/mL", "0.5 to 2.0 ng/mL (0.5 to 0.9 in heart failure)"]),
        ("Potassium", ["2.9 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("Magnesium", ["1.4 mg/dL", "1.5 to 2.5 mg/dL"]),
        ("Creatinine", ["1.9 mg/dL (1.2 last month)", "0.6 to 1.2 mg/dL"]),
        ("BUN", ["48 mg/dL", "10 to 20 mg/dL"]),
        ("Sodium", ["136 mEq/L", "135 to 145 mEq/L"])], id="dig-lab")]),
]

dig_qs = [
hl_text(M(PPT, ADVERSE, "recognize_cues", "moderate", CV, "ph ms"),
   "Review the nurses' notes. Highlight the findings that are most likely related to the client's medications.",
   ["A 78-year-old client with heart failure is admitted with ",
    ("4 days of nausea and poor appetite", True, "Anorexia and nausea are early signs of digoxin toxicity."),
    ". The client reports ",
    ("seeing yellow and rings around lights", True, "Yellow-green vision and halos are classic for digoxin toxicity."),
    " and has ",
    ("new confusion since yesterday", True, "Confusion in an older adult is a common sign of digoxin toxicity."),
    ". The client has ",
    ("1+ ankle edema that the client says is usual", False, "A stable baseline finding of chronic heart failure."),
    " and ",
    ("lungs that are clear", False, "Clear lungs argue against fluid overload; not a drug effect."),
    ". Weight is ",
    ("62 kg, down 2 kg in a week", True, "Weight loss after the furosemide increase points to volume depletion, which concentrates digoxin."),
    ", and the apical pulse is ",
    ("48/min with frequent PVCs", True, "Bradycardia and ventricular ectopy are cardiac signs of digoxin toxicity."),
    "."],
   "Gastrointestinal, visual and neurologic symptoms with slow pulse and ectopy fit digoxin toxicity, made worse "
   "by diuretic volume loss.",
   "Recognize cues of digoxin toxicity."),

matrix_mc(M("SECE.SIC", "medication-safety-systems", "analyze_cues", "difficult", CV, "ph mgmt"),
   "The nurse reviews how the client came to be toxic. For each factor, indicate whether it contributed to the "
   "event.",
   ["Contributed", "Did not contribute"],
   [("A walk-in clinic prescribed clarithromycin without a current medication list", 0,
     "Without the list, the prescriber could not see the digoxin interaction."),
    ("The antibiotic was filled at a different pharmacy from the heart medicines", 0,
     "Using two pharmacies defeats the pharmacy's interaction alert."),
    ("Furosemide was doubled with no follow-up potassium or creatinine check", 0,
     "A dose increase of a loop diuretic needs a metabolic panel within about a week."),
    ("The client takes digoxin at the same time each morning", 1, "Consistent timing is correct practice."),
    ("The client did not know the early signs of digoxin toxicity", 0,
     "Nausea and visual changes were present for days before admission; teaching would have prompted an earlier call."),
    ("The client keeps medications in the original labeled bottles", 1, "This supports safe self-administration.")],
   "Medication events usually come from system gaps: no shared medication list, several pharmacies, missing "
   "laboratory follow-up and incomplete teaching. Reconciliation at every visit closes these gaps.",
   "Analyze system factors in a medication-related adverse event."),

cloze(M(PA, LYTES, "prioritize_hypotheses", "difficult", CV, "ph ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client's most immediate risk is {{b1}}, made more likely by {{b2}}.",
   [[("a life-threatening ventricular dysrhythmia", True,
      "Digoxin toxicity with hypokalemia causes ventricular tachycardia and fibrillation."),
     ("acute pulmonary edema", False, "Lungs are clear and the client is volume depleted."),
     ("hyperkalemic cardiac arrest", False, "Acute massive digoxin overdose raises potassium; this client's is low."),
     ("an acute ischemic stroke", False, "Anticoagulation questions matter in AF, but confusion here fits toxicity.")],
    [("the potassium of 2.9 mEq/L and magnesium of 1.4 mg/dL", True,
      "These lower the threshold for digoxin-induced dysrhythmias."),
     ("the 1+ ankle edema", False, "Edema is chronic and stable."),
     ("the sodium of 136 mEq/L", False, "Normal."),
     ("the SpO2 of 96%", False, "Normal oxygenation.")]],
   "Digoxin toxicity plus low potassium and magnesium puts the client at highest risk of a lethal ventricular rhythm.",
   "Prioritize the most dangerous outcome of digoxin toxicity."),

matrix_mc(M(PPT, HF_RX, "generate_solutions", "difficult", CV, "ph"),
   "For each prescription, indicate whether it is indicated, not indicated, or contraindicated for the client now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Hold digoxin", 0, "The drug is stopped until the level and the rhythm are safe."),
    ("Potassium chloride 10 mEq/hour IV through an infusion pump", 0,
     "Potassium must be corrected; 10 mEq/hour is a safe peripheral rate with cardiac monitoring."),
    ("Magnesium sulfate 2 g IV over 1 hour", 0, "Correcting magnesium stabilizes the heart and helps potassium correct."),
    ("Furosemide 40 mg IV now", 2,
     "The client is dry, with low potassium and rising creatinine; more diuresis worsens all three."),
    ("Continue clarithromycin until the course is finished", 2,
     "The interaction is raising the digoxin level; the provider should switch to an antibiotic that does not interact."),
    ("Restrict fluids to 1.5 L per day", 1,
     "The client is volume depleted with orthostatic hypotension; restriction is not needed now."),
    ("Atropine available at the bedside", 0, "Atropine treats symptomatic bradycardia while the toxicity clears.")],
   "Stop digoxin, correct potassium and magnesium, stop the interacting antibiotic and the extra diuresis, and "
   "have atropine ready.",
   "Generate solutions for digoxin toxicity.",
   ex=[note("Prescriptions, 1100",
       "Hold digoxin. Potassium chloride 40 mEq IV at 10 mEq/hour. Magnesium sulfate 2 g IV over 1 hour. "
       "Furosemide 40 mg IV now. Continue clarithromycin. Fluid restriction 1.5 L/day. Atropine 1 mg at the "
       "bedside. Continuous cardiac monitoring.", id="dig-orders")]),

sata(M(PPT, ANTIDOTE, "take_action", "difficult", CV, "ph aha"),
   "At 1400, the client has a 7-beat run of ventricular tachycardia and the apical pulse falls to 38/min with "
   "BP 80/48 mm Hg. The provider prescribes digoxin immune fab. Which actions should the nurse take?",
   [("Give the digoxin immune fab over 30 minutes through a filter as prescribed", True,
     "Fab fragments bind digoxin and reverse life-threatening toxicity within an hour."),
    ("Place transcutaneous pacing pads on the client", True,
     "Severe bradycardia with hypotension may need pacing until the antidote works."),
    ("Recheck potassium within 1 to 2 hours after the dose", True,
     "As the pump is freed, potassium moves back into cells, so hypokalemia can worsen quickly."),
    ("Draw a digoxin level 1 hour after the dose to judge its effect", False,
     "Most tests measure bound plus free digoxin after fab, so the level rises falsely; judge by the rhythm and symptoms."),
    ("Prepare for synchronized cardioversion at 200 joules", False,
     "Cardioversion in digoxin toxicity can trigger ventricular fibrillation; it is not the first step."),
    ("Give the 1200 dose of carvedilol now", False, "A beta blocker would slow the heart further.")],
   "Give the antidote, be ready to pace, and watch potassium closely because it falls as fab works. Serum levels "
   "after fab are misleading.",
   "Take action for life-threatening digoxin toxicity.",
   ex=[flow("Monitor, 1400", ["1300", "1400"], [
       ("Rhythm", ["Slow atrial fibrillation with PVCs", "7-beat run of ventricular tachycardia, then slow AF"]),
       ("Apical pulse", ["46/min", "38/min"]),
       ("Blood pressure", ["98/58 mm Hg", "80/48 mm Hg"]),
       ("Potassium (1300)", ["3.2 mEq/L", "-"])], id="dig-1400")]),

matrix_mc(M(PA, HF_ASSESS, "evaluate_outcomes", "difficult", CV, "ph ms"),
   "Review the next morning's findings. For each finding, indicate whether it shows improvement, is expected after "
   "treatment, or needs follow-up.",
   ["Shows improvement", "Expected after treatment", "Needs follow-up"],
   [("Apical pulse 68/min, atrial fibrillation without ventricular runs", 0,
     "The rate has recovered and the ventricular ectopy has stopped."),
    ("Total digoxin level reported as 8.4 ng/mL", 1,
     "After fab, the test counts bound, inactive digoxin, so a high total level is expected and is not treated."),
    ("Potassium 4.0 mEq/L, magnesium 2.0 mg/dL", 0, "Both are corrected."),
    ("Nausea gone; the client is oriented and reading the menu", 0, "Toxic symptoms have resolved."),
    ("Weight up 1.5 kg overnight with new crackles at both bases", 2,
     "With digoxin removed and fluids given, heart failure is decompensating; this needs the provider now."),
    ("Creatinine 1.4 mg/dL", 0, "Kidney function is improving with rehydration.")],
   "Rhythm, electrolytes and symptoms show the toxicity is treated; the raised total level after fab is expected. "
   "Removing digoxin's support can unmask heart failure, so new crackles and weight gain need action.",
   "Evaluate outcomes after digoxin immune fab.",
   ex=[flow("Next morning, 0700", ["0700"], [
       ("Rhythm and rate", ["Atrial fibrillation 68/min, no ventricular runs"]),
       ("Digoxin (total)", ["8.4 ng/mL"]),
       ("Potassium / magnesium", ["4.0 mEq/L / 2.0 mg/dL"]),
       ("Symptoms", ["No nausea; oriented; reading the menu"]),
       ("Weight", ["63.5 kg (62.0 yesterday)"]),
       ("Lungs", ["Fine crackles at both bases"]),
       ("Creatinine", ["1.4 mg/dL"])], id="dig-0700")]),
]

# ================================================================================================
# Case 3: New atrial fibrillation with a rapid ventricular rate from thyrotoxicosis
# ================================================================================================
af_tabs = [
    ("af-notes", "Nurses' notes", [note("Emergency department, 1600",
        "A 62-year-old client reports a racing heart and shortness of breath that began \"three or four days "
        "ago.\" The client has lost 5 kg in 2 months without trying, feels hot, and has a fine tremor of the "
        "hands. History: hypertension, type 2 diabetes, a transient ischemic attack 2 years ago. An echocardiogram "
        "last year showed an ejection fraction of 55%. Home medications: amlodipine 5 mg daily, metformin 500 mg "
        "twice daily. The monitor shows an irregularly irregular narrow-complex rhythm with no visible P waves.",
        id="af-note")]),
    ("af-vitals", "Vital signs", [flow("Vital signs", ["1600"], [
        ("Blood pressure", ["108/68 mm Hg"]),
        ("Heart rate", ["148/min, irregular"]),
        ("Respirations", ["24/min"]),
        ("SpO2", ["94% on room air"]),
        ("Temperature", ["37.6 C (99.7 F)"])], id="af-flow")]),
    ("af-labs", "Laboratory results", [table("Laboratory results", ["1600", "Reference range"], [
        ("TSH", ["0.02 mIU/L", "0.4 to 4.0 mIU/L"]),
        ("Free T4", ["3.9 ng/dL", "0.8 to 1.8 ng/dL"]),
        ("Potassium", ["3.8 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("Troponin I", ["< 0.04 ng/mL", "< 0.04 ng/mL"]),
        ("Creatinine", ["0.9 mg/dL", "0.6 to 1.2 mg/dL"])], id="af-lab")]),
]

af_qs = [
hl_table(M(PA, DYS, "recognize_cues", "moderate", CV, "aha ms"),
   "Highlight the findings that are most important for planning the client's care.",
   ["Assessment", "Finding"],
   [(["Rhythm", "Irregularly irregular, no P waves, rate 148/min"], True,
     "This is atrial fibrillation with a rapid ventricular rate."),
    (["Symptom onset", "Three or four days ago"], True,
     "AF lasting 48 hours or longer may already have formed an atrial clot, which changes how rhythm is restored."),
    (["History", "Transient ischemic attack 2 years ago"], True,
     "Prior TIA, hypertension, diabetes and age give a high stroke risk, so anticoagulation is needed."),
    (["Thyroid tests", "TSH 0.02 mIU/L, free T4 3.9 ng/dL"], True,
     "Thyrotoxicosis is a reversible cause of the AF and must be treated for rate control to work."),
    (["Troponin I", "< 0.04 ng/mL"], False, "Normal; no sign of infarction."),
    (["Potassium", "3.8 mEq/L"], False, "Within range."),
    (["Ejection fraction", "55% last year"], False,
     "A normal ejection fraction is reassuring; it allows a calcium channel blocker for rate control.")],
   "The key cues are the rhythm, how long it has lasted, the stroke risk and the thyroid cause.",
   "Recognize cues in new atrial fibrillation."),

cloze(M(PA, DYS, "analyze_cues", "difficult", CV, "aha ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's shortness of breath is best explained by {{b1}}. Because the AF began more than 48 hours ago, "
   "restoring sinus rhythm now would raise the risk of {{b2}}. The thyroid results suggest the AF is {{b3}}.",
   [[("a short filling time that lowers cardiac output", True,
      "At 148/min the ventricles have little time to fill, and the atrial kick is lost, so output falls and pressure backs up."),
     ("a pulmonary embolism from the atrial clot", False, "Clots from the left atrium go to the systemic circulation, not the lungs."),
     ("an acute myocardial infarction", False, "Troponin is normal."),
     ("a reaction to amlodipine", False, "Amlodipine does not cause this.")],
    [("an embolic stroke", True,
      "A clot formed in the fibrillating atrium can be thrown out when organized contraction returns."),
     ("ventricular fibrillation", False, "Not the main risk of restoring rhythm after 48 hours."),
     ("hyperkalemia", False, "Unrelated."),
     ("hypothyroidism", False, "Unrelated.")],
    [("driven by thyrotoxicosis", True,
      "Excess thyroid hormone raises sympathetic tone and atrial irritability; weight loss, heat and tremor fit."),
     ("caused by the TIA", False, "A TIA is a consequence of AF, not a cause."),
     ("unrelated to the weight loss", False, "The weight loss is part of the thyrotoxic picture."),
     ("caused by low potassium", False, "Potassium is normal.")]],
   "AF with rapid rate lowers output through short filling and lost atrial kick; after 48 hours, cardioversion "
   "risks stroke; thyrotoxicosis is the driver.",
   "Analyze cues in atrial fibrillation with thyrotoxicosis."),

matrix_mc(M("SECE.SIC", "injury-prevention", "prioritize_hypotheses", "difficult", CV, "ms fund"),
   "The client is starting diltiazem and heparin and has had a fine tremor and weight loss. For each safety risk, "
   "indicate its priority for the nurse's plan in the first 24 hours.",
   ["High priority", "Lower priority"],
   [("A fall when getting up to the bathroom", 0,
     "Calcium channel blockers lower blood pressure; with weakness from thyrotoxicosis, a fall on heparin could "
     "cause serious bleeding."),
    ("Bleeding from the heparin infusion", 0, "Heparin plus planned oral anticoagulation needs bleeding checks."),
    ("An embolic stroke while the rate is being controlled", 0,
     "AF of more than 48 hours with prior TIA carries a high stroke risk until anticoagulation is therapeutic."),
    ("A pressure injury", 1, "The client is mobile and well nourished enough; risk is low over 24 hours."),
    ("Aspiration during meals", 1, "Nothing suggests a swallowing problem.")],
   "Safety planning ranks the risks the treatment creates (hypotension and falls, bleeding) together with the "
   "disease's own risk (stroke).",
   "Prioritize safety risks during rate control and anticoagulation."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph aha"),
   "For each prescription, indicate whether it is indicated, not indicated, or contraindicated for the client now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Diltiazem 0.25 mg/kg IV over 2 minutes, then an infusion", 0,
     "A calcium channel blocker slows AV conduction; it is safe with a normal ejection fraction and adequate pressure."),
    ("Synchronized cardioversion now", 2,
     "The client is stable and AF has lasted more than 48 hours without anticoagulation or an echo to exclude a clot."),
    ("Adenosine 6 mg rapid IV push", 1, "Adenosine is for re-entrant SVT; it does not treat AF."),
    ("Heparin infusion, then oral anticoagulation", 0,
     "High stroke risk needs anticoagulation now and for at least 4 weeks after any cardioversion."),
    ("Methimazole as prescribed by endocrinology", 0, "Treating the thyrotoxicosis removes the cause."),
    ("Levothyroxine 50 mcg daily", 2, "Thyroid hormone would worsen thyrotoxicosis and the AF.")],
   "Control the rate, anticoagulate and treat the thyroid. Do not cardiovert a stable client with AF of more than "
   "48 hours, and do not give drugs meant for other rhythms or for hypothyroidism.",
   "Generate solutions for AF with a rapid ventricular rate.",
   ex=[note("Prescriptions, 1630",
       "Diltiazem 0.25 mg/kg IV over 2 minutes, then 10 mg/hour. Synchronized cardioversion. Adenosine 6 mg IV. "
       "Heparin by protocol. Methimazole 20 mg daily. Levothyroxine 50 mcg daily.", id="af-orders")]),

sata(M(PPT, ADVERSE, "take_action", "difficult", CV, "ph aha"),
   "Review the 1745 findings. Which actions should the nurse take?",
   [("Stop the diltiazem infusion", True, "Hypotension from the infusion means the drug should be stopped first."),
    ("Lower the head of the bed and raise the legs", True, "This improves venous return and cerebral perfusion."),
    ("Notify the provider of the blood pressure and rhythm", True, "The rate plan needs to change."),
    ("Give the IV fluid bolus prescribed for hypotension", True, "Volume supports pressure after vasodilation."),
    ("Raise the diltiazem to 15 mg/hour to keep the rate below 100", False,
     "A higher dose would worsen hypotension; the rate is already controlled."),
    ("Give the client's evening dose of amlodipine", False,
     "A second calcium channel blocker would lower the blood pressure further."),
    ("Give adenosine to slow the rate further", False, "The rate is controlled, and adenosine does not treat AF.")],
   "Calcium channel blockers dilate vessels as well as slowing the heart; when hypotension appears, stop the drug, "
   "support perfusion and tell the provider. Hold other blood pressure drugs.",
   "Take action for hypotension during diltiazem infusion.",
   ex=[flow("After the diltiazem bolus and infusion", ["1630", "1745"], [
       ("Heart rate", ["148/min", "92/min"]),
       ("Blood pressure", ["108/68 mm Hg", "82/48 mm Hg"]),
       ("Client report", ["Short of breath", "Dizzy and light-headed when sitting up"]),
       ("Diltiazem infusion", ["Started at 10 mg/hour", "Running at 10 mg/hour"])], id="af-1745")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", CV, "ph"),
   "The client is ready for discharge on apixaban, metoprolol and methimazole. For each statement, indicate whether "
   "it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I will need regular INR blood tests to adjust my apixaban dose.\"", 1,
     "Apixaban does not need INR monitoring; that applies to warfarin."),
    ("\"If I get a sore throat with a fever, I'll stop the methimazole and call.\"", 0,
     "Sore throat and fever can mean agranulocytosis, a serious reaction to methimazole."),
    ("\"I can take ibuprofen for my headaches as before.\"", 1,
     "NSAIDs with an anticoagulant raise bleeding risk; acetaminophen is preferred."),
    ("\"I'll tell my dentist I take apixaban before any extraction.\"", 0,
     "Procedures may need the anticoagulant to be held under the prescriber's guidance."),
    ("\"If I miss a dose of apixaban, I'll take two at the next dose.\"", 1,
     "Doubling doses raises bleeding risk; a missed dose is taken the same day if remembered, then the usual schedule."),
    ("\"I'll check my pulse and call if it's under 60 or I feel faint.\"", 0,
     "Beta blockers slow the heart; this shows safe self-monitoring.")],
   "Discharge teaching is effective when the client knows apixaban needs no INR, avoids NSAIDs and double doses, "
   "and knows the warning signs of methimazole and beta blockers.",
   "Evaluate teaching for anticoagulants, beta blockers and antithyroid drugs.",
   ex=[note("Discharge, day 3",
       "Rate 78/min in atrial fibrillation. Discharge medications: apixaban 5 mg twice daily, metoprolol "
       "succinate 50 mg daily, methimazole 20 mg daily. Amlodipine stopped. Follow-up with cardiology and "
       "endocrinology in 2 weeks.", id="af-discharge")]),
]

# ================================================================================================
# Case 4: Hypertensive emergency after stopping clonidine
# ================================================================================================
htn_tabs = [
    ("htn-notes", "Nurses' notes", [note("Emergency department, 2200",
        "A 54-year-old client reports a severe headache since this morning and blurred vision. The client ran out "
        "of clonidine 0.2 mg twice daily 2 days ago and \"didn't think it mattered.\" History: hypertension for "
        "15 years, asthma (uses an albuterol inhaler twice a week). The client is restless and slow to answer "
        "questions, and is oriented to person and place only. Fundoscopic examination by the provider shows "
        "papilledema. The client is sweating and anxious.", id="htn-note")]),
    ("htn-vitals", "Vital signs", [flow("Vital signs", ["2200"], [
        ("Blood pressure", ["228/134 mm Hg"]),
        ("Heart rate", ["110/min"]),
        ("Respirations", ["20/min"]),
        ("SpO2", ["96% on room air"])], id="htn-flow")]),
    ("htn-labs", "Laboratory results", [table("Laboratory results", ["2200", "Reference range"], [
        ("Creatinine", ["2.0 mg/dL (1.0 six months ago)", "0.6 to 1.2 mg/dL"]),
        ("Urinalysis", ["Protein 2+, red blood cells 10 to 20/hpf", "Negative"]),
        ("Troponin I", ["0.12 ng/mL", "< 0.04 ng/mL"]),
        ("Potassium", ["3.9 mEq/L", "3.5 to 5.0 mEq/L"])], id="htn-lab")]),
]

htn_qs = [
hl_text(M(PA, HTN_CRISIS, "recognize_cues", "moderate", CV, "ms"),
   "Review the nurses' notes. Highlight the findings that show damage to a target organ.",
   ["The client has ",
    ("a severe headache with blurred vision", True, "Headache with visual change reflects cerebral and retinal effects of severe pressure."),
    " and ",
    ("is oriented to person and place only", True, "New confusion suggests hypertensive encephalopathy."),
    ". The provider notes ",
    ("papilledema on fundoscopic examination", True, "Papilledema shows raised intracranial pressure or retinal damage."),
    ". The client is ",
    ("sweating and anxious", False, "Common with clonidine rebound; a symptom, not organ damage."),
    ". The creatinine is ",
    ("2.0 mg/dL, double the value 6 months ago", True, "An acute rise in creatinine shows kidney injury."),
    ", with ",
    ("protein and red cells in the urine", True, "These point to acute kidney damage from severe pressure."),
    ". The heart rate is ",
    ("110/min", False, "Tachycardia fits clonidine rebound but is not evidence of organ damage by itself."),
    "."],
   "Hypertensive emergency is defined by acute target-organ damage: brain (encephalopathy, papilledema), kidney "
   "(rising creatinine, proteinuria, hematuria) or heart.",
   "Recognize target-organ damage in severe hypertension."),

cloze(M(PA, HTN_CRISIS, "analyze_cues", "difficult", CV, "ms ph"),
   "Complete the sentence by choosing from the lists of options.",
   "This is hypertensive {{b1}} because {{b2}}, and the most likely trigger is {{b3}}.",
   [[("emergency", True, "Severe pressure with acute organ damage is an emergency."),
     ("urgency", False, "Urgency is severe pressure without acute organ damage."),
     ("crisis of pregnancy", False, "Not applicable."),
     ("white-coat effect", False, "Organ damage rules this out.")],
    [("there is acute damage to the brain, kidneys and heart", True,
      "Encephalopathy, rising creatinine and a raised troponin show damage in three organs."),
     ("the blood pressure is above 180/120 mm Hg", False, "Pressure alone does not separate emergency from urgency."),
     ("the client has a headache", False, "Headache occurs in both."),
     ("the heart rate is above 100/min", False, "Not part of the definition.")],
    [("abrupt stopping of clonidine", True,
      "Stopping clonidine suddenly causes rebound sympathetic surge with severe hypertension, tachycardia and sweating."),
     ("albuterol use for asthma", False, "Twice-weekly albuterol does not explain this."),
     ("hypokalemia", False, "Potassium is normal."),
     ("a high-sodium diet", False, "Nothing in the data suggests it, and it would not explain the sweating and tachycardia.")]],
   "Organ damage makes it an emergency; the rebound sympathetic picture after clonidine is stopped is the trigger.",
   "Analyze hypertensive emergency and its trigger."),

cloze(M("SECE.MOC", "assignment-and-staffing", "prioritize_hypotheses", "difficult", CV, "mgmt ms"),
   "The emergency department charge nurse is deciding where to admit the client. Complete the sentences by choosing "
   "from the lists of options.",
   "The client should be admitted to {{b1}} because {{b2}}. The client should be assigned to {{b3}}.",
   [[("the intensive care unit", True,
      "A titrated vasoactive infusion with an arterial line and acute organ damage needs ICU monitoring."),
     ("a medical-surgical unit with telemetry", False, "Telemetry does not provide arterial line titration."),
     ("an observation unit", False, "Observation is for clients expected to go home within a day."),
     ("home with follow-up tomorrow", False, "Hypertensive emergency is never managed at home.")],
    [("the nicardipine needs titration every few minutes against arterial line readings", True,
      "Frequent titration and neurologic checks need a nurse-to-client ratio of 1:1 or 1:2."),
     ("the client has asthma", False, "Asthma does not decide the level of care here."),
     ("the client stopped clonidine", False, "The cause does not set the level of care."),
     ("the troponin is 0.12 ng/mL", False, "One of several factors, but the infusion and monitoring decide it.")],
    [("an ICU RN experienced with titrated vasoactive infusions", True,
      "The most complex part of care must go to the most qualified nurse."),
     ("a float nurse from a medical unit", False, "Titration in hypertensive emergency needs ICU competence."),
     ("an LPN supervised by the charge nurse", False, "An unstable client on a titrated infusion needs an RN."),
     ("a newly hired ICU nurse still in orientation", False, "Without a preceptor, this assignment is unsafe.")]],
   "Level of care and assignment follow the client's needs: a titrated vasoactive drip with an arterial line goes "
   "to the ICU and to an RN competent with it.",
   "Prioritize the level of care and assignment for a client in hypertensive emergency."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph ms"),
   "For each part of the plan, indicate whether it is indicated or contraindicated for this client.",
   ["Indicated", "Contraindicated"],
   [("Nicardipine IV infusion, titrated to the target", 0,
     "A titratable IV calcium channel blocker lowers pressure predictably."),
    ("Labetalol 20 mg IV push", 1,
     "Labetalol is a nonselective beta blocker; with asthma it can cause bronchospasm, so another drug is used."),
    ("Lower the mean arterial pressure by no more than 25% in the first hour", 0,
     "A controlled fall protects the brain, heart and kidneys, whose autoregulation is reset higher."),
    ("Bring the blood pressure to 120/80 mm Hg within the first hour", 1,
     "A rapid fall to normal can cause stroke, MI or kidney injury from hypoperfusion."),
    ("Arterial line for continuous blood pressure", 0, "Titration needs beat-to-beat readings."),
    ("Nifedipine 10 mg sublingual", 1, "Sublingual nifedipine causes an unpredictable, sharp fall in pressure.")],
   "Use a titratable IV drug suited to the client's other conditions, lower the pressure in a controlled way and "
   "monitor continuously.",
   "Generate solutions for hypertensive emergency.",
   ex=[note("Plan, 2230",
       "Nicardipine infusion. Labetalol 20 mg IV push. Goal: mean arterial pressure down by no more than 25% in the "
       "first hour, then toward 160/100 over the next 2 to 6 hours. Arterial line. Nifedipine 10 mg sublingual.",
       id="htn-plan")]),

sata(M(PA, HTN_CRISIS, "take_action", "difficult", CV, "ms ph"),
   "Review the 2330 findings. The nicardipine was increased twice by the night nurse. Which actions should the "
   "nurse take?",
   [("Decrease the nicardipine following the titration protocol", True,
     "Pressure has fallen too far, too fast; the infusion is reduced to stop the fall."),
    ("Perform a neurologic assessment, including a stroke scale", True,
     "New slurred speech and drowsiness suggest brain hypoperfusion or stroke."),
    ("Notify the provider of the change", True, "A new neurologic deficit needs urgent review and imaging."),
    ("Keep the head of the bed flat or low to support brain perfusion", True,
     "This helps cerebral blood flow while pressure is corrected, unless aspiration risk prevents it."),
    ("Give the client's home clonidine dose now", False, "Further lowering would worsen hypoperfusion."),
    ("Raise the head of the bed to 90 degrees", False, "Upright positioning lowers cerebral perfusion."),
    ("Continue the current rate and recheck in an hour", False, "A neurologic change needs action now.")],
   "Lowering pressure too fast can cause brain ischemia. Reduce the drug, assess neurologically, support perfusion "
   "and call the provider.",
   "Take action when blood pressure falls too fast.",
   ex=[flow("Arterial line readings", ["2200", "2300", "2330"], [
       ("Blood pressure", ["228/134 mm Hg", "176/104 mm Hg", "146/88 mm Hg"]),
       ("Mean arterial pressure", ["165 mm Hg", "128 mm Hg", "107 mm Hg"]),
       ("Neurologic", ["Oriented x2", "Oriented x2", "Drowsy, new slurred speech"]),
       ("Nicardipine", ["5 mg/hour", "10 mg/hour", "15 mg/hour"])], id="htn-2330")]),

matrix_mc(M(HPM, HTN, "evaluate_outcomes", "difficult", CV, "ms ph"),
   "Review the findings 36 hours after admission. For each finding, indicate whether it shows the expected "
   "progress or needs follow-up.",
   ["Expected progress", "Needs follow-up"],
   [("Blood pressure 162/98 mm Hg on oral medications", 0,
     "Pressure is lowered gradually to about 160/100 in the first day or two, then to goal over weeks."),
    ("Speech normal; alert and oriented x3", 0, "The neurologic change resolved with slower lowering."),
    ("Creatinine 1.6 mg/dL", 0, "Kidney function is improving as pressure comes under control."),
    ("Headache 1 of 10", 0, "Improving."),
    ("\"I stop my blood pressure pills when I feel fine.\"", 1,
     "This belief caused the admission; teaching about rebound and adherence is needed before discharge."),
    ("Plan to stop clonidine on the day of discharge", 1,
     "Clonidine should be tapered if it is stopped; stopping it suddenly would cause rebound again.")],
   "Gradual lowering with neurologic and kidney recovery is the expected course; the remaining risk is "
   "non-adherence and stopping clonidine suddenly.",
   "Evaluate outcomes after hypertensive emergency.",
   ex=[flow("36 hours after admission", ["Day 2, 1000"], [
       ("Blood pressure", ["162/98 mm Hg"]),
       ("Neurologic", ["Alert, oriented x3, speech normal"]),
       ("Creatinine", ["1.6 mg/dL"]),
       ("Headache", ["1 of 10"]),
       ("Client statement", ["\"I stop my blood pressure pills when I feel fine.\""]),
       ("Discharge plan note", ["Clonidine to be stopped on the day of discharge"])], id="htn-day2")]),
]

cases = [
    case("NSTEMI with a retroperitoneal bleed after catheterization", nstemi_tabs, nstemi_qs),
    case("Heart failure with digoxin toxicity", dig_tabs, dig_qs),
    case("New atrial fibrillation with a rapid rate from thyrotoxicosis", af_tabs, af_qs),
    case("Hypertensive emergency after stopping clonidine", htn_tabs, htn_qs),
]

BUILT = build(100, "cardiac-cases-acs-hf-af-htn",
              "Cardiac case studies: NSTEMI with a retroperitoneal bleed, digoxin toxicity, atrial fibrillation with "
              "thyrotoxicosis, and hypertensive emergency",
              items, cases, case_conversion=True)
