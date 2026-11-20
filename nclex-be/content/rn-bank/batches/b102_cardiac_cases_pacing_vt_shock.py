"""Batch 102 (case conversion, chunk 1: cardiac): four full NGN case studies on pacing, dysrhythmias and
hemodynamic monitoring (CASE-MAP.md: pacemaker, hemodynamic monitoring, shock).

Cases: drug-induced complete heart block treated with transcutaneous pacing; permanent pacemaker failure to
capture; ventricular tachycardia in a client with an ICD; cardiogenic shock guided by a pulmonary artery
catheter. Each case walks the six CJMM steps in order and unfolds with new data in steps 4 to 6."""
from qb import *

RRP, PA, PPT, HPM, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "SECE.SIC"
CV = "cardiovascular"
DYS = "cardiac-dysrhythmias"
CV_DX = "cardiovascular-diagnostics-and-monitoring"
CV_MEDS = "cardiovascular-medications"
CALC = "medication-dose-calculations"
POSTPROC = "post-procedure-complications"
BLS = "basic-life-support"
SHOCK = "shock-and-sepsis"
HOME = "equipment-and-home-safety"
TEACH = "health-teaching-and-behavior-change"
LYTES = "electrolyte-imbalances"

items = []

# ================================================================================================
# Case 1: Drug-induced complete heart block and transcutaneous pacing
# ================================================================================================
chb_tabs = [
    ("chb-notes", "Nurses' notes", [note("Emergency department, 0930",
        "A 79-year-old client fainted at home this morning. The client has hypertension and atrial flutter that "
        "converted last year. Medications: metoprolol tartrate 50 mg twice daily; diltiazem ER 240 mg daily, added "
        "by a different provider 10 days ago; lisinopril 10 mg daily. The client is drowsy and answers slowly, and "
        "the skin is cool and pale. Monitor: P waves regular at 82/min; QRS complexes wide (0.14 s) and regular at "
        "34/min; PR intervals vary with no pattern.", id="chb-note")]),
    ("chb-vitals", "Vital signs", [flow("Vital signs", ["0930"], [
        ("Blood pressure", ["78/44 mm Hg"]),
        ("Heart rate", ["34/min"]),
        ("Respirations", ["18/min"]),
        ("SpO2", ["93% on room air"]),
        ("Capillary glucose", ["104 mg/dL"])], id="chb-flow")]),
    ("chb-labs", "Laboratory results", [table("Laboratory results", ["0930", "Reference range"], [
        ("Potassium", ["5.2 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("Creatinine", ["1.3 mg/dL", "0.6 to 1.2 mg/dL"]),
        ("Troponin I", ["< 0.04 ng/mL", "< 0.04 ng/mL"])], id="chb-lab")]),
]

chb_qs = [
hl_table(M(PA, DYS, "recognize_cues", "moderate", CV, "aha ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Assessment", "Finding"],
   [(["Rhythm", "P waves 82/min, wide QRS 34/min, PR varies"], True,
     "Atria and ventricles beat independently: third-degree (complete) heart block."),
    (["Blood pressure", "78/44 mm Hg"], True, "Hypotension shows the slow rate is not perfusing the client."),
    (["Mental status", "Drowsy, answers slowly"], True, "Altered mental status is a sign of poor cerebral perfusion."),
    (["Medications", "Metoprolol plus diltiazem started 10 days ago"], True,
     "Two AV nodal blockers together can cause high-grade block."),
    (["Capillary glucose", "104 mg/dL"], False, "Normal; hypoglycemia does not explain the drowsiness."),
    (["Troponin I", "< 0.04 ng/mL"], False, "Normal; there is no sign of infarction.")],
   "Complete heart block with hypotension and altered mental status is unstable bradycardia; the drug combination is "
   "the likely cause.",
   "Recognize cues of unstable bradycardia."),

dtable(M(RRP, CV_DX, "analyze_cues", "difficult", CV, "aha ms"),
   "For each finding, choose what it means.",
   ["Finding", "Meaning"],
   [(["Regular P waves and regular QRS complexes with PR intervals that vary"], [
       ("No atrial impulses are conducted to the ventricles", True, "This is the defining sign of third-degree block."),
       ("Every other atrial impulse is conducted", False, "That is 2:1 second-degree block, with a fixed PR."),
       ("The PR lengthens until a beat is dropped", False, "That is Mobitz I (Wenckebach)."),
       ("Atrial fibrillation with a slow ventricular response", False, "Atrial fibrillation has no regular P waves.")]),
    (["QRS 0.14 seconds at 34/min"], [
       ("The escape rhythm starts below the AV node", True,
        "A wide, slow escape rhythm comes from the ventricles and is unreliable."),
       ("The escape rhythm starts in the AV junction", False, "A junctional escape is narrow and runs at 40 to 60/min."),
       ("A bundle branch block with sinus rhythm", False, "The P waves are not conducted."),
       ("Ventricular tachycardia", False, "The rate is 34/min.")]),
    (["Metoprolol with diltiazem"], [
       ("Additive slowing of conduction through the AV node", True,
        "Beta blockers and non-dihydropyridine calcium channel blockers both slow AV conduction."),
       ("Raised potassium causing the block", False, "A potassium of 5.2 adds little; the drugs are the main cause."),
       ("An allergic reaction to diltiazem", False, "This is a predictable drug effect."),
       ("No relation to the rhythm", False, "The timing points to the new drug.")])],
   "Complete heart block with a wide ventricular escape is unstable and unlikely to respond to atropine; the drug "
   "combination is the cause.",
   "Analyze a rhythm strip and its cause."),

cloze(M(PA, DYS, "prioritize_hypotheses", "difficult", CV, "aha"),
   "Complete the sentences by choosing from the lists of options.",
   "The priority is to {{b1}}. Atropine is {{b2}} because {{b3}}.",
   [[("raise the heart rate to restore perfusion", True, "Hypotension and altered mental status need a faster rate now."),
     ("lower the potassium", False, "Potassium is only mildly raised."),
     ("start anticoagulation", False, "Not the problem."),
     ("obtain a stat echocardiogram", False, "This would delay treatment.")],
    [("unlikely to work", True, "Atropine acts on the SA and AV nodes."),
     ("contraindicated", False, "It may be tried, but it should not delay pacing."),
     ("the definitive treatment", False, "Pacing is needed."),
     ("likely to cause hypertension", False, "That is not the concern.")],
    [("the block is below the AV node", True,
      "A wide escape rhythm means the block is below the node, where atropine has no effect, so pacing is prepared at once."),
     ("the client is older than 75", False, "Age does not change its action."),
     ("the client takes lisinopril", False, "Not relevant."),
     ("the blood pressure is too low", False, "Atropine's limitation is the level of the block.")]],
   "In unstable complete heart block with a wide escape, the priority is to restore the rate, usually with "
   "transcutaneous pacing, because atropine rarely works.",
   "Prioritize treatment of unstable complete heart block."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "aha ph"),
   "For each prescription, indicate whether it is indicated or contraindicated for the client now.",
   ["Indicated", "Contraindicated"],
   [("Transcutaneous pacing", 0, "Pacing restores the rate while the drug effects wear off."),
    ("Dopamine infusion if pacing does not restore perfusion", 0,
     "A chronotropic infusion is the alternative or addition to pacing."),
    ("Lidocaine 1 mg/kg IV for the wide QRS complexes", 1,
     "Lidocaine would suppress the ventricular escape rhythm that is keeping the client alive."),
    ("Hold metoprolol and diltiazem", 0, "Both are causing the block."),
    ("Carotid sinus massage", 1, "Vagal stimulation slows the heart further."),
    ("Glucagon IV if beta blocker toxicity is suspected", 0,
     "Glucagon raises heart rate by a route that bypasses the beta receptor.")],
   "Pace, support with a chronotropic drug if needed, and remove the cause. Never suppress a ventricular escape "
   "rhythm or add vagal stimulation.",
   "Generate solutions for drug-induced complete heart block.",
   ex=[note("Prescriptions, 0940",
       "Transcutaneous pacing. Dopamine 5 to 20 mcg/kg/minute if pacing is ineffective. Lidocaine 1 mg/kg IV. Hold "
       "metoprolol and diltiazem. Carotid sinus massage. Glucagon 3 mg IV.", id="chb-orders")]),

order(M(PA, DYS, "take_action", "difficult", CV, "aha"),
   "The nurse is starting transcutaneous pacing. Place the steps in the order the nurse should do them.",
   [("Place the pacing pads in the anterior-posterior position", "Pads go on first so pacing can start quickly."),
    ("Connect the monitoring leads to the pacing defibrillator", "The pacer needs the ECG to sense and to show capture."),
    ("Set the pacing rate to 60 to 70/min", "A rate that restores perfusion is chosen before the output."),
    ("Increase the output (mA) until each spike is followed by a wide QRS", "This confirms electrical capture."),
    ("Check the femoral pulse to confirm mechanical capture", "A pulse matching the paced rate shows the heart is contracting."),
    ("Give analgesia or sedation as prescribed", "Pacing is painful; once it is working, comfort is addressed.")],
   "Pads and leads, then rate, then output to electrical capture, then a pulse check for mechanical capture, then "
   "comfort.",
   "Take action to start transcutaneous pacing.",
   ex=[flow("0945", ["0945"], [
       ("Heart rate", ["32/min"]),
       ("Blood pressure", ["74/40 mm Hg"]),
       ("Atropine 1 mg IV", ["Given at 0942; no change"]),
       ("Client", ["Responds only to voice"])], id="chb-0945")]),

matrix_mc(M(RRP, CV_DX, "evaluate_outcomes", "difficult", CV, "aha"),
   "Pacing is set at 70/min and 65 mA. Review the 1000 findings. For each finding, indicate whether it shows "
   "effective pacing or needs follow-up.",
   ["Effective pacing", "Needs follow-up"],
   [("Each pacing spike followed by a wide QRS", 0, "This is electrical capture."),
    ("Femoral pulse 70/min", 0, "Mechanical capture: the paced beats are producing pulses."),
    ("Blood pressure 106/62 mm Hg; client alert and oriented", 0, "Perfusion and mental status have recovered."),
    ("Client grimacing and rating chest wall pain 8 of 10", 1, "Pacing pain needs analgesia and sedation as prescribed."),
    ("Three pacing spikes in a row with no QRS after them", 1,
     "Intermittent loss of capture needs the output raised and the pads checked."),
    ("Skin under the pads intact", 0, "No burns; continue to check.")],
   "Capture is confirmed by a QRS after each spike and a matching pulse. Pain and loss of capture need action.",
   "Evaluate the effectiveness of transcutaneous pacing.",
   ex=[flow("1000", ["1000"], [
       ("Monitor", ["Paced rhythm 70/min; one run of three spikes with no QRS"]),
       ("Femoral pulse", ["70/min"]),
       ("Blood pressure", ["106/62 mm Hg"]),
       ("Mental status", ["Alert and oriented"]),
       ("Pain", ["8 of 10, chest wall, grimacing"]),
       ("Skin under pads", ["Intact"])], id="chb-1000")]),
]

# ================================================================================================
# Case 2: Permanent pacemaker with failure to capture after insertion
# ================================================================================================
ppm_tabs = [
    ("ppm-notes", "Nurses' notes", [note("Telemetry unit, postoperative day 1, 0600",
        "A 74-year-old client had a dual-chamber permanent pacemaker placed yesterday through the left subclavian "
        "vein for sick sinus syndrome; lower rate 60/min. Overnight the client took the left arm out of the sling "
        "several times to reach for the phone. This morning the client reports dizziness when sitting up and has "
        "had hiccups for an hour. The incision has a small, soft swelling with no active bleeding.",
        id="ppm-note")]),
    ("ppm-tele", "Telemetry", [flow("Telemetry summary", ["2200", "0600"], [
        ("Heart rate", ["60 to 72/min", "42 to 48/min"]),
        ("Pacing", ["Each ventricular spike followed by a wide QRS",
                    "Ventricular spikes at 60/min; about 1 in 4 followed by a QRS"]),
        ("Blood pressure", ["128/76 mm Hg", "94/58 mm Hg"])], id="ppm-flow")]),
]

ppm_qs = [
hl_text(M(RRP, POSTPROC, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that require follow-up.",
   ["The client had a pacemaker placed yesterday and ",
    ("took the left arm out of the sling several times", True,
     "Raising or moving the arm on the device side early can pull a new lead out of position."),
    ". This morning the client has ",
    ("dizziness when sitting up", True, "Dizziness with a slow rate suggests poor output from failed pacing."),
    " and ",
    ("hiccups for an hour", True, "Persistent hiccups can mean the lead is pacing the diaphragm after moving or perforating."),
    ". The incision has ",
    ("a small, soft swelling without bleeding", False, "A small, stable hematoma is common; it is watched, not urgent."),
    ". Telemetry shows ",
    ("spikes at 60/min with only some followed by a QRS", True, "Spikes without a QRS are failure to capture."),
    "."],
   "Arm movement, dizziness, hiccups and spikes without a QRS point to lead displacement with failure to capture.",
   "Recognize cues of pacemaker malfunction."),

matrix_mc(M(RRP, CV_DX, "analyze_cues", "difficult", CV, "ms"),
   "For each telemetry finding in a client with a pacemaker set at a lower rate of 60/min, indicate what it shows.",
   ["Normal pacemaker function", "Failure to capture", "Failure to sense (undersensing)"],
   [("A pacing spike followed by a wide QRS", 0, "The spike captured the ventricle."),
    ("A pacing spike with no QRS after it", 1, "The impulse fired but did not depolarize the ventricle."),
    ("No spikes while the client's own rhythm is 76/min", 0,
     "The pacemaker senses the faster intrinsic rhythm and holds back; this is correct."),
    ("A spike falling on the T wave just after the client's own beat", 2,
     "The pacemaker did not sense the intrinsic beat and fired anyway, which can trigger ventricular tachycardia."),
    ("Spikes at 60/min while the client's own rhythm is 40/min", 0, "Pacing at the lower rate is expected.")],
   "Capture means a QRS after each spike; sensing means no spike when the client's own rate is above the set rate.",
   "Analyze pacemaker rhythm strips."),

matrix_mc(M(RRP, POSTPROC, "prioritize_hypotheses", "difficult", CV, "ms"),
   "For each possible cause of the client's failure to capture, indicate how likely it is.",
   ["Most likely", "Possible, check", "Unlikely"],
   [("The ventricular lead has moved out of position", 0,
     "Early after insertion, arm movement plus hiccups strongly suggest a displaced lead."),
    ("Raised potassium increasing the capture threshold", 1,
     "Electrolyte changes can raise the threshold; a potassium level is checked."),
    ("Battery depletion of the new generator", 2, "A new generator has a full battery."),
    ("A hematoma at the incision", 2, "A small hematoma does not affect the lead in the heart.")],
   "Lead displacement is the most likely cause of failure to capture in the first days.",
   "Prioritize causes of pacemaker failure to capture."),

sata(M(RRP, POSTPROC, "generate_solutions", "difficult", CV, "ms aha"),
   "Which prescriptions should the nurse anticipate?",
   [("A chest x-ray to check lead position", True, "Shows whether the lead has moved."),
    ("A 12-lead ECG and a device check by the pacemaker team", True, "Confirms the problem and the thresholds."),
    ("Transcutaneous pacing pads applied", True, "Backup if the rate or pressure falls further."),
    ("A serum potassium level", True, "Rules out an electrolyte cause."),
    ("Range-of-motion exercises of the left arm above the shoulder", False,
     "Raising the arm on the device side must wait until the lead heals in, usually 4 to 6 weeks."),
    ("An MRI of the chest to locate the lead", False, "MRI is unsafe with a new device unless it is MRI-conditional and programmed."),
    ("Increase the metoprolol dose for the hiccups", False, "Beta blockers do not treat hiccups and would slow the rate.")],
   "Confirm the lead position, check the device and electrolytes, and have backup pacing ready.",
   "Generate solutions for pacemaker failure to capture.",
   ex=[note("Provider note, 0700",
       "Suspect lead displacement. Plan pending chest x-ray and device interrogation.", id="ppm-plan")]),

grouping(M(SIC, HOME, "take_action", "difficult", CV, "ms"),
   "The lead was repositioned, and capture is now reliable. The nurse prepares discharge teaching. For each topic, "
   "select the instructions the nurse should include.",
   [("Arm and incision", [
       ("Do not raise the left arm above the shoulder for about 4 weeks", True, "Lets the lead heal into the heart wall."),
       ("Keep the incision dry until the provider says it can be soaked", True, "Prevents infection of the device pocket."),
       ("Do arm circles overhead to prevent a frozen shoulder", False, "Overhead movement can displace the lead.")]),
    ("Devices in daily life", [
       ("Keep a cell phone at least 6 inches from the device", True, "Strong magnets in phones can interfere."),
       ("Avoid microwave ovens", False, "Modern microwave ovens are safe."),
       ("Tell staff about the device before any MRI", True, "MRI can damage or reprogram some devices.")]),
    ("When to call", [
       ("Report dizziness, fainting or a pulse below 60/min", True, "These suggest pacing failure."),
       ("Report hiccups that do not stop", True, "Persistent hiccups may mean the lead has moved."),
       ("Call only if the incision bleeds heavily", False, "Redness, warmth or drainage also need reporting.")])],
   "Teaching covers protecting the lead, avoiding interference and recognizing malfunction or infection.",
   "Teach a client after pacemaker insertion.",
   ex=[flow("Day 2, after lead repositioning", ["1400"], [
       ("Heart rate", ["60 to 70/min"]),
       ("Pacing", ["Each spike followed by a QRS"]),
       ("Hiccups", ["Stopped"]),
       ("Blood pressure", ["124/74 mm Hg"])], id="ppm-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", CV, "ms"),
   "For each statement the client makes, indicate whether it shows understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"I'll carry my pacemaker ID card in my wallet.\"", 0, "The card informs staff in an emergency and at security."),
    ("\"I can use my phone on my left ear if I keep calls short.\"", 1,
     "The phone should be held to the ear on the opposite side, at least 6 inches from the device."),
    ("\"I'll stand in the security scanner but not stay in it.\"", 0,
     "Walking through at a normal pace is safe; the client should not linger or lean against it."),
    ("\"I can go back to welding in my garage next week.\"", 1, "Arc welding creates fields that can interfere."),
    ("\"I'll take my pulse each morning and call if it's under 60.\"", 0, "Below the set rate suggests a problem."),
    ("\"I can lift my grandchild over my head after my first week at home.\"", 1,
     "Lifting the arm above shoulder level is avoided for about 4 weeks.")],
   "Effective teaching shows correct device ID, phone and security precautions, pulse checks and arm limits.",
   "Evaluate pacemaker discharge teaching.",
   ex=[note("Discharge teaching, day 2",
       "Teach-back session with the client and the client's daughter.", id="ppm-teach")]),
]

# ================================================================================================
# Case 3: Ventricular tachycardia in a client with an ICD
# ================================================================================================
vt_tabs = [
    ("vt-notes", "Nurses' notes", [note("Emergency department, 1900",
        "A 61-year-old client with ischemic cardiomyopathy (ejection fraction 20%) and an implantable cardioverter-"
        "defibrillator (ICD) received three shocks at home in the past hour. The client is alert and anxious and "
        "reports palpitations. Medications: furosemide 80 mg twice daily (increased last week), carvedilol, "
        "sacubitril-valsartan, spironolactone. The monitor shows a regular wide-complex rhythm at 168/min with "
        "uniform QRS complexes.", id="vt-note")]),
    ("vt-vitals", "Vital signs", [flow("Vital signs", ["1900"], [
        ("Blood pressure", ["104/68 mm Hg"]),
        ("Heart rate", ["168/min, regular"]),
        ("Respirations", ["20/min"]),
        ("SpO2", ["95% on room air"])], id="vt-flow")]),
    ("vt-labs", "Laboratory results", [table("Laboratory results", ["1900", "Reference range"], [
        ("Potassium", ["3.1 mEq/L", "3.5 to 5.0 mEq/L"]),
        ("Magnesium", ["1.5 mg/dL", "1.5 to 2.5 mg/dL"]),
        ("Troponin I", ["0.05 ng/mL", "< 0.04 ng/mL"]),
        ("Creatinine", ["1.4 mg/dL", "0.6 to 1.2 mg/dL"])], id="vt-lab")]),
]

vt_qs = [
hl_table(M(PA, DYS, "recognize_cues", "moderate", CV, "aha"),
   "Highlight the findings that are most important for the nurse to act on.",
   ["Assessment", "Finding"],
   [(["Rhythm", "Regular wide-complex rhythm at 168/min"], True, "Monomorphic ventricular tachycardia."),
    (["ICD", "Three shocks in an hour"], True, "Repeated shocks mean recurrent VT (an electrical storm)."),
    (["Potassium", "3.1 mEq/L"], True, "Low potassium after the furosemide increase triggers ventricular dysrhythmias."),
    (["Mental status", "Alert, anxious"], False, "Alert means the client is perfusing; useful for deciding stability."),
    (["Troponin I", "0.05 ng/mL"], False, "A slight rise after shocks and VT is expected; not a new MI by itself."),
    (["SpO2", "95% on room air"], False, "Adequate.")],
   "Recurrent VT with ICD shocks and low potassium are the cues that drive treatment.",
   "Recognize cues of ventricular tachycardia with ICD shocks."),

cloze(M(PA, DYS, "analyze_cues", "difficult", CV, "aha"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's VT is currently {{b1}} because {{b2}}. The most likely trigger is {{b3}}.",
   [[("stable", True, "The client has a pulse, is alert, and has an acceptable blood pressure."),
     ("unstable", False, "There is no hypotension, altered mentation, ischemic chest pain or acute heart failure."),
     ("pulseless", False, "The client is talking."),
     ("polymorphic", False, "The QRS complexes are uniform.")],
    [("the client is alert with a systolic pressure above 90 mm Hg", True,
      "Stability is judged by perfusion, not by the rate."),
     ("the rate is below 200/min", False, "Rate alone does not decide stability."),
     ("the ICD is working", False, "The ICD's function does not define stability."),
     ("the troponin is only slightly raised", False, "Not the measure of stability.")],
    [("hypokalemia from the higher furosemide dose", True, "Low potassium lowers the threshold for VT."),
     ("spironolactone", False, "Spironolactone keeps potassium in."),
     ("an acute STEMI", False, "Troponin is only slightly raised and there is no ST elevation reported."),
     ("dehydration from the ICD", False, "Not a mechanism.")]],
   "Stable monomorphic VT with a reversible trigger: correct potassium and magnesium and give an antidysrhythmic.",
   "Analyze the stability and cause of VT."),

matrix_mc(M(PA, DYS, "prioritize_hypotheses", "difficult", CV, "aha"),
   "For each complication, indicate whether it is the most immediate threat or a lesser threat now.",
   ["Most immediate threat", "Lesser threat now"],
   [("Degeneration of VT to ventricular fibrillation or pulseless VT", 0,
     "With an ejection fraction of 20% and recurrent VT, collapse can happen at any moment."),
    ("Acute heart failure from the fast rate", 0, "A weak ventricle tolerates VT poorly and can fail quickly."),
    ("Psychological distress from the shocks", 1, "Real and important, but after rhythm control."),
    ("Kidney injury from the creatinine of 1.4", 1, "A mild rise that is monitored.")],
   "The immediate threats are collapse into a pulseless rhythm and acute pump failure.",
   "Prioritize hypotheses in VT with a weak ventricle."),

matrix_mr(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "aha ph"),
   "For each treatment, indicate the situations in which it is used. Each row may have more than one answer.",
   ["Stable VT with a pulse", "Unstable VT with a pulse", "Pulseless VT"],
   [("Amiodarone 150 mg IV over 10 minutes", {0}, "The antidysrhythmic dose for VT with a pulse."),
    ("Synchronized cardioversion", {0, 1},
     "Used at once for unstable VT with a pulse, and for stable VT that does not respond to drugs."),
    ("Defibrillation (unsynchronized shock)", {2}, "Pulseless VT is treated like VF."),
    ("Epinephrine 1 mg IV every 3 to 5 minutes", {2}, "Given in cardiac arrest after the second shock."),
    ("Amiodarone 300 mg IV push", {2}, "The arrest dose for shock-refractory VF or pulseless VT."),
    ("Potassium and magnesium replacement", {0, 1, 2}, "Correcting the trigger matters in every situation.")],
   "Treatment depends on pulse and stability: drugs or synchronized shock with a pulse, defibrillation and the arrest "
   "drugs without one. Electrolytes are corrected in all.",
   "Generate solutions for VT by stability and pulse.",
   ex=[note("Prescriptions, 1910",
       "Amiodarone 150 mg IV over 10 minutes, then 1 mg/minute for 6 hours. Potassium chloride 40 mEq IV at 10 mEq/hour. "
       "Magnesium sulfate 2 g IV over 1 hour. Defibrillator pads on the client.", id="vt-orders")]),

order(M(PA, BLS, "take_action", "difficult", CV, "aha"),
   "At 1925 the client becomes unresponsive. The monitor shows VT, the ICD has not fired, and there is no carotid "
   "pulse. Place the nurse's actions in order.",
   [("Call for help and the code team", "Help is summoned at once; the pads are already on."),
    ("Deliver an unsynchronized shock through the pads", "In a monitored arrest with pads on, defibrillation comes first."),
    ("Start compressions at once and continue for 2 minutes", "Compressions follow the shock without a pulse check."),
    ("Check the rhythm and shock again if VT persists", "The rhythm is checked after 2 minutes of CPR."),
    ("Give epinephrine 1 mg IV after the second shock", "Epinephrine follows the second shock in shockable rhythms.")],
   "In a monitored pulseless VT with pads on: call for help, shock, compress for 2 minutes, recheck and shock again, "
   "then epinephrine.",
   "Take action for pulseless VT.",
   ex=[flow("1925", ["1925"], [
       ("Responsiveness", ["None"]),
       ("Carotid pulse", ["Absent"]),
       ("Monitor", ["Wide-complex tachycardia at 190/min"]),
       ("ICD", ["No shock delivered"]),
       ("Defibrillator pads", ["Already on the chest"])], id="vt-1925")]),

matrix_mc(M(PPT, CV_MEDS, "evaluate_outcomes", "difficult", CV, "aha ph"),
   "After one shock, the client has a pulse and is on an amiodarone infusion. Review the 2300 findings. For each "
   "finding, indicate whether it shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Sinus rhythm 74/min with no further VT", 0, "Rhythm control is working."),
    ("Potassium 4.2 mEq/L, magnesium 2.1 mg/dL", 0, "The trigger has been corrected."),
    ("QTc 528 ms (442 ms on admission)", 1,
     "Amiodarone lengthens the QT; above 500 ms the risk of torsades rises, so the provider must review the dose."),
    ("Blood pressure 84/50 mm Hg during the infusion", 1, "Amiodarone IV can cause hypotension; report it."),
    ("Alert, oriented, follows commands", 0, "Neurologic recovery after a brief arrest."),
    ("Peripheral IV site red and painful along the vein", 1,
     "Amiodarone irritates veins; the site needs changing, ideally to a central line.")],
   "Rhythm and electrolytes show success. QT prolongation, hypotension and phlebitis are amiodarone effects needing "
   "action.",
   "Evaluate outcomes of amiodarone therapy.",
   ex=[flow("2300", ["2300"], [
       ("Rhythm", ["Sinus rhythm 74/min, no VT"]),
       ("Electrolytes", ["Potassium 4.2 mEq/L, magnesium 2.1 mg/dL"]),
       ("QTc", ["528 ms"]),
       ("Blood pressure", ["84/50 mm Hg"]),
       ("Neurologic", ["Alert, oriented, follows commands"]),
       ("IV site, left forearm", ["Red, painful along the vein"])], id="vt-2300")]),
]

