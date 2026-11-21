"""Batch 126 (case conversion, chunk 7: immunity, infection, STI, musculoskeletal and trauma): four full NGN case
studies (CASE-MAP.md, book_topics.py).

Cases: femur fracture in skeletal traction with fat embolism syndrome; acute gout flare with colchicine, then
urate-lowering therapy; below-knee amputation with phantom limb pain, contracture prevention and residual limb care;
rhabdomyolysis after a long lie on the floor. Each case walks the six CJMM steps in order, unfolds with new data in
steps 4 to 6, and includes a management of care, safety or health promotion question."""
from qb import *

RRP, PA, PPT, HPM, BCC, MOC, SIC = "PHI.RRP", "PHI.PA", "PHI.PPT", "HPM", "PHI.BCC", "SECE.MOC", "SECE.SIC"
MSK = "musculoskeletal"
FRACTURE = "fracture-complications"
TRACTION = "cast-and-traction-care"
GOUT = "arthritis-and-gout"
MSK_MEDS = "musculoskeletal-medications"
AMPUTATION = "amputation-care"
MOBILITY = "mobility-and-assistive-devices"
POSITION = "immobility-and-positioning"
LYTES = "electrolyte-imbalances"
KIDNEY = "kidney-injury-and-disease"
LABS = "laboratory-values-and-monitoring"
OXY = "oxygen-therapy-and-devices"
PAIN = "pain-assessment-and-comfort"
DELEG = "delegation-and-supervision"
TEACH = "health-teaching-and-behavior-change"
NUTR = "nutrition-and-therapeutic-diets"
SAFE = "safe-client-handling"
FLUIDS = "fluid-volume-imbalances"

items = []

# ================================================================================================
# Case 1: Femur fracture with fat embolism syndrome
# ================================================================================================
fes_tabs = [
    ("fes-notes", "Nurses' notes", [note("Orthopedic trauma unit",
        "A 23-year-old client was admitted yesterday after a motorcycle crash with a closed mid-shaft femur fracture "
        "and is in skeletal traction with a tibial pin, waiting for surgery tomorrow. At 0400 the client is restless "
        "and says, \"I can't catch my breath.\"", id="fes-note")]),
    ("fes-vitals", "Findings", [flow("Findings", ["2200", "0400"], [
        ("Respiratory rate", ["16", "30/min"]),
        ("SpO2", ["98% room air", "86% room air"]),
        ("Heart rate", ["88", "128/min"]),
        ("Temperature", ["37.2", "38.6 C"]),
        ("Mental status", ["Alert, oriented", "Restless, confused"]),
        ("Skin", ["Normal", "Pinpoint red spots on the chest, armpits and conjunctiva"]),
        ("Traction weights", ["Hanging freely", "Hanging freely"])], id="fes-flow")]),
]

