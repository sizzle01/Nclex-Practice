"""Batch 103 (case conversion, chunk 1: cardiac): four full NGN case studies and four standalone items
(CASE-MAP.md: the 15% standalone share is bowtie, trend and multi-client priority or delegation items).

Cases: anaphylactic shock in a client taking a beta blocker; arterial line and CVP monitoring after open
abdominal aortic aneurysm repair; infective endocarditis in a client who injects drugs; acute limb ischemia
with compartment syndrome after embolectomy. Each case walks the six CJMM steps in order and unfolds with new
data in steps 4 to 6."""
from qb import *

RRP, PA, PPT, MOC = "PHI.RRP", "PHI.PA", "PHI.PPT", "SECE.MOC"
CV = "cardiovascular"
SHOCK = "shock-and-sepsis"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
CV_MEDS = "cardiovascular-medications"
ADVERSE = "adverse-effects-of-medications"
POSTOP = "postoperative-care"
VASC = "vascular-disorders"
VALVE = "inflammatory-and-valvular-heart-disease"
ABX = "antimicrobial-therapy"
HF_ASSESS = "heart-failure-assessment"
PRIORITY = "prioritizing-client-care"
DELEGATE = "delegation-and-supervision"

# ================================================================================================
# Standalone items: bowtie, trend, multi-client priority, delegation
# ================================================================================================
items = [
bowtie(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", CV, "ph ms"),
   "Review the client's data. Complete the diagram by choosing the condition the client is most likely "
   "experiencing, 2 actions the nurse should take, and 2 parameters the nurse should monitor.",
   [("Angioedema from the ACE inhibitor", True,
     "Lip and tongue swelling without hives in a client on lisinopril is bradykinin-mediated angioedema; it can occur "
     "after years of use."),
    ("Anaphylaxis to a food", False, "Anaphylaxis usually brings hives, itching and hypotension."),
    ("Hereditary angioedema", False, "This begins in childhood with recurrent attacks."),
    ("Peritonsillar abscess", False, "This causes fever, sore throat and one-sided swelling.")],
   [("Stop the lisinopril and record it as an allergy", True,
     "The drug must never be restarted, and ARBs are used with caution."),
    ("Prepare for an emergency airway at the bedside", True,
     "Tongue swelling with a hoarse voice can close the airway; epinephrine and antihistamines work poorly here."),
    ("Give IV diphenhydramine as the main treatment", False, "Histamine is not the cause, so this does not reverse it."),
    ("Give the morning lisinopril dose with sips of water", False, "The drug causes the swelling."),
    ("Place the client flat in bed to rest and settle", False, "Lying flat can worsen airway obstruction.")],
   [("Voice, stridor and swallowing", True, "Show progression of upper airway swelling."),
    ("Respiratory rate and SpO2", True, "Show whether the airway is compromised."),
    ("Serum potassium every morning", False, "Not the immediate guide."),
    ("Fasting blood glucose level", False, "Not relevant to the airway."),
    ("Body weight measured once a day", False, "Not relevant.")],
   "ACE inhibitor angioedema is caused by bradykinin. Stop the drug, protect the airway and watch voice, stridor and "
   "breathing.",
   "Recognize and manage ACE inhibitor angioedema.",
   ex=[note("Emergency department, 0730",
       "A 63-year-old client who has taken lisinopril 20 mg daily for 4 years woke with swollen lips and tongue. "
       "No hives or itching. Voice hoarse; drooling. BP 148/86 mm Hg, HR 96/min, RR 22/min, SpO2 95%.",
       id="ang-note")]),

dtable(M(PA, HF_ASSESS, "evaluate_outcomes", "difficult", CV, "ms ph"),
   "A client with heart failure is being treated with IV furosemide. Review the 3-day trend. For each finding, "
   "choose how the nurse should interpret it.",
   ["Finding", "Day 1 to day 3", "Interpretation"],
   [(["Weight", "88.0 to 84.6 kg"], [
       ("Expected response to diuresis", True, "A loss of 3.4 kg is about 3.4 L of fluid removed."),
       ("Too rapid; needs action", False, "About 1 kg a day is the usual goal."),
       ("No change", False, "The weight has fallen.")]),
    (["Crackles", "Bases to mid-lung, then bases only"], [
       ("Improving pulmonary congestion", True, "Fewer crackles show less fluid in the lungs."),
       ("Worsening", False, "The crackles are smaller in extent."),
       ("Unrelated to treatment", False, "Diuresis reduces pulmonary fluid.")]),
    (["Potassium", "4.1 to 3.2 mEq/L"], [
       ("Needs follow-up before the next dose", True,
        "Loop diuretics waste potassium; 3.2 raises dysrhythmia risk, especially with digoxin."),
       ("Expected and needs no action", False, "It is below range and falling."),
       ("Shows overhydration", False, "Not related to overhydration.")]),
    (["Creatinine", "1.1 to 1.2 mg/dL"], [
       ("Stable kidney function", True, "A rise of 0.1 is within expected variation."),
       ("Acute kidney injury", False, "AKI needs a rise of at least 0.3 mg/dL."),
       ("Severe dehydration", False, "Not supported.")])],
   "The trend shows effective diuresis; the falling potassium needs replacement before more furosemide.",
   "Evaluate a heart failure treatment trend.",
   ex=[flow("Heart failure flowsheet", ["Day 1", "Day 2", "Day 3"], [
       ("Weight", ["88.0 kg", "86.1 kg", "84.6 kg"]),
       ("Crackles", ["Bases to mid-lung", "Lower third", "Bases only"]),
       ("Potassium", ["4.1 mEq/L", "3.6 mEq/L", "3.2 mEq/L"]),
       ("Creatinine", ["1.1 mg/dL", "1.1 mg/dL", "1.2 mg/dL"]),
       ("Medications", ["Furosemide 40 mg IV twice daily; digoxin 0.125 mg daily", "Same", "Same"])],
       id="hf-trend")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", CV, "mgmt ms"),
   "The nurse on a cardiac step-down unit receives report on four clients at 0700. Which client should the nurse "
   "see first?",
   [("A client 2 days after a stent whose heparin infusion was stopped, now with a radial site that is dry, a "
     "radial pulse 2+ and a heart rate of 92/min", False,
     "Stable access site and pulse; routine follow-up."),
    ("A client with heart failure on a furosemide infusion who has had 1,200 mL of urine output overnight and now "
     "reports leg cramps", True,
     "Large diuresis with new cramps suggests hypokalemia or hypomagnesemia, which can cause a fatal dysrhythmia."),
    ("A client with new atrial fibrillation at 96/min on diltiazem who asks why apixaban was added this morning", False,
     "The rate is controlled; the question is teaching that can wait."),
    ("A client with stable angina who rates chest discomfort 2 of 10 during a bed bath and says it went away with "
     "rest after 3 minutes", False,
     "Pain relieved promptly by rest is the stable pattern; it needs follow-up, not first priority.")],
   "Electrolyte loss after heavy diuresis threatens rhythm and life; the other clients are stable.",
   "Prioritize clients on a cardiac step-down unit."),

sata(M(MOC, DELEGATE, "take_action", "difficult", CV, "mgmt"),
   "The RN is working with an unlicensed assistive personnel (UAP) on a cardiac unit. Clients include one who had "
   "a femoral cardiac catheterization 3 hours ago, one with heart failure on daily weights and strict intake and "
   "output, and one starting a heparin infusion. Which tasks can the RN delegate to the UAP?",
   [("Weigh the client with heart failure on the same scale before breakfast", True,
     "Standard measurement with clear direction is within the UAP's role."),
    ("Record the intake and output for the client with heart failure", True, "Measuring and recording output is routine."),
    ("Check the femoral site and pedal pulses after catheterization", False,
     "Assessment of a new post-procedure client stays with the RN."),
    ("Report a blood pressure below 100 systolic to the RN at once", True,
     "Taking vital signs and reporting values the RN specifies is appropriate."),
    ("Adjust the heparin infusion rate after the aPTT result", False, "Medication titration is an RN responsibility."),
    ("Teach the client with heart failure about a low-sodium diet", False, "Teaching needs RN-level judgment.")],
   "UAPs can measure, record and report against clear parameters; assessment, teaching and drug titration remain "
   "with the RN.",
   "Delegate tasks safely on a cardiac unit."),
]

# ================================================================================================
# Case 1: Anaphylactic shock in a client taking a beta blocker
# ================================================================================================
ana_tabs = [
    ("ana-notes", "Nurses' notes", [note("Medical unit, 1410",
        "A 45-year-old client with cellulitis started a first dose of IV ceftriaxone at 1400. The allergy list says "
        "\"penicillin: rash as a child.\" The client takes metoprolol succinate 100 mg daily for hypertension. At "
        "1410 the client says, \"My throat feels tight,\" and has hives spreading across the chest. The voice is "
        "hoarse, there is wheezing in all lung fields, and the skin is flushed and warm.", id="ana-note")]),
    ("ana-vitals", "Vital signs", [flow("Vital signs", ["1355", "1410"], [
        ("Blood pressure", ["132/84 mm Hg", "78/40 mm Hg"]),
        ("Heart rate", ["68/min", "92/min"]),
        ("Respirations", ["16/min", "30/min"]),
        ("SpO2", ["98% on room air", "89% on room air"])], id="ana-flow")]),
]

ana_qs = [
hl_text(M(PA, SHOCK, "recognize_cues", "moderate", CV, "ms ph"),
   "Highlight the findings that require immediate action.",
   ["Ten minutes into a first dose of ceftriaxone, the client says ",
    ("\"My throat feels tight\"", True, "Throat tightness signals upper airway swelling."),
    " and has ",
    ("hives spreading across the chest", True, "Hives with other systems involved means anaphylaxis."),
    ". The voice is ",
    ("hoarse", True, "Hoarseness suggests laryngeal edema."),
    ", with ",
    ("wheezing in all lung fields", True, "Bronchospasm."),
    ". The client has ",
    ("cellulitis of the left leg", False, "The reason for the antibiotic, not the emergency."),
    ". Blood pressure is ",
    ("78/40 mm Hg", True, "Distributive shock from vasodilation."),
    "."],
   "Airway, breathing and circulation are all affected within minutes of a new drug: anaphylaxis.",
   "Recognize cues of anaphylaxis."),

matrix_mc(M(PA, SHOCK, "analyze_cues", "difficult", CV, "ms ph"),
   "For each finding, indicate the mechanism that best explains it.",
   ["Vasodilation and leaky capillaries", "Airway edema or bronchospasm", "Effect of the beta blocker"],
   [("Blood pressure 78/40 mm Hg", 0, "Mediators dilate vessels and fluid leaks out, so pressure falls."),
    ("Hoarse voice", 1, "Swelling of the larynx."),
    ("Wheezing", 1, "Smooth muscle contraction in the bronchi."),
    ("Heart rate only 92/min despite severe hypotension", 2,
     "Metoprolol blunts the expected tachycardia, which also hides the severity of shock."),
    ("Flushed, warm skin", 0, "Dilated skin vessels.")],
   "Anaphylaxis causes distributive shock and airway obstruction; a beta blocker blunts the heart rate response and "
   "can make the client resistant to epinephrine.",
   "Analyze the mechanisms of anaphylaxis in a client on a beta blocker."),

cloze(M(PA, SHOCK, "prioritize_hypotheses", "difficult", CV, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client is having {{b1}}. The greatest immediate threat is {{b2}}, and the response to standard treatment "
   "may be poor because of {{b3}}.",
   [[("anaphylaxis with shock", True, "Skin, airway and circulation are involved after a drug exposure."),
     ("a mild allergic reaction", False, "Hypotension and airway signs make it severe."),
     ("septic shock from cellulitis", False, "The timing fits the drug, not infection."),
     ("a panic attack", False, "Objective signs rule this out.")],
    [("airway obstruction from laryngeal edema", True, "A hoarse voice and throat tightness can progress to total obstruction."),
     ("acute kidney injury", False, "A later risk."),
     ("cellulitis spreading", False, "Not immediate."),
     ("hypoglycemia", False, "Not relevant.")],
    [("beta blockade from metoprolol", True, "Beta blockers reduce the response to epinephrine."),
     ("the client's age", False, "Age 45 does not reduce the response."),
     ("the penicillin allergy history", False, "This explains the reaction, not the poor response."),
     ("the cellulitis", False, "Not relevant to treatment response.")]],
   "Anaphylaxis with airway threat and shock; beta blockade may make it refractory to epinephrine.",
   "Prioritize threats in anaphylaxis."),

select_n(M(PPT, ADVERSE, "generate_solutions", "difficult", CV, "ph ms"),
   "Select the 3 actions the nurse should anticipate first.",
   [("Stop the ceftriaxone and keep the IV line open with new tubing", True, "Remove the trigger and keep access."),
    ("Epinephrine 0.5 mg IM (1 mg/mL) into the anterolateral thigh", True,
     "IM epinephrine is the first drug; the thigh gives the fastest absorption."),
    ("0.9% sodium chloride 1 to 2 L rapid IV bolus", True, "Large volumes are needed to refill the dilated vessels."),
    ("Diphenhydramine 50 mg IV as the first drug", False, "Antihistamines help hives but do not reverse shock or airway swelling."),
    ("Epinephrine 0.5 mg of the 1 mg/mL strength by IV push", False,
     "Undiluted IV epinephrine can cause fatal dysrhythmias; IV epinephrine is given only as a dilute infusion."),
    ("Methylprednisolone 125 mg IV before epinephrine", False, "Steroids act over hours and never replace epinephrine."),
    ("Sit the client fully upright with the legs down", False, "Upright positioning in shock can cause collapse.")],
   "Stop the drug, give IM epinephrine at once and restore volume; antihistamines and steroids are add-ons.",
   "Generate solutions for anaphylaxis.",
   ex=[note("Rapid response team protocol for anaphylaxis",
       "Stop the suspected drug. Epinephrine 0.5 mg IM, repeat every 5 to 15 minutes. Fluid bolus. Diphenhydramine, "
       "methylprednisolone and albuterol as add-on therapy.", id="ana-protocol")]),

sata(M(PPT, CV_MEDS, "take_action", "difficult", CV, "ph ms"),
   "Review the 1430 findings. Which actions should the nurse take?",
   [("Give glucagon IV as prescribed", True, "Glucagon raises heart rate and contractility despite beta blockade."),
    ("Start the epinephrine infusion by pump as prescribed", True, "A titrated infusion is used when IM doses fail."),
    ("Prepare for intubation with the anesthesia team", True, "Stridor shows the airway is closing."),
    ("Keep the client flat with the legs raised", True, "Supports venous return in shock."),
    ("Give the scheduled metoprolol dose", False, "Further beta blockade worsens the problem."),
    ("Give oral cetirizine for the hives", False, "Oral drugs are unsafe with a threatened airway and do not treat shock."),
    ("Wait 30 minutes before a third epinephrine dose", False, "Doses are repeated every 5 to 15 minutes.")],
   "Refractory anaphylaxis on a beta blocker: glucagon, an epinephrine infusion, airway preparation and positioning.",
   "Take action for refractory anaphylaxis.",
   ex=[flow("1430", ["1410", "1430"], [
       ("Blood pressure", ["78/40 mm Hg", "74/38 mm Hg"]),
       ("Heart rate", ["92/min", "96/min"]),
       ("Airway", ["Hoarse voice", "Inspiratory stridor"]),
       ("Treatment given", ["-", "Epinephrine 0.5 mg IM x2; 2 L 0.9% sodium chloride"])], id="ana-1430")]),

matrix_mc(M("SECE.SIC", "latex-and-drug-allergies", "evaluate_outcomes", "difficult", CV, "ph fund"),
   "The client has recovered and is being observed overnight. For each safety measure, indicate whether it is "
   "complete or needs correction.",
   ["Complete", "Needs correction"],
   [("The allergy list reads \"ceftriaxone: anaphylaxis (airway swelling, hypotension)\"", 0,
     "The drug and the type of reaction are recorded so future prescribers can judge risk."),
    ("The red allergy band lists penicillin only", 1, "The band must match the updated allergy list."),
    ("The pharmacy profile is updated and the order set flags cephalosporins", 0, "A system alert prevents re-exposure."),
    ("The client is to be discharged at 2100, 3 hours after the epinephrine infusion was stopped", 1,
     "Severe anaphylaxis needing an epinephrine infusion calls for longer observation for a biphasic reaction."),
    ("The client has a prescription for two epinephrine auto-injectors and practiced with a trainer", 0,
     "The client can treat a future reaction at once."),
    ("The client was told to take an antihistamine first and use the auto-injector if it does not work", 1,
     "Epinephrine is given first for any signs of anaphylaxis; delay increases deaths.")],
   "Safety after anaphylaxis means a complete allergy record, matching band and pharmacy alerts, enough observation "
   "time, and correct auto-injector teaching.",
   "Evaluate allergy safety measures after anaphylaxis.",
   ex=[flow("Safety checklist, 2000", ["2000"], [
       ("Blood pressure", ["118/70 mm Hg; epinephrine infusion stopped at 1800"]),
       ("Allergy list", ["Updated"]),
       ("Allergy band", ["Penicillin"]),
       ("Planned discharge", ["2100"]),
       ("Teaching", ["Epinephrine auto-injector practice done"])], id="ana-2000")]),
]

# ================================================================================================
# Case 2: Arterial line and CVP after open abdominal aortic aneurysm repair
# ================================================================================================
aaa_tabs = [
    ("aaa-notes", "Nurses' notes", [note("Surgical ICU, 1800",
        "A 71-year-old client returned at 1500 after open repair of an infrarenal abdominal aortic aneurysm with a "
        "cross-clamp time of 55 minutes. The client has a right radial arterial line and a central venous catheter. "
        "At 1800 the arterial line waveform is flattened with no dicrotic notch. A manual cuff pressure is "
        "128/72 mm Hg. The client has passed a loose, bloody stool. The left foot is mottled with absent pedal "
        "pulses; the right foot is warm with Doppler pulses.", id="aaa-note")]),
    ("aaa-flow", "Flowsheet", [flow("Hemodynamics", ["1500", "1600", "1700", "1800"], [
        ("Arterial line", ["132/74", "126/70", "118/68", "78/62"]),
        ("Cuff pressure", ["-", "-", "-", "128/72"]),
        ("Heart rate", ["88", "94", "102", "108"]),
        ("CVP (2 to 6 mm Hg)", ["6", "4", "3", "2"]),
        ("Urine output, mL/hour", ["60", "40", "25", "18"]),
        ("Hemoglobin, g/dL", ["11.2", "-", "-", "11.0"])], id="aaa-flowsheet")]),
]

aaa_qs = [
hl_table(M(RRP, CV_DX, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that require follow-up.",
   ["Finding", "Data"],
   [(["Arterial line", "78/62 mm Hg, flattened, no dicrotic notch"], True,
     "A damped waveform reads falsely low; it must be fixed before it is used for decisions."),
    (["CVP and urine output", "CVP falling 6 to 2; output falling to 18 mL/hour"], True,
     "Low filling pressure with low output suggests hypovolemia after surgery."),
    (["Stool", "Loose and bloody"], True,
     "After AAA repair this suggests ischemic colitis from loss of inferior mesenteric artery flow."),
    (["Left foot", "Mottled, pedal pulses absent"], True, "Distal embolization or blocked flow to the leg."),
    (["Hemoglobin", "11.2 then 11.0 g/dL"], False, "Stable, so active bleeding is unlikely."),
    (["Cuff pressure", "128/72 mm Hg"], False, "Normal; this is the reliable reading.")],
   "A damped line, falling filling pressures and output, bloody stool and a mottled foot all need follow-up.",
   "Recognize cues after open AAA repair."),

dtable(M(RRP, CV_DX, "analyze_cues", "difficult", CV, "ms"),
   "For each finding, choose the most likely explanation.",
   ["Finding", "Explanation"],
   [(["Flattened arterial waveform reading far below the cuff pressure"], [
       ("A damped system from air, a clot, a kink or a soft pressure bag", True,
        "Damping softens the waveform and lowers the systolic reading."),
       ("True hypotension", False, "The cuff pressure is 128/72."),
       ("Aortic graft failure", False, "This would affect both readings."),
       ("Transducer placed too low", False, "A low transducer reads falsely high.")]),
    (["CVP 2 mm Hg with urine output 18 mL/hour"], [
       ("Hypovolemia from third spacing and blood loss", True,
        "Large abdominal surgery moves fluid out of the vessels."),
       ("Heart failure", False, "Heart failure raises the CVP."),
       ("Urinary retention", False, "The client has a catheter and low filling pressure."),
       ("Expected and needs no action", False, "Output below 0.5 mL/kg/hour needs action.")]),
    (["Bloody diarrhea on the day of surgery"], [
       ("Ischemic colitis", True, "The sigmoid colon loses blood supply when the inferior mesenteric artery is sacrificed."),
       ("A bleeding hemorrhoid", False, "Not the concern in this setting."),
       ("Antibiotic-related diarrhea", False, "Too early."),
       ("Expected after bowel preparation", False, "Blood is not expected.")])],
   "Know the line is damped, the client is dry, and the colon may be ischemic: three separate problems.",
   "Analyze hemodynamic and surgical findings after AAA repair."),

matrix_mc(M("SECE.MOC", "prioritizing-client-care", "prioritize_hypotheses", "difficult", CV, "mgmt ms"),
   "At 1800 the nurse has the following issues. For each, indicate how the nurse should prioritize it.",
   ["Report to the surgeon now", "Handle at the bedside now", "Can wait"],
   [("Possible ischemic colitis", 0, "Bowel ischemia can progress to necrosis and needs urgent evaluation."),
    ("Left foot mottled with absent pulses", 0, "A threatened limb needs the surgeon now."),
    ("Damped arterial line waveform", 1,
     "The nurse troubleshoots the line now; the cuff confirms the pressure is safe meanwhile."),
    ("Low CVP with falling urine output", 0, "Needs a fluid plan and may signal kidney injury from the clamp."),
    ("The client's family asking for an update in the waiting room", 2,
     "Important, but after the urgent clinical issues; the charge nurse can tell them when the nurse will come.")],
   "The nurse reports threats to organs and limbs at once, fixes equipment problems at the bedside, and defers "
   "non-clinical requests safely.",
   "Prioritize competing problems after AAA repair."),

matrix_mc(M(RRP, POSTOP, "generate_solutions", "difficult", CV, "ms ph"),
   "For each possible intervention, indicate whether it is indicated or contraindicated now.",
   ["Indicated", "Contraindicated"],
   [("Fluid bolus as prescribed for the low CVP and urine output", 0, "Restores filling pressure and kidney perfusion."),
    ("Start norepinephrine for the arterial line pressure of 78/62", 1,
     "The low reading is a damped line; the cuff shows normal pressure."),
    ("Keep the client NPO and prepare for sigmoidoscopy", 0, "Colonoscopy or sigmoidoscopy confirms ischemic colitis."),
    ("Elevate the left leg on pillows above heart level", 1, "Elevation reduces arterial flow to an ischemic limb."),
    ("Level the transducer at the phlebostatic axis", 0, "Correct leveling is needed for accurate readings."),
    ("Apply a heating pad to warm the mottled foot", 1, "Ischemic skin burns easily.")],
   "Give fluid, investigate the bowel and fix the line; do not treat a damped reading or harm the ischemic leg.",
   "Generate solutions after AAA repair.",
   ex=[note("Surgeon on the phone, 1810",
       "Give 500 mL lactated Ringer's now. NPO. Sigmoidoscopy in the morning, sooner if pain or fever. Vascular team "
       "to see the left foot. Fix the arterial line.", id="aaa-orders")]),

order(M(RRP, CV_DX, "take_action", "difficult", CV, "ms"),
   "Place the nurse's steps for troubleshooting the damped arterial line in order.",
   [("Confirm the client's pressure with a manual cuff", "Check the client before the equipment."),
    ("Check the tubing for kinks, loose connections and air bubbles", "The commonest causes are found along the line."),
    ("Check that the pressure bag is inflated to 300 mm Hg", "A soft bag allows backflow and damping."),
    ("Level the transducer at the phlebostatic axis and zero it", "Gives an accurate baseline."),
    ("Do a fast-flush (square-wave) test", "Confirms the system now responds correctly.")],
   "Check the client first, then the line from catheter to bag, then level and zero, then test the system.",
   "Take action to troubleshoot an arterial line.",
   ex=[flow("1815", ["1815"], [
       ("Arterial waveform", ["Flattened, no dicrotic notch"]),
       ("Pressure bag", ["Reads 140 mm Hg"]),
       ("Tubing", ["Small air bubble near the transducer"])], id="aaa-1815")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", CV, "ms"),
   "Review the 2100 findings. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Arterial line 124/70 with a clear dicrotic notch", 0, "The line is accurate again."),
    ("CVP 6 mm Hg; urine output 45 mL/hour", 0, "Volume and kidney perfusion restored."),
    ("Abdomen more distended, with pain out of proportion to the incision", 1,
     "This suggests worsening bowel ischemia."),
    ("Temperature 38.6 C (101.5 F)", 1, "Fever with bloody stool raises concern for bowel necrosis."),
    ("Left foot warmer, Doppler pedal pulse present", 0, "Flow to the foot is returning.")],
   "The line, volume and leg have improved, but the abdomen and fever suggest progressing bowel ischemia.",
   "Evaluate outcomes after AAA repair.",
   ex=[flow("2100", ["2100"], [
       ("Arterial line", ["124/70 mm Hg, dicrotic notch present"]),
       ("CVP / urine output", ["6 mm Hg / 45 mL/hour"]),
       ("Abdomen", ["More distended; pain out of proportion to the incision"]),
       ("Temperature", ["38.6 C (101.5 F)"]),
       ("Left foot", ["Warmer; Doppler pulse present"])], id="aaa-2100")]),
]

# ================================================================================================
# Case 3: Infective endocarditis in a client who injects drugs
# ================================================================================================
ie_tabs = [
    ("ie-notes", "Nurses' notes", [note("Medical unit, admission",
        "A 32-year-old client who injects heroin has had fevers, night sweats and fatigue for 2 weeks, and now has a "
        "cough with sharp chest pain on breathing. Findings: a new holosystolic murmur at the left lower sternal "
        "border that increases with inspiration; painless flat red spots on the soles; dark lines under several "
        "fingernails; tender nodules on two fingertips; needle tracks on both arms. A CT of the chest shows "
        "multiple small nodules in both lungs.", id="ie-note")]),
    ("ie-vitals", "Vital signs and labs", [table("Vital signs and laboratory results", ["Admission", "Reference range"], [
        ("Temperature", ["39.2 C (102.6 F)", "-"]),
        ("Heart rate", ["112/min", "60 to 100/min"]),
        ("Blood pressure", ["108/64 mm Hg", "-"]),
        ("WBC", ["18,400/mm3", "5,000 to 10,000/mm3"]),
        ("C-reactive protein", ["142 mg/L", "< 10 mg/L"]),
        ("Creatinine", ["0.9 mg/dL", "0.6 to 1.2 mg/dL"])], id="ie-table")]),
]

ie_qs = [
hl_table(M(PA, VALVE, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that support infective endocarditis.",
   ["Finding", "Data"],
   [(["Murmur", "New holosystolic, louder with inspiration"], True,
     "A new murmur that increases with inspiration suggests tricuspid regurgitation, typical in people who inject drugs."),
    (["Soles", "Painless flat red spots"], True, "Janeway lesions."),
    (["Fingernails", "Dark lines under the nails"], True, "Splinter hemorrhages."),
    (["Fingertips", "Tender nodules"], True, "Osler nodes."),
    (["Creatinine", "0.9 mg/dL"], False, "Normal."),
    (["Blood pressure", "108/64 mm Hg"], False, "Acceptable.")],
   "A new murmur with embolic and immune skin signs and fever is endocarditis until proven otherwise.",
   "Recognize cues of infective endocarditis."),

matrix_mr(M(PA, VALVE, "analyze_cues", "difficult", CV, "ms"),
   "For each finding, indicate what it reflects. Each row may have more than one answer.",
   ["Infection on the valve", "Emboli from the valve", "Immune response to the infection"],
   [("Fever and WBC 18,400/mm3", {0, 2}, "Fever and a high white count reflect the infection and the body's response."),
    ("Nodules in both lungs on CT", {1}, "Right-sided valve vegetations throw septic emboli to the lungs."),
    ("Janeway lesions", {1}, "Small septic emboli in the skin."),
    ("Osler nodes", {2}, "Immune complex deposits in the fingertips."),
    ("New tricuspid regurgitation murmur", {0}, "Vegetations damage the valve."),
    ("Pleuritic chest pain", {1}, "Septic pulmonary emboli cause pleuritic pain.")],
   "Endocarditis causes signs from the infected valve, emboli and the immune response.",
   "Analyze findings in infective endocarditis."),

cloze(M("SECE.MOC", "client-rights-and-advocacy", "prioritize_hypotheses", "difficult", CV, "mgmt psych"),
   "On day 2 the client is sweating, yawning and restless and says, \"I'm leaving. I can't do six weeks in here.\" "
   "Complete the sentences by choosing from the lists of options.",
   "The most likely reason for wanting to leave is {{b1}}. The nurse's first priority is to {{b2}}. If the client "
   "still chooses to leave, the client {{b3}}.",
   [[("untreated opioid withdrawal", True,
      "Sweating, yawning and restlessness are withdrawal signs, a common reason people who inject drugs leave."),
     ("a reaction to vancomycin", False, "These are not drug-reaction signs."),
     ("worsening endocarditis", False, "Possible, but the picture is withdrawal."),
     ("a wish to avoid the cost of care", False, "Not suggested by the data.")],
    [("tell the provider so withdrawal can be treated, for example with buprenorphine", True,
      "Treating withdrawal removes the main reason for leaving and is the advocacy action."),
     ("call security to keep the client on the unit", False, "A capable adult cannot be held against their will."),
     ("explain that leaving will mean losing insurance coverage", False,
      "Untrue and coercive; leaving against advice does not void insurance."),
     ("remove the IV so the client can leave quickly", False, "Premature; the priority is to address the cause.")],
    [("has the right to leave if able to make decisions, after being told the risks", True,
      "Capable adults may refuse care; the team explains risks, offers alternatives and documents."),
     ("must stay because the infection is life-threatening", False, "A capable client may refuse even life-saving care."),
     ("cannot be given any follow-up or prescriptions", False, "Harm reduction includes follow-up and alternatives."),
     ("must sign the form or the nurse cannot let them go", False, "A client may refuse to sign; the nurse documents it.")]],
   "Look for the treatable reason behind a wish to leave, such as withdrawal, and respect the right of a capable "
   "client to decide.",
   "Prioritize advocacy when a client wants to leave against medical advice."),

sata(M(PPT, ABX, "generate_solutions", "difficult", CV, "ph ms"),
   "Which prescriptions should the nurse anticipate?",
   [("Three sets of blood cultures from separate sites before antibiotics", True,
     "Separate sites and times confirm continuous bacteremia and identify the organism."),
    ("Vancomycin IV dosed by drug levels", True, "Covers MRSA until results return."),
    ("Transesophageal echocardiogram", True, "Best view of vegetations and valve damage."),
    ("An offer of buprenorphine for opioid use disorder", True,
     "Treating opioid use disorder reduces leaving against advice and reinfection."),
    ("A heparin infusion for the lung emboli", False,
     "Septic emboli are not treated with anticoagulation, which raises the risk of bleeding into embolic sites."),
    ("Antibiotics held for 48 hours to see the fever pattern", False, "Delay allows valve destruction."),
    ("A 5-day course of oral antibiotics", False, "Endocarditis needs weeks of IV therapy.")],
   "Cultures before antibiotics, empiric IV vancomycin, an echocardiogram and treatment of the opioid use disorder.",
   "Generate solutions for infective endocarditis.",
   ex=[note("Admission plan",
       "Blood cultures x3. Vancomycin per pharmacy. TEE. Addiction medicine consult.", id="ie-plan")]),

sata(M(PPT, ABX, "take_action", "difficult", CV, "ph"),
   "Review the day 3 data. Which actions should the nurse take?",
   [("Hold the 1200 vancomycin dose and notify the provider and pharmacy", True,
     "A high trough with rising creatinine shows accumulation and kidney injury."),
    ("Infuse future doses over at least 60 minutes per gram", True,
     "Flushing with a fast infusion is vancomycin infusion reaction; slowing the rate prevents it."),
    ("Monitor intake and output and daily creatinine", True, "Tracks kidney function."),
    ("Give the 1200 dose and recheck the level tomorrow", False, "This risks further kidney injury."),
    ("Speed up the infusion to reduce time attached to the IV", False, "Faster infusion causes the reaction."),
    ("Stop all antibiotics until the creatinine is normal", False, "Treatment must continue with an adjusted plan.")],
   "Vancomycin can harm the kidneys and causes a rate-related infusion reaction; adjust the dose with pharmacy and "
   "slow the infusion.",
   "Take action for vancomycin adverse effects.",
   ex=[flow("Day 3", ["Day 1", "Day 3"], [
       ("Vancomycin trough", ["-", "24 mcg/mL (target 15 to 20)"]),
       ("Creatinine", ["0.9 mg/dL", "1.9 mg/dL"]),
       ("During the last infusion (run over 30 minutes)", ["-", "Red flushing of the neck and upper chest, itching"]),
       ("Blood cultures", ["Pending", "MRSA in 3 of 3 sets"])], id="ie-day3")]),

matrix_mc(M(PA, VALVE, "evaluate_outcomes", "difficult", CV, "ms"),
   "Review the day 10 findings. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Afebrile for 72 hours", 0, "The infection is responding."),
    ("Repeat blood cultures negative for 48 hours", 0, "Bacteremia has cleared."),
    ("C-reactive protein 38 mg/L", 0, "Inflammation is falling."),
    ("Sudden severe headache and new left arm weakness", 1,
     "Suggests an embolic stroke or a ruptured infected (mycotic) aneurysm; call a stroke alert."),
    ("Creatinine 1.1 mg/dL", 0, "Kidney function has recovered after the dose change.")],
   "Fever, cultures and inflammation show the infection is responding, but new neurologic signs need emergency action.",
   "Evaluate outcomes in endocarditis.",
   ex=[flow("Day 10", ["Day 10"], [
       ("Temperature", ["Afebrile for 72 hours"]),
       ("Blood cultures", ["Negative for 48 hours"]),
       ("C-reactive protein", ["38 mg/L"]),
       ("Neurologic", ["Sudden severe headache; new left arm weakness"]),
       ("Creatinine", ["1.1 mg/dL"])], id="ie-day10")]),
]

# ================================================================================================
# Case 4: Acute limb ischemia, then compartment syndrome after embolectomy
# ================================================================================================
ali_tabs = [
    ("ali-notes", "Nurses' notes", [note("Emergency department, 1100",
        "A 70-year-old client with peripheral arterial disease and atrial fibrillation stopped apixaban 2 months ago "
        "because of cost. Two hours ago the client had sudden, severe pain in the right leg at rest. The client "
        "usually gets calf cramps after walking one block. The right foot and calf are pale and cold, with numbness "
        "of the toes and weakness when moving them. No Doppler signal at the right popliteal or pedal arteries. "
        "Both lower legs are shiny and hairless with thick toenails.", id="ali-note")]),
    ("ali-vitals", "Vital signs", [flow("Vital signs", ["1100"], [
        ("Blood pressure", ["146/88 mm Hg"]),
        ("Heart rate", ["104/min, irregular"]),
        ("Pain", ["9 of 10, right leg"])], id="ali-flow")]),
]

ali_qs = [
hl_text(M(PA, VASC, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that show an acute threat to the limb.",
   ["The client has ",
    ("sudden severe pain in the right leg at rest", True, "Sudden rest pain means a new blockage."),
    ". The client usually has ",
    ("calf cramps after walking one block", False, "Chronic claudication; the baseline."),
    ". The right foot is ",
    ("pale and cold", True, "Pallor and coldness show no arterial inflow."),
    " with ",
    ("numbness and weakness of the toes", True, "Sensory and motor loss mean the limb is threatened."),
    ". There is ",
    ("no Doppler signal below the knee", True, "Absent flow."),
    ". Both legs are ",
    ("shiny and hairless", False, "Long-standing PAD changes."),
    "."],
   "The 6 Ps of acute arterial occlusion: pain, pallor, pulselessness, paresthesia, poikilothermia and paralysis.",
   "Recognize acute limb ischemia."),

matrix_mc(M(PA, VASC, "analyze_cues", "difficult", CV, "ms"),
   "For each finding, indicate whether it reflects chronic peripheral arterial disease or the new acute occlusion.",
   ["Chronic PAD", "Acute occlusion"],
   [("Shiny, hairless skin with thick toenails", 0, "Long-term poor perfusion."),
    ("Calf cramping after walking one block", 0, "Intermittent claudication."),
    ("Sudden pain at rest 2 hours ago", 1, "A new embolus or thrombus."),
    ("Toe weakness and numbness", 1, "Nerves and muscle are losing blood supply."),
    ("No Doppler signal at the popliteal artery", 1, "Complete loss of flow."),
    ("Atrial fibrillation without anticoagulation", 1, "The likely source of an embolus.")],
   "Chronic changes tell the history; sudden rest pain, neurologic loss and no Doppler signal show an acute embolic "
   "occlusion from untreated AF.",
   "Analyze chronic versus acute arterial findings."),

cloze(M(PA, VASC, "prioritize_hypotheses", "difficult", CV, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client most likely has {{b1}}, and because of {{b2}} the limb needs {{b3}}.",
   [[("an arterial embolus from the left atrium", True, "AF without anticoagulation throws emboli."),
     ("a deep vein thrombosis", False, "DVT causes a warm, swollen leg with pulses."),
     ("worsening claudication", False, "Claudication is relieved by rest."),
     ("a ruptured Baker's cyst", False, "Not consistent.")],
    [("the sensory and motor loss", True, "Neurologic deficit means the limb is immediately threatened."),
     ("the shiny skin", False, "Chronic."),
     ("the heart rate", False, "Not the limb's prognosis."),
     ("the history of claudication", False, "Chronic.")],
    [("revascularization within hours", True, "Muscle and nerve die within about 6 hours of complete ischemia."),
     ("a walking program", False, "For chronic claudication."),
     ("compression stockings", False, "Harmful in arterial disease."),
     ("outpatient follow-up", False, "This is an emergency.")]],
   "An embolic occlusion with neurologic loss needs urgent revascularization.",
   "Prioritize acute limb ischemia."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph ms"),
   "For each intervention, indicate whether it is indicated, not indicated, or contraindicated now.",
   ["Indicated", "Not indicated", "Contraindicated"],
   [("Heparin IV bolus and infusion", 0, "Stops clot growth while surgery is arranged."),
    ("Emergency vascular surgery consult for embolectomy", 0, "Definitive treatment."),
    ("Raise the right leg above the level of the heart", 2, "Elevation further reduces arterial flow."),
    ("Apply a heating pad to the cold foot", 2, "Ischemic tissue burns easily."),
    ("Cilostazol for claudication", 1, "For chronic symptoms, not acute occlusion."),
    ("Keep the leg level or slightly dependent", 0, "Gravity helps arterial flow.")],
   "Anticoagulate, get the surgeon and position the leg to help arterial flow; never elevate or heat an ischemic limb.",
   "Generate solutions for acute limb ischemia.",
   ex=[note("Plan, 1115", "Heparin per protocol. Vascular surgery consult. Embolectomy planned.", id="ali-plan")]),

sata(M(PA, VASC, "take_action", "difficult", CV, "ms"),
   "Review the findings 6 hours after embolectomy. Which actions should the nurse take?",
   [("Notify the surgeon now", True, "These are signs of compartment syndrome, which needs urgent fasciotomy."),
    ("Keep the leg at heart level", True, "Elevation lowers arterial pressure further; dependency adds swelling."),
    ("Loosen the dressing around the calf", True, "Removes outside pressure."),
    ("Prepare for compartment pressure measurement", True, "Confirms the diagnosis."),
    ("Raise the leg on two pillows to reduce the swelling", False, "Reduces flow into a compressed compartment."),
    ("Give more morphine and reassess in 1 hour", False, "Pain beyond expectation is the warning sign; masking it delays surgery."),
    ("Apply ice packs to the calf", False, "Cold reduces blood flow.")],
   "Pain out of proportion and on passive stretch with a tense calf after reperfusion is compartment syndrome.",
   "Take action for compartment syndrome after reperfusion.",
   ex=[flow("6 hours after embolectomy", ["1800"], [
       ("Pain", ["10 of 10, unchanged by morphine"]),
       ("Calf", ["Tense, swollen, shiny"]),
       ("Passive toe extension", ["Causes severe pain"]),
       ("Pedal Doppler", ["Present"])], id="ali-1800")]),

matrix_mc(M(PA, VASC, "evaluate_outcomes", "difficult", CV, "ms lab"),
   "After fasciotomy, review the next morning's findings. For each finding, indicate whether it shows improvement or "
   "needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Foot warm with a Doppler pulse", 0, "Perfusion maintained."),
    ("Pain 4 of 10", 0, "Pressure relieved."),
    ("Dark, cola-colored urine", 1, "Myoglobin from damaged muscle; risk of kidney injury."),
    ("CK 18,000 units/L", 1, "Rhabdomyolysis; needs aggressive fluids."),
    ("Potassium 6.0 mEq/L", 1, "Released from dead muscle cells; risk of dysrhythmia."),
    ("Toes move on command", 0, "Motor function returning.")],
   "Perfusion and pain have improved, but reperfusion has released myoglobin and potassium; rhabdomyolysis needs "
   "treatment.",
   "Evaluate outcomes after reperfusion and fasciotomy.",
   ex=[flow("Next morning", ["0600"], [
       ("Right foot", ["Warm; Doppler pulse present; toes move"]),
       ("Pain", ["4 of 10"]),
       ("Urine", ["Dark, cola-colored"]),
       ("CK", ["18,000 units/L"]),
       ("Potassium", ["6.0 mEq/L"])], id="ali-0600")]),
]

cases = [
    case("Anaphylactic shock in a client taking a beta blocker", ana_tabs, ana_qs),
    case("Arterial line and CVP monitoring after open AAA repair", aaa_tabs, aaa_qs),
    case("Infective endocarditis in a client who injects drugs", ie_tabs, ie_qs),
    case("Acute limb ischemia and compartment syndrome", ali_tabs, ali_qs),
]

BUILT = build(103, "cardiac-cases-shock-vascular-standalone",
              "Cardiac case studies: anaphylactic shock, arterial line monitoring after AAA repair, endocarditis and "
              "acute limb ischemia, with four standalone bowtie, trend, priority and delegation items",
              items, cases, case_conversion=True)
