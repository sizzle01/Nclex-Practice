"""Batch 101 (case conversion, chunk 1: cardiac): four full NGN case studies on vascular emergencies and
coagulation therapy (CASE-MAP.md).

Cases: type A aortic dissection; warfarin over-anticoagulation with a GI bleed in a client with a mechanical
valve; heparin-induced thrombocytopenia with arterial thrombosis; cardiac tamponade after coronary bypass.
Each case walks the six CJMM steps in order and unfolds with new data in steps 4 to 6."""
from qb import *

RRP, PA, PPT = "PHI.RRP", "PHI.PA", "PHI.PPT"
CV, GI, HEME = "cardiovascular", "gastrointestinal", "hematologic-oncologic"
VASC = "vascular-disorders"
CV_MEDS = "cardiovascular-medications"
ANTICOAG = "anticoagulant-therapy-and-monitoring"
ANTIDOTE = "antidotes-and-reversal-agents"
CALC = "medication-dose-calculations"
GIBLEED = "upper-gi-disorders-and-bleeding"
CLOT = "bleeding-and-clotting-disorders"
POSTOP = "postoperative-care"
POSTPROC = "post-procedure-complications"
STROKE = "stroke-care"
VTE = "venous-thromboembolism"

items = []

# ================================================================================================
# Case 1: Type A aortic dissection
# ================================================================================================
ad_tabs = [
    ("ad-notes", "Nurses' notes", [note("Emergency department, 2340",
        "A 58-year-old client had sudden, severe chest pain that \"tore\" through to the back between the shoulder "
        "blades while lifting a heavy box. History: poorly controlled hypertension; the client stopped "
        "amlodipine 3 months ago. The client is pale, sweating and anxious. A new soft diastolic murmur is heard at "
        "the left sternal border. The right radial pulse is stronger than the left. The chest x-ray shows a widened "
        "mediastinum. The ECG shows sinus tachycardia without ST elevation.", id="ad-note")]),
    ("ad-vitals", "Vital signs", [flow("Vital signs", ["2340"], [
        ("Blood pressure, right arm", ["196/104 mm Hg"]),
        ("Blood pressure, left arm", ["150/88 mm Hg"]),
        ("Heart rate", ["112/min"]),
        ("Respirations", ["24/min"]),
        ("SpO2", ["96% on room air"]),
        ("Pain", ["10 of 10"])], id="ad-flow")]),
    ("ad-labs", "Laboratory results", [table("Laboratory results", ["2340", "Reference range"], [
        ("Troponin I", ["< 0.04 ng/mL", "< 0.04 ng/mL"]),
        ("Creatinine", ["1.0 mg/dL", "0.6 to 1.2 mg/dL"]),
        ("Hemoglobin", ["14.1 g/dL", "13.5 to 17.5 g/dL"]),
        ("D-dimer", ["4.8 mcg/mL", "< 0.5 mcg/mL"])], id="ad-lab")]),
]