fes_qs = [
hl_table(M(RRP, FRACTURE, "recognize_cues", "moderate", MSK, "ms"),
   "Highlight the 0400 findings that require immediate action.",
   ["Finding", "0400"],
   [(["SpO2", "86%"], True, "Hypoxemia."),
    (["Mental status", "Restless, confused"], True, "Cerebral fat emboli or hypoxia."),
    (["Skin", "Petechiae on chest, armpits, conjunctiva"], True, "Classic petechial rash."),
    (["Respiratory rate", "30/min"], True, "Respiratory distress."),
    (["Traction weights", "Hanging freely"], False, "Correctly maintained.")],
   "Hypoxemia, confusion and petechiae after a long-bone fracture is fat embolism syndrome.",
   "Recognize cues of fat embolism syndrome."),

matrix_mc(M(RRP, FRACTURE, "analyze_cues", "difficult", MSK, "ms"),
   "For each feature, indicate whether it favors fat embolism syndrome or pulmonary embolism from a DVT.",
   ["Fat embolism syndrome", "Pulmonary embolism from a DVT"],
   [("Onset 24 to 72 hours after a long-bone fracture", 0, "Typical timing."),
    ("Petechiae on the upper body and conjunctiva", 0, "Specific sign."),
    ("Confusion as an early sign", 0, "Cerebral fat emboli."),
    ("Unilateral calf swelling several days after immobility", 1, "Source of a thrombus."),
    ("Sudden pleuritic chest pain with a known leg clot", 1, "Thromboembolism.")],
   "The triad of hypoxemia, confusion and petechiae within 72 hours points to fat embolism.",
   "Analyze fat embolism versus PE."),

cloze(M(RRP, FRACTURE, "prioritize_hypotheses", "difficult", MSK, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The nurse's first priority is {{b1}}, because treatment of fat embolism is mainly {{b2}}.",
   [[("high-flow oxygen and calling the rapid response team", True, "Treats hypoxemia now."),
     ("removing the traction weights", False, "Movement of the fracture releases more fat."),
     ("giving heparin", False, "Does not treat fat emboli."),
     ("giving a sedative for restlessness", False, "Masks hypoxia.")],
    [("supportive, focused on oxygenation", True, "No specific therapy."),
     ("thrombolytic therapy", False, "Not for fat emboli."),
     ("antibiotics", False, "Not infection."),
     ("immediate amputation", False, "Not indicated.")]],
   "Oxygenate and escalate; care is supportive.",
   "Prioritize fat embolism syndrome."),

sata(M(BCC, TRACTION, "generate_solutions", "difficult", MSK, "ms"),
   "Which interventions should the nurse include for the client in skeletal traction?",
   [("Keep the weights hanging freely off the floor", True, "Maintains pull."),
    ("Clean the pin sites as per policy", True, "Prevents pin infection."),
    ("Neurovascular checks of the foot", True, "Detects compromise."),
    ("Remove the weights when repositioning the client in bed", False, "Traction must stay continuous."),
    ("Rest the weights on the bed frame to reduce pain", False, "Loses traction.")],
   "Continuous traction, pin care and neurovascular checks.",
   "Generate the traction care plan.",
   ex=[note("0430", "Rapid response team present. Non-rebreather mask applied; SpO2 91%.", id="fes-0430")]),

sata(M(MOC, DELEG, "take_action", "difficult", MSK, "mgmt"),
   "While the RN manages the client, which tasks can be assigned to an LPN or delegated to a UAP?",
   [("UAP: record vital signs on the other clients on the team", True, "Routine on stable clients."),
    ("LPN: give scheduled oral medicines to stable clients", True, "Within LPN scope."),
    ("UAP: bring the portable suction and oxygen to the room", True, "A non-clinical task."),
    ("UAP: watch the client's confusion and decide if it is worse", False, "Assessment of an unstable client stays with the RN."),
    ("LPN: take over the unstable client so the RN can do other work", False, "An unstable client stays with the RN.")],
   "The RN keeps the unstable client; stable tasks go to others.",
   "Delegate during an emergency.",
   ex=[note("0500", "Transfer to ICU planned.", id="fes-0500")]),

matrix_mc(M(RRP, FRACTURE, "evaluate_outcomes", "difficult", MSK, "ms"),
   "48 hours later, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("SpO2 95% on 2 L/min", 0, "Improving."),
    ("Alert and oriented", 0, "Neurologic recovery."),
    ("Petechiae fading", 0, "Resolving."),
    ("Calf pain and swelling on the uninjured leg", 1, "Possible DVT."),
    ("Femur fixed with an intramedullary nail", 0, "Early fixation reduces further emboli."),
    ("Platelets 82,000/mm3", 1, "Thrombocytopenia occurs in fat embolism and needs monitoring.")],
   "Recovery from fat embolism; a new DVT and low platelets need follow-up.",
   "Evaluate fat embolism treatment.",
   ex=[note("48 hours", "Surgery completed once oxygenation improved.", id="fes-48h")]),
]