# ================================================================================================
# Case 4: Cardiogenic shock with a pulmonary artery catheter
# ================================================================================================
cs_tabs = [
    ("cs-notes", "Nurses' notes", [note("Cardiac ICU, 0400",
        "A 69-year-old client is 20 hours after a stent to the left anterior descending artery for an anterior STEMI. "
        "Over the last 2 hours the client has become restless and short of breath. Skin is cool and clammy, with "
        "crackles to the mid-lung fields and an S3. A pulmonary artery catheter was placed at 0330. Weight 90 kg.",
        id="cs-note")]),
    ("cs-hemo", "Hemodynamics", [table("Hemodynamic and laboratory values", ["0400", "Reference range"], [
        ("Blood pressure", ["82/56 mm Hg (MAP 65)", "-"]),
        ("Heart rate", ["118/min", "60 to 100/min"]),
        ("CVP", ["14 mm Hg", "2 to 6 mm Hg"]),
        ("Pulmonary artery wedge pressure", ["24 mm Hg", "8 to 12 mm Hg"]),
        ("Cardiac index", ["1.6 L/min/m2", "2.5 to 4.0 L/min/m2"]),
        ("Systemic vascular resistance", ["1,850 dynes/s/cm-5", "800 to 1,200 dynes/s/cm-5"]),
        ("Lactate", ["4.2 mmol/L", "0.5 to 2.0 mmol/L"]),
        ("Urine output", ["15 mL/hour", "-"]),
        ("SpO2", ["89% on 4 L/min nasal cannula", "-"])], id="cs-table")]),
]