ad_qs = [
hl_table(M(PA, VASC, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Assessment", "Finding"],
   [(["Pain", "Sudden tearing pain through to the back, 10 of 10"], True,
     "Sudden tearing pain radiating to the back is the classic description of aortic dissection."),
    (["Blood pressure", "Right arm 196/104, left arm 150/88 mm Hg"], True,
     "A difference of more than 20 mm Hg between the arms means the tear is affecting a branch artery."),
    (["Heart sounds", "New soft diastolic murmur"], True,
     "A new diastolic murmur suggests aortic regurgitation from a dissection reaching the aortic valve."),
    (["Chest x-ray", "Widened mediastinum"], True, "A widened mediastinum supports dissection of the thoracic aorta."),
    (["ECG", "Sinus tachycardia, no ST elevation"], False,
     "Expected with pain; the absence of ST elevation helps rule out STEMI but is not itself a problem."),
    (["Troponin I", "< 0.04 ng/mL"], False, "Normal; no sign of infarction.")],
   "Tearing back pain, unequal arm pressures, a new aortic murmur and a wide mediastinum are the cues of dissection.",
   "Recognize cues of aortic dissection."),

matrix_mr(M(PA, VASC, "analyze_cues", "difficult", CV, "ms"),
   "For each finding, indicate the condition or conditions it can be consistent with. Each row may have more than "
   "one answer.",
   ["Aortic dissection", "Acute myocardial infarction", "Pulmonary embolism"],
   [("Sudden severe chest pain", {0, 1, 2}, "All three cause sudden chest pain, so pain alone does not separate them."),
    ("Blood pressure difference of more than 20 mm Hg between the arms", {0},
     "Unequal pressures come from the tear narrowing a subclavian artery."),
    ("Widened mediastinum on chest x-ray", {0}, "This reflects an enlarged, dissected thoracic aorta."),
    ("Raised D-dimer", {0, 2}, "D-dimer rises with clot in PE and with blood in the false lumen in dissection."),
    ("ST elevation in two adjacent leads", {1},
     "ST elevation is the sign of STEMI; dissection can cause it only by involving a coronary artery."),
    ("Pleuritic pain with low oxygen saturation", {2}, "These fit PE.")],
   "The findings specific to dissection are unequal arm pressures, a wide mediastinum and a new aortic murmur; "
   "pain and D-dimer overlap with MI and PE.",
   "Analyze cues to tell aortic dissection from MI and PE."),

cloze(M(PA, VASC, "prioritize_hypotheses", "difficult", CV, "ms"),
   "The CT angiogram shows a dissection starting in the ascending aorta. Complete the sentences by choosing from "
   "the lists of options.",
   "This is a {{b1}} dissection, which is treated with {{b2}}. Until then, the greatest threat to the client's "
   "life is {{b3}}.",
   [[("type A", True, "Any dissection involving the ascending aorta is Stanford type A."),
     ("type B", False, "Type B starts beyond the left subclavian artery."),
     ("chronic", False, "This began within hours."),
     ("traumatic", False, "Lifting is a trigger with uncontrolled hypertension, not blunt trauma.")],
    [("emergency surgical repair", True,
      "Type A dissection is a surgical emergency because it can rupture or reach the heart."),
     ("medical blood pressure control alone", False, "This is the usual treatment for uncomplicated type B."),
     ("thrombolytic therapy", False, "Thrombolysis would be fatal in a dissection."),
     ("a cardiac catheterization with a stent", False, "Coronary stenting does not treat the aorta.")],
    [("aortic rupture or bleeding into the pericardium", True,
      "A type A tear can rupture or cause tamponade within minutes to hours."),
     ("an embolic stroke from atrial fibrillation", False, "The client is in sinus rhythm."),
     ("contrast-associated kidney injury", False, "A later, lesser risk."),
     ("hypoglycemia", False, "Not relevant.")]],
   "Ascending aorta involvement makes it type A, which needs emergency surgery; rupture and tamponade are the "
   "immediate threats.",
   "Prioritize hypotheses in type A aortic dissection."),

select_n(M(PPT, CV_MEDS, "generate_solutions", "difficult", CV, "ph ms"),
   "Review the prescriptions while the client waits for surgery. Select the 3 prescriptions the nurse should "
   "carry out first.",
   [("Esmolol infusion to a heart rate below 60/min", True,
     "Beta blockade first lowers the force and rate of each beat, reducing shear on the aortic wall."),
    ("Nicardipine IV infusion after the heart rate is controlled", True,
     "A vasodilator then lowers pressure to the target; given first it would cause reflex tachycardia."),
    ("Morphine IV for pain", True, "Pain drives sympathetic tone, which raises pressure and heart rate."),
    ("Heparin 80 units/kg IV bolus, then a continuous infusion", False, "Anticoagulation would worsen bleeding into the false lumen."),
    ("Aspirin 324 mg chewed now to limit clot formation", False, "Antiplatelet drugs increase bleeding before aortic surgery."),
    ("Hydralazine 10 mg IV push as the first drug given", False,
     "A vasodilator alone causes reflex tachycardia, which increases shear on the aortic wall."),
    ("0.9% sodium chloride 1 L bolus to protect the kidneys", False, "The client is hypertensive; fluid would raise pressure.")],
   "In dissection, control heart rate with a beta blocker first, then lower pressure with a vasodilator, and treat "
   "pain. Avoid anticoagulants and antiplatelets.",
   "Generate solutions for type A aortic dissection before surgery.",
   ex=[note("Prescriptions, 0010",
       "Esmolol infusion titrated to heart rate < 60/min. Nicardipine infusion titrated to systolic BP 100 to "
       "120 mm Hg once heart rate is controlled. Morphine 4 mg IV every 2 hours as needed. Heparin bolus and "
       "infusion. Aspirin 324 mg. Hydralazine 10 mg IV. 0.9% sodium chloride 1 L bolus. Surgery at 0200.",
       id="ad-orders")]),

cloze(M("SECE.MOC", "handoff-and-sbar-communication", "take_action", "difficult", CV, "mgmt ms"),
   "The nurse calls the surgeon about the 0045 findings. Complete the SBAR report by choosing from the lists of "
   "options.",
   "Situation: {{b1}}. Background: type A dissection on esmolol and nicardipine, surgery at 0200. Assessment: "
   "{{b2}}. Recommendation: {{b3}}.",
   [[("the left leg is newly numb and pulseless, and urine output has fallen to 10 mL/hour", True,
      "The situation states the new problem first, in one line."),
     ("the client's pain has improved from 10 to 6 of 10", False, "Good news, but not why the nurse is calling."),
     ("the heart rate is now 58/min on esmolol", False, "That is the target, not the problem."),
     ("the client is anxious about surgery", False, "Not the reason for an urgent call.")],
    [("I think the dissection is extending into the iliac and renal arteries", True,
      "The nurse states a clinical conclusion from the data."),
     ("I think the client is dehydrated", False, "Low output here comes from a blocked renal artery."),
     ("I think the esmolol dose is too high", False, "The rate and pressure are on target."),
     ("I am not sure what is happening", False, "SBAR asks the nurse to give an assessment.")],
    [("please come to see the client now and consider moving surgery forward", True,
      "Organ and limb ischemia make this more urgent; the request is specific."),
     ("may I give a fluid bolus for the urine output", False, "Fluid would raise pressure and does not treat a blocked artery."),
     ("may I stop the nicardipine", False, "Pressure control must continue."),
     ("please review the client at the 0200 surgery", False, "Waiting allows ischemia to progress.")]],
   "SBAR gives the problem, the context, the nurse's conclusion and a specific request. New limb and kidney "
   "ischemia in dissection calls for the surgeon now.",
   "Communicate an urgent change with SBAR.",
   ex=[flow("Reassessment", ["0010", "0045"], [
       ("Blood pressure (right arm)", ["196/104 mm Hg", "118/70 mm Hg"]),
       ("Heart rate", ["112/min", "58/min"]),
       ("Left leg", ["Warm, pulses 2+", "Numb, cool, pedal pulses absent"]),
       ("Urine output", ["60 mL/hour", "10 mL/hour"]),
       ("Pain", ["10 of 10", "6 of 10"])], id="ad-0045")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", CV, "ms"),
   "The client had emergency repair with a graft. Review the findings on the first day after surgery. For each "
   "finding, indicate whether it is expected or needs follow-up.",
   ["Expected", "Needs follow-up"],
   [("Systolic blood pressure 112 mm Hg on a nicardipine infusion", 0,
     "After repair, pressure is kept low to protect the suture lines."),
    ("Chest tube drainage 60 mL/hour, serosanguineous", 0,
     "Drainage under 100 mL/hour that is lightening in color is expected."),
    ("Unable to move or feel both legs", 1,
     "Paraplegia after aortic surgery means spinal cord ischemia; it needs immediate action, such as spinal drainage."),
    ("Urine output 15 mL/hour for 2 hours", 1, "Output below 0.5 mL/kg/hour suggests kidney injury from the dissection."),
    ("Left pedal pulses 2+ and warm foot", 0, "Blood flow to the leg has been restored by the repair."),
    ("Pain 4 of 10 with IV opioid", 0, "Moderate pain after sternotomy is expected and controlled.")],
   "Low pressure, modest drainage and restored leg perfusion are expected. Loss of leg movement and low urine output "
   "are serious complications of aortic repair.",
   "Evaluate outcomes after aortic dissection repair.",
   ex=[flow("Postoperative day 1, 0800", ["0800"], [
       ("Blood pressure", ["112/64 mm Hg on nicardipine"]),
       ("Chest tube", ["60 mL/hour, serosanguineous"]),
       ("Legs", ["Cannot move or feel either leg"]),
       ("Urine output", ["15 mL/hour for 2 hours"]),
       ("Left foot", ["Warm, pedal pulses 2+"]),
       ("Pain", ["4 of 10 on IV opioid"])], id="ad-pod1")]),
]

# ================================================================================================
# Case 2: Warfarin over-anticoagulation with a GI bleed and a mechanical valve
# ================================================================================================
warf_tabs = [
    ("warf-notes", "Nurses' notes", [note("Emergency department, 1300",
        "A 72-year-old client with a mechanical mitral valve has taken warfarin 5 mg daily for 6 years. Seven days "
        "ago, a clinic started trimethoprim-sulfamethoxazole for a urinary tract infection. Since yesterday, the "
        "client has had black, tarry stools and dizziness when standing. The client also takes aspirin 81 mg daily "
        "and omeprazole. Skin is pale and cool. Weight 165 lb (75 kg).", id="warf-note")]),
    ("warf-vitals", "Vital signs", [flow("Vital signs", ["1300"], [
        ("Blood pressure", ["96/60 mm Hg"]),
        ("Heart rate", ["112/min"]),
        ("Respirations", ["22/min"]),
        ("SpO2", ["97% on room air"])], id="warf-flow")]),
    ("warf-labs", "Laboratory results", [table("Laboratory results", ["1300", "Reference range"], [
        ("INR", ["8.2 (target 2.5 to 3.5)", "0.8 to 1.1"]),
        ("Hemoglobin", ["7.9 g/dL (12.8 last month)", "12 to 16 g/dL"]),
        ("BUN", ["52 mg/dL", "10 to 20 mg/dL"]),
        ("Creatinine", ["1.0 mg/dL", "0.6 to 1.2 mg/dL"]),
        ("Platelets", ["210,000/mm3", "150,000 to 400,000/mm3"])], id="warf-lab")]),
]

warf_qs = [
hl_text(M(PPT, ANTICOAG, "recognize_cues", "moderate", GI, "ph ms"),
   "Highlight the findings in the nurses' notes that require follow-up.",
   ["A client with ",
    ("a mechanical mitral valve", True, "A mechanical valve has a high clot risk, which affects how far anticoagulation can be reversed."),
    " takes warfarin. Seven days ago, ",
    ("trimethoprim-sulfamethoxazole was started", True, "This antibiotic strongly increases warfarin's effect."),
    ". The client has ",
    ("black, tarry stools", True, "Melena shows upper GI bleeding."),
    " and ",
    ("dizziness on standing", True, "Orthostatic symptoms show significant blood loss."),
    ". The client takes ",
    ("omeprazole", False, "A proton pump inhibitor protects the stomach; not a problem here."),
    " and ",
    ("aspirin 81 mg daily", True, "Aspirin adds antiplatelet bleeding risk to warfarin."),
    ". Weight is ",
    ("75 kg", False, "Needed for dosing, but not a problem."),
    "."],
   "The cues are the bleeding, its cause (a warfarin interaction plus aspirin) and the mechanical valve that limits "
   "how anticoagulation is managed.",
   "Recognize cues of warfarin-related bleeding."),

dtable(M(PPT, ANTICOAG, "analyze_cues", "difficult", GI, "ph lab"),
   "For each finding, choose the explanation that fits best.",
   ["Finding", "Explanation"],
   [(["INR 8.2"], [
       ("Trimethoprim-sulfamethoxazole slowed warfarin breakdown", True,
        "It inhibits CYP2C9, the enzyme that clears the more potent form of warfarin."),
       ("Vitamin K was absorbed from the diet", False, "Vitamin K would lower the INR."),
       ("The client has liver failure", False, "Nothing suggests liver failure."),
       ("The aspirin dose raised the INR", False, "Aspirin affects platelets, not the INR.")]),
    (["BUN 52 mg/dL with creatinine 1.0 mg/dL"], [
       ("Blood in the gut is digested and absorbed as protein", True,
        "A high BUN with a normal creatinine is typical of upper GI bleeding."),
       ("Acute kidney injury", False, "Creatinine is normal."),
       ("Overhydration", False, "Overhydration lowers BUN."),
       ("A high-protein diet", False, "Not supported by the history.")]),
    (["Hemoglobin 7.9 g/dL"], [
       ("Acute blood loss, possibly not yet fully shown", True,
        "Hemoglobin falls further as fluid shifts into the vessels, so the loss may be greater than it appears."),
       ("Iron deficiency from diet", False, "The fall from 12.8 in a month is too fast for this."),
       ("Lab error", False, "Symptoms match the result."),
       ("Hemolysis from the antibiotic", False, "Melena explains the loss.")]),
    (["Heart rate 112/min, BP 96/60 mm Hg"], [
       ("Compensation for low circulating volume", True, "Tachycardia keeps up output as volume falls."),
       ("A side effect of omeprazole", False, "Omeprazole does not cause this."),
       ("Anxiety alone", False, "Pale, cool skin and low pressure show hypovolemia."),
       ("Atrial fibrillation", False, "There is no irregular rhythm.")])],
   "A drug interaction raised the INR, and the upper GI bleed explains the high BUN, falling hemoglobin and "
   "compensatory tachycardia.",
   "Analyze cues of over-anticoagulation and upper GI bleeding."),

cloze(M(PA, GIBLEED, "prioritize_hypotheses", "difficult", GI, "ms ph"),
   "Complete the sentences by choosing from the lists of options.",
   "The client's most urgent problem is {{b1}}. Once the anticoagulation is reversed, the nurse must also watch for "
   "{{b2}} because {{b3}}.",
   [[("hypovolemic shock from GI bleeding", True, "Ongoing bleeding with tachycardia and hypotension is the immediate threat."),
     ("a urinary tract infection", False, "A minor problem compared with the bleed."),
     ("heart failure from the valve", False, "There is no sign of fluid overload."),
     ("hyperkalemia", False, "Not supported by the data.")],
    [("valve thrombosis or embolic stroke", True, "Without anticoagulation, a mechanical valve can clot within days."),
     ("hemorrhagic stroke", False, "The risk falls as anticoagulation is reversed."),
     ("hypoglycemia", False, "Not relevant."),
     ("aspirin toxicity", False, "Not relevant at 81 mg.")],
    [("a mechanical mitral valve has the highest clot risk", True,
      "Mitral mechanical valves have a high clot risk, so anticoagulation must restart as soon as bleeding is controlled."),
     ("the INR is above the target range", False, "This is what is being reversed."),
     ("the client takes omeprazole", False, "Unrelated."),
     ("the client is 72 years old", False, "Age adds risk but is not the main reason.")]],
   "Treat the bleed first, but remember that full reversal in a client with a mechanical mitral valve brings a "
   "real risk of valve thrombosis and stroke.",
   "Prioritize hypotheses in bleeding with a mechanical valve."),

grouping(M(PPT, ANTIDOTE, "generate_solutions", "difficult", GI, "ph ms"),
   "For each part of the plan, select the actions the nurse should anticipate. Each group has at least one answer.",
   [("Reverse the anticoagulation", [
       ("Vitamin K 10 mg by slow IV infusion", True, "IV vitamin K starts working in hours and lasts."),
       ("Four-factor prothrombin complex concentrate", True, "PCC replaces clotting factors within minutes for major bleeding."),
       ("Protamine sulfate", False, "Protamine reverses heparin, not warfarin."),
       ("Idarucizumab", False, "This reverses dabigatran only.")]),
    ("Restore circulating volume", [
       ("Two large-bore IV lines", True, "Allows rapid fluid and blood."),
       ("Packed red blood cells as prescribed", True, "Hemoglobin 7.9 with active bleeding needs transfusion."),
       ("D5W bolus", False, "Dextrose in water leaves the vessels and does not restore volume.")]),
    ("Medications to change", [
       ("Hold warfarin", True, "Warfarin is held until bleeding is controlled and the plan is reviewed."),
       ("Ask for a different antibiotic", True, "Removes the interaction that raised the INR."),
       ("Pantoprazole IV", True, "Acid suppression stabilizes clots on a peptic ulcer."),
       ("Continue aspirin 81 mg", False, "Aspirin adds to bleeding and is held.")])],
   "Reverse warfarin with IV vitamin K and PCC, restore volume with blood, and remove the causes: warfarin, aspirin "
   "and the interacting antibiotic.",
   "Generate solutions for warfarin-related bleeding.",
   ex=[note("Prescriptions, 1330",
       "Vitamin K 10 mg IV over 30 minutes. Four-factor PCC 50 units/kg IV. Type and crossmatch 2 units of packed "
       "red blood cells. Pantoprazole 80 mg IV. Hold warfarin and aspirin. Stop trimethoprim-sulfamethoxazole; "
       "start an alternative antibiotic. Gastroenterology consult for endoscopy.", id="warf-orders")]),

sata(M("SECE.SIC", "client-identification-and-error-prevention", "take_action", "difficult", GI, "fund lab"),
   "The first unit of packed red blood cells arrives from the blood bank at 1410. Which actions should the nurse "
   "take?",
   [("Check the unit, the label and the client's band with a second licensed nurse at the bedside", True,
     "Two-person bedside verification prevents ABO-incompatible transfusion, the most dangerous transfusion error."),
    ("Prime the blood tubing with 0.9% sodium chloride only", True,
     "Dextrose solutions cause red cells to clump and break down."),
    ("Stay with the client and check vital signs during the first 15 minutes", True,
     "Most acute hemolytic reactions start early, within the first 50 mL."),
    ("Start the unit within 2 hours of its release from the blood bank", False,
     "A unit is started within 30 minutes of release and finished within 4 hours, to limit bacterial growth."),
    ("Hang the unit with the pantoprazole infusion through the same line", False,
     "No drugs are given through blood tubing."),
    ("Warm the unit in the unit microwave to prevent hypothermia", False,
     "Microwaving damages red cells; only an approved blood warmer is used."),
    ("Let the UAP take the first set of vital signs alone", False,
     "The first 15 minutes need the RN's assessment for a reaction.")],
   "Safe transfusion depends on two-person bedside identification, saline-only tubing, prompt start and close "
   "early monitoring by the RN.",
   "Take action to give a blood transfusion safely.",
   ex=[note("Blood bank release, 1410",
       "Packed red blood cells, group O negative, 1 unit, crossmatch compatible. Released 1410. Client blood type "
       "O negative.", id="warf-blood")]),

matrix_mc(M(PA, STROKE, "evaluate_outcomes", "difficult", GI, "ms ph"),
   "Review the findings 18 hours after reversal. For each finding, indicate whether it shows the treatment worked "
   "or needs follow-up.",
   ["Treatment worked", "Needs follow-up"],
   [("INR 1.4", 0, "Reversal has worked; the provider now plans when to restart anticoagulation."),
    ("Hemoglobin 9.1 g/dL after 2 units", 0, "Stable after transfusion, so bleeding has stopped."),
    ("No melena since 0400", 0, "The bleed has stopped."),
    ("Heart rate 86/min, BP 124/72 mm Hg", 0, "Hemodynamics have recovered."),
    ("New weakness of the left arm and slurred speech", 1,
     "With the INR reversed in a client with a mechanical mitral valve, this suggests an embolic stroke; call a stroke alert."),
    ("Muffled, softer mechanical valve click", 1,
     "A softer click can mean clot on the valve, which needs urgent echocardiography.")],
   "Bleeding control is shown by the INR, stable hemoglobin and vital signs. New neurologic signs or a change in "
   "the valve click suggest the thrombotic complication of reversal.",
   "Evaluate outcomes after warfarin reversal.",
   ex=[flow("18 hours after reversal", ["0700"], [
       ("INR", ["1.4"]),
       ("Hemoglobin", ["9.1 g/dL"]),
       ("Stools", ["No melena since 0400"]),
       ("Vital signs", ["HR 86/min, BP 124/72 mm Hg"]),
       ("Neurologic", ["New left arm weakness, slurred speech"]),
       ("Heart sounds", ["Mechanical valve click softer than on admission"])], id="warf-0700")]),
]

# ================================================================================================
# Case 3: Heparin-induced thrombocytopenia
# ================================================================================================
hit_tabs = [
    ("hit-notes", "Nurses' notes", [note("Medical unit, day 7, 0800",
        "A 66-year-old client was admitted 7 days ago with a right femoral deep vein thrombosis and started on a "
        "heparin infusion. Today the client reports that the left foot is cold and painful. The left foot is pale "
        "and cool with a weak dorsalis pedis pulse; the right leg is less swollen than on admission. The client "
        "also has a red, painful patch on the abdomen where heparin was given before the infusion started. "
        "Weight 80 kg. Liver function is normal.", id="hit-note")]),
    ("hit-labs", "Laboratory results", [table("Platelet count and coagulation", ["Day 1", "Day 4", "Day 7", "Reference range"], [
        ("Platelets", ["240,000/mm3", "210,000/mm3", "98,000/mm3", "150,000 to 400,000/mm3"]),
        ("aPTT", ["32 s", "74 s", "70 s", "25 to 35 s; target 60 to 90 s on heparin"]),
        ("Hemoglobin", ["13.0 g/dL", "12.8 g/dL", "12.7 g/dL", "12 to 16 g/dL"])], id="hit-lab")]),
    ("hit-mar", "Medications", [note("Medication record",
        "Heparin infusion by protocol. Heparin 10 units/mL flush to the PICC line every 8 hours. Docusate 100 mg "
        "twice daily. Acetaminophen 650 mg every 6 hours as needed.", id="hit-mar-note")]),
]

hit_qs = [
hl_table(M(PPT, ANTICOAG, "recognize_cues", "moderate", HEME, "ph lab"),
   "Highlight the findings that require follow-up.",
   ["Assessment", "Finding"],
   [(["Platelets", "240,000 on day 1, 98,000 on day 7"], True,
     "A fall of more than 50% after 5 to 10 days of heparin is the typical pattern of HIT."),
    (["Left foot", "Cold, pale, painful, weak pulse"], True, "New arterial clot in the other leg suggests HIT thrombosis."),
    (["Abdominal skin", "Red, painful patch at a prior heparin site"], True,
     "Skin lesions at heparin injection sites are a sign of HIT."),
    (["Right leg", "Less swollen than on admission"], False, "The original DVT is improving."),
    (["aPTT", "70 seconds"], False, "Within the target range on heparin."),
    (["Hemoglobin", "12.7 g/dL"], False, "Stable; there is no bleeding.")],
   "The platelet fall with new clotting and skin lesions points to HIT, a clotting disorder despite the low platelets.",
   "Recognize cues of heparin-induced thrombocytopenia."),

matrix_mc(M("SECE.SIC", "medication-safety-systems", "analyze_cues", "difficult", HEME, "ph"),
   "HIT is suspected. For each item in the client's care, indicate whether it exposes the client to heparin.",
   ["Exposes the client to heparin", "Safe to continue"],
   [("Heparin 10 units/mL flush to the PICC every 8 hours", 0, "Even small flush doses keep the immune reaction going."),
    ("A heparin-bonded PICC line", 0, "Some catheters are coated with heparin and may need to be replaced."),
    ("0.9% sodium chloride flushes", 1, "Saline contains no heparin."),
    ("Enoxaparin for clot prevention if the infusion stops", 0,
     "Low molecular weight heparin cross-reacts with HIT antibodies."),
    ("Acetaminophen 650 mg every 6 hours as needed", 1, "No interaction with HIT."),
    ("A pressure bag for an arterial line containing heparin 2 units/mL", 0, "Arterial line flush bags often contain heparin.")],
   "Stopping the infusion is not enough: flushes, coated lines, flush bags and every heparin product must go, and the "
   "allergy record must show heparin.",
   "Analyze hidden sources of heparin in a client with HIT."),

matrix_mc(M(PA, CLOT, "prioritize_hypotheses", "difficult", HEME, "ph ms"),
   "For each possible problem, indicate its priority for the client now.",
   ["Highest priority", "Lower priority"],
   [("Loss of the left leg from arterial thrombosis", 0,
     "An ischemic limb can be lost within hours; it needs urgent treatment."),
    ("New clots elsewhere, such as pulmonary embolism or stroke", 0,
     "HIT continues to generate clots while heparin continues."),
    ("Bleeding from the low platelet count", 1, "Bleeding is uncommon in HIT."),
    ("Constipation from bed rest", 1, "Minor compared with thrombosis.")],
   "The priority in HIT is thrombosis in the limb and elsewhere.",
   "Prioritize hypotheses in HIT."),

sata(M(PPT, ANTICOAG, "generate_solutions", "difficult", HEME, "ph"),
   "Which prescriptions should the nurse anticipate for this client?",
   [("Stop the heparin infusion", True, "All heparin must stop."),
    ("Replace heparin flushes with 0.9% sodium chloride flushes", True, "Hidden heparin sources must also stop."),
    ("Start an argatroban infusion", True,
     "A direct thrombin inhibitor anticoagulates without heparin; with normal liver function argatroban is suitable."),
    ("Send a PF4 antibody test", True, "Confirms the diagnosis."),
    ("Transfuse platelets", False, "Platelet transfusion can fuel more clotting in HIT."),
    ("Switch to enoxaparin 1 mg/kg every 12 hours", False, "LMWH cross-reacts."),
    ("Start warfarin today", False,
     "Warfarin is started only after platelets recover, because early warfarin can cause venous limb gangrene in HIT.")],
   "Stop every heparin source, start a non-heparin anticoagulant and confirm the diagnosis; avoid platelets, LMWH "
   "and early warfarin.",
   "Generate solutions for HIT.",
   ex=[note("Prescriptions, 0900",
       "Vascular surgery consult for the left leg. Hematology consult. The provider asks the nurse to review the "
       "anticoagulation plan with pharmacy.", id="hit-orders")]),

calc(M(PPT, CALC, "take_action", "difficult", HEME, "ph"),
   "Argatroban is prescribed at 2 mcg/kg/minute. The bag contains argatroban 250 mg in 250 mL. The client weighs "
   "80 kg. At what rate should the nurse set the pump? Record your answer to one decimal place.",
   "mL/hour", 1, "Rate", 9.6,
   "2 mcg/kg/min x 80 kg = 160 mcg/min. 160 mcg/min x 60 = 9,600 mcg/hour = 9.6 mg/hour. The bag is 1 mg/mL, "
   "so the rate is 9.6 mL/hour.",
   "Calculate a weight-based anticoagulant infusion rate.",
   ex=[note("Argatroban prescription, 1000",
       "Argatroban 2 mcg/kg/minute IV continuous infusion. Bag: 250 mg in 250 mL 0.9% sodium chloride. Check aPTT "
       "2 hours after starting; target 1.5 to 3 times baseline.", id="hit-arg")]),

matrix_mc(M(PPT, ANTICOAG, "evaluate_outcomes", "difficult", HEME, "ph lab"),
   "Review the findings on day 10. For each finding, indicate whether it shows the treatment is working, is an "
   "expected effect of the drug, or needs follow-up.",
   ["Treatment working", "Expected drug effect", "Needs follow-up"],
   [("Platelets 162,000/mm3", 0, "Recovery of the platelet count shows the immune reaction is settling."),
    ("aPTT 68 seconds (baseline 32)", 0, "About twice baseline, inside the 1.5 to 3 times target."),
    ("INR 2.4 with no warfarin given", 1,
     "Argatroban raises the INR by itself; this matters when warfarin is added later, but it is not over-anticoagulation."),
    ("Left foot warm, pulse 2+ after thrombectomy", 0, "Limb perfusion has been restored."),
    ("Pink urine and new bruising on both arms", 2, "These signs of bleeding need the provider and an aPTT check.")],
   "Recovering platelets, a therapeutic aPTT and a warm foot show success; argatroban's INR rise is expected; new "
   "bleeding needs action.",
   "Evaluate outcomes of anticoagulation in HIT.",
   ex=[flow("Day 10", ["0800"], [
       ("Platelets", ["162,000/mm3"]),
       ("aPTT", ["68 s"]),
       ("INR", ["2.4 (no warfarin given)"]),
       ("Left foot", ["Warm, dorsalis pedis 2+ after thrombectomy on day 7"]),
       ("Skin and urine", ["Pink urine; new bruises on both forearms"])], id="hit-day10")]),
]

# ================================================================================================
# Case 4: Cardiac tamponade after coronary artery bypass grafting
# ================================================================================================
tam_tabs = [
    ("tam-notes", "Nurses' notes", [note("Cardiac surgery ICU, 0200",
        "A 64-year-old client is 6 hours after coronary artery bypass grafting. The client is extubated, on a "
        "low-dose nitroglycerin infusion, with mediastinal chest tubes to suction and temporary epicardial pacing "
        "wires. At 0200 the client is restless and says, \"I can't get comfortable.\" Neck veins are distended with "
        "the head of the bed at 30 degrees. Heart sounds are hard to hear. The systolic pressure falls 18 mm Hg on "
        "inspiration.", id="tam-note")]),
    ("tam-flow", "Flowsheet", [flow("Hemodynamics and chest tube drainage", ["2300", "0000", "0100", "0200"], [
        ("Blood pressure", ["118/70", "108/72", "96/74", "84/68"]),
        ("Heart rate", ["88", "98", "112", "124"]),
        ("CVP (2 to 6 mm Hg)", ["8", "11", "15", "19"]),
        ("Chest tube drainage, mL/hour", ["180", "210", "40", "10"]),
        ("Urine output, mL/hour", ["80", "60", "30", "15"])], id="tam-flowsheet")]),
]

tam_qs = [
hl_table(M(RRP, POSTOP, "recognize_cues", "moderate", CV, "ms"),
   "Highlight the findings that require immediate follow-up.",
   ["Finding", "Data"],
   [(["Chest tube drainage", "210 mL/hour, then suddenly 10 mL/hour"], True,
     "A sudden stop after heavy drainage suggests clotted tubes, so blood is collecting around the heart."),
    (["Blood pressure", "118/70 falling to 84/68 mm Hg"], True,
     "Falling systolic pressure with a narrowing pulse pressure (50 to 16 mm Hg) shows falling stroke volume."),
    (["CVP", "8 rising to 19 mm Hg"], True, "Rising filling pressure with falling output suggests compression of the heart."),
    (["Systolic fall on inspiration", "18 mm Hg"], True, "Pulsus paradoxus above 10 mm Hg is a sign of tamponade."),
    (["Airway", "Extubated"], False, "Expected progress after surgery."),
    (["Nitroglycerin", "Low-dose infusion"], False, "Common after bypass; not a finding in itself.")],
   "Sudden fall in chest tube drainage, falling pressure with a narrow pulse pressure, rising CVP and pulsus paradoxus "
   "are the cues of tamponade.",
   "Recognize cues of cardiac tamponade after cardiac surgery."),

cloze(M(RRP, POSTPROC, "analyze_cues", "difficult", CV, "ms"),
   "Complete the sentences by choosing from the lists of options.",
   "The sudden drop in chest tube drainage most likely means {{b1}}. The rising CVP with falling blood pressure shows "
   "that {{b2}}. The distended neck veins and muffled heart sounds with low blood pressure are known as {{b3}}.",
   [[("clot is blocking the tubes while bleeding continues", True,
      "Bleeding has not stopped; blood is now trapped in the mediastinum."),
     ("the bleeding has stopped", False, "Falling pressure and rising CVP argue against this."),
     ("the suction is set too high", False, "Higher suction would increase drainage."),
     ("the client is dehydrated", False, "CVP is rising, not falling.")],
    [("the heart cannot fill because it is compressed", True,
      "Pressure in the pericardium stops the ventricles filling, so CVP rises while output falls."),
     ("the client is fluid overloaded from surgery", False, "Overload would not cause falling pressure with falling drainage."),
     ("the grafts have closed", False, "Graft failure causes ischemia on the ECG and does not explain falling drainage."),
     ("the client is vasodilated from nitroglycerin", False, "Vasodilation lowers CVP.")],
    [("Beck's triad", True, "Hypotension, distended neck veins and muffled heart sounds."),
     ("Cushing's triad", False, "That is hypertension, bradycardia and irregular breathing with raised ICP."),
     ("Virchow's triad", False, "That describes risk factors for clotting."),
     ("Charcot's triad", False, "That describes cholangitis.")]],
   "Blocked tubes with continued bleeding cause tamponade, which stops the heart from filling.",
   "Analyze cues of postoperative cardiac tamponade."),

bowtie(M(PA, "shock-and-sepsis", "prioritize_hypotheses", "difficult", CV, "ms"),
   "Complete the diagram by choosing the condition the client is most likely experiencing, 2 actions the nurse "
   "should take, and 2 parameters the nurse should monitor.",
   [("Cardiac tamponade", True, "Beck's triad, pulsus paradoxus and blocked chest tubes after surgery."),
    ("Hypovolemic shock", False, "Hypovolemia lowers CVP; here it is rising."),
    ("Tension pneumothorax", False, "Possible after surgery, but tracheal shift and absent breath sounds are not reported."),
    ("Acute graft closure", False, "This would show new ischemia on the ECG.")],
   [("Call the cardiac surgeon to the bedside now", True, "Tamponade after surgery needs reopening or drainage."),
    ("Prepare the bedside chest-opening tray", True, "Emergency sternotomy may be done in the ICU."),
    ("Give furosemide for the raised CVP", False, "Diuresis lowers preload, which the compressed heart depends on."),
    ("Raise the nitroglycerin to treat ischemia", False, "Vasodilation would deepen the hypotension."),
    ("Lower the chest tube suction", False, "This would not relieve the compression.")],
   [("Blood pressure and pulse pressure", True, "Show the effect of compression on stroke volume."),
    ("CVP trend", True, "Rising CVP tracks rising pericardial pressure."),
    ("Blood glucose every 4 hours", False, "Not the key guide."),
    ("Core body temperature", False, "Not the key guide."),
    ("Bowel sounds in all four quadrants", False, "Not the key guide.")],
   "Tamponade after cardiac surgery is a surgical emergency: call the surgeon and prepare for reopening while "
   "tracking pressure and CVP.",
   "Prioritize care in postoperative cardiac tamponade."),

grouping(M("SECE.MOC", "delegation-and-supervision", "generate_solutions", "difficult", CV, "mgmt"),
   "The primary RN stays at the bedside while the cardiac surgeon is on the way. For each team member, select the "
   "tasks that fit their role.",
   [("Second ICU RN", [
       ("Stop the nitroglycerin and start the fluid bolus as prescribed", True, "IV medication changes need an RN."),
       ("Open and arrange the sterile chest-opening tray", True, "An ICU RN trained in the emergency protocol can prepare it."),
       ("Take the next set of vital signs only", False, "Wasting an RN on a single measured task in an emergency.")]),
    ("Unlicensed assistive personnel", [
       ("Bring the emergency cart and the blood from the blood bank", True, "Fetching equipment and blood is within the role."),
       ("Assess the chest tube for clots", False, "Assessment stays with RNs."),
       ("Stay with the family in the waiting room and update them on the plan", False,
        "Explaining the plan and the client's condition is not within the UAP role.")]),
    ("Charge nurse", [
       ("Call the operating room and anesthesia team", True, "Coordinates resources for possible reopening."),
       ("Cover the primary RN's other client", True, "Reassigning the workload keeps the other client safe."),
       ("Document the event in place of the primary RN", False, "Each nurse documents their own care.")])],
   "In an emergency, each role does what it is licensed and trained for: RNs give drugs and prepare for surgery, the "
   "UAP fetches, the charge nurse coordinates and covers workload.",
   "Plan delegation during a postoperative emergency.",
   ex=[note("Staffing, 0205",
       "On duty: the primary RN, a second ICU RN, the charge nurse and one UAP. The primary RN's second client is "
       "stable on a ventilator.", id="tam-staff")]),

sata(M(PA, "shock-and-sepsis", "take_action", "difficult", CV, "ms aha"),
   "At 0210 the monitor shows a narrow-complex rhythm at 118/min, and no pulse can be felt. The surgeon is 3 minutes "
   "away. Which actions should the nurse take?",
   [("Call for help and start the cardiac surgery arrest protocol", True,
     "Pulseless electrical activity after cardiac surgery is treated by protocol while the team prepares to reopen."),
    ("Prepare the chest-opening tray for emergency resternotomy", True,
     "Reopening relieves tamponade, the cause of the arrest; it should happen within about 5 minutes."),
    ("Start compressions if the chest cannot be opened within a minute", True,
     "Compressions maintain some perfusion until the chest can be opened."),
    ("Stop the nitroglycerin infusion", True,
     "A vasodilator lowers the preload and pressure the compressed heart depends on."),
    ("Deliver an unsynchronized shock at 200 joules right away", False,
     "PEA is not a shockable rhythm; shocks treat VF and pulseless VT."),
    ("Wait for the surgeon to arrive before starting resuscitation", False,
     "Every minute without perfusion worsens the outcome; the protocol starts at once."),
    ("Give furosemide 40 mg IV to bring the high CVP down", False, "Lowering preload worsens tamponade.")],
   "Pulseless electrical activity from tamponade after cardiac surgery needs emergency reopening, with "
   "compressions only as a bridge.",
   "Take action for arrest from tamponade after cardiac surgery.",
   ex=[flow("0210", ["0210"], [
       ("Monitor", ["Narrow-complex rhythm, 118/min"]),
       ("Pulse", ["None palpable"]),
       ("Arterial line", ["Flat waveform"]),
       ("Surgeon", ["3 minutes away"])], id="tam-0210")]),

matrix_mc(M(RRP, POSTOP, "evaluate_outcomes", "difficult", CV, "ms"),
   "Clot was removed from around the heart at the bedside and a bleeding graft site was repaired. Review the 0500 "
   "findings. For each finding, indicate whether it shows the treatment worked or needs follow-up.",
   ["Treatment worked", "Needs follow-up"],
   [("Blood pressure 116/70 mm Hg, pulse pressure 46 mm Hg", 0, "Stroke volume has recovered."),
    ("CVP 7 mm Hg", 0, "Filling pressure has fallen as the compression is relieved."),
    ("Chest tube drainage 60 mL/hour, lightening in color", 0, "Drainage is moderate and slowing."),
    ("Urine output 55 mL/hour", 0, "Kidney perfusion has recovered."),
    ("Systolic fall on inspiration of 16 mm Hg", 1,
     "Pulsus paradoxus that persists suggests continuing pericardial compression."),
    ("Client follows commands and moves all four limbs", 0, "Neurologic function is intact after the arrest.")],
   "Recovered pressure, falling CVP, steady drainage and good urine output show success; persistent pulsus "
   "paradoxus needs reporting.",
   "Evaluate outcomes after relief of cardiac tamponade.",
   ex=[flow("After bedside re-exploration", ["0500"], [
       ("Blood pressure", ["116/70 mm Hg"]),
       ("CVP", ["7 mm Hg"]),
       ("Chest tube drainage", ["60 mL/hour, lightening"]),
       ("Urine output", ["55 mL/hour"]),
       ("Systolic fall on inspiration", ["16 mm Hg"]),
       ("Neurologic", ["Follows commands, moves all limbs"])], id="tam-0500")]),
]

cases = [
    case("Type A aortic dissection", ad_tabs, ad_qs),
    case("Warfarin over-anticoagulation with a GI bleed and a mechanical valve", warf_tabs, warf_qs),
    case("Heparin-induced thrombocytopenia with arterial thrombosis", hit_tabs, hit_qs),
    case("Cardiac tamponade after coronary bypass surgery", tam_tabs, tam_qs),
]

BUILT = build(101, "cardiac-cases-vascular-coagulation",
              "Cardiac case studies: aortic dissection, warfarin reversal, heparin-induced thrombocytopenia and "
              "postoperative tamponade",
              items, cases, case_conversion=True)