# ================================================================================================
# Case 2: Acute gout flare
# ================================================================================================
gout_tabs = [
    ("gout-notes", "Clinic notes", [note("Primary care",
        "A 61-year-old client woke at 0300 with severe pain in the right big toe; even the bedsheet hurts. The client "
        "had a seafood buffet and several beers last night. Medicines: hydrochlorothiazide, low-dose aspirin. "
        "Creatinine clearance is 42 mL/min.", id="gout-note")]),
    ("gout-data", "Findings", [table("Findings", ["Result"], [
        ("Right first toe joint", ["Red, hot, swollen, exquisitely tender"]),
        ("Temperature", ["37.9 C"]),
        ("Uric acid", ["9.4 mg/dL"]),
        ("Joint fluid", ["Needle-shaped crystals, negative birefringence; Gram stain negative"]),
        ("Ears", ["Small firm white nodules on the rims"])], id="gout-table")]),
]

gout_qs = [
hl_table(M(PA, GOUT, "recognize_cues", "moderate", MSK, "ms"),
   "Highlight the findings that support acute gout.",
   ["Finding", "Result"],
   [(["Joint", "Red, hot, swollen first toe"], True, "Podagra."),
    (["Joint fluid", "Needle-shaped crystals"], True, "Urate crystals confirm gout."),
    (["Ear nodules", "Firm white"], True, "Tophi from chronic hyperuricemia."),
    (["Gram stain", "Negative"], False, "Argues against septic arthritis but does not support gout."),
    (["Onset", "Overnight after seafood and beer"], True, "Purine and alcohol triggers.")],
   "Crystals in joint fluid confirm gout.",
   "Recognize cues of acute gout."),

matrix_mr(M(PA, GOUT, "analyze_cues", "difficult", MSK, "ms ph"),
   "For each factor, select how it contributes to gout. Each row may have more than one answer.",
   ["Raises urate production", "Reduces urate excretion"],
   [("Seafood and organ meats", {0}, "High purine."),
    ("Beer", {0, 1}, "Purines and lactate reduce excretion."),
    ("Hydrochlorothiazide", {1}, "Reduces excretion."),
    ("Low-dose aspirin", {1}, "Reduces excretion."),
    ("Reduced kidney function", {1}, "Less excretion.")],
   "Diet raises production; diuretics, aspirin and kidney disease reduce excretion.",
   "Analyze gout risk factors."),

cloze(M(PPT, MSK_MEDS, "prioritize_hypotheses", "difficult", MSK, "ph"),
   "Complete the sentences by choosing from the lists of options.",
   "For the flare, the provider chooses {{b1}} because {{b2}}. Allopurinol is {{b3}}.",
   [[("low-dose colchicine with a reduced dose for kidney function", True, "Effective early; dose adjusted."),
     ("high-dose naproxen", False, "Risky with reduced kidney function."),
     ("allopurinol alone", False, "Does not treat a flare."),
     ("probenecid", False, "Not for flares and poor in kidney disease.")],
    [("NSAIDs could worsen the kidney function", True, "CrCl 42."),
     ("colchicine lowers uric acid", False, "It does not."),
     ("colchicine prevents tophi", False, "Not its role."),
     ("steroids are always contraindicated", False, "They are an option.")],
    [("started after the flare settles, or continued if already taken", True, "Starting during a flare can prolong it."),
     ("given in a high dose today", False, "Can worsen the flare."),
     ("never used with reduced kidney function", False, "Used with dose adjustment."),
     ("the first choice for pain", False, "Not an analgesic.")]],
   "Colchicine for the flare, urate-lowering later.",
   "Prioritize gout flare treatment."),

grouping(M(PA, GOUT, "generate_solutions", "difficult", MSK, "ms ph"),
   "For each area, select the actions the plan should include.",
   [("Comfort", [
       ("Rest and elevate the foot", True, "Reduces swelling."),
       ("Cold packs", True, "Reduces inflammation."),
       ("A bed cradle to keep sheets off the toe", True, "Prevents pressure."),
       ("Walk on the foot to keep it moving", False, "Increases pain.")]),
    ("Medicines", [
       ("Ask the provider about switching hydrochlorothiazide", True, "Reduces urate retention."),
       ("Ask about taking aspirin 650 mg for pain", False, "Aspirin raises urate.")]),
    ("Lifestyle", [
       ("Drink 2 to 3 liters of water daily", True, "Helps excretion."),
       ("Limit beer, organ meats and sugary drinks", True, "Reduces flares.")])],
   "Comfort, medication review and lifestyle changes.",
   "Generate the gout plan.",
   ex=[note("Orders", "Colchicine reduced dose. Losartan to replace hydrochlorothiazide.", id="gout-orders")]),

sata(M(PPT, MSK_MEDS, "take_action", "difficult", MSK, "ph"),
   "On day 2 of colchicine, review the findings. Which actions should the nurse take?",
   [("Tell the client to stop the colchicine", True, "Toxicity."),
    ("Notify the provider", True, "Change the plan."),
    ("Check whether the client started any new medicines", True, "Clarithromycin raises colchicine levels."),
    ("Advise taking extra colchicine because the toe still hurts", False, "Worsens toxicity."),
    ("Recommend loperamide and continue the colchicine", False, "Masks toxicity.")],
   "Diarrhea and weakness on colchicine, especially with an interacting drug, means stopping it.",
   "Take action on colchicine toxicity.",
   ex=[note("Phone call", "Six watery stools today, abdominal cramps, muscle weakness. Started clarithromycin for a "
       "chest infection yesterday.", id="gout-day2")]),

matrix_mc(M(HPM, TEACH, "evaluate_outcomes", "difficult", MSK, "ph"),
   "At 3 months on allopurinol, for each finding or statement, indicate whether it shows improvement or "
   "understanding, or needs follow-up.",
   ["Improvement or understanding", "Needs follow-up"],
   [("Uric acid 5.6 mg/dL", 0, "Target below 6."),
    ("\"I take allopurinol every day, even with no pain.\"", 0, "Continuous therapy."),
    ("A new itchy rash with fever and mouth sores", 1, "Possible severe allopurinol hypersensitivity; stop and seek care."),
    ("\"I'll stop the allopurinol if I get a flare.\"", 1, "Continue during flares."),
    ("\"I drink plenty of water and have cut back on beer.\"", 0, "Correct.")],
   "Urate at target; a rash or stopping during a flare needs follow-up.",
   "Evaluate gout management.",
   ex=[note("3 months", "Allopurinol started 2 weeks after the flare with colchicine prophylaxis.", id="gout-3m")]),
]

