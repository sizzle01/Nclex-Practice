"""Batch 119 (case conversion, chunk 5: neurologic): four full NGN case studies and four standalone items
(CASE-MAP.md, book_topics.py).

Cases: ALS with respiratory decline and goals-of-care decisions; Bell palsy and trigeminal neuralgia (cranial nerves
V and VII); a TIA followed by carotid endarterectomy, with statin therapy (antilipidemics); a head-to-toe assessment
using the admission template on a new long-term care resident. Each case walks the six CJMM steps in order, unfolds
with new data in steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                         "PSI")
NEURO, FUND, CV = "neurological", "fundamentals", "cardiovascular"
DEGEN = "degenerative-neurological-disorders"
NEURO_DX = "neurological-assessment-and-procedures"
NEURO_MEDS = "neurological-medications"
STROKE = "stroke-care"
ICP = "intracranial-pressure-and-head-injury"
ADVANCE = "advance-directives"
PALL = "palliative-and-end-of-life-care"
VASC = "vascular-disorders"
POSTOP = "postoperative-care"
CV_MEDS = "cardiovascular-medications"
TEACH = "health-teaching-and-behavior-change"
AGING = "aging-and-older-adult-health"
NUTR = "nutrition-and-therapeutic-diets"
SKIN = "pressure-injury-prevention"
INJURY = "injury-prevention"
SENSORY = "sensory-impairment-care"
PRIORITY = "prioritizing-client-care"
DELEG = "delegation-and-supervision"
EYE = "eye-and-ear-disorders"

# ================================================================================================
# Standalone items
# ================================================================================================
items = [
bowtie(M(PA, ICP, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "After reading the ICU note, complete the diagram with the most likely neurologic emergency, the 2 actions the "
   "nurse should take first, and the 2 assessments to repeat.",
   [("Rising intracranial pressure with impending herniation", True, "Falling GCS, Cushing triad and a dilated pupil."),
    ("Hypovolemic shock", False, "Blood pressure is high, not low."),
    ("Alcohol intoxication", False, "Does not explain the pupil and vital sign pattern."),
    ("Hypoglycemia", False, "Glucose is normal.")],
   [("Notify the neurosurgeon now", True, "Herniation needs urgent decompression."),
    ("Give hypertonic saline", True, "Lowers ICP."),
    ("Lay the head of the bed flat to rest", False, "Raises ICP."),
    ("Give a sedative for restlessness", False, "Masks deterioration."),
    ("Prepare for a lumbar puncture", False, "Can precipitate herniation.")],
   [("Pupil size and reactivity", True, "Tracks herniation."),
    ("Glasgow Coma Scale", True, "Tracks consciousness."),
    ("Urine specific gravity once a day", False, "Not the main guide."),
    ("Bowel sounds each shift", False, "Not relevant."),
    ("Weekly body weight", False, "Not relevant.")],
   "Cushing triad with a falling GCS and a blown pupil means herniation: escalate and lower ICP.",
   "Recognize and act on rising intracranial pressure.",
   ex=[note("ICU", "Client with a traumatic brain injury: GCS fell from 12 to 7 in an hour; BP 190/70 mm Hg, HR 50/min, "
       "irregular breathing; right pupil 6 mm and fixed. Glucose 118 mg/dL.", id="icp-note")]),

dtable(M(RRP, NEURO_DX, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "A client with a head injury has hourly neurologic checks. For each trend, choose the interpretation.",
   ["Measure", "0800 to 1100", "Interpretation"],
   [(["GCS", "14, 14, 13, 11"], [
       ("Deteriorating; report now", True, "A fall of 2 or more points is significant."),
       ("Stable", False, "It is falling."),
       ("Improving", False, "It is falling.")]),
    (["Motor response", "Obeys, obeys, localizes, flexes to pain"], [
       ("Worsening motor response", True, "Moving from obeying to abnormal flexion."),
       ("Normal variation", False, "A real change."),
       ("Improving", False, "Worse.")]),
    (["Pupils", "Equal and reactive throughout"], [
       ("Not yet changed; keep checking", True, "Pupil changes can be late."),
       ("Proves ICP is normal", False, "Late sign; GCS changed first."),
       ("Shows herniation", False, "Not yet.")])],
   "Falling GCS and motor response come before pupil changes; report the trend now.",
   "Evaluate a neurologic check trend.",
   ex=[flow("Hourly neuro checks", ["0800", "0900", "1000", "1100"], [
       ("GCS", ["14", "14", "13", "11"]),
       ("Motor", ["Obeys", "Obeys", "Localizes", "Flexes to pain"]),
       ("Pupils", ["Equal, reactive", "Equal, reactive", "Equal, reactive", "Equal, reactive"])], id="gcs-trend")]),

mc(M(MOC, PRIORITY, "prioritize_hypotheses", "difficult", NEURO, "mgmt ms"),
   "On a neurology unit at the start of the shift, four clients have needs. Whose bedside should the nurse go to "
   "first?",
   [("A client 2 days after an ischemic stroke who is due for a swallow screen before breakfast", False,
     "Important today, but scheduled."),
    ("A client with myasthenia gravis whose vital capacity fell from 2.0 to 1.2 L overnight", True,
     "Falling vital capacity signals impending respiratory failure."),
    ("A client with Parkinson disease whose 0800 levodopa is due in 20 minutes", False,
     "Time-critical, but there is still time to give it on schedule."),
    ("A client with multiple sclerosis who reports bladder urgency and wants a bedpan", False,
     "Delegate assistance; not unstable.")],
   "Declining respiratory muscle strength in myasthenia gravis is the most urgent problem.",
   "Prioritize clients on a neurology unit."),

sata(M(MOC, DELEG, "take_action", "difficult", NEURO, "mgmt"),
   "Which tasks can the RN delegate to the unlicensed assistive personnel (UAP) on a stroke unit?",
   [("Reposition a client with hemiplegia every 2 hours", True, "Routine positioning."),
    ("Help a screened client eat sitting upright", True, "Routine with clear instructions."),
    ("Report new slurred speech to the RN", True, "Reporting a change."),
    ("Do the first swallow screen on a client after a stroke", False, "Assessment by a nurse or speech therapist."),
    ("Perform the NIH Stroke Scale assessment each shift", False, "Neurologic assessment by the RN."),
    ("Give crushed tablets in applesauce to a client with dysphagia", False, "Medication administration.")],
   "UAPs reposition, assist with meals after screening and report changes; assessment and medicines stay with "
   "licensed nurses.",
   "Delegate on a stroke unit."),
]

# ================================================================================================
# Case 1: ALS and goals of care
# ================================================================================================
als_tabs = [
    ("als-notes", "Nurses' notes", [note("Neurology clinic",
        "A 61-year-old client with amyotrophic lateral sclerosis diagnosed 2 years ago now has slurred speech, weak "
        "hands, and trouble swallowing thin liquids, and has lost 6 kg in 3 months. The client sleeps propped on 3 "
        "pillows and wakes with headaches. Thinking and memory are unaffected. The client says, \"I don't want to "
        "end up on a machine, but I'm not ready to die.\"", id="als-note")]),
    ("als-data", "Findings", [table("Findings", ["Result"], [
        ("Forced vital capacity", ["48% of predicted"]),
        ("Morning ABG", ["PaCO2 52 mm Hg"]),
        ("Weight", ["58 kg (64 kg three months ago)"]),
        ("Advance directive", ["None"])], id="als-table")]),
]

als_qs = [
hl_text(M(PA, DEGEN, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that require follow-up.",
   ["The client has ",
    ("trouble swallowing thin liquids", True, "Bulbar weakness; aspiration risk."),
    " and has lost ",
    ("6 kg in 3 months", True, "Malnutrition."),
    ". The client ",
    ("sleeps on 3 pillows and wakes with headaches", True, "Nocturnal hypoventilation with CO2 retention."),
    ". The client's ",
    ("thinking and memory are unaffected", False, "Typical of ALS; the client can make their own decisions."),
    ". The forced vital capacity is ",
    ("48% of predicted", True, "Below 50% is a trigger to discuss ventilation."),
    "."],
   "Swallowing, weight loss and falling breathing capacity all signal progression.",
   "Recognize cues of ALS progression."),

matrix_mc(M(PA, DEGEN, "analyze_cues", "difficult", NEURO, "ms"),
   "For each statement, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("Morning headaches with a high PaCO2 suggest hypoventilation during sleep", 0, "CO2 builds up overnight."),
    ("ALS affects both upper and lower motor neurons", 0, "Mixed signs."),
    ("ALS usually causes early dementia", 1, "Cognition is usually preserved; some develop frontotemporal changes."),
    ("Noninvasive ventilation at night can improve symptoms and survival", 0, "Evidence supports it."),
    ("A feeding tube should be discussed while breathing capacity is still adequate", 0, "Safer placement before FVC falls too low.")],
   "Hypoventilation, preserved cognition and the timing of ventilation and feeding decisions.",
   "Analyze ALS findings."),

cloze(M(MOC, ADVANCE, "prioritize_hypotheses", "difficult", NEURO, "mgmt"),
   "Complete the sentences by choosing from the lists of options.",
   "The most urgent need is a {{b1}}, because {{b2}}. The client's statement means the team should {{b3}}.",
   [[("goals-of-care discussion about breathing support and feeding", True, "Decisions are needed before a crisis."),
     ("referral for physical therapy", False, "Helpful, but not most urgent."),
     ("new hearing test", False, "Not relevant."),
     ("trial of a stronger antidepressant", False, "Not indicated.")],
    [("breathing capacity is falling and a crisis could force an unplanned decision", True, "Planning ahead protects choice."),
     ("the client is confused", False, "Cognition is intact."),
     ("the family must decide", False, "The capable client decides."),
     ("insurance requires it", False, "Not the reason.")],
    [("explore what 'a machine' means and offer noninvasive options", True, "Clarifies values; NIV may fit."),
     ("schedule a tracheostomy", False, "Against the client's wish."),
     ("assume the client wants comfort care only", False, "The client said they are not ready to die."),
     ("avoid the topic to reduce distress", False, "Avoidance takes away choice.")]],
   "A timely goals-of-care conversation respects a capable client's mixed wishes.",
   "Prioritize goals-of-care planning in ALS."),

matrix_mr(M(MOC, ADVANCE, "generate_solutions", "difficult", NEURO, "mgmt ms"),
   "For each option, select the statements that apply. Each row may have more than one answer.",
   ["Supports breathing or nutrition", "Fits a wish to avoid a 'machine'", "Can be stopped later if the client chooses"],
   [("Noninvasive ventilation (BiPAP) at night", {0, 1, 2}, "A mask, not a tube; can be stopped."),
    ("Tracheostomy with a ventilator", {0, 2}, "Long-term invasive support; withdrawing it is legal but complex."),
    ("A gastrostomy feeding tube", {0, 2}, "Supports nutrition."),
    ("Comfort-focused care with medicines for breathlessness", {1, 2}, "Relieves symptoms without life support.")],
   "Each option is weighed against the client's own goals; all can be withdrawn if the client chooses.",
   "Generate options in a goals-of-care discussion.",
   ex=[note("Palliative care visit", "The client and spouse meet the palliative team.", id="als-pall")]),

sata(M(MOC, ADVANCE, "take_action", "difficult", NEURO, "mgmt"),
   "The client chooses nighttime BiPAP and a feeding tube but no tracheostomy or CPR. Which actions should the nurse "
   "take?",
   [("Help complete an advance directive", True, "Records the wishes."),
    ("Ask the provider for DNR and POLST orders", True, "Makes wishes actionable across settings."),
    ("Share the plan with home care", True, "Continuity."),
    ("Ask the spouse to sign the advance directive for the client", False, "The capable client signs."),
    ("Keep the decisions verbal so they can change easily", False, "Written documents protect the client; they can still be changed.")],
   "Document the decisions, appoint an agent and share the plan.",
   "Take action to record end-of-life wishes.",
   ex=[note("Two weeks later", "The client communicates by tablet and wants the plan in writing.", id="als-2wk")]),

matrix_mc(M(BCC, PALL, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "Three months later, for each finding, indicate whether the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("Morning headaches gone with BiPAP", 0, "Hypoventilation treated."),
    ("Weight stable on tube feeding", 0, "Nutrition maintained."),
    ("Spouse says, \"I'm exhausted; I can't sleep.\"", 1, "Caregiver strain; offer respite and support."),
    ("Advance directive copy in the home and the clinic record", 0, "Accessible."),
    ("Skin redness on the bridge of the nose", 1, "Mask pressure; refit or change the interface.")],
   "Breathing and nutrition goals are met; caregiver strain and mask pressure need attention.",
   "Evaluate ALS care outcomes.",
   ex=[note("3-month visit", "FVC 38% of predicted.", id="als-3m")]),
]

# ================================================================================================
# Case 2: Bell palsy and trigeminal neuralgia
# ================================================================================================
cn_tabs = [
    ("cn-notes", "Clinic notes", [note("Primary care",
        "Client A, 34, woke today unable to close the left eye, with drooping of the whole left side of the face, "
        "including the forehead. Taste is altered and loud sounds hurt the left ear. No arm or leg weakness; speech "
        "is clear. Client B, 66, has sudden stabbing, electric pains in the right cheek and jaw lasting seconds, set "
        "off by chewing, brushing teeth or a breeze; the client has stopped eating and lost 3 kg.", id="cn-note")]),
]

cn_qs = [
hl_text(M(RRP, NEURO_DX, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that identify which cranial nerve is affected in each client.",
   ["Client A cannot close the left eye, with ",
    ("drooping of the whole left face including the forehead", True, "A peripheral CN VII lesion affects the forehead too."),
    ", ",
    ("altered taste", True, "CN VII carries taste from the front of the tongue."),
    " and ",
    ("painful loud sounds in the left ear", True, "CN VII supplies the stapedius muscle."),
    ". Client A has ",
    ("no arm or leg weakness and clear speech", True, "Points away from a stroke."),
    ". Client B has ",
    ("brief electric pains in the cheek and jaw triggered by touch or chewing", True, "Typical of trigeminal (CN V) neuralgia."),
    " and is ",
    ("66 years old", False, "Age is typical but does not identify the nerve."),
    "."],
   "Client A has a CN VII (Bell) palsy; client B has CN V neuralgia.",
   "Recognize cranial nerve V and VII disorders."),

matrix_mc(M(PA, STROKE, "analyze_cues", "difficult", NEURO, "ms"),
   "For each finding, indicate whether it fits Bell palsy or a stroke.",
   ["Bell palsy", "Stroke"],
   [("Forehead is also weak on the affected side", 0, "Peripheral lesion; strokes usually spare the forehead."),
    ("Arm drift on the same side as the facial droop", 1, "Central lesion."),
    ("Slurred speech and confusion", 1, "Brain involvement."),
    ("Isolated facial weakness that developed over a day", 0, "Typical of Bell palsy."),
    ("Altered taste on the front of the tongue", 0, "Peripheral CN VII.")],
   "Forehead involvement and isolated facial weakness point to Bell palsy; limb or speech signs point to stroke.",
   "Analyze facial weakness."),

matrix_mc(M(PA, EYE, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "For each risk, indicate its priority.",
   ["High priority", "Lower priority"],
   [("Client A: corneal injury from an eye that does not close", 0, "Can cause permanent damage."),
    ("Client B: malnutrition and dehydration from avoiding eating", 0, "Already losing weight."),
    ("Client A: missing a stroke", 0, "Always rule out stroke with sudden facial weakness."),
    ("Client B: cosmetic concerns", 1, "Not the issue.")],
   "Protect the eye, rule out stroke, and keep the client with neuralgia nourished.",
   "Prioritize risks in cranial nerve disorders."),

grouping(M(PPT, NEURO_MEDS, "generate_solutions", "difficult", NEURO, "ph ms"),
   "For each client, select the treatments the nurse should anticipate.",
   [("Client A (Bell palsy)", [
       ("Prednisone started within 72 hours", True, "Improves recovery."),
       ("Lubricating eye drops by day and taping the eye at night", True, "Protects the cornea."),
       ("Carbamazepine", False, "For neuralgia, not palsy.")]),
    ("Client B (trigeminal neuralgia)", [
       ("Carbamazepine", True, "First-line drug."),
       ("Soft, lukewarm foods eaten on the unaffected side", True, "Avoids triggers."),
       ("Ice packs on the face", False, "Cold can trigger pain.")])],
   "Steroids and eye protection for Bell palsy; carbamazepine and trigger avoidance for neuralgia.",
   "Generate treatment for cranial nerve disorders.",
   ex=[note("Plans", "Client A: prednisone and valacyclovir. Client B: carbamazepine 100 mg twice daily.", id="cn-plans")]),

sata(M(PPT, NEURO_MEDS, "take_action", "difficult", NEURO, "ph"),
   "Client B returns 3 weeks after starting carbamazepine. Review the findings. Which actions should the nurse take?",
   [("Report the sore throat and fever with the low white count", True, "Possible bone marrow suppression."),
    ("Hold the next dose until the provider responds", True, "Serious adverse effect."),
    ("Check the sodium result", True, "Carbamazepine causes hyponatremia; the client is confused."),
    ("Ask about any rash or mouth sores", True, "Stevens-Johnson syndrome warning signs."),
    ("Tell the client to take the dose with grapefruit juice to boost the effect", False, "Grapefruit raises levels to toxic range."),
    ("Double the dose because the pain is controlled", False, "No reason to increase.")],
   "Carbamazepine can suppress the marrow, lower sodium and cause severe skin reactions.",
   "Take action for carbamazepine adverse effects.",
   ex=[flow("3-week visit", ["Week 3"], [
       ("Pain", ["Much better"]),
       ("Client report", ["Sore throat and fever for 2 days; feels muddled"]),
       ("WBC", ["2,800/mm3"]),
       ("Sodium", ["126 mEq/L"]),
       ("Carbamazepine level", ["9 mcg/mL (4 to 12)"])], id="cn-3wk")]),

matrix_mc(M(BCC, SENSORY, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "At follow-up, for each finding, indicate whether the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("Client A: can wrinkle the left forehead slightly at 3 weeks", 0, "Recovery under way."),
    ("Client A: red, gritty left eye, not using the night tape", 1, "Corneal irritation; reinforce eye protection."),
    ("Client A: no new limb weakness", 0, "No stroke signs."),
    ("Client B: eating soft foods, weight stable", 0, "Nutrition improved."),
    ("Client B: switched to another drug after the low white count, pain controlled", 0, "Safe alternative.")],
   "Both are improving; Client A needs better eye protection.",
   "Evaluate outcomes in cranial nerve disorders.",
   ex=[note("Follow-up", "Both clients seen in clinic.", id="cn-fu")]),
]

# ================================================================================================
# Case 3: TIA, carotid endarterectomy and statin therapy
# ================================================================================================
tia_tabs = [
    ("tia-notes", "Nurses' notes", [note("Emergency department and vascular unit",
        "A 68-year-old client had 20 minutes of right hand weakness and trouble finding words this morning; it has "
        "fully resolved. The client smokes, has hypertension and diabetes, and stopped atorvastatin a year ago "
        "because of muscle aches. Carotid ultrasound: 80% narrowing of the left internal carotid artery.",
        id="tia-note")]),
    ("tia-labs", "Labs", [table("Labs", ["Result", "Reference"], [
        ("LDL cholesterol", ["168 mg/dL", "below 70 for this client"]),
        ("HbA1c", ["8.2%", "-"]),
        ("ALT", ["28 units/L", "7 to 56"]),
        ("CK", ["110 units/L", "30 to 200"]),
        ("Blood pressure", ["164/94 mm Hg", "-"])], id="tia-table")]),
]

tia_qs = [
hl_table(M(PA, STROKE, "recognize_cues", "moderate", NEURO, "ms aha"),
   "Highlight the findings that raise the client's stroke risk.",
   ["Finding", "Value"],
   [(["Carotid ultrasound", "80% left internal carotid narrowing"], True, "Symptomatic severe stenosis."),
    (["LDL", "168 mg/dL off statin"], True, "Uncontrolled cholesterol."),
    (["Smoking", "Current"], True, "Major modifiable risk."),
    (["Blood pressure", "164/94 mm Hg"], True, "Hypertension."),
    (["Symptoms", "Fully resolved after 20 minutes"], False, "Defines a TIA but does not lower the risk."),
    (["ALT and CK", "Normal"], False, "A safe baseline for restarting a statin.")],
   "A TIA with severe carotid stenosis and uncontrolled risk factors carries a high early stroke risk.",
   "Recognize cues after a TIA."),

matrix_mc(M(PPT, CV_MEDS, "analyze_cues", "difficult", CV, "ph"),
   "For each statement about cholesterol-lowering drugs, indicate whether it is accurate or inaccurate.",
   ["Accurate", "Inaccurate"],
   [("High-intensity statins lower stroke risk after a TIA", 0, "Recommended for secondary prevention."),
    ("Muscle aches on a statin always mean the drug must be stopped forever", 1, "Another statin or dose may work; check CK."),
    ("Dark urine with severe muscle pain on a statin needs urgent review", 0, "Possible rhabdomyolysis."),
    ("Ezetimibe can be added if the LDL target is not reached", 0, "Lowers absorption of cholesterol."),
    ("Statins are best taken with grapefruit juice", 1, "Grapefruit raises some statin levels and toxicity."),
    ("PCSK9 inhibitors are injections that lower LDL strongly", 0, "An option when statins are not tolerated.")],
   "Statins prevent stroke; muscle symptoms are assessed, not automatically a lifelong stop.",
   "Analyze antilipidemic therapy."),

cloze(M(PA, STROKE, "prioritize_hypotheses", "difficult", NEURO, "aha ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The client is at high risk of {{b1}} in the next days, and the best way to reduce it is {{b2}}.",
   [[("a completed stroke", True, "The risk is highest soon after a TIA."),
     ("a heart attack only", False, "Possible, but stroke is the main risk."),
     ("a migraine", False, "Not relevant."),
     ("seizures", False, "Not the main risk.")],
    [("carotid endarterectomy within 2 weeks plus antiplatelet and statin therapy", True,
      "Early surgery for symptomatic severe stenosis, with medical therapy."),
     ("observation for 3 months", False, "Too slow."),
     ("anticoagulation with warfarin alone", False, "Not the standard for carotid disease."),
     ("thrombolysis now", False, "Symptoms have resolved.")]],
   "After a TIA with severe carotid stenosis, early endarterectomy with antiplatelet and statin therapy.",
   "Prioritize stroke prevention after a TIA."),

sata(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph aha"),
   "Which parts of the plan should the nurse anticipate?",
   [("Aspirin and clopidogrel for a short period", True, "Dual antiplatelet therapy after a minor stroke or TIA."),
    ("Atorvastatin 80 mg, or a different statin if aches return", True, "High-intensity statin."),
    ("Smoking cessation support", True, "Major risk factor."),
    ("Carotid endarterectomy this week", True, "Severe symptomatic stenosis."),
    ("Stop all blood pressure medicine before surgery", False, "Pressure is controlled, not stopped."),
    ("Warfarin instead of antiplatelets", False, "Not standard here.")],
   "Antiplatelets, statin, smoking cessation and early endarterectomy.",
   "Generate the stroke prevention plan.",
   ex=[note("Plan", "Endarterectomy scheduled in 3 days. Restart atorvastatin 40 mg at bedtime.", id="tia-plan")]),

sata(M(RRP, POSTOP, "take_action", "difficult", CV, "ms"),
   "Four hours after the left carotid endarterectomy, review the findings. Which actions should the nurse take?",
   [("Notify the surgeon now", True, "An expanding neck hematoma threatens the airway."),
    ("Keep the head of the bed raised", True, "Reduces swelling."),
    ("Have airway equipment and a suture removal kit ready", True, "The surgeon may need to open the wound."),
    ("Check for a new neurologic deficit", True, "Stroke after surgery."),
    ("Apply a tight pressure dressing to the neck", False, "Compresses the airway."),
    ("Give a sedative for anxiety", False, "Masks neurologic and airway change.")],
   "A growing neck hematoma after endarterectomy is an airway emergency.",
   "Take action after carotid endarterectomy.",
   ex=[flow("4 hours after surgery", ["Hour 4"], [
       ("Neck", ["Swelling growing on the left"]),
       ("Voice", ["Hoarse"]),
       ("Breathing", ["Noisy, RR 24/min"]),
       ("Tongue", ["Deviates to the left"]),
       ("Blood pressure", ["178/96 mm Hg"])], id="tia-postop")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", CV, "ph"),
   "At the 6-week visit, for each statement or result, indicate whether it shows the plan is working or needs "
   "follow-up.",
   ["Working", "Needs follow-up"],
   [("LDL 64 mg/dL on atorvastatin", 0, "At target."),
    ("\"I've cut down to 5 cigarettes a day.\"", 1, "Progress, but complete cessation is the goal."),
    ("\"I take my statin at bedtime and have no aches.\"", 0, "Tolerating it."),
    ("\"I'll stop clopidogrel when the prescription runs out next month as planned.\"", 0, "Short course as prescribed."),
    ("Blood pressure 152/90 mm Hg", 1, "Above target; adjust therapy."),
    ("Tongue still deviates slightly to the left", 1, "A hypoglossal nerve injury from surgery; follow up.")],
   "Cholesterol and adherence are good; smoking, blood pressure and a nerve injury need follow-up.",
   "Evaluate stroke prevention.",
   ex=[note("6-week visit", "The neck hematoma was evacuated on the day of surgery.", id="tia-6wk")]),
]

# ================================================================================================
# Case 4: Head-to-toe assessment with the admission template, long-term care
# ================================================================================================
ltc_tabs = [
    ("ltc-notes", "Admission template", [table("Head-to-toe admission template", ["Finding"], [
        ("General", ["84-year-old, alert, oriented x3, hard of hearing; came from hospital after pneumonia"]),
        ("Vital signs", ["BP 132/74 lying, 104/62 standing with dizziness; HR 88; T 36.8 C; SpO2 95%"]),
        ("Head, eyes, ears", ["Thick glasses; hearing aids left at the hospital; cerumen in both ears"]),
        ("Mouth", ["Loose upper denture; sore on the gum; mouth dry"]),
        ("Chest", ["Fine crackles at the right base; productive cough"]),
        ("Heart", ["Regular; 1+ ankle edema"]),
        ("Abdomen", ["Soft; last bowel movement 3 days ago"]),
        ("Skin", ["Heels red, do not blanch; skin tear on the left forearm"]),
        ("Mobility", ["Walks with a walker; unsteady turning"]),
        ("Nutrition", ["Weight 52 kg (58 kg a month ago); eats half of meals"]),
        ("Mood", ["Says, \"I'm a burden; there's no point eating.\""])], id="ltc-table")]),
]

ltc_qs = [
hl_table(M(RRP, NEURO_DX, "recognize_cues", "moderate", FUND, "fund"),
   "Highlight the template findings that require follow-up.",
   ["Area", "Finding"],
   [(["Vital signs", "BP falls to 104/62 standing with dizziness"], True, "Orthostatic hypotension and fall risk."),
    (["Skin", "Heels red and do not blanch"], True, "Stage 1 pressure injury."),
    (["Nutrition", "Lost 6 kg in a month; eats half of meals"], True, "Malnutrition."),
    (["Mouth", "Loose denture and a gum sore"], True, "Painful eating."),
    (["Mood", "\"I'm a burden; there's no point eating.\""], True, "Possible depression and passive suicidal thinking."),
    (["Heart", "Regular rhythm"], False, "Normal.")],
   "Using the template systematically finds problems across every system.",
   "Recognize cues with a head-to-toe template."),

matrix_mr(M(HPM, AGING, "analyze_cues", "difficult", FUND, "fund ms"),
   "For each finding, select the problems it contributes to. Each row may have more than one answer.",
   ["Falls", "Poor nutrition", "Pressure injury"],
   [("Orthostatic hypotension", {0}, "Dizziness on standing."),
    ("Loose denture and gum sore", {1}, "Painful chewing."),
    ("Low mood", {0, 1}, "Less eating and less attention."),
    ("Weight loss", {0, 1, 2}, "Less padding, weakness."),
    ("Unsteady turning with a walker", {0}, "Gait problem."),
    ("Hearing aids missing", {0}, "Less awareness of surroundings.")],
   "Problems in older adults are linked; one finding can feed several risks.",
   "Analyze linked geriatric problems."),

cloze(M(PSI, "suicide-risk-and-safety", "prioritize_hypotheses", "difficult", FUND, "psych fund"),
   "Complete the sentence by choosing from the lists of options.",
   "The finding to act on first is {{b1}}, because {{b2}}.",
   [[("the statement \"there's no point eating\"", True, "Passive suicidal thinking needs direct assessment."),
     ("the cerumen in the ears", False, "Needs care but is not urgent."),
     ("the 1+ ankle edema", False, "Mild and stable."),
     ("the thick glasses", False, "Not urgent.")],
    [("older adults have high suicide rates and the client may be refusing food as self-harm", True,
      "Ask directly about thoughts of suicide."),
     ("it is a normal part of aging", False, "Depression is not normal aging."),
     ("the family can handle it", False, "The nurse assesses."),
     ("it will pass after a week", False, "Not a safe assumption.")]],
   "A statement suggesting hopelessness and food refusal is assessed directly for suicide risk.",
   "Prioritize safety findings on admission."),

grouping(M(MOC, "collaboration-and-referrals", "generate_solutions", "difficult", FUND, "mgmt fund"),
   "For each team member, select the referrals the nurse should make.",
   [("Provider", [
       ("Orthostatic hypotension and medication review", True, "Possible drug or volume cause."),
       ("Depression screening result and safety assessment", True, "Needs evaluation and treatment."),
       ("Routine trimming of fingernails", False, "Nursing care.")]),
    ("Dietitian and dentist", [
       ("Weight loss and denture fit", True, "Nutrition and pain-free chewing."),
       ("Heel pressure injury staging", False, "A wound or nursing task.")]),
    ("Physical therapy and audiology", [
       ("Gait and walker safety", True, "Fall prevention."),
       ("Replace missing hearing aids", True, "Communication and safety.")])],
   "The template's findings lead to the right referrals.",
   "Generate referrals from a head-to-toe assessment.",
   ex=[note("Day 1", "The provider visits tomorrow morning.", id="ltc-day1")]),

sata(M(SIC, INJURY, "take_action", "difficult", FUND, "fund"),
   "Which actions should the nurse take today?",
   [("Ask directly about suicidal thoughts", True, "Asking does not increase risk."),
    ("Float the heels off the bed", True, "Offloads a stage 1 pressure injury."),
    ("Teach slow position changes", True, "Orthostatic fall prevention."),
    ("Keep the bed low, call light in reach", True, "Fall prevention."),
    ("Massage the red heels firmly twice a day", False, "Damages tissue."),
    ("Restrict fluids because of ankle edema", False, "Worsens orthostasis and dehydration.")],
   "Safety first: suicide assessment, heel offloading and fall prevention.",
   "Take action on admission findings.",
   ex=[note("Day 1, 1600", "The resident's daughter visits.", id="ltc-1600")]),

matrix_mc(M(HPM, AGING, "evaluate_outcomes", "difficult", FUND, "fund"),
   "At 2 weeks, the nurse repeats the head-to-toe assessment. For each finding, indicate whether it shows "
   "improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Heels blanch and are no longer red", 0, "Pressure injury resolved."),
    ("New denture fitted; eating most meals", 0, "Nutrition improving."),
    ("No orthostatic drop after a medicine was stopped", 0, "Resolved."),
    ("Weight 52.5 kg", 0, "Weight stable and starting to rise."),
    ("Still says, \"What's the point?\" and stays in bed", 1, "Depression persists; follow up with the provider and mental health team."),
    ("Hearing aids replaced", 0, "Communication improved.")],
   "Physical problems are improving; low mood persists and needs ongoing care.",
   "Evaluate outcomes with a repeat head-to-toe assessment.",
   ex=[note("2-week review", "Sertraline started on day 4.", id="ltc-2wk")]),
]

cases = [
    case("ALS and goals of care", als_tabs, als_qs),
    case("Bell palsy and trigeminal neuralgia", cn_tabs, cn_qs),
    case("TIA, carotid endarterectomy and statin therapy", tia_tabs, tia_qs),
    case("Head-to-toe admission template in long-term care", ltc_tabs, ltc_qs),
]

BUILT = build(119, "neuro-cases-als-cn-tia-h2t-standalone",
              "Neurologic case studies: ALS, cranial nerves V and VII, TIA with statin therapy and a head-to-toe "
              "admission template, with four standalone items",
              items, cases, case_conversion=True)