cs_qs = [
hl_table(M(RRP, CV_DX, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the values that show the heart is failing as a pump.",
   ["Measure", "Value"],
   [(["Cardiac index", "1.6 L/min/m2"], True, "A cardiac index below 2.2 shows severely reduced output."),
    (["Pulmonary artery wedge pressure", "24 mm Hg"], True, "High left-sided filling pressure: the left ventricle cannot empty."),
    (["Lactate", "4.2 mmol/L"], True, "Tissues are using anaerobic metabolism because of poor perfusion."),
    (["Urine output", "15 mL/hour"], True, "The kidneys are underperfused."),
    (["Heart rate", "118/min"], False, "Compensatory; it follows from low output rather than showing pump failure itself."),
    (["Systemic vascular resistance", "1,850 dynes/s/cm-5"], False,
     "High SVR is the body's compensation by vasoconstriction, not pump failure itself.")],
   "Low cardiac index with high wedge pressure, plus signs of poor tissue perfusion, define cardiogenic shock.",
   "Recognize hemodynamic cues of pump failure."),

dtable(M(RRP, CV_DX, "analyze_cues", "difficult", CV, "ms"),
   "For each hemodynamic pattern, choose the type of shock it fits.",
   ["Wedge pressure", "Cardiac index", "SVR", "Type of shock"],
   [(["High", "Low", "High"], [
       ("Cardiogenic", True, "The pump fails, so pressure backs up and the vessels constrict."),
       ("Hypovolemic", False, "Hypovolemia has low filling pressures."),
       ("Septic (early)", False, "Septic shock has low SVR."),
       ("Neurogenic", False, "Neurogenic shock has low SVR and bradycardia.")]),
    (["Low", "Low", "High"], [
       ("Hypovolemic", True, "Low volume lowers filling pressure and output, with compensatory vasoconstriction."),
       ("Cardiogenic", False, "Wedge pressure is high in cardiogenic shock."),
       ("Septic (early)", False, "SVR is low in sepsis."),
       ("Anaphylactic", False, "Anaphylaxis lowers SVR.")]),
    (["Low or normal", "High", "Low"], [
       ("Septic (early)", True, "Vasodilation lowers SVR, and output rises to compensate."),
       ("Cardiogenic", False, "Output is low in cardiogenic shock."),
       ("Hypovolemic", False, "SVR is high in hypovolemia."),
       ("Obstructive", False, "Obstruction lowers output.")])],
   "Wedge pressure, cardiac index and SVR together separate cardiogenic, hypovolemic and distributive shock.",
   "Analyze hemodynamic profiles of shock."),

cloze(M(PA, SHOCK, "prioritize_hypotheses", "difficult", CV, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client is in {{b1}} shock caused by {{b2}}, and the treatment goal is to {{b3}}.",
   [[("cardiogenic", True, "Pump failure after a large anterior MI."),
     ("hypovolemic", False, "Filling pressures are high."),
     ("septic", False, "SVR is high, not low."),
     ("neurogenic", False, "The client is tachycardic.")],
    [("loss of left ventricular muscle", True, "An anterior MI can destroy enough muscle to cause pump failure."),
     ("bleeding from the stent site", False, "Bleeding would lower filling pressures."),
     ("a reaction to contrast", False, "Not consistent with the profile."),
     ("dehydration", False, "Filling pressures are high.")],
    [("improve contractility and perfusion without adding volume", True,
      "The heart is full but weak; it needs an inotrope and perfusion support, not fluid."),
     ("give fluid until the wedge pressure rises", False, "Wedge pressure is already high."),
     ("lower the heart rate with a beta blocker", False, "A negative inotrope would worsen shock."),
     ("raise the SVR further", False, "A higher afterload makes the failing ventricle work harder.")]],
   "Cardiogenic shock after anterior MI needs contractility support and perfusion, not more volume.",
   "Prioritize the problem in cardiogenic shock."),

matrix_mc(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph ms"),
   "For each prescription, indicate whether it is indicated or contraindicated for the client now.",
   ["Indicated", "Contraindicated"],
   [("Dobutamine infusion", 0, "An inotrope increases contractility and cardiac output."),
    ("Norepinephrine infusion to keep the MAP at 65 mm Hg or above", 0,
     "A vasopressor maintains perfusion pressure when blood pressure is too low."),
    ("0.9% sodium chloride 1 L bolus", 1, "With a wedge pressure of 24 and crackles, fluid worsens pulmonary edema."),
    ("Mechanical circulatory support, such as an intra-aortic balloon pump", 0,
     "Mechanical support improves coronary perfusion and reduces afterload when drugs are not enough."),
    ("Metoprolol 5 mg IV for the tachycardia", 1, "A beta blocker lowers contractility in shock."),
    ("Nitroprusside infusion to lower the afterload", 1,
     "With a MAP of 65 and a systolic pressure of 82, a vasodilator would cause dangerous hypotension.")],
   "Support contractility and pressure, add mechanical support if needed, and avoid fluid, negative inotropes and "
   "vasodilators while the client is hypotensive.",
   "Generate solutions for cardiogenic shock.",
   ex=[note("Prescriptions, 0415",
       "Dobutamine 5 mcg/kg/minute. Norepinephrine titrated to MAP 65 mm Hg or above. 0.9% sodium chloride 1 L bolus. "
       "Cardiology to evaluate for an intra-aortic balloon pump. Metoprolol 5 mg IV. Nitroprusside 0.3 mcg/kg/minute.",
       id="cs-orders")]),

calc(M(PPT, CALC, "take_action", "difficult", CV, "ph"),
   "Dobutamine is prescribed at 5 mcg/kg/minute. The premixed bag contains dobutamine 500 mg in 250 mL. The client "
   "weighs 90 kg. At what rate should the nurse set the pump? Record your answer to one decimal place.",
   "mL/hour", 1, "Rate", 13.5,
   "5 mcg/kg/min x 90 kg = 450 mcg/min. 450 x 60 = 27,000 mcg/hour = 27 mg/hour. The bag is 500 mg/250 mL = "
   "2 mg/mL, so 27 / 2 = 13.5 mL/hour.",
   "Calculate an inotrope infusion rate.",
   ex=[note("Infusion label",
       "Dobutamine 500 mg in 250 mL D5W. Prescribed dose: 5 mcg/kg/minute. Weight: 90 kg.", id="cs-label")]),

matrix_mc(M(RRP, CV_DX, "evaluate_outcomes", "difficult", CV, "ms ph"),
   "Review the values 6 hours after dobutamine and norepinephrine were started. For each value, indicate whether it "
   "shows improvement or needs follow-up.",
   ["Shows improvement", "Needs follow-up"],
   [("Cardiac index 2.4 L/min/m2", 0, "Output has risen toward normal."),
    ("Wedge pressure 16 mm Hg", 0, "Left-sided filling pressure is falling."),
    ("Lactate 2.0 mmol/L", 0, "Tissue perfusion is improving."),
    ("Urine output 40 mL/hour", 0, "Kidney perfusion is improving."),
    ("Frequent PVCs with 4-beat runs of VT", 1,
     "Dobutamine increases myocardial irritability; the dose or plan needs review."),
    ("SVR 1,250 dynes/s/cm-5", 0, "Less vasoconstriction as output recovers.")],
   "Rising output with falling wedge pressure and lactate shows improvement. Ventricular ectopy is a dobutamine "
   "effect that needs action.",
   "Evaluate the response to inotropes in cardiogenic shock.",
   ex=[table("Values at 1000", ["0400", "1000"], [
       ("Cardiac index", ["1.6", "2.4 L/min/m2"]),
       ("Wedge pressure", ["24", "16 mm Hg"]),
       ("SVR", ["1,850", "1,250 dynes/s/cm-5"]),
       ("Lactate", ["4.2", "2.0 mmol/L"]),
       ("Urine output", ["15", "40 mL/hour"]),
       ("Rhythm", ["Sinus tachycardia", "Sinus 96/min with frequent PVCs and 4-beat runs of VT"])], id="cs-1000")]),
]

cases = [
    case("Drug-induced complete heart block and transcutaneous pacing", chb_tabs, chb_qs),
    case("Permanent pacemaker failure to capture", ppm_tabs, ppm_qs),
    case("Ventricular tachycardia in a client with an ICD", vt_tabs, vt_qs),
    case("Cardiogenic shock with a pulmonary artery catheter", cs_tabs, cs_qs),
]

BUILT = build(102, "cardiac-cases-pacing-vt-shock",
              "Cardiac case studies: complete heart block and pacing, pacemaker failure to capture, VT with an ICD, and "
              "cardiogenic shock with hemodynamic monitoring",
              items, cases, case_conversion=True)