# ================================================================================================
# Case 3: Below-knee amputation
# ================================================================================================
bka_tabs = [
    ("bka-notes", "Surgical notes", [note("Vascular surgery unit, day 2",
        "A 66-year-old client with diabetes and peripheral arterial disease had a right below-knee amputation 2 days "
        "ago for a gangrenous foot. The client keeps a pillow under the residual limb and sits in a chair most of the "
        "day. Tonight the client says, \"My right foot is burning, but I know it's gone. Am I going crazy?\"",
        id="bka-note")]),
    ("bka-data", "Findings", [table("Findings", ["Result"], [
        ("Residual limb", ["Dressing dry, mild swelling, warm, edges intact"]),
        ("Pain", ["7/10 burning in the missing foot"]),
        ("Knee", ["Lacks 10 degrees of full extension"]),
        ("Glucose", ["188 mg/dL"]),
        ("Mood", ["Tearful, says \"I'm useless now\""])], id="bka-table")]),
]

bka_qs = [
hl_table(M(BCC, AMPUTATION, "recognize_cues", "moderate", MSK, "ms"),
   "Highlight the findings that require nursing action.",
   ["Finding", "Result"],
   [(["Pain", "Burning in the missing foot"], True, "Phantom limb pain."),
    (["Knee", "Lacks 10 degrees of extension"], True, "An early flexion contracture."),
    (["Positioning", "Pillow under the residual limb, sitting all day"], True, "Causes contractures."),
    (["Mood", "Tearful, \"useless\""], True, "Grief and body image."),
    (["Dressing", "Dry"], False, "Expected.")],
   "Phantom pain, contracture risk and grief all need action.",
   "Recognize cues after amputation."),

matrix_mc(M(BCC, AMPUTATION, "analyze_cues", "difficult", MSK, "ms"),
   "For each statement, indicate whether it is accurate or inaccurate about phantom limb pain.",
   ["Accurate", "Inaccurate"],
   [("It is real pain from the nerves and brain", 0, "Neuropathic."),
    ("It is a sign of a psychiatric problem", 1, "It is common and physical."),
    ("It often improves over months", 0, "Usually lessens."),
    ("Gabapentin and mirror therapy can help", 0, "Neuropathic treatments."),
    ("Strong pain before surgery raises the risk", 0, "A known risk factor.")],
   "Phantom pain is real neuropathic pain and is treated.",
   "Analyze phantom limb pain."),

cloze(M(BCC, AMPUTATION, "prioritize_hypotheses", "difficult", MSK, "ms"),
   "Complete the sentence by choosing from the lists of options.",
   "The problem most likely to delay walking with a prosthesis is {{b1}}, caused by {{b2}}.",
   [[("a knee flexion contracture", True, "A prosthesis needs a straight knee."),
     ("phantom sensations", False, "Common and not a barrier."),
     ("a dry dressing", False, "Expected."),
     ("a glucose of 188", False, "Needs control but less of a barrier.")],
    [("pillows under the limb and long periods sitting", True, "Keeps the knee flexed."),
     ("lying prone", False, "Prevents contractures."),
     ("walking with a walker", False, "Helps."),
     ("wrapping the limb", False, "Shapes the limb.")]],
   "Flexion contractures prevent prosthetic fitting.",
   "Prioritize post-amputation problems."),

sata(M(BCC, AMPUTATION, "generate_solutions", "difficult", MSK, "ms"),
   "Which interventions should the nurse include?",
   [("Lie prone for 30 minutes several times a day if tolerated", True, "Stretches the hip and knee."),
    ("Keep the knee straight when sitting", True, "Prevents contracture."),
    ("Wrap the residual limb in a figure-eight from distal to proximal", True, "Shapes the limb."),
    ("Give gabapentin as prescribed for phantom pain", True, "Neuropathic pain."),
    ("Place a pillow under the knee for comfort", False, "Causes contracture."),
    ("Wrap the limb in circular turns tightest at the top", False, "Acts as a tourniquet.")],
   "Prone positioning, straight knee, correct wrapping and neuropathic pain treatment.",
   "Generate the amputation plan.",
   ex=[note("Day 3", "Physical therapy started. Gabapentin ordered.", id="bka-day3")]),

sata(M(SIC, SAFE, "take_action", "difficult", MSK, "ms"),
   "The client wants to get to the bathroom at night. Which actions should the nurse take to prevent a fall?",
   [("Keep a wheelchair locked at the bedside", True, "Safe transfer."),
    ("Teach the client to transfer toward the intact leg", True, "Stronger side."),
    ("Remind the client that the leg is gone before standing", True, "Phantom sensation can cause falls."),
    ("Let the client stand on the residual limb to save time", False, "Not healed; falls and wound injury."),
    ("Keep the room dark so the client sleeps better", False, "Increases falls.")],
   "Locked wheelchair, transfers to the intact side and reminders prevent falls.",
   "Take action to prevent falls after amputation.",
   ex=[note("Night", "Client nearly fell trying to stand on both legs at 0200.", id="bka-night")]),

matrix_mc(M(BCC, AMPUTATION, "evaluate_outcomes", "difficult", MSK, "ms"),
   "At 3 weeks, for each finding or statement, indicate whether it shows progress or needs follow-up.",
   ["Progress", "Needs follow-up"],
   [("Knee extends fully", 0, "Contracture prevented."),
    ("Phantom pain 3/10", 0, "Improved."),
    ("\"I check my other foot every day with a mirror.\"", 0, "Protects the remaining foot."),
    ("Wound edge red, warm and draining pus", 1, "Infection."),
    ("\"I'd like to talk with someone who uses a prosthesis.\"", 0, "Healthy coping; arrange peer support."),
    ("Glucose 260 mg/dL most mornings", 1, "Poor control slows healing.")],
   "Progress in function and coping; infection and glucose need follow-up.",
   "Evaluate amputation recovery.",
   ex=[note("3 weeks", "Rehabilitation unit.", id="bka-3wk")]),
]

