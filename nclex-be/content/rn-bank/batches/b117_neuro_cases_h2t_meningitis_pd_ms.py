"""Batch 117 (case conversion, chunk 5: neurologic): four full NGN case studies (CASE-MAP.md, book_topics.py).

Cases: a head-to-toe assessment on admission that uncovers a chronic subdural hematoma; bacterial meningitis;
Parkinson disease with medication timing, falls and a dangerous drug; a multiple sclerosis relapse. Each case walks
the six CJMM steps in order, unfolds with new data in steps 4 to 6, uses a wide mix of item types, and includes a
management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
NEURO, FUND = "neurological", "fundamentals"
NEURO_DX = "neurological-assessment-and-procedures"
ICP = "intracranial-pressure-and-head-injury"
MENING = "meningitis-and-encephalitis"
DEGEN = "degenerative-neurological-disorders"
NEURO_MEDS = "neurological-medications"
TBP = "transmission-based-precautions"
INJURY = "injury-prevention"
MEDSAFE = "medication-safety-systems"
TEACH = "health-teaching-and-behavior-change"
HANDOFF = "handoff-and-sbar-communication"
ABX = "antimicrobial-therapy"
ANTICOAG = "anticoagulant-therapy-and-monitoring"
SKIN = "pressure-injury-prevention"
BLADDER = "urinary-elimination-and-catheter-care"

items = []

# ================================================================================================
# Case 1: Head-to-toe assessment on admission uncovers a chronic subdural hematoma
# ================================================================================================
h2t_tabs = [
    ("h2t-notes", "Head-to-toe assessment", [note("Medical unit, admission 1500",
        "An 81-year-old client is admitted from home for \"weakness and not eating.\" The client takes apixaban for "
        "atrial fibrillation and fell in the garden 3 weeks ago, hitting the head; the family says the client "
        "\"seemed fine.\" Head-to-toe assessment by the admitting nurse:\n"
        "General: drowsy but rousable, slow to answer, oriented to person and place only (family says sharp until a week ago).\n"
        "Head and eyes: healed scalp bruise on the right; pupils 3 mm, equal and reactive; mild left facial droop when smiling.\n"
        "Neck: supple.\n"
        "Chest and lungs: clear; respirations 16/min.\n"
        "Heart: irregularly irregular, 82/min; BP 152/88 mm Hg.\n"
        "Abdomen: soft, bowel sounds present; last bowel movement 4 days ago.\n"
        "Arms and legs: left hand grip weaker than right; left arm drifts down when held out with eyes closed.\n"
        "Skin: stage 2 pressure injury on the sacrum, 2 x 3 cm; heels intact.\n"
        "Elimination: incontinent of urine since yesterday.", id="h2t-note")]),
]

h2t_qs = [
hl_text(M(RRP, NEURO_DX, "recognize_cues", "moderate", FUND, "fund ms"),
   "Highlight the findings from the head-to-toe assessment that need follow-up.",
   ["The client is ",
    ("drowsy and oriented to person and place only, a change from a week ago", True, "A new change in level of consciousness."),
    ". There is ",
    ("a mild left facial droop", True, "A new focal sign."),
    " and ",
    ("a weaker left grip with drift of the left arm", True, "Pronator drift shows subtle motor weakness."),
    ". The pupils are ",
    ("3 mm, equal and reactive", False, "Normal; a baseline to compare against."),
    ". The lungs are ",
    ("clear", False, "Normal."),
    ". There is ",
    ("a stage 2 pressure injury on the sacrum", True, "A skin finding that needs a care plan."),
    ", and the client ",
    ("has been incontinent of urine since yesterday", True, "New incontinence can reflect the neurologic change."),
    "."],
   "A systematic head-to-toe assessment finds the subtle neurologic signs and the skin and elimination problems.",
   "Recognize cues on a head-to-toe assessment."),

dtable(M(PA, ICP, "analyze_cues", "difficult", NEURO, "ms"),
   "For each finding, choose the most likely explanation.",
   ["Finding", "Explanation"],
   [(["Confusion and drowsiness developing over a week, 3 weeks after a head injury"], [
       ("A slowly expanding chronic subdural hematoma", True, "Older adults on anticoagulants bleed slowly from torn bridging veins."),
       ("Normal aging", False, "The change is new and recent."),
       ("Dehydration only", False, "Does not explain focal signs."),
       ("A urinary infection alone", False, "Possible contributor, but focal signs point to the brain.")]),
    (["Left face and arm weakness"], [
       ("A lesion on the right side of the brain", True, "Motor pathways cross, matching the right-sided head injury."),
       ("A left-sided lesion", False, "Would weaken the right side."),
       ("A spinal cord injury", False, "Would not cause facial droop."),
       ("Peripheral neuropathy", False, "Not focal in this way.")]),
    (["Apixaban use"], [
       ("Raises the risk of a slowly growing bleed after minor trauma", True, "Anticoagulation impairs clotting."),
       ("Protects against stroke and makes a bleed unlikely", False, "It prevents clots but raises bleeding risk."),
       ("Explains the pressure injury", False, "Not related."),
       ("Not relevant", False, "Highly relevant.")])],
   "Subacute confusion with focal signs weeks after a head injury on an anticoagulant suggests a chronic subdural.",
   "Analyze head-to-toe findings."),

cloze(M(PA, ICP, "prioritize_hypotheses", "difficult", NEURO, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The priority hypothesis is {{b1}}, and the most urgent action is to {{b2}}.",
   [[("a chronic subdural hematoma with rising intracranial pressure", True, "Fits the history and focal signs."),
     ("delirium from constipation", False, "Constipation contributes to delirium but not focal weakness."),
     ("dementia", False, "The change is acute."),
     ("a sacral pressure injury", False, "Needs care, but is not the threat to life.")],
    [("report to the provider for urgent CT and hold the apixaban", True, "Imaging and anticoagulant planning."),
     ("start a bowel regimen first", False, "Not the priority."),
     ("reposition every 2 hours and reassess tomorrow", False, "Delays diagnosis."),
     ("give a sleeping pill to settle the client", False, "Sedation masks deterioration.")]],
   "Focal signs after a fall on an anticoagulant need urgent imaging.",
   "Prioritize findings from a head-to-toe assessment."),

matrix_mr(M(MOC, HANDOFF, "generate_solutions", "difficult", NEURO, "mgmt fund"),
   "For each finding, select the team members the nurse should involve. Each row may have more than one answer.",
   ["Provider (now)", "Wound care nurse", "Physical therapy", "Pharmacist"],
   [("New focal weakness and confusion", {0}, "Needs urgent medical evaluation and imaging."),
    ("Stage 2 sacral pressure injury", {1}, "Wound care plan."),
    ("Recent fall and weakness", {0, 2}, "Neurologic evaluation, then mobility and fall planning."),
    ("Apixaban with a possible brain bleed", {0, 3}, "Hold and plan reversal with the pharmacist."),
    ("No bowel movement for 4 days", {0}, "A bowel regimen order is needed.")],
   "A good head-to-toe assessment leads to the right referrals for each problem.",
   "Generate a collaborative plan from assessment findings.",
   ex=[note("1520", "The provider is on the unit.", id="h2t-1520")]),

sata(M(PA, ICP, "take_action", "difficult", NEURO, "ms ph"),
   "CT shows a right chronic subdural hematoma with 6 mm midline shift. Which actions should the nurse take?",
   [("Hold apixaban and prepare the reversal agent as prescribed", True, "Andexanet or PCC before surgery."),
    ("Neurologic checks, including pupils, every hour", True, "Detects deterioration."),
    ("Keep the head of the bed at 30 degrees", True, "Supports venous drainage."),
    ("Seizure precautions", True, "Subdural hematomas can cause seizures."),
    ("Insert a urinary catheter only if prescribed, to avoid infection", True, "Catheters raise infection risk."),
    ("Give the evening apixaban dose because AF raises stroke risk", False, "Contraindicated with an intracranial bleed.")],
   "Reverse anticoagulation, monitor neurologic status closely and prevent complications before surgery.",
   "Take action for a chronic subdural hematoma.",
   ex=[note("CT, 1640", "Right chronic subdural hematoma, 6 mm midline shift. Neurosurgery plans burr hole drainage tomorrow.",
       id="h2t-ct")]),

matrix_mc(M(BCC, SKIN, "evaluate_outcomes", "difficult", NEURO, "ms fund"),
   "Two days after burr hole drainage, the nurse repeats the head-to-toe assessment. For each finding, indicate "
   "whether it shows improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Alert and oriented x3", 0, "Mental status recovered."),
    ("Left grip equal to the right; no arm drift", 0, "Focal weakness resolved."),
    ("Sacral wound now 2 x 4 cm with a dark center", 1, "Wound is worsening; review repositioning and the support surface."),
    ("Continent with prompted toileting", 0, "Elimination recovered."),
    ("New headache, vomiting and drowsiness this afternoon", 1, "Possible re-accumulation; urgent review."),
    ("Bowel movement yesterday", 0, "Constipation resolved.")],
   "Neurologic recovery is clear, but the wound is worsening and new symptoms suggest recurrence.",
   "Evaluate outcomes with a repeat head-to-toe assessment.",
   ex=[note("Postoperative day 2", "Head-to-toe reassessment.", id="h2t-pod2")]),
]

# ================================================================================================
# Case 2: Bacterial meningitis
# ================================================================================================
men_tabs = [
    ("men-notes", "Nurses' notes", [note("Emergency department, 0200",
        "A 19-year-old college student who lives in a dormitory has had a severe headache, fever and vomiting since "
        "last evening. The client keeps the eyes closed in the light, and bending the neck forward causes pain. A "
        "purple, non-blanching rash is appearing on the legs.", id="men-note")]),
    ("men-data", "Findings", [flow("Findings", ["0200"], [
        ("Temperature", ["39.8 C (103.6 F)"]),
        ("Heart rate", ["124/min"]),
        ("Blood pressure", ["96/58 mm Hg"]),
        ("GCS", ["14"]),
        ("Kernig and Brudzinski signs", ["Positive"])], id="men-flow")]),
]

men_qs = [
hl_table(M(PA, MENING, "recognize_cues", "moderate", NEURO, "ms cdc"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Neck", "Pain when bending forward; positive Kernig and Brudzinski"], True, "Meningeal irritation."),
    (["Rash", "Purple, non-blanching, on the legs"], True, "Petechial rash suggests meningococcal sepsis."),
    (["Blood pressure / heart rate", "96/58; 124/min"], True, "Early septic shock."),
    (["Temperature", "39.8 C"], True, "High fever."),
    (["Light", "Keeps eyes closed"], False, "Photophobia supports the diagnosis but is not itself an action point."),
    (["GCS", "14"], False, "Mildly reduced; a baseline to watch.")],
   "Headache, fever, neck stiffness and a non-blanching rash mean likely meningococcal meningitis with sepsis.",
   "Recognize cues of bacterial meningitis."),

matrix_mc(M(PA, MENING, "analyze_cues", "difficult", NEURO, "ms"),
   "For each expected spinal fluid result, indicate whether it fits bacterial or viral meningitis.",
   ["Bacterial", "Viral"],
   [("Cloudy fluid", 0, "Many white cells and organisms."),
    ("Very high neutrophils", 0, "Neutrophils predominate in bacterial infection."),
    ("Low glucose", 0, "Bacteria consume glucose."),
    ("Mostly lymphocytes with normal glucose", 1, "Typical of viral meningitis."),
    ("High protein with a raised opening pressure", 0, "Bacterial inflammation.")],
   "Bacterial meningitis: cloudy fluid, neutrophils, low glucose, high protein and pressure.",
   "Analyze spinal fluid findings."),

cloze(M(SIC, TBP, "prioritize_hypotheses", "difficult", NEURO, "cdc ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The client should be placed on {{b1}} precautions until {{b2}}. The roommates should be offered {{b3}}.",
   [[("droplet", True, "Neisseria meningitidis spreads by respiratory droplets."),
     ("airborne", False, "Not needed for meningococcus."),
     ("contact only", False, "Not enough."),
     ("standard only", False, "Not enough.")],
    [("24 hours of effective antibiotics", True, "Then the client is no longer infectious."),
     ("the fever is gone", False, "Not the rule."),
     ("discharge", False, "Too long."),
     ("the rash fades", False, "Not the rule.")],
    [("antibiotic prophylaxis, such as rifampin or ciprofloxacin", True, "Close contacts need prophylaxis."),
     ("nothing unless they get sick", False, "Prophylaxis prevents cases."),
     ("a chest x-ray", False, "Not useful."),
     ("isolation for 2 weeks", False, "Not needed.")]],
   "Droplet precautions for 24 hours of treatment and prophylaxis for close contacts.",
   "Prioritize infection control in meningococcal meningitis."),

order(M(PPT, ABX, "generate_solutions", "difficult", NEURO, "ph ms"),
   "Place the first actions in the correct order.",
   [("Start droplet precautions", "Protects staff and others from the start."),
    ("Draw blood cultures", "Taken before antibiotics but without delaying them."),
    ("Give IV dexamethasone", "Given just before or with the first antibiotic dose to reduce neurologic damage."),
    ("Give IV ceftriaxone and vancomycin", "Antibiotics must not wait for the lumbar puncture."),
    ("Assist with the lumbar puncture once the provider confirms it is safe", "Imaging first if there are signs of raised ICP.")],
   "Precautions, cultures, dexamethasone with the first antibiotic, then the lumbar puncture if safe.",
   "Generate the sequence of care in bacterial meningitis.",
   ex=[note("Orders, 0210", "Droplet precautions. Blood cultures x2. Dexamethasone 10 mg IV. Ceftriaxone 2 g IV and "
       "vancomycin IV. Lumbar puncture after CT if needed.", id="men-orders")]),

sata(M(PA, MENING, "take_action", "difficult", NEURO, "ms"),
   "At 0600, review the findings. Which actions should the nurse take?",
   [("Notify the provider of the fall in GCS and pupil change", True, "Raised ICP."),
    ("Keep the room quiet and dim", True, "Reduces stimulation."),
    ("Seizure precautions", True, "Seizures are common."),
    ("Give the prescribed fluid bolus for the low blood pressure", True, "Septic shock."),
    ("Restrict all IV fluids to prevent brain swelling", False, "Hypotension needs fluid; restriction is not routine."),
    ("Lay the client flat to improve blood pressure", False, "Raises ICP.")],
   "Treat shock and rising ICP together, with seizure precautions and a calm environment.",
   "Take action as meningitis worsens.",
   ex=[flow("0600", ["0200", "0600"], [
       ("GCS", ["14", "11"]),
       ("Pupils", ["Equal, reactive", "Right sluggish"]),
       ("Blood pressure", ["96/58", "84/48 mm Hg"]),
       ("Rash", ["Legs", "Spreading to the trunk"])], id="men-0600")]),

matrix_mc(M(PA, MENING, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "Review day 5. For each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improvement", "Needs follow-up"],
   [("Afebrile; GCS 15", 0, "Infection controlled."),
    ("Rash fading", 0, "Recovering."),
    ("Does not turn to sounds on the left and fails a whispered voice test", 1, "Hearing loss is a common complication; refer for audiology."),
    ("Sodium 128 mEq/L with concentrated urine", 1, "Possible SIADH after meningitis."),
    ("Blood pressure 118/72 mm Hg off vasopressors", 0, "Shock resolved.")],
   "Recovery is under way; hearing loss and SIADH are complications to act on.",
   "Evaluate outcomes in bacterial meningitis.",
   ex=[note("Day 5", "Droplet precautions stopped on day 2.", id="men-day5")]),
]

# ================================================================================================
# Case 3: Parkinson disease
# ================================================================================================
pd_tabs = [
    ("pd-notes", "Nurses' notes", [note("Surgical unit, postoperative day 1",
        "A 72-year-old client with Parkinson disease had a hip fracture repair yesterday. At home the client takes "
        "carbidopa-levodopa at 0700, 1100, 1500 and 1900, and rasagiline. Since surgery, doses were given at 0900 "
        "and 2100 only. Today the client is rigid, cannot turn in bed, has a soft voice and coughs when drinking. "
        "The client was given haloperidol 2 mg overnight for confusion.", id="pd-note")]),
    ("pd-data", "Findings", [flow("Findings", ["Postoperative day 1, 1000"], [
        ("Temperature", ["38.3 C (100.9 F)"]),
        ("Heart rate", ["108/min"]),
        ("Blood pressure", ["148/92 mm Hg"]),
        ("Muscle tone", ["Severe rigidity"]),
        ("CK", ["1,850 units/L (30 to 200)"])], id="pd-flow")]),
]

pd_qs = [
hl_table(M(PPT, NEURO_MEDS, "recognize_cues", "moderate", NEURO, "ph ms"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Value"],
   [(["Levodopa schedule", "Two doses a day instead of four"], True, "Missed and late doses worsen Parkinson symptoms quickly."),
    (["Haloperidol", "Given overnight"], True, "A dopamine blocker is contraindicated in Parkinson disease."),
    (["Rigidity, fever and CK 1,850", "With tachycardia"], True, "Suggests parkinsonism-hyperpyrexia syndrome."),
    (["Coughing when drinking", "New"], True, "Aspiration risk."),
    (["Rasagiline", "Home medicine"], False, "Continued; an interaction risk to keep in mind."),
    (["Blood pressure", "148/92 mm Hg"], False, "Slightly raised; part of the picture.")],
   "Missed levodopa plus a dopamine blocker caused severe rigidity, fever and a high CK.",
   "Recognize cues of a Parkinson medication emergency."),

grouping(M(PPT, NEURO_MEDS, "analyze_cues", "difficult", NEURO, "ph"),
   "For each drug, select how it affects a client with Parkinson disease.",
   [("Worsens Parkinson symptoms", [
       ("Haloperidol", True, "Blocks dopamine."),
       ("Metoclopramide", True, "Blocks dopamine."),
       ("Carbidopa-levodopa", False, "Treats symptoms.")]),
    ("Dangerous interaction with rasagiline (MAO-B inhibitor)", [
       ("Meperidine", True, "Can cause serotonin syndrome."),
       ("Dextromethorphan", True, "Serotonin syndrome risk."),
       ("Acetaminophen", False, "Safe.")]),
    ("Safer for nausea or confusion", [
       ("Ondansetron", True, "Does not block dopamine centrally."),
       ("Quetiapine at low dose for psychosis", True, "Least likely to worsen motor symptoms."),
       ("Prochlorperazine", False, "Blocks dopamine.")])],
   "Dopamine blockers worsen Parkinson disease; MAO-B inhibitors interact with serotonergic drugs.",
   "Analyze drug effects in Parkinson disease."),

cloze(M(PA, DEGEN, "prioritize_hypotheses", "difficult", NEURO, "ms ph"),
   "Complete the sentence by choosing from the lists of options.",
   "The client most likely has {{b1}}, and the priority is to {{b2}}.",
   [[("parkinsonism-hyperpyrexia syndrome from missed levodopa and haloperidol", True, "Resembles neuroleptic malignant syndrome."),
     ("a normal postoperative fever", False, "Rigidity and CK say otherwise."),
     ("a wound infection", False, "Too early and does not explain rigidity."),
     ("a stroke", False, "Not focal.")],
    [("restore the home levodopa schedule and stop haloperidol", True, "Dopamine replacement is the treatment."),
     ("give more haloperidol for agitation", False, "Worsens it."),
     ("cool the client only", False, "Cooling helps but does not treat the cause."),
     ("hold all oral medicines", False, "Levodopa must be given; use a safe route.")]],
   "Restoring dopamine treatment on time is the key treatment.",
   "Prioritize a Parkinson medication emergency."),

sata(M(SIC, MEDSAFE, "generate_solutions", "difficult", NEURO, "ph mgmt"),
   "Which measures should the plan include to prevent this happening again?",
   [("Give Parkinson doses at home times", True, "Time-critical medicines."),
    ("Flag drugs to avoid in the record", True, "Prevents dopamine blockers."),
    ("Ask for another route if swallowing fails", True, "Doses must not be missed."),
    ("Ask for a swallow assessment", True, "Coughing on fluids."),
    ("Give all of the client's medicines at the unit's standard times", False, "Standard times caused the missed doses."),
    ("Use haloperidol for postoperative delirium as usual", False, "Contraindicated in Parkinson disease.")],
   "Time-critical dosing, alerts for drugs to avoid and safe routes prevent harm in Parkinson disease.",
   "Generate safety measures for Parkinson medications.",
   ex=[note("Plan", "Carbidopa-levodopa at home times. Haloperidol added to allergies/intolerances. Swallow assessment. "
       "Cooling. IV fluids. Monitor CK.", id="pd-plan")]),

order(M(SIC, INJURY, "take_action", "difficult", NEURO, "fund ms"),
   "The client is ready to get up for the first time with the physical therapist. Place the nurse's actions in "
   "order.",
   [("Time the walk about 1 hour after a levodopa dose", "Mobility is best at peak effect."),
    ("Check lying and standing blood pressure", "Parkinson drugs and the disease cause orthostatic hypotension."),
    ("Sit the client on the bed edge with feet flat before standing", "Allows the pressure to adjust."),
    ("Use a gait belt and walker", "Supports a shuffling, unstable gait."),
    ("Cue large steps and a rhythm if the client freezes", "Helps overcome freezing.")],
   "Time mobility with the medicine, check for orthostasis, stand in stages, and use aids and cues.",
   "Take action to mobilize safely in Parkinson disease.",
   ex=[flow("Postoperative day 3", ["Day 3"], [
       ("Temperature", ["37.2 C"]),
       ("Rigidity", ["Much improved"]),
       ("CK", ["420 units/L"]),
       ("Last levodopa", ["1100"])], id="pd-day3")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", NEURO, "ph"),
   "Before discharge, the client's spouse describes the home plan. For each statement, indicate whether it shows "
   "understanding or needs more teaching.",
   ["Shows understanding", "Needs more teaching"],
   [("\"We'll give the doses at the same times every day, even in hospital.\"", 0, "Time-critical."),
    ("\"High-protein meals may make the levodopa work less well, so we'll spread protein through the day.\"", 0,
     "Protein competes with levodopa absorption."),
    ("\"If anyone suggests haloperidol, we'll ask for something else.\"", 0, "Advocacy."),
    ("\"We'll stop the levodopa if the dizziness gets bad.\"", 1, "Never stop suddenly; call instead."),
    ("\"We'll take cough syrup with dextromethorphan for colds.\"", 1, "Interacts with rasagiline."),
    ("\"We'll remove the hall rugs and add night-lights.\"", 0, "Fall prevention.")],
   "Timed dosing, protein spacing, avoiding contraindicated drugs and fall prevention.",
   "Evaluate Parkinson disease teaching.",
   ex=[note("Discharge", "Going home with outpatient physical therapy.", id="pd-dc")]),
]

# ================================================================================================
# Case 4: Multiple sclerosis relapse
# ================================================================================================
ms_tabs = [
    ("ms-notes", "Nurses' notes", [note("Neurology clinic",
        "A 32-year-old client with relapsing multiple sclerosis has had blurred, painful vision in the right eye for "
        "3 days and new numbness and weakness of the right leg. The client also needs to urinate urgently and leaks "
        "before reaching the toilet. Symptoms got worse after a hot shower. The client takes fingolimod.",
        id="ms-note")]),
    ("ms-data", "Findings", [table("Findings", ["Result"], [
        ("Right eye", ["Reduced acuity, pain on eye movement, red colors look washed out"]),
        ("Right leg", ["Strength 3/5, reduced sensation"]),
        ("Post-void residual (bladder scan)", ["240 mL"]),
        ("Temperature", ["37.0 C"]),
        ("Heart rate", ["58/min"]),
        ("MRI", ["New enhancing lesions"])], id="ms-table")]),
]

ms_qs = [
hl_table(M(PA, DEGEN, "recognize_cues", "moderate", NEURO, "ms"),
   "Highlight the findings that show a new relapse or need follow-up.",
   ["Finding", "Result"],
   [(["Right eye", "Pain on movement, washed-out reds"], True, "Optic neuritis."),
    (["Right leg", "Strength 3/5"], True, "New motor deficit."),
    (["Post-void residual", "240 mL"], True, "Retention with urgency; risk of infection."),
    (["MRI", "New enhancing lesions"], True, "Active inflammation."),
    (["Temperature", "37.0 C"], False, "Normal; infection is not driving the change."),
    (["Heart rate", "58/min on fingolimod"], False, "Fingolimod slows the heart; a baseline to note.")],
   "New optic neuritis, leg weakness and bladder dysfunction with new lesions mean a relapse.",
   "Recognize cues of an MS relapse."),

matrix_mc(M(PA, DEGEN, "analyze_cues", "difficult", NEURO, "ms"),
   "For each finding, indicate whether it shows a true relapse or a pseudo-relapse.",
   ["True relapse", "Pseudo-relapse (temporary worsening)"],
   [("New enhancing lesions on MRI", 0, "New inflammation."),
    ("Symptoms worse after a hot shower that settle when cool", 1, "Heat sensitivity (Uhthoff phenomenon)."),
    ("New symptoms lasting more than 24 hours without fever", 0, "Definition of a relapse."),
    ("Worsening during a urinary infection that resolves with treatment", 1, "Infection causes temporary worsening.")],
   "A relapse is new inflammation lasting over 24 hours; heat or infection causes temporary pseudo-relapses.",
   "Analyze MS relapse versus pseudo-relapse."),

bowtie(M(PA, DEGEN, "prioritize_hypotheses", "difficult", NEURO, "ms ph"),
   "Complete the diagram by choosing the condition the client most likely has, 2 actions the nurse should take, and "
   "2 parameters the nurse should monitor.",
   [("MS relapse with optic neuritis and a neurogenic bladder", True, "New lesions and symptoms over 24 hours."),
    ("Stroke", False, "Young client with typical MS pattern and painful eye movement."),
    ("Urinary infection causing a pseudo-relapse", False, "No fever; new MRI lesions."),
    ("Myasthenia gravis", False, "Different pattern.")],
   [("Give IV methylprednisolone as prescribed", True, "Shortens the relapse."),
    ("Teach intermittent self-catheterization", True, "Manages retention."),
    ("Recommend hot baths to relax spasms", False, "Heat worsens symptoms."),
    ("Stop fingolimod on the client's own", False, "Stopping suddenly can cause severe rebound."),
    ("Restrict fluids to stop leaking", False, "Concentrated urine irritates the bladder and raises infection risk.")],
   [("Visual acuity", True, "Tracks optic neuritis."),
    ("Blood glucose", True, "High-dose steroids raise glucose."),
    ("Serum lipase each day", False, "Not relevant."),
    ("Body weight each week", False, "Not the main guide."),
    ("Peak expiratory flow rate", False, "Not relevant.")],
   "Treat the relapse with steroids, manage the bladder, and monitor vision and glucose.",
   "Prioritize care in an MS relapse."),

sata(M(PPT, NEURO_MEDS, "generate_solutions", "difficult", NEURO, "ph"),
   "Which parts of the plan should the nurse anticipate?",
   [("Methylprednisolone 1 g IV daily for 3 to 5 days", True, "Standard relapse treatment."),
    ("Glucose checks during steroid treatment", True, "Steroid hyperglycemia."),
    ("A urology referral for the retention", True, "Bladder management."),
    ("Continue fingolimod as prescribed", True, "Do not stop abruptly."),
    ("A live zoster vaccine during the relapse", False, "Live vaccines are avoided on fingolimod."),
    ("Baclofen stopped suddenly before the infusions", False, "Abrupt stopping causes withdrawal.")],
   "Steroid pulses with glucose checks, bladder care and continuing disease-modifying therapy safely.",
   "Generate a plan for an MS relapse.",
   ex=[note("Plan", "Methylprednisolone 1 g IV daily x 3 at the infusion center. Bladder diary.", id="ms-plan")]),

sata(M(PPT, NEURO_MEDS, "take_action", "difficult", NEURO, "ph"),
   "During the second infusion, review the findings. Which actions should the nurse take?",
   [("Report the glucose to the provider", True, "Correction insulin may be needed."),
    ("Ask about stomach pain or black stools", True, "Steroids raise GI bleeding risk."),
    ("Check the blood pressure again", True, "Steroids raise blood pressure."),
    ("Stop the infusion permanently", False, "These effects are managed, not a reason to stop."),
    ("Give the infusion faster to finish sooner", False, "Infused over at least an hour.")],
   "High-dose steroid effects (glucose, BP, GI) are monitored and managed during treatment.",
   "Take action for steroid adverse effects.",
   ex=[flow("Infusion day 2", ["Day 2"], [
       ("Glucose", ["262 mg/dL"]),
       ("Blood pressure", ["154/92 mm Hg"]),
       ("Client report", ["Trouble sleeping; feels wired"])], id="ms-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", NEURO, "ms"),
   "At the 6-week visit, for each finding or statement, indicate whether it shows the plan is working or needs "
   "follow-up.",
   ["Working", "Needs follow-up"],
   [("Vision back to normal", 0, "Optic neuritis resolved."),
    ("Right leg strength 4+/5", 0, "Recovery."),
    ("\"I self-catheterize four times a day; no leaks.\"", 0, "Bladder managed."),
    ("\"I've been too tired to go to work; I'm sure it's nothing.\"", 1, "MS fatigue and mood need assessment and support."),
    ("\"I'll keep cool and use a cooling vest in summer.\"", 0, "Reduces heat-related worsening."),
    ("Cloudy, foul-smelling urine for 3 days", 1, "Possible urinary infection with self-catheterization.")],
   "Vision, strength and bladder have recovered; fatigue, mood and a possible infection need follow-up.",
   "Evaluate outcomes after an MS relapse.",
   ex=[note("6 weeks", "Follow-up MRI planned in 3 months.", id="ms-6wk")]),
]

cases = [
    case("Head-to-toe assessment that uncovers a chronic subdural hematoma", h2t_tabs, h2t_qs),
    case("Bacterial meningitis with meningococcal sepsis", men_tabs, men_qs),
    case("Parkinson disease after hip surgery", pd_tabs, pd_qs),
    case("Multiple sclerosis relapse", ms_tabs, ms_qs),
]

BUILT = build(117, "neuro-cases-h2t-meningitis-pd-ms",
              "Neurologic case studies: head-to-toe assessment, meningitis, Parkinson disease and multiple sclerosis",
              items, cases, case_conversion=True)
