"""Batch 118 (case conversion, chunk 5: neurologic): four full NGN case studies (CASE-MAP.md, book_topics.py).

Cases: cranial nerve assessment after craniotomy for a brain tumor; delirium on top of dementia with
anticholinergic burden and a cholinesterase inhibitor; new-onset epilepsy and antiseizure drug safety; an epidural
hematoma with a lucid interval. Each case walks the six CJMM steps in order, unfolds with new data in steps 4 to 6,
and includes a management of care, safety, psychosocial or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC, PSI = ("PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC",
                                         "PSI")
NEURO = "neurological"
NEURO_DX = "neurological-assessment-and-procedures"
ICP = "intracranial-pressure-and-head-injury"
POSTOP = "postoperative-care"
DELIRIUM = "dementia-and-delirium"
NEURO_MEDS = "neurological-medications"
AED = "antiseizure-medications"
SEIZURE = "seizure-management"
ADVERSE = "adverse-effects-of-medications"
CALC = "medication-dose-calculations"
LABS = "laboratory-values-and-monitoring"
RESTRAINT = "restraint-safety"
TEACH = "health-teaching-and-behavior-change"
PRIORITY = "prioritizing-client-care"
COLLAB = "collaboration-and-referrals"
CONSENT = "informed-consent"

items = []

# ================================================================================================
# Case 1: Cranial nerve assessment after craniotomy
# ================================================================================================
crani_tabs = [
    ("crani-notes", "Nurses' notes", [note("Neuro ICU, postoperative day 1",
        "A 56-year-old client had a craniotomy yesterday to remove a tumor at the base of the skull near the brainstem. "
        "Cranial nerve assessment this morning:\n"
        "CN II: reads the wall clock with each eye.\n"
        "CN III, IV, VI: eyes move fully; pupils 3 mm, equal, reactive.\n"
        "CN V: feels light touch on both sides of the face.\n"
        "CN VII: cannot close the right eye fully; right side of the mouth droops; forehead does not wrinkle on the right.\n"
        "CN VIII: hears a whisper in both ears.\n"
        "CN IX, X: weak gag; voice hoarse and wet; coughs after a teaspoon of water.\n"
        "CN XI: shrugs both shoulders strongly.\n"
        "CN XII: tongue deviates to the right when protruded.", id="crani-note")]),
]

crani_qs = [
hl_text(M(RRP, NEURO_DX, "recognize_cues", "moderate", NEURO, "ms fund"),
   "Highlight the abnormal cranial nerve findings.",
   ["CN II: ",
    ("reads the clock with each eye", False, "Normal vision."),
    ". CN III, IV, VI: ",
    ("full eye movement, equal reactive pupils", False, "Normal."),
    ". CN VII: ",
    ("cannot close the right eye; right mouth droop; right forehead does not wrinkle", True,
     "A complete right facial palsy, including the forehead, is a peripheral CN VII injury."),
    ". CN IX, X: ",
    ("weak gag, wet hoarse voice, coughs after water", True, "Bulbar weakness and aspiration risk."),
    ". CN XI: ",
    ("strong shoulder shrug", False, "Normal."),
    ". CN XII: ",
    ("tongue deviates to the right", True, "Weakness of the right hypoglossal nerve; the tongue points to the weak side."),
    "."],
   "CN VII, IX, X and XII are affected; these lie close to the brainstem where the tumor was.",
   "Recognize abnormal cranial nerve findings."),

matrix_mc(M(RRP, NEURO_DX, "analyze_cues", "difficult", NEURO, "ms fund"),
   "For each cranial nerve, indicate the test used to assess it.",
   ["Ask the client to smile, close the eyes tightly and raise the eyebrows", "Check the gag reflex and listen to the voice and swallow", "Ask the client to stick out the tongue", "Ask the client to shrug the shoulders against resistance"],
   [("CN VII (facial)", 0, "Facial expression muscles."),
    ("CN IX and X (glossopharyngeal and vagus)", 1, "Gag, swallowing and voice."),
    ("CN XII (hypoglossal)", 2, "Tongue movement."),
    ("CN XI (accessory)", 3, "Trapezius and sternocleidomastoid.")],
   "Each lower cranial nerve has a simple bedside test.",
   "Analyze cranial nerve testing."),

cloze(M(RRP, NEURO_DX, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The most immediate risk from these findings is {{b1}}, caused by {{b2}}; the incomplete eye closure risks {{b3}}.",
   [[("aspiration pneumonia", True, "Weak gag and cough after water."),
     ("hearing loss", False, "Hearing is intact."),
     ("vision loss", False, "Vision is intact."),
     ("shoulder weakness", False, "CN XI is intact.")],
    [("CN IX and X weakness", True, "Swallowing and airway protection."),
     ("CN VII weakness", False, "Affects the face and eye, not airway protection."),
     ("CN XII weakness", False, "Affects tongue movement, contributing to but not causing aspiration alone."),
     ("CN II injury", False, "Not affected.")],
    [("corneal abrasion and ulcer", True, "An eye that does not close dries out."),
     ("glaucoma", False, "Not related."),
     ("cataract", False, "Not related."),
     ("double vision", False, "Not related.")]],
   "Lower cranial nerve injury threatens the airway; facial nerve palsy threatens the cornea.",
   "Prioritize risks from cranial nerve deficits."),

sata(M(RRP, POSTOP, "generate_solutions", "difficult", NEURO, "ms"),
   "Which interventions should the nurse anticipate?",
   [("Keep the client NPO until a speech therapist assesses swallowing", True, "Aspiration risk."),
    ("Artificial tears and taping the right eye shut at night", True, "Protects the cornea."),
    ("Head of the bed at 30 degrees or more", True, "Reduces aspiration."),
    ("Suction at the bedside", True, "Clears pooled secretions."),
    ("Give oral tablets with a full glass of water", False, "Unsafe before the swallow assessment."),
    ("A patch over the left (normal) eye", False, "The affected right eye needs protection.")],
   "Protect the airway and the right cornea until function returns.",
   "Generate solutions for cranial nerve deficits.",
   ex=[note("Plan", "Speech therapy today. Ophthalmic lubricant. Dexamethasone for swelling. Neuro checks every 2 hours.",
       id="crani-plan")]),

sata(M(PA, ICP, "take_action", "difficult", NEURO, "ms"),
   "At 1400, review the findings. Which actions should the nurse take?",
   [("Report the change to the neurosurgeon now", True, "New CN III signs and a falling GCS suggest swelling or bleeding."),
    ("Prepare for an urgent CT", True, "Confirms the cause."),
    ("Keep the head midline with the head of the bed at 30 degrees", True, "Supports venous drainage."),
    ("Check the glucose", True, "Dexamethasone raises glucose, and hypoglycemia can mimic deterioration."),
    ("Give a sedative for restlessness", False, "Masks neurologic change."),
    ("Continue hourly checks without reporting", False, "This is a significant change.")],
   "A new dilated pupil with ptosis and a falling GCS after brain surgery needs urgent escalation.",
   "Take action for deterioration after craniotomy.",
   ex=[flow("1400", ["0800", "1400"], [
       ("GCS", ["15", "13"]),
       ("Right pupil", ["3 mm, reactive", "5 mm, sluggish"]),
       ("Right eyelid", ["Open", "Drooping"]),
       ("Blood pressure", ["132/78", "168/72 mm Hg"]),
       ("Heart rate", ["78", "58/min"])], id="crani-1400")]),

matrix_mc(M(MOC, COLLAB, "evaluate_outcomes", "difficult", NEURO, "mgmt ms"),
   "Two weeks later, the team plans discharge. For each finding, indicate whether it shows readiness or needs "
   "follow-up before discharge.",
   ["Ready", "Needs follow-up"],
   [("Passes a modified swallow study on soft foods and thickened liquids", 0, "Safe diet plan in place."),
    ("Right eye still does not close fully", 1, "Needs ongoing eye protection teaching and an eye review."),
    ("GCS 15, pupils equal", 0, "Neurologically stable."),
    ("Speech therapy and radiation oncology appointments booked", 0, "Continuity of care."),
    ("Glucose 240 to 300 mg/dL on dexamethasone, no plan", 1, "Needs a glucose plan or steroid taper."),
    ("Family taught signs of rising ICP", 0, "Prepared to act.")],
   "The swallow and follow-up are set; eye protection and steroid hyperglycemia need plans.",
   "Evaluate readiness for discharge after craniotomy.",
   ex=[note("Day 14", "A small postoperative bleed was managed without further surgery.", id="crani-day14")]),
]

# ================================================================================================
# Case 2: Delirium on top of dementia
# ================================================================================================
del_tabs = [
    ("del-notes", "Nurses' notes", [note("Medical unit, night shift",
        "An 86-year-old client with moderate Alzheimer disease was admitted 2 days ago with a urinary infection. At "
        "home the client takes donepezil. In hospital, oxybutynin and diphenhydramine at night were added. Tonight "
        "the client is suddenly much more confused, picks at the bed sheets, sees insects on the wall, and dozes off "
        "mid-sentence, then becomes agitated. The family says the client usually recognizes them and chats.",
        id="del-note")]),
    ("del-data", "Findings", [flow("Findings", ["2300"], [
        ("Temperature", ["37.9 C (100.2 F)"]),
        ("Heart rate", ["104/min"]),
        ("Bladder scan", ["620 mL"]),
        ("Last bowel movement", ["4 days ago"]),
        ("Pupils", ["5 mm, reactive"]),
        ("Confusion Assessment Method", ["Positive"])], id="del-flow")]),
]

del_qs = [
hl_table(M(PSI, DELIRIUM, "recognize_cues", "moderate", NEURO, "psych ms"),
   "Highlight the findings that suggest delirium or its causes.",
   ["Finding", "Value"],
   [(["Onset", "Sudden change tonight"], True, "Delirium starts suddenly; dementia progresses slowly."),
    (["Attention", "Dozes off mid-sentence, then agitated"], True, "Fluctuating attention and arousal."),
    (["Perception", "Sees insects on the wall"], True, "Visual hallucinations are common in delirium."),
    (["New medicines", "Oxybutynin and diphenhydramine"], True, "Anticholinergic drugs cause delirium in older adults."),
    (["Bladder scan", "620 mL"], True, "Urinary retention, worsened by anticholinergics."),
    (["Donepezil", "Usual home medicine"], False, "Treats dementia; the new drugs oppose it.")],
   "Sudden fluctuating confusion with hallucinations, retention and new anticholinergic drugs is delirium.",
   "Recognize cues of delirium."),

matrix_mc(M(PSI, DELIRIUM, "analyze_cues", "difficult", NEURO, "psych"),
   "For each feature, indicate whether it is typical of delirium or dementia.",
   ["Delirium", "Dementia"],
   [("Develops over hours to days", 0, "Acute onset."),
    ("Attention fluctuates through the day", 0, "Fluctuation is a hallmark."),
    ("Slow decline in memory over years", 1, "Progressive."),
    ("Often reversible when the cause is treated", 0, "Find and treat the cause."),
    ("Level of consciousness is usually normal", 1, "Alert until late stages.")],
   "Delirium is acute, fluctuating and often reversible; dementia is gradual.",
   "Analyze delirium versus dementia."),

matrix_mr(M(PPT, ADVERSE, "prioritize_hypotheses", "difficult", NEURO, "ph"),
   "For each contributing cause, indicate the problems it can explain. Each row may have more than one answer.",
   ["Confusion", "Urinary retention", "Constipation"],
   [("Oxybutynin", {0, 1, 2}, "Strongly anticholinergic."),
    ("Diphenhydramine", {0, 1, 2}, "Anticholinergic antihistamine."),
    ("Urinary infection", {0}, "Infection triggers delirium."),
    ("Immobility in hospital", {0, 2}, "Reduces orientation and bowel movement.")],
   "Anticholinergic drugs explain confusion, retention and constipation together; they also oppose donepezil.",
   "Prioritize the causes of delirium."),

grouping(M(PSI, DELIRIUM, "generate_solutions", "difficult", NEURO, "psych ms"),
   "For each area, select the interventions the nurse should plan.",
   [("Medicines", [
       ("Ask the provider to stop oxybutynin and diphenhydramine", True, "Remove anticholinergic burden."),
       ("Continue donepezil", True, "A cholinesterase inhibitor; stopping can worsen cognition."),
       ("Give haloperidol routinely at night", False, "Antipsychotics are a last resort for danger, not routine.")]),
    ("Body", [
       ("Drain the bladder as prescribed", True, "Retention worsens delirium."),
       ("Start a bowel regimen", True, "Constipation worsens delirium."),
       ("Keep the client in bed all day", False, "Mobility helps.")]),
    ("Environment", [
       ("Glasses, hearing aids, a clock and family visits", True, "Reorientation."),
       ("Lights on and noise at night to keep the client awake", False, "Protect sleep at night.")])],
   "Remove causes, treat retention and constipation, and support orientation and sleep.",
   "Generate a delirium plan.",
   ex=[note("2330", "The provider asks for the nurse's recommendations.", id="del-2330")]),

sata(M(SIC, RESTRAINT, "take_action", "difficult", NEURO, "fund psych"),
   "At 0100 the client tries to climb out of bed and pulls at the IV. Which actions should the nurse take?",
   [("Stay and speak calmly, using the name", True, "Least restrictive first."),
    ("Ask a family member to sit in", True, "A familiar presence calms."),
    ("Lower the bed and cover the IV site", True, "Safety without restraint."),
    ("Check for pain or a need to toilet", True, "Unmet needs drive agitation."),
    ("Apply soft wrist restraints right away", False, "Restraints increase delirium and injury; a last resort with an order."),
    ("Give diphenhydramine to help the client sleep", False, "Anticholinergic; worsens delirium.")],
   "Address needs and use the least restrictive measures; avoid restraints and anticholinergic sedatives.",
   "Take action for agitation in delirium.",
   ex=[note("0100", "The bladder was drained of 600 mL at midnight.", id="del-0100")]),

matrix_mc(M(PSI, DELIRIUM, "evaluate_outcomes", "difficult", NEURO, "psych"),
   "Review day 3. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Recognizes family and chats", 0, "Back to baseline."),
    ("Confusion Assessment Method negative", 0, "Delirium resolved."),
    ("Bowel movement and voiding without retention", 0, "Contributors corrected."),
    ("Diphenhydramine still on the discharge list", 1, "Should be removed."),
    ("Family asks how to prevent this next time", 1, "Teach early signs and medicines to avoid."),
    ("Afebrile on antibiotics", 0, "Infection treated.")],
   "Delirium has resolved; the discharge medication list and family teaching need attention.",
   "Evaluate delirium outcomes.",
   ex=[note("Day 3", "Oxybutynin and diphenhydramine stopped on day 2.", id="del-day3")]),
]

# ================================================================================================
# Case 3: New-onset epilepsy and antiseizure drug safety
# ================================================================================================
sz_tabs = [
    ("sz-notes", "Nurses' notes", [note("Neurology clinic and ward",
        "A 27-year-old client had two generalized tonic-clonic seizures this month. The client works as a delivery "
        "driver, uses an oral contraceptive and is of Han Chinese ancestry. The neurologist is choosing between "
        "carbamazepine, valproate and levetiracetam. The client was started on phenytoin in the ED last week.",
        id="sz-note")]),
    ("sz-labs", "Labs", [table("Labs", ["Result", "Reference"], [
        ("Total phenytoin level", ["8 mcg/mL", "10 to 20 mcg/mL"]),
        ("Albumin", ["2.0 g/dL", "3.5 to 5.0 g/dL"]),
        ("HLA-B*1502", ["Positive", "-"]),
        ("ALT", ["24 units/L", "7 to 56"]),
        ("Pregnancy test", ["Negative", "-"])], id="sz-table")]),
]

sz_qs = [
hl_table(M(PPT, AED, "recognize_cues", "moderate", NEURO, "ph"),
   "Highlight the findings that affect the choice and monitoring of antiseizure drugs.",
   ["Finding", "Value"],
   [(["HLA-B*1502", "Positive"], True, "High risk of Stevens-Johnson syndrome with carbamazepine."),
    (["Albumin", "2.0 g/dL"], True, "Low albumin makes the total phenytoin level misleading."),
    (["Contraception", "Oral contraceptive"], True, "Enzyme-inducing drugs make it fail; valproate harms a fetus."),
    (["Job", "Delivery driver"], True, "Driving must stop until seizure-free per local law."),
    (["ALT", "24 units/L"], False, "Normal baseline."),
    (["Pregnancy test", "Negative"], False, "Reassuring; still relevant for drug choice.")],
   "Genetics, albumin, contraception and driving all change the plan.",
   "Recognize cues for antiseizure drug choice."),

calc(M(PPT, CALC, "analyze_cues", "difficult", NEURO, "ph lab"),
   "With low albumin, the corrected phenytoin level = measured level / ((0.2 x albumin) + 0.1). What is the client's "
   "corrected phenytoin level? Record your answer as a whole number.",
   "mcg/mL", 0, "Corrected level", 16,
   "8 / ((0.2 x 2.0) + 0.1) = 8 / 0.5 = 16 mcg/mL, which is within range. Raising the dose for the 'low' level would "
   "risk toxicity, because more drug is unbound when albumin is low.",
   "Calculate and interpret a corrected phenytoin level."),

matrix_mc(M(PPT, AED, "prioritize_hypotheses", "difficult", NEURO, "ph"),
   "For each drug, indicate its main concern for this client.",
   ["Serious risk for this client", "Acceptable with monitoring"],
   [("Carbamazepine", 0, "HLA-B*1502 positive: high risk of Stevens-Johnson syndrome; also lowers contraceptive effect."),
    ("Valproate", 0, "Highest risk of birth defects in someone who could become pregnant."),
    ("Levetiracetam", 1, "Few interactions; watch mood and irritability."),
    ("Phenytoin", 1, "Usable with corrected levels, but it induces enzymes and has long-term side effects.")],
   "Genetic and pregnancy risks rule out carbamazepine and valproate for this client.",
   "Prioritize antiseizure drug risks."),

sata(M(HPM, TEACH, "generate_solutions", "difficult", NEURO, "ph ms"),
   "Levetiracetam is chosen. Which teaching should the plan include?",
   [("Take the medicine every day at the same times, even when well", True, "Missed doses cause breakthrough seizures."),
    ("Report new depression, anger or thoughts of self-harm", True, "Behavioral effects of levetiracetam."),
    ("Do not drive until seizure-free for the period the law requires", True, "Safety for the client and others."),
    ("Avoid alcohol and sleep deprivation", True, "Common seizure triggers."),
    ("Stop the medicine at once if a rash appears and seizures are rare", False, "Stopping suddenly can trigger status epilepticus; call first."),
    ("Take a bath alone to relax", False, "Showers or supervision reduce drowning risk.")],
   "Teaching covers adherence, mood changes, driving, triggers and water safety.",
   "Generate antiseizure medication teaching.",
   ex=[note("Plan", "Levetiracetam 500 mg twice daily. Taper phenytoin. Seizure diary.", id="sz-plan")]),

order(M(PA, SEIZURE, "take_action", "difficult", NEURO, "ms"),
   "While the phenytoin is being tapered on the ward, the client has a generalized seizure in bed. Place the "
   "nurse's actions in order.",
   [("Note the start time and stay with the client", "Timing guides treatment."),
    ("Lower the bed, pad the rails and move objects away", "Prevents injury."),
    ("Turn the client onto the side", "Lets secretions drain and keeps the airway open."),
    ("Give IV lorazepam as prescribed if the seizure lasts 5 minutes", "Status epilepticus treatment."),
    ("After it ends, check airway, breathing, neurologic status and injuries", "Post-ictal assessment.")],
   "Time it, protect, side-lying, benzodiazepine at 5 minutes, then post-ictal assessment.",
   "Take action during a seizure.",
   ex=[note("Ward, day 2", "Seizure started at 1405. IV access in place. Lorazepam is prescribed for seizures over 5 minutes.",
       id="sz-ward")]),

matrix_mc(M(PPT, AED, "evaluate_outcomes", "difficult", NEURO, "ph"),
   "At 3 months, for each finding, indicate whether the plan is working or needs follow-up.",
   ["Working", "Needs follow-up"],
   [("No seizures for 8 weeks", 0, "Controlled."),
    ("Takes doses on time using phone alarms", 0, "Good adherence."),
    ("Partner reports the client is irritable and tearful most days", 1, "Behavioral side effect; assess mood and safety."),
    ("Has not driven since the seizures", 0, "Safe."),
    ("Gums are normal after stopping phenytoin", 0, "No gingival overgrowth."),
    ("Plans a pregnancy next year but has not told the neurologist", 1, "Preconception planning and folic acid are needed.")],
   "Seizures are controlled; mood changes and pregnancy planning need follow-up.",
   "Evaluate antiseizure treatment.",
   ex=[note("3-month visit", "Phenytoin stopped. Levetiracetam 750 mg twice daily.", id="sz-3m")]),
]

# ================================================================================================
# Case 4: Epidural hematoma with a lucid interval
# ================================================================================================
edh_tabs = [
    ("edh-notes", "Nurses' notes", [note("Emergency department",
        "A 22-year-old client was hit on the left side of the head by a baseball and was briefly knocked out. The "
        "client woke up, said they felt fine, and walked into the ED with a headache. While waiting, the client "
        "became drowsy and vomited.", id="edh-note")]),
    ("edh-flow", "Neuro checks", [flow("Neurologic checks", ["1500", "1530", "1600"], [
        ("GCS", ["15", "13", "9"]),
        ("Left pupil", ["3 mm reactive", "4 mm reactive", "6 mm fixed"]),
        ("Right pupil", ["3 mm reactive", "3 mm reactive", "3 mm reactive"]),
        ("Blood pressure", ["128/76", "142/74", "182/64 mm Hg"]),
        ("Heart rate", ["84", "70", "52/min"]),
        ("Breathing", ["Regular", "Regular", "Irregular"])], id="edh-flowsheet")]),
]

edh_qs = [
hl_table(M(PA, ICP, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings at 1600 that require immediate action.",
   ["Finding", "1600"],
   [(["GCS", "9"], True, "Rapid decline."),
    (["Left pupil", "6 mm, fixed"], True, "Uncal herniation on the side of the bleed."),
    (["Blood pressure / heart rate", "182/64; 52/min"], True, "Widening pulse pressure with bradycardia."),
    (["Breathing", "Irregular"], True, "Cushing triad."),
    (["Right pupil", "3 mm reactive"], False, "Normal on the opposite side.")],
   "A falling GCS, a fixed ipsilateral pupil and Cushing triad mean impending herniation.",
   "Recognize cues of herniation."),

cloze(M(PA, ICP, "analyze_cues", "difficult", NEURO, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The brief period of feeling fine is called {{b1}}, typical of {{b2}}. Hypertension, bradycardia and irregular "
   "breathing together are {{b3}}.",
   [[("a lucid interval", True, "Consciousness returns before the bleed expands."),
     ("a postictal state", False, "Follows a seizure."),
     ("syncope", False, "A faint."),
     ("a concussion", False, "Not the term.")],
    [("an epidural hematoma from a torn middle meningeal artery", True, "Arterial bleeding expands fast."),
     ("a chronic subdural hematoma", False, "Develops over weeks."),
     ("a migraine", False, "Not traumatic."),
     ("a subarachnoid hemorrhage from an aneurysm", False, "Not this history.")],
    [("Cushing triad", True, "A late sign of rising ICP."),
     ("Beck triad", False, "Cardiac tamponade."),
     ("Charcot triad", False, "Cholangitis."),
     ("Virchow triad", False, "Clotting risk.")]],
   "A lucid interval then rapid decline is an epidural hematoma; Cushing triad shows herniation.",
   "Analyze an epidural hematoma."),

matrix_mc(M(PA, ICP, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "For each problem, indicate its priority.",
   ["Immediate", "After the emergency"],
   [("Brain herniation", 0, "Death within minutes to hours without surgery."),
    ("Airway protection as GCS falls", 0, "GCS 8 or less needs intubation."),
    ("Scalp laceration", 1, "Minor."),
    ("Informing the family", 1, "Important, done by the team as soon as possible.")],
   "Herniation and the airway come first.",
   "Prioritize in an epidural hematoma."),

sata(M(PA, ICP, "generate_solutions", "difficult", NEURO, "ms"),
   "Which interventions should the nurse anticipate?",
   [("Emergency craniotomy to evacuate the hematoma", True, "Definitive."),
    ("Intubation with brief hyperventilation as a bridge only", True, "Temporarily lowers ICP."),
    ("Hypertonic saline or mannitol", True, "Reduces ICP."),
    ("Head of the bed at 30 degrees with the head midline", True, "Venous drainage."),
    ("Lumbar puncture to measure pressure", False, "Can cause herniation."),
    ("Wait for the next neuro check in 30 minutes", False, "Delays surgery.")],
   "Emergency surgery with temporary ICP-lowering measures; never a lumbar puncture.",
   "Generate solutions for herniation from an epidural hematoma.",
   ex=[note("1605", "CT: left temporal epidural hematoma with midline shift. OR notified.", id="edh-ct")]),

sata(M(MOC, CONSENT, "take_action", "difficult", NEURO, "mgmt"),
   "The client cannot consent, and the parents are 2 hours away. Which actions are appropriate?",
   [("Proceed under the emergency exception", True, "Delay would cause death or serious harm."),
    ("The surgeon documents the urgency", True, "Records the emergency exception."),
    ("Contact the parents promptly", True, "Keeps the family informed."),
    ("Wait for the parents to arrive and sign the consent form", False, "Delay could be fatal."),
    ("Ask the friend in the waiting room to sign the consent", False, "A friend is not the legal decision-maker.")],
   "In a life-threatening emergency, treatment proceeds under implied consent, with documentation and family contact.",
   "Take action for consent in an emergency.",
   ex=[note("1610", "A friend who came with the client is in the waiting room.", id="edh-1610")]),

matrix_mc(M(PA, ICP, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "After surgery, review the next morning. For each finding, indicate whether it shows improvement or needs "
   "follow-up.",
   ["Improvement", "Needs follow-up"],
   [("GCS 14, pupils equal and reactive", 0, "Recovering."),
    ("Blood pressure 124/76, heart rate 80", 0, "Cushing triad resolved."),
    ("Clear fluid from the nose that tests positive for glucose", 1, "CSF leak; meningitis risk."),
    ("Urine output 400 mL/hour, sodium 152", 1, "Possible diabetes insipidus after head injury."),
    ("Headache 3 of 10", 0, "Expected.")],
   "Neurologic recovery is clear; a CSF leak and possible DI need action.",
   "Evaluate outcomes after epidural hematoma evacuation.",
   ex=[note("Postoperative day 1", "Extubated at 0700.", id="edh-pod1")]),
]

cases = [
    case("Cranial nerve assessment after craniotomy", crani_tabs, crani_qs),
    case("Delirium on top of dementia", del_tabs, del_qs),
    case("New-onset epilepsy and antiseizure drug safety", sz_tabs, sz_qs),
    case("Epidural hematoma with a lucid interval", edh_tabs, edh_qs),
]

BUILT = build(118, "neuro-cases-crani-delirium-seizure-edh",
              "Neurologic case studies: cranial nerves after craniotomy, delirium, new epilepsy and epidural hematoma",
              items, cases, case_conversion=True)