# ================================================================================================
# Case 4: Rhabdomyolysis after a long lie
# ================================================================================================
rh_tabs = [
    ("rh-notes", "Emergency notes", [note("Emergency department",
        "A 79-year-old client was found on the floor by a neighbor after lying there about 30 hours following a "
        "fall. The client takes atorvastatin. The client is weak, thirsty and passing small amounts of dark urine.",
        id="rh-note")]),
    ("rh-data", "Results", [table("Results", ["Result"], [
        ("CK", ["48,000 units/L"]),
        ("Potassium", ["6.2 mEq/L"]),
        ("Creatinine", ["2.8 mg/dL"]),
        ("Calcium", ["7.6 mg/dL"]),
        ("Urine dipstick", ["Large blood, but only 1 to 2 red cells per field"]),
        ("Urine output", ["15 mL/hr"]),
        ("Right hip and thigh", ["Bruised, swollen, tense"])], id="rh-table")]),
]

rh_qs = [
hl_table(M(PA, KIDNEY, "recognize_cues", "moderate", MSK, "ms lab"),
   "Highlight the findings that require immediate action.",
   ["Finding", "Result"],
   [(["CK", "48,000 units/L"], True, "Massive muscle breakdown."),
    (["Potassium", "6.2 mEq/L"], True, "Released from muscle."),
    (["Creatinine and output", "2.8 mg/dL, 15 mL/hr"], True, "Acute kidney injury."),
    (["Urine", "Blood on dipstick without red cells"], True, "Myoglobin."),
    (["Thigh", "Tense, swollen"], True, "Possible compartment syndrome.")],
   "A long lie with a very high CK, dark urine and kidney injury is rhabdomyolysis.",
   "Recognize cues of rhabdomyolysis."),

matrix_mc(M(PA, KIDNEY, "analyze_cues", "difficult", MSK, "ms"),
   "For each finding, indicate its cause in rhabdomyolysis.",
   ["Released from damaged muscle", "Myoglobin in the kidneys", "Calcium deposited in damaged muscle"],
   [("High potassium", 0, "From muscle cells."),
    ("Rising creatinine", 1, "Myoglobin toxic to tubules."),
    ("Low calcium early", 2, "Calcium binds in muscle."),
    ("Dark urine", 1, "Myoglobinuria."),
    ("High CK", 0, "Muscle enzyme.")],
   "Muscle contents cause hyperkalemia and kidney injury.",
   "Analyze rhabdomyolysis."),

matrix_mc(M(PA, KIDNEY, "prioritize_hypotheses", "difficult", MSK, "ms"),
   "For each problem, indicate its priority.",
   ["Treat first", "Treat soon", "Monitor"],
   [("Potassium 6.2 with peaked T waves", 0, "Cardiac risk."),
    ("Low urine output from myoglobin", 0, "Large-volume fluids protect the kidneys."),
    ("Tense thigh with rising pain", 1, "Assess for compartment syndrome."),
    ("Calcium 7.6 without symptoms", 2, "Not replaced unless symptomatic or hyperkalemia needs it.")],
   "Hyperkalemia and kidney protection with fluids come first.",
   "Prioritize rhabdomyolysis problems.",
   ex=[note("ECG", "Peaked T waves.", id="rh-ecg")]),

sata(M(PA, FLUIDS, "generate_solutions", "difficult", MSK, "ms"),
   "Which parts of the plan should the nurse anticipate?",
   [("Isotonic IV fluids at a high rate", True, "Flushes myoglobin."),
    ("A target urine output of about 200 to 300 mL/hr", True, "Protects the kidneys."),
    ("Hourly urine output with a catheter", True, "Guides fluids."),
    ("Stop the atorvastatin", True, "Statins can cause myopathy."),
    ("Restrict fluids to protect the heart", False, "Worsens kidney injury."),
    ("Lactated Ringer's with added potassium", False, "Adds potassium.")],
   "Aggressive isotonic fluids with hourly output; stop the statin.",
   "Generate the rhabdomyolysis plan.",
   ex=[note("Orders", "0.9% sodium chloride 500 mL/hr. Foley. Hold atorvastatin.", id="rh-orders")]),

sata(M(PA, FLUIDS, "take_action", "difficult", MSK, "ms"),
   "Six hours later, review the findings. Which actions should the nurse take?",
   [("Notify the provider of the crackles and low output", True, "Fluid overload with failing kidneys."),
    ("Slow the IV fluids as directed", True, "Prevents pulmonary edema."),
    ("Raise the head of the bed and apply oxygen", True, "Supports breathing."),
    ("Prepare for possible dialysis", True, "Fluid overload with oliguria."),
    ("Increase the fluid rate to force more urine", False, "Worsens edema."),
    ("Give potassium chloride for weakness", False, "Potassium is already high.")],
   "Crackles with ongoing oliguria during aggressive fluids means stop and prepare for dialysis.",
   "Take action on fluid overload in rhabdomyolysis.",
   ex=[flow("6 hours", ["Start", "6 hours"], [
       ("Urine output", ["15", "20 mL/hr"]),
       ("Fluids in", ["-", "3,100 mL"]),
       ("Lungs", ["Clear", "Crackles in both bases"]),
       ("SpO2", ["96%", "90%"])], id="rh-6h")]),

matrix_mc(M(PA, KIDNEY, "evaluate_outcomes", "difficult", MSK, "ms lab"),
   "On day 5, for each finding, indicate whether it shows improvement or needs follow-up.",
   ["Improved", "Needs follow-up"],
   [("CK 6,200 units/L", 0, "Falling."),
    ("Potassium 4.6 mEq/L", 0, "Normal."),
    ("Urine output 80 mL/hr", 0, "Kidney recovery."),
    ("Thigh pain with passive stretch and new numbness", 1, "Compartment syndrome."),
    ("Creatinine 1.6 mg/dL", 0, "Improving."),
    ("Calcium 11.2 mg/dL", 1, "Rebound hypercalcemia in recovery.")],
   "Recovery under way; compartment syndrome and rebound calcium need follow-up.",
   "Evaluate rhabdomyolysis treatment.",
   ex=[note("Day 5", "Two dialysis sessions given.", id="rh-day5")]),
]

cases = [
    case("Femur fracture in traction with fat embolism syndrome", fes_tabs, fes_qs),
    case("Acute gout flare and urate-lowering therapy", gout_tabs, gout_qs),
    case("Below-knee amputation with phantom limb pain", bka_tabs, bka_qs),
    case("Rhabdomyolysis after a long lie on the floor", rh_tabs, rh_qs),
]

BUILT = build(126, "msk-cases-fat-embolism-gout-bka-rhabdo",
              "Musculoskeletal case studies: fat embolism, gout, below-knee amputation and rhabdomyolysis",
              items, cases, case_conversion=True)
